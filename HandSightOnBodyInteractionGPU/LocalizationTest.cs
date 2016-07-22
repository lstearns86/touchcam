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
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace HandSightOnBodyInteractionGPU
{
    public partial class LocalizationTest : Form
    {
        Dictionary<string, string[]> locations = new Dictionary<string, string[]>
        {
            { "Nothing", new string[] { "Nothing" } },
            { "Palm", new string[] { "Up", "Down", "Left", "Right", "Center" } },
            { "Finger", new string[] { "Thumb", "Index", "Middle", "Ring", "Pinky" } },
            { "Wrist", new string[] { "Inner", "Outer", "BackOfHand" } },
            { "Ear", new string[] { "Upper", "Lower" } },
            { "Clothing", new string[] { "Pants", "Shirt" } },
            { "OtherSkin", new string[] { "Shoulder", "Thigh", "Cheek", "Neck" } },
            { "Knuckle", new string[] { "Thumb", "Index", "Middle", "Ring", "Pinky" } },
            { "Nail", new string[] { "Thumb", "Index", "Middle", "Ring", "Pinky" } }
        };

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        public int MakeLong(short lowPart, short highPart)
        {
            return (int)(((ushort)lowPart) | (uint)(highPart << 16));
        }

        public void ListView_SetSpacing(ListView listview, short cx, short cy)
        {
            const int LVM_FIRST = 0x1000;
            const int LVM_SETICONSPACING = LVM_FIRST + 53;
            // http://msdn.microsoft.com/en-us/library/bb761176(VS.85).aspx
            // minimum spacing = 4
            SendMessage(listview.Handle, LVM_SETICONSPACING,
            IntPtr.Zero, (IntPtr)MakeLong(cx, cy));

            // http://msdn.microsoft.com/en-us/library/bb775085(VS.85).aspx
            // DOESN'T WORK!
            // can't find ListView_SetIconSpacing in dll comctl32.dll
            //ListView_SetIconSpacing(listView.Handle, 5, 5);
        }

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
            TrainingSampleList.LargeImageList = new ImageList();
            TrainingSampleList.LargeImageList.ImageSize = new Size(120, 120);
            TrainingSampleList.LargeImageList.ColorDepth = ColorDepth.Depth24Bit;
            ListView_SetSpacing(TrainingSampleList, 120 + 12, 120 + 24);

            TimerChooser.Value = Properties.Settings.Default.CountdownTimer;
            BrightnessChooser.Value = Properties.Settings.Default.CameraBrightness;
            numLocationPredictions = Properties.Settings.Default.PredictionSmoothing;
            PredictionSmoothingChooser.Value = numLocationPredictions;

            Text = "Connecting to Camera and Sensors...";

            CoarseLocationChooser.Items.Clear();
            CoarseLocationChooser.Items.AddRange(locations.Keys.ToArray());
            CoarseLocationChooser.SelectedIndex = 0;

            TouchSegmentation.TouchDownEvent += () => { Invoke(new MethodInvoker(delegate { TouchStatusLabel.Text = "Touch Down"; })); };
            TouchSegmentation.TouchUpEvent += (Queue<Sensors.Reading> readings) => { Invoke(new MethodInvoker(delegate { TouchStatusLabel.Text = "Touch Up"; })); };

            Task.Factory.StartNew(() =>
            {
                Camera.Instance.FrameAvailable += Camera_FrameAvailable;
                Camera.Instance.Error += (string error) => 
                {
                    Debug.WriteLine(error);
                    Invoke(new MethodInvoker(delegate { Text = "Error: could not connect to camera"; }));
                };
                Camera.Instance.Brightness = 10;

                Sensors.Instance.ReadingAvailable += Instance_ReadingAvailable;
                Camera.Instance.Connect();
                Sensors.Instance.Connect();
            });
        }

        private void Instance_ReadingAvailable(Sensors.Reading reading)
        {
            FPS.Sensors.Update();
            TouchSegmentation.UpdateWithReading(reading);
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
                AddTemplate();

                Invoke(new MethodInvoker(delegate
                {
                    CountdownLabel.Visible = false;
                }));
                timer.Stop();
            }
        }

        private void AddTemplate(ImageTemplate template = null, string coarseLocation = null, string fineLocation = null, bool alreadyExists = false)
        {
            Monitor.Enter(trainingLock);

            if(template == null)
                lock (processingLock)
                {
                    template = CopyTemplate(currTemplate);
                }

            if (coarseLocation == null)
            {
                Invoke(new MethodInvoker(delegate { coarseLocation = (string)CoarseLocationChooser.SelectedItem; }));
            }

            if (fineLocation == null)
            {
                Invoke(new MethodInvoker(delegate { fineLocation = (string)FineLocationChooser.SelectedItem; }));
            }

            if (!alreadyExists)
            {
                Localization.AddTrainingExample(template, coarseLocation, fineLocation);
                Localization.Train();
            }

            Invoke(new MethodInvoker(delegate
            {
                int imageIndex = TrainingSampleList.LargeImageList.Images.Count;
                TrainingSampleList.LargeImageList.Images.Add(template.Image.Bitmap);
                TrainingSampleList.Items.Add(new ListViewItem() { Text = coarseLocation + ", " + fineLocation, ImageIndex = imageIndex, Tag = template });

                TrainingExamplesLabel.Text = Localization.GetNumTrainingExamples() + " training examples (" + Localization.GetNumTrainingClasses() + " classes)";
            }));

            Monitor.Exit(trainingLock);
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
                TrainingSampleList.LargeImageList.Images.Clear();
                TrainingSampleList.Items.Clear();
            }
            TrainingExamplesLabel.Text = "0 training examples (0 classes)";
        }

        ImageTemplate CopyTemplate(ImageTemplate template)
        {
            ImageTemplate newTemplate = template.Clone();
            return newTemplate;
        }

        BlockingCollection<string> coarseLocationPredictions = new BlockingCollection<string>();
        BlockingCollection<string> fineLocationPredictions = new BlockingCollection<string>();

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

                    foreach (string fineLocation in Localization.samples.Keys)
                        foreach (ImageTemplate template in Localization.samples[fineLocation])
                            AddTemplate(template, fineLocation, Localization.coarseLocations[fineLocation], true);

                    TrainingExamplesLabel.Text = Localization.GetNumTrainingExamples() + " training examples (" + Localization.GetNumTrainingClasses() + " classes)";
                }
            }
        }

        private void ExpandSecondarySettingsButton_ButtonClicked()
        {
            bool expanded = ExpandSecondarySettingsButton.Direction == HandSightOnBodyInteraction.ExpandContractButton.DirectionType.Right;
            expanded = !expanded;
            SecondaryControlPanel.Visible = expanded;
            ExpandSecondarySettingsButton.Direction = expanded ? HandSightOnBodyInteraction.ExpandContractButton.DirectionType.Right : HandSightOnBodyInteraction.ExpandContractButton.DirectionType.Left;
        }

        private void BrightnessChooser_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.CameraBrightness = (int)BrightnessChooser.Value;
            Camera.Instance.Brightness = (int)BrightnessChooser.Value;
        }

        private void TrainingSampleList_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Delete && TrainingSampleList.SelectedIndices.Count > 0)
            {
                lock (trainingLock)
                {
                    int index = TrainingSampleList.SelectedIndices[0];
                    ImageTemplate template = (ImageTemplate)TrainingSampleList.Items[index].Tag;
                    TrainingSampleList.Items.RemoveAt(index);
                    TrainingSampleList.LargeImageList.Images.RemoveAt(index);
                    for (int i = index; i < TrainingSampleList.Items.Count; i++) TrainingSampleList.Items[i].ImageIndex = i;
                    Localization.RemoveTrainingExample(template);
                    Localization.Train();
                }
            }
        }

        private void PredictionSmoothingChooser_ValueChanged(object sender, EventArgs e)
        {
            numLocationPredictions = (int)PredictionSmoothingChooser.Value;
            Properties.Settings.Default.PredictionSmoothing = numLocationPredictions;
            Properties.Settings.Default.Save();
        }

        private void LocationChooser_SelectedIndexChanged(object sender, EventArgs e)
        {
            FineLocationChooser.Items.Clear();
            FineLocationChooser.Items.AddRange(locations[(string)CoarseLocationChooser.SelectedItem]);
            FineLocationChooser.SelectedIndex = 0;
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

                    string coarseLocation = "", fineLocation = "";
                    if (Monitor.TryEnter(trainingLock))
                    {
                        if (Localization.GetNumTrainingExamples() > 0)
                        {
                            string predictedCoarseLocation = Localization.PredictCoarseLocation(currTemplate);
                            coarseLocationPredictions.Add(predictedCoarseLocation);
                            while (coarseLocationPredictions.Count > numLocationPredictions) coarseLocationPredictions.Take();

                            // compute the mode of the array
                            var groups = coarseLocationPredictions.GroupBy(v => v);
                            int maxCount = groups.Max(g => g.Count());
                            coarseLocation = groups.First(g => g.Count() == maxCount).Key;

                            string predictedFineLocation = Localization.PredictFineLocation(currTemplate, true, false, false, coarseLocation);
                            fineLocationPredictions.Add(predictedFineLocation);
                            while (fineLocationPredictions.Count > numLocationPredictions) fineLocationPredictions.Take();

                            // compute the mode of the array
                            groups = fineLocationPredictions.GroupBy(v => v);
                            maxCount = groups.Max(g => g.Count());
                            fineLocation = groups.First(g => g.Count() == maxCount).Key;
                        }
                        Monitor.Exit(trainingLock);
                    }

                    //LBP.GetInstance(frame.Image.Size).GetHistogram(frame);
                    Invoke(new MethodInvoker(delegate
                    {
                        Display.Image = frame.Image.Bitmap;
                        CoarsePredictionLabel.Text = coarseLocation;
                        FinePredictionLabel.Text = fineLocation;
                        Text = FPS.Camera.Average.ToString("0") + " fps camera / " + (Sensors.Instance.IsConnected ? FPS.Sensors.Average.ToString("0") + " fps sensors" : "Waiting for Sensors");
                    }));
                }
                catch { }
            });
        }
    }
}
