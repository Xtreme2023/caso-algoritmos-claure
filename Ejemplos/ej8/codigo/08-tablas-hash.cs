// CLASE 8 · TABLAS HASH — la búsqueda casi instantánea
// Idea: en vez de recorrer o de ordenar, una FUNCIÓN calcula en qué casillero debe estar el dato.
// Las partes A, B y D son exactas (conteos y trazas): salen iguales en toda máquina.
// La parte C mide tiempos con el protocolo (calentar, repetir 3 veces, promediar) y varía con la máquina.

using System.Diagnostics;

namespace C8.Hash;

public static class Demo
{
    // ---------- PARTE A · LA FUNCIÓN HASH, A MANO ----------
    private static int HashTexto(string clave, int casilleros)
    {
        int suma = 0;
        foreach (char c in clave) suma += c;      // sumo los códigos de las letras...
        return suma % casilleros;                  // ...y me quedo con el resto: un número entre 0 y casilleros-1
    }

    public static void Funcion()
    {
        Console.WriteLine("PARTE A · LA FUNCIÓN HASH: de la clave al casillero");
        Console.WriteLine("hash(clave) = (suma de los códigos de sus letras) mod 7      → 7 casilleros, del 0 al 6");
        Console.WriteLine();
        var claves = new[] { "Ana", "Beto", "Carla", "Diego", "Elena", "Fabian", "Gaby", "Hugo" };
        var tabla = new List<string>[7];
        for (int i = 0; i < tabla.Length; i++) tabla[i] = new List<string>();
        foreach (var clave in claves)
        {
            int casillero = HashTexto(clave, 7);
            bool choque = tabla[casillero].Count > 0;
            tabla[casillero].Add(clave);
            Console.WriteLine($"  {clave,-7} → casillero {casillero}{(choque ? "   ← COLISIÓN: ya había " + string.Join(", ", tabla[casillero].Take(tabla[casillero].Count - 1)) : "")}");
        }
        Console.WriteLine();
        Console.WriteLine("La tabla queda así (cada casillero guarda su propia lista: «encadenamiento»):");
        for (int i = 0; i < tabla.Length; i++)
            Console.WriteLine($"  [{i}] {(tabla[i].Count == 0 ? "(vacío)" : string.Join(" → ", tabla[i]))}");
        Console.WriteLine();
        Console.WriteLine("Buscar «Gaby»: calculo hash → voy DIRECTO al casillero y miro solo esa lista. No recorro las 8 claves.");
    }

    // ---------- PARTE B · UNA TABLA HASH PROPIA, CONTANDO COMPARACIONES ----------
    private sealed class TablaEncadenada
    {
        private readonly List<int>[] _casilleros;
        private readonly Func<int, int, int> _hash;
        public long Comparaciones;

        public TablaEncadenada(int casilleros, Func<int, int, int> hash)
        {
            _casilleros = new List<int>[casilleros];
            for (int i = 0; i < casilleros; i++) _casilleros[i] = new List<int>();
            _hash = hash;
        }

        public void Agregar(int clave) => _casilleros[_hash(clave, _casilleros.Length)].Add(clave);

        public bool Contiene(int clave)
        {
            foreach (int c in _casilleros[_hash(clave, _casilleros.Length)])
            {
                Comparaciones++;
                if (c == clave) return true;
            }
            return false;
        }

        public int MayorCadena() => _casilleros.Max(l => l.Count);
    }

    public static void Tabla()
    {
        Console.WriteLine("PARTE B · UNA TABLA HASH PROPIA: ¿cuántas comparaciones por búsqueda?");
        Console.WriteLine();
        Console.WriteLine("B1 · LA FUNCIÓN IMPORTA. 1.000 claves, todas múltiplos de 10 (0, 10, 20, ... 9.990); 1.009 casilleros.");
        var claves = Enumerable.Range(0, 1000).Select(i => i * 10).ToArray();
        Console.WriteLine($"  {"función",-34} {"cadena más larga",17} {"comparaciones por búsqueda (promedio)",39}");
        foreach (var (nombre, hash) in new (string, Func<int, int, int>)[]
        {
            ("clave mod 10  (mala con estas claves)", (k, m) => k % 10 % m),
            ("clave mod 1.009  (primo)",              (k, m) => k % m),
        })
        {
            var t = new TablaEncadenada(1009, hash);
            foreach (int k in claves) t.Agregar(k);
            foreach (int k in claves) t.Contiene(k);
            Console.WriteLine($"  {nombre,-34} {t.MayorCadena(),17:N0} {(double)t.Comparaciones / claves.Length,39:0.00}");
        }
        Console.WriteLine("  Con la función mala todas las claves caen en el casillero 0: la tabla se vuelve UNA lista de 1.000 (O(n)).");
        Console.WriteLine();
        Console.WriteLine("B2 · EL FACTOR DE CARGA α = datos / casilleros. 101 casilleros fijos, claves al azar.");
        Console.WriteLine($"  {"datos",7} {"casilleros",11} {"α",6} {"comparaciones (real)",22} {"1 + α/2 (teoría)",18}");
        var azar = new Random(11);
        var todas = Enumerable.Range(0, 100_000).OrderBy(_ => azar.Next()).ToArray();
        foreach (int n in new[] { 101, 202, 404, 808, 1616 })
        {
            var t = new TablaEncadenada(101, (k, m) => k % m);
            for (int i = 0; i < n; i++) t.Agregar(todas[i]);
            for (int i = 0; i < n; i++) t.Contiene(todas[i]);
            double alfa = n / 101.0;
            Console.WriteLine($"  {n,7:N0} {101,11} {alfa,6:0.0} {(double)t.Comparaciones / n,22:0.00} {1 + alfa / 2,18:0.00}");
        }
        Console.WriteLine("  Si los casilleros NO crecen, cada búsqueda cuesta ~α: vuelve a ser O(n).");
        Console.WriteLine();
        Console.WriteLine("B3 · LA CURA: CRECER (rehash). Cuando α pasa de 0,75 se duplican los casilleros y se reubican las claves.");
        Console.WriteLine($"  {"datos",7} {"casilleros",11} {"α",6} {"comparaciones (real)",22}");
        foreach (int n in new[] { 101, 202, 404, 808, 1616 })
        {
            int casilleros = 101;
            while ((double)n / casilleros > 0.75) casilleros *= 2;
            var t = new TablaEncadenada(casilleros, (k, m) => k % m);
            for (int i = 0; i < n; i++) t.Agregar(todas[i]);
            for (int i = 0; i < n; i++) t.Contiene(todas[i]);
            Console.WriteLine($"  {n,7:N0} {casilleros,11:N0} {(double)n / casilleros,6:0.00} {(double)t.Comparaciones / n,22:0.00}");
        }
        Console.WriteLine("  Con α acotada, la búsqueda cuesta casi 1 comparación sin importar n: O(1) en promedio.");
    }

