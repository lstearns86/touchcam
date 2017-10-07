using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace TouchCamLibrary
{
    public class Gesture
    {
        public static Bitmap defaultVisualization = null;

        string className;
        string location;

        bool trainingData = false;
        [JsonIgnore] public bool IsTrainingData { get { return trainingData; } set { trainingData = value; } }

        List<Sensors.Reading> sensorReadings, correctedSensorReadings;
        float[] features;

        public string ClassName { get { return className; } set { className = value; } }
        public string Location { get { return location; } set { location = value; } }

        public List<Sensors.Reading> SensorReadings { get { return sensorReadings; } }
        public List<Sensors.Reading> CorrectedSensorReadings { get { return correctedSensorReadings; }  }
        public float[] Features { get { return features; } set { features = value; } }

        public bool DefaultGesture = false;

        public string Path = "";

        private Bitmap visualization = null;
        public Bitmap Visualization
        {
            get
            {
                if (visualization == null && trainingData) UpdateVisualization();
                return visualization;
            }
            set
            {
                visualization = value;
            }
        }

        public Gesture() : this(false) { }

        public Gesture(bool isTrainingData)
        {
            trainingData = isTrainingData;
            features = null;
            sensorReadings = new List<Sensors.Reading>();
            correctedSensorReadings = new List<Sensors.Reading>();
        }

        public Gesture(string gestureName = "unknown", string gestureLocation = "unknown", bool isTrainingData = false)
            : this(isTrainingData)
        {
            this.className = gestureName;
            this.location = gestureLocation;
        }

        public Gesture(Sensors.Reading[] readings, string gestureName = "unknown", string gestureLocation = "unknown", bool isTrainingData = false)
            : this(gestureName, gestureLocation, isTrainingData)
        {
            sensorReadings = new List<Sensors.Reading>(readings);
            correctedSensorReadings = new List<Sensors.Reading>();
        }

        public static void InitNoVisualization()
        {
            defaultVisualization = new Bitmap(640, 640);
            Graphics g = Graphics.FromImage(defaultVisualization);
            g.Clear(Color.White);
            g.DrawLine(Pens.Red, 0, 0, 640, 640);
            g.DrawLine(Pens.Red, 640, 0, 0, 640);
        }

        public void NoVisualization()
        {
            if(defaultVisualization == null)
            {
                InitNoVisualization();
            }

            visualization = defaultVisualization;
        }

        public void UpdateVisualization()
        {
            if (DefaultGesture) { NoVisualization(); return; }

            Pen irPen = new Pen(Brushes.Black, 9);
            Pen xPen = new Pen(Brushes.Red, 5);
            Pen yPen = new Pen(Brushes.Green, 5);
            Pen zPen = new Pen(Brushes.Blue, 5);

            float duration = correctedSensorReadings[correctedSensorReadings.Count - 1].Timestamp - correctedSensorReadings[0].Timestamp;
            duration /= 1000.0f;

            Bitmap img = new Bitmap(640, 640);
            Graphics g = Graphics.FromImage(img);
            //g.DrawLine(Pens.Black, 0, 320, 640, 320);
            Sensors.Reading prevReading = null;
            float prevX = 0;
            foreach (Sensors.Reading reading in correctedSensorReadings)
            {
                float x = img.Width * (float)(reading.Timestamp - correctedSensorReadings[0].Timestamp) / (duration * 1000);
                if (prevReading != null)
                {
                    // draw IR1
                    float y = img.Height * (1 - reading.InfraredReflectance1);
                    float prevY = img.Height * (1 - prevReading.InfraredReflectance1);
                    g.DrawLine(irPen, prevX, prevY, x, y);

                    // draw IR2
                    y = img.Height * (1 - reading.InfraredReflectance2);
                    prevY = img.Height * (1 - prevReading.InfraredReflectance2);
                    g.DrawLine(irPen, prevX, prevY, x, y);

                    // draw Accelerometer1.X
                    y = img.Height / 2 + img.Height / 2 * (-reading.Accelerometer1.X / 20);
                    prevY = img.Height / 2 + img.Height / 2 * (-prevReading.Accelerometer1.X / 20);
                    g.DrawLine(xPen, prevX, prevY, x, y);

                    // draw Accelerometer1.Y
                    y = img.Height / 2 + img.Height / 2 * (-reading.Accelerometer1.Y / 20);
                    prevY = img.Height / 2 + img.Height / 2 * (-prevReading.Accelerometer1.Y / 20);
                    g.DrawLine(yPen, prevX, prevY, x, y);

                    // draw Accelerometer1.Z
                    y = img.Height / 2 + img.Height / 2 * (-reading.Accelerometer1.Z / 20);
                    prevY = img.Height / 2 + img.Height / 2 * (-prevReading.Accelerometer1.Z / 20);
                    g.DrawLine(zPen, prevX, prevY, x, y);
                }
                prevReading = reading;
                prevX = x;
            }

            visualization = img;
        }
    }
}
