# C6 — Recurrencias y forma de crecimiento

Para una recurrencia de la forma

text
T(n) = a T(n / b) + Θ(n^d)


`a` es el número de subproblemas, `b` indica cuánto se reduce el tamaño de
cada subproblema y `d` es el exponente del trabajo adicional. La comparación
entre `a` y `b^d` permite predecir la forma dominante de `T(n)`.

| Recurrencia | `a` | `b` | `d` | `b^d` | Forma predicha | `T(n) / forma predicha`<br>`2^10` | `2^15` | `2^20` |
|---|---:|---:|---:|---:|---|---:|---:|---:|
| Búsqueda binaria | 1 | 2 | 0 | 1 | `log n` | 1,10 | 1,07 | 1,05 |
| MergeSort | 2 | 2 | 1 | 2 | `n log n` | 1,10 | 1,07 | 1,05 |
| 4 mitades + `n` | 4 | 2 | 1 | 2 | `n^2` | 2,00 | 2,00 | 2,00 |
| 3 mitades + `n` | 3 | 2 | 1 | 2 | `n^1,585` | 2,97 | 3,00 | 3,00 |

## Lectura de los casos

- Búsqueda binaria:** `T(n) = T(n/2) + Θ(1)`. Solo queda un subproblema
  cuyo tamaño se reduce a la mitad, por eso el costo es `Θ(log n)`.
- MergeSort:** `T(n) = 2T(n/2) + Θ(n)`. Se procesan dos mitades y luego se
  hace una mezcla lineal, dando `Θ(n log n)`.
- 4 mitades + `n`:** `T(n) = 4T(n/2) + Θ(n)`. El trabajo recursivo domina:
  `n^(log₂ 4) = n²`.
- 3 mitades + `n`:** `T(n) = 3T(n/2) + Θ(n)`. El exponente es
  `log₂ 3 ≈ 1,585`, por lo que `T(n) = Θ(n^1,585)`.

## Protocolo de comprobación

Se usan tamaños `n = 2^10`, `2^15` y `2^20`, porque son potencias de dos y
evitan redondeos al dividir por dos. Para cada caso se calcula el tiempo de
ejecución y se divide entre la forma predicha. Si el cociente se mantiene
aproximadamente constante al crecer `n`, los datos respaldan la forma
asintótica indicada. La primera corrida se descarta por el calentamiento del
JIT y se informa el promedio de tres corridas posteriores.

Los cocientes de la tabla son evidencia empírica del crecimiento, no una
igualdad exacta: constantes de implementación, asignaciones y el hardware
pueden cambiar su valor sin cambiar la clase Big-O.
