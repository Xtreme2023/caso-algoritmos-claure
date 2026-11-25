// CLASE 12 · PROGRAMACIÓN DINÁMICA Y PANORAMA NP
// Programación dinámica = anotar lo ya calculado (la libreta del contador) para no repetir el mismo subproblema.
// Las partes A (conteos), B y C son exactas. Las partes con milisegundos (A, D y E) usan el protocolo
// (calentar, repetir 3 veces, promediar) y varían con la máquina.

using System.Diagnostics;

namespace C12.Dinamica;

public static class Demo
{
    // ---------- PARTE A · FIBONACCI: SIN MEMORIA, CON MEMORIA, CON TABLA ----------
    private static long _llamadas;

    private static long FibSinMemoria(int n)
    {
        _llamadas++;
        return n < 2 ? n : FibSinMemoria(n - 1) + FibSinMemoria(n - 2);
    }

    private static long FibConMemoria(int n, long[] libreta)
    {
        _llamadas++;
        if (n < 2) return n;
        if (libreta[n] != 0) return libreta[n];                     // ya lo calculé: lo leo de la libreta
        return libreta[n] = FibConMemoria(n - 1, libreta) + FibConMemoria(n - 2, libreta);
    }

    private static long FibConTabla(int n)                           // de abajo hacia arriba, sin recursión
    {
        if (n < 2) return n;
        long anterior = 0, actual = 1;
        for (int i = 2; i <= n; i++) (anterior, actual) = (actual, anterior + actual);
        return actual;
    }

    public static void Fibonacci()
    {
        Console.WriteLine("PARTE A · FIBONACCI: la misma cuenta, tres maneras");
        Console.WriteLine($"  {"n",3} {"llamadas SIN memoria",22} {"llamadas CON memoria",22}   {"Fib(n)",18}");
        foreach (int n in new[] { 10, 20, 30, 40 })
        {
            _llamadas = 0; long a = FibSinMemoria(n); long sin = _llamadas;
            _llamadas = 0; long b = FibConMemoria(n, new long[n + 1]); long con = _llamadas;
            Console.WriteLine($"  {n,3} {sin,22:N0} {con,22:N0}   {a,18:N0}{(a == b && a == FibConTabla(n) ? "" : "  ← NO COINCIDEN")}");
        }
        Console.WriteLine("  Sin memoria el costo se multiplica por ~1,6 cada +1 (exponencial). Con memoria, 2n-1 llamadas: lineal.");
        Console.WriteLine();
        Console.WriteLine($"  {"n",3} {"SIN memoria ms",16} {"x",6} {"CON memoria ms",16}");
        double prev = 0;
        foreach (int n in new[] { 30, 35, 40 })
        {
            double t1 = Promedio(() => FibSinMemoria(n));
            double t2 = Promedio(() => FibConMemoria(n, new long[n + 1]));
            Console.WriteLine($"  {n,3} {t1,16:0.0} {(prev == 0 ? "—" : "x" + (t1 / prev).ToString("0.0")),6} {t2,16:0.0000}");
            prev = t1;
        }
        Console.WriteLine();
        long f90 = FibConTabla(90);
        Console.WriteLine($"  Con memoria o tabla, Fib(90) = {f90:N0} sale al instante.");
        Console.WriteLine("  Sin memoria necesitaría ~9 × 10^18 llamadas: unos 600 años a 500 millones por segundo (PROYECCIÓN, no medida).");
    }

    // ---------- PARTE B · LA MOCHILA 0/1, CON TABLA ----------
    private static (int mejor, List<int> elegidos) Mochila(int[] pesos, int[] valores, int capacidad, bool imprimir)
    {
        int n = pesos.Length;
        var t = new int[n + 1, capacidad + 1];
        for (int i = 1; i <= n; i++)
            for (int c = 0; c <= capacidad; c++)
            {
                t[i, c] = t[i - 1, c];                                                  // no llevo el objeto i
                if (pesos[i - 1] <= c) t[i, c] = Math.Max(t[i, c], t[i - 1, c - pesos[i - 1]] + valores[i - 1]);   // o lo llevo
            }
        if (imprimir)
        {
            Console.WriteLine($"    {"capacidad →",12} " + string.Join(" ", Enumerable.Range(0, capacidad + 1).Select(c => $"{c,2}")));
            for (int i = 0; i <= n; i++)
                Console.WriteLine($"    {(i == 0 ? "sin objetos" : $"hasta obj {i}"),12} " + string.Join(" ", Enumerable.Range(0, capacidad + 1).Select(c => $"{t[i, c],2}")));
        }
        var elegidos = new List<int>();
        for (int i = n, c = capacidad; i > 0; i--)
            if (t[i, c] != t[i - 1, c]) { elegidos.Add(i); c -= pesos[i - 1]; }          // si el valor cambió, el objeto i está
        elegidos.Reverse();
        return (t[n, capacidad], elegidos);
    }

