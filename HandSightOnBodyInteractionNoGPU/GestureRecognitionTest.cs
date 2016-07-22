using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using HandSightLibrary;
using Emgu.CV;
using Emgu.CV.Structure;

namespace HandSightOnBodyInteractionNoGPU
{
    public partial class GestureRecognitionTest : Form
    {
        public enum READING_TYPE
        {
            ACCEL1_X, ACCEL1_Y, ACCEL1_Z, ACCEL2_X, ACCEL2_Y, ACCEL2_Z, GYRO1_X, GYRO1_Y, GYRO1_Z, GYRO2_X, GYRO2_Y, GYRO2_Z, MAG1_X, MAG1_Y, MAG1_Z, MAG2_X, MAG2_Y, MAG2_Z, IR1, IR2
        };

        public class TouchEvent
        {
            public int touchDown = 0;
            public int touchUp = 0;

            public TouchEvent(int start, int end)
            {
                touchDown = start;
                touchUp = end;
            }
        }

        public GestureRecognitionTest()
        {
            InitializeComponent();

            //UO

            /*
            for (int i = 1; i < 2; i++)
            {
                runAutoSegmentation(i);
            }*/

            Classifier classifier = new Classifier(Classifier.ClassifierType.SVM, Classifier.KernelType.Rbf);
            trainClassifier(1, 1, ref classifier);
            //classifier.Load("trainedClassifierP1");
            //testClassifier(1, 1, ref classifier);
        }

        private void trainClassifier(int pid, int groupID, ref Classifier classifier)
        {
            readToTrain(pid, groupID, ref classifier);
            //classifier.Save("trainedClassifierP1");
        }

        private void readToTrain(int pid, int groupID, ref Classifier classifier)
        {
            string path = "p" + pid + "_segIndices.csv";
            StreamReader indexReader = new StreamReader(path);

            while (!indexReader.EndOfStream)
            {
                String line = indexReader.ReadLine();
                string[] strArray = line.Split(',');
                String fileName = strArray[0];
                int startIdx = Int32.Parse(strArray[1]);
                int endIdx = Int32.Parse(strArray[2]);

                if (!fileName.EndsWith(groupID.ToString()))
                {
                    var directory = @"C:\Users\Uran\Desktop\PalmData\raw\p" + pid + "\\";
                    int verID = 1;
                    string path2 = directory + fileName;
                    string targetFile = path2 + ".json";
                    while (File.Exists(path2 + "_" + verID + ".json"))
                    {
                        targetFile = path2 + "_" + verID + ".json";
                        verID++;
                    }
                    List<Logging.LogEvent> allEvents = Logging.ReadLog(targetFile);
                    //List<Logging.SensorReadingEvent> logs = Logging.ReadEventLog(targetFile);
                    List<Logging.SensorReadingEvent> logs = new List<Logging.SensorReadingEvent>();
                    foreach (Logging.LogEvent logEvent in allEvents) if (logEvent is Logging.SensorReadingEvent) logs.Add((Logging.SensorReadingEvent)logEvent);
                    List<Sensors.Reading> stream = new List<Sensors.Reading>();
                    //Console.WriteLine("----" +fileName+": "+ startIdx + "," + endIdx + "," + logs.Count);
                    for (int i = startIdx; i <= endIdx; i++)
                    {
                        Logging.SensorReadingEvent log = (Logging.SensorReadingEvent)logs.ElementAt(i);
                        stream.Add(log.reading);
                    }

                    string[] classIDs = line.Split('_');

                    if (classIDs[1].Equals("Gesture"))
                    {
                        Console.WriteLine("Train: " + fileName);
                        float[] features = getFeatures(stream);
                        string classID = classIDs[2];
                        classifier.AddExample(classID, features);
                    }
                }
            }
            indexReader.Close();
            classifier.Train();

        }

        private void testClassifier(int pid, int groupID, ref Classifier classifier)
        {
            readToTest(pid, groupID, ref classifier);
        }

