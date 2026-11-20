// CLASE 7 (segunda mitad) · ESTRUCTURAS DE DATOS: PILA, COLA Y LISTA
// Una estructura es una forma de guardar datos Y el costo de las operaciones que se hacen sobre ellos.
// Las trazas (partes A y B) son exactas: salen iguales en toda máquina. Los tiempos (partes C y D)
// se miden con el protocolo (calentar, repetir 3 veces, promediar) y varían con la máquina.

using System.Diagnostics;

namespace C7.Estructuras;

public static class Demo
{
    private static string Fondo(Stack<char> pila) => pila.Count == 0 ? "(vacía)" : string.Join(" ", pila.Reverse());

    // ---------- PARTE A · LA PILA (LIFO: el último en entrar es el primero en salir) ----------
    private static bool Balanceado(string texto, bool mostrar)
    {
        var pila = new Stack<char>();
        const string abren = "([{", cierran = ")]}";
        foreach (char c in texto)
        {
            int i = abren.IndexOf(c);
            if (i >= 0)
            {
                pila.Push(c);
                if (mostrar) Console.WriteLine($"    lee '{c}'  → apilo           pila (fondo → cima): {Fondo(pila)}");
                continue;
            }
            int j = cierran.IndexOf(c);
            if (j < 0) continue;
            if (pila.Count == 0 || pila.Peek() != abren[j])
            {
                if (mostrar) Console.WriteLine($"    lee '{c}'  → NO coincide con el tope ('{(pila.Count == 0 ? "pila vacía" : pila.Peek().ToString())}')");
                return false;
            }
            pila.Pop();
            if (mostrar) Console.WriteLine($"    lee '{c}'  → coincide, desapilo  pila (fondo → cima): {Fondo(pila)}");
        }
        return pila.Count == 0;
    }

    public static void Pila()
    {
        Console.WriteLine("PARTE A · LA PILA: ¿están bien cerrados los paréntesis?");
        foreach (var texto in new[] { "{[()]}", "{[(])}" })
        {
            Console.WriteLine($"  Texto: {texto}");
            bool ok = Balanceado(texto, true);
            Console.WriteLine($"  Resultado: {(ok ? "BIEN CERRADO" : "MAL CERRADO")}");
            Console.WriteLine();
        }
        Console.WriteLine("Apilar (Push) y desapilar (Pop) cuestan O(1): siempre se toca solo la cima. Es lo que usa «Deshacer» de un editor.");
    }

    // ---------- PARTE B · LA COLA (FIFO: el primero en entrar es el primero en salir) ----------
    public static void Cola()
    {
        Console.WriteLine("PARTE B · LA COLA: la fila del banco");
        var cola = new Queue<string>();
        foreach (var nombre in new[] { "Ana", "Beto", "Carla", "Diego" })
        {
            cola.Enqueue(nombre);
            Console.WriteLine($"  llega {nombre,-6} → fila: [{string.Join(", ", cola)}]");
        }
        while (cola.Count > 0)
        {
            string atendido = cola.Dequeue();
            Console.WriteLine($"  atienden a {atendido,-6} → fila: [{string.Join(", ", cola)}]");
        }
        Console.WriteLine();
        Console.WriteLine("Encolar y desencolar cuestan O(1) con una cola de verdad. ¿Y si usamos una lista y sacamos siempre el primero?");
        Console.WriteLine();
        Console.WriteLine($"{"n",9} {"Queue ms",10} {"x",6} {"List.RemoveAt(0) ms",21} {"x",6}");
        double qPrev = 0, lPrev = 0;
        foreach (int n in new[] { 10_000, 20_000, 40_000 })
        {
            double q = Promedio(() =>
            {
                var c = new Queue<int>(n);
                for (int i = 0; i < n; i++) c.Enqueue(i);
                while (c.Count > 0) c.Dequeue();
            });
            double l = Promedio(() =>
            {
                var lista = new List<int>(n);
                for (int i = 0; i < n; i++) lista.Add(i);
                while (lista.Count > 0) lista.RemoveAt(0);
            });
            Console.WriteLine($"{n,9:N0} {q,10:0.00} {(qPrev == 0 ? "—" : "x" + (q / qPrev).ToString("0.0")),6} {l,21:0.0} {(lPrev == 0 ? "—" : "x" + (l / lPrev).ToString("0.0")),6}");
            qPrev = q; lPrev = l;
        }
        Console.WriteLine("Sacar siempre el primero de una lista obliga a correr TODOS los demás un lugar: O(n) cada vez, O(n²) en total.");
    }

