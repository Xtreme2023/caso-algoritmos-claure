// CLASE 13 · GRAFOS: MODELAR EL MUNDO CON NODOS Y ARISTAS, Y RECORRERLO (BFS Y DFS)
// Un grafo son NODOS (las paradas) y ARISTAS (los tramos que los unen). Sirve para rutas, redes y dependencias.
// Las partes A, B y C son exactas (trazas y conteos): salen iguales en toda máquina.
// La parte D mide tiempos con el protocolo (calentar, repetir 3 veces, promediar) y varía con la máquina.

using System.Diagnostics;

namespace C13.Grafos;

public static class Demo
{
    // El mapa de micros de una ciudad pequeña. Cada tramo se puede recorrer en los dos sentidos.
    private static readonly string[] Paradas = { "Plaza", "Terminal", "Mercado", "Hospital", "Colegio", "Parque", "Universidad", "Estadio", "Aeropuerto", "Hangar" };
    private static readonly (string, string)[] Tramos =
    {
        ("Plaza", "Terminal"), ("Plaza", "Mercado"), ("Terminal", "Hospital"), ("Mercado", "Colegio"),
        ("Mercado", "Parque"), ("Colegio", "Universidad"), ("Hospital", "Universidad"), ("Parque", "Estadio"),
        ("Aeropuerto", "Hangar"),
    };

    private static List<int>[] ConstruirLista()
    {
        var lista = new List<int>[Paradas.Length];
        for (int i = 0; i < lista.Length; i++) lista[i] = new List<int>();
        foreach (var (a, b) in Tramos)
        {
            int ia = Array.IndexOf(Paradas, a), ib = Array.IndexOf(Paradas, b);
            lista[ia].Add(ib); lista[ib].Add(ia);
        }
        return lista;
    }

    // ---------- PARTE A · MODELAR: LISTA Y MATRIZ DE ADYACENCIA ----------
    public static void Modelo()
    {
        Console.WriteLine("PARTE A · EL MAPA DE MICROS COMO GRAFO (10 nodos, 9 aristas)");
        var lista = ConstruirLista();
        Console.WriteLine("  Lista de adyacencia (cada nodo guarda sus vecinos):");
        for (int i = 0; i < Paradas.Length; i++)
            Console.WriteLine($"    {Paradas[i],-12} → {string.Join(", ", lista[i].Select(j => Paradas[j]))}");
        Console.WriteLine();
        Console.WriteLine("  Matriz de adyacencia (1 = hay tramo directo), primeras 6 paradas:");
        Console.WriteLine($"    {"",12} " + string.Join(" ", Paradas.Take(6).Select(p => p[..3])));
        for (int i = 0; i < 6; i++)
            Console.WriteLine($"    {Paradas[i],-12} " + string.Join("   ", Enumerable.Range(0, 6).Select(j => lista[i].Contains(j) ? "1" : "0")));
        Console.WriteLine();
        Console.WriteLine("  Lista: memoria ~ V + E (solo lo que existe). Matriz: V × V (guarda también todos los «no hay tramo»).");
    }

    // ---------- PARTE B · BFS (A LO ANCHO) Y DFS (EN PROFUNDIDAD) ----------
    private static (List<int> orden, int[] distancia, int[] anterior) Bfs(List<int>[] g, int origen)
    {
        var distancia = Enumerable.Repeat(-1, g.Length).ToArray();
        var anterior = Enumerable.Repeat(-1, g.Length).ToArray();
        var orden = new List<int>();
        var cola = new Queue<int>();
        distancia[origen] = 0; cola.Enqueue(origen);
        while (cola.Count > 0)
        {
            int u = cola.Dequeue();
            orden.Add(u);
            foreach (int v in g[u])
                if (distancia[v] == -1) { distancia[v] = distancia[u] + 1; anterior[v] = u; cola.Enqueue(v); }
        }
        return (orden, distancia, anterior);
    }

    private static List<int> Dfs(List<int>[] g, int origen)
    {
        var visto = new bool[g.Length];
        var orden = new List<int>();
        var pila = new Stack<int>();
        pila.Push(origen);
        while (pila.Count > 0)
        {
            int u = pila.Pop();
            if (visto[u]) continue;
            visto[u] = true; orden.Add(u);
            for (int k = g[u].Count - 1; k >= 0; k--) if (!visto[g[u][k]]) pila.Push(g[u][k]);   // el primer vecino sale primero
        }
        return orden;
    }

    public static void Recorridos()
    {
        Console.WriteLine("PARTE B · DOS FORMAS DE RECORRER EL MAPA DESDE «Plaza»");
        var g = ConstruirLista();
        int plaza = Array.IndexOf(Paradas, "Plaza");
        var (ordenBfs, distancia, anterior) = Bfs(g, plaza);
        Console.WriteLine("  BFS (a lo ancho, con una COLA): primero todos los vecinos, después los vecinos de los vecinos...");
        Console.WriteLine($"    orden: {string.Join(" → ", ordenBfs.Select(i => Paradas[i]))}");
        for (int nivel = 0; nivel <= distancia.Max(); nivel++)
            Console.WriteLine($"    a {nivel} tramo(s): {string.Join(", ", Enumerable.Range(0, g.Length).Where(i => distancia[i] == nivel).Select(i => Paradas[i]))}");
        Console.WriteLine();
        int destino = Array.IndexOf(Paradas, "Estadio");
        var camino = new List<string>();
        for (int v = destino; v != -1; v = anterior[v]) camino.Add(Paradas[v]);
        camino.Reverse();
        Console.WriteLine($"  Camino con MENOS tramos de Plaza a Estadio: {string.Join(" → ", camino)}  ({distancia[destino]} tramos)");
        Console.WriteLine();
        Console.WriteLine("  DFS (en profundidad, con una PILA): sigue un camino hasta el fondo y recién después vuelve.");
        Console.WriteLine($"    orden: {string.Join(" → ", Dfs(g, plaza).Select(i => Paradas[i]))}");
        Console.WriteLine();
        Console.WriteLine("  BFS usa una cola y encuentra el camino con MENOS tramos. DFS usa una pila (o recursión) y sirve para explorar a fondo,");
        Console.WriteLine("  detectar ciclos y ordenar dependencias. Los dos visitan cada nodo y cada arista una vez: O(V + E).");
    }

