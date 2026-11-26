namespace H3.Consolidador;

internal sealed record SourceRow(
    string SourceFile,
    int RowNumber,
    IReadOnlyDictionary<string, string> Fields)
{
    public string Get(params string[] aliases)
    {
        foreach (var alias in aliases)
        {
            if (Fields.TryGetValue(alias, out var value))
                return value.Trim();
        }

        return "";
    }
}

internal sealed record ReviewItem(
    string SourceFile,
    int RowNumber,
    string Ci,
    string Nombre,
    string CodigoMateria,
    string Motivo);

internal sealed class Enrollment
{
    public required string SourceFile { get; init; }
    public required int RowNumber { get; init; }
    public required string IdentityKey { get; init; }
    public required string NormalizedName { get; init; }
    public required string Nombre { get; init; }
    public required string CodigoMateria { get; init; }
    public required string Materia { get; init; }
    public required string Modulo { get; init; }
    public required string Tipo { get; init; }
    public required string CiOriginal { get; init; }
    public required decimal Parcial1 { get; init; }
    public required decimal Parcial2 { get; init; }
    public required decimal ExamenFinal { get; init; }
    public required decimal? Asistencias { get; init; }
    public required decimal? Sesiones { get; init; }
    public required decimal AsistenciaPct { get; init; }
    public string? Ci { get; set; }
    public bool RequiresIdentityReview { get; set; }
    public bool HasConflictingDuplicate { get; set; }
    public List<string> Departamentos { get; } = [];

    // Se muestra a un decimal, pero la regla de aprobación usa el promedio sin redondear.
    public decimal PromedioSinRedondear => (Parcial1 + Parcial2 + ExamenFinal) / 3m;
    public decimal NotaFinal =>
        decimal.Round(PromedioSinRedondear, 1, MidpointRounding.AwayFromZero);

    public bool IsApproved => PromedioSinRedondear >= 51m && AsistenciaPct >= 80m;
}

internal sealed record ConsolidationResult(
    IReadOnlyList<Enrollment> Enrollments,
    IReadOnlyList<ReviewItem> ReviewItems,
    int InputRows,
    int MergedDuplicates);
