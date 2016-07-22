using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandSightLibrary
{
    class GestureRecognition
    {
        static Classifier classifier = null;
        static Dictionary<string, List<Gesture>> samples = new Dictionary<string, List<Gesture>>();

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
            classifier = null;
        }

        public static void Save(string name)
        {
            throw new NotImplementedException();
        }

        public static void Load(string name)
        {
            throw new NotImplementedException();
        }

        public static void PreprocessGesture(Gesture gesture)
        {
            throw new NotImplementedException();
        }

        public static void AddTrainingExample(Gesture gesture, string gestureClass)
        {
            throw new NotImplementedException();
        }

        public static void Train()
        {
            throw new NotImplementedException();
        }

        public static string PredictGesture(Gesture gesture)
        {
            throw new NotImplementedException();
        }
    }
}
