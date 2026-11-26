# Explicación completa del consolidador

Este documento describe qué hace el programa, cómo interpreta las planillas y qué decisiones toma. Para ejecutar ejemplos, consulta el [README](./README.md); las razones para elegir las estructuras y algoritmos están en [decision.md](./decision.md), y los tiempos medidos están en [comparacion-rendimiento.md](./comparacion-rendimiento.md).

## 1. Objetivo

El programa consolida las inscripciones de un módulo recibidas desde varios departamentos. Para cada inscripción válida:

1. Lee y normaliza los campos de la planilla.
2. Detecta si la misma inscripción ya apareció en otro archivo.
3. Calcula la nota final y el porcentaje de asistencia.
4. Clasifica el resultado de la materia.
5. Si es un nivel de miembro, comprueba los datos históricos necesarios para informar si puede avanzar.
6. Escribe un consolidado y una lista separada de casos que requieren atención.

Cada ejecución normal trata una carpeta de entrada como un solo cierre de módulo. Puede contener más o menos de siete archivos, pero solo busca archivos `.csv` y `.xlsx` en esa carpeta, no en sus subcarpetas.

## 2. Cómo se ejecuta

Desde la raíz del repositorio:

```powershell
dotnet run -c Release --project hitos\H3\consolidador -- --entrada "D:\planillas\2026-M2" --historial "D:\planillas\historial_aprobaciones.csv" --salida "D:\planillas\resultado"
```

Se necesitan:

- **Entrada:** carpeta con las planillas del módulo que se consolida.
- **Historial:** aprobaciones anteriores por CI y nivel/materia. No debe ser el mismo conjunto de notas que se está procesando.
- **Salida:** carpeta donde se crearán `consolidado.csv` y `revisiones.csv`.

La aplicación crea la carpeta de salida si no existe. Si los archivos de salida ya existen, los reemplaza. Al primer uso, .NET restaura ClosedXML, que se usa para leer libros `.xlsx`.

## 3. Lectura de archivos

### CSV

El lector detecta si el separador es coma o punto y coma. Interpreta campos entre comillas, comillas escapadas y saltos de línea dentro de una celda. La fila que contiene los encabezados puede estar después de un título; debe incluir una columna de CI/carnet y otra de código de materia, nivel o materia.

### Excel

Se aceptan archivos `.xlsx`. Se revisan las hojas que tienen celdas usadas y se busca en cada hoja una fila de encabezados válida. Una hoja con contenido pero sin los encabezados requeridos causa un error explícito; no se omite silenciosamente.

Los archivos temporales de Excel que empiezan por `~$` se ignoran. Los archivos `.xls` antiguos no están soportados: deben guardarse como `.xlsx` o exportarse a CSV.

### Encabezados reconocidos

Los encabezados se comparan ignorando mayúsculas, tildes, espacios y puntuación. Entre los nombres aceptados están:

| Dato | Encabezados reconocidos |
|---|---|
| CI | `ci`, `carnet`, `carnetidentidad`, `numeroci`, `numerocarnet`, `nroci` |
| Nombre | `nombrecompleto`, `nombre`, `estudiante`, `alumno` |
| Materia/código | `codmateria`, `codigomateria`, `codcurso`, `codigoasignatura`, `nivel`, `nivelactual`, `materia`, `asignatura`, `curso` |
| Tipo de estudiante | `tipo`, `tipoestudiante`, `categoria` |
| Departamento | `departamento`, `depto`, `sede` |
| Parciales | `parcial1`/`nota1`/`p1` y `parcial2`/`nota2`/`p2` |
| Examen final | `examenfinal`, `notaexamen`, `final` |
| Asistencia | `asistencias`/`asistencia`/`presentes` más `sesiones`/`totalclases`/`clases`, o un porcentaje `asistenciapct`/`porcentajeasistencia`/`attendancepct` |

Las notas aceptan coma o punto decimal. Materias conocidas como `Nivel A1`, `General 1` y `Curso Abierto 1` se normalizan a `A1`, `G1` y `E1`, respectivamente.

