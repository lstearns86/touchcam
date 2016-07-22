using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.ML;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;



namespace HandSightLibrary.ImageProcessing
{
    public class Localization
    {
        static int numNeighbors = int.MaxValue;
        //static int numKDTrees = 4;
        //static int numFlannChecks = 48;
        //static int numClusters = 30;
        static int maxTemplateMatches = int.MaxValue;

        public static Dictionary<string, List<ImageTemplate>> samples = new Dictionary<string, List<ImageTemplate>>();
        static Classifier groupClassifier = null, groupClassifierSecondary = null;
        static Classifier sensorFusionClassifier = null;
        public static Dictionary<string, string> coarseLocations = new Dictionary<string,string>();
        public static Dictionary<string, HashSet<string>> groups = new Dictionary<string, HashSet<string>>();
        static Dictionary<string, List<Tuple<string, int>>> trainInfoByGroup = new Dictionary<string, List<Tuple<string, int>>>();
        static Dictionary<string, Classifier> bfClassifierByGroup = new Dictionary<string, Classifier>();
        static Dictionary<string, Classifier> classifierByGroup = new Dictionary<string, Classifier>();
        static Dictionary<string, Classifier> secondaryClassifierByGroup = new Dictionary<string, Classifier>();
        static Dictionary<string, Classifier> sensorFusionClassifierByGroup = new Dictionary<string, Classifier>();

        public static int GetNumTrainingExamples()
        {
            int count = 0;
            foreach(string key in samples.Keys)
                count += samples[key].Count;
            return count;
        }

        public static int GetNumTrainingClasses()
        {
            return samples.Count;
        }

        public static void Reset()
        {
            samples.Clear();
            groupClassifier = null;
            coarseLocations.Clear();
            groups.Clear();
        }

