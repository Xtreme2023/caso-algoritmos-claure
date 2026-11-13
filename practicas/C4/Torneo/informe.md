# Informe – Torneo de ordenamiento (C#)

Fecha: 2026-11-12 23:45
Protocolo: calentamiento JIT · 3 corridas · promedio · datos aleatorios con semilla fija.
Verificación: cada resultado se comparó contra `Array.Sort` antes de aceptar el tiempo.

## Tabla de tiempos (ms)

| Algoritmo              |     10.000 |     20.000 |     40.000 |     80.000 |
|------------------------|------------|------------|------------|------------|
| Burbuja (O(n²))        |  146,07 ms |  641,56 ms | 2877,58 ms | 11213,72 ms |
| Inserción (O(n²))      |   19,65 ms |   73,69 ms |  328,06 ms | 1153,56 ms |
| MergeSort (n log n)    |    3,10 ms |    2,45 ms |    5,20 ms |    8,65 ms |
| QuickSort (n log n)    |    2,74 ms |    4,34 ms |    4,02 ms |    6,76 ms |
| Array.Sort (nativo)    |    1,36 ms |    1,21 ms |    3,50 ms |    6,26 ms |

## Conclusiones

- Con N = 80.000, el más rápido fue **Array.Sort (nativo)** (6,26 ms) y el más lento **Burbuja (O(n²))** (11213,72 ms): una diferencia de ~1792 veces.
- Al duplicar N (de 40.000 a 80.000), Inserción multiplicó su tiempo por ~3,5 (esperado ≈ 4 por ser O(n²)), mientras que MergeSort lo hizo por ~1,7 (esperado ≈ 2 por ser O(n log n)).
- Burbuja es consistentemente peor que Inserción porque hace muchos más intercambios.
- `Array.Sort` (introsort optimizado) suele ganar a MergeSort/QuickSort propios por optimizaciones internas.
- Conclusión: los cuadráticos solo sirven para N pequeños; el costo crece demasiado rápido al escalar.
