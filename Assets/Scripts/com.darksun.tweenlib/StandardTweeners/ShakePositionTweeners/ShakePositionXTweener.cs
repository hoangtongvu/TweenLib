using TweenLib.Commons;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;

namespace TweenLib.StandardTweeners.ShakePositionTweeners
{
    [BurstCompile]
    public partial struct ShakePositionXTweener : ITweener<LocalTransform, float3>
    {
        public float3 GetDefaultStartValue(in LocalTransform componentData)
        {
            return componentData.Position;
        }

        [BurstCompile]
        public void Tween(ref LocalTransform componentData, in float normalizedTime, EasingType easingType, in float3 startValue, in float3 shakeData)
        {
            // shakeData.x is shake frequency, shakeData.y is shake intensity
            componentData.Position.x =
                ShakeHelper.FloatShake(in normalizedTime, shakeData.x, shakeData.y, in startValue.x);
        }

    }

}
