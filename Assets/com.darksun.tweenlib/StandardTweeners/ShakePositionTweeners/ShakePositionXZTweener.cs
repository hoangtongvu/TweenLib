using TweenLib.Utilities;
using TweenLib.Utilities.Helpers;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;

namespace TweenLib.StandardTweeners.ShakePositionTweeners
{
    [BurstCompile]
    public partial struct ShakePositionXZTweener : ITweener<LocalTransform, float3>
    {
        [BurstCompile]
        public void GetDefaultStartValue(in LocalTransform componentData, out float3 defaultStartValue)
            => defaultStartValue = componentData.Position;

        [BurstCompile]
        public void GetSum(in float3 a, in float3 b, out float3 result)
            => result = a + b;

        [BurstCompile]
        public void GetDifference(in float3 a, in float3 b, out float3 result)
            => result = a - b;

        [BurstCompile]
        public void Tween(ref LocalTransform componentData, in float normalizedTime, EasingType easingType, in float3 startValue, in float3 shakeData)
        {
            // shakeData.x is shake frequency, shakeData.y is shake intensity
            float2 startValueXZ = new(startValue.x, startValue.z);
            ShakeHelper.Float2Shake(in normalizedTime, shakeData.x, shakeData.y, in startValueXZ, out var newValueXZ);
            componentData.Position.x = newValueXZ.x;
            componentData.Position.z = newValueXZ.y;
        }

    }

}
