# Authenticacion

### [< Regresar a Tabla de Contenido](./Documentacion.md)
---

>**NOTA:** Este apartada hace referencia a la authenticacion para el usuo del API, para la comunicacion entre servicios, no confundir con la authenticacion de Usuarios/Personas.


El proyecto implementa autenticación basada en JSON Web Tokens (JWT) utilizando el paquete oficial de Microsoft:

```xml
<!-- AuthApi.csproj -->
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.2" />
```
## Configuracion en `Program.cs`:

```cs
builder.Services.AddJwtAuthentication(
    builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()!
);
```

## Configuracion `appsettings.json`:

```json
"JwtSettings": {
    "Issuer": "https://directoryAPI.fgjtam.gob.mx",
    "Audience": "*.fgjtam.gob.mx",
    "Key": "TuLlaveSecretaSuperSeguraDeAlMenos256Bits",
    "LifeTimeDays": 365
}
```

## Implementación en Controladores
Para proteger los endpoints del controlador se debe utilizar el atributo [Authorize]:

```cs
[ApiController]
[Route("api/[controller]")]
[Authorize] // Protege todo el controlador
public class UsuariosController : ControllerBase
{
    [HttpGet]
    public IActionResult GetUsuarios() { ... }

    [AllowAnonymous] // Excepción para endpoint público
    [HttpPost("login")]
    public IActionResult Login(LoginRequest request) { ... }
}
```

## Generación de Tokens:
Para generar los token, se debe hacer una peticion `POST` al endpoint `{{host}}/user/authenticate` con las credenciales como se muestra:

```json
{
  "email": "user@fgjtam.gob.mx",
  "password": "mypassword"
}
```
Esto generara una respuesta con el token incluido:

```json
{
	"id": 1,
	"firstName": "Juan Salvador",
	"lastName": "Rangel Almaguer",
	"username": "user.rangel@fgjtam.gob.mx",
	"token": "eyJhbGciOiJIUzI1NiI.......Lbgk34"
}
```

## Uso en Clientes
Los clientes deben incluir el token en el header Authorization para poder utilizar esta API:

```http
GET /api/usuarios HTTP/1.1
Authorization: Bearer eyJhbGciOiJIUzI1NiI.......Lbgk34
Host: directoryAPI.fgjtam.gob.mx
```