## 4. Normalización y validación de filas

Antes de agrupar, el programa:

- Convierte nombres, departamentos, tipos y códigos a una forma normalizada para comparar; quita tildes y puntuación. En la salida conserva el nombre recibido en la primera fila agrupada.
- Normaliza el CI quitando espacios, puntos, guiones y ceros iniciales. `0`, `N/A`, `S/D` y un CI inválido se tratan como ausentes.
- Exige nombre, materia, dos parciales, examen final y una asistencia válida. Las notas deben estar entre 0 y 100.
- Para asistencia por sesiones, exige `sesiones > 0` y `0 <= asistencias <= sesiones`. Si viene un porcentaje, exige que esté entre 0 y 100.

Una fila que no supera estas validaciones no se incorpora al consolidado: se añade a `revisiones.csv` con el archivo, la fila y el motivo. El programa no sustituye campos vacíos por ceros ni calcula resultados incompletos.

## 5. Identidad y detección de duplicados

Una inscripción se identifica con la clave compuesta:

```text
CI + código de materia + módulo
```

Así, un estudiante puede aparecer en varias materias sin que esas filas se confundan con duplicados. El módulo forma parte de la clave; se recomienda procesar un solo módulo por ejecución.

La clave se guarda en un `Dictionary`. La búsqueda es O(1) en promedio, por lo que leer y agrupar `n` filas cuesta O(n) esperado. La memoria usada para guardar las filas y las claves también crece con `n`.

### Cuando falta el CI

Como regla de respaldo, el programa usa:

```text
nombre normalizado + código de materia + módulo
```

Esta coincidencia puede agrupar una matrícula que llegó sin CI desde más de un departamento, pero **no confirma que sea la misma persona**. Por eso cada fila así agrupada queda marcada para revisión y no se autoriza el avance. La normalización iguala mayúsculas, tildes y puntuación; no interpreta cambios en el orden de los nombres.

### Copias contradictorias

Si una misma clave aparece varias veces, se comparan las notas, asistencia y tipo de estudiante:

- Si coinciden, se conserva una inscripción y se reúnen los nombres de los departamentos que la enviaron.
- Si difieren, no se elige una versión como correcta. La inscripción queda como `REVISAR_DATOS`, sus campos numéricos de salida se dejan vacíos y el caso se explica en `revisiones.csv`.

## 6. Nota, asistencia y aprobación

La nota final usa la fórmula acordada:

```text
(parcial 1 + parcial 2 + examen final) / 3
```

Se muestra redondeada a un decimal, pero la aprobación compara el promedio **sin redondear**. Esto evita que un promedio como 50,966 se apruebe solo porque se muestra como 51,0.

Cuando la entrada trae asistencias y sesiones:

```text
asistencia (%) = asistencias / sesiones * 100
```

La materia se aprueba si se cumplen ambas reglas:

```text
promedio sin redondear >= 51
asistencia >= 80 %
```

El resultado académico no se mezcla con la identidad ni con el prerrequisito: una fila con datos académicos completos puede tener resultado de materia aprobado, pero el avance seguir bloqueado si no se acredita la identidad o el nivel anterior.

## 7. Grafo de prerrequisitos

Para estudiantes de tipo `MIEMBRO`, el plan de niveles es un grafo dirigido:

```text
A1 -> A2 -> B1 -> B2 -> C1 -> C2
```

Cada arista significa que el nivel de origen es requisito para cursar el siguiente. El programa consulta el archivo histórico por la pareja CI–nivel; solo `APROBADO`, `SI`, `TRUE` o `1` cuentan como aprobados. Los estados de rechazo se reconocen, pero no se agregan al conjunto de aprobaciones.

BFS (búsqueda en anchura) comprueba la ruta del nivel actual al siguiente. Su costo es O(V + E); aquí el grafo tiene seis nodos y cinco aristas. La progresión se informa como disponible solo si la materia actual está aprobada, el prerrequisito inmediato aparece en el historial y la identidad no está pendiente de revisión.

