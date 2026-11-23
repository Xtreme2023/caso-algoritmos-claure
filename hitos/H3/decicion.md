# Hito H3 - Estrategia voraz o dinamica

## 1. Decision repetida del caso

En el caso de los pedidos de la pasteleria, cada vez que se prepara un pedido se debe elegir el siguiente elemento entre los pedidos pendientes.

La regla es:

1. elegir el plazo menor;
2. si varios pedidos tienen el mismo plazo, elegir el que llego primero.

El resultado que se quiere optimizar es el tiempo de la operacion completa de registrar y preparar una jornada de pedidos. Mas especificamente, se busca mantener la siguiente preparacion rapida y procesar todos los pedidos en el orden correcto.

## 2. Aplicacion voraz

La seleccion de la mejor opcion disponible en cada momento es suficiente:

- primero se elige el pedido urgente;
- luego el conjunto cambia;
- se repite la misma eleccion sobre los pedidos restantes.

Por tanto, esta es una solucion voraz. No es necesario mirar combinaciones futuras ni calcular una solucion global para decidir el siguiente pedido.

La prueba por intercambio es directa: si dos pedidos pendientes tienen plazos distintos y el de mayor plazo se prepara antes que el de menor plazo, intercambiarlos invierte el orden incorrecto y coloca primero al pedido mas urgente. El arreglo completo puede ordenarse repitiendo este razonamiento por plazo y, en caso de empate, por numero de llegada.

La estructura elegida fue una `PriorityQueue<string, long>` para extraer el minimo con costo $O(\log n)$ y un `HashSet<string>` para comprobar si un pedido sigue pendiente en $O(1)$ promedio.

| Operacion | Implementacion ingenua | Implementacion voraz |
|---|---:|---:|
| Registrar | $O(1)$ | $O(\log n)$ |
| Preparar siguiente | $O(n)$ | $O(\log n)$ |
| Saber si esta pendiente | $O(n)$ | $O(1)$ promedio |

## 3. Programacion dinamica

No se necesita programacion dinamica en este caso. Preparar un pedido no crea subproblemas que deban resolverse varias veces: eliminar el minimo deja una nueva instancia formada por el conjunto reducido y la propiedad optima sigue siendo elegir el minimo en ese conjunto.

La prioridad ya determina la decision, por lo que no existe un conjunto de estados solapados que convenga almacenar. Tampoco hay una funcion de valor del tipo `dp[i][j]` que mejore el resultado global.

Si el objetivo fuera, por ejemplo, elegir pedidos con limites de tiempo y capacidad para obtener el beneficio maximo sin exceder una capacidad, entonces si seria necesario considerar subconjuntos solapados y usar programacion dinamica o knapsack. Ese problema no es el comportamiento real de la pasteleria descrito en el caso.

## 4. Comparacion medida

Se utilizo la carga ya medida en `parcial2/Program.cs`: primero llegan `n` pedidos y despues se preparan todos. Las mediciones fueron tres corridas por tamano con `Stopwatch`, despues de un calentamiento.

| Tamano | Lista ingenua - promedio | Cola voraz - promedio |
|---:|---:|---:|
| 5.000 | 315,790 ms | 7,218 ms |
| 10.000 | 1.317,088 ms | 3,385 ms |
| 20.000 | 5.181,585 ms | 7,337 ms |

La solucion ingenua crecio aproximadamente 4,17 veces al pasar de 5.000 a 10.000 pedidos y 3,93 veces al pasar de 10.000 a 20.000. Esto concuerda con su costo cuadratico esperado.

La solucion voraz permanecio alrededor de milisegundos y el factor entre 10.000 y 20.000 fue aproximadamente 2,17 en esa ejecucion, cercano al crecimiento $n \log n$ esperado. El primer salto es mas ruidoso por el entorno de ejecucion y el mecanismo JIT.

Para el tamano final medido, la cola voraz fue aproximadamente 706 veces mas rapida que la lista ingenua:

$$
\frac{5181,585}{7,337} \approx 706
$$

La medicion completa y su protocolo estan en [medicion.md](../../parcial2/medicion.md).

## 5. Conclusion

Para este caso corresponde una estrategia voraz. Elegir el pedido con menor plazo, resolviendo empates por orden de llegada, produce el siguiente pedido esperado sin explorar alternativas ni resolver subproblemas repetidos. La cola de prioridad con el conjunto hash mantiene esta decision eficiente y la medicion evidencia la diferencia frente a la seleccion lineal.
