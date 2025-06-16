using Unity.Burst;
using Unity.Mathematics;

namespace TweenLib.Commons
{
    [BurstCompile]
    public static class ShakeHelper
    {
        [BurstCompile]
        public static float FloatShake(in float normalizedTime, in float frequency, in float intensity, in float startValue)
        {
            float time = normalizedTime * frequency;

            float offsetX = noise.cnoise(new float2(time, 0f));
            float offset = (1 - normalizedTime) * intensity * offsetX;

            return startValue + offset;
        }

        [BurstCompile]
        public static void Float2Shake(in float normalizedTime, in float frequency, in float intensity, in float2 startValue, out float2 value)
        {
            float time = normalizedTime * frequency;

            float offsetX = noise.cnoise(new float2(time, 0f));
            float offsetY = noise.cnoise(new float2(0f, time));
            float2 offset = (1 - normalizedTime) * intensity * new float2(offsetX, offsetY);

            value = startValue + offset;
        }

        [BurstCompile]
        public static void Float3Shake(in float normalizedTime, in float frequency, in float intensity, in float3 startValue, out float3 value)
        {
            float time = normalizedTime * frequency;

            float offsetX = noise.cnoise(new float2(time, 0f));
            float offsetY = noise.cnoise(new float2(0f, time));
            float offsetZ = noise.cnoise(new float2(time, time));
            float3 offset = (1 - normalizedTime) * intensity * new float3(offsetX, offsetY, offsetZ);

            value = startValue + offset;
        }

    }

}
