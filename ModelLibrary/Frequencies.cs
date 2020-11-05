using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypesLibrary;

namespace ModelLibrary
{
    public static class FrequenciesAggregator // Статический класс, инкапсулирующий ряд частот и ширины частотных полос
    {
        static double[] InfraFrequencies = new double[] { 0.8, 1, 1.25, 1.6, 2, 2.5, 3.2, 4, 5, 6.4, 8, 10, 12.8, 16 }; // Среднегеометрические частоты частотных полос инфразвукового спектра
        static double[] InfraFrequencyBands = new double[] { 0.05, 0.23, 0.3, 0.37, 0.46, 0.58, 0.74, 0.93, 1.16, 1.48, 1.85, 2.32, 2.96, 3.71 }; // Ширины частотных полос инфразвукового спектра (далее аналогично для ультразвука, слышимого звука и расширенного спектра при расчёте акустических характеристик двигателя)

        static double[] UltraFrequencies = new double[] { 12.5E3, 16.0E3, 20.0E3, 25.0E3, 31.5E3, 40E3, 50E3, 63E3, 80E3, 100E3 };
        static double[] UltraFrequencyBands = new double[] { 2.9E3, 3.8E3, 4.5E3, 5.7E3, 7.4E3, 9.32E3, 11.4E3, 14.9E3, 18E3, 23E3 };

        static double[] NormalFrequencies = new double[8] { 63, 125, 250, 500, 1000, 2000, 4000, 8000 };
        static double[] NormalFrequencyBands = new double[8] { 44, 90, 180, 355, 710, 1400, 2800, 5600 };

        static double[] ExpandedFrequencies = new double[40] { 6.875, 8.662, 10.913, 13.75, 17.324, 21.827, 27.5, 34.648, 43.654, 55, 69.296, 87.307, 110, 138.591, 174.614, 220, 277.183, 349.228, 440, 554.365, 698.456, 880, 1108.731, 1396.913, 1760, 2217.461, 2793.826, 3520, 4434.922, 5587.652, 7040, 8869.844, 11175.303, 14080, 17739.688, 22350.607, 28160, 35479.377, 44701.214, 56320 };
        static double[] ExpandedFrequencyBands = new double[40] { 1.592, 2.006, 2.527, 3.184, 4.012, 5.054, 6.368, 8.023, 10.109, 12.736, 16.046, 20.217, 25.472, 32.093, 40.434, 50.944, 64.185, 80.868, 101.888, 128.371, 161.737, 203.776, 256.741, 323.474, 407.551, 513.483, 646.948, 815.103, 1026.965, 1293.895, 1630.206, 2053.931, 2587.79, 3260.412, 4107.861, 5175.581, 6520.823, 8215.723, 10351.162, 13041.647 };
        public static void Infra(out double[] Frequencies, out double[] FrequencyBands)
        {
            Frequencies = InfraFrequencies;
            FrequencyBands = InfraFrequencyBands;       
        }
        public static void Ultra(out double[] Frequencies, out double[] FrequencyBands)
        {
            Frequencies = UltraFrequencies;
            FrequencyBands = UltraFrequencyBands;
        }
        public static void Normal(out double[] Frequencies, out double[] FrequencyBands)
        {
            Frequencies = NormalFrequencies;
            FrequencyBands = NormalFrequencyBands;
        }
        public static void Expanded(out double[] Frequencies, out double[] FrequencyBands)
        {
            Frequencies = ExpandedFrequencies;
            FrequencyBands = ExpandedFrequencyBands;
        }
    }
}
