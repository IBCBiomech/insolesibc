namespace mvvm.Messages
{
    public class OpenCameraMessage : Message
    {
        public OpenCameraMessage(int index)
        {
            this.index = index;
        }
        public int index { get; set; }
    }
}