        public static void Save(string name)
        {
            string dir = Path.Combine("savedProfiles", name);
            if(!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            else
            {
                foreach (string filename in Directory.GetFiles(dir, "*.png"))
                    File.Delete(filename);
            }

            foreach(string region in samples.Keys)
            {
                int i = 0;
                foreach(ImageTemplate template in samples[region])
                {
                    template.Image.Save(Path.Combine(dir, coarseLocations[region] + "_" + region + "_" + i + ".png"));
                    i++;
                }
            }
        }

        public static void Load(string name)
        {
            string dir = Path.Combine("savedProfiles", name);
            if (!Directory.Exists(dir))
                return;

            foreach(string filename in Directory.GetFiles(dir, "*.png"))
            {
                Image<Gray, byte> img = new Image<Gray, byte>(filename);
                ImageTemplate template = new ImageTemplate(img);
                ImageProcessing.ProcessTemplate(template);
                Match match = Regex.Match(Path.GetFileNameWithoutExtension(filename), @"([a-zA-Z]*)_([a-zA-Z]*)_\d+");
                string group = match.Groups[1].Value;
                string region = match.Groups[2].Value;
                AddTrainingExample(template, group, region);
            }

            Train();
        }

        public static void AddTrainingExample(ImageTemplate template, string group, string region)
        {
            if (!samples.ContainsKey(region)) samples.Add(region, new List<ImageTemplate>());
            samples[region].Add(template);
            if (!coarseLocations.ContainsKey(region)) coarseLocations[region] = group;
            if (!groups.ContainsKey(group)) groups.Add(group, new HashSet<string>());
            if (!groups[group].Contains(region)) groups[group].Add(region);
        }

        public static void RemoveTrainingExample(ImageTemplate template)
        {
            string group = "", region = "";
            foreach(string tempRegion in samples.Keys)
                foreach(ImageTemplate tempTemplate in samples[tempRegion])
                    if(template == tempTemplate)
                    {
                        region = tempRegion;
                        group = coarseLocations[region];
                    }
            samples[region].Remove(template);
            if(samples[region].Count == 0)
            {
                samples.Remove(region);
                coarseLocations.Remove(region);
                groups[group].Remove(region);
            }
            if(groups[group].Count == 0)
            {
                groups.Remove(group);
            }
        }

        public static void Train(bool enableCamera = true, bool enableSecondary = false)
        {
            int numTrainingSamples = 0;
            if(enableCamera) groupClassifier = new Classifier(Classifier.ClassifierType.SVM);
            if(enableSecondary) groupClassifierSecondary = new Classifier(Classifier.ClassifierType.SVM);
            foreach (string region in samples.Keys)
            {
                if (!coarseLocations.ContainsKey(region)) continue;
                string group = coarseLocations[region];
                foreach (ImageTemplate template in samples[region])
                {
                    numTrainingSamples++;
                    if (enableCamera) groupClassifier.AddExample(group, template.TextureMatrixRow);
                    if (enableSecondary) groupClassifierSecondary.AddExample(group, template.SecondaryFeaturesMatrixRow);
                }
            }
            if (enableCamera) groupClassifier.Train();
            if (enableSecondary) groupClassifierSecondary.Train();

            // Train sensor fusion classifier
            if (enableCamera && enableSecondary)
            {
                //int numClasses = samples.Keys.Count;
                //float[] w = new float[numClasses];
                //for (int i = 0; i < numClasses; i++) w[i] = 0.0f;
                //int numMisclassifications = 0;

                sensorFusionClassifier = new Classifier(Classifier.ClassifierType.NeuralNet);
                foreach (string region in samples.Keys)
                {
                    if (!coarseLocations.ContainsKey(region)) continue;
                    string group = coarseLocations[region];
                    foreach (ImageTemplate template in samples[region])
                    {
                        Dictionary<string, float> textureProbabilities, secondaryProbabilities;
                        groupClassifier.Predict(template.TextureMatrixRow, out textureProbabilities);
                        groupClassifierSecondary.Predict(template.SecondaryFeaturesMatrixRow, out secondaryProbabilities);
                        float[] input = new float[textureProbabilities.Keys.Count + secondaryProbabilities.Keys.Count];
                        int j = 0;
                        foreach (string key in textureProbabilities.Keys) input[j++] = textureProbabilities[key];
                        foreach (string key in secondaryProbabilities.Keys) input[j++] = secondaryProbabilities[key];
                        sensorFusionClassifier.AddExample(group, input);
                    }
                }
                sensorFusionClassifier.Train();
            }

            if (enableCamera)
            {
                trainInfoByGroup.Clear();
                bfClassifierByGroup.Clear();
                classifierByGroup.Clear();
            }
            if (enableSecondary)
            {
                secondaryClassifierByGroup.Clear();
                sensorFusionClassifierByGroup.Clear();
            }
            foreach (string groupName in groups.Keys)
            {
                if (groups[groupName].Count == 1) continue;

                List<Tuple<string, int>> trainInfo = new List<Tuple<string, int>>();
                Classifier bfClassifier = new Classifier(Classifier.ClassifierType.BruteForce);
                Classifier classifier = new Classifier(Classifier.ClassifierType.SVM);
                Classifier secondaryClassifier = new Classifier(Classifier.ClassifierType.SVM);
                foreach (string region in groups[groupName])
                {
                    if (!coarseLocations.ContainsKey(region)) continue;
                    for (int i = 0; i < samples[region].Count; i++)
                    {
                        ImageTemplate template = samples[region][i];

                        Matrix<float> features = template.TextureMatrixRow;
                        Matrix<float> secondaryFeatures = template.SecondaryFeaturesMatrixRow;

                        if(enableCamera) bfClassifier.AddExample(region, features);
                        if (enableCamera) classifier.AddExample(region, features);
                        if (enableSecondary) secondaryClassifier.AddExample(region, secondaryFeatures);
                        if (enableCamera) trainInfo.Add(new Tuple<string, int>(region, i));
                    }
                }
                if (enableCamera) bfClassifier.Train();
                if (enableCamera) classifier.Train();
                if(enableSecondary) secondaryClassifier.Train();
                if (enableCamera) trainInfoByGroup.Add(groupName, trainInfo);
                if (enableCamera) bfClassifierByGroup.Add(groupName, bfClassifier);
                if (enableCamera) classifierByGroup.Add(groupName, classifier);
                if (enableSecondary) secondaryClassifierByGroup.Add(groupName, secondaryClassifier);
            }

            if (enableCamera && enableSecondary)
            {
                sensorFusionClassifierByGroup.Clear();
                foreach (string groupName in groups.Keys)
                {
                    if (groups[groupName].Count == 1) continue;

                    Classifier classifier = new Classifier(Classifier.ClassifierType.NeuralNet);
                    foreach (string region in groups[groupName])
                    {
                        if (!coarseLocations.ContainsKey(region)) continue;
                        for (int i = 0; i < samples[region].Count; i++)
                        {
                            ImageTemplate template = samples[region][i];
                            Dictionary<string, float> textureProbabilities, secondaryProbabilities;
                            classifierByGroup[groupName].Predict(template.TextureMatrixRow, out textureProbabilities);
                            secondaryClassifierByGroup[groupName].Predict(template.SecondaryFeaturesMatrixRow, out secondaryProbabilities);
                            float[] input = new float[textureProbabilities.Keys.Count + secondaryProbabilities.Keys.Count];
                            int j = 0;
                            foreach (string key in textureProbabilities.Keys) input[j++] = textureProbabilities[key];
                            foreach (string key in secondaryProbabilities.Keys) input[j++] = secondaryProbabilities[key];
                            classifier.AddExample(region, input);
                        }
                    }
                    classifier.Train();
                    sensorFusionClassifierByGroup.Add(groupName, classifier);
                }
            }
        }

        //private static float orientationWeight = 0.33f;
        public static string PredictCoarseLocation(ImageTemplate query, bool enableCamera = true, bool enableSecondary = false)
        {
            List<Tuple<string, float>> probabilities = null;
            return PredictCoarseLocation(query, out probabilities, enableCamera, enableSecondary);
        }
        public static string PredictCoarseLocation(ImageTemplate query, out List<Tuple<string, float>> probabilities, bool enableCamera = true, bool enableSecondary = false)
        {
            Dictionary<string, float> probabilitiesTexture = null, probabilitiesSecondary = null;
            string predictedGroupTexture = enableCamera ? groupClassifier.Predict(query.TextureMatrixRow, out probabilitiesTexture) : "disabled";
            string predictedGroup = samples.Count == 1 ? samples.Keys.First() : predictedGroupTexture;
            probabilities = null;
            if (enableSecondary)
            {
                string predictedGroupSecondary = groupClassifierSecondary.Predict(query.SecondaryFeaturesMatrixRow, out probabilitiesSecondary);
                if (!enableCamera) predictedGroup = predictedGroupSecondary;
                if (enableCamera && predictedGroupTexture != predictedGroupSecondary)
                {
                    float[] input = new float[probabilitiesTexture.Keys.Count + probabilitiesSecondary.Keys.Count];
                    int j = 0;
                    foreach (string key in probabilitiesTexture.Keys) input[j++] = probabilitiesTexture[key];
                    foreach (string key in probabilitiesSecondary.Keys) input[j++] = probabilitiesSecondary[key];
                    Dictionary<string, float> ignore = new Dictionary<string,float>();
                    predictedGroup = sensorFusionClassifier.Predict(input, out ignore);
                }

                //List<Tuple<string, float>> sortedProbabilitiesTexture = new List<Tuple<string, float>>();
                //foreach (string group in probabilitiesTexture.Keys) sortedProbabilitiesTexture.Add(new Tuple<string, float>(group, probabilitiesTexture[group]));
                //sortedProbabilitiesTexture.Sort((a, b) => { return -a.Item2.CompareTo(b.Item2); });

                //if (sortedProbabilitiesTexture[0].Item2 / sortedProbabilitiesTexture[1].Item2 < 2)
                //{
                //    string classA = sortedProbabilitiesTexture[0].Item1;
                //    string classB = sortedProbabilitiesTexture[1].Item1;
                //    float a = probabilitiesSecondary[classA];
                //    float b = probabilitiesSecondary[classB];
                //    predictedGroup = a > b ? classA : classB;
                //}

                //List<Tuple<string, float>> jointProbabilities = new List<Tuple<string, float>>();
                //foreach (string className in probabilitiesTexture.Keys) jointProbabilities.Add(new Tuple<string, float>(className, (1 - orientationWeight) * probabilitiesTexture[className] + orientationWeight * probabilitiesSecondary[className]));
                //jointProbabilities.Sort((a, b) => { return -a.Item2.CompareTo(b.Item2); });
                //predictedGroup = jointProbabilities[0].Item1;
                //probabilities = jointProbabilities;
            }
            return predictedGroup;
        }

        //private static bool useFeatureMatching = true;
        public static string PredictFineLocation(ImageTemplate query, bool enableCamera = true, bool enableGeometricVerification = true, bool enableSecondary = false, params string[] predictedGroup) { bool temp; return PredictFineLocation(query, out temp, enableCamera, enableGeometricVerification, enableSecondary, predictedGroup); }
        public static string PredictFineLocation(ImageTemplate query, out bool foundFeatureMatch, bool enableCamera = true, bool enableGeometricVerification = true, bool enableSecondary = false, params string[] predictedGroup)
        {
            if (predictedGroup.Length == 0) predictedGroup = new string[] { PredictCoarseLocation(query) };

            string predictedRegion = "no_match";
            string matchInfo = "";
            double minTextureDist = double.MaxValue;
            int mostFeatureMatches = 0;
            foundFeatureMatch = false;
            int numTemplatesCompared = 0;
            query["textureMatch"] = null;
            query["featureMatch"] = null;
            query["numTemplatesCompared"] = 0;
            query["numFeatureMatches"] = 0;
            foreach (string group in predictedGroup)
            {
                if (groups[group].Count == 1)
                {
                    ImageTemplate match = samples[groups[group].First<string>()][0];
                    query["textureMatch"] = match;
                    query["textureDist"] = -1;
                    predictedRegion = (string)match["Region"];
                    continue;
                }

                Classifier bfClassifier = enableCamera ? bfClassifierByGroup[group] : null;
                Classifier classifier = enableCamera ? classifierByGroup[group] : null;
                List<Tuple<string, int>> trainInfo = enableCamera ? trainInfoByGroup[group] : null;

                Matrix<float> features = query.TextureMatrixRow;

                int[] best = enableCamera ? new int[Math.Min(numNeighbors, trainInfo.Count)] : null;
                Dictionary<string, float> probabilities = null;
                string predictedRegionTemp = "disabled", predictedRegionTexture = "disabled", matchInfoTemp = "disabled";
                ImageTemplate textureMatch = null;
                double textureDist = double.MaxValue;
                if (enableCamera)
                {
                    predictedRegionTemp = bfClassifier.Predict(features, out probabilities, best);
                    predictedRegionTexture = classifier.Predict(features, out probabilities);
                    matchInfoTemp = trainInfo[best[0]].Item1 + (trainInfo[best[0]].Item2 + 1) + " (texture)";
                    textureMatch = samples[predictedRegionTemp][trainInfo[best[0]].Item2];
                    textureDist = CvInvoke.CompareHist(Classifier.ArrayToMatrixRow(textureMatch.Texture), Classifier.ArrayToMatrixRow(query.Texture), HistogramCompMethod.Chisqr);
                }

                Classifier secondaryClassifier;
                Dictionary<string, float> secondaryProbabilities = new Dictionary<string, float>();
                string predictedRegionSecondary = "disabled";
                if (enableSecondary)
                {
                    secondaryClassifier = secondaryClassifierByGroup[group];
                    predictedRegionSecondary = secondaryClassifier.Predict(query.SecondaryFeaturesMatrixRow, out secondaryProbabilities);
                    if (!enableCamera) predictedRegion = predictedRegionSecondary;
                }

                if (enableCamera && textureDist < minTextureDist)
                {
                    query["textureMatch"] = textureMatch;
                    query["textureDist"] = textureDist;
                    matchInfo = matchInfoTemp;
                    predictedRegion = predictedRegionTexture;
                    minTextureDist = textureDist;

                    if (enableSecondary)
                    {
                        Classifier sensorFusionClassifier = sensorFusionClassifierByGroup[group];
                        float[] input = new float[probabilities.Keys.Count + secondaryProbabilities.Keys.Count];
                        int j = 0;
                        foreach (string key in probabilities.Keys) input[j++] = probabilities[key];
                        foreach (string key in secondaryProbabilities.Keys) input[j++] = secondaryProbabilities[key];
                        predictedRegion = sensorFusionClassifier.Predict(input, out probabilities);
                    }
                }
                //predictedName = predictedName.Substring(0, predictedName.Length - 2);

                ImageTemplate featureMatch = null;
                int numFeatureMatches = 0;
                if (enableCamera && enableGeometricVerification)// && (predictedGroup == "finger" || predictedGroup == "palm"))
                {
                    if (query.Descriptors != null && query.Descriptors.Size.Height > 0)
                    {
                        int k = 2;
                        //Emgu.CV.Features2D.BFMatcher matcher = new BFMatcher(DistanceType.L2);
                        //matcher.Add(query.descriptors);
                        //Emgu.CV.Flann.Index matcher = new Emgu.CV.Flann.Index(query.Descriptors, new Emgu.CV.Flann.KdTreeIndexParamses(numKDTrees));
                        using (Emgu.CV.Cuda.CudaBFMatcher matcher = new Emgu.CV.Cuda.CudaBFMatcher(Emgu.CV.Features2D.DistanceType.L2))
                        {

                            int mostMatches = -1;
                            for (int index = 0; index < best.Length && index < maxTemplateMatches; index++)
                            //foreach (int bestIndex in best)
                            {
                                int bestIndex = best[index];
                                string testClass = trainInfo[bestIndex].Item1;
                                int testIndex = trainInfo[bestIndex].Item2;

                                ImageTemplate test = samples[testClass][testIndex];
                                VectorOfVectorOfDMatch matches = new VectorOfVectorOfDMatch();
                                if (test.Descriptors.Size.Height == 0) continue;
                                //matcher.KnnMatch(test.descriptors, matches, k, null);
                                //int numTestRows = test.Descriptors.Size.Height;
                                //Matrix<int> matchIndices = new Matrix<int>(numTestRows, k);
                                //Matrix<float> distances = new Matrix<float>(numTestRows, k);
                                //matcher.KnnSearch(test.Descriptors, matchIndices, distances, k, numFlannChecks);
                                matcher.KnnMatch(query.Descriptors, test.Descriptors, matches, k);
                                //for (int i = 0; i < numTestRows; i++)
                                //{
                                //    MDMatch[] row = new MDMatch[k];
                                //    for (int j = 0; j < k; j++)
                                //    {
                                //        MDMatch match = new MDMatch();
                                //        match.QueryIdx = i;
                                //        match.TrainIdx = matchIndices[i, j];
                                //        match.Distance = distances[i, j];
                                //        row[j] = match;
                                //    }
                                //    matches.Push(new VectorOfDMatch(row));
                                //}

                                Mat mask = new Mat(query.Descriptors.Size.Height, 1, DepthType.Cv8U, 1);
                                mask.SetTo(new MCvScalar(255));
                                //Features2DToolbox.VoteForUniqueness(matches, 0.8, mask);
                                //Features2DToolbox.VoteForSizeAndOrientation(test.KeypointVector, query.KeypointVector, matches, mask, 2, 45);
                                int count = CvInvoke.CountNonZero(mask);
                                Matrix<double> H = null;
                                //Mat H = null;
                                if (count > 4) H = GeometricVerification.FindHomographyOpenCV(test.KeypointVector, query.KeypointVector, matches, mask, 10);
                                //if (count > 4) H = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(test.KeypointVector, query.KeypointVector, matches, mask, 2);
                                //GC.GetTotalMemory(true);
                                count = CvInvoke.CountNonZero(mask);
                                bool good = (H != null && IsGoodHomography(H) && AreGoodInliers(query.KeypointVector, matches, mask));
                                //bool good = true;

                                if (good && count > mostMatches)
                                {
                                    predictedRegionTemp = testClass;
                                    foundFeatureMatch = true;
                                    mostMatches = count;
                                    matchInfoTemp = testClass + testIndex + " (" + count + " features)";
                                    featureMatch = test;
                                    numFeatureMatches = count;
                                    if (mostMatches > 16)
                                    {
                                        numTemplatesCompared += index + 1;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                if (numFeatureMatches > mostFeatureMatches)
                {
                    predictedRegion = predictedRegionTemp;
                    matchInfo = matchInfoTemp;
                    mostFeatureMatches = numFeatureMatches;
                    query["featureMatch"] = featureMatch;
                    query["numFeatureMatches"] = numFeatureMatches;
                    query["numTemplatesCompared"] = numTemplatesCompared;
                }
            }

            return predictedRegion;
        }

        static double homographyConstraint1 = 4;
        static double homographyConstraint2 = 0.001;
        static bool IsGoodHomography(Matrix<double> H)
        {
            double det = H[0, 0] * H[1, 1] - H[1, 0] * H[0, 1];
            if (det < 1.0 / homographyConstraint1) // limits scale and reflectivity
                return false;

            double N1 = Math.Sqrt(H[0, 0] * H[0, 0] + H[1, 0] * H[1, 0]);
            if (N1 > homographyConstraint1 || N1 < 1.0 / homographyConstraint1)
                return false;

            double N2 = Math.Sqrt(H[0, 1] * H[0, 1] + H[1, 1] * H[1, 1]);
            if (N2 > homographyConstraint1 || N2 < 1.0 / homographyConstraint1)
                return false;

            double N3 = Math.Sqrt(H[2, 0] * H[2, 0] + H[2, 1] * H[2, 1]);
            if (N3 > homographyConstraint2)
                return false;

            return true;
        }

        static bool AreGoodInliers(VectorOfKeyPoint f1, VectorOfVectorOfDMatch matches, Mat mask)
        {
            // Make sure points have sufficient spread and are not too collinear
            int count = CvInvoke.CountNonZero(mask);
            Image<Gray, byte> maskImg = mask.ToImage<Gray, byte>();

            // perform PCA to extract directions of principle variance
            Matrix<float> points = new Matrix<float>(count, 2);
            int index = 0;
            for (int i = 0; i < matches.Size; i++) { if (maskImg.Data[i, 0, 0] > 0) { points[index, 0] = f1[matches[i][0].QueryIdx].Point.X; points[index, 1] = f1[i].Point.Y; index++; } }
            Mat meanMat = new Mat();
            Matrix<float> eigenvectors = new Matrix<float>(2, 2);
            CvInvoke.PCACompute(points, meanMat, eigenvectors);
            Matrix<float> mean = new Matrix<float>(1, 2, meanMat.DataPointer);

            // compute variance along principle component directions
            float v1 = 0, v2 = 0;
            for (int i = 0; i < count; i++)
            {
                float x = points[i, 0] - mean[0, 0];
                float y = points[i, 1] - mean[0, 1];
                float c1 = eigenvectors[0, 0] * x + eigenvectors[0, 1] * y;
                float c2 = eigenvectors[1, 0] * x + eigenvectors[1, 1] * y;

                v1 += c1 * c1;
                v2 += c2 * c2;
            }
            v1 /= count;
            v2 /= count;
            float sd1 = (float)Math.Sqrt(v1);
            float sd2 = (float)Math.Sqrt(v2);

            // remove outliers
            v1 = 0; v2 = 0;
            int count1 = 0, count2 = 0;
            for (int i = 0; i < count; i++)
            {
                float x = points[i, 0] - mean[0, 0];
                float y = points[i, 1] - mean[0, 1];
                float c1 = eigenvectors[0, 0] * x + eigenvectors[0, 1] * y;
                float c2 = eigenvectors[1, 0] * x + eigenvectors[1, 1] * y;

                if (Math.Abs(c1) < 3 * sd1) { v1 += c1 * c1; count1++; }
                if (Math.Abs(c2) < 3 * sd2) { v2 += c2 * c2; count2++; }
            }
            v1 /= count1;
            v2 /= count2;
            sd1 = (float)Math.Sqrt(v1);
            sd2 = (float)Math.Sqrt(v2);

            // make sure the spread is large enough and that the points aren't too collinear
            if (sd1 < 25 || sd1 / sd2 > 4)
                return false;

            return true;
        }

        static void EvaluateMatch(string className, ImageTemplate template)
        {

        }
    }
}
