using System;

using Alea.CUDA;
using Alea.CUDA.Utilities;
using Alea.CUDA.IL;

namespace HandSightLibrary.ImageProcessing
{
    public class LBP
    {
        const double PI = Math.PI;
        const int numNeighbors = 8, radius = 1, numVarBins = 16;

        static int numPatterns = (int)Math.Pow(2, numNeighbors);
        static int numUniformPatterns = numNeighbors + 2;
        static int numSubuniformPatterns = (numNeighbors - 1) * numNeighbors + 3;

        static short[] subuniformBins = null;
        static float[] neighborCoordinateX = null, neighborCoordinateY = null;
        static int[] hist = null, hist2 = null;

        static Worker worker = Worker.Default;
        static DeviceMemory<short> lbpImageGPU = null;
        static DeviceMemory<short> varImageGPU = null;
        static DeviceMemory<int> histGPU = null;
        //static DeviceMemory<short> uniformBinsGPU = null;
        static DeviceMemory<short> subuniformBinsGPU = null;
        static DeviceMemory<float> neighborCoordinateXGPU = null, neighborCoordinateYGPU = null;
        static LaunchParam lp = null;
        static bool initialized = false;

        static float[] varBins = new float[] { float.NegativeInfinity, 1.6113763094439419f, 8.4662133430334876f, 19.068693504197039f, 32.931756363447185f, 50.806257223632748f, 73.925768389128791f, 103.99872371684611f, 143.55929591350352f, 196.37682253643587f, 268.634109897359f, 371.5463598630414f, 526.48493535337172f, 780.620183093791f, 1280.9687670261649f, 11866.117177664119f, float.PositiveInfinity };
        //static float[] varBins = new float[] { float.NegativeInfinity, 8.4662133430334876f, 32.931756363447185f, 73.925768389128791f, 143.55929591350352f, 268.634109897359f, 526.48493535337172f, 1280.9687670261649f, float.PositiveInfinity };
        static DeviceMemory<float> varBinsGPU = null;

        /// <summary>
        /// CUDA Kernel that computes the LBP pattern for each image pixel
        /// </summary>
        /// <param name="width">image width</param>
        /// <param name="height">image height</param>
        /// <param name="image">image data (1D byte array)</param>
        /// <param name="lbpImage">pre-allocated lbp uniform bin data of same size as image (1D short array)</param>
        /// <param name="varImage">pre-allocated lbp variance bin data of same size as image (1D short array)</param>
        /// <param name="subuniformBins">lookup table to convert LBP patterns to subuniform bins</param>
        /// <param name="neighborCoordinateX">precomputed x coordinates for the neighbors</param>
        /// <param name="neighborCoordinateY">precomputed y coordinates for the neighbors</param>
        /// <param name="varBinCuts">precomputed array defining the cutoffs for the variance bins</param>
        [AOTCompile]
        static void LBP_Kernel(int width, int height, deviceptr<byte> image, deviceptr<short> lbpImage, deviceptr<short> varImage, deviceptr<short> subuniformBins, deviceptr<float> neighborCoordinateX, deviceptr<float> neighborCoordinateY, deviceptr<float> varBinCuts)
        {
            var x = blockIdx.x * blockDim.x + threadIdx.x;
            var y = blockIdx.y * blockDim.y + threadIdx.y;
            if (x < radius || x >= width - radius || y < radius || y >= height - radius) return;

            int code = 0;
            float v = image[(y) * width + (x)];
            float mean = 0;
            for (int i = 0; i < numNeighbors; i++)
            {
                // perform interpolation around a circle of specified radius
                //float xx = (float)radius * (float)Math.Cos(2.0 * PI * (double)i / (double)numNeighbors);
                //float yy = (float)radius * (float)Math.Sin(2.0 * PI * (double)i / (double)numNeighbors);
                float xx = radius * neighborCoordinateX[i];
                float yy = radius * neighborCoordinateY[i];

                // relative indices
                int fx = xx == (int)xx ? (int)xx : (xx > 0 ? (int)xx : (int)xx - 1);
                int fy = yy == (int)yy ? (int)yy : (yy > 0 ? (int)yy : (int)yy - 1);
                int cx = xx == (int)xx ? (int)xx : (xx > 0 ? (int)xx + 1 : (int)xx);
                int cy = yy == (int)yy ? (int)yy : (yy > 0 ? (int)yy + 1 : (int)yy);

                // fractional part
                float ty = yy - fy;
                float tx = xx - fx;

                // set interpolation weights
                float w1 = (1 - tx) * (1 - ty);
                float w2 = tx * (1 - ty);
                float w3 = (1 - tx) * ty;
                float w4 = tx * ty;

                var neighbor = w1 * image[(y + fy) * width + (x + fx)]
                             + w2 * image[(y + fy) * width + (x + cx)]
                             + w3 * image[(y + cy) * width + (x + fx)]
                             + w4 * image[(y + cy) * width + (x + cx)];

                code |= ((neighbor - v > 1e-5 ? 1 : 0) << (numNeighbors - i - 1));
                mean += neighbor;
            }

            // compute variance of neighbors
            mean /= numNeighbors;
            float var = 0;
            for (int i = 0; i < numNeighbors; i++)
                var += (v - mean) * (v - mean) / numNeighbors;

            // find variance bin
            short varBin = 0;
            for (short i = 0; i < numVarBins; i++)
                if (var >= varBinCuts[i] && var < varBinCuts[i + 1])
                {
                    varBin = i;
                    break;
                }

            //lbpImage[y * width + x] = (short)code;
            lbpImage[y * width + x] = subuniformBins[code];
            varImage[y * width + x] = varBin;
        }

