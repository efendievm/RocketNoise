using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypesLibrary
{
    public struct GeometricalParameters
    {
        public double Length { get; private set; }
        public double CharacteristicLength { get; private set; }
        public double MaximalArea { get; private set; }
        public double CharacteristicArea { get; private set; }
        public GeometricalParameters(
            double Length,
            double CharacteristicLength,
            double MaximalArea,
            double CharacteristicArea)
            : this()
        {
            this.Length = Length;
            this.CharacteristicLength = CharacteristicLength;
            this.MaximalArea = MaximalArea;
            this.CharacteristicArea = CharacteristicArea;
        }
    }
    public class SonicBoomParameters
    {
        public double Time { get; private set; }
        public double Height { get; private set; }
        public double MachNumber { get; private set; }
        public double OverPressure { get; private set; }
        public double SoundLevel { get; private set; }
        public double ImpactDistance { get; private set; }
        public double ImpactDuration { get; private set; }
        public string Mounth { get; private set; }
        public SonicBoomParameters(
            double Time,
            double Height,
            double MachNumber,
            double OverPressure,
            double SoundLevel,
            double ImpactDistance,
            double ImpactDuration,
            string Mounth)
        {
            this.Time = Time;
            this.Height = Height;
            this.MachNumber = MachNumber;
            this.OverPressure = OverPressure;
            this.SoundLevel = SoundLevel;
            this.ImpactDistance = ImpactDistance;
            this.ImpactDuration = ImpactDuration;
            this.Mounth = Mounth;
        }
    }
}
