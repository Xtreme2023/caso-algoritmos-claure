# Consolidador académico H3

Aplicación de consola para juntar planillas de un módulo, consolidar inscripciones, calcular aprobación y revisar si cada estudiante puede avanzar al siguiente nivel.

La explicación detallada del flujo, las reglas, los formatos y los archivos de salida está en [explicacion-programa.md](./explicacion-programa.md).

El caso real, quiénes son afectados, la decisión algorítmica, la evidencia y sus límites están resumidos en [analisis-caso-y-evidencia.md](./analisis-caso-y-evidencia.md).

## Requisitos

- .NET 9 SDK.
- Planillas departamentales en `.xlsx` o `.csv`.
- Un archivo adicional `.csv` o `.xlsx` con aprobaciones de módulos anteriores.

Los encabezados pueden incluir acentos, espacios y diferencias de mayúsculas. Se reconocen nombres como `ci`/`carnet`, `nombre_completo`/`nombre`, `cod_materia`/`materia`/`nivel`, `parcial_1`/`p1` y `asistencias`/`sesiones`.

En XLSX se lee cada hoja con datos. En CSV se detecta separador coma o punto y coma. La fila que contiene los encabezados debe incluir CI o carnet y código de materia o materia. Las notas pueden usar coma o punto decimal.

## Archivo de aprobaciones previas

Debe tener una fila de encabezados y, como mínimo, estas columnas:

```csv
ci;cod_materia;estado
1234567;A1;APROBADO
7654321;A2;APROBADO
```

Solo los registros con estado `APROBADO` se consideran prerrequisitos cumplidos. El archivo debe representar resultados de módulos anteriores, no las notas que se están consolidando.

## Ejecución

Coloca los archivos de los departamentos en una carpeta y ejecuta:

```powershell
dotnet run --project hitos\H3\consolidador -- --entrada "D:\planillas\2026-M2" --historial "D:\planillas\historial_aprobaciones.csv" --salida "D:\planillas\resultado"
```

La carpeta de entrada contiene las planillas del módulo actual; el historial y la salida deben estar fuera de esa carpeta. La primera compilación restaura ClosedXML para poder leer libros `.xlsx`.

Se generan:

- `consolidado.csv`: una fila por inscripción única, con notas, asistencia, resultado y decisión de avance.
- `revisiones.csv`: filas incompletas, CI faltantes, colisiones de identidad y versiones contradictorias de una matrícula.

Para comparar la búsqueda lenta con la deduplicación por diccionario en las planillas de entrada:

```powershell
dotnet run --project hitos\H3\consolidador -- --benchmark "D:\planillas\2026-M2"
```

El benchmark hace una ejecución de calentamiento/calibración y cinco repeticiones medidas por tamaño. La metodología y los resultados de H1 están en [comparacion-rendimiento.md](./comparacion-rendimiento.md). La ejecución normal no realiza esas repeticiones.

En modo normal, cada fila de entrada se valida y agrupa una vez; luego se recorren los resultados para generar revisiones y escribir la salida ordenada.

## Planillas de ejemplo

La carpeta `ejemplos/planillas` contiene siete CSV de datos ficticios. Incluyen una matrícula compartida entre departamentos, un CI faltante, un conflicto de notas, una nota cercana al umbral y casos de prerrequisito cumplido o faltante. El historial de ejemplo está en `ejemplos/historial_aprobaciones.csv`.

Para ejecutarlas desde la raíz del repositorio:

```powershell
dotnet run -c Release --project hitos\H3\consolidador -- --entrada "hitos\H3\consolidador\ejemplos\planillas" --historial "hitos\H3\consolidador\ejemplos\historial_aprobaciones.csv" --salida "hitos\H3\consolidador\ejemplos\salida"
```

La carpeta de salida queda fuera de `ejemplos/planillas`; los datos de muestra son solo para probar el programa, no corresponden a alumnos reales.

## Reglas

- Nota final: promedio aritmético de parcial 1, parcial 2 y examen final, redondeado a un decimal. La aprobación se determina con el promedio sin redondear.
- Aprobación: nota final `>= 51` y asistencia `>= 80 %`.
- Si hay asistencias y sesiones, el porcentaje es `asistencias / sesiones * 100`. También se acepta una columna de porcentaje de asistencia.
- Para estudiantes `MIEMBRO`, los niveles `A1 -> A2 -> B1 -> B2 -> C1 -> C2` forman el grafo dirigido de prerrequisitos. Para avanzar, la materia actual debe estar aprobada y el historial debe acreditar el nivel anterior. BFS verifica la ruta del grafo al siguiente nivel.
- Materias `G1`–`G3` y `E1`–`E2` no tienen arista en ese grafo y no reciben una decisión de progresión por niveles.
- Las matrículas con CI se identifican por CI, código de materia y módulo. CI `0` se trata como faltante.
- Sin CI, el programa agrupa por nombre normalizado, materia y módulo, pero marca la identidad para revisión. La normalización iguala mayúsculas, tildes y puntuación; no adivina nombres escritos en distinto orden.
- Si dos filas de la misma matrícula tienen notas o asistencias distintas, no se selecciona una versión automáticamente: la matrícula queda en revisión.

El programa no confirma automáticamente una identidad dudosa ni emite un avance como válido cuando falta el CI o existe un conflicto de datos.
