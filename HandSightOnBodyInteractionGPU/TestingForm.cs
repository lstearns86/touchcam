using System;
using System.Windows.Forms;

using HandSightLibrary;
using HandSightLibrary.ImageProcessing;
using System.IO;
using System.Collections.Generic;

namespace HandSightOnBodyInteractionGPU
{
    public partial class TestingForm : Form
    {
        public bool HideFromList { get { return true; } }

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

            GestureActionMap.TaskCompleted += TaskFinished;
        }

        private void SetTasks()
        {
            TaskChooser.Items.Clear();
            TaskChooser.Items.Add(GestureActionMap.GetMenuItem("Clock Menu", 2));
            TaskChooser.Items.Add(GestureActionMap.GetMenuItem("Daily Summary Menu", 0));
            TaskChooser.Items.Add(GestureActionMap.GetMenuItem("Health and Activities Menu", 1));
            TaskChooser.Items.Add(GestureActionMap.GetMenuItem("Notifications Menu", 3));
            TaskChooser.Items.Add("Voice Input");
            TaskChooser.SelectedIndex = 0;
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
                if (Properties.Settings.Default.RandomizeOrders) GestureActionMap.RandomizeMenuItems();

                TaskChooser.SelectedIndex = 0;
            }
            GestureActionMap.Reset();
        }

        private void StartLogging()
        {
            string dir = Properties.Settings.Default.LogDirectory;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            string filename = ParticipantIDChooser.Text;
            if (File.Exists(Path.Combine(dir, filename + ".log")))
            {
                int index = 2;
                while (File.Exists(Path.Combine(dir, filename + index + ".log"))) index++;
                filename += index;
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
        public bool TaskStarted { get { return taskStarted; } }
        public void TaskFinished()
        {
            taskStarted = false;

            Logging.LogOtherEvent("Task finished: " + TaskChooser.Text);

            if (TaskChooser.SelectedIndex + 1 < TaskChooser.Items.Count) TaskChooser.SelectedIndex++;
            else if (ModeChooser.SelectedIndex + 1 < ModeChooser.Items.Count) ModeChooser.SelectedIndex++;

            StartStopTaskButton.Text = "Start";
        }

        private void StartStopTaskButton_Click(object sender, EventArgs e)
        {
            taskStarted = !taskStarted;

            Logging.LogOtherEvent("Task " + (taskStarted ? "started" : "stopped") + ": " + TaskChooser.Text);

            StartStopTaskButton.Text = taskStarted ? "Stop" : "Start";

            GestureActionMap.CurrentTask = TaskChooser.Text;
        }

        private void RandomizeCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.RandomizeOrders = RandomizeCheckbox.Checked;
            Properties.Settings.Default.Save();
        }
    }
}
