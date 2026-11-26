using System.Globalization;
using System.Text;

namespace H3.Consolidador;

internal sealed class ConsolidationService
{
    private static readonly string[] LevelOrder = ["A1", "A2", "B1", "B2", "C1", "C2"];
    private readonly PromotionGraph _promotionGraph = new(LevelOrder);

    public static IReadOnlyList<string> GetValidDeduplicationKeys(IReadOnlyList<SourceRow> rows)
    {
        var keys = new List<string>(rows.Count);
        var reviews = new List<ReviewItem>();

        foreach (var row in rows)
        {
            if (TryBuildEnrollment(row, reviews, out var enrollment))
                keys.Add(enrollment.IdentityKey);
        }

        return keys;
    }

    public ConsolidationResult Consolidate(
        IReadOnlyList<SourceRow> rows,
        IReadOnlySet<(string Ci, string Course)> priorApprovals)
    {
        // Una clave hash evita comparar cada inscripción con todas las anteriores: O(n) promedio.
        var unique = new Dictionary<string, Enrollment>(StringComparer.Ordinal);
        var enrollments = new List<Enrollment>();
        var reviews = new List<ReviewItem>();
        var mergedDuplicates = 0;

        foreach (var row in rows)
        {
            if (!TryBuildEnrollment(row, reviews, out var enrollment))
                continue;

            if (!unique.TryGetValue(enrollment.IdentityKey, out var existing))
            {
                unique.Add(enrollment.IdentityKey, enrollment);
                enrollments.Add(enrollment);
                continue;
            }

            mergedDuplicates++;
            // Una copia contradictoria se retiene como incidencia, no como una decisión académica confiable.
            if (!SameAcademicData(existing, enrollment))
            {
                existing.HasConflictingDuplicate = true;
                reviews.Add(ToReview(enrollment,
                    "La misma matrícula aparece con notas o asistencia diferentes; no se elige una versión automáticamente."));
            }

            if (enrollment.RequiresIdentityReview)
            {
                existing.RequiresIdentityReview = true;
                reviews.Add(ToReview(enrollment,
                    "Se unió por nombre normalizado y materia/módulo porque falta el CI; verificar identidad."));
            }

            foreach (var department in enrollment.Departamentos)
            {
                if (!existing.Departamentos.Contains(department, StringComparer.OrdinalIgnoreCase))
                    existing.Departamentos.Add(department);
            }
        }

        foreach (var enrollment in enrollments)
        {
            if (enrollment.RequiresIdentityReview)
            {
                reviews.Add(ToReview(enrollment,
                    "CI ausente: la coincidencia solo es confiable si el nombre normalizado y materia/módulo identifican a la misma persona."));
            }
        }

        return new ConsolidationResult(enrollments, reviews, rows.Count, mergedDuplicates);
    }

    public (string Prerequisite, string NextLevel, string CanEnrollNext) EvaluateProgression(
        Enrollment enrollment,
        IReadOnlySet<(string Ci, string Course)> priorApprovals)
    {
        var course = NormalizeCourseCode(enrollment.CodigoMateria);
        if (!_promotionGraph.Contains(course))
            return ("NO_APLICA", "", "NO_APLICA");

        if (enrollment.Tipo.Length == 0)
            return ("REVISAR_TIPO", _promotionGraph.GetNextLevel(course) ?? "", "REVISAR");
        if (!enrollment.Tipo.Equals("MIEMBRO", StringComparison.Ordinal))
            return ("NO_APLICA", "", "NO_APLICA");

        if (enrollment.Ci is null || enrollment.RequiresIdentityReview)
            return ("REVISAR_IDENTIDAD", _promotionGraph.GetNextLevel(course) ?? "", "REVISAR");

        var prerequisite = _promotionGraph.GetPrerequisite(course);
        var prerequisiteMet = prerequisite is null ||
            priorApprovals.Contains((enrollment.Ci, prerequisite));
        var prerequisiteStatus = prerequisite is null
            ? "NO_APLICA"
            : prerequisiteMet ? "CUMPLIDO" : "NO_ACREDITADO";

        var next = _promotionGraph.GetNextLevel(course);
        if (next is null)
            return (prerequisiteStatus, "", "NO_HAY_NIVEL_SIGUIENTE");

        if (enrollment.HasConflictingDuplicate)
            return (prerequisiteStatus, next, "REVISAR_DATOS");

        if (!enrollment.IsApproved || !prerequisiteMet)
            return (prerequisiteStatus, next, "NO");

        var path = _promotionGraph.FindShortestPath(course, next);
        return path is { Count: 2 }
            ? (prerequisiteStatus, next, "SI")
            : (prerequisiteStatus, next, "NO");
    }

