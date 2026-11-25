# Cristofer Johany Claure Cabrera

# Análisis de algoritmos — Variante A

# Costo de PlanificadorIngenuo

Con `n` paquetes, el algoritmo recorre los `2^n` subconjuntos posibles. Para cada máscara revisa hasta `n` paquetes para sumar su peso y ganancia; por tanto, el tiempo es `Θ(n · 2^n)` (y también `O(n · 2^n)`). Usa `O(n)` memoria adicional para la lista de códigos elegidos. La máscara `long` también limita el uso correcto del algoritmo a instancias pequeñas.

# Recurrencia y teorema maestro

El teorema maestro de bolsillo se aplica a recurrencias de la forma `T(n) = aT(n/b) + f(n)`: el tamaño del subproblema se divide por un factor constante. En `T(n) = 2T(n-1) + 1`, el tamaño disminuye en uno, no se divide por `b`; por eso esa recurrencia no está en la forma requerida.

Al expandirla, `T(n) = 2^k T(n-k) + (2^k - 1)`. Con `k=n` se obtiene `T(n) = 2^n T(0) + 2^n - 1 = Θ(2^n)`, suponiendo costo constante en cada llamada. Esto corresponde a la versión recursiva que hace trabajo constante por nodo. El código iterativo de `PlanificadorIngenuo`, en cambio, revisa `n` paquetes por cada subconjunto, así que cuesta `Θ(n · 2^n)`.

# Voraz y estrategia elegida

El voraz por ganancia por kilo falla en el primer caso de las pruebas:

| Paquete | Peso | Ganancia | Ganancia/kg |
|---|---:|---:|---:|
| P1 | 10 kg | 60 Bs | 6 |
| P2 | 20 kg | 100 Bs | 5 |
| P3 | 30 kg | 120 Bs | 4 |

Con capacidad de `50 kg`, el voraz toma primero P1 y luego P2: suma `30 kg` y `160 Bs`; P3 ya no cabe. La combinación óptima es P2 + P3: pesa `50 kg` y gana `220 Bs`.

Elijo programación dinámica por ganancia total. Se repiten subproblemas: para los primeros `i` paquetes, distintas combinaciones pueden producir la misma ganancia `v`. Basta conservar para cada `(i, v)` el menor peso alcanzable. Sea `G` la suma de las ganancias positivas y `M_i(v)` ese peso mínimo:

```text
M_0(0) = 0; los demás estados son inalcanzables.
M_i(v) = min(M_(i-1)(v), M_(i-1)(v - ganancia_i) + kg_i)
```

La segunda alternativa solo existe si `v >= ganancia_i`. Al final se elige la mayor ganancia cuyo peso mínimo no supera la capacidad. Los estados de ganancia se recorren de mayor a menor para no reutilizar un paquete. El algoritmo es exacto porque una combinación con la misma ganancia y más peso nunca puede dar una solución mejor.

La complejidad es `O(n · G)` tiempo. La implementación conserva decisiones para reconstruir los códigos y usa `O(n · G)` memoria, además de `O(G)` para los pesos mínimos. Es pseudopolinomial: depende de la suma de ganancias, no directamente de la capacidad en kg.

# Consultas de pertenencia

`PlanificadorMejorado` guarda los códigos elegidos en un `HashSet<string>`. `EstaEnElViaje` consulta el conjunto en `O(1)` promedio (amortizado), incluso si se consulta miles de veces. Con una lista y `Contains`, cada consulta recorre hasta `n` códigos y cuesta `O(n)`.