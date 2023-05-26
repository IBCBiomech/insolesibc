using insoles.Messages;
using System.Collections.Generic;

namespace insoles.Services
{
    public interface ICameraService
    {
        public void Scan();
        public void OpenCamera(int index);
        public delegate void CameraScanEventHandler(List<CameraScan> data);
        public event CameraScanEventHandler ScanReceived;
    }
}
