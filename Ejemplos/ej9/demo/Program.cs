Console.OutputEncoding = System.Text.Encoding.UTF8;

var caso = args.Length > 0 ? args[0].ToLowerInvariant() : "todo";

switch (caso)
{
    case "arbol": C9.Arboles.Demo.Arbol(); break;
    case "altura": C9.Arboles.Demo.Altura(); break;
    case "rangos": C9.Arboles.Demo.Rangos(); break;
    case "degenerado": C9.Arboles.Demo.Degenerado(); break;
    case "todo": C9.Arboles.Demo.Correr(); break;
    default:
        Console.WriteLine("Uso: dotnet run -c Release -- [arbol | altura | rangos | degenerado | todo]");
        break;
}
