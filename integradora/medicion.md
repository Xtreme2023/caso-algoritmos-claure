# Cristofer Johany Claure Cabrera

# Mediciones — Variante A

Se ejecuto desde `integradora` con `dotnet run --project examen-int-A.csproj -c Release`. Para comparar los algoritmos usé `EjecutarCarga`, que crea la misma carga determinista con semilla `n` y fija la capacidad en un tercio del peso total. En cada tamaño hice una corrida de calentamiento y tres corridas medidas; la tabla muestra el promedio. Se verificó que ambas implementaciones devolvieran la misma ganancia. La creación de la carga está incluida porque forma parte de EjecutarCarga. El detalle del calentamiento y de cada repetición está en [corridas.md](./corridas.md).

Elegí `n=18`, así que los cuatro tamaños son `18, 20, 22, 24`. El ingenuo tardó `1.343 s` en el tamaño mayor, dentro del rango solicitado de `0.5 a 20 s`.

# Comparación de algoritmos

| Paquetes | Ganancia (ambos) | Ingenuo (ms) | Ingenuo / fila anterior | Mejorado (ms) | Mejorado / fila anterior |
|---:|---:|---:|---:|---:|---:|
| 18 | 511 | 18.313 | — | 0.907 | — |
| 20 | 520 | 74.177 | x4.05 | 0.086 | x0.09 |
| 22 | 743 | 392.007 | x5.28 | 0.112 | x1.30 |
| 24 | 1,040 | 1,342.979 | x3.43 | 0.138 | x1.23 |

El tiempo ingenuo crece aproximadamente por un factor de cuatro cada dos paquetes, como anticipa el crecimiento exponencial; las diferencias respecto a cuatro son normales en una medición real. La primera corrida medida de `PlanificadorMejorado` para n=18 fue mucho más lenta que las siguientes; esto muestra el ruido posible del sistema, especialmente en tiempos tan pequeños. En esta ejecución, para 24 paquetes el ingenuo tardó alrededor de 9,700 veces más.

# Prueba con 1.000 paquetes y capacidad 10.000 kg

Medí `PlanificadorMejorado` con los 1.000 paquetes generados por `CrearCarga(1000)` y capacidad fija de `10.000 kg`. Con una corrida de calentamiento y tres medidas promedió `72.578 ms`; la ganancia máxima fue `32,197 Bs`.

# Capacidad de 1.000.000.000 kg

Para los mismos paquetes, con capacidad de `1.000.000.000 kg`, el promedio fue `63.886 ms` y la ganancia `53,389 Bs`: todos los paquetes caben. La capacidad no aumenta el tamaño de la tabla porque esta estrategia indexa por ganancia total, no por kg. Para esta carga, la tabla de decisiones tiene unos 53.4 millones de celdas booleanas tanto con capacidad 10.000 como con capacidad mil millones. En una programación dinámica indexada por capacidad, en cambio, esa capacidad requeriría una tabla inviable.

# Conclusion

La programación dinamica entrega el óptimo sin enumerar los `2^n` subconjuntos y permite procesar 1.000 paquetes. Su costo es `O(n · G)`, donde `G` es la suma de ganancias positivas; por eso es pseudopolinomial, no eficiente para ganancias totales arbitrariamente grandes. Los tiempos dependen del equipo, el JIT, la recolección de basura y otros procesos, así que las tendencias son más informativas que una cifra aislada
