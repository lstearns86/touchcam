using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Awaiba.Drivers.Grabbers;
using Awaiba.FrameProcessing;

using Emgu.CV;
using Emgu.CV.Cuda;
using Emgu.CV.Structure;

namespace HandSightLibrary
{
    public class Camera
    {
        int prescaler = 1;
        int exposure = 255;
        int cam = 0;
        IduleProviderCsCam provider;

        bool calibrating = false;
        bool correctBrightness = true;

        CudaImage<Gray, float> meanImg = null;
        CudaImage<Gray, float> correctionFactor = new CudaImage<Gray, float>(640, 640);
        float numCalibrationSamples = 0;

        static Camera instance;
        public static Camera Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Camera();
                }
                return instance;
            }
        }

        public delegate void FrameAvailableDelegate(CudaImage<Gray, float> frame, uint timestamp);
        public event FrameAvailableDelegate FrameAvailable;
        private void OnFrameAvailable(CudaImage<Gray, float> frame, uint timestamp) { if (FrameAvailable != null) FrameAvailable(frame, timestamp); }

        public delegate void ErrorDelegate(string msg);
        public event ErrorDelegate Error;
        private void OnError(string msg) { if (Error != null) Error(msg); }

        public int Brightness
        {
            get
            {
                return prescaler;
            }
            set
            {
                prescaler = value;
                provider.WriteRegister(new NanEyeGSRegisterPayload(false, 0x05, true, 0, prescaler));
            }
        }

        Camera()
        {
            try
            {
                correctionFactor.SetTo(new MCvScalar(1));

                provider = new IduleProviderCsCam(0);
                provider.Initialize();
                provider.ImageTransaction += provider_ImageTransaction;
                provider.Interrupt += provider_Interrupt;
                provider.Exception += camera_Exception;
                provider.WriteRegister(new NanEyeGSRegisterPayload(false, 0x05, true, 0, prescaler));
                provider.WriteRegister(new NanEyeGSRegisterPayload(false, 0x06, true, 0, exposure));
            }
            catch (Exception ex) { OnError(ex.Message); }
        }

        ~Camera()
        {
            if (provider.IsConnected && provider.IsCapturing)
            {
                provider.StopCapture();
                provider.Dispose();
            }
        }

        public bool IsConnected
        {
            get
            {
                return provider.IsConnected;
            }
        }

        public bool IsCapturing
        {
            get
            {
                return provider.IsCapturing;
            }
        }

        public void Connect()
        {
            try
            {
                stopping = false;
                provider.StartCapture();
            }
            catch (Exception ex) { OnError(ex.Message); }
        }

        bool stopping = false;
        public void Disconnect()
        {
            try
            {
                stopping = true;
                provider.StopCapture();
            }
            catch (Exception ex) { OnError(ex.Message); }
        }

        public void StartCalibration()
        {
            calibrating = true;
            meanImg = null;
            numCalibrationSamples = 0;
        }

        public void StopCalibration()
        {
            calibrating = false;
        }

        void provider_Interrupt(object sender, InterruptEventArgs e)
        {
            OnError("Camera provider interrupted: flags=" + e.Flags);
        }

        string lockObj = "lock object";
        Image<Gray, float> img = new Image<Gray, float>(640, 640);
        CudaImage<Gray, float> cudaImg = new CudaImage<Gray, float>(640, 640);
        CudaImage<Gray, float> thresholdImg = new CudaImage<Gray, float>(640, 640);
        CudaImage<Gray, byte> saturationMask = new CudaImage<Gray, byte>(640, 640);
        private void provider_ImageTransaction(object sender, ImageReceivedEventArgs e)
        {
            if (stopping || e == null || e.PixelData == null) return;

            float[, ,] data = img.Data;
            for (int y = 0; y < e.Height; y++)
                for (int x = 0; x < e.Width; x++)
                {
                    byte b = e.PixelData[y * e.Width + x];
                    data[y, x, 0] = b;
                }
            cudaImg.Upload(img);

            if (calibrating)
            {
                if (meanImg == null) meanImg = new CudaImage<Gray, float>(640, 640);
                CudaInvoke.AddWeighted(meanImg, numCalibrationSamples / (numCalibrationSamples + 1), cudaImg, 1.0f / (numCalibrationSamples + 1), 0, meanImg);
                numCalibrationSamples++;

                MCvScalar mean = new MCvScalar(), std = new MCvScalar();
                CudaImage<Gray, byte> tempImg = meanImg.Convert<Gray, byte>();
                CudaInvoke.MeanStdDev(tempImg, ref mean, ref std);
                tempImg.Dispose();
                if (correctionFactor == null) correctionFactor = new CudaImage<Gray, float>(meanImg.Size);
                correctionFactor.SetTo(new MCvScalar(mean.V0));
                CudaInvoke.Divide(correctionFactor, meanImg, correctionFactor);
            }

            if (correctBrightness)
            {
                CudaInvoke.Multiply(correctionFactor, cudaImg, cudaImg);
                CudaInvoke.Threshold(cudaImg, thresholdImg, 255, 255, Emgu.CV.CvEnum.ThresholdType.Binary);
                thresholdImg.ConvertTo(saturationMask, Emgu.CV.CvEnum.DepthType.Cv8U);
                cudaImg.SetTo(new MCvScalar(255), saturationMask);
            }

            OnFrameAvailable(cudaImg, e.TimeStamp);
        }

        private void camera_Exception(object sender, OnExceptionEventArgs e)
        {
            OnError("Camera exception: " + e.ex.Message);
        }
    }
}
