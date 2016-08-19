using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;

namespace HandSightLibrary
{
    public class Sensors
    {
        //float ACCEL_UNIT = 0.732f / 1000.0f * 9.80665F; // m/s^2
        //float MAG_UNIT = 0.48f / 1000.0f; // gauss
        //float GYRO_UNIT = 0.07000f; // deg / s
        //float IR_UNIT = 1.0f / 1024.0f; // % max intensity
        float ACCEL_UNIT = 0.244f / 1000.0f * 9.80665F; // m/s^2
        float MAG_UNIT = 0.48f / 1000.0f; // gauss
        float GYRO_UNIT = 0.01750f; // deg / s
        float IR_UNIT = 1.0f / 1024.0f; // % max intensity

        string PORT_ID = "COM3"; // TODO: provide an interface to select the port, and save to user preference
        int BAUD_RATE = 250000;

        Point3D minMag1 = new Point3D(), maxMag1 = new Point3D();
        Point3D minMag2 = new Point3D(), maxMag2 = new Point3D();

        static Stopwatch stopwatch = new Stopwatch();

        public class Reading
        {
            public float Timestamp;
            public Point3D Accelerometer1;
            public Point3D Magnetometer1;
            public Point3D Gyroscope1;
            public Point3D Accelerometer2;
            public Point3D Magnetometer2;
            public Point3D Gyroscope2;
            public float InfraredReflectance1;
            public float InfraredReflectance2;
            public Quaternion Orientation1;
            public Quaternion Orientation2;

            public Reading()
            {
                Timestamp = 1000.0f * (float)stopwatch.ElapsedTicks / (float)Stopwatch.Frequency;
            }

            public Reading Clone()
            {
                Reading reading = new Reading();
                reading.Timestamp = Timestamp;
                reading.Accelerometer1 = Accelerometer1 == null ? null : new Point3D(Accelerometer1.X, Accelerometer1.Y, Accelerometer1.Z);
                reading.Magnetometer1 = Magnetometer1 == null ? null : new Point3D(Magnetometer1.X, Magnetometer1.Y, Magnetometer1.Z);
                reading.Gyroscope1 = Gyroscope1 == null ? null : new Point3D(Gyroscope1.X, Gyroscope1.Y, Gyroscope1.Z);
                reading.Accelerometer2 = Accelerometer2 == null ? null : new Point3D(Accelerometer2.X, Accelerometer2.Y, Accelerometer2.Z);
                reading.Magnetometer2 = Magnetometer2 == null ? null : new Point3D(Magnetometer2.X, Magnetometer2.Y, Magnetometer2.Z);
                reading.Gyroscope2 = Gyroscope2 == null ? null : new Point3D(Gyroscope2.X, Gyroscope2.Y, Gyroscope2.Z);
                reading.InfraredReflectance1 = InfraredReflectance1;
                reading.InfraredReflectance2 = InfraredReflectance2;
                reading.Orientation1 = Orientation1 == null ? null : Orientation1.Clone();
                reading.Orientation2 = Orientation2 == null ? null : Orientation2.Clone();
                return reading;
            }
        }

        public delegate void ReadingAvailableDelegate(Reading reading);
        public event ReadingAvailableDelegate ReadingAvailable;
        private void OnReadingAvailable(Reading reading) { if (ReadingAvailable != null) ReadingAvailable(reading); }

        private SerialPort device;

        public bool IsConnected { get { return (device != null && device.IsOpen); } }

        private static Sensors instance;
        public static Sensors Instance
        {
            get
            {
                if (instance == null) instance = new Sensors();
                return instance;
            }
        }

        private Sensors()
        {
            Connect();
            stopwatch.Restart();
        }

        ~Sensors()
        {
            Disconnect();
        }

        float brightness;
        public float Brightness
        {
            get { return brightness; }
            set
            {
                float percent = value;
                percent = (int)Math.Round(percent);
                if (percent < 0) percent = 0;
                if (percent > 1) percent = 1;
                if (IsConnected && brightness != percent)
                {
                    brightness = percent;
                    WriteCommand();
                }
            }
        }

        int numSensors = 1;
        public int NumSensors
        {
            get { return numSensors; }
            set
            {
                if (!IsConnected || numSensors != value)
                {
                    numSensors = value;
                    if (numSensors < 1) numSensors = 1;
                    if (numSensors > 2) numSensors = 2;
                    WriteCommand();
                }
            }
        }

