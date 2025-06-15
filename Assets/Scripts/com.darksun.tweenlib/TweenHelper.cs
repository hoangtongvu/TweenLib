using Unity.Burst;
using Unity.Mathematics;

namespace TweenLib
{
    [BurstCompile]
    public static class TweenHelper
    {
        [BurstCompile]
        public static float FloatTween(in float normalizedTime, EasingType easingType, in float startValue, in float target)
        {
            float t = EasingUtilities.CalculateEasing(normalizedTime, easingType);
            return math.lerp(startValue, target, t);
        }

        [BurstCompile]
        public static void Float2Tween(in float normalizedTime, EasingType easingType, in float2 startValue, in float2 target, out float2 value)
        {
            float t = EasingUtilities.CalculateEasing(normalizedTime, easingType);
            value = math.lerp(startValue, target, t);
        }

        [BurstCompile]
        public static void Float3Tween(in float normalizedTime, EasingType easingType, in float3 startValue, in float3 target, out float3 value)
        {
            float t = EasingUtilities.CalculateEasing(normalizedTime, easingType);
            value = math.lerp(startValue, target, t);
        }

        [BurstCompile]
        public static void Float4Tween(in float normalizedTime, EasingType easingType, in float4 startValue, in float4 target, out float4 value)
        {
            float t = EasingUtilities.CalculateEasing(normalizedTime, easingType);
            value = math.lerp(startValue, target, t);
        }

    }

}
