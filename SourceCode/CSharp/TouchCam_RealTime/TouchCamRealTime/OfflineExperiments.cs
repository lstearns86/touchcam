using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

using Alea.CUDA;

using Emgu.CV;
using Emgu.CV.Structure;

using TouchCamLibrary;
using TouchCamLibrary.ImageProcessing;

using AForge.Video.FFMPEG;

using Newtonsoft.Json;

namespace TouchCam
{
    public partial class OfflineExperiments : Form
    {
        static List<string> fingers = new List<string>(new string[] { "Thumb", "Index", "Middle", "Ring", "Baby" });
        static List<string> palm = new List<string>(new string[] { "PalmCenter", "PalmUp", "PalmDown", "PalmLeft", "PalmRight" });
        static List<string> other = new List<string>(new string[] { "BackHand", "Wrist", "Ear", "Shoulder", "Thigh" });

        static readonly Dictionary<string, List<string>> groups = new Dictionary<string, List<string>>
        {
            { "finger", fingers },
            { "palm", palm },
            { "backhand", new List<string>(new string[] { "BackHand", "Wrist" }) },
            { "ear", new List<string>(new string[] { "Ear" }) },
            { "shoulder", new List<string>(new string[] { "Shoulder" }) },
            { "thigh", new List<string>(new string[] { "Thigh" }) }
        };

        static readonly Dictionary<string, string> groupForRegion = new Dictionary<string, string>
        {
            { "Thumb", "finger" },
            { "Index", "finger" },
            { "Middle", "finger" },
            { "Ring", "finger" },
            { "Baby", "finger" },
            { "PalmUp", "palm" },
            { "PalmLeft", "palm" },
            { "PalmCenter", "palm" },
            { "PalmRight", "palm" },
            { "PalmDown", "palm" },
            { "BackHand", "backhand" },
            { "Wrist", "backhand" },
            { "Shoulder", "shoulder" },
            { "Ear", "ear" },
            { "Thigh", "thigh" },
            { "Palm", "palm" }
        };

        static readonly Dictionary<string, string> coarseForFine = new Dictionary<string, string>
        {
            { "Up", "Palm" },
            { "Left", "Palm" },
            { "Center", "Palm" },
            { "Right", "Palm" },
            { "Down", "Palm" },
            { "Inner", "Wrist" },
            { "Outer", "Wrist" },
            { "Front", "Ear" },
            { "Ear", "Ear" },
            { "Thigh", "Thigh" }
        };

        //static readonly Dictionary<string, List<string>> groups2 = new Dictionary<string, List<string>>
        //{
        //    { "palm", new List<string>(new string[] {"Up", "Down", "Left", "Right", "Center" }) },
        //    { "wrist", new List<string>(new string[] { "Inner", "Outer" }) },
        //    { "ear", new List<string>(new string[] { "Front" }) },
        //    { "thigh", new List<string>(new string[] { "Thigh" }) }
        //};

        //static readonly Dictionary<string, string> coarseForFine = new Dictionary<string, string>
        //{
        //    { "Up", "palm" },
        //    { "Down", "palm" },
        //    { "Left", "palm" },
        //    { "Right", "palm" },
        //    { "Center", "palm" },
        //    { "Inner", "wrist" },
        //    { "Outer", "wrist" },
        //    { "Front", "ear" },
        //    { "Thigh", "thigh" },
        //};

        public OfflineExperiments()
        {
            InitializeComponent();
        }

        private void SetProgressMax(int max)
        {
            Invoke(new MethodInvoker(delegate { Progress.Maximum = max; Application.DoEvents(); }));
        }

        private void SetProgress(int progress)
        {
            Invoke(new MethodInvoker(delegate { Progress.Value = progress; Application.DoEvents(); }));
        }

        private void IncrementProgress(int amount = 1)
        {
            Invoke(new MethodInvoker(delegate { Progress.Increment(amount); Application.DoEvents(); }));
        }

        private void SetStatus(string status)
        {
            Invoke(new MethodInvoker(delegate { StatusLabel.Text = status; Application.DoEvents(); }));
        }

        private void OfflineExperiments_Load(object sender, EventArgs e)
        {
            //ProcessData(@"D:\UserStudies\UIST2016\PalmData\", new string[] { "p1", "p2", "p3", "p4", "p5", "p6", "p7", "new_p8", "p9", "p10", "p11", "p12", "p13", "p14", "p15", "p16", "p17", "p18", "p19", "p20", "p21", "p22", "p23", "p24" }, "processed");
            //ProcessData(@"D:\UserStudies\UIST2016\PalmData\", new string[] { "p1" }, "processed");

            //ExtractVideoFrames(@"D:\UserStudies\UIST2016\PalmData\", new string[] { "p1", "p2", "p3", "p4", "p5", "p6", "p7", "new_p8", "p9", "p10", "p11", "p12", "p13", "p14", "p15", "p16", "p17", "p18", "p19", "p20", "p21", "p22", "p23", "p24" });
            //ExtractVideoFrames(@"D:\UserStudies\UIST2016\PalmData\", new string[] { "p1" });

            //EvaluateVideoPredictions(@"D:\UserStudies\UIST2016\PalmData\", new string[] { "p4" }, "LocationVideoFrames");
            //EvaluateVideoPredictions(@"D:\UserStudies\UIST2016\PalmData\", new string[] { "p1", "p2", "p3", "p4", "p5", "p6", "p7", "new_p8", "p9", "p10", "p11", "p12", "p13", "p14", "p15", "p16", "p17", "p18", "p19", "p20", "p21", "p22", "p23", "p24" }, "LocationVideoFrames");

            //ExtractVideoFrames2(@"D:\UserStudies\Ubicomp2017\", new string[] { "p01" }, "Logs");
            //ExtractVideoFrames2(@"D:\UserStudies\Ubicomp2017\", new string[] { "p01", "p02b", "p03", "p04", "p05", "p06", "p07", "p08", "p09", "p10", "p11", "p12" }, "Logs");

            //CrossValidate(@"D:\UserStudies\Ubicomp2017\", new string[] { "p09b" }, "Samples");
            //CrossValidate(@"D:\UserStudies\Ubicomp2017\", new string[] { "p01", "p02", "p03", "p04", "p05", "p06", "p07", "p08", "p09", "p10", "p11", "p12" }, "Samples");

            //CrossValidate(@"D:\UserStudies\UIST2016\PalmData\", new string[] { "p1", "p2", "p3", "p4", "p5", "p6", "p7", "new_p8", "p9", "p10", "p11", "p12", "p13", "p14", "p15", "p16", "p17", "p18", "p19", "p20", "p21", "p22", "p23", "p24" }, "processed", "*Gesture*.png", @"p\d+_Gesture_.*?_");

            //EvaluateVideoPredictions2(@"D:\UserStudies\Ubicomp2017\", new string[] { "p05" }, "Samples", Path.Combine("Logs", "LocationVideoFrames"));
            //EvaluateVideoPredictions2(@"D:\UserStudies\Ubicomp2017\", new string[] { "p01", "p02b", "p03", "p04", "p05", "p07", "p08", "p09b", "p10", "p11", "p12" }, "Samples", Path.Combine("Logs", "LocationVideoFrames"));

            //TestLocalizationAccuracyWithVideoPredictions(new string[] { @"D:\UserStudies\Ubicomp2017\p12\Samples" }, @"D:\UserStudies\Ubicomp2017\p12\Logs", 20);

            //EvaluateFrameAccuracy(@"D:\UserStudies\Ubicomp2017\", new string[] { "p01" }, Path.Combine("Logs", "LocationVideoFrames"));
            //EvaluateFrameAccuracy(@"D:\UserStudies\Ubicomp2017\", new string[] { "p01", "p02b", "p03", "p04", "p05", "p07", "p08", "p09b", "p10", "p11", "p12" }, Path.Combine("Logs", "LocationVideoFrames"));

            //string json = File.ReadAllText(@"D:\UserStudies\Ubicomp2017\TimeAndGestureStudy\pilot1\B\Samples\Swipe Left_0.gest");
            //json = json.Replace(",\"Visualization\":\"System.Drawing.Bitmap\"", "");
            //Gesture gestureA = JsonConvert.DeserializeObject<Gesture>(json);
            //json = File.ReadAllText(@"C:\Users\lstearns\Documents\GitHub\handsight-on-body-interaction\HandSightOnBodyInteractionGPU\bin\x64\Debug\savedProfiles\allGestures\Swipe Left_0.gest");
            //json = json.Replace(",\"Visualization\":\"System.Drawing.Bitmap\"", "");
            //Gesture gestureB = JsonConvert.DeserializeObject<Gesture>(json);
            //Debug.WriteLine("here");

            //TestGestureRecognitionAccuracy(@"C:\Users\lstearns\Documents\GitHub\handsight-on-body-interaction\HandSightOnBodyInteractionGPU\bin\x64\Debug\savedProfiles\combined_noTapOrSwipeDown.svm", @"D:\UserStudies\Ubicomp2017\TimeAndGestureStudy\pilot1\A\Samples", false);
            //TestGestureRecognitionAccuracy(@"C:\Users\lstearns\Documents\GitHub\handsight-on-body-interaction\HandSightOnBodyInteractionGPU\bin\x64\Debug\savedProfiles\combined.svm", @"D:\UserStudies\Ubicomp2017\TimeAndGestureStudy\pilot1\A\Samples", true);
            //CrossValidateGestureRecognition(@"D:\UserStudies\Ubicomp2017\TimeAndGestureStudy\pilot1\A\Samples", false);

            //TestLocalizationAccuracy(new string[] { @"C:\Users\lstearns\Documents\GitHub\handsight-on-body-interaction\HandSightOnBodyInteractionGPU\bin\x64\Debug\savedProfiles\uran0825" }, @"C:\Users\lstearns\Documents\GitHub\handsight-on-body-interaction\HandSightOnBodyInteractionGPU\bin\x64\Debug\savedProfiles\uran0902");
            //TestLocalizationAccuracy(new string[] { @"D:\UserStudies\Ubicomp2017\TimeAndGestureStudy\lee\day1_A\Samples", @"D:\UserStudies\Ubicomp2017\TimeAndGestureStudy\lee\day1_B\Samples", @"D:\UserStudies\Ubicomp2017\TimeAndGestureStudy\lee\day1_C\Samples", @"D:\UserStudies\Ubicomp2017\TimeAndGestureStudy\lee\day2_A\Samples", @"D:\UserStudies\Ubicomp2017\TimeAndGestureStudy\lee\day2_B\Samples" }, @"D:\UserStudies\Ubicomp2017\TimeAndGestureStudy\lee\day3_A\Samples");

            //TestLocalizationFromVideo(new string[] { @"D:\UserStudies\Ubicomp2017\TimeAndGestureStudy\lee\A\Logs" }, @"D:\UserStudies\Ubicomp2017\TimeAndGestureStudy\lee\A_2\Samples");
            //TestLocalizationAccuracyWithVideoPredictions(new string[] { @"D:\UserStudies\Ubicomp2017\TimeAndGestureStudy\lee\day1_A\Samples", @"D:\UserStudies\Ubicomp2017\TimeAndGestureStudy\lee\day1_B\Samples", @"D:\UserStudies\Ubicomp2017\TimeAndGestureStudy\lee\day1_C\Samples", @"D:\UserStudies\Ubicomp2017\TimeAndGestureStudy\lee\day2_A\Samples", @"D:\UserStudies\Ubicomp2017\TimeAndGestureStudy\lee\day2_B\Samples" }, @"D:\UserStudies\Ubicomp2017\TimeAndGestureStudy\lee\day3_A\Logs", 20);

            //ComputeImageStatistics(@"D:\UserStudies\Ubicomp2017\TimeAndGestureStudy\lee\day3_A\Samples");

            //TestLocalizationAccuracyCombinations(new string[] { @"D:\UserStudies\Ubicomp2017\TimeAndGestureStudy\lee\day1_A\Samples",
            //                                                    @"D:\UserStudies\Ubicomp2017\TimeAndGestureStudy\lee\day1_B\Samples",
            //                                                    @"D:\UserStudies\Ubicomp2017\TimeAndGestureStudy\lee\day1_C\Samples",
            //                                                    @"D:\UserStudies\Ubicomp2017\TimeAndGestureStudy\lee\day2_A\Samples",
            //                                                    @"D:\UserStudies\Ubicomp2017\TimeAndGestureStudy\lee\day2_B\Samples",
            //                                                    @"D:\UserStudies\Ubicomp2017\TimeAndGestureStudy\lee\day3_A\Samples",
            //                                                    @"D:\UserStudies\Ubicomp2017\TimeAndGestureStudy\lee\day4_A\Samples",
            //                                                    @"D:\UserStudies\Ubicomp2017\TimeAndGestureStudy\lee\day4_B\Samples",
            //                                                    @"D:\UserStudies\Ubicomp2017\TimeAndGestureStudy\lee\day5_A\Samples",
            //                                                    @"D:\UserStudies\Ubicomp2017\TimeAndGestureStudy\lee\day6_A\Samples"},
            //                                                    1, 1);
            //ExtractGestures(new string[] { @"D:\UserStudies\Ubicomp2017\TimeAndGestureStudy\pilot1\A\Logs\S2_pilot1_A_6.log", @"D:\UserStudies\Ubicomp2017\TimeAndGestureStudy\pilot1\B\Logs\S2_pilot1_B.log" },
            //                new string[] { @"D:\UserStudies\Ubicomp2017\TimeAndGestureStudy\pilot1\A\Samples\", @"D:\UserStudies\Ubicomp2017\TimeAndGestureStudy\pilot1\B\Samples\" });

            //TestLocalizationAccuracyCombinations2(new string[] { @"D:\UserStudies\Ubicomp2017\TimeAndGestureStudy\lee\day1_C\Logs",
            //                                                    @"D:\UserStudies\Ubicomp2017\TimeAndGestureStudy\lee\day2_A\Logs",
            //                                                    @"D:\UserStudies\Ubicomp2017\TimeAndGestureStudy\lee\day2_B\Logs",
            //                                                    @"D:\UserStudies\Ubicomp2017\TimeAndGestureStudy\lee\day3_A\Logs",
            //                                                    @"D:\UserStudies\Ubicomp2017\TimeAndGestureStudy\lee\day4_A\Logs",
            //                                                    @"D:\UserStudies\Ubicomp2017\TimeAndGestureStudy\lee\day4_B\Logs",
            //                                                    @"D:\UserStudies\Ubicomp2017\TimeAndGestureStudy\lee\day5_A\Logs",
            //                                                    @"D:\UserStudies\Ubicomp2017\TimeAndGestureStudy\lee\day6_A\Logs"},
            //                                                    1, 1);

            ConvertSensorReadingsToVideo(@"C:\Users\lstearns\Dropbox\_Research\HandSight\SharedFolders\ProjectHandSight_OnHandLocalization\Videos\2017_03_08 - TouchCamSingling\SensorVisualizations\RawSensorReadings\video_reading_visualizations_2.log", @"C:\Users\lstearns\Dropbox\_Research\HandSight\SharedFolders\ProjectHandSight_OnHandLocalization\Videos\2017_03_08 - TouchCamSingling\SensorVisualizations\sensor_visualizations_2_v5.avi", 60);

            //SaveSpeechSamples(@"C:\Users\lstearns\Dropbox\_Research\HandSight\SharedFolders\ProjectHandSight_OnHandLocalization\Videos\2017_03_11 - Applications & Scenarios\SpeechSamples");

            //TestImageMetrics(@"D:\UserStudies\Ubicomp2017\", new string[] { "p01", "p02", "p03", "p04", "p05", "p06", "p07", "p08", "p09", "p10", "p11", "p12" }, "Samples" );
            //TestImageMetricsVideo(@"D:\UserStudies\Ubicomp2017\", new string[] { "p01" }, "Logs");
            //TestVideoFrames(@"D:\UserStudies\Ubicomp2017\", new string[] { "p01", "p02", "p03", "p04", "p05", "p06", "p07", "p08", "p09", "p10", "p11", "p12" }, "Logs", "Samples");
        }

        private void TestImageMetrics(string root, string[] participants, string subfolder)
        {
            Task.Factory.StartNew(() =>
            {
                string data = "";
                SetStatus("Initializing");
                SetProgress(0);
                SetProgressMax(participants.Length);
                foreach (string pid in participants)
                {
                    SetStatus("Processing " + pid);
                    string dir = Path.Combine(root, pid, subfolder);
                    string[] files = Directory.GetFiles(dir, "*.png");
                    foreach (string filename in files)
                    {
                        SetStatus("Processing " + pid + ": " + Path.GetFileNameWithoutExtension(filename));
                        Image<Gray, byte> img = new Image<Gray, byte>(filename);
                        double signal = img.GetAverage().Intensity;
                        Image<Gray, float> laplacian = img.Laplace(1);
                        MCvScalar mean = new MCvScalar(), std = new MCvScalar();
                        CvInvoke.MeanStdDev(img, ref mean, ref std);
                        double contrast = mean.V0;
                        double noise = std.V0;

                        double SNR = signal / noise;
                        double CNR = contrast / noise;

                        float focus = ImageProcessing.ImageFocus(img);

                        float directionalVariance = ImageProcessing.EstimateMotionBlur(img);

                        data += Utils.CSV(pid, Path.GetFileNameWithoutExtension(filename), signal, contrast, noise, SNR, CNR, focus, directionalVariance) + Environment.NewLine;
                    }
                    IncrementProgress();
                }
                File.WriteAllText("results.txt", data);
                Invoke(new MethodInvoker(delegate
                {
                    Clipboard.SetText(data);
                }));
                SetStatus("Done");
            });
        }

        private void TestImageMetricsVideo(string root, string[] participants, string subfolder)
        {
            Task.Factory.StartNew(() =>
            {
                string data = "";

                foreach (string pid in participants)
                {
                    float numBadFrames = 0, numFrames = 0;
                    string dir = Path.Combine(root, pid, subfolder);
                    List<string> jsonFiles = new List<string>(Directory.GetFiles(dir, "*.log"));

                    foreach (string fileName in jsonFiles)
                    {
                        SetStatus("Loading Log File: " + Path.GetFileNameWithoutExtension(fileName));
                        SetProgress(0);
                        List<Logging.LogEvent> events = Logging.ReadLog(fileName);
                        events.Sort((a, b) => { return a.timestamp.CompareTo(b.timestamp); });
                        SetStatus("Processing Log File: " + Path.GetFileNameWithoutExtension(fileName));
                        SetProgressMax(events.Count);

                        bool isTraining = false;

                        // read the log and look for the autocapture training events, which will have labeled video frames in between
                        // identify all video indices for which we can determine a label
                        HashSet<int> isVideoTrainingSample = new HashSet<int>();
                        HashSet<int> isTrainingSample = new HashSet<int>();
                        List<int> currIndices = new List<int>();
                        string currClass = null;
                        int mostRecentIndex = 0;
                        foreach (Logging.LogEvent e in events)
                        {
                            if (e.message == "start_autocapture_location")
                                isTraining = true;
                            else if (e.message == "stop_autocapture_location" || e.message == "External Stop Autocapture" || e.message == "touch_up_timeout")
                            {
                                isTraining = false;
                            }
                            else if (e.message.StartsWith("Added template: "))
                            {
                                isTrainingSample.Add(mostRecentIndex);
                            }
                            else if (e is Logging.VideoFrameEvent)
                            {
                                mostRecentIndex = (e as Logging.VideoFrameEvent).frameIndex;
                                if (isTraining) isVideoTrainingSample.Add(mostRecentIndex);
                            }
                            IncrementProgress();
                        }

                        SetStatus("Reading Video File");
                        SetProgressMax(isVideoTrainingSample.Count);
                        SetProgress(0);

                        // extract video frames
                        string videoFileName = Path.ChangeExtension(fileName, "avi");
                        VideoFileReader videoReader = new VideoFileReader();
                        videoReader.Open(videoFileName);
                        Dictionary<string, int> countsForClass = new Dictionary<string, int>();
                        try
                        {
                            bool hasFramesToRead = true;
                            int frameIndex = 0;
                            while (hasFramesToRead)
                            {
                                Bitmap frame = videoReader.ReadVideoFrame();
                                if (frame == null) hasFramesToRead = false;
                                else if (isVideoTrainingSample.Contains(frameIndex))
                                {
                                    Image<Gray, byte> img = new Image<Gray, byte>(frame);
                                    float focus = ImageProcessing.ImageFocus(img);
                                    float directionalVariance = ImageProcessing.EstimateMotionBlur(img);

                                    if (focus < 1400 || directionalVariance > 20000)
                                        numBadFrames++;
                                    numFrames++;

                                    IncrementProgress();
                                }
                                frameIndex++;
                            }
                        }
                        catch { }
                        videoReader.Close();
                    }

                    data += Utils.CSV(pid, numFrames, numBadFrames, numBadFrames / numFrames) + Environment.NewLine;
                }
                File.WriteAllText("results.txt", data);
                Invoke(new MethodInvoker(delegate
                {
                    Clipboard.SetText(data);
                }));
                SetProgress(0);
                SetStatus("Done");
            });
        }

