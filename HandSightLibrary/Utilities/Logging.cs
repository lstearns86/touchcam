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
            public string videoFile;

            public LogEvent() : this("") { }
            public LogEvent(string message)
            {
                this.message = message;
                type = "generic";
                timestamp = 1000.0f * (float)stopwatch.ElapsedTicks / (float)Stopwatch.Frequency;
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
            public string location;
            public string predictedLocation;
            public string smoothedLocation;
            public bool manual;

            public LocationEvent(string location, bool manual)
                : base()
            {
                this.location = location;
                this.manual = manual;
                type = "location";
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
            public string message;
            
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

        #endregion

        #region Constants and Variables

        // constants
        const int defaultBitrate = 4 * 1024 * 1024;

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
            string path = filename + Index + ".avi";
            
            videoWriter = new VideoFileWriter();
            videoWriter.Open(path, videoSize.Width, videoSize.Height, 90, VideoCodec.MPEG4, defaultBitrate);

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
            string path = filename + Index + ".json";
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

        public static void LogLocationEvent(string location, bool manual = true)
        {
            if (!Running) return;

            eventQueue.Add(new LocationEvent(location, manual));
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
                    events.Add(logEvent);
                }
                catch { }

                if (progressReport != null) progressReport((float)i / (float)lines.Count);
            }

            return events;
        }
    }
}
