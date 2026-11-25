// CLASE 11 (segunda mitad) · ALGORITMOS VORACES (GREEDY)
// Un algoritmo voraz resuelve de a un paso, SIEMPRE eligiendo lo que parece mejor en ese momento, sin volver atrás.
// Es rapidísimo, pero solo es correcto en ciertos problemas: hay que PROBAR cuándo no engaña.
// Todo lo que imprime este programa es exacto, salvo la última tabla (tiempos), que varía con la máquina.

using System.Diagnostics;

namespace C11.Voraces;

public static class Demo
{
    // ---------- PARTE A · EL VUELTO CON LAS MONEDAS MÁS GRANDES PRIMERO ----------
    private static List<int> VueltoVoraz(int monto, int[] monedasDeMayorAMenor)
    {
        var usadas = new List<int>();
        foreach (int moneda in monedasDeMayorAMenor)
            while (monto >= moneda) { usadas.Add(moneda); monto -= moneda; }
        return usadas;
    }

    public static void Vuelto()
    {
        Console.WriteLine("PARTE A · EL VUELTO: siempre la moneda MÁS GRANDE que quepa");
        var monedas = new[] { 50, 20, 10, 5, 2, 1 };
        Console.WriteLine($"  Monedas y billetes: {string.Join(", ", monedas)}");
        foreach (int monto in new[] { 87, 43, 99 })
        {
            var usadas = VueltoVoraz(monto, monedas);
            Console.WriteLine($"  Vuelto de {monto,3}: {string.Join(" + ", usadas)}   ({usadas.Count} piezas)");
        }
        Console.WriteLine("  Con este sistema de monedas el voraz SIEMPRE da el mínimo de piezas.");
    }

    // ---------- PARTE B · CUANDO EL ATAJO ENGAÑA ----------
    // El mínimo REAL, probando todas las combinaciones (fuerza bruta recursiva; solo para montos chicos).
    private static int MinimoPiezas(int monto, int[] monedas)
    {
        if (monto == 0) return 0;
        int mejor = int.MaxValue;
        foreach (int m in monedas)
            if (m <= monto)
            {
                int r = MinimoPiezas(monto - m, monedas);
                if (r != int.MaxValue) mejor = Math.Min(mejor, r + 1);
            }
        return mejor;
    }

    public static void Engano()
    {
        Console.WriteLine("PARTE B · UN SISTEMA DE MONEDAS DONDE EL VORAZ SE EQUIVOCA");
        var monedas = new[] { 4, 3, 1 };
        Console.WriteLine($"  Monedas del país de «Voracia»: {string.Join(", ", monedas)}");
        Console.WriteLine($"  {"monto",6} {"voraz",-18} {"piezas",7} {"mínimo real",12} {"¿acierta?",10}");
        foreach (int monto in new[] { 5, 6, 7, 8, 9, 10 })
        {
            var voraz = VueltoVoraz(monto, monedas);
            int real = MinimoPiezas(monto, monedas);
            Console.WriteLine($"  {monto,6} {string.Join("+", voraz),-18} {voraz.Count,7} {real,12} {(voraz.Count == real ? "sí" : "NO"),10}");
        }
        Console.WriteLine("  Con 6: el voraz toma el 4 y se queda con 1+1 (3 piezas); lo mejor era 3+3 (2 piezas).");
        Console.WriteLine("  Elegir lo mejor AHORA no siempre lleva a lo mejor AL FINAL. Un voraz necesita una demostración, o un contraejemplo lo derriba.");
    }

    // ---------- PARTE C · SELECCIÓN DE ACTIVIDADES: TRES VORACES, UNO CORRECTO ----------
    private sealed record Actividad(string Nombre, int Inicio, int Fin)
    {
        public int Duracion => Fin - Inicio;
        public override string ToString() => $"{Nombre}({Inicio}-{Fin})";
    }

    private static List<Actividad> ElegirVoraz(IEnumerable<Actividad> ordenadas)
    {
        var elegidas = new List<Actividad>();
        int libreDesde = int.MinValue;
        foreach (var a in ordenadas)
            if (a.Inicio >= libreDesde) { elegidas.Add(a); libreDesde = a.Fin; }
        return elegidas;
    }

