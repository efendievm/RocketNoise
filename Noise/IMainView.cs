using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypesLibrary;
using System.Drawing;

namespace MainViewInterface
{
    public interface IMainView
    {
        event Action<OpenFlightNoiseInputDataEventArgs> OpenFlightNoiseInputData;
        event Action<SaveFlightNoiseInputDataEventArgs> SaveFlightNoiseInputData;
        event Action<CalculateFlightNoiseEventArgs> CalculateFlightNoise;
        event Action<ApplyFlightNoisePartialInputDataEventArgs> ApplyFlightNoisePartialInputData;
        event Action<List<WeatherParameters>> ApplyFlightNoiseWeatherParameters;
        event Action<List<double>> ApplyFlightNoiseSoundLevels;
        event Action<FlightNoiseExampleEventArgs> FlightNoiseExample;
        void SetFlightNoiseCalculationProgress(int Progress);

        event Action<OpenFireTestNoiseInputDataEventArgs> OpenFireTestNoiseInputData;
        event Action<SaveFireTestNoiseInputDataEventArgs> SaveFireTestNoiseInputData;
        event Action<CalculateFireTestNoiseEventArgs> CalculateFireTestNoise;
        event Action<ApplyEngineParametersEventArgs> ApplyFireTestNoisePartialInputData;
        event Action<List<WeatherParameters>> ApplyFireTestNoiseWeatherParameters;
        event Action<List<double>> ApplyFireTestNoiseSoundLevels;
        event Action<FireTestNoiseExampleEventArgs> FireTestNoiseExample;
        void SetFireTestNoiseCalculationlProgress(int Progress);

        event Action<OpenEngineNoiseInputDataEventArgs> OpenEngineNoiseInputData;
        event Action<SaveEngineNoiseInputDataEventArgs> SaveEngineNoiseInputData;
        event Action<CalculateEngineNoiseEventArgs> CalculateEngineNoise;
        event Action<ApplyEngineParametersEventArgs> ApplyEngineNoisePartialInputData;
        event Action<ApplyEngineNoiseContourParametersEventArgs> ApplyEngineNoiseContourParameters;
        event Action<EngineNoiseExampleEventArgs> EngineNoiseExample;
        void SetEngineNoiseCalculationlProgress(int Progress);

