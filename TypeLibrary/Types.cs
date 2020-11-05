using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypesLibrary
{
    public struct WeatherParameters // Класс, определяющий погодные условия
    {
        public string Mounth { get; private set; } // Название месяца
        public double Humidity { get; set; } // Относительная влажность (%)
        public double Temperature { get; set; } // Температура (К)
        public WeatherParameters(string Mounth, double Humidity, double Temperature)
            : this()
        {
            this.Mounth = Mounth;
            this.Humidity = Humidity;
            this.Temperature = Temperature;
        }
    }
    public struct Point // Струтура, определяющая координаты точки
    {
        public double X { get; private set; }
        public double Y { get; private set; }
        public Point(double X, double Y)
            : this()
        {
            this.X = X;
            this.Y = Y;
        }
    }
    public struct RadiusInterval
    {
        public double Initial { get; private set; }
        public double Final { get; private set; }
        public double Step { get; private set; }
        public RadiusInterval(double Initial, double Final, double Step)
            : this()
        {
            this.Initial = Initial;
            this.Final = Final;
            this.Step = Step;
        }
    }
    public enum FrequencyBand { Infra, Normal, Ultra }
}