        private void readToTest(int pid, int groupID, ref Classifier classifier)
        {
            string path = "p" + pid + "_segIndices.csv";
            StreamReader indexReader = new StreamReader(path);

            while (!indexReader.EndOfStream)
            {
                String line = indexReader.ReadLine();
                string[] strArray = line.Split(',');
                String fileName = strArray[0];
                int startIdx = Int32.Parse(strArray[1]);
                int endIdx = Int32.Parse(strArray[2]);

                if (fileName.EndsWith(groupID.ToString()))
                {
                    var directory = @"C:\Users\Uran\Desktop\PalmData\raw\p" + pid + "\\";
                    //List<Logging.SensorReadingEvent> logs = Logging.ReadEventLog(directory + fileName + ".json");
                    List<Logging.LogEvent> allEvents = Logging.ReadLog(directory + fileName + ".json");
                    //List<Logging.SensorReadingEvent> logs = Logging.ReadEventLog(targetFile);
                    List<Logging.SensorReadingEvent> logs = new List<Logging.SensorReadingEvent>();
                    foreach (Logging.LogEvent logEvent in allEvents) if (logEvent is Logging.SensorReadingEvent) logs.Add((Logging.SensorReadingEvent)logEvent);
                    List<Sensors.Reading> stream = new List<Sensors.Reading>();

                    for (int i = startIdx; i <= endIdx; i++)
                    {
                        Logging.SensorReadingEvent log = (Logging.SensorReadingEvent)logs.ElementAt(i);
                        stream.Add(log.reading);
                    }

                    float[] features = getFeatures(stream);
                    string[] classIDs = line.Split('_');
                    if (classIDs[1].Equals("Gesture"))
                    {
                        Dictionary<string, float> result = new Dictionary<string, float>();
                        //int[] best = new int[2];
                        string predict = classifier.Predict(features, out result);
                        Console.WriteLine("Testing " + fileName + ", predicted as " + predict);
                    }
                }
            }
            indexReader.Close();
        }

        private float getCorrelation(List<float> list_x, List<float> list_y)
        {
            double sumX = 0;
            double sumX2 = 0;
            double sumY = 0;
            double sumY2 = 0;
            double sumXY = 0;

            int n = list_x.Count;

            float[] Xs = new float[n];
            float[] Ys = new float[n];

            list_x.CopyTo(Xs);
            list_y.CopyTo(Ys);

            for (int i = 0; i < n; ++i)
            {
                double x = Xs[i];
                double y = Ys[i];

                sumX += x;
                sumX2 += x * x;
                sumY += y;
                sumY2 += y * y;
                sumXY += x * y;
            }

            double stdX = Math.Sqrt(sumX2 / n - sumX * sumX / n / n);
            double stdY = Math.Sqrt(sumY2 / n - sumY * sumY / n / n);
            double covariance = (sumXY / n - sumX * sumY / n / n);

            return (float)(covariance / stdX / stdY);
        }

        private float getMedian(List<float> stream)
        {
            float[] arr = new float[stream.Count];
            stream.CopyTo(arr);
            Array.Sort(arr);

            return arr[arr.Length / 2];
        }

        private float getAbsAvg(List<float> stream)
        {
            float absAvg = 0;
            int count = stream.Count;
            for (int i = 0; i < count; i++)
            {
                absAvg += Math.Abs(stream.ElementAt(i));
            }

            return absAvg / count;
        }

        private void smoothing(ref List<float>[] stream)
        {
            int count = stream[0].Count;

            for (int idx = 0; idx < stream.Length; idx++)
            {
                float[] arr = new float[count];
                stream[idx].CopyTo(arr);

                Image<Gray, float> image = new Image<Gray, float>(arr.Length, 1);
                //Matrix<float> matrix = new Matrix<float>(arr.Length, 1);
                for (int i = 0; i < arr.Length; i++)
                {
                    //matrix.Data[i, 0] = arr[i];
                    image.Data[i, 0, 0] = arr[i];
                }
                //matrix.Data.CopyTo(image, 0);
                image = image.SmoothGaussian(51, 13, 2, 0);
                ImageConverter converter = new ImageConverter();
                arr = (float[])converter.ConvertTo(image, typeof(float[]));
                stream[idx] = arr.ToList();
            }
        }


        private List<float>[] lowPass(List<float>[] stream)
        {
            float ALPHA = 0.15f;
            int count = stream[0].Count;
            List<float>[] output = new List<float>[stream.Length];

            for (int idx = 0; idx < stream.Length; idx++)
            {
                float[] arr = new float[count];
                stream[idx].CopyTo(arr);
                for (int i = 0; i < arr.Length; i++)
                {
                    output[idx][i] = output[idx][i] + ALPHA * (arr[i] - output[idx][i]);
                }
            }
            return output;
        }

