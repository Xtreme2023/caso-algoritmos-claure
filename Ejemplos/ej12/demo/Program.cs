Console.OutputEncoding = System.Text.Encoding.UTF8;

var caso = args.Length > 0 ? args[0].ToLowerInvariant() : "todo";

switch (caso)
{
    case "fibonacci": C12.Dinamica.Demo.Fibonacci(); break;
    case "mochila": C12.Dinamica.Demo.MochilaPequena(); C12.Dinamica.Demo.VorazContraDinamica(); break;
    case "costo": C12.Dinamica.Demo.Costo(); break;
    case "viajante": C12.Dinamica.Demo.Viajante(); break;
    case "todo": C12.Dinamica.Demo.Correr(); break;
    default:
        Console.WriteLine("Uso: dotnet run -c Release -- [fibonacci | mochila | costo | viajante | todo]");
        break;
}
