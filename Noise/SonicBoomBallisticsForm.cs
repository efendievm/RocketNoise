using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TypesLibrary;
using Excel = Microsoft.Office.Interop.Excel;
using MainViewInterface;
using System.IO;

namespace Noise
{
    public partial class SonicBoomBallisticsForm : Form
    {
        public SonicBoomBallisticsForm(Part part)
        {
            InitializeComponent();
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
        public SonicBoomBallistics Ballistics
        {
            get
            {
                Interpolation Height, Distance, Mach;
                if (Grid.Rows.Count == 0)
                {
                    throw new Exception("Не определена баллистика " + (part == Part.Rocket ? "МСРН" : "МСКА"));
                }
                else
                {
                    try
                    {
                        var time = new double[Grid.Rows.Count];
                        var height = new double[Grid.Rows.Count];
                        var distance = new double[Grid.Rows.Count];
                        var mach = new double[Grid.Rows.Count];
                        for (int i = 0; i < Grid.Rows.Count; i++)
                        {
                            var row = Grid.Rows[i];
                            time[i] = Convert.ToDouble(row.Cells[0].Value);
                            distance[i] = Convert.ToDouble(row.Cells[1].Value);
                            height[i] = Convert.ToDouble(row.Cells[2].Value);
                            mach[i] = Convert.ToDouble(row.Cells[3].Value);
                        }
                        Height = new Interpolation(time, height);
                        Distance = new Interpolation(time, distance);
                        Mach = new Interpolation(time, mach);
                        return new SonicBoomBallistics(Height, Distance, Mach);
                    }
                    catch
                    {
                        throw new Exception("Ошибка формата ввода баллистики " + (part == Part.Rocket ? "МСРН" : "МСКА"));
                    }
                }
            }
            set
            {
                Grid.Rows.Clear();
                for (int i = 0; i < value.Distance.X.Length; i++)
                {
                    Grid.Rows.Add();
                    Grid.Rows[i].Cells[0].Value = value.Distance.X[i];
                    Grid.Rows[i].Cells[1].Value = value.Distance.Y[i];
                    Grid.Rows[i].Cells[2].Value = value.Height.Y[i];
                    Grid.Rows[i].Cells[3].Value = value.MachNumber.Y[i];
                }
            }
        }
        private void SonicBoomBallisticsForm_FormClosing(object sender, FormClosingEventArgs e)
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
