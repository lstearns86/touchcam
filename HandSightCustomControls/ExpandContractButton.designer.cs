namespace HandSightOnBodyInteraction
{
    partial class ExpandContractButton
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // ExpandContractButton
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.Name = "ExpandContractButton";
            this.Size = new System.Drawing.Size(10, 10);
            this.Click += new System.EventHandler(this.ExpandContractButton_Click);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.ExpandContractButton_Paint);
            this.MouseEnter += new System.EventHandler(this.ExpandContractButton_MouseEnter);
            this.MouseLeave += new System.EventHandler(this.ExpandContractButton_MouseLeave);
            this.ResumeLayout(false);

        }

        #endregion
    }
}
