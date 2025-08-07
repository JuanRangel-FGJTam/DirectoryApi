# Compilacion y Publicacion

### [< Regresar a Tabla de Contenido](./Documentacion.md)
---

## Compilacion
La compilacion del proyecto se puede hacer a travez del CLI de .Net:

```bash
# Restaurar dependencias
dotnet restore

# Compilación en modo Debug (desarrollo)
dotnet build

# Compilación en modo Release (producción)
dotnet build --configuration Release
```

## Publicacion
La publicacion de la applicacion puede ser `Autocontenida` o `Dependiente del Framework`, cuando es auto-contenida no es necesaria tener instalado el sdk de .NET, en cambio cuando es dependiente, se debe instalar el sdk .NET correspondiente.

### Publicación Autocontenida (Recomendada para producción)]
```bash
# Publicar para Windows
dotnet publish -c Release -r win-x64 --self-contained true

# Publicar para Linux
dotnet publish -c Release -r linux-x64 --self-contained true
```

### Publicación Dependiente del Framework
```bash
dotnet publish -c Release
```