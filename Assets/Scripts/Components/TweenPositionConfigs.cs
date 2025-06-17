using TweenLib.Commons;
using TweenLib.Utilities;
using Unity.Entities;
using Unity.Mathematics;

namespace Components
{
    [System.Serializable]
    public struct TweenPositionConfigs : IComponentData
    {
        public float Duration;

        public bool UseCustomStartValue;
        public float3 StartValue;

        public float3 TargetValue;
        public TweenPositionType TweenPositionType;
        public EasingType EasingType;

        public byte LoopCount;
        public LoopType LoopType;

        public float DelaySeconds;

    }

    public enum TweenPositionType
    {
        XYZ = 0,
        XY = 1,
        YZ = 2,
        XZ = 3,
        X = 4,
        Y = 5,
        Z = 6,
    }

}
