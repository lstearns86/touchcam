using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandSightLibrary
{
    class TouchSegmentation
    {
        static List<Sensors.Reading> readings;

        public delegate void TouchDownEventDelegate();
        public static event TouchDownEventDelegate TouchDownEvent;
        private static void OnTouchDown() { TouchDownEvent?.Invoke(); }

        public delegate void TouchUpEventDelegate(List<Sensors.Reading> readings);
        public static event TouchUpEventDelegate TouchUpEvent;
        private static void OnTouchUp(List<Sensors.Reading> readings) { TouchUpEvent?.Invoke(readings); }

        public static void UpdateWithReading(Sensors.Reading reading)
        {
            // TODO: add reading to list, process to detect touch down or up events, trigger the appropriate event
            throw new NotImplementedException();
        }
    }
}
