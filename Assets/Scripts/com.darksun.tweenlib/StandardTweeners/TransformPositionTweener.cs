using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;

namespace TweenLib.StandardTweeners
{
    [BurstCompile]
    public partial struct TransformPositionTweener : ITweener<LocalTransform, float3>
    {
        public float3 GetDefaultStartValue(in LocalTransform componentData)
        {
            return componentData.Position;
        }

        [BurstCompile]
        public void Tween(ref LocalTransform componentData, in float normalizedTime, EasingType easingType, in float3 startValue, in float3 target)
        {
            TweenHelper.Float3Tween(in normalizedTime, easingType, in startValue, in target, out componentData.Position);
        }

    }

}
