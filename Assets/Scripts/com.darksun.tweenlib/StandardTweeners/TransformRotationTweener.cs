using TweenLib.Utilities;
using TweenLib.Utilities.Helpers;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;

namespace TweenLib.StandardTweeners
{
    [BurstCompile]
    public partial struct TransformRotationTweener : ITweener<LocalTransform, float4>
    {
        public float4 GetDefaultStartValue(in LocalTransform componentData)
        {
            return componentData.Rotation.value;
        }

        [BurstCompile]
        public void Tween(ref LocalTransform componentData, in float normalizedTime, EasingType easingType, in float4 startValue, in float4 target)
        {
            TweenHelper.Float4Tween(in normalizedTime, easingType, in startValue, in target, out componentData.Rotation.value);
        }

    }

}
