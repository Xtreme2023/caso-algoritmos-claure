 Eleccion
Elijo la  Opción 2  Ordenar n log n y usar búsqueda binaria. Su costo total de aproximadamente 5.5 millones de operaciones  es abismalmente menor que los 1.5 billones de la búsqueda lineal y los 90 billones del ordenamiento cuadrático
Al tener memoria limitada, puedo ordenar el arreglo original en su lugar  en la mañana una sola vez sin duplicarlo, y responder a las consultas eficientemente en O(log n) por el resto del día
 Cuando cambiaria de opinión
Cambiaria a la Opción 1 Búsqueda lineal sin ordenar si el costo total de las consultas a lo largo del día fuera menor que el costo de ordenar el arreglo una vez:
q × n < n × log en base 2*n
q < log en base 2*n
Dado que log en base 2 (300,000) ≈  19 , si el hospital pasara a realizar  menos de 19 consultas diarias , dejaría de compensar el esfuerzo de pre-ordenar el listado en la mañana.
