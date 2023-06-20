using insoles.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insoles.Services
{
    public interface ICodesService
    {
        float GetCode(Sensor s);
        float Foot();
        float Background();
        bool IsSensor(float code);
        bool IsValidCode(float code);
        Sensor GetSensor(float code);
    }
}
