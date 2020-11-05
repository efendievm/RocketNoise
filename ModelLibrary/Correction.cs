using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelLibrary
{
    public static class Correction // Класс, инкапсулирующий коректрировку по шкале A шумометра
    {
        // Элементы массива поставлены в соотвествие с элементами массива NormalFrequencies (см. класс Frequencies)
        // Пользоваться только при расчёте шума в слышимом диапазоне
        static double[] _Correction = new double[8] { -26.2, -16.1, -8.6, -3.2, 0, 1.2, 1, -1.1 };
        public static double Get(int Index)
        {
            return _Correction[Index];
        }
    }
}