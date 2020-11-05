using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypesLibrary;
using System.Xml;

namespace ModelLibrary
{
    class SonicBoomModel
    {
        Interpolation n_d_Int = new Interpolation(
            new double[14] { 0, 10, 20, 30, 39, 47, 52, 55, 60, 62.5, 65, 70, 75, 80 }.Select(x => x * 1000).ToArray(),
            new double[14] { 0.214, 0.268, 0.279, 0.193, 0, 2.229, 2.357, 1.868, 0, 0.279, 0.364, 0.429, 0.429, 0.407 });
        Interpolation n_p_Int = new Interpolation(
            new double[18] { 0, 10, 15, 20, 25, 32, 35, 40, 45, 47, 52.5, 60, 62.5, 65, 67.5, 70, 75, 80 }.Select(x => x * 1000).ToArray(),
            new double[18] { 0.004, 0.057, 0.1, 0.132, 0.164, 0.196, 0.218, 0.314, 0.486, 0.614, 0.582, 0.25, 0.175, 0.121, 0.1, 0.084, 0.068, 0.079 });
        Interpolation n_t_Int = new Interpolation(
            new double[24] { 0, 11, 12.25, 15, 20, 22.5, 25, 30, 32.5, 35, 40, 42.5, 45, 47.5, 50, 52.5, 55, 57.5, 60, 62.5, 65, 70, 75, 80 }.Select(x => x * 1000).ToArray(),
            new double[24] { 0.0003, 0.059, 0.048, 0.037, 0.023, 0.015, 0.01, 0.006, 0.004, -0.002, -0.009, -0.011, -0.013, -0.013, - 0.01 , -0.005, -0.003, 0.001, 0.008, 0.02, 0.03, 0.041, 0.046, 0.049 });
        Interpolation M_c_Int = new Interpolation(
            new double[10] { 0, 5, 10, 11, 20, 32.5, 44.5, 52.5, 61, 73 }.Select(x => x * 1000).ToArray(),
            new double[10] { 1, 1.06, 1.131, 1.152, 1.115, 1.12, 1.043, 1.043, 1.069, 1.238 });
        Interpolation K_damb_Int = new Interpolation(
            new double[9] { 0, 10.1, 15, 20, 30, 46, 55, 60, 80 }.Select(x => x * 1000).ToArray(),
            new double[9] { 0.998, 1.073, 1.052, 1.041, 1.009, 0.939, 0.945, 0.971, 1.138 });
        Interpolation K_dc_Int = new Interpolation(
            new double[14] { 0, 11, 12.5, 15, 20, 25, 30, 32.5, 47, 52.5, 61, 65, 70, 80 }.Select(x => x * 1000).ToArray(),
            new double[14] { 1.993, 2.143, 1.993, 1.821, 1.607, 1.414, 1.286, 1.243, 0.643, 0.686, 1.007, 1.221, 1.479, 1.929});
        Interpolation K_pamb_Int = new Interpolation(
            new double[16] { 0, 11, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80 }.Select(x => x * 1000).ToArray(),
            new double[16] { 1, 1.038, 1.091, 1.134, 1.188, 1.241, 1.295, 1.348, 1.402, 1.439, 1.466, 1.482, 1.493, 1.493, 1.488, 1.482 });
        Interpolation K_tamb_Int = new Interpolation(
            new double[14] { 0, 11, 12.5, 15, 17.5, 20, 25, 30, 42.5, 45, 50, 55, 60, 80 }.Select(x => x * 1000).ToArray(),
            new double[14] { 1, 0.85, 0.845, 0.534, 0.818, 0.796, 0.77, 0.748, 0.705, 0.7, 0.689, 0.679, 0.663, 0.561 });
        Interpolation K_s_Int = new Interpolation(
            new double[11] { 0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1 },
            new double[11] { 0.784, 0.752, 0.721, 0.689, 0.623, 0.608, 0.558, 0.572, 0.603, 0.635, 0.675 });
        double n_d(double Height)
        {
            return n_d_Int.Interpolate(Height);
        }
        double n_p(double Height)
        {
            return n_p_Int.Interpolate(Height);
        }
        double n_t(double Height)
        {
            return n_t_Int.Interpolate(Height);
        }
        double M_c(double Height)
        {
            return M_c_Int.Interpolate(Height);
        }
        double K_damb(double Height)
        {
            return K_damb_Int.Interpolate(Height);
        }
        double K_dc(double Height)
        {
            return K_dc_Int.Interpolate(Height);
        }
        double K_pamb(double Height)
        {
            return K_pamb_Int.Interpolate(Height);
        }
        double K_tamb(double Height)
        {
            return K_tamb_Int.Interpolate(Height);
        }
        double M_e(double MachNumber)
        {
            return MachNumber / Math.Sqrt(MachNumber * MachNumber - 1);
        }
        double K_d(double Height, double MachNumber)
        {
            double _M_e = M_e(MachNumber);
            double _K_dc = K_dc(Height);
            return _K_dc + (K_damb(Height) - _K_dc) * Math.Pow((_M_e - M_c(Height)) / (_M_e - 1), n_d(Height));
        }
        double K_p(double Height, double MachNumber)
        {
            double _M_e = M_e(MachNumber);
            double _M_c = M_c(Height);
            return K_pamb(Height) * Math.Pow((_M_e - 1) / (_M_e - _M_c), n_p(Height));
        }
        double K_t(double Height, double MachNumber)
        {
            return K_tamb(Height) * Math.Pow(MachNumber / (MachNumber - 1), n_p(Height));
        }
        double d_x(double Height, double MachNumber)
        {
            return K_d(Height, MachNumber) * Height / Math.Sqrt(Math.Pow(M_e(MachNumber), 2) - 1);
        }
        double h_e(double Height, double MachNumber)
        {
            return d_x(Height, MachNumber);
        }
        double[] SonicBoomFrequencies = Enumerable.Range(0, 101).Select(x => 0.1 + (100 - 0.1) / 100 * x).ToArray();
        double Delta_atm(double Height, double DistanceToGround, WeatherParameters WeatherParameters)
        {
            var atmospherePropagation = Atmosphere.Propagation(SonicBoomFrequencies, WeatherParameters, Height, DistanceToGround);
            return Math.Pow(10, 10 * Math.Log10(atmospherePropagation.Select(x => Math.Pow(10, -0.1 * x)).Sum() / SonicBoomFrequencies.Length) / 20);
        }
        double Delta_p(double Height, double MachNumber, double Length, double K_s, WeatherParameters WeatherParameters)
        {
            double _M_e = M_e(MachNumber);
            double d = d_x(Height, MachNumber);
            double _H_e = Math.Max(d, Height);
            if (_M_e < M_c(MachNumber))
                return 0;
            else
                return Math.Max(2 * K_p(Height, MachNumber) * Math.Sqrt(1.0135E5 * Atmosphere.ParametersAtHeight(Height, WeatherParameters.Temperature).Pressure) *
                    Math.Pow(Math.Pow(MachNumber, 2) - 1, 1.0 / 8) * Math.Pow(Length / _H_e, 3.0 / 4) * K_s * Delta_atm(Height, d, WeatherParameters), 0);
        }
        double Delta_t(double Height, double MachNumber, double Length, double K_s, WeatherParameters WeatherParameters)
        {
            double a = 20.046796 * Math.Sqrt(Atmosphere.ParametersAtHeight(Height, WeatherParameters.Temperature).Temperature);
            return K_t(Height, MachNumber) * 3.42 / a * MachNumber / Math.Pow(Math.Pow(MachNumber, 2) - 1, 3.0 / 8) *
                Math.Pow(Math.Max(h_e(Height, MachNumber), Height), 1.0 / 4) * Math.Pow(Length, 3.0 / 4) * K_s;
        }
        List<SonicBoomParameters> Calculate(
            SonicBoomBallistics Ballistics, 
            GeometricalParameters GeometricalParameters,
            List<WeatherParameters> WeatherParameters,
            Action<int> ProgressChanged)
        {
            List<SonicBoomParameters> SonicBoom = new List<SonicBoomParameters>();
            var K_s = K_s_Int.Interpolate(GeometricalParameters.CharacteristicArea / GeometricalParameters.MaximalArea) * 
                Math.Sqrt(GeometricalParameters.MaximalArea) / 
                (Math.Pow(GeometricalParameters.Length, 3.0 / 4) * Math.Pow(GeometricalParameters.CharacteristicLength, 1.0 / 4));
            var StartTime = Ballistics.Height.X[0];
            var EndTime = Ballistics.Height.X.Last();
            var TimeStep = 1;
            var TimeList = new List<double>();
            while (StartTime < EndTime)
            {
                TimeList.Add(StartTime);
                StartTime += TimeStep;
            }
            int n = 0;
            object Locker = new object();
            Parallel.ForEach(TimeList, Time =>
            {
                var height = Ballistics.Height.Interpolate(Time);
                var machNumber = Ballistics.MachNumber.Interpolate(Time);
                var OverPressure = WeatherParameters.Select(weather =>
                {
                    double p = 0;
                    try
                    {
                        p = Delta_p(
                            height,
                            machNumber,
                            GeometricalParameters.Length,
                            K_s,
                            weather);
                    }
                    catch { }
                    return new { Pressure = p, Weather = weather };
                }).MaxElemet(x => x.Pressure);
                lock (Locker)
                {
                    if (OverPressure.Pressure > 0)
                        SonicBoom.Add(new SonicBoomParameters(
                            Time,
                            height,
                            machNumber,
                            OverPressure.Pressure,
                            20 * Math.Log10(OverPressure.Pressure / 2E-5),
                            d_x(height, machNumber) + Ballistics.Distance.Interpolate(Time),
                            Delta_t(height, machNumber, GeometricalParameters.Length, K_s, OverPressure.Weather),
                            OverPressure.Weather.Mounth));
                    n++;
                    ProgressChanged((int)(100.0 * n / TimeList.Count));
                }            
            });
            return SonicBoom.OrderBy(x => x.Time).ToList();
        }

