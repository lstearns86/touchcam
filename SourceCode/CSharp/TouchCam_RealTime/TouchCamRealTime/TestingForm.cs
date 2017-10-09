using System;
using System.Windows.Forms;

using TouchCamLibrary;
using TouchCamLibrary.ImageProcessing;
using System.IO;
using System.Collections.Generic;
using System.Drawing;

namespace TouchCam
{
    /// <summary>
    /// UI for controlling the test phase of the experiments (i.e., prompt participants to perform tasks for each condition)
    /// </summary>
    public partial class TestingForm : Form
    {
        private List<string> phases = new List<string>(new string[] { "Intro", "Consent", "Demographics", "Smartphone Questions", "Smartwatch Questions", "On-body Questions", "Break", "Prototype Setup", "Location Training", "Gesture Training", "Explaining Condition 1", "Practicing Condition 1", "Testing Condition 1", "Questions Condition 1", "Explaining Condition 2", "Practicing Condition 2", "Testing Condition 2", "Questions Condition 2", "Explaining Condition 3", "Practicing Condition 3", "Testing Condition 3", "Questions Condition 3", "Final Questions", "Conclusion" });
        private int currPhase = -1;
        private int[] conditionOrder = { 0, 1, 2 };
        private List<string> tasks = new List<string>(new string[] { "Timer", "Next Event", "Steps", "Message 3", "Voice Input", "Alarm", "Weather", "Heart Rate", "Message 1", "Voice Input" });

        private int tempPhase = -1;
        private bool recordSensors = false;
        public bool RecordSensors
        {
            get
            {
                //Invoke(new MethodInvoker(delegate
                //{
                    if (tempPhase != currPhase)
                    {
                        recordSensors = currPhase >= phases.IndexOf("Prototype Setup") && currPhase < phases.IndexOf("Final Questions");
                        tempPhase = currPhase;
                    }
                //}));

                return recordSensors;
            }
        }
        public string CurrentPhase { get { if (currPhase >= 0 && currPhase < phases.Count) return phases[currPhase]; else return "N/A"; } }

        private Dictionary<string, string> taskInstructions = new Dictionary<string, string> {
            { "Timer", "Please open the \"Clock\" app, and then find and double-tap the \"Timer\" item." },
            { "Next Event", "Please open the \"Daily Summary\" app, and then find and double-tap the \"Next Event\" item." },
            { "Steps", "Please open the \"Health and Activities\" app, and then find and double-tap the \"Steps\" item." },
            { "Message 3", "Please open the \"Notifications\" app, and then find and double-tap the \"message from Charlie\"." },
            { "Alarm", "Please open the \"Clock\" app, and then find and double-tap the \"Alarm\" item." },
            { "Weather", "Please open the \"Daily Summary\" app, and then find and double-tap the \"Weather\" item." },
            { "Heart Rate", "Please open the \"Health and Activities\" app, and then find and double-tap the \"Heart Rate\" item." },
            { "Message 1", "Please open the \"Notifications\" app, and then find and double-tap the \"missed phone call from Alice\"." },
            { "Voice Input", "Please find and double-tap the \"voice input\" app." },
        };

        public bool HideFromList { get { return true; } }

        public delegate void TaskStartedDelegate(string task);
        public event TaskStartedDelegate TaskStarted;
        private void OnTaskStarted(string task) { TaskStarted?.Invoke(task); }

        public delegate void TaskFinishedDelegate(string task);
        public event TaskFinishedDelegate TaskFinished;
        private void OnTaskFinished(string task) { TaskFinished?.Invoke(task); }

        private bool runningParticipant = false;

        public TestingForm()
        {
            InitializeComponent();

            Location = Properties.Settings.Default.TestingLocation;

            EnableSpeechCheckbox.Checked = Properties.Settings.Default.EnableSpeechOutput;
            EnableApplicationDemoCheckbox.Checked = Properties.Settings.Default.EnableApplicationDemos;
            FixedApplicationResonsesCheckbox.Checked = Properties.Settings.Default.FixedApplicationResponses;
            RandomizeCheckbox.Checked = Properties.Settings.Default.RandomizeOrders;
            foreach (string mode in GestureActionMap.Modes)
                ModeChooser.Items.Add("Testing: " + mode);
            ModeChooser.SelectedIndex = 0;
            LockToCurrentTaskCheckbox.Checked = Properties.Settings.Default.LockToCurrentTask;

            if (ParticipantIDChooser.Items.Contains(Properties.Settings.Default.LastParticipant))
                ParticipantIDChooser.SelectedItem = Properties.Settings.Default.LastParticipant;
            else
                ParticipantIDChooser.Text = Properties.Settings.Default.LastParticipant;

            ConditionOrderTextbox.Text = Properties.Settings.Default.ConditionOrder;

            SetTasks();

            GestureActionMap.TaskCompleted += FinishTask;

            DialogResult result = MessageBox.Show("Would you like to start a new participant", "New Participant?", MessageBoxButtons.YesNoCancel);
            if (result == DialogResult.Yes) SetupNewParticipant();
            if (result != DialogResult.Cancel)
            {
                StartParticipant();
                runningParticipant = true;
            }
        }

