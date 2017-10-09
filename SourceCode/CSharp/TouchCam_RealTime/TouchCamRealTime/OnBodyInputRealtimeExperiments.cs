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

using TouchCamLibrary;
using TouchCamLibrary.ImageProcessing;

using Timer = System.Timers.Timer;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;

using System.Speech.Synthesis;
using System.Media;

namespace TouchCam
{
    /// <summary>
    /// Main program used for conducting realtime experiments
    /// </summary>
    public partial class OnBodyInputRealtimeExperiments : Form
    {
        // Mappings between coarse- and fine-grained classes. Note that only a few of these were actually used in our experiments.
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

        const string TestingVoice = "Microsoft Zira Desktop";
        const string MenuVoice = "Microsoft David Desktop";

        const float ledMaxBrightness = 1f; // can lower if the light is too bright (must be between 0 and 1)
        const int touchUpDelay = 100, sensorReadingPreBuffer = 50; // in milliseconds

        bool closing = false, recordingGesture = false;
        int countdown = -1;
        Timer timer = new Timer(1000);
        ImageTemplate currTemplate;
        string preprocessingLock = "processing lock", recognitionLock = "recognition lock", trainingLock = "training lock";
        string coarseLocationToTrain = "", fineLocationToTrain = "", gestureToTrain = "";

        BlockingCollection<Dictionary<string, float>> coarseLocationProbabilities = new BlockingCollection<Dictionary<string, float>>();
        BlockingCollection<Dictionary<string, float>> fineLocationProbabilities = new BlockingCollection<Dictionary<string, float>>();
        BlockingCollection<Dictionary<string, float>> gestureCoarseLocationProbabilities = new BlockingCollection<Dictionary<string, float>>();
        BlockingCollection<Dictionary<string, float>> gestureFineLocationProbabilities = new BlockingCollection<Dictionary<string, float>>();
        BlockingCollection<string> coarseLocationPredictions = new BlockingCollection<string>();
        BlockingCollection<string> gestureCoarseLocationPredictions = new BlockingCollection<string>();
        BlockingCollection<Sensors.Reading> sensorReadingHistory = new BlockingCollection<Sensors.Reading>();
        BlockingCollection<Sensors.Reading> gestureSensorReadings = new BlockingCollection<Sensors.Reading>();
        BlockingCollection<float> gestureFocusWeights = new BlockingCollection<float>();
        bool touchDown = false, recentTouchUp = false;
        bool hovering = false;
        DateTime touchStart = DateTime.Now;
        string hoverCoarseLocation = null, hoverFineLocation = null;
        
        SpeechSynthesizer speech = new SpeechSynthesizer();
        SoundPlayer captureSound, tickSound, beepSound, phoneSound;

        TrainingForm trainingForm = new TrainingForm();
        TestingForm testingForm;// = new TestingForm();
        bool autoTrainGesture = false, autoTrainLocation = false, prepareToAutoTrainLocation = false;

