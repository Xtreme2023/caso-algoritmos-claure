Console.OutputEncoding = System.Text.Encoding.UTF8;

var caso = args.Length > 0 ? args[0].ToLowerInvariant() : "todo";

switch (caso)
{
    case "modelo": C13.Grafos.Demo.Modelo(); break;
    case "recorridos": C13.Grafos.Demo.Recorridos(); break;
    case "componentes": C13.Grafos.Demo.Componentes(); break;
    case "representacion": C13.Grafos.Demo.Representacion(); break;
    case "todo": C13.Grafos.Demo.Correr(); break;
    default:
        Console.WriteLine("Uso: dotnet run -c Release -- [modelo | recorridos | componentes | representacion | todo]");
        break;
}
