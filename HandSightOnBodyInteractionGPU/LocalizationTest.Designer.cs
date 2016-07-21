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
            this.components = new System.ComponentModel.Container();
            this.MainControlPanel = new System.Windows.Forms.Panel();
            this.LoadProfileButton = new System.Windows.Forms.Button();
            this.SaveProfileButton = new System.Windows.Forms.Button();
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
            this.SecondaryControlPanel = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.BrightnessChooser = new System.Windows.Forms.NumericUpDown();
            this.ExpandSecondarySettingsButton = new HandSightOnBodyInteraction.ExpandContractButton();
            this.TrainingSampleList = new System.Windows.Forms.ListView();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.label5 = new System.Windows.Forms.Label();
            this.PredictionSmoothingChooser = new System.Windows.Forms.NumericUpDown();
            this.MainControlPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TimerChooser)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Display)).BeginInit();
            this.SecondaryControlPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BrightnessChooser)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PredictionSmoothingChooser)).BeginInit();
            this.SuspendLayout();
            // 
            // MainControlPanel
            // 
            this.MainControlPanel.Controls.Add(this.LoadProfileButton);
            this.MainControlPanel.Controls.Add(this.SaveProfileButton);
            this.MainControlPanel.Controls.Add(this.PredictionLabel);
            this.MainControlPanel.Controls.Add(this.TrainingExamplesLabel);
            this.MainControlPanel.Controls.Add(this.ResetButton);
            this.MainControlPanel.Controls.Add(this.RecordTrainingExampleButton);
            this.MainControlPanel.Controls.Add(this.label1);
            this.MainControlPanel.Controls.Add(this.LocationChooser);
            this.MainControlPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.MainControlPanel.Location = new System.Drawing.Point(0, 0);
            this.MainControlPanel.Name = "MainControlPanel";
            this.MainControlPanel.Size = new System.Drawing.Size(200, 563);
            this.MainControlPanel.TabIndex = 0;
            // 
            // LoadProfileButton
            // 
            this.LoadProfileButton.Location = new System.Drawing.Point(107, 473);
            this.LoadProfileButton.Name = "LoadProfileButton";
            this.LoadProfileButton.Size = new System.Drawing.Size(86, 48);
            this.LoadProfileButton.TabIndex = 11;
            this.LoadProfileButton.Text = "Load Profile";
            this.LoadProfileButton.UseVisualStyleBackColor = true;
            this.LoadProfileButton.Click += new System.EventHandler(this.LoadProfileButton_Click);
            // 
            // SaveProfileButton
            // 
            this.SaveProfileButton.Location = new System.Drawing.Point(12, 473);
            this.SaveProfileButton.Name = "SaveProfileButton";
            this.SaveProfileButton.Size = new System.Drawing.Size(86, 48);
            this.SaveProfileButton.TabIndex = 10;
            this.SaveProfileButton.Text = "Save Profile";
            this.SaveProfileButton.UseVisualStyleBackColor = true;
            this.SaveProfileButton.Click += new System.EventHandler(this.SaveProfileButton_Click);
            // 
            // CalibrationButton
            // 
            this.CalibrationButton.Location = new System.Drawing.Point(6, 528);
            this.CalibrationButton.Name = "CalibrationButton";
            this.CalibrationButton.Size = new System.Drawing.Size(175, 23);
            this.CalibrationButton.TabIndex = 9;
            this.CalibrationButton.Text = "Start Calibration";
            this.CalibrationButton.UseVisualStyleBackColor = true;
            this.CalibrationButton.Click += new System.EventHandler(this.CalibrationButton_Click);
            // 
            // PredictionLabel
            // 
            this.PredictionLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PredictionLabel.Location = new System.Drawing.Point(9, 261);
            this.PredictionLabel.Name = "PredictionLabel";
            this.PredictionLabel.Size = new System.Drawing.Size(181, 36);
            this.PredictionLabel.TabIndex = 8;
            this.PredictionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // TrainingExamplesLabel
            // 
            this.TrainingExamplesLabel.Location = new System.Drawing.Point(12, 447);
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
            this.RecordTrainingExampleButton.Location = new System.Drawing.Point(12, 56);
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
            this.label3.Location = new System.Drawing.Point(126, 13);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(47, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "seconds";
            // 
            // TimerChooser
            // 
            this.TimerChooser.Location = new System.Drawing.Point(45, 11);
            this.TimerChooser.Maximum = new decimal(new int[] {
            15,
            0,
            0,
            0});
            this.TimerChooser.Name = "TimerChooser";
            this.TimerChooser.Size = new System.Drawing.Size(75, 20);
            this.TimerChooser.TabIndex = 3;
            this.TimerChooser.ValueChanged += new System.EventHandler(this.TimerChooser_ValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 13);
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
            "Shoulder",
            "PalmUp",
            "PalmDown",
            "PalmLeft",
            "PalmRight",
            "PalmCenter"});
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
            this.Display.Size = new System.Drawing.Size(554, 563);
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
            this.CountdownLabel.Size = new System.Drawing.Size(554, 563);
            this.CountdownLabel.TabIndex = 2;
            this.CountdownLabel.Text = "10";
            this.CountdownLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CountdownLabel.Visible = false;
            // 
            // SecondaryControlPanel
            // 
            this.SecondaryControlPanel.Controls.Add(this.label5);
            this.SecondaryControlPanel.Controls.Add(this.PredictionSmoothingChooser);
            this.SecondaryControlPanel.Controls.Add(this.TrainingSampleList);
            this.SecondaryControlPanel.Controls.Add(this.label4);
            this.SecondaryControlPanel.Controls.Add(this.BrightnessChooser);
            this.SecondaryControlPanel.Controls.Add(this.label2);
            this.SecondaryControlPanel.Controls.Add(this.TimerChooser);
            this.SecondaryControlPanel.Controls.Add(this.CalibrationButton);
            this.SecondaryControlPanel.Controls.Add(this.label3);
            this.SecondaryControlPanel.Dock = System.Windows.Forms.DockStyle.Right;
            this.SecondaryControlPanel.Location = new System.Drawing.Point(767, 0);
            this.SecondaryControlPanel.Name = "SecondaryControlPanel";
            this.SecondaryControlPanel.Size = new System.Drawing.Size(185, 563);
            this.SecondaryControlPanel.TabIndex = 3;
            this.SecondaryControlPanel.Visible = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 39);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(59, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Brightness:";
            // 
            // BrightnessChooser
            // 
            this.BrightnessChooser.Location = new System.Drawing.Point(68, 37);
            this.BrightnessChooser.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.BrightnessChooser.Name = "BrightnessChooser";
            this.BrightnessChooser.Size = new System.Drawing.Size(105, 20);
            this.BrightnessChooser.TabIndex = 11;
            this.BrightnessChooser.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.BrightnessChooser.ValueChanged += new System.EventHandler(this.BrightnessChooser_ValueChanged);
            // 
            // ExpandSecondarySettingsButton
            // 
            this.ExpandSecondarySettingsButton.BackColor = System.Drawing.SystemColors.Control;
            this.ExpandSecondarySettingsButton.Direction = HandSightOnBodyInteraction.ExpandContractButton.DirectionType.Left;
            this.ExpandSecondarySettingsButton.Dock = System.Windows.Forms.DockStyle.Right;
            this.ExpandSecondarySettingsButton.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.ExpandSecondarySettingsButton.Location = new System.Drawing.Point(754, 0);
            this.ExpandSecondarySettingsButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.ExpandSecondarySettingsButton.Name = "ExpandSecondarySettingsButton";
            this.ExpandSecondarySettingsButton.Size = new System.Drawing.Size(13, 563);
            this.ExpandSecondarySettingsButton.TabIndex = 4;
            this.ExpandSecondarySettingsButton.ButtonClicked += new HandSightOnBodyInteraction.ExpandContractButton.ButtonClickedDelegate(this.ExpandSecondarySettingsButton_ButtonClicked);
            // 
            // TrainingSampleList
            // 
            this.TrainingSampleList.Location = new System.Drawing.Point(0, 99);
            this.TrainingSampleList.MultiSelect = false;
            this.TrainingSampleList.Name = "TrainingSampleList";
            this.TrainingSampleList.Size = new System.Drawing.Size(185, 422);
            this.TrainingSampleList.TabIndex = 12;
            this.TrainingSampleList.UseCompatibleStateImageBehavior = false;
            this.TrainingSampleList.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TrainingSampleList_KeyDown);
            // 
            // imageList1
            // 
            this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 65);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(110, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "Prediction Smoothing:";
            // 
            // PredictionSmoothingChooser
            // 
            this.PredictionSmoothingChooser.Location = new System.Drawing.Point(119, 63);
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
            this.PredictionSmoothingChooser.TabIndex = 14;
            this.PredictionSmoothingChooser.Value = new decimal(new int[] {
            15,
            0,
            0,
            0});
            this.PredictionSmoothingChooser.ValueChanged += new System.EventHandler(this.PredictionSmoothingChooser_ValueChanged);
            // 
            // LocalizationTest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(952, 563);
            this.Controls.Add(this.CountdownLabel);
            this.Controls.Add(this.Display);
            this.Controls.Add(this.ExpandSecondarySettingsButton);
            this.Controls.Add(this.SecondaryControlPanel);
            this.Controls.Add(this.MainControlPanel);
            this.Name = "LocalizationTest";
            this.Text = "Localization Test";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.LocalizationTest_FormClosing);
            this.MainControlPanel.ResumeLayout(false);
            this.MainControlPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TimerChooser)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Display)).EndInit();
            this.SecondaryControlPanel.ResumeLayout(false);
            this.SecondaryControlPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BrightnessChooser)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PredictionSmoothingChooser)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel MainControlPanel;
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
        private System.Windows.Forms.Panel SecondaryControlPanel;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown BrightnessChooser;
        private HandSightOnBodyInteraction.ExpandContractButton ExpandSecondarySettingsButton;
        private System.Windows.Forms.ListView TrainingSampleList;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown PredictionSmoothingChooser;
    }
}