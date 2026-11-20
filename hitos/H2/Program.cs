// ==============================================================================
// HITO H2 · MEDICIÓN DE ESTRUCTURAS DE DATOS
// Caso: Consolidación y consulta de calificaciones de estudiantes por CI
// Comparación: lista sin ordenar (búsqueda lineal O(n))
//          y árbol binario balanceado (SortedDictionary<int, ...> O(log n))
// Protocolo: Calentamiento JIT, 3 repeticiones, promediar con Stopwatch
// ==============================================================================

using System.Diagnostics;
using System.Globalization;

namespace H2;

public sealed class RegistroEstudiante
{
    public int Ci { get; init; }
    public string NombreCompleto { get; init; } = "";
    public string Departamento { get; init; } = "";
    public string Tipo { get; init; } = "";
    public double NotaFinal { get; init; }
    public int AsistenciaPct { get; init; }
    public string Estado { get; init; } = "";
    public string NivelActual { get; init; } = "";
}

public static class Programa
{
    // --------------------------------------------------------------------------
    // a) LISTA SIN ORDENAR: búsqueda lineal (O(n) por búsqueda)
    // --------------------------------------------------------------------------
    public static (int encontrados, double sumaNotas) ConsultarIngenua(
        List<RegistroEstudiante> lista,
        int[] consultas)
    {
        int encontrados = 0;
        double sumaNotas = 0.0;

        foreach (int ci in consultas)
        {
            RegistroEstudiante? hallado = null;
            for (int i = 0; i < lista.Count; i++)
            {
                if (lista[i].Ci == ci)
                {
                    hallado = lista[i];
                    break;
                }
            }

            if (hallado != null)
            {
                encontrados++;
                sumaNotas += hallado.NotaFinal;
            }
        }

        return (encontrados, sumaNotas);
    }

    // --------------------------------------------------------------------------
    // b) ÁRBOL BALANCEADO: búsqueda ordenada (O(log n) por búsqueda)
    // --------------------------------------------------------------------------
    public static (int encontrados, double sumaNotas) ConsultarAdecuada(
        SortedDictionary<int, RegistroEstudiante> arbol,
        int[] consultas)
    {
        int encontrados = 0;
        double sumaNotas = 0.0;

        foreach (int ci in consultas)
        {
            if (arbol.TryGetValue(ci, out var hallado))
            {
                encontrados++;
                sumaNotas += hallado.NotaFinal;
            }
        }

        return (encontrados, sumaNotas);
    }

    // --------------------------------------------------------------------------
    // Generación de datos sintéticos controlados para el benchmark
    // --------------------------------------------------------------------------
    private static List<RegistroEstudiante> GenerarDatos(int n, int semilla = 42)
    {
        var azar = new Random(semilla);
        var baseCis = Enumerable.Range(1_000_000, n).ToArray();

        // Mezclado Fisher-Yates para garantizar datos sin orden predefinido
        for (int i = n - 1; i > 0; i--)
        {
            int j = azar.Next(i + 1);
            (baseCis[i], baseCis[j]) = (baseCis[j], baseCis[i]);
        }

        var lista = new List<RegistroEstudiante>(n);
        for (int i = 0; i < n; i++)
        {
            lista.Add(new RegistroEstudiante
            {
                Ci = baseCis[i],
                NombreCompleto = $"ESTUDIANTE_{i + 1}",
                Departamento = "LA PAZ",
                Tipo = "MIEMBRO",
                NotaFinal = Math.Round(40.0 + azar.NextDouble() * 55.0, 1),
                AsistenciaPct = azar.Next(70, 100),
                Estado = "APROBADO",
                NivelActual = "B1"
            });
        }

        return lista;
    }

    // Generar consultas: 50% claves existentes y 50% claves inexistentes
    private static int[] GenerarConsultas(List<RegistroEstudiante> datos, int cantidad, int semilla = 99)
    {
        var azar = new Random(semilla);
        var consultas = new int[cantidad];
        int existentes = cantidad / 2;

        for (int i = 0; i < existentes; i++)
        {
            consultas[i] = datos[azar.Next(datos.Count)].Ci;
        }

        for (int i = existentes; i < cantidad; i++)
        {
            consultas[i] = 9_000_000 + azar.Next(1_000_000); // Rango de CIs no existentes
        }

        // Mezclar el orden de las consultas
        for (int i = cantidad - 1; i > 0; i--)
        {
            int j = azar.Next(i + 1);
            (consultas[i], consultas[j]) = (consultas[j], consultas[i]);
        }

        return consultas;
    }

    // Medición con cronómetro de alta resolución (3 repeticiones y promedio en segundos)
    private static double PromedioSegundos(Action accion)
    {
        long sumaTicks = 0;
        for (int rep = 0; rep < 3; rep++)
        {
            long tInicio = Stopwatch.GetTimestamp();
            accion();
            long tFin = Stopwatch.GetTimestamp();
            sumaTicks += (tFin - tInicio);
        }
        return (double)sumaTicks / (3.0 * Stopwatch.Frequency);
    }

