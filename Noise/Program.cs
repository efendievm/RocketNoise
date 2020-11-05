using ModelLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Noise
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var MainForm = new MainForm();
            new Presenter(MainForm, new Model());
            MainForm.InitializeFireTestNoiseView();
            MainForm.InitializeFlightNoiseView();
            MainForm.InitializeEngineNoiseView();
            MainForm.InitializeSonicBoomView();
            Application.Run(MainForm);
        }
    }
}
