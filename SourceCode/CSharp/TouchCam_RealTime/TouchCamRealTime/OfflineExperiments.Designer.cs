namespace TouchCam
{
    partial class OfflineExperiments
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
            this.Progress = new System.Windows.Forms.ProgressBar();
            this.StatusLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // Progress
            // 
            this.Progress.Dock = System.Windows.Forms.DockStyle.Top;
            this.Progress.Location = new System.Drawing.Point(0, 0);
            this.Progress.Name = "Progress";
            this.Progress.Size = new System.Drawing.Size(284, 10);
            this.Progress.TabIndex = 0;
            // 
            // StatusLabel
            // 
            this.StatusLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.StatusLabel.Location = new System.Drawing.Point(0, 10);
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Padding = new System.Windows.Forms.Padding(3);
            this.StatusLabel.Size = new System.Drawing.Size(284, 72);
            this.StatusLabel.TabIndex = 1;
            this.StatusLabel.Text = "label1";
            // 
            // OfflineExperiments
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 82);
            this.Controls.Add(this.StatusLabel);
            this.Controls.Add(this.Progress);
            this.Name = "OfflineExperiments";
            this.Text = "Performing Offline Experiments";
            this.Load += new System.EventHandler(this.OfflineExperiments_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ProgressBar Progress;
        private System.Windows.Forms.Label StatusLabel;
    }
}