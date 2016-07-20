using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Cuda;

using HandSightLibrary;
using HandSightLibrary.ImageProcessing;

namespace HandSightOnBodyInteractionGPU
{
    public partial class CameraViewer : Form
    {
        bool calibrating = false, closing = false;
        public CameraViewer()
        {
            InitializeComponent();

            Camera.Instance.FrameAvailable += Camera_FrameAvailable;
            Camera.Instance.Brightness = 5;
            Camera.Instance.Connect();
        }

        //void Camera_FrameAvailable(CudaImage<Gray, float> frame, uint timestamp)
        void Camera_FrameAvailable(VideoFrame frame)
        {
            if (closing) return;

            FPS.Camera.Update();
            //LBP.GetInstance(frame.Image.Size).GetHistogram(frame);
            Invoke(new MethodInvoker(delegate
            {
                Display.Image = frame.Image.Bitmap;
                Text = FPS.Camera.Average.ToString("0") + " fps";
            }));
        }

        void CalibrateButton_Click(object sender, EventArgs e)
        {
            calibrating = !calibrating;
            if (calibrating)
                Camera.Instance.StartCalibration();
            else
                Camera.Instance.StopCalibration();
            CalibrateButton.Text = (calibrating ? "Stop" : "Start") + " Calibration";
        }

        private void CameraViewer_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!closing)
            {
                closing = true;
                e.Cancel = true;

                // TODO: cleanup any sensors and resources
                Camera.Instance.Disconnect();

                Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(500);
                    Invoke(new MethodInvoker(delegate { Close(); }));
                });
            }
        }
    }
}
