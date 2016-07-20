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
using System.Collections.Concurrent;

namespace HandSightOnBodyInteractionGPU
{
    public partial class LocalizationTest : Form
    {
        bool calibrating = false, closing = false;
        int countdown = -1;
        Timer timer = new Timer(1000);
        ImageTemplate currTemplate;
        string processingLock = "processing lock", trainingLock = "training lock";

        public LocalizationTest()
        {
            InitializeComponent();

            timer.Elapsed += Timer_Elapsed;
            CountdownLabel.Parent = Display;

            TimerChooser.Value = Properties.Settings.Default.CountdownTimer;

            Camera.Instance.FrameAvailable += Camera_FrameAvailable;
            Camera.Instance.Brightness = 10;
            
            Camera.Instance.Connect();

            //Sensors.Instance.Connect();
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
                Monitor.Enter(trainingLock);
                ImageTemplate template = null;
                lock (processingLock)
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
                Monitor.Exit(trainingLock);
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
            lock (trainingLock)
            {
                Localization.Reset();
            }
            TrainingExamplesLabel.Text = "0 training examples (0 classes)";
        }

        ImageTemplate CopyTemplate(ImageTemplate template)
        {
            ImageTemplate newTemplate = template.Clone();
            return newTemplate;
        }

        BlockingCollection<string> locationPredictions = new BlockingCollection<string>();

        private void SaveProfileButton_Click(object sender, EventArgs e)
        {
            PromptDialog dialog = new PromptDialog("Enter Profile Name", "Save");
            if(dialog.ShowDialog() == DialogResult.OK)
            {
                // TODO: check for existing profile, ask to confirm overwrite
                Localization.Save(dialog.Value);
            }
        }

        private void LoadProfileButton_Click(object sender, EventArgs e)
        {
            // TODO: replace with selection form allowing only existing profiles
            PromptDialog dialog = new PromptDialog("Enter Profile Name", "Load");
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                lock (trainingLock)
                {
                    Localization.Reset();
                    Localization.Load(dialog.Value);
                    TrainingExamplesLabel.Text = Localization.GetNumTrainingExamples() + " training examples (" + Localization.GetNumTrainingClasses() + " classes)";
                }
            }
        }

        int numLocationPredictions = 15;
        void Camera_FrameAvailable(VideoFrame frame)
        {
            if (closing) return;

            ImageTemplate template = new ImageTemplate(frame.Clone());
            FPS.Camera.Update();

            Task.Factory.StartNew(() =>
            {
                try
                {
                    lock (processingLock)
                    {
                        ImageProcessing.ProcessTemplate(template, false);
                        currTemplate = template;
                    }

                    string location = "";
                    if (Monitor.TryEnter(trainingLock))
                    {
                        if (Localization.GetNumTrainingExamples() > 0)
                        {
                            string predictedLocation = Localization.PredictGroup(currTemplate);
                            locationPredictions.Add(predictedLocation);
                            while (locationPredictions.Count > numLocationPredictions) locationPredictions.Take();

                            // compute the mode of the array
                            var groups = locationPredictions.GroupBy(v => v);
                            int maxCount = groups.Max(g => g.Count());
                            location = groups.First(g => g.Count() == maxCount).Key;
                        }
                        Monitor.Exit(trainingLock);
                    }

                    //LBP.GetInstance(frame.Image.Size).GetHistogram(frame);
                    Invoke(new MethodInvoker(delegate
                    {
                        Display.Image = frame.Image.Bitmap;
                        PredictionLabel.Text = location;
                        Text = FPS.Camera.Average.ToString("0") + " fps";
                    }));
                }
                catch { }
            });
        }
    }
}
