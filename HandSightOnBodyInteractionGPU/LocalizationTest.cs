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

using Timer = System.Timers.Timer;

namespace HandSightOnBodyInteractionGPU
{
    public partial class LocalizationTest : Form
    {
        bool calibrating = false, closing = false;
        int countdown = -1;
        Timer timer = new Timer(1000);
        ImageTemplate currTemplate;

        public LocalizationTest()
        {
            InitializeComponent();

            timer.Elapsed += Timer_Elapsed;
            CountdownLabel.Parent = Display;

            TimerChooser.Value = Properties.Settings.Default.CountdownTimer;

            Camera.Instance.FrameAvailable += Camera_FrameAvailable;
            Camera.Instance.Brightness = 50;
            Camera.Instance.Connect();
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if(countdown > 0)
            {
                countdown--;
                Invoke(new MethodInvoker(delegate
                {
                    CountdownLabel.Text = countdown.ToString();
                }));
            }
            else if (countdown == 0)
            {
                ImageTemplate template = null;
                lock(this)
                {
                    template = CopyTemplate(currTemplate);
                }

                string location = "";
                Invoke(new MethodInvoker(delegate { location = (string)LocationChooser.SelectedItem; }));

                Localization.AddTrainingExample(template, location, location);
                Localization.Train();

                Invoke(new MethodInvoker(delegate
                {
                    CountdownLabel.Visible = false;
                    TrainingExamplesLabel.Text = Localization.GetNumTrainingExamples() + " training examples (" + Localization.GetNumTrainingClasses() + " classes)";
                }));
                timer.Stop();
            }
        }

        private void CalibrationButton_Click(object sender, EventArgs e)
        {
            calibrating = !calibrating;
            if (calibrating)
                Camera.Instance.StartCalibration();
            else
                Camera.Instance.StopCalibration();
            CalibrationButton.Text = (calibrating ? "Stop" : "Start") + " Calibration";
        }

        private void LocalizationTest_FormClosing(object sender, FormClosingEventArgs e)
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

        private void RecordTrainingExampleButton_Click(object sender, EventArgs e)
        {
            countdown = (int)TimerChooser.Value;
            CountdownLabel.Text = countdown.ToString();
            CountdownLabel.Visible = true;
            timer.Start();
        }

        private void TimerChooser_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.CountdownTimer = (int)TimerChooser.Value;
            Properties.Settings.Default.Save();
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            Localization.Reset();
            TrainingExamplesLabel.Text = "0 training examples (0 classes)";
        }

        ImageTemplate CopyTemplate(ImageTemplate template)
        {
            ImageTemplate newTemplate = template.Clone();
            return newTemplate;
        }

        void Camera_FrameAvailable(VideoFrame frame)
        {
            if (closing) return;

            lock (this)
            {
                currTemplate = new ImageTemplate(frame);
                ImageProcessing.ProcessTemplate(currTemplate, false);
            }

            string location = Localization.GetNumTrainingExamples() > 0 ? Localization.PredictGroup(currTemplate) : "";

            FPS.Camera.Update();
            LBP.GetInstance(frame.Image.Size).GetHistogram(frame);
            Invoke(new MethodInvoker(delegate
            {
                Display.Image = frame.Image.Bitmap;
                PredictionLabel.Text = location;
                Text = FPS.Camera.Average.ToString("0") + " fps";
            }));
        }
    }
}
