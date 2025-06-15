using TweenLib.Commons;
using Unity.Collections;
using Unity.Entities;

namespace TweenLib.Timer.Data
{
    public struct TimerList : IComponentData
    {
        public NativeList<TimerSeconds> Value;
    }

    [System.Serializable]
    public struct TimerSeconds
    {
        public BoolByte IsEnabled;
        public float Counter;
        public static readonly TimerSeconds Default = new()
        {
            IsEnabled = true,
            Counter = 0f,
        };

    }

}
