// CLASE 9 · ÁRBOLES Y ÁRBOL BINARIO DE BÚSQUEDA (BST)
// Regla del BST: en cada nodo, TODO lo de la izquierda es menor y TODO lo de la derecha es mayor.
// Buscar = el juego de adivinar el número: «¿mayor o menor?» y se descarta una rama entera.
// Las partes A y B son exactas (trazas y conteos): salen iguales en toda máquina.
// Las partes C y D miden tiempos con el protocolo (calentar, repetir 3 veces, promediar) y varían con la máquina.

using System.Diagnostics;

namespace C9.Arboles;

public static class Demo
{
    private sealed class Nodo
    {
        public int Clave;
        public Nodo? Izq, Der;
        public Nodo(int clave) => Clave = clave;
    }

    private sealed class Bst
    {
        public Nodo? Raiz;
        public long Comparaciones;

        public void Insertar(int clave)
        {
            if (Raiz == null) { Raiz = new Nodo(clave); return; }
            var actual = Raiz;
            while (true)
            {
                Comparaciones++;
                if (clave == actual.Clave) return;                    // sin repetidos
                if (clave < actual.Clave)
                {
                    if (actual.Izq == null) { actual.Izq = new Nodo(clave); return; }
                    actual = actual.Izq;
                }
                else
                {
                    if (actual.Der == null) { actual.Der = new Nodo(clave); return; }
                    actual = actual.Der;
                }
            }
        }

        public bool Buscar(int clave, List<int>? camino = null)
        {
            var actual = Raiz;
            while (actual != null)
            {
                Comparaciones++;
                camino?.Add(actual.Clave);
                if (clave == actual.Clave) return true;
                actual = clave < actual.Clave ? actual.Izq : actual.Der;
            }
            return false;
        }

        public int Altura()                                            // niveles del árbol (recorrido por niveles)
        {
            if (Raiz == null) return 0;
            int niveles = 0;
            var nivel = new List<Nodo> { Raiz };
            while (nivel.Count > 0)
            {
                niveles++;
                var siguiente = new List<Nodo>();
                foreach (var n in nivel)
                {
                    if (n.Izq != null) siguiente.Add(n.Izq);
                    if (n.Der != null) siguiente.Add(n.Der);
                }
                nivel = siguiente;
            }
            return niveles;
        }

        public IEnumerable<int> Inorden()                              // izquierda, nodo, derecha: sale ORDENADO
        {
            var pila = new Stack<Nodo>();
            var actual = Raiz;
            while (actual != null || pila.Count > 0)
            {
                while (actual != null) { pila.Push(actual); actual = actual.Izq; }
                actual = pila.Pop();
                yield return actual.Clave;
                actual = actual.Der;
            }
        }

        public void Dibujar(Nodo? n, int nivel)                        // el árbol «acostado»: derecha arriba, izquierda abajo
        {
            if (n == null) return;
            Dibujar(n.Der, nivel + 1);
            Console.WriteLine($"    {new string(' ', nivel * 6)}{n.Clave}");
            Dibujar(n.Izq, nivel + 1);
        }
    }

    private static void Preorden(Nodo? n, List<int> salida)
    {
        if (n == null) return;
        salida.Add(n.Clave); Preorden(n.Izq, salida); Preorden(n.Der, salida);
    }

    // ---------- PARTE A · CONSTRUIR, BUSCAR Y RECORRER (8 CLAVES, A MANO) ----------
    public static void Arbol()
    {
        Console.WriteLine("PARTE A · UN BST CON 8 CLAVES");
        var claves = new[] { 50, 30, 70, 20, 40, 60, 80, 35 };
        var bst = new Bst();
        foreach (int c in claves) bst.Insertar(c);
        Console.WriteLine($"  Se insertan, en este orden: {string.Join(", ", claves)}");
        Console.WriteLine("  El árbol «acostado» (la raíz a la izquierda; arriba van los mayores):");
        bst.Dibujar(bst.Raiz, 0);
        Console.WriteLine();
        foreach (int buscado in new[] { 35, 65 })
        {
            var camino = new List<int>();
            bool esta = bst.Buscar(buscado, camino);
            Console.WriteLine($"  Buscar {buscado}: pregunto por {string.Join(" → ", camino)}  → {(esta ? "ENCONTRADO" : "NO ESTÁ")} en {camino.Count} comparaciones (de 8 claves)");
        }
        Console.WriteLine();
        Console.WriteLine($"  Inorden (izquierda, nodo, derecha): {string.Join(" ", bst.Inorden())}   ← sale ORDENADO sin ordenar");
        var pre = new List<int>(); Preorden(bst.Raiz, pre);
        Console.WriteLine($"  Preorden (nodo, izquierda, derecha): {string.Join(" ", pre)}   ← sirve para COPIAR el árbol");
    }