        private void WriteCommand()
        {
            if (IsConnected)
            {
                int bVal = (int)(brightness * 127);
                if (bVal < 0) bVal = 0;
                else if (bVal > 127) bVal = 127;
                byte command = (byte)(bVal + (numSensors == 2 ? 128 : 0));
                //byte command = (byte)((byte)brightness | (byte)(numSensors - 1) << 1);
                device.Write(new byte[] { command }, 0, 1);
            }
        }

        int skippedReadings = 0;
        private Point3D CorrectMagnetometer1Bias(float x, float y, float z)
        {
            if (skippedReadings < 10)
            {
                skippedReadings++;
                return new Point3D(x, y, z);
            }
            else
            {
                if (x < minMag1.X) minMag1.X = x;
                if (x > maxMag1.X) maxMag1.X = x;
                if (y < minMag1.Y) minMag1.Y = y;
                if (y > maxMag1.Y) maxMag1.Y = y;
                if (z < minMag1.Z) minMag1.Z = z;
                if (z > maxMag1.Z) maxMag1.Z = z;

                // Remove hard and soft iron errors by correcting the bias and scale so that the readings are roughly spherical centered at 0.
                // Based upon an explanation and example code from http://www.camelsoftware.com/2016/03/13/imu-maths-calculate-orientation-pt3/
                Point3D vmaxMag1 = new Point3D();
                vmaxMag1.X = maxMag1.X - ((minMag1.X + maxMag1.X) / 2.0f);
                vmaxMag1.Y = maxMag1.Y - ((minMag1.Y + maxMag1.Y) / 2.0f);
                vmaxMag1.Z = maxMag1.Z - ((minMag1.Z + maxMag1.Z) / 2.0f);

                Point3D vminMag1 = new Point3D();
                vminMag1.X = minMag1.X - ((minMag1.X + maxMag1.X) / 2.0f);
                vminMag1.Y = minMag1.Y - ((minMag1.Y + maxMag1.Y) / 2.0f);
                vminMag1.Z = minMag1.Z - ((minMag1.Z + maxMag1.Z) / 2.0f);

                Point3D avgs = new Point3D();
                avgs.X = (vmaxMag1.X + (vminMag1.X * -1)) / 2.0f;
                avgs.Y = (vmaxMag1.Y + (vminMag1.Y * -1)) / 2.0f;
                avgs.Z = (vmaxMag1.Z + (vminMag1.Z * -1)) / 2.0f;

                float avgRad = (avgs.X + avgs.Y + avgs.Z) / 3.0f;

                float xScale = avgRad / avgs.X;
                float yScale = avgRad / avgs.Y;
                float zScale = avgRad / avgs.Z;

                float x2 = x - (minMag1.X + maxMag1.X) / 2.0f;
                float y2 = y - (minMag1.Y + maxMag1.Y) / 2.0f;
                float z2 = z - (minMag1.Z + maxMag1.Z) / 2.0f;

                x2 *= xScale;
                y2 *= yScale;
                z2 *= zScale;

                return new Point3D(x2, y2, z2);
            }
        }

