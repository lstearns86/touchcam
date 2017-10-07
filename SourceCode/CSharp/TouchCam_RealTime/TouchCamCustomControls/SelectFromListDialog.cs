using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TouchCamLibrary
{
    public partial class SelectFromListDialog : Form
    {
        public string SelectedItem { get { return (string)ChoiceList.Items[ChoiceList.SelectedIndex]; } }

        public SelectFromListDialog(string title, string confirm, params string[] items)
            : this(title, confirm, new List<string>(items))
        { }
        public SelectFromListDialog(string title, string confirm, List<string> items)
        {
            InitializeComponent();

            this.Text = title;
            this.ConfirmButton.Text = confirm;
            this.ChoiceList.Items.AddRange(items.ToArray());
            this.ChoiceList.SelectedIndex = 0;
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
