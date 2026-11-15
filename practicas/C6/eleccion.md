C6 — Elección de estructura para consultar carnets

Proceso y datos

El proceso de secretaría consolida las planillas de notas y asistencia y luego
responde consultas del tipo «¿está registrado este carnet?». La operación que
más pesa es  buscar: se hacen 10.000 consultas sobre una colección que no
viene ordenada.

La muestra disponible en [`H1/notas_2026_M2.csv`](../../H1/notas_2026_M2.csv)
tiene 305 inscripciones y 150 carnets distintos. Todas sus filas pertenecen a
`2026-M2`, por lo que el peor mes observado en la muestra también tiene 305
inscripciones y 150 carnets. Es una muestra pequeña del proceso; para el diseño
se exige la escala objetivo de **1.000.000 de carnets y 10.000 consultas**.

| Escenario | Inscripciones | Carnets distintos | Consultas |
|---|---:|---:|---:|
| Muestra actual (`2026-M2`) | 305 | 150 | — |
| Peor mes observado | 305 | 150 | — |
| Escala de diseño | — | 1.000.000 | 10.000 |

 Candidatos

| Candidato | Estrategia | Preparación | Peor caso por consulta |
|---|---|---:|---:|
| A | Recorrer la lista uno por uno | `O(1)` | `O(n)` |
| B | Ordenar una vez y buscar por posición media | `O(n log n)` | `O(log n)` |
| C | Construir un `HashSet` de carnets | `O(n)` esperado | `O(1)` esperado |

En A, una consulta puede revisar los 1.000.000 de elementos antes de encontrar
el carnet o confirmar que no existe. B paga el ordenamiento una sola vez y
después descarta la mitad del intervalo en cada paso. C usa la tabla hash para
ir directamente al cubículo asociado al carnet; el `O(1)` indicado es el costo
esperado habitual de `HashSet`, suponiendo una buena distribución de hashes.

 Elección

Elijo  C (`HashSet<string>`) porque la pregunta actual solo necesita saber
si el carnet está registrado. Con un millón de elementos y 10.000 consultas,
evita repetir recorridos lineales y no necesita conservar un orden.

| Necesidad | Elección |
|---|---|
| Solo comprobar existencia | C: `HashSet` |
| Comprobar existencia y pedir rangos o carnets ordenados | **B: lista ordenada + búsqueda binaria** |

Si más adelante se solicitan rangos (por ejemplo, todos los carnets entre dos
valores), cambiaré a B: la lista ordenada permite buscar los dos límites y
recorrer únicamente el intervalo. Para la consulta puntual de este módulo, el
costo de ordenar no aporta una ventaja frente a C.

Medición del duelo

Se compararon A y C con los mismos datos, sin orden garantizado, y con 10.000
consultas (mitad existentes y mitad inexistentes). Se calentó el JIT, se
ejecutaron tres corridas y se informó el promedio. La consulta se repitió sobre
una copia de la colección y se verificó que ambas estrategias devolvieran el
mismo número de coincidencias.

| Tamaño `n` | Consultas `q` | Recorrido A | `HashSet` C |
|---:|---:|---:|---:|
| 1.000.000 | 10.000 | ≈ 1 s | ≈ 1 ms |

El duelo evidencia la diferencia de crecimiento: A puede hacer hasta
`n × q = 10.000.000.000` comparaciones, mientras C realiza aproximadamente
`q` búsquedas esperadas, después de construir el conjunto una vez. Los tiempos
son aproximados y dependen del equipo; el protocolo, los tamaños y la cantidad
de consultas son los datos que deben mantenerse constantes para repetir la
comparación.

 Cómo medir y escalar

1. Usar n   10.000, 100.000, 1.000.000 carnets y mantener  q = 10.000`.
2. Generar los carnets con una semilla fija y mezclar la lista para no asumir
   ningún orden.
3. Preparar A y C fuera del cronómetro de consultas; medir aparte la
   construcción del `HashSet`.
4. Calentar el programa y descartar la primera corrida por el JIT.
5. Ejecutar tres corridas por tamaño y reportar promedio y unidad (ms).
6. Alternar el orden de A y C entre corridas para reducir el efecto de caché.
7. Validar que ambas estrategias entreguen el mismo total de encontrados.

La métrica principal es el tiempo total de las 10.000 consultas; como métrica
secundaria se registra el tiempo de preparación. Así se distingue el costo
único de construir la estructura del costo repetido de buscar.
