using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypesLibrary;

namespace ModelLibrary
{
    public static class DI  // Класс, инкапсулирующий показатель направленности
    {
        static Interpolation _DI = new Interpolation(
            new double[28] { 18.4993, 22.246, 30, 36, 42, 48, 54, 60, 66, 72, 78, 84, 90, 96, 102, 108, 114, 120, 126, 132, 138, 144, 150, 156, 162, 168, 174, 180 },
            new double[28] { 2.142, 2.81, 3.135, 3.126, 2.862, 2.438, 1.85, 1.091, 0.153, -0.976, -2.309, -3.721, -4.903, -5.862, -6.614, -7.171, -7.539, -7.725, -7.788, -7.847, -7.906, -7.966, -8.07, -8.266, -8.554, -9.084, -10.033, -11.469 });
        public static double Interpolate(double Angle)
        {
            return _DI.Interpolate(Angle);
        }
    }
}