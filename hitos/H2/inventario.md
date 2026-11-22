# Inventario de datos y operaciones

## Datos del caso

- **Descripción:** Consolidación de calificaciones y consulta de estudiantes por cada módulo académico.
- **Clave de búsqueda:** `ci` (carnet de identidad entero único).
- **Campos por registro:** `ci`, `nombre_completo`, `departamento`, `tipo`, `nota_final`, `asistencia_pct`, `estado` y `nivel_actual`.
- **Orden de magnitud:** Se manejan aproximadamente \(n = 10^4\) a \(10^5\) registros por módulo.

## Operaciones

| Operación | Frecuencia relativa | Frecuencia |
|---|---|---:|
| Buscar por clave (`ci`) | Muy frecuente | ⭐⭐⭐ |
| Insertar registro | Media | ⭐⭐ |
| Borrar registro | Poco frecuente | ⭐ |
| Obtener menor o siguiente (`ci` sucesor) | Poco frecuente | ⭐ |
| Consulta en rango `[a, b]` | Muy frecuente | ⭐⭐⭐ |
| Recorrer en orden | Muy frecuente | ⭐⭐⭐ |

Las operaciones que realizo más veces son **buscar por clave**, **consulta por rango** y **recorrer en orden**. La clave es única y los carnets tienen un orden natural; por eso el inventario necesita más que una búsqueda puntual.
