using System;
using System.Diagnostics;

using Emgu.CV;
using System.Threading;

namespace HandSightLibrary
{
    public class OrientationTracker
    {
        internal class SensorInfo
        {
            public Sensors.Reading Reading;
            public double Timestamp;
        }

        static OrientationTracker primary = null, secondary = null;
        public static OrientationTracker Primary
        {
            get
            {
                if (primary == null) primary = new OrientationTracker();
                return primary;
            }
        }
        public static OrientationTracker Secondary
        {
            get
            {
                if (secondary == null) secondary = new OrientationTracker();
                return secondary;
            }
        }

        Quaternion orientationEstimate = new Quaternion();
        double sampleFreq = 360;
        const double betaDef = 0.2;
        double beta = betaDef;

        const double GRAVITY = 9.80665f;
        const double RADIANS_PER_DEGREE = (double)Math.PI / 180.0f;
        const double DEGREES_PER_RADIAN = 180.0f / (double)Math.PI;

        KalmanFilter filter = new KalmanFilter();

        public void Reset()
        {
            orientationEstimate = new Quaternion();
            sampleFreq = 360;
            beta = betaDef;
            filter = new KalmanFilter();
        }

        public static Sensors.Reading SubtractGravity(Sensors.Reading sensorReading, bool useSecondSensor = false)
        {
            Sensors.Reading newReading = sensorReading.Clone();

            //// based on a formula obtained from http://www.varesano.net/blog/fabio/simple-gravity-compensation-9-dom-imus
            //Quaternion q1 = sensorReading.Orientation1;
            //Point3D g1 = new Point3D(
            //        (float)(2 * (q1.X * q1.Z - q1.W * q1.Y)),
            //        (float)(2 * (q1.W * q1.X + q1.Y * q1.Z)),
            //        (float)(q1.W * q1.W - q1.X * q1.X - q1.Y * q1.Y - q1.Z * q1.Z)
            //    );

            //newReading.Accelerometer1 -= g1 * (float)GRAVITY;

            Quaternion orientation1 = sensorReading.Orientation1;
            Point3D rotatedAcceleration1 = orientation1.RotateVector(sensorReading.Accelerometer1);
            rotatedAcceleration1.Z -= (float)GRAVITY;
            newReading.Accelerometer1 = orientation1.InverseRotateVector(rotatedAcceleration1);

            if (useSecondSensor)
            {
                //    Quaternion q2 = sensorReading.Orientation2;
                //    Point3D g2 = new Point3D(
                //            (float)(2 * (q2.X * q2.Z - q2.W * q2.Y)),
                //            (float)(2 * (q2.W * q2.X + q2.Y * q2.Z)),
                //            (float)(q2.W * q2.W - q2.X * q2.X - q2.Y * q2.Y - q2.Z * q2.Z)
                //        );

                //    newReading.Accelerometer2 -= g2 * (float)GRAVITY;

                Quaternion orientation2 = sensorReading.Orientation2;
                Point3D rotatedAcceleration2 = orientation2.RotateVector(sensorReading.Accelerometer2);
                rotatedAcceleration2.Z -= (float)GRAVITY;
                newReading.Accelerometer2 = orientation2.InverseRotateVector(rotatedAcceleration2);
            }

            return newReading;
        }

        public static Point3D SubtractOrientation(Point3D vector, Quaternion orientation)
        {
            // based on a formula obtained from http://gamedev.stackexchange.com/questions/28395/rotating-vector3-by-a-quaternion
            float s = (float)orientation.W;
            Point3D u = new Point3D(-(float)orientation.X, -(float)orientation.Y, -(float)orientation.Z);
            Point3D v = vector;

            float dotUV = Point3D.Dot(u, v);
            float dotUU = Point3D.Dot(u, u);
            Point3D crossUV = Point3D.Cross(u, v);

            Point3D vprime = 2.0f * dotUV * u + (s * s - dotUU) *v + 2.0f * s * crossUV;

            return vprime;
        }