        public void SetupNewParticipant()
        {
            try
            {
                ParticipantIDChooser.SelectedIndex++;
            }
            catch { }
        }

        public void StartParticipant()
        {
            if (!Logging.Running)
            {
                StartStopLoggingButton.Text = "Stop Logging";
                StartLogging();
            }
            EnableSpeechCheckbox.Checked = false;
            EnableApplicationDemoCheckbox.Checked = false;
            FixedApplicationResonsesCheckbox.Checked = true;
            RandomizeCheckbox.Checked = true;
        }

        private void SetTasks()
        {

            //TaskChooser.Items.Add(GestureActionMap.GetMenuItem("Clock Menu", 2));
            //TaskChooser.Items.Add(GestureActionMap.GetMenuItem("Daily Summary Menu", 0));
            //TaskChooser.Items.Add(GestureActionMap.GetMenuItem("Health and Activities Menu", 1));
            //TaskChooser.Items.Add(GestureActionMap.GetMenuItem("Notifications Menu", 3));

            TaskChooser.Items.Clear();
            TaskChooser.Items.AddRange(tasks.ToArray());
            TaskChooser.SelectedIndex = 0;

            RandomizeTasks();
        }

        private void RandomizeTasks()
        {
            if (!Properties.Settings.Default.RandomizeOrders) return;

            string participant = ParticipantIDChooser.Text;
            int pid = 0;
            int.TryParse(participant.Replace("pilot", "").Replace("p", ""), out pid);
            int condition = ModeChooser.SelectedIndex - 2;
            int seed = pid * 10 + condition;
            List<string> tempTasks = new List<string>(tasks);
            tempTasks.Shuffle(seed);

            TaskChooser.Items.Clear();
            TaskChooser.Items.AddRange(tempTasks.ToArray());
            TaskChooser.SelectedIndex = 0;
        }

        private void FixedApplicationResonsesCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.FixedApplicationResponses = FixedApplicationResonsesCheckbox.Checked;
            Properties.Settings.Default.Save();
        }

