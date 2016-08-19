using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace HandSightLibrary
{
    public class GestureRecognition
    {
        public enum READING_TYPE
        {
            //ACCEL1_X, ACCEL1_Y, ACCEL1_Z, ACCEL2_X, ACCEL2_Y, ACCEL2_Z, GYRO1_X, GYRO1_Y, GYRO1_Z, GYRO2_X, GYRO2_Y, GYRO2_Z, MAG1_X, MAG1_Y, MAG1_Z, MAG2_X, MAG2_Y, MAG2_Z, IR1, IR2
            ACCEL1_X, ACCEL1_Y, ACCEL1_Z, GYRO1_X, GYRO1_Y, GYRO1_Z, IR1, IR2
        };

        static Classifier classifier = new Classifier(Classifier.ClassifierType.SVM, Classifier.KernelType.Linear);
        public static Dictionary<string, List<Gesture>> samples = new Dictionary<string, List<Gesture>>();

        public static int GetNumExamples()
        {
            int count = 0;
            foreach (string key in samples.Keys)
                count += samples[key].Count;
            return count;
        }

        public static int GetNumClasses()
        {
            return samples.Count;
        }

        public static void Reset()
        {
            samples.Clear();
            classifier = new Classifier(Classifier.ClassifierType.SVM, Classifier.KernelType.Linear);;
        }

        public static void Save(string name, bool overwrite = true)
        {
            string dir = Path.Combine("savedProfiles", name);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            else
            {
                if(overwrite)
                    foreach (string filename in Directory.GetFiles(dir, "*.gest"))
                        File.Delete(filename);
            }

            foreach (string region in samples.Keys)
            {
                
                int i = 0;
                foreach (Gesture template in samples[region])
                {
                    //using (FileStream stream = new FileStream(Path.Combine(dir, region + "_" + i + ".gest"), FileMode.Create))
                    //{
                    //    stream.Write(BitConverter.GetBytes(template.Features.Length), 0, 4);
                    //    foreach (float v in template.Features)
                    //    {
                    //        stream.Write(BitConverter.GetBytes(v), 0, 4);
                    //    }
                    //    i++;
                    //}
                    string json = JsonConvert.SerializeObject(template);
                    File.WriteAllText(Path.Combine(dir, region + "_" + i + ".gest"), json);
                    i++;
                }
            }
        }

        public static void Load(string name)
        {
            string dir = Path.Combine("savedProfiles", name);
            if (!Directory.Exists(dir))
                return;

            foreach (string filename in Directory.GetFiles(dir, "*.gest"))
            {
                //float[] features = null;
                //using (FileStream stream = new FileStream(filename, FileMode.Open))
                //{
                //    byte[] buffer = new byte[stream.Length];
                //    try
                //    {
                //        stream.Read(buffer, 0, 4);
                //        int length = BitConverter.ToInt32(buffer, 0);
                //        features = new float[length];
                //        stream.Read(buffer, 0, 4 * length);
                //        for (int i = 0; i < length; i++)
                //            features[i] = BitConverter.ToSingle(buffer, i * 4);
                //    }
                //    catch { }
                //}

                //Gesture template = new Gesture();
                //Match match = Regex.Match(Path.GetFileNameWithoutExtension(filename), @"([a-zA-Z ]*)_\d+");
                //string className = match.Groups[1].Value;
                //template.ClassName = className;
                //template.Features = features;

                string json = File.ReadAllText(filename);
                Gesture template = JsonConvert.DeserializeObject<Gesture>(json);
                string className = template.ClassName;

                AddTrainingExample(template, className);
            }

            Train();
        }

        public static void PreprocessGesture(Gesture gesture, bool useSecondSensor = false)
        {
            Quaternion orientation1 = gesture.SensorReadings[0].Orientation1;
            Quaternion orientation2 = gesture.SensorReadings[0].Orientation2;
            foreach (Sensors.Reading reading in gesture.SensorReadings)
            {
                Sensors.Reading correctedReading = OrientationTracker.SubtractGravity(reading, useSecondSensor);
                correctedReading.Magnetometer1 = OrientationTracker.SubtractOrientation(correctedReading.Magnetometer1, orientation1);
                if (useSecondSensor) correctedReading.Magnetometer2 = OrientationTracker.SubtractOrientation(correctedReading.Magnetometer2, orientation2);
                gesture.CorrectedSensorReadings.Add(correctedReading);
            }

            gesture.Features = getFeatures(gesture.CorrectedSensorReadings);
            //gesture.Features = getFeatures(gesture.SensorReadings);
        }

        public static void AddTrainingExample(Gesture gesture, string gestureClass)
        {
            if (!samples.ContainsKey(gestureClass)) samples.Add(gestureClass, new List<Gesture>());
            samples[gestureClass].Add(gesture);
        }

        public static void RemoveTrainingExample(Gesture gesture)
        {
            string className = gesture.ClassName;
            samples[className].Remove(gesture);
            if (samples[className].Count == 0)
            {
                samples.Remove(className);
            }
        }

        public static void Train()
        {
            if (GetNumClasses() > 1)
            {
                foreach(string className in samples.Keys)
                    foreach(Gesture gesture in samples[className])
                        classifier.AddExample(className, gesture.Features);
                classifier.Train();
            }
        }

        public static string PredictGesture(Gesture gesture)
        {
            int numClasses = GetNumClasses();
            if (numClasses == 0)
                return "null";
            else if (numClasses == 1)
                return samples.Keys.ToArray()[0];
            else
            {
                Dictionary<string, float> probabilities = new Dictionary<string, float>();
                string className = classifier.Predict(gesture.Features, out probabilities);
                return className;
            }
        }

        #region Private Functions

        private static void movingAverage(int frameSize, ref List<float>[] stream)
        {
            int count = stream[0].Count;
            if (count < frameSize) return;

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

        private static void normalization(ref List<float>[] stream)
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

        private static void subsample(ref List<float>[] subsampled, List<float>[] raw)
        {
            int samplePnt = 50;
            int len = raw[0].Count;
            float stepSize = (float)len / (float)samplePnt;
            //len = stepSize * samplePnt;

            for (int idx = 0; idx < raw.Length; idx++)
            {
                List<float> sampled = new List<float>();
                for (float i = 0; Math.Ceiling(i) < len; i += stepSize)
                {
                    sampled.Add(raw[idx].ElementAt((int)Math.Floor(i)));
                }

                subsampled[idx] = sampled;
            }
        }

        private static void flatten(ref List<float> features, List<float>[] subsampled)
        {
            for (int idx = 0; idx < subsampled.Length; idx++)
            {
                features.AddRange(subsampled[idx]);
            }
        }

        private static void addFeatures(ref List<float> features, List<float>[] raw)
        {
            int frameCnt = 50;
            int scale = 10;
            //To do. fix this
            int len = raw[0].Count;
            float stepSize = (float)len / (float)frameCnt;
            //int stepSize = len / frameCnt;
            //len = stepSize * frameCnt;

            for (int idx = 0; idx < raw.Length; idx++)
            {
                for (float i = 0; Math.Ceiling(i) < len - 2 * stepSize; i += stepSize)
                {
                    //Console.WriteLine(i + "," + (i + 2 * frameCnt) + "," + len +"," + raw[idx].Count);
                    List<float> frame = new List<float>();
                    frame = raw[idx].GetRange((int)Math.Floor(i), (int)Math.Floor(2 * stepSize - 1));
                    features.Add(frame.Max() * scale);
                    features.Add(frame.Min() * scale);
                    features.Add(getMedian(frame) * scale);
                    features.Add(getAbsAvg(frame) * scale);
                    features.Add(frame.Average() * scale);
                }
            }
        }

        private static float getCorrelation(List<float> list_x, List<float> list_y)
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

        private static float getMedian(List<float> stream)
        {
            float[] arr = new float[stream.Count];
            stream.CopyTo(arr);
            Array.Sort(arr);

            return arr[arr.Length / 2];
        }

        private static float getAbsAvg(List<float> stream)
        {
            float absAvg = 0;
            int count = stream.Count;
            for (int i = 0; i < count; i++)
            {
                absAvg += Math.Abs(stream.ElementAt(i));
            }

            return absAvg / count;
        }

        private static float[] getFeatures(List<Sensors.Reading> stream)
        {
            int numChannels = Enum.GetNames(typeof(READING_TYPE)).Length;


            List<float>[] readings = new List<float>[numChannels];
            List<float>[] subsampledReadings = new List<float>[numChannels];
            List<float> featureAll = new List<float>();



            int size = stream.Count;

            for (int i = 0; i < numChannels; i++)
            {
                readings[i] = new List<float>();
                subsampledReadings[i] = new List<float>();
            }

            for (int i = 0; i < size; i++)
            {
                readings[(int)READING_TYPE.ACCEL1_X].Add(stream.ElementAt(i).Accelerometer1.X);
                readings[(int)READING_TYPE.ACCEL1_Y].Add(stream.ElementAt(i).Accelerometer1.Y);
                readings[(int)READING_TYPE.ACCEL1_Z].Add(stream.ElementAt(i).Accelerometer1.Z);
                //readings[(int)READING_TYPE.ACCEL2_X].Add(stream.ElementAt(i).Accelerometer2.X);
                //readings[(int)READING_TYPE.ACCEL2_Y].Add(stream.ElementAt(i).Accelerometer2.Y);
                //readings[(int)READING_TYPE.ACCEL2_Z].Add(stream.ElementAt(i).Accelerometer2.Z);
                readings[(int)READING_TYPE.GYRO1_X].Add(stream.ElementAt(i).Gyroscope1.X);
                readings[(int)READING_TYPE.GYRO1_Y].Add(stream.ElementAt(i).Gyroscope1.Y);
                readings[(int)READING_TYPE.GYRO1_Z].Add(stream.ElementAt(i).Gyroscope1.Z);
                //readings[(int)READING_TYPE.GYRO2_X].Add(stream.ElementAt(i).Gyroscope2.X);
                //readings[(int)READING_TYPE.GYRO2_Y].Add(stream.ElementAt(i).Gyroscope2.Y);
                //readings[(int)READING_TYPE.GYRO2_Z].Add(stream.ElementAt(i).Gyroscope2.Z);
                //readings[(int)READING_TYPE.MAG1_X].Add(stream.ElementAt(i).Magnetometer1.X);
                //readings[(int)READING_TYPE.MAG1_Y].Add(stream.ElementAt(i).Magnetometer1.Y);
                //readings[(int)READING_TYPE.MAG1_Z].Add(stream.ElementAt(i).Magnetometer1.Z);
                //readings[(int)READING_TYPE.MAG2_X].Add(stream.ElementAt(i).Magnetometer2.X);
                //readings[(int)READING_TYPE.MAG2_Y].Add(stream.ElementAt(i).Magnetometer2.Y);
                //readings[(int)READING_TYPE.MAG2_Z].Add(stream.ElementAt(i).Magnetometer2.Z);
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
            subsample(ref subsampledReadings, readings);
            flatten(ref featureAll, subsampledReadings);

            //Add features per frame            
            addFeatures(ref featureAll, readings);


            //Add correlation
            featureAll.Add(getCorrelation(readings[(int)READING_TYPE.ACCEL1_X], readings[(int)READING_TYPE.ACCEL1_Y]));
            featureAll.Add(getCorrelation(readings[(int)READING_TYPE.ACCEL1_Y], readings[(int)READING_TYPE.ACCEL1_Z]));
            featureAll.Add(getCorrelation(readings[(int)READING_TYPE.ACCEL1_Z], readings[(int)READING_TYPE.ACCEL1_X]));
            //featureAll.Add(getCorrelation(readings[(int)READING_TYPE.ACCEL2_X], readings[(int)READING_TYPE.ACCEL2_Y]));
            //featureAll.Add(getCorrelation(readings[(int)READING_TYPE.ACCEL2_Y], readings[(int)READING_TYPE.ACCEL2_Z]));
            //featureAll.Add(getCorrelation(readings[(int)READING_TYPE.ACCEL2_Z], readings[(int)READING_TYPE.ACCEL2_X]));

            featureAll.Add(getCorrelation(readings[(int)READING_TYPE.GYRO1_X], readings[(int)READING_TYPE.GYRO1_Y]));
            featureAll.Add(getCorrelation(readings[(int)READING_TYPE.GYRO1_Y], readings[(int)READING_TYPE.GYRO1_Z]));
            featureAll.Add(getCorrelation(readings[(int)READING_TYPE.GYRO1_Z], readings[(int)READING_TYPE.GYRO1_X]));
            //featureAll.Add(getCorrelation(readings[(int)READING_TYPE.GYRO2_X], readings[(int)READING_TYPE.GYRO2_Y]));
            //featureAll.Add(getCorrelation(readings[(int)READING_TYPE.GYRO2_Y], readings[(int)READING_TYPE.GYRO2_Z]));
            //featureAll.Add(getCorrelation(readings[(int)READING_TYPE.GYRO2_Z], readings[(int)READING_TYPE.GYRO2_X]));

            //featureAll.Add(getCorrelation(readings[(int)READING_TYPE.MAG1_X], readings[(int)READING_TYPE.MAG1_Y]));
            //featureAll.Add(getCorrelation(readings[(int)READING_TYPE.MAG1_Y], readings[(int)READING_TYPE.MAG1_Z]));
            //featureAll.Add(getCorrelation(readings[(int)READING_TYPE.MAG1_Z], readings[(int)READING_TYPE.MAG1_X]));

            //featureAll.Add(getCorrelation(readings[(int)READING_TYPE.MAG2_X], readings[(int)READING_TYPE.MAG2_Y]));
            //featureAll.Add(getCorrelation(readings[(int)READING_TYPE.MAG2_Y], readings[(int)READING_TYPE.MAG2_Z]));
            //featureAll.Add(getCorrelation(readings[(int)READING_TYPE.MAG2_Z], readings[(int)READING_TYPE.MAG2_X]));

            //To do. Add zero crossings

            return featureAll.ToArray();
        }

        #endregion
    }
}
