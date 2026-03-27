# Reglas de Desarrollo y Arquitectura - RRHH Objetivos

Este documento establece las buenas prácticas y reglas obligatorias para el desarrollo del proyecto `Objetivos.Web`.

## 1. Entity Framework Core y Base de Datos

### 1.1. Gestión de Esquema en Producción
**ESTRICTAMENTE PROHIBIDO** usar métodos destructivos de base de datos (`EnsureDeletedAsync()`, `EnsureCreatedAsync()`) en entornos que no sean puramente de desarrollo local, y aún así, deben usarse con extrema precaución y estar condicionados por el entorno.

**Mala práctica (NO HACER):**
```csharp
// Esto borra la base de datos en cada reinicio
await db.Database.EnsureDeletedAsync();
await db.Database.EnsureCreatedAsync();
```

**Buena práctica:**
* Usar **Migraciones** (`dotnet ef migrations add` y `dotnet ef database update`).
* En el inicio de la aplicación, aplicar migraciones pendientes automáticamente:
```csharp
if (app.Environment.IsProduction())
{
    await db.Database.MigrateAsync();
}
```

### 1.2. Poblar Datos (Seeding)
El volcado inicial de datos (Seeding) debe ser **idempotente**. Es decir, debe poder ejecutarse múltiples veces sin duplicar datos ni fallar, verificando siempre si los datos ya existen antes de insertarlos.

## 2. Inyección de Dependencias y Servicios

### 2.1. Ciclo de Vida de los Servicios
* **Entity Framework Context (`AppDbContext`)**: Debe registrarse siempre como `Scoped` (por defecto en ASP.NET Core). Nunca inyectar un DbContext en un Singleton.
* **Componentes Blazor Server**: Blazor Server mantiene una conexión persistente (SignalR). Los servicios inyectados como `Scoped` viven **durante toda la conexión del usuario** (como un Singleton por pestaña de navegador). Evitar almacenar estado global compartido accidentalmente en servicios `Scoped` a menos que sea estado específico de ese usuario.

## 3. Estado de la Aplicación y Autenticación

### 3.1. Navegación en Blazor
* Evitar el uso de `forceLoad: true` en `NavigationManager.NavigateTo()` a menos que sea estrictamente necesario (ej. recargar la app entera, salir de Blazor a MVC/Razor Pages, o limpiar sesión estricta). `forceLoad` destruye el estado de la aplicación almacenado en memoria (como servicios `Scoped`).

## 4. Manipulación de Archivos

### 4.1. Acceso Concurrente a Archivos
Cuando se lean archivos físicos (como CSV, JSON, etc.), usar siempre `FileStream` especificando `FileShare.Read` o `FileShare.ReadWrite` para evitar excepciones del tipo `IOException: The process cannot access the file` si el usuario tiene el archivo abierto en otro programa (ej. Excel).

**Buena práctica:**
```csharp
using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
using var sr = new StreamReader(fs);
```

## 5. Arquitectura de Componentes Razor

### 5.1. Responsabilidad Única
* Mantener los archivos `.razor` enfocados en la UI.
* Mover la lógica compleja de negocio a clases de servicio (ej. `ObjetivoService`, `AuthService`).
* Inyectar esos servicios en el componente a través de la directiva `@inject`.
