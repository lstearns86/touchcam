using System;
using System.Windows.Forms;

using System.Runtime.InteropServices;
using System.Drawing;

using HandSightLibrary;
using HandSightLibrary.ImageProcessing;
using System.Collections.Generic;
using System.IO;

namespace HandSightOnBodyInteractionGPU
{
    public partial class TrainingForm : Form
    {
        public bool HideFromList { get { return true; } }

        string profileName = null;
        bool unsaved = false;

        Dictionary<string, string[]> locations;
        string[] gestures;

        public delegate void TrainingDataUpdatedDelegate();
        public event TrainingDataUpdatedDelegate TrainingDataUpdated;
        private void OnTrainingDataUpdated() { TrainingDataUpdated?.Invoke(); }

        public delegate void RecordLocationDelegate(string coarseLocation, string fineLocation);
        public event RecordLocationDelegate RecordLocation;
        private void OnRecordLocation(string coarseLocation, string fineLocation) { RecordLocation?.Invoke(coarseLocation, fineLocation); }

        public delegate void AutoCaptureLocationDelegate(string coarseLocation, string fineLocation);
        public event AutoCaptureLocationDelegate AutoCaptureLocation;
        private void OnAutoCaptureLocation(string coarseLocation, string fineLocation) { AutoCaptureLocation?.Invoke(coarseLocation, fineLocation); }

        public delegate void StopAutoCapturingLocationDelegate();
        public event StopAutoCapturingLocationDelegate StopAutoCapturingLocation;
        private void OnStopAutoCapturingLocation() { StopAutoCapturingLocation?.Invoke(); }

        public delegate void RecordGestureDelegate(string gesture);
        public event RecordGestureDelegate RecordGesture;
        private void OnRecordGesture(string gesture) { RecordGesture?.Invoke(gesture); }

        public delegate void AutoCaptureGestureDelegate(string gesture);
        public event AutoCaptureGestureDelegate AutoCaptureGesture;
        private void OnAutoCaptureGesture(string gesture) { AutoCaptureGesture?.Invoke(gesture); }

        public delegate void StopAutoCapturingGesturesDelegate();
        public event StopAutoCapturingGesturesDelegate StopAutoCapturingGestures;
        private void OnStopAutoCapturingGestures() { StopAutoCapturingGestures?.Invoke(); }

        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        int MakeLong(short lowPart, short highPart)
        {
            return (int)(((ushort)lowPart) | (uint)(highPart << 16));
        }

        void ListView_SetSpacing(ListView listview, short cx, short cy)
        {
            const int LVM_FIRST = 0x1000;
            const int LVM_SETICONSPACING = LVM_FIRST + 53;
            // http://msdn.microsoft.com/en-us/library/bb761176(VS.85).aspx
            // minimum spacing = 4
            SendMessage(listview.Handle, LVM_SETICONSPACING,
            IntPtr.Zero, (IntPtr)MakeLong(cx, cy));

            // http://msdn.microsoft.com/en-us/library/bb775085(VS.85).aspx
            // DOESN'T WORK!
            // can't find ListView_SetIconSpacing in dll comctl32.dll
            //ListView_SetIconSpacing(listView.Handle, 5, 5);
        }

        private bool training = false;
        public bool Training { get { return training; } set { training = value; } }

        public TrainingForm()
        {
            InitializeComponent();

            Location = Properties.Settings.Default.TrainingLocation;
            Size = Properties.Settings.Default.TrainingSize;

            TimerChooser.Value = Properties.Settings.Default.CountdownTimer;

            LocationView.LargeImageList = new ImageList();
            LocationView.LargeImageList.ImageSize = new Size(120, 120);
            LocationView.LargeImageList.ColorDepth = ColorDepth.Depth24Bit;
            ListView_SetSpacing(LocationView, 120 + 12, 120 + 24);

            GestureView.LargeImageList = new ImageList();
            GestureView.LargeImageList.ImageSize = new Size(120, 120);
            GestureView.LargeImageList.ColorDepth = ColorDepth.Depth24Bit;
            ListView_SetSpacing(GestureView, 120 + 12, 120 + 24);

            locations = new Dictionary<string, string[]>();
            string[] locationLines = File.ReadAllLines("defaults/locations.txt");
            CoarseLocationChooser.Items.Clear();
            foreach (string locationLine in locationLines)
            {
                string[] parts = locationLine.Split(':');
                string coarseLocation = parts[0].Trim();
                string[] fineLocations = parts[1].Split(',');
                for (int i = 0; i < fineLocations.Length; i++) fineLocations[i] = fineLocations[i].Trim();
                locations.Add(coarseLocation, fineLocations);
                CoarseLocationChooser.Items.Add(coarseLocation);
            }
            gestures = File.ReadAllLines("defaults/gestures.txt");
            GestureChooser.Items.Clear();
            GestureChooser.Items.AddRange(gestures);

            CoarseLocationChooser.SelectedIndex = 0;
            GestureChooser.SelectedIndex = 0;

            IncludeGesturesInProfileCheckbox.Checked = Properties.Settings.Default.IncludeGesturesInProfile;

            UpdateLists();
        }

