namespace HandSightOnBodyInteractionNoGPU
{
    partial class ProgramSelectionForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProgramSelectionForm));
            this.RunButton = new System.Windows.Forms.Button();
            this.ProgramList = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // RunButton
            // 
            this.RunButton.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.RunButton.Location = new System.Drawing.Point(0, 403);
            this.RunButton.Name = "RunButton";
            this.RunButton.Size = new System.Drawing.Size(684, 47);
            this.RunButton.TabIndex = 1;
            this.RunButton.Text = "Run";
            this.RunButton.UseVisualStyleBackColor = true;
            this.RunButton.Click += new System.EventHandler(this.RunButton_Click);
            // 
            // ProgramList
            // 
            this.ProgramList.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ProgramList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ProgramList.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ProgramList.FormattingEnabled = true;
            this.ProgramList.ItemHeight = 42;
            this.ProgramList.Location = new System.Drawing.Point(0, 0);
            this.ProgramList.Name = "ProgramList";
            this.ProgramList.Size = new System.Drawing.Size(684, 403);
            this.ProgramList.TabIndex = 2;
            // 
            // ProgramSelectionForm
            // 
            this.AcceptButton = this.RunButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(684, 450);
            this.Controls.Add(this.ProgramList);
            this.Controls.Add(this.RunButton);
            //this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ProgramSelectionForm";
            this.Text = "Select Program to Run";
            this.Load += new System.EventHandler(this.ProgramSelectionForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button RunButton;
        private System.Windows.Forms.ListBox ProgramList;
    }
}