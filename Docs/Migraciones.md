# Migraciones

### [< Regresar a Tabla de Contenido](./Documentacion.md)
---

Cuando se desee modificar la estructura de la base de datos ya sea para agregar nuevos campos a la tablas existentes o crear nuevas tablas, estos cambios deben hacerse a travez de las entidades.
- Si se desea agregar una nueva tabla se crea una nueva identidad en la tabla `Entities` y se agrega al contexto `DirectoryDbContext.cs`.
- Si se desea alterar la estructura actual ya sea agregar nuevas columnas o remover, se modifica la entidad y su definicion en el contexto `DirectoryDbContext.cs`.

Una vez hechos los cambios requeridos, se ejecutara un comando para generara una nueva **Migracion** la cual contendra los cambios para aplicarse a la base de datos.
> **Nota:** Los archivos de migracion se generan despues de alterar la estructura de las entidades.

## Comandos
Para generar el archivo de migracion se ejecuta el commando:

    ```shell
    dotnet ef migrations add <nombre-de-la-migracion>
    ```

Para aplicar la migracion (los cambios) a la base de datos se debera ejecutar el commando:

    ```shell
    dotnet ef database update
    ```

Para listar las migraciones disponibles y pendientes por aplicar se ejecuta el comando:

    ```shell
    dotnet ef migrations list
    ```

Para deshacer los cambios y regresar a un punto anterior se ejecuta el comando:

    ```shell
    dotnet ef database update <nombre-de-la-migracion-previa>
    ```
    *Donde <nombre-de-la-migracion-previa> es el nombre de la migracion a la cual se quiere regresar.*

Para remover el ultimo archivo de migracion (Esto no afecta a la base de datos, solo al archivo de migracion):

    ```shell
    dotnet ef migrations remove
    ```

> **Para mas informacion consultar:**
>   - [Migraciones](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/?tabs=dotnet-core-cli)
>   - [Manejo de Migraciones](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/managing?source=recommendations&tabs=dotnet-core-cli)
>   - [Entity Framework Code First](https://learn.microsoft.com/en-us/ef/ef6/modeling/code-first/migrations/existing-database)
>