        private void TestVideoFrames(string root, string[] participants, string logSubfolder, string sampleSubfolder)
        {
            Task.Factory.StartNew(() =>
            {
                string data = "";

                foreach (string pid in participants)
                {
                    string trainingDir = Path.Combine(root, pid, sampleSubfolder);
                    string[] trainFiles = Directory.GetFiles(trainingDir);
                    SetProgress(0);
                    SetStatus("Loading Training Files");
                    SetProgressMax(trainFiles.Length);

                    var samples = new Dictionary<string, List<ImageTemplate>>();
                    Worker worker = Worker.Default;

                    Localization.Instance.Reset();
                    foreach (string filename in trainFiles)
                    {
                        Match match = Regex.Match(Path.GetFileNameWithoutExtension(filename), @"([a-zA-Z]+)_([a-zA-Z]+)_(\d+)");
                        string coarseLocation = match.Groups[1].Value;
                        string fineLocation = match.Groups[2].Value;
                        int index = int.Parse(match.Groups[3].Value);

                        Bitmap bmp = (Bitmap)Bitmap.FromFile(filename);
                        VideoFrame frame = new VideoFrame() { Image = new Image<Gray, byte>(bmp) };
                        frame.ImageGPU = worker.Malloc<byte>(640 * 640);
                        frame.ImageGPU.Scatter(frame.Image.Bytes);
                        ImageTemplate template = new ImageTemplate(frame);
                        template["frameIndex"] = index;
                        template["Path"] = Path.GetFileNameWithoutExtension(filename);
                        template["CoarseLocation"] = coarseLocation;
                        template["FineLocation"] = fineLocation;
                        if (!samples.ContainsKey(fineLocation)) samples.Add(fineLocation, new List<ImageTemplate>());
                        samples[fineLocation].Add(template);

                        ImageProcessing.ProcessTemplate(template, false);
                        Localization.Instance.AddTrainingExample(template, coarseLocation, fineLocation);

                        IncrementProgress();
                    }

                    SetStatus("Training Classifier");
                    Localization.Instance.Train();

                    float numBadFrames = 0, numFrames = 0;
                    Dictionary<string, float> coarseClassCorrect = new Dictionary<string, float>();
                    Dictionary<string, float> coarseClassCount = new Dictionary<string, float>();
                    Dictionary<string, float> coarseClassCorrectGoodFrames = new Dictionary<string, float>();
                    Dictionary<string, float> coarseClassCountGoodFrames = new Dictionary<string, float>();
                    Dictionary<string, float> fineClassCorrect = new Dictionary<string, float>();
                    Dictionary<string, float> fineClassCount = new Dictionary<string, float>();
                    Dictionary<string, float> fineClassCorrectGoodFrames = new Dictionary<string, float>();
                    Dictionary<string, float> fineClassCountGoodFrames = new Dictionary<string, float>();
                    string dir = Path.Combine(root, pid, logSubfolder);
                    List<string> jsonFiles = new List<string>(Directory.GetFiles(dir, "*.log"));

                    foreach (string fileName in jsonFiles)
                    {
                        SetStatus("Loading Log File: " + Path.GetFileNameWithoutExtension(fileName));
                        SetProgress(0);
                        List<Logging.LogEvent> events = Logging.ReadLog(fileName);
                        events.Sort((a, b) => { return a.timestamp.CompareTo(b.timestamp); });
                        SetStatus("Processing Log File: " + Path.GetFileNameWithoutExtension(fileName));
                        SetProgressMax(events.Count);

                        bool isTraining = false;

                        // read the log and look for the autocapture training events, which will have labeled video frames in between
                        // identify all video indices for which we can determine a label
                        Dictionary<int, string> frameClasses = new Dictionary<int, string>();
                        HashSet<int> isTrainingSample = new HashSet<int>();
                        List<int> currIndices = new List<int>();
                        string currClass = null;
                        int mostRecentIndex = 0;
                        foreach (Logging.LogEvent e in events)
                        {
                            if (e.message == "start_autocapture_location")
                                isTraining = true;
                            else if (e.message == "stop_autocapture_location" || e.message == "External Stop Autocapture" || e.message == "touch_up_timeout")
                            {
                                isTraining = false;
                                if(currClass != null)
                                {
                                    foreach (int index in currIndices) frameClasses.Add(index, currClass);
                                }
                                currIndices.Clear();
                                currClass = null;
                            }
                            else if (e.message.StartsWith("Added template: "))
                            {
                                isTrainingSample.Add(mostRecentIndex);
                                currClass = e.message.Replace("Added template: ", "").Replace(" ", "_");
                            }
                            else if (e is Logging.VideoFrameEvent)
                            {
                                mostRecentIndex = (e as Logging.VideoFrameEvent).frameIndex;
                                if (isTraining) currIndices.Add(mostRecentIndex);
                            }
                            IncrementProgress();
                        }

                        SetStatus("Reading Video File");
                        SetProgressMax(frameClasses.Count);
                        SetProgress(0);

                        // extract video frames
                        string videoFileName = Path.ChangeExtension(fileName, "avi");
                        VideoFileReader videoReader = new VideoFileReader();
                        videoReader.Open(videoFileName);
                        Dictionary<string, int> countsForClass = new Dictionary<string, int>();
                        try
                        {
                            bool hasFramesToRead = true;
                            int frameIndex = 0;
                            while (hasFramesToRead)
                            {
                                Bitmap frame = videoReader.ReadVideoFrame();
                                if (frame == null) hasFramesToRead = false;
                                else if (frameClasses.ContainsKey(frameIndex))
                                {
                                    string className = frameClasses[frameIndex];
                                    string[] parts = className.Split('_');
                                    string coarseClass = parts[0];
                                    string fineClass = parts[1];
                                    if (!coarseClassCorrect.ContainsKey(coarseClass)) coarseClassCorrect.Add(coarseClass, 0);
                                    if (!coarseClassCorrectGoodFrames.ContainsKey(coarseClass)) coarseClassCorrectGoodFrames.Add(coarseClass, 0);
                                    if (!coarseClassCount.ContainsKey(coarseClass)) coarseClassCount.Add(coarseClass, 0);
                                    if (!coarseClassCountGoodFrames.ContainsKey(coarseClass)) coarseClassCountGoodFrames.Add(coarseClass, 0);
                                    if (!fineClassCorrect.ContainsKey(fineClass)) fineClassCorrect.Add(fineClass, 0);
                                    if (!fineClassCorrectGoodFrames.ContainsKey(fineClass)) fineClassCorrectGoodFrames.Add(fineClass, 0);
                                    if (!fineClassCount.ContainsKey(fineClass)) fineClassCount.Add(fineClass, 0);
                                    if (!fineClassCountGoodFrames.ContainsKey(fineClass)) fineClassCountGoodFrames.Add(fineClass, 0);

                                    coarseClassCount[coarseClass]++;
                                    fineClassCount[fineClass]++;

                                    Image<Gray, byte> img = new Image<Gray, byte>(frame);
                                    ImageTemplate template = new ImageTemplate(img);
                                    float focus = ImageProcessing.ImageFocus(img);
                                    float directionalVariance = ImageProcessing.EstimateMotionBlur(img);
                                    ImageProcessing.ProcessTemplate(template, false);

                                    string coarseLocation = Localization.Instance.PredictCoarseLocation(template);
                                    string fineLocation = Localization.Instance.PredictFineLocation(template, true, false, false, coarseLocation);
                                    if (coarseClass == coarseLocation) coarseClassCorrect[coarseClass]++;
                                    if (fineClass == fineLocation) fineClassCorrect[fineClass]++;

                                    if (focus < 1400 || directionalVariance > 20000)
                                    {
                                        numBadFrames++;
                                    }
                                    else
                                    {
                                        coarseClassCountGoodFrames[coarseClass]++;
                                        fineClassCountGoodFrames[fineClass]++;
                                        if (coarseClass == coarseLocation)
                                        {
                                            coarseClassCorrectGoodFrames[coarseClass]++;
                                            //coarseClassCountGoodFrames[coarseClass]++;
                                        }
                                        if (fineClass == fineLocation)
                                        {
                                            fineClassCorrectGoodFrames[fineClass]++;
                                            //fineClassCountGoodFrames[fineClass]++;
                                        }
                                    }
                                    numFrames++;

                                    IncrementProgress();
                                }
                                frameIndex++;
                            }
                        }
                        catch { }
                        videoReader.Close();
                    }

                    float accuracyCoarseTotal = 0, accuracyCoarseGoodFrames = 0;
                    foreach(string coarseClass in coarseClassCorrect.Keys)
                    {
                        accuracyCoarseTotal += coarseClassCorrect[coarseClass] / coarseClassCount[coarseClass];
                        accuracyCoarseGoodFrames += coarseClassCorrectGoodFrames[coarseClass] / coarseClassCountGoodFrames[coarseClass];
                    }
                    accuracyCoarseTotal /= coarseClassCorrect.Keys.Count;
                    accuracyCoarseGoodFrames /= coarseClassCorrect.Keys.Count;

                    float accuracyFineTotal = 0, accuracyFineGoodFrames = 0;
                    foreach (string fineClass in fineClassCorrect.Keys)
                    {
                        accuracyFineTotal += fineClassCorrect[fineClass] / fineClassCount[fineClass];
                        accuracyFineGoodFrames += fineClassCorrectGoodFrames[fineClass] / fineClassCountGoodFrames[fineClass];
                    }
                    accuracyFineTotal /= fineClassCorrect.Keys.Count;
                    accuracyFineGoodFrames /= fineClassCorrect.Keys.Count;

                    if(data == null || data.Length == 0)
                        data += Utils.CSV("pid", "# Total Frames", "# Bad Frames", "% Bad Frames", "Overall Accuracy Coarse", "Overall Accuracy Fine", "Accuracy Coarse Good Frames", "Accuracy Fine Good Frames") + Environment.NewLine;

                    data += Utils.CSV(pid, numFrames, numBadFrames, numBadFrames / numFrames, accuracyCoarseTotal, accuracyFineTotal, accuracyCoarseGoodFrames, accuracyFineGoodFrames) + Environment.NewLine;

                    File.WriteAllText("results.txt", data);
                    Invoke(new MethodInvoker(delegate
                    {
                        Clipboard.SetText(data);
                    }));
                }
                File.WriteAllText("results.txt", data);
                Invoke(new MethodInvoker(delegate
                {
                    Clipboard.SetText(data);
                }));
                SetProgress(0);
                SetStatus("Done");
            });
        }

        private void SaveSpeechSamples(string outputDir = null)
        {
            if (outputDir == null) outputDir = Environment.CurrentDirectory;

            SetStatus("Writing Speech Samples to File");

            // set up the speech synthesizer
            System.Speech.Synthesis.SpeechSynthesizer speech = new System.Speech.Synthesis.SpeechSynthesizer();
            speech.Rate = 2;
            speech.SelectVoice("Microsoft David Desktop");

            speech.SetOutputToWaveFile(Path.Combine(outputDir, "mainMenu.wav"));
            speech.Speak("Main menu, ");

            speech.SetOutputToWaveFile(Path.Combine(outputDir, "clock.wav"));
            speech.Speak("Clock: the time is 10:25 AM.");

            speech.SetOutputToWaveFile(Path.Combine(outputDir, "clock_menuOpened.wav"));
            speech.Speak("Clock menu opened, ");

            speech.SetOutputToWaveFile(Path.Combine(outputDir, "clock_time.wav"));
            speech.Speak("Clock: the time is 10:25 AM.");

            speech.SetOutputToWaveFile(Path.Combine(outputDir, "clock_timer.wav"));
            speech.Speak("Timer: 7 minutes and 45 seconds remaining.");

            speech.SetOutputToWaveFile(Path.Combine(outputDir, "clock_stopwatch.wav"));
            speech.Speak("Stopwatch: 9 minutes and 27 seconds elapsed.");

            speech.SetOutputToWaveFile(Path.Combine(outputDir, "clock_alarm.wav"));
            speech.Speak("Alarm: set for 8 AM.");

            speech.SetOutputToWaveFile(Path.Combine(outputDir, "dailySummary.wav"));
            speech.Speak("Daily Summary: Wednesday, August 24th, 2016.");

            speech.SetOutputToWaveFile(Path.Combine(outputDir, "dailySummary_menuOpened.wav"));
            speech.Speak("Daily Summary menu opened, ");

            speech.SetOutputToWaveFile(Path.Combine(outputDir, "dailySummary_date.wav"));
            speech.Speak("Date: Wednesday, August 24th, 2016.");

            speech.SetOutputToWaveFile(Path.Combine(outputDir, "dailySummary_weather.wav"));
            speech.Speak("Weather: partly cloudy and 81 degrees currently and the high for today was forecast as 84 degrees.");

            speech.SetOutputToWaveFile(Path.Combine(outputDir, "dailySummary_nextEvent.wav"));
            speech.Speak("Next event: dentist appointment from 2pm to 3pm starts in 1 hour and 35 minutes.");

            speech.SetOutputToWaveFile(Path.Combine(outputDir, "notifications.wav"));
            speech.Speak("Notifications: you have 1 missed phone call and 2 new messages.");

            speech.SetOutputToWaveFile(Path.Combine(outputDir, "notifications_menuOpened.wav"));
            speech.Speak("Notifications menu opened, ");

            speech.SetOutputToWaveFile(Path.Combine(outputDir, "notifications_notifications.wav"));
            speech.Speak("Notifications: you have 1 missed phone call and 2 new messages.");

            speech.SetOutputToWaveFile(Path.Combine(outputDir, "notifications_notifications.wav"));
            speech.Speak("You have 1 missed phone call and 2 new messages.");

            speech.SetOutputToWaveFile(Path.Combine(outputDir, "notifications_message1.wav"));
            speech.Speak("Missed phone call from Alice, just now.");

            speech.SetOutputToWaveFile(Path.Combine(outputDir, "notifications_message2.wav"));
            speech.Speak("Message from Bob 16 minutes ago. Okay, I will see you soon.");

            speech.SetOutputToWaveFile(Path.Combine(outputDir, "notifications_message3.wav"));
            speech.Speak("Message from Charlie 5 hours ago. What's up?");

            speech.SetOutputToWaveFile(Path.Combine(outputDir, "healthAndActivities.wav"));
            speech.Speak("Health and activities: 1.8 miles traveled today.");

            speech.SetOutputToWaveFile(Path.Combine(outputDir, "healthAndActivities_menuOpened.wav"));
            speech.Speak("Health and activities menu opened, ");

            speech.SetOutputToWaveFile(Path.Combine(outputDir, "healthAndActivities_distance.wav"));
            speech.Speak("Distance: you've traveled 1.8 miles today.");

            speech.SetOutputToWaveFile(Path.Combine(outputDir, "healthAndActivities_steps.wav"));
            speech.Speak("Steps: you've taken 2366 steps today.");

            speech.SetOutputToWaveFile(Path.Combine(outputDir, "healthAndActivities_calories.wav"));
            speech.Speak("Calories: you've burnt 497 calories today.");

            speech.SetOutputToWaveFile(Path.Combine(outputDir, "healthAndActivities_heartRate.wav"));
            speech.Speak("Heart rate: 118 bpm.");

            speech.SetOutputToWaveFile(Path.Combine(outputDir, "voiceInput.wav"));
            speech.Speak("Voice Input");

            speech.SetOutputToWaveFile(Path.Combine(outputDir, "voiceInput_selected.wav"));
            speech.Speak("What can I help you with?");

            SetStatus("Done");
        }

