namespace H3.Consolidador;

internal static class HistoryReader
{
    public static IReadOnlySet<(string Ci, string Course)> ReadApprovedCourses(string path)
    {
        var rows = InputReader.ReadRows(path);
        var approved = new HashSet<(string Ci, string Course)>();

        foreach (var row in rows)
        {
            var ciRaw = row.Get("ci", "carnet", "carnetidentidad", "numeroci", "numerocarnet", "nroci");
            var ci = ConsolidationService.NormalizeCi(ciRaw);
            var course = ConsolidationService.NormalizeCourseCode(
                row.Get(
                    "codmateria", "codigomateria", "codcurso", "codigoasignatura",
                    "nivel", "nivelactual", "materia", "asignatura", "curso"));
            var state = InputReader.NormalizeHeader(row.Get("estado", "aprobado", "resultado"));

            if (ci is null || course.Length == 0)
                throw new InvalidDataException(
                    $"Historial inválido en '{Path.GetFileName(path)}', fila {row.RowNumber}: se requiere CI y nivel/código de materia.");

            if (state.Length == 0)
                throw new InvalidDataException(
                    $"Historial inválido en '{Path.GetFileName(path)}', fila {row.RowNumber}: falta el estado de aprobación.");

            if (state is not ("APROBADO" or "REPROBADO" or "SI" or "NO" or "TRUE" or "FALSE" or "1" or "0"))
                throw new InvalidDataException(
                    $"Historial inválido en '{Path.GetFileName(path)}', fila {row.RowNumber}: estado no reconocido '{state}'.");

            var isApproved = state is "APROBADO" or "SI" or "TRUE" or "1";
            if (isApproved)
                approved.Add((ci, course));
        }

        return approved;
    }
}
