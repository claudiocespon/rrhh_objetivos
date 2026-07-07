# 🎯 Plataforma de Evaluación de Objetivos RRHH

Bienvenido al repositorio oficial del sistema de Evaluación de Objetivos. Esta plataforma web permite a la compañía digitalizar y centralizar el proceso completo de definición, seguimiento y evaluación del desempeño de todos sus colaboradores.

---

## 🚀 Sobre el Proyecto
Desarrollada bajo la arquitectura de **Blazor Server** (.NET 10), la plataforma ofrece una experiencia moderna, rápida e interactiva (SPA) que facilita la comunicación fluida entre Jefes y Empleados a lo largo de todo el ciclo anual de objetivos, sin la complejidad de recargas de página.

### Características Principales
- 📊 **Dashboards Dinámicos:** Seguimiento en tiempo real del estado de los objetivos del equipo.
- 💬 **Feedback Continuo (Bitácora):** Herramienta integrada para dejar comentarios y evidencias de avances a lo largo del cuatrimestre.
- 📝 **Ciclo Completo:** Desde la creación y aprobación de objetivos, hasta la autoevaluación final y la calificación oficial del mánager.
- 🎓 **Capacitación:** Módulo para la asignación de cursos basados en las áreas de mejora detectadas en el desempeño.

---

## 📚 Documentación Oficial

Dependiendo de tu rol dentro del proyecto, te invitamos a consultar la documentación específica:

### 👨‍💻 Para Desarrolladores (DEV)
¿Vas a contribuir al código fuente? Revisa el stack tecnológico, las reglas de arquitectura y la estructura del monolito en C#.
👉 **[Ver Documentación para Desarrolladores](docs/DEV.md)**

### ⚙️ Para Infraestructura y Operaciones (IT)
Instrucciones sobre cómo hospedar la aplicación, requerimientos mínimos de memoria, configuración de la base de datos (SQLite) y despliegue del servidor web.
👉 **[Ver Documentación para IT](docs/IT.md)**

### 🏢 Para Usuarios y Consultores (FUNCIONAL)
Entiende el modelo de negocio, qué puede hacer cada rol dentro del sistema (Jefe, Colaborador, RRHH) y cómo es el ciclo de vida de un objetivo.
👉 **[Ver Documentación Funcional](docs/FUNCIONAL.md)**

---

## 🛠️ Instalación Local Rápida
1. Clona el repositorio: `git clone https://github.com/claudiocespon/rrhh_objetivos.git`
2. Abre la solución en Visual Studio 2022 o ejecuta `dotnet run` dentro de la carpeta `Objetivos.Web/`.
3. La aplicación creará automáticamente la base de datos de pruebas `objetivos.db` localmente al arrancar.

> [!NOTE]
> Para cualquier consulta adicional, revisa también los manuales y planes ubicados en la carpeta `/docs`.
