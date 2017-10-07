using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace TouchCamLibrary
{
    public class Utils
    {
        private static DateTime startTime = DateTime.Now;
        public static void Tic() { startTime = DateTime.Now; }
        public static void Toc(string description = null) { double elapsed = (DateTime.Now - startTime).TotalMilliseconds; Debug.WriteLine((description == null ? "Elapsed" : description) + ": " + elapsed + " ms"); startTime = DateTime.Now; }

        public static void ShowTwoImages(Bitmap img1, Bitmap img2, string title = "test", Form self = null)
        {
            Bitmap combined = new Bitmap(img1.Width + img2.Width, Math.Max(img1.Height, img2.Height));
            Graphics g = Graphics.FromImage(combined);
            g.DrawImage(img1, 0, 0, img1.Width, img1.Height);
            g.DrawImage(img2, img1.Width, (combined.Height - img2.Height) / 2, img2.Width, img2.Height);
            ShowImage(combined, title, self);
        }

        public static bool ShowImage(Image<Bgr, byte> image, string title, Form self = null) { return ShowImage(image.ToBitmap(), title, self); }
        public static bool ShowImage(Image<Gray, byte> image, string title, Form self = null) { return ShowImage(image.ToBitmap(), title, self); }
        public static bool ShowImage(Bitmap image, string title, Form self = null)
        {
            float maxHeight = Screen.PrimaryScreen.Bounds.Height - 100;
            float maxWidth = Screen.PrimaryScreen.Bounds.Width - 30;
            if (image.Height > maxHeight || image.Width > maxWidth)
            {
                float scale = Math.Min(maxHeight / image.Height, maxWidth / image.Width);
                Bitmap newImg = new Bitmap((int)(scale * image.Width), (int)(scale * image.Height));
                Graphics g = Graphics.FromImage(newImg);
                g.DrawImage(image, 0, 0, scale * image.Width, scale * image.Height);
                image = newImg;
            }

            Form form = new Form();
            form.Text = "Image: " + title;
            PictureBox box = new PictureBox();
            box.Image = image;
            box.Width = image.Width;
            box.Height = image.Height;
            form.KeyPreview = true;
            form.KeyDown += (object sender, KeyEventArgs e) =>
            {
                if (e.KeyCode == Keys.Escape)
                    form.DialogResult = System.Windows.Forms.DialogResult.Abort;
                else if (e.KeyCode == Keys.S)
                {
                    SaveFileDialog dialog = new SaveFileDialog();
                    dialog.DefaultExt = "png";
                    dialog.Filter = "PNG Images|*.png|All Files|*.*";
                    dialog.FilterIndex = 0;

                    if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        image.Save(dialog.FileName);
                    }
                }
                form.Close();
            };
            form.MouseWheel += (object sender, MouseEventArgs e) =>
            {
                // TODO: allow zooming
            };
            form.Shown += delegate { form.Activate(); };
            form.Controls.Add(box);
            form.AutoSize = true;
            form.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            form.StartPosition = FormStartPosition.CenterScreen;

            if (self != null) form.ShowDialog(self);
            else form.ShowDialog();

            return form.DialogResult != System.Windows.Forms.DialogResult.Abort;
        }

        // assumes that data lies between 0 and 1
        public static Image<Bgr, byte> GenerateHeatMap(Matrix<float> data)
        {
            int w = data.Cols;
            int h = data.Rows;
            Image<Bgr, byte> img = new Image<Bgr, byte>(w, h);

            for (int i = 0; i < h; i++)
                for (int j = 0; j < w; j++)
                {
                    float v = data[i, j];
                    byte r, g, b;
                    if (v < 0.5)
                    {
                        float v2 = v * 2;
                        r = (byte)(Color.Yellow.R * v2 + Color.Red.R * (1 - v2));
                        g = (byte)(Color.Yellow.G * v2 + Color.Red.G * (1 - v2));
                        b = (byte)(Color.Yellow.B * v2 + Color.Red.B * (1 - v2));
                    }
                    else
                    {
                        float v2 = (v - 0.5f) * 2;
                        r = (byte)(Color.Green.R * v2 + Color.Yellow.R * (1 - v2));
                        g = (byte)(Color.Green.G * v2 + Color.Yellow.G * (1 - v2));
                        b = (byte)(Color.Green.B * v2 + Color.Yellow.B * (1 - v2));
                    }
                    img.Data[i, j, 0] = b;
                    img.Data[i, j, 1] = g;
                    img.Data[i, j, 2] = r;
                }

            return img;
        }

        public static Bitmap VisualizeOrientations(Image<Gray, byte> img, Matrix<double> orientation) { return VisualizeOrientations(img.Convert<Bgr, byte>(), orientation, Color.Red); }
        public static Bitmap VisualizeOrientations(Image<Bgr, byte> img, Matrix<double> orientation) { return VisualizeOrientations(img, orientation, Color.Red); }
        public static Bitmap VisualizeOrientations(Image<Gray, byte> img, Matrix<double> orientation, Color orientationColor) { return VisualizeOrientations(img.Convert<Bgr, byte>(), orientation, orientationColor); }
        public static Bitmap VisualizeOrientations(Image<Bgr, byte> img, Matrix<double> orientation, Color orientationColor)
        {
            int w = img.Width;
            int h = img.Height;

            int halfWindow = 4;

            Bitmap visualization = new Bitmap(w, h);
            Graphics g = Graphics.FromImage(visualization);
            g.DrawImage(img.ToBitmap(), 0, 0, w, h);
            Pen pen = new Pen(Brushes.Red, 1);
            for (int i = halfWindow; i + halfWindow < h; i += 2 * halfWindow)
                for (int j = halfWindow; j + halfWindow < w; j += 2 * halfWindow)
                {
                    double angle = orientation[i, j];
                    g.DrawLine(pen, j + halfWindow * (float)Math.Cos(angle), i + halfWindow * (float)Math.Sin(angle), j + halfWindow * (float)Math.Cos(Math.PI + angle), i + halfWindow * (float)Math.Sin(Math.PI + angle));
                }

            return visualization;
        }

        public static Bitmap VisualizeFeatures(Image<Gray, byte> img, VectorOfKeyPoint features) { return VisualizeFeatures(img.Convert<Bgr, byte>(), features); }
        public static Bitmap VisualizeFeatures(Image<Bgr, byte> img, VectorOfKeyPoint features) { return VisualizeFeatures(img, features, Color.Red); }
        public static Bitmap VisualizeFeatures(Image<Bgr, byte> img, VectorOfKeyPoint features, Color color)
        {
            Bitmap bmp = new Bitmap(img.Width, img.Height);
            Graphics g = Graphics.FromImage(bmp);
            g.DrawImage(img.ToBitmap(), 0, 0, img.Width, img.Height);
            SolidBrush brush = new SolidBrush(color);
            foreach (MKeyPoint kp in features.ToArray())
                g.FillEllipse(brush, kp.Point.X - 5, kp.Point.Y - 5, 11, 11);

            return bmp;
        }

        public static Bitmap VisualizeFeatureMatches(Image<Bgr, byte> img1, Image<Bgr, byte> img2, VectorOfKeyPoint features1, VectorOfKeyPoint features2, Matrix<int> matches) { return VisualizeFeatureMatches(img1, img2, features1, features2, matches, Color.Red, Color.Blue); }
        public static Bitmap VisualizeFeatureMatches(Image<Bgr, byte> img1, Image<Bgr, byte> img2, VectorOfKeyPoint features1, VectorOfKeyPoint features2,  Matrix<int> matches, Color featureColor, Color matchColor)
        {
            Bitmap vis = new Bitmap(img1.Width + img2.Width, Math.Max(img1.Height, img2.Height));
            Graphics g = Graphics.FromImage(vis);
            g.DrawImage(img1.ToBitmap(), 0, 0, img1.Width, img2.Width);
            g.DrawImage(img2.ToBitmap(), img1.Width, 0, img2.Width, img2.Height);

            Brush brush = new SolidBrush(featureColor);
            Pen pen = new Pen(matchColor, 3);
            for (int i = 0; i < matches.Rows; i++)
            {
                if(matches[i,0] >= 0)
                {
                    PointF p0 = features1[i].Point;
                    PointF p1 = new PointF(features2[matches[i,0]].Point.X + img1.Width, features2[matches[i,0]].Point.Y);
                    g.FillEllipse(brush, p0.X - 5, p0.Y - 5, 11, 11);
                    g.FillEllipse(brush, p1.X - 5, p1.Y - 5, 11, 11);
                    g.DrawLine(pen, p0, p1);
                }
            }

            return vis;
        }

        public static T[] Flatten<T>(T[,] array)
        {
            int height = array.GetLength(0);
            int width = array.GetLength(1);
            T[] flat = new T[width * height];

            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    flat[i * width + j] = array[i, j];

            return flat;
        }

        public static List<T> Add<T>(List<T> a, List<T> b)
        {
            List<T> c = new List<T>(a);
            int n = Math.Min(a.Count, b.Count);
            for (int i = 0; i < n; i++)
            {
                // hack to allow generic operators, but potentially an issue since it won't generate a compile time error
                dynamic ci = c[i];
                ci += (dynamic)b[i];
                c[i] = ci;
            }
            return c;
        }

        public static float[] ToFloat(double[] array)
        {
            float[] newArray = new float[array.Length];
            for (int i = 0; i < array.Length; i++)
                newArray[i] = (float)array[i];
            return newArray;
        }

        public static double[] ToDouble(float[] array)
        {
            double[] newArray = new double[array.Length];
            for (int i = 0; i < array.Length; i++)
                newArray[i] = (double)array[i];
            return newArray;
        }

        public static Matrix<T> Rotate<T>(Matrix<T> mat, int dimension, int index) where T: struct
        {
            Matrix<T> newMat = new Matrix<T>(mat.Size);
            int width = mat.Width;
            int height = mat.Height;

            if (dimension == 0) // y
            {
                for (int i = 0; i < mat.Height; i++)
                    for (int j = 0; j < mat.Width; j++)
                        newMat[i, j] = mat[(i + index) % height, j];
            }
            else // x
            {
                for (int i = 0; i < mat.Height; i++)
                    for (int j = 0; j < mat.Width; j++)
                        newMat[i, j] = mat[i, (j + index) % width];
            }

            return newMat;
        }

        public static float DistL2Sqr(float[] a, float[] b)
        {
            if (a.Length != b.Length) return -1;

            float dist = 0;
            for (int i = 0; i < a.Length; i++)
                dist += (a[i] - b[i]) * (a[i] - b[i]);
            return dist;
        }
        public static float DistL2(float[] a, float[] b) { return (float)Math.Sqrt(DistL2Sqr(a, b)); }

        public static double DistL2Sqr(double[] a, double[] b)
        {
            if (a.Length != b.Length) return -1;

            double dist = 0;
            for (int i = 0; i < a.Length; i++)
                dist += (a[i] - b[i]) * (a[i] - b[i]);
            return dist;
        }
        public static double DistL2(double[] a, double[] b) { return Math.Sqrt(DistL2Sqr(a, b)); }

        public static int[] RandomPermutation(int n)
        {
            int[] array = new int[n];
            for (int i = 0; i < n; i++) array[i] = i;
            Shuffle(array);
            return array;
        }

        /// <summary>
        /// Knuth shuffle
        /// </summary>   
        public static void Shuffle(int[] array, int seed = -1)
        {
            Random random = seed < 0 ? new Random() : new Random(seed);
            int n = array.Count();
            while (n > 1)
            {
                n--;
                int i = random.Next(n + 1);
                int temp = array[i];
                array[i] = array[n];
                array[n] = temp;
            }
        }

        public static void ProcessConfusionMatrix(int[,] M, out double precision, out double recall, out double f1)
        {
            precision = 0; recall = 0; f1 = 0;
            for (int i = 0; i < M.GetLength(0); i++)
            {
                double sum_i = 0, n = 0;
                for (int j = 0; j < M.GetLength(1); j++) { sum_i += M[j, i]; n += M[i, j]; }
                if (sum_i > 0) precision += (double)M[i, i] / sum_i;
                recall += (double)M[i, i] / n;
            }
            precision /= M.GetLength(1);
            recall /= M.GetLength(0);
            f1 = 2 * precision * recall / (precision + recall);
        }

        public static double EstimateFocus(IInputArray img)
        {
            Mat edges = new Mat();
            CvInvoke.Canny(img, edges, 100, 50, 3);
            double sum = CvInvoke.Sum(edges).V0;
            return sum / 255.0;

            //UMat deriv = new UMat(), derivSqr = new UMat();
            //CvInvoke.Laplacian(img, deriv, DepthType.Cv64F);
            //CvInvoke.Multiply(deriv, deriv, derivSqr);
            //double mean = CvInvoke.Mean(derivSqr).V0;
            //deriv.Dispose(); derivSqr.Dispose();
            //return mean;

            //double min = 0, max = 0;
            //Point minLoc = new Point(), maxLoc = new Point();
            //CvInvoke.MinMaxLoc(deriv, ref min, ref max, ref minLoc, ref maxLoc);
            //return max;
            //CvInvoke.Multiply(deriv, deriv, derivSqr);
            //MCvScalar sum = CvInvoke.Sum(derivSqr);
            //int width = derivSqr.Cols, height = derivSqr.Rows;
            //deriv.Dispose(); derivSqr.Dispose();
            //return sum.V0 / (width * height);

            //UMat derivX = new UMat(), derivY = new UMat(), derivXSqr = new UMat(), derivYSqr = new UMat(), derivSqr = new UMat();
            //CvInvoke.Sobel(img, derivX, DepthType.Cv64F, 1, 0, 3);
            //CvInvoke.Sobel(img, derivY, DepthType.Cv64F, 0, 1, 3);
            //CvInvoke.Multiply(derivX, derivX, derivXSqr);
            //CvInvoke.Multiply(derivY, derivY, derivYSqr);
            //CvInvoke.Add(derivXSqr, derivYSqr, derivSqr);
            //double mean = CvInvoke.Mean(derivSqr).V0;
            //derivX.Dispose(); derivY.Dispose(); derivXSqr.Dispose(); derivYSqr.Dispose(); derivSqr.Dispose();
            //return mean;
        }

        public static string CSV(params object[] vals)
        {
            string csv = "";
            foreach (object v in vals) csv += v.ToString() + ",";
            csv = csv.TrimEnd(',');
            return csv;
        }

        public static double Max(double[] vals)
        {
            double max = double.MinValue;
            foreach (double val in vals) if (val > max) max = val;
            return max;
        }

        public static List<T> MergeLists<T>(params List<T>[] lists)
        {
            List<T> mergedList = new List<T>();
            foreach(List<T> list in lists)
                mergedList.AddRange(list);
            return mergedList;
        }

        public static string ToReadableTimespanString(TimeSpan span)
        {
            int years = span.Days;
            int weeks = (span.Days - years * 365) / 7;
            int days = span.Days - years * 365 - weeks * 7;
            int hours = span.Hours;
            int minutes = span.Minutes;
            int seconds = span.Seconds;

            // special cases, with some rounding for ease of comprehension
            if (years > 0) return years + " years";
            if (weeks > 0) return weeks + " weeks" + (days > 0 ? " and " + days + " days" : "");
            if (days > 1) return days + " days";
            if (days == 1) return (hours + 24) + " hours";

            string response = "";
            if (hours > 1) response += hours + " hours ";
            else if (hours == 1) response += "1 hour ";
            if (minutes > 1) response += minutes + " minutes ";
            else if (minutes == 1) response += "1 minute ";
            response += (response == "" ? "" : "and ") + (seconds == 1 ? "1 second" : seconds + " seconds");
            return response;
        }
    }
}
