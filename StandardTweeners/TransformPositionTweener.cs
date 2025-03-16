using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;

namespace TweenLib.StandardTweeners
{
    [BurstCompile]
    public partial struct TransformPositionTweener : ITweener<LocalTransform, float3>
    {
        [BurstCompile]
        public bool CanStop(in LocalTransform componentData, in float lifeTimeSecond, in float baseSpeed, in float3 target)
        {
            return math.all(math.abs(target - componentData.Position) < new float3(Configs.Epsilon));
        }

        [BurstCompile]
        public void Tween(ref LocalTransform componentData, in float baseSpeed, in float3 target)
        {
            componentData.Position =
                math.lerp(componentData.Position, target, baseSpeed * this.DeltaTime);
        }

    }

}
