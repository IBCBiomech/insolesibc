using insoles.ToolBar.Enums;
using System.ComponentModel;
using System.Diagnostics;

namespace insoles.ToolBar
{
    public class VirtualToolBarProperties : INotifyPropertyChanged
    {
        private VirtualToolBar virtualToolBar;

        public event PropertyChangedEventHandler PropertyChanged;
        // Mejor sacarlo programaticamente. De momento lo he dejado asi
        // scanEnabledString => expresion que retorna get 
        public string scanEnabledString = "return !virtualToolBar.capturing && virtualToolBar.recordState == RecordState.RecordStopped;";
        public bool scanEnabled
        {
            get
            {
                return !virtualToolBar.capturing
                    && virtualToolBar.recordState == RecordState.RecordStopped;
            }
            set
            {
                // Esto lo dejo porque con OneWay no funciona. Si se puede hacer con OneWay quitar los set
            }
        }
        public string connectEnabledString = "return virtualToolBar.buttonsEnabled && !virtualToolBar.capturing && virtualToolBar.recordState == RecordState.RecordStopped;";
        public bool connectEnabled
        {
            get
            {
                return virtualToolBar.buttonsEnabled && !virtualToolBar.capturing
                    && virtualToolBar.recordState == RecordState.RecordStopped;
            }
            set
            {

            }
        }
        public string disconnectEnabledString = "return virtualToolBar.buttonsEnabled && !virtualToolBar.capturing && virtualToolBar.recordState == RecordState.RecordStopped;";
        public bool disconnectEnabled
        {
            get
            {
                return virtualToolBar.buttonsEnabled && !virtualToolBar.capturing
                    && virtualToolBar.recordState == RecordState.RecordStopped;
            }
            set
            {

            }
        }
        public string captureEnabledString = "return virtualToolBar.buttonsEnabled && virtualToolBar.recordState == RecordState.RecordStopped && !virtualToolBar.capturing;";
        public bool captureEnabled
        {
            get
            {
                return virtualToolBar.buttonsEnabled && virtualToolBar.recordState == RecordState.RecordStopped &&
                    !virtualToolBar.capturing;
            }
            set
            {

            }
        }
        public string recordEnabledString = "return virtualToolBar.buttonsEnabled && virtualToolBar.capturing && virtualToolBar.recordState == RecordState.RecordStopped;";
        public bool recordEnabled
        {
            get
            {
                return virtualToolBar.buttonsEnabled && virtualToolBar.capturing &&
                    virtualToolBar.recordState == RecordState.RecordStopped;
            }
            set
            {

            }
        }
        public string videoCheckboxEnabledString = "return virtualToolBar.recordState == RecordState.RecordStopped && camaraViewport.someCameraOpened();";
        public bool videoCheckboxEnabled
        {
            get
            {
                return virtualToolBar.recordState == RecordState.RecordStopped;
                /*
                return virtualToolBar.recordState == RecordState.RecordStopped &&
                    camaraViewport.someCameraOpened();
                */
            }
            set
            {

            }
        }
        public string pauseEnabledString = "return virtualToolBar.buttonsEnabled && virtualToolBar.capturing;";
        public bool pauseEnabled
        {
            get
            {
                return virtualToolBar.buttonsEnabled && virtualToolBar.capturing;
            }
            set
            {

            }
        }
        public string stopEnabledString = "return virtualToolBar.buttonsEnabled && virtualToolBar.capturing;";
        public bool stopEnabled
        {
            get
            {
                return virtualToolBar.buttonsEnabled && virtualToolBar.capturing;
            }
            set
            {

            }
        }
        public string openCameraEnabledString = "return virtualToolBar.buttonsEnabled;";
        public bool openCameraEnabled
        {
            get
            {
                return virtualToolBar.buttonsEnabled;
            }
            set
            {

            }
        }
        public string capturedFilesEnabledString = "return virtualToolBar.buttonsEnabled && virtualToolBar.recordState == RecordState.RecordStopped;";
        public bool capturedFilesEnabled
        {
            get
            {
                return virtualToolBar.recordState == RecordState.RecordStopped;
            }
            set
            {

            }
        }
        public string[] vars = new string[] { "scanEnabled", "connectEnabled", "disconnectEnabled",
            "captureEnabled", "recordEnabled", "videoCheckboxEnabled", "pauseEnabled", "stopEnabled",
            "openCameraEnabled", "capturedFilesEnabled"};
        public VirtualToolBarProperties(VirtualToolBar virtualToolBar)
        {
            void addPropertiesChanged(string keyword)
            {
                bool shouldAdd(string var, string keyword)
                {
                    Trace.WriteLine(var + "String");
                    string expression = (string)GetType().GetField(var + "String").GetValue(this);
                    Trace.WriteLine(expression);
                    return expression.Contains(keyword);
                }
                foreach(string var in vars)
                {
                    if (shouldAdd(var, keyword))
                    {
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(var));
                    }
                }
            }
            void initCameraViewport()
            {
                /*
                camaraViewport.cameraChanged += (s, e) =>
                {
                    addPropertiesChanged("camaraViewport.someCameraOpened()");
                };
                */
            }
            this.virtualToolBar = virtualToolBar;
            virtualToolBar.buttonsEnabledChanged += (s, e) =>
            {
                addPropertiesChanged("virtualToolBar.buttonsEnabled");
            };
            virtualToolBar.recordChanged += (s, e) =>
            {
                addPropertiesChanged("virtualToolBar.recordState");
            };
            virtualToolBar.captureChanged += (s, e) =>
            {
                addPropertiesChanged("virtualToolBar.capturing");
            };
            /*
            if(((MainWindow)Application.Current.MainWindow).camaraViewport.Content != null)
            {
                camaraViewport = ((MainWindow)Application.Current.MainWindow).camaraViewport.Content as CamaraViewport.CamaraViewport;
                initCameraViewport();
            }
            else
            {
                ((MainWindow)Application.Current.MainWindow).camaraViewport.Navigated += (s, e) =>
                {
                    camaraViewport = ((MainWindow)Application.Current.MainWindow).camaraViewport.Content as CamaraViewport.CamaraViewport;
                    initCameraViewport();
                };
            }
            */
        }
    }
}
