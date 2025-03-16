using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;

namespace TweenLib.StandardTweeners
{
    [BurstCompile]
    public partial struct TransformRotationTweener : ITweener<LocalTransform, float4>
    {
        [BurstCompile]
        public bool CanStop(in LocalTransform componentData, in float lifeTimeSecond, in float baseSpeed, in float4 target)
        {
            return math.all(math.abs(target - componentData.Rotation.value) < new float4(Configs.Epsilon));
        }

        [BurstCompile]
        public void Tween(ref LocalTransform componentData, in float baseSpeed, in float4 target)
        {
            componentData.Rotation =
                math.lerp(componentData.Rotation.value, target, baseSpeed * this.DeltaTime);
        }

    }

}
