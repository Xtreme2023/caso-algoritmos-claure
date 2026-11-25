// Cristofer Johany Claure Cabrera
// EVALUACIÓN INTEGRADORA · VARIANTE A — "Furgoneta de reparto"
// Caso: una distribuidora tiene una lista de paquetes (cada uno con un peso en kg y una ganancia en Bs) y una furgoneta
// que aguanta una capacidad en kg. Hay que elegir QUÉ paquetes llevar para ganar lo máximo sin pasarse de la capacidad.
// Cada paquete se lleva entero o no se lleva.
// PlanificadorIngenuo es CORRECTO pero LENTO (prueba todas las combinaciones). Tu trabajo: analizarlo, elegir la
// estrategia, APLICARLA en PlanificadorMejorado, medir la diferencia y justificar todo.
// Puedes usar este archivo tal cual (si tienes .NET) o portar todo a TU lenguaje.

using System.Diagnostics;

namespace Integradora.VarianteA;

public sealed record Paquete(string Codigo, int Kg, int Ganancia);

public interface IPlanificador
{
    // Devuelve la ganancia máxima y los códigos de los paquetes elegidos (en cualquier orden).
    (int ganancia, List<string> codigos) Elegir(Paquete[] paquetes, int capacidadKg);

    // Después de Elegir: ¿este paquete va en la furgoneta?
    bool EstaEnElViaje(string codigo);
}

// ---------- SOLUCIÓN INGENUA: prueba todos los subconjuntos (2^n) ----------
public sealed class PlanificadorIngenuo : IPlanificador
{
    private List<string> _elegidos = new();

    public (int ganancia, List<string> codigos) Elegir(Paquete[] paquetes, int capacidadKg)
    {
        int n = paquetes.Length, mejor = 0;
        long mejorMascara = 0;
        for (long mascara = 0; mascara < (1L << n); mascara++)
        {
            int kg = 0, ganancia = 0;
            for (int i = 0; i < n; i++)
                if ((mascara & (1L << i)) != 0) { kg += paquetes[i].Kg; ganancia += paquetes[i].Ganancia; }
            if (kg <= capacidadKg && ganancia > mejor) { mejor = ganancia; mejorMascara = mascara; }
        }
        _elegidos = new List<string>();
        for (int i = 0; i < n; i++) if ((mejorMascara & (1L << i)) != 0) _elegidos.Add(paquetes[i].Codigo);
        return (mejor, new List<string>(_elegidos));
    }

    public bool EstaEnElViaje(string codigo) => _elegidos.Contains(codigo);          // recorre la lista
}

// ---------- TU SOLUCIÓN: elige la estrategia y la(s) estructura(s) y escríbela aquí ----------
public sealed class PlanificadorMejorado : IPlanificador
{
    private HashSet<string> _elegidos = new(StringComparer.Ordinal);

    public (int ganancia, List<string> codigos) Elegir(Paquete[] paquetes, int capacidadKg)
    {
        ArgumentNullException.ThrowIfNull(paquetes);

        if (paquetes.Any(p => p is null))
            throw new ArgumentException("La lista no puede contener paquetes nulos.", nameof(paquetes));
        if (paquetes.Any(p => p.Kg < 0))
            throw new ArgumentException("El peso de cada paquete debe ser no negativo.", nameof(paquetes));

        var utiles = paquetes.Select((paquete, indice) => (paquete, indice))
            .Where(item => item.paquete.Ganancia > 0)
            .ToArray();
        long sumaGanancias = utiles.Sum(item => (long)item.paquete.Ganancia);
        if (sumaGanancias >= int.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(paquetes), "La suma de ganancias excede el rango que puede representar la tabla.");

        int maxGanancia = (int)sumaGanancias;
        var pesoMinimo = new long[maxGanancia + 1];
        Array.Fill(pesoMinimo, long.MaxValue);
        pesoMinimo[0] = 0;
        var llevar = new bool[utiles.Length, maxGanancia + 1];

        int gananciaProcesada = 0;
        for (int i = 0; i < utiles.Length; i++)
        {
            var paquete = utiles[i].paquete;
            for (int ganancia = gananciaProcesada; ganancia >= 0; ganancia--)
            {
                if (pesoMinimo[ganancia] == long.MaxValue)
                    continue;

                int nuevaGanancia = ganancia + paquete.Ganancia;
                long nuevoPeso = pesoMinimo[ganancia] + paquete.Kg;
                if (nuevoPeso <= capacidadKg && nuevoPeso < pesoMinimo[nuevaGanancia])
                {
                    pesoMinimo[nuevaGanancia] = nuevoPeso;
                    llevar[i, nuevaGanancia] = true;
                }
            }
            gananciaProcesada += paquete.Ganancia;
        }

        int mejorGanancia = maxGanancia;
        while (mejorGanancia > 0 && pesoMinimo[mejorGanancia] == long.MaxValue)
            mejorGanancia--;

        var indicesElegidos = new List<int>();
        int gananciaRestante = mejorGanancia;
        for (int i = utiles.Length - 1; i >= 0; i--)
        {
            if (!llevar[i, gananciaRestante])
                continue;

            indicesElegidos.Add(utiles[i].indice);
            gananciaRestante -= utiles[i].paquete.Ganancia;
        }

        indicesElegidos.Reverse();
        var codigos = indicesElegidos.Select(i => paquetes[i].Codigo).ToList();
        _elegidos = new HashSet<string>(codigos, StringComparer.Ordinal);
        return (mejorGanancia, new List<string>(codigos));
    }

