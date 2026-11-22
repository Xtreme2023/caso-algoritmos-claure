Cristofer Johany Claure Cabrera

 Elección de estructuras

La solución ingenua usa una List<Pedido>. Registrar solamente agrega al final y tarda O(1) amortizado, pero PrepararSiguiente recorre toda la lista para hallar el menor plazo y el menor número de llegada, luego usa RemoveAt, ambas tareas del orden O(n). EstaPendiente recorre la lista hasta encontrar el nombre, también O(n). Por eso su costo crece muy rápido.

Elegí combinar una cola de prioridad con un conjunto hash:

- PriorityQueue<string, (int Plazo, int Llegada)>: conserva la pareja (plazo, llegada) como prioridad. La llegada es el orden secuencial en el que llegó el pedido, así que un mismo plazo resuelve el desempate correctamente.
- HashSet<string>: recuerda qué nombres siguen pendientes, sin guardar Pedido repetido.

 Operaciones

| Operación | Veces por jornada | Estructura usada | Costo promedio por operación |
|---|---:|---|---|
| Registrar(pedido, plazoMinutos) | 120.000 | PriorityQueue.Enqueue y HashSet.Add | O(log n) por el heap; el conjunto es O(1) promedio |
| PrepararSiguiente() | 120.000 | PriorityQueue.TryDequeue y HashSet.Remove | O(log n) por el heap; la eliminación del conjunto es O(1) promedio |
| EstaPendiente(pedido) | 40.000 | HashSet.Contains | O(1) promedio |
| Pendientes | Consultas puntuales | HashSet.Count | O(1) |

Las operaciones de prioridad están ligadas a n pedidos pendientes. El costo promedio declarado de la solución mejorada es O(log n) para registrar y preparar, O(1) promedio para pertenencia y cantidad.

 Comparación aproximada con n = 120.000

Para la lista simple, preparar cada pedido cuesta alrededor de buscar en el tamaño actual más quitar con RemoveAt. En el peor caso, cada preparación combina búsqueda y desplazamiento; el promedio de búsqueda y desplazamiento es aproximadamente n/2 durante la secuencia. Esto da alrededor de:

- Comparaciones de búsqueda: n(n-1)/2 = 120.000 · 119.999 / 2, aproximadamente 7.2 × 10^9.
- Desplazamientos de RemoveAt: del mismo orden aproximado, alrededor de 7.2 × 10^9.
- Consultas EstaPendiente: con 40.000 consultas y una lista promedio de tamaño cercano a n/2, alrededor de 40.000 · 60.000 = 2.4 × 10^9 visitas.

En conjunto, la jornada ingenua ronda decenas de miles de millones de operaciones elementales en el peor caso promedio de esta carga.

Con el diseño mejorado, log2(120.000) ≈ 17. Registrar y preparar hacen ambos una operación de heap:

- Aproximadamente 120.000 × 17 = 2.04 × 10^6 pasos para registros.
- Aproximadamente 120.000 × 17 = 2.04 × 10^6 pasos para preparaciones.
- Las operaciones hash de registro, extracción y consulta suman aproximadamente cientos de miles de operaciones promedio.

Por tanto, se espera aproximadamente 4 × 10^6 ajustes de heap más las operaciones hash para la carga completa, frente a decenas de miles de millones de recorridos/desplazamientos con la lista.

Cambiaría de opinión si la jornada priorizara mucho más las consultas EstaPendiente y casi no registrara ni preparara, porque entonces el HashSet bastaría y una cola de prioridad sería innecesaria. También cambiaría en entradas muy pequeñas: el alto uso de instrucciones no cambia mucho el resultado y una lista simple puede ser suficiente. Además, si el tamaño fuera enorme pero las preparaciones fueran pocas, el costo de mantener el heap podría no compensar frente a una estructura adaptada a la operación dominante.

La regla aplicada es elegir por la operación más frecuente: la jornada tiene 120.000 registros y 120.000 preparaciones, por eso la estructura prioritaria es el heap. Se combina con el HashSet porque las 40.000 consultas de pertenencia requieren evitar recorridos lineales.