    // ---------- PARTE C · EL DUELO DEL PRIMER DÍA, AHORA MEDIDO POR TAMAÑOS ----------
    public static void Medir()
    {
        Console.WriteLine("PARTE C · LISTA CONTRA HASH: 1.000 búsquedas de claves que NO están (el peor caso)");
        Console.WriteLine($"{"n",10} {"List.Contains ms",18} {"x",6} {"HashSet.Contains ms",21} {"x",6} {"armar el HashSet ms",21}");
        double lPrev = 0, hPrev = 0;
        foreach (int n in new[] { 500_000, 1_000_000, 2_000_000, 4_000_000 })
        {
            var lista = Enumerable.Range(0, n).Select(i => i * 2).ToList();
            var conjunto = new HashSet<int>(lista);
            var ausentes = Enumerable.Range(0, 1000).Select(i => i * 2 + 1).ToArray();
            int encontrados = 0;
            double l = Promedio(() => { foreach (int k in ausentes) if (lista.Contains(k)) encontrados++; });
            double h = Promedio(() => { foreach (int k in ausentes) if (conjunto.Contains(k)) encontrados++; });
            double armar = Promedio(() => { _ = new HashSet<int>(lista); });
            Console.WriteLine($"{n,10:N0} {l,18:0.00} {(lPrev == 0 ? "—" : "x" + (l / lPrev).ToString("0.0")),6} {h,21:0.000} {(hPrev == 0 ? "—" : "x" + (h / hPrev).ToString("0.0")),6} {armar,21:0.0}");
            lPrev = l; hPrev = h;
            if (encontrados != 0) Console.WriteLine("(error: alguien encontró lo que no estaba)");
        }
        Console.WriteLine();
        Console.WriteLine("Doble de datos: la lista tarda ~x2 en teoría (acá hay ruido de caché y de CPU: mira la tendencia, no cada fila); el hash sigue en microsegundos.");
        Console.WriteLine("Armar el HashSet cuesta O(n) UNA vez y se paga si vas a buscar muchas veces.");
    }

    // ---------- PARTE D · EL USO DE TODOS LOS DÍAS: CONTAR CON UN DICCIONARIO ----------
    public static void Palabras()
    {
        Console.WriteLine("PARTE D · UN DICCIONARIO (clave → valor): contar cuántas veces aparece cada palabra");
        const string texto = "todo tiene su tiempo y todo lo que se quiere debajo del cielo tiene su hora " +
                             "tiempo de nacer y tiempo de morir tiempo de plantar y tiempo de arrancar lo plantado";
        var cuenta = new Dictionary<string, int>();
        foreach (var palabra in texto.Split(' '))
            cuenta[palabra] = cuenta.TryGetValue(palabra, out int n) ? n + 1 : 1;
        Console.WriteLine($"  El texto tiene {texto.Split(' ').Length} palabras y {cuenta.Count} distintas. Las más repetidas:");
        foreach (var par in cuenta.OrderByDescending(p => p.Value).ThenBy(p => p.Key).Take(5))
            Console.WriteLine($"    {par.Key,-8} {par.Value}");
        Console.WriteLine();
        Console.WriteLine("  Cada palabra se busca y se actualiza en O(1) promedio: contar n palabras cuesta O(n), no O(n²).");
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
        Funcion();
        Console.WriteLine();
        Console.WriteLine(new string('=', 100));
        Tabla();
        Console.WriteLine();
        Console.WriteLine(new string('=', 100));
        Medir();
        Console.WriteLine();
        Console.WriteLine(new string('=', 100));
        Palabras();
    }
}
