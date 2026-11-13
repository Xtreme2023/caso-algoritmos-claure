using System.Diagnostics;
using System.Text;

namespace TorneoOrdenamiento;

class Program
{
    // Cantidades crecientes y protocolo: calentar · 3 corridas · promediar
    static readonly int[] Tamanos = { 10_000, 20_000, 40_000, 80_000 };
    const int Corridas = 3;

    // Competidores
    static readonly (string Nombre, Action<int[]> Sort)[] Competidores =
    {
        ("Burbuja (O(n²))",      BubbleSort),
        ("Inserción (O(n²))",    InsertionSort),
        ("MergeSort (n log n)",  a => MergeSort(a)),
        ("QuickSort (n log n)",  a => QuickSort(a, 0, a.Length - 1)),
        ("Array.Sort (nativo)",  a => Array.Sort(a)),
    };

    // Resultados del último torneo completo: [competidor][tamaño] en ms
    static double[][]? ultimosResultados;

    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Calentar();

        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("╔════════════════════════════════════════════╗");
            Console.WriteLine("║     TORNEO DE ORDENAMIENTO  (C# consola)   ║");
            Console.WriteLine("╠════════════════════════════════════════════╣");
            Console.WriteLine("║ 1. Inserción  vs MergeSort                 ║");
            Console.WriteLine("║ 2. Inserción  vs QuickSort                 ║");
            Console.WriteLine("║ 3. Inserción  vs Array.Sort (nativo)       ║");
            Console.WriteLine("║ 4. Burbuja    vs Inserción                 ║");
            Console.WriteLine("║ 5. TORNEO COMPLETO (todos, con tabla)      ║");
            Console.WriteLine("║ 6. Guardar informe (informe.md)            ║");
            Console.WriteLine("║ 0. Salir                                   ║");
            Console.WriteLine("╚════════════════════════════════════════════╝");
            Console.Write("Opción: ");

