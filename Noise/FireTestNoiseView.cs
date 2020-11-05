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
using Excel = Microsoft.Office.Interop.Excel;

namespace Noise
{
    public partial class MainForm : Form, IMainView
    {
        public void InitializeFireTestNoiseView()
        {
            FireTestGraphPane = zedGraphControl2.GraphPane;
            FireTestGraphPane.IsFontsScaled = false;
            FireTestGraphPane.Title.IsVisible = false;
            FireTestGraphPane.XAxis.Title.Text = "X, м";
            FireTestGraphPane.XAxis.MajorGrid.IsVisible = true;
            FireTestGraphPane.XAxis.MinorGrid.IsVisible = true;
            FireTestGraphPane.XAxis.MajorGrid.DashOff = 1;
            FireTestGraphPane.XAxis.MinorGrid.DashOff = 1;
            FireTestGraphPane.YAxis.Title.Text = "Y, м";
            FireTestGraphPane.YAxis.MajorGrid.IsVisible = true;
            FireTestGraphPane.YAxis.MinorGrid.IsVisible = true;
            FireTestGraphPane.YAxis.MajorGrid.DashOff = 1;
            FireTestGraphPane.YAxis.MinorGrid.DashOff = 1;
            FireTestNoiseWeatherParametersContainer = new WeatherParametersContainer(
                textBox16,
                textBox14,
                textBox15,
                AddFireTestNoiseWeatherParameterButton,
                ModifyFireTestNoiseWeatherParameterButton,
                DeleteFireTestNoiseWeatherParameterButton,
                ApplyFireTestNoiseWeatherParametersButton,
                FireTestNoiseWeatherParametersListBox,
                ApplyFireTestNoiseWeatherParameters);
            FireTestNoiseFlowParametersContainer = new FlowParametersContainer(
                FireTestNoiseMassFlowTextBox,
                FireTestNoiseNozzleDiameterTextBox,
                FireTestNoiseNozzleMachNumberTextBox,
                FireTestNoiseNozzleFlowVelocityTextBox,
                FireTestNoiseChamberSoundVelocityTextBox,
                FireTestNoiseNozzleAdiabaticIndexTextBox);
            FireTestNoiseSoundLevelContainer = new SoundLevelContainer(
                textBox9,
                textBox10,
                textBox11,
                textBox12,
                textBox13,
                AddFireTestNoiseSoundLevelButton,
                ModifyFireTestNoiseSoundLevelButton,
                DeleteFireTestNoiseSoundLevelButton,
                ApplyFireTestNoiseSoundLevelsButton,
                SetFireTestNoiseSoundLevelColorButton,
                FireTestNoiseSoundLevelsListBox,
                ApplyFireTestNoiseSoundLevels);
            foreach (var grid in GetControls(FlightNoiseTabPage, typeof(DataGridView)).Cast<DataGridView>())
            {
                grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                for (int i = 0; i < grid.Columns.Count; i++)
                    grid.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            }
            Dictionary<Button, Control> DropDownControls = new Dictionary<Button, Control>()
            {
                { FireTestNoiseEngineParametersButton, FireTestNoiseEngineParametersPanel },
                { FireTestFrequencyBandButton, FireTestFrequencyBandPanel },
                { FireTestCalculationAreaButton, FireTestCalculationAreaPanel },
                { FireTestNoiseWeatherParametersButton, FireTestNoiseWeatherParametersPanel },
                { FireTestNoiseLevelsButton, FireTestNoiseLevelsPanel }
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
        GraphPane FireTestGraphPane;
        WeatherParametersContainer FireTestNoiseWeatherParametersContainer;
        FlowParametersContainer FireTestNoiseFlowParametersContainer;
        SoundLevelContainer FireTestNoiseSoundLevelContainer;
        private void FireTestFrequencyBandRadioButton_Click(object sender, EventArgs e)
        {
            FireTestNoiseLevelLabel.Text = "Уровень шума, дБ";
            if (FireTestNormalBandRadioButton.Checked)
                FireTestNoiseLevelLabel.Text += "A";
        }
        private void SaveFireTestNoiseInputDataMenuItem_Click()
        {
            SaveFileDialog Dialog = new SaveFileDialog();
            if (Dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                SaveFireTestNoiseInputData(new SaveFireTestNoiseInputDataEventArgs(
                    Dialog.FileName,
                    Convert.ToDouble(FireTestNoiseThrustTextBox.Text) * 1E3,
                    FireTestNoiseFlowParametersContainer.FlowParameters,
                    FireTestNoiseWeatherParametersContainer.WeatherParameters,
                    FireTestNoiseSoundLevelContainer.SoundLevels.ToDictionary(x => x.SoundLevel, x => x.Color),
                    new RadiusInterval(
                        Convert.ToDouble(FireTestStartRadiusTextBox.Text),
                        Convert.ToDouble(FireTestFinalRadiusTextBox.Text),
                        Convert.ToDouble(FireTestRadiusStepTextBox.Text)),
                    FireTestNormalBandRadioButton.Checked ? 
                        FrequencyBand.Normal : 
                        FireTestUltraBandRadioButton.Checked ? FrequencyBand.Ultra : FrequencyBand.Infra));
        }
        private void SetFireTestNoiseInputData(FireTestNoiseInputData id)
        {
            FireTestNoiseThrustTextBox.Text = (id.Thrust * 1E-3).ToString();
            FireTestNoiseFlowParametersContainer.FlowParameters = id.FlowParameters;
            FireTestNoiseWeatherParametersContainer.WeatherParameters = id.WeatherParameters;
            FireTestNoiseSoundLevelContainer.SoundLevels = id.SoundLevels.Select(x => new ColoredSoundLevel(x.Key, x.Value)).ToList();
            FireTestStartRadiusTextBox.Text = id.RadiusInterval.Initial.ToString();
            FireTestFinalRadiusTextBox.Text = id.RadiusInterval.Final.ToString();
            FireTestRadiusStepTextBox.Text = id.RadiusInterval.Step.ToString();
            RadioButton checkRadioButton;
            if (id.FrequencyBand == FrequencyBand.Infra)
                checkRadioButton = FireTestInfraBandRadioButton;
            else if (id.FrequencyBand == FrequencyBand.Ultra)
                checkRadioButton = FireTestUltraBandRadioButton;
            else
                checkRadioButton = FireTestNormalBandRadioButton;
            checkRadioButton.Checked = true;
            FireTestFrequencyBandRadioButton_Click(this, EventArgs.Empty);
            ApplyFireTestNoisePartialInputData(new ApplyEngineParametersEventArgs(id.Thrust, id.FlowParameters));
            ApplyFireTestNoiseWeatherParameters(id.WeatherParameters);
            ApplyFireTestNoiseSoundLevels(id.SoundLevels.Select(x => x.Key).ToList());
        }
        private void OpenFireTestNoiseInputDataMenuItem_Click()
        {
            OpenFileDialog Dialog = new OpenFileDialog();
            if (Dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var InputDataEventArgs = new OpenFireTestNoiseInputDataEventArgs(Dialog.FileName);
                OpenFireTestNoiseInputData(InputDataEventArgs);
                SetFireTestNoiseInputData(InputDataEventArgs.FireTestNoiseInputData);
            }
        }
        private void FireTestNoiseMenu_Click(object sender, EventArgs e)
        {
            FrequencyBand FrequencyBand;
            if (sender == FireTestNoiseNormalMenu)
                FrequencyBand = FrequencyBand.Normal;
            else if (sender == FireTestNoiseUltraMenu)
                FrequencyBand = FrequencyBand.Ultra;
            else
                FrequencyBand = FrequencyBand.Infra;
            var InputDataEventArgs = new FireTestNoiseExampleEventArgs(FrequencyBand);
            FireTestNoiseExample(InputDataEventArgs);
            SetFireTestNoiseInputData(InputDataEventArgs.InputData);
            TabControl.SelectedIndex = 1;
            CalculatFireTestNoiseMenuItem_Click();
        }
        private async void CalculatFireTestNoiseMenuItem_Click()
        {
            FrequencyBand frequencyBand;
            if (FireTestInfraBandRadioButton.Checked)
                frequencyBand = FrequencyBand.Infra;
            else if (FireTestUltraBandRadioButton.Checked)
                frequencyBand = FrequencyBand.Ultra;
            else
                frequencyBand = FrequencyBand.Normal;
            var CalculateArgs = new CalculateFireTestNoiseEventArgs(
                frequencyBand,
                new RadiusInterval(
                    Convert.ToDouble(FireTestStartRadiusTextBox.Text),
                    Convert.ToDouble(FireTestFinalRadiusTextBox.Text),
                    Convert.ToDouble(FireTestRadiusStepTextBox.Text)));
            await Task.Run(() => CalculateFireTestNoise(CalculateArgs));
            var Contours = CalculateArgs.FireTestSoundContours;
            FireTestGraphPane.CurveList.Clear();
            for (int i = 0; i < FireTestNoiseSoundLevelContainer.SoundLevels.Count; i++)
                FireTestGraphPane.AddCurve(
                    FireTestNoiseSoundLevelContainer.SoundLevels[i].SoundLevel.ToString(),
                    Contours[i].Points.Select(p => p.X).ToArray(),
                    Contours[i].Points.Select(p => p.Y).ToArray(),
                    FireTestNoiseSoundLevelContainer.SoundLevels[i].Color,
                    SymbolType.None);
            FireTestGraphPane.AxisChange();
            zedGraphControl2.Invalidate();
            var PointsGrid = FireTestNoiseSoundLevelPointsGrid;
            PointsGrid.Columns.Clear();
            foreach (var circle in Contours)
            {
                var s = circle.SoundLevel.ToString() + " дБ";
                if (FireTestNormalBandRadioButton.Checked)
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
            PointsGrid.Rows.Add(Contours[0].Points.Count);
            for (int CircleIndex = 0; CircleIndex < Contours.Count; CircleIndex++)
            {
                var contour = Contours[CircleIndex];
                for (int i = 0; i < contour.Points.Count; i++)
                {
                    PointsGrid.Rows[i + 1].Cells[2 * CircleIndex + 0].Value = FormatConvert.ToString(contour.Points[i].X);
                    PointsGrid.Rows[i + 1].Cells[2 * CircleIndex + 1].Value = FormatConvert.ToString(contour.Points[i].Y);
                }
            }
        }
        private void ApplyPartialFireTestNoiseInputDataButton_Click(object sender, EventArgs e)
        {
            var ApplyArgs = new ApplyEngineParametersEventArgs(
                Convert.ToDouble(FireTestNoiseThrustTextBox.Text) * 1E3,
                FireTestNoiseFlowParametersContainer.FlowParameters);
            ApplyFireTestNoisePartialInputData(ApplyArgs);
        }
        public void SetFireTestNoiseCalculationlProgress(int Progress)
        {
            this.Invoke(new Action(() => FireTestNoiseProgressBar.Value = Progress));
        }

        public event Action<OpenFireTestNoiseInputDataEventArgs> OpenFireTestNoiseInputData;
        public event Action<SaveFireTestNoiseInputDataEventArgs> SaveFireTestNoiseInputData;
        public event Action<CalculateFireTestNoiseEventArgs> CalculateFireTestNoise;
        public event Action<ApplyEngineParametersEventArgs> ApplyFireTestNoisePartialInputData;
        public event Action<List<WeatherParameters>> ApplyFireTestNoiseWeatherParameters;
        public event Action<List<double>> ApplyFireTestNoiseSoundLevels;
        public event Action<FireTestNoiseExampleEventArgs> FireTestNoiseExample;
    }
}