# Captura de Datos de la Marcha: BiotecGAIT
# BioTec :registered: 2023

![Bio](/insoles/Images/biotecgait.png)
  

## Configuraciones

### Inicio de la base de datos (problema si no se añaden o recuperan Pacientes)
- Se recomienda Clean+Build
- Borrar las carpetas bin/ y obj/
- 
**Dentro de la carpeta insoles/**

- Añadir el paquete dotnet-ef en la .NET Cli:

```cmd
dotnet tool install --global dotnet-ef
```
- Ejecutar la creación inicial de la bd y actualizar (.Net CLI)
```cmd
dotnet ef migrations add InitialCreate
dotnet ef database update
```

# Estructura de la aplicación

## ROOT
1. App.xaml
    - Llama al MainWindow.xaml
2. App.xaml.cs 
    - Crear carpeta de grabación de datos
    - Key de Syncfusion
3. MainWindow.xaml
    - DataContext: NavigationVM
    - Son los comandos de navegar por las vistas
    - currentView tiene el valor de la vista cuando haces click en los 
    botones de Home, Análisis, Registro e Informes.
4. MainWindow.xaml.cs
    - States\DataBaseBridge inicializado
5. mydatabase.db -> base de datos
                
## Carpetas

1. Views
    - Home.xaml y Home.xaml.cs tienen sólo para cargar el logo de la app.
    - 
- Models
- ViewModels

- UserControls -> son controles particulares
- Styles -> estilos 
- States-> 
    * RegistroState -> son una serie de estados. Guardar estados de la aplicación?
    * DatabaseBridge->creación del CRUD de base de datos.
    * AnalisisState -> también implementa INotify... 
- Migrations: DDL creado por el EF para sqlite.
- Services->
- Proxys-> deprecated (revisa Bernat)
    * Precálculus-> matrices
    * Migrations -> EF automated
    * Messages -> datos intermedios:
	       CameraScan, InsoleData, InsoleScan
- Libraries-> dll
- Images
- Forms-> crearpaciente, etc. Flotantes.
- Fonts-> Fuentes
- Enums
- DataTemplateSelectors -> syncfusion
- DataHolders: Para cargar los tests al UI
    * FrameData, GraphDAta es el total vista de frame data, framedatinsoles hereda framedata y tiene left y data son DataInsoles. El FramaDataFactory, FrameDataFactoryInsoles (compatible para diferentes ficheros). 
- Database: EF
- Converters: booleanos a imágenes
- Controls_ colormap
- Commands
- Behaviours: obsoletos

# Nuevo
Documentación en la carpeta docs/  
Guarda los tests y videos en esta carpeta: %HOMEDRIVE%%HOMEPATH%\insoles , si no existe la crea al principio del programa
## Ejecutar el programa
Abrir con VisualStudio22. Publicar proyecto en la ubicacion deseada. Ejecutar insoles.exe
