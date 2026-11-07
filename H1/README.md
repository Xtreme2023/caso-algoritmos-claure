# H1 — Proceso a optimizar: consolidación de notas y promoción de nivel

Al cierre de cada módulo, la secretaría académica nacional junta las planillas de notas y asistencia de los 7 departamentos (La Paz, Cochabamba, Santa Cruz, Oruro, Potosí, Chuquisaca y Tarija) para decidir quién aprueba y quién puede inscribirse al siguiente nivel.
Cada departamento envía su planilla en Excel (por correo o WhatsApp) con formatos distintos: nombres escritos de otra forma, CI faltante, niveles en mayúscula o minúscula, notas con coma o punto y celdas vacías.
La secretaria académica, que lo hace todo a mano; los 7 coordinadores departamentales, que reciben pedidos de corrección; y los ~150 estudiantes, que no pueden inscribirse al siguiente módulo hasta conocer su resultado.
Aprox 150 estudiantes y 11 materias (6 niveles A1, A2, B1, B2, C1 y C2 para miembros, 3 generales para cualquier nivel y 2 para externos e internos).
Cada estudiante cursa entre 2 y 3 materias, lo que da ~400 inscripciones por módulo. Cada inscripción tiene 2 parciales, un examen final y ~12 sesiones de asistencia: ~1.200 notas y ~4.800 registros de asistencia por módulo.
nota final ≥ 51/100 y asistencia ≥ 80 %. Además, para subir de nivel se debe haber aprobado el nivel anterior (por ejemplo, no se pasa a B1 sin A2).
**Cuánto tarda hoy:** entre 3 y 4 días hábiles por cierre de módulo (~25 horas de trabajo manual).
La mayor parte del tiempo se va en buscar duplicados a ojo, copiar y pegar entre planillas, recalcular promedios y revisar uno por uno los prerrequisitos.
Alumnos contados dos veces (por traslado entre departamentos), alumnos promovidos sin el prerrequisito, promedios mal calculados y resultados publicados tarde.
Construir en C# un proceso que lea las 7 planillas, normalice y elimine duplicados, calcule notas y asistencia, valide prerrequisitos y genere el consolidado en minutos en lugar de días.
La solución se diseñará para escalar después a inscripciones, certificados y consulta de notas.

> Muestra de datos: `muestra_notas.csv` (20 filas con el formato de planilla que envían los departamentos, separador `;`). Los datos son supuestos.
