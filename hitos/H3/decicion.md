# Hito H3 - Voraz, dinamica o no aplica

## 1. Caso real: notas 2026-M2

La decision y el resultado se toman sobre el archivo [notas_2026_M2.csv](../H2/notas_2026_M2.csv). El archivo contiene 250 registros reales de prueba, todos con un CI unico, y 156 estudiantes tienen estado `APROBADO`.

Las operaciones principales del caso son buscar por `ci`, consultar rangos de carnet o nota y recorrer los registros en orden. Estas operaciones se repiten, pero no eligen entre varias soluciones parciales para optimizar una cantidad global.

Los resultados principales son independientes:

- un estudiante aprueba si su nota final es al menos 51 y su asistencia es al menos 80%;
- un registro puede consultarse por `ci`;
- un conjunto de registros puede recorrerse o filtrarse por rango.

Cada registro puede clasificarse con estas comparaciones sin decidir primero otra opcion que cambie el resto del conjunto.

## 2. ¿Voraz o dinamica?

No aplica una estrategia voraz ni una estrategia de programacion dinamica al problema principal de consolidacion.

No es voraz porque no hay una seleccion local cuyo resultado optimice un conjunto final. Por ejemplo, considerar primero al estudiante con mayor nota no determina si el siguiente recibe una promocion valida; cada estado se calcula por sus propios umbrales.

No es programacion dinamica porque no aparecen subproblemas repetidos ni decisiones dependientes de una capacidad o de un estado anterior. Si se revisan los datos con un recorrido, cada estudiante se procesa una sola vez con resultados independientes. Tampoco hace falta guardar soluciones parciales como `dp[i][j]`.

La estructura adecuada para el caso sigue siendo el acceso por clave y el recorrido ordenado, no una rutina voraz o dinamica. En el hito anterior se compara una lista lineal contra un `SortedDictionary` y se verifico que ambas producen el mismo resultado.

## 3. Problema analogo medido

Para comparar las dos familias se usa una version analogica con los 156 registros aprobados del CSV.

**Problema:** elegir registros con capacidad total de 8.000 puntos de asistencia. El peso de cada registro es `asistencia_pct` y el valor es `nota_final * 10`. Se busca maximizar la suma de valores sin superar la capacidad.

Este problema es analogo al negocio solo como ejercicio de seleccion: usa datos reales del mismo archivo, pero no afirma que la secretaria tenga una capacidad de asistencia de 8.000 puntos.

| Estrategia | Resultado medido | Tiempo medido |
|---|---:|---:|
| Voraz: mayor cociente nota/peso | 7.181,5 puntos | 0,204 ms promedio |
| Dinamica: mochila 0/1 exacta | 7.203,5 puntos | 488,321 ms promedio |

El voraz recorre los registros ordenados por cociente nota/peso y agrega uno si cabe en la capacidad. En esta carga obtuvo 7.181,5 puntos.

La dinamica usa estados `dp[c]` para la mejor suma que cabe en cada capacidad `c`. Revisa 156 registros y 8.001 capacidades, por lo que hay subproblemas que se reutilizan y el resultado exacto fue 7.203,5 puntos.

La diferencia fue de 22 puntos, y la carga demostro que el voraz puede ser mas rapido pero no garantiza el optimo en un problema de mochila 0/1.

## 4. Conclusion

Para `notas_2026_M2.csv`, el proceso principal no requiere voraz ni programacion dinamica. La clasificacion es directa y las consultas deben optimizarse con estructuras de datos adecuadas.

La comparacion análoga muestra la diferencia real: el voraz falla la solucion exacta en este problema de mochila, mientras que la dinamica explora estados repetidos y alcanza el optimo. Por tanto, el metodo debe elegirse segun el problema concreto y no sobre la presencia de un ciclo o una cantidad grande de datos.
