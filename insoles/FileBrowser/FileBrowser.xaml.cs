using System.Windows.Controls;
using System;
using System.IO;
using System.Linq;
using System.Windows.Input;
using insoles.FileBrowser.ShellClasses;
using System.Diagnostics;
using System.Windows;
using insoles.DeviceList.TreeClasses;
using System.Collections.Generic;

namespace insoles.FileBrowser
{
    public partial class FileBrowser : Page
    {
        public FileBrowser()
        {
            InitializeComponent();
            InitializeFileSystemObjects();
            try
            {
                finishInit();
            }
            catch
            {
                MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
                mainWindow.initialized += (sender, args) => finishInit();
            }
        }
        private void finishInit()
        {
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.fileSaver.filesAdded += OnFilesAdded;
        }
        #region Events

        private void FileSystemObject_AfterExplore(object sender, System.EventArgs e)
        {
            Cursor = Cursors.Arrow;
        }

        private void FileSystemObject_BeforeExplore(object sender, System.EventArgs e)
        {
            Cursor = Cursors.Wait;
        }
        // Funcion que se ejecuta al hacer doble click sobre un fichero
        private void onItemDoubleClick(object sender, EventArgs e)
        {
            if (sender is TreeViewItem)
            {
                if (!((TreeViewItem)sender).IsSelected)
                {
                    return;
                }
            }
            FileSystemObjectInfo fileSystemObjectInfo = ((TreeViewItem)sender).DataContext as FileSystemObjectInfo;
            FileSystemInfo fileSystemInfo = fileSystemObjectInfo.FileSystemInfo;
            if(fileSystemInfo is DirectoryInfo)
            {
                return;
            }
            string path = fileSystemInfo.FullName; // Usar este path para acceder al fichero
            Trace.WriteLine(path);
        }

        #endregion

        #region Methods
        // Inicializa los drives y expande el Escritorio
        private void InitializeFileSystemObjects()
        {
            if (Config.showOnlyInitialPath)
            {
                initializeOnlyInitialPath(Config.INITIAL_PATH);
                PreSelect(Config.INITIAL_PATH);
            }
            else {
                var drives = DriveInfo.GetDrives();
                DriveInfo
                    .GetDrives()
                    .ToList()
                    .ForEach(drive =>
                    {
                        var fileSystemObject = new FileSystemObjectInfo(drive);
                        fileSystemObject.BeforeExplore += FileSystemObject_BeforeExplore;
                        fileSystemObject.AfterExplore += FileSystemObject_AfterExplore;
                        treeView.Items.Add(fileSystemObject);
                    });
                PreSelect(Config.INITIAL_PATH);
            }
        }
        private void initializeOnlyInitialPath(string path)
        {
            DriveInfo
                .GetDrives()
                .ToList()
                .ForEach(drive =>
                {
                    var fileSystemObject = new FileSystemObjectInfo(drive);
                    if (IsParentPath(path, fileSystemObject.FileSystemInfo.FullName))
                    {
                        fileSystemObject.BeforeExplore += FileSystemObject_BeforeExplore;
                        fileSystemObject.AfterExplore += FileSystemObject_AfterExplore;
                        treeView.Items.Add(fileSystemObject);
                    }     
                });
        }
        // Carpeta seleccionada por defecto
        private void PreSelect(string path)
        {
            if (!Directory.Exists(path))
            {
                return;
            }
            var driveFileSystemObjectInfo = GetDriveFileSystemObjectInfo(path);
            driveFileSystemObjectInfo.IsExpanded = true;
            PreSelect(driveFileSystemObjectInfo, path);
        }
        // Funcion auxiliar recursiva de Preselect
        private void PreSelect(FileSystemObjectInfo fileSystemObjectInfo,
            string path)
        {
            foreach (var childFileSystemObjectInfo in fileSystemObjectInfo.Children)
            {
                var isParentPath = IsParentPath(path, childFileSystemObjectInfo.FileSystemInfo.FullName);
                if (isParentPath)
                {
                    if (string.Equals(childFileSystemObjectInfo.FileSystemInfo.FullName, path))
                    {
                        childFileSystemObjectInfo.IsExpanded = true;
                        /* Fichero encontrado */
                    }
                    else
                    {
                        childFileSystemObjectInfo.IsExpanded = true;
                        PreSelect(childFileSystemObjectInfo, path);
                    }
                }
            }
        }
        private void ReExplore(string path)
        {
            Trace.WriteLine("ReExplore " + path);
            if (!File.Exists(path))
            {
                Trace.WriteLine("path doesn't exist");
                return;
            }
            var driveFileSystemObjectInfo = GetDriveFileSystemObjectInfo(path);
            if (driveFileSystemObjectInfo.IsVisited)
            {
                ReExplore(driveFileSystemObjectInfo, path);
            }
        }
        private void ReExplore(FileSystemObjectInfo fileSystemObjectInfo,
            string path)
        {
            Trace.WriteLine("ReExplore " + fileSystemObjectInfo.FileSystemInfo.FullName + " " + path);
            foreach (var childFileSystemObjectInfo in fileSystemObjectInfo.Children)
            {
                var isParentPath = IsParentPath(path, childFileSystemObjectInfo.FileSystemInfo.FullName);
                if (isParentPath)
                {
                    if (childFileSystemObjectInfo.IsVisited)
                    {
                        if (IsContainerFolder(childFileSystemObjectInfo.FileSystemInfo, path))
                        {
                            childFileSystemObjectInfo.ReExploreFiles();
                        }
                        else
                        {
                            ReExplore(childFileSystemObjectInfo, path);
                        }
                    }
                }
            }
        }
        private void Explore(string path)
        {
            Trace.WriteLine("ReExplore " + path);
            if (!File.Exists(path))
            {
                Trace.WriteLine("path doesn't exist");
                return;
            }
            var driveFileSystemObjectInfo = GetDriveFileSystemObjectInfo(path);
            if (driveFileSystemObjectInfo.IsVisited)
            {
                Explore(driveFileSystemObjectInfo, path);
            }
        }
        private void Explore(FileSystemObjectInfo fileSystemObjectInfo,
            string path)
        {
            Trace.WriteLine("Explore " + fileSystemObjectInfo.FileSystemInfo.FullName + " " + path);
            foreach (var childFileSystemObjectInfo in fileSystemObjectInfo.Children)
            {
                var isParentPath = IsParentPath(path, childFileSystemObjectInfo.FileSystemInfo.FullName);
                if (isParentPath)
                {
                    if (childFileSystemObjectInfo.IsVisited)
                    {
                        if (IsContainerFolder(childFileSystemObjectInfo.FileSystemInfo, path))
                        {
                            childFileSystemObjectInfo.ExploreFile(path);
                        }
                        else
                        {
                            Explore(childFileSystemObjectInfo, path);
                        }
                    }
                }
            }
        }

