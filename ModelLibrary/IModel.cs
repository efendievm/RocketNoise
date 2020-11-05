using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypesLibrary;
using System.Drawing;
using System.Xml;

namespace ModelLibrary
{
    public delegate double FlightSoundLevel(double Time, double Radius, double Angle, WeatherParameters weatherParameters); // Ссылка на функцию, определяющую уровень шума от СЧ МСРКН в момент времени Time на расстоянии Radius от точки старта МСРКН под углом Angle к напрвлению полёта МСРКН при погодных условиях weatherParameters
    public delegate double StaticSoundLevel(double Radius, double Angle, WeatherParameters weatherParameters); // ССылка на функцию, определяющую уровень шума от двигателея на расстоянии Radius под углом Angle к оси газовой струи при погодных условиях weatherParameters
    public delegate double EngineSoundLevel(double Radius, double Angle);
    public delegate Dictionary<double, double> EngineSoundLevelAtFrequency(double Radius, double Angle);
    public struct FlightSoundCalculationInputData // Структура, содержащая основные исходные данные для расчёта уровня шума в полёте
    {
        public RocketBallistics RocketBallistics { get; private set; } // Параметры траектроии МСРН
        public VehicleBallistics VehicleBallistics { get; private set; } // Параметры траектроии МСКА
        public FlowParameters RocketFlowParameters { get; private set; } // Характеристики газовой струи МСРН
        public FlowParameters VehicleFlowParameters { get; private set; } // Характеристики газовой струи МСКА
        public List<WeatherParameters> WeatherParameters { get; private set; } // Погодные условия
        public FlightSoundCalculationInputData(
            RocketBallistics RocketBallistics,
            VehicleBallistics VehicleBallistics,
            FlowParameters RocketFlowParameters,
            FlowParameters VehicleFlowParameters,
            List<WeatherParameters> WeatherParameters) : this()
        {
            this.RocketBallistics = RocketBallistics;
            this.VehicleBallistics = VehicleBallistics;
            this.RocketFlowParameters = RocketFlowParameters;
            this.VehicleFlowParameters = VehicleFlowParameters;
            this.WeatherParameters = WeatherParameters;
        }
    }
    public struct FireTestNoiseCalculationInputData // Структура, содержащая основные исходные данные для расчёта уровня шума при огневых испытаниях
    {
        public double Thrust { get; private set; } // Тяга двигателя
        public FlowParameters FlowParameters { get; private set; } // Параметры газовой струи
        public List<WeatherParameters> WeatherParameters { get; private set; } // Погодные условия
        public RadiusInterval RadiusInterval { get; private set; } // Область расчёта
        public FrequencyBand FrequencyBand { get; private set; } // Спектр частот
        public FireTestNoiseCalculationInputData(
            double Thrust,
            FlowParameters FlowParameters,
            List<WeatherParameters> WeatherParameters,
            RadiusInterval RadiusInterval,
            FrequencyBand FrequencyBand) : this()
        {
            this.Thrust = Thrust;
            this.FlowParameters = FlowParameters;
            this.WeatherParameters = WeatherParameters;
            this.RadiusInterval = RadiusInterval;
            this.FrequencyBand = FrequencyBand;
        }
    }
    public struct EngineAcousticsLoadSummary // Сруктура, содержащая результаты расчёта акустических характристик двигателя
    {
        public FlowSoundParameters FlowSoundParameters { get; private set; } // Акустические параметры газовой струи
        public Dictionary<double, double> FrequencyCharacteristik { get; private set; } // Частотная характеристика двигателя (ключи словаря -- частоты, значения -- уровни шума дБ на расстоянии 1 м под углом 90 град.
        public Dictionary<double, double> RadiationPattern { get; private set; } // Диаграмма направленности двигателя (ключи словаря -- углы, значения -- уровни шума дБ на расстоянии 1 м
        public Dictionary<double, double> EngineAcousticsLoadAtFrequency { get; private set; } // Акустические нагрузки на двигатель(ключи словаря -- частоты, значения -- уровни шума дБ на срезе сопла
        public double SummaryLoad { get; private set; } // Суммарная акустическая нагрузка на двигатель
        public EngineAcousticsLoadSummary(
            FlowSoundParameters FlowSoundParameters,
            Dictionary<double, double> FrequencyCharacteristik,
            Dictionary<double, double> RadiationPattern,
            Dictionary<double, double> EngineAcousticsLoadAtFrequency,
            double SummaryLoad) : this()
        {
            this.FlowSoundParameters = FlowSoundParameters;
            this.FrequencyCharacteristik = FrequencyCharacteristik;
            this.RadiationPattern = RadiationPattern;
            this.EngineAcousticsLoadAtFrequency = EngineAcousticsLoadAtFrequency;
            this.SummaryLoad = SummaryLoad;
        }
    }
    public struct EngineSoundContourParameters // Параметры изображения распределения уровня шума
    {
        public double ContourAreaWidth { get; private set; } // Ширина изображения
        public double ContourAreaHeight { get; private set; } // Высота изображения
        public double NozzleCoordinate { get; private set; } // Координата выходного сечения
        public double MinSoundLevel { get; private set; } // Минимальный отображаемый уровень шума
        public double MaxSoundLevel { get; private set; } // Максимальный отображаемый уровень шума
        public EngineSoundContourParameters(
            double ContourAreaWidth,
            double ContourAreaHeight,
            double NozzleCoordinate,
            double MinSoundLevel,
            double MaxSoundLevel) : this()
        {
            this.ContourAreaWidth = ContourAreaWidth;
            this.ContourAreaHeight = ContourAreaHeight;
            this.NozzleCoordinate = NozzleCoordinate;
            this.MinSoundLevel = MinSoundLevel;
            this.MaxSoundLevel = MaxSoundLevel;
        }
    }
    public struct SonicBoomCalculationInputData // Структура, содержащая основные исходные данные для расчёта звукового удара
    {
        public SonicBoomBallistics Ballistics { get; private set; } // Баллистика СЧ МСРКН (МСРН или МСКА)
        public GeometricalParameters GeometricalParameters { get; private set; } // Геометрические параметры СЧ МСРКН (МСРН или МСКА)
        public SonicBoomCalculationInputData(SonicBoomBallistics Ballistics, GeometricalParameters GeometricalParameters) : this()
        {
            this.Ballistics = Ballistics;
            this.GeometricalParameters = GeometricalParameters;
        }
    }
    public interface IModel
    {
        FlightSoundLevel GetFlightSoundLevel // Функция, возвращающая функцию вида FlightSoundLevel
        (  
            Ballistics ballistics, // Баллистика
            FlowParameters flowParameters, // Параметры газовой струи
            FrequencyBand frequencyBand // Частотный спектр
        );
        StaticSoundLevel GetStaticSoundLevel // Функция, возвращающая функцию вида StaticSoundLevel
        (
            double Thrust, // Тяга двигателя
            FlowParameters flowParameters, // Параметры газовой струи
            FrequencyBand frequencyBand // Спектр частот
        );
        EngineSoundLevel GetEngineSoundLevel(double Thrust, FlowParameters FlowParameters); // Функция, возвращающая фукнцию вида EngineSoundLevel при исходных данных по тяге Thrust и параметрам газовой струи FlowParameters
        EngineSoundLevelAtFrequency GetEngineSoundLevelAtFrequency(double Thrust, FlowParameters FlowParameters); // Функция, возвращающая фукнцию вида EngineSoundLevelAtFrequency при исходных данных по тяге Thrust и параметрам газовой струи FlowParameters
        List<EffectiveFlightSound> CalculateFlightSound(FlightSoundCalculationInputData id); // Функция, рассчитывающая эквивалентные уровни шума
        List<FlightSoundCircles> CalculateFlightSound // Функция, определяющая геометрическое место точек, в которых уровни шума равны SoundLevels
        (
            FlightSoundCalculationInputData id,  // Основные исходные данные
            List<double> SoundLevels, // Уровни шума
            RadiusInterval RadiusInterval, // Область расчёта
            FrequencyBand FrequencyBand // Частотный спектр
        );
        void CalculateFlightSound // Метод, определяющий геометрическое место точек, в которых уровни шума в слышиомом диапазоне равны SoundLevel, и рассчитывающая эквивалентные уровни шума
        (
            FlightSoundCalculationInputData id, // Основные исходные данные
            List<double> SoundLevels, // Уровни шума
            RadiusInterval RadiusInterval, // Область расчёта геометрическое место точек, в которых уровни шума в слышиомом диапазоне равны SoundLevel
            out List<EffectiveFlightSound> EffectiveFlightSounds, // Переменаая, в которую записывается результат расчёта эквивалентных уровней шума
            out List<FlightSoundCircles> FlightSoundCircles // Переменная, в которую записывется результат расчёта геометрического места точек, в которых уровни шума в слышиомом диапазоне равны SoundLevel
        );
        List<FireTestSoundContour> CalculateFireTestNoise
        (
            FireTestNoiseCalculationInputData id,
            List<double> SoundLevels
        );
        FlowSoundParameters GetFlowSoundParameters(double Thrust, FlowParameters flowParameters);
        void CalculateEngineAcousticsLoadSummary
        (
            double Thrust,
            FlowParameters FlowParameters,
            out EngineAcousticsLoadSummary summary
        );
        void CalculateEngineSoundContour
        (
            double Thrust,
            FlowParameters FlowParameters,
            EngineSoundContourParameters ContourParameters,
            out EngineSoundContour EngineSoundContour
        );
        void ModifyEngineSoundContour
        (
            double MinSoundLevel,
            double MaxSoundLevel,
            ref EngineSoundContour EngineSoundContour
        );
        List<SonicBoomParameters> CalculateSonicBoom
        (
            SonicBoomCalculationInputData id,
            List<WeatherParameters> WeatherParameters
        );
        void CalculateSonicBoom
        (
            SonicBoomCalculationInputData RocketID,
            SonicBoomCalculationInputData VehicleID,
            List<WeatherParameters> WeatherParameters,
            out List<SonicBoomParameters> RocketSonicBoomBoomParameters,
            out List<SonicBoomParameters> VehicleSonicBoomBoomParameters
        );
        void SaveFlightNoiseInputData // Метод, сохраняющий исходные данные расчёта уровня шума при полёте МСРКН и ей СЧ
        (
            string FileName, // Имя файла
            FlightSoundCalculationInputData id, // Основыне исходные данные
            Dictionary<double, Color> SoundLevels, // Словарь, ключи которого -- уровни шума в слышимом диапазоне, значения -- цвет отображения геометрического места точек
            RadiusInterval RadiusInterval, // Область расчёта геометрическое место точек, в которых уровни шума в слышиомом диапазоне равны ключам словаря SoundLevels
            FrequencyBand FrequencyBand // Спектр частот
        );
        void OpenFlightNoiseInputData // Метод, пределяющий исходные данные для расчёта уровня шума в полёте из файла
        (
            string FileName, // Имя файла
            out FlightSoundCalculationInputData id, // Переменная, в которую записываются основные исходные данные
            out Dictionary<double, Color> SoundLevels, // Переменная, в которую записывается словарь, ключи которого -- уровни шума в слышимом диапазоне, значения -- цвет отображения геометрического места точек
            out RadiusInterval RadiusInterval, // Область расчёта геометрическое место точек, в которых уровни шума в слышиомом диапазоне равны ключам словаря SoundLevels
            out FrequencyBand FrequencyBand // Спектр частот
        );
        void OpenFlightNoiseInputData // Метод, пределяющий исходные данные для расчёта уровня шума в полёте из файла
        (
            XmlDocument xmlDoc, // Файл .xml и исходными данными
            out FlightSoundCalculationInputData id, // Переменная, в которую записываются основные исходные данные
            out Dictionary<double, Color> SoundLevels, // Переменная, в которую записывается словарь, ключи которого -- уровни шума в слышимом диапазоне, значения -- цвет отображения геометрического места точек
            out RadiusInterval RadiusInterval, // Область расчёта геометрическое место точек, в которых уровни шума в слышиомом диапазоне равны ключам словаря SoundLevels
            out FrequencyBand FrequencyBand // Спектр частот
        );
        void SaveFireTestNoiseInputData
        (
            string FileName,
            FireTestNoiseCalculationInputData id,
            Dictionary<double, Color> SoundLevels
        );
        void OpenFireTestNoiseInputData
        (
            string FileName,
            out FireTestNoiseCalculationInputData id,
            out Dictionary<double, Color> SoundLevels
        );
        void OpenFireTestNoiseInputData
        (
            XmlDocument xmlDoc,
            out FireTestNoiseCalculationInputData id,
            out Dictionary<double, Color> SoundLevels
        );
        void SaveEngineNoiseInputData
        (
            string FileName,
            double Thrust,
            FlowParameters FlowParameters,
            EngineSoundContourParameters ContourParameters
        );
        void OpenEngineNoiseInputData
        (
            string FileName,
            out double Thrust,
            out FlowParameters FlowParameters,
            out EngineSoundContourParameters ContourParameters
        );
        void OpenEngineNoiseInputData
        (
            XmlDocument xmlDoc,
            out double Thrust,
            out FlowParameters FlowParameters,
            out EngineSoundContourParameters ContourParameters
        );
        void SaveSonicBoomInputData
        (
            string FileName,
            SonicBoomCalculationInputData RocketID,
            SonicBoomCalculationInputData VehicleID,
            List<WeatherParameters> WeatherParameters
        );
        void OpenSonicBoomInputData
        (
            string FileName,
            out SonicBoomCalculationInputData RocketID,
            out SonicBoomCalculationInputData VehicleIDs,
            out List<WeatherParameters> WeatherParameters
        );
        void OpenSonicBoomInputData
        (
            XmlDocument xmlDoc,
            out SonicBoomCalculationInputData RocketID,
            out SonicBoomCalculationInputData VehicleIDs,
            out List<WeatherParameters> WeatherParameters
        );
        event Action<int> FlightNoiseCalculationProgressChanged; // Событие для сообщения о ходе выполнения расчёта уровня шума в полёте
        event Action<int> FireTestNoiseCalculationProgressChanged; // Событие
        event Action<int> EngineNoiseCalculationProgressChanged;
        event Action<int> SonicBoomCalculationProgressChanged;
    }
}
