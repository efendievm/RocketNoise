using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypesLibrary;

namespace ModelLibrary
{
    // Структура, содержащая информацию о параметрах атмосферы (температуре и давлению)
    public struct AtmosphereParameters
    {
        public double Temperature { get; private set; }
        public double Pressure { get; private set; }
        public AtmosphereParameters(double Temperature, double Pressure)
            : this()
        {
            this.Temperature = Temperature;
            this.Pressure = Pressure;
        }
    }
    // Класс, инкапсулирующий стандартную атмосферу
    public static class Atmosphere
    {

        public static AtmosphereParameters ParametersAtHeight(double Height, double GroundTemperature) // Функция, возвращающая давление и температуру воздуха на заданной высоте при заданной температуре на поверхности Земли (стандартная атмосфера)
        {
            double Temperature;
            double Pressure;
            double M = 28.9644E-3;
            if (Height <= 11E3)
            {
                Temperature = GroundTemperature - 6.5E-3 * Height;
                Pressure = 101325 * Math.Pow(GroundTemperature / Temperature, -9.8 * M / (8.31 * 6.5E-3));
            }
            else if (Height <= 20E3)
            {
                Temperature = 216.65;
                Pressure = 22632 * Math.Exp(-9.8 * M * (Height - 11E3) / (8.31 * Temperature));
            }
            else if (Height <= 32E3)
            {
                Temperature = 216.65 + 1E-3 * (Height - 20E3);
                Pressure = 5474.899 * Math.Pow(216.65 / Temperature, 9.8 * M / 8.31E-3);
            }
            else if (Height <= 47E3)
            {
                Temperature = 228.65 + 2.8E-3 * (Height - 32E3);
                Pressure = 868.0187 * Math.Pow(228.65 / Temperature, 9.8 * M / (8.31 * 2.8E-3));
            }
            else if (Height < 51E3)
            {
                Temperature = 110.9063;
                Pressure = 270.65 * Math.Exp(-9.8 * M * (Height - 47E3) / (8.31 * 110.9063));
            }
            else if (Height < 71E3)
            {
                Temperature = 270.65 - 2.8E-3 * (Height - 51E3);
                Pressure = 66.93887 * Math.Pow(270.65 / Temperature, -9.8 * M / (8.31 * 2.8E-3));
            }
            else
            {
                Temperature = 214.65 - 2E-3 * (Height - 71E3);
                Pressure = 3.956420 * Math.Pow(214.65 / Temperature, -9.8 * M / (8.31 * 2E-3));
            }
            return new AtmosphereParameters(Temperature, Pressure);
        }
        public static double AbsorptionRatio(double Humidity, AtmosphereParameters atmosphereParameters, double Frequency) // Функция, возвращающая коэффициент звукопоглощения атмосферы
        {
            // Humidity -- влажность воздуха на какой-либо высоте, %
            // atmosphereParameters -- параметры атмосферы на какой-либо высоте
            // Frequency -- частота
            Frequency = Frequency < 10E3 ? Frequency : 10E3; // Ограничение по частоте
            double Temperature = atmosphereParameters.Temperature;
            double RelativePressure = atmosphereParameters.Pressure / 101325; // Относительное давление
            double WaterConcentration = Humidity * Math.Pow(10, -6.8346 * Math.Pow(273.16 / Temperature, 1.261) + 4.6151) / RelativePressure; // Концентрация водяных паров
            double f_rO = RelativePressure * (24 + 4.04E4 * WaterConcentration * (0.02 + WaterConcentration) / (0.391 + WaterConcentration)); // Релаксационная частота кислорода
            double f_rN = RelativePressure * Math.Sqrt(293.15 / Temperature) * (9 + 280 * WaterConcentration * Math.Exp(-4.71 * (Math.Pow(293.15 / Temperature, 1.0 / 3) - 1))); // Релаксационная частота азота
            return 8.686 * Math.Pow(Frequency, 2) * (1.84E-11 / RelativePressure * Math.Sqrt(Temperature / 293.15) + Math.Pow(293.15 / Temperature, 5.0 / 2) *
                (0.01275 * Math.Exp(-2239.1 / Temperature) / (f_rO + Math.Pow(Frequency, 2) / f_rO) +
                 0.10680 * Math.Exp(-3352.0 / Temperature) / (f_rN + Math.Pow(Frequency, 2) / f_rN)));
        }
        public static double[] Propagation(double[] Frequencies, WeatherParameters weatherParameters, double Height, double DistanceToGround) //Функция, возвращающая значения затуханий звука вследствие звукополгощения для заданных частот
        {
            // Frequencies -- массив частот
            // Height -- расстояние источника шума от Земли по вертикали
            // DistanceToGround -- расстоянии от источника шума до наблюдателя на Земле
            int N = 20; // Число разбиений отрезка, соединяющего наблюдателя и источник шума
            AtmosphereParameters[] atmosphereParameters = new AtmosphereParameters[N]; // Параметры атмосферы на участках отрезка, соединяющего наблюдателя и источник шума
            double[] Humidity = new double[N]; // Массив относительных влажностей вохдуха на участках отрезка, соединяющего наблюдателя и источник шума
            for (int i = 0; i < N; i++) // Заполнение массивов atmosphereParameters и Humidity
            {
                atmosphereParameters[i] = ParametersAtHeight(i * Height / N, weatherParameters.Temperature);
                Humidity[i] = weatherParameters.Humidity * Math.Pow(10,
                    -0.0387 * (weatherParameters.Temperature - atmosphereParameters[i].Temperature) -
                    6.8346 * (Math.Pow(273.16 / weatherParameters.Temperature, 1.261) -
                              Math.Pow(273.16 / atmosphereParameters[i].Temperature, 1.261)));
            }
            double[] atmospherePropagation = new double[Frequencies.Length]; // Массив значений затухания звука
            for (int j = 0; j < Frequencies.Length; j++) // Заполнение массива atmospherePropagation
            {
                double currentPropogation = 0;
                for (int i = 0; i < N; i++) // Суммирование затуханий звука при частоте Frequencies[j] на участках отрезка, соединяющего наблюдателя и источник шума 
                    currentPropogation += AbsorptionRatio(Humidity[i], atmosphereParameters[i], Frequencies[j]) * DistanceToGround / N;
                atmospherePropagation[j] = currentPropogation;
            }
            return atmospherePropagation;
        }
    }
}