        #endregion

        #region Helpers
        // Obtiene el drive de un path
        private FileSystemObjectInfo GetDriveFileSystemObjectInfo(string path)
        {
            var directory = new DirectoryInfo(path);
            var drive = DriveInfo
                .GetDrives()
                .Where(d => d.RootDirectory.FullName == directory.Root.FullName)
                .FirstOrDefault();
            return GetDriveFileSystemObjectInfo(drive);
        }
        // Devuelve el FileSystemObjectInfo de un drive a partir del DriveInfo
        private FileSystemObjectInfo GetDriveFileSystemObjectInfo(DriveInfo drive)
        {
            foreach (var fso in treeView.Items.OfType<FileSystemObjectInfo>())
            {
                if (fso.FileSystemInfo.FullName == drive.RootDirectory.FullName)
                {
                    return fso;
                }
            }
            return null;
        }

        public static bool IsParentPath(string path,
            string targetPath)
        {
            return path.StartsWith(targetPath);
        }

        public static bool IsContainerFolder(FileSystemInfo folderInfo, string path)
        {
            Trace.WriteLine("IsContainerFolder");
            FileSystemInfo pathInfo = new FileInfo(path);
            Trace.WriteLine(folderInfo.FullName + Path.DirectorySeparatorChar + pathInfo.Name);
            Trace.WriteLine(pathInfo.FullName);
            return string.Equals(folderInfo.FullName + Path.DirectorySeparatorChar + pathInfo.Name, pathInfo.FullName);
        }
        /*
        public static bool IsContainerFolder(string folderPath, string filePath)
        { 
            Trace.WriteLine("IsContainerFolder " + folderPath + " " + filePath);
            if(IsParentPath(filePath, folderPath))
            {
                int index = filePath.IndexOf(folderPath);
                string cleanPath = (index < 0)
                    ? filePath
                    : filePath.Remove(index, folderPath.Length);
                string [] parts = cleanPath.Split(Path.DirectorySeparatorChar);
                Trace.WriteLine(parts.Length);
                foreach(string part in parts)
                {
                    Trace.Write(part + " ");
                }
                Trace.WriteLine("");
                return parts.Length == 1;
            }
            return false;
        }
        */

        #endregion

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed)
            {
                return;
            }
            Trace.WriteLine($"Changed: {e.FullPath}");
        }
        private void OnFilesAdded(object sender, List<string> files)
        {
            Trace.WriteLine("OnFilesAdded");
            foreach(var file in files)
            {
                Explore(file);
            }
        }
        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            ReExplore(e.FullPath);
        }

        public void OpenWith(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)sender;
            FileSystemObjectInfo fileSystemObjectInfo = (FileSystemObjectInfo)menuItem.DataContext;
            FileSystemInfo fileSystemInfo = fileSystemObjectInfo.FileSystemInfo;
            string file = fileSystemInfo.FullName;
            Process.Start("rundll32.exe", string.Format("shell32.dll,OpenAs_RunDLL {0}", file));
        }
    }
}

