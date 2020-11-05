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
        public void InitializeSonicBoomView()
        {
            SonicBoomWeatherParametersContainer = new WeatherParametersContainer(
                textBox24,
                textBox22,
                textBox23,
                AddSonicBoomWeatherParameterButton,
                ModifySonicBoomWeatherParameterButton,
                DeleteSonicBoomWeatherParameterButton,
                ApplySonicBoomWeatherParametersButton,
                SonicBoomWeatherParametersListBox,
                x => SonicBoomWeatherParameters = x);
            foreach (var grid in GetControls(SonicBoomTabPage, typeof(DataGridView)).Cast<DataGridView>())
            {
                grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                for (int i = 0; i < grid.Columns.Count; i++)
                    grid.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            }
            VehicleSonicBoomBallisticsForm = new SonicBoomBallisticsForm(Part.Vehicle);
            RocketSonicBoomBallisticsForm = new SonicBoomBallisticsForm(Part.Rocket);
            Dictionary<Button, Control> DropDownControls = new Dictionary<Button, Control>()
            {
                { RocketSonicBoomParametersButton, RocketSonicBoomParametersPanel },
                { VehicleSonicBoomParametersButton, VehicleSonicBoomParametersPanel },
                { SonicBoomWeatherParametersButton, SonicBoomWeatherParametersPanel }
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
        WeatherParametersContainer SonicBoomWeatherParametersContainer;
        List<WeatherParameters> SonicBoomWeatherParameters;
        GeometricalParameters RocketGeometricalParameters;
        GeometricalParameters VehicleGeometricalParameters;
        SonicBoomBallistics RocketSonicBoomBallistics;
        SonicBoomBallistics VehicleSonicBoomBallistics;
        SonicBoomBallisticsForm RocketSonicBoomBallisticsForm;
        SonicBoomBallisticsForm VehicleSonicBoomBallisticsForm;
        void GetTextBoxes(
            Part Part,
            out TextBox Length,
            out TextBox CharacteristicLength,
            out TextBox MaximalArea,
            out TextBox CharactericticsArea)
        {
            if (Part == Part.Rocket)
            {
                Length = textBox38;
                CharacteristicLength = textBox44;
                MaximalArea = textBox43;
                CharactericticsArea = textBox42;
            }
            else
            {
                Length = textBox45;
                CharacteristicLength = textBox50;
                MaximalArea = textBox49;
                CharactericticsArea = textBox48;
            }
        }
        private void ApplyPartialSonicBoomInputDataButton_Click(object sender, EventArgs e)
        {
            Func<Part, GeometricalParameters> GetGeometricalParameters = Part =>
            {
                TextBox Length, CharacteristicLength, MaximalArea, CharactericticArea;
                if (Part == Part.Rocket)
                    GetTextBoxes(Part.Rocket, out Length, out CharacteristicLength, out MaximalArea, out CharactericticArea);
                else
                    GetTextBoxes(Part.Vehicle, out Length, out CharacteristicLength, out MaximalArea, out CharactericticArea);
                return new GeometricalParameters(
                    Convert.ToDouble(Length.Text),
                    Convert.ToDouble(CharacteristicLength.Text),
                    Convert.ToDouble(MaximalArea.Text),
                    Convert.ToDouble(CharactericticArea.Text));
            };
            if (sender == ApplyRocketSonicBoomInputDataButton)
            {
                RocketGeometricalParameters = GetGeometricalParameters(Part.Rocket);
                RocketSonicBoomBallistics = RocketSonicBoomBallisticsForm.Ballistics;
            }
            else
            {
                VehicleGeometricalParameters = GetGeometricalParameters(Part.Vehicle);
                VehicleSonicBoomBallistics = VehicleSonicBoomBallisticsForm.Ballistics;
            }
        }
        private void SaveSonicBoomInputDataMenuItem_Click()
        {
            SaveFileDialog Dialog = new SaveFileDialog();
            if (Dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SaveSonicBoomInputData(new SaveSonicBoomInputDataEventArgs(
                    Dialog.FileName,
                    RocketSonicBoomBallistics,
                    VehicleSonicBoomBallistics,
                    RocketGeometricalParameters,
                    VehicleGeometricalParameters,
                    SonicBoomWeatherParametersContainer.WeatherParameters));
            }
        }
        private void SetSonicBoomInputData(SonicBoomInputData id)
        {
            Action<Part, GeometricalParameters, SonicBoomBallistics> SetPartialInputData = (part, GeometricalParameters, Ballistics) =>
            {
                TextBox Length, CharacteristicLength, MaximalArea, CharactericticArea;
                if (part == Part.Rocket)
                {
                    GetTextBoxes(Part.Rocket, out Length, out CharacteristicLength, out MaximalArea, out CharactericticArea);
                    RocketSonicBoomBallisticsForm.Ballistics = Ballistics;
                    RocketSonicBoomBallistics = Ballistics;
                    RocketGeometricalParameters = GeometricalParameters;
                }
                else
                {
                    GetTextBoxes(Part.Vehicle, out Length, out CharacteristicLength, out MaximalArea, out CharactericticArea);
                    VehicleSonicBoomBallisticsForm.Ballistics = Ballistics;
                    VehicleSonicBoomBallistics = Ballistics;
                    VehicleGeometricalParameters = GeometricalParameters;
                }
                Length.Text = GeometricalParameters.Length.ToString();
                CharacteristicLength.Text = GeometricalParameters.CharacteristicLength.ToString();
                MaximalArea.Text = GeometricalParameters.MaximalArea.ToString();
                CharactericticArea.Text = GeometricalParameters.CharacteristicLength.ToString();
            };
            SetPartialInputData(Part.Rocket, id.RocketGeometricalParameters, id.RocketBallistics);
            SetPartialInputData(Part.Vehicle, id.VehicleGeometricalParameters, id.VehicleBallistics);
            SonicBoomWeatherParametersContainer.WeatherParameters = id.WeatherParameters;
            SonicBoomWeatherParameters = id.WeatherParameters;
        }
        private void OpenSonicBoomInputDataMenuItem_Click()
        {
            OpenFileDialog Dialog = new OpenFileDialog();
            if (Dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var InputDataEventArgs = new OpenSonicBoomInputDataEventArgs(Dialog.FileName);
                OpenSonicBoomInputData(InputDataEventArgs);
                SetSonicBoomInputData(InputDataEventArgs.InputData);
            }
        }
        private void SonicBoomMenu_Click(object sender, EventArgs e)
        {
            var InputDataEventArgs = new SonicBoomExampleEventArgs();
            SonicBoomExample(InputDataEventArgs);
            SetSonicBoomInputData(InputDataEventArgs.InputData);
            TabControl.SelectedIndex = 2;
            CalculateSonicBoomMenuItem_Click();
        }
        private async void CalculateSonicBoomMenuItem_Click()
        {
            var CalculateArgs = new CalculateSonicBoomEventArgs(
                RocketGeometricalParameters, 
                VehicleGeometricalParameters, 
                RocketSonicBoomBallistics, 
                VehicleSonicBoomBallistics, 
                SonicBoomWeatherParameters);
            await Task.Run(() => CalculateSonicBoom(CalculateArgs));
            Action<List<SonicBoomParameters>, DataGridView> SetResults = (SonicBoomParameters, Grid) =>
            {
                Grid.Rows.Clear();
                if ((SonicBoomParameters == null) || (SonicBoomParameters.Count == 0)) return;
                Grid.Rows.Add(SonicBoomParameters.Count);
                for (int i = 0; i < SonicBoomParameters.Count; i++)
                {
                    var Row = Grid.Rows[i];
                    var sbp = SonicBoomParameters[i];
                    Row.Cells[0].Value = FormatConvert.ToString(sbp.Time);
                    Row.Cells[1].Value = FormatConvert.ToString(sbp.Height * 1E-3);
                    Row.Cells[2].Value = FormatConvert.ToString(sbp.MachNumber);
                    Row.Cells[3].Value = FormatConvert.ToString(sbp.OverPressure);
                    Row.Cells[4].Value = FormatConvert.ToString(sbp.SoundLevel);
                    Row.Cells[5].Value = FormatConvert.ToString(sbp.ImpactDistance * 1E-3);
                    Row.Cells[6].Value = FormatConvert.ToString(sbp.ImpactDuration);
                }
            };
            SetResults(CalculateArgs.RocketSonicBoomParameters,  RocketSonicBoomGrid);
            SetResults(CalculateArgs.VehicleSonicBoomParameters, VehicleSonicBoomGrid);
        }
        private void OpenSonicBallisticsButton_Click(object sender, EventArgs e)
        {
            if (sender == OpenRocketSonicBoomBallisticsButton)
                RocketSonicBoomBallisticsForm.ShowDialog();
            else
                VehicleSonicBoomBallisticsForm.ShowDialog();
        }
        public void SetSonicBoomCalculationProgress(int Progress)
        {
            this.Invoke(new Action(() => progressBar2.Value = Progress));
        }
        public event Action<OpenSonicBoomInputDataEventArgs> OpenSonicBoomInputData;
        public event Action<SaveSonicBoomInputDataEventArgs> SaveSonicBoomInputData;
        public event Action<CalculateSonicBoomEventArgs> CalculateSonicBoom;
        public event Action<SonicBoomExampleEventArgs> SonicBoomExample;
    }
}
