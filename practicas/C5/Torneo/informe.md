# Informe – Torneo de ordenamiento (C#)

Fecha: 2026-11-12 23:55
Protocolo: calentamiento JIT · 3 corridas · promedio · datos aleatorios con semilla fija.
Verificación: cada resultado se comparó contra `Array.Sort` antes de aceptar el tiempo.

## Tabla de tiempos (ms)

| Algoritmo              |     10.000 |     20.000 |     40.000 |     80.000 |
|------------------------|------------|------------|------------|------------|
| Burbuja (O(n²))        |  144,72 ms |  639,13 ms | 2729,46 ms | 11049,11 ms |
| Inserción (O(n²))      |   19,18 ms |   73,40 ms |  290,30 ms | 1156,29 ms |
| MergeSort (n log n)    |    3,33 ms |    2,54 ms |    3,87 ms |    8,60 ms |
| QuickSort (n log n)    |    2,27 ms |    4,47 ms |    3,66 ms |    6,35 ms |
| Array.Sort (nativo)    |    1,55 ms |    1,24 ms |    3,38 ms |    5,72 ms |

## Conclusiones

- Con N = 80.000, el más rápido fue **Array.Sort (nativo)** (5,72 ms) y el más lento **Burbuja (O(n²))** (11049,11 ms): una diferencia de ~1932 veces.
- Al duplicar N (de 40.000 a 80.000), Inserción multiplicó su tiempo por ~4,0 (esperado ≈ 4 por ser O(n²)), mientras que MergeSort lo hizo por ~2,2 (esperado ≈ 2 por ser O(n log n)).
- Burbuja es consistentemente peor que Inserción porque hace muchos más intercambios.
- `Array.Sort` (introsort optimizado) suele ganar a MergeSort/QuickSort propios por optimizaciones internas.
- Conclusión: los cuadráticos solo sirven para N pequeños; el costo crece demasiado rápido al escalar.
