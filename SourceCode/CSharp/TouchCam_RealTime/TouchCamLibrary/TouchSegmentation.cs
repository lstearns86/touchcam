using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchCamLibrary
{
    public class TouchSegmentation
    {
        static float frameRate = 200;
        static Queue<float> timestamps = new Queue<float>();

        const float thresh = 0.95f;

        static Queue<float> irReadings1 = new Queue<float>(), irReadings2 = new Queue<float>();
        static float movingAverage1 = 0, movingAverage2 = 0;
        static bool touchDown = false;
        
        public delegate void TouchDownEventDelegate();
        public static event TouchDownEventDelegate TouchDownEvent;
        private static void OnTouchDown() { TouchDownEvent?.Invoke(); }

        public delegate void TouchUpEventDelegate();
        public static event TouchUpEventDelegate TouchUpEvent;
        private static void OnTouchUp() { TouchUpEvent?.Invoke(); }

        public static bool HasTouchedDown { get { return touchDown; } }

        public static void Reset()
        {
            frameRate = 200;
            timestamps.Clear();
            irReadings1.Clear();
            irReadings2.Clear();
            movingAverage1 = 0;
            movingAverage2 = 0;
            touchDown = false;
        }

        public static bool UpdateWithReading(Sensors.Reading reading)
        {
            FPS.Sensors.Update();
            frameRate = FPS.Sensors.Average;

            UpdateMovingAverages(reading);

            if(touchDown)
            {
                if(movingAverage1 > thresh && movingAverage2 > thresh)
                {
                    OnTouchUp();
                    touchDown = false;
                }
            }
            else
            {
                if(movingAverage1 < thresh || movingAverage2 < thresh)
                {
                    OnTouchDown();
                    touchDown = true;
                }
            }

            return touchDown;
        }

        static float windowSize = 1.0f / 8.0f; // in seconds
        static void UpdateMovingAverages(Sensors.Reading reading)
        {
            movingAverage1 = movingAverage1 * irReadings1.Count;
            while(irReadings1.Count > 0 && irReadings1.Count >= frameRate * windowSize)
            {
                float val = irReadings1.Dequeue();
                movingAverage1 -= val;
            }
            movingAverage1 += reading.InfraredReflectance1;
            irReadings1.Enqueue(reading.InfraredReflectance1);
            movingAverage1 /= irReadings1.Count;

            movingAverage2 = movingAverage2 * irReadings2.Count;
            while (irReadings2.Count > 0 && irReadings2.Count >= frameRate * windowSize)
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
