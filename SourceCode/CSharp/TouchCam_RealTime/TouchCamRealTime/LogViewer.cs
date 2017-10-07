using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using TouchCamLibrary;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace TouchCam
{
    public partial class LogViewer : Form
    {
        List<Logging.LogEvent> events = new List<Logging.LogEvent>();
        DateTime fileStartTime, fileEndTime;

        public LogViewer()
        {
            InitializeComponent();
        }

        private void Checkbox_CheckedChanged(object sender, EventArgs e)
        {
            UpdateDisplay();
        }

        private List<Logging.LogEvent> FilterEvents()
        {
            List<Logging.LogEvent> filteredEvents = new List<Logging.LogEvent>();

            foreach (Logging.LogEvent e in events)
            {
                if (e is Logging.VideoFrameEvent)
                {
                    if (VideoFramesCheckbox.Checked)
                        filteredEvents.Add(e);
                }
                else if (e is Logging.SensorReadingEvent)
                {
                    if (SensorReadingsCheckbox.Checked)
                        filteredEvents.Add(e);
                }
                else if(e is Logging.TouchEvent)
                {
                    if (TouchEventsCheckbox.Checked)
                        filteredEvents.Add(e);
                }
                else if (e is Logging.LocationEvent)
                {
                    if (LocationCheckbox.Checked)
                        filteredEvents.Add(e);
                }
                else if (e is Logging.GestureEvent)
                {
                    if (GestureCheckbox.Checked)
                        filteredEvents.Add(e);
                }
                else if (e is Logging.AudioEvent)
                {
                    if (AudioCheckbox.Checked)
                        filteredEvents.Add(e);
                }
                else if (e is Logging.TrainingEvent)
                {
                    if (TrainingCheckbox.Checked)
                        filteredEvents.Add(e);
                }
                else if (e is Logging.UIEvent)
                {
                    Logging.UIEvent uie = (Logging.UIEvent)e;
                    if (uie.action.Contains("Phase"))
                    {
                        if (PhasesCheckbox.Checked)
                            filteredEvents.Add(e);
                    }
                    else if (uie.action.Contains("Task"))
                    {
                        if (TasksCheckbox.Checked)
                            filteredEvents.Add(e);
                    }
                    else
                    {
                        if (UICheckbox.Checked)
                            filteredEvents.Add(e);
                    }
                }
                else if (e is Logging.MenuEvent)
                {
                    if (MenuCheckbox.Checked)
                        filteredEvents.Add(e);
                }
                else if (e is Logging.FrameProcessedEvent || e.message == "frame_processed")
                {
                    if (FrameProcessedCheckbox.Checked)
                        filteredEvents.Add(e);
                }
                else if (e is Logging.HardwareEvent)
                {
                    if (HardwareCheckbox.Checked)
                        filteredEvents.Add(e);
                }
                else if (OtherCheckbox.Checked)
                    filteredEvents.Add(e);
            }

            return filteredEvents;
        }

        const int MAX_EVENTS_TO_DISPLAY = 1000;
        private void UpdateDisplay()
        {
            if (InvokeRequired) Invoke(new MethodInvoker(UpdateDisplay));

            DataBox.Items.Clear();
            if (rawLogEventsToolStripMenuItem.Checked)
            {
                List<Logging.LogEvent> filteredEvents = FilterEvents();
                foreach (Logging.LogEvent e in filteredEvents)
                {
                    if (e.timestamp >= startTimestamp && e.timestamp <= endTimestamp && DataBox.Items.Count <= MAX_EVENTS_TO_DISPLAY)
                    {
                        string text = e.ToJson();
                        float t = e.timestamp;
                        text = (t / 1000).ToString("0.0") + " s" + ": " + text;
                        ListViewItem item = new ListViewItem(text);
                        item.Tag = e;
                        DataBox.Items.Add(item);
                        //DataBox.Items.Add(text);
                    }
                }
                EventCountLabel.Text = filteredEvents.Count + " / " + events.Count;
            }
            else if(phaseDurationsToolStripMenuItem.Checked)
            {
                float startTime = events != null && events.Count > 0 ? events[0].timestamp : 0;
                float lastPhase = events[0].timestamp;
                string currentPhase = null;
                foreach (Logging.LogEvent e in events)
                {
                    if ((e is Logging.UIEvent) && (((Logging.UIEvent)e).action.Contains("Phase")) || e == events[events.Count - 1])
                    {
                        string phase = e is Logging.UIEvent ? ((Logging.UIEvent)e).action : "end";
                        float t = e.timestamp;
                        float elapsed = e.timestamp - lastPhase;
                        string elapsedString = "";
                        if(elapsed > 0)
                        {
                            float seconds = elapsed / 1000.0f;
                            int minutes = (int)(seconds / 60);
                            seconds -= minutes * 60;
                            elapsedString = (minutes > 0 ? minutes + " minutes, " : "") + seconds.ToString("0.0") + " seconds";
                        }
                        lastPhase = e.timestamp;

                        if (currentPhase != null)
                        {
                            string text = elapsedString + ": " + currentPhase;
                            DataBox.Items.Add(text);
                        }
                        currentPhase = phase;
                    }
                }
            }
            else if(taskDurationsToolStripMenuItem.Checked)
            {
                float lastTaskStarted = 0;
                float averageTaskDuration = 0, numTasks = 0;
                string currPhase = "";
                bool startedTask = false, openedMenu = false;
                foreach (Logging.LogEvent e in events)
                {
                    if (DataBox.Items.Count < MAX_EVENTS_TO_DISPLAY)
                    {
                        float t = e.timestamp;
                        float elapsed = -1;
                        //if ((e is Logging.UIEvent) && (((Logging.UIEvent)e).action.Contains("Phase")))
                        //{
                        //    if (currPhase != null && numTasks > 0)
                        //    {
                        //        averageTaskDuration /= numTasks;
                        //        DataBox.Items.Add("Completed " + currPhase);
                        //        DataBox.Items.Add("Average Task Duration: " + (averageTaskDuration / 1000.0f).ToString("0.00") + " s");
                        //    }
                        //    currPhase = ((Logging.UIEvent)e).action;
                        //    DataBox.Items.Add("Start " + currPhase);
                        //    averageTaskDuration = 0;
                        //    numTasks = 0;
                        //}
                        if((e is Logging.UIEvent) && (((Logging.UIEvent)e).action.Contains("Mode Set")))
                        {
                            DataBox.Items.Add(((Logging.UIEvent)e).action);
                        }
                        else if ((e is Logging.UIEvent) && (((Logging.UIEvent)e).action.Contains("Task started")))
                        {
                            lastTaskStarted = e.timestamp;
                            startedTask = true;
                            openedMenu = false;
                        }
                        else if ((e is Logging.UIEvent) && (((Logging.UIEvent)e).action.Contains("Task finished")))
                        {
                            elapsed = e.timestamp - lastTaskStarted;
                            averageTaskDuration += elapsed;
                            numTasks++;
                            startedTask = false;
                            openedMenu = false;

                            string elapsedString = "";
                            if (elapsed > 0)
                            {
                                float seconds = elapsed / 1000.0f;
                                //int minutes = (int)(seconds / 60);
                                //seconds -= minutes * 60;
                                //elapsedString = (minutes > 0 ? minutes + " minutes, " : "") + seconds.ToString("0.0") + " seconds";
                                elapsedString = seconds.ToString("0.0") + " seconds";
                            }

                            DataBox.Items.Add(elapsedString + " to complete task " + ((Logging.UIEvent)e).action);
                        }
                        else if((e is Logging.MenuEvent) && (((Logging.MenuEvent)e).menu != "Main Menu") && startedTask && !openedMenu)
                        {
                            openedMenu = true;
                            elapsed = e.timestamp - lastTaskStarted;
                            string elapsedString = "";
                            if(elapsed > 0)
                            {
                                float seconds = elapsed / 1000.0f;
                                elapsedString = seconds.ToString("0.0") + " seconds";
                            }

                            DataBox.Items.Add(elapsedString + " to open " + ((Logging.MenuEvent)e).menu);
                        }
                    }
                }
            }
            else if(menuNavigationEventsToolStripMenuItem.Checked)
            {
                bool startedTask = false, openedMenu = false;
                int numMainMenuItemsNavigated = 0, numSubMenuItemsNavigated = 0;
                foreach (Logging.LogEvent e in events)
                {
                    if (DataBox.Items.Count < MAX_EVENTS_TO_DISPLAY)
                    {
                        if ((e is Logging.UIEvent) && (((Logging.UIEvent)e).action.Contains("Mode Set")))
                        {
                            DataBox.Items.Add(((Logging.UIEvent)e).action);
                        }
                        else if ((e is Logging.UIEvent) && (((Logging.UIEvent)e).action.Contains("Task started")))
                        {
                            startedTask = true;
                            openedMenu = false;
                            numMainMenuItemsNavigated = 0;
                            numSubMenuItemsNavigated = 0;
                        }
                        else if ((e is Logging.UIEvent) && (((Logging.UIEvent)e).action.Contains("Task finished")))
                        {
                            startedTask = false;
                            openedMenu = false;

                            DataBox.Items.Add("Navigated " + numMainMenuItemsNavigated + " apps and " + numSubMenuItemsNavigated + " submenu items to complete task " + ((Logging.UIEvent)e).action);
                        }
                        else if (e is Logging.MenuEvent)
                        {
                            Logging.MenuEvent me = (Logging.MenuEvent)e;
                            if(me.menu != "Main Menu" && startedTask && !openedMenu)
                            {
                                openedMenu = true;
                            }

                            if(me.menu == "Main Menu")
                            {
                                numMainMenuItemsNavigated++;
                            }
                            else
                            {
                                numSubMenuItemsNavigated++;
                            }
                        }
                    }
                }
            }
        }

        private void LoadFile(string path)
        {
            Progress.Value = 0;
            Progress.Maximum = 100;
            int prevProgress = 0;

            Task.Factory.StartNew(() =>
            {
                Action<float> progressAction = (float progress) =>
                {
                    if ((int)(progress * 100) == prevProgress) return;
                    Invoke(new MethodInvoker(delegate { Progress.Value = (int)(progress * 100); }));
                    prevProgress = (int)(progress * 100);
                };

                bool append = !(events == null || events.Count == 0);
                if (events == null) events = new List<Logging.LogEvent>();
                List<Logging.LogEvent> tempEvents = Logging.ReadLog(path, progressAction);

                FileInfo info = new FileInfo(path);
                if (append)
                {
                    double elapsedBetweenFiles = (info.CreationTimeUtc - fileEndTime).TotalMilliseconds;
                    foreach (Logging.LogEvent e in tempEvents) e.timestamp += (float)elapsedBetweenFiles;
                    fileEndTime = info.LastWriteTimeUtc;
                }
                else
                {
                    fileStartTime = info.CreationTimeUtc;
                    fileEndTime = info.LastWriteTimeUtc;
                }
                events.AddRange(tempEvents);

                Invoke(new MethodInvoker(delegate { Progress.Value = 0; }));

                UpdateDisplay();
            });
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(!Directory.Exists(Properties.Settings.Default.LogOpenDirectory))
            {
                Properties.Settings.Default.LogDirectory = Path.GetFullPath("logs");
                Properties.Settings.Default.Save();
            }

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Select Log File to Open";
            dialog.InitialDirectory = Properties.Settings.Default.LogOpenDirectory;
            dialog.Filter = "Log Files|*.log";
            if(dialog.ShowDialog(this) == DialogResult.OK)
            {
                Properties.Settings.Default.LogOpenDirectory = Path.GetDirectoryName(dialog.FileName);
                Properties.Settings.Default.Save();
                events.Clear();
                LoadFile(dialog.FileName);
            }
        }

        private void appendToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (events == null || events.Count == 0) { openToolStripMenuItem.PerformClick(); return; }

            if (!Directory.Exists(Properties.Settings.Default.LogOpenDirectory))
            {
                Properties.Settings.Default.LogDirectory = Path.GetFullPath("logs");
                Properties.Settings.Default.Save();
            }

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Select Log File to Append";
            dialog.InitialDirectory = Properties.Settings.Default.LogOpenDirectory;
            dialog.Filter = "Log Files|*.log";
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                Properties.Settings.Default.LogOpenDirectory = Path.GetDirectoryName(dialog.FileName);
                Properties.Settings.Default.Save();
                LoadFile(dialog.FileName);
            }
        }

        private void LogViewer_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.C && e.Modifiers == Keys.Control)
            {
                string text = "";
                foreach (string item in DataBox.SelectedItems)
                {
                    if (taskDurationsToolStripMenuItem.Checked)
                    {
                        if (item.Contains("to complete task"))
                        {
                            string task = item.Substring(item.IndexOf("to complete task ") + "to complete task".Length).Replace(" Task finished: ", "").Replace("Message 1", "Missed Phone Call").Replace("Message 3", "Text Message");
                            string time = item.Substring(0, item.IndexOf("seconds") - 1);
                            if (task == "Voice Input")
                                text += task + "\t" + time + "\t";
                            text += task + "\t" + time + Environment.NewLine;
                        }
                        else if (item.Contains("to open"))
                        {
                            string app = item.Substring(item.IndexOf("to open ") + "to open ".Length).Replace(" Menu", "");
                            string time = item.Substring(0, item.IndexOf("seconds") - 1);
                            text += app + "\t" + time + "\t";
                        }
                    }
                    else if (menuNavigationEventsToolStripMenuItem.Checked)
                    {
                        if(item.Contains("Navigated"))
                        {
                            Match match = Regex.Match(item, @"Navigated (\d+) apps and (\d+) submenu items to complete task (.*)");
                            if(match.Success)
                            {
                                text += match.Groups[1] + "\t" + match.Groups[2] + "\t" + match.Groups[3] + Environment.NewLine;
                            }
                        }
                    }
                }
                Clipboard.SetText(text);
            }
        }

        float startTimestamp = float.MinValue;
        float endTimestamp = float.MaxValue;
        private void setStartLocationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(DataBox.SelectedItem is ListViewItem)
            {
                Logging.LogEvent ev = (Logging.LogEvent)((ListViewItem)DataBox.SelectedItem).Tag;
                startTimestamp = ev.timestamp;
                UpdateDisplay();
            }
        }

        private void setEndLocationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DataBox.SelectedItem is ListViewItem)
            {
                Logging.LogEvent ev = (Logging.LogEvent)((ListViewItem)DataBox.SelectedItem).Tag;
                endTimestamp = ev.timestamp;
                UpdateDisplay();
            }
        }

        private void resetStartEndFiltersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            startTimestamp = float.MinValue;
            endTimestamp = float.MaxValue;
            UpdateDisplay();
        }

        private void displayModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach(ToolStripMenuItem item in displayModeToolStripMenuItem.DropDownItems)
                if (item != sender) item.Checked = false;

            UpdateDisplay();
        }
    }
}
