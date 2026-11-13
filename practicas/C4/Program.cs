using System.Diagnostics;

namespace TorneoOrdenamiento;

static class Busquedas
{
    const int Corridas = 3;

    public static void Ejecutar()
    {
        Console.WriteLine("\n=== BÚSQUEDA LINEAL vs BINARIA ===");
        Calentar();
        ContarPasos();
        MedirTiempos();
        BinariaExigeOrden();
        CuantoCuestaOrdenar();
    }

    // ───────────────────────── 1) Pasos ─────────────────────────
    static void ContarPasos()
    {
        Console.WriteLine("\n--- 1) Pasos (comparaciones) necesarios ---");
        Console.WriteLine("Peor caso = buscar algo que NO existe. Promedio = 1.000 búsquedas de valores que sí existen.\n");
        Console.WriteLine($"{"N",10} | {"Lineal (peor)",14} | {"Binaria (peor)",14} | {"Lineal (prom)",14} | {"Binaria (prom)",14} | log2(N)");
        Console.WriteLine(new string('-', 92));

        var rnd = new Random(1);
        int ultimoL = 0, ultimoB = 0;

        foreach (int n in new[] { 1_000, 100_000, 1_000_000 })
        {
            int[] a = Ordenado(n);

            Lineal(a, -1, out int peorL);
            Binaria(a, -1, out int peorB);
            ultimoL = peorL;
            ultimoB = peorB;

            const int q = 1_000;
            long sumL = 0, sumB = 0;
            for (int i = 0; i < q; i++)
            {
                int x = a[rnd.Next(n)];
                Lineal(a, x, out int pl);
                Binaria(a, x, out int pb);
                sumL += pl;
                sumB += pb;
            }

            double promL = sumL / (double)q;
            double promB = sumB / (double)q;
            Console.WriteLine($"{n,10:N0} | {peorL,14:N0} | {peorB,14:N0} | {promL,14:N0} | {promB,14:F1} | {Math.Log2(n):F1}");
        }

        Console.WriteLine($"\nCon un millón de datos: {ultimoL:N0} pasos (lineal) contra {ultimoB} (binaria).");
    }

    // ───────────────────────── 2) Tiempos ─────────────────────────
    static void MedirTiempos()
    {
        const int n = 1_000_000;
        const int q = 1_000;
        Console.WriteLine($"\n--- 2) Tiempo: {q:N0} búsquedas en {n:N0} datos ordenados ({Corridas} corridas, promedio) ---\n");

        int[] a = Ordenado(n);
        var rnd = new Random(2);
        int[] objetivos = new int[q];
        for (int i = 0; i < q; i++) objetivos[i] = a[rnd.Next(n)];

        long chkL = 0, chkB = 0;
        double tL = Promedio(() => chkL = BuscarTodos(a, objetivos, q, false));
        double tB = Promedio(() => chkB = BuscarTodos(a, objetivos, q, true));

        Console.WriteLine($"  Lineal  : {tL,10:F3} ms");
        Console.WriteLine($"  Binaria : {tB,10:F3} ms");
        Console.WriteLine($"  La binaria fue ~{tL / Math.Max(tB, 0.0001):N0} veces más rápida.");
        Console.WriteLine($"  Mismos resultados: {(chkL == chkB ? "✔" : "✘")}");
        Console.WriteLine("  (tus milisegundos serán distintos a los de otra máquina; los pasos del punto 1 no)");
    }

    // ───────────────────────── 3) Binaria exige orden ─────────────────────────
    static void BinariaExigeOrden()
    {
        Console.WriteLine("\n--- 3) La binaria SOLO funciona con datos ORDENADOS ---\n");

        int[] desordenado = { 42, 7, 19, 3, 88, 25, 61 };
        int x = 7;

        Console.WriteLine($"Datos sin ordenar: [{string.Join(", ", desordenado)}]   buscamos {x}");
        int iL = Lineal(desordenado, x, out _);
        int iB = Binaria(desordenado, x, out _);
        Console.WriteLine($"  Lineal  → índice {iL}");
        Console.WriteLine($"  Binaria → índice {iB}   {(iB == -1 ? "← NO lo encuentra aunque está en el arreglo ✘" : "(acertó de casualidad)")}");

        int[] ordenado = (int[])desordenado.Clone();
        Array.Sort(ordenado);
        Console.WriteLine($"\nDatos ordenados:   [{string.Join(", ", ordenado)}]   buscamos {x}");
        int iB2 = Binaria(ordenado, x, out _);
        Console.WriteLine($"  Binaria → índice {iB2}   {(iB2 != -1 ? "✔ ahora sí" : "✘")}");
    }

