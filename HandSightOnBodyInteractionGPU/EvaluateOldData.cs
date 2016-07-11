using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

using Alea.CUDA;

using Emgu.CV;
using Emgu.CV.Structure;

using HandSightLibrary.ImageProcessing;

namespace HandSightOnBodyInteractionRealTime
{
    public partial class EvaluateOldData : Form
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

        public EvaluateOldData()
        {
            InitializeComponent();
        }

        private void TempBridge_Load(object sender, EventArgs e)
        {
            string dir = @"D:\UserStudies\UIST2016\PalmData\p1\processed";

            List<string> files = new List<string>(Directory.GetFiles(dir, "*.png"));
            var samples = new Dictionary<string, List<ImageTemplate>>();

            Worker worker = Worker.Default;

            foreach (string filename in files)
            {
                string location = Regex.Replace(Path.GetFileNameWithoutExtension(filename), @"\d+", "");
                int index = int.Parse(Regex.Replace(Path.GetFileNameWithoutExtension(filename), @"[^\d]+", ""));
                if (location.ToLower().Contains("gesture")) continue;

                Bitmap bmp = (Bitmap)Bitmap.FromFile(filename);
                VideoFrame frame = new VideoFrame() { Image = new Image<Gray, byte>(bmp) };
                frame.ImageGPU = worker.Malloc<byte>(new byte[640 * 640]);
                frame.ImageGPU.Scatter(frame.Image.Bytes);
                ImageTemplate template = new ImageTemplate(frame);
                template["frameIndex"] = index;
                template["Path"] = Path.GetFileNameWithoutExtension(filename);
                if (!samples.ContainsKey(location)) samples.Add(location, new List<ImageTemplate>());
                samples[location].Add(template);

                template.Texture = LBP.GetUniformHistogram(frame);
            }

            int numTemplates = 0;
            foreach (string className in samples.Keys) numTemplates += samples[className].Count;

            float averageMatchTime = 0, numMatches = 0;
            float numCorrectGroup = 0, numCorrectRegion = 0, numProcessedTemplates = 0;

            List<string> results = new List<string>();

            //float progress = 0;
            Stopwatch watch = new Stopwatch();
            int n = 0; foreach (string className in samples.Keys) n = Math.Max(n, samples[className].Count);
            for (int queryIndex = 0; queryIndex < n; queryIndex++)
            {
                Localization.Reset();
                foreach (string region in samples.Keys)
                    for (int i = 0; i < samples[region].Count; i++)
                    {
                        if (i == queryIndex) continue;
                        Localization.AddTrainingExample(samples[region][i], groupForRegion[region], region);
                    }
                Localization.Train();

                foreach (string className in samples.Keys)
                {
                    if (!groupForRegion.ContainsKey(className)) continue;
                    if (queryIndex >= samples[className].Count) continue;
                    ImageTemplate query = samples[className][queryIndex];

                    DateTime start = DateTime.Now;
                    watch.Restart();

                    List<Tuple<string, float>> groupProbabilities;
                    string predictedGroup = Localization.PredictGroup(query, out groupProbabilities);
                    string predictedRegion = "";
                    predictedRegion = Localization.PredictRegion(query, true, false, false, predictedGroup);
                    

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
            }

            double accuracyGroup = numCorrectGroup / numProcessedTemplates;
            double accuracyRegion = numCorrectRegion / numProcessedTemplates;
            averageMatchTime /= numMatches;

            Debug.WriteLine("");
            Debug.WriteLine("Accuracy (Group): " + (accuracyGroup * 100) + "%");
            Debug.WriteLine("Accuracy (Region): " + (accuracyRegion * 100) + "%");
            Debug.WriteLine("Average Matching Time: " + averageMatchTime);
        }
    }
}
