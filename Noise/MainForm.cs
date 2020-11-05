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
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            var TextBoxes = GetControls(this, typeof(TextBox)).Cast<TextBox>();
            foreach (var textBox in TextBoxes)
            {
                textBox.Multiline = true;
                textBox.WordWrap = false;
            }
        }
        private void SaveInputDataMenuItem_Click(object sender, EventArgs e)
        {
            if (TabControl.SelectedTab == FlightNoiseTabPage)
                SaveFlightNoiseInputDataMenuItem_Click();
            else if (TabControl.SelectedTab == FireTestNoiseTabPage)
                SaveFireTestNoiseInputDataMenuItem_Click();
            else if (TabControl.SelectedTab == EngineNoiseTabPage)
                SaveEngineNoiseInputDataMenuItem_Click();
            else if (TabControl.SelectedTab == SonicBoomTabPage)
                SaveSonicBoomInputDataMenuItem_Click();
        }
        private void OpenInputDataMenuItem_Click(object sender, EventArgs e)
        {
            if (TabControl.SelectedTab == FlightNoiseTabPage)
                OpenFlightNoiseInputDataMenuItem_Click();
            else if (TabControl.SelectedTab == FireTestNoiseTabPage)
            OpenFireTestNoiseInputDataMenuItem_Click();
                else if (TabControl.SelectedTab == EngineNoiseTabPage)
            OpenEngineNoiseInputDataMenuItem_Click();
                else if (TabControl.SelectedTab == SonicBoomTabPage)
            OpenSonicBoomInputDataMenuItem_Click();
        }
        private void CalculateMenuItem_Click(object sender, EventArgs e)
        {
            if (TabControl.SelectedTab == FlightNoiseTabPage)
                CalculateFlightNoiseMenuItem_Click();
            else if (TabControl.SelectedTab == FireTestNoiseTabPage)
                CalculatFireTestNoiseMenuItem_Click();
            else if (TabControl.SelectedTab == EngineNoiseTabPage)
                CalculatEngineNoiseMenuItem_Click();
            else if (TabControl.SelectedTab == SonicBoomTabPage)
                CalculateSonicBoomMenuItem_Click();
        }
        private List<Control> GetControls(Control Parent, Type type)
        {
            var controls = Parent.Controls.Cast<Control>();
            return controls.SelectMany(ctrl => GetControls(ctrl, type)).Concat(controls).Where(c => c.GetType() == type).ToList();
        }
        static class FormatConvert
        {
            public static string CharsToString(List<char> s)
            {
                return new string(s.ToArray());
            }
            public static string ToString(double Value)
            {
                if (Value == 0) return "0";
                double Root = Math.Truncate(Value);
                var Tail = Math.Abs(Value - Root).ToString("F50").ToCharArray().Skip(2).ToList();
                var NullTail = CharsToString(Tail.TakeWhile(x => x == '0').ToList());
                if (NullTail.Length == Tail.Count)
                    return Root.ToString();
                var DigitTailChars = Tail.Skip(NullTail.Count()).ToList();
                DigitTailChars = DigitTailChars.Take(Math.Min(4, DigitTailChars.Count)).ToList();
                string Res;
                if (DigitTailChars.Count < 4)
                    Res = Root.ToString() + "," + NullTail + CharsToString(DigitTailChars);
                else
                {
                    DigitTailChars.Insert(3, ',');
                    double RoundDigitTail = Math.Round(Convert.ToDouble(CharsToString(DigitTailChars)));
                    if (RoundDigitTail == 1000)
                    {
                        if (NullTail.Length == 0)
                            Res = (Root + Math.Sign(Root)).ToString();
                        else
                        {
                            var nullTail = NullTail.ToCharArray();
                            nullTail[nullTail.Length - 1] = '1';
                            NullTail = CharsToString(nullTail.ToList());
                            Res = Root.ToString() + "," + NullTail;
                        }
                    }
                    else
                        Res = Root.ToString() + "," + NullTail + RoundDigitTail.ToString();
                }
                var res = Res.ToCharArray();
                Array.Reverse(res);
                res = res.SkipWhile(x => x == '0').ToArray();
                Array.Reverse(res);
                return new string(res);
            }
        }
        class WeatherParametersContainer
        {
            ListBox WeatherParametersListBox;
            List<WeatherParameters> weatherParameters;
            public List<WeatherParameters> WeatherParameters
            {
                get
                {
                    return weatherParameters;
                }
                set
                {
                    weatherParameters = value;
                    WeatherParametersListBox.Items.Clear();
                    foreach (var w in weatherParameters)
                        WeatherParametersListBox.Items.Add(w.Mounth);
                }
            }
            public WeatherParametersContainer(
                TextBox NameTextBox,
                TextBox HumidityTextBox,
                TextBox TemperatureTextBox,
                Button AddButton,
                Button ModifyButton,
                Button DeleteButton,
                Button ApplyButton,
                ListBox WeatherParametersListBox,
                Action<List<WeatherParameters>> Apply)
            {
                this.WeatherParametersListBox = WeatherParametersListBox;
                AddButton.Click += (sender, e) =>
                {
                    if (weatherParameters == null)
                        weatherParameters = new List<WeatherParameters>();
                    if (weatherParameters.FindIndex(x => x.Mounth == NameTextBox.Text) != -1)
                    {
                        MessageBox.Show("Погодные условия с указанным наименованием уже существуют");
                        return;
                    }
                    weatherParameters.Add(new WeatherParameters(
                        NameTextBox.Text,
                        Convert.ToDouble(HumidityTextBox.Text),
                        Convert.ToDouble(TemperatureTextBox.Text) + 273));
                    WeatherParametersListBox.Items.Add(NameTextBox.Text);
                };
                ModifyButton.Click += (sender, e) =>
                {
                    int Selected = WeatherParametersListBox.SelectedIndex;
                    if (Selected == -1) return;
                    int FirstEqual = weatherParameters.FindIndex(x => x.Mounth == NameTextBox.Text);
                    if ((FirstEqual == Selected) || (FirstEqual == -1))
                    {
                        var weatherParameter = new WeatherParameters(
                            NameTextBox.Text,
                            Convert.ToDouble(HumidityTextBox.Text),
                            Convert.ToDouble(TemperatureTextBox.Text) + 273);
                        weatherParameters[Selected] = weatherParameter;
                        WeatherParametersListBox.Items[Selected] = NameTextBox.Text;
                    }
                    else
                    {
                        MessageBox.Show("Погодные условия с указанным наименованием уже существуют");
                        return;
                    }
                };
                DeleteButton.Click += (sender, e) =>
                {
                    int Selected = WeatherParametersListBox.SelectedIndex;
                    if (Selected == -1) return;
                    weatherParameters.RemoveAt(Selected);
                    WeatherParametersListBox.Items.RemoveAt(Selected);
                };
                ApplyButton.Click += (sender, e) => Apply(weatherParameters);
                WeatherParametersListBox.Click += (sender, e) =>
                {
                    if (WeatherParametersListBox.SelectedIndex == -1) return;
                    var SelectedWeatherParameters = weatherParameters[WeatherParametersListBox.SelectedIndex];
                    NameTextBox.Text = SelectedWeatherParameters.Mounth;
                    TemperatureTextBox.Text = FormatConvert.ToString(SelectedWeatherParameters.Temperature - 273);
                    HumidityTextBox.Text = FormatConvert.ToString(SelectedWeatherParameters.Humidity);
                };
            }
        }
        class FlowParametersContainer
        {
            TextBox MassFlowTextBox;
            TextBox NozzleDiameterTextBox;
            TextBox NozzleMachNumberTextBox;
            TextBox NozzleFlowVelocityTextBox;
            TextBox ChamberSoundVelocityTextBox;
            TextBox NozzleAdiabaticIndexTextBox;
            FlowParameters flowParameters;
            public FlowParameters FlowParameters
            {
                get 
                {
                    return flowParameters;
                }
                set
                {
                    flowParameters = value;
                    MassFlowTextBox.Text = flowParameters.MassFlow.ToString();
                    NozzleDiameterTextBox.Text = (flowParameters.NozzleDiameter * 1E3).ToString();
                    NozzleMachNumberTextBox.Text = flowParameters.NozzleMachNumber.ToString();
                    NozzleFlowVelocityTextBox.Text = flowParameters.NozzleFlowVelocity.ToString();
                    ChamberSoundVelocityTextBox.Text = flowParameters.ChamberSoundVelocity.ToString();
                    NozzleAdiabaticIndexTextBox.Text = flowParameters.NozzleAdiabaticIndex.ToString();
                }
            }
            public FlowParametersContainer(
                TextBox MassFlowTextBox,
                TextBox NozzleDiameterTextBox,
                TextBox NozzleMachNumberTextBox,
                TextBox NozzleFlowVelocityTextBox,
                TextBox ChamberSoundVelocityTextBox,
                TextBox NozzleAdiabaticIndexTextBox)
            {
                this.MassFlowTextBox = MassFlowTextBox;
                this.NozzleDiameterTextBox = NozzleDiameterTextBox;
                this.NozzleMachNumberTextBox = NozzleMachNumberTextBox;
                this.NozzleFlowVelocityTextBox = NozzleFlowVelocityTextBox;
                this.ChamberSoundVelocityTextBox = ChamberSoundVelocityTextBox;
                this.NozzleAdiabaticIndexTextBox = NozzleAdiabaticIndexTextBox;
            }
        }
        class ColoredSoundLevel
        {
            public double SoundLevel { get; set; }
            public Color Color { get; set; }
            public ColoredSoundLevel(double SoundLevel, Color Color)
            {
                this.SoundLevel = SoundLevel;
                this.Color = Color;
            }
        }
        class SoundLevelContainer
        {
            ListBox SoundLevelsListBox;
            List<ColoredSoundLevel> soundLevels;
            public List<ColoredSoundLevel> SoundLevels
            {
                get
                {
                    return soundLevels;
                }
                set
                {
                    soundLevels = value;
                    SoundLevelsListBox.Items.Clear();
                    foreach (var s in soundLevels)
                        SoundLevelsListBox.Items.Add(s.SoundLevel.ToString());
                }
            }
            public SoundLevelContainer(
                TextBox SoundLevelTextBox,
                TextBox ATextBox,
                TextBox RTextBox,
                TextBox GTextBox,
                TextBox BTextBox,
                Button AddButton,
                Button ModifyButton,
                Button DeleteButton,
                Button ApplyButton,
                Button ColorButton,
                ListBox SoundLevelsListBox,
                Action<List<double>> Apply)
            {
                this.SoundLevelsListBox = SoundLevelsListBox;
                AddButton.Click += (sender, e) =>
                {
                    if (soundLevels == null)
                        soundLevels = new List<ColoredSoundLevel>();
                    double SoundLevel = Convert.ToDouble(SoundLevelTextBox.Text);
                    if (soundLevels.FindIndex(x => x.SoundLevel == SoundLevel) != -1)
                    {
                        MessageBox.Show("Задайте новое значение уровня шума");
                        return;
                    }
                    Color color;
                    try
                    {
                        color = Color.FromArgb(
                            Convert.ToInt32(ATextBox.Text),
                            Convert.ToInt32(RTextBox.Text),
                            Convert.ToInt32(GTextBox.Text),
                            Convert.ToInt32(BTextBox.Text));
                    }
                    catch
                    {
                        color = Color.Black;
                    }
                    soundLevels.Add(new ColoredSoundLevel(SoundLevel, color));
                    SoundLevelsListBox.Items.Add(SoundLevel.ToString());
                };
                ModifyButton.Click += (sender, e) =>
                {
                    int Selected = SoundLevelsListBox.SelectedIndex;
                    if (Selected == -1) return;
                    double SoundLevel = Convert.ToDouble(SoundLevelTextBox.Text);
                    Color color;
                    try
                    {
                        color = Color.FromArgb(
                            Convert.ToInt32(ATextBox.Text),
                            Convert.ToInt32(RTextBox.Text),
                            Convert.ToInt32(GTextBox.Text),
                            Convert.ToInt32(BTextBox.Text));
                    }
                    catch
                    {
                        color = Color.Black;
                    }
                    int FirstIndex = soundLevels.FindIndex(x => x.SoundLevel == SoundLevel);
                    if ((FirstIndex == Selected) || (FirstIndex == -1))
                    {
                        soundLevels[Selected] = new ColoredSoundLevel(SoundLevel, color);
                        SoundLevelsListBox.Items[Selected] = SoundLevel.ToString();
                    }
                    else
                    {
                        MessageBox.Show("Задайте новое значение уровня шума");
                        return;
                    }
                };
                DeleteButton.Click += (sender, e) =>
                {
                    int Selected = SoundLevelsListBox.SelectedIndex;
                    if (Selected == -1) return;
                    soundLevels.RemoveAt(Selected);
                    SoundLevelsListBox.Items.RemoveAt(Selected);
                };
                ApplyButton.Click += (sender, e) => Apply(soundLevels.Select(x => x.SoundLevel).ToList());
                ColorButton.Click += (sender, e) =>
                {
                    var ColorDialog = new ColorDialog();
                    try
                    {
                        ColorDialog.Color = Color.FromArgb(
                            Convert.ToByte(ATextBox.Text),
                            Convert.ToByte(RTextBox.Text),
                            Convert.ToByte(GTextBox.Text),
                            Convert.ToByte(BTextBox.Text));
                    }
                    catch
                    {
                        ColorDialog.Color = Color.Black;
                    }
                    if (ColorDialog.ShowDialog() == DialogResult.OK)
                    {
                        ATextBox.Text = ColorDialog.Color.A.ToString();
                        RTextBox.Text = ColorDialog.Color.R.ToString();
                        GTextBox.Text = ColorDialog.Color.G.ToString();
                        BTextBox.Text = ColorDialog.Color.B.ToString();
                    }
                };
                SoundLevelsListBox.Click += (sender, e) =>
                {
                    if (SoundLevelsListBox.SelectedIndex == -1) return;
                    var SelectedSoundLevel = soundLevels[SoundLevelsListBox.SelectedIndex];
                    SoundLevelTextBox.Text = SelectedSoundLevel.SoundLevel.ToString();
                    ATextBox.Text = SelectedSoundLevel.Color.A.ToString();
                    RTextBox.Text = SelectedSoundLevel.Color.R.ToString();
                    GTextBox.Text = SelectedSoundLevel.Color.G.ToString();
                    BTextBox.Text = SelectedSoundLevel.Color.B.ToString();
                };
            }
        }
    }
}