        event Action<OpenSonicBoomInputDataEventArgs> OpenSonicBoomInputData;
        event Action<SaveSonicBoomInputDataEventArgs> SaveSonicBoomInputData;
        event Action<CalculateSonicBoomEventArgs> CalculateSonicBoom;
        event Action<SonicBoomExampleEventArgs> SonicBoomExample;
        void SetSonicBoomCalculationProgress(int Progress);
    }
    public enum Part { Rocket, Vehicle }
    public struct FlightNoiseInputData
    {
        public RocketBallistics RocketBallistics { get; set; }
        public VehicleBallistics VehicleBallistics { get; set; }
        public FlowParameters RocketFlowParameters { get; set; }
        public FlowParameters VehicleFlowParameters { get; set; }
        public List<WeatherParameters> WeatherParameters { get; set; }
        public Dictionary<double, Color> SoundLevels { get; set; }
        public RadiusInterval RadiusInterval { get; set; }
        public FrequencyBand FrequencyBand { get; set; }
    }
    public class OpenFlightNoiseInputDataEventArgs : EventArgs
    {
        public string FileName { get; private set; }
        public FlightNoiseInputData InputData { get; set; }
        public OpenFlightNoiseInputDataEventArgs(string FileName)
        {
            this.FileName = FileName;
        }
    }
    public class FlightNoiseExampleEventArgs : EventArgs
    {
        public FrequencyBand FrequencyBand { get; private set; }
        public FlightNoiseInputData InputData { get; set; }
        public FlightNoiseExampleEventArgs(FrequencyBand FrequencyBand)
        {
            this.FrequencyBand = FrequencyBand;
        }
    }
    public class SaveFlightNoiseInputDataEventArgs : EventArgs
    {
        public string FileName { get; private set; }
        public RocketBallistics RocketBallistics { get; private set; }
        public VehicleBallistics VehicleBallistics { get; private set; }
        public FlowParameters RocketFlowParameters { get; private set; }
        public FlowParameters VehicleFlowParameters { get; private set; }
        public List<WeatherParameters> WeatherParameters { get; private set; }
        public Dictionary<double, Color> SoundLevels { get; private set; }
        public RadiusInterval RadiusInterval { get; private set; }
        public FrequencyBand FrequencyBand { get; private set; }
        public SaveFlightNoiseInputDataEventArgs(
            string FileName,
            RocketBallistics RocketBallistics,
            VehicleBallistics VehicleBallistics,
            FlowParameters RocketFlowParameters,
            FlowParameters VehicleFlowParameters,
            List<WeatherParameters> WeatherParameters,
            Dictionary<double, Color> SoundLevels,
            RadiusInterval RadiusInterval,
            FrequencyBand FrequencyBand)
        {
            this.FileName = FileName;
            this.RocketBallistics = RocketBallistics;
            this.VehicleBallistics = VehicleBallistics;
            this.RocketFlowParameters = RocketFlowParameters;
            this.VehicleFlowParameters = VehicleFlowParameters;
            this.WeatherParameters = WeatherParameters;
            this.SoundLevels = SoundLevels;
            this.RadiusInterval = RadiusInterval;
            this.FrequencyBand = FrequencyBand;
        }
    }
    public class ApplyFlightNoisePartialInputDataEventArgs : EventArgs
    {
        public Part Part { get; private set; }
        public Ballistics Ballistics { get; private set; }
        public FlowParameters FlowParameters { get; private set; }
        public ApplyFlightNoisePartialInputDataEventArgs(
            Part Part,
            Ballistics Ballistics,
            FlowParameters FlowParameters)
        {
            this.Part = Part;
            this.Ballistics = Ballistics;
            this.FlowParameters = FlowParameters;
        }
    }
    public class CalculateFlightNoiseEventArgs : EventArgs
    {
        public FrequencyBand FrequencyBand { get; private set; }
        public RadiusInterval RadiusInterval { get; private set; }
        public List<EffectiveFlightSound> EffectiveFlightSounds { get; set; }
        public List<FlightSoundCircles> FlightSoundCircles { get; set; }
        public CalculateFlightNoiseEventArgs(FrequencyBand FrequencyBand, RadiusInterval RadiusInterval)
        {
            this.FrequencyBand = FrequencyBand;
            this.RadiusInterval = RadiusInterval;
        }
    }

    public struct FireTestNoiseInputData
    {
        public double Thrust { get; set; }
        public FlowParameters FlowParameters { get; set; }
        public List<WeatherParameters> WeatherParameters { get; set; }
        public Dictionary<double, Color> SoundLevels { get; set; }
        public RadiusInterval RadiusInterval { get; set; }
        public FrequencyBand FrequencyBand { get; set; }
    }
    public class OpenFireTestNoiseInputDataEventArgs : EventArgs
    {
        public string FileName { get; private set; }
        public FireTestNoiseInputData FireTestNoiseInputData { get; set; }
        public OpenFireTestNoiseInputDataEventArgs(string FileName)
        {
            this.FileName = FileName;
        }
    }
    public class FireTestNoiseExampleEventArgs : EventArgs
    {
        public FrequencyBand FrequencyBand { get; private set; }
        public FireTestNoiseInputData InputData { get; set; }
        public FireTestNoiseExampleEventArgs(FrequencyBand FrequencyBand)
        {
            this.FrequencyBand = FrequencyBand;
        }
    }
    public class SaveFireTestNoiseInputDataEventArgs : EventArgs
    {
        public string FileName { get; private set; }
        public double Thrust { get; private set; }
        public FlowParameters FlowParameters { get; private set; }
        public List<WeatherParameters> WeatherParameters { get; private set; }
        public Dictionary<double, Color> SoundLevels { get; private set; }
        public RadiusInterval RadiusInterval { get; private set; }
        public FrequencyBand FrequencyBand { get; private set; }
        public SaveFireTestNoiseInputDataEventArgs(
            string FileName,
            double Thrust,
            FlowParameters FlowParameters,
            List<WeatherParameters> WeatherParameters,
            Dictionary<double, Color> SoundLevels,
            RadiusInterval RadiusInterval,
            FrequencyBand FrequencyBand)
        {
            this.FileName = FileName;
            this.Thrust = Thrust;
            this.FlowParameters = FlowParameters;
            this.WeatherParameters = WeatherParameters;
            this.SoundLevels = SoundLevels;
            this.RadiusInterval = RadiusInterval;
            this.FrequencyBand = FrequencyBand;
        }
    }
    public class ApplyEngineParametersEventArgs : EventArgs
    {
        public double Thrust { get; private set; }
        public FlowParameters FlowParameters { get; private set; }
        public ApplyEngineParametersEventArgs(
            double Thrust,
            FlowParameters FlowParameters)
        {
            this.Thrust = Thrust;
            this.FlowParameters = FlowParameters;
        }
    }
    public class CalculateFireTestNoiseEventArgs : EventArgs
    {
        public FrequencyBand FrequencyBand { get; private set; }
        public RadiusInterval RadiusInterval { get; private set; }
        public List<FireTestSoundContour> FireTestSoundContours { get; set; }
        public CalculateFireTestNoiseEventArgs(FrequencyBand FrequencyBand, RadiusInterval RadiusInterval)
        {
            this.FrequencyBand = FrequencyBand;
            this.RadiusInterval = RadiusInterval;
        }
    }

