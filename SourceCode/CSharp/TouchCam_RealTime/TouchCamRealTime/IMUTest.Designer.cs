namespace TouchCam
{
    partial class IMUTest
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
            this.StatusBox = new System.Windows.Forms.TextBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.DisplayPanel = new System.Windows.Forms.Panel();
            this.ControlPanel = new System.Windows.Forms.Panel();
            this.RemoveGravityCheckbox = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.ControlPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // StatusBox
            // 
            this.StatusBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.StatusBox.Location = new System.Drawing.Point(0, 0);
            this.StatusBox.Multiline = true;
            this.StatusBox.Name = "StatusBox";
            this.StatusBox.ReadOnly = true;
            this.StatusBox.Size = new System.Drawing.Size(340, 317);
            this.StatusBox.TabIndex = 0;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 32);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.StatusBox);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.DisplayPanel);
            this.splitContainer1.Size = new System.Drawing.Size(686, 317);
            this.splitContainer1.SplitterDistance = 340;
            this.splitContainer1.TabIndex = 1;
            // 
            // DisplayPanel
            // 
            this.DisplayPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DisplayPanel.Location = new System.Drawing.Point(0, 0);
            this.DisplayPanel.Name = "DisplayPanel";
            this.DisplayPanel.Size = new System.Drawing.Size(342, 317);
            this.DisplayPanel.TabIndex = 0;
            // 
            // ControlPanel
            // 
            this.ControlPanel.Controls.Add(this.RemoveGravityCheckbox);
            this.ControlPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.ControlPanel.Location = new System.Drawing.Point(0, 0);
            this.ControlPanel.Name = "ControlPanel";
            this.ControlPanel.Size = new System.Drawing.Size(686, 32);
            this.ControlPanel.TabIndex = 2;
            // 
            // RemoveGravityCheckbox
            // 
            this.RemoveGravityCheckbox.AutoSize = true;
            this.RemoveGravityCheckbox.Location = new System.Drawing.Point(13, 9);
            this.RemoveGravityCheckbox.Name = "RemoveGravityCheckbox";
            this.RemoveGravityCheckbox.Size = new System.Drawing.Size(102, 17);
            this.RemoveGravityCheckbox.TabIndex = 0;
            this.RemoveGravityCheckbox.Text = "Remove Gravity";
            this.RemoveGravityCheckbox.UseVisualStyleBackColor = true;
            // 
            // IMUTest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(686, 349);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.ControlPanel);
            this.Name = "IMUTest";
            this.Text = "IMUTest";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.IMUTest_FormClosing);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ControlPanel.ResumeLayout(false);
            this.ControlPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox StatusBox;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Panel DisplayPanel;
        private System.Windows.Forms.Panel ControlPanel;
        private System.Windows.Forms.CheckBox RemoveGravityCheckbox;
    }
}