    public static string NormalizeCourseCode(string value)
    {
        var normalized = string.Concat(InputReader.NormalizeText(value).Where(char.IsLetterOrDigit));
        return normalized switch
        {
            "NIVELA1" => "A1",
            "NIVELA2" => "A2",
            "NIVELB1" => "B1",
            "NIVELB2" => "B2",
            "NIVELC1" => "C1",
            "NIVELC2" => "C2",
            "GENERAL1" => "G1",
            "GENERAL2" => "G2",
            "GENERAL3" => "G3",
            "CURSOABIERTO1" => "E1",
            "CURSOABIERTO2" => "E2",
            _ => normalized
        };
    }

    private static bool TryBuildEnrollment(
        SourceRow row,
        ICollection<ReviewItem> reviews,
        out Enrollment enrollment)
    {
        enrollment = null!;
        var ciRaw = row.Get("ci", "carnet", "carnetidentidad", "numeroci", "numerocarnet", "nroci");
        var name = row.Get("nombrecompleto", "nombre", "estudiante", "alumno");
        var normalizedName = InputReader.NormalizeText(name);
        var type = InputReader.NormalizeText(row.Get("tipo", "tipoestudiante", "categoria"));
        var department = InputReader.NormalizeText(row.Get("departamento", "depto", "sede"));
        if (department.Length == 0)
            department = InputReader.NormalizeText(Path.GetFileNameWithoutExtension(row.SourceFile));

        var subject = row.Get("materia", "asignatura", "curso");
        var code = NormalizeCourseCode(row.Get(
            "codmateria", "codigomateria", "codcurso", "codigoasignatura", "nivel", "nivelactual"));
        if (code.Length == 0)
            code = NormalizeCourseCode(subject);

        var module = NormalizeCourseCode(row.Get("modulo", "periodo", "gestion", "ciclo"));
        var p1Raw = row.Get("parcial1", "nota1", "p1");
        var p2Raw = row.Get("parcial2", "nota2", "p2");
        var finalRaw = row.Get("examenfinal", "notaexamen", "final");
        var attendanceRaw = row.Get("asistencias", "asistencia", "presentes");
        var sessionsRaw = row.Get("sesiones", "totalclases", "clases");
        var attendancePctRaw = row.Get("asistenciapct", "porcentajeasistencia", "attendancepct");

        var ci = NormalizeCi(ciRaw);
        var errors = new List<string>();

        if (normalizedName.Length == 0)
            errors.Add("Falta el nombre del estudiante.");
        if (code.Length == 0)
            errors.Add("Falta el código de materia.");

        if (!TryParseScore(p1Raw, out var p1))
            errors.Add("Parcial 1 vacío, no numérico o fuera del rango 0–100.");
        if (!TryParseScore(p2Raw, out var p2))
            errors.Add("Parcial 2 vacío, no numérico o fuera del rango 0–100.");
        if (!TryParseScore(finalRaw, out var final))
            errors.Add("Examen final vacío, no numérico o fuera del rango 0–100.");

        decimal? attendances;
        decimal? sessions;
        decimal attendancePct;
        if (attendancePctRaw.Length > 0)
        {
            if (!TryParseDecimal(attendancePctRaw, out attendancePct) ||
                attendancePct is < 0 or > 100)
            {
                errors.Add("Porcentaje de asistencia inválido; debe estar entre 0 y 100.");
                attendancePct = 0;
            }

            attendances = null;
            sessions = null;
        }
        else
        {
            var attendanceParsed = TryParseDecimal(attendanceRaw, out var attendanceValue);
            var sessionsParsed = TryParseDecimal(sessionsRaw, out var sessionValue);
            var attendanceValuesValid = attendanceParsed &&
                sessionsParsed &&
                sessionValue > 0 &&
                attendanceValue >= 0 &&
                attendanceValue <= sessionValue;

            if (!attendanceValuesValid)
            {
                errors.Add("Asistencias/sesiones inválidas; se requiere 0 ≤ asistencias ≤ sesiones y sesiones > 0.");
                attendances = null;
                sessions = null;
                attendancePct = 0;
            }
            else
            {
                attendances = attendanceValue;
                sessions = sessionValue;
                attendancePct = attendanceValue * 100m / sessionValue;
            }
        }

        if (errors.Count > 0)
        {
            reviews.Add(new ReviewItem(
                row.SourceFile,
                row.RowNumber,
                ci ?? ciRaw,
                name,
                code,
                string.Join(" ", errors)));
            return false;
        }

        var requiresIdentityReview = ci is null;
        // Una clave compuesta conserva materias distintas y limita el respaldo por nombre a la misma matrícula.
        var identityKey = ci is not null
            ? $"CI:{ci}|{code}|{module}"
            : $"NOMBRE:{normalizedName}|{code}|{module}";

        enrollment = new Enrollment
        {
            SourceFile = row.SourceFile,
            RowNumber = row.RowNumber,
            IdentityKey = identityKey,
            NormalizedName = normalizedName,
            Nombre = name.Trim(),
            Ci = ci,
            Tipo = type,
            CodigoMateria = code,
            Materia = subject.Trim(),
            Modulo = module,
            Parcial1 = p1,
            Parcial2 = p2,
            ExamenFinal = final,
            CiOriginal = ciRaw,
            Asistencias = attendances,
            Sesiones = sessions,
            AsistenciaPct = attendancePct,
            RequiresIdentityReview = requiresIdentityReview
        };
        enrollment.Departamentos.Add(department);
        return true;
    }

