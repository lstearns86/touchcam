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
        internal class Menu
        {
            public string Name = "";
            public MenuItem[] Items = null;
            public string ParentMenu = "";
            public string ParentMenuItem = "";
        }

        internal class MenuItem
        {
            public string Name = "";
            public string Text = "";
            public string Submenu = "";
            public string ExpandText = "";
        }

        internal class ActionMode
        {
            public string Name = "";
            public GestureAction[] Actions = null;
        }

        internal class GestureAction
        {
            public string Context = "*";
            public string Gesture = "*";
            public string CoarseLocation = "*";
            public string FineLocation = "*";
            public string SetsMenuItem = "";
            public string Text = "";
            public bool RepeatsText = true;

            /*public GestureAction(string data)
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
            }*/

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

        //private static Dictionary<string, List<GestureAction>> actions = new Dictionary<string, List<GestureAction>>();
        private static Dictionary<string, string> macros = new Dictionary<string, string>();
        private static Dictionary<string, ActionMode> modes = new Dictionary<string, ActionMode>();
        private static Dictionary<string, Menu> menus = new Dictionary<string, Menu>();
        private static string context = "Root Menu Clock";
        private static string currMenu = "Root Menu", currMenuItem = "Clock";
        private static string lastText = "{{TIME}}";
        private static string lastMenu = "Root Menu", lastMenuItem = "Clock";

        public static string Context { get { return context; } set { context = value; } }

        public static string[] Modes { get { if (actions == null) return null; else return modes.Keys.ToArray(); } }

        public static void LoadMacros(string macroData)
        {
            macros.Clear();
            string[] lines = Regex.Split(macroData, "\r\n|\r|\n");
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (line.IndexOf('#') >= 0) line = line.Substring(0, line.IndexOf('#'));
                line = line.Trim();

                if (line.Length > 0)
                {
                    string[] parts = line.Split('=')
                    macros.Add(parts[0], parts[1]);
                }
            }
        }

        public static void LoadMenus(string menuData)
        {
            menus.Clear();
            Menu[] tempMenus = JsonConvert.DeserializeObject<Menu[]>(menuData);
            foreach(Menu menu in tempMenus)
                menus.add(menu.Name, menu);
        }

        public static void LoadActions(string actionData)
        {
            modes.Clear();
            ActionMode[] tempModes = JsonConvert.DeserializeObject<ActionMode[]>(actionData);
            foreach(ActionMode mode in tempModes)
                modes.add(mode.Name, mode);

            /*actions.Clear();
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
            }*/
        }

        public static string PerformAction(string gesture, string coarseLocation, string fineLocation, string mode = null, bool fixedResponses = true)
        {
            if (mode == null || !Modes.Contains(mode)) mode = Modes[0];

            foreach (GestureAction action in modes[mode].Actions)
            {
                if (action.IsMatch(context, gesture, coarseLocation, fineLocation))
                {
                    //if (action.SetsNewContext) context = action.NewContext;
                    //string responseText = ParseMacros(action.Text, fixedResponses);
                    //if(!action.Text.Contains("{{REPEAT}}"))
                    //    lastText = action.Text;
                    //return responseText;

                    // TODO: process action
                    string responseText = action.Text
                    while(Regex.IsMatch(reponseText, "{{.*}}"))
                        responseText = ParseMacros(responseText, fixedResponses);
                    return "";
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
            if (text.Contains("{{NOREPEAT}}"))
            {
                text = text.Replace("{{NOREPEAT}}", "");
            }
            foreach(string macro in macros.Keys)
            {
                if(text.Contains(macro))
                    text = text.Replace(macro, macros[macro]);
            }
            return text;
        }
    }
}
