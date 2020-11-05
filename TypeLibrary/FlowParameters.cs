using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypesLibrary
{
    public struct FlowParameters  // Класс, содержащий информацию о параметрах газовой струи
    {
        public double MassFlow { get; set; } // Расход топлива
        public double NozzleDiameter { get; set; } // Диаметр сопла
        public double NozzleMachNumber { get; set; } // Число Маха на срезе сопла
        public double NozzleFlowVelocity { get; set; } // Скорость потока на срезе сопла
        public double ChamberSoundVelocity { get; set; } // Скорость звука в критическом сечении камеры сгорания
        public double NozzleAdiabaticIndex { get; set; } // Показатель адиабаты на срезе сопла
        public FlowParameters(
            double MassFlow,
            double NozzleDiameter,
            double NozzleMachNumber,
            double NozzleFlowVelocity,
            double ChamberSoundVelocity,
            double NozzleAdiabaticIndex)
            : this()
        {
            this.MassFlow = MassFlow;
            this.NozzleDiameter = NozzleDiameter;
            this.NozzleMachNumber = NozzleMachNumber;
            this.NozzleFlowVelocity = NozzleFlowVelocity;
            this.ChamberSoundVelocity = ChamberSoundVelocity;
            this.NozzleAdiabaticIndex = NozzleAdiabaticIndex;
        }
    }
    public struct FlowSoundParameters  // Структруа, содержащая информацию об акустической харакетристике газовой струи
    {
        public double MechanicalPower { get; private set; } // Мощность механическая газовой струи в выходном сечении сопла
        public double SoundPower { get; private set; } // Мощность звука газовой струи
        public double SoundPowerRatio { get; private set; } // Соотношение мощности механической в выходном сечении сопла к мощности звука газовой струи
        public double SoundMaximalPowerConeHalfAngle { get; private set; } // Угол полураствора конуса максимальной мощности звука газовой струи
        public double UndisturbedSupersonicFlowLength { get; private set; } // Расстояние от выходного сечения сопла до конца участка газовой струи с невозмущённым сверхзвуковым потоком
        public double DistanceToPointOfMaximalSoundLevel { get; private set; } // Расстояние от выходного сечения сопла до точки наибольшей интенсивности излучения звука газовой струи
        public double SupersonicFlowLength { get; private set; } // Расстояние от выходного сечения сопла до конца участка газовой струи со сверхзвуковым потоком
        public double _98ProzentSoundPowerRadiatingFlowLength { get; private set; } // Расстояние от выходного сечения сопла до конца участка газовой струи, излучающего ~98% звуковой мощности
        public double DistanceToPointOfFlowDestruction { get; private set; } // Расстояние от выходного сечения сопла до места полного разрушения газовой струи
        public FlowSoundParameters(
            double MechanicalPower,
            double SoundPower,
            double SoundPowerRatio,
            double SoundMaximalPowerConeHalfAngle,
            double UndisturbedSupersonicFlowLength,
            double DistanceToPointOfMaximalSoundLevel,
            double SupersonicFlowLength,
            double _98ProzentSoundPowerRadiatingFlowLength,
            double DistanceToPointOfFlowDestruction)
            : this()
        {
            this.MechanicalPower = MechanicalPower;
            this.SoundPower = SoundPower;
            this.SoundPowerRatio = SoundPowerRatio;
            this.SoundMaximalPowerConeHalfAngle = SoundMaximalPowerConeHalfAngle;
            this.UndisturbedSupersonicFlowLength = UndisturbedSupersonicFlowLength;
            this.DistanceToPointOfMaximalSoundLevel = DistanceToPointOfMaximalSoundLevel;
            this.SupersonicFlowLength = SupersonicFlowLength;
            this._98ProzentSoundPowerRadiatingFlowLength = _98ProzentSoundPowerRadiatingFlowLength;
            this.DistanceToPointOfFlowDestruction = DistanceToPointOfFlowDestruction;
        }
    }
}