            switch (Console.ReadLine()?.Trim())
            {
                case "1": Duelo(1, 2); break;
                case "2": Duelo(1, 3); break;
                case "3": Duelo(1, 4); break;
                case "4": Duelo(0, 1); break;
                case "5": TorneoCompleto(); break;
                case "6": GuardarInforme(); break;
                case "0": return;
                default: Console.WriteLine("Opción no válida."); break;
            }
        }
    }

    // ───────────────────────── Duelos 1 vs 1 ─────────────────────────
    static void Duelo(int iA, int iB)
    {
        var a = Competidores[iA];
        var b = Competidores[iB];

        Console.WriteLine($"\n=== {a.Nombre}  vs  {b.Nombre} ===");
        Console.WriteLine($"(calentado · {Corridas} corridas · promedio en ms)\n");
        Console.WriteLine($"{"N",8} | {a.Nombre,-22} | {b.Nombre,-22} | {"Veces más rápido",-18} | Ordenado");
        Console.WriteLine(new string('-', 92));

        foreach (int n in Tamanos)
        {
            int[] datos = GenerarDatos(n);
            double tA = Medir(a.Sort, datos, out bool okA);
            double tB = Medir(b.Sort, datos, out bool okB);

            string ganador = tA < tB ? $"{a.Nombre.Split(' ')[0]} x{tB / tA:F1}" : $"{b.Nombre.Split(' ')[0]} x{tA / tB:F1}";
            string ok = okA && okB ? "✔ ambos" : "✘ ERROR";
            Console.WriteLine($"{n,8} | {tA,19:F2} ms | {tB,19:F2} ms | {ganador,-18} | {ok}");
        }
    }

    // ───────────────────────── Torneo completo ─────────────────────────
    static void TorneoCompleto()
    {
        Console.WriteLine("\n=== TORNEO COMPLETO ===");
        Console.WriteLine("Aviso: los algoritmos cuadráticos con 80.000 elementos pueden tardar varios segundos.\n");

        var res = new double[Competidores.Length][];
        for (int c = 0; c < Competidores.Length; c++)
            res[c] = new double[Tamanos.Length];

        bool todoOk = true;
        for (int t = 0; t < Tamanos.Length; t++)
        {
            int[] datos = GenerarDatos(Tamanos[t]);
            for (int c = 0; c < Competidores.Length; c++)
            {
                Console.Write($"  N={Tamanos[t],6} · {Competidores[c].Nombre,-22} ... ");
                res[c][t] = Medir(Competidores[c].Sort, datos, out bool ok);
                todoOk &= ok;
                Console.WriteLine($"{res[c][t],10:F2} ms {(ok ? "✔" : "✘")}");
            }
        }

        ultimosResultados = res;
        Console.WriteLine();
        Console.WriteLine(ConstruirTabla(res));
        Console.WriteLine(todoOk
            ? "Verificación: todos los competidores dejaron el arreglo ORDENADO ✔"
            : "Verificación: ¡algún competidor falló! ✘");
    }

    // ───────────────────────── Medición ─────────────────────────
    /// <summary>Promedia Corridas ejecuciones sobre copias de los mismos datos.
    /// Verifica que el resultado esté ordenado ANTES de dar el tiempo por válido.</summary>
    static double Medir(Action<int[]> sort, int[] original, out bool ordenadoOk)
    {
        int[] esperado = (int[])original.Clone();
        Array.Sort(esperado);

        double total = 0;
        ordenadoOk = true;

        for (int i = 0; i < Corridas; i++)
        {
            int[] copia = (int[])original.Clone();

            var sw = Stopwatch.StartNew();
            sort(copia);
            sw.Stop();

            if (!EstaOrdenado(copia) || !copia.AsSpan().SequenceEqual(esperado))
                ordenadoOk = false;

            total += sw.Elapsed.TotalMilliseconds;
        }
        return total / Corridas;
    }

    // Calentamiento: obliga al JIT a compilar todos los algoritmos antes de medir
    static void Calentar()
    {
        Console.Write("Calentando JIT... ");
        int[] datos = GenerarDatos(2_000);
        foreach (var c in Competidores)
            c.Sort((int[])datos.Clone());
        Console.WriteLine("listo.");
    }

    static int[] GenerarDatos(int n)
    {
        var rnd = new Random(12345); // semilla fija → mismos datos para todos
        var a = new int[n];
        for (int i = 0; i < n; i++) a[i] = rnd.Next(0, 1_000_000);
        return a;
    }

    static bool EstaOrdenado(int[] a)
    {
        for (int i = 1; i < a.Length; i++)
            if (a[i - 1] > a[i]) return false;
        return true;
    }

    // ───────────────────────── Tabla e informe ─────────────────────────
    static string ConstruirTabla(double[][] res)
    {
        var sb = new StringBuilder();
        sb.Append($"| {"Algoritmo",-22} |");
        foreach (int n in Tamanos) sb.Append($" {n,10:N0} |");
        sb.AppendLine();
        sb.Append($"|{new string('-', 24)}|");
        foreach (int _ in Tamanos) sb.Append($"{new string('-', 12)}|");
        sb.AppendLine();

        for (int c = 0; c < Competidores.Length; c++)
        {
            sb.Append($"| {Competidores[c].Nombre,-22} |");
            foreach (double ms in res[c]) sb.Append($" {ms,7:F2} ms |");
            sb.AppendLine();
        }
        return sb.ToString();
    }

    static void GuardarInforme()
    {
        if (ultimosResultados is null)
        {
            Console.WriteLine("Primero ejecuta la opción 5 (torneo completo).");
            return;
        }

        var res = ultimosResultados;
        int last = Tamanos.Length - 1;

        // Ganador y perdedor con el N más grande
        int mejor = 0, peor = 0;
        for (int c = 1; c < res.Length; c++)
        {
            if (res[c][last] < res[mejor][last]) mejor = c;
            if (res[c][last] > res[peor][last]) peor = c;
        }

        // Crecimiento de Inserción (índice 1) al duplicar N
        double crecIns = res[1][last] / res[1][last - 1];
        double crecMerge = res[2][last] / res[2][last - 1];

        var sb = new StringBuilder();
        sb.AppendLine("# Informe – Torneo de ordenamiento (C#)");
        sb.AppendLine();
        sb.AppendLine($"Fecha: {DateTime.Now:yyyy-MM-dd HH:mm}");
        sb.AppendLine($"Protocolo: calentamiento JIT · {Corridas} corridas · promedio · datos aleatorios con semilla fija.");
        sb.AppendLine("Verificación: cada resultado se comparó contra `Array.Sort` antes de aceptar el tiempo.");
        sb.AppendLine();
        sb.AppendLine("## Tabla de tiempos (ms)");
        sb.AppendLine();
        sb.AppendLine(ConstruirTabla(res));
        sb.AppendLine("## Conclusiones");
        sb.AppendLine();
        sb.AppendLine($"- Con N = {Tamanos[last]:N0}, el más rápido fue **{Competidores[mejor].Nombre}** ({res[mejor][last]:F2} ms) " +
                      $"y el más lento **{Competidores[peor].Nombre}** ({res[peor][last]:F2} ms): " +
                      $"una diferencia de ~{res[peor][last] / res[mejor][last]:F0} veces.");
        sb.AppendLine($"- Al duplicar N (de {Tamanos[last - 1]:N0} a {Tamanos[last]:N0}), Inserción multiplicó su tiempo por ~{crecIns:F1} " +
                      $"(esperado ≈ 4 por ser O(n²)), mientras que MergeSort lo hizo por ~{crecMerge:F1} (esperado ≈ 2 por ser O(n log n)).");
        sb.AppendLine("- Burbuja es consistentemente peor que Inserción porque hace muchos más intercambios.");
        sb.AppendLine("- `Array.Sort` (introsort optimizado) suele ganar a MergeSort/QuickSort propios por optimizaciones internas.");
        sb.AppendLine("- Conclusión: los cuadráticos solo sirven para N pequeños; el costo crece demasiado rápido al escalar.");

        // Se guarda en la carpeta desde donde ejecutas `dotnet run` (la del proyecto, ej. c5)
        string ruta = Path.Combine(Directory.GetCurrentDirectory(), "informe.md");
        File.WriteAllText(ruta, sb.ToString(), Encoding.UTF8);

        Console.WriteLine($"Informe guardado en: {Path.GetFullPath(ruta)}");
    }

    // ───────────────────────── Algoritmos ─────────────────────────
    static void BubbleSort(int[] a)
    {
        int n = a.Length;
        for (int i = 0; i < n - 1; i++)
        {
            bool huboCambio = false;
            for (int j = 0; j < n - 1 - i; j++)
            {
                if (a[j] > a[j + 1])
                {
                    (a[j], a[j + 1]) = (a[j + 1], a[j]);
                    huboCambio = true;
                }
            }
            if (!huboCambio) break;
        }
    }

    static void InsertionSort(int[] a)
    {
        for (int i = 1; i < a.Length; i++)
        {
            int clave = a[i];
            int j = i - 1;
            while (j >= 0 && a[j] > clave)
            {
                a[j + 1] = a[j];
                j--;
            }
            a[j + 1] = clave;
        }
    }

    static void MergeSort(int[] a)
    {
        int[] aux = new int[a.Length];
        MergeSortRec(a, aux, 0, a.Length - 1);
    }

    static void MergeSortRec(int[] a, int[] aux, int lo, int hi)
    {
        if (lo >= hi) return;
        int mid = lo + (hi - lo) / 2;
        MergeSortRec(a, aux, lo, mid);
        MergeSortRec(a, aux, mid + 1, hi);

        int i = lo, j = mid + 1, k = lo;
        while (i <= mid && j <= hi)
            aux[k++] = a[i] <= a[j] ? a[i++] : a[j++];
        while (i <= mid) aux[k++] = a[i++];
        while (j <= hi) aux[k++] = a[j++];
        Array.Copy(aux, lo, a, lo, hi - lo + 1);
    }

    static void QuickSort(int[] a, int lo, int hi)
    {
        // Recursión solo sobre la mitad menor → profundidad máxima O(log n)
        while (lo < hi)
        {
            int p = Particionar(a, lo, hi);
            if (p - lo < hi - p)
            {
                QuickSort(a, lo, p - 1);
                lo = p + 1;
            }
            else
            {
                QuickSort(a, p + 1, hi);
                hi = p - 1;
            }
        }
    }

    static int Particionar(int[] a, int lo, int hi)
    {
        // Pivote = elemento central (evita el peor caso con datos ya ordenados)
        int mid = lo + (hi - lo) / 2;
        (a[mid], a[hi]) = (a[hi], a[mid]);
        int pivote = a[hi];

        int i = lo;
        for (int j = lo; j < hi; j++)
        {
            if (a[j] < pivote)
            {
                (a[i], a[j]) = (a[j], a[i]);
                i++;
            }
        }
        (a[i], a[hi]) = (a[hi], a[i]);
        return i;
    }
}