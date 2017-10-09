using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using TouchCamLibrary;
using TouchCamLibrary.ImageProcessing;
using System.Threading;

namespace TouchCam
{
    /// <summary>
    /// Utility to test that IMUs working properly and check orientation estimates
    /// </summary>
    public partial class IMUTest : Form
    {
        public IMUTest()
        {
            InitializeComponent();

            Task.Factory.StartNew(() =>
            {
                Sensors.Instance.ReadingAvailable += Sensors_ReadingAvailable;

                Sensors.Instance.Connect();

                Sensors.Instance.Brightness = 1;
                Sensors.Instance.NumSensors = 1;
            });
        }

        DateTime last = DateTime.Now;
        private void Sensors_ReadingAvailable(Sensors.Reading reading)
        {
            OrientationTracker.Primary.UpdateWithReading(reading);
            //OrientationTracker.Secondary.UpdateWithReading(reading, -1, true); // uncomment if using two IMUs
            if ((DateTime.Now - last).TotalMilliseconds > 30)
            {
                EulerAngles orientation = OrientationTracker.Primary.EstimateOrientation().GetEulerAngles();
                
                int resolution = 1;
                int yaw = resolution * (int)Math.Round(orientation.Yaw * 180 / Math.PI / resolution);
                int pitch = resolution * (int)Math.Round(orientation.Pitch * 180 / Math.PI / resolution);
                int roll = resolution * (int)Math.Round(orientation.Roll * 180 / Math.PI / resolution);
                
                if(RemoveGravityCheckbox.Checked)
                {
                    reading = OrientationTracker.SubtractGravity(reading);
                }

                Invoke(new MethodInvoker(delegate
                {
                    StatusBox.Text = "";
                    StatusBox.Text += "ax = " + reading.Accelerometer1.X + " m/s^2" + Environment.NewLine;
                    StatusBox.Text += "ay = " + reading.Accelerometer1.Y + " m/s^2" + Environment.NewLine;
                    StatusBox.Text += "az = " + reading.Accelerometer1.Z + " m/s^2" + Environment.NewLine;
                    StatusBox.Text += "gx = " + reading.Gyroscope1.X + " deg / s" + Environment.NewLine;
                    StatusBox.Text += "gy = " + reading.Gyroscope1.Y + " deg / s" + Environment.NewLine;
                    StatusBox.Text += "gz = " + reading.Gyroscope1.Z + " deg / s" + Environment.NewLine;
                    StatusBox.Text += "mx = " + reading.Magnetometer1.X + " gauss" + Environment.NewLine;
                    StatusBox.Text += "my = " + reading.Magnetometer1.Y + " gauss" + Environment.NewLine;
                    StatusBox.Text += "mz = " + reading.Magnetometer1.Z + " gauss" + Environment.NewLine;
                    StatusBox.Text += yaw + Environment.NewLine + pitch + Environment.NewLine + roll;
                }));
                last = DateTime.Now;
            }
        }

        bool closing = false;
        private void IMUTest_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!closing)
            {
                closing = true;
                e.Cancel = true;

                // TODO: cleanup any sensors and resources
                Sensors.Instance.Disconnect();

                Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(500);
                    Invoke(new MethodInvoker(delegate { Close(); }));
                });
            }
        }
    }
}
