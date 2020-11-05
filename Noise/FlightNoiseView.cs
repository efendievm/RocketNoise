using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZedGraph;
using MainViewInterface;
using TypesLibrary;

namespace Noise
{
    public partial class MainForm : Form, IMainView
    {
        public void InitializeFlightNoiseView()
        {
            FlightGraphPane = zedGraphControl1.GraphPane;
            FlightGraphPane.IsFontsScaled = false;
            FlightGraphPane.Title.IsVisible = false;
            FlightGraphPane.XAxis.Title.Text = "X, м";
            FlightGraphPane.XAxis.MajorGrid.IsVisible = true;
            FlightGraphPane.XAxis.MinorGrid.IsVisible = true;
            FlightGraphPane.XAxis.MajorGrid.DashOff = 1;
            FlightGraphPane.XAxis.MinorGrid.DashOff = 1;
            FlightGraphPane.YAxis.Title.Text = "Y, м";
            FlightGraphPane.YAxis.MajorGrid.IsVisible = true;
            FlightGraphPane.YAxis.MinorGrid.IsVisible = true;
            FlightGraphPane.YAxis.MajorGrid.DashOff = 1;
            FlightGraphPane.YAxis.MinorGrid.DashOff = 1;
            FlightNoiseWeatherParametersContainer = new WeatherParametersContainer(
                textBox1,
                textBox3,
                textBox2,
                AddFlightNoiseWeatherParameterButton,
                ModifyFlightNoiseWeatherParameterButton,
                DeleteFlightNoiseWeatherParameterButton,
                ApplyFlightNoiseWeatherParametersButton,
                FlightNoiseWeatherParametersListBox,
                ApplyFlightNoiseWeatherParameters);
            FlightNoiseRocketFlowParametersContainer = new FlowParametersContainer(
                RocketFlightNoiseMassFlowTextBox,
                RocketFlightNoiseNozzleDiameterTextBox,
                RocketFlightNoiseNozzleMachNumberTextBox,
                RocketFlightNoiseNozzleFlowVelocityTextBox,
                RocketFlightNoiseChamberSoundVelocityTextBox,
                RocketFlightNoiseNozzleAdiabaticIndexTextBox);
            FlightNoiseVehicleFlowParametersContainer = new FlowParametersContainer(
                VehicleFlightNoiseMassFlowTextBox,
                VehicleFlightNoiseNozzleDiameterTextBox,
                VehicleFlightNoiseNozzleMachNumberTextBox,
                VehicleFlightNoiseNozzleFlowVelocityTextBox,
                VehicleFlightNoiseChamberSoundVelocityTextBox,
                VehicleFlightNoiseNozzleAdiabaticIndexTextBox);
            FlightNoiseSoundLevelContainer = new SoundLevelContainer(
                textBox5,
                textBox4,
                textBox6,
                textBox7,
                textBox8,
                AddFlightNoiseSoundLevelButton,
                ModifyFlightNoiseSoundLevelButton,
                DeleteFlightNoiseSoundLevelButton,
                ApplyFlightNoiseSoundLevelsButton,
                SetFlightNoiseSoundLevelColorButton,
                FlightNoiseSoundLevelsListBox,
                ApplyFlightNoiseSoundLevels);
            foreach (var grid in GetControls(FlightNoiseTabPage, typeof(DataGridView)).Cast<DataGridView>())
            {
                grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                for (int i = 0; i < grid.Columns.Count; i++)
                    grid.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            }
            VehicleFlightNoiseBallisticsForm = new FlightNoiseBallisticsForm(Part.Vehicle);
            RocketFlightNoiseBallisticsForm = new FlightNoiseBallisticsForm(Part.Rocket);
            Dictionary<Button, Control> DropDownControls = new Dictionary<Button, Control>()
            {
                { RocketFlightParametersButton, RocketFlightNoiseInputDataTablePanel },
                { VehicleFlightParametersButton, VehicleFlightNoiseInputDataTablePanel },
                { FlightFrequencyBandButton, FlightFrequencyBandPanel },
                { FlightCalculationAreaButton, FlightCalculationAreaPanel },
                { FlightWeatherParametersButton, FlightWeatherParametersPanel },
                { FlightNoiseLevelsButton, FlightNoiseLevelsPanel }
            };
            foreach (var b in DropDownControls.Keys)
            {
                b.ImageList = ButtonsImageList;
                b.ImageIndex = 0;
                b.ImageAlign = ContentAlignment.MiddleRight;
                b.Click += (sender, e) =>
                {
                    DropDownControls[b].Visible = !DropDownControls[b].Visible;
                    b.ImageIndex = DropDownControls[b].Visible ? 1 : 0;
                };
            };
        }
        GraphPane FlightGraphPane;
        WeatherParametersContainer FlightNoiseWeatherParametersContainer;
        FlowParametersContainer FlightNoiseRocketFlowParametersContainer;
        FlowParametersContainer FlightNoiseVehicleFlowParametersContainer;
        SoundLevelContainer FlightNoiseSoundLevelContainer;
        FlightNoiseBallisticsForm RocketFlightNoiseBallisticsForm;
        FlightNoiseBallisticsForm VehicleFlightNoiseBallisticsForm;
        private void FlightFrequencyBandRadioButton_Click(object sender, EventArgs e)
        {
            FlightNoiseLevelLabel.Text = "Уровень шума, дБ";
            if (FlightNormalBandRadioButton.Checked)
                FlightNoiseLevelLabel.Text += "A";
            EffectiveFlightSoundGrid.Enabled = FlightNormalBandRadioButton.Checked;
        }
        private void SaveFlightNoiseInputDataMenuItem_Click()
        {
            SaveFileDialog Dialog = new SaveFileDialog();
            if (Dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                SaveFlightNoiseInputData(new SaveFlightNoiseInputDataEventArgs(
                    Dialog.FileName,
                    (RocketBallistics)RocketFlightNoiseBallisticsForm.Ballistics,
                    (VehicleBallistics)VehicleFlightNoiseBallisticsForm.Ballistics,
                    FlightNoiseRocketFlowParametersContainer.FlowParameters,
                    FlightNoiseVehicleFlowParametersContainer.FlowParameters,
                    FlightNoiseWeatherParametersContainer.WeatherParameters,
                    FlightNoiseSoundLevelContainer.SoundLevels.ToDictionary(x => x.SoundLevel, x => x.Color),
                    new RadiusInterval(
                        Convert.ToDouble(FlightStartRadiusTextBox.Text),
                        Convert.ToDouble(FlightFinalRadiusTextBox.Text),
                        Convert.ToDouble(FlightRadiusStepTextBox.Text)),
                    FlightNormalBandRadioButton.Checked ? 
                        FrequencyBand.Normal : 
                        FlightUltraBandRadioButton.Checked ? FrequencyBand.Ultra : FrequencyBand.Infra));
        }
        private void SetFlightNoiseInputData(FlightNoiseInputData id)
        {
            RocketFlightNoiseBallisticsForm.Ballistics = id.RocketBallistics;
            VehicleFlightNoiseBallisticsForm.Ballistics = id.VehicleBallistics;
            FlightNoiseRocketFlowParametersContainer.FlowParameters = id.RocketFlowParameters;
            FlightNoiseVehicleFlowParametersContainer.FlowParameters = id.VehicleFlowParameters;
            FlightNoiseWeatherParametersContainer.WeatherParameters = id.WeatherParameters;
            FlightNoiseSoundLevelContainer.SoundLevels = id.SoundLevels.Select(x => new ColoredSoundLevel(x.Key, x.Value)).ToList();
            FlightStartRadiusTextBox.Text = id.RadiusInterval.Initial.ToString();
            FlightFinalRadiusTextBox.Text = id.RadiusInterval.Final.ToString();
            FlightRadiusStepTextBox.Text = id.RadiusInterval.Step.ToString();
            RadioButton checkRadioButton;
            if (id.FrequencyBand == FrequencyBand.Infra)
                checkRadioButton = FlightInfraBandRadioButton;
            else if (id.FrequencyBand == FrequencyBand.Ultra)
                checkRadioButton = FlightUltraBandRadioButton;
            else
                checkRadioButton = FlightNormalBandRadioButton;
            checkRadioButton.Checked = true;
            FlightFrequencyBandRadioButton_Click(this, EventArgs.Empty);
            ApplyFlightNoisePartialInputData(new ApplyFlightNoisePartialInputDataEventArgs(Part.Rocket, id.RocketBallistics, id.RocketFlowParameters));
            ApplyFlightNoisePartialInputData(new ApplyFlightNoisePartialInputDataEventArgs(Part.Vehicle, id.VehicleBallistics, id.VehicleFlowParameters));
            ApplyFlightNoiseWeatherParameters(id.WeatherParameters);
            ApplyFlightNoiseSoundLevels(id.SoundLevels.Select(x => x.Key).ToList());
        }
        private void OpenFlightNoiseInputDataMenuItem_Click()
        {
            OpenFileDialog Dialog = new OpenFileDialog();
            if (Dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var InputDataEventArgs = new OpenFlightNoiseInputDataEventArgs(Dialog.FileName);
                OpenFlightNoiseInputData(InputDataEventArgs);
                SetFlightNoiseInputData(InputDataEventArgs.InputData);
            }
        }
        private void FlightNoiseMenu_Click(object sender, EventArgs e)
        {
            FrequencyBand FrequencyBand;
            if (sender == FlightNoiseNormalMenu)
                FrequencyBand = FrequencyBand.Normal;
            else if (sender == FlightNoiseUltraMenu)
                FrequencyBand = FrequencyBand.Ultra;
            else
                FrequencyBand = FrequencyBand.Infra;
            var InputDataEventArgs = new FlightNoiseExampleEventArgs(FrequencyBand);
            FlightNoiseExample(InputDataEventArgs);
            SetFlightNoiseInputData(InputDataEventArgs.InputData);
            TabControl.SelectedIndex = 0;
            CalculateFlightNoiseMenuItem_Click();
        }
        private async void CalculateFlightNoiseMenuItem_Click()
        {
            FrequencyBand frequencyBand;
            if (FlightInfraBandRadioButton.Checked)
                frequencyBand = FrequencyBand.Infra;
            else if (FlightUltraBandRadioButton.Checked)
                frequencyBand = FrequencyBand.Ultra;
            else
                frequencyBand = FrequencyBand.Normal;
            var CalculateArgs = new CalculateFlightNoiseEventArgs(
                frequencyBand,
                new RadiusInterval(
                    Convert.ToDouble(FlightStartRadiusTextBox.Text),
                    Convert.ToDouble(FlightFinalRadiusTextBox.Text),
                    Convert.ToDouble(FlightRadiusStepTextBox.Text)));
            await Task.Run(() => CalculateFlightNoise(CalculateArgs));
            var Circles = CalculateArgs.FlightSoundCircles;
            FlightGraphPane.CurveList.Clear();
            for (int i = 0; i < FlightNoiseSoundLevelContainer.SoundLevels.Count; i++)
                FlightGraphPane.AddCurve(
                    FlightNoiseSoundLevelContainer.SoundLevels[i].SoundLevel.ToString(),
                    Circles[i].Points.Select(p => p.X).ToArray(),
                    Circles[i].Points.Select(p => p.Y).ToArray(),
                    FlightNoiseSoundLevelContainer.SoundLevels[i].Color,
                    SymbolType.None);
            FlightGraphPane.AxisChange();
            zedGraphControl1.Invalidate();
            
            var PointsGrid = FlightNoiseSoundLevelPointsGrid;
            PointsGrid.Columns.Clear();
            foreach (var circle in Circles)
            {
                var s = circle.SoundLevel.ToString() + " дБ";
                if (FlightNormalBandRadioButton.Checked)
                    s += "A";
                PointsGrid.Columns.Add("", s);
                PointsGrid.Columns.Add("", "");
            }
            foreach (var column in PointsGrid.Columns.Cast<DataGridViewColumn>())
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            PointsGrid.Rows.Add();
            PointsGrid.Rows[0].Frozen = true;
            for (int i = 0; i < PointsGrid.Columns.Count; i++)
                PointsGrid.Rows[0].Cells[i].Value = i % 2 == 0 ? "X, м" : "Y, м";
            int MaxPointsCount = Circles.Max(c => c.Points.Count);
            if (MaxPointsCount != 0)
                PointsGrid.Rows.Add(MaxPointsCount);

            var CirclesGrid = FlightNoiseSoundLevelCirclesGrid;
            CirclesGrid.Rows.Clear();
            CirclesGrid.Columns[0].HeaderText = "Уровень шума, дБ";
            if (FlightNormalBandRadioButton.Checked)
                CirclesGrid.Columns[0].HeaderText += "A";
            for (int CircleIndex = 0; CircleIndex < Circles.Count; CircleIndex++)
            {
                var circle = Circles[CircleIndex];
                int CurrentRowIndex = CirclesGrid.Rows.Count;
                CirclesGrid.Rows.Add(4);
                CirclesGrid.Rows[CurrentRowIndex].Cells[0].Value = circle.SoundLevel;
                CirclesGrid.Rows[CurrentRowIndex + 0].Cells[1].Value = "Левая окружность. МСРН";
                CirclesGrid.Rows[CurrentRowIndex + 1].Cells[1].Value = "Правая окружность. МСРН";
                CirclesGrid.Rows[CurrentRowIndex + 2].Cells[1].Value = "Левая окружность. МСКА";
                CirclesGrid.Rows[CurrentRowIndex + 3].Cells[1].Value = "Правая окружность. МСКА";
                var SoundCircles = new List<FlightSoundCircle>()
                { 
                    circle.LeftRocketCircle, 
                    circle.RightRocketCircle, 
                    circle.LeftVehicleCircle, 
                    circle.RightVehicleCircle 
                };
                for (int i = 0; i < SoundCircles.Count; i++)
                {
                    CirclesGrid.Rows[i + CurrentRowIndex].Cells[2].Value = FormatConvert.ToString(SoundCircles[i].Distance);
                    CirclesGrid.Rows[i + CurrentRowIndex].Cells[3].Value = FormatConvert.ToString(SoundCircles[i].Radius);
                    CirclesGrid.Rows[i + CurrentRowIndex].Cells[4].Value = FormatConvert.ToString(SoundCircles[i].Time);
                    CirclesGrid.Rows[i + CurrentRowIndex].Cells[5].Value = SoundCircles[i].Mounth;
                }
                for (int i = 0; i < circle.Points.Count; i++)
                {
                    PointsGrid.Rows[i + 1].Cells[2 * CircleIndex + 0].Value = FormatConvert.ToString(circle.Points[i].X);
                    PointsGrid.Rows[i + 1].Cells[2 * CircleIndex + 1].Value = FormatConvert.ToString(circle.Points[i].Y);
                }
            }
            var EffectiveGrid = EffectiveFlightSoundGrid;
            EffectiveGrid.Rows.Clear();
            if ((CalculateArgs.EffectiveFlightSounds == null) || CalculateArgs.EffectiveFlightSounds.Count == 0)
                return;
            foreach (var soundLevel in CalculateArgs.EffectiveFlightSounds)
            {
                int CurrentRowIndex = EffectiveGrid.Rows.Count;
                EffectiveGrid.Rows.Add(2);
                EffectiveGrid.Rows[CurrentRowIndex + 0].Cells[0].Value = FormatConvert.ToString(soundLevel.Distance);
                EffectiveGrid.Rows[CurrentRowIndex + 0].Cells[1].Value = "0";
                EffectiveGrid.Rows[CurrentRowIndex + 0].Cells[2].Value = FormatConvert.ToString(soundLevel.RightMaxSoundLevel);
                EffectiveGrid.Rows[CurrentRowIndex + 0].Cells[3].Value = FormatConvert.ToString(soundLevel.RightEffektiveSoundLevel);
                EffectiveGrid.Rows[CurrentRowIndex + 0].Cells[4].Value = FormatConvert.ToString(soundLevel.RightTimes[0]);
                EffectiveGrid.Rows[CurrentRowIndex + 0].Cells[5].Value = FormatConvert.ToString(soundLevel.RightTimes[1]);
                EffectiveGrid.Rows[CurrentRowIndex + 0].Cells[6].Value = FormatConvert.ToString(soundLevel.RightTimes[2]);
                EffectiveGrid.Rows[CurrentRowIndex + 0].Cells[7].Value = soundLevel.RightWeatherConditions;
                EffectiveGrid.Rows[CurrentRowIndex + 1].Cells[1].Value = "180";
                EffectiveGrid.Rows[CurrentRowIndex + 1].Cells[2].Value = FormatConvert.ToString(soundLevel.LeftMaxSoundLevel);
                EffectiveGrid.Rows[CurrentRowIndex + 1].Cells[3].Value = FormatConvert.ToString(soundLevel.LeftEffektiveSoundLevel);
                EffectiveGrid.Rows[CurrentRowIndex + 1].Cells[4].Value = FormatConvert.ToString(soundLevel.LeftTimes[0]);
                EffectiveGrid.Rows[CurrentRowIndex + 1].Cells[5].Value = FormatConvert.ToString(soundLevel.LeftTimes[1]);
                EffectiveGrid.Rows[CurrentRowIndex + 1].Cells[6].Value = FormatConvert.ToString(soundLevel.LeftTimes[2]);
                EffectiveGrid.Rows[CurrentRowIndex + 1].Cells[7].Value = soundLevel.LeftWeatherConditions;
            }
        }
        private void OpenFlightNoiseBallisticsButton_Click(object sender, EventArgs e)
        {
            if (sender == OpenRocketFlightNoiseBallisticsButton)
                RocketFlightNoiseBallisticsForm.ShowDialog();
            else
                VehicleFlightNoiseBallisticsForm.ShowDialog();
        }
        private void ApplyPartialFlightNoiseInputDataButton_Click(object sender, EventArgs e)
        {
            var ApplyArgs = (sender == ApplyRocketFlightNoiseInputDataButton) ?
                new ApplyFlightNoisePartialInputDataEventArgs(
                    Part.Rocket,
                    RocketFlightNoiseBallisticsForm.Ballistics,
                    FlightNoiseRocketFlowParametersContainer.FlowParameters) :
                new ApplyFlightNoisePartialInputDataEventArgs(
                    Part.Vehicle,
                    VehicleFlightNoiseBallisticsForm.Ballistics,
                    FlightNoiseVehicleFlowParametersContainer.FlowParameters);
            ApplyFlightNoisePartialInputData(ApplyArgs);
        }
        public void SetFlightNoiseCalculationProgress(int Progress)
        {
            this.Invoke(new Action(() => FlightNoiseProgressBar.Value = Progress));
        }

        public event Action<OpenFlightNoiseInputDataEventArgs> OpenFlightNoiseInputData;
        public event Action<SaveFlightNoiseInputDataEventArgs> SaveFlightNoiseInputData;
        public event Action<CalculateFlightNoiseEventArgs> CalculateFlightNoise;
        public event Action<ApplyFlightNoisePartialInputDataEventArgs> ApplyFlightNoisePartialInputData;
        public event Action<List<WeatherParameters>> ApplyFlightNoiseWeatherParameters;
        public event Action<List<double>> ApplyFlightNoiseSoundLevels;
        public event Action<FlightNoiseExampleEventArgs> FlightNoiseExample;
    }
}