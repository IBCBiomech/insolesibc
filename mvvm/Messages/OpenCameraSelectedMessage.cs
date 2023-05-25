namespace mvvm.Messages
{
    public class OpenCameraSelectedMessage : Message
    {
        public OpenCameraSelectedMessage(int index)
        {
            this.index = index;
        }
        public int index { get; set; }
    }
}
