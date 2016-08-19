using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandSightLibrary
{
    public class Gesture
    {
        string className;
        string location;

        List<Sensors.Reading> sensorReadings, correctedSensorReadings;
        float[] features;

        public string ClassName { get { return className; } set { className = value; } }
        public string Location { get { return location; } set { location = value; } }

        public List<Sensors.Reading> SensorReadings { get { return sensorReadings; } }
        public List<Sensors.Reading> CorrectedSensorReadings { get { return correctedSensorReadings; }  }
        public float[] Features { get { return features; } set { features = value; } }

        public Gesture()
        {
            features = null;
            sensorReadings = new List<Sensors.Reading>();
            correctedSensorReadings = new List<Sensors.Reading>();
        }

        public Gesture(string gestureName = "unknown", string gestureLocation = "unknown")
            : this()
        {
            this.className = gestureName;
            this.location = gestureLocation;
        }

        public Gesture(Sensors.Reading[] readings, string gestureName = "unknown", string gestureLocation = "unknown")
            : this(gestureName, gestureLocation)
        {
            sensorReadings = new List<Sensors.Reading>(readings);
            correctedSensorReadings = new List<HandSightLibrary.Sensors.Reading>();
        }
    }
}
