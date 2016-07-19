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

using HandSightLibrary.ImageProcessing;

namespace HandSightOnBodyInteractionGPU
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

        private void SetProgressMax(int max)
        {
            Invoke(new MethodInvoker(delegate { Progress.Maximum = max; Application.DoEvents(); }));
        }

        private void SetProgress(int progress)
        {
            Invoke(new MethodInvoker(delegate { Progress.Value = progress; Application.DoEvents(); }));
        }

        private void IncrementProgress()
        {
            Invoke(new MethodInvoker(delegate { Progress.Increment(1); Application.DoEvents(); }));
        }

        private void SetStatus(string status)
        {
            Invoke(new MethodInvoker(delegate { StatusLabel.Text = status; Application.DoEvents(); }));
        }

        private void EvaluateOldData_Load(object sender, EventArgs e)
        {
            //string[] pids = new string[] { "p1", "p2", "p3", "p4", "p5", "p6", "p7", "new_p8", "p9", "p10", "p11", "p12", "p13", "p14", "p15", "p16", "p17", "p18", "p19", "p20", "p21", "p22", "p23", "p24" };
            string[] pids = new string[] { "p6" };
            string clipboardText = "";
            Task.Factory.StartNew(() =>
            {
                foreach (string pid in pids)
                {
                    string dir = @"D:\UserStudies\UIST2016\PalmData\" + pid + @"\processed";

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
                            predictedRegion = Localization.PredictRegion(query, true, true, false, predictedGroup);


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
    }
}