        bool exportCameraVideo = true, exportIRVideo = true, exportIMUVideo = true;
        private void ConvertSensorReadingsToVideo(string path, string output = null, int fps = 30)
        {
            Task.Factory.StartNew(() =>
            {
                SetStatus("Loading Log File");
                List<Logging.LogEvent> events = Logging.ReadLog(path);

                SetStatus("Loading Video File");
                string videoFileName = Path.ChangeExtension(path, "avi");
                List<Bitmap> frames = new List<Bitmap>();
                VideoFileReader videoReader = new VideoFileReader();
                if (exportCameraVideo)
                {
                    videoReader.Open(videoFileName);

                    Bitmap frame = null;
                    while (true)
                    {
                        frame = videoReader.ReadVideoFrame();
                        if (frame != null) frames.Add(frame);
                        else break;
                    }
                    videoReader.Close();
                }

                if (output == null) output = path;
                int videoWidth = 640, videoHeight = 640;
                //float sensorVideoWidthFactor = 5.0f / 2.0f;
                float sensorVideoWidthFactor = 1.0f;

                VideoFileWriter videoWriter = new VideoFileWriter();
                string videoOutputPath = Path.Combine(Path.GetDirectoryName(output), Path.GetFileNameWithoutExtension(output) + "_video" + ".mp4");
                if (exportCameraVideo) videoWriter.Open(videoOutputPath, videoWidth, videoHeight, fps, VideoCodec.MPEG4, 10 * 1024 * 1024);

                VideoFileWriter irWriter = new VideoFileWriter();
                string irOutputPath = Path.Combine(Path.GetDirectoryName(output), Path.GetFileNameWithoutExtension(output) + "_ir" + ".mp4");
                if (exportIRVideo) irWriter.Open(irOutputPath, (int)(videoWidth * sensorVideoWidthFactor), videoHeight, fps, VideoCodec.MPEG4, 10 * 1024 * 1024);

                VideoFileWriter accelWriter = new VideoFileWriter();
                string accelOutputPath = Path.Combine(Path.GetDirectoryName(output), Path.GetFileNameWithoutExtension(output) + "_accel" + ".mp4");
                if (exportIMUVideo) accelWriter.Open(accelOutputPath, (int)(videoWidth * sensorVideoWidthFactor), videoHeight, fps, VideoCodec.MPEG4, 10 * 1024 * 1024);

                VideoFileWriter gyroWriter = new VideoFileWriter();
                string gyroOutputPath = Path.Combine(Path.GetDirectoryName(output), Path.GetFileNameWithoutExtension(output) + "_gyro" + ".mp4");
                if (exportIMUVideo) gyroWriter.Open(gyroOutputPath, (int)(videoWidth * sensorVideoWidthFactor), videoHeight, fps, VideoCodec.MPEG4, 10 * 1024 * 1024);

                VideoFileWriter magWriter = new VideoFileWriter();
                string magOutputPath = Path.Combine(Path.GetDirectoryName(output), Path.GetFileNameWithoutExtension(output) + "_mag" + ".mp4");
                if (exportIMUVideo) magWriter.Open(magOutputPath, (int)(videoWidth * sensorVideoWidthFactor), videoHeight, fps, VideoCodec.MPEG4, 10 * 1024 * 1024);

                //VideoFileWriter combinedWriter = new VideoFileWriter();
                //string combinedOutputPath = Path.Combine(Path.GetDirectoryName(output), Path.GetFileNameWithoutExtension(output) + "_combined" + ".mp4");
                //if (exportCameraVideo && exportIMUVideo && exportIRVideo) combinedWriter.Open(combinedOutputPath, 3 * videoWidth, videoHeight, fps, VideoCodec.MPEG4, 3 * 10 * 1024 * 1024);

                SetStatus("Writing Video File");
                float lastFrameTime = float.MinValue;
                Bitmap lastFrame = null;
                float frameDuration = 1000.0f / fps;
                Queue<Logging.SensorReadingEvent> sensorReadings = new Queue<Logging.SensorReadingEvent>();
                float sensorReadingBuffer = 5000.0f;
                Queue<Logging.LogEvent> touchEvents = new Queue<Logging.LogEvent>();

                Font legendFont = new Font("Segoe", 20);
                Font graphLabelFont = new Font("Segoe", 20);
                Font axisFont = new Font("Segoe", 18);
                Font axisLabelFont = new Font("Segoe", 24);
                Font axisLabelSuperscriptFont = new Font("Segoe", 18);

                Pen irLeftPen = new Pen(Color.FromArgb(75, 172, 198));
                irLeftPen.Width = 3;
                irLeftPen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                Pen irRightPen = new Pen(Color.FromArgb(247, 150, 70));
                irRightPen.Width = 3;
                irRightPen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                Pen irThresholdPen = new Pen(Color.Red);
                irThresholdPen.Width = 2;
                irThresholdPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                irThresholdPen.DashCap = System.Drawing.Drawing2D.DashCap.Round;
                Pen irTouchEventPen = new Pen(Color.Black);
                irTouchEventPen.Width = 2;
                irTouchEventPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                Brush irTouchDownBrush = new SolidBrush(Color.FromArgb(235, 235, 235));

                int yAxisLeft = 10;
                int yAxisRight = 50;
                int graphMinX = yAxisRight + 10, graphMaxX = (int)(videoWidth * sensorVideoWidthFactor) - 10;
                int graphMinY = 10, graphMaxY = videoHeight - 80;

                int legendMargin = 10;
                int legendLineHeight = legendFont.Height;
                int legendColorWidth = 30;
                int legendLabelGap = 10;
                int legendColorOffsetY = legendLineHeight / 2;
                int irLegendWidth = TextRenderer.MeasureText("IR Right", legendFont).Width + legendMargin + legendColorWidth + legendLabelGap + legendMargin;
                int irLegendHeight = 2 * legendLineHeight + 2 * legendMargin;
                int irLegendX = graphMaxX - irLegendWidth - legendMargin, irLegendY = graphMaxY - irLegendHeight - legendMargin;

                int xAxisLeft = graphMinX;
                int xAxisRight = graphMaxX;
                int xAxisTop = graphMaxY + 5;
                int xAxisBottom = videoHeight - 10;

                int yAxisTop = graphMinY;
                int yAxisBottom = graphMaxY;

                int imuLegendWidth = TextRenderer.MeasureText("X", legendFont).Width + legendMargin + legendColorWidth + legendLabelGap + legendMargin, imuLegendHeight = 3 * legendLineHeight + 2 * legendMargin;
                int imuLegendX = graphMaxX - imuLegendWidth - legendMargin, imuLegendY = graphMaxY - imuLegendHeight - legendMargin;

                Pen imuXPen = new Pen(Color.FromArgb(192, 80, 77));
                imuXPen.Width = 3;
                imuXPen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                Pen imuYPen = new Pen(Color.FromArgb(155, 187, 89));
                imuYPen.Width = 3;
                imuYPen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                Pen imuZPen = new Pen(Color.FromArgb(79, 129, 189));
                imuZPen.Width = 3;
                imuZPen.EndCap = System.Drawing.Drawing2D.LineCap.Round;

                //float accelerometerMax = 0, gyroscopeMax = 0, magnetometerMax = 0;
                //if(exportIMUVideo)
                //    foreach(Logging.LogEvent e in events)
                //        if(e is Logging.SensorReadingEvent)
                //        {
                //            Sensors.Reading reading = (e as Logging.SensorReadingEvent).reading;
                //            if (Math.Abs(reading.Accelerometer1.X) > accelerometerMax) accelerometerMax = Math.Abs(reading.Accelerometer1.X);
                //            if (Math.Abs(reading.Accelerometer1.Y) > accelerometerMax) accelerometerMax = Math.Abs(reading.Accelerometer1.Y);
                //            if (Math.Abs(reading.Accelerometer1.Z) > accelerometerMax) accelerometerMax = Math.Abs(reading.Accelerometer1.Z);
                //            if (Math.Abs(reading.Gyroscope1.X) > gyroscopeMax) gyroscopeMax = Math.Abs(reading.Gyroscope1.X);
                //            if (Math.Abs(reading.Gyroscope1.Y) > gyroscopeMax) gyroscopeMax = Math.Abs(reading.Gyroscope1.Y);
                //            if (Math.Abs(reading.Gyroscope1.Z) > gyroscopeMax) gyroscopeMax = Math.Abs(reading.Gyroscope1.Z);
                //            if (Math.Abs(reading.Magnetometer1.X) > magnetometerMax) magnetometerMax = Math.Abs(reading.Magnetometer1.X);
                //            if (Math.Abs(reading.Magnetometer1.Y) > magnetometerMax) magnetometerMax = Math.Abs(reading.Magnetometer1.Y);
                //            if (Math.Abs(reading.Magnetometer1.Z) > magnetometerMax) magnetometerMax = Math.Abs(reading.Magnetometer1.Z);
                //        }
                float accelerometerMax = 40, gyroscopeMax = 360, magnetometerMax = 8;

                int numVideos = (exportCameraVideo ? 1 : 0) + (exportIRVideo ? 1 : 0) + (exportIMUVideo ? 3 : 0);
                TouchCamCustomControls.VideoPlaybackForm videoWindow = new TouchCamCustomControls.VideoPlaybackForm(numVideos, 240, (int)(240 * sensorVideoWidthFactor));
                videoWindow.Show();

                try
                {
                    SetProgressMax(events.Count);
                    bool touchDown = false;
                    foreach (Logging.LogEvent e in events)
                    {
                        if (e is Logging.VideoFrameEvent && exportCameraVideo)
                        {
                            int index = (e as Logging.VideoFrameEvent).frameIndex;
                            if (index < frames.Count) lastFrame = frames[index];
                        }
                        else if (e is Logging.SensorReadingEvent && (exportIRVideo || exportIMUVideo))
                        {
                            sensorReadings.Enqueue(e as Logging.SensorReadingEvent);
                            while (e.timestamp - sensorReadings.Peek().timestamp > sensorReadingBuffer) sensorReadings.Dequeue();
                        }
                        else if (e.message.Contains("touch"))
                        {
                            if (e.message != "touch_up" && !(touchDown && e.message == "touch_down")) touchEvents.Enqueue(e);

                            if (e.message == "touch_down") touchDown = true;
                            else if (e.message == "touch_up_timeout") touchDown = false;

                            while (touchEvents.Count > 0 && e.timestamp - touchEvents.Peek().timestamp > sensorReadingBuffer) touchEvents.Dequeue();
                        }

                        if (e.timestamp - lastFrameTime >= frameDuration)
                        {
                            //Bitmap combinedFrame = new Bitmap(3 * videoWidth, videoHeight);
                            //Graphics combinedGraphics = Graphics.FromImage(combinedFrame);
                            // write video frame
                            if (exportCameraVideo && lastFrame != null)
                            {
                                videoWriter.WriteVideoFrame(lastFrame);
                                videoWindow.SetFrame(lastFrame, 0);
                                //combinedGraphics.DrawImage(lastFrame, 0, 0, videoWidth, videoHeight);
                                Application.DoEvents();
                            }

                            // write IR visualization frame
                            if (exportIRVideo)
                            {
                                Bitmap irFrame = new Bitmap((int)(videoWidth * sensorVideoWidthFactor), videoHeight);
                                Graphics g = Graphics.FromImage(irFrame);
                                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                                g.Clear(Color.White);

                                // draw background to indicate touch down/up
                                bool tempTouchDown = false;
                                float touchStartX = -1;
                                foreach (Logging.LogEvent touchEvent in touchEvents)
                                {
                                    if (touchEvent.message == "touch_down")
                                    {
                                        tempTouchDown = true;
                                        touchStartX = (sensorReadingBuffer - (e.timestamp - touchEvent.timestamp)) / sensorReadingBuffer * (graphMaxX - graphMinX) + graphMinX;
                                    }
                                    else
                                    {
                                        if (!tempTouchDown) touchStartX = graphMinX;
                                        touchStartX = Math.Max(graphMinX, touchStartX);
                                        tempTouchDown = false;
                                        float touchEndX = (sensorReadingBuffer - (e.timestamp - touchEvent.timestamp)) / sensorReadingBuffer * (graphMaxX - graphMinX) + graphMinX;
                                        g.FillRectangle(irTouchDownBrush, touchStartX, graphMinY, touchEndX - touchStartX, graphMaxY - graphMinY);
                                    }
                                }
                                if (tempTouchDown)
                                {
                                    touchStartX = Math.Max(graphMinX, touchStartX);
                                    g.FillRectangle(irTouchDownBrush, touchStartX, graphMinY, graphMaxX - touchStartX, graphMaxY - graphMinY);
                                }
                                if (touchDown && touchEvents.Count == 0)
                                {
                                    g.FillRectangle(irTouchDownBrush, graphMinX, graphMinY, graphMaxX, graphMaxY);
                                }

                                // draw sensor readings
                                float lastX = float.NaN;
                                float lastYLeft = float.NaN, lastYRight = float.NaN;
                                foreach (Logging.SensorReadingEvent reading in sensorReadings)
                                {
                                    float x = (sensorReadingBuffer - (e.timestamp - reading.timestamp)) / sensorReadingBuffer * (graphMaxX - graphMinX) + graphMinX;
                                    //if (!float.IsNaN(lastX) && x - lastX < 5) continue;

                                    float yLeft = (1 - reading.reading.InfraredReflectance1) * (graphMaxY - graphMinY) + graphMinY;
                                    float yRight = (1 - reading.reading.InfraredReflectance2) * (graphMaxY - graphMinY) + graphMinY;
                                    if (!float.IsNaN(lastX))
                                    {
                                        g.DrawLine(irLeftPen, lastX, lastYLeft, x, yLeft);
                                        g.DrawLine(irRightPen, lastX, lastYRight, x, yRight);
                                    }
                                    lastX = x;
                                    lastYLeft = yLeft;
                                    lastYRight = yRight;
                                }

                                // mark touch events
                                g.SetClip(new Rectangle(graphMinX, graphMinY, graphMaxX, graphMaxY));
                                foreach (Logging.LogEvent touchEvent in touchEvents)
                                {
                                    float x = (sensorReadingBuffer - (e.timestamp - touchEvent.timestamp)) / sensorReadingBuffer * (graphMaxX - graphMinX) + graphMinX;
                                    if (x >= graphMinX)
                                    {
                                        g.DrawLine(irTouchEventPen, x, graphMinY, x, graphMaxY);
                                        bool isTouchDown = touchEvent.message.Contains("down");
                                        string eventName = "Touch " + (isTouchDown ? "Down" : "Up");
                                        g.DrawString(eventName, graphLabelFont, Brushes.Black, x + (isTouchDown ? -g.MeasureString(eventName, graphLabelFont).Width : 5), graphMaxY - (isTouchDown ? 10 : 10 + graphLabelFont.Height) - g.MeasureString(eventName, graphLabelFont).Height);
                                    }
                                }
                                g.ResetClip();

                                // draw threshold line and label
                                g.DrawLine(irThresholdPen, graphMinX, graphMinY + 0.1f * (graphMaxY - graphMinY), graphMaxX, graphMinY + 0.1f * (graphMaxY - graphMinY));
                                g.DrawString("Threshold", graphLabelFont, Brushes.Red, graphMaxX - 10 - g.MeasureString("Threshold", graphLabelFont).Width, graphMinY + 0.1f * (graphMaxY - graphMinY) - g.MeasureString("Threshold", graphLabelFont).Height);

                                // draw legend
                                g.FillRectangle(Brushes.White, irLegendX, irLegendY, irLegendWidth, irLegendHeight);
                                g.DrawRectangle(Pens.Black, irLegendX, irLegendY, irLegendWidth, irLegendHeight);
                                g.DrawLine(irLeftPen, irLegendX + legendMargin, irLegendY + legendMargin + legendColorOffsetY, irLegendX + legendMargin + legendColorWidth, irLegendY + legendMargin + legendColorOffsetY);
                                g.DrawLine(irRightPen, irLegendX + legendMargin, irLegendY + legendMargin + legendColorOffsetY + legendLineHeight, irLegendX + legendMargin + legendColorWidth, irLegendY + legendMargin + legendColorOffsetY + legendLineHeight);
                                g.DrawString("IR Left", legendFont, Brushes.Black, irLegendX + legendMargin + legendColorWidth + legendLabelGap, irLegendY + legendMargin);
                                g.DrawString("IR Right", legendFont, Brushes.Black, irLegendX + legendMargin + legendColorWidth + legendLabelGap, irLegendY + legendMargin + legendLineHeight);

                                // draw x axis
                                for (float t = (float)Math.Round(e.timestamp / 1000.0) * 1000.0f; t > e.timestamp - sensorReadingBuffer; t -= 1000)
                                {
                                    float x = (sensorReadingBuffer - (e.timestamp - t)) / sensorReadingBuffer * (graphMaxX - graphMinX) + graphMinX;
                                    if (x >= graphMinX)
                                    {
                                        g.DrawLine(Pens.Black, x, graphMaxY - 3, x, graphMaxY + 3);
                                        g.DrawString((t / 1000).ToString("0"), axisFont, Brushes.Black, x - g.MeasureString((t / 1000).ToString("0"), axisFont).Width / 2, xAxisTop);
                                    }
                                }
                                g.DrawString("Time (seconds)", axisLabelFont, Brushes.Black, xAxisLeft + (xAxisRight - xAxisLeft) / 2 - g.MeasureString("Time (seconds)", axisLabelFont).Width / 2, xAxisBottom - axisLabelFont.Height);

                                // draw y axis
                                for (int i = 0; i <= 4; i++)
                                {
                                    float y = graphMinY + i * (graphMaxY - graphMinY) / 4.0f;
                                    g.DrawLine(Pens.Black, graphMinX - 3, y, graphMinX + 3, y);
                                    g.DrawString(((4 - i) * 25).ToString("0"), axisFont, Brushes.Black, yAxisRight - g.MeasureString(((4 - i) * 25).ToString("0"), axisFont).Width, y - g.MeasureString(((4 - i) * 25).ToString("0"), axisFont).Height / 2);
                                }
                                //g.TranslateTransform(yAxisLeft, yAxisBottom);
                                //g.RotateTransform(-90);
                                //g.DrawString("IR Reading (% Max)", axisLabelFont, Brushes.Black, (yAxisBottom - yAxisTop) / 2 - g.MeasureString("IR Reading (% Max)", axisLabelFont).Width / 2, 0);
                                //g.ResetTransform();

                                irWriter.WriteVideoFrame(irFrame);
                                videoWindow.SetFrame(irFrame, exportCameraVideo ? 1 : 0);
                                //combinedGraphics.DrawImage(irFrame, videoWidth, 0, videoWidth, videoHeight);
                                Application.DoEvents();
                            }

                            // write IMU visualization frames
                            if (exportIMUVideo)
                            {
                                Bitmap accelFrame = new Bitmap((int)(videoWidth * sensorVideoWidthFactor), videoHeight);
                                Bitmap gyroFrame = new Bitmap((int)(videoWidth * sensorVideoWidthFactor), videoHeight);
                                Bitmap magFrame = new Bitmap((int)(videoWidth * sensorVideoWidthFactor), videoHeight);
                                Graphics ga = Graphics.FromImage(accelFrame);
                                Graphics gg = Graphics.FromImage(gyroFrame);
                                Graphics gm = Graphics.FromImage(magFrame);
                                ga.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                                ga.Clear(Color.White);
                                gg.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                                gg.Clear(Color.White);
                                gm.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                                gm.Clear(Color.White);

                                // draw background to indicate touch down/up
                                bool tempTouchDown = false;
                                float touchStartX = -1;
                                foreach (Logging.LogEvent touchEvent in touchEvents)
                                {
                                    if (touchEvent.message == "touch_down")
                                    {
                                        tempTouchDown = true;
                                        touchStartX = (sensorReadingBuffer - (e.timestamp - touchEvent.timestamp)) / sensorReadingBuffer * (graphMaxX - graphMinX) + graphMinX;
                                    }
                                    else
                                    {
                                        if (!tempTouchDown) touchStartX = graphMinX;
                                        touchStartX = Math.Max(graphMinX, touchStartX);
                                        tempTouchDown = false;
                                        float touchEndX = (sensorReadingBuffer - (e.timestamp - touchEvent.timestamp)) / sensorReadingBuffer * (graphMaxX - graphMinX) + graphMinX;
                                        ga.FillRectangle(irTouchDownBrush, touchStartX, graphMinY, touchEndX - touchStartX, graphMaxY - graphMinY);
                                        gg.FillRectangle(irTouchDownBrush, touchStartX, graphMinY, touchEndX - touchStartX, graphMaxY - graphMinY);
                                        gm.FillRectangle(irTouchDownBrush, touchStartX, graphMinY, touchEndX - touchStartX, graphMaxY - graphMinY);
                                    }
                                }
                                if (tempTouchDown)
                                {
                                    touchStartX = Math.Max(graphMinX, touchStartX);
                                    ga.FillRectangle(irTouchDownBrush, touchStartX, graphMinY, graphMaxX - touchStartX, graphMaxY - graphMinY);
                                    gg.FillRectangle(irTouchDownBrush, touchStartX, graphMinY, graphMaxX - touchStartX, graphMaxY - graphMinY);
                                    gm.FillRectangle(irTouchDownBrush, touchStartX, graphMinY, graphMaxX - touchStartX, graphMaxY - graphMinY);
                                }
                                if (touchDown && touchEvents.Count == 0)
                                {
                                    ga.FillRectangle(irTouchDownBrush, graphMinX, graphMinY, graphMaxX, graphMaxY);
                                    gg.FillRectangle(irTouchDownBrush, graphMinX, graphMinY, graphMaxX, graphMaxY);
                                    gm.FillRectangle(irTouchDownBrush, graphMinX, graphMinY, graphMaxX, graphMaxY);
                                }

                                // draw axis zero lines
                                ga.DrawLine(Pens.Black, graphMinX, graphMinY + (graphMaxY - graphMinY) / 2, graphMaxX, graphMinY + (graphMaxY - graphMinY) / 2);
                                gg.DrawLine(Pens.Black, graphMinX, graphMinY + (graphMaxY - graphMinY) / 2, graphMaxX, graphMinY + (graphMaxY - graphMinY) / 2);
                                gm.DrawLine(Pens.Black, graphMinX, graphMinY + (graphMaxY - graphMinY) / 2, graphMaxX, graphMinY + (graphMaxY - graphMinY) / 2);

                                // draw sensor readings
                                float lastX = float.NaN;
                                float lastYAccelX = float.NaN, lastYAccelY = float.NaN, lastYAccelZ = float.NaN;
                                float lastYGyroX = float.NaN, lastYGyroY = float.NaN, lastYGyroZ = float.NaN;
                                float lastYMagX = float.NaN, lastYMagY = float.NaN, lastYMagZ = float.NaN;
                                foreach (Logging.SensorReadingEvent reading in sensorReadings)
                                {
                                    float x = (sensorReadingBuffer - (e.timestamp - reading.timestamp)) / sensorReadingBuffer * (graphMaxX - graphMinX) + graphMinX;
                                    //if (!float.IsNaN(lastX) && x - lastX < 5) continue;

                                    float yAccelX = (-reading.reading.Accelerometer1.X) / (accelerometerMax * 2.0f) * (graphMaxY - graphMinY) + graphMinY + (graphMaxY - graphMinY) / 2.0f;
                                    float yAccelY = (-reading.reading.Accelerometer1.Y) / (accelerometerMax * 2.0f) * (graphMaxY - graphMinY) + graphMinY + (graphMaxY - graphMinY) / 2.0f;
                                    float yAccelZ = (-reading.reading.Accelerometer1.Z) / (accelerometerMax * 2.0f) * (graphMaxY - graphMinY) + graphMinY + (graphMaxY - graphMinY) / 2.0f;
                                    float yGyroX = (-reading.reading.Gyroscope1.X) / (gyroscopeMax * 2.0f) * (graphMaxY - graphMinY) + graphMinY + (graphMaxY - graphMinY) / 2.0f;
                                    float yGyroY = (-reading.reading.Gyroscope1.Y) / (gyroscopeMax * 2.0f) * (graphMaxY - graphMinY) + graphMinY + (graphMaxY - graphMinY) / 2.0f;
                                    float yGyroZ = (-reading.reading.Gyroscope1.Z) / (gyroscopeMax * 2.0f) * (graphMaxY - graphMinY) + graphMinY + (graphMaxY - graphMinY) / 2.0f;
                                    float yMagX = (-reading.reading.Magnetometer1.X) / (magnetometerMax * 2.0f) * (graphMaxY - graphMinY) + graphMinY + (graphMaxY - graphMinY) / 2.0f;
                                    float yMagY = (-reading.reading.Magnetometer1.Y) / (magnetometerMax * 2.0f) * (graphMaxY - graphMinY) + graphMinY + (graphMaxY - graphMinY) / 2.0f;
                                    float yMagZ = (-reading.reading.Magnetometer1.Z) / (magnetometerMax * 2.0f) * (graphMaxY - graphMinY) + graphMinY + (graphMaxY - graphMinY) / 2.0f;

                                    if (!float.IsNaN(lastX))
                                    {
                                        ga.DrawLine(imuXPen, lastX, lastYAccelX, x, yAccelX);
                                        ga.DrawLine(imuYPen, lastX, lastYAccelY, x, yAccelY);
                                        ga.DrawLine(imuZPen, lastX, lastYAccelZ, x, yAccelZ);
                                        gg.DrawLine(imuXPen, lastX, lastYGyroX, x, yGyroX);
                                        gg.DrawLine(imuYPen, lastX, lastYGyroY, x, yGyroY);
                                        gg.DrawLine(imuZPen, lastX, lastYGyroZ, x, yGyroZ);
                                        gm.DrawLine(imuXPen, lastX, lastYMagX, x, yMagX);
                                        gm.DrawLine(imuYPen, lastX, lastYMagY, x, yMagY);
                                        gm.DrawLine(imuZPen, lastX, lastYMagZ, x, yMagZ);
                                    }
                                    lastX = x;
                                    lastYAccelX = yAccelX;
                                    lastYAccelY = yAccelY;
                                    lastYAccelZ = yAccelZ;
                                    lastYGyroX = yGyroX;
                                    lastYGyroY = yGyroY;
                                    lastYGyroZ = yGyroZ;
                                    lastYMagX = yMagX;
                                    lastYMagY = yMagY;
                                    lastYMagZ = yMagZ;
                                }

                                // mark touch events
                                ga.SetClip(new Rectangle(graphMinX, graphMinY, graphMaxX, graphMaxY));
                                gg.SetClip(new Rectangle(graphMinX, graphMinY, graphMaxX, graphMaxY));
                                gm.SetClip(new Rectangle(graphMinX, graphMinY, graphMaxX, graphMaxY));
                                foreach (Logging.LogEvent touchEvent in touchEvents)
                                {
                                    float x = (sensorReadingBuffer - (e.timestamp - touchEvent.timestamp)) / sensorReadingBuffer * (graphMaxX - graphMinX) + graphMinX;
                                    if (x >= graphMinX)
                                    {
                                        ga.DrawLine(irTouchEventPen, x, graphMinY, x, graphMaxY);
                                        gg.DrawLine(irTouchEventPen, x, graphMinY, x, graphMaxY);
                                        gm.DrawLine(irTouchEventPen, x, graphMinY, x, graphMaxY);
                                        bool isTouchDown = touchEvent.message.Contains("down");
                                        string eventName = "Gesture " + (isTouchDown ? "Start" : "End");
                                        ga.DrawString(eventName, graphLabelFont, Brushes.Black, x + (isTouchDown ? -ga.MeasureString(eventName, graphLabelFont).Width : 5), graphMaxY - (isTouchDown ? 10 + graphLabelFont.Height : 10) - ga.MeasureString(eventName, graphLabelFont).Height);
                                        gg.DrawString(eventName, graphLabelFont, Brushes.Black, x + (isTouchDown ? -gg.MeasureString(eventName, graphLabelFont).Width : 5), graphMaxY - (isTouchDown ? 10 + graphLabelFont.Height : 10) - gg.MeasureString(eventName, graphLabelFont).Height);
                                        gm.DrawString(eventName, graphLabelFont, Brushes.Black, x + (isTouchDown ? -gm.MeasureString(eventName, graphLabelFont).Width : 5), graphMaxY - (isTouchDown ? 10 + graphLabelFont.Height : 10) - gm.MeasureString(eventName, graphLabelFont).Height);
                                        //g.DrawString(eventName, graphLabelFont, Brushes.Black, x + (isTouchDown ? -g.MeasureString(eventName, graphLabelFont).Width : 5), irGraphMaxY - (isTouchDown ? 10 : 25) - g.MeasureString(eventName, graphLabelFont).Height);
                                    }
                                }
                                ga.ResetClip();
                                gg.ResetClip();
                                gm.ResetClip();

                                // draw x axes
                                for (float t = (float)Math.Round(e.timestamp / 1000.0) * 1000.0f; t > e.timestamp - sensorReadingBuffer; t -= 1000)
                                {
                                    float x = (sensorReadingBuffer - (e.timestamp - t)) / sensorReadingBuffer * (graphMaxX - graphMinX) + graphMinX;
                                    if (x >= graphMinX)
                                    {
                                        ga.DrawLine(Pens.Black, x, (graphMinY + graphMaxY) / 2.0f - 3, x, (graphMinY + graphMaxY) / 2.0f + 3);
                                        ga.DrawString((t / 1000).ToString("0"), axisFont, Brushes.Black, x - ga.MeasureString((t / 1000).ToString("0"), axisFont).Width / 2, (graphMinY + graphMaxY) / 2.0f + 5);
                                        ga.DrawLine(Pens.Black, x, (graphMinY + graphMaxY) / 2.0f - 3, x, (graphMinY + graphMaxY) / 2.0f + 3);
                                        ga.DrawString((t / 1000).ToString("0"), axisFont, Brushes.Black, x - ga.MeasureString((t / 1000).ToString("0"), axisFont).Width / 2, (graphMinY + graphMaxY) / 2.0f + 5);
                                        ga.DrawLine(Pens.Black, x, (graphMinY + graphMaxY) / 2.0f - 3, x, (graphMinY + graphMaxY) / 2.0f + 3);
                                        ga.DrawString((t / 1000).ToString("0"), axisFont, Brushes.Black, x - ga.MeasureString((t / 1000).ToString("0"), axisFont).Width / 2, (graphMinY + graphMaxY) / 2.0f + 5);

                                        gg.DrawLine(Pens.Black, x, (graphMinY + graphMaxY) / 2.0f - 3, x, (graphMinY + graphMaxY) / 2.0f + 3);
                                        gg.DrawString((t / 1000).ToString("0"), axisFont, Brushes.Black, x - gg.MeasureString((t / 1000).ToString("0"), axisFont).Width / 2, (graphMinY + graphMaxY) / 2.0f + 5);
                                        gg.DrawLine(Pens.Black, x, (graphMinY + graphMaxY) / 2.0f - 3, x, (graphMinY + graphMaxY) / 2.0f + 3);
                                        gg.DrawString((t / 1000).ToString("0"), axisFont, Brushes.Black, x - gg.MeasureString((t / 1000).ToString("0"), axisFont).Width / 2, (graphMinY + graphMaxY) / 2.0f + 5);
                                        gg.DrawLine(Pens.Black, x, (graphMinY + graphMaxY) / 2.0f - 3, x, (graphMinY + graphMaxY) / 2.0f + 3);
                                        gg.DrawString((t / 1000).ToString("0"), axisFont, Brushes.Black, x - gg.MeasureString((t / 1000).ToString("0"), axisFont).Width / 2, (graphMinY + graphMaxY) / 2.0f + 5);

                                        gm.DrawLine(Pens.Black, x, (graphMinY + graphMaxY) / 2.0f - 3, x, (graphMinY + graphMaxY) / 2.0f + 3);
                                        gm.DrawString((t / 1000).ToString("0"), axisFont, Brushes.Black, x - gm.MeasureString((t / 1000).ToString("0"), axisFont).Width / 2, (graphMinY + graphMaxY) / 2.0f + 5);
                                        gm.DrawLine(Pens.Black, x, (graphMinY + graphMaxY) / 2.0f - 3, x, (graphMinY + graphMaxY) / 2.0f + 3);
                                        gm.DrawString((t / 1000).ToString("0"), axisFont, Brushes.Black, x - gm.MeasureString((t / 1000).ToString("0"), axisFont).Width / 2, (graphMinY + graphMaxY) / 2.0f + 5);
                                        gm.DrawLine(Pens.Black, x, (graphMinY + graphMaxY) / 2.0f - 3, x, (graphMinY + graphMaxY) / 2.0f + 3);
                                        gm.DrawString((t / 1000).ToString("0"), axisFont, Brushes.Black, x - gm.MeasureString((t / 1000).ToString("0"), axisFont).Width / 2, (graphMinY + graphMaxY) / 2.0f + 5);
                                    }
                                }
                                ga.DrawString("Time (seconds)", axisLabelFont, Brushes.Black, xAxisLeft + (xAxisRight - xAxisLeft) / 2 - ga.MeasureString("Time (seconds)", axisLabelFont).Width / 2, xAxisBottom - axisLabelFont.Height);
                                gg.DrawString("Time (seconds)", axisLabelFont, Brushes.Black, xAxisLeft + (xAxisRight - xAxisLeft) / 2 - gg.MeasureString("Time (seconds)", axisLabelFont).Width / 2, xAxisBottom - axisLabelFont.Height);
                                gm.DrawString("Time (seconds)", axisLabelFont, Brushes.Black, xAxisLeft + (xAxisRight - xAxisLeft) / 2 - gm.MeasureString("Time (seconds)", axisLabelFont).Width / 2, xAxisBottom - axisLabelFont.Height);

                                // draw y axes
                                for (int i = -4; i <= 4; i++)
                                {
                                    float y = graphMinY + -i * (graphMaxY - graphMinY) / 9.0f + (graphMaxY - graphMinY) / 2;
                                    float accel = i * accelerometerMax / 4.0f;
                                    ga.DrawLine(Pens.Black, graphMinX - 3, y, graphMinX + 3, y);
                                    ga.DrawString((accel).ToString("0"), axisFont, Brushes.Black, yAxisRight - ga.MeasureString((accel).ToString("0"), axisFont).Width, y - ga.MeasureString((accel).ToString("0"), axisFont).Height / 2);

                                    y = graphMinY + -i * (graphMaxY - graphMinY) / 9.0f + (graphMaxY - graphMinY) / 2;
                                    float gyro = i * gyroscopeMax / 4.0f;
                                    gg.DrawLine(Pens.Black, graphMinX - 3, y, graphMinX + 3, y);
                                    gg.DrawString((gyro).ToString("0"), axisFont, Brushes.Black, yAxisRight - gg.MeasureString((gyro).ToString("0"), axisFont).Width, y - gg.MeasureString((gyro).ToString("0"), axisFont).Height / 2);

                                    y = graphMinY + -i * (graphMaxY - graphMinY) / 9.0f + (graphMaxY - graphMinY) / 2;
                                    float mag = i * magnetometerMax / 4.0f;
                                    gm.DrawLine(Pens.Black, graphMinX - 3, y, graphMinX + 3, y);
                                    gm.DrawString((mag).ToString("0"), axisFont, Brushes.Black, yAxisRight - gm.MeasureString((mag).ToString("0"), axisFont).Width, y - gm.MeasureString((mag).ToString("0"), axisFont).Height / 2);
                                }
                                //ga.TranslateTransform(yAxisLeft, graphMaxY);
                                //ga.RotateTransform(-90);
                                //ga.DrawString("Accelerometer (m/s   )", axisLabelFont, Brushes.Black, (graphMaxY - graphMinY) / 2 - ga.MeasureString("Accelerometer (m/s   )", axisLabelFont).Width / 2, 0);
                                //ga.DrawString("2", axisLabelSuperscriptFont, Brushes.Black, (graphMaxY - graphMinY) / 2 - ga.MeasureString("Accelerometer (m/s   )", axisLabelFont).Width / 2 + ga.MeasureString("Accelerometer (m/s", axisLabelFont).Width - 8, -2);
                                //ga.ResetTransform();

                                //gg.TranslateTransform(yAxisLeft, graphMaxY);
                                //gg.RotateTransform(-90);
                                //gg.DrawString("Gyroscope (°/s)", axisLabelFont, Brushes.Black, (graphMaxY - graphMinY) / 2 - gg.MeasureString("Gyroscope (°/s)", axisLabelFont).Width / 2, 0);
                                //gg.ResetTransform();

                                //gm.TranslateTransform(yAxisLeft, graphMaxY);
                                //gm.RotateTransform(-90);
                                //gm.DrawString("Magnetometer (gauss)", axisLabelFont, Brushes.Black, (graphMaxY - graphMinY) / 2 - gm.MeasureString("Magnetometer (gauss)", axisLabelFont).Width / 2, 0);
                                //gm.ResetTransform();

                                // draw legend
                                ga.FillRectangle(Brushes.White, imuLegendX, imuLegendY, imuLegendWidth, imuLegendHeight);
                                ga.DrawRectangle(Pens.Black, imuLegendX, imuLegendY, imuLegendWidth, imuLegendHeight);
                                ga.DrawLine(imuXPen, imuLegendX + legendMargin, imuLegendY + legendMargin + legendColorOffsetY, imuLegendX + legendMargin + legendColorWidth, imuLegendY + legendMargin + legendColorOffsetY);
                                ga.DrawLine(imuYPen, imuLegendX + legendMargin, imuLegendY + legendMargin + legendColorOffsetY + legendLineHeight, imuLegendX + legendMargin + legendColorWidth, imuLegendY + legendMargin + legendColorOffsetY + legendLineHeight);
                                ga.DrawLine(imuZPen, imuLegendX + legendMargin, imuLegendY + legendMargin + legendColorOffsetY + 2 * legendLineHeight, imuLegendX + legendMargin + legendColorWidth, imuLegendY + legendMargin + legendColorOffsetY + 2 * legendLineHeight);
                                ga.DrawString("X", legendFont, Brushes.Black, imuLegendX + legendMargin + legendColorWidth + legendLabelGap, imuLegendY + legendMargin);
                                ga.DrawString("Y", legendFont, Brushes.Black, imuLegendX + legendMargin + legendColorWidth + legendLabelGap, imuLegendY + legendMargin + legendLineHeight);
                                ga.DrawString("Z", legendFont, Brushes.Black, imuLegendX + legendMargin + legendColorWidth + legendLabelGap, imuLegendY + legendMargin + 2 * legendLineHeight);

                                gg.FillRectangle(Brushes.White, imuLegendX, imuLegendY, imuLegendWidth, imuLegendHeight);
                                gg.DrawRectangle(Pens.Black, imuLegendX, imuLegendY, imuLegendWidth, imuLegendHeight);
                                gg.DrawLine(imuXPen, imuLegendX + legendMargin, imuLegendY + legendMargin + legendColorOffsetY, imuLegendX + legendMargin + legendColorWidth, imuLegendY + legendMargin + legendColorOffsetY);
                                gg.DrawLine(imuYPen, imuLegendX + legendMargin, imuLegendY + legendMargin + legendColorOffsetY + legendLineHeight, imuLegendX + legendMargin + legendColorWidth, imuLegendY + legendMargin + legendColorOffsetY + legendLineHeight);
                                gg.DrawLine(imuZPen, imuLegendX + legendMargin, imuLegendY + legendMargin + legendColorOffsetY + 2 * legendLineHeight, imuLegendX + legendMargin + legendColorWidth, imuLegendY + legendMargin + legendColorOffsetY + 2 * legendLineHeight);
                                gg.DrawString("X", legendFont, Brushes.Black, imuLegendX + legendMargin + legendColorWidth + legendLabelGap, imuLegendY + legendMargin);
                                gg.DrawString("Y", legendFont, Brushes.Black, imuLegendX + legendMargin + legendColorWidth + legendLabelGap, imuLegendY + legendMargin + legendLineHeight);
                                gg.DrawString("Z", legendFont, Brushes.Black, imuLegendX + legendMargin + legendColorWidth + legendLabelGap, imuLegendY + legendMargin + 2 * legendLineHeight);

                                gm.FillRectangle(Brushes.White, imuLegendX, imuLegendY, imuLegendWidth, imuLegendHeight);
                                gm.DrawRectangle(Pens.Black, imuLegendX, imuLegendY, imuLegendWidth, imuLegendHeight);
                                gm.DrawLine(imuXPen, imuLegendX + legendMargin, imuLegendY + legendMargin + legendColorOffsetY, imuLegendX + legendMargin + legendColorWidth, imuLegendY + legendMargin + legendColorOffsetY);
                                gm.DrawLine(imuYPen, imuLegendX + legendMargin, imuLegendY + legendMargin + legendColorOffsetY + legendLineHeight, imuLegendX + legendMargin + legendColorWidth, imuLegendY + legendMargin + legendColorOffsetY + legendLineHeight);
                                gm.DrawLine(imuZPen, imuLegendX + legendMargin, imuLegendY + legendMargin + legendColorOffsetY + 2 * legendLineHeight, imuLegendX + legendMargin + legendColorWidth, imuLegendY + legendMargin + legendColorOffsetY + 2 * legendLineHeight);
                                gm.DrawString("X", legendFont, Brushes.Black, imuLegendX + legendMargin + legendColorWidth + legendLabelGap, imuLegendY + legendMargin);
                                gm.DrawString("Y", legendFont, Brushes.Black, imuLegendX + legendMargin + legendColorWidth + legendLabelGap, imuLegendY + legendMargin + legendLineHeight);
                                gm.DrawString("Z", legendFont, Brushes.Black, imuLegendX + legendMargin + legendColorWidth + legendLabelGap, imuLegendY + legendMargin + 2 * legendLineHeight);

                                accelWriter.WriteVideoFrame(accelFrame);
                                gyroWriter.WriteVideoFrame(gyroFrame);
                                magWriter.WriteVideoFrame(magFrame);
                                videoWindow.SetFrame(accelFrame, exportCameraVideo ? (exportIRVideo ? 2 : 1) : (exportIRVideo ? 1 : 0));
                                videoWindow.SetFrame(gyroFrame, exportCameraVideo ? (exportIRVideo ? 3 : 2) : (exportIRVideo ? 2 : 1));
                                videoWindow.SetFrame(magFrame, exportCameraVideo ? (exportIRVideo ? 4 : 3) : (exportIRVideo ? 3 : 2));
                                //combinedGraphics.DrawImage(imuFrame, 2 * videoWidth, 0, videoWidth, videoHeight);
                                Application.DoEvents();
                            }

                            //if (exportCameraVideo && exportIRVideo && exportIMUVideo)
                            //    combinedWriter.WriteVideoFrame(combinedFrame);

                            // update frame timestamp
                            if (lastFrameTime < 0) lastFrameTime = e.timestamp;
                            else lastFrameTime += frameDuration;
                        }

                        IncrementProgress();
                    }
                }
                finally
                {
                    //if (exportCameraVideo && exportIRVideo && exportIMUVideo) combinedWriter.Close();
                    if (exportIMUVideo)
                    {
                        accelWriter.Close();
                        gyroWriter.Close();
                        magWriter.Close();
                    }
                    if (exportIRVideo) irWriter.Close();
                    if (exportCameraVideo) videoWriter.Close();
                }

                SetStatus("Done");
                SetProgress(0);
            });
        }

