# Plan de Implementación — Predicción de Riesgo con ML.NET

> **Requisito de proyecto** — Módulo de IA/ML integrado al sistema de evaluación  
> **Rol**: Solo Docentes (los administradores no ven este menú)  
> Sin código — documento de planificación

---

## Objetivo del módulo

Permitir al docente buscar un estudiante de su curso, ver su historial de notas parciales, y recibir una **predicción de riesgo académico** generada por un modelo ML.NET que estima la probabilidad de que el estudiante repruebe al final del período.

---

## Arquitectura general

```
Docente busca estudiante
        ↓
PrediccionController.Buscar()
        ↓
Vista: formulario de búsqueda + tabla de resultados
        ↓
Docente selecciona estudiante
        ↓
PrediccionController.Resultado(estudianteId)
        ↓
Carga historial de notas parciales desde BD
        ↓
ML.NET FastForest: genera probabilidad de riesgo (0.0 → 1.0)
        ↓
Vista: historial + probabilidad + gráfico Chart.js
```

---

## Archivos que se van a crear o modificar

### Nuevos archivos

| Archivo | Propósito |
|---------|-----------|
| `ML/RiesgoModelInput.cs` | Clase con los campos de entrada al modelo ML (notas parciales) |
| `ML/RiesgoModelOutput.cs` | Clase con el resultado del modelo (Score / probabilidad) |
| `ML/PrediccionService.cs` | Servicio que entrena el modelo y expone el método `Predecir()` |
| `Controllers/PrediccionController.cs` | Controlador con acciones `Buscar` y `Resultado` |
| `Views/Prediccion/Buscar.cshtml` | Vista: buscador de estudiante por nombre o código |
| `Views/Prediccion/Resultado.cshtml` | Vista: historial de notas + predicción + gráfico |
| `Models/PrediccionViewModel.cs` | ViewModel que lleva los datos de historial y resultado al Resultado.cshtml |

### Archivos que se modifican

| Archivo | Qué se cambia |
|---------|--------------|
| `Views/Shared/_Layout.cshtml` | Descomentar y completar el `<li>` "Análisis Reportes" que ya existe comentado (línea ~340) |
| `Program.cs` | Registrar `PrediccionService` como `Singleton` en el contenedor de dependencias |

---

## Fase 1 — Entrada y salida del modelo ML.NET

### `ML/RiesgoModelInput.cs`

Campos que el modelo recibe por estudiante:

- `float Nota1` — nota del primer parcial (0–100, o -1 si no existe aún)
- `float Nota2` — nota del segundo parcial
- `float Nota3` — nota del tercer parcial
- `float Nota4` — nota del cuarto parcial
- `float PromedioActual` — promedio calculado de las notas disponibles
- `float NotasRegistradas` — cuántas notas de las 4 ya fueron ingresadas

> Usar `-1f` como valor centinela para parciales sin nota, no `0f`, porque `0` es una nota válida.

### `ML/RiesgoModelOutput.cs`

Campos que devuelve el modelo:

- `float Score` — valor bruto del modelo
- `float Probabilidad` — valor transformado al rango `[0.0, 1.0]` que representa la probabilidad de riesgo (>= 0.5 = en riesgo)

---

## Fase 2 — Servicio de predicción (`PrediccionService`)

Este servicio se registra como `Singleton` para que el modelo ML.NET se entrene una sola vez al arrancar la aplicación.

### Datos de entrenamiento (internos al servicio)

Se genera un conjunto de datos sintético hardcodeado en el propio servicio — no se necesita un archivo externo. El conjunto cubre los patrones reales:

- Estudiantes con todas las notas altas → bajo riesgo
- Estudiantes con promedios entre 50–65 → riesgo medio
- Estudiantes con notas bajas o faltantes → alto riesgo
- Estudiantes con mejora progresiva (notas subiendo) → riesgo bajo
- Estudiantes con caída progresiva (notas bajando) → riesgo alto

Se recomienda al menos **60–80 registros sintéticos** para que el árbol tenga suficiente varianza.

### Algoritmo: FastForest Regression

`FastForest` (bosque aleatorio) es el algoritmo elegido porque:
- Maneja bien datos faltantes (notas sin registrar)
- No requiere normalización de datos
- Es robusto con conjuntos de entrenamiento pequeños
- Disponible en `Microsoft.ML.FastTree` (NuGet)

### Pipeline ML.NET

El pipeline de entrenamiento tendrá esta secuencia de transformaciones:

