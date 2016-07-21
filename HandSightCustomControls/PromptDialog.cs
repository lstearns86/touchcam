using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HandSightLibrary
{
    public partial class PromptDialog : Form
    {
        public string Value { get { return TextInput.Text; } }

        public PromptDialog(string title, string confirm)
        {
            InitializeComponent();

            this.Text = title;
            this.ConfirmButton.Text = confirm;
        }

        private void ConfirmButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