Los códigos `G1`–`G3` y `E1`–`E2` no pertenecen a ese grafo. Por eso no reciben una decisión de progresión por niveles. Los tipos distintos de `MIEMBRO` tampoco se evalúan contra la cadena A1–C2.

## 8. Archivos que produce

### `consolidado.csv`

Contiene una fila por matrícula consolidada, ordenada por CI —o nombre normalizado cuando falta CI— y código de materia. Usa punto y coma como separador e incluye la nota, la asistencia, el resultado y campos de avance:

- `estado_materia`: `APROBADO`, `REPROBADO`, `REVISAR_IDENTIDAD` o `REVISAR_DATOS`.
- `prerrequisito`: por ejemplo, `CUMPLIDO`, `NO_ACREDITADO`, `NO_APLICA` o `REVISAR_IDENTIDAD`.
- `proximo_nivel`: nivel siguiente, si existe.
- `puede_inscribirse_siguiente`: `SI`, `NO`, `REVISAR`, `REVISAR_DATOS`, `NO_APLICA` o `NO_HAY_NIVEL_SIGUIENTE`.
- `identidad`: `IDENTIFICADA` o `REVISAR_CI`.

Si la fuente proporcionó solo el porcentaje de asistencia, los campos de cantidad de asistencias y sesiones quedan vacíos, no se inventan.

### `revisiones.csv`

Registra filas incompletas, CI faltantes o inválidos y copias contradictorias. Incluye nombre del archivo, número de fila, CI recibido, nombre, materia y motivo para que la secretaría pueda corregir el origen.

La cifra `Aprobados` que imprime la consola cuenta matrículas cuyo resultado académico pasa los dos umbrales y que no tienen conflicto ni identidad pendiente. No es el número de personas autorizadas a avanzar: esa decisión se encuentra en `puede_inscribirse_siguiente`.

## 9. Modo de benchmark

El benchmark se ejecuta aparte del proceso normal:

```powershell
dotnet run -c Release --project hitos\H3\consolidador -- --benchmark "D:\planillas\2026-M2"
```

Compara `List.Contains` —búsqueda lineal, O(n²) al deduplicar muchas claves— contra el patrón de diccionario usado por el consolidador —O(n) esperado—. Verifica que ambos produzcan el mismo número de claves únicas. Escala la carga base por factores 1, 5, 10, 20 y 40, conservando el patrón de repetición, y mide solo la deduplicación.

Por cada algoritmo y tamaño hace calentamiento/calibración y cinco repeticiones medidas. No lee el historial ni crea los dos CSV de salida. Los resultados detallados y sus límites se explican en [comparacion-rendimiento.md](./comparacion-rendimiento.md).

## 10. Organización del código

| Archivo | Responsabilidad |
|---|---|
| `Program.cs` | Argumentos, elección entre ejecución normal y benchmark, coordinación y resumen por consola. |
| `InputReader.cs` | Lectura CSV/XLSX, búsqueda de encabezados y extracción de filas. |
| `HistoryReader.cs` | Carga de CI y niveles aprobados anteriores. |
| `ConsolidationService.cs` | Normalización de inscripciones, claves, deduplicación, validación y grafo BFS. |
| `Models.cs` | Filas de entrada, inscripción, incidencias y resultado de consolidación. |
| `OutputWriter.cs` | Escritura y orden de los dos CSV de salida. |
| `BenchmarkRunner.cs` | Comparación de la lista lenta con el diccionario. |

El flujo mantiene las filas y el consolidado en memoria. El costo esperado del proceso normal es O(n) para leer/validar/agrupar, O(h) para cargar `h` aprobaciones del historial, O(u(V + E)) para validar las rutas de `u` matrículas y O(u log u) para ordenar esas matrículas antes de escribir. Como el grafo actual tiene un tamaño fijo (seis nodos y cinco aristas), el costo total esperado se simplifica a O(n + h + u log u); la memoria es O(n + h). Para las cargas previstas de H1, la mejora principal es la consistencia y la revisión explícita de problemas, además de evitar la búsqueda cuadrática de duplicados.
