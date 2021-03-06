﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Alea.CUDA;
using Alea.CUDA.Utilities;
using Alea.CUDA.IL;

using Emgu.CV;
using Emgu.CV.Structure;

using LibDevice = Alea.CUDA.LibDevice;

namespace TouchCamLibrary.ImageProcessing
{
    public class VideoFrame
    {
        private int width = -1, height = -1;
        public Image<Gray, byte> Image;
        public DeviceMemory<byte> ImageGPU;
        public uint Timestamp;
        public int Width { get { if (width < 0) width = Image.Width; return width; } }
        public int Height { get { if (height < 0) height = Image.Height; return height; } }

        public VideoFrame Clone()
        {
            VideoFrame newFrame = new VideoFrame();
            newFrame.width = width;
            newFrame.height = height;
            newFrame.Image = Image.Clone();
            newFrame.ImageGPU = Worker.Default.Malloc<byte>(ImageGPU.Size);
            newFrame.ImageGPU.Scatter(newFrame.Image.Bytes);
            return newFrame;
        }
    }
}