        public List<SonicBoomParameters> Calculate(
            SonicBoomCalculationInputData id,
            List<WeatherParameters> WeatherParameters) // см. метод с аналогичной сигнатурой в IModel
        {
            return Calculate(id.Ballistics, id.GeometricalParameters, WeatherParameters, ProgressChanged);
        }
        public void Calculate(
            SonicBoomCalculationInputData RocketID,
            SonicBoomCalculationInputData VehicleID,
            List<WeatherParameters> WeatherParameters,
            out List<SonicBoomParameters> RocketSonicBoomBoomParameters,
            out List<SonicBoomParameters> VehicleSonicBoomBoomParameters) // см. метод с аналогичной сигнатурой в IModel
        {
            List<SonicBoomParameters> _RocketSonicBoomBoomParameters = null;
            List<SonicBoomParameters> _VehicleSonicBoomBoomParameters = null;
            int RocketProgress = 0;
            int VehicleProgress = 0;
            object Locker = new object();
            Action ProgressChanges = () =>
            {
                lock (Locker)
                {
                    ProgressChanged(Math.Min(RocketProgress, VehicleProgress));
                }
            };
            var RocketTask = new Task(() => _RocketSonicBoomBoomParameters = Calculate(
                RocketID.Ballistics,
                RocketID.GeometricalParameters,
                WeatherParameters,
                Progress =>
                {
                    RocketProgress = Progress;
                    ProgressChanges();
                }));
            var VehicleTask = new Task(() => _VehicleSonicBoomBoomParameters = Calculate(
                VehicleID.Ballistics,
                VehicleID.GeometricalParameters,
                WeatherParameters,
                Progress =>
                {
                    VehicleProgress = Progress;
                    ProgressChanges();
                }));
            RocketTask.Start();
            VehicleTask.Start();
            System.Threading.Tasks.Task.WhenAll(RocketTask, VehicleTask).Wait();
            ProgressChanged(100);
            RocketSonicBoomBoomParameters = _RocketSonicBoomBoomParameters;
            VehicleSonicBoomBoomParameters = _VehicleSonicBoomBoomParameters;
        }
        public void Save(
            string FileName,
            SonicBoomCalculationInputData RocketID,
            SonicBoomCalculationInputData VehicleID,
            List<WeatherParameters> WeatherParameters) // см. метод с аналогичной сигнатурой в IModel
        {
            var xmlDoc = new XmlDocument();
            var Root = xmlDoc.CreateElement("SonicBoomInputData");
            Action<XmlNode, string, double> AddAttribute = (node, name, value) =>
            {
                var attribute = xmlDoc.CreateAttribute(name);
                attribute.Value = value.ToString();
                node.Attributes.Append(attribute);
            };
            Func<string, SonicBoomBallistics, XmlNode> BallisticsNode = (Name, Ballistics) =>
            {
                var ballisticsNode = xmlDoc.CreateElement(Name);
                for (int i = 0; i < Ballistics.Distance.X.Length; i++)
                {
                    var node = xmlDoc.CreateElement("Moment");
                    AddAttribute(node, "Time", Ballistics.Height.X[i]);
                    AddAttribute(node, "MachNumber", Ballistics.MachNumber.Y[i]);
                    AddAttribute(node, "Distance", Ballistics.Distance.Y[i]);
                    AddAttribute(node, "Height", Ballistics.Height.Y[i]);
                    ballisticsNode.AppendChild(node);
                }
                return ballisticsNode;
            };
            Func<string, GeometricalParameters, XmlNode> GeometricalParametersNode = (Name, Parameters) =>
            {
                var flowParametersNode = xmlDoc.CreateElement(Name);
                AddAttribute(flowParametersNode, "Length", Parameters.Length);
                AddAttribute(flowParametersNode, "CharacteristicLength", Parameters.CharacteristicLength);
                AddAttribute(flowParametersNode, "MaximalArea", Parameters.MaximalArea);
                AddAttribute(flowParametersNode, "CharacteristicArea", Parameters.CharacteristicArea);
                return flowParametersNode;
            };
            Func<XmlNode> WeatherParametersNode = () =>
            {
                var weatherParametersNode = xmlDoc.CreateElement("WeatherParameters");
                foreach (var weatherParameters in WeatherParameters)
                {
                    var WeatherNode = xmlDoc.CreateElement(weatherParameters.Mounth);
                    AddAttribute(WeatherNode, "Humidity", weatherParameters.Humidity);
                    AddAttribute(WeatherNode, "Temperature", weatherParameters.Temperature);
                    weatherParametersNode.AppendChild(WeatherNode);
                }
                return weatherParametersNode;
            };
            Root.AppendChild(BallisticsNode("RocketBallisticsPath", RocketID.Ballistics));
            Root.AppendChild(BallisticsNode("VehicleBallisticsPath", VehicleID.Ballistics));
            Root.AppendChild(GeometricalParametersNode("RocketGeometricalParameters", RocketID.GeometricalParameters));
            Root.AppendChild(GeometricalParametersNode("VehicleGeometricalParameters", VehicleID.GeometricalParameters));
            Root.AppendChild(WeatherParametersNode());
            xmlDoc.AppendChild(Root);
            xmlDoc.Save(FileName);
        }
        public void Open(
            XmlDocument xmlDoc,
            out SonicBoomCalculationInputData RocketID,
            out SonicBoomCalculationInputData VehicleID,
            out List<WeatherParameters> WeatherParameters) // см. метод с аналогичной сигнатурой в IModel
        {
            Func<XmlNode, SonicBoomBallistics> GetBallistics = Node =>
            {
                List<double> Time = new List<double>();
                List<double> Mach = new List<double>();
                List<double> Distance = new List<double>();
                List<double> Height = new List<double>();
                for (int i = 0; i < Node.ChildNodes.Count; i++)
                {
                    var node = Node.ChildNodes[i];
                    Time.Add(Convert.ToDouble(node.Attributes["Time"].Value));
                    Distance.Add(Convert.ToDouble(node.Attributes["Distance"].Value));
                    Height.Add(Convert.ToDouble(node.Attributes["Height"].Value));
                    Mach.Add(Convert.ToDouble(node.Attributes["MachNumber"].Value));
                }
                return new SonicBoomBallistics(
                    new Interpolation(Time.ToArray(), Height.ToArray()),
                    new Interpolation(Time.ToArray(), Distance.ToArray()),
                    new Interpolation(Time.ToArray(), Mach.ToArray()));
            };
            Func<XmlNode, GeometricalParameters> GetGeometricalParameters = Node => new GeometricalParameters(
                Convert.ToDouble(Node.Attributes["Length"].Value),
                Convert.ToDouble(Node.Attributes["CharacteristicLength"].Value),
                Convert.ToDouble(Node.Attributes["MaximalArea"].Value),
                Convert.ToDouble(Node.Attributes["CharacteristicArea"].Value));
            Func<XmlNode, List<WeatherParameters>> GetWeatherParameters = Node => Node.ChildNodes.Cast<XmlNode>().Select(x => new WeatherParameters(
                x.Name,
                Convert.ToDouble(x.Attributes["Humidity"].Value),
                Convert.ToDouble(x.Attributes["Temperature"].Value))).ToList();
            var Root = xmlDoc.ChildNodes[0];
            Func<XmlNode, string, XmlNode> FindNode = (Node, ChildNodeName) => Node.ChildNodes.Cast<XmlNode>().FirstOrDefault(x => x.Name == ChildNodeName);
            RocketID = new SonicBoomCalculationInputData(GetBallistics(FindNode(Root, "RocketBallisticsPath")), GetGeometricalParameters(FindNode(Root, "RocketGeometricalParameters")));
            VehicleID = new SonicBoomCalculationInputData(GetBallistics(FindNode(Root, "VehicleBallisticsPath")), GetGeometricalParameters(FindNode(Root, "VehicleGeometricalParameters")));
            WeatherParameters = GetWeatherParameters(FindNode(Root, "WeatherParameters"));
        }
        public void Open(
            string FileName,
            out SonicBoomCalculationInputData RocketID,
            out SonicBoomCalculationInputData VehicleID,
            out List<WeatherParameters> WeatherParameters) // см. метод с аналогичной сигнатурой в IModel
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(FileName);
            Open(xmlDoc, out RocketID, out VehicleID, out WeatherParameters);
        }
        public event Action<int> ProgressChanged;
    }
}