    // ---------- PARTE B · LA ALTURA MANDA: ÁRBOL PAREJO CONTRA ÁRBOL DEGENERADO ----------
    public static void Altura()
    {
        Console.WriteLine("PARTE B · EL COSTO DE BUSCAR EN UN BST ES SU ALTURA");
        Console.WriteLine($"  {"n",7} {"datos llegan",14} {"altura",8} {"log2 n",8} {"comparaciones por búsqueda",28}");
        foreach (int n in new[] { 1_000, 10_000 })
        {
            var azar = new Random(5);
            var barajadas = Enumerable.Range(0, n).OrderBy(_ => azar.Next()).ToArray();
            foreach (var (nombre, orden) in new[] { ("al azar", barajadas), ("YA ORDENADOS", Enumerable.Range(0, n).ToArray()) })
            {
                var bst = new Bst();
                foreach (int c in orden) bst.Insertar(c);
                bst.Comparaciones = 0;
                foreach (int c in orden) bst.Buscar(c);
                Console.WriteLine($"  {n,7:N0} {nombre,14} {bst.Altura(),8:N0} {Math.Log2(n),8:0.0} {(double)bst.Comparaciones / n,28:0.0}");
            }
        }
        Console.WriteLine();
        Console.WriteLine("  Datos al azar: el árbol sale bastante parejo, altura de 2 a 3 veces log n, ~16 comparaciones por búsqueda con 10.000 claves.");
        Console.WriteLine("  Datos YA ORDENADOS: cada clave nueva cuelga a la derecha de la anterior. El árbol es una LISTA: altura n, ~5.000 comparaciones.");
        Console.WriteLine("  La cura se llama BALANCEO (AVL, árboles rojinegros): el árbol se reacomoda solo para no degenerar. Aquí es una noción; no se implementa.");
    }

    // ---------- PARTE C · LO QUE HACE UN ÁRBOL Y NO UN HASH: RANGOS ----------
    public static void Rangos()
    {
        Console.WriteLine("PARTE C · «¿CUÁNTOS VALORES HAY ENTRE lo Y hi?»  200 consultas, cada una con ~100 valores en el rango");
        Console.WriteLine("  (el árbol es SortedSet de .NET, un árbol rojinegro BALANCEADO; la lista no tiene orden: hay que mirarla toda)");
        Console.WriteLine($"  {"n",10} {"List (recorrer todo) ms",24} {"x",6} {"SortedSet (árbol) ms",23} {"x",6}");
        double lPrev = 0, aPrev = 0;
        foreach (int n in new[] { 200_000, 400_000, 800_000, 1_600_000 })
        {
            var azar = new Random(3);
            var datos = Enumerable.Range(0, n).Select(i => i * 4).OrderBy(_ => azar.Next()).ToList();
            var arbol = new SortedSet<int>(datos);
            var consultas = Enumerable.Range(0, 200).Select(i => i * 997 % n * 4).ToArray();
            long total = 0;
            double l = Promedio(() =>
            {
                foreach (int lo in consultas)
                {
                    int hi = lo + 400, cuenta = 0;
                    foreach (int x in datos) if (x >= lo && x <= hi) cuenta++;
                    total += cuenta;
                }
            });
            double a = Promedio(() =>
            {
                foreach (int lo in consultas) total += arbol.GetViewBetween(lo, lo + 400).Count;
            });
            Console.WriteLine($"  {n,10:N0} {l,24:0.0} {(lPrev == 0 ? "—" : "x" + (l / lPrev).ToString("0.0")),6} {a,23:0.00} {(aPrev == 0 ? "—" : "x" + (a / aPrev).ToString("0.0")),6}");
            lPrev = l; aPrev = a;
        }
        Console.WriteLine("  El árbol salta al primer valor del rango (log n) y solo recorre los ~100 que hay. Un hash no puede: no sabe de orden.");
    }

    // ---------- PARTE D · EL ÁRBOL DEGENERADO, MEDIDO ----------
    public static void Degenerado()
    {
        Console.WriteLine("PARTE D · ARMAR UN BST PROPIO CON CLAVES YA ORDENADAS (el peor caso)");
        Console.WriteLine($"  {"n",8} {"armar el árbol ms",19} {"x",6}   (n²/2 pasos: al duplicar n, ~x4)");
        double prev = 0;
        foreach (int n in new[] { 5_000, 10_000, 20_000 })
        {
            double t = Promedio(() =>
            {
                var bst = new Bst();
                for (int i = 0; i < n; i++) bst.Insertar(i);
            });
            Console.WriteLine($"  {n,8:N0} {t,19:0.0} {(prev == 0 ? "—" : "x" + (t / prev).ToString("0.0")),6}");
            prev = t;
        }
        Console.WriteLine("  Un árbol que degenera cuesta lo mismo que una lista. Por eso las librerías usan árboles BALANCEADOS.");
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
        Arbol();
        Console.WriteLine();
        Console.WriteLine(new string('=', 100));
        Altura();
        Console.WriteLine();
        Console.WriteLine(new string('=', 100));
        Rangos();
        Console.WriteLine();
        Console.WriteLine(new string('=', 100));
        Degenerado();
    }
}