        private void TimerChooser_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.CountdownTimer = (int)TimerChooser.Value;
            Properties.Settings.Default.Save();
        }

        public void AddLocation(ImageTemplate template)
        {
            if (InvokeRequired) { Invoke(new MethodInvoker(delegate { AddLocation(template); })); return; }

            string location = template["coarse"] + " " + template["fine"];
            ListViewGroup group = LocationView.Groups[location];
            if(group == null) group = LocationView.Groups.Add(location, location);
            LocationView.LargeImageList.Images.Add(template.Image.Bitmap);
            ListViewItem item = new ListViewItem() { Text = location + " " + (group.Items.Count + 1), ImageIndex = LocationView.LargeImageList.Images.Count - 1, Tag = template };
            LocationView.Items.Add(item);
            item.Group = group;
            //LocationView.TopItem = item;
            item.EnsureVisible();

            LocationCountLabel.Text = Localization.Instance.GetNumTrainingExamples() + " location examples (" + Localization.Instance.GetNumTrainingClasses() + " classes)";
        }

        public void UpdateLocationList()
        {
            if (InvokeRequired) { Invoke(new MethodInvoker(UpdateLocationList)); return; }

            int locationTopItemIndex = 0;
            try
            {
                locationTopItemIndex = LocationView.TopItem.Index;
            }
            catch
            { }
            LocationView.BeginUpdate();

            LocationView.Items.Clear();
            LocationView.Groups.Clear();
            LocationView.LargeImageList.Images.Clear();
            
            // group and display location examples
            int imageIndex = 0;
            foreach (string fineLocation in Localization.Instance.samples.Keys)
            {
                string coarseLocation = Localization.Instance.coarseLocations[fineLocation];
                string location = coarseLocation + " " + fineLocation;
                ListViewGroup group = LocationView.Groups.Add(location, location);
                int templateIndex = 1;
                foreach (ImageTemplate template in Localization.Instance.samples[fineLocation])
                {
                    LocationView.LargeImageList.Images.Add(template.Image.Bitmap);
                    ListViewItem item = new ListViewItem() { Text = location + " " + (templateIndex++), ImageIndex = (imageIndex++), Tag = template };
                    LocationView.Items.Add(item);
                    item.Group = group;
                }
            }

            LocationCountLabel.Text = Localization.Instance.GetNumTrainingExamples() + " location examples (" + Localization.Instance.GetNumTrainingClasses() + " classes)";
            
            LocationView.EndUpdate();
            try
            {
                //LocationView.TopItem = LocationView.Items[locationTopItemIndex];
                LocationView.Items[locationTopItemIndex].EnsureVisible();
            }
            catch
            { }

            unsaved = true;
        }

        public void AddGesture(Gesture template)
        {
            if (InvokeRequired) { Invoke(new MethodInvoker(delegate { AddGesture(template); })); return; }

            float duration = template.CorrectedSensorReadings[template.CorrectedSensorReadings.Count - 1].Timestamp - template.CorrectedSensorReadings[0].Timestamp;
            duration /= 1000.0f;

            Bitmap img = template.Visualization;

            ListViewGroup group = GestureView.Groups[template.ClassName];
            if(group == null) group = GestureView.Groups.Add(template.ClassName, template.ClassName);
            GestureView.LargeImageList.Images.Add(img);
            ListViewItem item = new ListViewItem() { Text = template.ClassName + " " + (group.Items.Count + 1) + " (" + duration.ToString("0.0") + "s)", ImageIndex = (GestureView.LargeImageList.Images.Count - 1), Tag = template };
            GestureView.Items.Add(item);
            item.Group = group;

            //GestureView.TopItem = item;
            item.EnsureVisible();

            GestureCountLabel.Text = GestureRecognition.GetNumExamples() + " gesture examples (" + GestureRecognition.GetNumClasses() + " classes)";
        }