    // ---------- PARTE C · LA LISTA: ARREGLO CONTRA LISTA ENLAZADA ----------
    public static void Listas()
    {
        Console.WriteLine("PARTE C · DOS FORMAS DE GUARDAR UNA LISTA");
        Console.WriteLine("  Arreglo (List<T>): casillas pegadas en memoria → saltar a la posición i es directo, pero meter al inicio corre todo.");
        Console.WriteLine("  Lista enlazada (LinkedList<T>): cada nodo apunta al siguiente → meter al inicio es atar un nodo, pero llegar a la posición i es caminar.");
        Console.WriteLine();
        Console.WriteLine("  C1 · INSERTAR al inicio, n veces");
        Console.WriteLine($"  {"n",9} {"List.Insert(0) ms",19} {"x",6} {"LinkedList.AddFirst ms",24} {"x",6}");
        double aPrev = 0, ePrev = 0;
        foreach (int n in new[] { 20_000, 40_000, 80_000 })
        {
            double a = Promedio(() => { var l = new List<int>(); for (int i = 0; i < n; i++) l.Insert(0, i); });
            double e = Promedio(() => { var l = new LinkedList<int>(); for (int i = 0; i < n; i++) l.AddFirst(i); });
            Console.WriteLine($"  {n,9:N0} {a,19:0.0} {(aPrev == 0 ? "—" : "x" + (a / aPrev).ToString("0.0")),6} {e,24:0.00} {(ePrev == 0 ? "—" : "x" + (e / ePrev).ToString("0.0")),6}");
            aPrev = a; ePrev = e;
        }
        Console.WriteLine();
        Console.WriteLine("  C2 · LEER 1.000 veces el elemento de la mitad (lista de 100.000)");
        const int tam = 100_000;
        var arreglo = Enumerable.Range(0, tam).ToList();
        var enlazada = new LinkedList<int>(arreglo);
        long suma = 0;
        double t1 = Promedio(() => { for (int i = 0; i < 1000; i++) suma += arreglo[tam / 2]; });
        double t2 = Promedio(() => { for (int i = 0; i < 1000; i++) suma += enlazada.ElementAt(tam / 2); });
        Console.WriteLine($"  List[i]         : {t1,8:0.00} ms");
        Console.WriteLine($"  LinkedList (caminando nodo a nodo): {t2,8:0.0} ms");
        if (suma == 42) Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine("No hay una estructura «mejor»: hay operaciones que HACES MÁS. Elegir la estructura es elegir cuáles operaciones salen baratas.");
    }

    // ---------- PARTE D · EL MAPA DE COSTOS ----------
    public static void Mapa()
    {
        Console.WriteLine("PARTE D · EL MAPA (peor caso, n datos)");
        Console.WriteLine($"  {"operación",-34} {"arreglo",-9} {"lista enlazada",-15} {"pila / cola",-12}");
        Console.WriteLine($"  {"leer la posición i",-34} {"O(1)",-9} {"O(n)",-15} {"—",-12}");
        Console.WriteLine($"  {"insertar o borrar al inicio",-34} {"O(n)",-9} {"O(1)",-15} {"O(1)",-12}");
        Console.WriteLine($"  {"insertar al final",-34} {"O(1)*",-9} {"O(1)**",-15} {"O(1)",-12}");
        Console.WriteLine($"  {"buscar un valor",-34} {"O(n)",-9} {"O(n)",-15} {"O(n)",-12}");
        Console.WriteLine("  * amortizado: de vez en cuando el arreglo crece y copia.   ** si se guarda el puntero al último nodo.");
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
        Pila();
        Console.WriteLine();
        Console.WriteLine(new string('=', 100));
        Cola();
        Console.WriteLine();
        Console.WriteLine(new string('=', 100));
        Listas();
        Console.WriteLine();
        Console.WriteLine(new string('=', 100));
        Mapa();
    }
}