        /// <summary>
        /// CUDA Kernel that computes a histogram over an array of LBP patterns
        /// </summary>
        /// <param name="width">image width</param>
        /// <param name="height">image height</param>
        /// <param name="lbpBins">LBP uniform bin data, computed in the LBP_Kernel function</param>
        /// <param name="varBins">LBP variance bin data, computed in the LBP_Kernel function</param>
        /// <param name="hist">Pre-allocated histogram array for output</param>
        [AOTCompile]
        static void Hist_Kernel(int width, int height, deviceptr<short> lbpBins, deviceptr<short> varBins, deviceptr<int> hist)
        {
            var x = blockIdx.x * blockDim.x + threadIdx.x;
            var y = blockIdx.y * blockDim.y + threadIdx.y;
            if (x < radius || x >= width - radius || y < radius || y >= height - radius) return;

            int lbpBin = lbpBins[(y) * width + (x)];
            int varBin = varBins[(y) * width + (x)];
            Intrinsic.__atomic_add(hist + lbpBin * numVarBins + varBin, 1);
        }

        /// <summary>
        /// Computes an LBP histogram over the full image
        /// </summary>
        /// <param name="image">an EMGU image</param>
        /// <returns>a normalized histogram of uniform LBP patterns</returns>
        public static float[] GetHistogram(VideoFrame frame)
        {
            if (!initialized) // Note: assumes that the image size will not change
            {
                // initialize data structures to avoid reallocating with every call
                //hist = new int[numUniformPatterns * numVarBins];
                //hist = new int[numPatterns * numVarBins];
                //hist = new int[numPatterns];
                hist = new int[numSubuniformPatterns * numVarBins];
                hist2 = new int[numSubuniformPatterns * numVarBins];
                lbpImageGPU = worker.Malloc<short>(frame.Width * frame.Height);
                varImageGPU = worker.Malloc<short>(frame.Width * frame.Height);
                histGPU = worker.Malloc<int>(hist.Length);

                // precompute the uniform bin for each LBP pattern, and push it to the GPU
                subuniformBins = new short[(short)Math.Pow(2, numNeighbors)];
                for (int i = 0; i < subuniformBins.Length; i++)
                {
                    short bin = GetPatternNum(i);
                    subuniformBins[i] = bin;
                }
                subuniformBinsGPU = worker.Malloc(subuniformBins);

                neighborCoordinateX = new float[numNeighbors];
                neighborCoordinateY = new float[numNeighbors];
                for (int i = 0; i < numNeighbors; i++)
                {
                    float xx = (float)Math.Cos(2.0 * PI * (double)i / (double)numNeighbors);
                    float yy = (float)Math.Sin(2.0 * PI * (double)i / (double)numNeighbors);
                    neighborCoordinateX[i] = xx;
                    neighborCoordinateY[i] = yy;
                }
                neighborCoordinateXGPU = worker.Malloc(neighborCoordinateX);
                neighborCoordinateYGPU = worker.Malloc(neighborCoordinateY);

                varBinsGPU = worker.Malloc(varBins);

                // initialize CUDA parameters
                var blockDims = new dim3(32, 32);
                var gridDims = new dim3(Common.divup(frame.Width, blockDims.x), Common.divup(frame.Height, blockDims.y));
                lp = new LaunchParam(gridDims, blockDims);

                initialized = true;
            }

            // reshape the image data to a 1D array, and push it to the GPU
            //Buffer.BlockCopy(image.Data, 0, imageData, 0, image.Width * image.Height);
            //byte[, ,] data = image.Data;
            //int w = image.Width, h = image.Height;
            //for (int i = 0; i < h; i++)
            //    for (int j = 0; j < w; j++)
            //        imageData[i * w + j] = data[i, j, 0];
            //imageGPU.ScatterScalar(data[i, j, 0], i * w + j);
            //imageGPU.Scatter(image);

            // run the LBP kernel
            worker.Launch(LBP_Kernel, lp, frame.Width, frame.Height, frame.ImageGPU.Ptr, lbpImageGPU.Ptr, varImageGPU.Ptr, subuniformBinsGPU.Ptr, neighborCoordinateXGPU.Ptr, neighborCoordinateYGPU.Ptr, varBinsGPU.Ptr);

            // zero out the histogram data on the GPU, then compute a new histogram from the LBP patterns
            for (int i = 0; i < hist.Length; i++) hist[i] = 0;
            histGPU.Scatter(hist);
            worker.Launch(Hist_Kernel, lp, frame.Width, frame.Height, lbpImageGPU.Ptr, varImageGPU.Ptr, histGPU.Ptr);
            histGPU.Gather(hist2);

            // shift the histogram segments to achieve rotation invariance
            hist[0] = hist2[0];
            hist[1] = hist2[1];
            for (int m = 0; m < numNeighbors - 1; m++)
            {
                int maxIndex = 0;
                int max = 0;
                for (int w = 0; w < numNeighbors; w++)
                {
                    int v = 0;
                    for(int i = 0; i < numVarBins; i++)
                        v += hist2[(m * numNeighbors + w) * numVarBins + i];
                    if (v > max)
                    {
                        max = v;
                        maxIndex = w;
                    }
                }
                for (int w = 0; w < numNeighbors; w++)
                {
                    for(int i = 0; i < numVarBins; i++)
                        hist[(m * numNeighbors + w) * numVarBins + i] = hist2[(m * numNeighbors + (w + maxIndex) % numNeighbors) * numVarBins + i];
                }
            }

            // normalize the histogram (doesn't need to run on GPU)
            int n = (frame.Width - 2 * radius) * (frame.Height - 2 * radius);
            float[] histNorm = new float[hist.Length];
            for (int i = 0; i < hist.Length; i++) histNorm[i] = (float)hist[i] / n;

            return histNorm;
        }

