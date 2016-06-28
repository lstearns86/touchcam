using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Emgu.CV;
using Emgu.CV.Cuda;
using Emgu.CV.Structure;

namespace HandSightLibrary
{
    class LBP
    {
        static int numNeighbors = 12, radius = 2;
        static int[] uniformBins = null;
        public static double[] GetImageHistogramCPU(Image<Gray, byte> image) { return GetImageHistogramCPU(image.ToUMat()); }
        public static double[] GetImageHistogramCPU(UMat image)
        {
            if (uniformBins == null)
            {
                uniformBins = new int[(int)Math.Pow(2, numNeighbors)];
                for (int i = 0; i < uniformBins.Length; i++)
                {
                    int bin = GetPatternNum(i);
                    uniformBins[i] = bin;
                }
            }

            int width = image.Cols, height = image.Rows;

            UMat orig = new UMat();
            image.ConvertTo(orig, Emgu.CV.CvEnum.DepthType.Cv32F);
            UMat[] neighbors = new UMat[numNeighbors];
            UMat patterns = new UMat(height, width, Emgu.CV.CvEnum.DepthType.Cv32F, 1);
            patterns.SetTo(new MCvScalar(0));
            UMat mean = new UMat(height, width, Emgu.CV.CvEnum.DepthType.Cv32F, 1);
            mean.SetTo(new MCvScalar(0));
            for (int i = 0; i < numNeighbors; i++)
            {
                UMat img = new UMat(height, width, Emgu.CV.CvEnum.DepthType.Cv32F, 1);
                Matrix<float> filter = new Matrix<float>(2 * radius + 1, 2 * radius + 1);
                filter.SetZero();

                float x = (float)radius * (float)Math.Cos(2.0 * Math.PI * i / (double)numNeighbors);
                float y = (float)radius * (float)Math.Sin(2.0 * Math.PI * i / (double)numNeighbors);

                // relative indices
                int fx = (int)Math.Floor(x);
                int fy = (int)Math.Floor(y);
                int cx = (int)Math.Ceiling(x);
                int cy = (int)Math.Ceiling(y);

                // fractional part
                float ty = y - fy;
                float tx = x - fx;

                // set interpolation weights
                float w1 = (1 - tx) * (1 - ty);
                float w2 = tx * (1 - ty);
                float w3 = (1 - tx) * ty;
                float w4 = tx * ty;

                filter[fy + radius, fx + radius] = w1;
                if (cx != fx) filter[fy + radius, cx + radius] = w2;
                if (cy != fy) filter[cy + radius, fx + radius] = w3;
                if (cx != fx && cy != fy) filter[cy + radius, cx + radius] = w4;

                CvInvoke.Filter2D(orig, img, filter.ToUMat(), new Point(radius, radius), 0, Emgu.CV.CvEnum.BorderType.Isolated);
                CvInvoke.Subtract(img, orig, img);

                neighbors[i] = img;

                UMat imgThresh = new UMat(height, width, Emgu.CV.CvEnum.DepthType.Cv32F, 1);
                CvInvoke.Threshold(img, imgThresh, 0, (double)(1 << i), Emgu.CV.CvEnum.ThresholdType.Binary);
                CvInvoke.Add(patterns, imgThresh, patterns);
                imgThresh.Dispose();

                CvInvoke.AddWeighted(mean, 1.0, img, 1.0 / numNeighbors, 0, mean);

                filter.Dispose();
            }

            Image<Gray, float> patternImg = patterns.ToImage<Gray, float>();
            
            double[] histogram = new double[numNeighbors + 2];
            int[] counters = new int[numNeighbors + 2];
            Parallel.For(0, width * height, (int i) =>
            {
                int y = i / width;
                int x = i % width;

                {
                    int pattern = (int)Math.Round(patternImg.Data[y, x, 0]);
                    
                    int LBPBin = uniformBins[pattern];
                    Interlocked.Increment(ref counters[LBPBin]);
                }
            });
            for (int i = 0; i < counters.GetLength(0); i++)
                histogram[i] = counters[i];

            patternImg.Dispose();
            mean.Dispose();
            patterns.Dispose();
            foreach (UMat neighbor in neighbors) neighbor.Dispose();
            orig.Dispose();

            return NormalizeHistogram(histogram);
        }

