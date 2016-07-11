using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Emgu.CV;
using Emgu.CV.CvEnum;
//using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.Util;
//using Emgu.CV.UI;
using Emgu.CV.ML;
using Emgu.CV.ML.Structure;

using Accord.MachineLearning;
using Accord.MachineLearning.VectorMachines;
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Statistics.Kernels;
using Accord.Neuro.Learning;
using AForge.Neuro;
using Accord.Neuro;
using Accord.Math;
using System.Diagnostics;

namespace HandSightLibrary
{
    public class Classifier
    {
        public enum ClassifierType { SVM, NeuralNet, KMeans, BruteForce }
        public enum KernelType { Auto, Rbf, Chi2, Poly, Linear }

        private Dictionary<string, int> idForName = new Dictionary<string, int>();
        private List<string> nameForID = new List<string>();
        //private static int k = 2;

        private ClassifierType type;
        private KernelType kernelType;

        private Matrix<float> trainData;
        private Matrix<int> trainLabels;
        private Matrix<int> sampleImportance;
        private ActivationNetwork network;
        private KMeans kmeans;
        private Dictionary<int, Dictionary<int, int>> clusterClasses;
        private MulticlassSupportVectorMachine svm;

        public Classifier(ClassifierType type, KernelType kernelType = KernelType.Auto)
        {
            this.type = type;
            this.kernelType = kernelType;
        }

        public static Matrix<float> ArrayToMatrixRow(float[] features)
        {
            Matrix<float> data = new Matrix<float>(1, features.Length);
            for (int i = 0; i < features.Length; i++) data[0, i] = features[i];
            return data;
        }

        public void AddExample(string className, float[] features, bool important = false)
        {
            if(!idForName.ContainsKey(className))
            {
                idForName.Add(className, nameForID.Count);
                nameForID.Add(className);
            }

            int id = idForName[className];
            AddExample(id, features, important);
        }

        private void AddExample(int classID, float[] features, bool important = false)
        {
            Matrix<float> data = ArrayToMatrixRow(features);
            AddExample(classID, data, important);
        }

        public void AddExample(string className, Matrix<float> features, bool important = false)
        {
            if (!idForName.ContainsKey(className))
            {
                idForName.Add(className, nameForID.Count);
                nameForID.Add(className);
            }

            int id = idForName[className];
            AddExample(id, features, important);
        }

        private void AddExample(int classID, Matrix<float> features, bool important = false)
        {
            if (trainData == null)
            {
                trainData = features;
                trainLabels = new Matrix<int>(new int[,] { { classID } });
                sampleImportance = new Matrix<int>(new int[,] { { important ? 1 : 0 } });
                //nameCount[classID] = 1;
                //classIndex = new Matrix<int>(new int[,] { { 0 } });
            }
            else
            {
                trainData = trainData.ConcateVertical(features);
                trainLabels = trainLabels.ConcateVertical(new Matrix<int>(new int[,] { { classID } }));
                sampleImportance = sampleImportance.ConcateVertical(new Matrix<int>(new int[,] { { important ? 1 : 0 } }));
                //classIndex = classIndex.ConcateVertical(new Matrix<int>(new int[,] { { nameCount[classID] } }));
                //nameCount[classID]++;
            }
        }

