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
using System.IO;

using System.Speech.Synthesis;

namespace HandSightOnBodyInteractionGPU
{
    public partial class OnBodyInputDemo : Form
    {
        Dictionary<string, string[]> locations = new Dictionary<string, string[]>
        {
            { "Nothing", new string[] { "Nothing" } },
            { "Palm", new string[] { "Up", "Down", "Left", "Right", "Center" } },
            { "Finger", new string[] { "Thumb", "Index", "Middle", "Ring", "Pinky" } },
            { "Wrist", new string[] { "Inner", "Outer", "BackOfHand" } },
            { "Ear", new string[] { "Upper", "Lower", "Front", "Rear" } },
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

        bool calibrating = false, closing = false, recordingGesture = false;
        int countdown = -1;
        Timer timer = new Timer(1000);
        ImageTemplate currTemplate;
        string processingLock = "processing lock", trainingLock = "training lock";

        //BlockingCollection<string> coarseLocationPredictions = new BlockingCollection<string>();
        //BlockingCollection<string> fineLocationPredictions = new BlockingCollection<string>();
        BlockingCollection<Dictionary<string, float>> coarseLocationProbabilities = new BlockingCollection<Dictionary<string, float>>();
        BlockingCollection<Dictionary<string, float>> fineLocationProbabilities = new BlockingCollection<Dictionary<string, float>>();
        BlockingCollection<Dictionary<string, float>> gestureCoarseLocationProbabilities = new BlockingCollection<Dictionary<string, float>>();
        BlockingCollection<Dictionary<string, float>> gestureFineLocationProbabilities = new BlockingCollection<Dictionary<string, float>>();
        BlockingCollection<Sensors.Reading> gestureSensorReadings = new BlockingCollection<Sensors.Reading>();
        BlockingCollection<float> gestureFocusWeights = new BlockingCollection<float>();
        bool touchDown = false, recentTouchUp = false;
        int touchUpDelay = 20;

        SpeechSynthesizer speech = new SpeechSynthesizer();

        public OnBodyInputDemo()
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
            CoarseOnlyCheckbox.Checked = Properties.Settings.Default.CoarseOnly;
            PredictionSmoothingChooser.Value = numLocationPredictions;
            SingleIMUCheckbox.Checked = Properties.Settings.Default.SingleIMU;

            Text = "Connecting to Camera and Sensors...";

            CoarseLocationChooser.Items.Clear();
            CoarseLocationChooser.Items.AddRange(locations.Keys.ToArray());
            CoarseLocationChooser.SelectedIndex = 0;

            TouchSegmentation.TouchDownEvent += () => 
            {
                touchDown = true;

                // reset the location predictions
                if (!recentTouchUp)
                {
                    while (coarseLocationProbabilities.Count > 0) { coarseLocationProbabilities.Take(); }
                    while (fineLocationProbabilities.Count > 0) { fineLocationProbabilities.Take(); }
                    while (gestureCoarseLocationProbabilities.Count > 0) { gestureCoarseLocationProbabilities.Take(); }
                    while (gestureFineLocationProbabilities.Count > 0) { gestureFineLocationProbabilities.Take(); }
                    while (gestureFocusWeights.Count > 0) { gestureFocusWeights.Take(); }
                    while (gestureSensorReadings.Count > 0) { gestureSensorReadings.Take(); }
                }

                Invoke(new MethodInvoker(delegate { TouchStatusLabel.Text = "Touch Down"; }));
            };
            //TouchSegmentation.TouchUpEvent += (Queue<Sensors.Reading> readings) => { Invoke(new MethodInvoker(delegate { TouchStatusLabel.Text = "Touch Up"; })); };
            TouchSegmentation.TouchUpEvent += () =>
            {
                touchDown = false;
                recentTouchUp = true;
                Invoke(new MethodInvoker(delegate { TouchStatusLabel.Text = "Touch Up"; }));
                Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(touchUpDelay);
                    recentTouchUp = false;
                    if(!touchDown)
                    {
                        // process the gesture
                        
                        Gesture gesture = new Gesture(gestureSensorReadings.ToArray());
                        if (gestureSensorReadings.Count > 10)
                        {
                            GestureRecognition.PreprocessGesture(gesture);

                            if (recordingGesture)
                            {
                                string gestureClass = "";
                                Invoke(new MethodInvoker(delegate { gestureClass = (string)GestureChooser.SelectedItem; }));

                                Monitor.Enter(trainingLock);

                                recordingGesture = false;

                                gesture.ClassName = gestureClass;
                                GestureRecognition.AddTrainingExample(gesture, gestureClass);
                                GestureRecognition.Train();

                                Monitor.Exit(trainingLock);
                            }
                            else
                            {
                                gesture.ClassName = GestureRecognition.PredictGesture(gesture);
                            }
                        }

                        // predict the most likely location over the coarse of the gesture, weighted by voting and image focus
                        Dictionary<string, float>[] tempCoarseProbabilities = gestureCoarseLocationProbabilities.ToArray();
                        Dictionary<string, float>[] tempFineProbabilities = gestureFineLocationProbabilities.ToArray();
                        float[] tempFocusWeights = gestureFocusWeights.ToArray();

                        // sum up the probabilities
                        Dictionary<string, float> totalProbabilities = new Dictionary<string, float>();
                        //foreach (Dictionary<string, float> probabilities in gestureCoarseLocationProbabilities)
                        for(int i = 0; i < tempCoarseProbabilities.Length; i++)
                        {
                            Dictionary<string, float> probabilities = tempCoarseProbabilities[i];
                            float weight = Math.Max(tempFocusWeights.Length > i ? tempFocusWeights[i] : 0.01f, 0.01f);
                            foreach (string key in probabilities.Keys)
                            {
                                if (!totalProbabilities.ContainsKey(key)) totalProbabilities[key] = 0;
                                totalProbabilities[key] += weight * probabilities[key] / numLocationPredictions;
                            }
                        }

                        float maxProb = 0;
                        string coarseLocation = "";
                        float coarseProbability = 0;
                        foreach (string key in totalProbabilities.Keys)
                            if (totalProbabilities[key] > maxProb)
                            {
                                maxProb = totalProbabilities[key];
                                coarseLocation = key;
                                coarseProbability = maxProb;
                            }

                        // sum up the probabilities
                        totalProbabilities = new Dictionary<string, float>();
                        //foreach (Dictionary<string, float> probabilities in gestureFineLocationProbabilities)
                        for (int i = 0; i < tempFineProbabilities.Length; i++)
                        {
                            Dictionary<string, float> probabilities = tempFineProbabilities[i];
                            float weight = Math.Max(tempFocusWeights.Length > i ? tempFocusWeights[i] : 0.01f, 0.01f);
                            foreach (string key in probabilities.Keys)
                            {
                                if (!totalProbabilities.ContainsKey(key)) totalProbabilities[key] = 0;
                                totalProbabilities[key] += weight * probabilities[key] / numLocationPredictions;
                            }
                        }

                        maxProb = 0;
                        string fineLocation = "";
                        float fineProbability = 0;
                        foreach (string key in totalProbabilities.Keys)
                            if (totalProbabilities[key] > maxProb)
                            {
                                maxProb = totalProbabilities[key];
                                fineLocation = key;
                                fineProbability = maxProb;
                            }

                        Invoke(new MethodInvoker(delegate
                        {
                            CoarsePredictionLabel.Text = "= " + coarseLocation;
                            CoarseProbabilityLabel.Text = " (response = " + (coarseProbability * 100).ToString("0.0") + ")";
                            FinePredictionLabel.Text = "= " + fineLocation;
                            FineProbabilityLabel.Text = " (response = " + (fineProbability * 100).ToString("0.0") + ")";
                            GesturePredictionLabel.Text = gesture.ClassName;
                            speech.SpeakAsyncCancelAll();
                            speech.SpeakAsync(gesture.ClassName + " " + coarseLocation + " " + fineLocation);
                        }));
                    }
                });
            };

