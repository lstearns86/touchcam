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

        public Gesture(Sensors.Reading[] readings, string gestureName = "unknown", string gestureLocation = "unknown")
        {
            sensorReadings = new List<Sensors.Reading>(readings);
            correctedSensorReadings = new List<HandSightLibrary.Sensors.Reading>();
            features = null;
            this.className = gestureName;
            this.location = gestureLocation;
        }
    }
}