        private Point3D CorrectMagnetometer2Bias(float x, float y, float z)
        {
            if (skippedReadings < 10)
            {
                //skippedReadings++;
                return new Point3D(x, y, z);
            }
            else
            {
                if (x < minMag2.X) minMag2.X = x;
                if (x > maxMag2.X) maxMag2.X = x;
                if (y < minMag2.Y) minMag2.Y = y;
                if (y > maxMag2.Y) maxMag2.Y = y;
                if (z < minMag2.Z) minMag2.Z = z;
                if (z > maxMag2.Z) maxMag2.Z = z;

                // Remove hard and soft iron errors by correcting the bias and scale so that the readings are roughly spherical centered at 0.
                // Based upon an explanation and example code from http://www.camelsoftware.com/2016/03/13/imu-maths-calculate-orientation-pt3/
                Point3D vmaxMag2 = new Point3D();
                vmaxMag2.X = maxMag2.X - ((minMag2.X + maxMag2.X) / 2.0f);
                vmaxMag2.Y = maxMag2.Y - ((minMag2.Y + maxMag2.Y) / 2.0f);
                vmaxMag2.Z = maxMag2.Z - ((minMag2.Z + maxMag2.Z) / 2.0f);

                Point3D vminMag2 = new Point3D();
                vminMag2.X = minMag2.X - ((minMag2.X + maxMag2.X) / 2.0f);
                vminMag2.Y = minMag2.Y - ((minMag2.Y + maxMag2.Y) / 2.0f);
                vminMag2.Z = minMag2.Z - ((minMag2.Z + maxMag2.Z) / 2.0f);

                Point3D avgs = new Point3D();
                avgs.X = (vmaxMag2.X + (vminMag2.X * -1)) / 2.0f;
                avgs.Y = (vmaxMag2.Y + (vminMag2.Y * -1)) / 2.0f;
                avgs.Z = (vmaxMag2.Z + (vminMag2.Z * -1)) / 2.0f;

                float avgRad = (avgs.X + avgs.Y + avgs.Z) / 3.0f;

                float xScale = avgRad / avgs.X;
                float yScale = avgRad / avgs.Y;
                float zScale = avgRad / avgs.Z;

                float x2 = x - (minMag2.X + maxMag2.X) / 2.0f;
                float y2 = y - (minMag2.Y + maxMag2.Y) / 2.0f;
                float z2 = z - (minMag2.Z + maxMag2.Z) / 2.0f;

                x2 *= xScale;
                y2 *= yScale;
                z2 *= zScale;

                return new Point3D(x2, y2, z2);
            }
        }