    public bool EstaEnElViaje(string codigo) => _elegidos.Contains(codigo);
}

public static class Programa
{
    private static Paquete[] Lote(params (string c, int kg, int g)[] datos) => datos.Select(d => new Paquete(d.c, d.kg, d.g)).ToArray();

    // Los casos de prueba (todos con ÓPTIMO ÚNICO): (paquetes, capacidad, ganancia esperada, códigos esperados).
    private static readonly (string que, Paquete[] paquetes, int capacidad, int esperada, string[] codigos)[] Casos =
    {
        ("el voraz se equivoca", Lote(("P1", 10, 60), ("P2", 20, 100), ("P3", 30, 120)), 50, 220, new[] { "P2", "P3" }),
        ("cuatro paquetes",      Lote(("P1", 2, 3), ("P2", 3, 4), ("P3", 4, 5), ("P4", 5, 6)), 8, 10, new[] { "P2", "P4" }),
        ("capacidad cero",       Lote(("P1", 5, 10), ("P2", 7, 12)), 0, 0, Array.Empty<string>()),
        ("sin paquetes",         Lote(), 30, 0, Array.Empty<string>()),
        ("nada cabe",            Lote(("P1", 40, 90), ("P2", 50, 100)), 30, 0, Array.Empty<string>()),
        ("entran todos",         Lote(("P1", 5, 10), ("P2", 7, 12), ("P3", 3, 4)), 100, 26, new[] { "P1", "P2", "P3" }),
    };

    private static void Probar(string titulo, Func<IPlanificador> fabrica)
    {
        int bien = 0, total = 0;
        try
        {
            foreach (var (que, paquetes, capacidad, esperada, codigos) in Casos)
            {
                var plan = fabrica();
                var (ganancia, elegidos) = plan.Elegir(paquetes, capacidad);
                total++;
                bool ok = ganancia == esperada && elegidos.OrderBy(x => x).SequenceEqual(codigos.OrderBy(x => x));
                if (ok) bien++;
                else Console.WriteLine($"   MAL en «{que}»: esperado {esperada} con [{string.Join(",", codigos)}], obtuvo {ganancia} con [{string.Join(",", elegidos)}]");
            }
            var p = fabrica();
            p.Elegir(Casos[0].paquetes, Casos[0].capacidad);
            total += 2;
            if (p.EstaEnElViaje("P2")) bien++; else Console.WriteLine("   MAL en «EstaEnElViaje(P2)»: esperado True");
            if (!p.EstaEnElViaje("P1")) bien++; else Console.WriteLine("   MAL en «EstaEnElViaje(P1)»: esperado False");
            Console.WriteLine($"{titulo}: {bien} de {total} pruebas correctas");
        }
        catch (NotImplementedException) { Console.WriteLine($"{titulo}: (sin escribir todavía)"); }
    }

    // La CARGA para medir: n paquetes al azar (semilla fija) y una capacidad de un tercio del peso total.
    private static Paquete[] CrearCarga(int n)
    {
        var azar = new Random(n);
        return Enumerable.Range(0, n).Select(i => new Paquete("C" + i, azar.Next(5, 60), azar.Next(10, 100))).ToArray();
    }

    public static int EjecutarCarga(IPlanificador plan, int n)
    {
        var paquetes = CrearCarga(n);
        int capacidad = paquetes.Sum(p => p.Kg) / 3;
        var (ganancia, _) = plan.Elegir(paquetes, capacidad);
        return ganancia;
    }

