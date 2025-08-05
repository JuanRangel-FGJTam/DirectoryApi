# Uso de Servicios e Injeccion de Dependencias

### [< Regresar a Tabla de Contenido](./Documentacion.md)
---

Para una mejor organizacion y reutilizacion del codigo se procura crear `Servicios` los cuales agrupan la logica de la aplicacion y actuan como intermediario entre el Controlador (Peticion) y los datos (Base de Datos, Servicio Externo).

Cuando se desee agregar una nueva funcionalidad, esta se debera encapsular en su clase Servicio correspondiente o generar una nueva, los servicio deben seguir estas practicas:

1. **Responsabilidad:**
    Cada servicio debe tener una única responsabilidad clara (Principio SRP). Ejemplo: `UsuarioService` maneja solo lógica relacionada con usuarios.

2. Los servicios no deben acceder directamente a HttpContext.
3. Deben ser independientes del framework de API.
4. Usar DTOs para comunicación con los controladores (No utilizar entidades).


## Registro de Servicio
Para poder utilizar el servicio desde cualquier parte de la aplicacion se debe registrar previamente, estas se registran en el archivo `Program.cs` como se muestra:

```cs
// Configuración recomendada
builder.Services.AddScoped<UsuarioService>();
```

### Tiempo de vida del servicio
Se puede definir el tiempo de vida del servicio mediante 3 opciones:

| Método | Descripción|
| ----------- | ----------- |
| AddTransient | Crea una nueva instancia cada vez que se solicita el servicio |
| AddScoped | Crea una instancia por petición HTTP (la más común en aplicaciones web) |
| AddSingleton | Crea una sola instancia que dura toda la vida de la aplicación |

```cs
// Ejemplos de registro de servicios con diferentes tiempos de vida
builder.Services.AddTransient<UsuarioService>();   // Nueva instancia por cada uso
builder.Services.AddScoped<UsuarioService>();     // Una instancia por petición HTTP
builder.Services.AddSingleton<UsuarioService>();  // Única instancia compartida
```

## Uso de Servicios
Una vez registrado el servicio este puede ser accesado desde cualquer parte de la applicacion mediante la injeccion de dependencias,
para injectar la dependencia en un controlador u otro servicio, se debe definir el servicio deseado en el constructor de la clase ejemplo:

```cs
public class PeopleController : ControllerBase
{
    private readonly PersonService personService;

    public PeopleController(PersonService personService)
    {
        this.personService = personService;
    }
}

// En versiones nuevas de c# se puede definir el constructor directamente en la definicion de la clase.
public class PeopleController(PersonService personService) : ControllerBase
{
    private readonly PersonService personService = personService;
}
```

Una vez injectado se podra acceder a sus funciones
```cs
public ActionResult<IEnumerable<PersonResponse>?> GetAllPeople()
{
    var people = this.personService.GetPeople().ToList();

    return Ok(people);
}
```

## Flujo:

```
Controlador → Servicio (lógica de negocio) → Repositorio (Entity Framework o SqlClient) → BD
```

>**NOTA:** Para mas informacion consutlar [Injeccion de Dependencias](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-9.0)