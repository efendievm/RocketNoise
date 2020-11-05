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
        public void InitializeEngineNoiseView()
        {
            EngineFrequencyGraphPane = zedGraphControl3.GraphPane;
            EngineFrequencyGraphPane.IsFontsScaled = false;
            EngineFrequencyGraphPane.Title.IsVisible = false;
            EngineFrequencyGraphPane.XAxis.Title.Text = "Частота, Гц";
            EngineFrequencyGraphPane.XAxis.Type = AxisType.Log;
            EngineFrequencyGraphPane.XAxis.MajorGrid.IsVisible = true;
            EngineFrequencyGraphPane.XAxis.MinorGrid.IsVisible = true;
            EngineFrequencyGraphPane.XAxis.MajorGrid.DashOff = 1;
            EngineFrequencyGraphPane.XAxis.MinorGrid.DashOff = 1;
            EngineFrequencyGraphPane.YAxis.Title.Text = "Уровень шума, дБ";
            EngineFrequencyGraphPane.YAxis.MajorGrid.IsVisible = true;
            EngineFrequencyGraphPane.YAxis.MinorGrid.IsVisible = true;
            EngineFrequencyGraphPane.YAxis.MajorGrid.DashOff = 1;
            EngineFrequencyGraphPane.YAxis.MinorGrid.DashOff = 1;

            RadiationPatternGraphPane = zedGraphControl4.GraphPane;
            RadiationPatternGraphPane.IsFontsScaled = false;
            RadiationPatternGraphPane.Title.IsVisible = false;
            RadiationPatternGraphPane.XAxis.Title.Text = "Угол, град";
            RadiationPatternGraphPane.XAxis.MajorGrid.IsVisible = true;
            RadiationPatternGraphPane.XAxis.MinorGrid.IsVisible = true;
            RadiationPatternGraphPane.XAxis.MajorGrid.DashOff = 1;
            RadiationPatternGraphPane.XAxis.MinorGrid.DashOff = 1;
            RadiationPatternGraphPane.YAxis.Title.Text = "Уровень шума, дБ";
            RadiationPatternGraphPane.YAxis.MajorGrid.IsVisible = true;
            RadiationPatternGraphPane.YAxis.MinorGrid.IsVisible = true;
            RadiationPatternGraphPane.YAxis.MajorGrid.DashOff = 1;
            RadiationPatternGraphPane.YAxis.MinorGrid.DashOff = 1;

            EngineNoiseFlowParametersContainer = new FlowParametersContainer(
                textBox31,
                textBox30,
                textBox29,
                textBox28,
                textBox27,
                textBox26);
            dataGridView4.Rows.Add("Мощность механическая газовой струи в выходном сечении сопла, МВт");
            dataGridView4.Rows.Add("Мощность звука газовой струи, МВт");
            dataGridView4.Rows.Add("Соотношение мощности механической в выходном сечении сопла к мощности звука газовой струи, %");
            dataGridView4.Rows.Add("Угол полураствора конуса максимальной мощности звука газовой струи, αст.изл, угл.град.");
            dataGridView4.Rows.Add("Расстояние от выходного сечения сопла до конца участка газовой струи с невозмущённым сверхзвуковым потоком, м");
            dataGridView4.Rows.Add("Расстояние от выходного сечения сопла до точки наибольшей интенсивности излучения звука газовой струи, м");
            dataGridView4.Rows.Add("Расстояние от выходного сечения сопла до конца участка газовой струи со сверхзвуковым потоком, м");
            dataGridView4.Rows.Add("Расстояние от выходного сечения сопла до конца участка газовой струи, излучающего ~98% звуковой мощности, м");
            dataGridView4.Rows.Add("Расстояние от выходного сечения сопла до места полного разрушения газовой струи, м");
            Hint = new ToolTip();
            OldPosition = new System.Drawing.Point(0, 0);
            Dictionary<Button, Control> DropDownControls = new Dictionary<Button, Control>()
            {
                { EngineAcousticsParametersButton, EngineAcousticsParametersPanel },
                { CountourGraphicParametersButton, CountourGraphicParametersPanel }
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
        GraphPane EngineFrequencyGraphPane;
        GraphPane RadiationPatternGraphPane;
        DoubleInterpolation Contour;
        FlowParametersContainer EngineNoiseFlowParametersContainer;
        ToolTip Hint;
        System.Drawing.Point OldPosition;
        private void Grid_Scroll(object sender, ScrollEventArgs e)
        {
            if (sender == dataGridView1)
                dataGridView3.FirstDisplayedScrollingRowIndex = dataGridView1.FirstDisplayedScrollingRowIndex;
            else if (sender == dataGridView2)
                dataGridView3.FirstDisplayedScrollingColumnIndex = dataGridView2.FirstDisplayedScrollingColumnIndex;
            else if (sender == dataGridView3)
            {
                if (e.ScrollOrientation == ScrollOrientation.VerticalScroll)
                    dataGridView1.FirstDisplayedScrollingRowIndex = dataGridView3.FirstDisplayedScrollingRowIndex;
                else
                    dataGridView2.FirstDisplayedScrollingColumnIndex = dataGridView3.FirstDisplayedScrollingColumnIndex;
            }
        }
        private void ApplyEngineNoiseInputDataButton_Click(object sender, EventArgs e)
        {
            var ApplyArgs = new ApplyEngineParametersEventArgs(
                Convert.ToDouble(textBox25.Text) * 1E3,
                EngineNoiseFlowParametersContainer.FlowParameters);
            ApplyEngineNoisePartialInputData(ApplyArgs);
        }
        private void ApplyEngineNoiseContourParametersButton_Click(object sender, EventArgs e)
        {
            ApplyEngineNoiseContourParameters(new ApplyEngineNoiseContourParametersEventArgs(
                ConvertToDouble(textBox17),
                ConvertToDouble(textBox18),
                ConvertToDouble(textBox19),
                ConvertToDouble(textBox21),
                ConvertToDouble(textBox20)
            ));
        }
        private double ConvertToDouble(TextBox TextBox)
        {
            try
            {
                return Convert.ToDouble(TextBox.Text);
            }
            catch
            {
                return 0;
            }
        }
        private void ConvertToString(TextBox TextBox, double Value)
        {
            TextBox.Text = Value != 0 ? Value.ToString() : "";
        }
        private void SaveEngineNoiseInputDataMenuItem_Click()
        {
            SaveFileDialog Dialog = new SaveFileDialog();
            if (Dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                SaveEngineNoiseInputData(new SaveEngineNoiseInputDataEventArgs(
                    Dialog.FileName,
                    Convert.ToDouble(FireTestNoiseThrustTextBox.Text) * 1E3,
                    FireTestNoiseFlowParametersContainer.FlowParameters,
                    ConvertToDouble(textBox17),
                    ConvertToDouble(textBox18),
                    ConvertToDouble(textBox19),
                    ConvertToDouble(textBox21),
                    ConvertToDouble(textBox20)));
        }
        private void SetEngineNoiseInputData(EngineNoiseInputData id)
        {
            textBox25.Text = (id.Thrust * 1E-3).ToString();
            EngineNoiseFlowParametersContainer.FlowParameters = id.FlowParameters;
            ConvertToString(textBox17, id.ContourAreaWidth);
            ConvertToString(textBox18, id.ContourAreaHeight);
            ConvertToString(textBox19, id.NozzleCoordinate);
            ConvertToString(textBox21, id.MinSoundLevel);
            ConvertToString(textBox20, id.MaxSoundLevel);
            ApplyEngineNoisePartialInputData(new ApplyEngineParametersEventArgs(id.Thrust, id.FlowParameters));
            ApplyEngineNoiseContourParameters(new ApplyEngineNoiseContourParametersEventArgs(
                id.ContourAreaWidth,
                id.ContourAreaHeight,
                id.NozzleCoordinate,
                id.MinSoundLevel,
                id.MaxSoundLevel));
        }
        private void OpenEngineNoiseInputDataMenuItem_Click()
        {
            OpenFileDialog Dialog = new OpenFileDialog();
            if (Dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var InputDataEventArgs = new OpenEngineNoiseInputDataEventArgs(Dialog.FileName);
                OpenEngineNoiseInputData(InputDataEventArgs);
                SetEngineNoiseInputData(InputDataEventArgs.InputData);
            }
        }
        private void EngineAcousticsMenu_Click(object sender, EventArgs e)
        {
            var InputDataEventArgs = new EngineNoiseExampleEventArgs();
            EngineNoiseExample(InputDataEventArgs);
            SetEngineNoiseInputData(InputDataEventArgs.InputData);
            TabControl.SelectedIndex = 3;
            CalculatEngineNoiseMenuItem_Click();
        }
        private async void CalculatEngineNoiseMenuItem_Click()
        {
            dataGridView1.Scroll -= Grid_Scroll;
            dataGridView2.Scroll -= Grid_Scroll;
            dataGridView3.Scroll -= Grid_Scroll;
            var CalculateArgs = new CalculateEngineNoiseEventArgs();
            await Task.Run(() => CalculateEngineNoise(CalculateArgs));
            dataGridView4.Rows[0].Cells[1].Value = FormatConvert.ToString(CalculateArgs.FlowSoundParameters.MechanicalPower * 1E-6);
            dataGridView4.Rows[1].Cells[1].Value = FormatConvert.ToString(CalculateArgs.FlowSoundParameters.SoundPower * 1E-6);
            dataGridView4.Rows[2].Cells[1].Value = FormatConvert.ToString(CalculateArgs.FlowSoundParameters.SoundPowerRatio);
            dataGridView4.Rows[3].Cells[1].Value = FormatConvert.ToString(CalculateArgs.FlowSoundParameters.SoundMaximalPowerConeHalfAngle);
            dataGridView4.Rows[4].Cells[1].Value = FormatConvert.ToString(CalculateArgs.FlowSoundParameters.UndisturbedSupersonicFlowLength);
            dataGridView4.Rows[5].Cells[1].Value = FormatConvert.ToString(CalculateArgs.FlowSoundParameters.DistanceToPointOfMaximalSoundLevel);
            dataGridView4.Rows[6].Cells[1].Value = FormatConvert.ToString(CalculateArgs.FlowSoundParameters.SupersonicFlowLength);
            dataGridView4.Rows[7].Cells[1].Value = FormatConvert.ToString(CalculateArgs.FlowSoundParameters._98ProzentSoundPowerRadiatingFlowLength);
            dataGridView4.Rows[8].Cells[1].Value = FormatConvert.ToString(CalculateArgs.FlowSoundParameters.DistanceToPointOfFlowDestruction);
            EngineFrequencyGrid.Rows.Clear();
            EngineFrequencyGrid.Rows.Add(CalculateArgs.FrequencyCharacteristik.Keys.Count);
            int i = 0;
            foreach (var FrequencyCharacteristik in CalculateArgs.FrequencyCharacteristik)
            {
                EngineFrequencyGrid.Rows[i].Cells[0].Value = FormatConvert.ToString(FrequencyCharacteristik.Key);
                EngineFrequencyGrid.Rows[i].Cells[1].Value = FormatConvert.ToString(FrequencyCharacteristik.Value);
                i++;
            }
            EngineFrequencyGraphPane.CurveList.Clear();
            EngineFrequencyGraphPane.AddCurve(
                "",
                CalculateArgs.FrequencyCharacteristik.Select(x => x.Key).ToArray(),
                CalculateArgs.FrequencyCharacteristik.Select(x => x.Value).ToArray(),
                Color.Black,
                SymbolType.None);
            EngineFrequencyGraphPane.AxisChange();
            zedGraphControl3.Invalidate();
            RadiationPatternGrid.Rows.Clear();
            RadiationPatternGrid.Rows.Add(CalculateArgs.RadiationPattern.Keys.Count);
            i = 0;
            foreach (var RadiationPattern in CalculateArgs.RadiationPattern)
            {
                RadiationPatternGrid.Rows[i].Cells[0].Value = FormatConvert.ToString(RadiationPattern.Key);
                RadiationPatternGrid.Rows[i].Cells[1].Value = FormatConvert.ToString(RadiationPattern.Value);
                i++;
            }
            RadiationPatternGraphPane.CurveList.Clear();
            RadiationPatternGraphPane.AddCurve(
                "",
                CalculateArgs.RadiationPattern.Select(x => x.Key).ToArray(),
                CalculateArgs.RadiationPattern.Select(x => x.Value).ToArray(),
                Color.Black,
                SymbolType.None);
            RadiationPatternGraphPane.AxisChange();
            zedGraphControl4.Invalidate();
            EngineAcousticsLoadGrid.Rows.Clear();
            EngineAcousticsLoadGrid.Rows.Add(CalculateArgs.EngineAcousticsLoadAtFrequency.Keys.Count);
            i = 0;
            foreach (var EngineAcousticsLoad in CalculateArgs.EngineAcousticsLoadAtFrequency)
            {
                EngineAcousticsLoadGrid.Rows[i].Cells[0].Value = FormatConvert.ToString(EngineAcousticsLoad.Key);
                EngineAcousticsLoadGrid.Rows[i].Cells[1].Value = FormatConvert.ToString(EngineAcousticsLoad.Value);
                i++;
            }
            textBox32.Text = FormatConvert.ToString(CalculateArgs.EngineAcousticsLoadSummary);
            pictureBox1.Image = CalculateArgs.EngineSoundContour.Contour;
            Contour = new DoubleInterpolation(CalculateArgs.EngineSoundContour.X, CalculateArgs.EngineSoundContour.Y, CalculateArgs.EngineSoundContour.SoundLevels);
            dataGridView2.Columns.Clear();
            dataGridView3.Columns.Clear();
            for (int j = 0; j < CalculateArgs.EngineSoundContour.Y.Length; j++)
            {
                dataGridView2.Columns.Add("", "");
                dataGridView3.Columns.Add("", "");
            }
            foreach (var column in dataGridView2.Columns.Cast<DataGridViewColumn>())
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            foreach (var column in dataGridView3.Columns.Cast<DataGridViewColumn>())
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView2.Rows.Add();
            for (int j = 0; j < CalculateArgs.EngineSoundContour.Y.Length; j++)
                dataGridView2.Rows[0].Cells[j].Value = FormatConvert.ToString(CalculateArgs.EngineSoundContour.Y[j]);
            dataGridView1.Rows.Clear();
            dataGridView1.Rows.Add(CalculateArgs.EngineSoundContour.X.Length);
            for (i = 0; i < CalculateArgs.EngineSoundContour.X.Length; i++)
            {
                dataGridView1.Rows[i].Cells[0].Value = FormatConvert.ToString(CalculateArgs.EngineSoundContour.X[i]);
                dataGridView3.Rows.Add();
                for (int j = 0; j < CalculateArgs.EngineSoundContour.Y.Length; j++)
                    dataGridView3.Rows[i].Cells[j].Value = FormatConvert.ToString(CalculateArgs.EngineSoundContour.SoundLevels[i, j]);
            }
            dataGridView1.Scroll += Grid_Scroll;
            dataGridView2.Scroll += Grid_Scroll;
            dataGridView3.Scroll += Grid_Scroll;
        }
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (Contour == null) return;
            double mux = ConvertToDouble(textBox17) / pictureBox1.Width;
            double muy = ConvertToDouble(textBox18) / pictureBox1.Height;
            double x = e.X * mux;
            double y = (pictureBox1.Height - e.Y) * muy;
            if (e.Location != OldPosition)
            {
                OldPosition = e.Location;
                Hint.Active = true;
                Hint.SetToolTip(
                    pictureBox1,
                    String.Format(
                        "X = {0 : 0.##} м; Y = {1 : 0.##} м; L = {2 : 0.##} дБ",
                        x - ConvertToDouble(textBox19), y - ConvertToDouble(textBox18) / 2, Contour.Interpolate(x, y)));
            }
        }
        public void SetEngineNoiseCalculationlProgress(int Progress)
        {
            this.Invoke(new Action(() => progressBar1.Value = Progress));
        }
        public event Action<OpenEngineNoiseInputDataEventArgs> OpenEngineNoiseInputData;
        public event Action<SaveEngineNoiseInputDataEventArgs> SaveEngineNoiseInputData;
        public event Action<CalculateEngineNoiseEventArgs> CalculateEngineNoise;
        public event Action<ApplyEngineParametersEventArgs> ApplyEngineNoisePartialInputData;
        public event Action<ApplyEngineNoiseContourParametersEventArgs> ApplyEngineNoiseContourParameters;
        public event Action<EngineNoiseExampleEventArgs> EngineNoiseExample;
    }
}
