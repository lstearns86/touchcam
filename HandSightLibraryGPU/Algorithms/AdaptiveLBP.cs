using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Alea.CUDA;
using Alea.CUDA.Utilities;
using Alea.CUDA.IL;

using Emgu.CV;
using Emgu.CV.Structure;

using LibDevice = Alea.CUDA.LibDevice;

namespace HandSightLibrary.ImageProcessing
{
    public class AdaptiveLBP
    {
        const double PI = Math.PI;
        const int numNeighbors = 8, radius = 1;

        static int numPatterns = (int)Math.Pow(2, numNeighbors);
        static int numUniformPatterns = numNeighbors + 2;

        static short[] uniformBins = null;
        static float[] neighborCoordinateX = null, neighborCoordinateY = null;
        static int[] hist = null;

        static Worker worker = Worker.Default;
        static DeviceMemory<short> lbpImageGPU = null;
        static DeviceMemory<int> histGPU = null;
        static DeviceMemory<short> uniformBinsGPU = null;
        static DeviceMemory<float> neighborCoordinateXGPU = null, neighborCoordinateYGPU = null;
        static LaunchParam lp = null;
        static bool initialized = false;

        /// <summary>
        /// CUDA Kernel that computes the LBP pattern for each image pixel
        /// </summary>
        /// <param name="width">image width</param>
        /// <param name="height">image height</param>
        /// <param name="image">image data (1D byte array)</param>
        /// <param name="lbpImage">pre-allocated lbp image data of same size as image (1D int array)</param>
        /// <param name="uniformBins">lookup table to convert LBP patterns to uniform bins</param>
        [AOTCompile]
        static void LBP_Kernel(int width, int height, deviceptr<byte> image, deviceptr<short> lbpImage, deviceptr<short> uniformBins, deviceptr<float> neighborCoordinateX, deviceptr<float> neighborCoordinateY)
        {
            var x = blockIdx.x * blockDim.x + threadIdx.x;
            var y = blockIdx.y * blockDim.y + threadIdx.y;
            if (x < radius || x >= width - radius || y < radius || y >= height - radius) return;

            int code = 0;
            float v = image[(y) * width + (x)];
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
            }

            //lbpImage[y * width + x] = code;
            lbpImage[y * width + x] = uniformBins[code];
        }

        /// <summary>
        /// CUDA Kernel that computes a histogram over an array of LBP patterns
        /// </summary>
        /// <param name="width">image width</param>
        /// <param name="height">image height</param>
        /// <param name="data">LBP image data, computed in the LBP_Kernel function</param>
        /// <param name="hist">Pre-allocated histogram array for output</param>
        [AOTCompile]
        static void Hist_Kernel(int width, int height, deviceptr<short> data, deviceptr<int> hist)
        {
            var x = blockIdx.x * blockDim.x + threadIdx.x;
            var y = blockIdx.y * blockDim.y + threadIdx.y;
            if (x < radius || x >= width - radius || y < radius || y >= height - radius) return;

            int code = data[(y) * width + (x)];
            Intrinsic.__atomic_add(hist + code, 1);
        }

        /// <summary>
        /// Computes an LBP histogram over the full image
        /// </summary>
        /// <param name="image">an EMGU image</param>
        /// <returns>a normalized histogram of uniform LBP patterns</returns>
        public static float[] GetUniformHistogram(VideoFrame frame)
        {
            if (!initialized) // Note: assumes that the image size will not change
            {
                // initialize data structures to avoid reallocating with every call
                hist = new int[numUniformPatterns];
                lbpImageGPU = worker.Malloc<short>(frame.Width * frame.Height);
                histGPU = worker.Malloc<int>(hist.Length);

                // precompute the uniform bin for each LBP pattern, and push it to the GPU
                uniformBins = new short[(short)Math.Pow(2, numNeighbors)];
                for (int i = 0; i < uniformBins.Length; i++)
                {
                    short bin = GetPatternNum(i);
                    uniformBins[i] = bin;
                }
                uniformBinsGPU = worker.Malloc(uniformBins);

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
            worker.Launch(LBP_Kernel, lp, frame.Width, frame.Height, frame.ImageGPU.Ptr, lbpImageGPU.Ptr, uniformBinsGPU.Ptr, neighborCoordinateXGPU.Ptr, neighborCoordinateYGPU.Ptr);

            // zero out the histogram data on the GPU, then compute a new histogram from the LBP patterns
            for (int i = 0; i < hist.Length; i++) hist[i] = 0;
            histGPU.Scatter(hist);
            worker.Launch(Hist_Kernel, lp, frame.Width, frame.Height, lbpImageGPU.Ptr, histGPU.Ptr);
            histGPU.Gather(hist);

            // normalize the histogram (doesn't need to run on GPU)
            int n = (frame.Width - 2 * radius) * (frame.Height - 2 * radius);
            float[] histNorm = new float[hist.Length];
            for (int i = 0; i < hist.Length; i++) histNorm[i] = (float)hist[i] / n;

            return histNorm;
        }

        /// <summary>
        /// Helper function to convert an LBP pattern to a uniform pattern
        /// </summary>
        /// <param name="pattern">LBP binary pattern</param>
        /// <returns>uniform LBP bin</returns>
        private static short GetPatternNum(int pattern)
        {
            if (CountChanges(pattern) <= 2)
            {
                int numOnes = 0;
                while (pattern > 0)
                {
                    numOnes += pattern & 1;
                    pattern = pattern >> 1;
                }
                return (short)numOnes;
            }
            else
                return (short)(GetSize(pattern) + 1);
        }

        /// <summary>
        /// Helper function to count the number of changes from 0 to 1 or 1 to 0 in the binary pattern
        /// </summary>
        /// <param name="pattern">LBP binary pattern</param>
        /// <returns>number of binary changes</returns>
        private static int CountChanges(int pattern)
        {
            int size = GetSize(pattern);
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
