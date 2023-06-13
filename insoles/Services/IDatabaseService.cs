﻿using insoles.Database;
using insoles.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insoles.Services
{
    public interface IDatabaseService
    {
        void AddPaciente(Paciente paciente);
        List<Paciente> GetPacientes();
    }
}
