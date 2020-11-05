using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypesLibrary
{
    public class Ballistics  // Родительский класс, определяющий баллистику СЧ МСРКН (используется при расчёте уровня шума при полёте СЧ МСРКН)
    {
        public Interpolation Height { get; private set; }
        public Interpolation Distance { get; private set; }
        public Interpolation Thrust { get; private set; }
        public Ballistics(Interpolation Height, Interpolation Distance, Interpolation Thrust)
        {
            this.Height = Height;
            this.Distance = Distance;
            this.Thrust = Thrust;
        }
    }
    public class RocketBallistics : Ballistics // Класс, содержащий баллистику МСРН (используется при расчёте уровня шума при полёте СЧ МСРКН)
    {
        public RocketBallistics(Interpolation Height, Interpolation Distance, Interpolation Thrust)
            : base(Height, Distance, Thrust) { }
    }
    public class VehicleBallistics : Ballistics // Класс, содержащий баллистику МСКА (используется при расчёте уровня шума при полёте СЧ МСРКН)
    {
        public double LandingRadius { get; private set; } // Радиус зоны посадки
        public double LandingStartTime { get; private set; } // Условное время начала посадки
        public VehicleBallistics(
            Interpolation Height,
            Interpolation Distance,
            Interpolation Thrust,
            double LandingRadius,
            double LandingStartTime)
            : base(Height, Distance, Thrust)
        {
            this.LandingRadius = LandingRadius;
            this.LandingStartTime = LandingStartTime;
        }
    }
    public class SonicBoomBallistics // Класс, определяющий баллистику СЧ МСРКН (используется при расчёте звукового удара при полёте СЧ МСРКН)
    {
        public Interpolation Height { get; private set; }
        public Interpolation Distance { get; private set; }
        public Interpolation MachNumber { get; private set; }
        public SonicBoomBallistics(Interpolation Height, Interpolation Distance, Interpolation MachNumber)
        {
            this.Height = Height;
            this.Distance = Distance;
            this.MachNumber = MachNumber;
        }
    }
}
