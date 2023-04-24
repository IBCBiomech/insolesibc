using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mvvm.Services
{
    public interface IApiService
    {
        void Scan();
        void Connect();
    }
}
