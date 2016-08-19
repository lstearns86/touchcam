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
            ((System.ComponentModel.ISupportInitialize)(this.PredictionSmoothingChooser)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BrightnessChooser)).BeginInit();
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
            this.CalibrationButton.Location = new System.Drawing.Point(10, 86);
            this.CalibrationButton.Name = "CalibrationButton";
            this.CalibrationButton.Size = new System.Drawing.Size(175, 23);
            this.CalibrationButton.TabIndex = 23;
            this.CalibrationButton.Text = "Start Calibration";
            this.CalibrationButton.UseVisualStyleBackColor = true;
            this.CalibrationButton.Click += new System.EventHandler(this.CalibrationButton_Click);
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(194, 120);
            this.Controls.Add(this.CalibrationButton);
            this.Controls.Add(this.SingleIMUCheckbox);
            this.Controls.Add(this.CoarseOnlyCheckbox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.PredictionSmoothingChooser);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.BrightnessChooser);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SettingsForm";
            this.Text = "Settings";
            ((System.ComponentModel.ISupportInitialize)(this.PredictionSmoothingChooser)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BrightnessChooser)).EndInit();
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
    }
}