    private static ReviewItem ToReview(Enrollment enrollment, string reason) =>
        new(enrollment.SourceFile, enrollment.RowNumber, enrollment.Ci ?? enrollment.CiOriginal, enrollment.Nombre,
            enrollment.CodigoMateria, reason);

    private static bool SameAcademicData(Enrollment left, Enrollment right) =>
        left.Parcial1 == right.Parcial1 &&
        left.Parcial2 == right.Parcial2 &&
        left.ExamenFinal == right.ExamenFinal &&
        left.Tipo == right.Tipo &&
        left.Asistencias == right.Asistencias &&
        left.Sesiones == right.Sesiones &&
        left.AsistenciaPct == right.AsistenciaPct;

    internal static string? NormalizeCi(string value)
    {
        var trimmed = value.Trim();
        if (trimmed.Length == 0 || trimmed == "0" ||
            trimmed.Equals("N/A", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Equals("S/D", StringComparison.OrdinalIgnoreCase))
            return null;

        var digits = new StringBuilder(trimmed.Length);
        foreach (var character in trimmed)
        {
            if (char.IsDigit(character))
                digits.Append(character);
            else if (!char.IsWhiteSpace(character) && character is not '-' and not '.')
                return null;
        }

        var normalized = digits.ToString().TrimStart('0');
        return normalized.Length == 0 ? null : normalized;
    }

    private static bool TryParseScore(string raw, out decimal score) =>
        TryParseDecimal(raw, out score) && score is >= 0 and <= 100;

    private static bool TryParseDecimal(string raw, out decimal value)
    {
        var normalized = raw.Trim().Replace(" ", "", StringComparison.Ordinal);
        if (normalized.Length == 0)
        {
            value = 0;
            return false;
        }

        if (normalized.Contains(',') && normalized.Contains('.'))
        {
            if (normalized.LastIndexOf(',') > normalized.LastIndexOf('.'))
                normalized = normalized.Replace(".", "", StringComparison.Ordinal).Replace(',', '.');
            else
                normalized = normalized.Replace(",", "", StringComparison.Ordinal);
        }
        else if (normalized.Contains(','))
        {
            normalized = normalized.Replace(',', '.');
        }

        return decimal.TryParse(
            normalized,
            NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture,
            out value);
    }

    private sealed class PromotionGraph
    {
        private readonly Dictionary<string, List<string>> _edges = new(StringComparer.Ordinal);
        private readonly Dictionary<string, string> _prerequisites = new(StringComparer.Ordinal);

        public PromotionGraph(IReadOnlyList<string> levels)
        {
            foreach (var level in levels)
                _edges[level] = [];

            // Cada arista representa el requisito directo para inscribirse al nivel siguiente.
            for (var index = 0; index < levels.Count - 1; index++)
            {
                _edges[levels[index]].Add(levels[index + 1]);
                _prerequisites[levels[index + 1]] = levels[index];
            }
        }

        public bool Contains(string course) => _edges.ContainsKey(course);

        public string? GetNextLevel(string course) =>
            _edges.TryGetValue(course, out var next) && next.Count > 0 ? next[0] : null;

        public string? GetPrerequisite(string course) =>
            _prerequisites.TryGetValue(course, out var prerequisite) ? prerequisite : null;

        public List<string>? FindShortestPath(string start, string target)
        {
            if (!_edges.ContainsKey(start) || !_edges.ContainsKey(target))
                return null;

            var queue = new Queue<string>();
            var previous = new Dictionary<string, string?>(StringComparer.Ordinal) { [start] = null };
            queue.Enqueue(start);

            // BFS encuentra la ruta más corta en O(V + E); la cadena actual tiene seis niveles.
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (current == target)
                    break;

                foreach (var neighbor in _edges[current])
                {
                    if (previous.ContainsKey(neighbor))
                        continue;

                    previous[neighbor] = current;
                    queue.Enqueue(neighbor);
                }
            }

            if (!previous.ContainsKey(target))
                return null;

            var path = new List<string>();
            for (string? node = target; node is not null; node = previous[node])
                path.Add(node);
            path.Reverse();
            return path;
        }
    }
}
