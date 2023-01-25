using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Media;
using insoles.FileBrowser.Enums;
using insoles.Common;
using System.Windows;
using System.Diagnostics;
using System.Windows.Threading;

// Almacena todos los drives, carpetas y ficheros. Se encarga de la interacción entre el usuario y la UI.
namespace insoles.FileBrowser.ShellClasses
{
    public class FileSystemObjectInfo : BaseObject
    {
        public FileSystemObjectInfo(FileSystemInfo info)
        {
            if (this is DummyFileSystemObjectInfo)
            {
                return;
            }

            Children = new ObservableCollection<FileSystemObjectInfo>();
            FileSystemInfo = info;

            if (info is DirectoryInfo)
            {
                ImageSource = FolderManager.GetImageSource(info.FullName, ItemState.Close);
                AddDummy();
            }
            else if (info is FileInfo)
            {
                ImageSource = FileManager.GetImageSource(info.FullName);
            }

            PropertyChanged += new PropertyChangedEventHandler(FileSystemObjectInfo_PropertyChanged);
        }

        public FileSystemObjectInfo(DriveInfo drive)
            : this(drive.RootDirectory)
        {
        }

        #region Events

        public event EventHandler BeforeExpand;

        public event EventHandler AfterExpand;

        public event EventHandler BeforeExplore;

        public event EventHandler AfterExplore;

        private void RaiseBeforeExpand()
        {
            BeforeExpand?.Invoke(this, EventArgs.Empty);
        }

        private void RaiseAfterExpand()
        {
            AfterExpand?.Invoke(this, EventArgs.Empty);
        }

        private void RaiseBeforeExplore()
        {
            BeforeExplore?.Invoke(this, EventArgs.Empty);
        }

        private void RaiseAfterExplore()
        {
            AfterExplore?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region EventHandlers
        // Explora directorios y archivos cuando se expande si es un directorio
        void FileSystemObjectInfo_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (FileSystemInfo is DirectoryInfo)
            {
                if (string.Equals(e.PropertyName, "IsExpanded", StringComparison.CurrentCultureIgnoreCase))
                {
                    RaiseBeforeExpand();
                    if (IsExpanded)
                    {
                        ImageSource = FolderManager.GetImageSource(FileSystemInfo.FullName, ItemState.Open);
                        if (HasDummy())
                        {
                            RaiseBeforeExplore();
                            RemoveDummy();
                            ExploreDirectories();
                            ExploreFiles();
                            RaiseAfterExplore();
                        }
                    }
                    else
                    {
                        ImageSource = FolderManager.GetImageSource(FileSystemInfo.FullName, ItemState.Close);
                    }
                    RaiseAfterExpand();
                }
            }
        }

        #endregion

        #region Properties

        public ObservableCollection<FileSystemObjectInfo> Children
        {
            get { return GetValue<ObservableCollection<FileSystemObjectInfo>>("Children"); }
            private set { SetValue("Children", value); }
        }

        public ImageSource ImageSource
        {
            get { return GetValue<ImageSource>("ImageSource"); }
            private set { SetValue("ImageSource", value); }
        }

        public bool IsExpanded
        {
            get { return GetValue<bool>("IsExpanded"); }
            set { SetValue("IsExpanded", value); }
        }

        public bool IsVisited
        {
            get
            {
                return !HasDummy();
            }
        }

        public FileSystemInfo FileSystemInfo
        {
            get { return GetValue<FileSystemInfo>("FileSystemInfo"); }
            private set { SetValue("FileSystemInfo", value); }
        }

        private DriveInfo Drive
        {
            get { return GetValue<DriveInfo>("Drive"); }
            set { SetValue("Drive", value); }
        }

