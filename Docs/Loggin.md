# Loggin

### [< Regresar a Tabla de Contenido](./Documentacion.md)
---
La aplicacion utiliza el packete `Serilog` para administrar los registros, `Serilog` es un packate que permite gestionar los destinos de los registros, es desir, puedes definir que los registro se almacenen en un archivo fisico, en una base de datos o en ambos, esto a travez de su configuracion.

Para definir el uso de `Serilog` este de configura en el archivo `Program.cs`:

```cs
var serilogConfiguration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(serilogConfiguration).CreateLogger();
builder.Host.UseSerilog();
```

con esto se carga serilog y se utiliza la configuracion establecida en el archivo `appsettings.json` en el segmento `Serilog` como se muestra:

```json
"Serilog": {
    "Using": [ "Serilog.Sinks.Console", "Serilog.Sinks.File"],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft.EntityFrameworkCore.Database.Command": "Warning",
        "Microsoft.EntityFrameworkCore.Infrastructure": "Warning"
      }
    },
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "File",
        "Args": {
          "path": "Logs/log.txt",
          "rollingInterval": "Day",
          "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
          }
      },
    ],
    "Enrich": ["FromLogContext", "WithMachineName"]
  },
```

Serilog utiliza lo que llaman `Sinks` para definir a donde se almacenaran los registro y en que formato, en el codigo anterior se define que se utilizara los sinks `Serilog.Sinks.Console` y `Serilog.Sinks.File`, los cuales son para escribir los logs en la consola y en un archivo fisico.

> Mas informacion:
>   - [Serilog Configuracion Basica](https://github.com/serilog/serilog/wiki/Configuration-Basics)
>   - [Serilog Sinks Disponibles](https://github.com/serilog/serilog/wiki/Provided-Sinks)


## Uso de ILogger
Una vez configurado el sistema de Logging, se puede utilizar la interfaz de .net `ILogger` para escribir logs, este se debe injectar en los Controladores y/o Servicios que se necesite como se muestra:

```cs
public class PeopleController() : ControllerBase
{
    private readonly ILogger<PeopleController> _logger = logger;

    public PeopleController(ILogger<PeopleController> logger)
    {
        var _logger = logger;
    }

    public IActionResult GetAllPeople()
    {
        this.logger.LogInformation("Obteniendo todas las personas.");
        .
        .
        .
    }
}
```