    private static (double calentamientoMs, double[] tiemposMs) MedirCorridas(Action accion)
    {
        var relojCalentamiento = Stopwatch.StartNew();
        accion();
        relojCalentamiento.Stop();

        var tiempos = new double[3];
        for (int i = 0; i < tiempos.Length; i++)
        {
            var reloj = Stopwatch.StartNew();
            accion();
            reloj.Stop();
            tiempos[i] = reloj.Elapsed.TotalMilliseconds;
        }
        return (relojCalentamiento.Elapsed.TotalMilliseconds, tiempos);
    }

    private static double Promedio(double[] tiemposMs) => tiemposMs.Average();

    private static void Medir()
    {
        Console.WriteLine();
        Console.WriteLine("EjecutarCarga: calentamiento y tres corridas medidas (ms), más promedio y factor vs. fila anterior:");
        Console.WriteLine($"  {"n",4} {"ganancia",9} {"Ing. warmup",12} {"Ing. #1",10} {"Ing. #2",10} {"Ing. #3",10} {"Ing. prom.",11} {"x ant.",8} {"Mej. warmup",12} {"Mej. #1",10} {"Mej. #2",10} {"Mej. #3",10} {"Mej. prom.",11} {"x ant.",8}");
        double anteriorIngenua = 0, anteriorMejorada = 0;
        foreach (int n in new[] { 18, 20, 22, 24 })
        {
            int gananciaIngenua = 0, gananciaMejorada = 0;
            var ingenuo = new PlanificadorIngenuo();
            var mejorado = new PlanificadorMejorado();
            var medicionIngenua = MedirCorridas(() => gananciaIngenua = EjecutarCarga(ingenuo, n));
            var medicionMejorada = MedirCorridas(() => gananciaMejorada = EjecutarCarga(mejorado, n));
            double tiempoIngenuo = Promedio(medicionIngenua.tiemposMs);
            double tiempoMejorado = Promedio(medicionMejorada.tiemposMs);
            string factorIngenuo = anteriorIngenua == 0 ? "—" : "x" + (tiempoIngenuo / anteriorIngenua).ToString("0.00");
            string factorMejorado = anteriorMejorada == 0 ? "—" : "x" + (tiempoMejorado / anteriorMejorada).ToString("0.00");
            Console.WriteLine($"  {n,4} {gananciaMejorada,9} {medicionIngenua.calentamientoMs,12:0.000} {medicionIngenua.tiemposMs[0],10:0.000} {medicionIngenua.tiemposMs[1],10:0.000} {medicionIngenua.tiemposMs[2],10:0.000} {tiempoIngenuo,11:0.000} {factorIngenuo,8} {medicionMejorada.calentamientoMs,12:0.000} {medicionMejorada.tiemposMs[0],10:0.000} {medicionMejorada.tiemposMs[1],10:0.000} {medicionMejorada.tiemposMs[2],10:0.000} {tiempoMejorado,11:0.000} {factorMejorado,8}{(gananciaIngenua == gananciaMejorada ? "" : "  ← NO COINCIDEN")}");
            anteriorIngenua = tiempoIngenuo;
            anteriorMejorada = tiempoMejorado;
        }

        var grande = CrearCarga(1_000);
        var planMil = new PlanificadorMejorado();
        int ganancia10Mil = 0;
        var medicion10Mil = MedirCorridas(() => ganancia10Mil = planMil.Elegir(grande, 10_000).ganancia);
        Console.WriteLine($"  1.000 paquetes, capacidad 10.000 kg: warmup {medicion10Mil.calentamientoMs:0.000}; corridas {string.Join(", ", medicion10Mil.tiemposMs.Select(t => t.ToString("0.000")))}; promedio {Promedio(medicion10Mil.tiemposMs):0.000} ms; ganancia {ganancia10Mil}");

        int gananciaMilMillones = 0;
        var medicionMilMillones = MedirCorridas(() => gananciaMilMillones = planMil.Elegir(grande, 1_000_000_000).ganancia);
        Console.WriteLine($"  1.000 paquetes, capacidad 1.000.000.000 kg: warmup {medicionMilMillones.calentamientoMs:0.000}; corridas {string.Join(", ", medicionMilMillones.tiemposMs.Select(t => t.ToString("0.000")))}; promedio {Promedio(medicionMilMillones.tiemposMs):0.000} ms; ganancia {gananciaMilMillones}");
    }

    public static void Main()
    {
        Probar("PlanificadorIngenuo ", () => new PlanificadorIngenuo());
        Probar("PlanificadorMejorado", () => new PlanificadorMejorado());
        Medir();
    }
}