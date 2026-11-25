Console.OutputEncoding = System.Text.Encoding.UTF8;

var caso = args.Length > 0 ? args[0].ToLowerInvariant() : "todo";

switch (caso)
{
    case "vuelto": C11.Voraces.Demo.Vuelto(); break;
    case "engano": C11.Voraces.Demo.Engano(); break;
    case "actividades": C11.Voraces.Demo.Actividades(); break;
    case "costo": C11.Voraces.Demo.Costo(); break;
    case "todo": C11.Voraces.Demo.Correr(); break;
    default:
        Console.WriteLine("Uso: dotnet run -c Release -- [vuelto | engano | actividades | costo | todo]");
        break;
}
