namespace HandSightLibrary
{
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
}
