using Unity.Collections;
using Unity.Entities;

namespace TweenLib.ShakeTween.Data
{
    public struct ShakeDataList : IComponentData
    {
        public NativeList<ShakeData> Value;
    }

}
