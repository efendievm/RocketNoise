using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypesLibrary;
using System.Xml;
using System.Drawing;
using System.Threading;
using Excel = Microsoft.Office.Interop.Excel;

namespace ModelLibrary
{
    class FlightNoiseModel
    {
        public FlightSoundLevel GetFlightSoundLevel(Ballistics ballistics, FlowParameters flowParameters, FrequencyBand frequencyBand) // Функция, возвращающая функцию типа FlightSoundLevel (см. файл IModel.cs)
        {
            // Определение среднегеометрических частот и ширин частотных полос по заданному спектру frequencyBand
            double[] Frequencies, FrequencyBands;
            if (frequencyBand == FrequencyBand.Infra)
                FrequenciesAggregator.Infra(out Frequencies, out FrequencyBands);
            else if (frequencyBand == FrequencyBand.Ultra)
                FrequenciesAggregator.Ultra(out Frequencies, out FrequencyBands);
            else
                FrequenciesAggregator.Normal(out Frequencies, out FrequencyBands);
            // Определение функции, возвращающей уровень шума от СЧ МСРКН в момент времени Time на расстоянии Radius от точки старта МСРКН под углом Angle к напрвлению полёта МСРКН при погодных условиях weatherParameters
            FlightSoundLevel flightSoundLevel = (Time, Radius, Angle, weatherParameters) =>
            {
                double F = ballistics.Thrust.Interpolate(Time); // Тяга двигателей в момент времени Time
                if (F == 0) return 0;
                double h = ballistics.Height.Interpolate(Time) - ballistics.Height.Interpolate(0); // Высота полёта СЧ МСРКН в момент времени Time
                double D = ballistics.Distance.Interpolate(Time); // Дальность полёта СЧ МСРКН в момен времени Time
                double DistanceToGround = Math.Pow(h * h + D * D - 2 * D * Radius * Math.Cos(Angle) + Radius * Radius, 0.5); // Расстояние от СЧ МСРКН до наблюдателя, находящегося на расстоянии Radius от точки старта под углм Angle к направлению полёта МСРКН
                double[] spectralDecomposition = SpectralDecomposition.Get(Atmosphere.ParametersAtHeight(h, weatherParameters.Temperature).Pressure, flowParameters, Frequencies, FrequencyBands); // Разложение шума на спектр по частотам
                double[] atmospherePropagation = Atmosphere.Propagation(Frequencies, weatherParameters, h, DistanceToGround); // Затухание шума вследствие звукопоглощения атмосферой на частотах Frequencies
                double L0 = 10 * Math.Log10(0.5 * 0.0022 * F * flowParameters.NozzleFlowVelocity) + 120 - 11 - 20 * Math.Log10(DistanceToGround) + DI.Interpolate(Math.Acos(h / DistanceToGround) * 180 / Math.PI); // Уровень шума в дБ без учёта затухания вследствие звукопоглощения атмосферой
                double L = 0; // Возвращаемое значение
                for (int i = 0; i < Frequencies.Length; i++) // Учёт затухания вследствие звукопоглощения атмосферой и корректрировки по шкале A в случае расчёта в слышимом диапазоне
                {
                    double CurrentL = L0 + spectralDecomposition[i] - atmospherePropagation[i] + (frequencyBand == FrequencyBand.Normal ? Correction.Get(i) : 0);
                    if (CurrentL > 0)
                        L += Math.Pow(10, 0.1 * CurrentL);
                }
                if (L > 0) return 10 * Math.Log10(L);
                else return 0;
            };
            return flightSoundLevel;
        }
        List<EffectiveFlightSound> Calculate(FlightSoundCalculationInputData id, Action<int> ProgressChanged) // Функция, возвращающая эквивалентные уровни шума
        {
            // ProgressChanged --  внешняя функция, вызываеммая при изменении хода расчёта
            var EndTime = Math.Max(id.RocketBallistics.Distance.X.Last(), id.VehicleBallistics.Distance.X.Last()); // Конечное время расчёта как наибольшее из времён полёта МСРН и МСКА
            double LandingRadius = ((VehicleBallistics)id.VehicleBallistics).LandingRadius; // Радиус зоны посадки МСКА
            double LandingStartTime = ((VehicleBallistics)id.VehicleBallistics).LandingStartTime; // Условное время начала посадки МСКА
            var RocketSoundLevel = GetFlightSoundLevel(id.RocketBallistics, id.RocketFlowParameters, FrequencyBand.Normal); // Переменная-функуция, определяющая уровень шума от МСРН
            var VehicleSoundLevel = GetFlightSoundLevel(id.VehicleBallistics, id.VehicleFlowParameters, FrequencyBand.Normal); // Переменная-функуция, определяющая уровень шума от МСКА
            Func<double, double, double> TotalSoundLevel = (rocketSoundLevel, vehicleSoundLevel) => // Функция, определяющая суммарный уровень шума от ракеты rocketSoundLevel и капуслы vehicleSoundLevel
            {
                if (rocketSoundLevel + vehicleSoundLevel != 0)
                    return 10 * Math.Log10(
                        Math.Sign(rocketSoundLevel) * Math.Pow(10, 0.1 * rocketSoundLevel) +
                        Math.Sign(vehicleSoundLevel) * Math.Pow(10, 0.1 * vehicleSoundLevel));
                else
                    return 0;
            };
            Func<double, double, double, double> GetMimimumDistance = (L, d, h) =>
            // Функция, возвращающая фиктивное положение наблюдателя, при котором расстояние между МСКА и действительным положением наблюдателя достигает минимальное значание при некторой траектории посадки МСКА, ограниченной границами зоны посадки
            {
                // L -- номинальная дальность полёта МСКА
                // h -- высота полёта МСКА
                // d -- действительный радиус-вектор наблюдателя на Земле
                double x = 0; // Смещение положения МСКА от номинальной тракетрии полёта в границах области зоны посадки
                if (Math.Abs(d - L) < LandingRadius) // Если наблюдатель находится в зоне посадки МСКА
                    x = d - L;
                else
                {
                    double S1 = Math.Sqrt(Math.Pow(h, 2) + Math.Pow(L + LandingRadius - d, 2)); // Расстояние от МСКА до наблюдателя, когда реальное положении МСКА в крайней правой точке зоны посадки МСКА по направлению полёта МСРКН
                    double S2 = Math.Sqrt(Math.Pow(h, 2) + Math.Pow(L - LandingRadius - d, 2)); // Расстояние от МСКА до наблюдателя, когда реальное положении МСКА в крайней левой  точке зоны посадки МСКА по направлению полёта МСРКН
                    if (S1 < S2)
                        x = LandingRadius;
                    else
                        x = -LandingRadius;
                }
                return d - x;
            };
            // Функция, возвращающая суммарный уровень шума от ракеты и капуслы в момент времени Time при погодных условиях weatherParameters на расстоянии distance от точки старта МСРКН в направлении полёта и в направлении, противоположенном направлению полёта МСРКН
            Func<double, double, WeatherParameters, double[]> GetCurrentSoundLevels = (Time, distance, weatherParameters) =>
            {
                double rocketRightSoundLevel = RocketSoundLevel(Time, distance, 0, weatherParameters); // Уровень шума от МСРН в направлении полёта МСРКН
                double rocketLeftSoundLevel = RocketSoundLevel(Time, distance, Math.PI, weatherParameters); // Уровень шума от МСРН в направлении, противоположенном направлению полёта МСРКН
                double vehicleRightSoundLevel = 0; // Уровень шума от МСКА в направлении полёта МСРКН
                double vehicleLeftSoundLevel = 0; // Уровень шума от МСКА в направлении, противоположенном направлению полёта МСРКН
                if (Time < LandingStartTime) // Если МСКА не на этапе спуска
                {
                    vehicleRightSoundLevel = VehicleSoundLevel(Time, distance, 0, weatherParameters);
                    vehicleLeftSoundLevel = VehicleSoundLevel(Time, distance, Math.PI, weatherParameters);
                }
                else
                {
                    double L = id.VehicleBallistics.Distance.Interpolate(Time); // номинальная дальность полёта МСКА в момент времени Time
                    double h = id.VehicleBallistics.Height.Interpolate(Time); // высота полёта МСКА в момент времени Time
                    var MinimumRightDistance = GetMimimumDistance(L, distance, h); // фиктивное положение наблюдателя, при котором расстояние между МСКА и действительным положением наблюдателя на расстоянии distance по направлению полёта МСКА минимально
                    var MinimumLeftDistance = GetMimimumDistance(L, -distance, h); // фиктивное положение наблюдателя, при котором расстояние между МСКА и действительным положением наблюдателя на расстоянии distance по направлению, противополеженном направлению полёта МСКА, минимально
                    vehicleRightSoundLevel = VehicleSoundLevel(Time, Math.Abs(MinimumRightDistance), (MinimumRightDistance > 0) ? 0 : Math.PI, weatherParameters);
                    vehicleLeftSoundLevel = VehicleSoundLevel(Time, Math.Abs(MinimumLeftDistance), (MinimumLeftDistance > 0) ? 0 : Math.PI, weatherParameters);
                }
                return new double[] 
                { 
                    TotalSoundLevel(rocketRightSoundLevel, vehicleRightSoundLevel),
                    TotalSoundLevel(rocketLeftSoundLevel, vehicleLeftSoundLevel) 
                };
            };
            double TimeStep = 1; // шаг по времени
            var Distances = new List<double>() { 100 }; // Список расстояний от точки старта МСРКН, на которых определяется уровень шума в полёте
            while (Distances.Last() <= 12000) // Заполенение Distances
                Distances.Add(Distances.Last() + 100);
            var _RightSoundLevels = new Dictionary<double, double>(); // словарь, ключи которого -- расстояния из списка Distances, значения -- уровни шума на расстояних из списка Distances по направлению полёта МСРКН
            var _LeftSoundLevels = new Dictionary<double, double>(); // словарь, ключи которого -- расстояния из списка Distances, значения -- уровни шума на расстояних из списка Distances по направлению, противоположенном направлению полёта МСРКН
            var _WorstRightMounth = new Dictionary<double, string>(); // словарь, ключи которого -- расстояния из списка Distances, значения -- названия месяцов, в которых уровни шума на расстояних из списка Distances по направлению полёта МСРКН максимальны
            var _WorstLeftMounth = new Dictionary<double, string>();  // словарь, ключи которого -- расстояния из списка Distances, значения -- названия месяцов, в которых уровни шума на расстояних из списка Distances по направлению, противоположенном направлению полёта МСРКН, максимальны
            int n = 0; // число расчётов уровня шума на расстояниях из списка Distances
            object Locker = new object(); // объект, блокирующий списки RightSoundLevels, LeftSoundLevels, WorstRightMounth, WorstLeftMounth и переменную n на запись
            Parallel.ForEach(Distances, distance => // параллельный расчёт урровней шума на расстояниях distance из списка Distances
            {
                double rightSoundLevel = 0; // максимальный уровень шума на расстоянии distance по направлению полёта МСРКН
                double leftSoundLevel = 0; // максимальный уровень шума на расстоянии distance по направлению, противоположенном направлению полёта МСРКН
                var worstRightMounth = id.WeatherParameters[0].Mounth; // погодные условия, при которых rightSoundLevel максимальный
                var worstLeftMounth = id.WeatherParameters[0].Mounth; // погодные условия, при которых leftSoundLevel максимальный
                foreach (var weatherParameters in id.WeatherParameters) // расчёт проводится для всех погодных условий weatherParameters из списка входных погодных условий для выявления макисмальных rightSoundLevel и leftSoundLevel
                {
                    double Time = 0;
                    while (Time <= EndTime) // расчёт проводится для каждого момента времени полёта для выявления макисмальных rightSoundLevel и leftSoundLevel
                    {
                        var CurrentSoundLevels = GetCurrentSoundLevels(Time, distance, weatherParameters); // массив из уровней шума на расстоянии distance по направлению и против направления полёта МСРКН в текущий момент времени time при текущих погодных условиях weatherParameters
                        var _rightSoundLevel = CurrentSoundLevels[0]; // текущий уровень шума на расстоянии distance по направлению полёта МСРКН
                        var _leftSoundLevel = CurrentSoundLevels[1]; // текущий уровень шума на расстоянии distance по направлению, противоположенном направлению полёта МСРКН
                        if (_rightSoundLevel > rightSoundLevel) // обновляются значения переменных rightSoundLevel и worstRightMounth, если текущий уровень шума на расстоянии distance по направлению полёта МСРКН больше текущего максимального
                        {
                            rightSoundLevel = _rightSoundLevel;
                            worstRightMounth = weatherParameters.Mounth;
                        }
                        if (_leftSoundLevel > leftSoundLevel) // обновляются значения переменных leftSoundLevel и worstLeftMounth, если текущий уровень шума на расстоянии distance по направлению, противоположенном направлению полёта МСРКН, больше текущего максимального
                        {
                            leftSoundLevel = _leftSoundLevel;
                            worstLeftMounth = weatherParameters.Mounth;
                        }
                        Time += TimeStep; // переход к следующему моменту времени полёта
                    }
                }
                lock (Locker) // блокируются на запись списки RightSoundLevels, LeftSoundLevels, WorstRightMounth, WorstLeftMounth и переменная n
                {
                    _RightSoundLevels.Add(distance, rightSoundLevel);
                    _WorstRightMounth.Add(distance, worstRightMounth);
                    _LeftSoundLevels.Add(distance, leftSoundLevel);
                    _WorstLeftMounth.Add(distance, worstLeftMounth);
                    n++;
                    ProgressChanged((int)(100.0 * n / Distances.Count)); // вызывается внешняя функция с аргументом, равным текущему прогрессу расчёта
                }
            });
            var RightSoundLevels = _RightSoundLevels.OrderBy(x => x.Key).Select(x => x.Value).ToList(); // список уровней шума на расстояниях из списка Distances по направлению полёта МСРКН
            var LeftSoundLevels = _LeftSoundLevels.OrderBy(x => x.Key).Select(x => x.Value).ToList(); // список уровней шума на расстояниях из списка Distances по направлению, противоположенном напралению полёта МСРКН
            var WorstRightMounth = _WorstRightMounth.OrderBy(x => x.Key).Select(x => x.Value).ToList(); // список названий месяцев, при которых уровни шума на расстояниях из списка Distances по направлению полёта МСРКН максимальны
            var WorstLeftMounth = _WorstLeftMounth.OrderBy(x => x.Key).Select(x => x.Value).ToList(); // список названий месяцев, при которых уровни шума на расстояниях из списка Distances по направлению, противоположенном направлению полёта МСРКН, максимальны
            var BaseSoundLevels = new double[3] { 85, 75, 70 }; // Характерные уровни шума
            Action<double, double[]> IncrementTimes = (SoundLevel, EffectiveTimes) => // процедура, увеличивающая элементы массива времён EffectiveTimes в случае, если уровень шума SoundLevel превышает характерные значения
            {
                for (int j = 0; j < BaseSoundLevels.Length; j++)
                {
                    if (SoundLevel > BaseSoundLevels[j])
                    {
                        for (int k = j; k < BaseSoundLevels.Length; k++)
                            EffectiveTimes[k] += TimeStep;
                        break;
                    }
                }
            };
            var Result = new List<EffectiveFlightSound>(); // переменаая, в которую записывается результат расчёта эквивалентных уровней шума
            for (int i = 0; i < Distances.Count; i++)
            {
                var distance = Distances[i]; // текущее расстояние из списка Distances
                var worstRightMounth = id.WeatherParameters.First(x => x.Mounth == WorstRightMounth[i]); // название месяца, в котором уровень шума на расстоянии distance по направлению полёта МСРКН максимален
                var worstLeftMounth = id.WeatherParameters.First(x => x.Mounth == WorstLeftMounth[i]); // название месяца, в котором уровень шума на расстоянии distance по направлению, противоположенном направлению полёта МСРКН, максимален
                var RightTimes = new double[BaseSoundLevels.Length]; // массив времён, в течении которых уровень шума на расстоянии distance по направлению полёта МСРКН превышает характерные значения
                var LeftTimes = new double[BaseSoundLevels.Length]; // массив времён, в течении которых уровень шума на расстоянии distance по направлению, противоположенном направлению полёта МСРКН, превышает характерные значения
                double RightEffectiveSoundLevel = 0; // переменная заготовка для эквивалентного уровеня шума на расстоянии distance по направлению полёта МСРКН
                double LeftEffectiveSoundLevel = 0; // переменная заготовка для эквивалентного уровеня шума на расстоянии distance по направлению, противоположенном направлению полёта МСРКН
                double Time = 0;
                while (Time <= EndTime)
                {
                    var rightSoundLevel = GetCurrentSoundLevels(Time, distance, worstRightMounth)[0]; // уровень шума в момент времени полёта Time на расстоянии distance по направлению полёта МСРКН при наихудших погодных условиях
                    var leftSoundLevel = GetCurrentSoundLevels(Time, distance, worstLeftMounth)[1]; // уровень шума в момент времени полёта Time на расстоянии distance по направлению, противоположенном направлению полёта МСРКН, при наихудших погодных условиях
                    RightEffectiveSoundLevel += Math.Pow(10, 0.1 * rightSoundLevel); // логарифмическое суммирование уровней шума на расстоянии distance по направлению полёта МСРКН
                    LeftEffectiveSoundLevel += Math.Pow(10, 0.1 * leftSoundLevel); // логарифмическое суммирование уровней шума на расстоянии distance по направлению, противоположенном направлению полёта МСРКН
                    IncrementTimes(rightSoundLevel, RightTimes); // увеличение значений времён превышения уровня шума на расстоянии distance по направлению полёта МСРКН характерных значений уровней шума, если таковое имеет место
                    IncrementTimes(leftSoundLevel, LeftTimes); // увеличение значений времён превышения уровня шума на расстоянии distance по направлению, противоположенном направлению полёта МСРКН, характерных значений уровней шума, если таковое имеет место
                    Time += TimeStep;
                }
                RightEffectiveSoundLevel = 10 * Math.Log10(RightEffectiveSoundLevel * TimeStep / 57600); // пересчёт логарифмической суммы уровней шума на расстоянии distance по направлению полёта МСРКН во всех моментах времени полёта в эффективный уровень шума
                LeftEffectiveSoundLevel = 10 * Math.Log10(LeftEffectiveSoundLevel * TimeStep / 57600); // пересчёт логарифмической суммы уровней шума на расстоянии distance против направления полёта МСРКН во всех моментах времени полёта в эффективный уровень шума
                Result.Add(new EffectiveFlightSound(
                    distance,
                    RightSoundLevels[i],
                    LeftSoundLevels[i],
                    RightEffectiveSoundLevel,
                    LeftEffectiveSoundLevel,
                    RightTimes,
                    LeftTimes,
                    worstRightMounth.Mounth,
                    worstLeftMounth.Mounth));
            }
            ProgressChanged(100);
            return Result;
        }
        List<FlightSoundCircles> Calculate(
            FlightSoundCalculationInputData id,
            List<double> SoundLevels,
            RadiusInterval RadiusInterval,
            FrequencyBand FrequencyBand,
            Action<int> ProgressChanged) // функция, определяющая геометрическое место точек, в которых уровни шума равны SoundLevels
        {
            // ProgressChanged --  внешняя функция, вызываеммая при изменении хода расчёта
            var EndTime = Math.Max(id.RocketBallistics.Distance.X.Last(), id.VehicleBallistics.Distance.X.Last()); // Конечное время расчёта как наибольшее из времён полёта МСРН и МСКА
            double LandingRadius = ((VehicleBallistics)id.VehicleBallistics).LandingRadius; // Радиус зоны посадки МСКА
            double LandingStartTime = ((VehicleBallistics)id.VehicleBallistics).LandingStartTime; // Условное время начала посадки МСКА
            var RocketSoundLevel = GetFlightSoundLevel(id.RocketBallistics, id.RocketFlowParameters, FrequencyBand); // Переменная-функуция, определяющая уровень шума от МСРН
            var VehicleSoundLevel = GetFlightSoundLevel(id.VehicleBallistics, id.VehicleFlowParameters, FrequencyBand); // Переменная-функуция, определяющая уровень шума от МСКА
            var RocketSoundLevelCircles = new List<List<FlightSoundCircle>>(); // список, содержащий списки окружностей, на которых уровни шума от МСРН равны SoundLevels в каждый момент времени полёта МСРН
            var VehicleSoundLevelCircles = new List<List<FlightSoundCircle>>(); // список, содержащий списки окружностей, на которых уровни шума от МСКА равны SoundLevels в каждый момент времени полёта МСКА
            foreach (var soundLevel in SoundLevels) // добавление в RocketSoundLevelCircles и VehicleSoundLevelCircles списков окружностей, на которых уровни шума равны SoundLevels
            {
                RocketSoundLevelCircles.Add(new List<FlightSoundCircle>());
                VehicleSoundLevelCircles.Add(new List<FlightSoundCircle>());
            }
            double startTime = 0;
            double timeStep = 1; // шаг по времени
            var TimeList = new List<double>(); // список моментов времени полёта СЧ МСРКН
            while (startTime < EndTime) // заплнение списка TimeList
            {
                TimeList.Add(startTime);
                startTime += timeStep;
            }
            int n = 0; // число расчётов для каждого момента времени геометрическое место точек, в которых уровни шума равны SoundLevels
            object Locker = new object(); // объект, блокирующий списки RocketSoundLevelCircles, VehicleSoundLevelCircles, и переменную n на запись
            Parallel.ForEach(TimeList, time =>
            // для каждого момента времени параллельно определяются уровни шума на окружностях с центрами в проекциях СЧ МСРКН на Землю, на которых уровни шума равны SoundLevels
            {
                double radius = RadiusInterval.Initial; // радиус окржности с центор в проекции СЧ МСРКН на Землю
                double radiusStep = RadiusInterval.Step; // шаг изменения радуса
                double maxRadius = RadiusInterval.Final; // иаксимальный радиус окржности с центор в проекции СЧ МСРКН на Землю
                double rocketDistance = id.RocketBallistics.Distance.Interpolate(time); // дальность полёта МСРН в момент времени time
                double vehicleDistance = id.VehicleBallistics.Distance.Interpolate(time); // дальность полёта МСКА в момент времени time
                var RocketCircles = new List<FlightSoundCircle>(); // список окружностей с центрами в проекции МСРН на Землю в момент времени time
                var VehicleCircles = new List<FlightSoundCircle>(); // список окружностей с центрами в проекции МСКА на Землю в момент времени time
                double landingRadius = (time > LandingStartTime) ? LandingRadius : 0; // увеличение радиуса окружности, на котором уровень шума от МСКА равен какому-либо значению, на величину радиуса зоны посадки МСКА в случае, если текущее время больше условного времени начала посадки МСКА
                while (radius <= maxRadius) // для каждого радиуса окружности с центром в проекции СЧ МСРКН на Землю определяется уровень шума на данной окружности
                {
                    double RocketSound = 0; // уровень шума от МСРН на окружности радиуса radius с центром в проекции МСРН на Землю в момент времени time
                    double VehicleSound = 0; // уровень шума от МСКА на окружности радиуса radius с центром в проекции МСКА на Землю в момент времени time
                    string RocketMounth = ""; // название месяца, в котором уровень шума от МСРН на окружности радиуса radius с центром в проекции МСРН на Землю в момент времени time максимален
                    string VehicleMounth = ""; // название месяца, в котором уровень шума от МСКА на окружности радиуса radius с центром в проекции МСКА на Землю в момент времени time максимален
                    foreach (var weatherParameters in id.WeatherParameters) // для всех погодных условий из списка заданных рассчитывается уровень шума от СЧ МСРКН на окружности радиуса radius для определения максимального
                    {
                        var _RocketSound = RocketSoundLevel(time, radius + rocketDistance, 0, weatherParameters); // уровень шума от МСРН в момент времени time на расстоянии от точки старта МСРКН radius + rocketDistance в направлении полёта МСРКН при погодных условиях weatherParameters
                        var _VehicleSound = VehicleSoundLevel(time, radius + vehicleDistance, 0, weatherParameters); // уровень шума от МСКА в момент времени time на расстоянии от точки старта МСРКН radius + rocketDistance в направлении полёта МСРКН при погодных условиях weatherParameters
                        if (_RocketSound > RocketSound) // обновляются значения переменных RocketSound и RocketMounth, если текущий уровень шума от МСРН на окружности радиуса radius с центром в проекции МСРН на Землю в момент времени time больше текущего максимального
                        {
                            RocketSound = _RocketSound;
                            RocketMounth = weatherParameters.Mounth;
                        }
                        if (_VehicleSound > VehicleSound) // обновляются значения переменных VehicleSound и VehicleMounth, если текущий уровень шума от МСКА на окружности радиуса radius с центром в проекции МСКА на Землю в момент времени time больше текущего максимального
                        {
                            VehicleSound = _VehicleSound;
                            VehicleMounth = weatherParameters.Mounth;
                        }
                    }
                    if (RocketSound != 0) // в список RocketCircles добавляется окружность с радиусом radius с центром в проекции МСРН на Землю rocketDistance в момент времени time, уровень шума на которой в месяце RocketMounth максимальный и равен RocketSound
                        RocketCircles.Add(new FlightSoundCircle(RocketSound, rocketDistance, radius, time, RocketMounth));
                    if (VehicleSound != 0) // в список VehicleCircles добавляется окружность с радиусом radius с центром в проекции МСКА на Землю vehicleDistance в момент времени time, уровень шума на которой в месяце VehicleMounth максимальный и равен VehicleSound
                        VehicleCircles.Add(new FlightSoundCircle(VehicleSound, vehicleDistance, radius + landingRadius, time, VehicleMounth));
                    radius += radiusStep; // увеличение текущего радиуса окружности
                }
                RocketCircles.Sort((x, y) => (int)(x.SoundLevel - y.SoundLevel)); // сортировка списка RocketCircles в порядке возрастания уровня шума на них
                VehicleCircles.Sort((x, y) => (int)(x.SoundLevel - y.SoundLevel)); // сортировка списка VehicleCircles в порядке возрастания уровня шума на них
                lock (Locker)
                {
                    for (int i = 0; i < SoundLevels.Count; i++) // для каждого уровня шума soundLevel из заданных SoundLevels определются окружности, на которых уровень шума от СЧ МСРКН в момент времени time равен soundLevel и добалются в списки RocketSoundLevelCircles и CurrentVehicleCircle
                    {
                        var soundLevel = SoundLevels[i]; // текущий уровень шума из заданных SoundLevels
                        var CurrentRocketCircle = RocketCircles.FirstOrDefault(x => x.SoundLevel > soundLevel); // из списка RocketCircles находится окружность, на которой уровень шума от МСРН в момент времени time равен soundLevel
                        if (CurrentRocketCircle != null)
                            RocketSoundLevelCircles[i].Add(CurrentRocketCircle); // в список окружностей, на которых уровень шума от МСРН равен soundLevel, добавляется найденная окружность
                        var CurrentVehicleCircle = VehicleCircles.FirstOrDefault(x => x.SoundLevel > soundLevel); // из списка VehicleCircles находится окружность, на которой уровень шума от МСКА в момент времени time равен soundLevel
                        if (CurrentVehicleCircle != null)
                            VehicleSoundLevelCircles[i].Add(CurrentVehicleCircle); // в список окружностей, на которых уровень шума от МСКА равен soundLevel, добавляется найденная окружность
                    }
                    n++;
                    ProgressChanged((int)(100.0 * n / TimeList.Count));
                }
            });
            // каждый элемент списка RightRocketCircles представляет собой список окружностей, на которых уровни шума от МСРН равны заданным из SoundLevels, крайние точки которых в направлении полёта МСРКН наиболее удалены от точки старата МСРКН
            var RightRocketCircles = RocketSoundLevelCircles.Select(x => x.MaxElemet(y => y.Distance + y.Radius)).ToList();
            // каждый элемент списка LefttRocketCircles представляет собой список окружностей, на которых уровни шума от МСРН равны заданным из SoundLevels, крайние точки которых в направлению, противоположеннном направлению полёта МСРКН, наиболее удалены от точки старата МСРКН
            var LeftRocketCircles = RocketSoundLevelCircles.Select(x => x.MinElemet(y => y.Distance - y.Radius)).ToList();
            // каждый элемент списка RightVehicleCircles представляет собой список окружностей, на которых уровни шума от МСКА равны заданным из SoundLevels, крайние точки которых в направлении полёта МСРКН наиболее удалены от точки старата МСРКН
            var RightVehicleCircles = VehicleSoundLevelCircles.Select(x => x.MaxElemet(y => y.Distance + y.Radius)).ToList();
            // каждый элемент списка LefttVehicleCircles представляет собой список окружностей, на которых уровни шума от МСКА равны заданным из SoundLevels, крайние точки которых в направлению, противоположеннном направлению полёта МСРКН, наиболее удалены от точки старата МСРКН
            var LeftVehicleCircles = VehicleSoundLevelCircles.Select(x => x.MinElemet(y => y.Distance - y.Radius)).ToList();
            Func<FlightSoundCircle, FlightSoundCircle> ToDefaultIfNull = circle => (circle != null) ? circle : new FlightSoundCircle(-1, -1, -1, -1, "-"); // функция, возвращающая оуружность с дефолтными параметрами, в случае, если входная окружность не определена
            var Result = new List<FlightSoundCircles>(); // переменаая, в которую записывается результат расчёта геометрических мест точек, в которых уровни шума равны SoundLevels
            for (int i = 0; i < SoundLevels.Count; i++)
            {
                var RightRocketCircle = RightRocketCircles[i]; // окружность, на которой уровень шума от МСРН равен SoundLevelsi[i] и крайняя точка в направлении полёта МСРКН наиболее удалена от точки старта МСРКН
                var LeftRocketCircle = LeftRocketCircles[i]; // окружность, на которой уровень шума от МСРН равен SoundLevelsi[i] и крайняя точка в направлении, противоположенном направлению полёта МСРКН, наиболее удалена от точки старта МСРКН
                var RightVehicleCircle = RightVehicleCircles[i]; // окружность, на которой уровень шума от МСКА равен SoundLevelsi[i] и крайняя точка в направлении полёта МСРКН наиболее удалена от точки старта МСРКН
                var LeftVehicleCircle = LeftVehicleCircles[i]; // окружность, на которой уровень шума от МСКА равен SoundLevelsi[i] и крайняя точка в направлении, противоположенном направлению полёта МСРКН, наиболее удалена от точки старта МСРКН
                var Circles = new List<FlightSoundCircle>() { RightRocketCircle, LeftRocketCircle, RightVehicleCircle, LeftVehicleCircle }; // список из характерых окружностей, на которых уровень шума равен SoundLevelsi[i]
                var Points = new List<TypesLibrary.Point>(); // мсассив точек, на которых уровень шума при полёте МСРКН и её СЧ равен SoundLevels[i]
                if (Circles.Count(c => c != null) != 0) // если существует хотя бы одна окружность из списка Circles
                {
                    double xmin = Circles.Where(c => c != null).Min(c => c.Distance - c.Radius); // из списка Circles находится координата точки, наиболее удалённой от точки старта МСРКН в направлении, противоположенном направлению полёта МСРКН
                    double xmax = Circles.Where(c => c != null).Max(c => c.Distance + c.Radius); // из списка Circles находится координата точки, наиболее удалённой от точки старта МСРКН в направлении полёта МСРКН
                    int N = 1000; // количесиво разбиений отрезка xmin-xmax
                    double dx = (xmax - xmin) / N; // шаг разбиения отрезка xmin-xmax
                    for (int j = 0; j < N; j++) // для каждой точки из отрезка xmin-xmax находится максимальная длина отрезка, перпендикулярного отрузке xmin-xmax, на конце которого уровень шума равен SoundLevels[i] (далее это длина обозначается y)
                    {
                        double x = xmin + dx * j; // текщая координата j-ой точки отрезка xmin-xmax
                        double y = Circles
                            .Where(circle => circle != null) // из списка Circles проводится выборка существующих окружностей
                            .Max(circle =>
                            // для каждой окружности circle из списка существующих окружностей Circles находится отрезок максималной длины, удовлетворяющий следующим условиям:
                            // 1. координата одного конца равна x
                            // 2. второй конец лежит на circle
                            // 3. перепендикулярен отрезку xmin-xmax
                            {
                                if (Math.Abs(circle.Distance - x) > circle.Radius) // если точка отрезка xmin-xmax находится вне круга с центром в окружности circle и радиусом окружности circle, искомый отрезок не существует
                                    return 0;
                                else
                                    return
                                        Math.Sqrt(Math.Pow(circle.Radius, 2) - Math.Pow(x - circle.Distance, 2)); // иначе определяем длину искомого отрезка по теореме Пифагора
                            });
                        Points.Add(new TypesLibrary.Point(x, y)); // добавляем координаты конца найденного отрезка, на котором кровень шума равен SoundLevels[i], в список Points
                    }
                    Points = Points.Concat(Points.Reverse<TypesLibrary.Point>().Select(x => new TypesLibrary.Point(x.X, -x.Y))).ToList(); // симметрично отражаем координаты точек Points относительно отрезка xmin-xmax
                    // добавляем в списко Result объект, содержащий параметры характерых окружностей (дефолтные в случае, если какая-либо из характерных окружностей не существует) и список координат точек Points, огибающих характерные окружности
                    Result.Add(new FlightSoundCircles(SoundLevels[i], ToDefaultIfNull(RightRocketCircle), ToDefaultIfNull(LeftRocketCircle), ToDefaultIfNull(RightVehicleCircle), ToDefaultIfNull(LeftVehicleCircle), Points));
                }
                else // иначе в список Result для уровня шума SoundLevels[i] добаляется объект с дефолтными параметрами характерных окружностей и пустым списком координат точек, в которых уровень шума принимает значение SoundLevel[i]
                    Result.Add(new FlightSoundCircles(SoundLevels[i], ToDefaultIfNull(null), ToDefaultIfNull(null), ToDefaultIfNull(null), ToDefaultIfNull(null), Points));
            }
            ProgressChanged(100);
            return Result;
        }
        public List<EffectiveFlightSound> Calculate(FlightSoundCalculationInputData id)  // см. метод с аналогичной сигнатурой в интерфейсе IModel
        {
            return Calculate(id, Progress => ProgressChanged(Progress));
        }
        public List<FlightSoundCircles> Calculate(
            FlightSoundCalculationInputData id,
            List<double> SoundLevels,
            RadiusInterval RadiusInterval,
            FrequencyBand FrequencyBand)  // см. метод с аналогичной сигнатурой в интерфейсе IModel
        {
            return Calculate(id, SoundLevels, RadiusInterval, FrequencyBand, Progress => ProgressChanged(Progress));
        }
        public void Calculate(
            FlightSoundCalculationInputData id,
            List<double> SoundLevels,
            RadiusInterval RadiusInterval,
            out List<EffectiveFlightSound> EffectiveFlightSounds,
            out List<FlightSoundCircles> FlightSoundCircles) // см. метод с анолагичной сигнатурой в интерфейсе IModel
        {
            List<EffectiveFlightSound> _EffectiveFlightSounds = null;
            List<FlightSoundCircles> _FlightSoundCircles = null;
            int EffectiveFlightSoundsProgress = 0; // состояние расчёта эквивалентных уровней шума
            int FlightSoundCirclesProgress = 0; // состояние расчёта геометрического места точек, в которых уровни шума в слышиомом диапазоне равны SoundLevel
            object Locker = new object(); // объект, блокирующий переменные EffectiveFlightSoundsProgres и FlightSoundCirclesProgress на запись
            Action ProgressChanges = () =>
            {
                lock (Locker)
                {
                    ProgressChanged(Math.Min(EffectiveFlightSoundsProgress, FlightSoundCirclesProgress)); // отправка вызвающему коду сообщения о прогрессе выполненя расчёта
                }
            };
            // Поток, в котором выполняется расчёт эквивалентных уровней шума
            var EffectiveFlightSoundsTask = new Task(() => _EffectiveFlightSounds = Calculate(id, Progress =>
            {
                EffectiveFlightSoundsProgress = Progress;
                ProgressChanges();
            }));
            // потоко, в котором выполняется расчёт геометрических место тосек, в которых уровени шума равны заданным SoundLevels
            var FlightSoundCirclesTask = new Task(() => _FlightSoundCircles = Calculate(id, SoundLevels, RadiusInterval, FrequencyBand.Normal, Progress =>
            {
                FlightSoundCirclesProgress = Progress;
                ProgressChanges();
            }));
            EffectiveFlightSoundsTask.Start(); // старт потока
            FlightSoundCirclesTask.Start(); // старт потока
            System.Threading.Tasks.Task.WhenAll(EffectiveFlightSoundsTask, FlightSoundCirclesTask).Wait(); // ожидание выполнения потоков
            ProgressChanged(100);
            EffectiveFlightSounds = _EffectiveFlightSounds; // запись в возаращаемую переменную результата расчёта эквивалентных урвней шума
            FlightSoundCircles = _FlightSoundCircles; // запись в возаращаемую переменную результата расчёта геометрических место тосек, в которых уровени шума равны заданным SoundLevels
        }
        public void Save(
            string FileName,
            FlightSoundCalculationInputData id,
            Dictionary<double, Color> SoundLevels,
            RadiusInterval RadiusInterval,
            FrequencyBand FrequencyBand) // см. метод с аналогичной сигнатурой в интерфейсе IModel
        {
            var xmlDoc = new XmlDocument();
            var Root = xmlDoc.CreateElement("FlightNoiseInputData");
            Action<XmlNode, string, double> AddAttribute = (node, name, value) =>
            {
                var attribute = xmlDoc.CreateAttribute(name);
                attribute.Value = value.ToString();
                node.Attributes.Append(attribute);
            };
            Func<string, Ballistics, XmlNode> BallisticsNode = (Name, Ballistics) =>
            {
                var ballisticsNode = xmlDoc.CreateElement(Name);
                for (int i = 0; i < Ballistics.Distance.X.Length; i++)
                {
                    var node = xmlDoc.CreateElement("Moment");
                    AddAttribute(node, "Time", Ballistics.Height.X[i]);
                    AddAttribute(node, "Thrust", Ballistics.Thrust.Y[i]);
                    AddAttribute(node, "Distance", Ballistics.Distance.Y[i]);
                    AddAttribute(node, "Height", Ballistics.Height.Y[i]);
                    ballisticsNode.AppendChild(node);
                }
                if (Ballistics is VehicleBallistics)
                {
                    var LandingRadiusNode = xmlDoc.CreateElement("LandingRadius");
                    LandingRadiusNode.InnerText = ((VehicleBallistics)Ballistics).LandingRadius.ToString();
                    ballisticsNode.AppendChild(LandingRadiusNode);
                    var LandingStartTimeNode = xmlDoc.CreateElement("LandingStartTime");
                    LandingStartTimeNode.InnerText = ((VehicleBallistics)Ballistics).LandingStartTime.ToString();
                    ballisticsNode.AppendChild(LandingStartTimeNode);

                } 
                return ballisticsNode;
            };
            Func<string, FlowParameters, XmlNode> FlowParametersNode = (Name, Parameters) =>
            {
                var flowParametersNode = xmlDoc.CreateElement(Name);
                AddAttribute(flowParametersNode, "MassFlow", Parameters.MassFlow);
                AddAttribute(flowParametersNode, "NozzleDiameter", Parameters.NozzleDiameter);
                AddAttribute(flowParametersNode, "NozzleMachNumber", Parameters.NozzleMachNumber);
                AddAttribute(flowParametersNode, "NozzleFlowVelocity", Parameters.NozzleFlowVelocity);
                AddAttribute(flowParametersNode, "ChamberSoundVelocity", Parameters.ChamberSoundVelocity);
                AddAttribute(flowParametersNode, "NozzleAdiabaticIndex", Parameters.NozzleAdiabaticIndex);
                return flowParametersNode;
            };
            Func<XmlNode> WeatherParametersNode = () =>
            {
                var weatherParametersNode = xmlDoc.CreateElement("WeatherParameters");
                foreach(var weatherParameters in id.WeatherParameters)
                {
                    var WeatherNode = xmlDoc.CreateElement(weatherParameters.Mounth);
                    AddAttribute(WeatherNode, "Humidity", weatherParameters.Humidity);
                    AddAttribute(WeatherNode, "Temperature", weatherParameters.Temperature);
                    weatherParametersNode.AppendChild(WeatherNode);
                }
                return weatherParametersNode;
            };
            Func<XmlNode> SoundLevelsNode = () =>
            {
                var soundLevelsNode = xmlDoc.CreateElement("SoundLevels");
                foreach(var soundLevel in SoundLevels)
                {
                    var SoundLevelNode = xmlDoc.CreateElement("SoundLevel");
                    SoundLevelNode.InnerText = soundLevel.Key.ToString();
                    AddAttribute(SoundLevelNode, "A", soundLevel.Value.A);
                    AddAttribute(SoundLevelNode, "R", soundLevel.Value.R);
                    AddAttribute(SoundLevelNode, "G", soundLevel.Value.G);
                    AddAttribute(SoundLevelNode, "B", soundLevel.Value.B);
                    soundLevelsNode.AppendChild(SoundLevelNode);
                }
                return soundLevelsNode;
            };
            Func<XmlNode> RadiusIntervalNode = () =>
            {
                var radiusIntervalNode = xmlDoc.CreateElement("RadiusInterval");
                AddAttribute(radiusIntervalNode, "Initial", RadiusInterval.Initial);
                AddAttribute(radiusIntervalNode, "Final", RadiusInterval.Final);
                AddAttribute(radiusIntervalNode, "Step", RadiusInterval.Step);
                return radiusIntervalNode;
            };
            Func<XmlNode> FrequencyBandNode = () =>
            {
                var frequencyBandNode = xmlDoc.CreateElement("FrequencyBand");
                if (FrequencyBand == TypesLibrary.FrequencyBand.Infra)
                    frequencyBandNode.InnerText = "Infra";
                else if (FrequencyBand == TypesLibrary.FrequencyBand.Ultra)
                    frequencyBandNode.InnerText = "Ultra";
                else
                    frequencyBandNode.InnerText = "Normal";
                return frequencyBandNode;
            };
            Root.AppendChild(BallisticsNode("RocketBallistics", id.RocketBallistics));
            Root.AppendChild(BallisticsNode("VehicleBallistics", id.VehicleBallistics));
            Root.AppendChild(FlowParametersNode("RocketFlowParameters", id.RocketFlowParameters));
            Root.AppendChild(FlowParametersNode("VehicleFlowParameters", id.VehicleFlowParameters));
            Root.AppendChild(WeatherParametersNode());
            if ((SoundLevels != null) && (SoundLevels.Count != 0))
                Root.AppendChild(SoundLevelsNode());
            Root.AppendChild(RadiusIntervalNode());
            Root.AppendChild(FrequencyBandNode());
            xmlDoc.AppendChild(Root);
            xmlDoc.Save(FileName);
        }
        public void Open(
            XmlDocument xmlDoc,
            out FlightSoundCalculationInputData id,
            out Dictionary<double, Color> SoundLevels,
            out RadiusInterval RadiusInterval,
            out FrequencyBand FrequencyBand) // см. метод с аналогичной сигнатурой в интерфейсе IModel
        {
            Func<XmlNode, string, XmlNode> FindNode = (Node, ChildNodeName) => Node.ChildNodes.Cast<XmlNode>().FirstOrDefault(x => x.Name == ChildNodeName);
            Func<XmlNode, bool, Ballistics> GetBallistics = (Node, IsVehicle) =>
            {
                List<double> Time = new List<double>();
                List<double> Thrust = new List<double>();
                List<double> Distance = new List<double>();
                List<double> Height = new List<double>();
                for (int i = 0; i < Node.ChildNodes.Count - (IsVehicle ? 2 : 0); i++)
                {
                    var node = Node.ChildNodes[i];
                    Time.Add(Convert.ToDouble(node.Attributes["Time"].Value));
                    Thrust.Add(Convert.ToDouble(node.Attributes["Thrust"].Value));
                    Distance.Add(Convert.ToDouble(node.Attributes["Distance"].Value));
                    Height.Add(Convert.ToDouble(node.Attributes["Height"].Value));
                }
                if (!IsVehicle)
                    return new RocketBallistics(
                        new Interpolation(Time.ToArray(), Height.ToArray()),
                        new Interpolation(Time.ToArray(), Distance.ToArray()),
                        new Interpolation(Time.ToArray(), Thrust.ToArray()));
                else
                    return new VehicleBallistics(
                        new Interpolation(Time.ToArray(), Height.ToArray()),
                        new Interpolation(Time.ToArray(), Distance.ToArray()),
                        new Interpolation(Time.ToArray(), Thrust.ToArray()),
                        Convert.ToDouble(FindNode(Node, "LandingRadius").InnerText),
                        Convert.ToDouble(FindNode(Node, "LandingStartTime").InnerText));
            };
            Func<XmlNode, FlowParameters> GetFlowParameters = Node => new FlowParameters(
                Convert.ToDouble(Node.Attributes["MassFlow"].Value),
                Convert.ToDouble(Node.Attributes["NozzleDiameter"].Value),
                Convert.ToDouble(Node.Attributes["NozzleMachNumber"].Value),
                Convert.ToDouble(Node.Attributes["NozzleFlowVelocity"].Value),
                Convert.ToDouble(Node.Attributes["ChamberSoundVelocity"].Value),
                Convert.ToDouble(Node.Attributes["NozzleAdiabaticIndex"].Value));
            Func<XmlNode, List<WeatherParameters>> GetWeatherParameters = Node => Node.ChildNodes.Cast<XmlNode>().Select(x => new WeatherParameters(
                x.Name,
                Convert.ToDouble(x.Attributes["Humidity"].Value),
                Convert.ToDouble(x.Attributes["Temperature"].Value))).ToList();
            Func<XmlNode, Dictionary<double, Color>> GetSoundLevels = Node =>
            {
                if (Node == null) return null;
                return Node.ChildNodes.Cast<XmlNode>()
                    .Select(x => new
                    {
                        SoundLevel = Convert.ToDouble(x.InnerText),
                        Color = Color.FromArgb(
                            Convert.ToInt32(x.Attributes["A"].Value),
                            Convert.ToInt32(x.Attributes["R"].Value),
                            Convert.ToInt32(x.Attributes["G"].Value),
                            Convert.ToInt32(x.Attributes["B"].Value))
                    })
                    .ToDictionary(x => x.SoundLevel, x => x.Color);
            };
            Func<XmlNode, RadiusInterval> GetRadiusInterval = Node => new RadiusInterval(
                Convert.ToDouble(Node.Attributes["Initial"].Value),
                Convert.ToDouble(Node.Attributes["Final"].Value),
                Convert.ToDouble(Node.Attributes["Step"].Value));
            Func<XmlNode, FrequencyBand> GetFrequencyBand = Node =>
            {
                var band = Node.InnerText;
                if (band == "Infra")
                    return TypesLibrary.FrequencyBand.Infra;
                else if (band == "Ultra")
                    return TypesLibrary.FrequencyBand.Ultra;
                else
                    return TypesLibrary.FrequencyBand.Normal;
            };
            var Root = xmlDoc.ChildNodes[0];
            id = new FlightSoundCalculationInputData(
                (RocketBallistics)GetBallistics(FindNode(Root, "RocketBallistics"), false),
                (VehicleBallistics)GetBallistics(FindNode(Root, "VehicleBallistics"), true),
                GetFlowParameters(FindNode(Root, "RocketFlowParameters")),
                GetFlowParameters(FindNode(Root, "VehicleFlowParameters")),
                GetWeatherParameters(FindNode(Root, "WeatherParameters")));
            SoundLevels = GetSoundLevels(FindNode(Root, "SoundLevels"));
            RadiusInterval = GetRadiusInterval(FindNode(Root, "RadiusInterval"));
            FrequencyBand = GetFrequencyBand(FindNode(Root, "FrequencyBand"));
        }
        public void Open(
            string FileName,
            out FlightSoundCalculationInputData id,
            out Dictionary<double, Color> SoundLevels,
            out RadiusInterval RadiusInterval,
            out FrequencyBand FrequencyBand) // см. метод с аналогичной сигнатурой в интерфейсе IModel
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(FileName);
            Open(xmlDoc, out id, out SoundLevels, out RadiusInterval, out FrequencyBand);
        }

        public event Action<int> ProgressChanged;
    }
    static class EnumerableMaxElement
    {
        private static T ExtremumElemet<T>(IEnumerable<T> Collection, Func<T, double> selector, Func<double, double, bool> compare) 
            where T: class
        {
            if ((Collection == null) || (Collection.Count() == 0)) return null;
            double ExtrValue = selector(Collection.ElementAt(0));
            T ExtrItem = Collection.ElementAt(0);
            foreach (var item in Collection.Skip(0))
            {
                double Value = selector(item);
                if (compare(Value, ExtrValue))
                {
                    ExtrValue = Value;
                    ExtrItem = item;
                }
            }
            return ExtrItem;
        }
        public static T MaxElemet<T>(this IEnumerable<T> Collection, Func<T, double> selector)
            where T : class
        {
            Func<double, double, bool> compare = (Value, ExtrValue) => Value > ExtrValue;
            return ExtremumElemet(Collection, selector, compare);
        }
        public static T MinElemet<T>(this IEnumerable<T> Collection, Func<T, double> selector)
            where T : class
        {
            Func<double, double, bool> compare = (Value, ExtrValue) => Value < ExtrValue;
            return ExtremumElemet(Collection, selector, compare);
        }
    }
}