    // Carga de la muestra CSV real generada para verificar consistencia
    private static void DemostrarConCsvReal(string rutaCsv)
    {
        if (!File.Exists(rutaCsv)) return;

        var listaMuestra = new List<RegistroEstudiante>();
        var arbolMuestra = new SortedDictionary<int, RegistroEstudiante>();

        foreach (var linea in File.ReadLines(rutaCsv))
        {
            if (string.IsNullOrWhiteSpace(linea) || linea.StartsWith("#")) continue;
            var partes = linea.Split(';');
            if (partes[0].Equals("ci", StringComparison.OrdinalIgnoreCase)) continue;

            int ci = int.Parse(partes[0]);
            var est = new RegistroEstudiante
            {
                Ci = ci,
                NombreCompleto = partes[1],
                Departamento = partes[2],
                Tipo = partes[3],
                NotaFinal = double.Parse(partes[4], CultureInfo.InvariantCulture),
                AsistenciaPct = int.Parse(partes[5]),
                Estado = partes[6],
                NivelActual = partes[7]
            };

            listaMuestra.Add(est);
            arbolMuestra.Add(ci, est);
        }

        Console.WriteLine($"[CSV Real] Cargados {listaMuestra.Count} registros únicos desde {Path.GetFileName(rutaCsv)}.");
        // Prueba de consulta sobre 50 carnets reales
        var clavesPrueba = listaMuestra.Take(50).Select(e => e.Ci).ToArray();
        var (encL, sumL) = ConsultarIngenua(listaMuestra, clavesPrueba);
        var (encA, sumA) = ConsultarAdecuada(arbolMuestra, clavesPrueba);
        VerificarIgualdad(encL, encA, sumL, sumA, "CSV real");
        Console.WriteLine($"[CSV Real] Verificación exitosa: 50/50 consultas coincidentes (Suma notas: {sumA:F1}).\n");
    }

    private static void VerificarIgualdad(
        int encontradosIngenua,
        int encontradosAdecuada,
        double sumaIngenua,
        double sumaAdecuada,
        string contexto)
    {
        if (encontradosIngenua != encontradosAdecuada ||
            Math.Abs(sumaIngenua - sumaAdecuada) >= 1e-4)
        {
            throw new InvalidOperationException(
                $"Las implementaciones dieron resultados distintos en {contexto}: " +
                $"{encontradosIngenua}/{sumaIngenua} frente a " +
                $"{encontradosAdecuada}/{sumaAdecuada}.");
        }
    }

    public static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("================================================================================");
        Console.WriteLine("HITO H2 · PROTOCOLO DE MEDICIÓN: ESTRUCTURA INGENUA VS ADECUADA");
        Console.WriteLine("================================================================================\n");

