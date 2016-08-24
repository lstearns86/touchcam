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
                    Text = parts[1].Substring(1);
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

        public static string[] Modes { get { if (actions == null) return null; else return actions.Keys.ToArray(); } }

        public static void Load(string actionData)
        {
            actions.Clear();
            string[] lines = Regex.Split(actionData, "\r\n|\r|\n");
            string mode = "default";
            for(int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (line.IndexOf('#') >= 0) line = line.Substring(0, line.IndexOf('#'));
                line = line.Trim();

                if(line.Length > 0)
                {
                    if(line.StartsWith(":") && line.EndsWith(":"))
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

            foreach(GestureAction action in actions[mode])
            {
                if(action.IsMatch(context, gesture, coarseLocation, fineLocation))
                {
                    if (action.SetsNewContext) context = action.Text;
                    return ParseMacros(action.Text, fixedResponses);
                }
            }
            return "";
        }

        private static string ParseMacros(string text, bool fixedResponses = true)
        {
            // TODO: provide dynamic responses if enabled
            if(text.Contains("{{TIME}}"))
            {
                string replacement = fixedResponses ? "10:25 AM" : DateTime.Now.ToShortTimeString();
                text = text.Replace("{{TIME}}", replacement);
            }
            if (text.Contains("{{TIMER}}"))
            {
                TimeSpan remaining = TimeSpan.FromMinutes(60) - (DateTime.Now - start);
                int minutes = (int)remaining.TotalMinutes;
                int seconds = (int)(remaining.TotalSeconds - 60 * minutes);
                string replacement = fixedResponses ? "7 minutes and 45 seconds remaining" : ((remaining.TotalMinutes > 0 ? minutes + " minutes and " : "") + seconds + " seconds remaining");
                text = text.Replace("{{TIMER}}", replacement);
            }
            if (text.Contains("{{ALARM}}"))
            {
                text = text.Replace("{{ALARM}}", "Alarm set for 8 AM");
            }
            if (text.Contains("{{MESSAGECOUNT}}"))
            {
                text = text.Replace("{{MESSAGECOUNT}}", "15 new messages");
            }
            if (text.Contains("{{WEATHER}}"))
            {
                text = text.Replace("{{WEATHER}}", "It's sunny and 79 degrees.");
            }
            if (text.Contains("{{MESSAGES}}"))
            {
                text = text.Replace("{{MESSAGES}}", "2 new text messages");
            }
            if (text.Contains("{{EMAILS}}"))
            {
                text = text.Replace("{{EMAILS}}", "6 new emails");
            }
            if (text.Contains("{{FACEBOOK}}"))
            {
                text = text.Replace("{{FACEBOOK}}", "7 new facebook messages");
            }
            if (text.Contains("{{STEPS}}"))
            {
                text = text.Replace("{{STEPS}}", "497 calories burnt");
            }
            if (text.Contains("{{CALORIES}}"))
            {
                text = text.Replace("{{CALORIES}}", "2366 steps");
            }
            if (text.Contains("{{DISTANCE}}"))
            {
                text = text.Replace("{{DISTANCE}}", "1.8 miles traveled");
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
                text = text.Replace("{{VOICEINPUT}}", "How can I help you today?");
            }
            return text;
        }
    }
}
