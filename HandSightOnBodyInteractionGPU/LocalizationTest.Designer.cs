namespace HandSightOnBodyInteractionGPU
{
    partial class LocalizationTest
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
            this.ToolPanel = new System.Windows.Forms.Panel();
            this.CalibrationButton = new System.Windows.Forms.Button();
            this.PredictionLabel = new System.Windows.Forms.Label();
            this.TrainingExamplesLabel = new System.Windows.Forms.Label();
            this.ResetButton = new System.Windows.Forms.Button();
            this.RecordTrainingExampleButton = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.TimerChooser = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.LocationChooser = new System.Windows.Forms.ComboBox();
            this.Display = new System.Windows.Forms.PictureBox();
            this.CountdownLabel = new System.Windows.Forms.Label();
            this.SaveProfileButton = new System.Windows.Forms.Button();
            this.LoadProfileButton = new System.Windows.Forms.Button();
            this.ToolPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TimerChooser)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Display)).BeginInit();
            this.SuspendLayout();
            // 
            // ToolPanel
            // 
            this.ToolPanel.Controls.Add(this.LoadProfileButton);
            this.ToolPanel.Controls.Add(this.SaveProfileButton);
            this.ToolPanel.Controls.Add(this.CalibrationButton);
            this.ToolPanel.Controls.Add(this.PredictionLabel);
            this.ToolPanel.Controls.Add(this.TrainingExamplesLabel);
            this.ToolPanel.Controls.Add(this.ResetButton);
            this.ToolPanel.Controls.Add(this.RecordTrainingExampleButton);
            this.ToolPanel.Controls.Add(this.label3);
            this.ToolPanel.Controls.Add(this.TimerChooser);
            this.ToolPanel.Controls.Add(this.label2);
            this.ToolPanel.Controls.Add(this.label1);
            this.ToolPanel.Controls.Add(this.LocationChooser);
            this.ToolPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.ToolPanel.Location = new System.Drawing.Point(0, 0);
            this.ToolPanel.Name = "ToolPanel";
            this.ToolPanel.Size = new System.Drawing.Size(200, 563);
            this.ToolPanel.TabIndex = 0;
            // 
            // CalibrationButton
            // 
            this.CalibrationButton.Location = new System.Drawing.Point(12, 157);
            this.CalibrationButton.Name = "CalibrationButton";
            this.CalibrationButton.Size = new System.Drawing.Size(181, 23);
            this.CalibrationButton.TabIndex = 9;
            this.CalibrationButton.Text = "Start Calibration";
            this.CalibrationButton.UseVisualStyleBackColor = true;
            this.CalibrationButton.Click += new System.EventHandler(this.CalibrationButton_Click);
            // 
            // PredictionLabel
            // 
            this.PredictionLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PredictionLabel.Location = new System.Drawing.Point(12, 303);
            this.PredictionLabel.Name = "PredictionLabel";
            this.PredictionLabel.Size = new System.Drawing.Size(181, 36);
            this.PredictionLabel.TabIndex = 8;
            this.PredictionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // TrainingExamplesLabel
            // 
            this.TrainingExamplesLabel.Location = new System.Drawing.Point(13, 502);
            this.TrainingExamplesLabel.Name = "TrainingExamplesLabel";
            this.TrainingExamplesLabel.Size = new System.Drawing.Size(180, 23);
            this.TrainingExamplesLabel.TabIndex = 7;
            this.TrainingExamplesLabel.Text = "0 training examples (0 classes)";
            this.TrainingExamplesLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // ResetButton
            // 
            this.ResetButton.Location = new System.Drawing.Point(12, 528);
            this.ResetButton.Name = "ResetButton";
            this.ResetButton.Size = new System.Drawing.Size(181, 23);
            this.ResetButton.TabIndex = 6;
            this.ResetButton.Text = "Reset";
            this.ResetButton.UseVisualStyleBackColor = true;
            this.ResetButton.Click += new System.EventHandler(this.ResetButton_Click);
            // 
            // RecordTrainingExampleButton
            // 
            this.RecordTrainingExampleButton.Location = new System.Drawing.Point(12, 103);
            this.RecordTrainingExampleButton.Name = "RecordTrainingExampleButton";
            this.RecordTrainingExampleButton.Size = new System.Drawing.Size(181, 48);
            this.RecordTrainingExampleButton.TabIndex = 5;
            this.RecordTrainingExampleButton.Text = "Record Training Location";
            this.RecordTrainingExampleButton.UseVisualStyleBackColor = true;
            this.RecordTrainingExampleButton.Click += new System.EventHandler(this.RecordTrainingExampleButton_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(146, 68);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(47, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "seconds";
            // 
            // TimerChooser
            // 
            this.TimerChooser.Location = new System.Drawing.Point(57, 66);
            this.TimerChooser.Maximum = new decimal(new int[] {
            15,
            0,
            0,
            0});
            this.TimerChooser.Name = "TimerChooser";
            this.TimerChooser.Size = new System.Drawing.Size(83, 20);
            this.TimerChooser.TabIndex = 3;
            this.TimerChooser.ValueChanged += new System.EventHandler(this.TimerChooser_ValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 68);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(36, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Timer:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(92, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Training Location:";
            // 
            // LocationChooser
            // 
            this.LocationChooser.FormattingEnabled = true;
            this.LocationChooser.Items.AddRange(new object[] {
            "Nothing",
            "Finger",
            "Palm",
            "Wrist",
            "Ear",
            "Thigh",
            "Shoulder"});
            this.LocationChooser.Location = new System.Drawing.Point(12, 29);
            this.LocationChooser.Name = "LocationChooser";
            this.LocationChooser.Size = new System.Drawing.Size(181, 21);
            this.LocationChooser.TabIndex = 0;
            // 
            // Display
            // 
            this.Display.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Display.Location = new System.Drawing.Point(200, 0);
            this.Display.Name = "Display";
            this.Display.Size = new System.Drawing.Size(591, 563);
            this.Display.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.Display.TabIndex = 1;
            this.Display.TabStop = false;
            // 
            // CountdownLabel
            // 
            this.CountdownLabel.BackColor = System.Drawing.Color.Transparent;
            this.CountdownLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CountdownLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 100F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CountdownLabel.ForeColor = System.Drawing.Color.White;
            this.CountdownLabel.Location = new System.Drawing.Point(200, 0);
            this.CountdownLabel.Name = "CountdownLabel";
            this.CountdownLabel.Size = new System.Drawing.Size(591, 563);
            this.CountdownLabel.TabIndex = 2;
            this.CountdownLabel.Text = "10";
            this.CountdownLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CountdownLabel.Visible = false;
            // 
            // SaveProfileButton
            // 
            this.SaveProfileButton.Location = new System.Drawing.Point(12, 186);
            this.SaveProfileButton.Name = "SaveProfileButton";
            this.SaveProfileButton.Size = new System.Drawing.Size(86, 48);
            this.SaveProfileButton.TabIndex = 10;
            this.SaveProfileButton.Text = "Save Profile";
            this.SaveProfileButton.UseVisualStyleBackColor = true;
            this.SaveProfileButton.Click += new System.EventHandler(this.SaveProfileButton_Click);
            // 
            // LoadProfileButton
            // 
            this.LoadProfileButton.Location = new System.Drawing.Point(107, 186);
            this.LoadProfileButton.Name = "LoadProfileButton";
            this.LoadProfileButton.Size = new System.Drawing.Size(86, 48);
            this.LoadProfileButton.TabIndex = 11;
            this.LoadProfileButton.Text = "Load Profile";
            this.LoadProfileButton.UseVisualStyleBackColor = true;
            this.LoadProfileButton.Click += new System.EventHandler(this.LoadProfileButton_Click);
            // 
            // LocalizationTest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(791, 563);
            this.Controls.Add(this.CountdownLabel);
            this.Controls.Add(this.Display);
            this.Controls.Add(this.ToolPanel);
            this.Name = "LocalizationTest";
            this.Text = "Localization Test";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.LocalizationTest_FormClosing);
            this.ToolPanel.ResumeLayout(false);
            this.ToolPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TimerChooser)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Display)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel ToolPanel;
        private System.Windows.Forms.Button CalibrationButton;
        private System.Windows.Forms.Label PredictionLabel;
        private System.Windows.Forms.Label TrainingExamplesLabel;
        private System.Windows.Forms.Button ResetButton;
        private System.Windows.Forms.Button RecordTrainingExampleButton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown TimerChooser;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox LocationChooser;
        private System.Windows.Forms.PictureBox Display;
        private System.Windows.Forms.Label CountdownLabel;
        private System.Windows.Forms.Button LoadProfileButton;
        private System.Windows.Forms.Button SaveProfileButton;
    }
}