        public void UpdateGestureList()
        {
            if (InvokeRequired) { Invoke(new MethodInvoker(UpdateGestureList)); return; }

            int gestureTopItemIndex = 0;
            try
            {
                gestureTopItemIndex = GestureView.TopItem.Index;
            }
            catch { }
            GestureView.BeginUpdate();

            GestureView.Items.Clear();
            GestureView.Groups.Clear();
            GestureView.LargeImageList.Images.Clear();

            // group and display gesture examples
            int imageIndex = 0;
            foreach (string gestureName in GestureRecognition.samples.Keys)
            {
                ListViewGroup group = GestureView.Groups.Add(gestureName, gestureName);
                int templateIndex = 1;
                foreach (Gesture template in GestureRecognition.samples[gestureName])
                {
                    float duration = template.CorrectedSensorReadings[template.CorrectedSensorReadings.Count - 1].Timestamp - template.CorrectedSensorReadings[0].Timestamp;
                    duration /= 1000.0f;

                    Bitmap img = template.Visualization;

                    GestureView.LargeImageList.Images.Add(img);
                    ListViewItem item = new ListViewItem() { Text = gestureName + " " + (templateIndex++) + " (" + duration.ToString("0.0") + "s)", ImageIndex = (imageIndex++), Tag = template };
                    GestureView.Items.Add(item);
                    item.Group = group;
                }
            }

            GestureCountLabel.Text = GestureRecognition.GetNumExamples() + " gesture examples (" + GestureRecognition.GetNumClasses() + " classes)";

            GestureView.EndUpdate();
            try
            {
                //GestureView.TopItem = GestureView.Items[gestureTopItemIndex];
                GestureView.Items[gestureTopItemIndex].EnsureVisible();
            }
            catch { }

            unsaved = true;
        }
        
        public void UpdateLists()
        {
            UpdateLocationList();
            UpdateGestureList();
        }