1. `ReplaceMissingValues` → sustituye los `-1` por la media del conjunto
2. `Concatenate` → combina todos los campos numéricos en el vector `Features`
3. `FastForestRegression` → entrenamiento del modelo
4. `ColumnRename` → renombra `Score` a `Probabilidad`

El resultado `Score` ya está en rango `[0,1]` con `FastForestRegression` porque internamente promedia árboles de clasificación binaria.

### Método principal

```
float Predecir(RiesgoModelInput input)
```

Recibe los datos del estudiante y devuelve la probabilidad de riesgo (0.0 = sin riesgo, 1.0 = riesgo máximo).

---

## Fase 3 — Controlador (`PrediccionController`)

### Acción `Buscar` (GET)

- Verifica sesión docente (llama `VerificarDocente()`)
- Recibe parámetro opcional `q` (texto de búsqueda)
- Si `q` no está vacío: consulta la BD filtrando estudiantes del curso activo del docente cuyo nombre o código contenga `q`
- Pasa los resultados a la vista `Buscar.cshtml`
- `ActiveMenu = "Prediccion"`

### Acción `Resultado` (GET)

- Verifica sesión docente
- Recibe `estudianteId` (int)
- Valida que el estudiante pertenece al curso activo del docente (seguridad — no puede ver estudiantes de otro docente)
- Carga las notas parciales del estudiante desde la BD
- Construye el `RiesgoModelInput` con esas notas
- Llama a `PrediccionService.Predecir(input)`
- Construye el `PrediccionViewModel` con historial + probabilidad + nivel de riesgo
- Retorna la vista `Resultado.cshtml`

### Niveles de riesgo

El controlador clasifica la probabilidad en tres niveles para facilitar la presentación visual:

| Probabilidad | Nivel | Color en vista |
|--------------|-------|---------------|
| 0.0 – 0.39 | Bajo | Verde |
| 0.40 – 0.65 | Medio | Amarillo/Naranja |
| 0.66 – 1.0 | Alto | Rojo |

---

## Fase 4 — ViewModel (`PrediccionViewModel`)

Hereda de `LayoutViewModel` (igual que el resto del proyecto).

Propiedades adicionales:



- `Estudiante EstudianteInfo` — datos básicos del estudiante (nombre, código, correo)
- `List<NotaParcial> NotasParciales` — historial de las notas ingresadas
- `float Probabilidad` — resultado del modelo (0.0 – 1.0)
- `string NivelRiesgo` — "Bajo", "Medio", "Alto"
- `string ColorRiesgo` — clase CSS ("text-success", "text-warning", "text-danger")
- `string[] EtiquetasGrafico` — para Chart.js: ["Parcial 1", "Parcial 2", "Parcial 3", "Parcial 4"]
- `float[] DatosGrafico` — valores de cada nota para Chart.js (null se mapea a 0 en el gráfico)
- `float PromedioActual` — promedio calculado de notas existentes

---

## Fase 5 — Vistas

### `Views/Prediccion/Buscar.cshtml`

Layout: usa `_Layout.cshtml`, `ActiveMenu = "Prediccion"`

Secciones de la vista:

1. **Encabezado de página** — título "Predicción de Riesgo Académico" + subtítulo "Busca un estudiante para analizar su riesgo"

2. **Formulario de búsqueda** — campo de texto con placeholder "Nombre o código del estudiante" + botón "Buscar". Envía GET a la misma acción con parámetro `q`.

3. **Tabla de resultados** (visible solo si `q` no está vacío):
   - Columnas: Código | Nombre | Promedio actual | Estado | Acción
   - Cada fila tiene un botón "Ver predicción" que lleva a `Prediccion/Resultado/{id}`
   - Si no hay resultados: mensaje "No se encontraron estudiantes con ese criterio"

4. **Estado vacío inicial** (sin búsqueda): ícono de lupa + texto "Ingresa el nombre o código de un estudiante para comenzar"

---

### `Views/Prediccion/Resultado.cshtml`

Layout: usa `_Layout.cshtml`, `ActiveMenu = "Prediccion"`



Secciones de la vista:

1. **Breadcrumb**: Inicio › Predicción › [Nombre del estudiante]

2. **Tarjeta de información del estudiante**:
   - Nombre completo, código, correo
   - Promedio actual + estado (Aprobado / Reprobado / Sin Notas)

