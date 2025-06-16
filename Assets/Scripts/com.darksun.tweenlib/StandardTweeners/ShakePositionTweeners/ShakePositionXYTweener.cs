using TweenLib.Commons;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;

namespace TweenLib.StandardTweeners.ShakePositionTweeners
{
    [BurstCompile]
    public partial struct ShakePositionXYTweener : ITweener<LocalTransform, float3>
    {
        public float3 GetDefaultStartValue(in LocalTransform componentData)
        {
            return componentData.Position;
        }

        [BurstCompile]
        public void Tween(ref LocalTransform componentData, in float normalizedTime, EasingType easingType, in float3 startValue, in float3 shakeData)
        {
            // shakeData.x is shake frequency, shakeData.y is shake intensity
            float2 startValueXY = new(startValue.x, startValue.y);
            ShakeHelper.Float2Shake(in normalizedTime, shakeData.x, shakeData.y, in startValueXY, out var newValueXY);
            componentData.Position.x = newValueXY.x;
            componentData.Position.y = newValueXY.y;
        }

    }

}
