using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Alea.CUDA;
using Alea.CUDA.Utilities;
using Alea.CUDA.IL;

using Emgu.CV;
using Emgu.CV.Cuda;
using Emgu.CV.Structure;
using Emgu.Util;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using Emgu.CV.XFeatures2D;
using System.Diagnostics;

namespace HandSightLibrary.ImageProcessing
{
    public class ImageProcessing
    {
        static CudaSURF surf = new CudaSURF(100);
        static float[] imgScales = new float[] { 1.0f, 2.0f, 3.0f, 4.0f };
        //static float[] imgScales = new float[] { 0.0f, 0.5f, 1.0f, 1.5f, 2.0f, 2.5f, 3.0f, 3.5f, 4.5f, 5.0f };
        //static float[] imgScales = new float[] { 0.0f, 2.5f, 3.5f, 4.0f, 5.0f };
        //static float[] imgScales = new float[] { 2.0f };
        static DeviceMemory<byte>[] pyramid = null;
        static Worker worker = Worker.Default;

        public static void ProcessTemplate(ImageTemplate template, bool extractKeypoints = true)
        {
            if(pyramid == null)
            {
                pyramid = new DeviceMemory<byte>[imgScales.Length];
                for(int i = 0; i < imgScales.Length; i++)
                {
                    double scaleFactor = 1.0 / Math.Pow(2, imgScales[i]);
                    int height = (int)Math.Ceiling((template.Image.Height * scaleFactor) / 4.0) * 4;
                    int width = (int)Math.Ceiling((template.Image.Width * scaleFactor) / 4.0) * 4;
                    pyramid[i] = worker.Malloc<byte>(width * height);
                }
            }

            template.Pyramid = new Image<Gray, byte>[imgScales.Length];
            List<float> texture = new List<float>();

            //List<float> variances = new List<float>();
            float[][] textureScales = new float[imgScales.Length][];

            //for (int i = 0; i < imgScales.Length; i++)
            Parallel.For(0, imgScales.Length, (int i) =>
            {
                double scaleFactor = 1.0 / Math.Pow(2, imgScales[i]);
                int height = (int)Math.Ceiling((template.Image.Height * scaleFactor) / 4.0) * 4;
                int width = (int)Math.Ceiling((template.Image.Width * scaleFactor) / 4.0) * 4;
                //int height = (int)(template.Image.Height * scaleFactor);
                //int width = (int)(template.Image.Width * scaleFactor);
                Image<Gray, byte> temp = new Image<Gray, byte>(width, height);
                CvInvoke.Resize(template.Image, temp, temp.Size);
                template.Pyramid[i] = temp;
                pyramid[i].Scatter(temp.Bytes);

                VideoFrame frame = new VideoFrame() { Image = temp, ImageGPU = pyramid[i] };
                //float[] textureCurrentScale = AdaptiveLBP.GetInstance(temp.Size).GetHistogram(new VideoFrame() { Image = temp, ImageGPU = pyramid[i] });
                float[] textureCurrentScale = LBP.GetInstance(temp.Size).GetHistogram(frame);
                //float[] textureCurrentScale = Utils.ToFloat(Utils.Flatten(OldLBP.GetImageHistogramEfficient(temp)));
                //texture.AddRange(textureCurrentScale);
                textureScales[i] = textureCurrentScale;

                //variances.AddRange(LBP.GetInstance(temp.Size).GetVariances(frame));
            });
            foreach (float[] textureScale in textureScales) texture.AddRange(textureScale);
            template.Texture = texture.ToArray();
            //template["variances"] = variances;

            if (extractKeypoints)
            {
                GpuMat gpuImg = new GpuMat(template.Image);
                GpuMat keypoints = surf.DetectKeyPointsRaw(gpuImg);
                GpuMat descriptors = surf.ComputeDescriptorsRaw(gpuImg, null, keypoints);
                template.Keypoints = keypoints;
                VectorOfKeyPoint keypointVector = new VectorOfKeyPoint();
                surf.DownloadKeypoints(keypoints, keypointVector);
                template.KeypointVector = keypointVector;
                template.Descriptors = descriptors;
            }
        }

