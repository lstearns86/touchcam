using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Emgu.CV;
using Emgu.CV.Structure;
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

        public class EulerAngles
        {
            public float Yaw, Pitch, Roll;
            public EulerAngles() : this(0, 0, 0) { }
            public EulerAngles(float yaw, float pitch, float roll)
            {
                this.Yaw = yaw;
                this.Pitch = pitch;
                this.Roll = roll;
            }
        }

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

        Quaternion orientationEstimate = new Quaternion();
        float sampleFreq = 190;
        const float betaDef = 0.2f;
        float beta = betaDef;
        const float GRAVITY = 9.80665f;
        const float RADIANS_PER_DEGREE = (float)Math.PI / 180.0f;
        const float DEGREES_PER_RADIAN = 180.0f / (float)Math.PI;
        const int motionWindow = 190; // ~1 second

        public void Reset()
        {
            orientationEstimate = new Quaternion();
            sampleFreq = 190;
            beta = betaDef;
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
                float elapsedTime = (float)(info.Timestamp - lastReadingTimestamp);
                sampleFreq = 1000.0f / elapsedTime;
                lastReadingTimestamp = info.Timestamp;
                float ax, ay, az, gx, gy, gz, mx, my, mz;
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

                MadgwickAHRSupdate(-gx * RADIANS_PER_DEGREE,
                                   -gy * RADIANS_PER_DEGREE,
                                   gz * RADIANS_PER_DEGREE,
                                   -ax,
                                   -ay,
                                   az,
                                   -mx,
                                   -my,
                                   mz);

                Monitor.Exit(stopwatch);
            }
        }

        public Quaternion EstimateOrientation()
        {
            return orientationEstimate.Clone();
        }

        // AHRS algorithm update
        // Code from http://www.x-io.co.uk/open-source-imu-and-ahrs-algorithms/
        // Paper by Sebastian O.H. Madgwick et al., "Estimation of IMU and MARG orientation using a gradient descent algorithm", 2011
        // Gyroscope readings should be in radians per second, accelerometer and magnetometer units are irrelevant because the vectors are normalized.
        void MadgwickAHRSupdate(float gx, float gy, float gz, float ax, float ay, float az, float mx, float my, float mz)
        {
            float recipNorm;
            float s0, s1, s2, s3;
            float qDot1, qDot2, qDot3, qDot4;
            float hx, hy;
            float _2q0mx, _2q0my, _2q0mz, _2q1mx, _2bx, _2bz, _4bx, _4bz, _2q0, _2q1, _2q2, _2q3, _2q0q2, _2q2q3, q0q0, q0q1, q0q2, q0q3, q1q1, q1q2, q1q3, q2q2, q2q3, q3q3;
            float q0 = orientationEstimate.W, q1 = orientationEstimate.X, q2 = orientationEstimate.Y, q3 = orientationEstimate.Z;

            // Use IMU algorithm if magnetometer measurement invalid (avoids NaN in magnetometer normalisation)
            if (Math.Abs(mx) < 1e-10 && Math.Abs(my) < 1e-10 && Math.Abs(mz) < 1e-10)
            {
                MadgwickAHRSupdateIMU(gx, gy, gz, ax, ay, az);
                return;
            }

            // Rate of change of quaternion from gyroscope
            qDot1 = 0.5f * (-q1 * gx - q2 * gy - q3 * gz);
            qDot2 = 0.5f * (q0 * gx + q2 * gz - q3 * gy);
            qDot3 = 0.5f * (q0 * gy - q1 * gz + q3 * gx);
            qDot4 = 0.5f * (q0 * gz + q1 * gy - q2 * gx);

            // Compute feedback only if accelerometer measurement valid (avoids NaN in accelerometer normalisation)
            if (!(Math.Abs(ax) < 1e-10 && Math.Abs(ay) < 1e-10 && Math.Abs(az) < 1e-10))
            {

                // Normalise accelerometer measurement
                recipNorm = invSqrt(ax * ax + ay * ay + az * az);
                ax *= recipNorm;
                ay *= recipNorm;
                az *= recipNorm;

                // Normalise magnetometer measurement
                recipNorm = invSqrt(mx * mx + my * my + mz * mz);
                mx *= recipNorm;
                my *= recipNorm;
                mz *= recipNorm;

                if (mx == 0 && my == 0 && mz == 0)
                    Debug.WriteLine("Error: bad calculation");

                // Auxiliary variables to avoid repeated arithmetic
                _2q0mx = 2.0f * q0 * mx;
                _2q0my = 2.0f * q0 * my;
                _2q0mz = 2.0f * q0 * mz;
                _2q1mx = 2.0f * q1 * mx;
                _2q0 = 2.0f * q0;
                _2q1 = 2.0f * q1;
                _2q2 = 2.0f * q2;
                _2q3 = 2.0f * q3;
                _2q0q2 = 2.0f * q0 * q2;
                _2q2q3 = 2.0f * q2 * q3;
                q0q0 = q0 * q0;
                q0q1 = q0 * q1;
                q0q2 = q0 * q2;
                q0q3 = q0 * q3;
                q1q1 = q1 * q1;
                q1q2 = q1 * q2;
                q1q3 = q1 * q3;
                q2q2 = q2 * q2;
                q2q3 = q2 * q3;
                q3q3 = q3 * q3;

                // Reference direction of Earth's magnetic field
                hx = mx * q0q0 - _2q0my * q3 + _2q0mz * q2 + mx * q1q1 + _2q1 * my * q2 + _2q1 * mz * q3 - mx * q2q2 - mx * q3q3;
                hy = _2q0mx * q3 + my * q0q0 - _2q0mz * q1 + _2q1mx * q2 - my * q1q1 + my * q2q2 + _2q2 * mz * q3 - my * q3q3;
                _2bx = (float)Math.Sqrt(hx * hx + hy * hy);
                _2bz = -_2q0mx * q2 + _2q0my * q1 + mz * q0q0 + _2q1mx * q3 - mz * q1q1 + _2q2 * my * q3 - mz * q2q2 + mz * q3q3;
                _4bx = 2.0f * _2bx;
                _4bz = 2.0f * _2bz;

                // Gradient decent algorithm corrective step
                s0 = -_2q2 * (2.0f * q1q3 - _2q0q2 - ax) + _2q1 * (2.0f * q0q1 + _2q2q3 - ay) - _2bz * q2 * (_2bx * (0.5f - q2q2 - q3q3) + _2bz * (q1q3 - q0q2) - mx) + (-_2bx * q3 + _2bz * q1) * (_2bx * (q1q2 - q0q3) + _2bz * (q0q1 + q2q3) - my) + _2bx * q2 * (_2bx * (q0q2 + q1q3) + _2bz * (0.5f - q1q1 - q2q2) - mz);
                s1 = _2q3 * (2.0f * q1q3 - _2q0q2 - ax) + _2q0 * (2.0f * q0q1 + _2q2q3 - ay) - 4.0f * q1 * (1 - 2.0f * q1q1 - 2.0f * q2q2 - az) + _2bz * q3 * (_2bx * (0.5f - q2q2 - q3q3) + _2bz * (q1q3 - q0q2) - mx) + (_2bx * q2 + _2bz * q0) * (_2bx * (q1q2 - q0q3) + _2bz * (q0q1 + q2q3) - my) + (_2bx * q3 - _4bz * q1) * (_2bx * (q0q2 + q1q3) + _2bz * (0.5f - q1q1 - q2q2) - mz);
                s2 = -_2q0 * (2.0f * q1q3 - _2q0q2 - ax) + _2q3 * (2.0f * q0q1 + _2q2q3 - ay) - 4.0f * q2 * (1 - 2.0f * q1q1 - 2.0f * q2q2 - az) + (-_4bx * q2 - _2bz * q0) * (_2bx * (0.5f - q2q2 - q3q3) + _2bz * (q1q3 - q0q2) - mx) + (_2bx * q1 + _2bz * q3) * (_2bx * (q1q2 - q0q3) + _2bz * (q0q1 + q2q3) - my) + (_2bx * q0 - _4bz * q2) * (_2bx * (q0q2 + q1q3) + _2bz * (0.5f - q1q1 - q2q2) - mz);
                s3 = _2q1 * (2.0f * q1q3 - _2q0q2 - ax) + _2q2 * (2.0f * q0q1 + _2q2q3 - ay) + (-_4bx * q3 + _2bz * q1) * (_2bx * (0.5f - q2q2 - q3q3) + _2bz * (q1q3 - q0q2) - mx) + (-_2bx * q0 + _2bz * q2) * (_2bx * (q1q2 - q0q3) + _2bz * (q0q1 + q2q3) - my) + _2bx * q1 * (_2bx * (q0q2 + q1q3) + _2bz * (0.5f - q1q1 - q2q2) - mz);
                recipNorm = invSqrt(s0 * s0 + s1 * s1 + s2 * s2 + s3 * s3); // normalise step magnitude
                s0 *= recipNorm;
                s1 *= recipNorm;
                s2 *= recipNorm;
                s3 *= recipNorm;

                // Apply feedback step
                qDot1 -= beta * s0;
                qDot2 -= beta * s1;
                qDot3 -= beta * s2;
                qDot4 -= beta * s3;
            }

            // Integrate rate of change of quaternion to yield quaternion
            q0 += qDot1 * (1.0f / sampleFreq);
            q1 += qDot2 * (1.0f / sampleFreq);
            q2 += qDot3 * (1.0f / sampleFreq);
            q3 += qDot4 * (1.0f / sampleFreq);

            // Normalise quaternion
            recipNorm = invSqrt(q0 * q0 + q1 * q1 + q2 * q2 + q3 * q3);
            q0 *= recipNorm;
            q1 *= recipNorm;
            q2 *= recipNorm;
            q3 *= recipNorm;

            if (float.IsNaN(q0) || float.IsNaN(q1) || float.IsNaN(q2) || float.IsNaN(q3))
                Debug.WriteLine("error: NaN");

            orientationEstimate.W = q0;
            orientationEstimate.X = q1;
            orientationEstimate.Y = q2;
            orientationEstimate.Z = q3;
        }

        // IMU algorithm update
        void MadgwickAHRSupdateIMU(float gx, float gy, float gz, float ax, float ay, float az)
        {
            float recipNorm;
            float s0, s1, s2, s3;
            float qDot1, qDot2, qDot3, qDot4;
            float _2q0, _2q1, _2q2, _2q3, _4q0, _4q1, _4q2, _8q1, _8q2, q0q0, q1q1, q2q2, q3q3;
            float q0 = orientationEstimate.W, q1 = orientationEstimate.X, q2 = orientationEstimate.Y, q3 = orientationEstimate.Z;

            // Rate of change of quaternion from gyroscope
            qDot1 = 0.5f * (-q1 * gx - q2 * gy - q3 * gz);
            qDot2 = 0.5f * (q0 * gx + q2 * gz - q3 * gy);
            qDot3 = 0.5f * (q0 * gy - q1 * gz + q3 * gx);
            qDot4 = 0.5f * (q0 * gz + q1 * gy - q2 * gx);

            // Compute feedback only if accelerometer measurement valid (avoids NaN in accelerometer normalisation)
            if (!(Math.Abs(ax) < 1e-10 && Math.Abs(ay) < 1e-10 && Math.Abs(az) < 1e-10))
            {

                // Normalise accelerometer measurement
                recipNorm = invSqrt(ax * ax + ay * ay + az * az);
                ax *= recipNorm;
                ay *= recipNorm;
                az *= recipNorm;

                // Auxiliary variables to avoid repeated arithmetic
                _2q0 = 2.0f * q0;
                _2q1 = 2.0f * q1;
                _2q2 = 2.0f * q2;
                _2q3 = 2.0f * q3;
                _4q0 = 4.0f * q0;
                _4q1 = 4.0f * q1;
                _4q2 = 4.0f * q2;
                _8q1 = 8.0f * q1;
                _8q2 = 8.0f * q2;
                q0q0 = q0 * q0;
                q1q1 = q1 * q1;
                q2q2 = q2 * q2;
                q3q3 = q3 * q3;

                // Gradient decent algorithm corrective step
                s0 = _4q0 * q2q2 + _2q2 * ax + _4q0 * q1q1 - _2q1 * ay;
                s1 = _4q1 * q3q3 - _2q3 * ax + 4.0f * q0q0 * q1 - _2q0 * ay - _4q1 + _8q1 * q1q1 + _8q1 * q2q2 + _4q1 * az;
                s2 = 4.0f * q0q0 * q2 + _2q0 * ax + _4q2 * q3q3 - _2q3 * ay - _4q2 + _8q2 * q1q1 + _8q2 * q2q2 + _4q2 * az;
                s3 = 4.0f * q1q1 * q3 - _2q1 * ax + 4.0f * q2q2 * q3 - _2q2 * ay;
                recipNorm = invSqrt(s0 * s0 + s1 * s1 + s2 * s2 + s3 * s3); // normalise step magnitude
                s0 *= recipNorm;
                s1 *= recipNorm;
                s2 *= recipNorm;
                s3 *= recipNorm;

                // Apply feedback step
                qDot1 -= beta * s0;
                qDot2 -= beta * s1;
                qDot3 -= beta * s2;
                qDot4 -= beta * s3;
            }

            // Integrate rate of change of quaternion to yield quaternion
            q0 += qDot1 * (1.0f / sampleFreq);
            q1 += qDot2 * (1.0f / sampleFreq);
            q2 += qDot3 * (1.0f / sampleFreq);
            q3 += qDot4 * (1.0f / sampleFreq);

            // Normalise quaternion
            recipNorm = invSqrt(q0 * q0 + q1 * q1 + q2 * q2 + q3 * q3);
            q0 *= recipNorm;
            q1 *= recipNorm;
            q2 *= recipNorm;
            q3 *= recipNorm;

            if (float.IsNaN(q0) || float.IsNaN(q1) || float.IsNaN(q2) || float.IsNaN(q3))
                Debug.WriteLine("error: NaN");

            orientationEstimate.W = q0;
            orientationEstimate.X = q1;
            orientationEstimate.Y = q2;
            orientationEstimate.Z = q3;
        }

        float invSqrt(float x)
        {
            // Fast inverse square-root
            // See: http://en.wikipedia.org/wiki/Fast_inverse_square_root
            // Note: the original used this hack to speed up the calculation, but 
            // it's apparently unnecessary in C# (and doesn't seem to work as is)
            //float halfx = 0.5f * x;
            //float y = x;
            //long i = *(long*)&y;
            //i = 0x5f3759df - (i >> 1);
            //y = *(float*)&i;
            //y = y * (1.5f - (halfx * y * y));
            //return y;

            // calculate the slow but safe way
            return (float)(1.0 / Math.Sqrt(x));
        }
    }
}
