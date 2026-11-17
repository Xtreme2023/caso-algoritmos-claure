Cristofer Johany Claure Cabrera
public static bool HayRepetidosCurado(int[] carnets)
    {
        if (carnets.Length <= 1) return false;
        
        // Se trabaja sobre una copia para no modificar la lista original
        int[] copia = (int[])carnets.Clone();
        Array.Sort(copia); // Ordenamiento: O(n log n)
        
        // Recorrido lineal: O(n)
        for (int i = 0; i < copia.Length - 1; i++)
        {
            if (copia[i] == copia[i + 1]) return true;
        }
        return false;
    }
-PROTOCOLO DE MEDICION -
N=10000  | Lenta:   150 milisegundos | Curada:    0 milisegundos
N=20000  | Lenta:   589 milisegundos | Curada:    1 milisegundos
N=40000  | Lenta:  2372 milisegundos | Curada:    2 milisegundos
N=80000  | Lenta:  9298 milisegundos | Curada:    5 milisegundos