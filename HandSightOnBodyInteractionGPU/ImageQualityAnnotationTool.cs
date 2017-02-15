using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using HandSightLibrary.ImageProcessing;
using Emgu.CV;
using Emgu.CV.Structure;

namespace HandSightOnBodyInteractionGPU
{
    public partial class ImageQualityAnnotationTool : Form
    {
        private List<Bitmap> images = new List<Bitmap>();
        private int currImageIndex = 0;
        private List<bool[]> currAnnotations = new List<bool[]>();

        public ImageQualityAnnotationTool()
        {
            InitializeComponent();
        }

        private void openDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(!Directory.Exists(Properties.Settings.Default.OpenDirectory))
            {
                Properties.Settings.Default.OpenDirectory = Environment.CurrentDirectory;
                Properties.Settings.Default.Save();
            }

            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.SelectedPath = Properties.Settings.Default.OpenDirectory;
            dialog.ShowNewFolderButton = false;
            dialog.Description = "Select folder containing images you would like to annotate";
            if(dialog.ShowDialog() == DialogResult.OK)
            {
                Properties.Settings.Default.OpenDirectory = dialog.SelectedPath;
                Properties.Settings.Default.Save();

                images.Clear();
                currAnnotations.Clear();
                currImageIndex = 0;
                string[] files = Directory.GetFiles(dialog.SelectedPath, "*.png");
                foreach (string file in files)
                {
                    Bitmap img = (Bitmap)Bitmap.FromFile(file);
                    img.Tag = Path.GetFileNameWithoutExtension(file);
                    images.Add(img);
                    currAnnotations.Add(new bool[6]);
                }

                ShowCurrentImage();
            }
        }

        private void ShowCurrentImage()
        {
            if(currImageIndex >= 0 && currImageIndex < images.Count)
            {
                Bitmap bmp = images[currImageIndex];
                Image<Gray, byte> img = new Image<Gray, byte>(bmp);
                float focus = ImageProcessing.ImageFocus(img);

                Display.Image = bmp;

                InfoLabel.Text = (string)bmp.Tag + ": ";

                bool hasAnnotations = false;
                if (currAnnotations[currImageIndex][0]) { InfoLabel.Text += "out of focus, "; hasAnnotations = true; }
                if (currAnnotations[currImageIndex][1]) { InfoLabel.Text += "too dark, "; hasAnnotations = true; }
                if (currAnnotations[currImageIndex][2]) { InfoLabel.Text += "too light, "; hasAnnotations = true; }
                if (currAnnotations[currImageIndex][3]) { InfoLabel.Text += "poor contrast, "; hasAnnotations = true; }
                if (currAnnotations[currImageIndex][4]) { InfoLabel.Text += "off target, "; hasAnnotations = true; }
                if (currAnnotations[currImageIndex][5]) { InfoLabel.Text += "artifact/object in view, "; hasAnnotations = true; }
                InfoLabel.Text = InfoLabel.Text.TrimEnd(',', ' ');
                if (!hasAnnotations) InfoLabel.Text += " (no annotations)";

                InfoLabel.Text += " focus=" + focus.ToString("0.0");
            }
        }

        private void ImageQualityAnnotationTool_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Left)
            {
                if (currImageIndex > 0) currImageIndex--;
                ShowCurrentImage();
            }
            else if(e.KeyCode == Keys.Right)
            {
                if (currImageIndex + 1 < images.Count)
                {
                    currImageIndex++;
                    ShowCurrentImage();
                }
                else
                {
                    MessageBox.Show("Last image");
                }
            }
            else if(e.KeyCode == Keys.NumPad1 || e.KeyCode == Keys.D1)
            {
                currAnnotations[currImageIndex][0] = !currAnnotations[currImageIndex][0];
                ShowCurrentImage();
            }
            else if (e.KeyCode == Keys.NumPad2 || e.KeyCode == Keys.D2)
            {
                currAnnotations[currImageIndex][1] = !currAnnotations[currImageIndex][1];
                ShowCurrentImage();
            }
            else if (e.KeyCode == Keys.NumPad3 || e.KeyCode == Keys.D3)
            {
                currAnnotations[currImageIndex][2] = !currAnnotations[currImageIndex][2];
                ShowCurrentImage();
            }
            else if (e.KeyCode == Keys.NumPad4 || e.KeyCode == Keys.D4)
            {
                currAnnotations[currImageIndex][3] = !currAnnotations[currImageIndex][3];
                ShowCurrentImage();
            }
            else if (e.KeyCode == Keys.NumPad5 || e.KeyCode == Keys.D5)
            {
                currAnnotations[currImageIndex][4] = !currAnnotations[currImageIndex][4];
                ShowCurrentImage();
            }
            else if (e.KeyCode == Keys.NumPad6 || e.KeyCode == Keys.D6)
            {
                currAnnotations[currImageIndex][5] = !currAnnotations[currImageIndex][5];
                ShowCurrentImage();
            }
        }

        private void copyToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string text = "";
            for(int i = 0; i < images.Count; i++)
            {
                text += (string)images[i].Tag + "\t";
                text += (currAnnotations[i][0] ? "1" : "0") + "\t";
                text += (currAnnotations[i][1] ? "1" : "0") + "\t";
                text += (currAnnotations[i][2] ? "1" : "0") + "\t";
                text += (currAnnotations[i][3] ? "1" : "0") + "\t";
                text += (currAnnotations[i][4] ? "1" : "0") + "\t";
                text += (currAnnotations[i][5] ? "1" : "0") + "\t";
                text += Environment.NewLine;
            }

            Clipboard.SetText(text);
        }

        private void saveAnnotationsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(Properties.Settings.Default.SaveDirectory))
            {
                Properties.Settings.Default.SaveDirectory = Environment.CurrentDirectory;
                Properties.Settings.Default.Save();
            }

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.InitialDirectory = Properties.Settings.Default.SaveDirectory;
            dialog.OverwritePrompt = true;
            dialog.Filter = "CSV Files|*.csv";
            dialog.Title = "Select location to save annotation data";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Properties.Settings.Default.SaveDirectory = Path.GetDirectoryName(dialog.FileName);
                Properties.Settings.Default.Save();

                string text = "";
                for (int i = 0; i < images.Count; i++)
                {
                    text += (string)images[i].Tag + "\t";
                    text += (currAnnotations[i][0] ? "1" : "0") + ",";
                    text += (currAnnotations[i][1] ? "1" : "0") + ",";
                    text += (currAnnotations[i][2] ? "1" : "0") + ",";
                    text += (currAnnotations[i][3] ? "1" : "0") + ",";
                    text += (currAnnotations[i][4] ? "1" : "0") + ",";
                    text += (currAnnotations[i][5] ? "1" : "0") + ",";
                    text += Environment.NewLine;
                }

                File.WriteAllText(dialog.FileName, text);
            }
        }
    }
}
