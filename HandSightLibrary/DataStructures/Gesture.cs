using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandSightLibrary
{
    class Gesture
    {
        List<Sensors.Reading> sensorReadings;
        float[] features;

        public List<Sensors.Reading> SensorReadings { get { return sensorReadings; } set { sensorReadings = value; } }
        public float[] Features { get { return features; } set { features = value; } }

        public Gesture()
        {

        }
    }
}
