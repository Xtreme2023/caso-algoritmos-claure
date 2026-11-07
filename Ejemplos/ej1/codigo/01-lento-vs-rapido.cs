// CLASE 1 · EL DUELO — mismo problema, dos soluciones, rendimientos opuestos
// Problema: verificar si 10.000 carnets pertenecen al padrón de 1.000.000 de registros.

using System.Diagnostics;

namespace C1.Duelo;

public static class Demo
{
    public static void Correr()
    {
        const int tamanoPadron = 1_000_000;
        const int consultas = 10_000;

        var padronLista = new List<int>(tamanoPadron);
        for (int i = 0; i < tamanoPadron; i++) padronLista.Add(1_000_000 + i * 3);

        var padronConjunto = new HashSet<int>(padronLista);

        var aleatorio = new Random(42);
        var carnetsABuscar = new List<int>(consultas);
        for (int i = 0; i < consultas; i++) carnetsABuscar.Add(1_000_000 + aleatorio.Next(tamanoPadron * 3));

        Console.WriteLine($"Padrón: {tamanoPadron:N0} carnets · Consultas: {consultas:N0}");
        Console.WriteLine();

        var reloj = Stopwatch.StartNew();
        int encontradosLento = 0;
        foreach (var carnet in carnetsABuscar)
            if (padronLista.Contains(carnet)) encontradosLento++;
        reloj.Stop();
        Console.WriteLine($"[SOLUCIÓN A] Recorrer la lista uno por uno:  {reloj.ElapsedMilliseconds,6} ms  ({encontradosLento} encontrados)");

        reloj.Restart();
        int encontradosRapido = 0;
        foreach (var carnet in carnetsABuscar)
            if (padronConjunto.Contains(carnet)) encontradosRapido++;
        reloj.Stop();
        Console.WriteLine($"[SOLUCIÓN B] Preguntar a la estructura correcta: {reloj.ElapsedMilliseconds,3} ms  ({encontradosRapido} encontrados)");

        Console.WriteLine();
        Console.WriteLine("Mismo problema. Misma respuesta. La diferencia es EL ALGORITMO.");
        Console.WriteLine("De eso se trata esta materia: decidir con evidencia, no por intuición.");
    }
}