        SensorInfo latestReading = null;
        Stopwatch stopwatch = new Stopwatch();
        double lastReadingTimestamp = 0;
        public void UpdateWithReading(Sensors.Reading sensorReading, double timestamp = -1, bool secondary = false)
        {
            if (!stopwatch.IsRunning) stopwatch.Restart();
            if (timestamp < 0) timestamp = (double)stopwatch.ElapsedTicks / (double)Stopwatch.Frequency * 1000.0;
            SensorInfo info = new SensorInfo() { Reading = sensorReading, Timestamp = timestamp };
            latestReading = info;

            if (Monitor.TryEnter(stopwatch))
            {
                double elapsedTime = (double)(info.Timestamp - lastReadingTimestamp);
                sampleFreq = 1000.0f / elapsedTime;
                lastReadingTimestamp = info.Timestamp;
                double ax, ay, az, gx, gy, gz, mx, my, mz;
                if (secondary)
                {
                    ax = sensorReading.Accelerometer2.X;
                    ay = sensorReading.Accelerometer2.Y;
                    az = sensorReading.Accelerometer2.Z;
                    gx = sensorReading.Gyroscope2.X;
                    gy = sensorReading.Gyroscope2.Y;
                    gz = sensorReading.Gyroscope2.Z;
                    mx = sensorReading.Magnetometer2.X;
                    my = sensorReading.Magnetometer2.Y;
                    mz = sensorReading.Magnetometer2.Z;
                }
                else
                {
                    ax = sensorReading.Accelerometer1.X;
                    ay = sensorReading.Accelerometer1.Y;
                    az = sensorReading.Accelerometer1.Z;
                    gx = sensorReading.Gyroscope1.X;
                    gy = sensorReading.Gyroscope1.Y;
                    gz = sensorReading.Gyroscope1.Z;
                    mx = sensorReading.Magnetometer1.X;
                    my = sensorReading.Magnetometer1.Y;
                    mz = sensorReading.Magnetometer1.Z;
                }

                MadgwickAHRSupdate(gx * RADIANS_PER_DEGREE,
                                   gy * RADIANS_PER_DEGREE,
                                   gz * RADIANS_PER_DEGREE,
                                   ax,
                                   ay,
                                   az,
                                   mx,
                                   my,
                                   mz);

                if (!secondary)
                    sensorReading.Orientation1 = orientationEstimate;
                else
                    sensorReading.Orientation2 = orientationEstimate;

                Monitor.Exit(stopwatch);
            }
        }

        public Quaternion EstimateOrientation()
        {
            return orientationEstimate.Clone();
        }