        public bool Train()
        {
            if (type == ClassifierType.SVM)
            {
                float[,] mat = trainData.Data;
                double[][] inputs = new double[trainData.Rows][];
                for (int i = 0; i < trainData.Rows; i++)
                {
                    int numFeatures = Math.Max(trainData.Cols, 2);
                    inputs[i] = new double[numFeatures];
                    for (int j = 0; j < numFeatures; j++)
                        inputs[i][j] = mat[i, Math.Min(trainData.Cols - 1, j)];
                }
                int[] outputs = new int[trainLabels.Rows];
                for (int i = 0; i < trainLabels.Rows; i++)
                    outputs[i] = trainLabels[i, 0];

                int numClasses = nameForID.Count;

                if (numClasses <= 1) return true;

                IKernel kernel;
                switch (kernelType)
                {
                    case KernelType.Linear:
                        kernel = new Linear();
                        break;
                    case KernelType.Poly:
                        kernel = new Polynomial(3);
                        break;
                    case KernelType.Rbf:
                        kernel = Gaussian.Estimate(inputs, inputs.Length / 4);
                        break;
                    case KernelType.Chi2:
                        kernel = new ChiSquare();
                        break;
                    default:
                        kernel = inputs[0].Length > 20 ? (IKernel)(new ChiSquare()) : (IKernel)(Gaussian.Estimate(inputs, inputs.Length / 4));
                        break;
                }
                svm = new MulticlassSupportVectorMachine(inputs: inputs[0].Length, kernel: kernel, classes: numClasses);
                var teacher = new MulticlassSupportVectorLearning(svm, inputs, outputs)
                {
                    Algorithm = (machine, classInputs, classOutputs, i, j) => new SequentialMinimalOptimization(machine, classInputs, classOutputs)
                    {
                        Tolerance = 1e-6,
                        UseComplexityHeuristic = true
                        //Tolerance = 0.001,
                        //Complexity = 1,
                        //CacheSize = 200
                    }
                };
                try
                {
                    double error = teacher.Run();
                }
                catch (Exception ex) { Debug.WriteLine("Error training SVM: " + ex.Message); }

                teacher = new MulticlassSupportVectorLearning(svm, inputs, outputs)
                {
                    Algorithm = (machine, classInputs, classOutputs, i, j) => new ProbabilisticOutputCalibration(machine, classInputs, classOutputs)
                };
                try
                {
                    double error = teacher.Run();
                }
                catch (Exception ex) { Debug.WriteLine("Error calibrating SVM: " + ex.Message); }

                return true;

                //model = new SVM();
                ////model = new Boost();
                ////model = new KNearest();
                ////model.DefaultK = k;

                //model.SetKernel(SVM.SvmKernelType.Chi2);
                //model.Type = SVM.SvmType.NuSvc;
                ////model.C = 312.5;
                //model.Gamma = 0.0001; //0.50625;
                //model.Nu = 0.01;
                ////model.Degree = 3;
                ////model.Coef0 = 10;
                //model.TermCriteria = new MCvTermCriteria(10000, 1e-6);

                //TrainData data = new TrainData(trainData, Emgu.CV.ML.MlEnum.DataLayoutType.RowSample, trainLabels);
                //new TrainData(trainData, Emgu.CV.ML.MlEnum.DataLayoutType.RowSample, trainLabels, null, sampleImportance);
                ////bool trained = model.TrainAuto(data, 10);
                //bool trained = model.Train(data);
                //if (!trained) throw new Exception();

                //return trained;
            }
            else if (type == ClassifierType.NeuralNet)
            {
                float[,] mat = trainData.Data;
                double[][] inputs = new double[trainData.Rows][];
                List<int> randomOrder = new List<int>(trainData.Rows);
                for (int i = 0; i < trainData.Rows; i++) randomOrder.Add(i);
                randomOrder.Shuffle();
                for (int i = 0; i < trainData.Rows; i++)
                {
                    inputs[randomOrder[i]] = new double[trainData.Cols];
                    for (int j = 0; j < trainData.Cols; j++)
                        inputs[randomOrder[i]][j] = mat[i, j];
                }
                int[] classes = new int[trainLabels.Rows];
                for (int i = 0; i < trainLabels.Rows; i++)
                    classes[randomOrder[i]] = trainLabels[i, 0];
                
                // First we have to convert this problem into a way that  the neural
                // network can handle. The first step is to expand the classes into 
                // indicator vectors, where a 1 into a position signifies that this
                // position indicates the class the sample belongs to.
                // 
                double[][] outputs = Accord.Statistics.Tools.Expand(classes, -1, +1);

                // Create an activation function for the net
                var function = new BipolarSigmoidFunction();

                // Create an activation network with the function and
                //  N inputs, (M+N)/2 hidden neurons and M possible outputs:
                int N = inputs[0].Length;
                int M = nameForID.Count;
                network = new ActivationNetwork(function, N, (M + N) / 2, M);

                // Randomly initialize the network
                new NguyenWidrow(network).Randomize();

                // Teach the network using parallel Rprop:
                var teacher = new ParallelResilientBackpropagationLearning(network);

                double error = 1.0;
                int iter = 0;
                while (error > 1e-7 && iter < 100)
                {
                    //for (int iter = 0; iter < 10000 && error > 1e-5; iter++)
                    error = teacher.RunEpoch(inputs, outputs);
                    iter++;
                }

                return true;
            }
            else if (type == ClassifierType.KMeans)
            {
                //knModel = new KNearest();
                //knModel.DefaultK = 5;
                //TrainData data = new TrainData(trainData, Emgu.CV.ML.MlEnum.DataLayoutType.RowSample, trainLabels);
                //bool trained = knModel.Train(data);
                //return trained;

                List<double[]> rows = new List<double[]>();
                float[,] data = trainData.Data;
                for (int i = 0; i < trainData.Rows; i++)
                {
                    double[] row = new double[trainData.Cols];
                    for (int j = 0; j < trainData.Cols; j++) row[j] = data[i, j];
                    rows.Add(row);
                }
                double[][] points = rows.ToArray();

                kmeans = new Accord.MachineLearning.KMeans(nameForID.Count * 2);
                int[] labels = kmeans.Compute(points);
                clusterClasses = new Dictionary<int, Dictionary<int, int>>();
                for (int i = 0; i < labels.Length; i++)
                {
                    if (!clusterClasses.ContainsKey(labels[i])) clusterClasses.Add(labels[i], new Dictionary<int, int>());
                    int label = trainLabels.Data[i, 0];
                    if (!clusterClasses[labels[i]].ContainsKey(label)) clusterClasses[labels[i]].Add(label, 0);
                    clusterClasses[labels[i]][label]++;
                }

                return true;
            }
            else
            {
                return true;
            }
        }

