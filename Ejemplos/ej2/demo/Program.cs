Console.OutputEncoding = System.Text.Encoding.UTF8;

var caso = args.Length > 0 ? args[0].ToLowerInvariant() : "";

switch (caso)
{
    case "ordenes": C2.Crecimiento.Demo.Correr(); break;
    case "medicion": C3.Medicion.Demo.Correr(); break;
    default:
        Console.WriteLine("Uso: dotnet run -c Release -- [ordenes | medicion]");
        break;
}