    // ───────────────────────── 4) ¿Cuándo conviene ordenar? ─────────────────────────
    static void CuantoCuestaOrdenar()
    {
        const int n = 1_000_000;
        Console.WriteLine($"\n--- 4) Ordenar cuesta una vez: ¿cuándo se paga? ({n:N0} datos) ---\n");

        var rnd = new Random(7);
        int[] datos = new int[n];
        for (int i = 0; i < n; i++) datos[i] = rnd.Next();

        int[] objetivos = new int[100_000];
        for (int i = 0; i < objetivos.Length; i++) objetivos[i] = datos[rnd.Next(n)];

        // Costo de ordenar (una vez)
        double tOrd = 0;
        for (int r = 0; r < Corridas; r++)
        {
            int[] copia = (int[])datos.Clone();
            var sw = Stopwatch.StartNew();
            Array.Sort(copia);
            sw.Stop();
            tOrd += sw.Elapsed.TotalMilliseconds;
        }
        tOrd /= Corridas;

        int[] ordenados = (int[])datos.Clone();
        Array.Sort(ordenados);

        // Costo de UNA búsqueda
        const int qLineal = 200;
        double tLineal = Promedio(() => BuscarTodos(datos, objetivos, qLineal, false)) / qLineal;
        double tBinaria = Promedio(() => BuscarTodos(ordenados, objetivos, objetivos.Length, true)) / objetivos.Length;

        Console.WriteLine($"  Ordenar una vez        : {tOrd,10:F2} ms");
        Console.WriteLine($"  1 búsqueda lineal      : {tLineal * 1000,10:F2} µs  (datos sin ordenar)");
        Console.WriteLine($"  1 búsqueda binaria     : {tBinaria * 1000,10:F2} µs  (datos ordenados)\n");

        Console.WriteLine($"{"Búsquedas",10} | {"Solo lineal",16} | {"Ordenar + binaria",18} | Conviene");
        Console.WriteLine(new string('-', 66));
        foreach (int k in new[] { 1, 10, 100, 1_000, 10_000 })
        {
            double totalLineal = k * tLineal;
            double totalOrdenar = tOrd + k * tBinaria;
            string gana = totalLineal <= totalOrdenar ? "lineal" : "ordenar + binaria";
            Console.WriteLine($"{k,10:N0} | {totalLineal,13:F2} ms | {totalOrdenar,15:F2} ms | {gana}");
        }

        double equilibrio = tOrd / Math.Max(tLineal - tBinaria, 1e-9);
        Console.WriteLine($"\nPunto de equilibrio: a partir de ~{Math.Ceiling(equilibrio):N0} búsquedas sobre los mismos datos, ordenar se paga solo.");
    }

    // ───────────────────────── Algoritmos ─────────────────────────
    static int Lineal(int[] a, int x, out int pasos)
    {
        pasos = 0;
        for (int i = 0; i < a.Length; i++)
        {
            pasos++;
            if (a[i] == x) return i;
        }
        return -1;
    }

    static int Binaria(int[] a, int x, out int pasos)
    {
        pasos = 0;
        int lo = 0, hi = a.Length - 1;
        while (lo <= hi)
        {
            pasos++;
            int mid = lo + (hi - lo) / 2;
            if (a[mid] == x) return mid;
            if (a[mid] < x) lo = mid + 1;
            else hi = mid - 1;
        }
        return -1;
    }

    // ───────────────────────── Utilidades ─────────────────────────
    // Números pares ordenados: 0, 2, 4, ... (los impares no existen → sirven como "no encontrado")
    static int[] Ordenado(int n)
    {
        var a = new int[n];
        for (int i = 0; i < n; i++) a[i] = 2 * i;
        return a;
    }

    static long BuscarTodos(int[] a, int[] objetivos, int cantidad, bool binaria)
    {
        long suma = 0;
        for (int i = 0; i < cantidad; i++)
            suma += binaria ? Binaria(a, objetivos[i], out _) : Lineal(a, objetivos[i], out _);
        return suma;
    }

    static double Promedio(Action accion)
    {
        double total = 0;
        for (int r = 0; r < Corridas; r++)
        {
            var sw = Stopwatch.StartNew();
            accion();
            sw.Stop();
            total += sw.Elapsed.TotalMilliseconds;
        }
        return total / Corridas;
    }

    static void Calentar()
    {
        int[] a = Ordenado(1_000);
        long s = 0;
        for (int i = 0; i < 3_000; i++)
        {
            s += Lineal(a, a[i % a.Length], out _);
            s += Binaria(a, a[i % a.Length], out _);
        }
        if (s == -1) Console.WriteLine(); // evita que el compilador descarte el calentamiento
    }
}

class Program
{
    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Busquedas.Ejecutar();
        Console.WriteLine("\nListo. Presiona Enter para salir...");
        Console.ReadLine();
    }
}