        public string Predict(float[] features, out Dictionary<string, float> probabilities, int[] best = null)
        {
            Matrix<float> data = ArrayToMatrixRow(features);
            return Predict(data, out probabilities, best);
        }
        public string Predict(Matrix<float> data, out Dictionary<string, float> probabilities, int[] best = null)
        {
            float response = -1;
            probabilities = new Dictionary<string, float>();

            if (type == ClassifierType.SVM)
            {
                //if (model == null) return "null";

                ////Matrix<float> results = new Matrix<float>(nameForID.Count, nameForID.Count);

                //response = model.Predict(data);

                if (svm == null) return "null";

                if (nameForID.Count == 1) response = 0;
                else
                {
                    int numFeatures = Math.Max(data.Cols, 2);
                    double[] input = new double[numFeatures];
                    for (int i = 0; i < numFeatures; i++) input[i] = data[0, Math.Min(data.Cols - 1, i)];
                    double[] responses = new double[nameForID.Count];
                    response = svm.Compute(input, out responses);
                    for (int i = 0; i < responses.Length; i++) probabilities.Add(nameForID[i], (float)responses[i]);
                }
            }
            else if (type == ClassifierType.NeuralNet)
            {
                double[] input = new double[data.Cols];
                for (int i = 0; i < data.Cols; i++) input[i] = data[0, i];
                double[] fusion = network.Compute(input);
                int prediction; fusion.Max(out prediction);
                response = prediction;
            }
            else if (type == ClassifierType.KMeans)
            {
                //if (knModel == null) return "null";

                //Matrix<float> results = new Matrix<float>(k, 1);

                //response = knModel.Predict(data);

                //if (trainData == null) return "error: no training data";

                //List<Tuple<double, int, float>> results = new List<Tuple<double, int, float>>();
                //for (int i = 0; i < trainData.Rows; i++)
                //{
                //    double dist = CvInvoke.CompareHist(trainData.GetRow(i), data, HistogramCompMethod.Chisqr);
                //    results.Add(new Tuple<double, int, float>(dist, i, trainLabels[i, 0]));
                //}
                //results.Sort((Tuple<double, int, float> a, Tuple<double, int, float> b) => { return a.Item1.CompareTo(b.Item1); });
                //response = results[0].Item3;

                double[] dataRow = new double[data.Cols];
                for (int i = 0; i < dataRow.Length; i++) dataRow[i] = data[0, i];
                if (kmeans == null) return "null";
                List<Tuple<double, int>> clusterDistances = new List<Tuple<double, int>>();
                for (int i = 0; i < kmeans.Clusters.Count; i++)
                {
                    double dist = Utils.DistL2Sqr(kmeans.Clusters[i].Mean, dataRow);
                    clusterDistances.Add(new Tuple<double, int>(dist, i));
                }
                clusterDistances.Sort((Tuple<double, int> a, Tuple<double, int> b) => { return a.Item1.CompareTo(b.Item1); });
                int mostInstances = 0;
                int totalCount = 0;
                foreach (int classLabel in clusterClasses[clusterDistances[0].Item2].Keys)
                {
                    int instances = clusterClasses[clusterDistances[0].Item2][classLabel];
                    totalCount += instances;
                    if (instances > mostInstances)
                    {
                        mostInstances = instances;
                        response = classLabel;
                    }
                }
                //confidence = (float)mostInstances / (float)totalCount;
            }
            else
            {
                if (trainData == null) return "error: no training data";

                List<Tuple<double, int, float>> results = new List<Tuple<double, int, float>>();
                for (int i = 0; i < trainData.Rows; i++)
                {
                    double dist = CvInvoke.CompareHist(trainData.GetRow(i), data, HistogramCompMethod.Chisqr);
                    results.Add(new Tuple<double, int, float>(dist, i, trainLabels[i, 0]));
                }
                results.Sort((Tuple<double, int, float> a, Tuple<double, int, float> b) => { return a.Item1.CompareTo(b.Item1); });
                response = results[0].Item3;
                if (best != null)
                {
                    //HashSet<float> includedClasses = new HashSet<float>();
                    int n = Math.Min(results.Count, best.Length);
                    for (int i = 0; i < n; i++) best[i] = results[i].Item2;
                    //int index = 0;
                    //for (int i = 0; i < results.Count && includedClasses.Count < nameForID.Count; i++)
                    //{
                    //    float id = results[i].Item3;
                    //    if (includedClasses.Contains(id)) continue;
                    //    includedClasses.Add(id);
                    //    best[index++] = results[i].Item2;
                    //}
                }
            }
            
            int classID = (int)Math.Round(response);
            string className = (classID >= 0 && classID < trainLabels.Rows) ? nameForID[classID] : "no_match";
            return className;
        }