    private static int MaximoPorFuerzaBruta(Actividad[] a)                 // prueba los 2^n subconjuntos
    {
        int mejor = 0;
        for (int mascara = 0; mascara < (1 << a.Length); mascara++)
        {
            var elegidas = new List<Actividad>();
            for (int i = 0; i < a.Length; i++) if ((mascara & (1 << i)) != 0) elegidas.Add(a[i]);
            elegidas.Sort((x, y) => x.Inicio.CompareTo(y.Inicio));
            bool ok = true;
            for (int i = 1; i < elegidas.Count && ok; i++) if (elegidas[i].Inicio < elegidas[i - 1].Fin) ok = false;
            if (ok) mejor = Math.Max(mejor, elegidas.Count);
        }
        return mejor;
    }

    public static void Actividades()
    {
        Console.WriteLine("PARTE C · UNA SALA, MUCHAS ACTIVIDADES: ¿cuántas caben sin solaparse?");
        var todas = new[]
        {
            new Actividad("Larga", 0, 40),
            new Actividad("A1", 1, 5), new Actividad("A2", 4, 7), new Actividad("A3", 6, 11),
            new Actividad("B1", 12, 17), new Actividad("B2", 16, 19), new Actividad("B3", 18, 23),
            new Actividad("C1", 24, 29), new Actividad("C2", 28, 31), new Actividad("C3", 30, 35),
        };
        Console.WriteLine($"  Actividades: {string.Join(", ", todas.Select(a => a.ToString()))}");
        Console.WriteLine();
        Console.WriteLine($"  {"criterio voraz",-34} {"elige",-45} {"cuántas",8}");
        foreach (var (nombre, orden) in new (string, IEnumerable<Actividad>)[]
        {
            ("la que EMPIEZA primero",          todas.OrderBy(a => a.Inicio).ThenBy(a => a.Fin)),
            ("la MÁS CORTA",                    todas.OrderBy(a => a.Duracion).ThenBy(a => a.Inicio)),
            ("la que TERMINA primero",          todas.OrderBy(a => a.Fin).ThenBy(a => a.Inicio)),
        })
        {
            var e = ElegirVoraz(orden);
            Console.WriteLine($"  {nombre,-34} {string.Join(" ", e.Select(a => a.Nombre)),-45} {e.Count,8}");
        }
        int optimo = MaximoPorFuerzaBruta(todas);
        Console.WriteLine($"  Probando los {1 << todas.Length:N0} subconjuntos (fuerza bruta): el máximo posible es {optimo}.");
        Console.WriteLine();
        Console.WriteLine("  Dos criterios «razonables» fallan; «terminar primero» da el óptimo: deja la sala libre lo antes posible.");
    }

    // ---------- PARTE D · EL VORAZ CONTRA LA FUERZA BRUTA, MEDIDO ----------
    public static void Costo()
    {
        Console.WriteLine("PARTE D · MISMO PROBLEMA, DOS ESTRATEGIAS (actividades al azar, mismo resultado)");
        Console.WriteLine($"  {"n",4} {"óptimo",7} {"voraz (terminar primero) ms",29} {"fuerza bruta (2^n) ms",23} {"x",6}");
        double prev = 0;
        foreach (int n in new[] { 14, 16, 18, 20, 22 })
        {
            var azar = new Random(n);
            var acts = Enumerable.Range(0, n).Select(i =>
            {
                int ini = azar.Next(0, 200);
                return new Actividad("T" + i, ini, ini + azar.Next(3, 25));
            }).ToArray();
            int voraz = 0, bruta = 0;
            double tv = Promedio(() => voraz = ElegirVoraz(acts.OrderBy(a => a.Fin).ThenBy(a => a.Inicio)).Count);
            double tb = Promedio(() => bruta = MaximoPorFuerzaBruta(acts));
            Console.WriteLine($"  {n,4} {bruta,7} {tv,29:0.000} {tb,23:0.0} {(prev == 0 ? "—" : "x" + (tb / prev).ToString("0.0")),6}{(voraz == bruta ? "" : "   ← NO COINCIDEN")}");
            prev = tb;
        }
        Console.WriteLine("  Cada 2 actividades más, la fuerza bruta cuesta ~x4 (2^n): se vuelve imposible con n = 40. El voraz sigue en microsegundos (n log n).");
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
        Vuelto();
        Console.WriteLine();
        Console.WriteLine(new string('=', 100));
        Engano();
        Console.WriteLine();
        Console.WriteLine(new string('=', 100));
        Actividades();
        Console.WriteLine();
        Console.WriteLine(new string('=', 100));
        Costo();
    }
}