        /// <summary>
        /// Helper function to convert an LBP pattern to a subuniform pattern
        /// </summary>
        /// <param name="pattern">LBP binary pattern</param>
        /// <returns>uniform LBP bin</returns>
        private static short GetPatternNum(int pattern)
        {
            int u = CountChanges(pattern);
            if (u == 0)
            {
                if (pattern == 0) return (short)(numSubuniformPatterns - 3);
                else return (short)(numSubuniformPatterns - 2);
            }
            else if (u == 2)
            {
                int m = CountOnes(pattern);
                int w = GetRotationIndex(pattern);
                return (short)((m - 1) * numNeighbors + w);
            }
            else
                return (short)(numSubuniformPatterns - 1);
        }

        /// <summary>
        /// Helper function to count the number of changes from 0 to 1 or 1 to 0 in the binary pattern
        /// </summary>
        /// <param name="pattern">LBP binary pattern</param>
        /// <returns>number of binary changes</returns>
        private static int CountChanges(int pattern)
        {
            int size = GetSize(pattern);
            //int size = numPatterns;
            int numChanges = 0;
            int currBit = pattern & 1;
            pattern += currBit << size;
            for (int i = 0; i < size; i++)
            {
                pattern = pattern >> 1;
                int nextBit = pattern & 1;
                if (nextBit != currBit)
                    numChanges++;
                currBit = nextBit;
            }
            return numChanges;
        }

        /// <summary>
        /// Helper function to count the number of "1" bits in a number
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private static int CountOnes(int n)
        {
            int count = 0;
            while (n > 0)
            {
                count++;
                n = n & (n - 1);
            }
            return count;
        }

        /// <summary>
        /// Helper function to identify the rotation of a uniform LBP pattern. The rotation is defined as the bit index where "1" transitions to "0";
        /// </summary>
        /// <param name="pattern">LBP binary pattern</param>
        /// <returns></returns>
        private static int GetRotationIndex(int pattern)
        {
            int size = numPatterns;
            int prevBit = pattern & 1;
            pattern = pattern >> 1;
            for (int i = 0; i < size - 1; i++)
            {
                int currBit = pattern & 1;
                if (currBit < prevBit) return i + 1;
                prevBit = currBit;
            }
            return 0;
        }

        /// <summary>
        /// Helper function to determine the number of bits in a pattern (e.g., 8 for LBP-8, 16 for LBP-16)
        /// </summary>
        /// <param name="pattern">LBP binary pattern</param>
        /// <returns>bit number for the leftmost 1</returns>
        private static int GetSize(int pattern)
        {
            int size = 0;
            int tempPattern = pattern;
            while (tempPattern > 0)
            {
                size++;
                tempPattern = tempPattern >> 1;
            }
            return size;
        }
    }
}
