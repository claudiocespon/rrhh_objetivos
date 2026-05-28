# Especificaciones de Cambios – PQ Talent (Plataforma de Objetivos RRHH)

> **Fuente:** Emails internos de Pablo Cirac (Analista de RRHH, Permaquim S.A.) del 24/04/2026.  
> **Destinatario original:** Claudio Daniel Cespón (IA & Análisis de Datos, Permaquim S.A.)  
> **Principio arquitectónico:** Ningún valor de negocio debe estar hardcodeado. Todo dato configurable (pilares, soft skills, escalas, estados, parámetros) debe gestionarse mediante tablas administrables con CRUD completo en el panel de administración.

---

## PARTE I – Panel de Administración (Admin)

El panel de administración es la base de toda la configuración de la plataforma. Cada entidad listada a continuación debe tener su propio ABM (Alta / Baja / Modificación) accesible solo para el rol `admin`.

---

### ⚠️ BUG ACTIVO: El rol `admin` no visualiza las secciones de administración

**Problema reportado:** El usuario con rol `admin` no está viendo los CRUDs del sistema (por ejemplo, la sección de Pilares) en el panel de administración.

**Causa probable y puntos a revisar:**

**1. Guard / middleware de rutas:**
- Verificar que las rutas del panel admin (ej: `/admin/pilares`, `/admin/soft-skills`, etc.) estén protegidas con un guard que permita acceso al rol `admin`.
- Confirmar que el rol del usuario autenticado se está leyendo correctamente desde el token/sesión y coincide exactamente con el string o ID que el guard evalúa (case-sensitive, sin espacios).

**2. Menú de navegación del admin:**
- Verificar que el componente de menú/sidebar del panel admin evalúa el rol del usuario para renderizar los ítems.
- Si el menú filtra ítems por rol, confirmar que el rol `admin` está incluido en la condición de cada ítem de administración.
- El menú NO debe quedar vacío en silencio: si el usuario `admin` no tiene ítems visibles, agregar logging o un mensaje de diagnóstico visible en desarrollo.

**3. Registro de rutas:**
- Confirmar que las rutas admin están registradas en el router (no comentadas, no eliminadas en un refactor reciente).
- Si el framework usa lazy loading de módulos, verificar que el módulo admin se carga correctamente para el rol `admin`.

**4. Permisos en base de datos (si aplica):**
- Si el sistema usa una tabla de permisos o roles en BD (ej: `roles`, `permisos`, `role_permiso`), verificar que el rol `admin` tiene asignados los permisos correspondientes a cada entidad administrable.

**Corrección requerida:**
- El rol `admin` debe ver en su menú/sidebar un ítem o sección **"Administración"** que contenga acceso directo a cada uno de los CRUDs listados en esta Parte I.
- Las rutas deben estar correctamente guardadas y el menú debe renderizar los ítems sin necesidad de ninguna configuración adicional para un usuario con rol `admin`.
- Implementar un test de smoke: al iniciar sesión como `admin`, verificar que `/admin/pilares` devuelve `200` y no redirige a login ni a una pantalla de error de permisos.

---

### A. Tabla: `pilares`

Gestiona los pilares organizacionales de evaluación. Reemplaza cualquier lista estática de pilares en el código.

**Campos:**

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `id` | UUID / autoincrement | Clave primaria |
| `nombre` | string (255) | Nombre del pilar (ej: "Crecimiento de Ventas") |
| `descripcion` | text | Alcance / definición del pilar, visible en la plataforma |
| `activo` | boolean | Indica si el pilar está disponible para asignarse en evaluaciones |
| `orden` | integer | Orden de visualización en formularios y reportes |
| `creado_en` | timestamp | Fecha de creación |
| `actualizado_en` | timestamp | Fecha de última modificación |

