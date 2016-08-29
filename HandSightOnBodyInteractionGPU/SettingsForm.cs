using System;
using System.Windows.Forms;

using HandSightLibrary;
using HandSightLibrary.ImageProcessing;

namespace HandSightOnBodyInteractionGPU
{
    public partial class SettingsForm : Form
    {
        public bool HideFromList { get { return true; } }

        public SettingsForm()
        {
            InitializeComponent();

            Location = Properties.Settings.Default.SettingsLocation;

            BrightnessChooser.Value = Properties.Settings.Default.CameraBrightness;
            PredictionSmoothingChooser.Value = Properties.Settings.Default.PredictionSmoothing;
            CoarseOnlyCheckbox.Checked = Properties.Settings.Default.CoarseOnly;
            SingleIMUCheckbox.Checked = Properties.Settings.Default.SingleIMU;
            EnableSoundEffectsCheckbox.Checked = Properties.Settings.Default.EnableSoundEffects;
            EnableSpeechCheckbox.Checked = Properties.Settings.Default.EnableSpeechOutput;
            EnableApplicationDemoCheckbox.Checked = Properties.Settings.Default.EnableApplicationDemos;
            FixedApplicationResonsesCheckbox.Checked = Properties.Settings.Default.FixedApplicationResponses;
            HoverTimeThresholdChooser.Value = Properties.Settings.Default.HoverTimeThreshold;
            GestureModeChooser.Items.AddRange(GestureActionMap.Modes);
            if (GestureModeChooser.Items.Contains(Properties.Settings.Default.GestureMode))
                GestureModeChooser.SelectedItem = Properties.Settings.Default.GestureMode;
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

        private void IMUCalibrationButton_Click(object sender, EventArgs e)
        {
            Sensors.Instance.ResetCalibration();
            OrientationTracker.Primary.Reset();
            OrientationTracker.Secondary.Reset();
        }

        private void FixedApplicationResonsesCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.FixedApplicationResponses = FixedApplicationResonsesCheckbox.Checked;
            Properties.Settings.Default.Save();
        }

        private void EnableApplicationDemoCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.EnableApplicationDemos = EnableApplicationDemoCheckbox.Checked;
            Properties.Settings.Default.Save();
        }

        private void EnableSoundEffectsCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.EnableSoundEffects = EnableSoundEffectsCheckbox.Checked;
            Properties.Settings.Default.Save();
        }

        private void EnableSpeechCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.EnableSpeechOutput = EnableSpeechCheckbox.Checked;
            Properties.Settings.Default.Save();
        }

        private void GestureModeChooser_SelectedIndexChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.GestureMode = (string)GestureModeChooser.SelectedItem;
            Properties.Settings.Default.Save();
            GestureActionMap.Reset();
        }

        private void SettingsForm_Move(object sender, EventArgs e)
        {
            Properties.Settings.Default.SettingsLocation = Location;
            Properties.Settings.Default.Save();
        }

        private void HoverTimeThresholdChooser_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.HoverTimeThreshold = (int)HoverTimeThresholdChooser.Value;
            Properties.Settings.Default.Save();
        }

        private void SettingsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.SettingsVisible = false;
            Properties.Settings.Default.Save();
        }

        private void ResetMenuLocationButton_Click(object sender, EventArgs e)
        {
            GestureActionMap.Reset();
        }
    }
}
