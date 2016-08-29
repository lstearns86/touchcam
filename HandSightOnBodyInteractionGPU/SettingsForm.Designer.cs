namespace HandSightOnBodyInteractionGPU
{
    partial class SettingsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
            this.SingleIMUCheckbox = new System.Windows.Forms.CheckBox();
            this.CoarseOnlyCheckbox = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.PredictionSmoothingChooser = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.BrightnessChooser = new System.Windows.Forms.NumericUpDown();
            this.CalibrationButton = new System.Windows.Forms.Button();
            this.IMUCalibrationButton = new System.Windows.Forms.Button();
            this.EnableSoundEffectsCheckbox = new System.Windows.Forms.CheckBox();
            this.EnableApplicationDemoCheckbox = new System.Windows.Forms.CheckBox();
            this.FixedApplicationResonsesCheckbox = new System.Windows.Forms.CheckBox();
            this.EnableSpeechCheckbox = new System.Windows.Forms.CheckBox();
            this.GestureModeChooser = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.HoverTimeThresholdChooser = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.ResetMenuLocationButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.PredictionSmoothingChooser)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BrightnessChooser)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.HoverTimeThresholdChooser)).BeginInit();
            this.SuspendLayout();
            // 
            // SingleIMUCheckbox
            // 
            this.SingleIMUCheckbox.AutoSize = true;
            this.SingleIMUCheckbox.Location = new System.Drawing.Point(107, 63);
            this.SingleIMUCheckbox.Name = "SingleIMUCheckbox";
            this.SingleIMUCheckbox.Size = new System.Drawing.Size(78, 17);
            this.SingleIMUCheckbox.TabIndex = 22;
            this.SingleIMUCheckbox.Text = "Single IMU";
            this.SingleIMUCheckbox.UseVisualStyleBackColor = true;
            this.SingleIMUCheckbox.CheckedChanged += new System.EventHandler(this.SingleIMUCheckbox_CheckedChanged);
            // 
            // CoarseOnlyCheckbox
            // 
            this.CoarseOnlyCheckbox.AutoSize = true;
            this.CoarseOnlyCheckbox.Location = new System.Drawing.Point(15, 63);
            this.CoarseOnlyCheckbox.Name = "CoarseOnlyCheckbox";
            this.CoarseOnlyCheckbox.Size = new System.Drawing.Size(83, 17);
            this.CoarseOnlyCheckbox.TabIndex = 21;
            this.CoarseOnlyCheckbox.Text = "Coarse Only";
            this.CoarseOnlyCheckbox.UseVisualStyleBackColor = true;
            this.CoarseOnlyCheckbox.CheckedChanged += new System.EventHandler(this.CoarseOnlyCheckbox_CheckedChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 35);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(110, 13);
            this.label5.TabIndex = 19;
            this.label5.Text = "Prediction Smoothing:";
            // 
            // PredictionSmoothingChooser
            // 
            this.PredictionSmoothingChooser.Location = new System.Drawing.Point(128, 33);
            this.PredictionSmoothingChooser.Maximum = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.PredictionSmoothingChooser.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.PredictionSmoothingChooser.Name = "PredictionSmoothingChooser";
            this.PredictionSmoothingChooser.Size = new System.Drawing.Size(54, 20);
            this.PredictionSmoothingChooser.TabIndex = 20;
            this.PredictionSmoothingChooser.Value = new decimal(new int[] {
            15,
            0,
            0,
            0});
            this.PredictionSmoothingChooser.ValueChanged += new System.EventHandler(this.PredictionSmoothingChooser_ValueChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(59, 13);
            this.label4.TabIndex = 17;
            this.label4.Text = "Brightness:";
            // 
            // BrightnessChooser
            // 
            this.BrightnessChooser.Location = new System.Drawing.Point(77, 7);
            this.BrightnessChooser.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.BrightnessChooser.Name = "BrightnessChooser";
            this.BrightnessChooser.Size = new System.Drawing.Size(105, 20);
            this.BrightnessChooser.TabIndex = 18;
            this.BrightnessChooser.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.BrightnessChooser.ValueChanged += new System.EventHandler(this.BrightnessChooser_ValueChanged);
            // 
            // CalibrationButton
            // 
            this.CalibrationButton.Location = new System.Drawing.Point(12, 293);
            this.CalibrationButton.Name = "CalibrationButton";
            this.CalibrationButton.Size = new System.Drawing.Size(170, 23);
            this.CalibrationButton.TabIndex = 23;
            this.CalibrationButton.Text = "Start Camera Calibration";
            this.CalibrationButton.UseVisualStyleBackColor = true;
            this.CalibrationButton.Click += new System.EventHandler(this.CalibrationButton_Click);
            // 
            // IMUCalibrationButton
            // 
            this.IMUCalibrationButton.Location = new System.Drawing.Point(12, 322);
            this.IMUCalibrationButton.Name = "IMUCalibrationButton";
            this.IMUCalibrationButton.Size = new System.Drawing.Size(170, 23);
            this.IMUCalibrationButton.TabIndex = 24;
            this.IMUCalibrationButton.Text = "Reset IMU Calibration";
            this.IMUCalibrationButton.UseVisualStyleBackColor = true;
            this.IMUCalibrationButton.Click += new System.EventHandler(this.IMUCalibrationButton_Click);
            // 
            // EnableSoundEffectsCheckbox
            // 
            this.EnableSoundEffectsCheckbox.AutoSize = true;
            this.EnableSoundEffectsCheckbox.Location = new System.Drawing.Point(15, 86);
            this.EnableSoundEffectsCheckbox.Name = "EnableSoundEffectsCheckbox";
            this.EnableSoundEffectsCheckbox.Size = new System.Drawing.Size(129, 17);
            this.EnableSoundEffectsCheckbox.TabIndex = 25;
            this.EnableSoundEffectsCheckbox.Text = "Enable Sound Effects";
            this.EnableSoundEffectsCheckbox.UseVisualStyleBackColor = true;
            this.EnableSoundEffectsCheckbox.CheckedChanged += new System.EventHandler(this.EnableSoundEffectsCheckbox_CheckedChanged);
            // 
            // EnableApplicationDemoCheckbox
            // 
            this.EnableApplicationDemoCheckbox.AutoSize = true;
            this.EnableApplicationDemoCheckbox.Location = new System.Drawing.Point(15, 132);
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
            this.FixedApplicationResonsesCheckbox.Location = new System.Drawing.Point(15, 155);
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
            this.EnableSpeechCheckbox.Location = new System.Drawing.Point(15, 109);
            this.EnableSpeechCheckbox.Name = "EnableSpeechCheckbox";
            this.EnableSpeechCheckbox.Size = new System.Drawing.Size(134, 17);
            this.EnableSpeechCheckbox.TabIndex = 28;
            this.EnableSpeechCheckbox.Text = "Enable Speech Output";
            this.EnableSpeechCheckbox.UseVisualStyleBackColor = true;
            this.EnableSpeechCheckbox.CheckedChanged += new System.EventHandler(this.EnableSpeechCheckbox_CheckedChanged);
            // 
            // GestureModeChooser
            // 
            this.GestureModeChooser.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.GestureModeChooser.FormattingEnabled = true;
            this.GestureModeChooser.Location = new System.Drawing.Point(15, 198);
            this.GestureModeChooser.Name = "GestureModeChooser";
            this.GestureModeChooser.Size = new System.Drawing.Size(170, 21);
            this.GestureModeChooser.TabIndex = 29;
            this.GestureModeChooser.SelectedIndexChanged += new System.EventHandler(this.GestureModeChooser_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 182);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 13);
            this.label1.TabIndex = 30;
            this.label1.Text = "Gesture Mode:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 261);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 13);
            this.label2.TabIndex = 31;
            this.label2.Text = "Hover Threshold:";
            // 
            // HoverTimeThresholdChooser
            // 
            this.HoverTimeThresholdChooser.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.HoverTimeThresholdChooser.Location = new System.Drawing.Point(105, 259);
            this.HoverTimeThresholdChooser.Maximum = new decimal(new int[] {
            3000,
            0,
            0,
            0});
            this.HoverTimeThresholdChooser.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.HoverTimeThresholdChooser.Name = "HoverTimeThresholdChooser";
            this.HoverTimeThresholdChooser.Size = new System.Drawing.Size(54, 20);
            this.HoverTimeThresholdChooser.TabIndex = 32;
            this.HoverTimeThresholdChooser.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.HoverTimeThresholdChooser.ValueChanged += new System.EventHandler(this.HoverTimeThresholdChooser_ValueChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(164, 261);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(20, 13);
            this.label3.TabIndex = 33;
            this.label3.Text = "ms";
            // 
            // ResetMenuLocationButton
            // 
            this.ResetMenuLocationButton.Location = new System.Drawing.Point(15, 225);
            this.ResetMenuLocationButton.Name = "ResetMenuLocationButton";
            this.ResetMenuLocationButton.Size = new System.Drawing.Size(170, 23);
            this.ResetMenuLocationButton.TabIndex = 34;
            this.ResetMenuLocationButton.Text = "Reset Menu Location";
            this.ResetMenuLocationButton.UseVisualStyleBackColor = true;
            this.ResetMenuLocationButton.Click += new System.EventHandler(this.ResetMenuLocationButton_Click);
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(194, 357);
            this.Controls.Add(this.ResetMenuLocationButton);
            this.Controls.Add(this.HoverTimeThresholdChooser);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.GestureModeChooser);
            this.Controls.Add(this.EnableSpeechCheckbox);
            this.Controls.Add(this.FixedApplicationResonsesCheckbox);
            this.Controls.Add(this.EnableApplicationDemoCheckbox);
            this.Controls.Add(this.EnableSoundEffectsCheckbox);
            this.Controls.Add(this.IMUCalibrationButton);
            this.Controls.Add(this.CalibrationButton);
            this.Controls.Add(this.SingleIMUCheckbox);
            this.Controls.Add(this.CoarseOnlyCheckbox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.PredictionSmoothingChooser);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.BrightnessChooser);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Settings";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SettingsForm_FormClosing);
            this.Move += new System.EventHandler(this.SettingsForm_Move);
            ((System.ComponentModel.ISupportInitialize)(this.PredictionSmoothingChooser)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BrightnessChooser)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.HoverTimeThresholdChooser)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox SingleIMUCheckbox;
        private System.Windows.Forms.CheckBox CoarseOnlyCheckbox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown PredictionSmoothingChooser;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown BrightnessChooser;
        private System.Windows.Forms.Button CalibrationButton;
        private System.Windows.Forms.Button IMUCalibrationButton;
        private System.Windows.Forms.CheckBox EnableSoundEffectsCheckbox;
        private System.Windows.Forms.CheckBox EnableApplicationDemoCheckbox;
        private System.Windows.Forms.CheckBox FixedApplicationResonsesCheckbox;
        private System.Windows.Forms.CheckBox EnableSpeechCheckbox;
        private System.Windows.Forms.ComboBox GestureModeChooser;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown HoverTimeThresholdChooser;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button ResetMenuLocationButton;
    }
}