        private void ComputeImageStatistics(string dir)
        {
            string clipboardText = "";
            Task.Factory.StartNew(() =>
            {
                List<string> files = new List<string>(Directory.GetFiles(dir, "*.png"));
                SetProgress(0);
                SetStatus("Loading Training Files");
                SetProgressMax(files.Count);

                Worker worker = Worker.Default;

                foreach (string filename in files)
                {
                    Match match = Regex.Match(Path.GetFileNameWithoutExtension(filename), @"([a-zA-Z]+)_([a-zA-Z]+)_(\d+)");
                    string coarseLocation = match.Groups[1].Value;
                    string fineLocation = match.Groups[2].Value;
                    int index = int.Parse(match.Groups[3].Value);

                    Bitmap bmp = (Bitmap)Bitmap.FromFile(filename);
                    VideoFrame frame = new VideoFrame() { Image = new Image<Gray, byte>(bmp) };
                    frame.ImageGPU = worker.Malloc<byte>(640 * 640);
                    frame.ImageGPU.Scatter(frame.Image.Bytes);
                    ImageTemplate template = new ImageTemplate(frame);

                    float focus = ImageProcessing.ImageFocus(template);
                    float variance = ImageProcessing.ImageVariance(template);
                    Debug.WriteLine(coarseLocation + "_" + fineLocation + "_" + index + ": focus = " + focus + ", variance = " + variance);

                    IncrementProgress();
                }
            });
        }

        private void ExtractVideoFrames(string rootDir, string[] pids)
        {
            Task.Factory.StartNew(() =>
            {
                SetProgress(0);
                SetProgressMax(pids.Length * 100);
                foreach (string pid in pids)
                {
                    string dir = Path.Combine(rootDir, pid);
                    string outputDir = Path.Combine(dir, "LocationVideoFrames");
                    if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);

                    List<string> jsonFiles = new List<string>(Directory.GetFiles(dir, "*_Location*.json"));

                    SetStatus(pid + ": Loading and Processing Files");

                    foreach (string fileName in jsonFiles)
                    {
                        List<Logging.LogEvent> events = Logging.ReadLog(fileName);
                        IncrementProgress((int)(25.0f / jsonFiles.Count));

                        //// store frame indices that correspond to touch-down event
                        //HashSet<int> touchDownFrameIndices = new HashSet<int>();
                        //bool touchDown = false;
                        //int currVideoFrame = 0;
                        //foreach (Logging.LogEvent e in events)
                        //{
                        //    if (e is Logging.SensorReadingEvent)
                        //    {
                        //        touchDown = TouchSegmentation.UpdateWithReading(((Logging.SensorReadingEvent)e).reading);
                        //        if (touchDown && ! touchDownFrameIndices.Contains(currVideoFrame)) touchDownFrameIndices.Add(currVideoFrame);
                        //    }
                        //    else if (e is Logging.VideoFrameEvent)
                        //    {
                        //        currVideoFrame = ((Logging.VideoFrameEvent)e).frameIndex;
                        //    }
                        //}

                        //// read the video file, but only store frames that occurred during a touch-down event
                        //string videoFileName = Path.ChangeExtension(fileName, "avi");
                        //VideoFileReader videoReader = new VideoFileReader();
                        //videoReader.Open(videoFileName);
                        //Dictionary<int, Bitmap> frames = new Dictionary<int, Bitmap>();
                        //videoReader.ReadVideoFrame();
                        ////for (int i = 0; i < videoReader.FrameCount; i++) frames.Add(videoReader.ReadVideoFrame());
                        //try
                        //{
                        //    bool hasFramesToRead = true;
                        //    int frameIndex = 0;
                        //    while (hasFramesToRead)
                        //    {
                        //        Bitmap frame = videoReader.ReadVideoFrame();
                        //        if (frame == null) hasFramesToRead = false;
                        //        else if(touchDownFrameIndices.Contains(frameIndex)) frames.Add(frameIndex, frame);
                        //        frameIndex++;
                        //    }
                        //}
                        //catch { }
                        //videoReader.Close();
                        //IncrementProgress((int)(25.0f / jsonFiles.Count));

                        //bool touchDown = false, savingLocationFrames = false;
                        //int touchDownStart = -1;
                        //int currVideoFrame = 0;
                        //string currLocation = "none";
                        //int currLocationIndex = 0;
                        //int locationFrameIndex = 0;
                        ////TouchSegmentation.Reset();
                        //string videoFileName = Path.ChangeExtension(fileName, "avi");
                        //VideoFileReader videoReader = new VideoFileReader();
                        //videoReader.Open(videoFileName);
                        //List<Bitmap> touchEventVideoFrames = new List<Bitmap>();
                        //foreach(Logging.LogEvent e in events)
                        //{
                        //    // track touch down/up events
                        //    if (e is Logging.SensorReadingEvent)
                        //    {
                        //        // check if currently touching down
                        //        bool newTouchDown = TouchSegmentation.UpdateWithReading(((Logging.SensorReadingEvent)e).reading);

                        //        // if just touched down, mark the current video frame
                        //        if (newTouchDown && !touchDown)
                        //        {
                        //            touchDownStart = currVideoFrame;
                        //            touchEventVideoFrames.Clear();
                        //        }

                        //        // if we're saving video frames and this is the end of the touch event, save all of the frames
                        //        else if (!newTouchDown && savingLocationFrames)
                        //        {
                        //            // save all frames
                        //            //for (int frameIndex = touchDownStart; frameIndex < currVideoFrame; frameIndex++)
                        //            for(int i = 0; i < touchEventVideoFrames.Count; i++)
                        //            {
                        //                Bitmap frame = touchEventVideoFrames[i];
                        //                frame.Save(Path.Combine(outputDir, currLocation + "_" + currLocationIndex + "_" + (i - (locationFrameIndex - touchDownStart)).ToString("+#;-#;0") + ".png"));
                        //                frame.Dispose();
                        //            }

                        //            touchEventVideoFrames.Clear();
                        //            GC.GetTotalMemory(true);
                        //            savingLocationFrames = false;
                        //        }

                        //        touchDown = newTouchDown;
                        //    }

                        //    // track current video frame
                        //    else if (e is Logging.VideoFrameEvent)
                        //    {
                        //        currVideoFrame = ((Logging.VideoFrameEvent)e).frameIndex;
                        //        if (touchDown) touchEventVideoFrames.Add(videoReader.ReadVideoFrame());
                        //    }

                        //    // look for location events, extract all video frames from touch-down to touch-up
                        //    else if (e is Logging.LocationEvent)
                        //    {
                        //        string locationString = e.message;
                        //        string locationName = locationString.TrimNumbers();
                        //        int locationIndex = int.Parse(locationString.Substring(locationName.Length));
                        //        currLocation = locationName;
                        //        currLocationIndex = locationIndex;
                        //        locationFrameIndex = currVideoFrame;
                        //        savingLocationFrames = true;

                        //        if(!touchDown)
                        //        {
                        //            Console.WriteLine("Error: not touching down");
                        //        }
                        //    }
                        //}

                        int queueSize = 46; // half a second (90 fps), plus the current frame
                        string videoFileName = Path.ChangeExtension(fileName, "avi");
                        VideoFileReader videoReader = new VideoFileReader();
                        videoReader.Open(videoFileName);
                        Queue<Bitmap> frames = new Queue<Bitmap>();

                        float logTotalProgress = 50.0f / jsonFiles.Count;
                        float incrementAmount = logTotalProgress / events.Count;
                        float progressTracker = 0;
                        bool savingAdditionalFrames = false;
                        int additionalFrameCount = 0;
                        string currLocationName = "none";
                        int currLocationIndex = -1;
                        foreach (Logging.LogEvent e in events)
                        {
                            // update the video frame queue whenever a new frame was logged
                            if(e is Logging.VideoFrameEvent)
                            {
                                var vfe = e as Logging.VideoFrameEvent;
                                Bitmap frame = videoReader.ReadVideoFrame();
                                if (frame == null) break;
                                frames.Enqueue(frame);
                                if(savingAdditionalFrames && additionalFrameCount < queueSize)
                                {
                                    additionalFrameCount++;
                                    frame.Save(Path.Combine(outputDir, currLocationName + "_" + currLocationIndex + "_" + additionalFrameCount.ToString("+#;-#;0") + ".png"));
                                    if (additionalFrameCount >= queueSize) savingAdditionalFrames = false;
                                }
                                while (frames.Count > queueSize)
                                {
                                    Bitmap disposeFrame = frames.Dequeue();
                                    disposeFrame.Dispose();
                                }
                            }

                            // save the images
                            else if (e is Logging.LocationEvent)
                            {
                                // parse the location name and index
                                string locationString = e.message;
                                string locationName = locationString.TrimNumbers();
                                int locationIndex = int.Parse(locationString.Substring(locationName.Length));

                                // save the previous frames and the current frame
                                int index = -queueSize + 1;
                                foreach(Bitmap frame in frames)
                                {
                                    frame.Save(Path.Combine(outputDir, locationName + "_" + locationIndex + "_" + index.ToString("+#;-#;0") + ".png"));
                                    index++;
                                }

                                // save the next frames
                                //for(; index < queueSize; index++)
                                //{
                                //    Bitmap frame = videoReader.ReadVideoFrame();
                                //    if(frame != null) frame.Save(Path.Combine(outputDir, locationName + "_" + locationIndex + "_" + index.ToString("+#;-#;0") + ".png"));
                                //}
                                currLocationName = locationName;
                                currLocationIndex = locationIndex;
                                savingAdditionalFrames = true;
                                additionalFrameCount = 0;
                            }

                            progressTracker += incrementAmount;
                            while(progressTracker >= 1)
                            {
                                IncrementProgress();
                                progressTracker -= 1;
                            }
                        }

                        videoReader.Close();
                        //IncrementProgress((int)(50.0f / jsonFiles.Count));

                        GC.GetTotalMemory(true);
                        IncrementProgress((int)(25.0f / jsonFiles.Count));
                    }
                }
                SetProgress(0);
                SetStatus("Done");
            });
        }

