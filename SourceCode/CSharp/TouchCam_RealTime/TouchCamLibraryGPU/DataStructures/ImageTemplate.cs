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

namespace TouchCamLibrary.ImageProcessing
{
    public class ImageTemplate
    {
        private Dictionary<string, object> info;
        private VideoFrame frame;

        public Image<Gray, byte> Image { get { return frame.Image; } set { frame.Image = value; } }
        public DeviceMemory<byte> ImageGPU { get { return frame.ImageGPU; } set { frame.ImageGPU = value; } }
        public uint Timestamp { get { return frame.Timestamp; } set { frame.Timestamp = value; } }
        public Image<Gray, byte>[] Pyramid;
        public DeviceMemory<byte>[] PyramidGPU;

        private float[] texture, secondaryFeatures;
        private Matrix<float> textureMatrixRow, secondaryFeaturesMatrixRow;
        public float[] Texture { get { return texture; } set { texture = value; if (texture == null) TextureMatrixRow = null; else TextureMatrixRow = Classifier.ArrayToMatrixRow(texture); } }
        public Matrix<float> TextureMatrixRow { get { return textureMatrixRow; } set { textureMatrixRow = value; } }
        public float[] SecondaryFeatures { get { return secondaryFeatures; } set { secondaryFeatures = value; if (secondaryFeatures == null) SecondaryFeaturesMatrixRow = null; else SecondaryFeaturesMatrixRow = Classifier.ArrayToMatrixRow(secondaryFeatures); } }
        public Matrix<float> SecondaryFeaturesMatrixRow { get { return secondaryFeaturesMatrixRow; } set { secondaryFeaturesMatrixRow = value; } }

        private GpuMat keypoints, descriptors;
        private VectorOfKeyPoint keypointVector;
        public GpuMat Keypoints { get { return keypoints; } set { keypoints = value; } }
        public GpuMat Descriptors { get { return descriptors; } set { descriptors = value; } }
        public VectorOfKeyPoint KeypointVector {  get { return keypointVector; } set { keypointVector = value; } }

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

        public ImageTemplate(Image<Gray, byte> img)
            : this()
        {
            frame = new VideoFrame();
            frame.Image = img;
            frame.Timestamp = 0;
            frame.ImageGPU = Worker.Default.Malloc<byte>(img.Width * img.Height);
            frame.ImageGPU.Scatter(img.Bytes);
        }
        
        public ImageTemplate()
        {
            this.frame = new VideoFrame();
            info = new Dictionary<string, object>();
        }

        public ImageTemplate Clone()
        {
            ImageTemplate newTemplate = new ImageTemplate();
            if(Image != null) newTemplate.Image = Image.Clone();
            if (ImageGPU != null)
            {
                newTemplate.ImageGPU = Worker.Default.Malloc<byte>(frame.Width * frame.Height);
                newTemplate.ImageGPU.Scatter(newTemplate.Image.Bytes);
                newTemplate.Timestamp = Timestamp;
            }
            if (Pyramid != null)
            {
                newTemplate.Pyramid = new Image<Gray, byte>[Pyramid.Length];
                for (int i = 0; i < Pyramid.Length; i++)
                    newTemplate.Pyramid[i] = Pyramid[i].Clone();

                if (PyramidGPU != null)
                {
                    newTemplate.PyramidGPU = new DeviceMemory<byte>[PyramidGPU.Length];
                    for (int i = 0; i < PyramidGPU.Length; i++)
                    {
                        newTemplate.PyramidGPU[i] = Worker.Default.Malloc<byte>(PyramidGPU[i].Length);
                        newTemplate.PyramidGPU[i].Scatter(Pyramid[i].Bytes);
                    }
                }
            }

            if (texture != null) newTemplate.texture = (float[])texture.Clone();
            if (secondaryFeatures != null) newTemplate.secondaryFeatures = (float[])secondaryFeatures.Clone();
            if (textureMatrixRow != null) newTemplate.textureMatrixRow = textureMatrixRow.Clone();
            if (secondaryFeaturesMatrixRow != null) newTemplate.secondaryFeaturesMatrixRow = secondaryFeaturesMatrixRow.Clone();
            if (keypoints != null) newTemplate.keypoints = (GpuMat)keypoints.Clone();
            if (descriptors != null) newTemplate.descriptors = (GpuMat)descriptors.Clone();
            if (keypointVector != null) newTemplate.keypointVector = new VectorOfKeyPoint(keypointVector.ToArray());

            return newTemplate;
        }
    }
}
