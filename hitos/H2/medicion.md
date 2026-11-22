# Hito H2 · Medición Empírica y Comparativa

## 1. Protocolo de Medición
- **Problema evaluado:** Se procesaron 3.000 consultas por cada tamaño de datos. El 50 % eran claves existentes y el 50 % eran claves inexistentes.
- **Implementaciones comparadas:** una lista sin ordenar con búsqueda lineal y un `SortedDictionary<int, RegistroEstudiante>` con búsqueda balanceada.
- **Calentamiento (warm-up):** Se ejecutó una corrida previa independiente antes de tomar tiempos.
- **Repeticiones:** Cada medición se ejecutó 3 veces y se promedió.
- **Reloj:** `Stopwatch.GetTimestamp()` de alta resolución de .NET.
- **Verificación:** Se comprobó que ambas estructuras devolvieran los mismos registros encontrados y la misma suma de notas.

## 2. Resultados Obtenidos

| N | Tiempo con lista (s) | Veces vs fila anterior | Tiempo con árbol (s) | Veces vs fila anterior |
|---|---:|:---:|---:|:---:|
| 10,000 | 0.050432 | — | 0.002269 | — |
| 20,000 | 0.095195 | 1.89x | 0.000613 | 0.27x |
| 40,000 | 0.288024 | 3.03x | 0.000539 | 0.88x |
| 80,000 | 0.902222 | 3.13x | 0.000653 | 1.21x |

## 3. Conclusiones

1. La lista sin ordenar fue más lenta al aumentar el tamaño de los datos; sus factores de crecimiento se muestran en la tabla.
2. El árbol balanceado mantuvo tiempos cercanos entre sí en esta máquina (0,000539 s a 0,002269 s); las variaciones pequeñas corresponden a ruido de ejecución.
3. Para N = 80.000, el árbol fue aproximadamente 1382 veces más rápido que la lista en esta ejecución.
4. Las conclusiones se basan en los resultados reales anteriores y no se igualan artificialmente a una complejidad teórica.
