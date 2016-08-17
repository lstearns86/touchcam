using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandSightLibrary
{
    public class Point3D
    {
        public Point3D(float x = 0, float y = 0, float z = 0) { this.X = x; this.Y = y; this.Z = z; }
        public float X;
        public float Y;
        public float Z;

        public static float Dot(Point3D a, Point3D b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }

        public static Point3D Cross(Point3D a, Point3D b)
        {
            float x = a.Y * b.Z - a.Z * b.Y;
            float y = a.Z * b.X - a.X * b.Z;
            float z = a.X * b.Y - a.Y * b.X;
            return new Point3D(x, y, z);
        }

        public static Point3D operator +(Point3D a, Point3D b)
        {
            return new Point3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static Point3D operator -(Point3D a, Point3D b)
        {
            return new Point3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static Point3D operator *(float s, Point3D a)
        {
            return new Point3D(s * a.X, s * a.Y, s * a.Z);
        }

        public static Point3D operator *(Point3D a, float s)
        {
            return new Point3D(s * a.X, s * a.Y, s * a.Z);
        }
    }
}
