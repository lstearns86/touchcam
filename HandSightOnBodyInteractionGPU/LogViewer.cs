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

using HandSightLibrary;

namespace HandSightOnBodyInteractionGPU
{
    public partial class LogViewer : Form
    {
        List<Logging.LogEvent> events = new List<Logging.LogEvent>();

        public LogViewer()
        {
            InitializeComponent();
        }

        private void Checkbox_CheckedChanged(object sender, EventArgs e)
        {
            FilterEvents();
        }

        private void FilterEvents()
        {
            if (InvokeRequired) Invoke(new MethodInvoker(FilterEvents));

            List<Logging.LogEvent> filteredEvents = new List<Logging.LogEvent>();
            foreach(Logging.LogEvent e in events)
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
                    if (UICheckbox.Checked)
                        filteredEvents.Add(e);
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
                else if(e is Logging.HardwareEvent)
                {
                    if (HardwareCheckbox.Checked)
                        filteredEvents.Add(e);
                }
                else if (OtherCheckbox.Checked)
                    filteredEvents.Add(e);
            }

            DataBox.Items.Clear();
            float startTime = events != null && events.Count > 0 ? events[0].timestamp : 0;
            foreach(Logging.LogEvent e in filteredEvents)
            {
                if (DataBox.Items.Count < 1000)
                {
                    string text = e.ToJson();
                    float t = e.timestamp;
                    text = (t / 1000).ToString("0.0") + " s: " + text;
                    DataBox.Items.Add(text);
                }
            }

            EventCountLabel.Text = filteredEvents.Count + " / " + events.Count;
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
                events = Logging.ReadLog(path, progressAction);

                Invoke(new MethodInvoker(delegate { Progress.Value = 0; }));

                FilterEvents();
            });
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Select Log File to Open";
            dialog.InitialDirectory = Path.GetFullPath("logs");
            dialog.Filter = "Log Files|*.log";
            if(dialog.ShowDialog(this) == DialogResult.OK)
            {
                events.Clear();
                LoadFile(dialog.FileName);
            }
        }

        private void appendToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Select Log File to Append";
            dialog.InitialDirectory = Path.GetFullPath("logs");
            dialog.Filter = "Log Files|*.log";
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                LoadFile(dialog.FileName);
            }
        }
    }
}
