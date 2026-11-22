Cristofer Johany Claure Cabrera

 Medicion empírica

 Protocolo

Se eligio n = 5.000 porque la ingenua tardó alrededor de 5,18 segundos con 4n = 20.000, dentro del intervalo pedido de 0,5 a 20 segundos.

Se uso  EjecutarCarga en cada ejecución: primero llegan n pedidos y luego se preparan los n. Antes de las mediciones hice una corrida de calentamiento con cada receptor a tamaño 4n = 20.000. Después medí los tres tamaños n, 2n y 4n, y para cada receptor hice tres corridas con Stopwatch, mostrando milisegundos totales y su promedio.

 Resultados reales

 ReceptorIngenuo

| Tamaño | Corrida 1 (ms) | Corrida 2 (ms) | Corrida 3 (ms) | Promedio (ms) | Veces respecto de la fila anterior |
|---:|---:|---:|---:|---:|---:|
| 5.000 | 318,247 | 310,621 | 318,501 | 315,790 | — |
| 10.000 | 1388,349 | 1318,698 | 1244,219 | 1317,088 | 4,171 |
| 20.000 | 5308,775 | 5108,378 | 5127,601 | 5181,585 | 3,934 |

 ReceptorMejorado

| Tamaño | Corrida 1 (ms) | Corrida 2 (ms) | Corrida 3 (ms) | Promedio (ms) | Veces respecto de la fila anterior |
|---:|---:|---:|---:|---:|---:|
| 5.000 | 6,700 | 5,361 | 9,594 | 7,218 | — |
| 10.000 | 4,612 | 3,016 | 2,527 | 3,385 | 0,469 |
| 20.000 | 7,800 | 6,680 | 7,533 | 7,337 | 2,168 |

 Conclusion

La ingenua tiene crecimiento cuadrático: al duplicar el tamaño, sus promedios crecen aproximadamente en  4,17 y 3,93 veces, consistente con O(n^2). La mejorada crece como una carga O(n log n): el primer salto tiene mucho ruido y el segundo queda cerca de un factor 2,17. Importa la forma general del crecimiento; hay ruido de medición por el caché, la recolección de memoria y carga del sistema.
