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

using HandSightLibrary;
using HandSightLibrary.ImageProcessing;

using AForge.Video.FFMPEG;

namespace HandSightOnBodyInteractionGPU
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
            //EvaluateVideoPredictions(@"D:\UserStudies\UIST2016\PalmData\", new string[] { /*"p1", "p2", "p3", "p4",*/ "p5", "p6", "p7", "new_p8", "p9", "p10", "p11", "p12", "p13", "p14", "p15", "p16", "p17", "p18", "p19", "p20", "p21", "p22", "p23", "p24" }, "LocationVideoFrames");

            //ExtractVideoFrames2(@"D:\UserStudies\Ubicomp2017\", new string[] { "p01" }, "Logs");
            //ExtractVideoFrames2(@"D:\UserStudies\Ubicomp2017\", new string[] { "p01", "p02b", "p03", "p04", "p05", "p06", "p07", "p08", "p09", "p10", "p11", "p12" }, "Logs");

            //CrossValidate(@"D:\UserStudies\Ubicomp2017\", new string[] { "p09b" }, "Samples");
            //CrossValidate(@"D:\UserStudies\Ubicomp2017\", new string[] { "p01", "p02", "p03", "p04", "p05", "p06", "p07", "p08", "p09", "p10", "p11", "p12" }, "Samples");

            //EvaluateVideoPredictions2(@"D:\UserStudies\Ubicomp2017\", new string[] { "p01" }, "Samples", Path.Combine("Logs", "LocationVideoFrames"));
            //EvaluateVideoPredictions2(@"D:\UserStudies\Ubicomp2017\", new string[] { "p01", "p02", "p03", "p04", "p05", "p06", "p07", "p08", "p09", "p10", "p11", "p12" }, "Samples", Path.Combine("Logs", "LocationVideoFrames"));

            EvaluateFrameAccuracy(@"D:\UserStudies\Ubicomp2017\", new string[] { "p01" }, Path.Combine("Logs", "LocationVideoFrames"));
            //EvaluateFrameAccuracy(@"D:\UserStudies\Ubicomp2017\", new string[] { "p01", "p02b", "p03", "p04", "p05", "p07", "p08", "p09b", "p10", "p11", "p12" }, Path.Combine("Logs", "LocationVideoFrames"));
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

        private void CrossValidate(string rootDir, string[] pids, string subDir)
        {
            string clipboardText = "";
            Task.Factory.StartNew(() =>
            {
                foreach (string pid in pids)
                {
                    string dir = Path.Combine(rootDir, pid, subDir);

                    List<string> files = new List<string>(Directory.GetFiles(dir, "*.png"));

                    var samples = new Dictionary<string, List<ImageTemplate>>();

                    SetProgress(0);
                    SetStatus(pid + ": Loading and Preprocessing Files");
                    SetProgressMax(files.Count);

                    Worker worker = Worker.Default;

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
                        Localization.Instance.AddTrainingExample(template, coarseLocation, fineLocation);

                        IncrementProgress();
                    }
                    Localization.Instance.Train();

                    int numTemplates = 0;
                    foreach (string className in samples.Keys) numTemplates += samples[className].Count;

                    float numMatches = 0;
                    float numCorrectCoarse = 0, numCorrectFine = 0, numProcessedTemplates = 0;

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

                        numMatches++;

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

        private void EvaluateVideoPredictions(string rootDir, string[] pids, string subDir)
        {
            string clipboardText = "";
            int minVideoWindow = 1, maxVideoWindow = 30;
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
                    HandSightLibrary.Utils.Shuffle(indices);

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

                    List<string> results = new List<string>();

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

                    clipboardText += accuracyGroup + "\t" + accuracyRegion + "\t" + averageProcessingTime + "\t" + averageMatchTime + Environment.NewLine;

                    Invoke(new MethodInvoker(delegate
                    {
                        Clipboard.SetText(clipboardText);
                    }));
                }
                File.WriteAllText("results.txt", clipboardText);
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
    }
}