        public OnBodyInputRealtimeExperiments()
        {
            InitializeComponent();

            Location = Properties.Settings.Default.MainLocation;
            Size = Properties.Settings.Default.MainSize;

            captureSound = new SoundPlayer("sounds\\camera_capture.wav");
            tickSound = new SoundPlayer("sounds\\tick.wav");
            beepSound = new SoundPlayer("sounds\\camera_beep.wav");
            phoneSound = new SoundPlayer("sounds\\phone.wav");
            speech.Rate = 2;

            captureSound.LoadAsync();
            tickSound.LoadAsync();
            beepSound.LoadAsync();

            timer.Elapsed += Timer_Elapsed;
            CountdownLabel.Parent = Display;
            numLocationPredictions = Properties.Settings.Default.PredictionSmoothing;

            Text = "Connecting to Camera and Sensors...";

            // set up the gestures and menus
            GestureActionMap.LoadMacros(File.ReadAllText("defaults/macros.txt"));
            GestureActionMap.LoadMenus(File.ReadAllText("defaults/menus.txt"));
            GestureActionMap.LoadActions(File.ReadAllText("defaults/actions.txt"));

            // needs to be initialized after the GestureActionMap
            testingForm = new TestingForm();

            testingForm.TaskStarted += (string task) =>
            {
                speech.SpeakAsyncCancelAll();
                speech.SelectVoice(TestingVoice);
                speech.SpeakAsync("Begin");
            };

            testingForm.TaskFinished += (string task) =>
            {
                speech.SpeakAsyncCancelAll();
                speech.SelectVoice(TestingVoice);
                speech.SpeakAsync("Task Completed");
            };

            //trainingForm.TrainingDataUpdated += () => { UpdateTrainingLabel(); };
            trainingForm.RecordLocation += (string coarseLocation, string fineLocation) =>
            {
                Logging.LogTrainingEvent("prepare_to_capture_location");
                coarseLocationToTrain = coarseLocation;
                fineLocationToTrain = fineLocation;

                // change the countdown timer setting if you don't want to capture the sample immediately (useful if you're training on yourself, for example)
                countdown = Properties.Settings.Default.CountdownTimer;
                if (countdown > 0)
                {
                    tickSound.Play();
                    CountdownLabel.Text = countdown.ToString();
                    CountdownLabel.Visible = true;
                    timer.Start();
                }
                else
                {
                    if (Properties.Settings.Default.EnableSoundEffects) captureSound.Play();
                    ImageTemplate newTemplate = AddTemplate();
                    Logging.LogTrainingEvent("Added template: " + newTemplate["coarse"] + " " + newTemplate["fine"]);
                    //trainingForm.UpdateLocationList();
                }
            };
            trainingForm.RecordGesture += (string gesture) =>
            {
                Logging.LogTrainingEvent("start_recording_gesture");
                if (Properties.Settings.Default.EnableSoundEffects) beepSound.Play();
                gestureToTrain = gesture;
                recordingGesture = true;
            };
            trainingForm.AutoCaptureGesture += (string gesture) =>
            {
                Logging.LogTrainingEvent("start_autocapture_gesture");
                if (Properties.Settings.Default.EnableSoundEffects) beepSound.Play();
                gestureToTrain = gesture;
                autoTrainGesture = true;
            };
            trainingForm.StopAutoCapturingGestures += () =>
            {
                Logging.LogTrainingEvent("stop_autocapture_gesture");
                autoTrainGesture = false;
                GestureRecognition.Train();
                trainingForm.UpdateGestureList();
            };
            trainingForm.AutoCaptureLocation += (string coarseLocation, string fineLocation) =>
            {
                coarseLocationToTrain = coarseLocation;
                fineLocationToTrain = fineLocation;

                Logging.LogTrainingEvent("start_autocapture_location");

                countdown = Properties.Settings.Default.CountdownTimer;
                if (countdown > 0)
                {
                    prepareToAutoTrainLocation = true;
                    tickSound.Play();
                    CountdownLabel.Text = countdown.ToString();
                    CountdownLabel.Visible = true;
                    timer.Start();
                }
                else
                {
                    if (Properties.Settings.Default.EnableSoundEffects) beepSound.Play();
                    autoTrainLocation = true;
                }
            };
            trainingForm.StopAutoCapturingLocation += () =>
            {
                autoTrainLocation = false;
                Logging.LogTrainingEvent("stop_autocapture_location");
            };

            TouchSegmentation.TouchDownEvent += () => 
            {
                touchDown = true;
                hovering = false;
                hoverCoarseLocation = null;
                hoverFineLocation = null;
                touchStart = DateTime.Now;
                Sensors.Instance.Brightness = ledMaxBrightness;

                Logging.LogOtherEvent("touch_down");

                // reset the location predictions
                if (!recentTouchUp)
                {
                    while (coarseLocationProbabilities.Count > 0) { coarseLocationProbabilities.Take(); }
                    while (fineLocationProbabilities.Count > 0) { fineLocationProbabilities.Take(); }
                    while (gestureCoarseLocationProbabilities.Count > 0) { gestureCoarseLocationProbabilities.Take(); }
                    while (gestureFineLocationProbabilities.Count > 0) { gestureFineLocationProbabilities.Take(); }
                    while (coarseLocationPredictions.Count > 0) { coarseLocationPredictions.Take(); }
                    while (gestureCoarseLocationPredictions.Count > 0) { gestureCoarseLocationPredictions.Take(); }
                    while (gestureFocusWeights.Count > 0) { gestureFocusWeights.Take(); }
                    while (gestureSensorReadings.Count > 0) { gestureSensorReadings.Take(); }
                    foreach (Sensors.Reading reading in sensorReadingHistory) gestureSensorReadings.Add(reading);
                    Logging.LogOtherEvent("smoothing_reset");
                }

                Invoke(new MethodInvoker(delegate { TouchStatusLabel.Text = "Touch Down"; }));
            };
            TouchSegmentation.TouchUpEvent += () =>
            {
                touchDown = false;
                recentTouchUp = true;
                Invoke(new MethodInvoker(delegate { TouchStatusLabel.Text = "Touch Up"; }));
                Logging.LogOtherEvent("touch_up");
                
                // trigger an event after a short delay to prevent false positives and allow for the double-tap gesture
                // it is cancelled if the user touches down again within that time
                Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(touchUpDelay);
                    recentTouchUp = false;
                    if(!touchDown)
                    {
                        Logging.LogOtherEvent("touch_up_timeout");
                        Sensors.Instance.Brightness = 0;

                        if (autoTrainLocation)
                        {
                            autoTrainLocation = false;
                            trainingForm.StopAutoCapture();
                        }

                        if ((hovering && !recordingGesture && !autoTrainGesture) || trainingForm.Training) return;

                        // process the gesture

                        Gesture gesture = new Gesture(gestureSensorReadings.ToArray());
                        if (gestureSensorReadings.Count > 60)
                        {
                            GestureRecognition.PreprocessGesture(gesture);

                            if (recordingGesture || autoTrainGesture)
                            {
                                //Monitor.Enter(recognitionLock);

                                recordingGesture = false;

                                if (Properties.Settings.Default.EnableSoundEffects) { captureSound.Play(); Logging.LogAudioEvent("capture", false); }

                                DateTime start = DateTime.Now;
                                gesture.ClassName = gestureToTrain;
                                GestureRecognition.AddTrainingExample(gesture, gestureToTrain);
                                Logging.LogTrainingEvent("Add gesture: " + gestureToTrain);
                                if (!autoTrainGesture)
                                {
                                    GestureRecognition.Train();
                                    Debug.WriteLine("Training: " + (DateTime.Now - start).TotalMilliseconds + " ms");
                                }

                                start = DateTime.Now;
                                if (!autoTrainGesture)
                                {
                                    //trainingForm.UpdateGestureList();
                                    trainingForm.AddGesture(gesture);
                                    Debug.WriteLine("Updating List: " + (DateTime.Now - start).TotalMilliseconds + " ms");
                                }

                                //Monitor.Exit(recognitionLock);
                            }
                            else
                            {
                                gesture.ClassName = GestureRecognition.PredictGesture(gesture);
                            }
                        }

                        Monitor.Enter(recognitionLock);

                        // predict the most likely location over the coarse of the gesture, weighted by voting and image focus
                        Dictionary<string, float>[] tempCoarseProbabilities = gestureCoarseLocationProbabilities.ToArray();
                        Dictionary<string, float>[] tempFineProbabilities = gestureFineLocationProbabilities.ToArray();
                        string[] tempCoarsePredictions = gestureCoarseLocationPredictions.ToArray();
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
                            if (i < tempCoarsePredictions.Length && tempCoarsePredictions[i] == coarseLocation)
                            {
                                Dictionary<string, float> probabilities = tempFineProbabilities[i];
                                float weight = Math.Max(tempFocusWeights.Length > i ? tempFocusWeights[i] : 0.01f, 0.01f);
                                foreach (string key in probabilities.Keys)
                                {
                                    if (!totalProbabilities.ContainsKey(key)) totalProbabilities[key] = 0;
                                    totalProbabilities[key] += weight * probabilities[key] / numLocationPredictions;
                                }
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

                        Logging.LogGestureEvent(gesture.ClassName, coarseLocation + " " + fineLocation, gesture);

                        Monitor.Exit(recognitionLock);

                        if (fineLocation == "")
                            Debug.WriteLine("Error");

                        Invoke(new MethodInvoker(delegate
                        {
                            CoarsePredictionLabel.Text = "= " + coarseLocation;
                            CoarseProbabilityLabel.Text = " (response = " + (coarseProbability * 100).ToString("0.0") + ")";
                            FinePredictionLabel.Text = "= " + fineLocation;
                            FineProbabilityLabel.Text = " (response = " + (fineProbability * 100).ToString("0.0") + ")";
                            GesturePredictionLabel.Text = gesture.ClassName;
                            if (Properties.Settings.Default.EnableSpeechOutput)
                            {
                                if (Properties.Settings.Default.EnableApplicationDemos)
                                {
                                    string actionResult = GestureActionMap.PerformAction(gesture.ClassName, coarseLocation, fineLocation, Properties.Settings.Default.GestureMode, Properties.Settings.Default.FixedApplicationResponses, lockToCurrentTask: Properties.Settings.Default.LockToCurrentTask);
                                    if (actionResult != null && actionResult.Length > 0)
                                    {
                                        speech.SpeakAsyncCancelAll();
                                        speech.SelectVoice(MenuVoice);
                                        //speech.SpeakAsync(gesture.ClassName + " " + coarseLocation + " " + fineLocation);
                                        speech.SpeakAsync(actionResult);
                                        Logging.LogAudioEvent(actionResult);
                                    }
                                }
                                else
                                {
                                    speech.SpeakAsyncCancelAll();
                                    speech.SelectVoice(MenuVoice);
                                    speech.SpeakAsync(gesture.ClassName + " " + coarseLocation + " " + fineLocation);
                                    Logging.LogAudioEvent(gesture.ClassName + " " + coarseLocation + " " + fineLocation);
                                }
                            }
                        }));
                    }
                });
            };

