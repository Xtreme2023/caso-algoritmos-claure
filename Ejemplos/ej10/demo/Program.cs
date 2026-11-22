Console.OutputEncoding = System.Text.Encoding.UTF8;

var caso = args.Length > 0 ? args[0].ToLowerInvariant() : "todo";

switch (caso)
{
    case "heap": C10.Heap.Demo.Heap(); break;
    case "prioridad": C10.Heap.Demo.Prioridad(); break;
    case "ordenar": C10.Heap.Demo.Ordenar(); break;
    case "torneo": C10.Heap.Demo.Torneo(); break;
    case "topk": C10.Heap.Demo.TopK(); break;
    case "todo": C10.Heap.Demo.Correr(); break;
    default:
        Console.WriteLine("Uso: dotnet run -c Release -- [heap | prioridad | ordenar | torneo | topk | todo]");
        break;
}