**Operaciones CRUD en Admin:**
- **Listar** todos los pilares con filtro por estado activo/inactivo.
- **Crear** nuevo pilar con nombre, descripción y orden.
- **Editar** nombre, descripción, orden y estado de cualquier pilar existente.
- **Desactivar** (soft delete): no eliminar físicamente si el pilar tiene evaluaciones asociadas; solo marcarlo como `activo = false` para que no aparezca en nuevas evaluaciones.
- **Reordenar** mediante drag-and-drop o campos de orden numérico.

**Reglas de negocio:**
- Solo los pilares con `activo = true` aparecen disponibles al crear/editar evaluaciones.
- Las evaluaciones ya creadas conservan referencia al pilar aunque este sea desactivado posteriormente.
- No existe un límite fijo de pilares (no hardcodear "3 pilares").

**Datos iniciales a cargar** *(seed, no hardcode)*:
- Crecimiento de Ventas
- Orientación al Cliente
- Eficiencia Organizacional

> ⚠️ Las definiciones exactas deben obtenerse de la planilla adjunta enviada por Pablo Cirac.

---

### B. Tabla: `soft_skills`

Gestiona las competencias blandas evaluables. Reemplaza cualquier lista estática de soft skills.

**Campos:**

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `id` | UUID / autoincrement | Clave primaria |
| `nombre` | string (255) | Nombre de la soft skill |
| `descripcion` | text | Definición de la soft skill, visible en la plataforma |
| `activo` | boolean | Disponibilidad para asignarse en evaluaciones |
| `orden` | integer | Orden de visualización |
| `creado_en` | timestamp | |
| `actualizado_en` | timestamp | |

**Operaciones CRUD en Admin:**
- **Listar** con filtro por activo/inactivo.
- **Crear** nueva soft skill con nombre y descripción.
- **Editar** todos los campos.
- **Desactivar** (soft delete): no eliminar si tiene evaluaciones asociadas.
- **Reordenar**.

**Reglas de negocio:**
- Solo las soft skills con `activo = true` aparecen al configurar evaluaciones.
- Las evaluaciones existentes conservan sus soft skills aun si estas son desactivadas.

> ⚠️ Solicitar a RRHH el listado completo de soft skills con sus definiciones para el seed inicial.

---

### C. Tabla: `escala_valoracion`

Define las opciones disponibles para valorar pilares, soft skills y el resultado final de evaluaciones. Reemplaza el enum hardcodeado (Excelente / Muy bueno / Bueno / Regular / Malo) y el sistema de estrellas.

**Campos:**

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `id` | UUID / autoincrement | Clave primaria |
| `etiqueta` | string (100) | Texto visible (ej: "Excelente", "Muy bueno") |
| `valor_numerico` | decimal (nullable) | Valor numérico opcional para cálculos o reportes |
| `orden` | integer | Orden de presentación en el selector |
| `activo` | boolean | Si está disponible para usarse en evaluaciones nuevas |
| `creado_en` | timestamp | |
| `actualizado_en` | timestamp | |

**Operaciones CRUD en Admin:**
- **Listar** todas las opciones con su orden y estado.
- **Crear** nueva opción de escala.
- **Editar** etiqueta, valor numérico y orden.
- **Desactivar** (soft delete): no eliminar si hay evaluaciones que ya la usaron.
- **Reordenar**.

**Reglas de negocio:**
- El selector de valoración en formularios se genera dinámicamente desde esta tabla (solo `activo = true`, ordenado por `orden ASC`).
- No hay un mínimo ni máximo de opciones hardcodeado.

**Datos iniciales a cargar** *(seed)*:

| orden | etiqueta | valor_numerico |
|-------|----------|----------------|
| 1 | Excelente | 5 |
| 2 | Muy bueno | 4 |
| 3 | Bueno | 3 |
| 4 | Regular | 2 |
| 5 | Malo | 1 |

---

### D. Tabla: `estados_objetivo`

Define los estados posibles de un objetivo a lo largo de su ciclo de vida. Reemplaza cualquier enum de estados en el código.

