using System.Diagnostics;
using System.Globalization;

namespace H3.Consolidador;

internal static class BenchmarkRunner
{
    private const int Repetitions = 5;
    private static readonly int[] ScaleFactors = [1, 5, 10, 20, 40];

    public static int Run(string inputDirectory)
    {
        if (!Directory.Exists(inputDirectory))
            throw new DirectoryNotFoundException($"No existe la carpeta de entrada: {inputDirectory}");

        var rows = ReadInputRows(inputDirectory);
        var baseKeys = ConsolidationService.GetValidDeduplicationKeys(rows);
        if (baseKeys.Count == 0)
            throw new InvalidDataException("No hay inscripciones válidas para comparar.");

        var baseKeyArray = baseKeys.ToArray();
        var uniqueKeys = CountWithDictionary(baseKeyArray);
        Console.WriteLine($"Filas leídas: {rows.Count}; inscripciones válidas: {baseKeys.Count}; claves únicas: {uniqueKeys}.");
        Console.WriteLine($"Calentamiento/calibración y {Repetitions} repeticiones medidas por algoritmo y tamaño.");
        Console.WriteLine();
        Console.WriteLine("| Filas | Claves únicas | Lista O(n²) (ms) | Diccionario O(n) prom. (ms) | Lista / diccionario |");
        Console.WriteLine("|---:|---:|---:|---:|---:|");

        foreach (var factor in ScaleFactors)
        {
            var keys = ExpandKeys(baseKeyArray, factor);
            var listCount = CountWithList(keys);
            var dictionaryCount = CountWithDictionary(keys);
            if (listCount != dictionaryCount)
                throw new InvalidOperationException("Lista y diccionario produjeron cantidades distintas de claves.");

            var listTime = AverageMilliseconds(keys, CountWithList, listCount);
            var dictionaryTime = AverageMilliseconds(keys, CountWithDictionary, dictionaryCount);
            var ratio = dictionaryTime > 0 ? listTime / dictionaryTime : 0;

            Console.WriteLine(string.Format(
                CultureInfo.InvariantCulture,
                "| {0:N0} | {1:N0} | {2:F4} | {3:F4} | {4:F1}x |",
                keys.Length,
                dictionaryCount,
                listTime,
                dictionaryTime,
                ratio));
        }

        Console.WriteLine();
        Console.WriteLine("Comparación limitada a detectar claves repetidas; no incluye lectura, normalización, notas ni escritura de resultados.");
        return 0;
    }

    private static IReadOnlyList<SourceRow> ReadInputRows(string inputDirectory)
    {
        var inputFiles = Directory.EnumerateFiles(inputDirectory, "*", SearchOption.TopDirectoryOnly)
            .Where(path => Path.GetExtension(path).Equals(".csv", StringComparison.OrdinalIgnoreCase) ||
                           Path.GetExtension(path).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            .Where(path => !Path.GetFileName(path).StartsWith("~$", StringComparison.Ordinal))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (inputFiles.Length == 0)
            throw new InvalidDataException("La carpeta de entrada no contiene archivos CSV o XLSX.");

        return inputFiles.SelectMany(InputReader.ReadRows).ToArray();
    }

    private static string[] ExpandKeys(IReadOnlyList<string> baseKeys, int factor)
    {
        var expanded = new string[baseKeys.Count * factor];
        for (var batch = 0; batch < factor; batch++)
        {
            for (var index = 0; index < baseKeys.Count; index++)
                expanded[batch * baseKeys.Count + index] = $"{baseKeys[index]}|lote:{batch}";
        }

        return expanded;
    }

    private static int CountWithList(string[] keys)
    {
        var unique = new List<string>(keys.Length);
        foreach (var key in keys)
        {
            if (!unique.Contains(key))
                unique.Add(key);
        }

        return unique.Count;
    }

    private static int CountWithDictionary(string[] keys)
    {
        var unique = new Dictionary<string, byte>(keys.Length, StringComparer.Ordinal);
        foreach (var key in keys)
        {
            if (!unique.TryGetValue(key, out _))
                unique.Add(key, 0);
        }

        return unique.Count;
    }

    private static double AverageMilliseconds(string[] keys, Func<string[], int> algorithm, int expectedCount)
    {
        var calibrationTimer = Stopwatch.StartNew();
        var calibrationCount = algorithm(keys);
        calibrationTimer.Stop();
        if (calibrationCount != expectedCount)
            throw new InvalidOperationException("El algoritmo produjo una cantidad inesperada durante la calibración.");

        var calibrationMilliseconds = Math.Max(calibrationTimer.Elapsed.TotalMilliseconds, 0.001);
        var iterationsPerRepetition = Math.Clamp(
            (int)Math.Ceiling(40d / calibrationMilliseconds),
            1,
            10_000);

        double totalMilliseconds = 0;
        for (var repetition = 0; repetition < Repetitions; repetition++)
        {
            var timer = Stopwatch.StartNew();
            for (var iteration = 0; iteration < iterationsPerRepetition; iteration++)
            {
                var actualCount = algorithm(keys);
                if (actualCount != expectedCount)
                    throw new InvalidOperationException("El algoritmo cambió la cantidad de claves únicas entre repeticiones.");
            }
            timer.Stop();

            totalMilliseconds += timer.Elapsed.TotalMilliseconds / iterationsPerRepetition;
        }

        return totalMilliseconds / Repetitions;
    }
}