        private void ExtractVideoFrames2(string rootDir, string[] pids, string subDir)
        {
            Task.Factory.StartNew(() =>
            {
                SetProgress(0);
                SetProgressMax(pids.Length * 100);
                foreach (string pid in pids)
                {
                    string dir = Path.Combine(rootDir, pid, subDir);
                    string outputDir = Path.Combine(dir, "LocationVideoFrames");
                    if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);

                    List<string> jsonFiles = new List<string>(Directory.GetFiles(dir, "*.log"));

                    SetStatus(pid + ": Loading and Processing Files");

                    foreach (string fileName in jsonFiles)
                    {
                        List<Logging.LogEvent> events = Logging.ReadLog(fileName);
                        events.Sort((a, b) => { return a.timestamp.CompareTo(b.timestamp); });
                        IncrementProgress((int)(25.0f / jsonFiles.Count));

                        bool isTraining = false;

                        // read the log and look for the autocapture training events, which will have labeled video frames in between
                        // identify all video indices for which we can determine a label
                        Dictionary<int, string> frameClasses = new Dictionary<int, string>();
                        HashSet<int> isTrainingSample = new HashSet<int>();
                        List<int> currIndices = new List<int>();
                        string currClass = null;
                        int mostRecentIndex = 0;
                        foreach(Logging.LogEvent e in events)
                        {
                            if (e.message == "start_autocapture_location")
                                isTraining = true;
                            else if (e.message == "stop_autocapture_location" || e.message == "External Stop Autocapture" || e.message == "touch_up_timeout")
                            {
                                isTraining = false;
                                if(currClass != null)
                                    foreach (int index in currIndices) frameClasses.Add(index, currClass);
                                currIndices.Clear();
                                currClass = null;
                            }
                            else if (e.message.StartsWith("Added template: "))
                            {
                                currClass = e.message.Replace("Added template: ", "").Replace(" ", "_");
                                isTrainingSample.Add(mostRecentIndex);
                            }
                            else if(e is Logging.VideoFrameEvent)
                            {
                                mostRecentIndex = (e as Logging.VideoFrameEvent).frameIndex;
                                if(isTraining) currIndices.Add(mostRecentIndex);
                            }
                        }
                        IncrementProgress((int)(25.0f / jsonFiles.Count));

                        // extract video frames
                        string videoFileName = Path.ChangeExtension(fileName, "avi");
                        VideoFileReader videoReader = new VideoFileReader();
                        videoReader.Open(videoFileName);
                        Dictionary<string, int> countsForClass = new Dictionary<string, int>();
                        try
                        {
                            bool hasFramesToRead = true;
                            int frameIndex = 0;
                            while (hasFramesToRead)
                            {
                                Bitmap frame = videoReader.ReadVideoFrame();
                                if (frame == null) hasFramesToRead = false;
                                else if (frameClasses.ContainsKey(frameIndex))
                                {
                                    if (!countsForClass.ContainsKey(frameClasses[frameIndex])) countsForClass.Add(frameClasses[frameIndex], 0);
                                    int templateIndex = countsForClass[frameClasses[frameIndex]]++;
                                    frame.Save(Path.Combine(outputDir, frameClasses[frameIndex] + "_" + templateIndex + (isTrainingSample.Contains(frameIndex) ? "_train" : "") + ".png"));
                                }
                                frameIndex++;
                            }
                        }
                        catch { }
                        videoReader.Close();
                        IncrementProgress((int)(50.0f / jsonFiles.Count));
                    }
                }
                SetProgress(0);
                SetStatus("Done");
            });
        }

        private void ExtractGestures(string[] files, string[] outputDirectories)
        {
            Task.Factory.StartNew(() =>
            {
                for(int fileIndex = 0; fileIndex < Math.Min(files.Length, outputDirectories.Length); fileIndex++)
                {
                    string file = files[fileIndex];
                    SetStatus("Loading \"" + file + "\"");
                    SetProgress(0);

                    List<Logging.LogEvent> events = Logging.ReadLog(file);
                    //events.Sort((a, b) => { return a.timestamp.CompareTo(b.timestamp); });

                    SetStatus("Processing \"" + file + "\"");
                    SetProgressMax(events.Count);

                    bool prepareToRecordGesture = false, recordingGesture = false;

                    Gesture currGesture = null;
                    List<Sensors.Reading> gestureSensorReadings = new List<Sensors.Reading>();
                    int gestureIndex = 0, numGestureClasses = 5;
                    string prevGestureFile = null, prevGestureClass = "";
                    foreach (Logging.LogEvent e in events)
                    {
                        if (e.message == "start_recording_gesture")
                            prepareToRecordGesture = true;
                        else if (e.message == "stop_recording_gesture" || e.message == "touch_up_timeout")
                        {
                            if (recordingGesture)
                            {
                                prepareToRecordGesture = false;
                                recordingGesture = false;

                                if (gestureSensorReadings.Count > 60)
                                {
                                    Gesture gesture = new Gesture(gestureSensorReadings.ToArray());
                                    GestureRecognition.PreprocessGesture(gesture);
                                    currGesture = gesture;
                                }
                            }
                        }
                        else if (e.message == "touch_down" && prepareToRecordGesture)
                        {
                            prepareToRecordGesture = false;
                            recordingGesture = true;
                            gestureSensorReadings.Clear();
                            currGesture = null;
                        }
                        else if (e.message.StartsWith("Add gesture:"))
                        {
                            if(currGesture != null)
                            {
                                string className = e.message.Replace("Add gesture: ", "");
                                if(prevGestureFile != null && className == prevGestureClass && gestureIndex % numGestureClasses != 0)
                                {
                                    File.Delete(prevGestureFile);
                                    gestureIndex--;
                                }

                                currGesture.ClassName = className;
                                string json = JsonConvert.SerializeObject(currGesture);
                                int index = 0;
                                string gestureFile = Path.Combine(outputDirectories[fileIndex], className + "_" + index + ".gest");
                                while (File.Exists(gestureFile)) gestureFile = Path.Combine(outputDirectories[fileIndex], className + "_" + (++index) + ".gest");
                                File.WriteAllText(gestureFile, json);
                                prevGestureFile = gestureFile;
                                prevGestureClass = className;
                                gestureIndex++;
                                currGesture = null;
                                gestureSensorReadings.Clear();
                            }
                        }
                        else if(e is Logging.SensorReadingEvent)
                        {
                            if(recordingGesture)
                            {
                                Sensors.Reading reading = (e as Logging.SensorReadingEvent).reading;
                                gestureSensorReadings.Add(reading);
                            }
                        }
                        IncrementProgress();
                    }
                }
                SetProgress(0);
                SetStatus("Done");
            });
        }

        private void CrossValidate(string rootDir, string[] pids, string subDir, string filenamePattern = null, string namePrefix = "")
        {
            string clipboardText = "";
            Task.Factory.StartNew(() =>
            {
                foreach (string pid in pids)
                {
                    string dir = Path.Combine(rootDir, pid, subDir);

                    List<string> files = new List<string>(Directory.GetFiles(dir, filenamePattern == null ? "*.png": filenamePattern));

                    var samples = new Dictionary<string, List<ImageTemplate>>();

                    SetProgress(0);
                    SetStatus(pid + ": Loading and Preprocessing Files");
                    SetProgressMax(files.Count);

                    Worker worker = Worker.Default;

                    for (int fileIndex = 0; fileIndex < files.Count; fileIndex++)
                    {
                        string filename = files[fileIndex];
                        string coarseLocation, fineLocation;
                        int index;
                        Match match = Regex.Match(Path.GetFileNameWithoutExtension(filename), namePrefix + @"([a-zA-Z]+)_([a-zA-Z]+)_(\d+)");
                        if (match.Success)
                        {
                            coarseLocation = match.Groups[1].Value;
                            fineLocation = match.Groups[2].Value;
                            index = int.Parse(match.Groups[3].Value);
                        }
                        else
                        {
                            match = Regex.Match(Path.GetFileNameWithoutExtension(filename), namePrefix + @"([a-zA-Z]+)(\d+)");
                            coarseLocation = match.Groups[1].Value;
                            fineLocation = coarseLocation;
                            index = int.Parse(match.Groups[2].Value);
                        }
                        
                        Bitmap bmp = (Bitmap)Bitmap.FromFile(filename);
                        VideoFrame frame = new VideoFrame() { Image = new Image<Gray, byte>(bmp) };
                        frame.ImageGPU = worker.Malloc<byte>(640 * 640);
                        frame.ImageGPU.Scatter(frame.Image.Bytes);
                        ImageTemplate template = new ImageTemplate(frame);
                        template["frameIndex"] = index;
                        template["Path"] = Path.GetFileNameWithoutExtension(filename);
                        template["CoarseLocation"] = coarseLocation;
                        template["FineLocation"] = fineLocation;
                        if (!samples.ContainsKey(fineLocation)) samples.Add(fineLocation, new List<ImageTemplate>());
                        samples[fineLocation].Add(template);

                        ImageProcessing.ProcessTemplate(template, false);

                        IncrementProgress();
                    }

                    int numTemplates = 0;
                    foreach (string className in samples.Keys) numTemplates += samples[className].Count;

                    float numMatches = 0;
                    float numCorrectCoarse = 0, numCorrectFine = 0, numProcessedTemplates = 0;

                    List<string> results = new List<string>();

                    SetStatus(pid + ": Matching Templates");
                    SetProgress(0);

                    //float progress = 0;
                    int n = 0; foreach (string className in samples.Keys) n = Math.Max(n, samples[className].Count);
                    SetProgressMax(n);
                    for (int queryIndex = 0; queryIndex < n; queryIndex++)
                    {
                        Localization.Instance.Reset();
                        foreach (string fineLocation in samples.Keys)
                            for (int i = 0; i < samples[fineLocation].Count; i++)
                            {
                                if (i == queryIndex) continue;
                                ImageTemplate template = samples[fineLocation][i];
                                Localization.Instance.AddTrainingExample(template, (string)template["CoarseLocation"], (string)template["FineLocation"]);
                            }
                        Localization.Instance.Train();

                        foreach (string className in samples.Keys)
                        {
                            //if (!coarseForFine.ContainsKey(className)) continue;
                            if (queryIndex >= samples[className].Count) continue;
                            ImageTemplate query = samples[className][queryIndex];

                            DateTime start = DateTime.Now;

                            //List<Tuple<string, float>> groupProbabilities;
                            string predictedCoarse = Localization.Instance.PredictCoarseLocation(query);
                            string predictedFine = "";
                            //predictedRegion = Localization.Instance.PredictFineLocation(query, true, true, false, predictedGroup);
                            bool foundFeatureMatch = false;
                            Dictionary<string, float> fineProbabilities = new Dictionary<string, float>();
                            predictedFine = Localization.Instance.PredictFineLocation(query, out foundFeatureMatch, out fineProbabilities, true, false, false, predictedCoarse);


                            bool coarseCorrect = predictedCoarse == (string)query["CoarseLocation"];
                            bool fineCorrect = predictedFine == className;

                            numProcessedTemplates++;
                            numCorrectCoarse += coarseCorrect ? 1 : 0;
                            numCorrectFine += fineCorrect ? 1 : 0;

                            numMatches++;

                            int index = samples[className].IndexOf(query);
                        }

                        IncrementProgress();
                    }

                    double accuracyCoarse = numCorrectCoarse / numProcessedTemplates;
                    double accuracyFine = numCorrectFine / numProcessedTemplates;

                    Debug.WriteLine("");
                    Debug.WriteLine("Accuracy (Coarse): " + (accuracyCoarse * 100) + "%");
                    Debug.WriteLine("Accuracy (Fine): " + (accuracyFine * 100) + "%");

                    SetProgress(0);
                    SetStatus(pid + "\nAccuracy (Coarse): " + (accuracyCoarse * 100) + "%\n" + "Accuracy (Fine): " + (accuracyFine * 100) + "%");

                    clipboardText += accuracyCoarse + "\t" + accuracyFine + Environment.NewLine;

                    Invoke(new MethodInvoker(delegate
                    {
                        Clipboard.SetText(clipboardText);
                    }));
                }
                File.WriteAllText("results.txt", clipboardText);
            });
        }

        private void EvaluateVideoPredictions2(string rootDir, string[] pids, string sampleDir, string testDir)
        {
            string clipboardText = "";
            Task.Factory.StartNew(() =>
            {
                foreach (string pid in pids)
                {
                    string dir = Path.Combine(rootDir, pid, sampleDir);

                    List<string> files = new List<string>(Directory.GetFiles(dir, "*.png"));

                    var samples = new Dictionary<string, List<ImageTemplate>>();

                    SetProgress(0);
                    SetStatus(pid + ": Loading and Preprocessing Files");
                    SetProgressMax(files.Count);

                    Worker worker = Worker.Default;
                    Localization.Instance.Reset();

                    Dictionary<string, float> maxFocusForClass = new Dictionary<string, float>();
                    Dictionary<string, float> maxBrightnessForClass = new Dictionary<string, float>();
                    Dictionary<string, float> maxVarianceForClass = new Dictionary<string, float>();

                    for (int fileIndex = 0; fileIndex < files.Count; fileIndex++)
                    {
                        string filename = files[fileIndex];
                        Match match = Regex.Match(Path.GetFileNameWithoutExtension(filename), @"([a-zA-Z]+)_([a-zA-Z]+)_(\d+)");
                        string coarseLocation = match.Groups[1].Value;
                        string fineLocation = match.Groups[2].Value;
                        int index = int.Parse(match.Groups[3].Value);

                        Bitmap bmp = (Bitmap)Bitmap.FromFile(filename);
                        VideoFrame frame = new VideoFrame() { Image = new Image<Gray, byte>(bmp) };
                        frame.ImageGPU = worker.Malloc<byte>(640 * 640);
                        frame.ImageGPU.Scatter(frame.Image.Bytes);
                        ImageTemplate template = new ImageTemplate(frame);
                        template["frameIndex"] = index;
                        template["Path"] = Path.GetFileNameWithoutExtension(filename);
                        template["CoarseLocation"] = coarseLocation;
                        template["FineLocation"] = fineLocation;
                        if (!samples.ContainsKey(fineLocation)) samples.Add(fineLocation, new List<ImageTemplate>());
                        samples[fineLocation].Add(template);

                        ImageProcessing.ProcessTemplate(template, false);
                        //Localization.Instance.AddTrainingExample(template, coarseLocation, fineLocation);

                        float focus = ImageProcessing.ImageFocus(template.Image);
                        float brightness = ImageProcessing.ImageBrightness(template.Image);
                        float variance = ImageProcessing.ImageVariance(template.Image);
                        if (!maxFocusForClass.ContainsKey(fineLocation)) maxFocusForClass.Add(fineLocation, 0);
                        if (!maxBrightnessForClass.ContainsKey(fineLocation)) maxBrightnessForClass.Add(fineLocation, 0);
                        if (!maxVarianceForClass.ContainsKey(fineLocation)) maxVarianceForClass.Add(fineLocation, 0);
                        if (focus > maxFocusForClass[fineLocation]) maxFocusForClass[fineLocation] = focus;
                        if (brightness > maxBrightnessForClass[fineLocation]) maxBrightnessForClass[fineLocation] = brightness;
                        if (variance > maxVarianceForClass[fineLocation]) maxVarianceForClass[fineLocation] = variance;

                        IncrementProgress();
                    }

                    int numTemplates = 0;
                    foreach(string fineLocation in samples.Keys)
                        foreach(ImageTemplate template in samples[fineLocation])
                        {
                            string coarseLocation = coarseForFine[fineLocation];
                            float focus = ImageProcessing.ImageFocus(template.Image);
                            //if (focus >= maxFocusForClass[fineLocation] / 2.0f)
                            {
                                Localization.Instance.AddTrainingExample(template, coarseLocation, fineLocation);
                                numTemplates++;
                            }
                        }
                    Localization.Instance.Train();

                    //int numTemplates = 0;
                    //foreach (string className in samples.Keys) numTemplates += samples[className].Count;

                    float numMatches = 0;
                    float numCorrectCoarse = 0, numCorrectFine = 0, numProcessedImages = 0;
                    int numErrors = 0;

                    List<string> results = new List<string>();

                    SetStatus(pid + ": Matching Test Images");
                    SetProgress(0);

                    List<string> testImages = new List<string>(Directory.GetFiles(Path.Combine(rootDir, pid, testDir), "*.png"));
                    SetProgressMax(testImages.Count);

                    DeviceMemory<byte> reusableImageGPU = worker.Malloc<byte>(640 * 640);
                    for(int testFileIndex = 0; testFileIndex < testImages.Count; testFileIndex++)
                    {
                        string testFileName = testImages[testFileIndex];
                        Match testMatch = Regex.Match(Path.GetFileNameWithoutExtension(testFileName), @"([a-zA-Z]+)_([a-zA-Z]+)_(\d+)(_train)?");
                        string testCoarseLocation = testMatch.Groups[1].Value;
                        string testFineLocation = testMatch.Groups[2].Value;
                        int testIndex = int.Parse(testMatch.Groups[3].Value);

                        if (testMatch.Groups[4].Value == "_train") continue; // skip training examples

                        Bitmap testBmp = (Bitmap)Bitmap.FromFile(testFileName);
                        VideoFrame testFrame = new VideoFrame() { Image = new Image<Gray, byte>(testBmp) };
                        testFrame.ImageGPU = reusableImageGPU;
                        testFrame.ImageGPU.Scatter(testFrame.Image.Bytes);
                        ImageTemplate query = new ImageTemplate(testFrame);
                        query["frameIndex"] = testIndex;
                        query["Path"] = Path.GetFileNameWithoutExtension(testFileName);
                        query["CoarseLocation"] = testCoarseLocation;
                        query["FineLocation"] = testFineLocation;

                        float focus = ImageProcessing.ImageFocus(query.Image);
                        float brightness = ImageProcessing.ImageBrightness(query.Image);
                        float variance = ImageProcessing.ImageVariance(query.Image);
                        if (focus < maxFocusForClass[testFineLocation] / 2.0f || brightness < maxBrightnessForClass[testFineLocation] / 2.0f || variance < maxVarianceForClass[testFineLocation] / 2.0f) continue;

                        ImageProcessing.ProcessTemplate(query, false);

                        Dictionary<string, float> coarseProbabilities = new Dictionary<string, float>();
                        string predictedCoarse = Localization.Instance.PredictCoarseLocation(query, out coarseProbabilities);
                        string predictedFine = "";
                        bool foundFeatureMatch = false;
                        Dictionary<string, float> fineProbabilities = new Dictionary<string, float>();
                        predictedFine = Localization.Instance.PredictFineLocation(query, out foundFeatureMatch, out fineProbabilities, true, false, false, predictedCoarse);


                        bool coarseCorrect = predictedCoarse == testCoarseLocation;
                        bool fineCorrect = predictedFine == testFineLocation;
                        if (!coarseCorrect || !fineCorrect)
                            numErrors++;

                        numProcessedImages++;
                        numCorrectCoarse += coarseCorrect ? 1 : 0;
                        numCorrectFine += fineCorrect ? 1 : 0;

                        numMatches++;

                        IncrementProgress();
                    }

                    double accuracyCoarse = numCorrectCoarse / numProcessedImages;
                    double accuracyFine = numCorrectFine / numProcessedImages;

                    Debug.WriteLine("");
                    Debug.WriteLine("Accuracy (Coarse): " + (accuracyCoarse * 100) + "%");
                    Debug.WriteLine("Accuracy (Fine): " + (accuracyFine * 100) + "%");
                    Debug.WriteLine(numErrors + " errors");

                    SetProgress(0);
                    SetStatus(pid + "\nAccuracy (Coarse): " + (accuracyCoarse * 100) + "%\n" + "Accuracy (Fine): " + (accuracyFine * 100) + "%");

                    clipboardText += accuracyCoarse + "\t" + accuracyFine + "\t" + numTemplates + "\t" + numProcessedImages + Environment.NewLine;

                    Invoke(new MethodInvoker(delegate
                    {
                        Clipboard.SetText(clipboardText);
                    }));
                }
                File.WriteAllText("results.txt", clipboardText);
            });
        }

        private void EvaluateVideoPredictions(string rootDir, string[] pids, string subDir)
        {
            string clipboardText = "";
            int minVideoWindow = 31, maxVideoWindow = 35;
            Task.Factory.StartNew(() =>
            {
                Worker worker = Worker.Default;
                VideoFrame tempFrame = new VideoFrame();
                tempFrame.ImageGPU = worker.Malloc<byte>(640 * 640);

                foreach (string pid in pids)
                {
                    string dir = Path.Combine(rootDir, pid, subDir);

                    List<string> files = new List<string>(Directory.GetFiles(dir, "*.png"));
                    SetProgress(0);
                    SetStatus(pid + ": Loading and Processing Image Files");
                    SetProgressMax(files.Count);

                    // load and process the training files, which end in _0
                    Dictionary<string, Dictionary<int, ImageTemplate>> trainingFiles = new Dictionary<string, Dictionary<int, ImageTemplate>>();
                    foreach (string filename in files)
                    {
                        Match match = Regex.Match(Path.GetFileNameWithoutExtension(filename), @"(.*)_(\d+)_([+-]?\d+)");
                        string location = match.Groups[1].Value;
                        int index = int.Parse(match.Groups[2].Value);
                        int offset = int.Parse(match.Groups[3].Value);

                        if (offset == 0)
                        {
                            Bitmap bmp = (Bitmap)Bitmap.FromFile(filename);
                            VideoFrame frame = new VideoFrame() { Image = new Image<Gray, byte>(bmp) };
                            frame.ImageGPU = worker.Malloc<byte>(640 * 640);
                            frame.ImageGPU.Scatter(frame.Image.Bytes);
                            ImageTemplate template = new ImageTemplate(frame);
                            template["frameIndex"] = index;
                            template["Path"] = Path.GetFileNameWithoutExtension(filename);
                            template["Region"] = location;
                            if (!trainingFiles.ContainsKey(location)) trainingFiles.Add(location, new Dictionary<int, ImageTemplate>());
                            trainingFiles[location].Add(index, template);
                            ImageProcessing.ProcessTemplate(template, false);
                        }

                        IncrementProgress();
                    }

                    int numTemplates = 0;
                    foreach (string className in trainingFiles.Keys) numTemplates += trainingFiles[className].Count;

                    for (int videoWindow = minVideoWindow; videoWindow <= maxVideoWindow; videoWindow++)
                    {
                        float numMatches = 0;
                        float numCorrectCoarse = 0, numCorrectFine = 0, numProcessedTemplates = 0;

                        List<string> results = new List<string>();

                        SetStatus(pid + ": Matching Templates (w = " + videoWindow + ")");
                        SetProgress(0);

                        //float progress = 0;
                        int n = 0; foreach (string className in trainingFiles.Keys) n = Math.Max(n, trainingFiles[className].Count);
                        SetProgressMax(n);
                        for (int queryIndex = 0; queryIndex < n; queryIndex++)
                        {
                            Localization.Instance.Reset();
                            foreach (string region in trainingFiles.Keys)
                                for (int i = 0; i < trainingFiles[region].Count; i++)
                                {
                                    if (i == queryIndex) continue;
                                    Localization.Instance.AddTrainingExample(trainingFiles[region][i], groupForRegion[region], region);
                                }
                            Localization.Instance.Train();

                            foreach (string className in trainingFiles.Keys)
                            {
                                if (!groupForRegion.ContainsKey(className)) continue;
                                if (queryIndex >= trainingFiles[className].Count) continue;
                                ImageTemplate query = trainingFiles[className][queryIndex];

                                List<Dictionary<string, float>> coarseProbabilities = new List<Dictionary<string, float>>();
                                List<Dictionary<string, float>> fineProbabilities = new List<Dictionary<string, float>>();
                                List<string> coarsePredictions = new List<string>();
                                List<float> focusWeights = new List<float>();
                                string predictedCoarseLocation = "", predictedFineLocation = "";
                                Dictionary<string, float> totalProbabilities;
                                float maxProb = 0;
                                for (int i = -videoWindow + 1; i <= 0; i++)
                                {
                                    Image<Gray, byte> img = new Image<Gray, byte>(Path.Combine(dir, className + "_" + queryIndex + "_" + i + ".png"));
                                    tempFrame.ImageGPU.Scatter(img.Bytes);
                                    tempFrame.Image = img;
                                    ImageTemplate template = new ImageTemplate(tempFrame);
                                    ImageProcessing.ProcessTemplate(template);
                                    float focus = ImageProcessing.ImageFocus(template);
                                    focusWeights.Add(focus);

                                    Dictionary<string, float> frameCoarseProbabilities = new Dictionary<string, float>();
                                    predictedCoarseLocation = Localization.Instance.PredictCoarseLocation(template, out frameCoarseProbabilities);
                                    coarseProbabilities.Add(frameCoarseProbabilities);

                                    totalProbabilities = new Dictionary<string, float>();
                                    foreach (Dictionary<string, float> probabilities in coarseProbabilities)
                                    {
                                        foreach (string key in probabilities.Keys)
                                        {
                                            if (!totalProbabilities.ContainsKey(key)) totalProbabilities[key] = 0;
                                            totalProbabilities[key] += probabilities[key] / coarseProbabilities.Count;
                                        }
                                    }

                                    maxProb = 0;
                                    foreach (string key in totalProbabilities.Keys)
                                        if (totalProbabilities[key] > maxProb)
                                        {
                                            maxProb = totalProbabilities[key];
                                            predictedCoarseLocation = key;
                                        }
                                    coarsePredictions.Add(predictedCoarseLocation);

                                    bool foundFeatureMatch = false;
                                    Dictionary<string, float> frameFineProbabilities = new Dictionary<string, float>();
                                    predictedFineLocation = Localization.Instance.PredictFineLocation(template, out foundFeatureMatch, out frameFineProbabilities, true, false, false, predictedCoarseLocation);
                                    fineProbabilities.Add(frameFineProbabilities);
                                }

                                totalProbabilities = new Dictionary<string, float>();
                                Dictionary<string, float>[] tempCoarseProbabilities = coarseProbabilities.ToArray();
                                Dictionary<string, float>[] tempFineProbabilities = fineProbabilities.ToArray();
                                string[] tempCoarsePredictions = coarsePredictions.ToArray();
                                float[] tempFocusWeights = focusWeights.ToArray();
                                //foreach (Dictionary<string, float> probabilities in gestureCoarseLocationProbabilities)
                                for (int i = 0; i < tempCoarseProbabilities.Length; i++)
                                {
                                    Dictionary<string, float> probabilities = tempCoarseProbabilities[i];
                                    float weight = Math.Max(tempFocusWeights.Length > i ? tempFocusWeights[i] : 0.01f, 0.01f);
                                    foreach (string key in probabilities.Keys)
                                    {
                                        if (!totalProbabilities.ContainsKey(key)) totalProbabilities[key] = 0;
                                        totalProbabilities[key] += weight * probabilities[key] / coarseProbabilities.Count;
                                    }
                                }

                                maxProb = 0;
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
                                            totalProbabilities[key] += weight * probabilities[key] / coarseProbabilities.Count;
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

                                bool coarseCorrect = coarseLocation == groupForRegion[className];
                                bool fineCorrect = fineLocation == className;

                                numProcessedTemplates++;
                                numCorrectCoarse += coarseCorrect ? 1 : 0;
                                numCorrectFine += fineCorrect ? 1 : 0;

                                numMatches++;
                            }

                            IncrementProgress();
                        }

                        double accuracyCoarse = numCorrectCoarse / numProcessedTemplates;
                        double accuracyFine = numCorrectFine / numProcessedTemplates;

                        Debug.WriteLine("");
                        Debug.WriteLine("Window Size: " + videoWindow);
                        Debug.WriteLine("Accuracy (Coarse): " + (accuracyCoarse * 100) + "%");
                        Debug.WriteLine("Accuracy (Fine): " + (accuracyFine * 100) + "%");

                        SetProgress(0);
                        SetStatus(pid + "\nWindow Size: " + videoWindow + "\nAccuracy (Group): " + (accuracyCoarse * 100) + "%\n" + "Accuracy (Region): " + (accuracyFine * 100) + "%");

                        clipboardText += accuracyCoarse + "\t" + accuracyFine + "\t";
                        File.WriteAllText("results.txt", clipboardText);

                        Invoke(new MethodInvoker(delegate
                        {
                            Clipboard.SetText(clipboardText);
                        }));
                    }

                    clipboardText += Environment.NewLine;
                    File.WriteAllText("results.txt", clipboardText);
                }
            });
        }

        private void ProcessData(string rootDir, string[] pids, string subDir)
        { 
            string clipboardText = "";
            List<string> results = new List<string>();
            File.WriteAllText("results.txt", "");
            bool firstImage = true;
            Task.Factory.StartNew(() =>
            {
                foreach (string pid in pids)
                {
                    string dir = Path.Combine(rootDir, pid, subDir);

                    List<string> allFiles = new List<string>(Directory.GetFiles(dir, "*.png"));
                    List<string> files = new List<string>();
                    foreach (string filename in allFiles) // filter the files
                    {
                        string location = Regex.Replace(Path.GetFileNameWithoutExtension(filename), @"\d+", "");
                        int index = int.Parse(Regex.Replace(Path.GetFileNameWithoutExtension(filename), @"[^\d]+", ""));
                        if (location.ToLower().Contains("gesture")) continue;
                        files.Add(filename);
                    }
                    int[] indices = new int[files.Count];
                    for (int i = 0; i < files.Count; i++) indices[i] = i;
                    TouchCamLibrary.Utils.Shuffle(indices);

                    var samples = new Dictionary<string, List<ImageTemplate>>();

                    SetProgress(0);
                    SetStatus(pid + ": Loading and Preprocessing Files");
                    SetProgressMax(files.Count);

                    Worker worker = Worker.Default;

                    Stopwatch watch = new Stopwatch();
                    float averageProcessingTime = 0;

                    List<float> allVariances = new List<float>();
                    //foreach (string filename in files)
                    for (int fileIndex = 0; fileIndex < files.Count; fileIndex++)
                    {
                        string filename = files[indices[fileIndex]];
                        //string filename = files[fileIndex];
                        string location = Regex.Replace(Path.GetFileNameWithoutExtension(filename), @"\d+", "");
                        int index = int.Parse(Regex.Replace(Path.GetFileNameWithoutExtension(filename), @"[^\d]+", ""));

                        Bitmap bmp = (Bitmap)Bitmap.FromFile(filename);
                        VideoFrame frame = new VideoFrame() { Image = new Image<Gray, byte>(bmp) };
                        frame.ImageGPU = worker.Malloc<byte>(640 * 640);
                        frame.ImageGPU.Scatter(frame.Image.Bytes);
                        ImageTemplate template = new ImageTemplate(frame);
                        template["frameIndex"] = index;
                        template["Path"] = Path.GetFileNameWithoutExtension(filename);
                        template["Region"] = location;
                        if (!samples.ContainsKey(location)) samples.Add(location, new List<ImageTemplate>());
                        samples[location].Add(template);

                        watch.Restart();
                        //template.Texture = AdaptiveLBP.GetInstance(frame.Image.Size).GetHistogram(frame);
                        ImageProcessing.ProcessTemplate(template, false);
                        //allVariances.AddRange((List<float>)template["variances"]);

                        if(firstImage)
                        {
                            watch.Restart();
                            ImageProcessing.ProcessTemplate(template, false);
                            firstImage = false;
                        }

                        float elapsed = (float)watch.ElapsedTicks / (float)Stopwatch.Frequency * 1000.0f;
                        averageProcessingTime += elapsed;

                        IncrementProgress();
                    }
                    averageProcessingTime /= files.Count;

                    //List<float> varBinCuts = new List<float>();
                    //allVariances.Sort();
                    //int itemsPerBin = allVariances.Count / 8;

                    //varBinCuts.Add(float.NegativeInfinity);
                    //for (int i = itemsPerBin; i < allVariances.Count; i += itemsPerBin)
                    //    varBinCuts.Add(allVariances[i]);
                    //varBinCuts.Add(float.PositiveInfinity);

                    int numTemplates = 0;
                    foreach (string className in samples.Keys) numTemplates += samples[className].Count;

                    float averageMatchTime = 0, numMatches = 0;
                    float numCorrectGroup = 0, numCorrectRegion = 0, numProcessedTemplates = 0;

                    SetStatus(pid + ": Matching Templates");
                    SetProgress(0);

                    //float progress = 0;
                    int n = 0; foreach (string className in samples.Keys) n = Math.Max(n, samples[className].Count);
                    SetProgressMax(n);
                    for (int queryIndex = 0; queryIndex < n; queryIndex++)
                    {
                        Localization.Instance.Reset();
                        foreach (string region in samples.Keys)
                            for (int i = 0; i < samples[region].Count; i++)
                            {
                                if (i == queryIndex) continue;
                                Localization.Instance.AddTrainingExample(samples[region][i], groupForRegion[region], region);
                            }
                        Localization.Instance.Train();

                        foreach (string className in samples.Keys)
                        {
                            if (!groupForRegion.ContainsKey(className)) continue;
                            if (queryIndex >= samples[className].Count) continue;
                            ImageTemplate query = samples[className][queryIndex];

                            DateTime start = DateTime.Now;
                            watch.Restart();

                            //List<Tuple<string, float>> groupProbabilities;
                            string predictedGroup = Localization.Instance.PredictCoarseLocation(query);
                            string predictedRegion = "";
                            //predictedRegion = Localization.Instance.PredictFineLocation(query, true, true, false, predictedGroup);
                            bool foundFeatureMatch = false;
                            Dictionary<string, float> fineProbabilities = new Dictionary<string, float>();
                            predictedRegion = Localization.Instance.PredictFineLocation(query, out foundFeatureMatch, out fineProbabilities, true, false, false, predictedGroup);

                            bool groupCorrect = predictedGroup == groupForRegion[className];
                            bool regionCorrect = predictedRegion == className;

                            numProcessedTemplates++;
                            numCorrectGroup += groupCorrect ? 1 : 0;
                            numCorrectRegion += regionCorrect ? 1 : 0;

                            float elapsed = (float)watch.ElapsedTicks / (float)Stopwatch.Frequency * 1000.0f;

                            averageMatchTime += (float)elapsed;
                            numMatches++;

                            int index = samples[className].IndexOf(query);

                            string result = Utils.CSV(pid, groupForRegion[className], className, query["Path"], predictedGroup, predictedRegion);
                            results.Add(result);
                            File.AppendAllText("results.txt", result + Environment.NewLine);
                            clipboardText += result;
                        }

                        IncrementProgress();
                    }

                    double accuracyGroup = numCorrectGroup / numProcessedTemplates;
                    double accuracyRegion = numCorrectRegion / numProcessedTemplates;
                    averageMatchTime /= numMatches;

                    Debug.WriteLine("");
                    Debug.WriteLine("Accuracy (Group): " + (accuracyGroup * 100) + "%");
                    Debug.WriteLine("Accuracy (Region): " + (accuracyRegion * 100) + "%");
                    Debug.WriteLine("Average Processing Time: " + averageProcessingTime + " ms");
                    Debug.WriteLine("Average Matching Time: " + averageMatchTime + " ms");

                    SetProgress(0);
                    SetStatus(pid + "\nAccuracy (Group): " + (accuracyGroup * 100) + "%\n" + "Accuracy (Region): " + (accuracyRegion * 100) + "%\n" + "Average Processing Time: " + averageProcessingTime + " ms\n" + "Average Matching Time: " + averageMatchTime + " ms");

                    //clipboardText += accuracyGroup + "\t" + accuracyRegion + "\t" + averageProcessingTime + "\t" + averageMatchTime + Environment.NewLine;

                    Invoke(new MethodInvoker(delegate
                    {
                        Clipboard.SetText(clipboardText);
                    }));
                }
                //File.WriteAllText("results.txt", clipboardText);
            });
        }

        private void TestTrainingPhase(string rootDir, string[] pids, string sampleDir, string logDir)
        {
            Task.Factory.StartNew(() =>
            {
                SetProgress(0);
                SetProgressMax(pids.Length * 100);
                foreach (string pid in pids)
                {
                    string dir = Path.Combine(rootDir, pid, logDir);
                    
                    List<string> logFiles = new List<string>(Directory.GetFiles(dir, "*.log"));

                    List<FileInfo> templateFiles = new List<FileInfo>((new DirectoryInfo(Path.Combine(rootDir, pid, sampleDir))).GetFiles("*.png"));
                    templateFiles.Sort((a, b) => { return a.CreationTime.CompareTo(b.CreationTime); });

                    SetStatus(pid + ": Loading and Processing Files");

                    foreach (string fileName in logFiles)
                    {
                        List<Logging.LogEvent> events = Logging.ReadLog(fileName);
                        events.Sort((a, b) => { return a.timestamp.CompareTo(b.timestamp); });
                        IncrementProgress((int)(25.0f / logFiles.Count));

                        //bool isTraining = false;

                        //// read the log and look for the autocapture training events, which will have labeled video frames in between
                        //// identify all video indices for which we can determine a label
                        //Dictionary<int, string> frameClasses = new Dictionary<int, string>();
                        //HashSet<int> isTrainingSample = new HashSet<int>();
                        //List<int> currIndices = new List<int>();
                        //string currClass = null;
                        //int mostRecentIndex = 0;
                        //foreach (Logging.LogEvent e in events)
                        //{
                        //    if (e.message == "start_autocapture_location")
                        //        isTraining = true;
                        //    else if (e.message == "stop_autocapture_location" || e.message == "External Stop Autocapture")
                        //    {
                        //        isTraining = false;
                        //        if (currClass != null)
                        //            foreach (int index in currIndices) frameClasses.Add(index, currClass);
                        //        currIndices.Clear();
                        //        currClass = null;
                        //    }
                        //    else if (e.message.StartsWith("Added template: "))
                        //    {
                        //        currClass = e.message.Replace("Added template: ", "").Replace(" ", "_");
                        //        isTrainingSample.Add(mostRecentIndex);
                        //    }
                        //    else if (e is Logging.VideoFrameEvent)
                        //    {
                        //        mostRecentIndex = (e as Logging.VideoFrameEvent).frameIndex;
                        //        if (isTraining) currIndices.Add(mostRecentIndex);
                        //    }
                        //}
                        //IncrementProgress((int)(25.0f / logFiles.Count));

                        //// extract video frames
                        //string videoFileName = Path.ChangeExtension(fileName, "avi");
                        //VideoFileReader videoReader = new VideoFileReader();
                        //videoReader.Open(videoFileName);
                        //Dictionary<string, int> countsForClass = new Dictionary<string, int>();
                        //try
                        //{
                        //    bool hasFramesToRead = true;
                        //    int frameIndex = 0;
                        //    while (hasFramesToRead)
                        //    {
                        //        Bitmap frame = videoReader.ReadVideoFrame();
                        //        if (frame == null) hasFramesToRead = false;
                        //        else if (frameClasses.ContainsKey(frameIndex))
                        //        {
                        //            if (!countsForClass.ContainsKey(frameClasses[frameIndex])) countsForClass.Add(frameClasses[frameIndex], 0);
                        //            int templateIndex = countsForClass[frameClasses[frameIndex]]++;
                        //            frame.Save(Path.Combine(outputDir, frameClasses[frameIndex] + "_" + templateIndex + (isTrainingSample.Contains(frameIndex) ? "_train" : "") + ".png"));
                        //        }
                        //        frameIndex++;
                        //    }
                        //}
                        //catch { }
                        //videoReader.Close();
                        //IncrementProgress((int)(50.0f / logFiles.Count));
                    }
                }
                SetProgress(0);
                SetStatus("Done");
            });
        }

        private void EvaluateFrameAccuracy(string rootDir, string[] pids, string testDir)
        {
            string clipboardText = "";
            int numSamplesPerClass = 100;
            int numTrials = 1;
            Task.Factory.StartNew(() =>
            {
                foreach (string pid in pids)
                {
                    string dir = Path.Combine(rootDir, pid, testDir);

                    List<string> files = new List<string>(Directory.GetFiles(dir, "*.png"));

                    SetProgress(0);
                    SetStatus(pid + ": Preparing Files");
                    SetProgressMax(files.Count);

                    var samples = new Dictionary<string, List<string>>();
                    var trainingSet = new Dictionary<string, List<string>>();
                    var testingSet = new Dictionary<string, List<string>>();
                    for (int fileIndex = 0; fileIndex < files.Count; fileIndex++)
                    {
                        string filename = files[fileIndex];
                        Match match = Regex.Match(Path.GetFileNameWithoutExtension(filename), @"([a-zA-Z]+)_([a-zA-Z]+)_(\d+)(_train)?");
                        string coarseLocation = match.Groups[1].Value;
                        string fineLocation = match.Groups[2].Value;
                        int index = int.Parse(match.Groups[3].Value);
                        bool isTraining = match.Groups[4].Value == "_train";

                        if(!samples.ContainsKey(coarseLocation + "_" + fineLocation))
                        {
                            samples.Add(coarseLocation + "_" + fineLocation, new List<string>());
                            trainingSet.Add(coarseLocation + "_" + fineLocation, new List<string>());
                            testingSet.Add(coarseLocation + "_" + fineLocation, new List<string>());
                        }

                        if(!isTraining)
                            samples[coarseLocation + "_" + fineLocation].Add(filename);

                        IncrementProgress();
                    }

                    int numTrainSamples = samples.Count * numSamplesPerClass;
                    int numTestSamples = 0;
                    foreach (string className in samples.Keys) numTestSamples += samples[className].Count - numSamplesPerClass;

                    double accuracyCoarse = 0, accuracyFine = 0;
                    for (int trial = 0; trial < numTrials; trial++)
                    {
                        SetProgress(0);
                        SetStatus(pid + " (" + trial + "): Building Training and Testing Sets");
                        SetProgressMax(samples.Count);

                        foreach (string location in samples.Keys)
                        {
                            int[] randomIndices = Utils.RandomPermutation(samples[location].Count);
                            for (int i = 0; i < samples[location].Count; i++)
                            {
                                if (i < numSamplesPerClass) trainingSet[location].Add(samples[location][randomIndices[i]]);
                                else testingSet[location].Add(samples[location][randomIndices[i]]);
                            }
                            IncrementProgress();
                        }

                        SetProgress(0);
                        SetStatus(pid + " (" + trial + "): Processing Training Images");
                        SetProgressMax(samples.Count * numSamplesPerClass);

                        Worker worker = Worker.Default;
                        DeviceMemory<byte> reusableImageGPU = worker.Malloc<byte>(640 * 640);
                        Localization.Instance.Reset();
                        foreach (string location in trainingSet.Keys)
                            foreach (string filename in trainingSet[location])
                            {
                                Match match = Regex.Match(Path.GetFileNameWithoutExtension(filename), @"([a-zA-Z]+)_([a-zA-Z]+)_(\d+)");
                                string coarseLocation = match.Groups[1].Value;
                                string fineLocation = match.Groups[2].Value;
                                int index = int.Parse(match.Groups[3].Value);

                                Bitmap bmp = (Bitmap)Bitmap.FromFile(filename);
                                VideoFrame frame = new VideoFrame() { Image = new Image<Gray, byte>(bmp) };
                                frame.ImageGPU = reusableImageGPU;
                                frame.ImageGPU.Scatter(frame.Image.Bytes);
                                ImageTemplate template = new ImageTemplate(frame);
                                template["frameIndex"] = index;
                                template["Path"] = Path.GetFileNameWithoutExtension(filename);
                                template["CoarseLocation"] = coarseLocation;
                                template["FineLocation"] = fineLocation;

                                ImageProcessing.ProcessTemplate(template, false);
                                Localization.Instance.AddTrainingExample(template, coarseLocation, fineLocation);

                                IncrementProgress();
                            }
                        SetStatus(pid + " (" + trial + "): Training Classifiers");
                        Localization.Instance.Train();

                        //int numTestSamples = 0;
                        //foreach (string location in testingSet.Keys) numTestSamples += testingSet[location].Count;

                        SetProgress(0);
                        SetStatus(pid + " (" + trial + "): Classifying Test Images (100.0% / 100.0%)");
                        SetProgressMax(numTestSamples);

                        float numCorrectCoarse = 0, numCorrectFine = 0, numProcessedTemplates = 0;

                        foreach (string testLocation in testingSet.Keys)
                            foreach (string testFilename in testingSet[testLocation])
                            {
                                Match testMatch = Regex.Match(Path.GetFileNameWithoutExtension(testFilename), @"([a-zA-Z]+)_([a-zA-Z]+)_(\d+)");
                                string testCoarseLocation = testMatch.Groups[1].Value;
                                string testFineLocation = testMatch.Groups[2].Value;
                                int testIndex = int.Parse(testMatch.Groups[3].Value);

                                Bitmap testBmp = (Bitmap)Bitmap.FromFile(testFilename);
                                VideoFrame testFrame = new VideoFrame() { Image = new Image<Gray, byte>(testBmp) };
                                testFrame.ImageGPU = reusableImageGPU;
                                testFrame.ImageGPU.Scatter(testFrame.Image.Bytes);
                                ImageTemplate query = new ImageTemplate(testFrame);
                                query["frameIndex"] = testIndex;
                                query["Path"] = Path.GetFileNameWithoutExtension(testFilename);
                                query["CoarseLocation"] = testCoarseLocation;
                                query["FineLocation"] = testFineLocation;

                                //if (ImageProcessing.ImageFocus(query) < 1000) continue;

                                ImageProcessing.ProcessTemplate(query, false);

                                Dictionary<string, float> coarseProbabilities = new Dictionary<string, float>();
                                string predictedCoarse = Localization.Instance.PredictCoarseLocation(query, out coarseProbabilities);
                                string predictedFine = "";
                                bool foundFeatureMatch = false;
                                Dictionary<string, float> fineProbabilities = new Dictionary<string, float>();
                                predictedFine = Localization.Instance.PredictFineLocation(query, out foundFeatureMatch, out fineProbabilities, true, false, false, predictedCoarse);

                                bool coarseCorrect = predictedCoarse == testCoarseLocation;
                                bool fineCorrect = predictedFine == testFineLocation;
                                //if (!coarseCorrect)
                                //      Debug.WriteLine("error");

                                numProcessedTemplates++;
                                numCorrectCoarse += coarseCorrect ? 1 : 0;
                                numCorrectFine += fineCorrect ? 1 : 0;

                                IncrementProgress();
                                SetStatus(pid + " (" + trial + "): Classifying Test Images (" + (numCorrectCoarse / numProcessedTemplates * 100).ToString("0.0") + "% / " + (numCorrectFine / numProcessedTemplates * 100).ToString("0.0") + "%)");
                            }

                        accuracyCoarse += numCorrectCoarse / numProcessedTemplates;
                        accuracyFine += numCorrectFine / numProcessedTemplates;
                    }
                    accuracyCoarse /= numTrials;
                    accuracyFine /= numTrials;

                    Debug.WriteLine("");
                    Debug.WriteLine("Accuracy (Coarse): " + (accuracyCoarse * 100) + "%");
                    Debug.WriteLine("Accuracy (Fine): " + (accuracyFine * 100) + "%");
                    Debug.WriteLine("N_train = " + numSamplesPerClass * samples.Count + ", N_test = " + numTestSamples);

                    SetProgress(0);
                    SetStatus(pid + "\nAccuracy (Coarse): " + (accuracyCoarse * 100) + "%\n" + "Accuracy (Fine): " + (accuracyFine * 100) + "%\n" + "N_train = " + numSamplesPerClass * samples.Count + ", N_test = " + numTestSamples);

                    clipboardText += accuracyCoarse + "\t" + accuracyFine + "\t" + (numSamplesPerClass * samples.Count) + "\t" + numTestSamples + Environment.NewLine;

                    Invoke(new MethodInvoker(delegate
                    {
                        Clipboard.SetText(clipboardText);
                    }));
                }
                File.WriteAllText("results.txt", clipboardText);
            });
        }

        private void TestGestureRecognitionAccuracy(string classifierPath, string testDir, bool includeTapAndSwipeDown = true)
        {
            string clipboardText = "";
            Task.Factory.StartNew(() =>
            {
                GestureRecognition.LoadClassifier(classifierPath);

                string[] files = Directory.GetFiles(testDir, "*.gest");
                float numCorrect = 0, numProcessed = 0;

                SetProgress(0);
                SetStatus("Classifying");
                SetProgressMax(files.Length);

                foreach (string file in files)
                {
                    try
                    {
                        string[] parts = Path.GetFileNameWithoutExtension(file).Split('_');
                        string className = parts[0];
                        int index = int.Parse(parts[1]);
                        if (!includeTapAndSwipeDown && (className == "Tap" || className == "Swipe Down")) continue;

                        string json = File.ReadAllText(file);
                        json = json.Replace(",\"Visualization\":\"System.Drawing.Bitmap\"", "");
                        Gesture gesture = JsonConvert.DeserializeObject<Gesture>(json);

                        string predictedClass = GestureRecognition.PredictGesture(gesture);
                        if (predictedClass == className) numCorrect++;
                        else
                            Debug.WriteLine("Error: " + predictedClass + " != " + className + " (" + index + ")");
                        numProcessed++;
                    }
                    finally { IncrementProgress(); }
                }

                float accuracy = numCorrect / numProcessed;
                Debug.WriteLine("Accuracy: " + (accuracy * 100).ToString("0.0") + "%");
            });
        }

        private void CrossValidateGestureRecognition(string testDir, bool includeTapAndSwipeDown = true)
        {
            Task.Factory.StartNew(() =>
            {
                SetProgress(0);
                SetStatus("Loading Gesture Data");

                string[] files = Directory.GetFiles(testDir, "*.gest");
                Dictionary<string, List<Gesture>> samples = new Dictionary<string, List<Gesture>>();
                int numSamples = 0;
                foreach(string file in files)
                {
                    string[] parts = Path.GetFileNameWithoutExtension(file).Split('_');
                    string className = parts[0];
                    int index = int.Parse(parts[1]);
                    if (!includeTapAndSwipeDown && (className == "Tap" || className == "Swipe Down")) continue;

                    string json = File.ReadAllText(file);
                    json = json.Replace(",\"Visualization\":\"System.Drawing.Bitmap\"", "");
                    Gesture gesture = JsonConvert.DeserializeObject<Gesture>(json);

                    if (!samples.ContainsKey(className)) samples.Add(className, new List<Gesture>());
                    samples[className].Add(gesture);
                    numSamples++;
                }

                SetProgress(0);
                SetStatus("Classifying");
                SetProgressMax(numSamples);
                float numCorrect = 0, numProcessed = 0;

                foreach (string className in samples.Keys)
                {
                    for (int testIndex = 0; testIndex < samples[className].Count; testIndex++)
                    {
                        GestureRecognition.Reset();
                        foreach (string trainClassName in samples.Keys)
                            for (int trainIndex = 0; trainIndex < samples[trainClassName].Count; trainIndex++)
                                if (trainClassName != className || trainIndex != testIndex)
                                    GestureRecognition.AddTrainingExample(samples[trainClassName][trainIndex], trainClassName);
                        GestureRecognition.Train();

                        string predictedClass = GestureRecognition.PredictGesture(samples[className][testIndex]);
                        if (predictedClass == className) numCorrect++;
                        else
                            Debug.WriteLine("Error: " + predictedClass + " != " + className);
                        numProcessed++;

                        IncrementProgress();
                    }
                }

                float accuracy = numCorrect / numProcessed;
                Debug.WriteLine("Accuracy: " + (accuracy * 100).ToString("0.0") + "%");
                SetProgress(0);
                SetStatus("Done");
            });
        }

        float minFocus = 0;
        private void TestLocalizationAccuracy(string[] trainingDirs, string testDir)
        {
            string clipboardText = "";
            Task.Factory.StartNew(() =>
            {
                List<string> trainFiles = new List<string>();
                foreach (string dir in trainingDirs)
                {
                    string[] files = Directory.GetFiles(dir, "*.png");
                    trainFiles.AddRange(files);
                }
                SetProgress(0);
                SetStatus("Loading Training Files");
                SetProgressMax(trainFiles.Count);

                var samples = new Dictionary<string, List<ImageTemplate>>();
                Worker worker = Worker.Default;

                Localization.Instance.Reset();
                foreach (string filename in trainFiles)
                {
                    Match match = Regex.Match(Path.GetFileNameWithoutExtension(filename), @"([a-zA-Z]+)_([a-zA-Z]+)_(\d+)");
                    string coarseLocation = match.Groups[1].Value;
                    string fineLocation = match.Groups[2].Value;
                    int index = int.Parse(match.Groups[3].Value);

                    Bitmap bmp = (Bitmap)Bitmap.FromFile(filename);
                    VideoFrame frame = new VideoFrame() { Image = new Image<Gray, byte>(bmp) };
                    frame.ImageGPU = worker.Malloc<byte>(640 * 640);
                    frame.ImageGPU.Scatter(frame.Image.Bytes);
                    ImageTemplate template = new ImageTemplate(frame);
                    template["frameIndex"] = index;
                    template["Path"] = Path.GetFileNameWithoutExtension(filename);
                    template["CoarseLocation"] = coarseLocation;
                    template["FineLocation"] = fineLocation;

                    float focus = ImageProcessing.ImageFocus(template);
                    if (focus >= minFocus)
                    {
                        if (!samples.ContainsKey(fineLocation)) samples.Add(fineLocation, new List<ImageTemplate>());
                        samples[fineLocation].Add(template);

                        ImageProcessing.ProcessTemplate(template, false);
                        Localization.Instance.AddTrainingExample(template, coarseLocation, fineLocation);
                    }

                    IncrementProgress();
                }

                SetStatus("Training Classifier");
                Localization.Instance.Train();

                string[] testFiles = Directory.GetFiles(testDir, "*.png");
                SetProgress(0);
                SetStatus("Classifying");
                SetProgressMax(testFiles.Length);
                float numCorrectCoarse = 0, numCorrectFine = 0, numProcessed = 0;

                foreach (string filename in testFiles)
                {
                    try
                    {
                        Match match = Regex.Match(Path.GetFileNameWithoutExtension(filename), @"([a-zA-Z]+)_([a-zA-Z]+)_(\d+)");
                        string coarseLocation = match.Groups[1].Value;
                        string fineLocation = match.Groups[2].Value;
                        int index = int.Parse(match.Groups[3].Value);

                        Bitmap bmp = (Bitmap)Bitmap.FromFile(filename);
                        VideoFrame frame = new VideoFrame() { Image = new Image<Gray, byte>(bmp) };
                        frame.ImageGPU = worker.Malloc<byte>(640 * 640);
                        frame.ImageGPU.Scatter(frame.Image.Bytes);
                        ImageTemplate template = new ImageTemplate(frame);
                        template["frameIndex"] = index;
                        template["Path"] = Path.GetFileNameWithoutExtension(filename);
                        template["CoarseLocation"] = coarseLocation;
                        template["FineLocation"] = fineLocation;

                        float focus = ImageProcessing.ImageFocus(template);
                        if (focus >= minFocus)
                        {
                            ImageProcessing.ProcessTemplate(template, false);

                            string predictedCoarse = Localization.Instance.PredictCoarseLocation(template);
                            string predictedFine = "";
                            //predictedRegion = Localization.Instance.PredictFineLocation(query, true, true, false, predictedGroup);
                            bool foundFeatureMatch = false;
                            Dictionary<string, float> fineProbabilities = new Dictionary<string, float>();
                            predictedFine = Localization.Instance.PredictFineLocation(template, out foundFeatureMatch, out fineProbabilities, true, false, false, predictedCoarse);

                            if (predictedCoarse == coarseLocation) numCorrectCoarse++;
                            if (predictedFine == fineLocation) numCorrectFine++;

                            if (predictedCoarse != coarseLocation || predictedFine != fineLocation)
                            {
                                Debug.WriteLine("Error: " + predictedCoarse + ", " + predictedFine + " != " + coarseLocation + ", " + fineLocation + " (" + index + ")");
                            }

                            frame.ImageGPU.Dispose();

                            numProcessed++;
                        }
                    }
                    finally { IncrementProgress(); }
                }

                float accuracyCoarse = numCorrectCoarse / numProcessed;
                float accuracyFine = numCorrectFine / numProcessed;
                Debug.WriteLine("Accuracy: " + (accuracyCoarse * 100).ToString("0.0") + "% / " + (accuracyFine * 100).ToString("0.0") + "%");
            });
        }

        private void TestLocalizationAccuracyCombinations(string[] dirs, int initNumTrainingDirs = 1, int maxNumTrainingDirs = 1, string[] dirsToEvaluate = null)
        {
            List<string> evalDirs = new List<string>();
            if (dirsToEvaluate == null || dirsToEvaluate.Length == 0) evalDirs.AddRange(dirs);
            else evalDirs.AddRange(dirsToEvaluate);
            string clipboardText = "";
            Task.Factory.StartNew(() =>
            {
                for (int numTrainingDirs = initNumTrainingDirs; numTrainingDirs <= maxNumTrainingDirs; numTrainingDirs++)
                {
                    var combinations = (new List<string>(dirs)).Combinations(numTrainingDirs);
                    foreach (var combination in combinations)
                    {
                        bool shouldEval = false;

                        List<string> trainingDirs = new List<string>();
                        string trainCombination = "";
                        foreach (string dir in combination)
                        {
                            trainingDirs.Add(dir);
                            string[] parts = dir.Split('\\');
                            trainCombination += parts[parts.Length - 2] + ", ";
                            if (evalDirs.Contains(dir))
                                shouldEval = true;
                        }
                        trainCombination = trainCombination.TrimEnd(',', ' ');
                        if (!shouldEval) continue;

                        List<string> trainFiles = new List<string>();
                        foreach (string dir in trainingDirs)
                        {
                            string[] files = Directory.GetFiles(dir, "*.png");
                            trainFiles.AddRange(files);
                        }
                        SetProgress(0);
                        SetStatus(trainCombination + ": Loading Training Files");
                        SetProgressMax(trainFiles.Count);

                        var samples = new Dictionary<string, List<ImageTemplate>>();
                        Worker worker = Worker.Default;

                        int numTrainImages = 0;
                        Localization.Instance.Reset();
                        foreach (string filename in trainFiles)
                        {
                            Match match = Regex.Match(Path.GetFileNameWithoutExtension(filename), @"([a-zA-Z]+)_([a-zA-Z]+)_(\d+)");
                            string coarseLocation = match.Groups[1].Value;
                            string fineLocation = match.Groups[2].Value;
                            int index = int.Parse(match.Groups[3].Value);

                            Bitmap bmp = (Bitmap)Bitmap.FromFile(filename);
                            VideoFrame frame = new VideoFrame() { Image = new Image<Gray, byte>(bmp) };
                            frame.ImageGPU = worker.Malloc<byte>(640 * 640);
                            frame.ImageGPU.Scatter(frame.Image.Bytes);
                            ImageTemplate template = new ImageTemplate(frame);
                            template["frameIndex"] = index;
                            template["Path"] = Path.GetFileNameWithoutExtension(filename);
                            template["CoarseLocation"] = coarseLocation;
                            template["FineLocation"] = fineLocation;

                            float focus = ImageProcessing.ImageFocus(template);
                            if (focus >= minFocus)
                            {
                                if (!samples.ContainsKey(fineLocation)) samples.Add(fineLocation, new List<ImageTemplate>());
                                samples[fineLocation].Add(template);

                                ImageProcessing.ProcessTemplate(template, false);
                                Localization.Instance.AddTrainingExample(template, coarseLocation, fineLocation);

                                numTrainImages++;
                            }

                            IncrementProgress();
                        }

                        SetStatus(trainCombination + ": Training Classifier");
                        Localization.Instance.Train();

                        List<string> testDirs = new List<string>();
                        foreach (string dir in dirs)
                            if (!trainingDirs.Contains(dir))
                                testDirs.Add(dir);

                        foreach (string testDir in testDirs)
                        {
                            string[] parts = testDir.Split('\\');
                            string testDirName = parts[parts.Length - 2];
                            string[] testFiles = Directory.GetFiles(testDir, "*.png");
                            SetProgress(0);
                            SetStatus(testDirName + ": Classifying");
                            SetProgressMax(testFiles.Length);
                            float numCorrectCoarse = 0, numCorrectFine = 0, numProcessed = 0;

                            foreach (string filename in testFiles)
                            {
                                try
                                {
                                    Match match = Regex.Match(Path.GetFileNameWithoutExtension(filename), @"([a-zA-Z]+)_([a-zA-Z]+)_(\d+)");
                                    string coarseLocation = match.Groups[1].Value;
                                    string fineLocation = match.Groups[2].Value;
                                    int index = int.Parse(match.Groups[3].Value);

                                    Bitmap bmp = (Bitmap)Bitmap.FromFile(filename);
                                    VideoFrame frame = new VideoFrame() { Image = new Image<Gray, byte>(bmp) };
                                    frame.ImageGPU = worker.Malloc<byte>(640 * 640);
                                    frame.ImageGPU.Scatter(frame.Image.Bytes);
                                    ImageTemplate template = new ImageTemplate(frame);
                                    template["frameIndex"] = index;
                                    template["Path"] = Path.GetFileNameWithoutExtension(filename);
                                    template["CoarseLocation"] = coarseLocation;
                                    template["FineLocation"] = fineLocation;

                                    float focus = ImageProcessing.ImageFocus(template);
                                    if (focus >= minFocus)
                                    {
                                        ImageProcessing.ProcessTemplate(template, false);

                                        string predictedCoarse = Localization.Instance.PredictCoarseLocation(template);
                                        string predictedFine = "";
                                        //predictedRegion = Localization.Instance.PredictFineLocation(query, true, true, false, predictedGroup);
                                        bool foundFeatureMatch = false;
                                        Dictionary<string, float> fineProbabilities = new Dictionary<string, float>();
                                        predictedFine = Localization.Instance.PredictFineLocation(template, out foundFeatureMatch, out fineProbabilities, true, false, false, predictedCoarse);

                                        if (predictedCoarse == coarseLocation) numCorrectCoarse++;
                                        if (predictedFine == fineLocation) numCorrectFine++;

                                        //if (predictedCoarse != coarseLocation || predictedFine != fineLocation)
                                        //{
                                        //    Debug.WriteLine("Error: " + predictedCoarse + ", " + predictedFine + " != " + coarseLocation + ", " + fineLocation + " (" + index + ")");
                                        //}

                                        frame.ImageGPU.Dispose();

                                        numProcessed++;
                                    }
                                }
                                finally { IncrementProgress(); }
                            }

                            float accuracyCoarse = numCorrectCoarse / numProcessed;
                            float accuracyFine = numCorrectFine / numProcessed;
                            Debug.WriteLine(trainCombination + " -> " + testDirName + " Accuracy: " + (accuracyCoarse * 100).ToString("0.0") + "% / " + (accuracyFine * 100).ToString("0.0") + "%");
                            clipboardText += trainCombination + "\t" + testDirName + "\t" + accuracyCoarse + "\t" + accuracyFine + "\t" + numTrainImages + "\t" + numProcessed + Environment.NewLine;

                            try
                            {
                                Invoke(new MethodInvoker(delegate
                                {
                                    Clipboard.SetText(clipboardText);
                                }));
                            }
                            catch { Debug.WriteLine("Error: Could not write to clipboard"); }
                        }
                    }
                }
                SetStatus("Done");
                SetProgress(0);
                File.WriteAllText("results.txt", clipboardText);
            });
        }

        private void TestLocalizationFromVideo(string[] trainingDirs, string testDir)
        {
            int numSamplesPerClass = 100;
            Task.Factory.StartNew(() =>
            {
                Worker worker = Worker.Default;

                foreach (string dir in trainingDirs)
                {
                    List<string> jsonFiles = new List<string>(Directory.GetFiles(dir, "*.log"));

                    foreach (string fileName in jsonFiles)
                    {
                        SetStatus("Loading Log File: " + Path.GetFileNameWithoutExtension(fileName));
                        SetProgress(0);
                        List<Logging.LogEvent> events = Logging.ReadLog(fileName);
                        events.Sort((a, b) => { return a.timestamp.CompareTo(b.timestamp); });
                        SetStatus("Processing Log File: " + Path.GetFileNameWithoutExtension(fileName));
                        SetProgressMax(events.Count);

                        bool isTraining = false;

                        // read the log and look for the autocapture training events, which will have labeled video frames in between
                        // identify all video indices for which we can determine a label
                        Dictionary<int, string> frameClasses = new Dictionary<int, string>();
                        Dictionary<string, List<int>> sampleFrameIndices = new Dictionary<string, List<int>>();
                        HashSet<int> isTrainingSample = new HashSet<int>();
                        List<int> currIndices = new List<int>();
                        string currClass = null;
                        int mostRecentIndex = 0;
                        foreach (Logging.LogEvent e in events)
                        {
                            if (e.message == "start_autocapture_location")
                                isTraining = true;
                            else if (e.message == "stop_autocapture_location" || e.message == "External Stop Autocapture" || e.message == "touch_up_timeout")
                            {
                                isTraining = false;
                                if (currClass != null)
                                {
                                    //foreach (int index in currIndices) frameClasses.Add(index, currClass);
                                    if (!sampleFrameIndices.ContainsKey(currClass)) sampleFrameIndices.Add(currClass, new List<int>());
                                    sampleFrameIndices[currClass].AddRange(currIndices);
                                }
                                currIndices.Clear();
                                currClass = null;
                            }
                            else if (e.message.StartsWith("Added template: "))
                            {
                                currClass = e.message.Replace("Added template: ", "").Replace(" ", "_");
                                isTrainingSample.Add(mostRecentIndex);
                            }
                            else if (e is Logging.VideoFrameEvent)
                            {
                                mostRecentIndex = (e as Logging.VideoFrameEvent).frameIndex;
                                if (isTraining) currIndices.Add(mostRecentIndex);
                            }
                            IncrementProgress();
                        }

                        // select random samples
                        Random rand = new Random();
                        foreach(string className in sampleFrameIndices.Keys)
                        {
                            List<int> frames = sampleFrameIndices[className];
                            frames.Shuffle(rand.Next());
                            for(int i = 0; i < numSamplesPerClass; i++)
                                frameClasses.Add(frames[i], className);
                        }

                        SetStatus("Reading Video File");
                        SetProgressMax(frameClasses.Count);
                        SetProgress(0);

                        // extract video frames
                        string videoFileName = Path.ChangeExtension(fileName, "avi");
                        VideoFileReader videoReader = new VideoFileReader();
                        videoReader.Open(videoFileName);
                        Dictionary<string, int> countsForClass = new Dictionary<string, int>();
                        try
                        {
                            bool hasFramesToRead = true;
                            int frameIndex = 0;
                            while (hasFramesToRead)
                            {
                                Bitmap frame = videoReader.ReadVideoFrame();
                                if (frame == null) hasFramesToRead = false;
                                else if (frameClasses.ContainsKey(frameIndex))
                                {
                                    if (!countsForClass.ContainsKey(frameClasses[frameIndex])) countsForClass.Add(frameClasses[frameIndex], 0);
                                    int templateIndex = countsForClass[frameClasses[frameIndex]]++;

                                    string fineLocation = frameClasses[frameIndex].Split('_')[1];
                                    string coarseLocation = frameClasses[frameIndex].Split('_')[0];

                                    VideoFrame vf = new VideoFrame() { Image = new Image<Gray, byte>(frame) };
                                    vf.ImageGPU = worker.Malloc<byte>(640 * 640);
                                    vf.ImageGPU.Scatter(vf.Image.Bytes);
                                    ImageTemplate template = new ImageTemplate(vf);
                                    template["CoarseLocation"] = coarseLocation;
                                    template["FineLocation"] = fineLocation;
                                    
                                    ImageProcessing.ProcessTemplate(template, false);
                                    Localization.Instance.AddTrainingExample(template, coarseLocation, fineLocation);

                                    vf.ImageGPU.Dispose();
                                    vf.Image.Dispose();

                                    IncrementProgress();
                                }
                                frameIndex++;
                            }
                        }
                        catch { }
                        videoReader.Close();
                    }
                }

                SetStatus("Training Classifier");
                Localization.Instance.Train();

                string[] testFiles = Directory.GetFiles(testDir, "*.png");
                SetProgress(0);
                SetStatus("Classifying");
                SetProgressMax(testFiles.Length);
                float numCorrectCoarse = 0, numCorrectFine = 0, numProcessed = 0;

                foreach (string filename in testFiles)
                {
                    try
                    {
                        Match match = Regex.Match(Path.GetFileNameWithoutExtension(filename), @"([a-zA-Z]+)_([a-zA-Z]+)_(\d+)");
                        string coarseLocation = match.Groups[1].Value;
                        string fineLocation = match.Groups[2].Value;
                        int index = int.Parse(match.Groups[3].Value);

                        Bitmap bmp = (Bitmap)Bitmap.FromFile(filename);
                        VideoFrame frame = new VideoFrame() { Image = new Image<Gray, byte>(bmp) };
                        frame.ImageGPU = worker.Malloc<byte>(640 * 640);
                        frame.ImageGPU.Scatter(frame.Image.Bytes);
                        ImageTemplate template = new ImageTemplate(frame);
                        template["frameIndex"] = index;
                        template["Path"] = Path.GetFileNameWithoutExtension(filename);
                        template["CoarseLocation"] = coarseLocation;
                        template["FineLocation"] = fineLocation;
                        
                        ImageProcessing.ProcessTemplate(template, false);

                        string predictedCoarse = Localization.Instance.PredictCoarseLocation(template);
                        string predictedFine = "";
                        //predictedRegion = Localization.Instance.PredictFineLocation(query, true, true, false, predictedGroup);
                        bool foundFeatureMatch = false;
                        Dictionary<string, float> fineProbabilities = new Dictionary<string, float>();
                        predictedFine = Localization.Instance.PredictFineLocation(template, out foundFeatureMatch, out fineProbabilities, true, false, false, predictedCoarse);

                        if (predictedCoarse == coarseLocation) numCorrectCoarse++;
                        if (predictedFine == fineLocation) numCorrectFine++;

                        if (predictedCoarse != coarseLocation || predictedFine != fineLocation)
                        {
                            Debug.WriteLine("Error: " + predictedCoarse + ", " + predictedFine + " != " + coarseLocation + ", " + fineLocation + " (" + index + ")");
                        }

                        frame.ImageGPU.Dispose();

                        numProcessed++;
                    }
                    finally { IncrementProgress(); }
                }

                float accuracyCoarse = numCorrectCoarse / numProcessed;
                float accuracyFine = numCorrectFine / numProcessed;
                Debug.WriteLine("Accuracy: " + (accuracyCoarse * 100).ToString("0.0") + "% / " + (accuracyFine * 100).ToString("0.0") + "%");
            });
        }

        private void TestLocalizationAccuracyCombinations2(string[] dirs, int initNumTrainingDirs = 1, int maxNumTrainingDirs = 1, string[] dirsToEvaluate = null)
        {
            List<string> evalDirs = new List<string>();
            if (dirsToEvaluate == null || dirsToEvaluate.Length == 0) evalDirs.AddRange(dirs);
            else evalDirs.AddRange(dirsToEvaluate);
            string clipboardText = "";
            Task.Factory.StartNew(() =>
            {
                for (int numTrainingDirs = initNumTrainingDirs; numTrainingDirs <= maxNumTrainingDirs; numTrainingDirs++)
                {
                    var combinations = (new List<string>(dirs)).Combinations(numTrainingDirs);
                    foreach (var combination in combinations)
                    {
                        bool shouldEval = false;

                        List<string> trainingDirs = new List<string>();
                        string trainCombination = "";
                        foreach (string dir in combination)
                        {
                            trainingDirs.Add(dir);
                            string[] parts = dir.Split('\\');
                            trainCombination += parts[parts.Length - 2] + ", ";
                            if (evalDirs.Contains(dir))
                                shouldEval = true;
                        }
                        trainCombination = trainCombination.TrimEnd(',', ' ');
                        if (!shouldEval) continue;

                        Dictionary<string, List<ImageTemplate>> trainingSamples = new Dictionary<string, List<ImageTemplate>>();
                        foreach (string dir in trainingDirs)
                        {
                            Dictionary<string, List<ImageTemplate>> temp = GetRandomSamplesFromVideo(dir, 100);
                            foreach (string fineLocation in temp.Keys)
                            {
                                if (!trainingSamples.ContainsKey(fineLocation)) trainingSamples.Add(fineLocation, new List<ImageTemplate>());
                                trainingSamples[fineLocation].AddRange(temp[fineLocation]);
                            }
                        }

                        TrainFromSamples(trainingSamples);

                        List<string> testDirs = new List<string>();
                        foreach (string dir in dirs)
                            if (!trainingDirs.Contains(dir))
                                testDirs.Add(dir);

                        foreach (string testDir in testDirs)
                        {
                            string[] parts = testDir.Split('\\');
                            string testDirName = parts[parts.Length - 2];
                            string[] testFiles = Directory.GetFiles(testDir, "*.png");
                            SetProgress(0);
                            SetStatus(testDirName + ": Classifying");
                            SetProgressMax(testFiles.Length);
                            Tuple<float, float, int> results = TestLocalizationAccuracyWithVideoPredictions(testDir, 20);
                            float accuracyCoarse = results.Item1;
                            float accuracyFine = results.Item2;
                            int numProcessed = results.Item3;
                            int numTrainImages = trainingDirs.Count * 100 * 9;

                            Debug.WriteLine(trainCombination + " -> " + testDirName + " Accuracy: " + (accuracyCoarse * 100).ToString("0.0") + "% / " + (accuracyFine * 100).ToString("0.0") + "%");
                            clipboardText += trainCombination + "\t" + testDirName + "\t" + accuracyCoarse + "\t" + accuracyFine + "\t" + numTrainImages + "\t" + numProcessed + Environment.NewLine;

                            try
                            {
                                Invoke(new MethodInvoker(delegate
                                {
                                    Clipboard.SetText(clipboardText);
                                }));
                            }
                            catch { Debug.WriteLine("Error: Could not write to clipboard"); }
                            File.WriteAllText("results.txt", clipboardText);
                        }
                    }
                }
                SetStatus("Done");
                SetProgress(0);
                File.WriteAllText("results.txt", clipboardText);
            });
        }

        private Dictionary<string, List<ImageTemplate>> GetSamplesFromImages(string[] dirs)
        {
            List<string> trainFiles = new List<string>();
            foreach (string trainingDir in dirs)
            {
                string[] files = Directory.GetFiles(trainingDir, "*.png");
                trainFiles.AddRange(files);
            }
            SetProgress(0);
            SetStatus("Loading Image Files");
            SetProgressMax(trainFiles.Count);

            var samples = new Dictionary<string, List<ImageTemplate>>();
            Worker worker = Worker.Default;

            Localization.Instance.Reset();
            foreach (string filename in trainFiles)
            {
                Match match = Regex.Match(Path.GetFileNameWithoutExtension(filename), @"([a-zA-Z]+)_([a-zA-Z]+)_(\d+)");
                string coarseLocation = match.Groups[1].Value;
                string fineLocation = match.Groups[2].Value;
                int index = int.Parse(match.Groups[3].Value);

                Bitmap bmp = (Bitmap)Bitmap.FromFile(filename);
                VideoFrame frame = new VideoFrame() { Image = new Image<Gray, byte>(bmp) };
                frame.ImageGPU = worker.Malloc<byte>(640 * 640);
                frame.ImageGPU.Scatter(frame.Image.Bytes);
                ImageTemplate template = new ImageTemplate(frame);
                template["frameIndex"] = index;
                template["Path"] = Path.GetFileNameWithoutExtension(filename);
                template["CoarseLocation"] = coarseLocation;
                template["FineLocation"] = fineLocation;
                if (!samples.ContainsKey(fineLocation)) samples.Add(fineLocation, new List<ImageTemplate>());
                samples[fineLocation].Add(template);

                ImageProcessing.ProcessTemplate(template, false);
                //Localization.Instance.AddTrainingExample(template, coarseLocation, fineLocation);

                IncrementProgress();
            }

            return samples;
        }

        private Dictionary<string, List<ImageTemplate>> GetRandomSamplesFromVideo(string dir, int numSamplesPerClass)
        {
            Worker worker = Worker.Default;

            List<string> jsonFiles = new List<string>(Directory.GetFiles(dir, "*.log"));
            Dictionary<string, List<ImageTemplate>> samples = new Dictionary<string, List<ImageTemplate>>();

            foreach (string fileName in jsonFiles)
            {
                SetStatus("Loading Log File: " + Path.GetFileNameWithoutExtension(fileName));
                SetProgress(0);
                List<Logging.LogEvent> events = Logging.ReadLog(fileName);
                events.Sort((a, b) => { return a.timestamp.CompareTo(b.timestamp); });
                SetStatus("Processing Log File: " + Path.GetFileNameWithoutExtension(fileName));
                SetProgressMax(events.Count);

                bool isTraining = false;

                // read the log and look for the autocapture training events, which will have labeled video frames in between
                // identify all video indices for which we can determine a label
                Dictionary<int, string> frameClasses = new Dictionary<int, string>();
                Dictionary<string, List<int>> sampleFrameIndices = new Dictionary<string, List<int>>();
                HashSet<int> isTrainingSample = new HashSet<int>();
                List<int> currIndices = new List<int>();
                string currClass = null;
                int mostRecentIndex = 0;
                foreach (Logging.LogEvent e in events)
                {
                    if (e.message == "start_autocapture_location")
                        isTraining = true;
                    else if (e.message == "stop_autocapture_location" || e.message == "External Stop Autocapture" || e.message == "touch_up_timeout")
                    {
                        isTraining = false;
                        if (currClass != null)
                        {
                            //foreach (int index in currIndices) frameClasses.Add(index, currClass);
                            if (!sampleFrameIndices.ContainsKey(currClass)) sampleFrameIndices.Add(currClass, new List<int>());
                            sampleFrameIndices[currClass].AddRange(currIndices);
                        }
                        currIndices.Clear();
                        currClass = null;
                    }
                    else if (e.message.StartsWith("Added template: "))
                    {
                        currClass = e.message.Replace("Added template: ", "").Replace(" ", "_");
                        isTrainingSample.Add(mostRecentIndex);
                    }
                    else if (e is Logging.VideoFrameEvent)
                    {
                        mostRecentIndex = (e as Logging.VideoFrameEvent).frameIndex;
                        if (isTraining) currIndices.Add(mostRecentIndex);
                    }
                    IncrementProgress();
                }

                // select random samples
                Random rand = new Random();
                foreach (string className in sampleFrameIndices.Keys)
                {
                    List<int> frames = sampleFrameIndices[className];
                    frames.Shuffle(rand.Next());
                    for (int i = 0; i < numSamplesPerClass; i++)
                        frameClasses.Add(frames[i], className);
                }

                SetStatus("Reading Video File");
                SetProgressMax(frameClasses.Count);
                SetProgress(0);

                // extract video frames
                string videoFileName = Path.ChangeExtension(fileName, "avi");
                VideoFileReader videoReader = new VideoFileReader();
                videoReader.Open(videoFileName);
                Dictionary<string, int> countsForClass = new Dictionary<string, int>();
                try
                {
                    bool hasFramesToRead = true;
                    int frameIndex = 0;
                    while (hasFramesToRead)
                    {
                        Bitmap frame = videoReader.ReadVideoFrame();
                        if (frame == null) hasFramesToRead = false;
                        else if (frameClasses.ContainsKey(frameIndex))
                        {
                            if (!countsForClass.ContainsKey(frameClasses[frameIndex])) countsForClass.Add(frameClasses[frameIndex], 0);
                            int templateIndex = countsForClass[frameClasses[frameIndex]]++;

                            string fineLocation = frameClasses[frameIndex].Split('_')[1];
                            string coarseLocation = frameClasses[frameIndex].Split('_')[0];

                            VideoFrame vf = new VideoFrame() { Image = new Image<Gray, byte>(frame) };
                            vf.ImageGPU = worker.Malloc<byte>(640 * 640);
                            vf.ImageGPU.Scatter(vf.Image.Bytes);
                            ImageTemplate template = new ImageTemplate(vf);
                            template["CoarseLocation"] = coarseLocation;
                            template["FineLocation"] = fineLocation;

                            ImageProcessing.ProcessTemplate(template, false);
                            //Localization.Instance.AddTrainingExample(template, coarseLocation, fineLocation);
                            if (!samples.ContainsKey(fineLocation)) samples.Add(fineLocation, new List<ImageTemplate>());
                            samples[fineLocation].Add(template);

                            vf.ImageGPU.Dispose();
                            vf.Image.Dispose();

                            IncrementProgress();
                        }
                        frameIndex++;
                    }
                }
                catch { }
                videoReader.Close();
            }

            return samples;
        }

        private void TrainFromSamples(Dictionary<string, List<ImageTemplate>> samples)
        {
            Localization.Instance.Reset();
            foreach (string fineLocation in samples.Keys)
            {
                string coarseLocation = coarseForFine[fineLocation];
                foreach (ImageTemplate template in samples[fineLocation])
                    Localization.Instance.AddTrainingExample(template, coarseLocation, fineLocation);
            }

            SetStatus("Training Classifier");
            Localization.Instance.Train();
        }

        private Tuple<float, float, int> TestLocalizationAccuracy(Dictionary<string, List<ImageTemplate>> test)
        {
            SetProgress(0);
            float numCorrectCoarse = 0, numCorrectFine = 0, numProcessed = 0;

            int numTestImages = 0;
            foreach (string fineLocation in test.Keys) numTestImages += test[fineLocation].Count;
            SetProgressMax(numTestImages);

            foreach (string fineLocation in test.Keys)
            {
                string coarseLocation = coarseForFine[fineLocation];
                foreach (ImageTemplate template in test[fineLocation])
                {
                    string predictedCoarse = Localization.Instance.PredictCoarseLocation(template);
                    string predictedFine = "";
                    bool foundFeatureMatch = false;
                    Dictionary<string, float> fineProbabilities = new Dictionary<string, float>();
                    predictedFine = Localization.Instance.PredictFineLocation(template, out foundFeatureMatch, out fineProbabilities, true, false, false, predictedCoarse);

                    if (predictedCoarse == coarseLocation) numCorrectCoarse++;
                    if (predictedFine == fineLocation) numCorrectFine++;

                    numProcessed++;
                }
            }

            float accuracyCoarse = numCorrectCoarse / numProcessed;
            float accuracyFine = numCorrectFine / numProcessed;
            Debug.WriteLine("Accuracy: " + (accuracyCoarse * 100).ToString("0.0") + "% / " + (accuracyFine * 100).ToString("0.0") + "%");
            //try
            //{
            //    Invoke(new MethodInvoker(delegate
            //    {
            //        Clipboard.SetText(accuracyCoarse + "\t" + accuracyFine + "\t" + numProcessed);
            //    }));
            //}
            //catch { Debug.WriteLine("Error: Could not write to clipboard"); }

            SetStatus("Done");
            SetProgress(0);

            return new Tuple<float, float, int>(accuracyCoarse, accuracyFine, (int)numProcessed);
        }

        private Tuple<float, float, int> TestLocalizationAccuracyWithVideoPredictions(string testDir, int smoothing)
        {
            Worker worker = Worker.Default;

            string[] jsonFiles = Directory.GetFiles(testDir, "*.log");
            SetProgress(0);
            float numCorrectCoarse = 0, numCorrectFine = 0, numProcessed = 0;

            foreach (string fileName in jsonFiles)
            {
                string shortFileName = Path.GetFileNameWithoutExtension(fileName);
                SetStatus("Reading JSON file: " + shortFileName);
                SetProgress(0);
                List<Logging.LogEvent> events = Logging.ReadLog(fileName);
                events.Sort((a, b) => { return a.timestamp.CompareTo(b.timestamp); });

                SetStatus("Processing Events: " + shortFileName);
                SetProgressMax(events.Count);

                Dictionary<int, string> trainingVideoIndicesAndClasses = new Dictionary<int, string>();
                int currVideoIndex = 0, lastTrainingFrame = 0;
                foreach (Logging.LogEvent e in events)
                {
                    if (e.message.StartsWith("Added template: "))
                    {
                        string currClass = e.message.Replace("Added template: ", "").Replace(" ", "_");
                        trainingVideoIndicesAndClasses.Add(currVideoIndex, currClass);
                        lastTrainingFrame = currVideoIndex;
                    }
                    else if (e is Logging.VideoFrameEvent)
                    {
                        currVideoIndex = (e as Logging.VideoFrameEvent).frameIndex;
                    }
                    IncrementProgress();
                }

                // extract video frames
                SetStatus("Processing Video File: " + shortFileName);
                SetProgress(0);
                SetProgressMax(lastTrainingFrame);
                string videoFileName = Path.ChangeExtension(fileName, "avi");
                VideoFileReader videoReader = new VideoFileReader();
                videoReader.Open(videoFileName);
                Queue<Bitmap> frameQueue = new Queue<Bitmap>();
                try
                {
                    bool hasFramesToRead = true;
                    int frameIndex = 0;
                    while (hasFramesToRead && frameIndex <= lastTrainingFrame)
                    {
                        Bitmap frame = videoReader.ReadVideoFrame();
                        if (frame == null) hasFramesToRead = false;
                        else
                        {
                            frameQueue.Enqueue(frame);
                            while (frameQueue.Count > smoothing) frameQueue.Dequeue();
                            if (trainingVideoIndicesAndClasses.ContainsKey(frameIndex))
                            {
                                string coarseClass = trainingVideoIndicesAndClasses[frameIndex].Split('_')[0];
                                string fineClass = trainingVideoIndicesAndClasses[frameIndex].Split('_')[1];
                                List<Dictionary<string, float>> coarseProbabilities = new List<Dictionary<string, float>>();
                                List<Dictionary<string, float>> fineProbabilities = new List<Dictionary<string, float>>();
                                List<string> coarsePredictions = new List<string>();
                                List<float> focusWeights = new List<float>();
                                string predictedCoarseLocation = "", predictedFineLocation = "";
                                Dictionary<string, float> totalProbabilities;
                                float maxProb = 0;
                                foreach (Bitmap bmp in frameQueue)
                                {
                                    VideoFrame tempFrame = new VideoFrame() { Image = new Image<Gray, byte>(bmp) };
                                    tempFrame.ImageGPU = worker.Malloc<byte>(640 * 640);
                                    tempFrame.ImageGPU.Scatter(tempFrame.Image.Bytes);
                                    ImageTemplate template = new ImageTemplate(tempFrame);

                                    ImageProcessing.ProcessTemplate(template, false);
                                    float focus = ImageProcessing.ImageFocus(template);
                                    focusWeights.Add(focus);

                                    Dictionary<string, float> frameCoarseProbabilities = new Dictionary<string, float>();
                                    predictedCoarseLocation = Localization.Instance.PredictCoarseLocation(template, out frameCoarseProbabilities);
                                    coarseProbabilities.Add(frameCoarseProbabilities);

                                    totalProbabilities = new Dictionary<string, float>();
                                    foreach (Dictionary<string, float> probabilities in coarseProbabilities)
                                    {
                                        foreach (string key in probabilities.Keys)
                                        {
                                            if (!totalProbabilities.ContainsKey(key)) totalProbabilities[key] = 0;
                                            totalProbabilities[key] += probabilities[key] / coarseProbabilities.Count;
                                        }
                                    }

                                    maxProb = 0;
                                    foreach (string key in totalProbabilities.Keys)
                                        if (totalProbabilities[key] > maxProb)
                                        {
                                            maxProb = totalProbabilities[key];
                                            predictedCoarseLocation = key;
                                        }
                                    coarsePredictions.Add(predictedCoarseLocation);

                                    bool foundFeatureMatch = false;
                                    Dictionary<string, float> frameFineProbabilities = new Dictionary<string, float>();
                                    predictedFineLocation = Localization.Instance.PredictFineLocation(template, out foundFeatureMatch, out frameFineProbabilities, true, false, false, predictedCoarseLocation);
                                    fineProbabilities.Add(frameFineProbabilities);
                                }

                                totalProbabilities = new Dictionary<string, float>();
                                Dictionary<string, float>[] tempCoarseProbabilities = coarseProbabilities.ToArray();
                                Dictionary<string, float>[] tempFineProbabilities = fineProbabilities.ToArray();
                                string[] tempCoarsePredictions = coarsePredictions.ToArray();
                                float[] tempFocusWeights = focusWeights.ToArray();
                                //foreach (Dictionary<string, float> probabilities in gestureCoarseLocationProbabilities)
                                for (int i = 0; i < tempCoarseProbabilities.Length; i++)
                                {
                                    Dictionary<string, float> probabilities = tempCoarseProbabilities[i];
                                    float weight = Math.Max(tempFocusWeights.Length > i ? tempFocusWeights[i] : 0.01f, 0.01f);
                                    foreach (string key in probabilities.Keys)
                                    {
                                        if (!totalProbabilities.ContainsKey(key)) totalProbabilities[key] = 0;
                                        totalProbabilities[key] += weight * probabilities[key] / coarseProbabilities.Count;
                                    }
                                }

                                maxProb = 0;
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
                                            totalProbabilities[key] += weight * probabilities[key] / coarseProbabilities.Count;
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

                                if (coarseLocation == coarseClass) numCorrectCoarse++;
                                if (fineLocation == fineClass) numCorrectFine++;
                                if (coarseLocation != coarseClass || fineLocation != fineClass) Debug.WriteLine(coarseLocation + " " + fineLocation + " != " + coarseClass + " " + fineClass);
                                numProcessed++;
                            }
                        }
                        frameIndex++;
                        IncrementProgress();
                    }
                }
                catch { }
                videoReader.Close();
            }

            float accuracyCoarse = numCorrectCoarse / numProcessed;
            float accuracyFine = numCorrectFine / numProcessed;
            Debug.WriteLine("Accuracy: " + (accuracyCoarse * 100).ToString("0.0") + "% / " + (accuracyFine * 100).ToString("0.0") + "%");
            //try
            //{
            //    Invoke(new MethodInvoker(delegate
            //    {
            //        Clipboard.SetText(accuracyCoarse + "\t" + accuracyFine + "\t" + numProcessed);
            //    }));
            //}
            //catch { Debug.WriteLine("Error: Could not write to clipboard"); }

            SetStatus("Done");
            SetProgress(0);

            return new Tuple<float, float, int>(accuracyCoarse, accuracyFine, (int)numProcessed);
        }

        private void TestLocalizationAccuracyWithVideoPredictions(string[] trainingDirs, string testDir, int smoothing)
        {
            string clipboardText = "";
            Task.Factory.StartNew(() =>
            {
                List<string> trainFiles = new List<string>();
                foreach (string trainingDir in trainingDirs)
                {
                    string[] files = Directory.GetFiles(trainingDir, "*.png");
                    trainFiles.AddRange(files);
                }
                SetProgress(0);
                SetStatus("Loading Training Files");
                SetProgressMax(trainFiles.Count);

                var samples = new Dictionary<string, List<ImageTemplate>>();
                Worker worker = Worker.Default;

                Localization.Instance.Reset();
                foreach (string filename in trainFiles)
                {
                    Match match = Regex.Match(Path.GetFileNameWithoutExtension(filename), @"([a-zA-Z]+)_([a-zA-Z]+)_(\d+)");
                    string coarseLocation = match.Groups[1].Value;
                    string fineLocation = match.Groups[2].Value;
                    int index = int.Parse(match.Groups[3].Value);

                    Bitmap bmp = (Bitmap)Bitmap.FromFile(filename);
                    VideoFrame frame = new VideoFrame() { Image = new Image<Gray, byte>(bmp) };
                    frame.ImageGPU = worker.Malloc<byte>(640 * 640);
                    frame.ImageGPU.Scatter(frame.Image.Bytes);
                    ImageTemplate template = new ImageTemplate(frame);
                    template["frameIndex"] = index;
                    template["Path"] = Path.GetFileNameWithoutExtension(filename);
                    template["CoarseLocation"] = coarseLocation;
                    template["FineLocation"] = fineLocation;
                    if (!samples.ContainsKey(fineLocation)) samples.Add(fineLocation, new List<ImageTemplate>());
                    samples[fineLocation].Add(template);

                    ImageProcessing.ProcessTemplate(template, false);
                    Localization.Instance.AddTrainingExample(template, coarseLocation, fineLocation);

                    IncrementProgress();
                }

                SetStatus("Training Classifier");
                Localization.Instance.Train();

                string[] jsonFiles = Directory.GetFiles(testDir, "*.log");
                SetProgress(0);
                float numCorrectCoarse = 0, numCorrectFine = 0, numProcessed = 0;

                foreach (string fileName in jsonFiles)
                {
                    string shortFileName = Path.GetFileNameWithoutExtension(fileName);
                    SetStatus("Reading JSON file: " + shortFileName);
                    SetProgress(0);
                    List<Logging.LogEvent> events = Logging.ReadLog(fileName);
                    events.Sort((a, b) => { return a.timestamp.CompareTo(b.timestamp); });

                    SetStatus("Processing Events: " + shortFileName);
                    SetProgressMax(events.Count);

                    Dictionary<int, string> trainingVideoIndicesAndClasses = new Dictionary<int, string>();
                    int currVideoIndex = 0, lastTrainingFrame = 0;
                    foreach (Logging.LogEvent e in events)
                    {
                        if (e.message.StartsWith("Added template: "))
                        {
                            string currClass = e.message.Replace("Added template: ", "").Replace(" ", "_");
                            trainingVideoIndicesAndClasses.Add(currVideoIndex, currClass);
                            lastTrainingFrame = currVideoIndex;
                        }
                        else if (e is Logging.VideoFrameEvent)
                        {
                            currVideoIndex = (e as Logging.VideoFrameEvent).frameIndex;
                        }
                        IncrementProgress();
                    }

                    // extract video frames
                    SetStatus("Processing Video File: " + shortFileName);
                    SetProgress(0);
                    SetProgressMax(lastTrainingFrame);
                    string videoFileName = Path.ChangeExtension(fileName, "avi");
                    VideoFileReader videoReader = new VideoFileReader();
                    videoReader.Open(videoFileName);
                    Queue<Bitmap> frameQueue = new Queue<Bitmap>();
                    try
                    {
                        bool hasFramesToRead = true;
                        int frameIndex = 0;
                        while (hasFramesToRead && frameIndex <= lastTrainingFrame)
                        {
                            Bitmap frame = videoReader.ReadVideoFrame();
                            if (frame == null) hasFramesToRead = false;
                            else
                            {
                                frameQueue.Enqueue(frame);
                                while (frameQueue.Count > smoothing) frameQueue.Dequeue();
                                if (trainingVideoIndicesAndClasses.ContainsKey(frameIndex))
                                {
                                    string coarseClass = trainingVideoIndicesAndClasses[frameIndex].Split('_')[0];
                                    string fineClass = trainingVideoIndicesAndClasses[frameIndex].Split('_')[1];
                                    List<Dictionary<string, float>> coarseProbabilities = new List<Dictionary<string, float>>();
                                    List<Dictionary<string, float>> fineProbabilities = new List<Dictionary<string, float>>();
                                    List<string> coarsePredictions = new List<string>();
                                    List<float> focusWeights = new List<float>();
                                    string predictedCoarseLocation = "", predictedFineLocation = "";
                                    Dictionary<string, float> totalProbabilities;
                                    float maxProb = 0;
                                    foreach(Bitmap bmp in frameQueue)
                                    {
                                        VideoFrame tempFrame = new VideoFrame() { Image = new Image<Gray, byte>(bmp) };
                                        tempFrame.ImageGPU = worker.Malloc<byte>(640 * 640);
                                        tempFrame.ImageGPU.Scatter(tempFrame.Image.Bytes);
                                        ImageTemplate template = new ImageTemplate(tempFrame);
                                        
                                        ImageProcessing.ProcessTemplate(template, false);
                                        float focus = ImageProcessing.ImageFocus(template);
                                        focusWeights.Add(focus);

                                        Dictionary<string, float> frameCoarseProbabilities = new Dictionary<string, float>();
                                        predictedCoarseLocation = Localization.Instance.PredictCoarseLocation(template, out frameCoarseProbabilities);
                                        coarseProbabilities.Add(frameCoarseProbabilities);

                                        totalProbabilities = new Dictionary<string, float>();
                                        foreach (Dictionary<string, float> probabilities in coarseProbabilities)
                                        {
                                            foreach (string key in probabilities.Keys)
                                            {
                                                if (!totalProbabilities.ContainsKey(key)) totalProbabilities[key] = 0;
                                                totalProbabilities[key] += probabilities[key] / coarseProbabilities.Count;
                                            }
                                        }

                                        maxProb = 0;
                                        foreach (string key in totalProbabilities.Keys)
                                            if (totalProbabilities[key] > maxProb)
                                            {
                                                maxProb = totalProbabilities[key];
                                                predictedCoarseLocation = key;
                                            }
                                        coarsePredictions.Add(predictedCoarseLocation);

                                        bool foundFeatureMatch = false;
                                        Dictionary<string, float> frameFineProbabilities = new Dictionary<string, float>();
                                        predictedFineLocation = Localization.Instance.PredictFineLocation(template, out foundFeatureMatch, out frameFineProbabilities, true, false, false, predictedCoarseLocation);
                                        fineProbabilities.Add(frameFineProbabilities);
                                    }

                                    totalProbabilities = new Dictionary<string, float>();
                                    Dictionary<string, float>[] tempCoarseProbabilities = coarseProbabilities.ToArray();
                                    Dictionary<string, float>[] tempFineProbabilities = fineProbabilities.ToArray();
                                    string[] tempCoarsePredictions = coarsePredictions.ToArray();
                                    float[] tempFocusWeights = focusWeights.ToArray();
                                    //foreach (Dictionary<string, float> probabilities in gestureCoarseLocationProbabilities)
                                    for (int i = 0; i < tempCoarseProbabilities.Length; i++)
                                    {
                                        Dictionary<string, float> probabilities = tempCoarseProbabilities[i];
                                        float weight = Math.Max(tempFocusWeights.Length > i ? tempFocusWeights[i] : 0.01f, 0.01f);
                                        foreach (string key in probabilities.Keys)
                                        {
                                            if (!totalProbabilities.ContainsKey(key)) totalProbabilities[key] = 0;
                                            totalProbabilities[key] += weight * probabilities[key] / coarseProbabilities.Count;
                                        }
                                    }

                                    maxProb = 0;
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
                                                totalProbabilities[key] += weight * probabilities[key] / coarseProbabilities.Count;
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

                                    if (coarseLocation == coarseClass) numCorrectCoarse++;
                                    if (fineLocation == fineClass) numCorrectFine++;
                                    if (coarseLocation != coarseClass || fineLocation != fineClass) Debug.WriteLine(coarseLocation + " " + fineLocation + " != " + coarseClass + " " + fineClass);
                                    numProcessed++;
                                }
                            }
                            frameIndex++;
                            IncrementProgress();                            
                        }
                    }
                    catch { }
                    videoReader.Close();
                }

                float accuracyCoarse = numCorrectCoarse / numProcessed;
                float accuracyFine = numCorrectFine / numProcessed;
                Debug.WriteLine("Accuracy: " + (accuracyCoarse * 100).ToString("0.0") + "% / " + (accuracyFine * 100).ToString("0.0") + "%");
                try
                {
                    Invoke(new MethodInvoker(delegate
                    {
                        Clipboard.SetText(accuracyCoarse + "\t" + accuracyFine + "\t" + numProcessed);
                    }));
                }
                catch { Debug.WriteLine("Error: Could not write to clipboard"); }

                SetStatus("Done");
                SetProgress(0);
            });
        }
    }
}
