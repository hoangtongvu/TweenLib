using Unity.Entities;
using Unity.Mathematics;

namespace Components
{
    [System.Serializable]
    public struct ShakePositionConfigs : IComponentData
    {
        public float Duration;

        public bool UseCustomStartValue;
        public float3 StartValue;

        public float Frequency;
        public float Intensity;
        public ShakePositionType ShakePositionType;

        public float DelaySeconds;
    }

    public enum ShakePositionType
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
