# Documentación de Infraestructura y Operaciones (IT)

Esta guía detalla los requisitos de despliegue, configuración y mantenimiento operativo de la **Plataforma de Evaluación de Objetivos**.

## Requisitos del Servidor 🖥️
- **Runtime:** .NET 10 (ASP.NET Core Runtime 10.0+)
- **Sistema Operativo:** Windows Server (IIS) o Linux (Kestrel/Nginx/Apache).
- **RAM Mínima:** 2GB (recomendado para Blazor Server con múltiples sesiones simultáneas).
- **Almacenamiento:** Mínimo 5GB (para base de datos SQLite y logs).

## Base de Datos e Importación de Datos 🗄️
- El sistema utiliza **SQLite** y la base de datos se guarda localmente en un archivo (ej. `Data/objetivos.db`).
- **Backup:** Solo se necesita realizar una copia de seguridad periódica del archivo `.db`.
- **Carga de Datos Iniciales:** La aplicación permite importar los usuarios iniciales (RRHH, Empleados, Jefes) a través de los archivos ubicados en la carpeta `Data/` (ej. `usuarios_responsables.csv` y planillas `.xlsx`).

## Despliegue (Deployment) 🚀
Al ser una aplicación Blazor Server, mantiene una conexión persistente (SignalR/WebSockets) con cada cliente conectado.
1. Compilar el proyecto en modo release: `dotnet publish -c Release -o ./publish`
2. Asegurar permisos de escritura en la carpeta de publicación para que la app pueda actualizar la base de datos `objetivos.db`.
3. Configurar un Proxy Inverso (opcional pero recomendado) con soporte explícito para **WebSockets**.

## Gestión de Sesiones y Seguridad 🔒
- La sesión se almacena mediante **ProtectedSessionStorage** (encriptación de datos a nivel de cookie y almacenamiento persistente seguro en el servidor/cliente).
- No requiere configuración extra de Active Directory a menos que se desarrolle una integración específica.
