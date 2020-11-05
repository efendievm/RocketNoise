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
    class FireTestNoiseModel
    {
        public StaticSoundLevel GetStaticSoundLevel(double Thrust, FlowParameters flowParameters, FrequencyBand frequencyBand) // см. метод с аналогичной сигнатурой в IModel
        {
            double[] Frequencies, FrequencyBands; // массив среднегеометрических частот и ширин частотных полос
            // определение массивов Frequencies, FrequencyBands по исходному спектру частот frequencyBand
            if (frequencyBand == FrequencyBand.Infra)
                FrequenciesAggregator.Infra(out Frequencies, out FrequencyBands);
            else if (frequencyBand == FrequencyBand.Ultra)
                FrequenciesAggregator.Ultra(out Frequencies, out FrequencyBands);
            else
                FrequenciesAggregator.Normal(out Frequencies, out FrequencyBands);
            double[] spectralDecomposition = SpectralDecomposition.Get(1.01325E5, flowParameters, Frequencies, FrequencyBands); // Разложение шума на спектр по частотам
            // Определение функции, возвращающей уровень шума от двиагтеля на расстоянии Radius от сопла под углом Angle к напрвлению оси газовой струи при погодных условиях weatherParameters
            return (Radius, Angle, weatherParameters) =>
            {
                double[] atmospherePropagation = new double[Frequencies.Length]; // массив затуханий звука на частотах Frequencies на расстоянии Radius при погодных условиях weatherParameters
                var atmosphereParameters = new AtmosphereParameters(weatherParameters.Temperature, 1.01325E5); // создание объекта, хранящего атмосферные параметры
                for (int j = 0; j < Frequencies.Length; j++)
                    atmospherePropagation[j] = Atmosphere.AbsorptionRatio(weatherParameters.Humidity, atmosphereParameters, Frequencies[j]) * Radius; // заполнение массива atmospherePropagation
                double L0 = 10 * Math.Log10(0.5 * 0.0022 * Thrust * flowParameters.NozzleFlowVelocity) + 120 - 11 - 20 * Math.Log10(Radius) + DI.Interpolate(Angle * 180 / Math.PI); // Уровень шума в искомой точке без учёта корреляции по шкале A и затухания звука
                double L = 0; // Уровень шума в искомой точке с учётом корреляции по шкале A и затухания звука
                for (int i = 0; i < Frequencies.Length; i++)
                {
                    double correction = frequencyBand == FrequencyBand.Normal ? Correction.Get(i) : 0; // корреляции по шкале Aв случае расчёта в слышимом диапазоне частот
                    double CurrentL = L0 + spectralDecomposition[i] - atmospherePropagation[i] + correction; // Уровень шума в искомой точке на частоте Frequencies[i] с учётом корреляции по шкале A и затуханием звука
                    if (CurrentL > 0)
                        L += Math.Pow(10, 0.1 * CurrentL); // логарифмическое суммиррование уровней шума на частотах Frequencies[i]
                }
                if (L > 0) return 10 * Math.Log10(L);
                else return 0;
            };
        }
        public List<FireTestSoundContour> Calculate(FireTestNoiseCalculationInputData id, List<double> SoundLevels)
        {
            var SoundLevel = GetStaticSoundLevel(id.Thrust, id.FlowParameters, id.FrequencyBand); // функция, возвращающая уровень шума в зависимости от расстояния до наблюдателя, угла между направлением на наблюдателя и осью газовой струи и погодных условий
            var AngleSoundLevels = new Dictionary<double, Dictionary<double, double>>(); // словарь, ключи которого -- углы между направлением на наблюдателя и осью газовой струи, значения -- словари, ключи которых -- расстояния до наблюдателя, значения -- уровени шума SoundLevels 
            var Angles = Enumerable.Range(0, 181).Select(x => x * Math.PI / 180); // список углов между направлением на наблюдателя и осью газовой струи (от 0 до pi с шагом pi / 180)
            int n = 0; // количество расчётов уровней шума на различных расстояниях под углами к оси газовой струи из списка Angles
            object Locker = new object(); // объект, блокирующий на запись словарь AngleSoundLevels и переменную n
            Parallel.ForEach(Angles, Angle =>
            // параллельно для каждого угла Angle между направлением на наблоюдателя и ось газовой струи в списке Angles определяется словрь, ключи которого -- расстояния до наблюдателя под углом Angle, значения -- уровени шума
            {
                var CurrentAngleAllSoundLevels = new Dictionary<double, double>(); // словарь, ключи которого -- расстояния до наблюдателя под углом Angle, значения -- уровени шума
                double Radius = id.RadiusInterval.Initial; // начальныое расстояние до наблюдателя
                double RadiusStep = id.RadiusInterval.Step; // конечное расстояние до наблюдателя (область расчёта)
                while (Radius < id.RadiusInterval.Final)
                {
                    double soundLevel = id.WeatherParameters.Max(w => SoundLevel(Radius, Angle, w)); // для всех погодынх условий w из списка заданных id.WeatherParameters определяются уровни шума на расстоянии Radius под углом Angle к оси газовой струи и выбирается максимальный
                    CurrentAngleAllSoundLevels.Add(Radius, soundLevel); // найденный уровень шума добавляется в словарь
                    Radius += RadiusStep;
                }
                var SortedCurrentAngleAllSoundLevels = CurrentAngleAllSoundLevels.OrderBy(x => x.Value); // словарь CurrentAngleAllSoundLevels сортируется по значением уровней шума (по ключам), чтобы далее осуществлять поиск
                var CurrentAngleSoundLevels = new Dictionary<double, double>(); // словарь, ключи которого -- расстояния до наблюдателя по углом Angle к оси газовой струи, значения -- уровени шума SoundLevels 
                foreach (var soundLevel in SoundLevels)
                // для каждого уровня шума soundLevel, принадлежащему списку SoundLevels, находится из словаря SortedCurrentAngleAllSoundLevels расстояние r под углом Angle к оси газовой струи, на котором уровень шума равен soundLevel и добавляется в словарь CurrentAngleSoundLevels
                {
                    double r = 0;
                    try
                    {
                        r = SortedCurrentAngleAllSoundLevels.First(x => x.Value > soundLevel).Key;
                    }
                    catch { }
                    finally
                    {
                        CurrentAngleSoundLevels.Add(soundLevel, r);
                    }
                }
                lock (Locker) // блокируется мьютекс
                {
                    AngleSoundLevels.Add(Angle * 180 / Math.PI, CurrentAngleSoundLevels); // в словарь AngleSoundLevels добавляется пара ключь-значение, где ключ -- текущий угол Angle, значение -- словарь, ключи которого -- расстояния, занчения -- уровни шума SoundLevels
                    n++;
                    ProgressChanged((int)(100.0 * n / Angles.Count()));
                }
            });
            return AngleSoundLevels
                .SelectMany(x => x.Value, (x, y) => new { Angle = x.Key, Radius = y.Value, SoundLevel = y.Key })
                // из словаря AngleSoundLevels создаётся список объектов с полями Angle (угол между направлением на наблюдателя и осью газовой струи), SoundLevel (уровень шума), Radius (расстояние до наблюдателя) по следующему принципу (описание метода SelectMany):
                // 1. текущей паре ключ-значение x словаря AngleSoundLevel, имеющему тип типа KeyPair<double, Dictionary<double, double>, ставится в соответсвие словарь Dictionary<double, double>, являющийся значением пары ключ-значение x (операция x => x.Value)
                // 2. текущей паре ключ-значение x словаря AngleSoundLevel и текущей паре ключ-значение y полученного на шаге 1 словаря ставится в соответсвие объект с полями Angle = x.Key, Radius = y.Value, SoundLevel = y.Key (назовём этот объект объектом типа *)
                // 3. повторяется шаг 3 для всех пар ключ-значение y и собирается список объектов типа *
                // 4. повторяются шаги 1, 2, 3 для всех пар ключ-значение x и собирается полный список объектов типа *
                .GroupBy(x => x.SoundLevel) // полученный методом SelectMany список объектов типа * группируется по полю SoundLevel
                .Select(x => // группы объектов типа * со значениями полей SoundLevel, равными soundLevel из заданного списка уровней шума SoundLevels, проецируются с список объектов типа FireTestSoundContour
                {
                    double soundLevel = x.Key; // уровень шума текущей группы объектов типа *
                    var Points = x
                        .Select(y => new { Angle = y.Angle, Radius = y.Radius }) // элементы y группы x проецируются в список радиус-векторов 
                        .OrderBy(y => y.Angle) // список радиус-вектров сортируется по углу наклона к оси газовой струи
                        .Select(p => new TypesLibrary.Point(p.Radius * Math.Cos(p.Angle * Math.PI / 180), p.Radius * Math.Sin(p.Angle * Math.PI / 180))); // элементы p списка радиус-векторов проецируются в список координат их концов
                    Points = Points.Concat(Points.Reverse<TypesLibrary.Point>().Select(p => new TypesLibrary.Point(p.X, -p.Y))); // симметрично отражаем координаты точек Points относительно оси газовой струи
                    return new FireTestSoundContour(
                        soundLevel,
                        Points.ToList());
                })
                .ToList();
        }
        public void Save(
            string FileName,
            FireTestNoiseCalculationInputData id,
            Dictionary<double, Color> SoundLevels)  // см. метод с аналогичной сигнатурой в IModel
        {
            var xmlDoc = new XmlDocument();
            var Root = xmlDoc.CreateElement("FireTestNoiseInputData");
            Action<XmlNode, string, double> AddAttribute = (node, name, value) =>
            {
                var attribute = xmlDoc.CreateAttribute(name);
                attribute.Value = value.ToString();
                node.Attributes.Append(attribute);
            };
            Func<XmlNode> RadiusIntervalNode = () =>
            {
                var radiusIntervalNode = xmlDoc.CreateElement("RadiusInterval");
                AddAttribute(radiusIntervalNode, "Initial", id.RadiusInterval.Initial);
                AddAttribute(radiusIntervalNode, "Final", id.RadiusInterval.Final);
                AddAttribute(radiusIntervalNode, "Step", id.RadiusInterval.Step);
                return radiusIntervalNode;
            };
            Func<XmlNode> FrequencyBandNode = () =>
            {
                var frequencyBandNode = xmlDoc.CreateElement("FrequencyBand");
                if (id.FrequencyBand == TypesLibrary.FrequencyBand.Infra)
                    frequencyBandNode.InnerText = "Infra";
                else if (id.FrequencyBand == TypesLibrary.FrequencyBand.Ultra)
                    frequencyBandNode.InnerText = "Ultra";
                else
                    frequencyBandNode.InnerText = "Normal";
                return frequencyBandNode;
            };
            var thrustNode = xmlDoc.CreateElement("Thrust");
            thrustNode.InnerText = id.Thrust.ToString();
            Root.AppendChild(thrustNode);
            var flowParametersNode = xmlDoc.CreateElement("FlowParameters");
            AddAttribute(flowParametersNode, "MassFlow", id.FlowParameters.MassFlow);
            AddAttribute(flowParametersNode, "NozzleDiameter", id.FlowParameters.NozzleDiameter);
            AddAttribute(flowParametersNode, "NozzleMachNumber", id.FlowParameters.NozzleMachNumber);
            AddAttribute(flowParametersNode, "NozzleFlowVelocity", id.FlowParameters.NozzleFlowVelocity);
            AddAttribute(flowParametersNode, "ChamberSoundVelocity", id.FlowParameters.ChamberSoundVelocity);
            AddAttribute(flowParametersNode, "NozzleAdiabaticIndex", id.FlowParameters.NozzleAdiabaticIndex);
            Root.AppendChild(flowParametersNode);
            var weatherParametersNode = xmlDoc.CreateElement("WeatherParameters");
            foreach (var weatherParameters in id.WeatherParameters)
            {
                var WeatherNode = xmlDoc.CreateElement(weatherParameters.Mounth);
                AddAttribute(WeatherNode, "Humidity", weatherParameters.Humidity);
                AddAttribute(WeatherNode, "Temperature", weatherParameters.Temperature);
                weatherParametersNode.AppendChild(WeatherNode);
            }
            Root.AppendChild(weatherParametersNode);
            if ((SoundLevels != null) && (SoundLevels.Count != 0))
            {
                var soundLevelsNode = xmlDoc.CreateElement("SoundLevels");
                foreach (var soundLevel in SoundLevels)
                {
                    var SoundLevelNode = xmlDoc.CreateElement("SoundLevel");
                    SoundLevelNode.InnerText = soundLevel.Key.ToString();
                    AddAttribute(SoundLevelNode, "A", soundLevel.Value.A);
                    AddAttribute(SoundLevelNode, "R", soundLevel.Value.R);
                    AddAttribute(SoundLevelNode, "G", soundLevel.Value.G);
                    AddAttribute(SoundLevelNode, "B", soundLevel.Value.B);
                    soundLevelsNode.AppendChild(SoundLevelNode);
                }
                Root.AppendChild(soundLevelsNode);
            }
            Root.AppendChild(RadiusIntervalNode());
            Root.AppendChild(FrequencyBandNode());
            xmlDoc.AppendChild(Root);
            xmlDoc.Save(FileName);
        }
        public void Open(
            XmlDocument xmlDoc,
            out FireTestNoiseCalculationInputData id,
            out Dictionary<double, Color> SoundLevels) // см. метод с аналогичной сигнатурой в IModel
        {
            Func<XmlNode, double> GetThrust = Node => Convert.ToDouble(Node.InnerText);
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
            Func<XmlNode, string, XmlNode> FindNode = (Node, ChildNodeName) => Node.ChildNodes.Cast<XmlNode>().FirstOrDefault(x => x.Name == ChildNodeName);
            id = new FireTestNoiseCalculationInputData(
                GetThrust(FindNode(Root, "Thrust")),
                GetFlowParameters(FindNode(Root, "FlowParameters")),
                GetWeatherParameters(FindNode(Root, "WeatherParameters")),
                GetRadiusInterval(FindNode(Root, "RadiusInterval")),
                GetFrequencyBand(FindNode(Root, "FrequencyBand")));
            SoundLevels = GetSoundLevels(FindNode(Root, "SoundLevels"));
        }
        public void Open(
            string FileName,
            out FireTestNoiseCalculationInputData id,
            out Dictionary<double, Color> SoundLevels) // см. метод с аналогичной сигнатурой в IModel
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(FileName);
            Open(xmlDoc, out id, out SoundLevels);
        }
        public event Action<int> ProgressChanged;
    }
}