        private void movingAverage(int frameSize, ref List<float>[] stream)
        {
            int count = stream[0].Count;

            for (int idx = 0; idx < stream.Length; idx++)
            {
                float[] arr = new float[count];
                stream[idx].CopyTo(arr);
                float sum = 0;
                float[] avgPoints = new float[arr.Length - frameSize + 1];
                for (int counter = 0; counter <= arr.Length - frameSize; counter++)
                {
                    int innerLoopCounter = 0;
                    int index = counter;
                    while (innerLoopCounter < frameSize)
                    {
                        sum = sum + arr[index];

                        innerLoopCounter += 1;

                        index += 1;
                    }
                    avgPoints[counter] = sum / frameSize;
                    sum = 0;
                }
                stream[idx] = avgPoints.ToList();
            }
        }

        private void normalization(ref List<float>[] stream)
        {
            int count = stream[0].Count;

            for (int idx = 0; idx < stream.Length; idx++)
            {
                float[] arr = new float[count];
                stream[idx].CopyTo(arr);
                float avg = stream[idx].Average();
                float std = (float)Math.Sqrt(stream[idx].Average(v => Math.Pow(v - avg, 2)));

                for (int i = 0; i < count; i++)
                {
                    arr[i] = (arr[i] - avg) / std;
                }

                stream[idx] = arr.ToList();
            }
        }

        private void subsample(ref List<float> features, List<float>[] raw)
        {
            int samplePnt = 50;
            int len = raw[0].Count;
            int stepSize = len / (samplePnt + 1);

            for (int idx = 0; idx < raw.Length; idx++)
            {
                List<float> sampled = new List<float>();
                for (int i = 0; i < len; i += stepSize)
                {
                    sampled.Add(raw[idx].ElementAt(i));
                }

                features.AddRange(sampled.GetRange(0, samplePnt));
            }
        }


        private void addFeatures(ref List<float> features, List<float>[] raw)
        {
            int frameCnt = 50;
            int scale = 10;
            //To do. fix this
            int len = raw[0].Count;
            int stepSize = len / (frameCnt + 1);

            for (int idx = 0; idx < raw.Length; idx++)
            {
                for (int i = 0; i < len - 2 * stepSize; i += stepSize)
                {
                    //Console.WriteLine(i + "," + (i + 2 * frameCnt) + "," + len +"," + raw[idx].Count);
                    List<float> frame = new List<float>();
                    frame = raw[idx].GetRange(i, 2 * stepSize - 1);
                    features.Add(frame.Max() * scale);
                    features.Add(frame.Min() * scale);
                    features.Add(getMedian(frame) * scale);
                    features.Add(getAbsAvg(frame) * scale);
                    features.Add(frame.Average() * scale);
                }
            }
        }

