using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;

namespace HandSightLibrary
{
    public class Sensors
    {
        float ACCEL_UNIT = 0.732f / 1000.0f * 9.80665F; // m/s^2
        float MAG_UNIT = 0.48f / 1000.0f; // gauss
        float GYRO_UNIT = 0.07000f; // deg / s
        float IR_UNIT = 1.0f / 1024.0f; // % max intensity

        string PORT_ID = "COM3"; // TODO: provide an interface to select the port, and save to user preference
        int BAUD_RATE = 250000;

        public struct Point3D
        {
            public Point3D(float x, float y, float z) { this.X = x; this.Y = y; this.Z = z; }
            public float X;
            public float Y;
            public float Z;
        }
        public class Reading
        {
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
                                if (b == '\n' && buffer[i - 1] == '\r' && buffer[i-2] == '\n' && buffer[i-3] == '\r') completeLine = true;
                                i++;
                                if (i >= buffer.Length) i = 0;
                            }
                            catch (Exception) { }
                        }

                        if(i > 44)
                        {
                            int bufferIndex = 0;
                            float ir1 = BitConverter.ToSingle(buffer, bufferIndex++ * 4) * IR_UNIT;
                            float ir2 = BitConverter.ToSingle(buffer, bufferIndex++ * 4) * IR_UNIT;
                            float ax = BitConverter.ToSingle(buffer, bufferIndex++ * 4) * ACCEL_UNIT;
                            float ay = BitConverter.ToSingle(buffer, bufferIndex++ * 4) * ACCEL_UNIT;
                            float az = BitConverter.ToSingle(buffer, bufferIndex++ * 4) * ACCEL_UNIT;
                            float mx = BitConverter.ToSingle(buffer, bufferIndex++ * 4) * MAG_UNIT;
                            float my = BitConverter.ToSingle(buffer, bufferIndex++ * 4) * MAG_UNIT;
                            float mz = BitConverter.ToSingle(buffer, bufferIndex++ * 4) * MAG_UNIT;
                            float gx = BitConverter.ToSingle(buffer, bufferIndex++ * 4) * GYRO_UNIT;
                            float gy = BitConverter.ToSingle(buffer, bufferIndex++ * 4) * GYRO_UNIT;
                            float gz = BitConverter.ToSingle(buffer, bufferIndex++ * 4) * GYRO_UNIT;

                            Reading reading = new Reading();
                            reading.Accelerometer1 = new Point3D(ax, ay, az);
                            reading.Magnetometer1 = new Point3D(mx, my, mz);
                            reading.Gyroscope1 = new Point3D(gx, gy, gz);
                            reading.InfraredReflectance1 = ir1;
                            reading.InfraredReflectance2 = ir2;

                            if (numSensors == 2)
                            {
                                ax = BitConverter.ToSingle(buffer, bufferIndex++ * 4) * ACCEL_UNIT;
                                ay = BitConverter.ToSingle(buffer, bufferIndex++ * 4) * ACCEL_UNIT;
                                az = BitConverter.ToSingle(buffer, bufferIndex++ * 4) * ACCEL_UNIT;
                                mx = BitConverter.ToSingle(buffer, bufferIndex++ * 4) * MAG_UNIT;
                                my = BitConverter.ToSingle(buffer, bufferIndex++ * 4) * MAG_UNIT;
                                mz = BitConverter.ToSingle(buffer, bufferIndex++ * 4) * MAG_UNIT;
                                gx = BitConverter.ToSingle(buffer, bufferIndex++ * 4) * GYRO_UNIT;
                                gy = BitConverter.ToSingle(buffer, bufferIndex++ * 4) * GYRO_UNIT;
                                gz = BitConverter.ToSingle(buffer, bufferIndex++ * 4) * GYRO_UNIT;
                                
                                reading.Accelerometer2 = new Point3D(ax, ay, az);
                                reading.Magnetometer2 = new Point3D(mx, my, mz);
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
