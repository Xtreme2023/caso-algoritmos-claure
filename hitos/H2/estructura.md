# Hito H2 · Elección de Estructura de Datos

## 1. Contexto del Caso

- **Descripción del caso:** Sistema de consolidación de calificaciones y promoción de nivel académico de estudiantes (Módulo 2026-M2). La secretaría académica consolida las planillas departamentales de notas y asistencias, debiendo responder consultas continuas sobre la situación de cada estudiante, filtrar reportes por rangos (ej. notas de aprobación o lotes de carnets) y generar actas oficiales ordenadas correlativamente.
- **Clave de búsqueda:** `ci` (Carnet de Identidad del estudiante, número entero único).
- **Campos de cada registro:** `ci`, `nombre_completo`, `departamento`, `tipo`, `nota_final`, `asistencia_pct`, `estado`, `nivel_actual`.
- **Frecuencia relativa de operaciones:**
  - **Buscar por clave (`ci`):** Muy frecuente (consultas continuas de secretaría, coordinadores departamentales y estudiantes).
  - **Consulta por rango `[a, b]`:** Muy frecuente (filtrado de notas aprobatorias `[51, 100]`, preselección de becas y división de actas por intervalos de carnet).
  - **Recorrer en orden:** Frecuente (emisión del padrón oficial y libro de calificaciones ordenado ascendentemente por carnet).
  - **Insertar:** Media (incorporación de nuevos registros o inscripciones extemporáneas).
  - **Menor / siguiente (sucesor):** Poco (navegación correlativa al siguiente estudiante en el acta).
  - **Borrar:** Poco (anulaciones de matrícula o rectificaciones extraordinarias).
- **Operaciones principales (las 3 que más realizo):**
  1. Buscar por clave (`ci`)
  2. Consulta por rango `[a, b]`
  3. Recorrer en orden

---

## 2. Estructura Elegida

Para este caso se eligió un **Árbol Binario de Búsqueda Balanceado (Árbol AVL / Red-Black Tree)**, implementado en .NET a través de la colección genérica `SortedDictionary<int, RegistroEstudiante>` (la cual mantiene un árbol binario balanceado en memoria ordenado por la clave `ci`).

### Tabla de Costo Teórico $O$ de las Operaciones Principales

| Operación | Costo $O$ Promedio | Costo $O$ Peor Caso | Por qué |
|---|:---:|:---:|---|
| **Buscar por clave (`ci`)** | $O(\log n)$ | $O(\log n)$ | Gracias a las rotaciones y al factor de balanceo estricto, la altura del árbol nunca supera $c \cdot \log_2 n$. En cada comparación con un nodo se descarta una rama completa (aproximadamente la mitad de las claves restantes). |
| **Consulta por rango `[a, b]`** | $O(\log n + k)$ | $O(\log n + k)$ | Desciende por el árbol en $O(\log n)$ hasta ubicar la primera clave $\ge a$. A partir de allí recorre secuencialmente en inorden únicamente los $k$ nodos que caen dentro del intervalo $[a, b]$, sin necesidad de visitar ni procesar los $n - k$ elementos restantes. |
| **Recorrer en orden** | $O(n)$ | $O(n)$ | El recorrido inorden (subárbol izquierdo $\rightarrow$ nodo actual $\rightarrow$ subárbol derecho) visita cada uno de los $n$ nodos exactamente una vez, produciendo las claves ordenadas de menor a mayor sin requerir ningún algoritmo adicional de ordenamiento externo. |

---

## 3. Por qué elegí esta estructura

Elegí el **Árbol Binario de Búsqueda Balanceado** porque ofrece un equilibrio óptimo para el conjunto completo de operaciones del sistema académico, a diferencia de las estructuras unidimensionales.

La principal **alternativa descartada fue la Tabla Hash (`Dictionary<int, RegistroEstudiante>`)**: aunque una tabla hash proporciona una búsqueda por clave ligeramente más rápida en tiempo constante $O(1)$ promedio, su función de dispersión destruye totalmente el orden natural de los carnets; por lo tanto, responder una consulta de rango $[a, b]$ o generar el acta ordenada obligaría a la tabla hash a inspeccionar todos los casilleros ($O(n)$) o a volcar y ordenar todo el conjunto en $O(n \log n)$, lo cual degrada severamente el rendimiento en cierres de módulo masivos. 

Asimismo, se descartó la **Lista o Arreglo no ordenado (`List<RegistroEstudiante>`)**, ya que la búsqueda lineal y el filtrado por rango requieren recorrer elemento por elemento con costo $O(n)$ en cada consulta, volviéndose inviable al acumular miles de solicitudes. 

El árbol balanceado garantiza que la búsqueda puntual sea veloz ($O(\log n)$), que las consultas de rango visiten solo los registros pertinentes ($O(\log n + k)$) y que el padrón consolidado se imprima en orden correlativo en $O(n)$ sin sobrecostos de memoria ni reordenamiento.
