// CLASE 2 · O GRANDE — cómo crecen las operaciones cuando crecen los datos
// Dos algoritmos reales medidos con tamaños crecientes: lineal O(n) vs cuadrático O(n²).

using System.Diagnostics;

namespace C2.Crecimiento;

public static class Demo
{
    public static void Correr()
    {
        Console.WriteLine("TEORÍA — operaciones según el orden de crecimiento:");
        Console.WriteLine($"{"n",10} {"log n",8} {"n",10} {"n log n",12} {"n²",16}");
        foreach (var n in new[] { 10, 100, 1_000, 10_000 })
        {
            var log = Math.Log2(n);
            Console.WriteLine($"{n,10:N0} {log,8:0.0} {n,10:N0} {n * log,12:N0} {(long)n * n,16:N0}");
        }

        Console.WriteLine();
        Console.WriteLine("PRÁCTICA — los mismos dos algoritmos, con 10 veces más datos cada vez:");
        Console.WriteLine($"{"n",10} {"suma O(n)",14} {"duplicados O(n²)",20}");

        foreach (var n in new[] { 2_000, 20_000, 200_000 })
        {
            var datos = new int[n];
            var aleatorio = new Random(7);
            for (int i = 0; i < n; i++) datos[i] = aleatorio.Next(1_000_000_000);

            var reloj = Stopwatch.StartNew();
            long suma = 0;
            foreach (var d in datos) suma += d;
            reloj.Stop();
            var msLineal = reloj.Elapsed.TotalMilliseconds;

            reloj.Restart();
            int duplicados = 0;
            for (int i = 0; i < n; i++)
                for (int j = i + 1; j < n; j++)
                    if (datos[i] == datos[j]) duplicados++;
            reloj.Stop();
            var msCuadratico = reloj.Elapsed.TotalMilliseconds;

            Console.WriteLine($"{n,10:N0} {msLineal,11:0.00} ms {msCuadratico,17:N0} ms");
        }

        Console.WriteLine();
        Console.WriteLine("Datos x10  →  el O(n²) tarda ~x100 (columna derecha). El O(n) tarda tan poco que apenas se alcanza a medir.");
        Console.WriteLine("La forma de crecer ES la firma del algoritmo. Eso nombra la O grande.");
    }
}
