# Configuracion

### [< Regresar a Tabla de Contenido](./Documentacion.md)
---

## Configuración de la APP

### Archivo `Program.cs`

El archivo `Program.cs` es el punto inicial de la aplicacion, en el se puede configurar los aspectos del la aplicacion como:
- Inyección de dependencias
- Middleware pipeline
- Configuración de servicios
-Rutas y endpoints

### Archivo `appSettings.json`
Es el archivo de configuracion y este es utilizada por `Program.cs` para cargar las configuraciones a continuacion se describe sus partes:

```json
{
  "ConnectionStrings": {
    "AuthApi": "cadena-de-conexion"
  },

  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },

  "Secret": "",

  "JwtSettings":{
    "Issuer": "",
    "Audience": "",
    "Key": "",
    "LifeTimeDays": 365
  },

  "ResetPasswordSettings":{
    "TokenLifeTimeSeconds": 3600,
    "DestinationUrl": ""
  },

  "RegisterPersonSettings":{
    "TokenLifeTimeSeconds": 57600
  },

  "EmailSettings":{
    "ApiUri": "",
    "From": "",
    "Token": ""
  },

  "WelcomeEmailSources":
  {
    "ImageNameSrc": "url-imagen",
    "ImageProfileSrc": "url-imagen"
  },

  "MinioSettings": {
    "Endpoint": "",
    "AccessKey": "",
    "SecretKey": "",
    "BucketName": "directory-api",
    "BucketNameTmp": "fiscalia-digital-temporal",
    "ExpiryDuration": "seconds"
  },

  "Serilog": {
    "Using": [ "Serilog.Sinks.Console", "Serilog.Sinks.File"],
    "MinimumLevel": "Information",
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
    "Enrich": [ "FromLogContext", "WithMachineName"],
    "Properties": {
      "Application": "AuthApi"
    }
  },
  
  "AllowedHosts": "*",
  "UbicanosSettings": {
    "Host": ""
  }
}
```

### 1. ConnectionStrings
Define las cadenas de conexion utilizadas por por la applicacion.

### 2. Loggin
Define que es lo que se pude logear y cual es el nivel minimo para loger, en el ejemplo:

```json
"Logging": {
  "LogLevel": {
    "Default": "Information",
    "Microsoft.AspNetCore": "Warning"
  }
},
```

se define que por defecto se logee todo con nivel Igual o mayor a `Information`, es decir que si logeas algo con nivel `Debug` que es minimo a `Information` este no se loogeara,
en cambio los packetes `Microsoft.AspNetCore` que logeen algo, solo se mostrara si es un `Warning` o un `Error`.
> Mas informacion [Loggin in C# and .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging?tabs=command-line)

### 3. Secret
El la llave utilizada para encriptar y des-encriptar los registros de la base de datos, por ejemplo, el nombre y la informacion de contacto de las personas.

### 4. JwtSettings
Es la configuracion utilizada para la generacion y validation de los Tokens

### 5. ResetPasswordSettings
En este apartado se especifica el tiempo de vida del token generado una vez se reciba una peticion para recuperar la contrasena.

### 6. RegisterPersonSettings
En este apartado se especifica el tiempo de vida del token generado para el registro de una nueva persona.

### 7. EmailSettings
Se define la configuracion para comunicarse con el api de Correo de la fiscalia.

### 8. MinioSettings
Se define la configuracion para comunicarse con el servicio de Minio para la almacenacion y recuperacion de documentos.

### 9. Serilog
Se define la configuracion para el registro de logs
