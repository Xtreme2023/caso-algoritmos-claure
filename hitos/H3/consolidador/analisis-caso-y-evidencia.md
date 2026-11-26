# Caso real, decisión y evidencia

## 1. El proceso real y quién lo sufre

Al cierre de cada módulo, los siete departamentos envían planillas de notas y asistencia con formatos distintos. La secretaría académica nacional las reúne manualmente, corrige diferencias, detecta estudiantes repetidos, recalcula resultados y comprueba los prerrequisitos.

El trabajo afecta directamente a:

- **La secretaría académica**, que invierte aproximadamente 25 horas y 3–4 días hábiles por cierre.
- **Los siete coordinadores departamentales**, que reciben solicitudes para corregir datos.
- **Alrededor de 150 estudiantes**, que esperan el resultado para inscribirse al siguiente módulo.

El volumen estimado por módulo es de unas 400 inscripciones, cerca de 1.200 notas y 4.800 registros de asistencia. Los errores que se quieren reducir incluyen duplicar estudiantes trasladados, calcular mal una nota, promover sin prerrequisito y publicar los resultados tarde.

## 2. La operación más repetida y cuántas veces

Para consolidar, la operación que se repite por cada inscripción válida es **buscar si ya existe la misma matrícula**, usando CI, materia y módulo como clave. El diccionario del consolidador hace una consulta por cada fila válida: en el archivo de H1 fueron 298 consultas; para el volumen esperado de un módulo serían aproximadamente 400. Si la clave ya existe, se comparan los datos para decidir si es una copia consistente o un conflicto que necesita revisión.

La alternativa lenta con `List.Contains` también consulta una vez por fila, pero cada consulta puede comparar la clave con muchas de las claves ya guardadas. En el peor caso, su trabajo total crece como O(n²); con un `Dictionary`, el costo esperado de las consultas crece como O(n).

## 3. Decisión y alternativa descartada

La solución recorre las filas, normaliza sus campos y agrupa matrículas en un `Dictionary` con clave compuesta. Luego calcula nota y asistencia, y usa un grafo de niveles más BFS para comprobar el avance de los miembros. Ordena el resultado solo al escribir el consolidado.

Se descarta buscar cada matrícula con una lista lineal porque repite comparaciones y escala peor. No se usa programación dinámica ni un algoritmo voraz: aquí no se busca maximizar una selección ni resolver subproblemas de optimización; se aplican reglas académicas explícitas.

## 4. Evidencia: fila de la tabla

La primera fila de la tabla de [comparación de rendimiento](./comparacion-rendimiento.md) corresponde a las 298 filas válidas de H1:

| Filas válidas | Claves únicas | Lista O(n²), ms | Diccionario O(n) promedio, ms | Lista / diccionario |
|---:|---:|---:|---:|---:|
| 298 | 285 | 0,3998 | 0,0346 | 11,6x |

En esa muestra hubo 13 claves repetidas (298 filas menos 285 claves únicas). El benchmark confirma que ambas alternativas encontraron la misma cantidad de claves únicas y que, para esa carga, el diccionario tardó menos en **la deduplicación**.

**Límite de esta evidencia:** el benchmark no mide lectura de Excel/CSV, revisión de datos, cálculo de resultados, publicación ni el proceso manual completo. Por eso el resultado de 11,6x no significa que el cierre completo vaya a tardar 11,6 veces menos ni demuestra todavía una reducción de 25 horas a minutos. Además, 400 inscripciones es una estimación operativa; H1 aportó 298 filas válidas para esta medición.

## 5. Lo que todavía no resuelve

- No determina automáticamente quién es una persona cuando falta el CI o el nombre está escrito en un orden distinto; deja el caso para confirmar.
- No elige cuál nota es correcta si dos planillas de la misma matrícula se contradicen; solicita revisión.
- No sustituye la aprobación de la secretaría antes de publicar resultados oficiales.
- No automatiza la recepción desde correo o WhatsApp ni cubre todavía inscripciones, certificados o consultas de notas.
- No se ha medido el tiempo total de un cierre real de principio a fin; las cifras de rendimiento disponibles son solo de deduplicación.

## Resumen: qué cambias, cuánto cuesta, qué NO tocas

1. **Qué cambias:** pasas de buscar, copiar, recalcular y validar planillas manualmente a ejecutar una consolidación que normaliza, agrupa, calcula y señala excepciones.
2. **Cuánto cuesta:** una búsqueda esperada O(1) por matrícula válida, es decir, O(n) para `n` matrículas; además, ordenar `u` resultados cuesta O(u log u), más las comprobaciones del grafo. En H1, la comparación de deduplicación fue 0,3998 ms con lista frente a 0,0346 ms con diccionario para 298 filas; esto no es una medición de extremo a extremo.
3. **Qué NO tocas:** no cambias los umbrales académicos acordados ni inventas una resolución automática para identidades o notas dudosas; tampoco afirmas que ya están automatizados el registro, los certificados o la consulta estudiantil.
