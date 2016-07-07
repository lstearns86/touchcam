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
using Emgu.CV.Structure;
using Emgu.Util;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using Emgu.CV.XFeatures2D;

namespace HandSightLibrary
{
    public class ImageProcessing
    {
        static CudaSURF surf = new CudaSURF(50);
        static float[] imgScales = new float[] { 0.5f, 1.0f, 1.5f, 2.0f, 2.5f, 3.0f, 3.5f, 4.0f };

        public static void ProcessTemplate(ImageTemplate template, bool extractKeypoints = true)
        {
            
        }
    }
}