        private float[] getFeatures(List<Sensors.Reading> stream)
        {
            List<float>[] readings = new List<float>[20];
            List<float> featureAll = new List<float>();



            int size = stream.Count;

            for (int i = 0; i < 20; i++)
            {
                readings[i] = new List<float>();
            }

            for (int i = 0; i < size; i++)
            {
                readings[(int)READING_TYPE.ACCEL1_X].Add(stream.ElementAt(i).Accelerometer1.X);
                readings[(int)READING_TYPE.ACCEL1_Y].Add(stream.ElementAt(i).Accelerometer1.Y);
                readings[(int)READING_TYPE.ACCEL1_Z].Add(stream.ElementAt(i).Accelerometer1.Z);
                readings[(int)READING_TYPE.ACCEL2_X].Add(stream.ElementAt(i).Accelerometer2.X);
                readings[(int)READING_TYPE.ACCEL2_Y].Add(stream.ElementAt(i).Accelerometer2.Y);
                readings[(int)READING_TYPE.ACCEL2_Z].Add(stream.ElementAt(i).Accelerometer2.Z);
                readings[(int)READING_TYPE.GYRO1_X].Add(stream.ElementAt(i).Gyroscope1.X);
                readings[(int)READING_TYPE.GYRO1_Y].Add(stream.ElementAt(i).Gyroscope1.Y);
                readings[(int)READING_TYPE.GYRO1_Z].Add(stream.ElementAt(i).Gyroscope1.Z);
                readings[(int)READING_TYPE.GYRO2_X].Add(stream.ElementAt(i).Gyroscope2.X);
                readings[(int)READING_TYPE.GYRO2_Y].Add(stream.ElementAt(i).Gyroscope2.Y);
                readings[(int)READING_TYPE.GYRO2_Z].Add(stream.ElementAt(i).Gyroscope2.Z);
                readings[(int)READING_TYPE.MAG1_X].Add(stream.ElementAt(i).Magnetometer1.X);
                readings[(int)READING_TYPE.MAG1_Y].Add(stream.ElementAt(i).Magnetometer1.Y);
                readings[(int)READING_TYPE.MAG1_Z].Add(stream.ElementAt(i).Magnetometer1.Z);
                readings[(int)READING_TYPE.MAG2_X].Add(stream.ElementAt(i).Magnetometer2.X);
                readings[(int)READING_TYPE.MAG2_Y].Add(stream.ElementAt(i).Magnetometer2.Y);
                readings[(int)READING_TYPE.MAG2_Z].Add(stream.ElementAt(i).Magnetometer2.Z);
                readings[(int)READING_TYPE.IR1].Add(stream.ElementAt(i).InfraredReflectance1);
                readings[(int)READING_TYPE.IR2].Add(stream.ElementAt(i).InfraredReflectance2);
            }

            //To do. Apply smoothing
            //smoothing(ref readings);
            //readings = lowPass(readings);
            movingAverage(10, ref readings);

            //Normalization
            normalization(ref readings);

            //Sampling 
            subsample(ref featureAll, readings);

            //Add features per frame            
            addFeatures(ref featureAll, readings);


            //Add correlation
            featureAll.Add(getCorrelation(readings[(int)READING_TYPE.ACCEL1_X], readings[(int)READING_TYPE.ACCEL1_Y]));
            featureAll.Add(getCorrelation(readings[(int)READING_TYPE.ACCEL1_Y], readings[(int)READING_TYPE.ACCEL1_Z]));
            featureAll.Add(getCorrelation(readings[(int)READING_TYPE.ACCEL1_Z], readings[(int)READING_TYPE.ACCEL1_X]));
            featureAll.Add(getCorrelation(readings[(int)READING_TYPE.ACCEL2_X], readings[(int)READING_TYPE.ACCEL2_Y]));
            featureAll.Add(getCorrelation(readings[(int)READING_TYPE.ACCEL2_Y], readings[(int)READING_TYPE.ACCEL2_Z]));
            featureAll.Add(getCorrelation(readings[(int)READING_TYPE.ACCEL2_Z], readings[(int)READING_TYPE.ACCEL2_X]));

            featureAll.Add(getCorrelation(readings[(int)READING_TYPE.GYRO1_X], readings[(int)READING_TYPE.GYRO1_Y]));
            featureAll.Add(getCorrelation(readings[(int)READING_TYPE.GYRO1_Y], readings[(int)READING_TYPE.GYRO1_Z]));
            featureAll.Add(getCorrelation(readings[(int)READING_TYPE.GYRO1_Z], readings[(int)READING_TYPE.GYRO1_X]));
            featureAll.Add(getCorrelation(readings[(int)READING_TYPE.GYRO2_X], readings[(int)READING_TYPE.GYRO2_Y]));
            featureAll.Add(getCorrelation(readings[(int)READING_TYPE.GYRO2_Y], readings[(int)READING_TYPE.GYRO2_Z]));
            featureAll.Add(getCorrelation(readings[(int)READING_TYPE.GYRO2_Z], readings[(int)READING_TYPE.GYRO2_X]));

            featureAll.Add(getCorrelation(readings[(int)READING_TYPE.MAG1_X], readings[(int)READING_TYPE.MAG1_Y]));
            featureAll.Add(getCorrelation(readings[(int)READING_TYPE.MAG1_Y], readings[(int)READING_TYPE.MAG1_Z]));
            featureAll.Add(getCorrelation(readings[(int)READING_TYPE.MAG1_Z], readings[(int)READING_TYPE.MAG1_X]));

            featureAll.Add(getCorrelation(readings[(int)READING_TYPE.MAG2_X], readings[(int)READING_TYPE.MAG2_Y]));
            featureAll.Add(getCorrelation(readings[(int)READING_TYPE.MAG2_Y], readings[(int)READING_TYPE.MAG2_Z]));
            featureAll.Add(getCorrelation(readings[(int)READING_TYPE.MAG2_Z], readings[(int)READING_TYPE.MAG2_X]));

            //To do. Add zero crossings

            return featureAll.ToArray();
        }

