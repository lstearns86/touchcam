using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using HandSightLibrary;
using HandSightLibrary.ImageProcessing;

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;

using System.Speech.Synthesis;
using System.Media;

using Newtonsoft.Json;

namespace HandSightOnBodyInteractionGPU
{
    public partial class LocationAndGestureDataCollectionTool : Form
    {
        bool calibrating = false, closing = false, recordingGesture = false;
        ImageTemplate currTemplate;
        string preprocessingLock = "processing lock", recognitionLock = "recognition lock", trainingLock = "training lock";
        string coarseLocationToTrain = "", fineLocationToTrain = "", gestureToTrain = "";

        BlockingCollection<Sensors.Reading> sensorReadingHistory = new BlockingCollection<Sensors.Reading>();
        BlockingCollection<Sensors.Reading> gestureSensorReadings = new BlockingCollection<Sensors.Reading>();
        bool touchDown = false, recentTouchUp = false;
        int touchUpDelay = 100, sensorReadingPreBuffer = 50;
        DateTime touchStart = DateTime.Now;
        
        SpeechSynthesizer speech = new SpeechSynthesizer();
        SoundPlayer captureSound, beepSound, chimeSound;

        TrainingForm trainingForm = new TrainingForm();
        bool autoTrainGesture = false, autoTrainLocation = false, prepareToAutoTrainLocation = false;

        private void LocationAndGestureDataCollectionTool_Load(object sender, EventArgs e)
        {
            StartLogging();
        }

