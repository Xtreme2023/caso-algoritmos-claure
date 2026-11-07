// CLASE 4 · REGLAS DE LA O GRANDE — cómo se simplifica una complejidad
// Regla 0: ignorar constantes        3n      → O(n)
// Regla 1: ignorar términos menores  n² + n  → O(n²)
// Regla 2: suma (bloques seguidos)   O(n) + O(n²) = O(n²)
// Regla 3: producto (anidados)       O(n) × O(n)  = O(n²)
// Cada regla viene con su tabla: sin tabla no hay afirmación.

using System.Diagnostics;

namespace C4.ReglasBigO;

public static class Demo
{
    static long sumidero; // evita que el compilador descarte el trabajo

    public static void Correr()
    {
        Teoria();
        Practica();
        ReglaConstantes();
        ReglaTerminosMenores();
        ReglaSuma();
        ReglaProducto();

        Console.WriteLine();
        Console.WriteLine("RESUMEN: se descartan constantes y términos menores; en bloques seguidos gana el peor;");
        Console.WriteLine("en ciclos anidados se multiplica. O grande describe el PEOR caso y la FORMA de crecer.");
    }

    // ── TEORÍA ───────────────────────────────────────────────────────────────
    static void Teoria()
    {
        Console.WriteLine("TEORÍA — operaciones según el orden de crecimiento:");
        Console.WriteLine($"{"n",10} {"log n",8} {"n",10} {"n log n",12} {"n²",16}");
        foreach (var n in new[] { 10, 100, 1_000, 10_000 })
        {
            var log = Math.Log2(n);
            Console.WriteLine($"{n,10:N0} {log,8:0.0} {n,10:N0} {n * log,12:N0} {(long)n * n,16:N0}");
        }
    }

    // ── PRÁCTICA (igual que la Clase 2) ──────────────────────────────────────
    static void Practica()
    {
        Console.WriteLine();
        Console.WriteLine("PRÁCTICA — los mismos dos algoritmos, con 10 veces más datos cada vez:");
        Console.WriteLine($"{"n",10} {"suma O(n)",14} {"duplicados O(n²)",20}");
        foreach (var n in new[] { 2_000, 20_000, 200_000 })
        {
            var datos = Datos(n);
            var reloj = Stopwatch.StartNew();
            sumidero += Suma(datos);
            reloj.Stop();
            var msLineal = reloj.Elapsed.TotalMilliseconds;

            reloj.Restart();
            sumidero += Duplicados(datos);
            reloj.Stop();
            var msCuadratico = reloj.Elapsed.TotalMilliseconds;

            Console.WriteLine($"{n,10:N0} {msLineal,11:0.00} ms {msCuadratico,17:N0} ms");
        }
        Console.WriteLine();
        Console.WriteLine("Datos x10  →  el O(n²) tarda ~x100 (columna derecha). El O(n) tarda tan poco que apenas se alcanza a medir.");
        Console.WriteLine("La forma de crecer ES la firma del algoritmo. Eso nombra la O grande.");
    }

    // ── REGLA 0 · Ignorar constantes: 3n → O(n) ─────────────────────────────
    static void ReglaConstantes()
    {
        Console.WriteLine();
        Console.WriteLine("REGLA 0 · Ignorar constantes: 3n pasos → O(n)   (1 calentamiento + 5 corridas + promedio)");
        Console.WriteLine($"{"n",12} {"n pasos",12} {"ratio",7} {"3n pasos",12} {"ratio",7} {"3n / n",8}");
        double ant1 = 0, ant3 = 0;
        foreach (var n in new[] { 300_000, 3_000_000, 30_000_000 })
        {
            var datos = Datos(n);
            double t1 = Medir(() => Suma(datos));
            double t3 = Medir(() => Suma(datos) + Suma(datos) + Suma(datos));   // el mismo ciclo 3 veces
            Console.WriteLine($"{n,12:N0} {t1,9:0.00} ms {R(t1, ant1),7} {t3,9:0.00} ms {R(t3, ant3),7} {$"x{t3 / t1:0.0}",8}");
            ant1 = t1; ant3 = t3;
        }
        Console.WriteLine("El 3 vive en la última columna: se queda en ~x3, no crece con n.");
        Console.WriteLine("Las dos columnas crecen IGUAL (misma forma) → las dos son O(n). El 3 se descarta.");
    }

