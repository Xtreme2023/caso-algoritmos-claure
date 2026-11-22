// CLASE 10 · HEAPS (MONTÍCULOS), COLAS DE PRIORIDAD Y HEAPSORT
// Un heap es un árbol binario COMPLETO guardado en un arreglo, con una regla: cada padre es menor o igual que sus hijos
// (heap mínimo). El menor siempre está en la posición 0. Hijos de i: 2i+1 y 2i+2. Padre de i: (i-1)/2.
// Las partes A, B y C son exactas (trazas y conteos): salen iguales en toda máquina.
// Las partes D y E miden tiempos con el protocolo (calentar, repetir 3 veces, promediar) y varían con la máquina.

using System.Diagnostics;

namespace C10.Heap;

public static class Demo
{
    // ---------- EL HEAP MÍNIMO, ESCRITO A MANO ----------
    private sealed class HeapMin
    {
        private readonly int[] _a = new int[64];
        private int _n;
        public int Cantidad => _n;
        public int Minimo => _a[0];

        public void Insertar(int x, bool traza)
        {
            _a[_n++] = x;
            int i = _n - 1;
            while (i > 0)
            {
                int padre = (i - 1) / 2;
                if (_a[padre] <= _a[i]) break;
                if (traza) Console.WriteLine($"      {_a[i]} es menor que su padre {_a[padre]} → intercambian (sube)");
                (_a[padre], _a[i]) = (_a[i], _a[padre]);
                i = padre;
            }
        }

        public int ExtraerMinimo(bool traza)
        {
            int minimo = _a[0];
            _a[0] = _a[--_n];                                  // el último pasa a la raíz...
            int i = 0;
            while (true)                                       // ...y BAJA hasta que sus hijos sean mayores
            {
                int izq = 2 * i + 1, der = 2 * i + 2, menor = i;
                if (izq < _n && _a[izq] < _a[menor]) menor = izq;
                if (der < _n && _a[der] < _a[menor]) menor = der;
                if (menor == i) break;
                if (traza) Console.WriteLine($"      {_a[i]} es mayor que su hijo {_a[menor]} → intercambian (baja)");
                (_a[i], _a[menor]) = (_a[menor], _a[i]);
                i = menor;
            }
            return minimo;
        }

        public string Contenido() => "[" + string.Join(", ", _a.Take(_n)) + "]";
    }

    // ---------- PARTE A · INSERTAR Y EXTRAER, PASO A PASO ----------
    public static void Heap()
    {
        Console.WriteLine("PARTE A · UN HEAP MÍNIMO CON 7 CLAVES (el arreglo ES el árbol)");
        var heap = new HeapMin();
        foreach (int x in new[] { 50, 30, 70, 20, 40, 60, 10 })
        {
            Console.WriteLine($"  Insertar {x,2}:");
            heap.Insertar(x, true);
            Console.WriteLine($"    arreglo: {heap.Contenido()}   menor = {heap.Minimo}");
        }
        Console.WriteLine();
        Console.WriteLine("  Extraer el menor, tres veces (siempre sale el de la posición 0):");
        for (int i = 0; i < 3; i++)
        {
            int salio = heap.ExtraerMinimo(true);
            Console.WriteLine($"    sale {salio,2}   → arreglo: {heap.Contenido()}");
        }
        Console.WriteLine();
        Console.WriteLine("  Ver el menor cuesta O(1). Insertar y extraer cuestan O(log n): se sube o se baja a lo sumo la altura del árbol.");
    }

    // ---------- PARTE B · LA COLA DE PRIORIDAD: LA FILA DEL HOSPITAL ----------
    public static void Prioridad()
    {
        Console.WriteLine("PARTE B · LA FILA DEL HOSPITAL: primero el más grave, no el que llegó primero");
        var llegadas = new (string nombre, int gravedad)[]
        {
            ("Ana", 4), ("Beto", 2), ("Carla", 5), ("Diego", 1), ("Elena", 3), ("Fabian", 2),
        };
        Console.WriteLine("  (gravedad 1 = crítico ... 5 = leve; en un empate, el que llegó antes)");
        Console.WriteLine();
        Console.WriteLine("  Llegan en este orden: " + string.Join(", ", llegadas.Select(p => $"{p.nombre}({p.gravedad})")));
        var cola = new PriorityQueue<string, (int gravedad, int llegada)>();
        for (int i = 0; i < llegadas.Length; i++) cola.Enqueue(llegadas[i].nombre, (llegadas[i].gravedad, i));
        Console.WriteLine("  Los atienden en este orden: " + string.Join(", ", Atender(cola, llegadas)));
        Console.WriteLine();
        Console.WriteLine("  Con una cola normal (FIFO) Diego, que es el crítico, habría esperado a los otros cinco.");
        Console.WriteLine("  PriorityQueue de .NET es un heap: Enqueue y Dequeue cuestan O(log n); Peek (ver el primero), O(1).");
    }

