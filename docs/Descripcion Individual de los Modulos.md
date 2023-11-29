# Commands
### Descripción general y propósito
Todos los comandos de la aplicación
### Dependencias
- insoles.Forms
- insoles.Model
- insoles.States
- insoles.Services
- insoles.Utilities
- insoles.UserControls
- Syncfusion.DocIO
- Syncfusion.DocIO.DLS
- Newtonsoft.Json
- Microsoft.Win32
### Implementacion
- AcceptarCrearPacienteCommand.cs
- AcceptarEditarPacienteCommand.cs
- BorrarPacienteCommand.cs
- BorrarTestCommand.cs
- CalibrarCommand.cs
- CalibrarLeftStartCommand.cs
- CalibrarResetCommand.cs
- CalibrarRightStartCommand.cs
- CaptureCommand.cs
- CargarInformeCommand.cs
- CargarTestCommand.cs
- CargarCameraCommand.cs
- ConnectCommand.cs
- CrearCarpetaInformeCommand.cs
- CrearCarpetaTestCommand.cs
- CrearPacienteCommand.cs
- DisconnectCommand.cs
- EditarPacienteCommand.cs
- ExportarFCTestCommand.cs
- GenerarInformeCommand.cs
- ImportarTestCommand.cs
- NormalizarCommand.cs
- ObtenerPacientesCommand.cs
- OpenCameraCommand.cs
- PauseCommand.cs
- RecordCommand.cs
- RenombrarCarpetaInformeCommand.cs
- RenombrarCarpetaTestCommand.cs
- RenombrarFicheroCSVTestCommand.cs
- RenombrarFicheroVideo1TestCommand.cs
- RenombrarFicheroVideo2TestCommand.cs
- ResetLayoutCommand.cs
- ScanCommand.cs
- StopCommand.cs
- TimelineFastBackwardCommand.cs
- TimelineFastFordwardCommand.cs
- TimelinePauseCommand.cs
- TimelinePlayCommand.cs
# Controlls
### Descripción general y propósito
Colormap de scotplot para poder escoger cualquier color
### Dependencias
- ScottPlot.Drawing
### Implementacion
- CustomColormap.cs
# Converters
### Descripción general y propósito
Converters de MVVM para canviar los valores a mostrar
### Dependencias
No tiene
### Implementacion
- BooleanToCircleConverter.cs
- FalseToVisibleConverter.cs
- FractionConverter.cs
- IntToPercentageConverter.cs
- NullToVisibilityConverter.cs
- PathToFileNameConverter.cs
- PreTextConverter.cs
- TimeSpanConverter.cs
- TrueToVisibleConverter.cs
# Database
### Descripción general y propósito
DBContext para acceder a la base de datos
### Dependencias
- insoles.Model
- Microsoft.EntityFrameworkCore
### Implementacion
- DBContextSqlLite.cs
# DataHolders
### Descripción general y propósito
Clases para almacenar datos de la aplicacion. Almacenan los datos al importar un fichero.
### Dependencias
- insoles.Enums
### Implementacion
- DataInsole.cs
- FrameData.cs
- FrameDataFactory.cs
- FrameDataFactoryInsoles.cs
- FrameDataInsoles.cs
- FramePressures.cs
- GraphData.cs
- VariablesData.cs
# DataTemplateSelectors
### Descripción general y propósito
DataTemplateSelectors para MVVM. Solo hay uno para el user control Pacientes
### Dependencias
- insoles.Model
- insoles.Models
- Syncfusion.UI.Xaml.TreeView.Engine
### Implementacion
- PacienteDataTemplateSelector.cs
# Enums
### Descripción general y propósito
Enums de la aplicación
### Dependencias
No tiene
### Implementacion
- Metric.cs
- Sensor.cs
- Side.cs
- Units.cs
# Forms
### Descripción general y propósito
Formularios de la aplicación
### Dependencias
- insoles.Commands
- insoles.Model
- insoles.States
### Implementacion
- CalibrarForm.xaml
- CalibrarForm.xaml.cs
- CrearPacienteForm.xaml
- CrearPacienteForm.xaml.cs
- EditarPacienteForm.xaml
- EditarPacienteForm.xaml.cs
- TextInputForm.xaml
- TextInputForm.xaml.cs
# Heatmap
### Descripción general y propósito
Heatmap. Vista y funciones de pintado.
### Dependencias
- insoles.DataHolders
- insoles.Enums
- insoles.Services
- insoles.States
- MathNet.Numerics.LinearAlgebra
- NumSharp
### Implementacion
- DataType.cs
- HeatMap.xaml
- HeatMap.xaml.cs
- HeatMapImage.cs
- MockDatasGen.cs
# Message
### Descripción general y propósito
Clases para guardar datos y enviarlos de un lado a otro de la applicación
### Dependencias
- insoles.Enums
- insoles.Utilities
### Implementacion
- CameraScan.cs
- GraphRange.cs
- InsoleData.cs
- InsoleScan.cs
# Models
### Descripción general y propósito
Modelos de la aplicacion (MVVM)
### Dependencias
- insoles.Commands
- insoles.Enums
- insoles.Utilities
- insoles.States
- insoles.ViewModel
- ScottPlot
- ScottPlot.Plottable
### Implementacion
- CameraModel.cs
- GraphSumPressuresLiveModel.cs
- Infome.cs
- InformeFile.cs
- InformeFileTreeView.cs
- InformeTreeView.cs
- InsoleModel.cs
- MetaFolderTreeView.cs
- Paciente.cs
- PacientesTreeView.cs
- PacientesTreeViewBase.cs
- PacienteTreeView.cs
- Test.cs
- TestFileTreeView.cs
- TestsTreeView.cs
- TestTreeView.cs
- TextTestFileTreeView.cs
- VideoTestFileTreeView.cs
# Precalculus
### Descripción general y propósito
Ficheros para precalcular el heatmap antiguo.
### Dependencias
No tiene
### Implementacion
No tiene
# Proxys
### Descripción general y propósito
Ya no se usa
### Dependencias
No tiene
### Implementacion
- ListViewProxy.cs
# Services
### Descripción general y propósito
Servicios de la app. Tienen su interfaz y al menos una implementacion. Algunos no tienen interfaz.
### Dependencias
- insoles.DataHolders
- insoles.Enums
- insoles.Utilities
- insoles.Messages
- insoles.States
- insoles.Database
- WisewalkSDK
- DirectShowLib
- AForge.Video.DirectShow
- Emgu.CV
- Newtonsoft.Json
- MathNet.Numerics
- Microsoft.EntityFrameworkCore
### Implementacion
- ApiService.cs
- ButterflyService.cs
- CamaraService.cs
- CamaraStreamService.cs
- CodesService.cs
- FakeApiService.cs
- FileExtractorService.cs
- IApiService.cs
- IButterflyService.cs
- ICameraService.cs
- ICodesService.cs
- IDatabaseService.cs
- IFileExtractorService.cs
- InformesGeneratorService.cs
- IPlantillaService.cs
- IPressureMapService.cs
- IRetocarPlantillaService.cs
- ISaveService.cs
- LiveCalculationsService.cs
- PlantillaService.cs
- PressureMapCentersSevice.cs
- PressureMapService.cs
- RetocarPlantillaReducedService.cs
- SaveService.cs
- SQLiteDatabaseService.cs
# States
### Descripción general y propósito
Guardan el estado de las diferentes partes de la aplicación
### Dependencias
- insoles.Model
- insoles.Services
- Microsoft.Data.Sqlite
### Implementacion
- AnalisisState.cs
- DatabaseBridge.cs
- InformesState.cs
- RegistroState.cs
# Styles
### Descripción general y propósito
Estilos de algunos componentes
### Dependencias
No tiene
### Implementacion
- Button.xaml
- ButtonIcon.xaml
- Icon.xaml
- Image.xaml
- InsoleColumns.xamls
- Page.xaml
- Text.xaml
# UserControls
### Descripción general y propósito
UserControls de la app. Son las vistas incrustadas.
### Dependencias
- insoles.Model
- insoles.DataHolders
- insoles.Services
- insoles.Messages
- insoles.Controlls
- insoles.Enums
- insoles.States
- insoles.Utilities
- ScottPlot
- MathNet.Numerics
- stdgraph.Lib
### Implementacion
- CamaraReplay.xaml
- CamaraReplay.xaml.cs
- Dispositivos.xaml
- Dispositivos.xaml.cs
- EditorInformes.xaml
- EditorInformes.xaml.cs
- GrafoMariposa.xaml
- GrafoMariposa.xaml.cs
- GRF.xaml
- GRF.xaml.cs
- Heatmap.xaml
- Heatmap.xaml.cs
- PacientesUserControl.xaml
- PacientesUserControl.xaml.cs
- PacientesUserControlAnalisis.xaml
- PacientesUserControlAnalisis.xaml.cs
- TimeLine.xaml
- TimeLine.xaml.cs
# Utilities
### Descripción general y propósito
Funciones auxiliares que usan multiples partes de la applicacion. DataTemplate hace el enlace entre views y viewmodels
### Dependencias
- insoles.Enums
- Emgu.CV
- MathNet.Numerics.LinearAlgebra
### Implementacion
- Btn.cs
- DataTemplate.xaml
- DeepCopy.cs
- FormatConversions.cs
- HelperFunctions.cs
- MathNetHelpers.cs
- ModelBase.cs
- RelayCommand.cs
- UnitsConversion.cs
- ViewModelBase.cs
## Utilities.Normalization
### Descripción general y propósito
Funciones auxiliares para la normalizacion del GRF
### Dependencias
- CsvHelper
- ScottPlot
- MathNet.Numerics.Statistics
### Implementacion
- Cargas.cs
- CubicInterpol.cs
- NormalStats.cs
# ViewModels
### Descripción general y propósito
ViewModels de la applicación (MVVM)
### Dependencias
- insoles.Commands
- insoles.DataHolders
- insoles.Messages
- insoles.Model
- insoles.Services
- insoles.States
- insoles.UserControls
- insoles.Utilities
- insoles.Commands
- Emgu.CV
- ScottPlot
- MathNet.Numerics
### Implementacion
- AnalisisVM.cs
- HomeVM.cs
- InformesVM.cs
- NavigationVM.cs
- RegistroVM.cs
# Views
### Descripción general y propósito
Views de la applicación (MVVM)
### Dependencias
No tiene
### Implementacion
- Analisis.xaml
- Analisis.xaml.cs
- Home.xaml
- Home.xaml.cs
- Informes.xaml
- Informes.xaml.cs
- Registro.xaml
- Registro.xaml.cs
# WPFHeatmap
### Descripción general y propósito
Otro heatmap. No se usa
### Dependencias
- insoles.DataHolders
- insoles.Enums
- insoles.Services
- insoles.States
- MathNet.Numerics.LinearAlgebra
### Implementacion
- AttachedTitle.cs
- HeatMap.cs
- HeatPoint.cs
- WPFHeatmap.xaml
- WPFHeatmap.xaml.cs
- Effects/HeatColorizer.cs
- Effects/HeatColorizer.fx
- Effects/HeatColorizer.ps
# MainWindow
### Descripción general y propósito
Main window. Contenedor de las views (analisis, registro y informes)
### Dependencias
- insoles.Services
- insoles.States
### Implementacion
- MainWindow.xaml
- MainWindow.xaml.cs
