namespace HandSightLibrary
{
    public class EulerAngles
    {
        public double Yaw, Pitch, Roll;
        public EulerAngles() : this(0, 0, 0) { }
        public EulerAngles(double yaw, double pitch, double roll)
        {
            Yaw = yaw;
            Pitch = pitch;
            Roll = roll;
        }
    }
}
