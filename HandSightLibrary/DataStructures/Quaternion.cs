using System;

using Emgu.CV;

namespace HandSightLibrary
{
    public class Quaternion
    {
        public float W;
        public float X;
        public float Y;
        public float Z;

        public Quaternion() : this(1, 0, 0, 0) { }
        public Quaternion(float W, float X, float Y, float Z)
        {
            this.W = W;
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }

        public static Quaternion Identity { get { return new Quaternion(); } }

        public Matrix<float> GetRotationMatrix()
        {
            Matrix<float> R = new Matrix<float>(3, 3);
            float sqw = W * W;
            float sqx = X * X;
            float sqy = Y * Y;
            float sqz = Z * Z;

            // invs (inverse square length) is only required if quaternion is not already normalised
            float invs = 1 / (sqx + sqy + sqz + sqw);
            R[0, 0] = (sqx - sqy - sqz + sqw) * invs; // since sqw + sqx + sqy + sqz =1/invs*invs
            R[1, 1] = (-sqx + sqy - sqz + sqw) * invs;
            R[2, 2] = (-sqx - sqy + sqz + sqw) * invs;

            float tmp1 = X * Y;
            float tmp2 = Z * W;
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
            double pitch = Math.Atan2(2 * (W * X + Y * Z), 1 - 2 * (X * X + Y * Y));
            double roll = Math.Asin(2 * (W * X - Z * X));
            double yaw = Math.Atan2(2 * (W * Z + X * Y), 1 - 2 * (Y * Y + Z * Z));
            return new EulerAngles((float)yaw, (float)pitch, (float)roll);
        }

        public Quaternion Clone()
        {
            return new Quaternion(W, X, Y, Z);
        }
    }
}
