# Hito H4 · BFS o DFS

## 1. Datos utilizados

Se analizaron los datos de [notas_2026_M2.csv](../H1/notas_2026_M2.csv).

- 305 registros (inscripciones).
- 150 estudiantes únicos identificados por `ci`.
- Promedio de 2,03 inscripciones por estudiante.
- 11 materias: A1, A2, B1, B2, C1, C2, G1, G2, G3, E1 y E2.
- 106 estudiantes aparecen con más de una inscripción.
- Los registros provienen de 7 departamentos.

El CSV no contiene explícitamente una red de relaciones entre estudiantes o materias. Sin embargo, sí contiene los elementos necesarios para construir una representación en grafo.

## 2. ¿El caso se puede modelar como grafo?

**Sí.** Para la promoción académica se puede modelar el plan de niveles como un **grafo dirigido**.

### Nodos

Cada nodo representa una situación académica identificable:

- `A1`, `A2`, `B1`, `B2`, `C1` y `C2`: niveles de miembros.
- `G1`, `G2` y `G3`: cursos generales.
- `E1` y `E2`: cursos abiertos externos.
- Opcionalmente, se puede usar un nodo por estudiante y nivel, por ejemplo `CI 6196645 - A2`.

### Aristas

Una arista dirigida representa un requisito o una progresión:

- `A1 -> A2`
- `A2 -> B1`
- `B1 -> B2`
- `B2 -> C1`
- `C1 -> C2`

La arista `A2 -> B1` significa que el nivel anterior debe estar aprobado antes de avanzar a B1.

Cada arista puede guardar como datos asociados:

- el `ci` del estudiante;
- la nota final;
- el porcentaje de asistencia;
- si el avance está aprobado o rechazado.

También existe un grafo bipartito alternativo:

- un conjunto de nodos representa a los estudiantes;
- el otro conjunto representa las materias;
- una arista `estudiante -> materia` representa una inscripción en el módulo.

Este segundo modelo es útil para detectar duplicados o consultar todas las materias de un estudiante. El modelo dirigido de niveles es el más adecuado para decidir promoción.

## 3. Elección: BFS o DFS

Elegimos **BFS (Breadth-First Search)** como recorrido principal.

BFS responde de forma natural si un nivel puede alcanzarse desde el nivel actual, una pregunta frecuente en la promoción. Por ejemplo:

```text
A1 -> A2 -> B1 -> B2 -> C1 -> C2
```

Al visitar el grafo por niveles de distancia, BFS permite obtener:

1. si existe una ruta hacia el nivel solicitado;
2. el número mínimo de pasos entre niveles;
3. los niveles alcanzables antes de una promoción;
4. el primer nivel bloqueado por prerrequisitos no aprobados.

La complejidad es \(O(V + E)\) por recorrido, donde `V` es la cantidad de nodos y `E` la cantidad de aristas.

**DFS** no se descarta: puede utilizarse como recorrido complementario para:

- recorrer profundamente una cadena de promoción;
- validar o detectar ciclos en el plan académico;
- realizar una comprobación previa de prerequisitos.

Sin embargo, DFS no identifica directamente la ruta mínima y puede recorrer primero una rama que no sea la promoción más corta. Por eso BFS es la elección principal para este caso.

## 4. Ejemplo de decisión

Supóngase que un estudiante aprobó A1 y desea matricularse en B1.

```text
Inicio: A1
A2 no aprobado
```

La ruta `A1 -> A2 -> B1` no debe considerarse disponible hasta que exista evidencia de aprobación de A2. Un recorrido BFS puede detenerse en A2 y mostrar que B1 no es alcanzable en ese momento.

La regla puede expresarse con los datos reales:

- nota final mayor o igual a 51;
- asistencia mayor o igual a 80 %;
- aprobación del nivel anterior correspondiente.

## 5. Conclusión

El caso sí puede representarse como grafo. Los nodos son niveles académicos o pares estudiante-nivel, y las aristas representan prerequisitos, promoción o inscripción.

**BFS** es el recorrido recomendado para comprobar alcanzabilidad y encontrar la secuencia mínima de promociones. **DFS** puede emplearse como algoritmo secundario para detectar ciclos o recorrer por profundidad todo el plan académico.