    // ---------- PARTE C · COMPONENTES: ¿QUÉ ZONAS ESTÁN DESCONECTADAS? ----------
    public static void Componentes()
    {
        Console.WriteLine("PARTE C · ¿SE PUEDE LLEGAR DE UNA PARADA A OTRA? (componentes conexas)");
        var g = ConstruirLista();
        var visto = new bool[g.Length];
        int numero = 0;
        for (int i = 0; i < g.Length; i++)
        {
            if (visto[i]) continue;
            numero++;
            var (orden, _, _) = Bfs(g, i);
            foreach (int v in orden) visto[v] = true;
            Console.WriteLine($"  Zona {numero}: {string.Join(", ", orden.Select(v => Paradas[v]))}  ({orden.Count} paradas)");
        }
        Console.WriteLine();
        Console.WriteLine($"  Hay {numero} zonas: desde «Plaza» NO se llega a «Aeropuerto». Una sola pasada de BFS por zona responde esto en O(V + E).");
    }

    // ---------- PARTE D · LA REPRESENTACIÓN IMPORTA ----------
    private static List<int>[] GrafoAlAzar(int v, int semilla)
    {
        var azar = new Random(semilla);
        var g = new List<int>[v];
        for (int i = 0; i < v; i++) g[i] = new List<int>();
        for (int i = 0; i + 1 < v; i++) { g[i].Add(i + 1); g[i + 1].Add(i); }      // una cadena: todo queda conectado
        for (int k = 0; k < 2 * v; k++)
        {
            int a = azar.Next(v), b = azar.Next(v);
            if (a != b) { g[a].Add(b); g[b].Add(a); }
        }
        return g;
    }

    private static int BfsConMatriz(bool[,] m, int v)
    {
        var visto = new bool[v];
        var cola = new Queue<int>();
        visto[0] = true; cola.Enqueue(0);
        int alcanzados = 0;
        while (cola.Count > 0)
        {
            int u = cola.Dequeue(); alcanzados++;
            for (int w = 0; w < v; w++)                                              // hay que mirar TODA la fila
                if (m[u, w] && !visto[w]) { visto[w] = true; cola.Enqueue(w); }
        }
        return alcanzados;
    }

    public static void Representacion()
    {
        Console.WriteLine("PARTE D · EL MISMO BFS, DOS REPRESENTACIONES (grafo ralo: unas 3 aristas por nodo)");
        Console.WriteLine($"  {"nodos",9} {"lista ms",10} {"x",6} {"matriz ms",11} {"x",6}   (lista: O(V+E) · matriz: O(V²))");
        double lPrev = 0, mPrev = 0;
        foreach (int v in new[] { 2_000, 4_000, 8_000 })
        {
            var g = GrafoAlAzar(v, 5);
            var m = new bool[v, v];
            for (int u = 0; u < v; u++) foreach (int w in g[u]) m[u, w] = true;
            int a1 = 0, a2 = 0;
            double l = Promedio(() => a1 = Bfs(g, 0).orden.Count);
            double mm = Promedio(() => a2 = BfsConMatriz(m, v));
            Console.WriteLine($"  {v,9:N0} {l,10:0.00} {(lPrev == 0 ? "—" : "x" + (l / lPrev).ToString("0.0")),6} {mm,11:0.0} {(mPrev == 0 ? "—" : "x" + (mm / mPrev).ToString("0.0")),6}{(a1 == a2 ? "" : "   ← NO COINCIDEN")}");
            lPrev = l; mPrev = mm;
        }
        Console.WriteLine();
        Console.WriteLine($"  {"nodos",9} {"BFS con lista ms",18} {"x",6} {"memoria de una matriz",24}");
        double prev = 0;
        foreach (int v in new[] { 100_000, 200_000, 400_000, 800_000 })
        {
            var g = GrafoAlAzar(v, 9);
            double t = Promedio(() => Bfs(g, 0));
            double gigas = (double)v * v / 8 / 1e9;                                  // 1 bit por casilla
            Console.WriteLine($"  {v,9:N0} {t,18:0.0} {(prev == 0 ? "—" : "x" + (t / prev).ToString("0.0")),6} {gigas,17:N1} GB (1 bit/casilla)");
            prev = t;
        }
        Console.WriteLine("  Con la lista, doble de nodos y aristas: ~x2. Una matriz de 800.000 × 800.000 no cabe en un computador normal. Elegir la representación es elegir si el problema se puede resolver.");
    }

    private static double Promedio(Action accion)
    {
        accion();
        double suma = 0;
        for (int i = 0; i < 3; i++)
        {
            var reloj = Stopwatch.StartNew();
            accion();
            reloj.Stop();
            suma += reloj.Elapsed.TotalMilliseconds;
        }
        return suma / 3;
    }

    public static void Correr()
    {
        Modelo();
        Console.WriteLine();
        Console.WriteLine(new string('=', 100));
        Recorridos();
        Console.WriteLine();
        Console.WriteLine(new string('=', 100));
        Componentes();
        Console.WriteLine();
        Console.WriteLine(new string('=', 100));
        Representacion();
    }
}
