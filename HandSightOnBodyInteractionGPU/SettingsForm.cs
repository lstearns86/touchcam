using System;
using System.Windows.Forms;

using HandSightLibrary;
using HandSightLibrary.ImageProcessing;

namespace HandSightOnBodyInteractionGPU
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();

            BrightnessChooser.Value = Properties.Settings.Default.CameraBrightness;
            PredictionSmoothingChooser.Value = Properties.Settings.Default.PredictionSmoothing;
            CoarseOnlyCheckbox.Checked = Properties.Settings.Default.CoarseOnly;
            SingleIMUCheckbox.Checked = Properties.Settings.Default.SingleIMU;
        }

        private void BrightnessChooser_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.CameraBrightness = (int)BrightnessChooser.Value;
            Properties.Settings.Default.Save();

            if (Camera.Instance.IsConnected)
                Camera.Instance.Brightness = (int)BrightnessChooser.Value;
        }

        private void PredictionSmoothingChooser_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.PredictionSmoothing = (int)PredictionSmoothingChooser.Value;
            Properties.Settings.Default.Save();
        }

        private void CoarseOnlyCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.CoarseOnly = CoarseOnlyCheckbox.Checked;
            Properties.Settings.Default.Save();
        }

        private void SingleIMUCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.SingleIMU = SingleIMUCheckbox.Checked;
            Properties.Settings.Default.Save();

            if(Sensors.Instance.IsConnected)
            {
                Sensors.Instance.NumSensors = SingleIMUCheckbox.Checked ? 1 : 2;
            }
        }

        bool calibrating = false;
        private void CalibrationButton_Click(object sender, EventArgs e)
        {
            calibrating = !calibrating;
            if (calibrating)
                Camera.Instance.StartCalibration();
            else
                Camera.Instance.StopCalibration();
            CalibrationButton.Text = (calibrating ? "Stop" : "Start") + " Calibration";
        }
    }
}
