using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

using AForge.Video.FFMPEG;
// Note: for video compression, I recommend the x264vfw library
// http://sourceforge.net/projects/x264vfw/
// (This doesn't apply anymore, but I'm leaving this comment here 
//  because the link might become useful in the future.)

using Newtonsoft.Json;
//"Tools" > "NuGet Package Manager" > "Package Manager Console", then type the following:
//Install-Package Newtonsoft.Json

namespace HandSightLibrary
{
    public class Logging
    {
        #region Log Events
        // objects to hold the log events for serialization
        public class LogEvent
        {
            public string type;
            public float timestamp;
            public string message;
            //public string videoFile;

            public LogEvent() : this("") { }
            public LogEvent(string message)
            {
                this.message = message;
                type = "generic";
                timestamp = 1000.0f * (float)stopwatch.ElapsedTicks / (float)Stopwatch.Frequency;
            }

            public string ToJson()
            {
                return JsonConvert.SerializeObject(this);
            }
        }
        public class VideoFrameEvent : LogEvent
        {
            public int frameIndex;
            
            public VideoFrameEvent(int frameIndex) 
                : base()
            {
                this.frameIndex = frameIndex;
                type = "video_frame";
            }
        }
        public class FrameProcessedEvent : LogEvent
        {
            public FrameProcessedEvent()
            {
                this.message = "frame_processed";
                type = "frame_processed";
            }
        }
        public class SensorReadingEvent : LogEvent
        {
            public Sensors.Reading reading;

            public SensorReadingEvent(Sensors.Reading reading) 
                : base()
            {
                this.reading = reading;
                type = "sensor_reading";
            }
        }
        public class LocationEvent : LogEvent
        {
            public string predictedLocation;
            public string smoothedLocation;
            public float likelihood;

            public LocationEvent(string predictedLocation, string smoothedLocation = "", float likelihood = 0)
                : base()
            {
                this.predictedLocation = predictedLocation;
                this.smoothedLocation = smoothedLocation;
                this.likelihood = 0;
                type = "location";
            }
        }

        public class GestureEvent : LogEvent
        {
            public Gesture gesture;
            public float duration;
            public string predictedGesture;
            public string predictedLocation;

            public GestureEvent(string predictedGesture, string predictedLocation, Gesture gesture)
                : base()
            {
                this.predictedGesture = predictedGesture;
                this.predictedLocation = predictedLocation;
                this.gesture = gesture;
                type = "gesture";
            }
        }

        public class AudioEvent : LogEvent
        {
            public string speechOutput;
            public string audioCue;

            public AudioEvent(string text, bool isSpeech)
            {
                if (isSpeech) this.speechOutput = text;
                else this.audioCue = text;
                type = "audio";
            }
        }

        public class TrainingEvent : LogEvent
        {
            public TrainingEvent(string message)
            {
                this.message = message;
                type = "training";
            }
        }

        public class UIEvent : LogEvent
        {
            public string action;

            public UIEvent(string action)
            {
                this.action = action;
                type = "user_interface";
            }
        }

        public class MenuEvent : LogEvent
        {
            public string menu;
            public int menuIndex;
            public string item;

            public MenuEvent(string menu, int menuIndex, string item)
            {
                this.menu = menu;
                this.menuIndex = menuIndex;
                this.item = item;
                type = "menu";
            }
        }

        public class HardwareEvent : LogEvent
        {
            public string component;

            public HardwareEvent(string component, string message)
            {
                this.component = component;
                this.message = message;
                type = "hardware";
            }
        }

        public class TouchEvent : LogEvent
        {
            public TouchEvent(string message)
            {
                this.message = message;
                type = "touch";
            }
        }

        #endregion

        #region Constants and Variables

        // constants
        const int defaultBitrate = 10 * 1024 * 1024;

        // internal variables
        static bool loggingVideo = false, loggingEvents = false, shouldEndLogging = false;
        static Size videoSize = new Size(640, 640);
        static string filename = "";
        static VideoFileWriter videoWriter;
        static BlockingCollection<Bitmap> videoQueue = new BlockingCollection<Bitmap>();
        static BlockingCollection<LogEvent> eventQueue = new BlockingCollection<LogEvent>();
        static Task videoTask = null, eventTask = null;
        static int videoFrameIndex;
        static Stopwatch stopwatch = new Stopwatch();
        static StreamWriter eventWriter;
        static int index = 0;