    public struct EngineNoiseInputData
    {
        public double Thrust { get; set; }
        public FlowParameters FlowParameters { get; set; }
        public double ContourAreaWidth { get; set; }
        public double ContourAreaHeight { get; set; }
        public double NozzleCoordinate { get; set; }
        public double MinSoundLevel { get; set; }
        public double MaxSoundLevel { get; set; }
    }
    public class OpenEngineNoiseInputDataEventArgs
    {
        public string FileName { get; private set; }
        public EngineNoiseInputData InputData { get; set; }
        public OpenEngineNoiseInputDataEventArgs(string FileName)
        {
            this.FileName = FileName;
        }
    }
    public class EngineNoiseExampleEventArgs
    {
        public EngineNoiseInputData InputData { get; set; }
    }
    public class SaveEngineNoiseInputDataEventArgs : EventArgs
    {
        public string FileName { get; private set; }
        public double Thrust { get; private set; }
        public FlowParameters FlowParameters { get; private set; }
        public double ContourAreaWidth { get; private set; }
        public double ContourAreaHeight { get; private set; }
        public double NozzleCoordinate { get; private set; }
        public double MinSoundLevel { get; private set; }
        public double MaxSoundLevel { get; private set; }
        public SaveEngineNoiseInputDataEventArgs(
            string FileName,
            double Thrust,
            FlowParameters FlowParameters,
            double ContourAreaWidth,
            double ContourAreaHeight,
            double NozzleCoordinate,
            double MinSoundLevel,
            double MaxSoundLevel)
        {
            this.FileName = FileName;
            this.Thrust = Thrust;
            this.FlowParameters = FlowParameters;
            this.ContourAreaWidth = ContourAreaWidth;
            this.ContourAreaHeight = ContourAreaHeight;
            this.NozzleCoordinate = NozzleCoordinate;
            this.MinSoundLevel = MinSoundLevel;
            this.MaxSoundLevel = MaxSoundLevel;
        }
    }
    public class ApplyEngineNoiseContourParametersEventArgs : EventArgs
    {
        public double ContourAreaWidth { get; private set; }
        public double ContourAreaHeight { get; private set; }
        public double NozzleCoordinate { get; private set; }
        public double MinSoundLevel { get; private set; }
        public double MaxSoundLevel { get; private set; }
        public ApplyEngineNoiseContourParametersEventArgs(
            double ContourAreaWidth,
            double ContourAreaHeight,
            double NozzleCoordinate,
            double MinSoundLevel,
            double MaxSoundLevel)
        {
            this.ContourAreaWidth = ContourAreaWidth;
            this.ContourAreaHeight = ContourAreaHeight;
            this.NozzleCoordinate = NozzleCoordinate;
            this.MinSoundLevel = MinSoundLevel;
            this.MaxSoundLevel = MaxSoundLevel;
        }
    }
    public class CalculateEngineNoiseEventArgs : EventArgs
    {
        public FlowSoundParameters FlowSoundParameters { get; set; }
        public Dictionary<double, double> FrequencyCharacteristik { get; set; }
        public Dictionary<double, double> RadiationPattern { get; set; }
        public Dictionary<double, double> EngineAcousticsLoadAtFrequency { get; set; }
        public double EngineAcousticsLoadSummary { get; set; }
        public EngineSoundContour EngineSoundContour { get; set; }
    }

