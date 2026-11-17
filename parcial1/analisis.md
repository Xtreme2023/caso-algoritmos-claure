Cristofer Johany Claure Cabrera
a) Los cuatro fragmentos
F1: Costo O(n). justificación: Hay un solo bucle que realiza n iteraciones, ejecutando una operación de tiempo constante.

F2: Costo O(n²). jstificación: Hay dos bucles anidados dependientes. En el peor caso forman una progresión aritmética que suma n(n+1)/2 iteraciones.

F3: Costo O(log n).justificación: El valor de la variable k se divide a la mitad en cada iteración, reduciendo el problema de manera logarítmica.

F4: Costo O(n log n). justificación: El bucle externo se ejecuta n veces y, en cada iteración, el bucle interno multiplica k por 2, realizando O(log n) operaciones.
b) Las recurrencias
1. T(n) = 3T(n/2) + n
a = 3, b = 2, d = 1.
Comparación: a > b^d (3 > 2 elevado a la 1).
Resultado: O(n^log₂(3)).
Aproximadamente: O(n^1.58).
2. T(n) = T(n/2) + nelevaso a potencia 2
 a = 1, b = 2, d = 2.
Comparación: a < b^d (1 < 2n elevado a potencia 2).
 Resultado: O(n²).
