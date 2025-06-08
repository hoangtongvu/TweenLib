using Unity.Collections;
using Unity.Entities;

namespace TweenLib.Timer.Data
{
    public struct TimerIdPool : IComponentData
    {
        public NativeQueue<int> Value;
    }

}
