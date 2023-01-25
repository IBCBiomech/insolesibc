using insoles.DeviceList.TreeClasses;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace insoles.DeviceList
{
    public partial class DeviceList : Page
    {
        private const int MAX_IMU_USED = 2;
        private MainWindow mainWindow;
        //private const Key multiselectKey = Key.LeftCtrl;
        //private bool multiSelectionKeyPressed = false;
        //public List<TreeViewItem> selected { get;private set; } 
        public DeviceList()
        {
            InitializeComponent();
            mainWindow = Application.Current.MainWindow as MainWindow;
            baseItem.IsExpanded = true;
        }

        // Funciones para eliminar todos los elementos de IMU, camara y Insoles
        #region clear
        public void clearAll()
        {
            clearCameras();
            clearInsoles();
        }
        public void clearCameras()
        {
            VM.cameras.Clear();
        }
        public void clearInsoles()
        {
            VM.insoles.Clear();
        }
        #endregion
        // Funciones para get y set la coleccion entera de IMU, camara y Insoles
        // y funciones para añadir un elemento a la coleccion
        #region getters setters
        #region Cameras
        public ObservableCollection<CameraInfo> getCameras()
        {
            return VM.cameras;
        }
        public void setCameras(ObservableCollection<CameraInfo> cameras)
        {
            VM.cameras = cameras;
        }
        public void setCameras(List<CameraInfo> cameras)
        {
            VM.cameras = new ObservableCollection<CameraInfo>(VM.cameras.Where(cam => cameras.Any(newCamera => newCamera.name == cam.name)));
            foreach (CameraInfo cam in cameras)
            {
                if (!VM.cameras.Any(camOld => camOld.name == cam.name))
                {
                    VM.cameras.Add(cam);
                }
                else // Cambiar el numero de la camara si es diferente (no se si con las camaras es necesario)
                {
                    int index = VM.cameras.ToList().FindIndex(camOld => camOld.name == cam.name);
                    if (VM.cameras[index].number != cam.number)
                    {
                        VM.cameras[index].number = cam.number;
                    }
                }
            }
        }
        public void addCamera(CameraInfo camera)
        {
            if (!camerainList(camera))
            {
                VM.cameras.Add(camera);
            }
        }
        private bool camerainList(CameraInfo camera)
        {
            foreach (CameraInfo cameraInList in VM.cameras)
            {
                if (camera.number == cameraInList.number) // Tiene que identificar a una camara de forma unica
                {
                    return true;
                }
            }
            return false;
        }
        #endregion
        #region Insoles
        public ObservableCollection<InsolesInfo> getInsoles()
        {
            return VM.insoles;
        }
        public void setInsoles(ObservableCollection<InsolesInfo> insoles)
        {
            VM.insoles = insoles;
        }
        public void addInsole(InsolesInfo insole)
        {
            if (!VM.insoles.Contains(insole))
            {
                VM.insoles.Add(insole);
            }
        }
        #endregion
        #endregion
        // Funciones para mostrar y esconder el header y los elementos de IMU, camara y Insoles
        #region show hide
        #region cameras
        public void showCameras()
        {
            cameras.Visibility = Visibility.Visible;
        }
        public void hideCameras()
        {
            cameras.Visibility = Visibility.Collapsed;
        }
        #endregion
        #region insoles
        public void showInsoles()
        {
            insoles.Visibility = Visibility.Visible;
        }
        public void hideInsoles()
        {
            insoles.Visibility = Visibility.Collapsed;
        }
        #endregion
        #endregion
        // Funciones que manejan el hacer doble click sobre un IMU o una Camara
        #region double click handlers
        private void onIMUDoubleClick(object sender, MouseButtonEventArgs args)
        {
            if (sender is MultiSelectTreeViewItem)
            {
                if (!((MultiSelectTreeViewItem)sender).IsSelected)
                {
                    return;
                }
            }
            //connectIMU((MultiSelectTreeViewItem)sender);
        }
        private void onCameraDoubleClick(object sender, MouseButtonEventArgs args)
        {
            if (sender is MultiSelectTreeViewItem)
            {
                if (!((MultiSelectTreeViewItem)sender).IsSelected)
                {
                    return;
                }
            }
            //connectCamera((MultiSelectTreeViewItem)sender);
        }
        #endregion
        // Funcion que se llama al conectar una camara (doble click o boton connect) para cambiar el TreeView
        public void connectCamera(MultiSelectTreeViewItem treeViewItem)
        {
            int calculateFps(int number)
            {
                return 120;
            }
            CameraInfo cameraInfo = treeViewItem.DataContext as CameraInfo;
            cameraInfo.fps = calculateFps(cameraInfo.number);
        }
        public void connectIMU(string mac, byte handler)
        {
            InsolesInfo imuInfo = VM.insoles.Where((insole) => insole.address == mac).First();
            imuInfo.handler = handler;
            imuInfo.connected = true;
        }
    }
}
