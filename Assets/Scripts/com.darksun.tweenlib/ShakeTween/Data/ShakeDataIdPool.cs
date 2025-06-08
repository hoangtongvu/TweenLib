using Unity.Collections;
using Unity.Entities;

namespace TweenLib.ShakeTween.Data
{
    public struct ShakeDataIdPool : IComponentData
    {
        public NativeQueue<int> Value;
    }

}
