# Proyecto Prototipo InnerInsoles
# IBC Bio :registered: 2023

![Bio](Images/bio.png)
  

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

Views
Models
ViewModels

UserControls -> son controles particulares
Styles -> estilos 
States-> 
	RegistroState
	DatabaseBridge
	AnalisisState
Services->
Proxys-> deprecated (revisa Bernat)
Precálculus-> matrices
Migrations -> EF automated
Messages -> datos intermedios:
	       CameraScan, InsoleData, InsoleScan
Libraries-> dll
Images
Forms-> crearpaciente, etc. Flotantes.
Fonts-> Fuentes
Enums
DataTemplateSelectors -> syncfusion
DataHolders: Para cargar los tests al UI
•	FrameData, GraphDAta es el total vista de frame data, framedatinsoles hereda framedata y tiene left y data son DataInsoles. El FramaDataFactory, FrameDataFactoryInsoles (compatible para diferentes ficheros). 
Database: EF
Converters: booleanos a imágenes
Controls_ colormap
Commands
Behaviours: obsoletos

