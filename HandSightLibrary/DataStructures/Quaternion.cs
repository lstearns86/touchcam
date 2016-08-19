using System;

using Emgu.CV;

namespace HandSightLibrary
{
    public class Quaternion
    {
        public double W;
        public double X;
        public double Y;
        public double Z;

        public Quaternion() : this(1, 0, 0, 0) { }
        public Quaternion(double W, double X, double Y, double Z)
        {
            this.W = W;
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }

        public static Quaternion Identity { get { return new Quaternion(); } }

        public Matrix<double> GetRotationMatrix()
        {
            Matrix<double> R = new Matrix<double>(3, 3);
            double sqw = W * W;
            double sqx = X * X;
            double sqy = Y * Y;
            double sqz = Z * Z;

            // invs (inverse square length) is only required if quaternion is not already normalised
            double invs = 1 / (sqx + sqy + sqz + sqw);
            R[0, 0] = (sqx - sqy - sqz + sqw) * invs; // since sqw + sqx + sqy + sqz =1/invs*invs
            R[1, 1] = (-sqx + sqy - sqz + sqw) * invs;
            R[2, 2] = (-sqx - sqy + sqz + sqw) * invs;

            double tmp1 = X * Y;
            double tmp2 = Z * W;
            R[1, 0] = 2.0f * (tmp1 + tmp2) * invs;
            R[0, 1] = 2.0f * (tmp1 - tmp2) * invs;

            tmp1 = X * Z;
            tmp2 = Y * W;
            R[2, 0] = 2.0f * (tmp1 - tmp2) * invs;
            R[0, 2] = 2.0f * (tmp1 + tmp2) * invs;
            tmp1 = Y * Z;
            tmp2 = X * W;
            R[2, 1] = 2.0f * (tmp1 + tmp2) * invs;
            R[1, 2] = 2.0f * (tmp1 - tmp2) * invs;

            return R;
        }

        public EulerAngles GetEulerAngles()
        {
            double roll = Math.Atan2(2 * (W * X + Y * Z), 1 - 2 * (X * X + Y * Y));
            double pitch = Math.Asin(2 * (W * Y - Z * X));
            //roll = roll + Math.PI; if (roll > Math.PI) roll -= 2 * Math.PI;
            double yaw = Math.Atan2(2 * (W * Z + X * Y), 1 - 2 * (Y * Y + Z * Z));
            //yaw = -yaw;
            //double yaw = Math.Atan2(2.0f * (X * Y + W * Z), W * W + X * X - Y * Y - Z * Z);
            //double pitch = -Math.Asin(2.0f * (X * Z - W * Y));
            //double roll = Math.Atan2(2.0f * (W * X + Y * Z), W * W - X * X - Y * Y + Z * Z);
            return new EulerAngles(yaw, pitch, roll);
        }

        public Point3D RotateVector(Point3D vector)
        {
            float s = (float)W;
            Point3D u = new Point3D((float)X, (float)Y, (float)Z);
            Point3D v = vector;

            float dotUV = Point3D.Dot(u, v);
            float dotUU = Point3D.Dot(u, u);
            Point3D crossUV = Point3D.Cross(u, v);

            Point3D vprime = 2.0f * dotUV * u + (s * s - dotUU) * v + 2.0f * s * crossUV;
            return vprime;
        }

        public Point3D InverseRotateVector(Point3D vector)
        {
            float s = (float)W;
            Point3D u = new Point3D(-(float)X, -(float)Y, -(float)Z);
            Point3D v = vector;

            float dotUV = Point3D.Dot(u, v);
            float dotUU = Point3D.Dot(u, u);
            Point3D crossUV = Point3D.Cross(u, v);

            Point3D vprime = 2.0f * dotUV * u + (s * s - dotUU) * v + 2.0f * s * crossUV;
            return vprime;
        }

        public Quaternion Clone()
        {
            return new Quaternion(W, X, Y, Z);
        }
    }
}
