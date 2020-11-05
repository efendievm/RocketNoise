using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypesLibrary;

namespace ModelLibrary
{
    public static class SpectralDecomposition // Статический класс для разложения шума на спектр
    {
        static Interpolation NRSP = new Interpolation // Нормализация звуковой мощности участка газовой струи по спектру частот
        (
            new double[13] { 0.05, 0.1, 0.2, 0.5, 1, 2, 5, 10, 20, 35, 50, 70, 100 },
            new double[13] { -20, -16.8, -13.7, -9.6, -7.2, -6.9, -13.4, -20.3, -27, -32.5, -36.2, -39.6, -43.1 }
        );
        static Interpolation NRSPL = new Interpolation // Нормализация звуковой мощности по длине газовой струи
        (
            new double[9] { 0.1, 0.2, 0.5, 1, 1.5, 2, 3, 4, 5 },
            new double[9] { -18.7, -14.7, -9.2, -5.1, -3, -3.5, -9.5, -15.5, -21.6 }
        );
        public static double[] Get // Функция, возвращающая разложение шума на спектр по частотам
        (
            double Pressure,  // Давлению на какой-либо высоте
            FlowParameters flowParameters,  // Параметры газовой струи
            double[] Frequencies, // Среднегеометрические частоты
            double[] FrequencyBands // Ширины частотных полос
        )
        {
            double CoreLength = 1.75 * 2 * 1 / flowParameters.NozzleMachNumber * Math.Sqrt(flowParameters.MassFlow * flowParameters.NozzleFlowVelocity /
                (Math.PI * Pressure * flowParameters.NozzleAdiabaticIndex)) * Math.Pow(1 + 0.38 * flowParameters.NozzleMachNumber, 2); // Длина невозмущённого сверхзвукового потока
            double JetLength = 5 * CoreLength; // Расстояние от выходного сечения сопла до конца участка газовой струи, излучаю-щего ~98% звуковой мощности
            int StepCount = 15; // Количество участков разбиения газовой струи
            double Step = JetLength / StepCount; // Шаг разбиения газовой струи
            double[] Sh = new double[StepCount]; // Массив чисел Струхаля участков газовой струи
            double[] x = new double[StepCount]; // Массив координат участков газовой струи
            double[] NSP = new double[Frequencies.Length]; // Частотный спектр шума
            double NozzleSoundVelocity = flowParameters.NozzleFlowVelocity / flowParameters.NozzleMachNumber; // Скорость звука на срезе сопла
            for (int j = 1; j <= StepCount; j++) // Заполнение массивов координат и чисел Струхаля участков газовой струи
            {
                x[j - 1] = j * Step;
                Sh[j - 1] = x[j - 1] * NozzleSoundVelocity / (flowParameters.NozzleFlowVelocity * flowParameters.ChamberSoundVelocity);
            }
            for (int i = 0; i < Frequencies.Length; i++) // Определение частотного спектра шума
            {
                double CurrentNSP = 0; // Переменная-заготовка для элемента частотного спектра, соответсвующего частоте Frequencies[i]
                for (int j = 0; j < StepCount; j++) // Логарифмическое суммирование уровней шума от участков газовой струи на частоте Frequencies[i]
                    CurrentNSP += Math.Pow(10, 0.1 * (NRSP.Interpolate(Frequencies[i] * Sh[j]) + 10 * Math.Log10(FrequencyBands[i] * Sh[j]) +
                        NRSPL.Interpolate(x[j] / CoreLength) + 10 * Math.Log10(Step / CoreLength)));
                NSP[i] = 10 * Math.Log10(CurrentNSP); // Определение элемента частотного спектра, соответсвующего частоте Frequencies[i]
            }
            return NSP;
        }
    }
}