        void MadgwickAHRSupdate(double gx, double gy, double gz, double ax, double ay, double az, double mx, double my, double mz)
        {
            double q1 = orientationEstimate.W, q2 = orientationEstimate.X, q3 = orientationEstimate.Y, q4 = orientationEstimate.Z;   // short name local variable for readability
            double norm;
            double hx, hy, _2bx, _2bz;
            double s1, s2, s3, s4;
            double qDot1, qDot2, qDot3, qDot4;

            // Auxiliary variables to avoid repeated arithmetic
            double _2q1mx;
            double _2q1my;
            double _2q1mz;
            double _2q2mx;
            double _4bx;
            double _4bz;
            double _2q1 = 2.0f * q1;
            double _2q2 = 2.0f * q2;
            double _2q3 = 2.0f * q3;
            double _2q4 = 2.0f * q4;
            double _2q1q3 = 2.0f * q1 * q3;
            double _2q3q4 = 2.0f * q3 * q4;
            double q1q1 = q1 * q1;
            double q1q2 = q1 * q2;
            double q1q3 = q1 * q3;
            double q1q4 = q1 * q4;
            double q2q2 = q2 * q2;
            double q2q3 = q2 * q3;
            double q2q4 = q2 * q4;
            double q3q3 = q3 * q3;
            double q3q4 = q3 * q4;
            double q4q4 = q4 * q4;

            // Normalise accelerometer measurement
            norm = Math.Sqrt(ax * ax + ay * ay + az * az);
            if (norm == 0.0f) return; // handle NaN
            norm = 1.0f / norm;
            ax *= norm;
            ay *= norm;
            az *= norm;

            // Normalise magnetometer measurement
            norm = Math.Sqrt(mx * mx + my * my + mz * mz);
            if (norm == 0.0f) return; // handle NaN
            norm = 1.0f / norm;
            mx *= norm;
            my *= norm;
            mz *= norm;

            // Reference direction of Earth's magnetic field
            _2q1mx = 2.0f * q1 * mx;
            _2q1my = 2.0f * q1 * my;
            _2q1mz = 2.0f * q1 * mz;
            _2q2mx = 2.0f * q2 * mx;
            hx = mx * q1q1 - _2q1my * q4 + _2q1mz * q3 + mx * q2q2 + _2q2 * my * q3 + _2q2 * mz * q4 - mx * q3q3 - mx * q4q4;
            hy = _2q1mx * q4 + my * q1q1 - _2q1mz * q2 + _2q2mx * q3 - my * q2q2 + my * q3q3 + _2q3 * mz * q4 - my * q4q4;
            _2bx = Math.Sqrt(hx * hx + hy * hy);
            _2bz = -_2q1mx * q3 + _2q1my * q2 + mz * q1q1 + _2q2mx * q4 - mz * q2q2 + _2q3 * my * q4 - mz * q3q3 + mz * q4q4;
            _4bx = 2.0f * _2bx;
            _4bz = 2.0f * _2bz;

            // Gradient decent algorithm corrective step
            s1 = -_2q3 * (2.0f * q2q4 - _2q1q3 - ax) + _2q2 * (2.0f * q1q2 + _2q3q4 - ay) - _2bz * q3 * (_2bx * (0.5f - q3q3 - q4q4) + _2bz * (q2q4 - q1q3) - mx) + (-_2bx * q4 + _2bz * q2) * (_2bx * (q2q3 - q1q4) + _2bz * (q1q2 + q3q4) - my) + _2bx * q3 * (_2bx * (q1q3 + q2q4) + _2bz * (0.5f - q2q2 - q3q3) - mz);
            s2 = _2q4 * (2.0f * q2q4 - _2q1q3 - ax) + _2q1 * (2.0f * q1q2 + _2q3q4 - ay) - 4.0f * q2 * (1.0f - 2.0f * q2q2 - 2.0f * q3q3 - az) + _2bz * q4 * (_2bx * (0.5f - q3q3 - q4q4) + _2bz * (q2q4 - q1q3) - mx) + (_2bx * q3 + _2bz * q1) * (_2bx * (q2q3 - q1q4) + _2bz * (q1q2 + q3q4) - my) + (_2bx * q4 - _4bz * q2) * (_2bx * (q1q3 + q2q4) + _2bz * (0.5f - q2q2 - q3q3) - mz);
            s3 = -_2q1 * (2.0f * q2q4 - _2q1q3 - ax) + _2q4 * (2.0f * q1q2 + _2q3q4 - ay) - 4.0f * q3 * (1.0f - 2.0f * q2q2 - 2.0f * q3q3 - az) + (-_4bx * q3 - _2bz * q1) * (_2bx * (0.5f - q3q3 - q4q4) + _2bz * (q2q4 - q1q3) - mx) + (_2bx * q2 + _2bz * q4) * (_2bx * (q2q3 - q1q4) + _2bz * (q1q2 + q3q4) - my) + (_2bx * q1 - _4bz * q3) * (_2bx * (q1q3 + q2q4) + _2bz * (0.5f - q2q2 - q3q3) - mz);
            s4 = _2q2 * (2.0f * q2q4 - _2q1q3 - ax) + _2q3 * (2.0f * q1q2 + _2q3q4 - ay) + (-_4bx * q4 + _2bz * q2) * (_2bx * (0.5f - q3q3 - q4q4) + _2bz * (q2q4 - q1q3) - mx) + (-_2bx * q1 + _2bz * q3) * (_2bx * (q2q3 - q1q4) + _2bz * (q1q2 + q3q4) - my) + _2bx * q2 * (_2bx * (q1q3 + q2q4) + _2bz * (0.5f - q2q2 - q3q3) - mz);
            norm = Math.Sqrt(s1 * s1 + s2 * s2 + s3 * s3 + s4 * s4);    // normalise step magnitude
            norm = 1.0f / norm;
            s1 *= norm;
            s2 *= norm;
            s3 *= norm;
            s4 *= norm;

            // Compute rate of change of quaternion
            qDot1 = 0.5f * (-q2 * gx - q3 * gy - q4 * gz) - beta * s1;
            qDot2 = 0.5f * (q1 * gx + q3 * gz - q4 * gy) - beta * s2;
            qDot3 = 0.5f * (q1 * gy - q2 * gz + q4 * gx) - beta * s3;
            qDot4 = 0.5f * (q1 * gz + q2 * gy - q3 * gx) - beta * s4;

            // Integrate to yield quaternion
            q1 += qDot1 * (1.0 / sampleFreq);
            q2 += qDot2 * (1.0 / sampleFreq);
            q3 += qDot3 * (1.0 / sampleFreq);
            q4 += qDot4 * (1.0 / sampleFreq);
            norm = Math.Sqrt(q1 * q1 + q2 * q2 + q3 * q3 + q4 * q4);    // normalise quaternion
            norm = 1.0f / norm;
            orientationEstimate.W = q1 * norm;
            orientationEstimate.X = q2 * norm;
            orientationEstimate.Y = q3 * norm;
            orientationEstimate.Z = q4 * norm;
        }
    }
}
