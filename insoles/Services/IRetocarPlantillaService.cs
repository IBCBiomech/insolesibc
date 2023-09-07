using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insoles.Services
{
    public interface IRetocarPlantillaService
    {
        public Matrix<float> retocar(Matrix<float> matrix);
    }
}