        public bool Connect()
        {
            if (IsConnected) return true;

            try
            {
                string[] connectedPorts = SerialPort.GetPortNames();
                if (!connectedPorts.Contains<string>(PORT_ID))
                {
                    if (connectedPorts.Length == 1)
                    {
                        string message = "Warning: specified port " + PORT_ID + " not detected. Attempting to use " + connectedPorts[0] + " instead.";
                        Debug.WriteLine(message);
                        PORT_ID = connectedPorts[0];
                    }
                    else
                    {
                        string message = "Error: specified port " + PORT_ID + " not detected. Available ports are: ";
                        foreach (string port in connectedPorts) message += port + ", ";
                        message = message.TrimEnd(',', ' ');
                        Debug.WriteLine(message);
                        return false;
                    }
                }

                device = new SerialPort(PORT_ID, BAUD_RATE);
                device.DtrEnable = true;
                device.RtsEnable = true;
                device.Open();
                Task.Factory.StartNew(() =>
                {
                    while (device == null || !device.IsOpen || device.BytesToRead == 0) ;

                    while (IsConnected)
                    {
                        //try
                        //{
                        //    string line = device.ReadLine().TrimEnd();
                        //    string[] parts = line.Split(',');
                        //    float ax = float.Parse(parts[0]) * ACCEL_UNIT;
                        //    float ay = float.Parse(parts[1]) * ACCEL_UNIT;
                        //    float az = float.Parse(parts[2]) * ACCEL_UNIT;
                        //    float mx = float.Parse(parts[3]) * MAG_UNIT;
                        //    float my = float.Parse(parts[4]) * MAG_UNIT;
                        //    float mz = float.Parse(parts[5]) * MAG_UNIT;
                        //    float gx = float.Parse(parts[6]) * GYRO_UNIT;
                        //    float gy = float.Parse(parts[7]) * GYRO_UNIT;
                        //    float gz = float.Parse(parts[8]) * GYRO_UNIT;
                        //    float ir1 = float.Parse(parts[9]) * IR_UNIT;
                        //    float ir2 = float.Parse(parts[10]) * IR_UNIT;

                        //    Reading reading = new Reading();
                        //    reading.Accelerometer1 = new Point3D(ax, ay, az);
                        //    reading.Magnetometer1 = new Point3D(mx, my, mz);
                        //    reading.Gyroscope1 = new Point3D(gx, gy, gz);
                        //    reading.InfraredReflectance1 = ir1;
                        //    reading.InfraredReflectance2 = ir2;

                        //    OnReadingAvailable(reading);
                        //}
                        //catch { }

                        byte[] buffer = new byte[1024];
                        byte b = 0, i = 0;
                        bool completeLine = false;
                        while(!completeLine)
                        {
                            try
                            {
                                b = (byte)device.ReadByte();
                                buffer[i] = b;
                                if (b == '\n' && buffer[(i + buffer.Length - 1) % buffer.Length] == '\r' && buffer[(i + buffer.Length - 2) % buffer.Length] == '\n' && buffer[(i + buffer.Length - 3) % buffer.Length] == '\r') completeLine = true;
                                i++;
                                if (i >= buffer.Length) i = 0;
                            }
                            catch (Exception)
                            {

                            }
                        }

                        if(i > 22)
                        {
                            int bufferIndex = 0;
                            float ir1 = BitConverter.ToInt16(buffer, bufferIndex++ * 2) * IR_UNIT;
                            float ir2 = BitConverter.ToInt16(buffer, bufferIndex++ * 2) * IR_UNIT;
                            float ax = BitConverter.ToInt16(buffer, bufferIndex++ * 2) * ACCEL_UNIT;
                            float ay = BitConverter.ToInt16(buffer, bufferIndex++ * 2) * ACCEL_UNIT;
                            float az = BitConverter.ToInt16(buffer, bufferIndex++ * 2) * ACCEL_UNIT;
                            float mx = BitConverter.ToInt16(buffer, bufferIndex++ * 2);
                            float my = BitConverter.ToInt16(buffer, bufferIndex++ * 2);
                            float mz = BitConverter.ToInt16(buffer, bufferIndex++ * 2);
                            float gx = BitConverter.ToInt16(buffer, bufferIndex++ * 2) * GYRO_UNIT;
                            float gy = BitConverter.ToInt16(buffer, bufferIndex++ * 2) * GYRO_UNIT;
                            float gz = BitConverter.ToInt16(buffer, bufferIndex++ * 2) * GYRO_UNIT;

                            Reading reading = new Reading();
                            reading.Accelerometer1 = new Point3D(ax, ay, az);
                            reading.Magnetometer1 = CorrectMagnetometer1Bias(mx, my, mz);
                            reading.Magnetometer1.X *= MAG_UNIT;
                            reading.Magnetometer1.Y *= MAG_UNIT;
                            reading.Magnetometer1.Z *= MAG_UNIT;
                            reading.Gyroscope1 = new Point3D(gx, gy, gz);
                            reading.InfraredReflectance1 = ir1;
                            reading.InfraredReflectance2 = ir2;

                            if (numSensors == 2)
                            {
                                ax = BitConverter.ToInt16(buffer, bufferIndex++ * 2) * ACCEL_UNIT;
                                ay = BitConverter.ToInt16(buffer, bufferIndex++ * 2) * ACCEL_UNIT;
                                az = BitConverter.ToInt16(buffer, bufferIndex++ * 2) * ACCEL_UNIT;
                                mx = BitConverter.ToInt16(buffer, bufferIndex++ * 2);
                                my = BitConverter.ToInt16(buffer, bufferIndex++ * 2);
                                mz = BitConverter.ToInt16(buffer, bufferIndex++ * 2);
                                gx = BitConverter.ToInt16(buffer, bufferIndex++ * 2) * GYRO_UNIT;
                                gy = BitConverter.ToInt16(buffer, bufferIndex++ * 2) * GYRO_UNIT;
                                gz = BitConverter.ToInt16(buffer, bufferIndex++ * 2) * GYRO_UNIT;
                                
                                reading.Accelerometer2 = new Point3D(ax, ay, az);
                                reading.Magnetometer2 = CorrectMagnetometer2Bias(mx, my, mz);
                                reading.Magnetometer2.X *= MAG_UNIT;
                                reading.Magnetometer2.Y *= MAG_UNIT;
                                reading.Magnetometer2.Z *= MAG_UNIT;
                                reading.Gyroscope2 = new Point3D(gx, gy, gz);
                            }

                            OnReadingAvailable(reading);
                        }
                        else
                        {
                            Debug.WriteLine("Wrong number of bytes");
                        }
                    }
                });
                
                if(!IsConnected) 
                    Debug.WriteLine("Error: could not connect to port " + PORT_ID + ". No exceptions were thrown.");
                return IsConnected;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: could not connect to port " + PORT_ID + ". Exception: " + ex.Message);
                device = null;
                return false;
            }
        }

        public void Disconnect()
        {
            if (IsConnected)
            {
                Brightness = 0;
                device.Close();
                device.Dispose();
                device = null;
            }
        }
    }
}