        //static Image<Gray, float> mean = new Image<Gray, float>(640, 640), std = new Image<Gray, float>(640, 640), temp = new Image<Gray, float>(640, 640);
        //static CudaCannyEdgeDetector canny = new CudaCannyEdgeDetector(50, 100, 3);
        //static GpuMat edges = new GpuMat(110, 110, DepthType.Cv8U, 1);
        //static Image<Gray, byte> edges = new Image<Gray, byte>(110, 110);
        static Image<Gray, byte> edges = new Image<Gray, byte>(640, 640);
        public static float ImageFocus(ImageTemplate template)
        {
            //MCvScalar mean = new MCvScalar(), std = new MCvScalar();
            //CvInvoke.MeanStdDev(template.Image, ref mean, ref std);
            //return (float)((std.V0 * std.V0) / mean.V0);

            //temp = template.Image.Convert<Gray, float>();
            //CvInvoke.Blur(temp, mean, new Size(3, 3), new Point(-1, -1));

            //Rectangle originalROI = template.Image.ROI;
            //template.Image.ROI = new Rectangle(0, 0, 639, 640);
            //temp.ROI = new Rectangle(1, 0, 639, 640);
            //template.Image.CopyTo(temp);

            //template.Image.ROI = originalROI;
            //temp.ROI = originalROI;

            //CvInvoke.Subtract(template.Image, temp, temp);
            //MCvScalar sum = CvInvoke.Sum(temp);

            //return (float)sum.V0;

            //template.Image.ROI = new Rectangle(265, 265, 110, 110);
            //GpuMat img = new GpuMat(template.Image);

            CvInvoke.Canny(template.Image, edges, 100, 50, 3);
            //canny.Detect(img, edges);
            //double sum = CudaInvoke.AbsSum(edges).V0;
            double sum = CvInvoke.Sum(edges).V0;
            //template.Image.ROI = new Rectangle(0, 0, 640, 640);
            return (float)(sum / 255.0);
        }

        public static float ImageFocus(Image<Gray, byte> img)
        {
            //Rectangle initROI = img.ROI;
            //img.ROI = new Rectangle(265, 265, 110, 110);
            
            CvInvoke.Canny(img, edges, 100, 50, 3);
            edges.ROI = new Rectangle(20, 20, 600, 600);
            double sum = CvInvoke.Sum(edges).V0;
            edges.ROI = new Rectangle(0, 0, 640, 640);
            //img.ROI = initROI;
            return (float)(sum / 255.0);
        }

        public static float ImageFocus2(Image<Gray, byte> img)
        {
            Rectangle prevROI = img.ROI;
            img.ROI = new Rectangle(20, 20, 600, 600);
            Image<Gray, float> tempImg = img.Convert<Gray, float>();
            Image<Gray, float> Gx = new Image<Gray, float>(img.Size), Gy = new Image<Gray, float>(img.Size);
            CvInvoke.Sobel(tempImg, Gx, DepthType.Cv32F, 1, 0, 3);
            CvInvoke.Sobel(tempImg, Gy, DepthType.Cv32F, 0, 1, 3);

            Image<Gray, float> FM = Gx.Mul(Gx).Add(Gy.Mul(Gy));
            double focusMeasure = FM.GetAverage().Intensity;

            img.ROI = prevROI;

            return (float)focusMeasure / (img.Width * img.Height);
        }

