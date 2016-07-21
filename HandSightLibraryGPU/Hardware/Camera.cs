using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Alea.CUDA;
using Alea.CUDA.Utilities;
using Alea.CUDA.IL;
using Alea.CUDA.Unbound;

using Awaiba.Drivers.Grabbers;
using Awaiba.FrameProcessing;

using Emgu.CV;
using Emgu.CV.Cuda;
using Emgu.CV.Structure;
using System.Threading;
using System.IO;

namespace HandSightLibrary.ImageProcessing
{
    public class Camera
    {
        const int numPixels = 640 * 640;
        int prescaler = 1;
        int exposure = 255;
        IduleProviderCsCam provider;

        // reusable image object to avoid allocating a new one every frame
        // note: this could cause problems if attempting to multithread, or save an image for later
        // you should always create a clone if you want to use this beyond the current frame
        Image<Gray, byte> img = new Image<Gray, byte>(640, 640);

        // variables for calibration and brightness correction (using CUDA)
        bool correctBrightness = true;
        bool calibrating = false;
        Worker worker = Worker.Default;
        LaunchParam lp = null;
        DeviceMemory<float> meanImg = null;
        DeviceMemory<float> correctionFactor = null;
        float numCalibrationSamples = 0;
        DeviceMemory<byte> imgGPU = null;
        DeviceMemory<float> scalarOutput = null;
        DeviceReduce<float> addReduce = null;

        // singleton instance
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

        // public events
        public delegate void FrameAvailableDelegate(VideoFrame frame);
        public event FrameAvailableDelegate FrameAvailable;
        private void OnFrameAvailable(VideoFrame frame) { if (FrameAvailable != null) FrameAvailable(frame); }

        public delegate void ErrorDelegate(string msg);
        public event ErrorDelegate Error;
        private void OnError(string msg) { if (Error != null) Error(msg); }

        /// <summary>
        /// The camera's pre-scaler brightness, between 0-255. Note that the higher this is, the lower the frame rate will be.
        /// </summary>
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

        /// <summary>
        /// Private Constructor
        /// </summary>
        Camera()
        {
            try
            {
                // pre-allocate and initialize the variables and data structures for brightness correction
                imgGPU = worker.Malloc<byte>(new byte[640 * 640]);
                meanImg = worker.Malloc<float>(new float[640 * 640]);
                addReduce = DeviceSumModuleF32.Default.Create(numPixels);
                correctionFactor = worker.Malloc<float>(640 * 640);
                float[] temp = new float[640 * 640];
                for (int i = 0; i < temp.Length; i++) temp[i] = 1;
                correctionFactor.Scatter(temp);
                scalarOutput = worker.Malloc<float>(1);

                if(File.Exists("correctionFactor.dat"))
                {
                    FileStream stream = new FileStream("correctionFactor.dat", FileMode.Open);
                    byte[] buffer = new byte[640 * 640 * 4];
                    stream.Read(buffer, 0, (int)Math.Min(buffer.Length, stream.Length));
                    for (int i = 0; i < 640 * 640; i++) temp[i] = BitConverter.ToSingle(buffer, 4 * i);
                    stream.Close();

                    correctionFactor.Scatter(temp);
                }
                
                // initialize CUDA parameters
                var blockDims = new dim3(32, 32);
                var gridDims = new dim3(Common.divup(640, blockDims.x), Common.divup(640, blockDims.y));
                lp = new LaunchParam(gridDims, blockDims);

                // set up the camera parameters and events
                provider = new IduleProviderCsCam(0);
                provider.Initialize();
                provider.ImageTransaction += provider_ImageTransaction;
                provider.Interrupt += provider_Interrupt;
                provider.Exception += camera_Exception;
                provider.WriteRegister(new NanEyeGSRegisterPayload(false, 0x05, true, 0, prescaler));
                provider.WriteRegister(new NanEyeGSRegisterPayload(false, 0x06, true, 0, exposure));
                ProcessingWrapper.pr[0].ReduceProcessing = true;
            }
            catch (Exception ex) { OnError(ex.Message); }
        }

        /// <summary>
        /// Deconstructor
        /// </summary>
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

        /// <summary>
        /// Call this method once you've finished setting up the camera and are ready to begin capturing video
        /// </summary>
        public void Connect()
        {
            try
            {
                stopping = false;
                provider.StartCapture();
            }
            catch (Exception ex) { OnError(ex.Message); }
        }

        /// <summary>
        /// Call this method to stop capturing video
        /// </summary>
        bool stopping = false;
        public void Disconnect()
        {
            try
            {
                stopping = true;
                Task.Factory.StartNew(() => { provider.StopCapture(); }); // has to run in another thread for some reason... it never returns, but must be called or the program crashes
            }
            catch (Exception ex) { OnError(ex.Message); }
        }

        /// <summary>
        /// Reset the brightness correction parameters and start calibrating them
        /// </summary>
        public void StartCalibration()
        {
            calibrating = true;
            meanImg.Scatter(new float[640 * 640]);
            float[] temp = new float[640 * 640];
            for (int i = 0; i < temp.Length; i++) temp[i] = 1;
            correctionFactor.Scatter(temp);
            numCalibrationSamples = 0;
        }