        private void EnableApplicationDemoCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.EnableApplicationDemos = EnableApplicationDemoCheckbox.Checked;
            Properties.Settings.Default.Save();
        }

        private void EnableSpeechCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.EnableSpeechOutput = EnableSpeechCheckbox.Checked;
            Properties.Settings.Default.Save();
        }

        private void TaskChooser_SelectedIndexChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.CurrentTask = (string)TaskChooser.SelectedItem;
            TaskLabel.Text = "Task: (" + (TaskChooser.SelectedIndex + 1) + " / " + TaskChooser.Items.Count + ")";
            if (taskInstructions.ContainsKey((string)TaskChooser.SelectedItem)) TaskInstructionsLabel.Text = taskInstructions[(string)TaskChooser.SelectedItem];
        }

        private void TestingForm_Move(object sender, EventArgs e)
        {
            Properties.Settings.Default.TestingLocation = Location;
            Properties.Settings.Default.Save();
        }

        private void TestingForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.TestingVisible = false;
            Properties.Settings.Default.Save();
        }

        private void ResetMenuLocationButton_Click(object sender, EventArgs e)
        {
            GestureActionMap.Reset();
        }

        private void RandomizeSubmenusButton_Click(object sender, EventArgs e)
        {
            if(Properties.Settings.Default.RandomizeOrders) GestureActionMap.RandomizeMenuItems();
        }

        private void ResetMenusButton_Click(object sender, EventArgs e)
        {
            GestureActionMap.ResetMenus();
        }

        private void ModeChooser_SelectedIndexChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.TestingMode = (string)ModeChooser.SelectedItem;
            if (((string)ModeChooser.SelectedItem).Contains("Testing: "))
                Properties.Settings.Default.GestureMode = ((string)ModeChooser.SelectedItem).Replace("Testing: ", "");
            Properties.Settings.Default.Save();

            if(((string)ModeChooser.SelectedItem).Contains("Testing: "))
            {
                if (Properties.Settings.Default.RandomizeOrders)
                {
                    GestureActionMap.RandomizeMenuItems();
                    RandomizeTasks();
                }

                TaskChooser.SelectedIndex = 0;

                if(runningParticipant)
                {
                    EnableSpeechCheckbox.Checked = true;
                    EnableApplicationDemoCheckbox.Checked = true;
                    LockToCurrentTaskCheckbox.Checked = false;
                }
            }
            else
            {
                if(runningParticipant)
                {
                    EnableSpeechCheckbox.Checked = false;
                    EnableApplicationDemoCheckbox.Checked = false;
                    LockToCurrentTaskCheckbox.Checked = false;
                }
            }
            GestureActionMap.Reset();

            Logging.LogUIEvent("Mode Set: " + ModeChooser.SelectedItem);
        }

        private void StartLogging()
        {
            string dir = Properties.Settings.Default.LogDirectory;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            string filename = ParticipantIDChooser.Text;
            if (File.Exists(Path.Combine(dir, filename + ".log")))
            {
                int index = 2;
                while (File.Exists(Path.Combine(dir, filename + "_" + index + ".log"))) index++;
                filename += "_" + index;
            }

            Logging.Start(Path.Combine(dir, filename + ".log"));
        }

        private void StartStopLoggingButton_Click(object sender, EventArgs e)
        {
            if (Logging.Running)
            {
                StartStopLoggingButton.Text = "Start Logging";
                Logging.Stop();
            }
            else
            {
                StartStopLoggingButton.Text = "Stop Logging";

                StartLogging();
            }
        }

        private void ResetLoggingButton_Click(object sender, EventArgs e)
        {
            if(Logging.Running)
            {
                Logging.Stop();

                StartLogging();
            }
        }

        private void ParticipantIDChooser_SelectedIndexChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.LastParticipant = ParticipantIDChooser.Text;
            Properties.Settings.Default.Save();

            // TODO: reset logging?
            if (Properties.Settings.Default.RandomizeOrders) GestureActionMap.RandomizeMenuItems();
            SetTasks();
            RandomizeTasks();
        }

        private void ParticipantIDChooser_TextUpdate(object sender, EventArgs e)
        {
            Properties.Settings.Default.LastParticipant = ParticipantIDChooser.Text;
            Properties.Settings.Default.Save();

            // TODO: reset logging?
            if (Properties.Settings.Default.RandomizeOrders) GestureActionMap.RandomizeMenuItems();
            SetTasks();
            RandomizeTasks();
        }

        private void SetLoggingLocationButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.ShowNewFolderButton = true;
            dialog.SelectedPath = (new DirectoryInfo(Properties.Settings.Default.LogDirectory)).FullName;
            dialog.Description = "Select a location to save log files and videos";
            if(dialog.ShowDialog() == DialogResult.OK)
            {
                Properties.Settings.Default.LogDirectory = dialog.SelectedPath;
                Properties.Settings.Default.Save();
                ResetLoggingButton.PerformClick();
            }
        }

        bool taskStarted = false;
        public bool IsTaskStarted { get { return taskStarted; } }
        public void FinishTask()
        {
            taskStarted = false;

            Invoke(new MethodInvoker(delegate
            {
                Logging.LogUIEvent("Task finished: " + TaskChooser.Text);

                if (TaskChooser.SelectedIndex + 1 < TaskChooser.Items.Count) TaskChooser.SelectedIndex++;
                else { currPhase++; SetPhase(); }

                StartStopTaskButton.Text = "Start";

                GestureActionMap.Reset();

                OnTaskFinished(TaskChooser.Text);
            }));
        }

        private void StartStopTaskButton_Click(object sender, EventArgs e)
        {
            taskStarted = !taskStarted;

            LockToCurrentTaskCheckbox.Checked = true;

            int prevPhase = currPhase;
            while(currPhase < 0 || !phases[currPhase].Contains("Testing"))
            {
                currPhase++;
                if (currPhase >= phases.Count) { currPhase = prevPhase; break; }
            }
            SetPhase(currPhase != prevPhase);

            Logging.LogUIEvent("Task " + (taskStarted ? "started" : "stopped") + ": " + TaskChooser.Text);

            if(taskStarted) OnTaskStarted(TaskChooser.Text);

            StartStopTaskButton.Text = taskStarted ? "Stop" : "Start";

            GestureActionMap.Reset();
            GestureActionMap.CurrentTask = TaskChooser.Text;
        }

        private void RandomizeCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.RandomizeOrders = RandomizeCheckbox.Checked;
            Properties.Settings.Default.Save();
        }

        public void SetPhase(string phase)
        {
            int index = 0;
            while (index >= 0 && index < phases.Count && phases[index] != phase)
                index++;
            if(index >= 0 && index < phases.Count)
            {
                int prevPhase = currPhase;
                currPhase = index;
                SetPhase(currPhase != prevPhase);
            }
        }

        private void SetPhase(bool newPhase = true)
        {
            PrevPhaseButton.Enabled = currPhase > 0;
            NextPhaseButton.Enabled = currPhase <= phases.Count;

            if (currPhase >= 0 && currPhase < phases.Count)
            {
                PhaseLabel.Text = phases[currPhase];
                if(newPhase) Logging.LogUIEvent("Phase: " + phases[currPhase]);

                if (phases[currPhase] == "Location Training") ModeChooser.SelectedIndex = 0;
                else if (phases[currPhase] == "Gesture Testing/Training") ModeChooser.SelectedIndex = 1;
                else if (phases[currPhase].Contains("Condition 1")) ModeChooser.SelectedIndex = 2 + conditionOrder[0];
                else if (phases[currPhase].Contains("Condition 2")) ModeChooser.SelectedIndex = 2 + conditionOrder[1];
                else if (phases[currPhase].Contains("Condition 3")) ModeChooser.SelectedIndex = 2 + conditionOrder[2];
            }
            else if (currPhase >= phases.Count)
            {
                PhaseLabel.Text = "Finished";
                if (newPhase) Logging.LogUIEvent("Phase: Finished");
            }
            else if(currPhase < 0)
            {
                PhaseLabel.Text = "Beginning";
                if (newPhase) Logging.LogUIEvent("Phase: Beginning");
            }

            if((phases[currPhase].Contains("Practicing") || phases[currPhase].Contains("Testing")) && !(phases[currPhase].Contains("Location") || phases[currPhase].Contains("Gesture")))
            {
                EnableApplicationDemoCheckbox.Checked = true;
                EnableSpeechCheckbox.Checked = true;
                if (taskInstructions.ContainsKey((string)TaskChooser.SelectedItem)) TaskInstructionsLabel.Text = taskInstructions[(string)TaskChooser.SelectedItem];
            }
            else
            {
                EnableApplicationDemoCheckbox.Checked = false;
                EnableSpeechCheckbox.Checked = false;
            }
        }

        private void NextPhaseButton_Click(object sender, EventArgs e)
        {
            currPhase++;
            if (currPhase > phases.Count) currPhase = phases.Count;
            
            SetPhase();
        }

        private void PrevPhaseButton_Click(object sender, EventArgs e)
        {
            currPhase--;
            if (currPhase < -1) currPhase = -1;
            
            SetPhase();
        }

        private void ConditionOrderTextbox_TextChanged(object sender, EventArgs e)
        {
            ConditionOrderTextbox.BackColor = Color.White;
            try
            {
                string[] parts = ConditionOrderTextbox.Text.Split(',');
                if (parts.Length != 3) throw new Exception();

                int condition1 = int.Parse(parts[0].Trim());
                int condition2 = int.Parse(parts[1].Trim());
                int condition3 = int.Parse(parts[2].Trim());

                if (condition1 < 0 || condition1 >= 3 || condition2 < 0 || condition2 >= 3 || condition3 < 0 || condition3 >= 3 || condition1 == condition2 || condition1 == condition3 || condition2 == condition3) throw new Exception();

                conditionOrder[0] = condition1;
                conditionOrder[1] = condition2;
                conditionOrder[2] = condition3;

                Properties.Settings.Default.ConditionOrder = ConditionOrderTextbox.Text;
                Properties.Settings.Default.Save();
                Logging.LogUIEvent("Set Condition Order: " + ConditionOrderTextbox.Text);
            }
            catch
            {
                ConditionOrderTextbox.BackColor = Color.LightPink;
            }
        }

        private void LockToCurrentTaskButton_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.LockToCurrentTask = LockToCurrentTaskCheckbox.Checked;
            Properties.Settings.Default.Save();
        }
    }
}
