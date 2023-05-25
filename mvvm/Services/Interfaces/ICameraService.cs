using OpenCvSharp;

namespace mvvm.Services.Interfaces
{
    public interface ICameraService
    {
        Mat GetInitFrame();
    }
}
