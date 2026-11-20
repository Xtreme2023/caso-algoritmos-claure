Console.OutputEncoding = System.Text.Encoding.UTF8;

var caso = args.Length > 0 ? args[0].ToLowerInvariant() : "todo";

switch (caso)
{
    case "funcion": C8.Hash.Demo.Funcion(); break;
    case "tabla": C8.Hash.Demo.Tabla(); break;
    case "medir": C8.Hash.Demo.Medir(); break;
    case "palabras": C8.Hash.Demo.Palabras(); break;
    case "todo": C8.Hash.Demo.Correr(); break;
    default:
        Console.WriteLine("Uso: dotnet run -c Release -- [funcion | tabla | medir | palabras | todo]");
        break;
}
