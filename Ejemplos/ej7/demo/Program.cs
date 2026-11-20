Console.OutputEncoding = System.Text.Encoding.UTF8;

var caso = args.Length > 0 ? args[0].ToLowerInvariant() : "todo";

switch (caso)
{
    case "pila": C7.Estructuras.Demo.Pila(); break;
    case "cola": C7.Estructuras.Demo.Cola(); break;
    case "listas": C7.Estructuras.Demo.Listas(); break;
    case "mapa": C7.Estructuras.Demo.Mapa(); break;
    case "todo": C7.Estructuras.Demo.Correr(); break;
    default:
        Console.WriteLine("Uso: dotnet run -c Release -- [pila | cola | listas | mapa | todo]");
        break;
}
