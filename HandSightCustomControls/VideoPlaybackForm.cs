using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HandSightCustomControls
{
    public partial class VideoPlaybackForm : Form
    {
        int numVideos = 1;
        List<PictureBox> videoBoxes = new List<PictureBox>();
        int videoWidth = 320, videoHeight = 320;

        public VideoPlaybackForm(int numVideos = 1, int videoHeight = 320, int videoWidth = 320)
        {
            this.numVideos = numVideos;
            this.videoHeight = videoHeight;
            this.videoWidth = videoWidth;
            InitializeComponent();
        }

        private void VideoPlaybackForm_Load(object sender, EventArgs e)
        {
            Height = videoHeight;
            Width = numVideos * videoWidth;
            for(int i = 0; i < numVideos; i++)
            {
                PictureBox box = new PictureBox();
                box.Height = videoHeight;
                box.Width = videoWidth;
                box.SizeMode = PictureBoxSizeMode.Zoom;
                box.Location = new Point(i * videoWidth, 0);
                box.MouseDown += VideoPlaybackForm_MouseDown;
                box.MouseMove += VideoPlaybackForm_MouseMove;
                Controls.Add(box);
                videoBoxes.Add(box);
            }
        }

        Point clickPoint = new Point(), startLocation = new Point();
        private void VideoPlaybackForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                clickPoint = Cursor.Position;
                startLocation = Location;
            }
        }

        private void VideoPlaybackForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) Close();
        }

        private void VideoPlaybackForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                Location = new Point(startLocation.X + (Cursor.Position.X - clickPoint.X), startLocation.Y + (Cursor.Position.Y - clickPoint.Y));
        }

        public void SetFrame(Bitmap frame, int index)
        {
            if(!IsDisposed && frame != null && index >= 0 && index < numVideos)
            {
                videoBoxes[index].Image = frame;
            }
        }
    }
}
