using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypesLibrary
{
    public class Interpolation // Класс, инкапсулирующий алгоритм интерполяции функции одной переменной
    {
        public double[] X { get; private set; }
        public double[] Y { get; private set; }
        public double Interpolate(double x)
        {
            if (X.Length == 1) return Y[0];
            if (x.CompareTo(X[0]) < 0) return Y[0];
            if (x.CompareTo(X[X.Length - 1]) > 0) return Y[Y.Length - 1];
            int L = 0;
            int R = X.Length - 1;
            int M = (L + R) / 2;
            while ((R - L) != 1)
            {
                if (x.CompareTo(X[M]) > 0) L = M;
                else R = M;
                M = (L + R) / 2;
            }
            return Y[L] + (Y[R] - Y[L]) / (X[R] - X[L]) * (x - X[L]);
        }
        public Interpolation(double[] X, double[] Y)
        {
            if ((X == null) || (Y == null) || (X.Length != Y.Length) || (X.Length * Y.Length == 0))
                throw new Exception("Массивы данных для интерполяции должны быть не пустыми и одинаковой длины");
            this.X = new double[X.Length];
            this.Y = new double[Y.Length];
            for (int i = 0; i < X.Length; i++)
            {
                this.X[i] = X[i];
                this.Y[i] = Y[i];
            }
        }
    }
    public class DoubleInterpolation // Класс, инкапсулирующий алгоритм интерполяции функции двух переменных
    {
        public double[] X { get; private set; }
        public double[] Y { get; private set; }
        public double[,] Z { get; private set; }
        public double Interpolate(double x, double y)
        {
            Func<double[], double, int> NearestMax = (Source, Value) =>
            {
                int i = 0;
                for (i = 0; i < Source.Length; i++)
                    if (Value <= Source[i]) break;
                if (i == 0) i = 1;
                if (i >= Source.Length) i = Source.Length - 1;
                return i;
            };
            int XBottomIndex = NearestMax(X, x);
            int YRightIndex = NearestMax(Y, y);
            int XTopIndex = XBottomIndex - 1;
            int YLeftIndex = YRightIndex - 1;
            double XTop = X[XTopIndex];
            double XBottom = X[XBottomIndex];
            double YRight = Y[YRightIndex];
            double YLeft = Y[YLeftIndex];
            double ZTopLeft = Z[XTopIndex, YLeftIndex];
            double ZTopRight = Z[XTopIndex, YRightIndex];
            double ZBottomLeft = Z[XBottomIndex, YLeftIndex];
            double ZBottomRight = Z[XBottomIndex, YRightIndex];
            return (ZTopLeft * (XBottom - x) * (YRight - y) +
                    ZTopRight * (XBottom - x) * (y - YLeft) +
                    ZBottomLeft * (x - XTop) * (YRight - y) +
                    ZBottomRight * (x - XTop) * (y - YLeft)) /
                    ((XBottom - XTop) * (YRight - YLeft));
        }
        public DoubleInterpolation(double[] X, double[] Y, double[,] Z)
        {
            if ((X == null) || (Y == null) || (Z == null) || (X.Length != Z.GetLength(0)) || (Y.Length != Z.GetLength(1)) || (X.Length * Y.Length == 0))
                throw new Exception("Массивы данных для интерполяции должны быть не пустыми и число элементов в массивах Х и Y должны быть равны соответственно числу строк и столюцов матрицы Z");
            this.X = new double[X.Length];
            this.Y = new double[Y.Length];
            this.Z = new double[X.Length, Y.Length];
            for (int i = 0; i < X.Length; i++)
            {
                this.X[i] = X[i];
                for (int j = 0; j < Y.Length; j++)
                    this.Z[i, j] = Z[i, j];
            }
            for (int i = 0; i < Y.Length; i++)
                this.Y[i] = Y[i];
        }
    }
}