        // external properties
        public static bool Running { get { return loggingVideo || loggingEvents; } }
        public static Size VideoSize { get { return videoSize; } set { if (!Running) videoSize = value; } }
        public static string Filename { get { return filename; } set { if (!Running) { if (filename != value) { index = 0; } filename = value; } } }
        public static int Index { get { return index; } set { if (!Running) index = value; } }

        #endregion

        static void StartVideo()
        {
            string path = Path.ChangeExtension(filename, "avi"); //+ Index + ".avi";
            
            videoWriter = new VideoFileWriter();
            videoWriter.Open(path, videoSize.Width, videoSize.Height, 60, VideoCodec.MPEG4, defaultBitrate);

            loggingVideo = true;

            // start the background thread to write incoming frames
            videoTask = Task.Factory.StartNew(() =>
            {
                Debug.WriteLine("Started Writing to Video");
                while (loggingVideo)
                {
                    try
                    {
                        Bitmap frame;
                        bool success = videoQueue.TryTake(out frame, 10);
                        if (success)
                            videoWriter.WriteVideoFrame(frame);
                        if (shouldEndLogging && videoQueue.Count == 0) loggingVideo = false;
                    }
                    catch (AccessViolationException) { }
                }
                if (videoWriter != null) videoWriter.Dispose();
                Debug.WriteLine("Ended Writing to Video");
            });
        }

        static void StartEvents()
        {
            string path = filename;// + Index + ".json";
            eventWriter = new StreamWriter(path);
            
            // TODO: write additional header info (e.g., gesture, pid, etc.)
            eventWriter.WriteLine("{\"log_id\": \"" + Path.GetFileNameWithoutExtension(filename) + Index + "\", \"event_array\": [");
            eventWriter.Write("{\"type\": \"generic\", \"message\": \"start_logging\", \"timestamp\": " + (1000.0f * (float)stopwatch.ElapsedTicks / (float)Stopwatch.Frequency) + "}");

            // start the background thread to write incoming events
            loggingEvents = true;
            eventTask = Task.Factory.StartNew(() =>
            {
                Debug.WriteLine("Started Writing to JSON");
                while (loggingEvents)
                {
                    try
                    {
                        LogEvent eventItem;
                        bool success = eventQueue.TryTake(out eventItem, 10);
                        if (success)
                        {
                            string line = JsonConvert.SerializeObject(eventItem);
                            eventWriter.Write("," + Environment.NewLine + line);
                        }
                        if (shouldEndLogging && eventQueue.Count == 0) loggingEvents = false;
                    }
                    catch (AccessViolationException) { }
                }
                eventWriter.WriteLine(Environment.NewLine + "]}");
                eventWriter.Flush();
                eventWriter.Close();
                Debug.WriteLine("Ended Writing to JSON");
            });
        }

        public static void Start(string newFilename = null)
        {
            if (Running) return;

            videoFrameIndex = 0;
            if (newFilename != null) filename = newFilename;
            videoQueue = new BlockingCollection<Bitmap>();
            eventQueue = new BlockingCollection<LogEvent>();
            shouldEndLogging = false;
            stopwatch.Restart();
            StartVideo();
            StartEvents();
        }

        public static void Stop()
        {
            if (!Running) return;

            shouldEndLogging = true;
            videoTask.Wait();
            eventTask.Wait();
        }

        public static void LogVideoFrame(Bitmap frame, bool isCloned = false)
        {
            if (!Running) return;

            // Need to clone the frame in order to avoid multithreading errors.
            // You can either do it here or when supplying it (in which case you 
            // should pass isCloned = true to avoid duplicate cloning).
            int index = Interlocked.Increment(ref videoFrameIndex);
            Bitmap frameToWrite = frame;
            if (!isCloned) frameToWrite = (Bitmap)frame.Clone();
            videoQueue.Add(frameToWrite);
            eventQueue.Add(new VideoFrameEvent(index));
        }

        public static void LogSensorReading(Sensors.Reading reading)
        {
            if (!Running) return;

            eventQueue.Add(new SensorReadingEvent(reading));
        }

        public static void LogLocationEvent(string predictedLocation, string smoothedLocation = "", float likelihood = 0)
        {
            if (!Running) return;

            eventQueue.Add(new LocationEvent(predictedLocation, smoothedLocation, likelihood));
        }

