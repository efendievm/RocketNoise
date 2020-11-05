using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModelLibrary;
using TypesLibrary;
using MainViewInterface;
using System.Drawing;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;
using System.IO;
using System.Xml;

namespace Noise
{
    public class Presenter
    {
        IMainView MainView;
        IModel Model;
        class CurrentFlightNoiseCalculation
        {
            public bool PartialInputDataChanged = false;
            public bool WeatherParametersChanged = false;
            public bool SoundLevelsChanged = false;
            public bool FrequencyBandChanged = false;
            public bool RadiusIntervalChanged = false;
            public bool FirstCalculation = true;
            public FrequencyBand FrequencyBand;            
            public RadiusInterval RadiusInterval = new RadiusInterval(0, 0, 0);            
            public RocketBallistics RocketBallistics = null;
            public VehicleBallistics VehicleBallistics = null;
            public FlowParameters RocketFlowParameters = new FlowParameters();
            public FlowParameters VehicleFlowParameters = new FlowParameters();
            public List<WeatherParameters> WeatherParameters = new List<WeatherParameters>();
            public List<double> SoundLevels = new List<double>();
            public List<EffectiveFlightSound> EffectiveFlightSounds = new List<EffectiveFlightSound>();
            public List<FlightSoundCircles> FlightSoundCircles = new List<FlightSoundCircles>();
        }
        class CurrentFireTestNoiseCalculation
        {
            public bool InputDataChanged = false;
            public bool FirstCalculation = true;
            public FrequencyBand FrequencyBand;
            public RadiusInterval RadiusInterval = new RadiusInterval(0, 0, 0);       
            public double Thrust = 0;
            public FlowParameters FlowParameters = new FlowParameters();
            public List<WeatherParameters> WeatherParameters = new List<WeatherParameters>();
            public List<double> SoundLevels = new List<double>();
            public List<FireTestSoundContour> FireTestSoundContours = new List<FireTestSoundContour>();
        }
        class CurrentEngineNoiseCalculation
        {
            public bool InputDataChanged = false;
            public bool ContourCoordinatesChanged = false;
            public bool ContourColorsChanged = false;
            public double Thrust = 0;
            public FlowParameters FlowParameters = new FlowParameters();
            public double ContourAreaWidth = 0;
            public double ContourAreaHeight = 0;
            public double NozzleCoordinate = 0;
            public double MinSoundLevel = 0;
            public double MaxSoundLevel = 0;
            public FlowSoundParameters FlowSoundParameters = new FlowSoundParameters();
            public Dictionary<double, double> FrequencyCharacteristik = new Dictionary<double,double>();
            public Dictionary<double, double> RadiationPattern = new Dictionary<double, double>();
            public Dictionary<double, double> EngineAcousticsLoadAtFrequency = new Dictionary<double, double>();
            public double EngineAcousticsLoadSummary = 0;
            public EngineSoundContour EngineSoundContour = new EngineSoundContour(new double[0], new double[0], new double[0, 0], new Bitmap(1, 1));
        }
        CurrentFlightNoiseCalculation flightNoiseCalculation = new CurrentFlightNoiseCalculation();
        CurrentFireTestNoiseCalculation fireTestNoiseCalculation = new CurrentFireTestNoiseCalculation();
        CurrentEngineNoiseCalculation engineNoiseCalculation = new CurrentEngineNoiseCalculation();
        public Presenter(IMainView MainView, IModel Model)
        {
            this.MainView = MainView;
            this.Model = Model;
            MainView.OpenFlightNoiseInputData += MainView_OpenFlightNoiseInputData;
            MainView.SaveFlightNoiseInputData += MainView_SaveFlightNoiseInputData;
            MainView.CalculateFlightNoise += MainView_CalculateFlightSound;
            MainView.ApplyFlightNoisePartialInputData += MainView_ApplyFlightNoisePartialInputData;
            MainView.ApplyFlightNoiseSoundLevels += MainView_ApplyFlightNoiseSoundLevels;
            MainView.ApplyFlightNoiseWeatherParameters += MainView_ApplyFlightNoiseWeatherParameters;
            MainView.FlightNoiseExample += MainView_FlightNoiseExample;
            Model.FlightNoiseCalculationProgressChanged += Model_FlightNoiseCalculationProgressChanged;

            MainView.OpenFireTestNoiseInputData += MainView_OpenFireTestNoiseInputData;
            MainView.SaveFireTestNoiseInputData += MainView_SaveFireTestNoiseInputData;
            MainView.CalculateFireTestNoise += MainView_CalculateFireTestSound;
            MainView.ApplyFireTestNoisePartialInputData += MainView_ApplyFireTestNoisePartialInputData;
            MainView.ApplyFireTestNoiseSoundLevels += MainView_ApplyFireTestNoiseSoundLevels;
            MainView.ApplyFireTestNoiseWeatherParameters += MainView_ApplyFireTestNoiseWeatherParameters;
            MainView.FireTestNoiseExample += MainView_FireTestNoiseExample;
            Model.FireTestNoiseCalculationProgressChanged += Model_FireTestNoiseCalculationProgressChanged;

            MainView.SaveEngineNoiseInputData += MainView_SaveEngineNoiseInputData;
            MainView.OpenEngineNoiseInputData += MainView_OpenEngineNoiseInputData;
            MainView.ApplyEngineNoisePartialInputData += MainView_ApplyEngineNoisePartialInputData;
            MainView.ApplyEngineNoiseContourParameters += MainView_ApplyEngineNoiseContourParameters;
            MainView.CalculateEngineNoise += MainView_CalculateEngineNoise;
            MainView.EngineNoiseExample += MainView_EngineNoiseExample;
            Model.EngineNoiseCalculationProgressChanged += Model_EngineNoiseCalculationProgressChanged;

            MainView.OpenSonicBoomInputData += MainView_OpenSonicBoomInputData;
            MainView.SaveSonicBoomInputData += MainView_SaveSonicBoomInputData;
            MainView.CalculateSonicBoom += MainView_CalculateSonicBoom;
            MainView.SonicBoomExample += MainView_SonicBoomExample;
            Model.SonicBoomCalculationProgressChanged += Model_SonicBoomCalculationProgressChanged;
        }
        //FlightNoiseCalculation
        void MainView_ApplyFlightNoiseWeatherParameters(List<WeatherParameters> WeatherParameters)
        {
            if ((WeatherParameters.Except(flightNoiseCalculation.WeatherParameters, new WeatherCompare()).Count() != 0) ||
                ((WeatherParameters != null) && (flightNoiseCalculation.WeatherParameters == null)) ||
                ((WeatherParameters != null) && (flightNoiseCalculation.WeatherParameters != null) && (WeatherParameters.Count != flightNoiseCalculation.WeatherParameters.Count)))
            {
                flightNoiseCalculation.WeatherParameters = WeatherParameters.Select(x => x).ToList();
                flightNoiseCalculation.WeatherParametersChanged = true;
            }
        }
        void MainView_ApplyFlightNoiseSoundLevels(List<double> SoundLevels)
        {
            if ((SoundLevels.Count != flightNoiseCalculation.SoundLevels.Count) || (SoundLevels.Except(flightNoiseCalculation.SoundLevels).Count() != 0))
            {
                flightNoiseCalculation.SoundLevels = SoundLevels.Select(x => x).ToList();
                flightNoiseCalculation.SoundLevelsChanged = true;
            }
        }
        void MainView_ApplyFlightNoisePartialInputData(ApplyFlightNoisePartialInputDataEventArgs e)
        {
            flightNoiseCalculation.PartialInputDataChanged = true;
            if (e.Part == Part.Rocket)
            {
                flightNoiseCalculation.RocketBallistics = (RocketBallistics)e.Ballistics;
                flightNoiseCalculation.RocketFlowParameters = e.FlowParameters;
            }
            else
            {
                flightNoiseCalculation.VehicleBallistics = (VehicleBallistics)e.Ballistics;
                flightNoiseCalculation.VehicleFlowParameters = e.FlowParameters;
            }
        }
        void MainView_CalculateFlightSound(CalculateFlightNoiseEventArgs e)
        {
            List<EffectiveFlightSound> EffectiveFlightSounds = flightNoiseCalculation.EffectiveFlightSounds;
            List<FlightSoundCircles> FlightSoundCircles = flightNoiseCalculation.FlightSoundCircles;
            if (flightNoiseCalculation.FirstCalculation)
            {
                flightNoiseCalculation.FrequencyBandChanged = true;
                flightNoiseCalculation.RadiusIntervalChanged = true;
                flightNoiseCalculation.FirstCalculation = false;
            }
            else
            {
                if ((flightNoiseCalculation.RadiusInterval.Initial != e.RadiusInterval.Initial) ||
                    (flightNoiseCalculation.RadiusInterval.Final != e.RadiusInterval.Final) ||
                    (flightNoiseCalculation.RadiusInterval.Step != e.RadiusInterval.Step))
                {
                    flightNoiseCalculation.RadiusIntervalChanged = true;
                }
                if (flightNoiseCalculation.FrequencyBand != e.FrequencyBand)
                    flightNoiseCalculation.FrequencyBandChanged = true;
            }
            flightNoiseCalculation.RadiusInterval = e.RadiusInterval;
            flightNoiseCalculation.FrequencyBand = e.FrequencyBand;
            var id = new FlightSoundCalculationInputData(
                flightNoiseCalculation.RocketBallistics,
                flightNoiseCalculation.VehicleBallistics,
                flightNoiseCalculation.RocketFlowParameters,
                flightNoiseCalculation.VehicleFlowParameters,
                flightNoiseCalculation.WeatherParameters);
            if (flightNoiseCalculation.PartialInputDataChanged || flightNoiseCalculation.WeatherParametersChanged)
            {
                if ((flightNoiseCalculation.SoundLevels.Count != 0) && (e.FrequencyBand == FrequencyBand.Normal))
                    Model.CalculateFlightSound(id, flightNoiseCalculation.SoundLevels, flightNoiseCalculation.RadiusInterval, out EffectiveFlightSounds, out FlightSoundCircles);
                else if ((flightNoiseCalculation.SoundLevels.Count == 0) && (e.FrequencyBand == FrequencyBand.Normal))
                    EffectiveFlightSounds = Model.CalculateFlightSound(id);
                else if ((flightNoiseCalculation.SoundLevels.Count != 0) && (e.FrequencyBand != FrequencyBand.Normal))
                    FlightSoundCircles = Model.CalculateFlightSound(id, flightNoiseCalculation.SoundLevels, flightNoiseCalculation.RadiusInterval, flightNoiseCalculation.FrequencyBand);
            }
            else if ((flightNoiseCalculation.FrequencyBandChanged || flightNoiseCalculation.RadiusIntervalChanged || flightNoiseCalculation.SoundLevelsChanged) &&
                     (flightNoiseCalculation.SoundLevels.Count != 0))
            {
                FlightSoundCircles = Model.CalculateFlightSound(id, flightNoiseCalculation.SoundLevels, flightNoiseCalculation.RadiusInterval, flightNoiseCalculation.FrequencyBand);
            }
            flightNoiseCalculation.PartialInputDataChanged = false;
            flightNoiseCalculation.WeatherParametersChanged = false;
            flightNoiseCalculation.SoundLevelsChanged = false;
            flightNoiseCalculation.FrequencyBandChanged = false;
            flightNoiseCalculation.RadiusIntervalChanged = false;
            flightNoiseCalculation.EffectiveFlightSounds = EffectiveFlightSounds;
            flightNoiseCalculation.FlightSoundCircles = FlightSoundCircles;
            e.EffectiveFlightSounds = EffectiveFlightSounds;
            e.FlightSoundCircles = FlightSoundCircles;
        }
        void MainView_SaveFlightNoiseInputData(SaveFlightNoiseInputDataEventArgs e)
        {
            Model.SaveFlightNoiseInputData(
                e.FileName, 
                new FlightSoundCalculationInputData(
                    e.RocketBallistics, 
                    e.VehicleBallistics,
                    e.RocketFlowParameters, 
                    e.VehicleFlowParameters,
                    e.WeatherParameters),
                e.SoundLevels,
                e.RadiusInterval,
                e.FrequencyBand);
        }
        void MainView_OpenFlightNoiseInputData(OpenFlightNoiseInputDataEventArgs e)
        {
            Model.OpenFlightNoiseInputData(
                e.FileName,
                out FlightSoundCalculationInputData id,
                out Dictionary<double, Color> soundLevels,
                out RadiusInterval radiusInterval,
                out FrequencyBand frequencyBand);
            e.InputData = new FlightNoiseInputData()
            {
                RocketBallistics = id.RocketBallistics,
                VehicleBallistics = id.VehicleBallistics,
                RocketFlowParameters = id.RocketFlowParameters,
                VehicleFlowParameters = id.VehicleFlowParameters,
                WeatherParameters = id.WeatherParameters,
                SoundLevels = soundLevels,
                RadiusInterval = radiusInterval,
                FrequencyBand = frequencyBand,
            };
        }
        void MainView_FlightNoiseExample(FlightNoiseExampleEventArgs e)
        {
            XmlDocument xmlDoc = new XmlDocument();
            if (e.FrequencyBand == FrequencyBand.Normal)
                xmlDoc.LoadXml(InputDataExampleResource.FlightNoise);
            else if (e.FrequencyBand == FrequencyBand.Infra)
                xmlDoc.LoadXml(InputDataExampleResource.FlightNoiseInfra);
            else
                xmlDoc.LoadXml(InputDataExampleResource.FlightNoiseUltra);
            Model.OpenFlightNoiseInputData(
                xmlDoc,
                out FlightSoundCalculationInputData id,
                out Dictionary<double, Color> soundLevels,
                out RadiusInterval radiusInterval,
                out FrequencyBand frequencyBand);
            e.InputData = new FlightNoiseInputData()
            {
                RocketBallistics = id.RocketBallistics,
                VehicleBallistics = id.VehicleBallistics,
                RocketFlowParameters = id.RocketFlowParameters,
                VehicleFlowParameters = id.VehicleFlowParameters,
                WeatherParameters = id.WeatherParameters,
                SoundLevels = soundLevels,
                RadiusInterval = radiusInterval,
                FrequencyBand = frequencyBand,
            };
        }
        void Model_FlightNoiseCalculationProgressChanged(int Progress)
        {
            MainView.SetFlightNoiseCalculationProgress(Progress);
        }
        //FireTestNoiseCalculation
        void MainView_ApplyFireTestNoiseWeatherParameters(List<WeatherParameters> WeatherParameters)
        {
            if ((WeatherParameters.Except(fireTestNoiseCalculation.WeatherParameters, new WeatherCompare()).Count() != 0) ||
                ((WeatherParameters != null) && (fireTestNoiseCalculation.WeatherParameters == null)) ||
                ((WeatherParameters != null) && (fireTestNoiseCalculation.WeatherParameters != null) && (WeatherParameters.Count != fireTestNoiseCalculation.WeatherParameters.Count)))
            {
                fireTestNoiseCalculation.WeatherParameters = WeatherParameters.Select(x => x).ToList();
                fireTestNoiseCalculation.InputDataChanged = true;
            }
        }
        void MainView_ApplyFireTestNoiseSoundLevels(List<double> SoundLevels)
        {
            if (SoundLevels.Except(fireTestNoiseCalculation.SoundLevels).Count() != 0)
            {
                fireTestNoiseCalculation.SoundLevels = SoundLevels.Select(x => x).ToList();
                fireTestNoiseCalculation.InputDataChanged = true;
            }
        }
        void MainView_ApplyFireTestNoisePartialInputData(ApplyEngineParametersEventArgs e)
        {
            fireTestNoiseCalculation.InputDataChanged = true;
            fireTestNoiseCalculation.Thrust = e.Thrust;
            fireTestNoiseCalculation.FlowParameters = e.FlowParameters;
        }
        void MainView_CalculateFireTestSound(CalculateFireTestNoiseEventArgs e)
        {
            if (fireTestNoiseCalculation.FirstCalculation)
            {
                fireTestNoiseCalculation.InputDataChanged = true;
                fireTestNoiseCalculation.FirstCalculation = false;
            }
            else
            {
                if ((fireTestNoiseCalculation.RadiusInterval.Initial != e.RadiusInterval.Initial) ||
                    (fireTestNoiseCalculation.RadiusInterval.Final != e.RadiusInterval.Final) ||
                    (fireTestNoiseCalculation.RadiusInterval.Step != e.RadiusInterval.Step))
                {
                    fireTestNoiseCalculation.InputDataChanged = true;
                }
                if (fireTestNoiseCalculation.FrequencyBand != e.FrequencyBand)
                    fireTestNoiseCalculation.InputDataChanged = true;
            }
            fireTestNoiseCalculation.RadiusInterval = e.RadiusInterval;
            fireTestNoiseCalculation.FrequencyBand = e.FrequencyBand;
            if (fireTestNoiseCalculation.InputDataChanged)
            {
                if (fireTestNoiseCalculation.SoundLevels.Count != 0)
                    fireTestNoiseCalculation.FireTestSoundContours = Model.CalculateFireTestNoise(
                        new FireTestNoiseCalculationInputData(
                            fireTestNoiseCalculation.Thrust,
                            fireTestNoiseCalculation.FlowParameters,
                            fireTestNoiseCalculation.WeatherParameters,
                            e.RadiusInterval,
                            e.FrequencyBand),
                        fireTestNoiseCalculation.SoundLevels);
            }
            fireTestNoiseCalculation.InputDataChanged = false;
            e.FireTestSoundContours = fireTestNoiseCalculation.FireTestSoundContours;
        }
        void MainView_SaveFireTestNoiseInputData(SaveFireTestNoiseInputDataEventArgs e)
        {
            Model.SaveFireTestNoiseInputData(
                e.FileName,
                new FireTestNoiseCalculationInputData(e.Thrust, e.FlowParameters, e.WeatherParameters, e.RadiusInterval, e.FrequencyBand),
                e.SoundLevels);
        }
        void MainView_OpenFireTestNoiseInputData(OpenFireTestNoiseInputDataEventArgs e)
        {
            Model.OpenFireTestNoiseInputData(
                e.FileName, 
                out FireTestNoiseCalculationInputData id, 
                out Dictionary<double, Color> soundLevels);
            e.FireTestNoiseInputData = new FireTestNoiseInputData()
            {
                Thrust = id.Thrust,
                FlowParameters = id.FlowParameters,
                WeatherParameters = id.WeatherParameters,
                SoundLevels = soundLevels,
                RadiusInterval = id.RadiusInterval,
                FrequencyBand = id.FrequencyBand
            };
        }
        void MainView_FireTestNoiseExample(FireTestNoiseExampleEventArgs e)
        {
            XmlDocument xmlDoc = new XmlDocument();
            if (e.FrequencyBand == FrequencyBand.Normal)
                xmlDoc.LoadXml(InputDataExampleResource.FireTest);
            else if (e.FrequencyBand == FrequencyBand.Infra)
                xmlDoc.LoadXml(InputDataExampleResource.FireTestInfra);
            else
                xmlDoc.LoadXml(InputDataExampleResource.FireTestUltra);
            Model.OpenFireTestNoiseInputData(
                xmlDoc,
                out FireTestNoiseCalculationInputData id,
                out Dictionary<double, Color> soundLevels);
            e.InputData = new FireTestNoiseInputData()
            {
                Thrust = id.Thrust,
                FlowParameters = id.FlowParameters,
                WeatherParameters = id.WeatherParameters,
                SoundLevels = soundLevels,
                RadiusInterval = id.RadiusInterval,
                FrequencyBand = id.FrequencyBand
            };
        }
        void Model_FireTestNoiseCalculationProgressChanged(int Progress)
        {
            MainView.SetFireTestNoiseCalculationlProgress(Progress);
        }
        //EngineNoiseCalculation
        void MainView_ApplyEngineNoiseContourParameters(ApplyEngineNoiseContourParametersEventArgs e)
        {
            engineNoiseCalculation.ContourCoordinatesChanged =
                !((engineNoiseCalculation.ContourAreaHeight == e.ContourAreaHeight) &&
                (engineNoiseCalculation.ContourAreaWidth == e.ContourAreaWidth) &&
                (engineNoiseCalculation.NozzleCoordinate == e.NozzleCoordinate));
            engineNoiseCalculation.ContourAreaHeight = e.ContourAreaHeight;
            engineNoiseCalculation.ContourAreaWidth = e.ContourAreaWidth;
            engineNoiseCalculation.NozzleCoordinate = e.NozzleCoordinate;
            engineNoiseCalculation.ContourColorsChanged = !((engineNoiseCalculation.MinSoundLevel == e.MinSoundLevel) && 
                (engineNoiseCalculation.MaxSoundLevel == e.MaxSoundLevel));
            engineNoiseCalculation.MinSoundLevel = e.MinSoundLevel;
            engineNoiseCalculation.MaxSoundLevel = e.MaxSoundLevel;
        }
        void MainView_ApplyEngineNoisePartialInputData(ApplyEngineParametersEventArgs e)
        {
            engineNoiseCalculation.InputDataChanged = true;
            engineNoiseCalculation.FlowParameters = e.FlowParameters;
            engineNoiseCalculation.Thrust = e.Thrust;
        }
        void MainView_CalculateEngineNoise(CalculateEngineNoiseEventArgs e)
        {
            var summary = new EngineAcousticsLoadSummary(
                engineNoiseCalculation.FlowSoundParameters,
                engineNoiseCalculation.FrequencyCharacteristik,
                engineNoiseCalculation.RadiationPattern,
                engineNoiseCalculation.EngineAcousticsLoadAtFrequency,
                engineNoiseCalculation.EngineAcousticsLoadSummary);
            EngineSoundContour EngineSoundContour = engineNoiseCalculation.EngineSoundContour;
            Func<bool> ContourCoordinatesNotNull = () =>
                engineNoiseCalculation.ContourAreaHeight *
                engineNoiseCalculation.ContourAreaWidth *
                engineNoiseCalculation.NozzleCoordinate *
                engineNoiseCalculation.MinSoundLevel *
                engineNoiseCalculation.MaxSoundLevel != 0;
            if (engineNoiseCalculation.InputDataChanged)
            {
                Model.CalculateEngineAcousticsLoadSummary(
                    engineNoiseCalculation.Thrust,
                    engineNoiseCalculation.FlowParameters,
                    out summary);
                if (ContourCoordinatesNotNull())
                    Model.CalculateEngineSoundContour(
                        engineNoiseCalculation.Thrust,
                        engineNoiseCalculation.FlowParameters,
                        new EngineSoundContourParameters(
                            engineNoiseCalculation.ContourAreaWidth,
                            engineNoiseCalculation.ContourAreaHeight,
                            engineNoiseCalculation.NozzleCoordinate,
                            engineNoiseCalculation.MinSoundLevel,
                            engineNoiseCalculation.MaxSoundLevel),
                        out EngineSoundContour);
            }
            else if (engineNoiseCalculation.ContourCoordinatesChanged)
            {
                if (ContourCoordinatesNotNull())
                    Model.CalculateEngineSoundContour(
                        engineNoiseCalculation.Thrust,
                        engineNoiseCalculation.FlowParameters,
                        new EngineSoundContourParameters(
                            engineNoiseCalculation.ContourAreaWidth,
                            engineNoiseCalculation.ContourAreaHeight,
                            engineNoiseCalculation.NozzleCoordinate,
                            engineNoiseCalculation.MinSoundLevel,
                            engineNoiseCalculation.MaxSoundLevel),
                        out EngineSoundContour);
            }
            else if (engineNoiseCalculation.ContourColorsChanged)
            {
                if (ContourCoordinatesNotNull())
                    Model.ModifyEngineSoundContour(engineNoiseCalculation.MinSoundLevel, engineNoiseCalculation.MaxSoundLevel, ref EngineSoundContour);
            }
            engineNoiseCalculation.FlowSoundParameters = summary.FlowSoundParameters;
            engineNoiseCalculation.FrequencyCharacteristik = summary.FrequencyCharacteristik;
            engineNoiseCalculation.RadiationPattern = summary.RadiationPattern;
            engineNoiseCalculation.EngineAcousticsLoadAtFrequency = summary.EngineAcousticsLoadAtFrequency;
            engineNoiseCalculation.EngineAcousticsLoadSummary = summary.SummaryLoad;
            engineNoiseCalculation.EngineSoundContour = EngineSoundContour;
            e.FlowSoundParameters = summary.FlowSoundParameters;
            e.FrequencyCharacteristik = summary.FrequencyCharacteristik;
            e.RadiationPattern = summary.RadiationPattern;
            e.EngineAcousticsLoadAtFrequency = summary.EngineAcousticsLoadAtFrequency;
            e.EngineAcousticsLoadSummary = summary.SummaryLoad;
            e.EngineSoundContour = EngineSoundContour;
            engineNoiseCalculation.InputDataChanged = false;
            engineNoiseCalculation.ContourCoordinatesChanged = false;
            engineNoiseCalculation.ContourColorsChanged = false;
        }
        void MainView_OpenEngineNoiseInputData(OpenEngineNoiseInputDataEventArgs e)
        {
            Model.OpenEngineNoiseInputData(
                e.FileName,
                out double thrust,
                out FlowParameters flowParameters,
                out EngineSoundContourParameters ContourParameters);
            e.InputData = new EngineNoiseInputData()
            {
                Thrust = thrust,
                FlowParameters = flowParameters,
                ContourAreaWidth = ContourParameters.ContourAreaWidth,
                ContourAreaHeight = ContourParameters.ContourAreaHeight,
                NozzleCoordinate = ContourParameters.NozzleCoordinate,
                MinSoundLevel = ContourParameters.MinSoundLevel,
                MaxSoundLevel = ContourParameters.MaxSoundLevel,
            };
        }
        void MainView_SaveEngineNoiseInputData(SaveEngineNoiseInputDataEventArgs e)
        {
            Model.SaveEngineNoiseInputData(e.FileName, e.Thrust, e.FlowParameters, new EngineSoundContourParameters(e.ContourAreaWidth, e.ContourAreaHeight, e.NozzleCoordinate, e.MinSoundLevel, e.MaxSoundLevel));
        }
        private void MainView_EngineNoiseExample(EngineNoiseExampleEventArgs e)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(InputDataExampleResource.EngineAcoustics);
            Model.OpenEngineNoiseInputData(
                xmlDoc,
                out double thrust,
                out FlowParameters flowParameters,
                out EngineSoundContourParameters ContourParameters);
            e.InputData = new EngineNoiseInputData()
            {
                Thrust = thrust,
                FlowParameters = flowParameters,
                ContourAreaWidth = ContourParameters.ContourAreaWidth,
                ContourAreaHeight = ContourParameters.ContourAreaHeight,
                NozzleCoordinate = ContourParameters.NozzleCoordinate,
                MinSoundLevel = ContourParameters.MinSoundLevel,
                MaxSoundLevel = ContourParameters.MaxSoundLevel,
            };
        }
        void Model_EngineNoiseCalculationProgressChanged(int Progress)
        {
            MainView.SetEngineNoiseCalculationlProgress(Progress);
        }
        //SonicBoomCalculation
        void MainView_CalculateSonicBoom(CalculateSonicBoomEventArgs e)
        {
            Func<GeometricalParameters, bool> IsGeometricalParametersNull = Parameters => Parameters.CharacteristicArea * Parameters.CharacteristicLength * Parameters.Length * Parameters.MaximalArea == 0;
            List<SonicBoomParameters> Rocket;
            List<SonicBoomParameters> Vehicle;
            Model.CalculateSonicBoom(
                new SonicBoomCalculationInputData(e.RocketBallistics, e.RocketGeometricalParameters),
                new SonicBoomCalculationInputData(e.VehicleBallistics, e.VehicleGeometricalParameters),
                e.WeatherParameters,
                out Rocket,
                out Vehicle);
            e.RocketSonicBoomParameters  = Rocket;
            e.VehicleSonicBoomParameters = Vehicle;
        }
        void MainView_SaveSonicBoomInputData(SaveSonicBoomInputDataEventArgs e)
        {
            Model.SaveSonicBoomInputData(
                e.FileName,
                new SonicBoomCalculationInputData(e.RocketBallistics, e.RocketGeometricalParameters),
                new SonicBoomCalculationInputData(e.VehicleBallistics, e.VehicleGeometricalParameters), 
                e.WeatherParameters);
        }
        void MainView_OpenSonicBoomInputData(OpenSonicBoomInputDataEventArgs e)
        {
            Model.OpenSonicBoomInputData(
                e.FileName,
                out SonicBoomCalculationInputData RocketID,
                out SonicBoomCalculationInputData VehicleID,
                out List<WeatherParameters> weatherParameters);
            e.InputData = new SonicBoomInputData()
            {
                RocketBallistics = RocketID.Ballistics,
                VehicleBallistics = VehicleID.Ballistics,
                RocketGeometricalParameters = RocketID.GeometricalParameters,
                VehicleGeometricalParameters = VehicleID.GeometricalParameters,
                WeatherParameters = weatherParameters
            };
        }
        void MainView_SonicBoomExample(SonicBoomExampleEventArgs e)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(InputDataExampleResource.SonicBoom);
            Model.OpenSonicBoomInputData(
                xmlDoc,
                out SonicBoomCalculationInputData RocketID,
                out SonicBoomCalculationInputData VehicleID,
                out List<WeatherParameters> weatherParameters);
            e.InputData = new SonicBoomInputData()
            {
                RocketBallistics = RocketID.Ballistics,
                VehicleBallistics = VehicleID.Ballistics,
                RocketGeometricalParameters = RocketID.GeometricalParameters,
                VehicleGeometricalParameters = VehicleID.GeometricalParameters,
                WeatherParameters = weatherParameters
            };
        }
        void Model_SonicBoomCalculationProgressChanged(int Progress)
        {
            MainView.SetSonicBoomCalculationProgress(Progress);
        }
        class WeatherCompare : IEqualityComparer<WeatherParameters>
        {
            public bool Equals(WeatherParameters x, WeatherParameters y)
            {
                return (x.Humidity == y.Humidity) && (x.Temperature == y.Temperature);
            }
            public int GetHashCode(WeatherParameters obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}