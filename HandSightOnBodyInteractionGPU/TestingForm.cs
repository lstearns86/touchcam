using System;
using System.Windows.Forms;

using HandSightLibrary;
using HandSightLibrary.ImageProcessing;
using System.IO;
using System.Collections.Generic;
using System.Drawing;

namespace HandSightOnBodyInteractionGPU
{
    public partial class TestingForm : Form
    {
        private string[] phases = { "Intro", "Consent", "Demographics", "Smartphone Usage", "Smartwatch Demonstration", "Smartwatch Questionnaire", "On-body Intro", "Prototype Setup", "Location Training", "Gesture Testing/Training", "Training Condition 1", "Testing Condition 1", "Questions Condition 1", "Training Condition 2", "Testing Condition 2", "Questions Condition 2", "Training Condition 3", "Testing Condition 3", "Questions Condition 3", "Final Questionnare", "Conclusion" };
        private int currPhase = -1;
        private int[] conditionOrder = { 0, 1, 2 };

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

            if (ParticipantIDChooser.Items.Contains(Properties.Settings.Default.LastParticipant))
                ParticipantIDChooser.SelectedItem = Properties.Settings.Default.LastParticipant;
            else
                ParticipantIDChooser.Text = Properties.Settings.Default.LastParticipant;

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
            List<string> items = new List<string>();
            items.Add("Timer");
            items.Add("Next Event");
            items.Add("Steps");
            items.Add("Message 3");
            items.Add("Voice Input");
            items.Add("Alarm");
            items.Add("Weather");
            items.Add("Heart Rate");
            items.Add("Message 1");
            items.Add("Voice Input");
            
            TaskChooser.Items.Clear();
            TaskChooser.Items.AddRange(items.ToArray());
            TaskChooser.SelectedIndex = 0;

            RandomizeTasks();
        }

        private void RandomizeTasks()
        {
            if (!Properties.Settings.Default.RandomizeOrders) return;

            List<string> tasks = new List<string>();
            foreach (string item in TaskChooser.Items)
                tasks.Add(item);
            tasks.Shuffle();

            TaskChooser.Items.Clear();
            TaskChooser.Items.AddRange(tasks.ToArray());
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
                }
            }
            else
            {
                if(runningParticipant)
                {
                    EnableSpeechCheckbox.Checked = false;
                    EnableApplicationDemoCheckbox.Checked = false;
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
                else if (ModeChooser.SelectedIndex + 1 < ModeChooser.Items.Count) ModeChooser.SelectedIndex++;

                StartStopTaskButton.Text = "Start";

                GestureActionMap.Reset();

                OnTaskFinished(TaskChooser.Text);
            }));
        }

        private void StartStopTaskButton_Click(object sender, EventArgs e)
        {
            taskStarted = !taskStarted;

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

        private void NextPhaseButton_Click(object sender, EventArgs e)
        {
            currPhase++;
            if (currPhase > phases.Length) currPhase = phases.Length;
            PrevPhaseButton.Enabled = currPhase > 0;
            NextPhaseButton.Enabled = currPhase < phases.Length;
            
            if(currPhase >= 0 && currPhase < phases.Length)
            {
                PhaseLabel.Text = phases[currPhase];
                Logging.LogUIEvent("Phase: " + phases[currPhase]);

                if (phases[currPhase] == "Location Training") ModeChooser.SelectedIndex = 0;
                else if (phases[currPhase] == "Gesture Testing/Training") ModeChooser.SelectedIndex = 1;
                else if (phases[currPhase].Contains("Condition 1")) ModeChooser.SelectedIndex = 2;
                else if (phases[currPhase].Contains("Condition 2")) ModeChooser.SelectedIndex = 3;
                else if (phases[currPhase].Contains("Condition 3")) ModeChooser.SelectedIndex = 4;
            }
            else if(currPhase >= phases.Length)
            {
                PhaseLabel.Text = "Finished";
                Logging.LogUIEvent("Phase: Finished");
            }
        }

        private void PrevPhaseButton_Click(object sender, EventArgs e)
        {
            currPhase--;
            if (currPhase < -1) currPhase = -1;
            PrevPhaseButton.Enabled = currPhase > 0;
            NextPhaseButton.Enabled = currPhase >= phases.Length;

            if (currPhase >= 0 && currPhase < phases.Length)
            {
                PhaseLabel.Text = phases[currPhase];
                Logging.LogUIEvent("Phase: " + phases[currPhase]);

                if (phases[currPhase] == "Location Training") ModeChooser.SelectedIndex = 0;
                else if (phases[currPhase] == "Gesture Testing/Training") ModeChooser.SelectedIndex = 1;
                else if (phases[currPhase].Contains("Condition 1")) ModeChooser.SelectedIndex = 2 + conditionOrder[0];
                else if (phases[currPhase].Contains("Condition 2")) ModeChooser.SelectedIndex = 2 + conditionOrder[1];
                else if (phases[currPhase].Contains("Condition 3")) ModeChooser.SelectedIndex = 2 + conditionOrder[2];
            }
            else if (currPhase < 0)
            {
                PhaseLabel.Text = "Beginning";
                Logging.LogUIEvent("Phase: Beginning");
            }
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
            }
            catch
            {
                ConditionOrderTextbox.BackColor = Color.LightPink;
            }
        }
    }
}