        private void runAutoSegmentation(int pid)
        {
            String[] types = { "Gesture", "Speed" };
            String[] gestures = { "Tap", "SwipeDown", "SwipeUp", "SwipeLeft", "SwipeRight", "Circle", "Triangle", "Square" };
            String[] locations = { "Palm", "Wrist", "Thigh" };
            String[] speeds = { "Fast", "Med", "Slow" };

            string path = "p" + pid + "_segIndices.csv";
            StreamWriter eventWriter = new StreamWriter(path);

            foreach (String type in types)
            {
                foreach (String gesture in gestures)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        String fileName = "p" + pid + "_" + type + "_";
                        if (type.Equals("Gesture"))
                        {
                            foreach (String location in locations)
                            {
                                fileName = "p" + pid + "_" + type + "_";
                                fileName += gesture + "_" + location + i;
                                segmentation(pid, fileName, eventWriter);
                            }
                        }
                        else
                        {
                            foreach (String speed in speeds)
                            {
                                fileName = "p" + pid + "_" + type + "_";
                                fileName += gesture + "_" + speed + i;
                                segmentation(pid, fileName, eventWriter);
                            }
                        }

                    }
                }
            }

            eventWriter.Flush();
            eventWriter.Close();
        }

        private void segmentation(int pid, String fileName, StreamWriter writer)
        {
            int windowSize = 50;
            double thresh = 0.95;
            int start = 0;
            int end = 0;

            var directory = @"C:\Users\Uran\Desktop\PalmData\raw\p" + pid + "\\";
            int verID = 1;
            string path = directory + fileName;
            string targetFile = path + ".json";
            while (File.Exists(path + "_" + verID + ".json"))
            {
                targetFile = path + "_" + verID + ".json";
                verID++;
            }
            //List<Logging.SensorReadingEvent> logs = Logging.ReadEventLog(targetFile);
            List<Logging.LogEvent> allEvents = Logging.ReadLog(targetFile);
            //List<Logging.SensorReadingEvent> logs = Logging.ReadEventLog(targetFile);
            List<Logging.SensorReadingEvent> logs = new List<Logging.SensorReadingEvent>();
            foreach (Logging.LogEvent logEvent in allEvents) if (logEvent is Logging.SensorReadingEvent) logs.Add((Logging.SensorReadingEvent)logEvent);
            int count = logs.Count;
            int index = 0;
            List<TouchEvent> touchEvent = new List<TouchEvent>();
            Queue<double> win1 = new Queue<double>();
            Queue<double> win2 = new Queue<double>();

            Boolean hasTouchDown = false;
            while (index < count)
            {
                while (!hasTouchDown && index < count)
                {
                    Logging.SensorReadingEvent log = (Logging.SensorReadingEvent)logs.ElementAt(index);
                    win1.Enqueue(log.reading.InfraredReflectance1);
                    win2.Enqueue(log.reading.InfraredReflectance2);
                    if (index >= windowSize)
                    {
                        if (win1.Average() < thresh || win2.Average() < thresh)
                        {
                            start = Math.Max(0, index - windowSize);
                            hasTouchDown = true;
                        }
                        win1.Dequeue();
                        win2.Dequeue();
                    }
                    index++;
                }

                while (hasTouchDown && index < count)
                {
                    Logging.SensorReadingEvent log = (Logging.SensorReadingEvent)logs.ElementAt(index);
                    win1.Enqueue(log.reading.InfraredReflectance1);
                    win2.Enqueue(log.reading.InfraredReflectance2);
                    if (index >= windowSize)
                    {
                        if (win1.Average() > thresh && win2.Average() > thresh)
                        {
                            end = index;
                            int segCount = touchEvent.Count;
                            if (segCount > 0)
                            {
                                if ((touchEvent.ElementAt(segCount - 1).touchUp > start) || (start - touchEvent.ElementAt(segCount - 1).touchUp < 2 * windowSize))
                                {
                                    touchEvent.ElementAt(segCount - 1).touchUp = end;
                                }
                                else
                                {
                                    touchEvent.Add(new TouchEvent(start, end));
                                }
                            }
                            else
                            {
                                touchEvent.Add(new TouchEvent(start, end));
                            }
                            hasTouchDown = false;
                        }
                        win1.Dequeue();
                        win2.Dequeue();
                    }
                    index++;
                }
            }

            writer.Write(fileName + ",");
            if (touchEvent.Count > 0)
            {
                for (int i = 0; i < touchEvent.Count; i++)
                {
                    writer.Write(touchEvent.ElementAt(i).touchDown + "," + touchEvent.ElementAt(i).touchUp + ",");
                    Console.WriteLine(fileName + "," + touchEvent.ElementAt(i).touchDown + "," + touchEvent.ElementAt(i).touchUp + "\n");
                }
            }
            else
            {
                writer.Write("Error");
            }
            writer.Write(Environment.NewLine);

        }
    }
}
