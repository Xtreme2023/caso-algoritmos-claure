namespace H3.Consolidador;

internal static class Program
{
    private static int Main(string[] args)
    {
        if (args.Length == 1 && args[0] is "--help" or "-h")
        {
            PrintUsage();
            return 0;
        }

        if (args.Length == 2 && args[0] == "--benchmark")
            return BenchmarkRunner.Run(args[1]);

        if (!TryParseArguments(args, out var inputDirectory, out var historyPath, out var outputDirectory))
        {
            PrintUsage();
            return 2;
        }

        if (!Directory.Exists(inputDirectory))
            throw new DirectoryNotFoundException($"No existe la carpeta de entrada: {inputDirectory}");
        if (!File.Exists(historyPath))
            throw new FileNotFoundException("No se encontró el archivo de aprobaciones previas.", historyPath);

        var fullHistoryPath = Path.GetFullPath(historyPath);
        var inputFiles = Directory.EnumerateFiles(inputDirectory, "*", SearchOption.TopDirectoryOnly)
            .Where(path => Path.GetExtension(path).Equals(".csv", StringComparison.OrdinalIgnoreCase) ||
                           Path.GetExtension(path).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            .Where(path => !Path.GetFileName(path).StartsWith("~$", StringComparison.Ordinal))
            .Where(path => !Path.GetFullPath(path).Equals(fullHistoryPath, StringComparison.OrdinalIgnoreCase))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (inputFiles.Length == 0)
            throw new InvalidDataException("La carpeta de entrada no contiene archivos CSV o XLSX.");

        var rows = inputFiles.SelectMany(InputReader.ReadRows).ToArray();
        var priorApprovals = HistoryReader.ReadApprovedCourses(historyPath);
        var service = new ConsolidationService();
        var result = service.Consolidate(rows, priorApprovals);

        Directory.CreateDirectory(outputDirectory);
        var consolidatedPath = Path.Combine(outputDirectory, "consolidado.csv");
        var reviewPath = Path.Combine(outputDirectory, "revisiones.csv");
        OutputWriter.WriteConsolidatedCsv(consolidatedPath, result.Enrollments, service, priorApprovals);
        OutputWriter.WriteReviewCsv(reviewPath, result.ReviewItems);

        Console.WriteLine($"Archivos de entrada: {inputFiles.Length}");
        Console.WriteLine($"Filas leídas: {result.InputRows}");
        Console.WriteLine($"Inscripciones consolidadas: {result.Enrollments.Count}");
        Console.WriteLine($"Duplicados agrupados: {result.MergedDuplicates}");
        Console.WriteLine($"Casos para revisión: {result.ReviewItems.Count}");
        Console.WriteLine($"Aprobados: {result.Enrollments.Count(item => !item.HasConflictingDuplicate && !item.RequiresIdentityReview && item.IsApproved)}");
        Console.WriteLine($"Consolidado: {Path.GetFullPath(consolidatedPath)}");
        Console.WriteLine($"Revisiones: {Path.GetFullPath(reviewPath)}");
        return 0;
    }

    private static bool TryParseArguments(
        IReadOnlyList<string> args,
        out string inputDirectory,
        out string historyPath,
        out string outputDirectory)
    {
        inputDirectory = "";
        historyPath = "";
        outputDirectory = "";

        for (var index = 0; index < args.Count; index++)
        {
            if (index + 1 >= args.Count)
                return false;

            var value = args[++index];
            switch (args[index - 1])
            {
                case "--entrada":
                    inputDirectory = value;
                    break;
                case "--historial":
                    historyPath = value;
                    break;
                case "--salida":
                    outputDirectory = value;
                    break;
                default:
                    return false;
            }
        }

        return inputDirectory.Length > 0 && historyPath.Length > 0 && outputDirectory.Length > 0;
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Uso:");
        Console.WriteLine("  dotnet run --project hitos/H3/consolidador -- --entrada <carpeta-planillas> --historial <aprobaciones-previas.csv> --salida <carpeta-salida>");
        Console.WriteLine("  dotnet run --project hitos/H3/consolidador -- --benchmark <carpeta-planillas>");
        Console.WriteLine();
        Console.WriteLine("La carpeta de entrada puede contener archivos CSV o XLSX con encabezados en la primera fila que los contiene.");
    }
}
