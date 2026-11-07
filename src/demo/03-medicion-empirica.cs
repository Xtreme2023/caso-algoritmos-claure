// CLASE 3 · MEDIR BIEN — el protocolo del experimento: calentar, repetir, promediar
// Verificamos EMPÍRICAMENTE que la búsqueda lineal es O(n): datos x4 → tiempo ~x4.

using System.Diagnostics;

namespace C3.Medicion;

public static class Demo
{
    public static void Correr()
    {
        Console.WriteLine("PROTOCOLO: 1 corrida de calentamiento + 5 corridas medidas + promedio.");
        Console.WriteLine();
        Console.WriteLine($"{"n",12} {"promedio",12} {"ratio vs anterior",20}");

        double anterior = 0;
        foreach (var n in new[] { 250_000, 1_000_000, 4_000_000, 16_000_000 })
        {
            var datos = new int[n];
            var aleatorio = new Random(11);
            for (int i = 0; i < n; i++) datos[i] = aleatorio.Next();

            BuscarMaximo(datos);

            double suma = 0;
            for (int corrida = 0; corrida < 5; corrida++)
            {
                var reloj = Stopwatch.StartNew();
                BuscarMaximo(datos);
                reloj.Stop();
                suma += reloj.Elapsed.TotalMilliseconds;
            }
            var promedio = suma / 5;

            var ratio = anterior > 0 ? $"x{promedio / anterior:0.0}" : "—";
            Console.WriteLine($"{n,12:N0} {promedio,9:0.00} ms {ratio,20}");
            anterior = promedio;
        }

        Console.WriteLine();
        Console.WriteLine("Datos x4 → el tiempo debería subir ~x4 en cada salto si la búsqueda del máximo es O(n).");
        Console.WriteLine("Los saltos sueltos se desvían por el RUIDO: se mira la tendencia y se repite si hace falta.");
    }

    private static int BuscarMaximo(int[] datos)
    {
        int maximo = datos[0];
        foreach (var d in datos)
            if (d > maximo) maximo = d;
        return maximo;
    }
}