        // using the method from http://www.cse.cuhk.edu.hk/leojia/all_final_papers/blur_detect_cvpr08.pdf
        public static float EstimateMotionBlur(Image<Gray, byte> img)
        {
            try
            {
                Rectangle prevROI = img.ROI;
                img.ROI = new Rectangle(20, 20, 600, 600);
                Image<Gray, float> tempImg = img.Convert<Gray, float>();
                Image<Gray, float> Gx = new Image<Gray, float>(img.Size), Gy = new Image<Gray, float>(img.Size);
                CvInvoke.Sobel(tempImg, Gx, DepthType.Cv32F, 1, 0, 3);
                CvInvoke.Sobel(tempImg, Gy, DepthType.Cv32F, 0, 1, 3);

                Image<Gray, float> Gx2 = Gx.Mul(Gx);
                Image<Gray, float> GxGy = Gx.Mul(Gy);
                Image<Gray, float> Gy2 = Gy.Mul(Gy);

                int window = 3;
                Image<Gray, float> M11 = Gx2.SmoothBlur(window, window, false);
                Image<Gray, float> M12 = GxGy.SmoothBlur(window, window, false);
                Image<Gray, float> M22 = Gy2.SmoothBlur(window, window, false);
                // define matrix M = [a,b;c,d]

                // T = a+d
                Image<Gray, float> trace = M11.Add(M22);

                // D = ad-bc
                Image<Gray, float> determinant = (M11.Mul(M22)).Sub(M12.Mul(M12));

                // e1 = T/2 + sqrt(T^2/4-D)
                Image<Gray, float> e1 = trace.Mul(0.5).Add(trace.Mul(trace).Mul(0.25).Sub(determinant).Pow(0.5));

                // e2 = T/2 - sqrt(T^2/4-D)
                Image<Gray, float> e2 = trace.Mul(0.5).Sub(trace.Mul(trace).Mul(0.25).Sub(determinant).Pow(0.5));

                int numAngleBins = 18;
                double[] angleHistogram = new double[numAngleBins];
                int imgWidth = img.Width, imgHeight = img.Height;
                float[,,] M11data = M11.Data, M12data = M12.Data, M22data = M22.Data, e1data = e1.Data, e2data = e2.Data;
                for (int i = 0; i < imgHeight; i++) // TODO: optimize this part better
                    for (int j = 0; j < imgWidth; j++)
                    {
                        float a = M11data[i, j, 0];
                        float b = M12data[i, j, 0];
                        float c = b;
                        float d = M22data[i, j, 0];
                        float L1 = e1data[i, j, 0];
                        float L2 = e2data[i, j, 0];

                        //if(double.IsNaN(L1) || double.IsNaN(L2) || L1 <= 0 || L2 <= 0)
                        //{
                        //    Debug.WriteLine("here");
                        //}

                        double v1x, v1y, v2x, v2y;
                        if (Math.Abs(c) > 1e-5)
                        {
                            v1x = L1 - d;
                            v1y = c;
                            v2x = L2 - d;
                            v2y = c;
                        }
                        else if (Math.Abs(b) > 1e-5)
                        {
                            v1x = b;
                            v1y = L1 - a;
                            v2x = b;
                            v2y = L2 - a;
                        }
                        else
                        {
                            v1x = 1;
                            v1y = 0;
                            v2x = 0;
                            v2y = 1;
                        }

                        double angle = Math.Atan2(v2y, v2x) * 180.0 / Math.PI + 180;
                        int angleBin = (int)(angle / (2 * numAngleBins));
                        if(L1 > 0 && L2 > 0) angleHistogram[angleBin] += Math.Sqrt(L1 / L2);
                    }

                // compute histogram mean
                double mean = 0;
                for (int i = 0; i < numAngleBins; i++) mean += angleHistogram[i];
                mean /= numAngleBins;

                // compute histogram variance
                double var = 0;
                for (int i = 0; i < numAngleBins; i++) var += (angleHistogram[i] - mean) * (angleHistogram[i] - mean);
                var /= numAngleBins;

                img.ROI = prevROI;

                if(double.IsNaN(var))
                {
                    Debug.WriteLine("here");
                }

                return (float)var / (img.Width * img.Height);
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public static float ImageVariance(ImageTemplate template)
        {
            MCvScalar mean = new MCvScalar(), std = new MCvScalar();
            CvInvoke.MeanStdDev(template.Image, ref mean, ref std);
            return (float)(std.V0 * std.V0);
        }

        public static float ImageVariance(Image<Gray, byte> img)
        {
            MCvScalar mean = new MCvScalar(), std = new MCvScalar();
            CvInvoke.MeanStdDev(img, ref mean, ref std);
            return (float)(std.V0 * std.V0);
        }

        public static float ImageBrightness(ImageTemplate template)
        {
            MCvScalar mean = new MCvScalar(), std = new MCvScalar();
            CvInvoke.MeanStdDev(template.Image, ref mean, ref std);
            return (float)mean.V0;
        }

        public static float ImageBrightness(Image<Gray, byte> img)
        {
            MCvScalar mean = new MCvScalar(), std = new MCvScalar();
            CvInvoke.MeanStdDev(img, ref mean, ref std);
            return (float)mean.V0;
        }
    }
}
