namespace TouchCam
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
            this.label2 = new System.Windows.Forms.Label();
            this.HoverTimeThresholdChooser = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.ReconnectCameraButton = new System.Windows.Forms.Button();
            this.ReconnectIMUButton = new System.Windows.Forms.Button();
            this.FlushIMUButton = new System.Windows.Forms.Button();
            this.EnableSwipeDownCheckbox = new System.Windows.Forms.CheckBox();
            this.EnableSingleTapCheckbox = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.PortChooser = new System.Windows.Forms.ComboBox();
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
            this.CalibrationButton.Location = new System.Drawing.Point(12, 188);
            this.CalibrationButton.Name = "CalibrationButton";
            this.CalibrationButton.Size = new System.Drawing.Size(170, 23);
            this.CalibrationButton.TabIndex = 23;
            this.CalibrationButton.Text = "Start Camera Calibration";
            this.CalibrationButton.UseVisualStyleBackColor = true;
            this.CalibrationButton.Click += new System.EventHandler(this.CalibrationButton_Click);
            // 
            // IMUCalibrationButton
            // 
            this.IMUCalibrationButton.Location = new System.Drawing.Point(12, 258);
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
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 164);
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
            this.HoverTimeThresholdChooser.Location = new System.Drawing.Point(105, 162);
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
            this.label3.Location = new System.Drawing.Point(164, 164);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(20, 13);
            this.label3.TabIndex = 33;
            this.label3.Text = "ms";
            // 
            // ReconnectCameraButton
            // 
            this.ReconnectCameraButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ReconnectCameraButton.Location = new System.Drawing.Point(12, 287);
            this.ReconnectCameraButton.Name = "ReconnectCameraButton";
            this.ReconnectCameraButton.Size = new System.Drawing.Size(59, 35);
            this.ReconnectCameraButton.TabIndex = 34;
            this.ReconnectCameraButton.Text = "Reconnect Camera";
            this.ReconnectCameraButton.UseVisualStyleBackColor = true;
            this.ReconnectCameraButton.Click += new System.EventHandler(this.ReconnectCameraButton_Click);
            // 
            // ReconnectIMUButton
            // 
            this.ReconnectIMUButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ReconnectIMUButton.Location = new System.Drawing.Point(77, 287);
            this.ReconnectIMUButton.Name = "ReconnectIMUButton";
            this.ReconnectIMUButton.Size = new System.Drawing.Size(58, 35);
            this.ReconnectIMUButton.TabIndex = 35;
            this.ReconnectIMUButton.Text = "Reconnect IMU";
            this.ReconnectIMUButton.UseVisualStyleBackColor = true;
            this.ReconnectIMUButton.Click += new System.EventHandler(this.ReconnectIMUButton_Click);
            // 
            // FlushIMUButton
            // 
            this.FlushIMUButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FlushIMUButton.Location = new System.Drawing.Point(141, 287);
            this.FlushIMUButton.Name = "FlushIMUButton";
            this.FlushIMUButton.Size = new System.Drawing.Size(41, 35);
            this.FlushIMUButton.TabIndex = 36;
            this.FlushIMUButton.Text = "Flush IMU";
            this.FlushIMUButton.UseVisualStyleBackColor = true;
            this.FlushIMUButton.Click += new System.EventHandler(this.FlushIMUButton_Click);
            // 
            // EnableSwipeDownCheckbox
            // 
            this.EnableSwipeDownCheckbox.AutoSize = true;
            this.EnableSwipeDownCheckbox.Location = new System.Drawing.Point(15, 109);
            this.EnableSwipeDownCheckbox.Name = "EnableSwipeDownCheckbox";
            this.EnableSwipeDownCheckbox.Size = new System.Drawing.Size(122, 17);
            this.EnableSwipeDownCheckbox.TabIndex = 37;
            this.EnableSwipeDownCheckbox.Text = "Enable Swipe Down";
            this.EnableSwipeDownCheckbox.UseVisualStyleBackColor = true;
            this.EnableSwipeDownCheckbox.CheckedChanged += new System.EventHandler(this.EnableSwipeDownCheckbox_CheckedChanged);
            // 
            // EnableSingleTapCheckbox
            // 
            this.EnableSingleTapCheckbox.AutoSize = true;
            this.EnableSingleTapCheckbox.Location = new System.Drawing.Point(15, 132);
            this.EnableSingleTapCheckbox.Name = "EnableSingleTapCheckbox";
            this.EnableSingleTapCheckbox.Size = new System.Drawing.Size(113, 17);
            this.EnableSingleTapCheckbox.TabIndex = 38;
            this.EnableSingleTapCheckbox.Text = "Enable Single Tap";
            this.EnableSingleTapCheckbox.UseVisualStyleBackColor = true;
            this.EnableSingleTapCheckbox.CheckedChanged += new System.EventHandler(this.EnableSingleTapCheckbox_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 234);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 13);
            this.label1.TabIndex = 39;
            this.label1.Text = "COM Port:";
            // 
            // PortChooser
            // 
            this.PortChooser.FormattingEnabled = true;
            this.PortChooser.Location = new System.Drawing.Point(77, 231);
            this.PortChooser.Name = "PortChooser";
            this.PortChooser.Size = new System.Drawing.Size(105, 21);
            this.PortChooser.TabIndex = 40;
            this.PortChooser.SelectedIndexChanged += new System.EventHandler(this.PortChooser_SelectedIndexChanged);
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(194, 334);
            this.Controls.Add(this.PortChooser);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.EnableSingleTapCheckbox);
            this.Controls.Add(this.EnableSwipeDownCheckbox);
            this.Controls.Add(this.FlushIMUButton);
            this.Controls.Add(this.ReconnectIMUButton);
            this.Controls.Add(this.ReconnectCameraButton);
            this.Controls.Add(this.HoverTimeThresholdChooser);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
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
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown HoverTimeThresholdChooser;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button ReconnectCameraButton;
        private System.Windows.Forms.Button ReconnectIMUButton;
        private System.Windows.Forms.Button FlushIMUButton;
        private System.Windows.Forms.CheckBox EnableSwipeDownCheckbox;
        private System.Windows.Forms.CheckBox EnableSingleTapCheckbox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox PortChooser;
    }
}