        public static double[] GetImageHistogramGPU(CudaImage<Gray, byte> image)
        {
            if (uniformBins == null)
            {
                uniformBins = new int[(int)Math.Pow(2, numNeighbors)];
                for (int i = 0; i < uniformBins.Length; i++)
                {
                    int bin = GetPatternNum(i);
                    uniformBins[i] = bin;
                }
            }

            int width = image.Size.Width, height = image.Size.Height;

            UMat orig = new UMat();
            image.ConvertTo(orig, Emgu.CV.CvEnum.DepthType.Cv32F);
            UMat[] neighbors = new UMat[numNeighbors];
            UMat patterns = new UMat(height, width, Emgu.CV.CvEnum.DepthType.Cv32F, 1);
            patterns.SetTo(new MCvScalar(0));
            UMat mean = new UMat(height, width, Emgu.CV.CvEnum.DepthType.Cv32F, 1);
            mean.SetTo(new MCvScalar(0));
            for (int i = 0; i < numNeighbors; i++)
            {
                UMat img = new UMat(height, width, Emgu.CV.CvEnum.DepthType.Cv32F, 1);
                Matrix<float> filter = new Matrix<float>(2 * radius + 1, 2 * radius + 1);
                filter.SetZero();

                float x = (float)radius * (float)Math.Cos(2.0 * Math.PI * i / (double)numNeighbors);
                float y = (float)radius * (float)Math.Sin(2.0 * Math.PI * i / (double)numNeighbors);

                // relative indices
                int fx = (int)Math.Floor(x);
                int fy = (int)Math.Floor(y);
                int cx = (int)Math.Ceiling(x);
                int cy = (int)Math.Ceiling(y);

                // fractional part
                float ty = y - fy;
                float tx = x - fx;

                // set interpolation weights
                float w1 = (1 - tx) * (1 - ty);
                float w2 = tx * (1 - ty);
                float w3 = (1 - tx) * ty;
                float w4 = tx * ty;

                filter[fy + radius, fx + radius] = w1;
                if (cx != fx) filter[fy + radius, cx + radius] = w2;
                if (cy != fy) filter[cy + radius, fx + radius] = w3;
                if (cx != fx && cy != fy) filter[cy + radius, cx + radius] = w4;

                CvInvoke.Filter2D(orig, img, filter.ToUMat(), new Point(radius, radius), 0, Emgu.CV.CvEnum.BorderType.Isolated);
                CvInvoke.Subtract(img, orig, img);

                neighbors[i] = img;

                UMat imgThresh = new UMat(height, width, Emgu.CV.CvEnum.DepthType.Cv32F, 1);
                CvInvoke.Threshold(img, imgThresh, 0, (double)(1 << i), Emgu.CV.CvEnum.ThresholdType.Binary);
                CvInvoke.Add(patterns, imgThresh, patterns);
                imgThresh.Dispose();

                CvInvoke.AddWeighted(mean, 1.0, img, 1.0 / numNeighbors, 0, mean);

                filter.Dispose();
            }

            Image<Gray, float> patternImg = patterns.ToImage<Gray, float>();

            double[] histogram = new double[numNeighbors + 2];
            int[] counters = new int[numNeighbors + 2];
            Parallel.For(0, width * height, (int i) =>
            {
                int y = i / width;
                int x = i % width;

                {
                    int pattern = (int)Math.Round(patternImg.Data[y, x, 0]);

                    int LBPBin = uniformBins[pattern];
                    Interlocked.Increment(ref counters[LBPBin]);
                }
            });
            for (int i = 0; i < counters.GetLength(0); i++)
                histogram[i] = counters[i];

            patternImg.Dispose();
            mean.Dispose();
            patterns.Dispose();
            foreach (UMat neighbor in neighbors) neighbor.Dispose();
            orig.Dispose();

            return NormalizeHistogram(histogram);
        }

        public static float[] NormalizeHistogramF(float[] hist)
        {
            float sum = 0;
            foreach (float h in hist) sum += h;
            for (int i = 0; i < hist.Length; i++) hist[i] = hist[i] / sum;
            return hist;
        }

        public static double[] NormalizeHistogram(double[] hist)
        {
            double sum = 0;
            foreach (double h in hist) sum += h;
            for (int i = 0; i < hist.Length; i++) hist[i] = hist[i] / sum;
            return hist;
        }

        public static double[,] NormalizeHistogram(double[,] hist)
        {
            double sum = 0;
            foreach (double h in hist) sum += h;
            for (int i = 0; i < hist.GetLength(0); i++) for (int j = 0; j < hist.GetLength(1); j++) hist[i, j] = hist[i, j] / sum;
            return hist;
        }

        private static int GetPatternNum(int pattern)
        {
            if (CountChanges(pattern) <= 2)
            {
                int numOnes = 0;
                while (pattern > 0)
                {
                    numOnes += pattern & 1;
                    pattern = pattern >> 1;
                }
                return numOnes;
            }
            else
                return GetSize(pattern) + 1;
        }

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
