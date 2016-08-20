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
                Context = "^" + inputs[0].Replace("*", ".*") + "$";
                Gesture = "^" + inputs[1].Replace("*", ".*") + "$";
                CoarseLocation = "^" + inputs[2].Replace("*", ".*") + "$";
                FineLocation = "^" + inputs[3].Replace("*", ".*") + "$";
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

        private static List<GestureAction> actions = new List<GestureAction>();
        private static string context = "none";

        public static void Load(string actionData)
        {
            actions.Clear();
            string[] lines = Regex.Split(actionData, "\r\n|\r|\n");
            for(int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (line.IndexOf('#') >= 0) line = line.Substring(0, line.IndexOf('#'));
                line = line.Trim();

                if(line.Length > 0)
                {
                    actions.Add(new GestureAction(line));
                }
            }
        }

        public static string PerformAction(string gesture, string coarseLocation, string fineLocation)
        {
            foreach(GestureAction action in actions)
            {
                if(action.IsMatch(context, gesture, coarseLocation, fineLocation))
                {
                    if (action.SetsNewContext) context = action.Text;
                    return action.Text;
                }
            }
            return "";
        }
    }
}