**Campos:**

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `id` | UUID / autoincrement | Clave primaria |
| `nombre` | string (100) | Nombre visible del estado |
| `slug` | string (50) | Identificador interno único para lógica de negocio |
| `color_hex` | string (7) | Color para badges/tags en la UI |
| `orden` | integer | Orden de visualización en filtros |
| `activo` | boolean | Si el estado puede asignarse actualmente |
| `creado_en` | timestamp | |

**Operaciones CRUD en Admin:**
- **Listar** estados con su color y slug.
- **Crear** nuevo estado.
- **Editar** nombre, color y orden (el `slug` no debe editarse si hay lógica que depende de él; mostrar advertencia).
- **Desactivar**.

**Reglas de negocio:**
- La lógica de negocio referencia estados por `slug`, nunca por `id` ni por string literal.
- Los slugs del flujo base se documentan como constantes en el código aunque sus valores de presentación sean dinámicos.

**Datos iniciales a cargar** *(seed)*:

| slug | nombre | color_hex |
|------|--------|-----------|
| `borrador` | Borrador | `#9E9E9E` |
| `pendiente_aprobacion` | Pendiente de aprobación | `#FF9800` |
| `aprobado` | Aprobado | `#4CAF50` |
| `en_curso` | En curso | `#2196F3` |
| `completado` | Completado | `#8BC34A` |
| `vencido` | Vencido | `#F44336` |

---

### E. Tabla: `estados_evaluacion`

Análogo a `estados_objetivo`, para el ciclo de vida de evaluaciones y autoevaluaciones.

**Campos:** idénticos a `estados_objetivo` (id, nombre, slug, color_hex, orden, activo, creado_en).

**Operaciones CRUD en Admin:** idénticas a `estados_objetivo`.

**Datos iniciales a cargar** *(seed)*:

| slug | nombre | color_hex |
|------|--------|-----------|
| `pendiente` | Pendiente | `#FF9800` |
| `en_progreso` | En progreso | `#2196F3` |
| `completada` | Completada | `#4CAF50` |
| `proxima_a_vencer` | Próxima a vencer | `#FF5722` |

---

### F. Tabla: `configuracion_plataforma` (clave–valor)

Centraliza todos los parámetros de configuración global. Reemplaza constantes hardcodeadas en el código.

**Campos:**

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `clave` | string (100) PK | Identificador único del parámetro |
| `valor` | text | Valor del parámetro |
| `descripcion` | text | Explicación para el administrador |
| `tipo` | string | `string`, `integer`, `boolean`, `email`, `json` — para validación en el form |
| `actualizado_en` | timestamp | |
| `actualizado_por` | FK usuario | Quién modificó el parámetro por última vez |

**Operaciones CRUD en Admin:**
- **Listar** todos los parámetros con clave, valor y descripción.
- **Editar** el valor (con validación según `tipo`).
- **No permitir crear ni eliminar** claves desde la UI — las claves las define el equipo técnico, el admin solo edita valores.

**Parámetros iniciales a cargar** *(seed)*:

| clave | valor inicial | tipo | descripción |
|-------|--------------|------|-------------|
| `email_soporte` | `rrhh@permaquim.com` | email | Email de ayuda e inconvenientes |
| `dias_proximo_vencimiento` | `7` | integer | Días antes del vencimiento para marcar como "Próximo a vencer" |
| `objetivo_area_habilitado` | `true` | boolean | Habilita objetivo específico por área en evaluaciones |
| `calculos_comerciales_habilitados` | `false` | boolean | Habilita cálculos del área comercial (diferido) |
| `resultado_final_manual` | `true` | boolean | Resultado final de evaluación ingresado manualmente por el jefe |
| `texto_guia_plataforma` | `""` | text | Contenido del manual de uso en la sección Guía |
| `jefe_puede_crear_objetivos` | `false` | boolean | Si es `true`, el rol `jefe` puede dar de alta objetivos. Si es `false`, solo el rol `empleado` / `colaborador` puede crearlos. **Valor actual: `false`.** |

---

### G. Tabla: `areas`

Gestiona las áreas organizacionales para el objetivo específico por área.

