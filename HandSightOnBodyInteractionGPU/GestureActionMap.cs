using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HandSightOnBodyInteractionGPU
{
    public class GestureActionMap
    {
        internal class GestureAction
        {
            public string Context = "*";
            public string Gesture = "*";
            public string CoarseLocation = "*";
            public string FineLocation = "*";
            public bool SetsNewContext = false;
            public string Text = "";
            public string NewContext = "";

            public GestureAction(string data)
            {
                string[] parts = data.Split('=');
                string[] inputs = parts[0].Split('~');
                Context = "^" + inputs[0] + "$";
                Gesture = "^" + inputs[1] + "$";
                CoarseLocation = "^" + inputs[2] + "$";
                FineLocation = "^" + inputs[3] + "$";
                if (parts[1].StartsWith("!"))
                {
                    SetsNewContext = true;
                    string tempText = parts[1].Substring(1);
                    if (tempText.Contains("~"))
                    {
                        string[] textParts = tempText.Split('~');
                        NewContext = textParts[0];
                        Text = textParts[1];
                    }
                    else
                    {
                        NewContext = tempText;
                        Text = tempText;
                    }
                }
                else
                {
                    SetsNewContext = false;
                    Text = parts[1];
                }
            }

            public bool IsMatch(string context, string gesture, string coarseLocation, string fineLocation)
            {
                bool match = true;
                if (!Regex.IsMatch(context, Context)) match = false;
                if (!Regex.IsMatch(gesture, Gesture)) match = false;
                if (!Regex.IsMatch(coarseLocation, CoarseLocation)) match = false;
                if (!Regex.IsMatch(fineLocation, FineLocation)) match = false;
                return match;
            }
        }

        private static DateTime start = DateTime.Now;

        private static Dictionary<string, List<GestureAction>> actions = new Dictionary<string, List<GestureAction>>();
        private static string context = "none";
        private static string lastText = "{{TIME}}";

        public static string Context { get { return context; } set { context = value; } }

        public static string[] Modes { get { if (actions == null) return null; else return actions.Keys.ToArray(); } }

        public static void Load(string actionData)
        {
            actions.Clear();
            string[] lines = Regex.Split(actionData, "\r\n|\r|\n");
            string mode = "default";
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (line.IndexOf('#') >= 0) line = line.Substring(0, line.IndexOf('#'));
                line = line.Trim();

                if (line.Length > 0)
                {
                    if (line.StartsWith(":") && line.EndsWith(":"))
                    {
                        mode = line.Trim(':');
                        actions.Add(mode, new List<GestureAction>());
                    }
                    else
                    {
                        actions[mode].Add(new GestureAction(line));
                    }
                }
            }
        }

        public static string PerformAction(string gesture, string coarseLocation, string fineLocation, string mode = null, bool fixedResponses = true)
        {
            if (mode == null || !Modes.Contains(mode)) mode = Modes[0];

            foreach (GestureAction action in actions[mode])
            {
                if (action.IsMatch(context, gesture, coarseLocation, fineLocation))
                {
                    if (action.SetsNewContext) context = action.NewContext;
                    string responseText = ParseMacros(action.Text, fixedResponses);
                    if(!action.Text.Contains("{{REPEAT}}"))
                        lastText = action.Text;
                    return responseText;
                }
            }
            lastText = "";
            return "";
        }

        private static string ToReadableTimespanString(TimeSpan span)
        {
            int years = span.Days;
            int weeks = (span.Days - years * 365) / 7;
            int days = span.Days - years * 365 - weeks * 7;
            int hours = span.Hours;
            int minutes = span.Minutes;
            int seconds = span.Seconds;

            // special cases, with some rounding for ease of comprehension
            if (years > 0) return years + " years";
            if (weeks > 0) return weeks + " weeks" + (days > 0 ? " and " + days + " days" : "");
            if (days > 1) return days + " days";
            if (days == 1) return (hours + 24) + " hours";

            string response = "";
            if (hours > 1) response += hours + " hours ";
            else if (hours == 1) response += "1 hour ";
            if (minutes > 1) response += minutes + " minutes ";
            else if (minutes == 1) response += "1 minute ";
            response += (response == "" ? "" : "and ") + (seconds == 1 ? "1 second" : seconds + " seconds");
            return response;
        }

        private static string ParseMacros(string text, bool fixedResponses = true)
        {
            if (text.Contains("{{REPEAT}}"))
            {
                text = text.Replace("{{REPEAT}}", lastText);
                text = Regex.Replace(text, "{{ONETIME:.*?}}", "");
            }
            if(text.Contains("{{ONETIME"))
            {
                Regex regex = new Regex("{{ONETIME:(.*?)}}");
                MatchCollection matches = regex.Matches(text);
                for(int i = 0; i < matches.Count; i++)
                    text = regex.Replace(text, matches[i].Groups[1].Value, 1);
            }
            if(text.Contains("{{TIME}}"))
            {
                string replacement = fixedResponses ? "10:25 AM" : DateTime.Now.ToShortTimeString();
                text = text.Replace("{{TIME}}", replacement);
            }
            if (text.Contains("{{TIMER}}"))
            {
                TimeSpan remaining = TimeSpan.FromMinutes(60) - (DateTime.Now - start);
                string replacement = fixedResponses ? "7 minutes and 45 seconds" : ToReadableTimespanString(remaining);
                text = text.Replace("{{TIMER}}", replacement);
            }
            if (text.Contains("{{STOPWATCH}}"))
            {
                TimeSpan elapsed = (DateTime.Now - start);
                string replacement = fixedResponses ? "7 minutes and 45 seconds" : ToReadableTimespanString(elapsed);
                text = text.Replace("{{STOPWATCH}}", replacement);
            }
            if (text.Contains("{{DATE}}"))
            {
                string replacement = fixedResponses ? "Wednesday, August 24th, 2016" : DateTime.Now.ToLongDateString();
                text = text.Replace("{{DATE}}", replacement);
            }
            if (text.Contains("{{NEXTEVENT}}"))
            {
                text = text.Replace("{{NEXTEVENT}}", "dentist appointment from 2pm to 3pm starts in 1 hour and 35 minutes");
            }
            if (text.Contains("{{ALARM}}"))
            {
                text = text.Replace("{{ALARM}}", "8 AM");
            }
            if (text.Contains("{{MESSAGECOUNT}}"))
            {
                text = text.Replace("{{MESSAGECOUNT}}", "1 missed phone call and 2 new messages");
            }
            if (text.Contains("{{NOTIFICATION1}}"))
            {
                text = text.Replace("{{NOTIFICATION1}}", "Missed phone call from Alice, just now");
            }
            if (text.Contains("{{NOTIFICATION2}}"))
            {
                text = text.Replace("{{NOTIFICATION2}}", "Message from Bob 16 minutes ago. Okay, I will see you soon");
            }
            if (text.Contains("{{NOTIFICATION3}}"))
            {
                text = text.Replace("{{NOTIFICATION3}}", "Message from Charlie 5 hours ago. What's up?");
            }
            if (text.Contains("{{WEATHER}}"))
            {
                text = text.Replace("{{WEATHER}}", "It's partly cloudy and 81 degrees currently and the high for today was forecast as 84 degrees");
            }
            if (text.Contains("{{MISSEDCALLS}}"))
            {
                text = text.Replace("{{MISSEDCALLS}}", "1");
            }
            if (text.Contains("{{MESSAGES}}"))
            {
                text = text.Replace("{{MESSAGES}}", "2");
            }
            if (text.Contains("{{EMAILS}}"))
            {
                text = text.Replace("{{EMAILS}}", "6");
            }
            if (text.Contains("{{FACEBOOK}}"))
            {
                text = text.Replace("{{FACEBOOK}}", "7");
            }
            if (text.Contains("{{CALORIES}}"))
            {
                text = text.Replace("{{CALORIES}}", "497");
            }
            if (text.Contains("{{STEPS}}"))
            {
                text = text.Replace("{{STEPS}}", "2366");
            }
            if (text.Contains("{{DISTANCE}}"))
            {
                text = text.Replace("{{DISTANCE}}", "1.8 miles");
            }
            if (text.Contains("{{HEARTRATE}}"))
            {
                text = text.Replace("{{HEARTRATE}}", "118 bpm");
            }
            if (text.Contains("{{VOLUMEUP}}"))
            {
                text = text.Replace("{{VOLUMEUP}}", "Volume up");
            }
            if (text.Contains("{{VOLUMEDOWN}}"))
            {
                text = text.Replace("{{VOLUMEDOWN}}", "Volume down");
            }
            if (text.Contains("{{ANSWERCALL}}"))
            {
                text = text.Replace("{{ANSWERCALL}}", "Call answered. Hello?");
            }
            if (text.Contains("{{REJECTCALL}}"))
            {
                text = text.Replace("{{REJECTCALL}}", "Call Rejected");
            }
            if (text.Contains("{{VOICEINPUT}}"))
            {
                text = text.Replace("{{VOICEPROMPT}}", "How can I help you today?");
            }
            return text;
        }
    }
}