            // Initialize the camera and other sensors in a background thread to avoid hanging the UI
            Task.Factory.StartNew(() =>
            {
                Camera.Instance.FrameAvailable += Camera_FrameAvailable;
                Camera.Instance.Error += (string error) => 
                {
                    Debug.WriteLine(error);
                    Invoke(new MethodInvoker(delegate { Text = "Error: could not connect to camera"; }));
                };
                Camera.Instance.Brightness = Properties.Settings.Default.CameraBrightness;

                Sensors.Instance.ReadingAvailable += Sensors_ReadingAvailable;

                Camera.Instance.Connect(); Logging.LogOtherEvent("camera_connected");
                Sensors.Instance.Connect(Properties.Settings.Default.SensorPort); Logging.LogOtherEvent("sensors_connected");

                Sensors.Instance.NumSensors = Properties.Settings.Default.SingleIMU ? 1 : 2;

                Invoke(new MethodInvoker(delegate
                {
                    if (Properties.Settings.Default.TrainingVisible) trainingToolStripMenuItem.PerformClick();
                    if (Properties.Settings.Default.SettingsVisible) settingsToolStripMenuItem.PerformClick();
                    if (Properties.Settings.Default.TestingVisible) testingToolStripMenuItem.PerformClick();
                }));
            });
        }

