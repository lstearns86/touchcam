using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TouchCam
{
    public partial class ExpandContractButton : UserControl
    {
        public enum DirectionType { Up, Down, Left, Right }
        private DirectionType direction = DirectionType.Up;
        private bool hovering = false;

        [
        Category("Appearance"),
        Description("Specifies the arrow direction.")
        ]
        public DirectionType Direction
        {
            get { return direction; }
            set { direction = value; Refresh(); }
        }

        public delegate void ButtonClickedDelegate();
        public event ButtonClickedDelegate ButtonClicked;
        private void OnClick() { if (ButtonClicked != null) ButtonClicked(); }

        public ExpandContractButton()
        {
            InitializeComponent();
        }

        private void ExpandContractButton_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(hovering? ForeColor : BackColor);
            Point center = new Point(Width / 2, Height / 2);
            switch (direction)
            {
                case DirectionType.Up:
                    e.Graphics.FillPolygon(new SolidBrush(hovering ? BackColor : ForeColor), new Point[] { new Point(center.X - 5, center.Y + 5), new Point(center.X, center.Y - 5), new Point(center.X + 5, center.Y + 5) });
                    break;
                case DirectionType.Down:
                    e.Graphics.FillPolygon(new SolidBrush(hovering ? BackColor : ForeColor), new Point[] { new Point(center.X - 5, center.Y - 5), new Point(center.X + 5, center.Y - 5), new Point(center.X, center.Y + 5) });
                    break;
                case DirectionType.Left:
                    e.Graphics.FillPolygon(new SolidBrush(hovering ? BackColor : ForeColor), new Point[] { new Point(center.X - 5, center.Y), new Point(center.X + 5, center.Y + 5), new Point(center.X + 5, center.Y - 5) });
                    break;
                case DirectionType.Right:
                    e.Graphics.FillPolygon(new SolidBrush(hovering ? BackColor : ForeColor), new Point[] { new Point(center.X - 5, center.Y - 5), new Point(center.X - 5, center.Y + 5), new Point(center.X + 5, center.Y) });
                    break;
            }
        }

        private void ExpandContractButton_MouseEnter(object sender, EventArgs e)
        {
            hovering = true;
            Refresh();
        }

        private void ExpandContractButton_MouseLeave(object sender, EventArgs e)
        {
            hovering = false;
            Refresh();
        }

        private void ExpandContractButton_Click(object sender, EventArgs e)
        {
            OnClick();
        }
    }
}