**Campos:**

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `id` | UUID / autoincrement | Clave primaria |
| `nombre` | string (255) | Nombre del área |
| `descripcion` | text (nullable) | Descripción opcional |
| `activo` | boolean | |
| `creado_en` | timestamp | |
| `actualizado_en` | timestamp | |

**Operaciones CRUD en Admin:** Listar, Crear, Editar, Desactivar.

**Reglas de negocio:**
- Cada empleado pertenece a un área (`empleados.area_id`).
- El objetivo específico de área se vincula a `areas.id` en cada evaluación.

---

## PARTE II – Módulo de Objetivos

### 1. Carga de Objetivos – Control por Rol (Parametrizado)

**Situación actual:** El jefe/a aún puede dar de alta objetivos, lo cual no debería estar permitido.  
**Cambio requerido:** La posibilidad de crear objetivos debe controlarse mediante el parámetro `jefe_puede_crear_objetivos` de `configuracion_plataforma`. El valor actual es `false`: **solo el rol `empleado` / `colaborador` puede dar de alta objetivos.**

**Requerimientos:**

**Control de acceso por parámetro:**
- Al renderizar la UI, leer el parámetro `jefe_puede_crear_objetivos`.
- Si `false`: ocultar completamente el botón / formulario de "Nuevo objetivo" para cualquier usuario con rol `jefe`. No solo deshabilitar visualmente — no debe renderizarse.
- Si `true`: el rol `jefe` recupera la capacidad de crear objetivos (comportamiento anterior).
- La validación debe aplicarse también en el backend: cualquier endpoint de creación de objetivos debe verificar el parámetro y el rol del usuario autenticado, retornando `403 Forbidden` si el jefe intenta crear con el parámetro en `false`.
- El parámetro es editable por el rol `admin` desde el panel de configuración, sin necesidad de deploy.

**Flujo para el rol `empleado` / `colaborador` (activo con parámetro en `false`):**
- Habilitar el formulario de carga de objetivos para el rol `empleado`.
- Los pilares disponibles se cargan dinámicamente desde `pilares` (solo `activo = true`).
- Estado inicial del objetivo creado por empleado: slug `pendiente_aprobacion` (desde `estados_objetivo`).

**Flujo para el rol `jefe` (independiente del parámetro):**
- El jefe/a **siempre** puede ver los objetivos de sus reportes.
- El jefe/a **siempre** puede aprobar o rechazar objetivos de sus reportes.
- Al aprobar, el estado cambia al slug `aprobado`.
- Lo único que varía según el parámetro es la capacidad de **crear** objetivos.

### 2. Porcentaje por Pilar / Objetivo de Área

- Cada pilar asignado a una evaluación tiene un campo `porcentaje` (decimal).
- Si el parámetro `objetivo_area_habilitado = true`, se muestra la sección de objetivo específico de área con su porcentaje.
- La suma de todos los porcentajes debe ser exactamente **100%**.
- Validación en frontend (indicador en tiempo real) y backend.

### 3. Agrupación – "Cuerpo de Objetivos"

- En la vista del jefe, los objetivos de cada reporte se agrupan en un bloque colapsable por empleado.
- Encabezado del bloque: nombre del empleado + contador de estados (ej: `3 de 5 aprobados`).
- Los labels de estados se leen desde `estados_objetivo`.

---

## PARTE III – Módulo de Evaluación / Autoevaluación

### 4. Reubicación de la Autoevaluación

- El formulario de autoevaluación debe aparecer y completarse desde la sección **"Autoevaluación"**, tanto si está pendiente como si ya fue realizada.
- En "Objetivos y Competencias" no debe mostrarse el formulario activo.

### 5. Escala de Valoración Dinámica

- En todos los formularios de evaluación y autoevaluación, el selector de valoración para pilares y soft skills se genera desde `escala_valoracion` (solo `activo = true`, ordenado por `orden ASC`).
- Reemplaza el sistema de estrellas y cualquier selector estático.

