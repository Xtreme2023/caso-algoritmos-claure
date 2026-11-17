// PARCIAL 1 · VARIANTE A — "Registro de carnets"
// Problema: dada una lista de carnets, ¿hay alguno repetido?
// La función HayRepetidosPorPares es CORRECTA pero LENTA. Tu trabajo: analizarla, CURARLA (escribir una
// versión que cueste menos), medir las dos y elegir. Puedes usar este archivo tal cual (si tienes .NET)
// o portar todo a TU lenguaje.

using System.Diagnostics;

namespace Parcial1.VarianteA;

public static class Registro
{
    // Solución ACTUAL (lenta): comparar cada carnet con todos los que le siguen.
    public static bool HayRepetidosPorPares(int[] carnets)
    {
        for (int i = 0; i < carnets.Length; i++)
            for (int j = i + 1; j < carnets.Length; j++)
                if (carnets[i] == carnets[j]) return true;
        return false;
    }

    // TU CURA: escribe aquí una versión correcta que cueste MENOS que O(n²).
    
    // No modifiques la lista que recibes (trabaja sobre una copia si la necesitas ordenada).
    public static bool HayRepetidosCurado(int[] carnets)
    {
        if (carnets.Length <= 1) return false;
        
        // Se trabaja sobre una copia para no modificar la lista original
        int[] copia = (int[])carnets.Clone();
        Array.Sort(copia); // O(n log n) orden
        
        // Recorrido lineal: O(n)
        for (int i = 0; i < copia.Length - 1; i++)
        {
            if (copia[i] == copia[i + 1]) return true;
        }
        return false;
    }
    // El PEOR CASO para las dos: una lista SIN repetidos (hay que mirarlo todo).
    public static int[] GenerarSinRepetidos(int n, int semilla = 42)
    {
        var datos = new int[n];
        for (int i = 0; i < n; i++) datos[i] = i;
        var azar = new Random(semilla);
        for (int i = n - 1; i > 0; i--)
        {
            int k = azar.Next(i + 1);
            (datos[i], datos[k]) = (datos[k], datos[i]);
        }
        return datos;
    }
}

public static class Programa
{
    // Los cinco casos que tu cura debe pasar (la esperada va a la derecha).
    private static readonly (int[] datos, bool esperado, string que)[] Casos =
    {
        (new[] { 7, 3, 9, 3, 5 },   true,  "repetido en el medio"),
        (new[] { 7, 3, 9, 4, 5 },   false, "sin repetidos"),
        (new int[] { },             false, "lista vacía"),
        (new[] { 8 },               false, "un solo elemento"),
        (new[] { 4, 1, 2, 6, 4 },   true,  "repetido en los extremos"),
    };

    public static void Main()
    {
        foreach (var (datos, esperado, que) in Casos)
        {

            bool lenta = Registro.HayRepetidosPorPares(datos);
            string curada;
            try { curada = Registro.HayRepetidosCurado(datos).ToString(); }
            catch (NotImplementedException) { curada = "(sin escribir)"; }
            Console.WriteLine($"{que,-26} esperado {esperado,-5} · por pares {lenta,-5} · tu cura {curada}");
        }

        // Una medición de ejemplo, SIN protocolo (el protocolo lo agregas tú en tu medición):
      Console.WriteLine("\n-PROTOCOLO DE MEDICION -");
        int[] tamanos = { 10000, 20000, 40000, 80000 }; 
        
        foreach (int n in tamanos)
        {
            var datos = Registro.GenerarSinRepetidos(n);

            // Calentamiento
            Registro.HayRepetidosPorPares(datos);
            Registro.HayRepetidosCurado(datos);

            // Medición Lenta (Por Pares)
            long totalLenta = 0;
            for (int i = 0; i < 3; i++)
            {
                var sw = Stopwatch.StartNew();
                Registro.HayRepetidosPorPares(datos);
                sw.Stop();
                totalLenta += sw.ElapsedMilliseconds;
            }
            long promLenta = totalLenta / 3;

            // Medición de la cura
            long totalCurada = 0;

            for (int i = 0; i < 3; i++)
            {
                var sw = Stopwatch.StartNew();
                Registro.HayRepetidosCurado(datos);
                sw.Stop();
                totalCurada += sw.ElapsedMilliseconds;
            }
            long promCurada = totalCurada / 3;

            Console.WriteLine($"N={n,-6} | Lenta: {promLenta,5} milisegundos | Curada: {promCurada,4} milisegundos");
        }
    }
}