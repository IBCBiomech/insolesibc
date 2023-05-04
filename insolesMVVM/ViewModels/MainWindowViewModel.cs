using CommunityToolkit.Mvvm.Messaging;
using insolesMVVM.Messages;
using System.Diagnostics;

namespace insolesMVVM.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public string Greeting => "Welcome to Avalonia!";
        public MainWindowViewModel() 
        {
            WeakReferenceMessenger.Default.Register<TestMessage>(this, onTestMessageReceived);
        }
        public void onTestMessageReceived(object sender, TestMessage args)
        {
            Trace.WriteLine("CommunityToolkit.Mvvm.Messaging works");
        }
    }
}