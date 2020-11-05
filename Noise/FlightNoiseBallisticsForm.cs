using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TypesLibrary;
using Excel = Microsoft.Office.Interop.Excel;
using MainViewInterface;

namespace Noise
{
    public partial class FlightNoiseBallisticsForm : Form
    {
        public FlightNoiseBallisticsForm(Part part)
        {
            InitializeComponent();
            VehiclePanel.Visible = part == Part.Vehicle;
            this.part = part;
        }
        Part part;
        private void ModifyRowsButton(object sender, EventArgs e)
        {
            if (sender == RemoveRowsButton)
                Grid.Rows.Clear();
            if (Grid.Rows.Count == 0)
            {
                if (sender == AddRowButton)
                    Grid.Rows.Add();
            }
            else
            {
                int Index = Grid.CurrentRow.Index;
                if (sender == AddRowButton)
                {
                    if (Index == Grid.Rows.Count - 1)
                        Grid.Rows.Add();
                    else if (Index == 0)
                        Grid.Rows.Insert(1, 1);
                    else
                        Grid.Rows.Insert(Index + 1, 1);
                }
                else if (sender == RemoveRowButton)
                    Grid.Rows.RemoveAt(Index);
            }
        }
        private void GetData(out Interpolation Height, out Interpolation Distance, out Interpolation Thrust)
        {
            if (Grid.Rows.Count == 0)
            {
                throw new Exception("Не определена баллистика");
            }
            else
            {
                try
                {
                    var time = new double[Grid.Rows.Count];
                    var height = new double[Grid.Rows.Count];
                    var distance = new double[Grid.Rows.Count];
                    var thrust = new double[Grid.Rows.Count];
                    for (int i = 0; i < Grid.Rows.Count; i++)
                    {
                        var row = Grid.Rows[i];
                        time[i] = Convert.ToDouble(row.Cells[0].Value);
                        thrust[i] = Convert.ToDouble(row.Cells[1].Value) * 1E3;
                        distance[i] = Convert.ToDouble(row.Cells[2].Value);
                        height[i] = Convert.ToDouble(row.Cells[3].Value);
                    }
                    Height = new Interpolation(time, height);
                    Distance = new Interpolation(time, distance);
                    Thrust = new Interpolation(time, thrust);
                }
                catch
                {
                    throw new Exception("Ошибка формата ввода баллистики");
                }
            }
        }
        private RocketBallistics RocketBallistics()
        { 
            Interpolation Height, Distance, Thrust;
            try
            {
                GetData(out Height, out Distance, out Thrust);
                return new RocketBallistics(Height, Distance, Thrust);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message + " МСРН");
            }
        }
        private VehicleBallistics VehicleBallistics()
        {
            Interpolation Height, Distance, Thrust;
            try
            {
                GetData(out Height, out Distance, out Thrust);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message + " МСКА");
            }
            try
            {
                return new VehicleBallistics(Height, Distance, Thrust, Convert.ToDouble(LandRadiusTextBox.Text), Convert.ToDouble(LandTimeTextBox.Text));
            }
            catch
            {
                throw new Exception("Ошибка формата ввода радиуса посадки и/или условного момента времени начала посадки МСКА");
            }
        }
        public Ballistics Ballistics
        {
            get
            {
                try
                {
                    if (part == Part.Vehicle) return VehicleBallistics();
                    else return RocketBallistics();
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
            set 
            {
                Grid.Rows.Clear();
                for (int i = 0; i < value.Distance.X.Length; i++)
                {
                    Grid.Rows.Add();
                    Grid.Rows[i].Cells[0].Value = value.Distance.X[i];
                    Grid.Rows[i].Cells[1].Value = value.Thrust.Y[i] * 1E-3;
                    Grid.Rows[i].Cells[2].Value = value.Distance.Y[i];
                    Grid.Rows[i].Cells[3].Value = value.Height.Y[i];
                }
                if (part == Part.Vehicle)
                {
                    LandRadiusTextBox.Text = ((VehicleBallistics)value).LandingRadius.ToString();
                    LandTimeTextBox.Text = ((VehicleBallistics)value).LandingStartTime.ToString();
                }
            }
        }
        private void FlightNoiseBallisticsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Visible = false;
        }
        private async void OpenButton_Click(object sender, EventArgs e)
        {
            var OpenDialog = new OpenFileDialog();
            if (OpenDialog.ShowDialog() == DialogResult.OK)
            {
                string BallisticsPath = OpenDialog.FileName;
                Excel.Application ExcelApp = new Excel.Application();
                ExcelApp.Visible = false;
                ExcelApp.DisplayAlerts = false;
                Excel.Workbook ExcelWb;
                try
                {
                    ExcelWb = ExcelApp.Workbooks.Open(BallisticsPath);
                }
                catch
                {
                    try
                    {
                        ExcelWb = ExcelApp.Workbooks.Open(Directory.GetCurrentDirectory() + "\\" + BallisticsPath);
                    }
                    catch
                    {
                        throw new Exception("Файл не найден");
                    }
                }
                Excel.Worksheet ExcelWs = (Excel.Worksheet)ExcelWb.ActiveSheet;
                List<double> TimeList = new List<double>();
                List<double> ThrustList = new List<double>();
                List<double> HeightList = new List<double>();
                List<double> DistanceList = new List<double>();
                int CurrentPosition = 2;
                Grid.Rows.Clear();
                await Task.Run(() =>
                {
                    try
                    {
                        while (ExcelWs.Cells[CurrentPosition, 1].Value != null)
                        {
                            this.Invoke(new Action(() =>
                            {
                                Grid.Rows.Add();
                                Grid.Rows[CurrentPosition - 2].Cells[0].Value = ExcelWs.Cells[CurrentPosition, 1].Value;
                                Grid.Rows[CurrentPosition - 2].Cells[1].Value = ExcelWs.Cells[CurrentPosition, 2].Value;
                                Grid.Rows[CurrentPosition - 2].Cells[2].Value = ExcelWs.Cells[CurrentPosition, 3].Value;
                                Grid.Rows[CurrentPosition - 2].Cells[3].Value = ExcelWs.Cells[CurrentPosition, 4].Value;
                                CurrentPosition++;
                            }));
                        }
                        this.Invoke(new Action(() =>
                        {
                            if (part == Part.Vehicle)
                            {
                                LandRadiusTextBox.Text = ExcelWs.Cells[2, 5].Value.ToString();
                                LandTimeTextBox.Text = ExcelWs.Cells[2, 6].Value.ToString();
                            }
                        }));
                    }
                    catch
                    {
                        throw new Exception("Неверный формат данных");
                    }
                    finally
                    {
                        ExcelWb.Close();
                        ExcelApp.Quit();
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(ExcelApp);
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(ExcelWs);
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(ExcelWb);
                        ExcelApp = null;
                        ExcelWs = null;
                        ExcelWb = null;
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        GC.Collect();
                    }
                });
            }
        }
    }
}
