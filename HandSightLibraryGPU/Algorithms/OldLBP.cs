using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

using Emgu.CV;
using Emgu.CV.Structure;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace HandSightLibrary
{
    public class OldLBP
    {
        #region Constants

        public static int NUM_VAR_BINS = 16;
        //public static int NUM_VAR_BINS = 8;

        private static double const8LL = (1.0 - Math.Sqrt(2) / 2.0) * (1.0 - Math.Sqrt(2) / 2.0);
        private static double const8LR = (Math.Sqrt(2) / 2.0) * (1.0 - Math.Sqrt(2) / 2.0);
        private static double const8UL = const8LR;
        private static double const8UR = 1.0 / 2.0;

        private static double const16LL1 = (2.0 - 2.0 * Math.Cos(Math.PI / 8.0)) * (1.0 - 2.0 * Math.Sin(Math.PI / 8.0));
        private static double const16LR1 = (2.0 * Math.Cos(Math.PI / 8.0) - 1.0) * (1.0 - 2.0 * Math.Sin(Math.PI / 8.0));
        private static double const16UL1 = (2.0 - 2.0 * Math.Cos(Math.PI / 8.0)) * (2.0 * Math.Sin(Math.PI / 8.0));
        private static double const16UR1 = (2.0 * Math.Cos(Math.PI / 8.0) - 1.0) * (2.0 * Math.Sin(Math.PI / 8.0));

        private static double const16LL2 = (2.0 - 2.0 * Math.Cos(Math.PI / 4.0)) * (2.0 - 2.0 * Math.Sin(Math.PI / 4.0));
        private static double const16LR2 = (2.0 * Math.Cos(Math.PI / 4.0) - 1.0) * (2.0 - 2.0 * Math.Sin(Math.PI / 4.0));
        private static double const16UL2 = (2.0 - 2.0 * Math.Cos(Math.PI / 4.0)) * (2.0 * Math.Sin(Math.PI / 4.0) - 1.0);
        private static double const16UR2 = (2.0 * Math.Cos(Math.PI / 4.0) - 1.0) * (2.0 * Math.Sin(Math.PI / 4.0) - 1.0);

        private static double const16LL3 = (1.0 - 2.0 * Math.Cos(3.0 * Math.PI / 8.0)) * (2.0 - 2.0 * Math.Sin(3.0 * Math.PI / 8.0));
        private static double const16LR3 = (2.0 * Math.Cos(3.0 * Math.PI / 8.0)) * (2.0 - 2.0 * Math.Sin(3.0 * Math.PI / 8.0));
        private static double const16UL3 = (1.0 - 2.0 * Math.Cos(3.0 * Math.PI / 8.0)) * (2.0 * Math.Sin(3.0 * Math.PI / 8.0) - 1.0);
        private static double const16UR3 = (2.0 * Math.Cos(3.0 * Math.PI / 8.0)) * (2.0 * Math.Sin(3.0 * Math.PI / 8.0) - 1.0);

        #endregion Constants

        //public static byte[,] ImageToMatrix(Image<Gray, byte> img)
        //{
        //    byte[,] mat = new byte[img.Height, img.Width];
        //    for (int i = 0; i < img.Height; i++)
        //        for (int j = 0; j < img.Width; j++)
        //            //mat[i, j] = (byte)img[i, j].Intensity;
        //            mat[i, j] = img.Data[i, j, 0];

        //    return mat;
        //}

        public static List<double> GetImageVariances(Image<Gray, byte> image)
        {
            List<double> variances = new List<double>();
            for (int y = 0; y < image.Height; y++)
                for (int x = 0; x < image.Width; x++)
                    variances.Add(VAR16(image, x, y));

            return variances;
        }

        public static float[] GetImageHistogramF(Image<Gray, byte> image)
        {
            float[] histogram = new float[18];
            int[] counters = new int[18];

            int width = image.Width;
            int height = image.Height;
            Parallel.For(0, width * height, (int index) =>
            {
                int x = index % width;
                int y = index / width;
                int LBPBin = LBP16U(image, x, y);
                Interlocked.Increment(ref counters[LBPBin]);
                //histogram[LBPBin]++;
            });
            for (int i = 0; i < counters.Length; i++) histogram[i] = counters[i];

            return NormalizeHistogramF(histogram);
        }

        public static double[] GetImageHistogram(Image<Gray, byte> image)
        {
            double[] histogram = new double[18];
            int[] counters = new int[18];

            int width = image.Width;
            int height = image.Height;
            Parallel.For(0, width * height, (int index) =>
            {
                int x = index % width;
                int y = index / width;
                int LBPBin = LBP16U(image, x, y);
                Interlocked.Increment(ref counters[LBPBin]);
                //histogram[LBPBin]++;
            });
            for (int i = 0; i < counters.Length; i++) histogram[i] = counters[i];

            return NormalizeHistogram(histogram);
        }

        //static int checkX = 100, checkY = 100;
        public static double[,] GetImageHistogram(Image<Gray, byte> image, List<double> varBinCuts, Image<Gray, byte> mask = null)
        {
            double[,] histogram = new double[18, NUM_VAR_BINS];
            int[,] counters = new int[18, NUM_VAR_BINS];

            int width = image.Width;
            int height = image.Height;
            if (mask == null) { mask = new Image<Gray, byte>(width, height); mask.SetValue(1); }
            byte[,,] maskData = mask.Data;
            Parallel.For(0, width * height, (int index) =>
            //for (int y = 0; y < image.GetLength(0); y++)
            //    for (int x = 0; x < image.GetLength(1); x++)
            {
                int x = index % width;
                int y = index / width;
                if (maskData[y, x, 0] > 0)
                {
                    int LBPBin = LBP16U(image, x, y);
                    int VARBin = GetBin(VAR16(image, x, y), varBinCuts);
                    Interlocked.Increment(ref counters[LBPBin, VARBin]);
                    //histogram[LBPBin, VARBin]++;
                }
            });
            for (int i = 0; i < counters.GetLength(0); i++)
                for (int j = 0; j < counters.GetLength(1); j++)
                    histogram[i, j] = counters[i, j];

            return NormalizeHistogram(histogram);
        }

        public static Image<Gray, byte> VisualizeLBP(Image<Gray, byte> image, List<double> varBinCuts)
        {
            int numNeighbors = 12, radius = 2;
            int width = image.Width, height = image.Height;

            UMat orig = image.Convert<Gray, float>().ToUMat();
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

                CvInvoke.AddWeighted(mean, 1.0, img, 1.0 / numNeighbors, 0, mean);
            }

            UMat variances = new UMat(height, width, Emgu.CV.CvEnum.DepthType.Cv32F, 1);
            variances.SetTo(new MCvScalar(0));
            for (int i = 0; i < numNeighbors; i++)
            {
                UMat img = neighbors[i];
                CvInvoke.Subtract(img, mean, img);
                CvInvoke.Multiply(img, img, img);
                CvInvoke.AddWeighted(variances, 1.0, img, 1.0 / numNeighbors, 0, variances);
            }

            Image<Gray, float> patternImg = patterns.ToImage<Gray, float>();
            Image<Gray, float> varImg = variances.ToImage<Gray, float>();

            return patternImg.Convert<Gray, byte>();
        }

        static int numNeighbors = 12, radius = 2;
        //static int numNeighbors = 8, radius = 1;
        static int[] uniformBins = null;
        static List<double> varBinCuts = new List<double>(new double[] { double.NegativeInfinity, 1.6113763094439419, 8.4662133430334876, 19.068693504197039, 32.931756363447185, 50.806257223632748, 73.925768389128791, 103.99872371684611, 143.55929591350352, 196.37682253643587, 268.634109897359, 371.5463598630414, 526.48493535337172, 780.620183093791, 1280.9687670261649, 11866.117177664119, double.PositiveInfinity });
        //static List<double> varBinCuts = new List<double>(new double[] { float.NegativeInfinity, 0.1530831107286976f, 0.3703053669767446f, 1.0091557115025451f, 2.6608580363204877f, 9.007633262523651f, 100f, 11681.98538285752f, float.PositiveInfinity });
        //static List<double> varBinCuts = new List<double>(new double[] { float.NegativeInfinity, 0.07696558913089005f, 0.12761939488119856f, 0.20253442626276127f, 0.2929285576959671f, 0.494220705074244f, 0.78565825079749674f, 1.2305471351723409f, 1.9189949699948552f, 3.0559455449911184f, 5.2160133559361048f, 9.9662062912596685f, 23.152473028030922f, 50, 1000, float.PositiveInfinity });
        public static double[,] GetImageHistogramEfficient(Image<Gray, byte> image, Image<Gray, byte> mask = null) { return GetImageHistogramEfficient(image.ToUMat(), mask); }
        public static double[,] GetImageHistogramEfficient(UMat image, Image<Gray, byte> mask = null)
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

            //int numNeighbors = 12, radius = 2;
            //int numNeighbors = 8, radius = 1;
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

            UMat variances = new UMat(height, width, Emgu.CV.CvEnum.DepthType.Cv32F, 1);
            variances.SetTo(new MCvScalar(0));
            for (int i = 0; i < numNeighbors; i++)
            {
                UMat img = neighbors[i];
                CvInvoke.Subtract(img, mean, img);
                CvInvoke.Multiply(img, img, img);
                CvInvoke.AddWeighted(variances, 1.0, img, 1.0 / numNeighbors, 0, variances);
            }

            Image<Gray, float> patternImg = patterns.ToImage<Gray, float>();
            Image<Gray, float> varImg = variances.ToImage<Gray, float>();

            double[,] histogram = new double[numNeighbors + 2, NUM_VAR_BINS];
            int[,] counters = new int[numNeighbors + 2, NUM_VAR_BINS];
            //double[,] histogram = new double[(int)Math.Pow(2, numNeighbors), 1];
            //int[,] counters = new int[(int)Math.Pow(2, numNeighbors), 1];
            byte[,,] maskData = mask == null ? null : mask.Data;
            Parallel.For(0, width * height, (int i) =>
            {
                int y = i / width;
                int x = i % width;

                if (mask == null || maskData[y, x, 0] > 0)
                {
                    int pattern = (int)Math.Round(patternImg.Data[y, x, 0]);
                    double variance = varImg.Data[y, x, 0];
                    if (double.IsNaN(variance)) variance = 0;

                    int LBPBin = uniformBins[pattern];
                    //int LBPBin = pattern;
                    //int LBPBin = GetPatternNum(pattern);
                    //int VARBin = 0;
                    int VARBin = GetBin(variance, varBinCuts);
                    Interlocked.Increment(ref counters[LBPBin, VARBin]);
                }
            });
            for (int i = 0; i < counters.GetLength(0); i++)
                for (int j = 0; j < counters.GetLength(1); j++)
                    histogram[i, j] = counters[i, j];

            patternImg.Dispose();
            varImg.Dispose();
            variances.Dispose();
            mean.Dispose();
            patterns.Dispose();
            foreach (UMat neighbor in neighbors) neighbor.Dispose();
            orig.Dispose();

            return NormalizeHistogram(histogram);
        }

        public static double[] GetImageHistogramSimpleAndEfficient(Image<Gray, byte> image, Image<Gray, byte> mask = null)
        {
            int numNeighbors = 12, radius = 2;
            int width = image.Width, height = image.Height;

            UMat orig = image.Convert<Gray, float>().ToUMat();
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

                CvInvoke.AddWeighted(mean, 1.0, img, 1.0 / numNeighbors, 0, mean);
            }

            Image<Gray, float> patternImg = patterns.ToImage<Gray, float>();

            //int n = (int)Math.Pow(2, numNeighbors);
            int n = numNeighbors + 2;
            double[] histogram = new double[n];
            int[] counters = new int[n];
            byte[,,] maskData = mask == null ? null : mask.Data;
            Parallel.For(0, width * height, (int i) =>
            {
                int y = i / width;
                int x = i % width;

                if (mask == null || maskData[y, x, 0] > 0)
                {
                    int pattern = (int)Math.Round(patternImg.Data[y, x, 0]);

                    //int LBPBin = pattern;
                    int LBPBin = GetPatternNum(pattern);
                    if (LBPBin >= n)
                        Debug.WriteLine("Error");
                    Interlocked.Increment(ref counters[LBPBin]);
                }
            });
            for (int i = 0; i < counters.GetLength(0); i++)
                histogram[i] = counters[i];

            return NormalizeHistogram(histogram);
        }

        public static double GetMatchLikelihood(double[] h1, double[] h2)
        {
            return LogLikelihood(h1, h2);
        }

        public static double LogLikelihood(double[] sample, double[] model)
        {
            double sum = 0.0;
            for (int i = 0; i < sample.Length; i++)
                sum += sample[i] * Math.Log(model[i], 2);

            return sum;
        }

        public static double LogLikelihood(double[,] sample, double[,] model)
        {
            double sum = 0.0;
            for (int i = 0; i < sample.GetLength(0); i++)
                for (int j = 0; j < sample.GetLength(1); j++)
                    sum += sample[i, j] * Math.Log(model[i, j], 2);

            return sum;
        }

        public static double DistanceL2Sqr(double[] sample, double[] model)
        {
            double dist = 0;
            for (int i = 0; i < sample.Length; i++)
                dist += (sample[i] - model[i]) * (sample[i] - model[i]);
            return dist;
        }

        public static double DistanceL2Sqr(double[,] sample, double[,] model)
        {
            double dist = 0;
            for (int i = 0; i < sample.GetLength(0); i++)
                for (int j = 0; j < sample.GetLength(1); j++)
                    dist += (sample[i, j] - model[i, j]) * (sample[i, j] - model[i, j]);
            return dist;
        }

        public static List<double> CalculateBins(List<double> varList)
        {
            List<double> varBinCuts = new List<double>();

            varList.Sort();
            int itemsPerBin = varList.Count / (NUM_VAR_BINS - 1);

            varBinCuts.Add(double.NegativeInfinity);
            for (int i = itemsPerBin; i < varList.Count; i += itemsPerBin)
                varBinCuts.Add(varList[i]);
            varBinCuts.Add(double.PositiveInfinity);

            return varBinCuts;
        }

        //public static int GetBin(double value, List<double> varBinCuts)
        //{
        //    for (int i = 0; i + 1 < varBinCuts.Count; i++)
        //        if (value >= varBinCuts[i] && value < varBinCuts[i + 1])
        //            return i;
        //    return -1;
        //}

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

        private static int LBP8(Image<Gray, byte> image, int x, int y)
        {
            byte[,,] data = image.Data;

            double[] values = getValues8(image, x, y);

            // subtract pixel value for brightness invariance
            for (int i = 0; i < 8; i++)
                values[i] -= data[y, x, 0];

            // convert the values into a binary pattern
            int pattern = 0;
            for (int i = 0; i < 8; i++)
                if (values[i] > 0)
                    pattern += 1 << i;

            return pattern;
        }

        private static int LBP8U(Image<Gray, byte> image, int x, int y)
        {
            byte[,,] data = image.Data;

            double[] values = getValues8(image, x, y);

            // subtract pixel value for brightness invariance
            for (int i = 0; i < 8; i++)
                values[i] -= data[y, x, 0];

            // convert the values into a binary pattern
            int pattern = 0;
            for (int i = 0; i < 8; i++)
                if (values[i] > 0)
                    pattern += 1 << i;

            // return the uniform pattern number 0-8 corresponding to the number of values greater 
            // than the center pixel, or 9 for a non-uniform pattern
            return GetPatternNum(pattern);
        }

        private static int LBP16(Image<Gray, byte> image, int x, int y)
        {
            byte[,,] data = image.Data;

            double[] values = getValues16(image, x, y);

            // subtract pixel value for brightness invariance
            for (int i = 0; i < 16; i++)
                values[i] -= data[y, x, 0];

            // convert the values into a binary pattern
            int pattern = 0;
            for (int i = 0; i < 16; i++)
                if (values[i] > 0)
                    pattern += 1 << i;

            return pattern;
        }

        private static int LBP16U(Image<Gray, byte> image, int x, int y)
        {
            byte[,,] data = image.Data;

            double[] values = getValues16(image, x, y);

            // subtract pixel value for brightness invariance
            for (int i = 0; i < 16; i++)
                values[i] -= data[y, x, 0];

            // convert the values into a binary pattern
            int pattern = 0;
            for (int i = 0; i < 16; i++)
                if (values[i] > 0)
                    pattern += 1 << i;

            // return the uniform pattern number 0-16 corresponding to the number of values greater 
            // than the center pixel, or 17 for a non-uniform pattern
            return GetPatternNum(pattern);
        }

        private static double VAR8(Image<Gray, byte> image, int x, int y)
        {
            double[] values = getValues8(image, x, y);

            double averageValue = 0;
            foreach (double value in values)
                averageValue += value;
            averageValue /= 8.0;

            double var = 0;
            foreach (double value in values)
                var += (value - averageValue) * (value - averageValue);
            var /= 8.0;

            return var;
        }

        private static double VAR16(Image<Gray, byte> image, int x, int y)
        {
            double[] values = getValues16(image, x, y);

            double averageValue = 0;
            foreach (double value in values)
                averageValue += value;
            averageValue /= 16.0;

            double var = 0;
            foreach (double value in values)
                var += (value - averageValue) * (value - averageValue);
            var /= 16.0;

            return var;
        }

        private static double[] getValues8(Image<Gray, byte> image, int x, int y)
        {
            byte[,,] data = image.Data;
            int width = image.Width;
            int height = image.Height;

            // get the values of the 9 pixel block surrounding the specified pixel location (circular)
            byte[,] localMap = new byte[3, 3];
            for (int j = -1; j <= 1; j++)
                for (int i = -1; i <= 1; i++)
                {
                    int cx, cy;
                    if (x + i < 0) cx = width + (x + i);
                    else if (x + i >= width) cx = x + i - width;
                    else cx = x + i;
                    if (y + j < 0) cy = height + (y + j);
                    else if (y + j >= height) cy = y + j - height;
                    else cy = y + j;
                    localMap[j + 1, i + 1] = data[cy, cx, 0];
                }

            // copy the values in a circle to an array, interpolate for the diagonal pixels
            double[] values = new double[8];
            values[0] = localMap[1, 2];
            values[1] = localMap[1, 1] * const8LL + localMap[1, 2] * const8LR + localMap[2, 1] * const8UL + localMap[2, 2] * const8UR;
            values[2] = localMap[2, 1];
            values[3] = localMap[1, 1] * const8LL + localMap[1, 0] * const8LR + localMap[2, 1] * const8UL + localMap[2, 0] * const8UR;
            values[4] = localMap[1, 0];
            values[5] = localMap[1, 1] * const8LL + localMap[1, 0] * const8LR + localMap[0, 1] * const8UL + localMap[0, 0] * const8UR;
            values[6] = localMap[0, 1];
            values[7] = localMap[1, 1] * const8LL + localMap[1, 2] * const8LR + localMap[0, 1] * const8UL + localMap[0, 2] * const8UR;

            return values;
        }

        private static double[] getValues16(Image<Gray, byte> image, int x, int y)
        {
            byte[,,] data = image.Data;
            int width = data.GetLength(1); // much faster than image.Width for some reason
            int height = data.GetLength(0);

            // get the values of the 25 pixel block surrounding the specified pixel location
            byte[,] localMap = new byte[5, 5];
            for (int j = -2; j <= 2; j++)
                for (int i = -2; i <= 2; i++)
                {
                    int cx, cy;
                    if (x + i < 0) cx = width + (x + i);
                    else if (x + i >= width) cx = x + i - width;
                    else cx = x + i;
                    if (y + j < 0) cy = height + (y + j);
                    else if (y + j >= height) cy = y + j - height;
                    else cy = y + j;
                    localMap[j + 2, i + 2] = data[cy, cx, 0];
                }

            // copy the values in a circle to an array, interpolate for the diagonal pixels
            double[] values = new double[16];
            values[0] = localMap[2, 4];
            values[1] = localMap[2, 3] * const16LL1 + localMap[2, 4] * const16LR1 + localMap[3, 3] * const16UL1 + localMap[3, 4] * const16UR1;
            values[2] = localMap[3, 3] * const16LL2 + localMap[3, 4] * const16LR2 + localMap[4, 3] * const16UL2 + localMap[4, 4] * const16UR2;
            values[3] = localMap[3, 2] * const16LL3 + localMap[3, 3] * const16LR3 + localMap[4, 2] * const16UL3 + localMap[4, 3] * const16UR3;
            values[4] = localMap[4, 2];
            values[5] = localMap[3, 2] * const16LL3 + localMap[3, 1] * const16LR3 + localMap[4, 2] * const16UL3 + localMap[4, 1] * const16UR3;
            values[6] = localMap[3, 1] * const16LL2 + localMap[3, 0] * const16LR2 + localMap[4, 1] * const16UL2 + localMap[4, 0] * const16UR2;
            values[7] = localMap[2, 1] * const16LL1 + localMap[2, 0] * const16LR1 + localMap[3, 1] * const16UL1 + localMap[3, 0] * const16UR1;
            values[8] = localMap[2, 0];
            values[9] = localMap[2, 1] * const16LL1 + localMap[2, 0] * const16LR1 + localMap[1, 1] * const16UL1 + localMap[1, 0] * const16UR1;
            values[10] = localMap[1, 1] * const16LL2 + localMap[1, 0] * const16LR2 + localMap[0, 1] * const16UL2 + localMap[0, 0] * const16UR2;
            values[11] = localMap[1, 2] * const16LL3 + localMap[1, 1] * const16LR3 + localMap[0, 2] * const16UL3 + localMap[0, 1] * const16UR3;
            values[12] = localMap[0, 2];
            values[13] = localMap[1, 2] * const16LL3 + localMap[1, 3] * const16LR3 + localMap[0, 2] * const16UL3 + localMap[0, 3] * const16UR3;
            values[14] = localMap[1, 3] * const16LL2 + localMap[1, 4] * const16LR2 + localMap[0, 3] * const16UL2 + localMap[0, 4] * const16UR2;
            values[15] = localMap[2, 3] * const16LL1 + localMap[2, 4] * const16LR1 + localMap[1, 3] * const16UL1 + localMap[1, 4] * const16UR1;

            return values;
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

        public static int GetBin(double value, List<double> varBinCuts)
        {
            for (int i = 0; i + 1 < varBinCuts.Count; i++)
                if (value >= varBinCuts[i] && value < varBinCuts[i + 1])
                    return i;
            return -1;
        }
    }
}
