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
    class EngineNoiseModel
    {
        public EngineSoundLevel GetEngineSoundLevel(double Thrust, FlowParameters FlowParameters) // см. метод с аналогичной сигнатурой в IModel
        {
            var EngineSoundLevelAtFrequency = GetEngineSoundLevelAtFrequency(Thrust, FlowParameters);
            return (Radius, Angle) => 10 * Math.Log10(EngineSoundLevelAtFrequency(Radius, Angle)
                .Select(x => Math.Pow(10, 0.1 * x.Value))
                .Sum());
        }
        public EngineSoundLevelAtFrequency GetEngineSoundLevelAtFrequency(double Thrust, FlowParameters FlowParameters) // см. метод с аналогичной сигнатурой в IModel
        {
            double[] ExpandedFrequencies, ExpandedFrequenciesBand;
            FrequenciesAggregator.Expanded(out ExpandedFrequencies, out ExpandedFrequenciesBand);
            double[] spectralDecomposition = SpectralDecomposition.Get(1.01325E5, FlowParameters, ExpandedFrequencies, ExpandedFrequenciesBand);
            double L0 = 10 * Math.Log10(0.5 * 0.0022 * Thrust * FlowParameters.NozzleFlowVelocity) + 120 - 11;
            return (Radius, Angle) =>
            {
                var L = L0 - 20 * Math.Log10(Radius) + DI.Interpolate(Angle * 180 / Math.PI);
                var Result = new Dictionary<double, double>();
                for (int i = 0; i < ExpandedFrequencies.Length; i++)
                    Result.Add(ExpandedFrequencies[i], L + spectralDecomposition[i]);
                return Result;
            };
        }
        public FlowSoundParameters GetFlowSoundParameters(double Thrust, FlowParameters flowParameters) // см. метод с аналогичной сигнатурой в IModel
        {
            double OverExpandedDiameter = 2 * 1 / flowParameters.NozzleMachNumber * Math.Sqrt(
                flowParameters.MassFlow * flowParameters.NozzleFlowVelocity /
                (Math.PI * 1.0125E5 * flowParameters.NozzleAdiabaticIndex));
            double UndisturbedSupersonicFlowLength = 1.75 * OverExpandedDiameter *
                Math.Pow(1 + 0.38 * flowParameters.NozzleMachNumber, 2);
            double SupersonicFlowLength = OverExpandedDiameter *
                (5 * Math.Pow(flowParameters.NozzleMachNumber, 1.8) + 0.8);
            double DistanceToPointdOfMaximalSoundLevel = 1.5 * UndisturbedSupersonicFlowLength;
            double _98ProzentSoundPowerRadiatingFlowLength = 5 * UndisturbedSupersonicFlowLength;
            double DistanceToPointOfFlowDestruction = 8.38 * UndisturbedSupersonicFlowLength;
            double MechanicalPower = 0.5 * Thrust * flowParameters.NozzleFlowVelocity;
            double SoundPowerRatio = 0.22;
            double SoundPower = MechanicalPower * SoundPowerRatio / 100;
            double SoundMaximalPowerConeHalfAngle = 30;
            return new FlowSoundParameters(
                MechanicalPower,
                SoundPower,
                SoundPowerRatio,
                SoundMaximalPowerConeHalfAngle,
                UndisturbedSupersonicFlowLength,
                DistanceToPointdOfMaximalSoundLevel,
                SupersonicFlowLength,
                _98ProzentSoundPowerRadiatingFlowLength,
                DistanceToPointOfFlowDestruction);
        }
        public void Calculate(
            double Thrust,
            FlowParameters FlowParameters,
            out EngineAcousticsLoadSummary summary) // см. метод с аналогичной сигнатурой в IModel
        {
            var FlowSoundParameters = GetFlowSoundParameters(Thrust, FlowParameters);
            var EngineSoundLevelAtFrequency = GetEngineSoundLevelAtFrequency(Thrust, FlowParameters);
            var engineSoundLevel = GetEngineSoundLevel(Thrust, FlowParameters);
            var FrequencyCharacteristik = EngineSoundLevelAtFrequency(1, Math.PI / 2);
            var Angles = new List<double>();
            for (int i = 0; i <= 180; i += 10)
                Angles.Add(i);
            var RadiationPattern = Angles
                .Select(x => new { Angle = x, SoundLevel = engineSoundLevel(1, x * Math.PI / 180) })
                .ToDictionary(x => x.Angle, x => x.SoundLevel);
            var EngineAcousticsLoadAtFrequency = EngineSoundLevelAtFrequency(FlowSoundParameters.DistanceToPointOfMaximalSoundLevel, Math.PI);
            var EngineAcousticsLoadSummary = engineSoundLevel(FlowSoundParameters.DistanceToPointOfMaximalSoundLevel, Math.PI);
            summary = new EngineAcousticsLoadSummary(FlowSoundParameters, FrequencyCharacteristik, RadiationPattern, EngineAcousticsLoadAtFrequency, EngineAcousticsLoadSummary);
        }
        public void Calculate(
            double Thrust,
            FlowParameters FlowParameters,
            EngineSoundContourParameters ContourParameters,
            out EngineSoundContour EngineSoundContour) // см. метод с аналогичной сигнатурой в IModel
        {
            var FlowSoundParameters = GetFlowSoundParameters(Thrust, FlowParameters);
            var EngineSoundLevel = GetEngineSoundLevel(Thrust, FlowParameters);
            int Nx, Ny, W, H;
            if (ContourParameters.ContourAreaWidth >= ContourParameters.ContourAreaHeight)
            {
                Ny = 51;
                H = 1000;
                Nx = (int)(Ny * ContourParameters.ContourAreaWidth / ContourParameters.ContourAreaHeight);
                W = (int)(H * ContourParameters.ContourAreaWidth / ContourParameters.ContourAreaHeight);
            }
            else
            {
                Nx = 51;
                W = 1000;
                Ny = (int)(Nx * ContourParameters.ContourAreaHeight / ContourParameters.ContourAreaWidth);
                H = (int)(W * ContourParameters.ContourAreaHeight / ContourParameters.ContourAreaWidth);
            }
            var X = new double[Nx];
            var Y = new double[Ny];
            double dX = ContourParameters.ContourAreaWidth / (Nx - 1);
            double dY = ContourParameters.ContourAreaHeight / (Ny - 1);
            for (int i = 0; i < Nx; i++)
                X[i] = i * dX;
            for (int i = 0; i < Ny; i++)
                Y[i] = i * dY;
            var SoundLevels = new double[Nx, Ny];
            for (int i = 0; i < Nx; i++)
            {
                double x = X[i] - FlowSoundParameters.DistanceToPointOfMaximalSoundLevel - ContourParameters.NozzleCoordinate;
                x = Math.Abs(x) < 0.001 ? 0.001 : x;
                for (int j = 0; j < Ny; j++)
                {
                    double y = Math.Abs(Y[j] - ContourParameters.ContourAreaHeight / 2);
                    double Angle;
                    if (x == 0)
                        Angle = Math.Sign(y) * Math.PI / 2;
                    else
                    {
                        Angle = Math.Atan(y / Math.Abs(x));
                        if (x < 0)
                            Angle = Math.PI - Angle;
                    }
                    double Radius = Math.Sqrt(x * x + y * y);
                    SoundLevels[i, j] = EngineSoundLevel(Radius, Angle);
                }
            }
            var Contour = new Bitmap(W, H);
            EngineSoundContour = new EngineSoundContour(X, Y, SoundLevels, Contour);
            ModifyEngineSoundContour(ContourParameters.MinSoundLevel, ContourParameters.MaxSoundLevel, ref EngineSoundContour);
        }
        public void ModifyEngineSoundContour(
            double MinSoundLevel,
            double MaxSoundLevel,
            ref EngineSoundContour EngineSoundContour) // см. метод с аналогичной сигнатурой в IModel
        {
            int ImageWidth = EngineSoundContour.Contour.Width;
            int ImageHeight = EngineSoundContour.Contour.Height;
            double[] X = EngineSoundContour.X;
            double[] Y = EngineSoundContour.Y;
            double[,] SoundLevels = EngineSoundContour.SoundLevels;
            var Contour = new Bitmap(ImageWidth, ImageHeight);
            using (var g = Graphics.FromImage(Contour))
            {
                int gNx = 300, gNy = 300;
                double mux = X.Max() / ImageWidth;
                double muy = Y.Max() / ImageHeight;
                int dx = ImageWidth / gNx;
                int dy = ImageHeight / gNy;
                double Max = SoundLevels[0, 0];
                double Min = Max;
                var Interpolator = new DoubleInterpolation(X, Y, SoundLevels);
                for (int i = 0; i < SoundLevels.GetLength(0); i++)
                    for (int j = 0; j < SoundLevels.GetLength(1); j++)
                        if (SoundLevels[i, j] > Max)
                            Max = SoundLevels[i, j];
                        else if (SoundLevels[i, j] < Min)
                            Min = SoundLevels[i, j];
                var Intervals = new double[] { 0, 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0, 11.0 };
                var R = new Interpolation(Intervals, new double[] { 0, 0, 0, 0, 0, 0, 124, 203, 255, 255, 255, 255 });
                var G = new Interpolation(Intervals, new double[] { 0, 124, 203, 255, 255, 255, 255, 255, 255, 203, 124, 0 });
                var B = new Interpolation(Intervals, new double[] { 255, 255, 255, 255, 203, 124, 0, 0, 0, 0, 0, 0 });
                Func<double, Color> GetColor = x =>
                {
                    x = (x - MinSoundLevel) / (MaxSoundLevel - MinSoundLevel);
                    return Color.FromArgb(
                        (int)R.Interpolate(x * 11),
                        (int)G.Interpolate(x * 11),
                        (int)B.Interpolate(x * 11));
                };
                for (int i = 0; i < gNx; i++)
                {
                    ProgressChanged((int)(i * 100.0 / gNx));
                    int x = i * ImageWidth / gNx;
                    for (int j = 0; j < gNy; j++)
                    {
                        int y = j * ImageHeight / gNy;
                        var color = GetColor(Interpolator.Interpolate(x * mux, y * muy));
                        using (var Brush = new SolidBrush(color))
                        {
                            g.FillRectangle(Brush, x, y, i * dx, i * dy);
                        }
                    }
                }
            }
            ProgressChanged(100);
            EngineSoundContour = new EngineSoundContour(X, Y, SoundLevels, Contour);
        }
        public void Save(
            string FileName,
            double Thrust,
            FlowParameters FlowParameters,
            EngineSoundContourParameters ContourParameters) // см. метод с аналогичной сигнатурой в IModel
        {
            var xmlDoc = new XmlDocument();
            var Root = xmlDoc.CreateElement("EngineNoiseInputData");
            Action<XmlNode, string, double> AddAttribute = (node, name, value) =>
            {
                var attribute = xmlDoc.CreateAttribute(name);
                attribute.Value = value.ToString();
                node.Attributes.Append(attribute);
            };
            var thrustNode = xmlDoc.CreateElement("Thrust");
            thrustNode.InnerText = Thrust.ToString();
            Root.AppendChild(thrustNode);
            var flowParametersNode = xmlDoc.CreateElement("FlowParameters");
            AddAttribute(flowParametersNode, "MassFlow", FlowParameters.MassFlow);
            AddAttribute(flowParametersNode, "NozzleDiameter", FlowParameters.NozzleDiameter);
            AddAttribute(flowParametersNode, "NozzleMachNumber", FlowParameters.NozzleMachNumber);
            AddAttribute(flowParametersNode, "NozzleFlowVelocity", FlowParameters.NozzleFlowVelocity);
            AddAttribute(flowParametersNode, "ChamberSoundVelocity", FlowParameters.ChamberSoundVelocity);
            AddAttribute(flowParametersNode, "NozzleAdiabaticIndex", FlowParameters.NozzleAdiabaticIndex);
            Root.AppendChild(flowParametersNode);
            var contourParanetersNode = xmlDoc.CreateElement("ContourParaneters");
            AddAttribute(contourParanetersNode, "Width", ContourParameters.ContourAreaWidth);
            AddAttribute(contourParanetersNode, "Height", ContourParameters.ContourAreaHeight);
            AddAttribute(contourParanetersNode, "NozzleCoordinate", ContourParameters.NozzleCoordinate);
            AddAttribute(contourParanetersNode, "MinSoundLevel", ContourParameters.MinSoundLevel);
            AddAttribute(contourParanetersNode, "MaxSoundLevel", ContourParameters.MaxSoundLevel);
            Root.AppendChild(contourParanetersNode);
            xmlDoc.AppendChild(Root);
            xmlDoc.Save(FileName);
        }
        public void Open(
            XmlDocument xmlDoc,
            out double Thrust,
            out FlowParameters FlowParameters,
            out EngineSoundContourParameters ContourParameters) // см. метод с аналогичной сигнатурой в IModel
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
            var Root = xmlDoc.ChildNodes[0];
            Func<XmlNode, string, XmlNode> FindNode = (Node, ChildNodeName) => Node.ChildNodes.Cast<XmlNode>().FirstOrDefault(x => x.Name == ChildNodeName);
            Thrust = GetThrust(FindNode(Root, "Thrust"));
            FlowParameters = GetFlowParameters(FindNode(Root, "FlowParameters"));
            var contourParanetersNode = FindNode(Root, "ContourParaneters");
            ContourParameters = new EngineSoundContourParameters(
                Convert.ToInt32(contourParanetersNode.Attributes["Width"].Value),
                Convert.ToInt32(contourParanetersNode.Attributes["Height"].Value),
                Convert.ToInt32(contourParanetersNode.Attributes["NozzleCoordinate"].Value),
                Convert.ToInt32(contourParanetersNode.Attributes["MinSoundLevel"].Value),
                Convert.ToInt32(contourParanetersNode.Attributes["MaxSoundLevel"].Value));
        }
        public void Open(
            string FileName,
            out double Thrust,
            out FlowParameters FlowParameters,
            out EngineSoundContourParameters ContourParameters) // см. метод с аналогичной сигнатурой в IModel
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(FileName);
            Open(FileName, out Thrust, out FlowParameters, out ContourParameters);
        }
        public event Action<int> ProgressChanged;
    }
}