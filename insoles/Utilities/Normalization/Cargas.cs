using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Esta clase no se usa, es otra forma de utilizar el GRF
/// </summary>
public class GRFCalculator
{
    public GRFCalculator()
    {
        // Ejemplo de array de cargas y tiempos (valores ficticios)
        double[] cargas = { 100, 150, 200, 250, 200, 150, 100 };
        double[] tiempos = { 0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6 };

        // Calcular GRF medio y desviación típica
        Tuple<double[], double[]> grf = CalcularGRF(cargas, tiempos);
        double[] grfMedio = grf.Item1;
        double[] grfDesviacionTipica = grf.Item2;

        // Imprimir los resultados
        Console.WriteLine("GRF medio:");
        ImprimirArray(grfMedio);
        Console.WriteLine("GRF desviación típica:");
        ImprimirArray(grfDesviacionTipica);
    }

    // Función para calcular GRF medio y desviación típica
    public static Tuple<double[], double[]> CalcularGRF(double[] cargas, double[] tiempos)
    {
        List<double> grfMedio = new List<double>();
        List<double> grfDesviacionTipica = new List<double>();

        // Separar los datos por fases
        List<List<double>> fasesGRF = SepararFases(cargas, tiempos);

        // Calcular el GRF medio y desviación típica de cada fase
        foreach (List<double> faseGRF in fasesGRF)
        {
            double media = faseGRF.Average();
            double desviacionTipica = CalcularDesviacionTipica(faseGRF, media);

            grfMedio.Add(media);
            grfDesviacionTipica.Add(desviacionTipica);
        }

        return Tuple.Create(grfMedio.ToArray(), grfDesviacionTipica.ToArray());
    }

    // Función para separar los datos por fases (por ejemplo, fase de apoyo y fase de vuelo)
    public static List<List<double>> SepararFases(double[] cargas, double[] tiempos)
    {
        List<List<double>> fasesGRF = new List<List<double>>();
        List<double> faseGRF = new List<double>();

        for (int i = 0; i < cargas.Length; i++)
        {
            if (EsFaseDeApoyo(tiempos[i]))
            {
                faseGRF.Add(cargas[i]);
            }
            else
            {
                if (faseGRF.Count > 0)
                {
                    fasesGRF.Add(faseGRF);
                    faseGRF = new List<double>();
                }
            }
        }

        if (faseGRF.Count > 0)
        {
            fasesGRF.Add(faseGRF);
        }

        return fasesGRF;
    }

    // Función para determinar si un tiempo corresponde a la fase de apoyo
    public static bool EsFaseDeApoyo(double tiempo)
    {
        // Implementa la lógica para determinar si el tiempo corresponde a la fase de apoyo
        // Puedes ajustar esta función según tus criterios y datos específicos
        // Por ejemplo, puedes usar umbrales de tiempo o algoritmos más complejos

        // Ejemplo: Consideramos que una fase de apoyo ocurre entre los tiempos 0.1 y 0.4
        if (tiempo >= 0.1 && tiempo <= 0.4)
        {
            return true;
        }

        return false;
    }

    // Función para calcular la desviación típica
    public static double CalcularDesviacionTipica(List<double> valores, double media)
    {
        double sumatoria = valores.Sum(valor => Math.Pow(valor - media, 2));
        double varianza = sumatoria / valores.Count;
        double desviacionTipica = Math.Sqrt(varianza);
        return desviacionTipica;
    }

    // Función para imprimir un array
    public static void ImprimirArray(double[] array)
    {
        foreach (double valor in array)
        {
            Console.WriteLine(valor);
        }
    }
}