    private static IEnumerable<string> Atender(PriorityQueue<string, (int, int)> cola, (string nombre, int gravedad)[] llegadas)
    {
        while (cola.Count > 0)
        {
            string quien = cola.Dequeue();
            yield return $"{quien}({llegadas.First(p => p.nombre == quien).gravedad})";
        }
    }

    // ---------- PARTE C · HEAPSORT, CONTANDO COMPARACIONES ----------
    private static long HeapSort(int[] a)
    {
        long c = 0;
        int n = a.Length;
        for (int i = n / 2 - 1; i >= 0; i--) Hundir(a, i, n, ref c);   // 1) armar el heap (máximo) en O(n)
        for (int fin = n - 1; fin > 0; fin--)                         // 2) n veces: el mayor pasa al final y se rearma
        {
            (a[0], a[fin]) = (a[fin], a[0]);
            Hundir(a, 0, fin, ref c);
        }
        return c;
    }

    private static void Hundir(int[] a, int i, int n, ref long c)
    {
        while (true)
        {
            int izq = 2 * i + 1, der = izq + 1, mayor = i;
            if (izq < n) { c++; if (a[izq] > a[mayor]) mayor = izq; }
            if (der < n) { c++; if (a[der] > a[mayor]) mayor = der; }
            if (mayor == i) return;
            (a[i], a[mayor]) = (a[mayor], a[i]);
            i = mayor;
        }
    }

    public static void Ordenar()
    {
        Console.WriteLine("PARTE C · HEAPSORT: armar un heap y sacar el mayor n veces");
        Console.WriteLine($"  {"n",8} {"comparaciones",15} {"x",6} {"n × log2 n",14} {"comparaciones / (n log2 n)",28}");
        var azar = new Random(21);
        long prev = 0;
        foreach (int n in new[] { 1_000, 2_000, 4_000, 8_000, 16_000 })
        {
            var datos = Enumerable.Range(0, n).Select(_ => azar.Next()).ToArray();
            long c = HeapSort(datos);
            if (!EstaOrdenado(datos)) throw new InvalidOperationException("HeapSort dejó el arreglo desordenado");
            double nlogn = n * Math.Log2(n);
            Console.WriteLine($"  {n,8:N0} {c,15:N0} {(prev == 0 ? "—" : "x" + ((double)c / prev).ToString("0.0")),6} {nlogn,14:N0} {c / nlogn,28:0.00}");
            prev = c;
        }
        Console.WriteLine("  Doble de datos: un poco más del doble de comparaciones (n log n). Hasta 2 comparaciones por nivel: aquí ~1,7 a 1,8 × n log2 n.");
        Console.WriteLine("  Garantía: n log n SIEMPRE (no hay peor caso malo como en QuickSort) y ordena en el lugar, sin memoria extra.");
    }

    private static bool EstaOrdenado(int[] a)
    {
        for (int i = 1; i < a.Length; i++) if (a[i - 1] > a[i]) return false;
        return true;
    }

    // ---------- PARTE D · EL TORNEO FINAL (se suma HeapSort) ----------
    private static void MergeSort(int[] a)
    {
        var tmp = new int[a.Length];
        Mezclar(a, tmp, 0, a.Length - 1);
    }

    private static void Mezclar(int[] a, int[] tmp, int ini, int fin)
    {
        if (ini >= fin) return;
        int medio = ini + (fin - ini) / 2;
        Mezclar(a, tmp, ini, medio);
        Mezclar(a, tmp, medio + 1, fin);
        int i = ini, j = medio + 1, k = ini;
        while (i <= medio && j <= fin) tmp[k++] = a[i] <= a[j] ? a[i++] : a[j++];
        while (i <= medio) tmp[k++] = a[i++];
        while (j <= fin) tmp[k++] = a[j++];
        for (k = ini; k <= fin; k++) a[k] = tmp[k];
    }