### 6. Terminología: "Valoración"

- Reemplazar **"puntaje"** por **"valoración"** en toda la plataforma: labels, títulos, tooltips, reportes y mensajes de sistema.
- Find-and-replace global en vistas y archivos de i18n.

### 7. Resultado Final – Cálculo Manual

- Controlado por el parámetro `resultado_final_manual` en `configuracion_plataforma`.
- Si `true`:
  - El campo de resultado final es un selector cuyas opciones vienen de `escala_valoracion`.
  - Se elimina el cálculo automático por promedio ponderado.
  - Se elimina el cartel que indica el cálculo automático.
  - Los valores ponderados de cada pilar pueden mostrarse como referencia de solo lectura.
- Si `false` (comportamiento futuro): se puede reactivar el cálculo automático sin cambios de código.

### 8. Eliminar "Evidencias Verificadas"

- Remover de la UI de la sección "Evaluación" (vista del jefe) todo campo o bloque de "evidencias verificadas".
- Si el campo existe en BD: marcarlo como deprecated en la migración (no eliminar datos históricos; excluirlo de nuevas evaluaciones).

### 9. Objetivo Específico por Área

- Habilitado/deshabilitado por el parámetro `objetivo_area_habilitado`.
- Si habilitado, en el formulario de evaluación aparece una sección adicional con:
  - Selector de área (desde `areas`, solo activas).
  - Nombre/descripción del objetivo específico.
  - Porcentaje de ponderación.
  - Selector de valoración (desde `escala_valoracion`).
- El porcentaje del objetivo de área se suma a los pilares; total debe ser 100%.
- El objetivo específico es opcional.

---

## PARTE IV – Definiciones y Sección "Guía"

### 10. Definición de Pilares y Soft Skills en Formularios

- Junto a cada pilar y soft skill en los formularios, mostrar su `descripcion` leída dinámicamente desde `pilares` y `soft_skills`.
- Mecanismo sugerido: tooltip o panel desplegable.

### 11. Nueva Sección: "Guía"

- Sección accesible desde el menú principal.
- Contenido completamente dinámico:
  - **Pilares:** Lista `pilares` con `activo = true`, mostrando `nombre` y `descripcion`.
  - **Soft Skills:** Lista `soft_skills` con `activo = true`, mostrando `nombre` y `descripcion`.
  - **Manual de uso:** Texto leído desde el parámetro `texto_guia_plataforma` en `configuracion_plataforma`.

---

## PARTE V – Dashboard

> **Criterio de diseño:** Todas las métricas se presentan en **cards individuales** (no layout horizontal). Los estados que alimentan los contadores se leen dinámicamente desde `estados_objetivo` y `estados_evaluacion` por slug.

### 12. Cards de Métricas por Sección

**Sección Dashboard** — empleados (datos propios) y jefes (datos de equipo):

| Card | Slug de estado fuente |
|------|-----------------------|
| Total de Objetivos | todos |
| Objetivos Completos | `completado` |
| Objetivos en Curso | `en_curso` |
| Pendientes de Revisión | `pendiente_aprobacion` |
| Próximos a Vencer | calculado con parámetro `dias_proximo_vencimiento` |

**Sección Autoevaluación** — jefes ven su equipo; empleados ven solo las propias:

| Card | Slug de estado fuente |
|------|-----------------------|
| Total de Autoevaluaciones | todos |
| Completadas | `completada` |
| Pendientes | `pendiente` |
| Próximas a Vencer | calculado con `dias_proximo_vencimiento` |

**Sección Evaluación** — empleados y jefes según corresponda:

| Card | Slug de estado fuente |
|------|-----------------------|
| Total de Evaluaciones | todos |
| Completadas | `completada` |
| Pendientes de Evaluación | `pendiente` |
| Próximas a Vencer | calculado con `dias_proximo_vencimiento` |

