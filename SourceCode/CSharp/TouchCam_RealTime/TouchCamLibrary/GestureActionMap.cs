using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace TouchCamLibrary
{
    /// <summary>
    /// Maps gestures to actions and menu navigation, and provides text for speech output
    /// Controlled via xml and macros (see examples in the defaults folder of the main project in this solution)
    /// </summary>
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

            public bool IsMatch(string context, string gesture, string coarseLocation, string fineLocation)
            {
                bool match = true;
                if (!Regex.IsMatch(context, Context, RegexOptions.IgnoreCase)) match = false;
                if (!Regex.IsMatch(gesture, Gesture, RegexOptions.IgnoreCase)) match = false;
                if (!Regex.IsMatch(coarseLocation, CoarseLocation, RegexOptions.IgnoreCase)) match = false;
                if (!Regex.IsMatch(fineLocation, FineLocation, RegexOptions.IgnoreCase)) match = false;
                return match;
            }
        }

        private static DateTime start = DateTime.Now;

        public delegate void TaskCompletedDelegate();
        public static event TaskCompletedDelegate TaskCompleted;
        private static void OnTaskCompleted() { TaskCompleted?.Invoke(); }

        //private static Dictionary<string, List<GestureAction>> actions = new Dictionary<string, List<GestureAction>>();
        private static Dictionary<string, string> macros = new Dictionary<string, string>();
        private static Dictionary<string, ActionMode> modes = new Dictionary<string, ActionMode>();
        private static Dictionary<string, Menu> menus = new Dictionary<string, Menu>();
        private static string context = "Main Menu";
        private static string currMenu = "Main Menu";
        private static string lastText = "{{CLOCK}}";
        private static int currMenuIndex = 0;

        private static string currentTask;
        public static string CurrentTask { get { return currentTask; } set { currentTask = value; } }

        public static void StartTask(string task) { currentTask = task; }

        public static void Reset()
        {
            context = "Main Menu";
            currMenu = "Main Menu";
            lastText = "{{CLOCK}}";
            currMenuIndex = 0;
        }

        public static string Context { get { return Context; } set { context = value; } }

        public static string[] Modes { get { if (modes == null) return null; else return modes.Keys.ToArray(); } }

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
                    string[] parts = line.Split('=');
                    macros.Add("{{" + parts[0] + "}}", parts[1]);
                }
            }
        }

        static string savedMenuData = "";
        public static void LoadMenus(string menuData)
        {
            savedMenuData = menuData;
            menus.Clear();
            Menu[] tempMenus = JsonConvert.DeserializeObject<Menu[]>(menuData);
            foreach(Menu menu in tempMenus)
                menus.Add(menu.Name, menu);
        }

        public static void LoadActions(string actionData)
        {
            modes.Clear();
            ActionMode[] tempModes = JsonConvert.DeserializeObject<ActionMode[]>(actionData);
            foreach(ActionMode mode in tempModes)
                modes.Add(mode.Name, mode);
        }

        public static void RandomizeMenuItems()
        {
            foreach(string app in menus.Keys)
            {
                if(app != "Main Menu")
                    menus[app].Items.ShuffleAfterFirst();
            }
        }

        public static void ResetMenus()
        {
            LoadMenus(savedMenuData);
            currMenu = "Main Menu";
            currMenuIndex = 0;
        }

        public static string GetMenuItem(string menu, int index)
        {
            if(menus.ContainsKey(menu) && index >= 0 && index < menus[menu].Items.Length)
            {
                return menus[menu].Items[index].Name;
            }

            return "";
        }

        public static string PerformAction(string gesture, string coarseLocation, string fineLocation, string mode = null, bool fixedResponses = true, bool lockToCurrentTask = false)
        {
            if (mode == null || !Modes.Contains(mode)) mode = Modes[0];

            foreach (GestureAction action in modes[mode].Actions)
            {
                if (action.IsMatch(context, gesture, coarseLocation, fineLocation))
                {
                    // process action, update the menu location and generate response text
                    string responseText = "";
                    if(action.SetsMenuItem != null && action.SetsMenuItem.Length > 0)
                    {
                        if(action.SetsMenuItem == "{{NEXT_MENU_ITEM}}")
                        {
                            currMenuIndex = (currMenuIndex + 1) % menus[currMenu].Items.Length;
                            responseText = menus[currMenu].Items[currMenuIndex].Text;
                            Logging.LogMenuEvent(currMenu, currMenuIndex, menus[currMenu].Items[currMenuIndex].Name);
                        }
                        else if (action.SetsMenuItem == "{{PREV_MENU_ITEM}}")
                        {
                            currMenuIndex = (currMenuIndex + menus[currMenu].Items.Length - 1) % menus[currMenu].Items.Length;
                            responseText = menus[currMenu].Items[currMenuIndex].Text;
                            Logging.LogMenuEvent(currMenu, currMenuIndex, menus[currMenu].Items[currMenuIndex].Name);
                        }
                        else if(action.SetsMenuItem == "{{PREV_CONTEXT}}")
                        {
                            responseText = action.Text;
                            Logging.LogUIEvent("set_context=" + action.SetsMenuItem);
                        }
                        else if(action.SetsMenuItem == "{{EXPAND_MENU}}")
                        {
                            if (menus.ContainsKey(menus[currMenu].Items[currMenuIndex].Submenu))
                            {
                                // check to see if the current menu item's submenu items contains the target task
                                if (lockToCurrentTask)
                                {
                                    string submenu = menus[currMenu].Items[currMenuIndex].Submenu;
                                    bool submenuContainsCurrentTask = menus[currMenu].Items[currMenuIndex].Name == currentTask;
                                    if (currMenu != submenu)
                                        foreach (MenuItem item in menus[submenu].Items)
                                            if (item.Name == currentTask)
                                                submenuContainsCurrentTask = true;
                                    if (!submenuContainsCurrentTask) return "";
                                }

                                string prevMenu = currMenu;
                                responseText = menus[currMenu].Items[currMenuIndex].ExpandText;
                                currMenu = menus[currMenu].Items[currMenuIndex].Submenu;
                                if (prevMenu != currMenu)
                                {
                                    currMenuIndex = 0;
                                    responseText += menus[currMenu].Items[currMenuIndex].Text; // add the text for the initial menu item
                                }
                                else
                                {
                                    if (currentTask == menus[currMenu].Items[currMenuIndex].Name)
                                    {
                                        responseText = "Task Completed";
                                        currentTask = null;
                                        OnTaskCompleted();
                                        return "";
                                    }
                                }
                                Logging.LogMenuEvent(currMenu, currMenuIndex, menus[currMenu].Items[currMenuIndex].Name);
                            }
                            else
                            {
                                responseText = menus[currMenu].Items[currMenuIndex].Text;

                                if (currentTask == menus[currMenu].Items[currMenuIndex].Name)
                                {
                                    responseText = "Task Completed";
                                    currentTask = null;
                                    OnTaskCompleted();
                                    return "";
                                }
                                Logging.LogMenuEvent(currMenu, currMenuIndex, menus[currMenu].Items[currMenuIndex].Name);
                            }
                        }
                        else if(action.SetsMenuItem == "{{COLLAPSE_MENU}}")
                        {
                            if (!menus.ContainsKey(menus[currMenu].ParentMenu)) return "";

                            for (int i = 0; i < menus[menus[currMenu].ParentMenu].Items.Length; i++)
                                if (menus[menus[currMenu].ParentMenu].Items[i].Name == menus[currMenu].ParentMenuItem)
                                {
                                    currMenuIndex = i;
                                    break;
                                }
                            currMenu = menus[currMenu].ParentMenu;
                            responseText = menus[currMenu].Name + ", " + menus[currMenu].Items[currMenuIndex].Text;
                            Logging.LogMenuEvent(currMenu, currMenuIndex, menus[currMenu].Items[currMenuIndex].Name);
                        }
                        else
                        {
                            for(int i = 0; i < menus[currMenu].Items.Length; i++)
                            {
                                if(menus[currMenu].Items[i].Name == action.SetsMenuItem)
                                {
                                    currMenuIndex = i;
                                    responseText = menus[currMenu].Items[i].Text;
                                    Logging.LogMenuEvent(currMenu, currMenuIndex, menus[currMenu].Items[currMenuIndex].Name);
                                }
                            }
                        }
                    }
                    else
                    {
                        responseText = action.Text;
                    }

                    // update the current context
                    context = currMenu + " " + menus[currMenu].Items[currMenuIndex].Name;
                    
                    // store the current response before expanding macros to be reused if needed
                    if (action.RepeatsText && !responseText.Contains("{{NOREPEAT}}") && !responseText.Contains("{{REPEAT}}")) lastText = responseText;

                    // expand macros
                    responseText = ParseMacros(responseText, fixedResponses);

                    return responseText;
                }
            }
            lastText = "";
            return "";
        }

        // recursively expand defined macros
        private static string ParseMacros(string text, bool fixedResponses = true, bool isRepeated = false)
        {
            MatchCollection macroMatches = Regex.Matches(text, @"\{\{(.*?)\}\}");
            foreach(Match match in macroMatches)
            {
                if(match.Value == "{{REPEAT}}")
                {
                    text = text.Replace("{{REPEAT}}", ParseMacros(lastText, fixedResponses, true));
                }
                else if(match.Value.Contains("{{ONETIME"))
                {
                    Regex regex = new Regex("{{ONETIME:(.*?)}}");
                    MatchCollection matches = regex.Matches(text);
                    for (int i = 0; i < matches.Count; i++)
                        text = regex.Replace(text, isRepeated ? "" : ParseMacros(matches[i].Groups[1].Value, fixedResponses), 1);
                }
                else if(match.Value == "{{TIME}}")
                {
                    string replacement = fixedResponses ? "10:25 AM" : DateTime.Now.ToShortTimeString();
                    text = text.Replace("{{TIME}}", replacement);
                }
                else if (match.Value == "{{TIMER}}")
                {
                    TimeSpan remaining = TimeSpan.FromMinutes(60) - (DateTime.Now - start);
                    string replacement = fixedResponses ? "7 minutes and 45 seconds" : Utils.ToReadableTimespanString(remaining);
                    text = text.Replace("{{TIMER}}", replacement);
                }
                else if (match.Value == "{{STOPWATCH}}")
                {
                    TimeSpan elapsed = (DateTime.Now - start);
                    string replacement = fixedResponses ? "9 minutes and 27 seconds" : Utils.ToReadableTimespanString(elapsed);
                    text = text.Replace("{{STOPWATCH}}", replacement);
                }
                else if (match.Value == "{{DATE}}")
                {
                    string replacement = fixedResponses ? "Wednesday, August 24th, 2016" : DateTime.Now.ToLongDateString();
                    text = text.Replace("{{DATE}}", replacement);
                }
                else if (match.Value == "{{NOREPEAT}}")
                {
                    text = text.Replace("{{NOREPEAT}}", "");
                }
                else
                {
                    foreach (string macro in macros.Keys)
                    if (match.Value == macro)
                        text = text.Replace(macro, ParseMacros(macros[macro], fixedResponses, isRepeated));
                }
            }
            
            return text;
        }
    }
}
