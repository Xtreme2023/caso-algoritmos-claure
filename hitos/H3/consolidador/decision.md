# Decisión de diseño

## Problema

Consolidar planillas de siete departamentos, normalizar campos heterogéneos, detectar matrículas repetidas, calcular aprobación, comprobar prerrequisitos y producir archivos utilizables para la secretaría.

La muestra de H1 contiene 305 inscripciones y 150 CI distintos. Una persona puede tener varias materias, por lo que CI por sí solo no es una clave de matrícula. También hay filas con CI `0`/faltante y campos académicos vacíos; esos casos no deben convertirse en resultados confiables por defecto.

## Algoritmos y estructuras seleccionados

1. **Recorrido lineal de entrada, O(n).** Cada fila se lee y normaliza una vez. Se calculan las tres notas y la asistencia durante este mismo recorrido.
2. **Diccionario con clave compuesta, O(1) promedio por inserción.** Se usa `CI + código de materia + módulo` para identificar una matrícula y agrupar copias recibidas desde departamentos distintos. Sin CI, se usa nombre normalizado + materia + módulo y se marca la identidad para revisión manual.
3. **Comparación de valores duplicados.** Si dos copias de una matrícula tienen notas o asistencia diferentes, el sistema no escoge una arbitrariamente; genera una incidencia.
4. **Grafo dirigido de niveles y BFS, O(V + E).** Para estudiantes `MIEMBRO`, las aristas son `A1 -> A2 -> B1 -> B2 -> C1 -> C2`. La evidencia de aprobación del nivel anterior se consulta en un archivo de historial por CI. BFS valida la ruta al nivel siguiente; para esta cadena, V y E son constantes pequeñas.
5. **Salida ordenada, O(n log n).** Se ordenan las inscripciones por CI y materia únicamente al producir el archivo final. No se mantiene un árbol ordenado durante toda la importación porque la carga es por lotes.

## Alternativas descartadas

- **Comparar cada par de filas para hallar duplicados:** O(n²); no escala y repite comparaciones.
- **Lista lineal para buscar claves en cada fila:** puede llevar la consolidación a O(n²).
- **Tabla hash como índice único de salida:** da búsquedas rápidas, pero no conserva por sí sola el orden requerido para actas.
- **Programación dinámica o algoritmo voraz:** no hay una selección que maximizar ni subproblemas de optimización. Las notas se clasifican con umbrales independientes.
- **Fusionar solo por nombre:** puede juntar homónimos. La coincidencia exacta normalizada ayuda a reducir variaciones de formato, pero no demuestra identidad; por eso se informa para revisión.

## Decisiones académicas configuradas

- Nota final: promedio simple de parcial 1, parcial 2 y examen final.
- Se aprueba con nota final `>= 51` y asistencia `>= 80 %`.
- Sin una nota válida o asistencia válida no se emite resultado automático.
- La progresión por nivel de miembros requiere que el nivel inmediatamente anterior aparezca como `APROBADO` en el historial.
- Cursos generales y cursos abiertos externos/internos no se consideran parte del grafo A1–C2.

## Alcance de las mediciones

Los tiempos documentados en H2 comparan búsquedas por CI en una lista y un `SortedDictionary`; no miden este proceso completo ni la entrada XLSX. La elección aquí prioriza una sola pasada, agrupación hash y salida ordenada, que se ajustan a las operaciones reales de consolidación.