    // ── REGLA 1 · Ignorar términos menores: n² + n → O(n²) ──────────────────
    static void ReglaTerminosMenores()
    {
        Console.WriteLine();
        Console.WriteLine("REGLA 1 · Ignorar términos menores: n² + n → O(n²)   (operaciones contadas)");
        Console.WriteLine($"{"n",10} {"n²",18} {"n",10} {"n² + n",18} {"aporte de n",13}");
        foreach (var n in new[] { 10, 100, 1_000, 10_000, 100_000 })
        {
            long n2 = (long)n * n, total = n2 + n;
            Console.WriteLine($"{n,10:N0} {n2,18:N0} {n,10:N0} {total,18:N0} {100.0 * n / total,11:0.000} %");
        }
        Console.WriteLine("Mientras n crece, el término n pesa cada vez menos: solo queda el dominante, n².");
    }

    // ── REGLA 2 · Suma: bloques seguidos, predomina el peor ─────────────────
    static void ReglaSuma()
    {
        Console.WriteLine();
        Console.WriteLine("REGLA 2 · Suma: bloque O(n) y DESPUÉS bloque O(n²) → O(n) + O(n²) = O(n²)");
        Console.WriteLine($"{"n",10} {"total",15} {"ratio",7} {"parte O(n)",13} {"% del total",12}");
        double ant = 0;
        foreach (var n in new[] { 1_000, 10_000, 100_000 })
        {
            var datos = Datos(n);
            double total = Medir(() => Suma(datos) + Duplicados(datos));
            double lineal = Medir(() => Suma(datos));
            Console.WriteLine($"{n,10:N0} {total,12:#,##0.000} ms {R(total, ant),7} {lineal,10:0.000} ms {100 * lineal / total,10:0.000} %");
            ant = total;
        }
        Console.WriteLine("El total crece ~x100 como el bloque O(n²); la parte O(n) casi no se nota.");
    }

    // ── REGLA 3 · Producto: ciclos anidados se multiplican ──────────────────
    static void ReglaProducto()
    {
        Console.WriteLine();
        Console.WriteLine("REGLA 3 · Producto: un ciclo dentro de otro → se multiplican");
        Console.WriteLine($"{"n",10} {"n × 100  O(n)·O(1)",20} {"ratio",7} {"n × n  O(n)·O(n)",20} {"ratio",7}");
        double antK = 0, antN = 0;
        foreach (var n in new[] { 300, 3_000, 30_000 })
        {
            var datos = Datos(n);
            double k = Medir(() => Anidado(datos, 100));
            double c = Medir(() => Anidado(datos, n));
            Console.WriteLine($"{n,10:N0} {k,17:0.000} ms {R(k, antK),7} {c,17:#,##0.000} ms {R(c, antN),7}");
            antK = k; antN = c;
        }
        Console.WriteLine("Ciclo interno de 100 vueltas fijas: O(n) × O(1) = O(n)  → ~x10.");
        Console.WriteLine("Ciclo interno que depende de n:     O(n) × O(n) = O(n²) → ~x100.");
    }

    // ── Algoritmos ───────────────────────────────────────────────────────────
    static long Suma(int[] d)                          // un ciclo → O(n)
    {
        long s = 0;
        foreach (var x in d) s += x;
        return s;
    }

    static long Duplicados(int[] d)                    // ciclo dentro de ciclo → O(n²)
    {
        long dup = 0;
        for (int i = 0; i < d.Length; i++)
            for (int j = i + 1; j < d.Length; j++)
                if (d[i] == d[j]) dup++;
        return dup;
    }

    static long Anidado(int[] d, int vueltasInternas)  // n × m
    {
        long s = 0;
        for (int i = 0; i < d.Length; i++)
            for (int j = 0; j < vueltasInternas; j++)
                s += d[j % d.Length] ^ i;
        return s;
    }

    // ── Protocolo de la Clase 3 ──────────────────────────────────────────────
    static double Medir(Func<long> trabajo)
    {
        sumidero += trabajo();                         // calentamiento (se descarta)
        double suma = 0;
        var reloj = new Stopwatch();
        for (int corrida = 0; corrida < 5; corrida++)
        {
            reloj.Restart();
            sumidero += trabajo();
            reloj.Stop();
            suma += reloj.Elapsed.TotalMilliseconds;
        }
        return suma / 5;
    }

    static int[] Datos(int n)
    {
        var aleatorio = new Random(7);
        var datos = new int[n];
        for (int i = 0; i < n; i++) datos[i] = aleatorio.Next(1_000_000_000);
        return datos;
    }

    static string R(double actual, double anterior) => anterior > 0 ? $"x{actual / anterior:0.0}" : "—";
}