    public static void MochilaPequena()
    {
        Console.WriteLine("PARTE B · LA MOCHILA: cada objeto tiene peso y valor; la mochila aguanta una capacidad");
        var pesos = new[] { 2, 3, 4, 5 };
        var valores = new[] { 3, 4, 5, 6 };
        Console.WriteLine("  Objetos (peso, valor): 1 (2, 3) · 2 (3, 4) · 3 (4, 5) · 4 (5, 6) · capacidad 8");
        var (mejor, elegidos) = Mochila(pesos, valores, 8, true);
        Console.WriteLine();
        Console.WriteLine($"  Valor máximo con capacidad 8: {mejor}  → se llevan los objetos {string.Join(" y ", elegidos)}");
        Console.WriteLine("  Cada celda responde: «con estos objetos y esta capacidad, ¿cuánto valor máximo cabe?» y usa SOLO celdas ya calculadas.");
    }

    // ---------- PARTE C · EL VORAZ FALLA, LA DINÁMICA ACIERTA ----------
    public static void VorazContraDinamica()
    {
        Console.WriteLine("PARTE C · LA MOCHILA DONDE EL VORAZ SE EQUIVOCA (la lección de la clase 11)");
        var pesos = new[] { 10, 20, 30 };
        var valores = new[] { 60, 100, 120 };
        const int capacidad = 50;
        Console.WriteLine($"  Objetos (peso, valor): (10, 60) · (20, 100) · (30, 120) · capacidad {capacidad}");
        Console.WriteLine("  Voraz «mayor valor por kilo»: razones 6, 5 y 4 → lleva (10, 60) y (20, 100): 160. El (30, 120) ya no cabe.");
        var (mejor, elegidos) = Mochila(pesos, valores, capacidad, false);
        Console.WriteLine($"  Programación dinámica: {mejor}  → objetos {string.Join(" y ", elegidos)} (pesos {string.Join(" + ", elegidos.Select(i => pesos[i - 1]))} = {elegidos.Sum(i => pesos[i - 1])})");
        Console.WriteLine($"  Fuerza bruta (probar los {1 << pesos.Length} subconjuntos): {FuerzaBruta(pesos, valores, capacidad)}");
    }

    private static int FuerzaBruta(int[] pesos, int[] valores, int capacidad)
    {
        int n = pesos.Length, mejor = 0;
        for (int mascara = 0; mascara < (1 << n); mascara++)
        {
            int p = 0, v = 0;
            for (int i = 0; i < n; i++) if ((mascara & (1 << i)) != 0) { p += pesos[i]; v += valores[i]; }
            if (p <= capacidad && v > mejor) mejor = v;
        }
        return mejor;
    }

    // ---------- PARTE D · DINÁMICA CONTRA FUERZA BRUTA, MEDIDO ----------
    public static void Costo()
    {
        Console.WriteLine("PARTE D · MOCHILA: tabla (n × capacidad) CONTRA probar todos los subconjuntos (2^n)");
        Console.WriteLine($"  {"objetos",8} {"valor óptimo",13} {"tabla ms",10} {"fuerza bruta ms",17} {"x",6}");
        double prev = 0;
        foreach (int n in new[] { 16, 18, 20, 22 })
        {
            var azar = new Random(n);
            var pesos = Enumerable.Range(0, n).Select(_ => azar.Next(5, 60)).ToArray();
            var valores = Enumerable.Range(0, n).Select(_ => azar.Next(10, 100)).ToArray();
            int capacidad = pesos.Sum() / 3, a = 0, b = 0;
            double t1 = Promedio(() => a = Mochila(pesos, valores, capacidad, false).mejor);
            double t2 = Promedio(() => b = FuerzaBruta(pesos, valores, capacidad));
            Console.WriteLine($"  {n,8} {a,13} {t1,10:0.000} {t2,17:0.0} {(prev == 0 ? "—" : "x" + (t2 / prev).ToString("0.0")),6}{(a == b ? "" : "   ← NO COINCIDEN")}");
            prev = t2;
        }
        Console.WriteLine("  Cada 2 objetos más, la fuerza bruta cuesta ~x4 (2^n). La tabla cuesta n × capacidad: crece de a poco.");
        var az = new Random(1);
        var pg = Enumerable.Range(0, 1000).Select(_ => az.Next(5, 60)).ToArray();
        var vg = Enumerable.Range(0, 1000).Select(_ => az.Next(10, 100)).ToArray();
        double tg = Promedio(() => Mochila(pg, vg, 10_000, false));
        Console.WriteLine($"  Con 1.000 objetos y capacidad 10.000 la tabla tarda {tg:0.0} ms (10 millones de celdas). La fuerza bruta probaría 2^1000 subconjuntos: imposible.");
    }

