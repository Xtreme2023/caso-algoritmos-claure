using System.Globalization;
using System.Text;

namespace H3.Consolidador;

internal static class OutputWriter
{
    public static void WriteConsolidatedCsv(
        string path,
        IReadOnlyList<Enrollment> enrollments,
        ConsolidationService service,
        IReadOnlySet<(string Ci, string Course)> priorApprovals)
    {
        using var writer = new StreamWriter(path, false, new UTF8Encoding(true));
        WriteRow(writer,
        [
            "ci", "nombre_completo", "departamentos", "tipo", "cod_materia", "materia", "modulo",
            "parcial_1", "parcial_2", "examen_final", "nota_final", "asistencias", "sesiones",
            "asistencia_pct", "estado_materia", "prerrequisito", "proximo_nivel",
            "puede_inscribirse_siguiente", "identidad"
        ]);

        foreach (var enrollment in enrollments.OrderBy(item => item.Ci ?? item.NormalizedName, StringComparer.Ordinal)
                     .ThenBy(item => item.CodigoMateria, StringComparer.Ordinal))
        {
            var (prerequisite, nextLevel, canEnroll) =
                service.EvaluateProgression(enrollment, priorApprovals);
            var courseStatus = enrollment.HasConflictingDuplicate
                ? "REVISAR_DATOS"
                : enrollment.RequiresIdentityReview
                    ? "REVISAR_IDENTIDAD"
                    : enrollment.IsApproved ? "APROBADO" : "REPROBADO";
            var identityStatus = enrollment.RequiresIdentityReview ? "REVISAR_CI" : "IDENTIFICADA";
            var conflict = enrollment.HasConflictingDuplicate;

            // No se presenta la primera versión como elegida cuando hay datos duplicados contradictorios.
            WriteRow(writer,
            [
                enrollment.Ci ?? "",
                enrollment.Nombre,
                string.Join(", ", enrollment.Departamentos.OrderBy(value => value, StringComparer.Ordinal)),
                enrollment.Tipo,
                enrollment.CodigoMateria,
                enrollment.Materia,
                enrollment.Modulo,
                conflict ? "" : Format(enrollment.Parcial1),
                conflict ? "" : Format(enrollment.Parcial2),
                conflict ? "" : Format(enrollment.ExamenFinal),
                conflict ? "" : Format(enrollment.NotaFinal),
                conflict ? "" : Format(enrollment.Asistencias),
                conflict ? "" : Format(enrollment.Sesiones),
                conflict ? "" : Format(enrollment.AsistenciaPct),
                courseStatus,
                prerequisite,
                nextLevel,
                canEnroll,
                identityStatus
            ]);
        }
    }

    public static void WriteReviewCsv(string path, IReadOnlyList<ReviewItem> items)
    {
        using var writer = new StreamWriter(path, false, new UTF8Encoding(true));
        WriteRow(writer, ["archivo", "fila", "ci", "nombre", "cod_materia", "motivo"]);
        foreach (var item in items)
        {
            WriteRow(writer,
            [
                Path.GetFileName(item.SourceFile),
                item.RowNumber.ToString(CultureInfo.InvariantCulture),
                item.Ci,
                item.Nombre,
                item.CodigoMateria,
                item.Motivo
            ]);
        }
    }

    private static string Format(decimal value) =>
        value.ToString("0.##", CultureInfo.InvariantCulture);

    private static string Format(decimal? value) =>
        value.HasValue ? Format(value.Value) : "";

    private static void WriteRow(TextWriter writer, IReadOnlyList<string> fields)
    {
        writer.WriteLine(string.Join(';', fields.Select(Escape)));
    }

    private static string Escape(string value) =>
        value.IndexOfAny([';', '"', '\r', '\n']) >= 0
            ? $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\""
            : value;
}