        private void TrainingForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
            Properties.Settings.Default.TrainingVisible = false;
            Properties.Settings.Default.Save();
        }

        private void LocationView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                RemoveButton.PerformClick();
            }
        }

        private void RemoveButton_Click(object sender, EventArgs e)
        {
            if (TrainingDataViewer.SelectedIndex == 0) // location tab is showing
            {
                foreach(ListViewItem removeItem in LocationView.SelectedItems)
                {
                    int imageIndex = removeItem.ImageIndex;
                    ImageTemplate template = (ImageTemplate)removeItem.Tag;
                    Localization.Instance.RemoveTrainingExample(template);
                    LocationView.LargeImageList.Images.RemoveAt(imageIndex);
                    foreach(ListViewItem updateItem in LocationView.Items)
                    {
                        if (updateItem.ImageIndex > imageIndex) updateItem.ImageIndex--;
                    }

                    if (removeItem.Group.Items.Count == 0)
                        LocationView.Groups.Remove(removeItem.Group);
                    LocationView.Items.Remove(removeItem);
                }

                training = true;
                Localization.Instance.Train();
                training = false;
                OnTrainingDataUpdated();
            }
            else // gesture tab is showing
            {
                foreach (ListViewItem removeItem in GestureView.SelectedItems)
                {
                    int imageIndex = removeItem.ImageIndex;
                    Gesture template = (Gesture)removeItem.Tag;
                    
                    GestureRecognition.RemoveTrainingExample(template);
                    GestureView.LargeImageList.Images.RemoveAt(imageIndex);
                    foreach (ListViewItem updateItem in GestureView.Items)
                    {
                        if (updateItem.ImageIndex > imageIndex) updateItem.ImageIndex--;
                    }

                    if (removeItem.Group.Items.Count == 0)
                        GestureView.Groups.Remove(removeItem.Group);
                    GestureView.Items.Remove(removeItem);
                }

                training = true;
                GestureRecognition.Train();
                training = false;
                OnTrainingDataUpdated();
            }
        }

        private void GestureView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                RemoveButton.PerformClick();
            }
        }

        private void CoarseLocationChooser_SelectedIndexChanged(object sender, EventArgs e)
        {
            FineLocationChooser.Items.Clear();
            FineLocationChooser.Items.AddRange(locations[(string)CoarseLocationChooser.SelectedItem]);
            FineLocationChooser.SelectedIndex = 0;
        }

        private void RecordLocationButton_Click(object sender, EventArgs e)
        {
            OnRecordLocation((string)CoarseLocationChooser.SelectedItem, (string)FineLocationChooser.SelectedItem);
        }

        private void RecordGestureButton_Click(object sender, EventArgs e)
        {
            OnRecordGesture((string)GestureChooser.SelectedItem);
        }

        private void SaveProfileButton_Click(object sender, EventArgs e)
        {
            bool overwrite = false;
            if(profileName != null)
            {
                DialogResult result = MessageBox.Show("Would you like to overwrite the existing profile named \"" + profileName + "\"", "Overwrite Existing Profile?", MessageBoxButtons.YesNoCancel);
                if (result == DialogResult.Cancel) return;
                else if (result == DialogResult.Yes)
                {
                    overwrite = true;
                    Directory.Delete(Path.Combine("savedProfiles", profileName), true);
                }
            }

            if(!overwrite)
            {
                PromptDialog dialog = new PromptDialog("Enter Profile Name", "Save");
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    if (Directory.Exists(Path.Combine("savedProfiles", dialog.Value)))
                    {
                        if (MessageBox.Show("A profile with that name already exists. Would you like to overwrite it?", "Confirm overwrite profile", MessageBoxButtons.YesNoCancel) != DialogResult.Yes)
                            return;
                        Directory.Delete(Path.Combine("savedProfiles", dialog.Value), true);
                    }

                    profileName = dialog.Value;
                }
            }

            Localization.Instance.Save(profileName);

            if (Properties.Settings.Default.IncludeGesturesInProfile)
                GestureRecognition.Save(profileName);

            unsaved = false;
        }

        private void LoadProfileButton_Click(object sender, EventArgs e)
        {
            List<string> profiles = new List<string>();
            foreach (string dir in Directory.GetDirectories("savedProfiles"))
                profiles.Add((new DirectoryInfo(dir)).Name);
            if(profiles.Contains("default")) profiles.Remove("default");
            if (profiles.Count == 0)
            {
                MessageBox.Show("Error: no saved profiles!");
                return;
            }
            SelectFromListDialog dialog = new SelectFromListDialog("Select Saved Profile", "Load", profiles);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                profileName = dialog.SelectedItem;

                training = true;
                Localization.Instance.Reset();
                Localization.Instance.Load(dialog.SelectedItem);

                if (Properties.Settings.Default.IncludeGesturesInProfile)
                {
                    GestureRecognition.Reset();
                    GestureRecognition.Load(dialog.SelectedItem);
                }
                training = false;

                UpdateLists();

                unsaved = false;
            }
        }

        private void IncludeGesturesInProfileCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.IncludeGesturesInProfile = IncludeGesturesInProfileCheckbox.Checked;
            Properties.Settings.Default.Save();
        }

        private void SaveDefaultGesturesButton_Click(object sender, EventArgs e)
        {
            GestureRecognition.Save("default", false);
        }

        private void LoadDefaultGesturesButton_Click(object sender, EventArgs e)
        {
            try
            {
                GestureRecognition.Load("default");
                UpdateLists();
            }
            catch
            {
                MessageBox.Show("Error: could not load default gestures");
            }
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            training = true;
            Localization.Instance.Reset();
            GestureRecognition.Reset();
            training = false;
            UpdateLists();
        }

        private void TrainingForm_Move(object sender, EventArgs e)
        {
            Properties.Settings.Default.TrainingLocation = Location;
            Properties.Settings.Default.Save();
        }

        private void TrainingForm_Resize(object sender, EventArgs e)
        {
            Properties.Settings.Default.TrainingSize = Size;
            Properties.Settings.Default.Save();
        }

        bool autoGesture = false, autoLocation = false;
        private void AutoCaptureGestureButton_Click(object sender, EventArgs e)
        {
            autoGesture = !autoGesture;
            if (autoGesture)
            {
                OnAutoCaptureGesture((string)GestureChooser.SelectedItem);
                AutoCaptureGestureButton.Text = "Stop";
            }
            else
            {
                OnStopAutoCapturingGestures();
                AutoCaptureGestureButton.Text = "Auto";
            }
        }

        private void AutoCaptureLocationButton_Click(object sender, EventArgs e)
        {
            autoLocation = !autoLocation;
            if (autoLocation)
            {
                OnAutoCaptureLocation((string)CoarseLocationChooser.SelectedItem, (string)FineLocationChooser.SelectedItem);
                AutoCaptureLocationButton.Text = "Stop";
            }
            else
            {
                OnStopAutoCapturingLocation();
                AutoCaptureLocationButton.Text = "Auto";
            }
        }

        public void StopAutoCapture()
        {
            autoLocation = false;
            autoGesture = false;
            AutoCaptureGestureButton.Text = "Auto";
            AutoCaptureLocationButton.Text = "Auto";
        }
    }
}
