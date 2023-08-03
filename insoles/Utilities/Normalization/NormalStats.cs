﻿using CsvHelper.Configuration;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stdgraph.Lib
{
    public class NormalStats

    {
        public List<double> time = new List<double>();
        public List<double> rtotal = new List<double>();
        public NormalStats() { }
        #region Normalization 0-100 api
        //--------------------------------------------------------------------------
        // Pongo por autoría
        //--------------------------------------------------------------------------
        // This function returns the data filtered. Converted to C# 2 July 2014.
        // Original source written in VBA for Microsoft Excel, 2000 by Sam Van
        // Wassenbergh (University of Antwerp), 6 june 2007.
        //--------------------------------------------------------------------------
        public double[] Butterworth(double[] indata, double deltaTimeinsec, double CutOff)
        {
            if (indata == null) return null;
            if (CutOff == 0) return indata;

            double Samplingrate = 1 / deltaTimeinsec;
            long dF2 = indata.Length - 1;        // The data range is set with dF2
            double[] Dat2 = new double[dF2 + 4]; // Array with 4 extra points front and back
            double[] data = indata; // Ptr., changes passed data

            // Copy indata to Dat2
            for (long r = 0; r < dF2; r++)
            {
                Dat2[2 + r] = indata[r];
            }
            Dat2[1] = Dat2[0] = indata[0];
            Dat2[dF2 + 3] = Dat2[dF2 + 2] = indata[dF2];

            const double pi = 3.14159265358979;
            double wc = Math.Tan(CutOff * pi / Samplingrate);
            double k1 = 1.414213562 * wc; // Sqrt(2) * wc
            double k2 = wc * wc;
            double a = k2 / (1 + k1 + k2);
            double b = 2 * a;
            double c = a;
            double k3 = b / k2;
            double d = -2 * a + k3;
            double e = 1 - 2 * a - k3;

            // RECURSIVE TRIGGERS - ENABLE filter is performed (first, last points constant)
            double[] DatYt = new double[dF2 + 4];
            DatYt[1] = DatYt[0] = indata[0];
            for (long s = 2; s < dF2 + 2; s++)
            {
                DatYt[s] = a * Dat2[s] + b * Dat2[s - 1] + c * Dat2[s - 2]
                           + d * DatYt[s - 1] + e * DatYt[s - 2];
            }
            DatYt[dF2 + 3] = DatYt[dF2 + 2] = DatYt[dF2 + 1];

            // FORWARD filter
            double[] DatZt = new double[dF2 + 2];
            DatZt[dF2] = DatYt[dF2 + 2];
            DatZt[dF2 + 1] = DatYt[dF2 + 3];
            for (long t = -dF2 + 1; t <= 0; t++)
            {
                DatZt[-t] = a * DatYt[-t + 2] + b * DatYt[-t + 3] + c * DatYt[-t + 4]
                            + d * DatZt[-t + 1] + e * DatZt[-t + 2];
            }

            // Calculated points copied for return
            for (long p = 0; p < dF2; p++)
            {
                data[p] = DatZt[p];
            }

            return data;
        }

        // Cálculo de la desviación estándar de cada punto
        public double[] StdDevPointCalculation(IEnumerable<double> ys_temp)
        {
            double media = ys_temp.Average();

            // Calcular la desviación típica para cada punto
            List<double> desviacionesTipicas = new List<double>();
            foreach (double valor in ys_temp)
            {
                // Calcular la diferencia entre el valor y la media
                double diferencia = valor - media;

                // Elevar al cuadrado la diferencia
                double diferenciaCuadrada = Math.Pow(diferencia, 2);

                //El sqrt para que el gráfico no se vaya con valores grandes
                double diferenciaSqrt = Math.Sqrt(diferenciaCuadrada);

                double std = Math.Min(diferenciaSqrt, valor);
                // Agregar la diferencia cuadrada a la lista
                desviacionesTipicas.Add(std);
            }

            double[] dts = desviacionesTipicas.ToArray();

            return dts;
        }

        public void ConvertCsvToClass(string path)
        {
            //For CsvConfiguration

            CSVFileHeaders record;

            List<CSVFileHeaders> records;

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                NewLine = Environment.NewLine,
            };

            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                records = new List<CSVFileHeaders>();
                csv.Read();
                csv.ReadHeader();
                while (csv.Read())
                {
                    record = new CSVFileHeaders
                    {
                        Time = csv.GetField<double>("TIME"),
                        RTotal = csv.GetField<double>("LTOTAL")
                    };
                    records.Add(record);
                    time.Add(record.Time);
                    rtotal.Add(record.RTotal);

                }
            }


        }

        public double CalculateMean(double[] values)
        {
            double sum = 0;
            foreach (double value in values)
            {
                sum += value;
            }
            return sum / values.Length;
        }

        public double FindMinValue(double[] values)
        {
            double min = values[0];
            foreach (double value in values)
            {
                if (value < min)
                {
                    min = value;
                }
            }
            return min;
        }

        public double FindMaxValue(double[] values)
        {
            double max = values[0];
            foreach (double value in values)
            {
                if (value > max)
                {
                    max = value;
                }
            }
            return max;
        }

        public double[] NormalizeCargas(double[] cargas, double cargaMin, double cargaMax)
        {
            double[] cargasNormalizadas = new double[cargas.Length];
            for (int i = 0; i < cargas.Length; i++)
            {
                cargasNormalizadas[i] = (cargas[i] - cargaMin) / (cargaMax - cargaMin);
            }
            return cargasNormalizadas;
        }

        // linspace function
        public double[] linspace(double StartValue, double EndValue, int numberofpoints)
        {

            double[] parameterVals = new double[numberofpoints];
            double increment = Math.Abs(StartValue - EndValue) / Convert.ToDouble(numberofpoints - 1);
            int j = 0; //will keep a track of the numbers 
            double nextValue = StartValue;
            for (int i = 0; i < numberofpoints; i++)
            {


                parameterVals.SetValue(nextValue, j);
                j++;
                if (j > numberofpoints)
                {
                    throw new IndexOutOfRangeException();
                }
                nextValue = nextValue + increment;
            }
            return parameterVals;

        }
        public double CalculateStandardDeviation(double[] values)
        {
            double mean = CalculateMean(values);
            double sumSquaredDifferences = 0;
            foreach (double value in values)
            {
                sumSquaredDifferences += Math.Pow(value - mean, 2);
            }
            double variance = sumSquaredDifferences / values.Length;
            return Math.Sqrt(variance);
        }
        #endregion

    }

    public class CSVFileHeaders
    {
        public double Time
        {
            get; set;
        }
        public double RTotal
        {
            get; set;
        }
    }
}
