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
    }
}
