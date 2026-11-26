# Conclusión

La solución adecuada para consolidar las planillas no es un algoritmo voraz ni programación dinámica. Es una combinación de procesamiento por lotes:

- recorrer cada inscripción una vez;
- normalizar sus campos y calcular nota y asistencia;
- usar un diccionario con clave compuesta para agrupar matrículas;
- enviar identidades incompletas y datos contradictorios a revisión;
- representar los prerrequisitos como un grafo dirigido y usar BFS para comprobar la ruta de avance;
- ordenar solo al generar el acta.

El costo esperado es O(n) para leer y agrupar, más O(n log n) para ordenar el resultado. Para unas 400 inscripciones, ambas etapas son pequeñas; la mejora más importante frente al proceso manual es la consistencia, la detección explícita de errores y la trazabilidad, no una promesa de velocidad basada en una medición distinta.

El programa produce un consolidado y un archivo separado de revisiones. Los registros con CI faltante pueden agruparse con nombre normalizado, materia y módulo según la regla solicitada, pero quedan marcados para confirmar identidad; los conflictos de notas no se resuelven automáticamente. La secretaría debe revisar esos casos antes de publicar decisiones oficiales.