    // ---------- PARTE E · PANORAMA HONESTO: EL AGENTE VIAJERO ----------
    private static double[,] Distancias(int n)
    {
        var azar = new Random(100 + n);
        var x = Enumerable.Range(0, n).Select(_ => azar.NextDouble() * 100).ToArray();
        var y = Enumerable.Range(0, n).Select(_ => azar.NextDouble() * 100).ToArray();
        var d = new double[n, n];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++) d[i, j] = Math.Sqrt((x[i] - x[j]) * (x[i] - x[j]) + (y[i] - y[j]) * (y[i] - y[j]));
        return d;
    }

    private static double MejorRuta(double[,] d, int n)
    {
        var ciudades = Enumerable.Range(0, n).ToArray();
        double mejor = double.MaxValue;
        Permutar(d, ciudades, 1, n, 0, ref mejor);                              // la ciudad 0 queda fija: es el punto de partida
        return mejor;
    }

    private static void Permutar(double[,] d, int[] ciudades, int k, int n, double acumulado, ref double mejor)
    {
        if (k == n)
        {
            double total = acumulado + d[ciudades[n - 1], 0];                  // vuelve al inicio
            if (total < mejor) mejor = total;
            return;
        }
        for (int i = k; i < n; i++)
        {
            (ciudades[k], ciudades[i]) = (ciudades[i], ciudades[k]);
            Permutar(d, ciudades, k + 1, n, acumulado + d[ciudades[k - 1], ciudades[k]], ref mejor);
            (ciudades[k], ciudades[i]) = (ciudades[i], ciudades[k]);
        }
    }

    public static void Viajante()
    {
        Console.WriteLine("PARTE E · EL AGENTE VIAJERO: visitar n ciudades y volver, por el camino más corto");
        Console.WriteLine("  No se conoce un algoritmo eficiente (polinomial) para el caso general: es un problema NP-difícil.");
        Console.WriteLine();
        var calentar = Distancias(9);
        var tiempoDeCalentamiento = Stopwatch.StartNew();
        while (tiempoDeCalentamiento.ElapsedMilliseconds < 600) MejorRuta(calentar, 9);   // que el JIT optimice ANTES de medir
        Console.WriteLine($"  {"ciudades",9} {"recorridos posibles (n-1)!",27} {"ms (probando todos)",22} {"x",6}");
        double prev = 0;
        foreach (int n in new[] { 8, 9, 10, 11 })
        {
            var d = Distancias(n);
            long recorridos = 1; for (int i = 2; i < n; i++) recorridos *= i;
            double t = Promedio(() => MejorRuta(d, n));
            Console.WriteLine($"  {n,9} {recorridos,27:N0} {t,22:0.0} {(prev == 0 ? "—" : "x" + (t / prev).ToString("0.0")),6}");
            prev = t;
        }
        Console.WriteLine();
        Console.WriteLine("  Cada ciudad más multiplica el trabajo por ~n. PROYECCIÓN (no medida): con 20 ciudades, 19! ≈ 1,2 × 10^17 recorridos.");
        Console.WriteLine("  NP no significa «imposible»: significa que nadie sabe resolverlo rápido en general. En la práctica se usan aproximaciones, heurísticas y casos pequeños.");
    }

    private static double Promedio(Action accion)
    {
        accion();
        double suma = 0;
        for (int i = 0; i < 3; i++)
        {
            var reloj = Stopwatch.StartNew();
            accion();
            reloj.Stop();
            suma += reloj.Elapsed.TotalMilliseconds;
        }
        return suma / 3;
    }

    public static void Correr()
    {
        Fibonacci();
        Console.WriteLine();
        Console.WriteLine(new string('=', 100));
        MochilaPequena();
        Console.WriteLine();
        Console.WriteLine(new string('=', 100));
        VorazContraDinamica();
        Console.WriteLine();
        Console.WriteLine(new string('=', 100));
        Costo();
        Console.WriteLine();
        Console.WriteLine(new string('=', 100));
        Viajante();
    }
}
