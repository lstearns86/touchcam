using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandSightLibrary
{
    public class TouchSegmentation
    {
        static float frameRate = 200;
        static Queue<float> timestamps = new Queue<float>();

        const int windowSize = 50;
        const float thresh = 0.95f;

        static Queue<Sensors.Reading> readings = new Queue<Sensors.Reading>();
        static Queue<float> irReadings1 = new Queue<float>(), irReadings2 = new Queue<float>();
        static float movingAverage1 = 0, movingAverage2 = 0;
        static bool touchDown = false;
        static float lastTouchUpEvent = 0;

        public delegate void TouchDownEventDelegate();
        public static event TouchDownEventDelegate TouchDownEvent;
        private static void OnTouchDown() { TouchDownEvent?.Invoke(); }

        public delegate void TouchUpEventDelegate(Queue<Sensors.Reading> readings);
        public static event TouchUpEventDelegate TouchUpEvent;
        private static void OnTouchUp(Queue<Sensors.Reading> readings) { TouchUpEvent?.Invoke(readings); }

        public static bool HasTouchedDown { get { return touchDown; } }

        public static void UpdateWithReading(Sensors.Reading reading)
        {
            FPS.Sensors.Update();
            frameRate = FPS.Sensors.Average;

            UpdateMovingAverages(reading);

            if(touchDown)
            {
                if(movingAverage1 > thresh && movingAverage2 > thresh)
                {
                    OnTouchUp(readings);
                    touchDown = false;
                }
            }
            else
            {
                if(movingAverage1 < thresh || movingAverage2 < thresh)
                {
                    OnTouchDown();
                    touchDown = true;
                    readings.Clear(); // TODO: check if last touch-up event was recent enough, and if so combine touch events
                    readings.Enqueue(reading);
                }
            }
        }

        static void UpdateMovingAverages(Sensors.Reading reading)
        {
            movingAverage1 = movingAverage1 * irReadings1.Count;
            while(irReadings1.Count > 0 && irReadings1.Count >= frameRate / 4)
            {
                float val = irReadings1.Dequeue();
                movingAverage1 -= val;
            }
            movingAverage1 += reading.InfraredReflectance1;
            irReadings1.Enqueue(reading.InfraredReflectance1);
            movingAverage1 /= irReadings1.Count;

            movingAverage2 = movingAverage2 * irReadings2.Count;
            while (irReadings2.Count > 0 && irReadings2.Count >= frameRate / 4)
            {
                float val = irReadings2.Dequeue();
                movingAverage2 -= val;
            }
            movingAverage2 += reading.InfraredReflectance2;
            irReadings2.Enqueue(reading.InfraredReflectance2);
            movingAverage2 /= irReadings2.Count;
        }
    }
}
