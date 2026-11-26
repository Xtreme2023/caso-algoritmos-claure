# Comparación de rendimiento: lista lineal frente a diccionario

## Qué se compara

Se mide la detección de matrículas repetidas usando las mismas claves normalizadas del consolidador:

- **Lenta:** buscar cada clave con `List.Contains` y agregarla si no aparece. Cada búsqueda recorre la lista existente; en el peor caso el costo total es O(n²).
- **Seleccionada:** consultar un `Dictionary` con `TryGetValue` y agregar la clave si no existe. El costo esperado total es O(n), con memoria adicional O(n).

Ambas alternativas deben producir el mismo número de claves únicas. El benchmark comprueba esa igualdad en cada tamaño antes de registrar el tiempo.

## Protocolo y datos

- Archivo base: `hitos/H1/notas_2026_M2.csv`.
- El archivo tiene 305 filas; 298 tienen los campos necesarios para consolidar. Esas filas producen 285 claves únicas y 13 repeticiones.
- Para observar cómo crecen los costos, se replican las claves válidas en lotes, agregando un identificador de lote. Se conserva la proporción de claves repetidas; los tamaños mayores son una **carga sintética**, no nuevas planillas reales.
- Compilación y ejecución en `Release`, .NET 9.
- Por cada tamaño y algoritmo: una ejecución de calentamiento/calibración y cinco repeticiones medidas. La tabla resume tres invocaciones independientes del benchmark (quince mediciones por celda), promediando los tiempos por invocación.
- Cada repetición medida agrupa llamadas hasta aproximar 40 ms y divide el tiempo por el número de llamadas; así se reportan milisegundos por operación y se reduce el ruido en los tamaños pequeños.
- El tiempo medido cubre únicamente la búsqueda y agrupación de claves. No incluye lectura de XLSX/CSV, normalización, cálculo de notas, validación de prerrequisitos ni escritura de resultados.

## Resultados

| Filas | Claves únicas | Lista O(n²), ms | Diccionario O(n) promedio, ms | Lista / diccionario |
|---:|---:|---:|---:|---:|
| 298 | 285 | 0,3998 | 0,0346 | 11,6x |
| 1.490 | 1.425 | 10,5493 | 0,0671 | 157,2x |
| 2.980 | 2.850 | 17,9311 | 0,1284 | 139,7x |
| 5.960 | 5.700 | 46,1672 | 0,3684 | 125,3x |
| 11.920 | 11.400 | 212,8116 | 0,8046 | 264,5x |

En todos los tamaños ambas alternativas devolvieron el mismo número de claves únicas. Los tiempos pueden variar entre ejecuciones por carga de CPU, memoria y recolección de basura; la tendencia de crecimiento es más relevante que una cifra aislada.

## Interpretación

En el tamaño real disponible la búsqueda con diccionario ya resulta más rápida, pero la muestra es pequeña y el tiempo total de la aplicación también incluye otras tareas. Al escalar las mismas claves, `List.Contains` crece mucho más deprisa que el acceso hash. Por eso el consolidador usa un diccionario para la deduplicación, y ordena una sola vez al generar el archivo final.

La ejecución normal no repite cinco veces la consolidación. Recorre las filas de entrada una vez para validarlas y agruparlas; después procesa el consolidado para revisiones y salida ordenada. Las cinco repeticiones corresponden exclusivamente a la medición del modo `--benchmark`.

El diccionario es la elección adecuada para esta operación, no un algoritmo universalmente óptimo para todo problema. Para recorridos ordenados y consultas por rango se debe usar o construir un índice ordenado según la carga de trabajo.

## Reproducir

Desde la raíz del repositorio:

```powershell
dotnet run --project hitos\H3\consolidador -- --benchmark "D:\des\caso-algoritmos-claure\hitos\H1"
```

La opción `--benchmark` solo compara el costo de deduplicación; no genera el consolidado ni sustituye la ejecución normal.