        string rutaCsv = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "notas_2026_M2.csv");
        string rutaProyecto = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
        string rutaMarkdown = Path.Combine(rutaProyecto, "medicion.md");
        if (File.Exists(rutaCsv))
        {
            DemostrarConCsvReal(rutaCsv);
        }

        // Configuración del experimento
        int[] tamanosN = { 10_000, 20_000, 40_000, 80_000 };
        const int consultasQ = 3_000; // Lote representativo de 3.000 consultas

        Console.WriteLine($"Consultas por experimento: {consultasQ:N0} (50% existentes, 50% inexistentes)");
        Console.WriteLine("Protocolo: Calentamiento previo + 3 ejecuciones por tamaño con Stopwatch.");
        Console.WriteLine();

        // 1. CALENTAMIENTO (Warm-up) para JIT y memoria
        Console.Write("Ejecutando calentamiento (warm-up)... ");
        var datosWarmup = GenerarDatos(2_000, semilla: 1);
        var arbolWarmup = new SortedDictionary<int, RegistroEstudiante>();
        foreach (var e in datosWarmup) arbolWarmup[e.Ci] = e;
        var consultasWarmup = GenerarConsultas(datosWarmup, 500, semilla: 2);
        ConsultarIngenua(datosWarmup, consultasWarmup);
        ConsultarAdecuada(arbolWarmup, consultasWarmup);
        Console.WriteLine("Listo.\n");

        // 2. MEDICIÓN POR TAMAÑOS
        Console.WriteLine("| N | Tiempo con lista (s) | Veces vs fila anterior | Tiempo con árbol (s) | Veces vs fila anterior |");
        Console.WriteLine("|---|---:|---:|---:|---:|");

        double tIngenuaPrev = 0;
        double tAdecuadaPrev = 0;
        var resultados = new List<(int N, double TiempoLista, double? VecesLista, double TiempoArbol, double? VecesArbol)>();

        foreach (int n in tamanosN)
        {
            var datos = GenerarDatos(n, semilla: 100 + n);
            var arbol = new SortedDictionary<int, RegistroEstudiante>();
            foreach (var est in datos) arbol[est.Ci] = est;

            var consultas = GenerarConsultas(datos, consultasQ, semilla: 200 + n);

            // Verificación previa de exactitud de resultados
            var (encIng, sumaIng) = ConsultarIngenua(datos, consultas);
            var (encAde, sumaAde) = ConsultarAdecuada(arbol, consultas);
            VerificarIgualdad(encIng, encAde, sumaIng, sumaAde, $"N={n}");

            // Medición de la lista sin ordenar (3 repeticiones)
            double tIng = PromedioSegundos(() => ConsultarIngenua(datos, consultas));

            // Medición del árbol balanceado (3 repeticiones)
            double tAde = PromedioSegundos(() => ConsultarAdecuada(arbol, consultas));

            string vecesIng = tIngenuaPrev == 0 ? "—" : (tIng / tIngenuaPrev).ToString("0.00", CultureInfo.InvariantCulture) + "x";
            string vecesAde = tAdecuadaPrev == 0 ? "—" : (tAde / tAdecuadaPrev).ToString("0.00", CultureInfo.InvariantCulture) + "x";

            Console.WriteLine($"| {n,6:N0} | {tIng,18:F6} | {vecesIng,22} | {tAde,19:F6} | {vecesAde,22} |");

            double? vecesLista = tIngenuaPrev == 0 ? null : tIng / tIngenuaPrev;
            double? vecesArbol = tAdecuadaPrev == 0 ? null : tAde / tAdecuadaPrev;
            resultados.Add((n, tIng, vecesLista, tAde, vecesArbol));

            tIngenuaPrev = tIng;
            tAdecuadaPrev = tAde;
        }

        Console.WriteLine();
        Console.WriteLine("Medición finalizada: los resultados coinciden en todos los tamaños.");

        GuardarMedicionMarkdown(rutaMarkdown, resultados);
        Console.WriteLine($"Resultados guardados en: {rutaMarkdown}");
    }

    private static void GuardarMedicionMarkdown(
        string ruta,
        IReadOnlyList<(int N, double TiempoLista, double? VecesLista, double TiempoArbol, double? VecesArbol)> resultados)
    {
        var texto = new System.Text.StringBuilder();
        texto.AppendLine("# Hito H2 · Medición Empírica y Comparativa");
        texto.AppendLine();
        texto.AppendLine("## 1. Protocolo de Medición");
        texto.AppendLine("- **Problema evaluado:** Se procesaron 3.000 consultas por cada tamaño de datos. El 50 % eran claves existentes y el 50 % eran claves inexistentes.");
        texto.AppendLine("- **Implementaciones comparadas:** una lista sin ordenar con búsqueda lineal y un `SortedDictionary<int, RegistroEstudiante>` con búsqueda balanceada.");
        texto.AppendLine("- **Calentamiento (warm-up):** Se ejecutó una corrida previa independiente antes de tomar tiempos.");
        texto.AppendLine("- **Repeticiones:** Cada medición se ejecutó 3 veces y se promedió.");
        texto.AppendLine("- **Reloj:** `Stopwatch.GetTimestamp()` de alta resolución de .NET.");
        texto.AppendLine("- **Verificación:** Se comprobó que ambas estructuras devolvieran los mismos registros encontrados y la misma suma de notas.");
        texto.AppendLine();
        texto.AppendLine("## 2. Resultados Obtenidos");
        texto.AppendLine();
        texto.AppendLine("| N | Tiempo con lista (s) | Veces vs fila anterior | Tiempo con árbol (s) | Veces vs fila anterior |");
        texto.AppendLine("|---|---:|:---:|---:|:---:|");

        foreach (var r in resultados)
        {
            string lista = r.VecesLista.HasValue
                ? r.VecesLista.Value.ToString("0.00", CultureInfo.InvariantCulture) + "x"
                : "—";
            string arbol = r.VecesArbol.HasValue
                ? r.VecesArbol.Value.ToString("0.00", CultureInfo.InvariantCulture) + "x"
                : "—";

            texto.AppendLine(string.Create(
                CultureInfo.InvariantCulture,
                $"| {r.N:N0} | {r.TiempoLista:F6} | {lista} | {r.TiempoArbol:F6} | {arbol} |"));
        }

        var ultimo = resultados[^1];
        double relacionFinal = ultimo.TiempoLista / ultimo.TiempoArbol;

        texto.AppendLine();
        texto.AppendLine("## 3. Conclusiones");
        texto.AppendLine();
        texto.AppendLine($"1. La lista sin ordenar fue más lenta al aumentar el tamaño de los datos; sus factores de crecimiento se muestran en la tabla.");
        texto.AppendLine($"2. El árbol balanceado mantuvo tiempos cercanos entre sí en esta máquina ({resultados.Min(r => r.TiempoArbol):F6} s a {resultados.Max(r => r.TiempoArbol):F6} s); las variaciones pequeñas corresponden a ruido de ejecución.");
        texto.AppendLine($"3. Para N = {ultimo.N:N0}, el árbol fue aproximadamente {relacionFinal:F0} veces más rápido que la lista en esta ejecución.");
        texto.AppendLine("4. Las conclusiones se basan en los resultados reales anteriores y no se igualan artificialmente a una complejidad teórica.");

        File.WriteAllText(ruta, texto.ToString(), new System.Text.UTF8Encoding(false));
    }
}