        /// <summary>
        /// Stop calibrating, saving the current brightness correction parameters
        /// </summary>
        public void StopCalibration()
        {
            calibrating = false;

            float[] temp = new float[correctionFactor.Length];
            correctionFactor.Gather(temp);
            FileStream stream = new FileStream("correctionFactor.dat", FileMode.OpenOrCreate);
            foreach (float value in temp) stream.Write(BitConverter.GetBytes(value), 0, 4);
            stream.Close();
        }

        void provider_Interrupt(object sender, InterruptEventArgs e)
        {
            OnError("Camera provider interrupted: flags=" + e.Flags);
        }

        /// <summary>
        /// CUDA kernel for tracking a running mean for each pixel
        /// </summary>
        /// <param name="width">image width in pixels</param>
        /// <param name="image">pointer to image bytes on GPU</param>
        /// <param name="meanImg">pointer to mean pixel data on GPU (output)</param>
        /// <param name="numCalibrationSamples">number of samples that have been averaged so far</param>
        [AOTCompile]
        static void RunningMean_Kernel(int width, deviceptr<byte> image, deviceptr<float> meanImg, float numCalibrationSamples)
        {
            var x = blockIdx.x * blockDim.x + threadIdx.x;
            var y = blockIdx.y * blockDim.y + threadIdx.y;
            var p = y * width + x;
            meanImg[p] = meanImg[p] * numCalibrationSamples / (numCalibrationSamples + 1) + image[p] * 1.0f / (numCalibrationSamples + 1);
        }

        /// <summary>
        /// CUDA kernel for updating the correction factor for each pixel, using the ratio between the average overall brightness and that pixel's average brightness
        /// </summary>
        /// <param name="width">image width in pixels</param>
        /// <param name="meanImg">pointer to mean pixel data on GPU</param>
        /// <param name="correctionFactor">pointer to correction factor pixel data on GPU (output)</param>
        /// <param name="mean">mean across the entire image</param>
        [AOTCompile]
        static void UpdateCorrectionFactor_Kernel(int width, deviceptr<float> meanImg, deviceptr<float> correctionFactor, float mean)
        {
            var x = blockIdx.x * blockDim.x + threadIdx.x;
            var y = blockIdx.y * blockDim.y + threadIdx.y;
            var p = y * width + x;
            correctionFactor[p] = mean / meanImg[p];
        }

        /// <summary>
        /// CUDA kernel for applying the current correction factor to each pixel
        /// </summary>
        /// <param name="width">image width in pixels</param>
        /// <param name="correctionFactor">pointer to correction factor pixel data on GPU</param>
        /// <param name="image">pointer to image bytes on GPU (output)</param>
        [AOTCompile]
        static void ApplyCorrectionFactor_Kernel(int width, deviceptr<float> correctionFactor, deviceptr<byte> image)
        {
            var x = blockIdx.x * blockDim.x + threadIdx.x;
            var y = blockIdx.y * blockDim.y + threadIdx.y;
            var p = y * width + x;
            var v = (float)image[p] * correctionFactor[p];
            if (v < 0) v = 0;
            else if (v > 255) v = 255;
            image[p] = (byte)v;
        }

        /// <summary>
        /// Helper kernel for debugging purposes, converts a float image to a byte image on the GPU
        /// </summary>
        /// <param name="width">image width in pixels</param>
        /// <param name="input">pointer to float image</param>
        /// <param name="output">pointer to byte image (output)</param>
        [AOTCompile]
        static void Truncate_Kernel(int width, deviceptr<float> input, deviceptr<byte> output)
        {
            var x = blockIdx.x * blockDim.x + threadIdx.x;
            var y = blockIdx.y * blockDim.y + threadIdx.y;
            var p = y * width + x;
            output[p] = (byte)input[p];
        }

        /// <summary>
        /// Called every time a new frame shows up from the camera
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void provider_ImageTransaction(object sender, ImageReceivedEventArgs e)
        {
            if (stopping || e == null || e.PixelData == null) return;

            if (Monitor.TryEnter(this))
            {

                imgGPU.Scatter(e.PixelData);

                if (calibrating)
                {
                    // compute a running mean of each pixel
                    worker.Launch(RunningMean_Kernel, lp, e.Width, imgGPU.Ptr, meanImg.Ptr, numCalibrationSamples);
                    numCalibrationSamples++;

                    // compute the mean over the full mean image, using a reduction operation
                    addReduce.Reduce(meanImg.Ptr, scalarOutput.Ptr, numPixels);
                    var mean = scalarOutput.GatherScalar();
                    mean /= numPixels;

                    // update the correction factor for each pixel
                    worker.Launch(UpdateCorrectionFactor_Kernel, lp, e.Width, meanImg.Ptr, correctionFactor.Ptr, mean);
                }

                if (correctBrightness)
                {
                    worker.Launch(ApplyCorrectionFactor_Kernel, lp, e.Width, correctionFactor.Ptr, imgGPU.Ptr);
                    imgGPU.Gather(e.PixelData);
                }

                img.Bytes = e.PixelData; // note: have to do this last, because if you set it earlier but then modify the bytes it won't update the image

                // trigger a new frame event
                OnFrameAvailable(new VideoFrame { Image = img, ImageGPU = imgGPU, Timestamp = e.TimeStamp });

                Monitor.Exit(this);
            }
        }

        private void camera_Exception(object sender, OnExceptionEventArgs e)
        {
            OnError("Camera exception: " + e.ex.Message);
        }
    }
}
