﻿using insoles.Services;
using insoles.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace insoles.Commands
{
    public class StopCommand : ICommand
    {
        private RegistroState state;
        private IApiService apiService;
        private ISaveService saveService; 
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public StopCommand(RegistroState state, IApiService apiService,ISaveService saveService)
        {
            this.state = state;
            this.apiService = apiService;
            this.saveService = saveService;
        }

        public bool CanExecute(object? parameter)
        {
            return state.recording;
        }

        public void Execute(object? parameter)
        {
            apiService.Stop();
            saveService.Stop();
            state.capturing = false;
            state.recording = false;
        }
    }
}
