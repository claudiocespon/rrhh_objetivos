# Documentación para Desarrolladores (DEV)

Esta guía contiene las directrices técnicas para mantener y escalar la **Plataforma de Evaluación de Objetivos**.

## Stack Tecnológico 💻
El proyecto se rige por un stack estricto que **NO debe ser alterado** sin autorización:
- **Framework:** ASP.NET Core Blazor Server (.NET 10)
- **Lenguaje:** C# 13
- **UI Components:** Radzen Blazor
- **ORM:** Entity Framework Core 10
- **Base de Datos:** SQLite (archivo local `objetivos.db`)

## Arquitectura 🏗️
El proyecto utiliza una arquitectura **Monolítica** simple:
- **NO** se utiliza Web API separada.
- **NO** se utilizan repositorios genéricos, MediatR, ni CQRS.
- La lógica de negocio reside en la capa `Services/` (ej. `ObjetivoService`, `RendimientoService`).
- Los servicios son inyectados directamente en los componentes Razor de `Pages/`.
- La autenticación se maneja vía `ProtectedSessionStorage` implementado a través de `ICurrentUserService`.

## Estructura del Proyecto
El código fuente se encuentra en el proyecto `Objetivos.Web/`:
- `/Data`: Configuración de DbContext y Seed de datos iniciales.
- `/Domain`: Entidades (clases C# puras) y Enums.
- `/Services`: Toda la lógica de reglas de negocio.
- `/Components/Pages`: Pantallas y flujos funcionales del sistema.

> [!WARNING]
> No introducir librerías de front-end (React, Angular) ni cambiar la base de datos sin autorización arquitectónica.
