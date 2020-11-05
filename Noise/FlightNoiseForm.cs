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
using TypesLibary;
using Excel = Microsoft.Office.Interop.Excel;

namespace Noise
{
    public partial class MainForm : Form, IMainView
    {
        class FlightSoundLevel
        {
            public double SoundLevel { get; set; }
            public Color Color { get; set; }
            public FlightSoundLevel(double SoundLevel, Color Color)
            {
                this.SoundLevel = SoundLevel;
                this.Color = Color;
            }

        }
        GraphPane FlightGraphPane;
        List<WeatherParameters> FlightNoiseWeatherParameters;
        List<FlightSoundLevel> FlightNoiseSoundLevels;
        FlowParameters GetFlowParameters(Part part)
        {
            if (part == Part.Rocket)
                return new FlowParameters(
                            Convert.ToDouble(RocketFlightNoiseMassFlowTextBox.Text),
                            Convert.ToDouble(RocketFlightNoiseNozzleDiameterTextBox.Text) * 1E-3,
                            Convert.ToDouble(RocketFlightNoiseNozzleMachNumberTextBox.Text),
                            Convert.ToDouble(RocketFlightNoiseNozzleFlowVelocityTextBox.Text),
                            Convert.ToDouble(RocketFlightNoiseChamberSoundVelocityTextBox.Text),
                            Convert.ToDouble(RocketFlightNoiseNozzleAdiabaticIndexTextBox.Text));
            else
                return new FlowParameters(
                            Convert.ToDouble(VehicleFlightNoiseMassFlowTextBox.Text),
                            Convert.ToDouble(VehicleFlightNoiseNozzleDiameterTextBox.Text) * 1E-3,
                            Convert.ToDouble(VehicleFlightNoiseNozzleMachNumberTextBox.Text),
                            Convert.ToDouble(VehicleFlightNoiseNozzleFlowVelocityTextBox.Text),
                            Convert.ToDouble(VehicleFlightNoiseChamberSoundVelocityTextBox.Text),
                            Convert.ToDouble(VehicleFlightNoiseNozzleAdiabaticIndexTextBox.Text));
        }
        private void SaveFlightNoiseInputDataMenuItem_Click()
        {
            SaveFileDialog Dialog = new SaveFileDialog();
            if (Dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                SaveFlightNoiseInputData(this, new SaveFlightNoiseInputDataEventArgs(
                    Dialog.FileName,
                    RocketFlightNoiseBallisticsTextBox.Text,
                    VehicleFlightNoiseBallisticsTextBox.Text,
                    GetFlowParameters(Part.Rocket),
                    GetFlowParameters(Part.Vehicle),
                    FlightNoiseWeatherParameters,
                    FlightNoiseSoundLevels.ToDictionary(x => x.SoundLevel, x => x.Color)));
        }
        private void OpenFlightNoiseInputDataMenuItem_Click()
        {
            OpenFileDialog Dialog = new OpenFileDialog();
            if (Dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var InputData = new OpenFlightNoiseInputDataEventArgs(Dialog.FileName);
                OpenFlightNoiseInputData(this, InputData);
                RocketFlightNoiseBallisticsTextBox.Text = InputData.RocketBallisticsPath;
                VehicleFlightNoiseBallisticsTextBox.Text = InputData.VehicleBallisticsPath;
                RocketFlightNoiseMassFlowTextBox.Text = InputData.RocketFlowParameters.MassFlow.ToString();
                RocketFlightNoiseNozzleDiameterTextBox.Text = (InputData.RocketFlowParameters.NozzleDiameter * 1E3).ToString();
                RocketFlightNoiseNozzleMachNumberTextBox.Text = InputData.RocketFlowParameters.NozzleMachNumber.ToString();
                RocketFlightNoiseNozzleFlowVelocityTextBox.Text = InputData.RocketFlowParameters.NozzleFlowVelocity.ToString();
                RocketFlightNoiseChamberSoundVelocityTextBox.Text = InputData.RocketFlowParameters.ChamberSoundVelocity.ToString();
                RocketFlightNoiseNozzleAdiabaticIndexTextBox.Text = InputData.RocketFlowParameters.NozzleAdiabaticIndex.ToString();
                VehicleFlightNoiseMassFlowTextBox.Text = InputData.VehicleFlowParameters.MassFlow.ToString();
                VehicleFlightNoiseNozzleDiameterTextBox.Text = (InputData.VehicleFlowParameters.NozzleDiameter * 1E3).ToString();
                VehicleFlightNoiseNozzleMachNumberTextBox.Text = InputData.VehicleFlowParameters.NozzleMachNumber.ToString();
                VehicleFlightNoiseNozzleFlowVelocityTextBox.Text = InputData.VehicleFlowParameters.NozzleFlowVelocity.ToString();
                VehicleFlightNoiseChamberSoundVelocityTextBox.Text = InputData.VehicleFlowParameters.ChamberSoundVelocity.ToString();
                VehicleFlightNoiseNozzleAdiabaticIndexTextBox.Text = InputData.VehicleFlowParameters.NozzleAdiabaticIndex.ToString();
                FlightNoiseWeatherParameters = InputData.WeatherParameters;
                FlightNoiseWeatherParametersListBox.Items.Clear();
                foreach (var weatherParameters in FlightNoiseWeatherParameters)
                    FlightNoiseWeatherParametersListBox.Items.Add(weatherParameters.Mounth);
                FlightNoiseSoundLevels = InputData.SoundLevels.Select(x => new FlightSoundLevel(x.Key, x.Value)).ToList();
                FlightNoiseSoundLevelsListBox.Items.Clear();
                foreach (var soundLevel in FlightNoiseSoundLevels)
                    FlightNoiseSoundLevelsListBox.Items.Add(soundLevel.SoundLevel.ToString());
                ApplyPartialFlightNoiseInputDataButton_Click(ApplyRocketFlightNoiseInputDataButton, EventArgs.Empty);
                ApplyPartialFlightNoiseInputDataButton_Click(ApplyVehicleFlightNoiseInputDataButton, EventArgs.Empty);
                FlightNoiseSoundLevelButton_Click(ApplyFlightNoiseSoundLevelsButton, EventArgs.Empty);
                FlightNoiseWeatherParametersButton_Click(ApplyFlightNoiseWeatherParameterButton, EventArgs.Empty);
            }
        }
        private async void CalculateMenuItem_Click(object sender, EventArgs e)
        {
            var CalculateArgs = new CalculateFlightNoiseEventArgs();
            await Task.Run(() => CalculateFlightNoise(this, CalculateArgs));
            var Circles = CalculateArgs.FlightSoundCircles;
            FlightGraphPane.CurveList.Clear();
            for (int i = 0; i < FlightNoiseSoundLevels.Count; i++)
                FlightGraphPane.AddCurve(
                    FlightNoiseSoundLevels[i].SoundLevel.ToString(),
                    Circles[i].Points.Select(p => p.X).ToArray(),
                    Circles[i].Points.Select(p => p.Y).ToArray(),
                    FlightNoiseSoundLevels[i].Color,
                    SymbolType.None);
            FlightGraphPane.AxisChange();
            zedGraphControl1.Invalidate();
            var CirclesGrid = FlightNoiseSoundLevelCirclesGrid;
            var PointsGrid = FlightNoiseSoundLevelPointsGrid;
            CirclesGrid.Rows.Clear();
            PointsGrid.Columns.Clear();
            foreach (var circle in Circles)
            {
                PointsGrid.Columns.Add("", circle.SoundLevel.ToString() + " дБА");
                PointsGrid.Columns.Add("", "");
            }
            PointsGrid.Rows.Add();
            PointsGrid.Rows[0].Frozen = true;
            for (int i = 0; i < PointsGrid.Columns.Count; i++)
                PointsGrid.Rows[0].Cells[i].Value = i % 2 == 0 ? "X, м" : "Y, м";
            PointsGrid.Rows.Add(Circles[0].Points.Count);
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
                var SoundCircles = new List<FlightSoundCircle> 
                { 
                    circle.LeftRocketCircle, 
                    circle.RightRocketCircle, 
                    circle.LeftVehicleCircle, 
                    circle.RightVehicleCircle 
                };
                for (int i = 0; i < SoundCircles.Count; i++)
                {
                    CirclesGrid.Rows[i + CurrentRowIndex].Cells[2].Value = SoundCircles[i].Distance;
                    CirclesGrid.Rows[i + CurrentRowIndex].Cells[3].Value = SoundCircles[i].Radius;
                    CirclesGrid.Rows[i + CurrentRowIndex].Cells[4].Value = SoundCircles[i].Time;
                    CirclesGrid.Rows[i + CurrentRowIndex].Cells[5].Value = SoundCircles[i].Mounth;
                }
                for (int i = 0; i < circle.Points.Count; i++)
                {
                    PointsGrid.Rows[i + 1].Cells[2 * CircleIndex + 0].Value = circle.Points[i].X;
                    PointsGrid.Rows[i + 1].Cells[2 * CircleIndex + 1].Value = circle.Points[i].Y;
                }
            }
            var EffectiveGrid = EffectiveFlightSoundGrid;
            EffectiveGrid.Rows.Clear();
            foreach (var soundLevel in CalculateArgs.EffectiveFlightSounds)
            {
                int CurrentRowIndex = EffectiveGrid.Rows.Count;
                EffectiveGrid.Rows.Add(2);
                EffectiveGrid.Rows[CurrentRowIndex + 0].Cells[0].Value = soundLevel.Distance;
                EffectiveGrid.Rows[CurrentRowIndex + 0].Cells[1].Value = "0";
                EffectiveGrid.Rows[CurrentRowIndex + 0].Cells[2].Value = soundLevel.RightMaxSoundLevel;
                EffectiveGrid.Rows[CurrentRowIndex + 0].Cells[3].Value = soundLevel.RightEffektiveSoundLevel;
                EffectiveGrid.Rows[CurrentRowIndex + 0].Cells[4].Value = soundLevel.RightTimes[0];
                EffectiveGrid.Rows[CurrentRowIndex + 0].Cells[5].Value = soundLevel.RightTimes[1];
                EffectiveGrid.Rows[CurrentRowIndex + 0].Cells[6].Value = soundLevel.RightTimes[2];
                EffectiveGrid.Rows[CurrentRowIndex + 0].Cells[7].Value = soundLevel.RightWeatherConditions;
                EffectiveGrid.Rows[CurrentRowIndex + 1].Cells[1].Value = "180";
                EffectiveGrid.Rows[CurrentRowIndex + 1].Cells[2].Value = soundLevel.LeftMaxSoundLevel;
                EffectiveGrid.Rows[CurrentRowIndex + 1].Cells[3].Value = soundLevel.LeftEffektiveSoundLevel;
                EffectiveGrid.Rows[CurrentRowIndex + 1].Cells[4].Value = soundLevel.LeftTimes[0];
                EffectiveGrid.Rows[CurrentRowIndex + 1].Cells[5].Value = soundLevel.LeftTimes[1];
                EffectiveGrid.Rows[CurrentRowIndex + 1].Cells[6].Value = soundLevel.LeftTimes[2];
                EffectiveGrid.Rows[CurrentRowIndex + 1].Cells[7].Value = soundLevel.LeftWeatherConditions;
            }
        }
        private void FlightNoiseWeatherParametersButton_Click(object sender, EventArgs e)
        {
            if (FlightNoiseWeatherParameters == null)
                FlightNoiseWeatherParameters = new List<WeatherParameters>();
            if (sender == AddFlightNoiseWeatherParameterButton)
            {
                if (FlightNoiseWeatherParameters.FindIndex(x => x.Mounth == textBox1.Text) != -1)
                {
                    MessageBox.Show("Погодные условия с указанным наименованием уже существуют");
                    return;
                }
                FlightNoiseWeatherParameters.Add(new WeatherParameters(
                    textBox1.Text,
                    Convert.ToDouble(textBox3.Text),
                    Convert.ToDouble(textBox2.Text) + 273));
                FlightNoiseWeatherParametersListBox.Items.Add(textBox1.Text);
            }
            else if (sender == ModifyFlightNoiseWeatherParameterButton)
            {
                int Selected = FlightNoiseWeatherParametersListBox.SelectedIndex;
                if (Selected == -1) return;
                int FirstEqual = FlightNoiseWeatherParameters.FindIndex(x => x.Mounth == textBox1.Text);
                if ((FirstEqual == Selected) || (FirstEqual == -1))
                {
                    var weatherParameter = new WeatherParameters(
                        textBox1.Text,
                        Convert.ToDouble(textBox3.Text),
                        Convert.ToDouble(textBox2.Text) + 273);
                    FlightNoiseWeatherParameters[Selected] = weatherParameter;
                    FlightNoiseWeatherParametersListBox.Items[Selected] = textBox1.Text;
                }
                else
                {
                    MessageBox.Show("Погодные условия с указанным наименованием уже существуют");
                    return;
                }
            }
            else if (sender == DeleteFlightNoiseWeatherParameterButton)
            {
                int Selected = FlightNoiseWeatherParametersListBox.SelectedIndex;
                if (Selected == -1) return;
                FlightNoiseWeatherParameters.RemoveAt(Selected);
                FlightNoiseWeatherParametersListBox.Items.RemoveAt(Selected);
            }
            else if (sender == ApplyFlightNoiseWeatherParameterButton)
                ApplyFlightNoiseWeatherParameters(FlightNoiseWeatherParameters);
        }
        private void FlightNoiseWeatherParametersListBox_Click(object sender, EventArgs e)
        {
            if (FlightNoiseWeatherParametersListBox.SelectedIndex == -1) return;
            var SelectedWeatherParameters = FlightNoiseWeatherParameters[FlightNoiseWeatherParametersListBox.SelectedIndex];
            textBox1.Text = SelectedWeatherParameters.Mounth;
            textBox2.Text = (SelectedWeatherParameters.Temperature - 273).ToString();
            textBox3.Text = SelectedWeatherParameters.Humidity.ToString();
        }
        private void FlightNoiseSoundLevelButton_Click(object sender, EventArgs e)
        {
            if (FlightNoiseSoundLevels == null)
                FlightNoiseSoundLevels = new List<FlightSoundLevel>();
            if (sender == AddFlightNoiseSoundLevelButton)
            {
                double SoundLevel = Convert.ToDouble(textBox5.Text);
                if (FlightNoiseSoundLevels.First(x => x.SoundLevel == SoundLevel) != null)
                {
                    MessageBox.Show("Задайте новое значение уровня шума");
                    return;
                }
                Color color;
                try
                {
                    color = Color.FromArgb(
                        Convert.ToInt32(textBox4.Text),
                        Convert.ToInt32(textBox6.Text),
                        Convert.ToInt32(textBox7.Text),
                        Convert.ToInt32(textBox8.Text));
                }
                catch
                {
                    color = Color.Black;
                }
                FlightNoiseSoundLevels.Add(new FlightSoundLevel(SoundLevel, color));
                FlightNoiseSoundLevelsListBox.Items.Add(SoundLevel.ToString());
            }
            else if (sender == ModifyFlightNoiseSoundLevelButton)
            {
                int Selected = FlightNoiseSoundLevelsListBox.SelectedIndex;
                if (Selected == -1) return;
                double SoundLevel = Convert.ToDouble(textBox5.Text);
                Color color;
                try
                {
                    color = Color.FromArgb(
                        Convert.ToInt32(textBox4.Text),
                        Convert.ToInt32(textBox6.Text),
                        Convert.ToInt32(textBox7.Text),
                        Convert.ToInt32(textBox8.Text));
                }
                catch
                {
                    color = Color.Black;
                }
                var FirstEqual = FlightNoiseSoundLevels.First(x => x.SoundLevel == SoundLevel);
                if ((FirstEqual == FlightNoiseSoundLevels[Selected]) || (FirstEqual == null))
                {
                    FlightNoiseSoundLevels[Selected] = new FlightSoundLevel(SoundLevel, color);
                    FlightNoiseSoundLevelsListBox.Items[Selected] = SoundLevel.ToString();
                }
                else
                {
                    MessageBox.Show("Задайте новое значение уровня шума");
                    return;
                }
            }
            else if (sender == DeleteFlightNoiseSoundLevelButton)
            {
                int Selected = FlightNoiseSoundLevelsListBox.SelectedIndex;
                if (Selected == -1) return;
                FlightNoiseSoundLevels.RemoveAt(Selected);
                FlightNoiseSoundLevelsListBox.Items.RemoveAt(Selected);
            }
            else if (sender == ApplyFlightNoiseSoundLevelsButton)
                ApplyFlightNoiseSoundLevels(FlightNoiseSoundLevels.Select(x => x.SoundLevel).ToList());
        }
        private void FlightNoiseSoundLevelsListBox_Click(object sender, EventArgs e)
        {
            if (FlightNoiseSoundLevelsListBox.SelectedIndex == -1) return;
            var SelectedSoundLevel = FlightNoiseSoundLevels[FlightNoiseSoundLevelsListBox.SelectedIndex];
            textBox5.Text = SelectedSoundLevel.SoundLevel.ToString();
            textBox4.Text = SelectedSoundLevel.Color.A.ToString();
            textBox6.Text = SelectedSoundLevel.Color.R.ToString();
            textBox7.Text = SelectedSoundLevel.Color.G.ToString();
            textBox8.Text = SelectedSoundLevel.Color.B.ToString();
        }
        private void OpenFlightNoiseBallisticsButton_Click(object sender, EventArgs e)
        {
            var OpenDialog = new OpenFileDialog();
            if (OpenDialog.ShowDialog() == DialogResult.OK)
                ((sender == OpenRocketFlightNoiseBallisticsButton) ? RocketFlightNoiseBallisticsTextBox : VehicleFlightNoiseBallisticsTextBox).Text = OpenDialog.FileName;
        }
        private void ApplyPartialFlightNoiseInputDataButton_Click(object sender, EventArgs e)
        {
            ApplyFlightNoisePartialInputDataEventArgs ApplyArgs = (sender == ApplyRocketFlightNoiseInputDataButton) ?
                new ApplyFlightNoisePartialInputDataEventArgs(
                    Part.Rocket,
                    RocketFlightNoiseBallisticsTextBox.Text,
                    GetFlowParameters(Part.Rocket)) :
                new ApplyFlightNoisePartialInputDataEventArgs(
                    Part.Vehicle,
                    VehicleFlightNoiseBallisticsTextBox.Text,
                    GetFlowParameters(Part.Vehicle));
            ApplyFlightNoisePartialInputData(this, ApplyArgs);
        }
        private void SetFlightNoiseSoundLevelColorButton_Click(object sender, EventArgs e)
        {
            var ColorDialog = new ColorDialog();
            try
            {
                ColorDialog.Color = Color.FromArgb(
                    Convert.ToByte(textBox4.Text),
                    Convert.ToByte(textBox6.Text),
                    Convert.ToByte(textBox7.Text),
                    Convert.ToByte(textBox8.Text));
            }
            catch
            {
                ColorDialog.Color = Color.Black;
            }
            if (ColorDialog.ShowDialog() == DialogResult.OK)
            {
                textBox4.Text = ColorDialog.Color.A.ToString();
                textBox6.Text = ColorDialog.Color.R.ToString();
                textBox7.Text = ColorDialog.Color.G.ToString();
                textBox8.Text = ColorDialog.Color.B.ToString();
            }
        }
        public void SetFlightSoundLevelProgress(int Progress)
        {
            this.Invoke(new Action(() => FlightNoiseProgressBar.Value = Progress));
        }

        public event EventHandler<OpenFlightNoiseInputDataEventArgs> OpenFlightNoiseInputData;
        public event EventHandler<SaveFlightNoiseInputDataEventArgs> SaveFlightNoiseInputData;
        public event EventHandler<CalculateFlightNoiseEventArgs> CalculateFlightNoise;
        public event EventHandler<ApplyFlightNoisePartialInputDataEventArgs> ApplyFlightNoisePartialInputData;
        public event Action<List<WeatherParameters>> ApplyFlightNoiseWeatherParameters;
        public event Action<List<double>> ApplyFlightNoiseSoundLevels;
    }
}