        public Visibility CanOpenWith
        {
            get 
            { 
                if(FileSystemInfo is FileInfo)
                {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        #endregion

        #region Methods
        private void AddDummy()
        {
            Children.Add(new DummyFileSystemObjectInfo());
        }

        private bool HasDummy()
        {
            return GetDummy() != null;
        }

        private DummyFileSystemObjectInfo GetDummy()
        {
            return Children.OfType<DummyFileSystemObjectInfo>().FirstOrDefault();
        }

        private void RemoveDummy()
        {
            Children.Remove(GetDummy());
        }
        // Explora los directorios de un directiorio
        private void ExploreDirectories()
        {
            if (Drive?.IsReady == false)
            {
                return;
            }
            if (FileSystemInfo is DirectoryInfo)
            {
                var directories = ((DirectoryInfo)FileSystemInfo).GetDirectories();
                foreach (var directory in directories.OrderBy(d => d.Name))
                {
                    if (!Config.showOnlyInitialPath || FileBrowser.IsParentPath(Config.INITIAL_PATH, directory.FullName) ||
                        FileBrowser.IsParentPath(directory.FullName, Config.INITIAL_PATH)) {
                        if ((directory.Attributes & FileAttributes.System) != FileAttributes.System &&
                            (directory.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                        {
                            var fileSystemObject = new FileSystemObjectInfo(directory);
                            fileSystemObject.BeforeExplore += FileSystemObject_BeforeExplore;
                            fileSystemObject.AfterExplore += FileSystemObject_AfterExplore;
                            Children.Add(fileSystemObject);
                        }
                    }
                }
            }
        }

        private void FileSystemObject_AfterExplore(object sender, EventArgs e)
        {
            RaiseAfterExplore();
        }

        private void FileSystemObject_BeforeExplore(object sender, EventArgs e)
        {
            RaiseBeforeExplore();
        }
        // Explora los ficheros de un directiorio
        private void ExploreFiles()
        {
            if (Drive?.IsReady == false)
            {
                return;
            }
            if (FileSystemInfo is DirectoryInfo)
            {
                var files = ((DirectoryInfo)FileSystemInfo).GetFiles();
                foreach (var file in files.OrderBy(d => d.Name))
                {
                    if (!Config.showOnlyInitialPath || FileBrowser.IsParentPath(file.FullName, Config.INITIAL_PATH))
                    {
                        if ((file.Attributes & FileAttributes.System) != FileAttributes.System &&
                        (file.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden && showFile(file))
                        {
                            Children.Add(new FileSystemObjectInfo(file));
                        }
                    }
                }
            }
        }
        public void ReExploreFiles()
        {
            if (Drive?.IsReady == false)
            {
                return;
            }
            if (FileSystemInfo is DirectoryInfo)
            {
                var files = ((DirectoryInfo)FileSystemInfo).GetFiles();
                foreach (var file in files.OrderBy(d => d.Name))
                {
                    FileSystemObjectInfo fileSystemObjectInfo = new FileSystemObjectInfo(file);
                    if (showFile(file) && !Children.Any((FileSystemObjectInfo fsoi) =>
                    {
                        Trace.WriteLine(fsoi.FileSystemInfo.FullName);
                        Trace.WriteLine(file.FullName);
                        return string.Equals(fsoi.FileSystemInfo.FullName, file.FullName);
                    }))
                    {
                        Dispatcher.CurrentDispatcher.BeginInvoke(() =>
                        {
                            Children.Add(fileSystemObjectInfo);
                            Trace.WriteLine("Added " + fileSystemObjectInfo.FileSystemInfo.FullName);
                        });
                    }
                }
            }
        }
        public void ExploreFile(string filename)
        {
            if (Drive?.IsReady == false)
            {
                return;
            }
            if (FileSystemInfo is DirectoryInfo)
            {
                var files = ((DirectoryInfo)FileSystemInfo).GetFiles();
                foreach (var file in files.OrderBy(d => d.Name))
                {
                    if(string.Equals(file.FullName, filename))
                    {
                        FileSystemObjectInfo fileSystemObjectInfo = new FileSystemObjectInfo(file);
                        Dispatcher.CurrentDispatcher.BeginInvoke(() =>
                        {
                            Children.Add(fileSystemObjectInfo);
                            Trace.WriteLine("Added " + fileSystemObjectInfo.FileSystemInfo.FullName);
                        });
                    }
                }
            }
        }
        // Devuelve true si se tiene que mostrar el fichero
        private bool showFile(FileInfo file)
        {
            return Config.allowedExtensions.Contains(file.Extension);
        }

        #endregion
    }
}