    private static void QuickSort(int[] a, int ini, int fin)
    {
        if (ini >= fin) return;
        int p = ini + (fin - ini) / 2;
        (a[p], a[fin]) = (a[fin], a[p]);
        int pivote = a[fin], i = ini;
        for (int j = ini; j < fin; j++) if (a[j] < pivote) { (a[i], a[j]) = (a[j], a[i]); i++; }
        (a[i], a[fin]) = (a[fin], a[i]);
        QuickSort(a, ini, i - 1);
        QuickSort(a, i + 1, fin);
    }

    public static void Torneo()
    {
        Console.WriteLine("PARTE D · EL TORNEO FINAL DE ORDENAMIENTOS (datos al azar)");
        Console.WriteLine($"  {"n",10} {"HeapSort ms",13} {"MergeSort ms",14} {"QuickSort ms",14} {"Array.Sort ms",15}");
        foreach (int n in new[] { 100_000, 1_000_000, 4_000_000 })
        {
            var azar = new Random(8);
            var original = Enumerable.Range(0, n).Select(_ => azar.Next()).ToArray();
            double h = PromedioConCopia(original, a => HeapSort(a));
            double m = PromedioConCopia(original, MergeSort);
            double q = PromedioConCopia(original, a => QuickSort(a, 0, a.Length - 1));
            double s = PromedioConCopia(original, a => Array.Sort(a));
            Console.WriteLine($"  {n,10:N0} {h,13:0.0} {m,14:0.0} {q,14:0.0} {s,15:0.0}");
        }
        Console.WriteLine("  Los cuatro son n log n. HeapSort suele quedar más lento que QuickSort: salta por la memoria (padre e hijos están lejos) y la caché de la CPU lo siente.");
    }

    // ---------- PARTE E · EL USO REAL: LOS k MAYORES SIN ORDENAR TODO ----------
    public static void TopK()
    {
        Console.WriteLine("PARTE E · LOS 10 MEJORES PUNTAJES DE 5.000.000 (ranking)");
        const int n = 5_000_000, k = 10;
        var azar = new Random(4);
        var puntajes = Enumerable.Range(0, n).Select(_ => azar.Next()).ToArray();
        int[] conHeap = Array.Empty<int>(), conSort = Array.Empty<int>();
        double th = Promedio(() =>
        {
            var heap = new PriorityQueue<int, int>();                  // heap MÍNIMO de tamaño k: guarda los k mayores vistos
            foreach (int p in puntajes)
            {
                if (heap.Count < k) heap.Enqueue(p, p);
                else if (p > heap.Peek()) { heap.Dequeue(); heap.Enqueue(p, p); }
            }
            var salida = new List<int>();
            while (heap.Count > 0) salida.Add(heap.Dequeue());
            salida.Reverse();
            conHeap = salida.ToArray();
        });
        double ts = Promedio(() =>
        {
            var copia = (int[])puntajes.Clone();
            Array.Sort(copia);
            conSort = copia.Skip(n - k).Reverse().ToArray();
        });
        Console.WriteLine($"  Heap de tamaño {k}  : O(n log k)   {th,8:0.0} ms");
        Console.WriteLine($"  Ordenar todo      : O(n log n)   {ts,8:0.0} ms");
        Console.WriteLine($"  ¿Mismos 10 mayores? {(conHeap.SequenceEqual(conSort) ? "sí" : "NO")}");
        Console.WriteLine("  El heap solo mantiene 10 elementos: cada dato nuevo cuesta a lo sumo log 10. Es el truco de los rankings.");
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

    private static double PromedioConCopia(int[] original, Action<int[]> ordenar)
    {
        var trabajo = (int[])original.Clone();
        ordenar(trabajo);                                              // calentamiento
        if (!EstaOrdenado(trabajo)) throw new InvalidOperationException("quedó desordenado");
        double suma = 0;
        for (int i = 0; i < 3; i++)
        {
            Array.Copy(original, trabajo, original.Length);
            var reloj = Stopwatch.StartNew();
            ordenar(trabajo);
            reloj.Stop();
            suma += reloj.Elapsed.TotalMilliseconds;
        }
        return suma / 3;
    }

    public static void Correr()
    {
        Heap();
        Console.WriteLine();
        Console.WriteLine(new string('=', 100));
        Prioridad();
        Console.WriteLine();
        Console.WriteLine(new string('=', 100));
        Ordenar();
        Console.WriteLine();
        Console.WriteLine(new string('=', 100));
        Torneo();
        Console.WriteLine();
        Console.WriteLine(new string('=', 100));
        TopK();
    }
}