3. **Tarjeta de predicción ML** — la más importante visualmente:
   - Título: "Análisis de Riesgo con IA"
   - Probabilidad en número grande: ej. "72%"
   - Badge de nivel: "RIESGO ALTO" en rojo / "RIESGO MEDIO" en naranja / "RIESGO BAJO" en verde
   - Barra de progreso horizontal que refleja la probabilidad
   - Texto explicativo según nivel:
     - Bajo: "El estudiante muestra un rendimiento estable. Se recomienda mantener el seguimiento regular."
     - Medio: "El estudiante presenta señales de alerta. Se recomienda atención especial en los próximos parciales."
     - Alto: "El estudiante tiene alta probabilidad de reprobar. Se recomienda intervención inmediata."

4. **Tabla de historial de notas**:
   - Columnas: Parcial | Nota | Fecha de registro | Observación
   - Si un parcial no tiene nota: celda muestra "Pendiente" con estilo gris
   - Fila del promedio al final

5. **Gráfico de rendimiento** (Chart.js — gráfico de línea):
   - Eje X: Parcial 1, Parcial 2, Parcial 3, Parcial 4
   - Eje Y: 0 – 100
   - Línea azul: notas del estudiante
   - Línea de referencia roja punteada en `y = 60` (nota mínima para aprobar)
   - Tooltips activados al pasar el mouse
   - Si un parcial no tiene nota, el punto no se dibuja (`null` en el array de datos de Chart.js)

6. **Botón "Volver a búsqueda"** — regresa a `Prediccion/Buscar`

---

## Fase 6 — Menú en `_Layout.cshtml`

En el layout existe un bloque comentado (~línea 340) que tiene el esqueleto del menú "Análisis Reportes". Se descomenta y se adapta así:

- Solo visible cuando `!esAdmin` (igual que el menú de Docentes)
- `menu == "Prediccion"` para activar el estado activo
- Submenú con un solo ítem: "Predicción de Riesgo" → `asp-controller="Prediccion" asp-action="Buscar"`
- Ícono: se puede usar `~/img/icons/analytics.svg` o cualquiera de los disponibles en la carpeta de íconos
- Texto del menú padre: "Análisis IA"

---

## Fase 7 — NuGet packages a instalar

Antes de escribir código, agregar estos paquetes al `.csproj`:

| Paquete | Versión | Para qué |
|---------|---------|---------|
| `Microsoft.ML` | 3.0.1 | Runtime base de ML.NET |
| `Microsoft.ML.FastTree` | 3.0.1 | Algoritmo FastForest |

> Usar versión `3.0.x` — compatible con .NET 8. Evitar `4.x` que requiere .NET 9.

Comando de instalación (desde la carpeta del `.csproj`):
```
dotnet add package Microsoft.ML --version 3.0.1
dotnet add package Microsoft.ML.FastTree --version 3.0.1
```

---

## Registro en `Program.cs`

El servicio `PrediccionService` se registra como `Singleton` porque el modelo ML.NET debe entrenarse una sola vez al iniciar la aplicación (el entrenamiento tarda ~1–2 segundos):

```
builder.Services.AddSingleton<PrediccionService>();
```

Se inyecta en el controlador via constructor igual que `ApplicationDbContext`.

---

## Orden de implementación sugerido

1. Instalar los NuGet packages
2. Crear `ML/RiesgoModelInput.cs` y `ML/RiesgoModelOutput.cs`
3. Crear `ML/PrediccionService.cs` (con datos sintéticos + pipeline + método Predecir)
4. Registrar `PrediccionService` en `Program.cs`
5. Crear `Models/PrediccionViewModel.cs`
6. Crear `Controllers/PrediccionController.cs`
7. Crear `Views/Prediccion/Buscar.cshtml`
8. Crear `Views/Prediccion/Resultado.cshtml` (sin gráfico primero, luego agregar Chart.js)
9. Modificar `_Layout.cshtml` para agregar el ítem de menú
10. Prueba completa: buscar estudiante → ver predicción → verificar gráfico

---

## Limitaciones conocidas (para documentar en el proyecto)

- El modelo se entrena con datos **sintéticos** al arrancar — no con datos históricos reales de la BD. Esto es aceptable para demostración académica.
- En un sistema de producción real, el modelo se entrenaría periódicamente con datos históricos de estudiantes de semestres anteriores.
- La predicción no reemplaza el juicio del docente; es una herramienta de apoyo.
- Estudiantes con 0 notas registradas reciben predicción basada solo en el patrón de "sin datos" — el modelo los clasificará con riesgo medio-alto por defecto.

---

*Plan generado para EduPath AI — Módulo de Predicción ML.NET — Abril 2026*