        //public void saveModel(string name){
        //    model.Save(name);
        //}

        //public void loadModel(string name)
        //{
        //    model = new SVM();
        //    //model.Load(name);
        //    model.Read((new FileStorage(name, FileStorage.Mode.Read)).GetFirstTopLevelNode());
        //}

        /*
        public Matrix<float> runSVM(Matrix<float> trainData, Matrix<float> label, int classNum, int trainID)
        {
            //init SVM
            model = new SVM();

            //copying
            Matrix<float> trainData2 = trainData.Clone();
            Matrix<float> label2 = label.Clone();

            //remove test data from the training data
            for (int i = 0; i < classNum; i++)
            {
                trainData2.RemoveRows(trainID + i * trainNum, (trainID + 1) + i * trainNum);
                label2.RemoveRows(trainID + i * trainNum, (trainID + 1) + i * trainNum);
            }

            //training data, labels, null, null, svm parameters, k-fold cross validation
            bool trained = model.TrainAuto(trainData2, label2, null, null, p.MCvSVMParams, 5);

            //testing
            Matrix<float> predicted = new Matrix<float>(trainNum, 1);
            for (int i = 0; i < classNum; i++)
            {
                //Console.WriteLine("Query from classID:" + i);
                Matrix<float> test = trainData.GetRows(trainID + i * trainNum, (trainID + 1) + i * trainNum, 1);
                int predictedClass = (int)model.Predict(test);
                predicted.GetRows(i, i + 1, 1).SetValue(predictedClass);
                //Console.WriteLine("Prediction:" + predictedClass);

                confusion[i][predictedClass]++;
            }

            return predicted;
        }*/


        //public void printResult()
        //{
        //    Console.WriteLine("Confusion Matrix:");
        //    for (int a = 0; a < nameForID.Count; a++)
        //    {
        //        for (int b = 0; b < nameForID.Count; b++)
        //        {
        //            Console.Write(confusion[a][b] + "|");
        //        }
        //        Console.Write("\n");
        //    }

        //    Console.WriteLine("Presion|Recall|F1-measure");
        //    int[] rowSum = new int[3];
        //    int[] colSum = new int[3];
        //    float[] precision = new float[3];
        //    float[] recall = new float[3];
        //    float[] f1 = new float[3];
        //    for (int a = 0; a < nameForID.Count; a++)
        //    {
        //        for (int b = 0; b < 3; b++)
        //        {
        //            rowSum[a] += confusion[a][b];
        //            colSum[b] += confusion[a][b];
        //        }
        //    }
        //    for (int a = 0; a < nameForID.Count; a++)
        //    {
        //        precision[a] = confusion[a][a] / rowSum[a];
        //        recall[a] = confusion[a][a] / colSum[a];
        //        f1[a] = 2 * precision[a] * recall[a] / (precision[a] + recall[a]);
        //        Console.WriteLine(precision[a] + "|" + recall[a] + "|" + f1[a]);
        //    }

        //}
    }
}
