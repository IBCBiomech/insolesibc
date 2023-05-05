using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive;
using ReactiveUI;
using CommunityToolkit.Mvvm.Messaging;
using insolesMVVM.Messages;

namespace insolesMVVM.ViewModels
{
	public class ToolBarViewModel : ViewModelBase
	{
        private readonly ReactiveCommand<Unit, Unit> _scanCommand;
        private readonly ReactiveCommand<Unit, Unit> _connectCommand;
        private readonly ReactiveCommand<Unit, Unit> _openCameraCommand;
        private readonly ReactiveCommand<Unit, Unit> _recordCommand;

        public ToolBarViewModel()
        {
            _scanCommand = ReactiveCommand.Create(() => 
            {
                Trace.WriteLine("Scan");
                WeakReferenceMessenger.Default.Send(new ScanMessage());
            });
            _connectCommand = ReactiveCommand.Create(() => {
                Trace.WriteLine("Connect");
            });
            _openCameraCommand = ReactiveCommand.Create(() => {
                Trace.WriteLine("Open Camera");
            });
            _recordCommand = ReactiveCommand.Create(() => {
                Trace.WriteLine("Record");
            });
        }

        public ReactiveCommand<Unit, Unit> ScanCommand => _scanCommand;
        public ReactiveCommand<Unit, Unit> ConnectCommand => _connectCommand;
        public ReactiveCommand<Unit, Unit> OpenCameraCommand => _openCameraCommand;
        public ReactiveCommand<Unit, Unit> RecordCommand => _recordCommand;
    }
}