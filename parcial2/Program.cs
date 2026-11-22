//Cristofer Johany Claure Cabrera
// PARCIAL 2 · VARIANTE B — "Pedidos de una pastelería"
// Caso: pedidos que llegan con un PLAZO en minutos (el menor plazo es el más urgente) y se preparan del plazo más
// corto al más largo; si dos tienen el mismo plazo, se prepara primero el que llegó antes.
// ReceptorIngenuo es CORRECTO pero LENTO (usa una lista simple). Tu trabajo: elegir la estructura que conviene,
// APLICARLA en ReceptorMejorado, medir la diferencia y justificar tu elección.
// Puedes usar este archivo tal cual (si tienes .NET) o portar todo a TU lenguaje.

using System.Diagnostics;

namespace Parcial2.VarianteB;

public interface IReceptorPedidos
{
    void Registrar(string pedido, int plazoMinutos);   // llega un pedido (nombres únicos)
    string? PrepararSiguiente();                       // saca y devuelve el de menor plazo; null si no hay ninguno
    bool EstaPendiente(string pedido);                 // ¿este pedido sigue pendiente? (prepararlo lo saca)
    int Pendientes { get; }                            // cuántos hay pendientes
}

public sealed record Pedido(string Nombre, int Plazo, int Llegada);

// ---------- SOLUCIÓN INGENUA (lista simple): correcta pero lenta ----------
public sealed class ReceptorIngenuo : IReceptorPedidos
{
    private readonly List<Pedido> _lista = new();
    private int _llegadas;

    public void Registrar(string pedido, int plazoMinutos) => _lista.Add(new Pedido(pedido, plazoMinutos, _llegadas++));

    public string? PrepararSiguiente()
    {
        if (_lista.Count == 0) return null;
        int mejor = 0;
        for (int i = 1; i < _lista.Count; i++)
        {
            var p = _lista[i]; var m = _lista[mejor];
            if (p.Plazo < m.Plazo || (p.Plazo == m.Plazo && p.Llegada < m.Llegada)) mejor = i;
        }
        string nombre = _lista[mejor].Nombre;
        _lista.RemoveAt(mejor);
        return nombre;
    }

    public bool EstaPendiente(string pedido)
    {
        foreach (var p in _lista) if (p.Nombre == pedido) return true;
        return false;
    }

    public int Pendientes => _lista.Count;
}

// ---------- TU SOLUCIÓN: elige la(s) estructura(s) y escríbela aquí ----------
public sealed class ReceptorMejorado : IReceptorPedidos
{
    private readonly PriorityQueue<string, long> _cola;
    private readonly HashSet<string> _pendientes;
    private uint _llegadas;

    public ReceptorMejorado(int capacidadInicial = 0)
    {
        _cola = new PriorityQueue<string, long>(capacidadInicial);
        _pendientes = new HashSet<string>(capacidadInicial, StringComparer.Ordinal);
    }

    public void Registrar(string pedido, int plazoMinutos)
    {
        if (!_pendientes.Add(pedido))
            throw new ArgumentException($"El pedido '{pedido}' ya está pendiente.", nameof(pedido));

        long prioridad = ((long)plazoMinutos << 32) | _llegadas++;
        _cola.Enqueue(pedido, prioridad);
    }

    public string? PrepararSiguiente()
    {
        if (!_cola.TryDequeue(out string? pedido, out _)) return null;
        _pendientes.Remove(pedido);
        return pedido;
    }

    public bool EstaPendiente(string pedido) => _pendientes.Contains(pedido);

    public int Pendientes => _pendientes.Count;
}

public static class Programa
{
    // El guion de pruebas: cada paso es (operación, argumento, plazo, resultado esperado).
    private static readonly (string op, string arg, int plazo, string esperado)[] Guion =
    {
        ("registrar", "Torta", 120, ""), ("registrar", "Galletas", 30, ""), ("registrar", "Pan", 120, ""), ("registrar", "Brazo", 60, ""),
        ("pendientes", "", 0, "4"),
        ("esta", "Galletas", 0, "True"), ("esta", "Flan", 0, "False"),
        ("preparar", "", 0, "Galletas"), ("esta", "Galletas", 0, "False"),
        ("preparar", "", 0, "Brazo"),
        ("registrar", "Alfajor", 30, ""),
        ("preparar", "", 0, "Alfajor"), ("preparar", "", 0, "Torta"), ("preparar", "", 0, "Pan"),
        ("preparar", "", 0, "(ninguno)"), ("pendientes", "", 0, "0"),
    };

    private static string Ejecutar(IReceptorPedidos g, string op, string arg, int plazo)
    {
        switch (op)
        {
            case "registrar": g.Registrar(arg, plazo); return "";
            case "preparar": return g.PrepararSiguiente() ?? "(ninguno)";
            case "esta": return g.EstaPendiente(arg).ToString();
            default: return g.Pendientes.ToString();
        }
    }

    private static void Probar(string titulo, IReceptorPedidos g)
    {
        int bien = 0, total = 0;
        try
        {
            foreach (var (op, arg, plazo, esperado) in Guion)
            {
                string obtenido = Ejecutar(g, op, arg, plazo);
                if (op == "registrar") continue;
                total++;
                if (obtenido == esperado) bien++;
                else Console.WriteLine($"   MAL en «{op} {arg}»: esperado {esperado}, obtuvo {obtenido}");
            }
            Console.WriteLine($"{titulo}: {bien} de {total} pasos correctos");
        }
        catch (NotImplementedException) { Console.WriteLine($"{titulo}: (sin escribir todavía)"); }
    }

    // La CARGA para medir: n pedidos llegan y después se preparan los n.
    public static long EjecutarCarga(IReceptorPedidos g, int n)
    {
        for (int i = 0; i < n; i++) g.Registrar("Q" + i, ((int)((i * 6007L) % 6) + 1) * 15);
        long control = 0;
        for (int i = 0; i < n; i++) control += g.PrepararSiguiente()!.Length;
        return control;
    }

    public static void Main()
    {
        Probar("ReceptorIngenuo ", new ReceptorIngenuo());
        Probar("ReceptorMejorado", new ReceptorMejorado());

        const int n = 5_000;

        // Una corrida de calentamiento por receptor, ambas con el mayor tamaño.
        EjecutarCarga(new ReceptorIngenuo(), 4 * n);
        EjecutarCarga(new ReceptorMejorado(), 4 * n);

        Console.WriteLine();
        Console.WriteLine("n; receptor; corrida1_ms; corrida2_ms; corrida3_ms; promedio_ms");

        foreach (int tamaño in new[] { n, 2 * n, 4 * n })
        {
            foreach (bool medirIngenuo in new[] { true, false })
            {
                string nombre = medirIngenuo ? "Ingenuo" : "Mejorado";
                double[] tiempos = new double[3];
                for (int corrida = 0; corrida < 3; corrida++)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();

                    IReceptorPedidos receptor = medirIngenuo
                        ? new ReceptorIngenuo()
                        : new ReceptorMejorado();

                    var reloj = Stopwatch.StartNew();
                    EjecutarCarga(receptor, tamaño);
                    reloj.Stop();
                    tiempos[corrida] = reloj.Elapsed.TotalMilliseconds;
                }

                Console.WriteLine(
                    $"{tamaño}; {nombre}; " +
                    $"{tiempos[0]:0.000}; {tiempos[1]:0.000}; {tiempos[2]:0.000}; " +
                    $"{tiempos.Average():0.000}");
            }
        }
    }
}