using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace TypesLibrary
{
    public class FlightSoundCircle // Класс, содержащий информацию о геометрическом месте точек (окружности), в которых уровень шума от СЧ МСРКН (МСРН или МСКА) принимает определённое значение
    {
        public double Time { get; private set; } // Время полёта, при которм уровень шума принмает значение SoundLevel
        public double SoundLevel { get; private set; } // Уровень шума
        public double Distance { get; private set; } // Расстояние от точки старта МСРКН до проекции радиус-вектора СЧ МСРКН на земную поверхность (координата цетра окружности, на которой уровень шума равен SoundLevel)
        public double Radius { get; private set; } // Радиус окружности, на которой уровень шума равен SoundLevel
        public string Mounth { get; private set; } // Название месяца
        public FlightSoundCircle(double SoundLevel, double Distance, double Radius, double Time, string Mounth)
        {
            this.SoundLevel = SoundLevel;
            this.Distance = Distance;
            this.Radius = Radius;
            this.Time = Time;
            this.Mounth = Mounth;
        }
    }
    public struct FlightSoundCircles // Класс, содержащий информацию о геометрическом месте точек, в которых уровень шума от СЧ МСРКН (МСРН и МСКА) принимает определённое значение
    {
        public double SoundLevel { get; private set; } // Уровень шума
        public FlightSoundCircle RightRocketCircle { get; private set; } // Характеристики окружности, на которой уровень шума от МСРН в крайней точке по направлению полёта МСРКН принимает значение SoundLevel
        public FlightSoundCircle LeftRocketCircle { get; private set; } // Характеристики окружности, на которой уровень шума от МСРН в крайней точке против направления полёта МСРКН принимает значение SoundLevel
        public FlightSoundCircle RightVehicleCircle { get; private set; } // Характеристики окружности, на которой уровень шума от МСКА в крайней точке по направлению полёта МСРКН принимает значение SoundLevel
        public FlightSoundCircle LeftVehicleCircle { get; private set; } // Характеристики окружности, на которой уровень шума от МСКА в крайней точке против направления полёта МСРКН принимает значение SoundLevel
        public List<Point> Points { get; private set; } // Координаты точек, в которых уровень шума принимает значение SoundLevel
        public FlightSoundCircles(
            double SoundLevel,
            FlightSoundCircle RightRocketCircle,
            FlightSoundCircle LeftRocketCircle,
            FlightSoundCircle RightVehicleCircle,
            FlightSoundCircle LeftVehicleCircle,
            List<Point> Points)
            : this()
        {
            this.SoundLevel = SoundLevel;
            this.RightRocketCircle = RightRocketCircle;
            this.LeftRocketCircle = LeftRocketCircle;
            this.RightVehicleCircle = RightVehicleCircle;
            this.LeftVehicleCircle = LeftVehicleCircle;
            this.Points = Points;
        }
    }
    public struct EffectiveFlightSound // Класс, содержащий результат расчёта эффективного уровня шума
    {
        public double Distance { get; private set; } // Расстояние от точки старта МСРКН
        public double RightMaxSoundLevel { get; private set; } // Максимальный уровень шума на расстоянии Distance от точки старта МСРКН в направлении полёта МСРКН
        public double LeftMaxSoundLevel { get; private set; }  // Максимальный уровень шума на расстоянии Distance от точки старта МСРКН в направлении, обратном направлению полёта МСРКН
        public double RightEffektiveSoundLevel { get; private set; } // Эффективный уровень шума на расстоянии Distance от точки старта МСРКН в направлении полёта МСРКН
        public double LeftEffektiveSoundLevel { get; private set; } // Эффективный уровень шума на расстоянии Distance от точки старта МСРКН в направлении, обратном направлению полёта МСРКН
        public double[] RightTimes { get; private set; } // Массив значений времён, в течение котороых максимальный уровень звукового давления на расстоянии Distance от точки старта МСРКН в направлении полёта МСРКН превышает характерные уровни шума (85 дБА, 75 дБА и 70 дБА)
        public double[] LeftTimes { get; private set; } // Массив значений времён, в течение котороых максимальный уровень звукового давления на расстоянии Distance от точки старта МСРКН в направлении, противоположенном направлению полёта МСРКН, превышает характерные уровни шума (85 дБА, 75 дБА и 70 дБА)
        public string RightWeatherConditions { get; private set; } // Месяц, в котором уровень шума на расстоянии Distance от точки старта МСРКН в направлении полёта МСРКН принимает максимальное значение
        public string LeftWeatherConditions { get; private set; } // Месяц, в котором уровень шума на расстоянии Distance от точки старта МСРКН в направлении, обратном направлению полёта МСРКН, принимает максимальное значение
        public EffectiveFlightSound(
            double Distance,
            double RightMaxSoundLevel,
            double LeftMaxSoundLevel,
            double RightEffektiveSoundLevel,
            double LeftEffektiveSoundLevel,
            double[] RightTimes,
            double[] LeftTimes,
            string RightWeatherConditions,
            string LeftWeatherConditions)
            : this()
        {
            this.Distance = Distance;
            this.RightMaxSoundLevel = RightMaxSoundLevel;
            this.LeftMaxSoundLevel = LeftMaxSoundLevel;
            this.RightEffektiveSoundLevel = RightEffektiveSoundLevel;
            this.LeftEffektiveSoundLevel = LeftEffektiveSoundLevel;
            this.RightTimes = RightTimes;
            this.LeftTimes = LeftTimes;
            this.RightWeatherConditions = RightWeatherConditions;
            this.LeftWeatherConditions = LeftWeatherConditions;
        }
    }
    public class EngineSoundContour
    {
        public double[] X { get; private set; }
        public double[] Y { get; private set; }
        public double[,] SoundLevels { get; private set; }
        public Image Contour;
        public EngineSoundContour(double[] X, double[] Y, double[,] SoundLevels, Image Contour)
        {
            this.X = X;
            this.Y = Y;
            this.SoundLevels = SoundLevels;
            this.Contour = Contour;
        }
    }
    public struct FireTestSoundContour // Класс, содержащий информацию о геометрическом месте точек, в которых уровень шума при огневых испытаниях принимает определённое значение
    {
        public double SoundLevel { get; private set; } // Уровень шума
        public List<Point> Points { get; private set; } // Координаты точек, в которых уровень шума при огневых испытаниях принимает значение SoundLevel
        public FireTestSoundContour(
            double SoundLevel,
            List<Point> Points)
            : this()
        {
            this.SoundLevel = SoundLevel;
            this.Points = Points;
        }
    }
}
