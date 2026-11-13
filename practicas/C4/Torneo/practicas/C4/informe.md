# Informe – Torneo de ordenamiento (C#)

Fecha: 2026-11-12 23:40
Protocolo: calentamiento JIT · 3 corridas · promedio · datos aleatorios con semilla fija.
Verificación: cada resultado se comparó contra `Array.Sort` antes de aceptar el tiempo.

## Tabla de tiempos (ms)

| Algoritmo              |     10.000 |     20.000 |     40.000 |     80.000 |
|------------------------|------------|------------|------------|------------|
| Burbuja (O(n²))        |  142,80 ms |  641,64 ms | 2718,80 ms | 11069,83 ms |
| Inserción (O(n²))      |   19,03 ms |   90,13 ms |  289,48 ms | 1161,20 ms |
| MergeSort (n log n)    |    3,53 ms |    2,28 ms |    3,74 ms |    8,26 ms |
| QuickSort (n log n)    |    2,10 ms |    4,17 ms |    3,29 ms |    6,94 ms |
| Array.Sort (nativo)    |    1,59 ms |    1,48 ms |    2,48 ms |    5,82 ms |

## Conclusiones

- Con N = 80.000, el más rápido fue **Array.Sort (nativo)** (5,82 ms) y el más lento **Burbuja (O(n²))** (11069,83 ms): una diferencia de ~1902 veces.
- Al duplicar N (de 40.000 a 80.000), Inserción multiplicó su tiempo por ~4,0 (esperado ≈ 4 por ser O(n²)), mientras que MergeSort lo hizo por ~2,2 (esperado ≈ 2 por ser O(n log n)).
- Burbuja es consistentemente peor que Inserción porque hace muchos más intercambios.
- `Array.Sort` (introsort optimizado) suele ganar a MergeSort/QuickSort propios por optimizaciones internas.
- Conclusión: los cuadráticos solo sirven para N pequeños; el costo crece demasiado rápido al escalar.
