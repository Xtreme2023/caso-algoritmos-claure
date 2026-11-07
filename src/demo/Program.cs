// Demos de Análisis de Algoritmos. Correr SIEMPRE en la terminal y en Release:
//   dotnet run -c Release -- ordenes     → Clase 2: órdenes de crecimiento
//   dotnet run -c Release -- medicion    → Clase 3: medición empírica
//   dotnet run -c Release -- reglas      → Clase 4: reglas de la O grande
// (F5 corre en Debug y sin argumentos: las mediciones no valen.)

using System.Text;

Console.OutputEncoding = Encoding.UTF8;

#if DEBUG
Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine("⚠ Estás en modo DEBUG: las mediciones NO valen. Usa: dotnet run -c Release -- <demo>");
Console.ResetColor();
#endif

switch (args.FirstOrDefault()?.ToLowerInvariant())
{
    case "ordenes":  C2.Crecimiento.Demo.Correr(); break;
    case "medicion": C3.Medicion.Demo.Correr();    break;
    case "reglas":   C4.ReglasBigO.Demo.Correr();  break;
    default:
        Console.WriteLine("Indica qué demo correr:");
        Console.WriteLine("  dotnet run -c Release -- ordenes     (Clase 2: órdenes de crecimiento)");
        Console.WriteLine("  dotnet run -c Release -- medicion    (Clase 3: medición empírica)");
        Console.WriteLine("  dotnet run -c Release -- reglas      (Clase 4: reglas de la O grande)");
        break;
}