        private void StartLogging()
        {
            SaveFileDialog dialog = new SaveFileDialog();
            //dialog.OverwritePrompt = true;
            dialog.InitialDirectory = Properties.Settings.Default.LogDirectory;
            dialog.Title = "Choose where to save log files";
            dialog.Filter = "Log Files|*.log";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string dir = Path.GetDirectoryName(dialog.FileName);
                string filename = Path.GetFileNameWithoutExtension(dialog.FileName);
                if (File.Exists(Path.Combine(dir, filename + ".log")))
                {
                    int index = 2;
                    while (File.Exists(Path.Combine(dir, filename + "_" + index + ".log"))) index++;
                    filename += "_" + index;
                }

                Logging.Start(Path.Combine(dir, filename + ".log"));
            }
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Logging.Running) Logging.Stop();
            StartLogging();
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Logging.Stop();
        }

        int numAutoSamplesGathered = 0;

        public LocationAndGestureDataCollectionTool()
        {
            InitializeComponent();

            Location = Properties.Settings.Default.MainLocation;
            Size = Properties.Settings.Default.MainSize;

            captureSound = new SoundPlayer("sounds\\camera_capture.wav");
            beepSound = new SoundPlayer("sounds\\camera_beep.wav");
            chimeSound = new SoundPlayer("sounds\\endTask.wav");
            speech.Rate = 2;

            captureSound.LoadAsync();
            beepSound.LoadAsync();
            chimeSound.LoadAsync();

            Text = "Connecting to Camera and Sensors...";

            trainingForm.RecordLocation += (string coarseLocation, string fineLocation) =>
            {
                Logging.LogTrainingEvent("prepare_to_capture_location");
                coarseLocationToTrain = coarseLocation;
                fineLocationToTrain = fineLocation;

                if (Properties.Settings.Default.EnableSoundEffects) captureSound.Play();
                ImageTemplate newTemplate = AddTemplate();
                Logging.LogTrainingEvent("Added template: " + newTemplate["coarse"] + " " + newTemplate["fine"]);
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
                numAutoSamplesGathered = 0;
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

                if (Properties.Settings.Default.EnableSoundEffects) beepSound.Play();
                autoTrainLocation = true;
            };
            trainingForm.StopAutoCapturingLocation += () =>
            {
                autoTrainLocation = false;
                Logging.LogTrainingEvent("stop_autocapture_location");
            };

            TouchSegmentation.TouchDownEvent += () => 
            {
                touchDown = true;
                touchStart = DateTime.Now;
                Sensors.Instance.Brightness = 1;

                Logging.LogTouchEvent("touch_down");

                // reset the location predictions
                if (!recentTouchUp)
                {
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
                Logging.LogTouchEvent("touch_up");
                
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

                        if (trainingForm.Training) return;

                        // process the gesture

                        Gesture gesture = new Gesture(gestureSensorReadings.ToArray());
                        if (gestureSensorReadings.Count > 60)
                        {
                            GestureRecognition.PreprocessGesture(gesture);

                            if (recordingGesture || autoTrainGesture)
                            {
                                //Monitor.Enter(recognitionLock);

                                if(recordingGesture) { Logging.LogTrainingEvent("stop_recording_gesture"); }

                                recordingGesture = false;

                                if (Properties.Settings.Default.EnableSoundEffects) { captureSound.Play(); Logging.LogAudioEvent("capture", false); }

                                DateTime start = DateTime.Now;
                                gesture.ClassName = gestureToTrain;
                                gesture.IsTrainingData = true;
                                GestureRecognition.AddTrainingExample(gesture, gestureToTrain);

                                // autosave the gesture in the background
                                Task.Factory.StartNew(() =>
                                {
                                    if (!Directory.Exists(Path.Combine("savedProfiles", "autosave"))) Directory.CreateDirectory(Path.Combine("savedProfiles", "autosave"));
                                    int index = 1;
                                    while (File.Exists(Path.Combine("savedProfiles", "autosave", gestureToTrain + "_" + index + ".png"))) index++;
                                    gesture.Path = Path.Combine("savedProfiles", "autosave", gestureToTrain + "_" + index + ".png");
                                    string json = JsonConvert.SerializeObject(gesture);
                                    Invoke(new MethodInvoker(delegate { File.WriteAllText(gesture.Path, json); }));
                                });

                                Logging.LogTrainingEvent("Add gesture: " + gestureToTrain + ", path = " + gesture.Path);

                                if (autoTrainGesture)
                                {
                                    numAutoSamplesGathered++;
                                    if(numAutoSamplesGathered >= Properties.Settings.Default.NumAutoGestureSamples)
                                    {
                                        if (Properties.Settings.Default.EnableSoundEffects) { chimeSound.Play(); Logging.LogAudioEvent("chime", false); }
                                        Logging.LogTrainingEvent("stop_autocapture_gesture");
                                        autoTrainGesture = false;
                                    }
                                }
                                
                                start = DateTime.Now;
                                if (!autoTrainGesture)
                                {
                                    trainingForm.AddGesture(gesture);
                                    trainingForm.IsSaved = false;
                                    Debug.WriteLine("Updating List: " + (DateTime.Now - start).TotalMilliseconds + " ms");
                                }
                            }
                        }
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
                Camera.Instance.Brightness = Properties.Settings.Default.CameraBrightness;

                Sensors.Instance.ReadingAvailable += Sensors_ReadingAvailable;

                Camera.Instance.Connect(); Logging.LogOtherEvent("camera_connected");
                Sensors.Instance.Connect(Properties.Settings.Default.SensorPort); Logging.LogOtherEvent("sensors_connected");

                Sensors.Instance.NumSensors = Properties.Settings.Default.SingleIMU ? 1 : 2;

                Invoke(new MethodInvoker(delegate
                {
                    if (Properties.Settings.Default.TrainingVisible) trainingToolStripMenuItem.PerformClick();
                    if (Properties.Settings.Default.SettingsVisible) settingsToolStripMenuItem.PerformClick();
                }));
            });
        }

        DateTime last = DateTime.Now;

        private void OnBodyInputDemo_Move(object sender, EventArgs e)
        {
            Properties.Settings.Default.MainLocation = Location;
            Properties.Settings.Default.Save();
        }

        private void OnBodyInputDemo_Resize(object sender, EventArgs e)
        {
            Properties.Settings.Default.MainSize = Size;
            Properties.Settings.Default.Save();
        }

        private void Sensors_ReadingAvailable(Sensors.Reading reading)
        {
            OrientationTracker.Primary.UpdateWithReading(reading);
            
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
                template["path"] = Path.Combine("savedProfiles", "autosave", coarseLocationToTrain + "_" + fineLocationToTrain + "_" + index + ".png");
                Invoke(new MethodInvoker(delegate { template.Image.Save((string)template["path"]); }));
            });

            //trainingForm.UpdateLocationList();
            try
            {
                trainingForm.AddLocation(template);
                if (newTemplate) trainingForm.IsSaved = false;
            }
            catch (Exception ex)
            {

            }

            return template;
        }

        private void LocalizationTest_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(!trainingForm.IsSaved)
            {
                if(MessageBox.Show("You have unsaved training samples. Are you sure you want to close without saving?", "Unsaved Data", MessageBoxButtons.YesNoCancel) != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
            }

            if (!closing)
            {
                closing = true;
                e.Cancel = true;

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

        void Camera_FrameAvailable(VideoFrame frame)
        {
            if (closing) return;

            ImageTemplate template = new ImageTemplate(frame.Clone());
            FPS.Camera.Update();

            Task.Factory.StartNew(() =>
            {
                try
                {
                    Logging.LogVideoFrame(frame.Image.Bitmap);

                    float focus = 0;
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
                                coarseLocation = predictedCoarseLocation;
                                
                                if (!Properties.Settings.Default.CoarseOnly)
                                {
                                    bool foundFeatureMatch = false;
                                    Dictionary<string, float> fineProbabilities = new Dictionary<string, float>();
                                    predictedFineLocation = Localization.Instance.PredictFineLocation(currTemplate, out foundFeatureMatch, out fineProbabilities, true, false, false, coarseLocation);
                                    fineLocation = predictedFineLocation;
                                }

                                Logging.LogLocationEvent(predictedCoarseLocation + " " + predictedFineLocation);

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
                                trainingForm.IsSaved = false;
                                trainingForm.Training = false;
                                Invoke(new MethodInvoker(delegate { TrainingLabel.Visible = false; }));
                                Monitor.Exit(trainingLock);
                                return;
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
                            FinePredictionLabel.Text = fineLocation;
                        }
                        Text = FPS.Camera.Average.ToString("0")  + " fps camera / " + (Sensors.Instance.IsConnected ? FPS.Sensors.Average.ToString("0") + " fps sensors / " + FPS.Instance("processing").Average.ToString("0") + "/" + FPS.Instance("matching").Average.ToString("0") + " fps processing" : "Waiting for Sensors") /*+ " / focus = " + focus.ToString("0")*/;
                    }));
                }
                catch { }
            });
        }
    }
}
