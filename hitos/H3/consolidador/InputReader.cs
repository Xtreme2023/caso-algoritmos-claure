using System.Globalization;
using System.Text;
using ClosedXML.Excel;

namespace H3.Consolidador;

internal static class InputReader
{
    private static readonly string[] CiAliases =
        ["ci", "carnet", "carnetidentidad", "numeroci", "numerocarnet", "nroci"];
    private static readonly string[] CourseAliases =
        ["codmateria", "codigomateria", "codcurso", "codigoasignatura", "nivel", "nivelactual"];
    private static readonly string[] SubjectAliases = ["materia", "asignatura", "curso"];

    public static IReadOnlyList<SourceRow> ReadRows(string path)
    {
        return Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".csv" => ReadCsv(path),
            ".xlsx" => ReadWorkbook(path),
            var extension => throw new InvalidDataException(
                $"Formato no compatible: '{extension}'. Use archivos CSV o XLSX.")
        };
    }

    private static IReadOnlyList<SourceRow> ReadCsv(string path)
    {
        var text = File.ReadAllText(path, Encoding.UTF8);
        var delimiter = DetectDelimiter(text);
        var records = ParseCsv(text, delimiter);
        return MakeRows(path, records);
    }

    private static IReadOnlyList<SourceRow> ReadWorkbook(string path)
    {
        using var workbook = new XLWorkbook(path);
        var rows = new List<SourceRow>();

        foreach (var worksheet in workbook.Worksheets)
        {
            var usedRows = worksheet.RowsUsed().ToArray();
            if (usedRows.Length == 0)
                continue;

            // Busca la fila de títulos aunque la planilla tenga un encabezado institucional antes.
            var headerRow = usedRows.FirstOrDefault(row =>
                HasRequiredHeaderCells(row.CellsUsed().Select(cell => NormalizeHeader(cell.GetString()))));

            if (headerRow is null)
                throw new InvalidDataException(
                    $"No se encontró una fila de encabezados válida en '{Path.GetFileName(path)}', hoja '{worksheet.Name}'.");

            var headers = headerRow.CellsUsed()
                .ToDictionary(
                    cell => cell.Address.ColumnNumber,
                    cell => NormalizeHeader(cell.GetString()));

            foreach (var row in usedRows.Where(row => row.RowNumber() > headerRow.RowNumber()))
            {
                var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var cell in row.CellsUsed())
                {
                    if (headers.TryGetValue(cell.Address.ColumnNumber, out var header) && header.Length > 0)
                        fields[header] = cell.GetFormattedString(CultureInfo.InvariantCulture).Trim();
                }

                if (fields.Values.Any(value => value.Length > 0))
                    rows.Add(new SourceRow(path, row.RowNumber(), fields));
            }
        }

        if (rows.Count == 0)
            throw new InvalidDataException($"No se encontraron filas de datos en '{Path.GetFileName(path)}'.");

        return rows;
    }

    private static IReadOnlyList<SourceRow> MakeRows(string path, IReadOnlyList<CsvRecord> records)
    {
        var headerIndex = -1;
        for (var index = 0; index < records.Count; index++)
        {
            if (HasRequiredHeaderCells(records[index].Fields.Select(NormalizeHeader)))
            {
                headerIndex = index;
                break;
            }
        }

        if (headerIndex < 0)
            throw new InvalidDataException(
                $"No se encontró una fila de encabezados válida en '{Path.GetFileName(path)}'.");

        var headers = records[headerIndex].Fields.Select(NormalizeHeader).ToArray();
        var rows = new List<SourceRow>();

        for (var index = headerIndex + 1; index < records.Count; index++)
        {
            var record = records[index];
            var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var column = 0; column < headers.Length && column < record.Fields.Count; column++)
            {
                if (headers[column].Length > 0)
                    fields[headers[column]] = record.Fields[column].Trim();
            }

            if (fields.Values.Any(value => value.Length > 0))
                rows.Add(new SourceRow(path, record.LineNumber, fields));
        }

        if (rows.Count == 0)
            throw new InvalidDataException($"No se encontraron filas de datos en '{Path.GetFileName(path)}'.");

        return rows;
    }

    private static bool HasRequiredHeaderCells(IEnumerable<string> headers)
    {
        var set = headers.ToHashSet(StringComparer.OrdinalIgnoreCase);
        return set.Overlaps(CiAliases) &&
               (set.Overlaps(CourseAliases) || set.Overlaps(SubjectAliases));
    }

    internal static string NormalizeHeader(string value)
    {
        return NormalizeText(value).Replace(" ", "", StringComparison.Ordinal);
    }

    internal static string NormalizeText(string value)
    {
        // Quita tildes y puntuación para comparar encabezados e identidades con formatos distintos.
        var decomposed = value.Normalize(NormalizationForm.FormD);
        var result = new StringBuilder(decomposed.Length);

        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
                continue;

            result.Append(char.IsLetterOrDigit(character) ? char.ToUpperInvariant(character) : ' ');
        }

        return string.Join(' ', result.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    private static char DetectDelimiter(string text)
    {
        var inQuotes = false;
        var semicolons = 0;
        var commas = 0;

        for (var index = 0; index < text.Length; index++)
        {
            var character = text[index];
            if (character == '"')
            {
                if (inQuotes && index + 1 < text.Length && text[index + 1] == '"')
                {
                    index++;
                    continue;
                }

                inQuotes = !inQuotes;
            }
            else if (!inQuotes && character == ';')
            {
                semicolons++;
            }
            else if (!inQuotes && character == ',')
            {
                commas++;
            }
            else if (!inQuotes && (character == '\r' || character == '\n'))
            {
                break;
            }
        }

        return semicolons >= commas ? ';' : ',';
    }

    private static IReadOnlyList<CsvRecord> ParseCsv(string text, char delimiter)
    {
        // El estado de comillas permite que delimitadores y saltos de línea dentro de una celda no corten la fila.
        var records = new List<CsvRecord>();
        var fields = new List<string>();
        var field = new StringBuilder();
        var inQuotes = false;
        var line = 1;
        var recordLine = 1;

        void FinishRecord()
        {
            fields.Add(field.ToString());
            field.Clear();
            if (fields.Any(value => value.Length > 0))
                records.Add(new CsvRecord(recordLine, fields));
            fields = [];
            recordLine = line + 1;
        }

        for (var index = 0; index < text.Length; index++)
        {
            var character = text[index];

            if (character == '"')
            {
                if (inQuotes && index + 1 < text.Length && text[index + 1] == '"')
                {
                    field.Append('"');
                    index++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (!inQuotes && character == delimiter)
            {
                fields.Add(field.ToString());
                field.Clear();
            }
            else if (character == '\r' || character == '\n')
            {
                if (character == '\r' && index + 1 < text.Length && text[index + 1] == '\n')
                    index++;

                if (inQuotes)
                    field.Append('\n');
                else
                    FinishRecord();

                line++;
            }
            else
            {
                field.Append(character);
            }
        }

        if (inQuotes)
            throw new InvalidDataException("El CSV termina dentro de un campo entre comillas.");

        if (field.Length > 0 || fields.Count > 0)
            FinishRecord();

        return records;
    }

    private sealed record CsvRecord(int LineNumber, IReadOnlyList<string> Fields);
}
