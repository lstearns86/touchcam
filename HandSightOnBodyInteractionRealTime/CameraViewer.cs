using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Cuda;

using HandSightLibrary;

namespace HandSightOnBodyInteractionRealTime
{
    public partial class CameraViewer : Form
    {
        bool calibrating = false;
        public CameraViewer()
        {
            InitializeComponent();

            Camera.Instance.FrameAvailable += Camera_FrameAvailable;
            Camera.Instance.Brightness = 50;
            Camera.Instance.Connect();
        }

        void Camera_FrameAvailable(CudaImage<Gray, float> frame, uint timestamp)
        {
            Display.Image = frame.Bitmap;
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
    }
}
