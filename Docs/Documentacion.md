#Documentación para el Desarrollo de una API en .NET

## Introduccion
Esta documentación está diseñada para desarrolladores que trabajarán en el mantenimiento o expansión de nuestra API .NET. Proporciona una visión general de la arquitectura, convenciones y flujos de trabajo.

## Contenido
    - Requisitos
    - Estructura del Projecto
    - Convenciones de Código
    - Configuración de la App
    - [Uso de Servicios e Injeccion de Dependencias](./Servicios.md)
    - [Migraciones](./Migraciones.md)
    - [Authentication & Authorization](./Authenticacion.md)
    - [Logging](./Loggin.md)
    - [Compilacion y Publicacion](./Publicacion.md)

## Requisitos
    - **SDK .NET:** Version 8.0 o superior [^1]
    - **IDE Recomendado:** Visual Studio Code
    - **Extensiones utiles de VS Code:**
        - C# Dev Kit
        - Region Viewer
        - TODO Highlight
        - Better Comments

## Estructura del Projecto
```
  / Controllers
  / Data                    # Interfaces, Clases, DBContext
    / Exceptions            # Excepciones personalziadas
  / Entities                # Entidades de la bd
  / Helpers
  / Jobs
  / Migrations              # Carperta de migraciones (auto generadas)
  / Models                  # Modelos utilizados dentro de la app, no confundir con Entidades
  / Services                # Servicios utilizados e injectados dentro de la app
  / Validator               # Archivos de validacion
  AuthApi.csproj            # Archivo de definicio y configuracion del proyecto
  Program.cs                # Punto inicial del la app e inicializacion de la misma
  appsettings.json          # Archivo de configuracion

```

## Convenciones de Código

### Controladores
- Utilizar los atributos `[Authorize]` para proteger el todo el controlador o solo una funcions del controlador.
- Nomenclatura: `[NombreEntidad]Controller` Ej: `PeopleController`

### Modelos y DTOs
- Utilizar los modelos cuando sea posible en vez de utilizar directamente las Entidades.
- Separar modelos de entrada (request) y salida (response).

[^1]: Puedes consultar y/o modificar la versión del SDK de .NET en el archivo AuthApi.csproj, cambiando el valor dentro de la etiqueta <TargetFramework>.
    ```xml
    <TargetFramework>net8.0</TargetFramework>
    ```