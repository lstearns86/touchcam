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
            this.FineProbabilityLabel = new System.Windows.Forms.Label();
            this.CoarseProbabilityLabel = new System.Windows.Forms.Label();
            this.FinePredictionLabel = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.FineLocationChooser = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.TouchStatusLabel = new System.Windows.Forms.Label();
            this.LoadProfileButton = new System.Windows.Forms.Button();
            this.SaveProfileButton = new System.Windows.Forms.Button();
            this.CoarsePredictionLabel = new System.Windows.Forms.Label();
            this.TrainingExamplesLabel = new System.Windows.Forms.Label();
            this.ResetButton = new System.Windows.Forms.Button();
            this.RecordTrainingExampleButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.CoarseLocationChooser = new System.Windows.Forms.ComboBox();
            this.CalibrationButton = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.TimerChooser = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.Display = new System.Windows.Forms.PictureBox();
            this.CountdownLabel = new System.Windows.Forms.Label();
            this.SecondaryControlPanel = new System.Windows.Forms.Panel();
            this.SingleIMUCheckbox = new System.Windows.Forms.CheckBox();
            this.CoarseOnlyCheckbox = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.PredictionSmoothingChooser = new System.Windows.Forms.NumericUpDown();
            this.TrainingSampleList = new System.Windows.Forms.ListView();
            this.label4 = new System.Windows.Forms.Label();
            this.BrightnessChooser = new System.Windows.Forms.NumericUpDown();
            this.ExpandSecondarySettingsButton = new HandSightOnBodyInteraction.ExpandContractButton();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.InfoBox = new System.Windows.Forms.TextBox();
            this.MainControlPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TimerChooser)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Display)).BeginInit();
            this.SecondaryControlPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PredictionSmoothingChooser)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BrightnessChooser)).BeginInit();
            this.SuspendLayout();
            // 
            // MainControlPanel
            // 
            this.MainControlPanel.Controls.Add(this.FineProbabilityLabel);
            this.MainControlPanel.Controls.Add(this.CoarseProbabilityLabel);
            this.MainControlPanel.Controls.Add(this.FinePredictionLabel);
            this.MainControlPanel.Controls.Add(this.label7);
            this.MainControlPanel.Controls.Add(this.FineLocationChooser);
            this.MainControlPanel.Controls.Add(this.label6);
            this.MainControlPanel.Controls.Add(this.TouchStatusLabel);
            this.MainControlPanel.Controls.Add(this.LoadProfileButton);
            this.MainControlPanel.Controls.Add(this.SaveProfileButton);
            this.MainControlPanel.Controls.Add(this.CoarsePredictionLabel);
            this.MainControlPanel.Controls.Add(this.TrainingExamplesLabel);
            this.MainControlPanel.Controls.Add(this.ResetButton);
            this.MainControlPanel.Controls.Add(this.RecordTrainingExampleButton);
            this.MainControlPanel.Controls.Add(this.label1);
            this.MainControlPanel.Controls.Add(this.CoarseLocationChooser);
            this.MainControlPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.MainControlPanel.Location = new System.Drawing.Point(0, 0);
            this.MainControlPanel.Name = "MainControlPanel";
            this.MainControlPanel.Size = new System.Drawing.Size(200, 557);
            this.MainControlPanel.TabIndex = 0;
            // 
            // FineProbabilityLabel
            // 
            this.FineProbabilityLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FineProbabilityLabel.Location = new System.Drawing.Point(9, 341);
            this.FineProbabilityLabel.Name = "FineProbabilityLabel";
            this.FineProbabilityLabel.Size = new System.Drawing.Size(181, 22);
            this.FineProbabilityLabel.TabIndex = 18;
            this.FineProbabilityLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // CoarseProbabilityLabel
            // 
            this.CoarseProbabilityLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CoarseProbabilityLabel.Location = new System.Drawing.Point(9, 288);
            this.CoarseProbabilityLabel.Name = "CoarseProbabilityLabel";
            this.CoarseProbabilityLabel.Size = new System.Drawing.Size(181, 17);
            this.CoarseProbabilityLabel.TabIndex = 17;
            this.CoarseProbabilityLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // FinePredictionLabel
            // 
            this.FinePredictionLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FinePredictionLabel.Location = new System.Drawing.Point(9, 305);
            this.FinePredictionLabel.Name = "FinePredictionLabel";
            this.FinePredictionLabel.Size = new System.Drawing.Size(181, 36);
            this.FinePredictionLabel.TabIndex = 16;
            this.FinePredictionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 59);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(30, 13);
            this.label7.TabIndex = 15;
            this.label7.Text = "Fine:";
            // 
            // FineLocationChooser
            // 
            this.FineLocationChooser.FormattingEnabled = true;
            this.FineLocationChooser.Location = new System.Drawing.Point(64, 56);
            this.FineLocationChooser.Name = "FineLocationChooser";
            this.FineLocationChooser.Size = new System.Drawing.Size(129, 21);
            this.FineLocationChooser.TabIndex = 14;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 32);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(43, 13);
            this.label6.TabIndex = 13;
            this.label6.Text = "Coarse:";
            // 
            // TouchStatusLabel
            // 
            this.TouchStatusLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TouchStatusLabel.Location = new System.Drawing.Point(13, 134);
            this.TouchStatusLabel.Name = "TouchStatusLabel";
            this.TouchStatusLabel.Size = new System.Drawing.Size(181, 36);
            this.TouchStatusLabel.TabIndex = 12;
            this.TouchStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
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
            // CoarsePredictionLabel
            // 
            this.CoarsePredictionLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CoarsePredictionLabel.Location = new System.Drawing.Point(9, 252);
            this.CoarsePredictionLabel.Name = "CoarsePredictionLabel";
            this.CoarsePredictionLabel.Size = new System.Drawing.Size(181, 36);
            this.CoarsePredictionLabel.TabIndex = 8;
            this.CoarsePredictionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
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
            this.RecordTrainingExampleButton.Location = new System.Drawing.Point(13, 83);
            this.RecordTrainingExampleButton.Name = "RecordTrainingExampleButton";
            this.RecordTrainingExampleButton.Size = new System.Drawing.Size(181, 48);
            this.RecordTrainingExampleButton.TabIndex = 5;
            this.RecordTrainingExampleButton.Text = "Record Training Location";
            this.RecordTrainingExampleButton.UseVisualStyleBackColor = true;
            this.RecordTrainingExampleButton.Click += new System.EventHandler(this.RecordTrainingExampleButton_Click);
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
            // CoarseLocationChooser
            // 
            this.CoarseLocationChooser.FormattingEnabled = true;
            this.CoarseLocationChooser.Location = new System.Drawing.Point(64, 29);
            this.CoarseLocationChooser.Name = "CoarseLocationChooser";
            this.CoarseLocationChooser.Size = new System.Drawing.Size(129, 21);
            this.CoarseLocationChooser.TabIndex = 0;
            this.CoarseLocationChooser.SelectedIndexChanged += new System.EventHandler(this.LocationChooser_SelectedIndexChanged);
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
            // Display
            // 
            this.Display.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Display.Location = new System.Drawing.Point(200, 0);
            this.Display.Name = "Display";
            this.Display.Size = new System.Drawing.Size(554, 557);
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
            this.CountdownLabel.Size = new System.Drawing.Size(554, 557);
            this.CountdownLabel.TabIndex = 2;
            this.CountdownLabel.Text = "10";
            this.CountdownLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CountdownLabel.Visible = false;
            // 
            // SecondaryControlPanel
            // 
            this.SecondaryControlPanel.Controls.Add(this.SingleIMUCheckbox);
            this.SecondaryControlPanel.Controls.Add(this.CoarseOnlyCheckbox);
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
            this.SecondaryControlPanel.Size = new System.Drawing.Size(185, 557);
            this.SecondaryControlPanel.TabIndex = 3;
            this.SecondaryControlPanel.Visible = false;
            // 
            // SingleIMUCheckbox
            // 
            this.SingleIMUCheckbox.AutoSize = true;
            this.SingleIMUCheckbox.Location = new System.Drawing.Point(98, 93);
            this.SingleIMUCheckbox.Name = "SingleIMUCheckbox";
            this.SingleIMUCheckbox.Size = new System.Drawing.Size(78, 17);
            this.SingleIMUCheckbox.TabIndex = 16;
            this.SingleIMUCheckbox.Text = "Single IMU";
            this.SingleIMUCheckbox.UseVisualStyleBackColor = true;
            this.SingleIMUCheckbox.CheckedChanged += new System.EventHandler(this.SingleIMUCheckbox_CheckedChanged);
            // 
            // CoarseOnlyCheckbox
            // 
            this.CoarseOnlyCheckbox.AutoSize = true;
            this.CoarseOnlyCheckbox.Location = new System.Drawing.Point(6, 93);
            this.CoarseOnlyCheckbox.Name = "CoarseOnlyCheckbox";
            this.CoarseOnlyCheckbox.Size = new System.Drawing.Size(83, 17);
            this.CoarseOnlyCheckbox.TabIndex = 15;
            this.CoarseOnlyCheckbox.Text = "Coarse Only";
            this.CoarseOnlyCheckbox.UseVisualStyleBackColor = true;
            this.CoarseOnlyCheckbox.CheckedChanged += new System.EventHandler(this.CoarseOnlyCheckbox_CheckedChanged);
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
            // TrainingSampleList
            // 
            this.TrainingSampleList.Location = new System.Drawing.Point(0, 116);
            this.TrainingSampleList.MultiSelect = false;
            this.TrainingSampleList.Name = "TrainingSampleList";
            this.TrainingSampleList.Size = new System.Drawing.Size(185, 405);
            this.TrainingSampleList.TabIndex = 12;
            this.TrainingSampleList.UseCompatibleStateImageBehavior = false;
            this.TrainingSampleList.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TrainingSampleList_KeyDown);
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
            this.ExpandSecondarySettingsButton.Size = new System.Drawing.Size(13, 557);
            this.ExpandSecondarySettingsButton.TabIndex = 4;
            this.ExpandSecondarySettingsButton.ButtonClicked += new HandSightOnBodyInteraction.ExpandContractButton.ButtonClickedDelegate(this.ExpandSecondarySettingsButton_ButtonClicked);
            // 
            // imageList1
            // 
            this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // InfoBox
            // 
            this.InfoBox.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.InfoBox.Location = new System.Drawing.Point(0, 557);
            this.InfoBox.Multiline = true;
            this.InfoBox.Name = "InfoBox";
            this.InfoBox.ReadOnly = true;
            this.InfoBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.InfoBox.Size = new System.Drawing.Size(952, 75);
            this.InfoBox.TabIndex = 5;
            // 
            // LocalizationTest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(952, 632);
            this.Controls.Add(this.CountdownLabel);
            this.Controls.Add(this.Display);
            this.Controls.Add(this.ExpandSecondarySettingsButton);
            this.Controls.Add(this.SecondaryControlPanel);
            this.Controls.Add(this.MainControlPanel);
            this.Controls.Add(this.InfoBox);
            this.Name = "LocalizationTest";
            this.Text = "Localization Test";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.LocalizationTest_FormClosing);
            this.MainControlPanel.ResumeLayout(false);
            this.MainControlPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TimerChooser)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Display)).EndInit();
            this.SecondaryControlPanel.ResumeLayout(false);
            this.SecondaryControlPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PredictionSmoothingChooser)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BrightnessChooser)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel MainControlPanel;
        private System.Windows.Forms.Button CalibrationButton;
        private System.Windows.Forms.Label CoarsePredictionLabel;
        private System.Windows.Forms.Label TrainingExamplesLabel;
        private System.Windows.Forms.Button ResetButton;
        private System.Windows.Forms.Button RecordTrainingExampleButton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown TimerChooser;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox CoarseLocationChooser;
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
        private System.Windows.Forms.Label TouchStatusLabel;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox FineLocationChooser;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label FinePredictionLabel;
        private System.Windows.Forms.CheckBox CoarseOnlyCheckbox;
        private System.Windows.Forms.Label FineProbabilityLabel;
        private System.Windows.Forms.Label CoarseProbabilityLabel;
        private System.Windows.Forms.CheckBox SingleIMUCheckbox;
        private System.Windows.Forms.TextBox InfoBox;
    }
}