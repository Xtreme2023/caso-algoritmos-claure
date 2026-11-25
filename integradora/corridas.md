# Cristofer Johany Claure Cabrera

# Registro de corridas — Variante A

## Pruebas funcionales

Resultado de la ejecución: `PlanificadorIngenuo` pasó 8/8 y `PlanificadorMejorado` pasó 8/8. Cada planificación coincide con la ganancia y los códigos esperados; también se verificaron las dos consultas de pertenencia.

| Prueba | Capacidad | Esperado | Ingenuo | Mejorado |
|---|---:|---|---|---|
| El voraz se equivoca | 50 kg | 220 Bs: P2, P3 | Pasa | Pasa |
| Cuatro paquetes | 8 kg | 10 Bs: P2, P4 | Pasa | Pasa |
| Capacidad cero | 0 kg | 0 Bs: ninguno | Pasa | Pasa |
| Sin paquetes | 30 kg | 0 Bs: ninguno | Pasa | Pasa |
| Nada cabe | 30 kg | 0 Bs: ninguno | Pasa | Pasa |
| Entran todos | 100 kg | 26 Bs: P1, P2, P3 | Pasa | Pasa |
| `EstaEnElViaje("P2")` | — | `true` | Pasa | Pasa |
| `EstaEnElViaje("P1")` | — | `false` | Pasa | Pasa |

## Medición con `EjecutarCarga`

Ejecución con `dotnet run --project examen-int-A.csproj -c Release`, desde `integradora`. La carga es determinista por semilla `n`; la capacidad es un tercio del peso total. Cada fila contiene una corrida de calentamiento (no incluida en el promedio) y las tres corridas medidas. Los tiempos están en milisegundos.

### Planificador ingenuo

| n | Ganancia | Calentamiento | Medida 1 | Medida 2 | Medida 3 | Promedio | Factor vs. fila anterior |
|---:|---:|---:|---:|---:|---:|---:|---:|
| 18 | 511 | 22.653 | 17.808 | 18.108 | 19.023 | 18.313 | — |
| 20 | 520 | 79.663 | 73.403 | 73.461 | 75.667 | 74.177 | x4.05 |
| 22 | 743 | 318.245 | 535.684 | 316.277 | 324.061 | 392.007 | x5.28 |
| 24 | 1,040 | 1,338.681 | 1,326.095 | 1,366.438 | 1,336.404 | 1,342.979 | x3.43 |

### Planificador mejorado

| n | Ganancia | Calentamiento | Medida 1 | Medida 2 | Medida 3 | Promedio | Factor vs. fila anterior |
|---:|---:|---:|---:|---:|---:|---:|---:|
| 18 | 511 | 3.117 | 2.558 | 0.097 | 0.067 | 0.907 | — |
| 20 | 520 | 0.180 | 0.090 | 0.084 | 0.084 | 0.086 | x0.09 |
| 22 | 743 | 0.164 | 0.128 | 0.104 | 0.103 | 0.112 | x1.30 |
| 24 | 1,040 | 0.214 | 0.144 | 0.155 | 0.115 | 0.138 | x1.23 |

Las ganancias coinciden entre ambos planificadores en los cuatro tamaños. La primera corrida medida del mejorado con `n=18` fue mucho más lenta que las siguientes; se conserva en la tabla para mostrar el ruido real, no se descarta como atípica.

## 1.000 paquetes

Se usaron los mismos paquetes generados con semilla `1000`. En cada fila se muestra el calentamiento, las tres corridas medidas, su promedio y la ganancia devuelta.

| Capacidad | Calentamiento (ms) | Medida 1 (ms) | Medida 2 (ms) | Medida 3 (ms) | Promedio (ms) | Ganancia |
|---:|---:|---:|---:|---:|---:|---:|
| 10.000 kg | 60.786 | 58.589 | 79.057 | 80.088 | 72.578 | 32,197 Bs |
| 1.000.000.000 kg | 70.002 | 66.401 | 63.908 | 61.348 | 63.886 | 53,389 Bs |
