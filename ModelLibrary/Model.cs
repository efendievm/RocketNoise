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
    public class Model : IModel // класс, реалиpующий интерфейс IModel
    {
        FlightNoiseModel flightNoiseModel = new FlightNoiseModel();
        FireTestNoiseModel fireTestNoiseModel = new FireTestNoiseModel();
        EngineNoiseModel engineNoiseModel = new EngineNoiseModel();
        SonicBoomModel sonicBoomModel = new SonicBoomModel();
        public Model()
        {
            flightNoiseModel.ProgressChanged += Progress => FlightNoiseCalculationProgressChanged(Progress);
            fireTestNoiseModel.ProgressChanged += Progress => FireTestNoiseCalculationProgressChanged(Progress);
            engineNoiseModel.ProgressChanged += Progress => EngineNoiseCalculationProgressChanged(Progress);
            sonicBoomModel.ProgressChanged += Progress => SonicBoomCalculationProgressChanged(Progress);
        }
        public FlightSoundLevel GetFlightSoundLevel(Ballistics ballistics, FlowParameters flowParameters, FrequencyBand frequencyBand)
        {
            return flightNoiseModel.GetFlightSoundLevel(ballistics, flowParameters, frequencyBand);
        }
        public StaticSoundLevel GetStaticSoundLevel(double Thrust, FlowParameters flowParameters, FrequencyBand frequencyBand)
        {
            return fireTestNoiseModel.GetStaticSoundLevel(Thrust, flowParameters, frequencyBand);
        }
        public EngineSoundLevel GetEngineSoundLevel(double Thrust, FlowParameters FlowParameters)
        {
            return engineNoiseModel.GetEngineSoundLevel(Thrust, FlowParameters);
        }
        public EngineSoundLevelAtFrequency GetEngineSoundLevelAtFrequency(double Thrust, FlowParameters FlowParameters)
        {
            return engineNoiseModel.GetEngineSoundLevelAtFrequency(Thrust, FlowParameters);
        }
        public List<EffectiveFlightSound> CalculateFlightSound(FlightSoundCalculationInputData id)
        {
            return flightNoiseModel.Calculate(id);
        }
        public List<FlightSoundCircles> CalculateFlightSound(
            FlightSoundCalculationInputData id,
            List<double> SoundLevels,
            RadiusInterval RadiusInterval,
            FrequencyBand FrequencyBand)
        {
            return flightNoiseModel.Calculate(id, SoundLevels, RadiusInterval, FrequencyBand);
        }
        public void CalculateFlightSound(
            FlightSoundCalculationInputData id,
            List<double> SoundLevels,
            RadiusInterval RadiusInterval,
            out List<EffectiveFlightSound> EffectiveFlightSounds,
            out List<FlightSoundCircles> FlightSoundCircles)
        {
            flightNoiseModel.Calculate(id, SoundLevels, RadiusInterval, out EffectiveFlightSounds, out FlightSoundCircles);
        }
        public List<FireTestSoundContour> CalculateFireTestNoise(FireTestNoiseCalculationInputData id, List<double> SoundLevels)
        {
            return fireTestNoiseModel.Calculate(id, SoundLevels);
        }
        public FlowSoundParameters GetFlowSoundParameters(double Thrust, FlowParameters flowParameters)
        {
            return engineNoiseModel.GetFlowSoundParameters(Thrust, flowParameters);
        }
        public void CalculateEngineAcousticsLoadSummary(double Thrust, FlowParameters FlowParameters, out EngineAcousticsLoadSummary summary)
        {
            engineNoiseModel.Calculate(Thrust, FlowParameters, out summary);
        }
        public void CalculateEngineSoundContour(double Thrust, FlowParameters FlowParameters, EngineSoundContourParameters ContourParameters, out EngineSoundContour EngineSoundContour)
        {
            engineNoiseModel.Calculate(Thrust, FlowParameters, ContourParameters, out EngineSoundContour);
        }
        public void ModifyEngineSoundContour(double MinSoundLevel, double MaxSoundLevel, ref EngineSoundContour EngineSoundContour)
        {
            engineNoiseModel.ModifyEngineSoundContour(MinSoundLevel, MaxSoundLevel, ref EngineSoundContour);
        }
        public List<SonicBoomParameters> CalculateSonicBoom(SonicBoomCalculationInputData id, List<WeatherParameters> WeatherParameters)
        {
            return sonicBoomModel.Calculate(id, WeatherParameters);
        }
        public void CalculateSonicBoom(
            SonicBoomCalculationInputData RocketID,
            SonicBoomCalculationInputData VehicleID,
            List<WeatherParameters> WeatherParameters,
            out List<SonicBoomParameters> RocketSonicBoomBoomParameters,
            out List<SonicBoomParameters> VehicleSonicBoomBoomParameters)
        {
            sonicBoomModel.Calculate(RocketID, VehicleID, WeatherParameters, out RocketSonicBoomBoomParameters, out VehicleSonicBoomBoomParameters);
        }
        public void SaveFlightNoiseInputData(
            string FileName,
            FlightSoundCalculationInputData id,
            Dictionary<double, Color> SoundLevels,
            RadiusInterval RadiusInterval,
            FrequencyBand FrequencyBand)
        {
            flightNoiseModel.Save(FileName, id, SoundLevels, RadiusInterval, FrequencyBand);
        }
        public void OpenFlightNoiseInputData(
            string FileName,
            out FlightSoundCalculationInputData id,
            out Dictionary<double, Color> SoundLevels,
            out RadiusInterval RadiusInterval,
            out FrequencyBand FrequencyBand)
        {
            flightNoiseModel.Open(FileName, out id, out SoundLevels, out RadiusInterval, out FrequencyBand);
        }
        public void OpenFlightNoiseInputData // Метод, пределяющий исходные данные для расчёта уровня шума в полёте из файла
        (
            XmlDocument xmlDoc, // Файл .xml и исходными данными
            out FlightSoundCalculationInputData id, // Переменная, в которую записываются основные исходные данные
            out Dictionary<double, Color> SoundLevels, // Переменная, в которую записывается словарь, ключи которого -- уровни шума в слышимом диапазоне, значения -- цвет отображения геометрического места точек
            out RadiusInterval RadiusInterval, // Область расчёта геометрическое место точек, в которых уровни шума в слышиомом диапазоне равны ключам словаря SoundLevels
            out FrequencyBand FrequencyBand // Спектр частот
        )
        {
            flightNoiseModel.Open(xmlDoc, out id, out SoundLevels, out RadiusInterval, out FrequencyBand);
        }
        public void SaveFireTestNoiseInputData(string FileName, FireTestNoiseCalculationInputData id, Dictionary<double, Color> SoundLevels)
        {
            fireTestNoiseModel.Save(FileName, id, SoundLevels);
        }
        public void OpenFireTestNoiseInputData(string FileName, out FireTestNoiseCalculationInputData id, out Dictionary<double, Color> SoundLevels)
        {
            fireTestNoiseModel.Open(FileName, out id, out SoundLevels);
        }
        public void OpenFireTestNoiseInputData(XmlDocument xmlDoc, out FireTestNoiseCalculationInputData id, out Dictionary<double, Color> SoundLevels)
        {
            fireTestNoiseModel.Open(xmlDoc, out id, out SoundLevels);
        }
        public void SaveEngineNoiseInputData(
            string FileName,
            double Thrust,
            FlowParameters FlowParameters,
            EngineSoundContourParameters ContourParameters)
        {
            engineNoiseModel.Save(FileName, Thrust, FlowParameters, ContourParameters);
        }
        public void OpenEngineNoiseInputData(
            string FileName,
            out double Thrust,
            out FlowParameters FlowParameters,
            out EngineSoundContourParameters ContourParameters)
        {
            engineNoiseModel.Open(FileName, out Thrust, out FlowParameters, out ContourParameters);
        }
        public void OpenEngineNoiseInputData(
           XmlDocument xmlDoc,
           out double Thrust,
           out FlowParameters FlowParameters,
           out EngineSoundContourParameters ContourParameters)
        {
            engineNoiseModel.Open(xmlDoc, out Thrust, out FlowParameters, out ContourParameters);
        }
        public void SaveSonicBoomInputData(
            string FileName,
            SonicBoomCalculationInputData RocketID,
            SonicBoomCalculationInputData VehicleID,
            List<WeatherParameters> WeatherParameters)
        {
            sonicBoomModel.Save(FileName, RocketID, VehicleID, WeatherParameters);
        }
        public void OpenSonicBoomInputData(
            string FileName,
            out SonicBoomCalculationInputData RocketID,
            out SonicBoomCalculationInputData VehicleID,
            out List<WeatherParameters> WeatherParameters)
        {
            sonicBoomModel.Open(FileName, out RocketID, out VehicleID, out WeatherParameters);
        }
        public void OpenSonicBoomInputData(
            XmlDocument xmlDoc,
            out SonicBoomCalculationInputData RocketID,
            out SonicBoomCalculationInputData VehicleID,
            out List<WeatherParameters> WeatherParameters)
        {
            sonicBoomModel.Open(xmlDoc, out RocketID, out VehicleID, out WeatherParameters);
        }
        public event Action<int> FlightNoiseCalculationProgressChanged;
        public event Action<int> FireTestNoiseCalculationProgressChanged;
        public event Action<int> EngineNoiseCalculationProgressChanged;
        public event Action<int> SonicBoomCalculationProgressChanged;
    }
}