    public struct SonicBoomInputData
    {
        public SonicBoomBallistics RocketBallistics { get; set; }
        public SonicBoomBallistics VehicleBallistics { get; set; }
        public GeometricalParameters RocketGeometricalParameters { get; set; }
        public GeometricalParameters VehicleGeometricalParameters { get; set; }
        public List<WeatherParameters> WeatherParameters { get; set; }
    }
    public class OpenSonicBoomInputDataEventArgs : EventArgs
    {
        public string FileName { get; private set; }
        public SonicBoomInputData InputData { get; set; }
        public OpenSonicBoomInputDataEventArgs(string FileName)
        {
            this.FileName = FileName;
        }
    }
    public class SonicBoomExampleEventArgs
    {
        public SonicBoomInputData InputData { get; set; }
    }
    public class SaveSonicBoomInputDataEventArgs : EventArgs
    {
        public string FileName { get; private set; }
        public SonicBoomBallistics RocketBallistics { get; private set; }
        public SonicBoomBallistics VehicleBallistics { get; private set; }
        public GeometricalParameters RocketGeometricalParameters { get; private set; }
        public GeometricalParameters VehicleGeometricalParameters { get; private set; }
        public List<WeatherParameters> WeatherParameters { get; private set; }
        public SaveSonicBoomInputDataEventArgs(
            string FileName,
            SonicBoomBallistics RocketBallistics,
            SonicBoomBallistics VehicleBallistics,
            GeometricalParameters RocketGeometricalParameters,
            GeometricalParameters VehicleGeometricalParameters,
            List<WeatherParameters> WeatherParameters)
        {
            this.FileName = FileName;
            this.RocketBallistics = RocketBallistics;
            this.VehicleBallistics = VehicleBallistics;
            this.RocketGeometricalParameters = RocketGeometricalParameters;
            this.VehicleGeometricalParameters = VehicleGeometricalParameters;
            this.WeatherParameters = WeatherParameters;
        }
    }
    public class ApplySonicBoomPartialInputDataEventArgs : EventArgs
    {
        public Part Part { get; private set; }
        public SonicBoomBallistics Ballistics { get; private set; }
        public GeometricalParameters GeometricalParameters { get; private set; }
        public ApplySonicBoomPartialInputDataEventArgs(
            Part Part,
            SonicBoomBallistics Ballistics,
            GeometricalParameters GeometricalParameters)
        {
            this.Part = Part;
            this.Ballistics = Ballistics;
            this.GeometricalParameters = GeometricalParameters;
        }
    }
    public class CalculateSonicBoomEventArgs : EventArgs
    {
        public GeometricalParameters RocketGeometricalParameters { get; private set; }
        public GeometricalParameters VehicleGeometricalParameters { get; private set; }
        public SonicBoomBallistics RocketBallistics { get; private set; }
        public SonicBoomBallistics VehicleBallistics { get; private set; }
        public List<WeatherParameters> WeatherParameters { get; private set; }
        public List<SonicBoomParameters> RocketSonicBoomParameters { get; set; }
        public List<SonicBoomParameters> VehicleSonicBoomParameters { get; set; }
        public CalculateSonicBoomEventArgs(
            GeometricalParameters RocketGeometricalParameters,
            GeometricalParameters VehicleGeometricalParameters,
            SonicBoomBallistics RocketBallistics,
            SonicBoomBallistics VehicleBallistics,
            List<WeatherParameters> WeatherParameters)
        {
            this.RocketGeometricalParameters = RocketGeometricalParameters;
            this.VehicleGeometricalParameters = VehicleGeometricalParameters;
            this.RocketBallistics = RocketBallistics;
            this.VehicleBallistics = VehicleBallistics;
            this.WeatherParameters = WeatherParameters;
        }
    }
}