            Task.Factory.StartNew(() =>
            {
                Camera.Instance.FrameAvailable += Camera_FrameAvailable;
                Camera.Instance.Error += (string error) => 
                {
                    Debug.WriteLine(error);
                    Invoke(new MethodInvoker(delegate { Text = "Error: could not connect to camera"; }));
                };
                Camera.Instance.Brightness = 10;

                Sensors.Instance.ReadingAvailable += Sensors_ReadingAvailable;

                Camera.Instance.Connect();
                Sensors.Instance.Connect();

                Sensors.Instance.Brightness = 1;
                Sensors.Instance.NumSensors = SingleIMUCheckbox.Checked ? 1 : 2;
            });
        }

        DateTime last = DateTime.Now;
        private void Sensors_ReadingAvailable(Sensors.Reading reading)
        {
            OrientationTracker.Primary.UpdateWithReading(reading);
            //OrientationTracker.Secondary.UpdateWithReading(reading, -1, true);
            if ((DateTime.Now - last).TotalMilliseconds > 30)
            {
                Quaternion quaternion = OrientationTracker.Primary.EstimateOrientation();
                EulerAngles orientation = quaternion.GetEulerAngles();
                //OrientationTracker.EulerAngles orientation = OrientationTracker.Primary.EstimateOrientation();
                //orientation.Roll += (float)Math.PI; if (orientation.Roll > Math.PI) orientation.Roll -= (float)(2 * Math.PI);
                //orientation.Pitch = -orientation.Pitch;

                int resolution = 10;
                int yaw = resolution * (int)Math.Round(orientation.Yaw * 180 / Math.PI / resolution);
                int pitch = resolution * (int)Math.Round(orientation.Pitch * 180 / Math.PI / resolution);
                int roll = resolution * (int)Math.Round(orientation.Roll * 180 / Math.PI / resolution);
                //int yaw = resolution * (int)Math.Round(orientation.Yaw / resolution);
                //int pitch = resolution * (int)Math.Round(orientation.Pitch / resolution);
                //int roll = resolution * (int)Math.Round(orientation.Roll / resolution);
                reading.Orientation1 = quaternion;

                Invoke(new MethodInvoker(delegate
                {
                    OrientationLabel.Text = yaw + ", " + pitch + ", " + roll;
                    //OrientationLabel.Text = orientation.W.ToString("0.0") + ", " + orientation.X.ToString("0.0") + ", " + orientation.Y.ToString("0.0") + ", " + orientation.Z.ToString("0.0");
                }));
                last = DateTime.Now;
            }

            TouchSegmentation.UpdateWithReading(reading);

            if(touchDown) gestureSensorReadings.Add(reading);
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

            if (coarseLocation == null)
            {
                Invoke(new MethodInvoker(delegate { coarseLocation = (string)CoarseLocationChooser.SelectedItem; }));
            }

            if (fineLocation == null)
            {
                Invoke(new MethodInvoker(delegate { fineLocation = (string)FineLocationChooser.SelectedItem; }));
            }

            if (template == null)
                lock (processingLock)
                {
                    template = CopyTemplate(currTemplate);
                    template["coarse"] = coarseLocation;
                    template["fine"] = fineLocation;
                }

            if (!alreadyExists)
            {
                Localization.Instance.AddTrainingExample(template, coarseLocation, fineLocation);
                Localization.Instance.Train();
            }

            Invoke(new MethodInvoker(delegate
            {
                int imageIndex = TrainingSampleList.LargeImageList.Images.Count;
                TrainingSampleList.LargeImageList.Images.Add(template.Image.Bitmap);
                TrainingSampleList.Items.Add(new ListViewItem() { Text = coarseLocation + ", " + fineLocation, ImageIndex = imageIndex, Tag = template });

                TrainingExamplesLabel.Text = Localization.Instance.GetNumTrainingExamples() + " training examples (" + Localization.Instance.GetNumTrainingClasses() + " classes)";
            }));

            Monitor.Exit(trainingLock);

            // perform cross-validation
            //if (!alreadyExists) PerformCrossValidation();
        }

        private void PerformCrossValidation()
        {
            Invoke(new MethodInvoker(delegate { InfoBox.Text = ""; InfoBox.Text += "Performing Cross-Validation..." + Environment.NewLine; }));
            Task.Factory.StartNew(() =>
            {
                foreach (string className in Localization.Instance.samples.Keys)
                {
                    int correct = 0;
                    if (Localization.Instance.samples[className].Count > 1)
                    {
                        Parallel.ForEach(Localization.Instance.samples[className], (ImageTemplate testTemplate) =>
                        //foreach (ImageTemplate testTemplate in Localization.Instance.samples[className])
                        {
                            Localization localization = new Localization();
                            foreach (string trainClassName in Localization.Instance.samples.Keys)
                                foreach (ImageTemplate trainTemplate in Localization.Instance.samples[trainClassName])
                                    if (trainTemplate != testTemplate)
                                        localization.AddTrainingExample(trainTemplate, (string)trainTemplate["coarse"], (string)trainTemplate["fine"]);
                            localization.Train();
                            bool foundFeatureMatch;
                            Dictionary<string, float> coarseProbabilities, fineProbabilities;
                            string coarsePrediction = localization.PredictCoarseLocation(testTemplate, out coarseProbabilities);
                            string finePrediction = localization.PredictFineLocation(testTemplate, out foundFeatureMatch, out fineProbabilities, true, false, false, coarsePrediction);
                            float coarseProbability = coarseProbabilities[coarsePrediction];
                            float fineProbability = fineProbabilities[finePrediction];

                            if (coarsePrediction.Equals((string)testTemplate["coarse"]) && finePrediction.Equals((string)testTemplate["fine"]))
                            //correct++;
                            Interlocked.Increment(ref correct);
                            //Invoke(new MethodInvoker(delegate
                            //{
                            //    InfoBox.Text += ((string)testTemplate["fine"] + Localization.Instance.samples[className].IndexOf(testTemplate) + " = " + coarsePrediction + " (" + (coarseProbability * 100).ToString("0.0") + ") / " + finePrediction + " (" + (100 * fineProbability).ToString("0.0") + ")") + ", ";
                            //}));
                        });
                    }

                    Invoke(new MethodInvoker(delegate
                    {
                        InfoBox.Text += Localization.Instance.coarseLocations[className] + className + ": " + ((float)correct / Localization.Instance.samples[className].Count * 100).ToString("0.0") + "%, ";
                    }));
                }
                Invoke(new MethodInvoker(delegate
                {
                    InfoBox.Text += Environment.NewLine + "Done" + Environment.NewLine;
                }));
            });
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
                Sensors.Instance.Disconnect();
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
            if (countdown > 0)
            {
                CountdownLabel.Text = countdown.ToString();
                CountdownLabel.Visible = true;
                timer.Start();
            }
            else
            {
                AddTemplate();
            }
        }

        private void RecordGestureButton_Click(object sender, EventArgs e)
        {
            recordingGesture = true;
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
                Localization.Instance.Reset();
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

        private void SaveProfileButton_Click(object sender, EventArgs e)
        {
            PromptDialog dialog = new PromptDialog("Enter Profile Name", "Save");
            if(dialog.ShowDialog() == DialogResult.OK)
            {
                // TODO: check for existing profile, ask to confirm overwrite
                Localization.Instance.Save(dialog.Value);
            }
        }

        private void LoadProfileButton_Click(object sender, EventArgs e)
        {
            // TODO: replace with selection form allowing only existing profiles
            //PromptDialog dialog = new PromptDialog("Enter Profile Name", "Load");
            List<string> profiles = new List<string>();
            foreach (string dir in Directory.GetDirectories("savedProfiles"))
                profiles.Add((new DirectoryInfo(dir)).Name);
            if (profiles.Count == 0)
            {
                MessageBox.Show("Error: no saved profiles!");
                return;
            }
            SelectFromListDialog dialog = new SelectFromListDialog("Select Saved Profile", "Load", profiles);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                lock (trainingLock)
                {
                    Localization.Instance.Reset();
                    Localization.Instance.Load(dialog.SelectedItem);

                    foreach (string fineLocation in Localization.Instance.samples.Keys)
                        foreach (ImageTemplate template in Localization.Instance.samples[fineLocation])
                            AddTemplate(template, fineLocation, Localization.Instance.coarseLocations[fineLocation], true);

                    TrainingExamplesLabel.Text = Localization.Instance.GetNumTrainingExamples() + " training examples (" + Localization.Instance.GetNumTrainingClasses() + " classes)";

                    //PerformCrossValidation();
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
                    Localization.Instance.RemoveTrainingExample(template);
                    Localization.Instance.Train();
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

        private void SingleIMUCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            bool singleIMU = SingleIMUCheckbox.Checked;
            Properties.Settings.Default.SingleIMU = singleIMU;
            Properties.Settings.Default.Save();
            Sensors.Instance.NumSensors = singleIMU ? 1 : 2;
        }

        private void CoarseOnlyCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.CoarseOnly = CoarseOnlyCheckbox.Checked;
            Properties.Settings.Default.Save();
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
                    float focus = 0;
                    lock (processingLock)
                    {
                        ImageProcessing.ProcessTemplate(template, false);
                        focus = ImageProcessing.ImageFocus(template);
                        currTemplate = template;
                        FPS.Instance("processing").Update();
                    }

                    string coarseLocation = "", fineLocation = "";
                    float coarseProbability = 0, fineProbability = 0;
                    bool hasUpdate = false;
                    if (touchDown && Monitor.TryEnter(trainingLock))
                    {
                        try
                        {
                            if (Localization.Instance.GetNumTrainingExamples() > 0)
                            {
                                Dictionary<string, float> coarseProbabilities = new Dictionary<string, float>();
                                string predictedCoarseLocation = Localization.Instance.PredictCoarseLocation(currTemplate, out coarseProbabilities);
                                //coarseLocationPredictions.Add(predictedCoarseLocation);
                                //while (coarseLocationPredictions.Count > numLocationPredictions) coarseLocationPredictions.Take();
                                coarseLocationProbabilities.Add(coarseProbabilities);
                                gestureCoarseLocationProbabilities.Add(coarseProbabilities);
                                while (coarseLocationProbabilities.Count > numLocationPredictions) coarseLocationProbabilities.Take();

                                //// compute the mode of the array
                                //var groups = coarseLocationPredictions.GroupBy(v => v);
                                //int maxCount = groups.Max(g => g.Count());
                                //coarseLocation = groups.First(g => g.Count() == maxCount).Key;

                                // sum up the probabilities
                                Dictionary<string, float> totalProbabilities = new Dictionary<string, float>();
                                foreach (Dictionary<string, float> probabilities in coarseLocationProbabilities)
                                {
                                    foreach (string key in probabilities.Keys)
                                    {
                                        if (!totalProbabilities.ContainsKey(key)) totalProbabilities[key] = 0;
                                        totalProbabilities[key] += probabilities[key] / numLocationPredictions;
                                    }
                                }

                                float maxProb = 0;
                                foreach (string key in totalProbabilities.Keys)
                                    if (totalProbabilities[key] > maxProb)
                                    {
                                        maxProb = totalProbabilities[key];
                                        coarseLocation = key;
                                        coarseProbability = maxProb;
                                    }

                                if (!Properties.Settings.Default.CoarseOnly)
                                {
                                    bool foundFeatureMatch = false;
                                    Dictionary<string, float> fineProbabilities = new Dictionary<string, float>();
                                    string predictedFineLocation = Localization.Instance.PredictFineLocation(currTemplate, out foundFeatureMatch, out fineProbabilities, true, false, false, coarseLocation);
                                    //fineLocationPredictions.Add(predictedFineLocation);
                                    //while (fineLocationPredictions.Count > numLocationPredictions) fineLocationPredictions.Take();
                                    fineLocationProbabilities.Add(fineProbabilities);
                                    gestureFineLocationProbabilities.Add(fineProbabilities);
                                    while (fineLocationProbabilities.Count > numLocationPredictions) fineLocationProbabilities.Take();

                                    // compute the mode of the array
                                    //var groups = fineLocationPredictions.GroupBy(v => v);
                                    //int maxCount = groups.Max(g => g.Count());
                                    //fineLocation = groups.First(g => g.Count() == maxCount).Key;

                                    // sum up the probabilities
                                    totalProbabilities = new Dictionary<string, float>();
                                    foreach (Dictionary<string, float> probabilities in fineLocationProbabilities)
                                    {
                                        foreach (string key in probabilities.Keys)
                                        {
                                            if (!totalProbabilities.ContainsKey(key)) totalProbabilities[key] = 0;
                                            totalProbabilities[key] += probabilities[key] / numLocationPredictions;
                                        }
                                    }

                                    maxProb = 0;
                                    foreach (string key in totalProbabilities.Keys)
                                        if (totalProbabilities[key] > maxProb)
                                        {
                                            maxProb = totalProbabilities[key];
                                            fineLocation = key;
                                            fineProbability = maxProb;
                                        }
                                }

                                gestureFocusWeights.Add(focus);

                                FPS.Instance("matching").Update();
                                hasUpdate = true;
                            }
                        }
                        finally
                        {
                            Monitor.Exit(trainingLock);
                        }
                    }

                    //LBP.GetInstance(frame.Image.Size).GetHistogram(frame);
                    Invoke(new MethodInvoker(delegate
                    {
                        Display.Image = frame.Image.Bitmap;
                        if (hasUpdate)
                        {
                            CoarsePredictionLabel.Text = coarseLocation;
                            CoarseProbabilityLabel.Text = " (response = " + (coarseProbability * 100).ToString("0.0") + ")";
                            //Debug.WriteLine(coarseLocation);
                            FinePredictionLabel.Text = fineLocation;
                            FineProbabilityLabel.Text = " (response = " + (fineProbability * 100).ToString("0.0") + ")";
                            //Debug.WriteLine(fineLocation);
                        }
                        //else if(!touchDown)
                        //{
                        //    CoarsePredictionLabel.Text = "";
                        //    CoarseProbabilityLabel.Text = "";
                        //    FinePredictionLabel.Text = "";
                        //    FineProbabilityLabel.Text = "";
                        //}
                        //focus = (int)(focus / 100) * 100;
                        Text = FPS.Camera.Average.ToString("0") + " fps camera / " + (Sensors.Instance.IsConnected ? FPS.Sensors.Average.ToString("0") + " fps sensors / " + FPS.Instance("processing").Average.ToString("0") + " fps processing" : "Waiting for Sensors") /*+ " / focus = " + focus.ToString("0")*/;
                    }));
                }
                catch { }
            });
        }
    }
}
