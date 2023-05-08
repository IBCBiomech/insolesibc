using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.Messaging;
using HarfBuzzSharp;
using insolesMVVM.Messages;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.WpfExtensions;
using ReactiveUI;

namespace insolesMVVM.ViewModels
{
	public class CameraViewportViewModel : ViewModelBase
	{
		private Bitmap currentFrame;
		public Bitmap CurrentFrame {
			get => currentFrame;
			set
			{
                this.RaiseAndSetIfChanged(ref currentFrame, value);
            }
		}
		public CameraViewportViewModel()
		{
			var initMat = ((App)Application.Current).CameraService.GetInitFrame();
            CurrentFrame = MatToBitmap(initMat);
            WeakReferenceMessenger.Default.Register<FrameAvailableMessage>(this, ChangeFrame);
        }
		private void ChangeFrame(object sender, FrameAvailableMessage message)
		{
			//Trace.WriteLine("ChangeFrame from CamaraViewportViewModel");
			//Trace.WriteLine(message.frame.Height + "x" + message.frame.Width);
			var mat = message.frame;
			CurrentFrame = MatToBitmap(mat);
        }
		private Bitmap MatToBitmap(Mat mat)
		{
            var bitmap = BitmapConverter.ToBitmap(mat);
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;
                return new Bitmap(memory);
            }
        }
    }
}