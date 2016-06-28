namespace HandSightOnBodyInteractionRealTime
{
    partial class CameraViewer
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
            this.Display = new System.Windows.Forms.PictureBox();
            this.CalibrateButton = new System.Windows.Forms.Button();
            this.ToolPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Display)).BeginInit();
            this.SuspendLayout();
            // 
            // ToolPanel
            // 
            this.ToolPanel.Controls.Add(this.CalibrateButton);
            this.ToolPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.ToolPanel.Location = new System.Drawing.Point(0, 0);
            this.ToolPanel.Name = "ToolPanel";
            this.ToolPanel.Size = new System.Drawing.Size(528, 35);
            this.ToolPanel.TabIndex = 0;
            // 
            // Display
            // 
            this.Display.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Display.Location = new System.Drawing.Point(0, 35);
            this.Display.Name = "Display";
            this.Display.Size = new System.Drawing.Size(528, 464);
            this.Display.TabIndex = 1;
            this.Display.TabStop = false;
            // 
            // CalibrateButton
            // 
            this.CalibrateButton.Location = new System.Drawing.Point(4, 4);
            this.CalibrateButton.Name = "CalibrateButton";
            this.CalibrateButton.Size = new System.Drawing.Size(102, 23);
            this.CalibrateButton.TabIndex = 0;
            this.CalibrateButton.Text = "Start Calibration";
            this.CalibrateButton.UseVisualStyleBackColor = true;
            this.CalibrateButton.Click += new System.EventHandler(this.CalibrateButton_Click);
            // 
            // CameraViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(528, 499);
            this.Controls.Add(this.Display);
            this.Controls.Add(this.ToolPanel);
            this.Name = "CameraViewer";
            this.Text = "Camera Viewer";
            this.ToolPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.Display)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel ToolPanel;
        private System.Windows.Forms.PictureBox Display;
        private System.Windows.Forms.Button CalibrateButton;
    }
}

