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
        [BurstCompile]
        public void GetDefaultStartValue(in LocalTransform componentData, out float4 defaultStartValue)
            => defaultStartValue = componentData.Rotation.value;

        [BurstCompile]
        public void GetSum(in float4 a, in float4 b, out float4 result)
            => result = a + b;

        [BurstCompile]
        public void GetDifference(in float4 a, in float4 b, out float4 result)
            => result = a - b;

        [BurstCompile]
        public void Tween(ref LocalTransform componentData, in float normalizedTime, EasingType easingType, in float4 startValue, in float4 target)
        {
            TweenHelper.Float4Tween(in normalizedTime, easingType, in startValue, in target, out componentData.Rotation.value);
        }

    }

}