**Reglas de negocio:**
- El umbral de "Próximo a vencer" se lee desde `configuracion_plataforma.dias_proximo_vencimiento` (nunca hardcodeado).
- Los labels de los cards usan los `nombre` de las filas correspondientes en `estados_objetivo` / `estados_evaluacion`.

---

## PARTE VI – Configuración y Notificaciones

### 13. Email de Soporte

- El email se lee desde `configuracion_plataforma` con clave `email_soporte`.
- Todos los puntos de la UI que muestran el email de contacto lo consumen dinámicamente desde este parámetro.

### 14. Nómina de Empleados

- Actualizar la nómina (altas/bajas) gestionándola desde el Admin → ABM de Empleados/Usuarios.
- Coordinación con Salvador Crosio.

---

## PARTE VII – Diagrama de Tablas Admin (Resumen)

```
configuracion_plataforma     pilares               soft_skills
────────────────────────     ───────               ───────────
clave (PK)                   id (PK)               id (PK)
valor                        nombre                nombre
descripcion                  descripcion           descripcion
tipo                         activo                activo
actualizado_en               orden                 orden
actualizado_por              creado_en             creado_en
                             actualizado_en        actualizado_en

escala_valoracion            estados_objetivo      estados_evaluacion
─────────────────            ────────────────      ──────────────────
id (PK)                      id (PK)               id (PK)
etiqueta                     nombre                nombre
valor_numerico               slug (unique)         slug (unique)
orden                        color_hex             color_hex
activo                       orden                 orden
creado_en                    activo                activo
actualizado_en               creado_en             creado_en

areas
──────
id (PK)
nombre
descripcion
activo
creado_en
actualizado_en
```

---

## Resumen de Cambios por Prioridad Sugerida

| # | Cambio | Área Afectada |
|---|--------|---------------|
| 1 | Crear tablas admin con CRUDs: pilares, soft_skills, escala_valoracion, estados_objetivo, estados_evaluacion, areas, configuracion_plataforma | Backend / Admin |
| 2 | Seed de datos iniciales en todas las tablas | BD / Migrations |
| 3 | Carga de objetivos por empleados + aprobación del jefe | Módulo Objetivos |
| 4 | Reubicación de autoevaluación a sección correcta | Módulo Autoevaluación |
| 5 | Reemplazar selectores estáticos por escala_valoracion dinámica | Módulo Evaluación / Autoevaluación |
| 6 | Cambio "puntaje" → "valoración" (find & replace global) | Toda la plataforma |
| 7 | Resultado final manual (controlado por configuracion_plataforma) | Módulo Evaluación |
| 8 | Eliminar "evidencias verificadas" | Módulo Evaluación |
| 9 | Agrupación de objetivos en "Cuerpo de objetivos" para jefes | Vista Jefe |
| 10 | Pilares + porcentajes + objetivo específico de área (todos dinámicos) | Módulo Pilares / Evaluación |
| 11 | Sección "Guía" con contenido dinámico desde tablas | Menú / Navegación |
| 12 | Dashboard con cards de métricas usando estados dinámicos | Módulo Dashboard |
| 13 | Email de soporte desde configuracion_plataforma | Config / Notificaciones |
| 14 | Actualización de nómina de empleados | Administración |

---

## Pendientes / Aclaraciones Necesarias

- **Definiciones de pilares:** Obtener la planilla de Pablo Cirac con el alcance exacto de cada pilar para el seed de `pilares.descripcion`.
- **Listado de soft skills:** Obtener de RRHH el listado completo con definiciones para el seed de `soft_skills`.
- **Slugs de estados existentes:** Validar con el equipo técnico los slugs que ya existen en el código para no romper lógica al migrar a tablas dinámicas.
- **Formato del resultado final manual:** Confirmar con RRHH si el selector usa `escala_valoracion` o texto libre.
- **Tipo de visualización en Dashboard:** Confirmar con Salvador Crosio si las cards muestran solo número, barra de progreso, gráfico de torta, etc.
- **Valor inicial de `dias_proximo_vencimiento`:** Confirmar el umbral en días (sugerido: 7 días).