        DateTime last = DateTime.Now;

        private void OnBodyInputDemo_Move(object sender, EventArgs e)
        {
            Properties.Settings.Default.MainLocation = Location;
            Properties.Settings.Default.Save();
        }

        private void testingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            testingForm.Show();
            Properties.Settings.Default.TestingVisible = true;
            Properties.Settings.Default.Save();
        }

        private void triggerPhoneCallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GestureActionMap.Context = "Phone";
            if (Properties.Settings.Default.EnableSoundEffects) { phoneSound.Play(); Logging.LogAudioEvent("phone", false); }

        }

        private void OnBodyInputDemo_Resize(object sender, EventArgs e)
        {
            Properties.Settings.Default.MainSize = Size;
            Properties.Settings.Default.Save();
        }

        private void Sensors_ReadingAvailable(Sensors.Reading reading)
        {
            OrientationTracker.Primary.UpdateWithReading(reading);
            //OrientationTracker.Secondary.UpdateWithReading(reading, -1, true); // uncomment if using two IMUs

            Quaternion quaternion = OrientationTracker.Primary.EstimateOrientation();
            EulerAngles orientation = quaternion.GetEulerAngles();

            int resolution = 10;
            int yaw = resolution * (int)Math.Round(orientation.Yaw * 180 / Math.PI / resolution);
            int pitch = resolution * (int)Math.Round(orientation.Pitch * 180 / Math.PI / resolution);
            int roll = resolution * (int)Math.Round(orientation.Roll * 180 / Math.PI / resolution);
            reading.Orientation1 = quaternion;

            if ((DateTime.Now - last).TotalMilliseconds > 30)
            {
                Invoke(new MethodInvoker(delegate
                {
                    OrientationLabel.Text = yaw + ", " + pitch + ", " + roll;
                    //OrientationLabel.Text = orientation.W.ToString("0.0") + ", " + orientation.X.ToString("0.0") + ", " + orientation.Y.ToString("0.0") + ", " + orientation.Z.ToString("0.0");
                }));
                last = DateTime.Now;
            }

            TouchSegmentation.UpdateWithReading(reading);

            Logging.LogSensorReading(reading);

            sensorReadingHistory.Add(reading);
            while(sensorReadingHistory.Count > sensorReadingPreBuffer) sensorReadingHistory.Take();
            if(touchDown) gestureSensorReadings.Add(reading);
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            (new SettingsForm()).Show();
            Properties.Settings.Default.SettingsVisible = true;
            Properties.Settings.Default.Save();
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if(countdown > 1)
            {
                countdown--;
                if (Properties.Settings.Default.EnableSoundEffects) { tickSound.Play(); Logging.LogAudioEvent("tick", false); }
                Invoke(new MethodInvoker(delegate
                {
                    CountdownLabel.Text = countdown.ToString();
                }));
            }
            else if (countdown <= 1)
            {
                countdown = 0;
                Invoke(new MethodInvoker(delegate
                {
                    CountdownLabel.Text = countdown.ToString();
                }));
                timer.Stop();

                if (prepareToAutoTrainLocation)
                {
                    prepareToAutoTrainLocation = false;
                    autoTrainLocation = true;
                }
                else
                {
                    if (Properties.Settings.Default.EnableSoundEffects) { captureSound.Play(); Logging.LogAudioEvent("capture", false); }
                    ImageTemplate template = AddTemplate();
                    Logging.LogTrainingEvent("Added template: " + template["coarse"] + " " + template["fine"]);
                }

                Invoke(new MethodInvoker(delegate
                {
                    CountdownLabel.Visible = false;
                }));
            }
        }

        private ImageTemplate AddTemplate(ImageTemplate template = null)
        {
            bool newTemplate = template == null;
            if(template == null) template = CopyTemplate(currTemplate);
            template["coarse"] = coarseLocationToTrain;
            template["fine"] = fineLocationToTrain;
                
            Localization.Instance.AddTrainingExample(template, coarseLocationToTrain, fineLocationToTrain);
            Localization.Instance.Train();

            // autosave the image in the background
            //Bitmap tempImage = (Bitmap)template.Image.Bitmap.Clone();
            Task.Factory.StartNew(() =>
            {
                if (!Directory.Exists(Path.Combine("savedProfiles", "autosave"))) Directory.CreateDirectory(Path.Combine("savedProfiles", "autosave"));
                int index = 1;
                while (File.Exists(Path.Combine("savedProfiles", "autosave", coarseLocationToTrain + "_" + fineLocationToTrain + "_" + index + ".png"))) index++;
                Invoke(new MethodInvoker(delegate { template.Image.Save(Path.Combine("savedProfiles", "autosave", coarseLocationToTrain + "_" + fineLocationToTrain + "_" + index + ".png")); }));
            });

            //trainingForm.UpdateLocationList();
            try
            {
                trainingForm.AddLocation(template);
            }
            catch (Exception ex)
            {

            }

            return template;
        }

        //private void PerformCrossValidation()
        //{
        //    Invoke(new MethodInvoker(delegate { InfoBox.Text = ""; InfoBox.Text += "Performing Cross-Validation..." + Environment.NewLine; }));
        //    Task.Factory.StartNew(() =>
        //    {
        //        foreach (string className in Localization.Instance.samples.Keys)
        //        {
        //            int correct = 0;
        //            if (Localization.Instance.samples[className].Count > 1)
        //            {
        //                Parallel.ForEach(Localization.Instance.samples[className], (ImageTemplate testTemplate) =>
        //                //foreach (ImageTemplate testTemplate in Localization.Instance.samples[className])
        //                {
        //                    Localization localization = new Localization();
        //                    foreach (string trainClassName in Localization.Instance.samples.Keys)
        //                        foreach (ImageTemplate trainTemplate in Localization.Instance.samples[trainClassName])
        //                            if (trainTemplate != testTemplate)
        //                                localization.AddTrainingExample(trainTemplate, (string)trainTemplate["coarse"], (string)trainTemplate["fine"]);
        //                    localization.Train();
        //                    bool foundFeatureMatch;
        //                    Dictionary<string, float> coarseProbabilities, fineProbabilities;
        //                    string coarsePrediction = localization.PredictCoarseLocation(testTemplate, out coarseProbabilities);
        //                    string finePrediction = localization.PredictFineLocation(testTemplate, out foundFeatureMatch, out fineProbabilities, true, false, false, coarsePrediction);
        //                    float coarseProbability = coarseProbabilities[coarsePrediction];
        //                    float fineProbability = fineProbabilities[finePrediction];

        //                    if (coarsePrediction.Equals((string)testTemplate["coarse"]) && finePrediction.Equals((string)testTemplate["fine"]))
        //                    //correct++;
        //                    Interlocked.Increment(ref correct);
        //                    //Invoke(new MethodInvoker(delegate
        //                    //{
        //                    //    InfoBox.Text += ((string)testTemplate["fine"] + Localization.Instance.samples[className].IndexOf(testTemplate) + " = " + coarsePrediction + " (" + (coarseProbability * 100).ToString("0.0") + ") / " + finePrediction + " (" + (100 * fineProbability).ToString("0.0") + ")") + ", ";
        //                    //}));
        //                });
        //            }

        //            Invoke(new MethodInvoker(delegate
        //            {
        //                InfoBox.Text += Localization.Instance.coarseLocations[className] + className + ": " + ((float)correct / Localization.Instance.samples[className].Count * 100).ToString("0.0") + "%, ";
        //            }));
        //        }
        //        Invoke(new MethodInvoker(delegate
        //        {
        //            InfoBox.Text += Environment.NewLine + "Done" + Environment.NewLine;
        //        }));
        //    });
        //}

        private void LocalizationTest_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!closing)
            {
                closing = true;
                e.Cancel = true;

                // TODO: cleanup any sensors and resources
                Logging.Stop();
                Sensors.Instance.Disconnect();
                Camera.Instance.Disconnect();

                Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(500);
                    Invoke(new MethodInvoker(delegate { Close(); }));
                });
            }
        }

        ImageTemplate CopyTemplate(ImageTemplate template)
        {
            ImageTemplate newTemplate = template.Clone();
            return newTemplate;
        }

        private void trainingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            trainingForm.Show();
            Properties.Settings.Default.TrainingVisible = true;
            Properties.Settings.Default.Save();
        }

        int numLocationPredictions = 15;
        void Camera_FrameAvailable(VideoFrame frame)
        {
            if (closing) return;

            numLocationPredictions = Properties.Settings.Default.TestingMode.Contains("Training") ? 1 : Properties.Settings.Default.PredictionSmoothing;

            ImageTemplate template = new ImageTemplate(frame.Clone());
            FPS.Camera.Update();

            Task.Factory.StartNew(() =>
            {
                try
                {
                    Logging.LogVideoFrame(frame.Image.Bitmap);

                    float focus = 0;
                    //lock (processingLock)
                    if (Monitor.TryEnter(preprocessingLock))
                    {
                        ImageProcessing.ProcessTemplate(template, false);
                        focus = ImageProcessing.ImageFocus(template);
                        Logging.LogFrameProcessed();
                        currTemplate = template;
                        FPS.Instance("processing").Update();
                        Monitor.Exit(preprocessingLock);
                    }
                    else
                    {
                        return;
                    }

                    string coarseLocation = "", fineLocation = "";
                    float coarseProbability = 0, fineProbability = 0;
                    bool hasUpdate = false;
                    string predictedCoarseLocation = "", predictedFineLocation = "";
                    if (touchDown && !trainingForm.Training && Monitor.TryEnter(recognitionLock))
                    {
                        try
                        {
                            if (Localization.Instance.GetNumTrainingExamples() > 0)
                            {
                                Dictionary<string, float> coarseProbabilities = new Dictionary<string, float>();
                                predictedCoarseLocation = Localization.Instance.PredictCoarseLocation(currTemplate, out coarseProbabilities);
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

                                coarseLocationPredictions.Add(coarseLocation);
                                gestureCoarseLocationPredictions.Add(coarseLocation);
                                while (coarseLocationPredictions.Count > numLocationPredictions) coarseLocationPredictions.Take();

                                if (!Properties.Settings.Default.CoarseOnly)
                                {
                                    bool foundFeatureMatch = false;
                                    Dictionary<string, float> fineProbabilities = new Dictionary<string, float>();
                                    predictedFineLocation = Localization.Instance.PredictFineLocation(currTemplate, out foundFeatureMatch, out fineProbabilities, true, false, false, coarseLocation);
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
                                    //foreach (Dictionary<string, float> probabilities in fineLocationProbabilities)
                                    for(int fineIndex = 0; fineIndex < fineLocationProbabilities.Count; fineIndex++)
                                    {
                                        Dictionary<string, float> probabilities = fineLocationProbabilities.ToArray()[fineIndex];

                                        // make sure that the fine prediction matches the classes for the coarse prediction
                                        if (coarseLocationPredictions.ToArray()[fineIndex] == coarseLocation)
                                        {
                                            foreach (string key in probabilities.Keys)
                                            {
                                                if (!totalProbabilities.ContainsKey(key)) totalProbabilities[key] = 0;
                                                totalProbabilities[key] += probabilities[key] / numLocationPredictions;
                                            }
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

                                Logging.LogLocationEvent(predictedCoarseLocation + " " + predictedFineLocation, coarseLocation + " " + fineLocation);

                                gestureFocusWeights.Add(focus);

                                FPS.Instance("matching").Update();
                                hasUpdate = true;
                            }

                            if(autoTrainLocation && (coarseLocation != coarseLocationToTrain || fineLocation != fineLocationToTrain) && Monitor.TryEnter(trainingLock))
                            {
                                trainingForm.Training = true;
                                ImageTemplate newTemplate = CopyTemplate(currTemplate);
                                Invoke(new MethodInvoker(delegate { TrainingLabel.Visible = true; }));
                                if (Properties.Settings.Default.EnableSoundEffects) { captureSound.Play(); Logging.LogAudioEvent("capture", false); }
                                AddTemplate(newTemplate);
                                Logging.LogTrainingEvent("Added template: " + newTemplate["coarse"] + " " + newTemplate["fine"]);
                                Thread.Sleep(100);
                                //if (Properties.Settings.Default.EnableSoundEffects) { beepSound.Play(); Logging.LogAudioEvent("beep", false); }
                                trainingForm.Training = false;
                                Invoke(new MethodInvoker(delegate { TrainingLabel.Visible = false; }));
                                Monitor.Exit(trainingLock);
                                return;
                            }

                            if ((DateTime.Now - touchStart).TotalMilliseconds > Properties.Settings.Default.HoverTimeThreshold)
                            {
                                hovering = true;
                                //if (hoverCoarseLocation == null || coarseLocation == hoverCoarseLocation) // make sure we have the same coarse location, to help prevent jumping around
                                {
                                    hoverCoarseLocation = coarseLocation;
                                    if (hoverFineLocation == null || fineLocation != hoverFineLocation) // make sure we haven't reported the fine location already
                                    {
                                        hoverFineLocation = fineLocation;
                                        Debug.WriteLine(hoverCoarseLocation + " " + hoverFineLocation);
                                        Invoke(new MethodInvoker(delegate { GesturePredictionLabel.Text = "Hover"; }));
                                        if (Properties.Settings.Default.EnableSpeechOutput)
                                        {
                                            if (Properties.Settings.Default.EnableApplicationDemos)
                                            {
                                                string actionResult = GestureActionMap.PerformAction("Hover", coarseLocation, fineLocation, Properties.Settings.Default.GestureMode, Properties.Settings.Default.FixedApplicationResponses);
                                                if (actionResult != null && actionResult.Length > 0)
                                                {
                                                    speech.SpeakAsyncCancelAll();
                                                    speech.SelectVoice(MenuVoice);
                                                    speech.SpeakAsync(actionResult);
                                                    Logging.LogAudioEvent(actionResult);
                                                }
                                            }
                                            else
                                            {
                                                speech.SpeakAsyncCancelAll();
                                                speech.SelectVoice(MenuVoice);
                                                speech.SpeakAsync("Hover " + coarseLocation + " " + fineLocation);
                                                Logging.LogAudioEvent("Hover " + coarseLocation + " " + fineLocation);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        finally
                        {
                            Monitor.Exit(recognitionLock);
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
                        Text = FPS.Camera.Average.ToString("0") + " fps camera / " + (Sensors.Instance.IsConnected ? FPS.Sensors.Average.ToString("0") + " fps sensors / " + FPS.Instance("processing").Average.ToString("0") + " fps processing / " + FPS.Instance("matching").Average.ToString("0") + " fps matching" : "Waiting for Sensors") /*+ " / focus = " + focus.ToString("0")*/;
                    }));
                }
                catch { }
            });
        }
    }
}
