namespace HandSightOnBodyInteractionGPU
{
    partial class TrainingForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TrainingForm));
            this.panel1 = new System.Windows.Forms.Panel();
            this.ResetGesturesButton = new System.Windows.Forms.Button();
            this.AutoCaptureGestureButton = new System.Windows.Forms.Button();
            this.AutoCaptureLocationButton = new System.Windows.Forms.Button();
            this.GestureCountLabel = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.TimerChooser = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.OverwriteExistingSamplesCheckbox = new System.Windows.Forms.CheckBox();
            this.LoadAllSavedGesturesButton = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.GestureChooser = new System.Windows.Forms.ComboBox();
            this.RecordGestureButton = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.FineLocationChooser = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.LoadProfileButton = new System.Windows.Forms.Button();
            this.SaveProfileButton = new System.Windows.Forms.Button();
            this.LocationCountLabel = new System.Windows.Forms.Label();
            this.ResetLocationsButton = new System.Windows.Forms.Button();
            this.RecordLocationButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.CoarseLocationChooser = new System.Windows.Forms.ComboBox();
            this.TrainingDataViewer = new System.Windows.Forms.TabControl();
            this.LocationTab = new System.Windows.Forms.TabPage();
            this.LocationView = new System.Windows.Forms.ListView();
            this.GestureTab = new System.Windows.Forms.TabPage();
            this.GestureView = new System.Windows.Forms.ListView();
            this.RemoveButton = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TimerChooser)).BeginInit();
            this.TrainingDataViewer.SuspendLayout();
            this.LocationTab.SuspendLayout();
            this.GestureTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.ResetGesturesButton);
            this.panel1.Controls.Add(this.AutoCaptureGestureButton);
            this.panel1.Controls.Add(this.AutoCaptureLocationButton);
            this.panel1.Controls.Add(this.GestureCountLabel);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.TimerChooser);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.OverwriteExistingSamplesCheckbox);
            this.panel1.Controls.Add(this.LoadAllSavedGesturesButton);
            this.panel1.Controls.Add(this.label8);
            this.panel1.Controls.Add(this.GestureChooser);
            this.panel1.Controls.Add(this.RecordGestureButton);
            this.panel1.Controls.Add(this.label7);
            this.panel1.Controls.Add(this.FineLocationChooser);
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.LoadProfileButton);
            this.panel1.Controls.Add(this.SaveProfileButton);
            this.panel1.Controls.Add(this.LocationCountLabel);
            this.panel1.Controls.Add(this.ResetLocationsButton);
            this.panel1.Controls.Add(this.RecordLocationButton);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.CoarseLocationChooser);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(202, 648);
            this.panel1.TabIndex = 0;
            // 
            // ResetGesturesButton
            // 
            this.ResetGesturesButton.Location = new System.Drawing.Point(106, 600);
            this.ResetGesturesButton.Name = "ResetGesturesButton";
            this.ResetGesturesButton.Size = new System.Drawing.Size(86, 36);
            this.ResetGesturesButton.TabIndex = 49;
            this.ResetGesturesButton.Text = "Reset Gestures";
            this.ResetGesturesButton.UseVisualStyleBackColor = true;
            this.ResetGesturesButton.Click += new System.EventHandler(this.ResetGesturesButton_Click);
            // 
            // AutoCaptureGestureButton
            // 
            this.AutoCaptureGestureButton.Location = new System.Drawing.Point(153, 298);
            this.AutoCaptureGestureButton.Name = "AutoCaptureGestureButton";
            this.AutoCaptureGestureButton.Size = new System.Drawing.Size(41, 48);
            this.AutoCaptureGestureButton.TabIndex = 48;
            this.AutoCaptureGestureButton.Text = "Auto";
            this.AutoCaptureGestureButton.UseVisualStyleBackColor = true;
            this.AutoCaptureGestureButton.Click += new System.EventHandler(this.AutoCaptureGestureButton_Click);
            // 
            // AutoCaptureLocationButton
            // 
            this.AutoCaptureLocationButton.Location = new System.Drawing.Point(151, 136);
            this.AutoCaptureLocationButton.Name = "AutoCaptureLocationButton";
            this.AutoCaptureLocationButton.Size = new System.Drawing.Size(41, 48);
            this.AutoCaptureLocationButton.TabIndex = 47;
            this.AutoCaptureLocationButton.Text = "Auto";
            this.AutoCaptureLocationButton.UseVisualStyleBackColor = true;
            this.AutoCaptureLocationButton.Click += new System.EventHandler(this.AutoCaptureLocationButton_Click);
            // 
            // GestureCountLabel
            // 
            this.GestureCountLabel.Location = new System.Drawing.Point(12, 349);
            this.GestureCountLabel.Name = "GestureCountLabel";
            this.GestureCountLabel.Size = new System.Drawing.Size(180, 22);
            this.GestureCountLabel.TabIndex = 46;
            this.GestureCountLabel.Text = "0 training examples (0 classes)";
            this.GestureCountLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 14);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(36, 13);
            this.label2.TabIndex = 43;
            this.label2.Text = "Timer:";
            // 
            // TimerChooser
            // 
            this.TimerChooser.Location = new System.Drawing.Point(54, 12);
            this.TimerChooser.Maximum = new decimal(new int[] {
            15,
            0,
            0,
            0});
            this.TimerChooser.Name = "TimerChooser";
            this.TimerChooser.Size = new System.Drawing.Size(75, 20);
            this.TimerChooser.TabIndex = 44;
            this.TimerChooser.ValueChanged += new System.EventHandler(this.TimerChooser_ValueChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(135, 14);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(47, 13);
            this.label3.TabIndex = 45;
            this.label3.Text = "seconds";
            // 
            // OverwriteExistingSamplesCheckbox
            // 
            this.OverwriteExistingSamplesCheckbox.AutoSize = true;
            this.OverwriteExistingSamplesCheckbox.Location = new System.Drawing.Point(15, 478);
            this.OverwriteExistingSamplesCheckbox.Name = "OverwriteExistingSamplesCheckbox";
            this.OverwriteExistingSamplesCheckbox.Size = new System.Drawing.Size(153, 17);
            this.OverwriteExistingSamplesCheckbox.TabIndex = 42;
            this.OverwriteExistingSamplesCheckbox.Text = "Overwrite Existing Samples";
            this.OverwriteExistingSamplesCheckbox.UseVisualStyleBackColor = true;
            this.OverwriteExistingSamplesCheckbox.CheckedChanged += new System.EventHandler(this.OverwriteExistingSamplesCheckbox_CheckedChanged);
            // 
            // LoadAllSavedGesturesButton
            // 
            this.LoadAllSavedGesturesButton.Location = new System.Drawing.Point(12, 555);
            this.LoadAllSavedGesturesButton.Name = "LoadAllSavedGesturesButton";
            this.LoadAllSavedGesturesButton.Size = new System.Drawing.Size(180, 39);
            this.LoadAllSavedGesturesButton.TabIndex = 41;
            this.LoadAllSavedGesturesButton.Text = "Load All Saved Gestures";
            this.LoadAllSavedGesturesButton.UseVisualStyleBackColor = true;
            this.LoadAllSavedGesturesButton.Click += new System.EventHandler(this.LoadAllSavedGesturesButton_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(13, 274);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(47, 13);
            this.label8.TabIndex = 39;
            this.label8.Text = "Gesture:";
            // 
            // GestureChooser
            // 
            this.GestureChooser.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.GestureChooser.FormattingEnabled = true;
            this.GestureChooser.Items.AddRange(new object[] {
            "Tap",
            "Double Tap",
            "Swipe Left",
            "Swipe Right",
            "Swipe Up",
            "Swipe Down",
            "Circle",
            "Triangle",
            "Square"});
            this.GestureChooser.Location = new System.Drawing.Point(65, 271);
            this.GestureChooser.Name = "GestureChooser";
            this.GestureChooser.Size = new System.Drawing.Size(129, 21);
            this.GestureChooser.TabIndex = 38;
            // 
            // RecordGestureButton
            // 
            this.RecordGestureButton.Location = new System.Drawing.Point(14, 298);
            this.RecordGestureButton.Name = "RecordGestureButton";
            this.RecordGestureButton.Size = new System.Drawing.Size(131, 48);
            this.RecordGestureButton.TabIndex = 37;
            this.RecordGestureButton.Text = "Record Gesture";
            this.RecordGestureButton.UseVisualStyleBackColor = true;
            this.RecordGestureButton.Click += new System.EventHandler(this.RecordGestureButton_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(11, 112);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(30, 13);
            this.label7.TabIndex = 36;
            this.label7.Text = "Fine:";
            // 
            // FineLocationChooser
            // 
            this.FineLocationChooser.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.FineLocationChooser.FormattingEnabled = true;
            this.FineLocationChooser.Location = new System.Drawing.Point(63, 109);
            this.FineLocationChooser.Name = "FineLocationChooser";
            this.FineLocationChooser.Size = new System.Drawing.Size(129, 21);
            this.FineLocationChooser.TabIndex = 35;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(11, 85);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(43, 13);
            this.label6.TabIndex = 34;
            this.label6.Text = "Coarse:";
            // 
            // LoadProfileButton
            // 
            this.LoadProfileButton.Location = new System.Drawing.Point(106, 501);
            this.LoadProfileButton.Name = "LoadProfileButton";
            this.LoadProfileButton.Size = new System.Drawing.Size(86, 48);
            this.LoadProfileButton.TabIndex = 33;
            this.LoadProfileButton.Text = "Load Profile";
            this.LoadProfileButton.UseVisualStyleBackColor = true;
            this.LoadProfileButton.Click += new System.EventHandler(this.LoadProfileButton_Click);
            // 
            // SaveProfileButton
            // 
            this.SaveProfileButton.Location = new System.Drawing.Point(12, 501);
            this.SaveProfileButton.Name = "SaveProfileButton";
            this.SaveProfileButton.Size = new System.Drawing.Size(86, 48);
            this.SaveProfileButton.TabIndex = 32;
            this.SaveProfileButton.Text = "Save Profile";
            this.SaveProfileButton.UseVisualStyleBackColor = true;
            this.SaveProfileButton.Click += new System.EventHandler(this.SaveProfileButton_Click);
            // 
            // LocationCountLabel
            // 
            this.LocationCountLabel.Location = new System.Drawing.Point(12, 187);
            this.LocationCountLabel.Name = "LocationCountLabel";
            this.LocationCountLabel.Size = new System.Drawing.Size(180, 21);
            this.LocationCountLabel.TabIndex = 31;
            this.LocationCountLabel.Text = "0 training examples (0 classes)";
            this.LocationCountLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // ResetLocationsButton
            // 
            this.ResetLocationsButton.Location = new System.Drawing.Point(12, 600);
            this.ResetLocationsButton.Name = "ResetLocationsButton";
            this.ResetLocationsButton.Size = new System.Drawing.Size(86, 36);
            this.ResetLocationsButton.TabIndex = 30;
            this.ResetLocationsButton.Text = "Reset Locations";
            this.ResetLocationsButton.UseVisualStyleBackColor = true;
            this.ResetLocationsButton.Click += new System.EventHandler(this.ResetLocationsButton_Click);
            // 
            // RecordLocationButton
            // 
            this.RecordLocationButton.Location = new System.Drawing.Point(12, 136);
            this.RecordLocationButton.Name = "RecordLocationButton";
            this.RecordLocationButton.Size = new System.Drawing.Size(133, 48);
            this.RecordLocationButton.TabIndex = 29;
            this.RecordLocationButton.Text = "Record Location";
            this.RecordLocationButton.UseVisualStyleBackColor = true;
            this.RecordLocationButton.Click += new System.EventHandler(this.RecordLocationButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 66);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(92, 13);
            this.label1.TabIndex = 28;
            this.label1.Text = "Training Location:";
            // 
            // CoarseLocationChooser
            // 
            this.CoarseLocationChooser.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CoarseLocationChooser.FormattingEnabled = true;
            this.CoarseLocationChooser.Location = new System.Drawing.Point(63, 82);
            this.CoarseLocationChooser.Name = "CoarseLocationChooser";
            this.CoarseLocationChooser.Size = new System.Drawing.Size(129, 21);
            this.CoarseLocationChooser.TabIndex = 27;
            this.CoarseLocationChooser.SelectedIndexChanged += new System.EventHandler(this.CoarseLocationChooser_SelectedIndexChanged);
            // 
            // TrainingDataViewer
            // 
            this.TrainingDataViewer.Controls.Add(this.LocationTab);
            this.TrainingDataViewer.Controls.Add(this.GestureTab);
            this.TrainingDataViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TrainingDataViewer.Location = new System.Drawing.Point(202, 0);
            this.TrainingDataViewer.Name = "TrainingDataViewer";
            this.TrainingDataViewer.SelectedIndex = 0;
            this.TrainingDataViewer.Size = new System.Drawing.Size(755, 613);
            this.TrainingDataViewer.TabIndex = 1;
            // 
            // LocationTab
            // 
            this.LocationTab.Controls.Add(this.LocationView);
            this.LocationTab.Location = new System.Drawing.Point(4, 22);
            this.LocationTab.Name = "LocationTab";
            this.LocationTab.Padding = new System.Windows.Forms.Padding(3);
            this.LocationTab.Size = new System.Drawing.Size(747, 587);
            this.LocationTab.TabIndex = 0;
            this.LocationTab.Text = "Locations";
            this.LocationTab.UseVisualStyleBackColor = true;
            // 
            // LocationView
            // 
            this.LocationView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LocationView.Location = new System.Drawing.Point(3, 3);
            this.LocationView.Name = "LocationView";
            this.LocationView.Size = new System.Drawing.Size(741, 581);
            this.LocationView.TabIndex = 0;
            this.LocationView.UseCompatibleStateImageBehavior = false;
            this.LocationView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.LocationView_KeyDown);
            // 
            // GestureTab
            // 
            this.GestureTab.Controls.Add(this.GestureView);
            this.GestureTab.Location = new System.Drawing.Point(4, 22);
            this.GestureTab.Name = "GestureTab";
            this.GestureTab.Padding = new System.Windows.Forms.Padding(3);
            this.GestureTab.Size = new System.Drawing.Size(747, 587);
            this.GestureTab.TabIndex = 1;
            this.GestureTab.Text = "Gestures";
            this.GestureTab.UseVisualStyleBackColor = true;
            // 
            // GestureView
            // 
            this.GestureView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GestureView.Location = new System.Drawing.Point(3, 3);
            this.GestureView.Name = "GestureView";
            this.GestureView.Size = new System.Drawing.Size(741, 581);
            this.GestureView.TabIndex = 0;
            this.GestureView.UseCompatibleStateImageBehavior = false;
            this.GestureView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GestureView_KeyDown);
            // 
            // RemoveButton
            // 
            this.RemoveButton.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.RemoveButton.Location = new System.Drawing.Point(202, 613);
            this.RemoveButton.Name = "RemoveButton";
            this.RemoveButton.Size = new System.Drawing.Size(755, 35);
            this.RemoveButton.TabIndex = 43;
            this.RemoveButton.Text = "Remove Training Example";
            this.RemoveButton.UseVisualStyleBackColor = true;
            this.RemoveButton.Click += new System.EventHandler(this.RemoveButton_Click);
            // 
            // TrainingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(957, 648);
            this.Controls.Add(this.TrainingDataViewer);
            this.Controls.Add(this.RemoveButton);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "TrainingForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Training";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TrainingForm_FormClosing);
            this.Move += new System.EventHandler(this.TrainingForm_Move);
            this.Resize += new System.EventHandler(this.TrainingForm_Resize);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TimerChooser)).EndInit();
            this.TrainingDataViewer.ResumeLayout(false);
            this.LocationTab.ResumeLayout(false);
            this.GestureTab.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.CheckBox OverwriteExistingSamplesCheckbox;
        private System.Windows.Forms.Button LoadAllSavedGesturesButton;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox GestureChooser;
        private System.Windows.Forms.Button RecordGestureButton;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox FineLocationChooser;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button LoadProfileButton;
        private System.Windows.Forms.Button SaveProfileButton;
        private System.Windows.Forms.Label LocationCountLabel;
        private System.Windows.Forms.Button ResetLocationsButton;
        private System.Windows.Forms.Button RecordLocationButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox CoarseLocationChooser;
        private System.Windows.Forms.TabControl TrainingDataViewer;
        private System.Windows.Forms.TabPage LocationTab;
        private System.Windows.Forms.TabPage GestureTab;
        private System.Windows.Forms.Button RemoveButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown TimerChooser;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label GestureCountLabel;
        private System.Windows.Forms.ListView LocationView;
        private System.Windows.Forms.ListView GestureView;
        private System.Windows.Forms.Button AutoCaptureGestureButton;
        private System.Windows.Forms.Button AutoCaptureLocationButton;
        private System.Windows.Forms.Button ResetGesturesButton;
    }
}