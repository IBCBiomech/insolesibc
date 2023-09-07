using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insoles.Services
{
    public class RetocarPlantillaReducedService : IRetocarPlantillaService
    {
        private ICodesService codes;
        public RetocarPlantillaReducedService(ICodesService codes)
        {
            this.codes = codes;
        }
        public Matrix<float> retocar(Matrix<float> matrix)
        {
            for(int i = 54; i <= 63; i++)
            {
                matrix[105, i] = codes.Background();
            }
            for (int i = 56; i <= 60; i++)
            {
                matrix[140, i] = codes.Background();
            }
            return matrix;
        }
    }
}
