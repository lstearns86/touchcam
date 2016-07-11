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
using Emgu.CV.Util;

namespace HandSightLibrary.ImageProcessing
{
    public class ImageTemplate
    {
        private Dictionary<string, object> info;
        private VideoFrame frame;

        public Image<Gray, byte> Image { get { return frame.Image; } set { frame.Image = value; } }
        public DeviceMemory<byte> ImageGPU { get { return frame.ImageGPU; } set { frame.ImageGPU = value; } }
        public uint Timestamp { get { return frame.Timestamp; } set { frame.Timestamp = value; } }
        public CudaImage<Gray, byte>[] Pyramid;

        private float[] texture, secondaryFeatures;
        private Matrix<float> textureMatrixRow, secondaryFeaturesMatrixRow;
        public float[] Texture { get { return texture; } set { texture = value; if (texture == null) TextureMatrixRow = null; else TextureMatrixRow = Classifier.ArrayToMatrixRow(texture); } }
        public Matrix<float> TextureMatrixRow { get { return textureMatrixRow; } set { textureMatrixRow = value; } }
        public float[] SecondaryFeatures { get { return secondaryFeatures; } set { secondaryFeatures = value; if (secondaryFeatures == null) SecondaryFeaturesMatrixRow = null; else SecondaryFeaturesMatrixRow = Classifier.ArrayToMatrixRow(secondaryFeatures); } }
        public Matrix<float> SecondaryFeaturesMatrixRow { get { return secondaryFeaturesMatrixRow; } set { secondaryFeaturesMatrixRow = value; } }

        public object this[string key]
        {
            get
            {
                key = key.ToLower();
                if (info.ContainsKey(key)) return info[key];
                else return "";
            }
            set
            {
                info[key.ToLower()] = value;
            }
        }

        public ImageTemplate(VideoFrame frame)
            : this()
        {
            this.frame = frame;
        }
        
        public ImageTemplate()
        {
            info = new Dictionary<string, object>();
        }
    }
}