        public static void LogGestureEvent(string predictedGesture, string predictedLocation = "", Gesture gesture = null)
        {
            if (!Running) return;

            eventQueue.Add(new GestureEvent(predictedGesture, predictedLocation, gesture));
        }

        public static void LogAudioEvent(string text, bool isSpeech = true)
        {
            if (!Running) return;

            eventQueue.Add(new AudioEvent(text, isSpeech));
        }

        public static void LogTrainingEvent(string message)
        {
            if (!Running) return;

            eventQueue.Add(new TrainingEvent(message));
        }

        public static void LogUIEvent(string action)
        {
            if (!Running) return;

            eventQueue.Add(new UIEvent(action));
        }

        public static void LogMenuEvent(string menu, int menuIndex, string item)
        {
            if (!Running) return;

            eventQueue.Add(new MenuEvent(menu, menuIndex, item));
        }

        public static void LogFrameProcessed()
        {
            if (!Running) return;

            eventQueue.Add(new FrameProcessedEvent());
        }

        public static void LogHardwareEvent(string component, string message)
        {
            if (!Running) return;

            eventQueue.Add(new HardwareEvent(component, message));
        }

        public static void LogOtherEvent(string message)
        {
            if (!Running) return;

            eventQueue.Add(new LogEvent(message));
        }

        public static void LogOtherEvent(LogEvent logEvent)
        {
            if (!Running) return;

            eventQueue.Add(logEvent);
        }

        public static void LogTouchEvent(string message)
        {
            if (!Running) return;

            eventQueue.Add(new TouchEvent(message));
        }

        public static List<LogEvent> ReadLog(string path, Action<float> progressReport = null)
        {
            List<LogEvent> events = new List<LogEvent>();

            List<string> lines = new List<string>(File.ReadAllLines(path));
            //StreamReader reader = new StreamReader(path);
            //while (!reader.EndOfStream)
            for(int i = 0; i < lines.Count; i++)
            {
                //string line = reader.ReadLine();
                string line = lines[i];
                line = line.Trim(' ', '\r', '\n', '\t', ',');
                if (line.Length == 0 || line == "]}" || line.Contains("log_id")) continue;

                try
                {
                    LogEvent logEvent = (LogEvent)JsonConvert.DeserializeObject(line, typeof(LogEvent));
                    if (logEvent.type == "sensor_reading")
                    {
                        logEvent = (SensorReadingEvent)JsonConvert.DeserializeObject(line, typeof(SensorReadingEvent));
                    }
                    else if (logEvent.type == "video_frame")
                    {
                        logEvent = (VideoFrameEvent)JsonConvert.DeserializeObject(line, typeof(VideoFrameEvent));
                    }
                    else if (logEvent.type == "location")
                    {
                        logEvent = (LocationEvent)JsonConvert.DeserializeObject(line, typeof(LocationEvent));
                    }
                    else if (logEvent.type == "frame_processed")
                    {
                        logEvent = (FrameProcessedEvent)JsonConvert.DeserializeObject(line, typeof(FrameProcessedEvent));
                    }
                    else if (logEvent.type == "gesture")
                    {
                        logEvent = (GestureEvent)JsonConvert.DeserializeObject(line, typeof(GestureEvent));
                    }
                    else if (logEvent.type == "audio")
                    {
                        logEvent = (AudioEvent)JsonConvert.DeserializeObject(line, typeof(AudioEvent));
                    }
                    else if (logEvent.type == "training")
                    {
                        logEvent = (TrainingEvent)JsonConvert.DeserializeObject(line, typeof(TrainingEvent));
                    }
                    else if (logEvent.type == "user_interface")
                    {
                        logEvent = (UIEvent)JsonConvert.DeserializeObject(line, typeof(UIEvent));
                    }
                    else if (logEvent.type == "menu")
                    {
                        logEvent = (MenuEvent)JsonConvert.DeserializeObject(line, typeof(MenuEvent));
                    }
                    else if (logEvent.type == "hardware")
                    {
                        logEvent = (HardwareEvent)JsonConvert.DeserializeObject(line, typeof(HardwareEvent));
                    }
                    else if (logEvent.type == "touch")
                    {
                        logEvent = (TouchEvent)JsonConvert.DeserializeObject(line, typeof(TouchEvent));
                    }
                    events.Add(logEvent);
                }
                catch { }

                progressReport?.Invoke((float)i / (float)lines.Count);
            }

            return events;
        }
    }
}
