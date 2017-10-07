namespace TouchCam
{
    partial class TestingForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TestingForm));
            this.EnableApplicationDemoCheckbox = new System.Windows.Forms.CheckBox();
            this.FixedApplicationResonsesCheckbox = new System.Windows.Forms.CheckBox();
            this.EnableSpeechCheckbox = new System.Windows.Forms.CheckBox();
            this.TaskChooser = new System.Windows.Forms.ComboBox();
            this.TaskLabel = new System.Windows.Forms.Label();
            this.ResetMenuLocationButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.ModeChooser = new System.Windows.Forms.ComboBox();
            this.StartStopLoggingButton = new System.Windows.Forms.Button();
            this.ResetLoggingButton = new System.Windows.Forms.Button();
            this.SetLoggingLocationButton = new System.Windows.Forms.Button();
            this.ParticipantIDChooser = new System.Windows.Forms.ComboBox();
            this.RandomizeSubmenusButton = new System.Windows.Forms.Button();
            this.ResetMenusButton = new System.Windows.Forms.Button();
            this.StartStopTaskButton = new System.Windows.Forms.Button();
            this.RandomizeCheckbox = new System.Windows.Forms.CheckBox();
            this.PrevPhaseButton = new System.Windows.Forms.Button();
            this.NextPhaseButton = new System.Windows.Forms.Button();
            this.PhaseLabel = new System.Windows.Forms.Label();
            this.ConditionOrderTextbox = new System.Windows.Forms.TextBox();
            this.NextPhaseLabel = new System.Windows.Forms.Label();
            this.LockToCurrentTaskCheckbox = new System.Windows.Forms.CheckBox();
            this.TaskInstructionsLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // EnableApplicationDemoCheckbox
            // 
            this.EnableApplicationDemoCheckbox.AutoSize = true;
            this.EnableApplicationDemoCheckbox.Location = new System.Drawing.Point(213, 96);
            this.EnableApplicationDemoCheckbox.Name = "EnableApplicationDemoCheckbox";
            this.EnableApplicationDemoCheckbox.Size = new System.Drawing.Size(150, 17);
            this.EnableApplicationDemoCheckbox.TabIndex = 26;
            this.EnableApplicationDemoCheckbox.Text = "Enable Application Demos";
            this.EnableApplicationDemoCheckbox.UseVisualStyleBackColor = true;
            this.EnableApplicationDemoCheckbox.CheckedChanged += new System.EventHandler(this.EnableApplicationDemoCheckbox_CheckedChanged);
            // 
            // FixedApplicationResonsesCheckbox
            // 
            this.FixedApplicationResonsesCheckbox.AutoSize = true;
            this.FixedApplicationResonsesCheckbox.Location = new System.Drawing.Point(213, 119);
            this.FixedApplicationResonsesCheckbox.Name = "FixedApplicationResonsesCheckbox";
            this.FixedApplicationResonsesCheckbox.Size = new System.Drawing.Size(162, 17);
            this.FixedApplicationResonsesCheckbox.TabIndex = 27;
            this.FixedApplicationResonsesCheckbox.Text = "Fixed Application Responses";
            this.FixedApplicationResonsesCheckbox.UseVisualStyleBackColor = true;
            this.FixedApplicationResonsesCheckbox.CheckedChanged += new System.EventHandler(this.FixedApplicationResonsesCheckbox_CheckedChanged);
            // 
            // EnableSpeechCheckbox
            // 
            this.EnableSpeechCheckbox.AutoSize = true;
            this.EnableSpeechCheckbox.Location = new System.Drawing.Point(213, 73);
            this.EnableSpeechCheckbox.Name = "EnableSpeechCheckbox";
            this.EnableSpeechCheckbox.Size = new System.Drawing.Size(134, 17);
            this.EnableSpeechCheckbox.TabIndex = 28;
            this.EnableSpeechCheckbox.Text = "Enable Speech Output";
            this.EnableSpeechCheckbox.UseVisualStyleBackColor = true;
            this.EnableSpeechCheckbox.CheckedChanged += new System.EventHandler(this.EnableSpeechCheckbox_CheckedChanged);
            // 
            // TaskChooser
            // 
            this.TaskChooser.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.TaskChooser.FormattingEnabled = true;
            this.TaskChooser.Location = new System.Drawing.Point(15, 104);
            this.TaskChooser.Name = "TaskChooser";
            this.TaskChooser.Size = new System.Drawing.Size(106, 21);
            this.TaskChooser.TabIndex = 29;
            this.TaskChooser.SelectedIndexChanged += new System.EventHandler(this.TaskChooser_SelectedIndexChanged);
            // 
            // TaskLabel
            // 
            this.TaskLabel.AutoSize = true;
            this.TaskLabel.Location = new System.Drawing.Point(12, 88);
            this.TaskLabel.Name = "TaskLabel";
            this.TaskLabel.Size = new System.Drawing.Size(66, 13);
            this.TaskLabel.TabIndex = 30;
            this.TaskLabel.Text = "Task: (1/10)";
            // 
            // ResetMenuLocationButton
            // 
            this.ResetMenuLocationButton.Location = new System.Drawing.Point(213, 10);
            this.ResetMenuLocationButton.Name = "ResetMenuLocationButton";
            this.ResetMenuLocationButton.Size = new System.Drawing.Size(170, 23);
            this.ResetMenuLocationButton.TabIndex = 34;
            this.ResetMenuLocationButton.Text = "Reset Menu Location";
            this.ResetMenuLocationButton.UseVisualStyleBackColor = true;
            this.ResetMenuLocationButton.Click += new System.EventHandler(this.ResetMenuLocationButton_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 40);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(75, 13);
            this.label2.TabIndex = 36;
            this.label2.Text = "Testing Mode:";
            // 
            // ModeChooser
            // 
            this.ModeChooser.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ModeChooser.FormattingEnabled = true;
            this.ModeChooser.Items.AddRange(new object[] {
            "Training Locations",
            "Training Gestures"});
            this.ModeChooser.Location = new System.Drawing.Point(15, 56);
            this.ModeChooser.Name = "ModeChooser";
            this.ModeChooser.Size = new System.Drawing.Size(170, 21);
            this.ModeChooser.TabIndex = 35;
            this.ModeChooser.SelectedIndexChanged += new System.EventHandler(this.ModeChooser_SelectedIndexChanged);
            // 
            // StartStopLoggingButton
            // 
            this.StartStopLoggingButton.Location = new System.Drawing.Point(213, 188);
            this.StartStopLoggingButton.Name = "StartStopLoggingButton";
            this.StartStopLoggingButton.Size = new System.Drawing.Size(82, 34);
            this.StartStopLoggingButton.TabIndex = 37;
            this.StartStopLoggingButton.Text = "Start Logging";
            this.StartStopLoggingButton.UseVisualStyleBackColor = true;
            this.StartStopLoggingButton.Click += new System.EventHandler(this.StartStopLoggingButton_Click);
            // 
            // ResetLoggingButton
            // 
            this.ResetLoggingButton.Location = new System.Drawing.Point(301, 188);
            this.ResetLoggingButton.Name = "ResetLoggingButton";
            this.ResetLoggingButton.Size = new System.Drawing.Size(82, 34);
            this.ResetLoggingButton.TabIndex = 38;
            this.ResetLoggingButton.Text = "Reset Logging";
            this.ResetLoggingButton.UseVisualStyleBackColor = true;
            this.ResetLoggingButton.Click += new System.EventHandler(this.ResetLoggingButton_Click);
            // 
            // SetLoggingLocationButton
            // 
            this.SetLoggingLocationButton.Location = new System.Drawing.Point(213, 228);
            this.SetLoggingLocationButton.Name = "SetLoggingLocationButton";
            this.SetLoggingLocationButton.Size = new System.Drawing.Size(170, 23);
            this.SetLoggingLocationButton.TabIndex = 39;
            this.SetLoggingLocationButton.Text = "Set Logging Location";
            this.SetLoggingLocationButton.UseVisualStyleBackColor = true;
            this.SetLoggingLocationButton.Click += new System.EventHandler(this.SetLoggingLocationButton_Click);
            // 
            // ParticipantIDChooser
            // 
            this.ParticipantIDChooser.FormattingEnabled = true;
            this.ParticipantIDChooser.Items.AddRange(new object[] {
            "pilot1",
            "pilot2",
            "pilot3",
            "pilot4",
            "pilot5",
            "pilot6",
            "p01",
            "p02",
            "p03",
            "p04",
            "p05",
            "p06",
            "p07",
            "p08",
            "p09",
            "p10",
            "p11",
            "p12"});
            this.ParticipantIDChooser.Location = new System.Drawing.Point(15, 12);
            this.ParticipantIDChooser.Name = "ParticipantIDChooser";
            this.ParticipantIDChooser.Size = new System.Drawing.Size(106, 21);
            this.ParticipantIDChooser.TabIndex = 40;
            this.ParticipantIDChooser.SelectedIndexChanged += new System.EventHandler(this.ParticipantIDChooser_SelectedIndexChanged);
            this.ParticipantIDChooser.TextUpdate += new System.EventHandler(this.ParticipantIDChooser_TextUpdate);
            // 
            // RandomizeSubmenusButton
            // 
            this.RandomizeSubmenusButton.Location = new System.Drawing.Point(213, 39);
            this.RandomizeSubmenusButton.Name = "RandomizeSubmenusButton";
            this.RandomizeSubmenusButton.Size = new System.Drawing.Size(106, 23);
            this.RandomizeSubmenusButton.TabIndex = 41;
            this.RandomizeSubmenusButton.Text = "Randomize Menus";
            this.RandomizeSubmenusButton.UseVisualStyleBackColor = true;
            this.RandomizeSubmenusButton.Click += new System.EventHandler(this.RandomizeSubmenusButton_Click);
            // 
            // ResetMenusButton
            // 
            this.ResetMenusButton.Location = new System.Drawing.Point(325, 39);
            this.ResetMenusButton.Name = "ResetMenusButton";
            this.ResetMenusButton.Size = new System.Drawing.Size(58, 23);
            this.ResetMenusButton.TabIndex = 42;
            this.ResetMenusButton.Text = "Reset";
            this.ResetMenusButton.UseVisualStyleBackColor = true;
            this.ResetMenusButton.Click += new System.EventHandler(this.ResetMenusButton_Click);
            // 
            // StartStopTaskButton
            // 
            this.StartStopTaskButton.Location = new System.Drawing.Point(127, 103);
            this.StartStopTaskButton.Name = "StartStopTaskButton";
            this.StartStopTaskButton.Size = new System.Drawing.Size(58, 23);
            this.StartStopTaskButton.TabIndex = 43;
            this.StartStopTaskButton.Text = "Start";
            this.StartStopTaskButton.UseVisualStyleBackColor = true;
            this.StartStopTaskButton.Click += new System.EventHandler(this.StartStopTaskButton_Click);
            // 
            // RandomizeCheckbox
            // 
            this.RandomizeCheckbox.AutoSize = true;
            this.RandomizeCheckbox.Location = new System.Drawing.Point(213, 142);
            this.RandomizeCheckbox.Name = "RandomizeCheckbox";
            this.RandomizeCheckbox.Size = new System.Drawing.Size(113, 17);
            this.RandomizeCheckbox.TabIndex = 44;
            this.RandomizeCheckbox.Text = "Randomize Orders";
            this.RandomizeCheckbox.UseVisualStyleBackColor = true;
            this.RandomizeCheckbox.CheckedChanged += new System.EventHandler(this.RandomizeCheckbox_CheckedChanged);
            // 
            // PrevPhaseButton
            // 
            this.PrevPhaseButton.Enabled = false;
            this.PrevPhaseButton.Location = new System.Drawing.Point(15, 189);
            this.PrevPhaseButton.Name = "PrevPhaseButton";
            this.PrevPhaseButton.Size = new System.Drawing.Size(80, 45);
            this.PrevPhaseButton.TabIndex = 45;
            this.PrevPhaseButton.Text = "Prev";
            this.PrevPhaseButton.UseVisualStyleBackColor = true;
            this.PrevPhaseButton.Click += new System.EventHandler(this.PrevPhaseButton_Click);
            // 
            // NextPhaseButton
            // 
            this.NextPhaseButton.Location = new System.Drawing.Point(105, 189);
            this.NextPhaseButton.Name = "NextPhaseButton";
            this.NextPhaseButton.Size = new System.Drawing.Size(80, 45);
            this.NextPhaseButton.TabIndex = 46;
            this.NextPhaseButton.Text = "Next";
            this.NextPhaseButton.UseVisualStyleBackColor = true;
            this.NextPhaseButton.Click += new System.EventHandler(this.NextPhaseButton_Click);
            // 
            // PhaseLabel
            // 
            this.PhaseLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PhaseLabel.Location = new System.Drawing.Point(13, 137);
            this.PhaseLabel.Name = "PhaseLabel";
            this.PhaseLabel.Size = new System.Drawing.Size(172, 49);
            this.PhaseLabel.TabIndex = 47;
            this.PhaseLabel.Text = "Beginning";
            this.PhaseLabel.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // ConditionOrderTextbox
            // 
            this.ConditionOrderTextbox.Location = new System.Drawing.Point(128, 13);
            this.ConditionOrderTextbox.Name = "ConditionOrderTextbox";
            this.ConditionOrderTextbox.Size = new System.Drawing.Size(57, 20);
            this.ConditionOrderTextbox.TabIndex = 48;
            this.ConditionOrderTextbox.Text = "0, 1, 2";
            this.ConditionOrderTextbox.TextChanged += new System.EventHandler(this.ConditionOrderTextbox_TextChanged);
            // 
            // NextPhaseLabel
            // 
            this.NextPhaseLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NextPhaseLabel.Location = new System.Drawing.Point(13, 237);
            this.NextPhaseLabel.Name = "NextPhaseLabel";
            this.NextPhaseLabel.Size = new System.Drawing.Size(172, 16);
            this.NextPhaseLabel.TabIndex = 49;
            this.NextPhaseLabel.Text = "Next: Intro";
            this.NextPhaseLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // LockToCurrentTaskCheckbox
            // 
            this.LockToCurrentTaskCheckbox.AutoSize = true;
            this.LockToCurrentTaskCheckbox.Location = new System.Drawing.Point(213, 165);
            this.LockToCurrentTaskCheckbox.Name = "LockToCurrentTaskCheckbox";
            this.LockToCurrentTaskCheckbox.Size = new System.Drawing.Size(126, 17);
            this.LockToCurrentTaskCheckbox.TabIndex = 50;
            this.LockToCurrentTaskCheckbox.Text = "Lock to Current Task";
            this.LockToCurrentTaskCheckbox.UseVisualStyleBackColor = true;
            this.LockToCurrentTaskCheckbox.CheckedChanged += new System.EventHandler(this.LockToCurrentTaskButton_CheckedChanged);
            // 
            // TaskInstructionsLabel
            // 
            this.TaskInstructionsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TaskInstructionsLabel.Location = new System.Drawing.Point(16, 254);
            this.TaskInstructionsLabel.Name = "TaskInstructionsLabel";
            this.TaskInstructionsLabel.Size = new System.Drawing.Size(367, 54);
            this.TaskInstructionsLabel.TabIndex = 51;
            this.TaskInstructionsLabel.Text = "Task Instructions";
            this.TaskInstructionsLabel.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // TestingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(391, 317);
            this.Controls.Add(this.TaskInstructionsLabel);
            this.Controls.Add(this.LockToCurrentTaskCheckbox);
            this.Controls.Add(this.NextPhaseLabel);
            this.Controls.Add(this.ConditionOrderTextbox);
            this.Controls.Add(this.PhaseLabel);
            this.Controls.Add(this.NextPhaseButton);
            this.Controls.Add(this.PrevPhaseButton);
            this.Controls.Add(this.RandomizeCheckbox);
            this.Controls.Add(this.StartStopTaskButton);
            this.Controls.Add(this.ResetMenusButton);
            this.Controls.Add(this.RandomizeSubmenusButton);
            this.Controls.Add(this.ParticipantIDChooser);
            this.Controls.Add(this.SetLoggingLocationButton);
            this.Controls.Add(this.ResetLoggingButton);
            this.Controls.Add(this.StartStopLoggingButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.ModeChooser);
            this.Controls.Add(this.ResetMenuLocationButton);
            this.Controls.Add(this.TaskLabel);
            this.Controls.Add(this.TaskChooser);
            this.Controls.Add(this.EnableSpeechCheckbox);
            this.Controls.Add(this.FixedApplicationResonsesCheckbox);
            this.Controls.Add(this.EnableApplicationDemoCheckbox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "TestingForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Testing";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TestingForm_FormClosing);
            this.Move += new System.EventHandler(this.TestingForm_Move);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.CheckBox EnableApplicationDemoCheckbox;
        private System.Windows.Forms.CheckBox FixedApplicationResonsesCheckbox;
        private System.Windows.Forms.CheckBox EnableSpeechCheckbox;
        private System.Windows.Forms.ComboBox TaskChooser;
        private System.Windows.Forms.Label TaskLabel;
        private System.Windows.Forms.Button ResetMenuLocationButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox ModeChooser;
        private System.Windows.Forms.Button StartStopLoggingButton;
        private System.Windows.Forms.Button ResetLoggingButton;
        private System.Windows.Forms.Button SetLoggingLocationButton;
        private System.Windows.Forms.ComboBox ParticipantIDChooser;
        private System.Windows.Forms.Button RandomizeSubmenusButton;
        private System.Windows.Forms.Button ResetMenusButton;
        private System.Windows.Forms.Button StartStopTaskButton;
        private System.Windows.Forms.CheckBox RandomizeCheckbox;
        private System.Windows.Forms.Button PrevPhaseButton;
        private System.Windows.Forms.Button NextPhaseButton;
        private System.Windows.Forms.Label PhaseLabel;
        private System.Windows.Forms.TextBox ConditionOrderTextbox;
        private System.Windows.Forms.Label NextPhaseLabel;
        private System.Windows.Forms.CheckBox LockToCurrentTaskCheckbox;
        private System.Windows.Forms.Label TaskInstructionsLabel;
    }
}