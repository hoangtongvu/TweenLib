using TweenLib.ShakeTween;
using TweenLib.StandardTweeners;
using TweenLib.Timer;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Systems.Initialization
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [BurstCompile]
    public partial struct CubeTweenSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            var query0 = SystemAPI.QueryBuilder()
                .WithAll<
                    LocalTransform>()
                .Build();

            state.RequireForUpdate(query0);

        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!Input.GetKeyDown(KeyCode.Space)) return;

            foreach (var (transformRef, canTweenTag, tweenDataRef) in
                SystemAPI.Query<
                    RefRO<LocalTransform>
                    , EnabledRefRW<Can_TransformPositionTweener_TweenTag>
                    , RefRW<TransformPositionTweener_TweenData>>()
                    .WithOptions(EntityQueryOptions.IgnoreComponentEnabledState))
            {
                UnityEngine.Debug.Log("TWEEN");

                var pos = transformRef.ValueRO.Position;
                pos.x += 3f;
                
                TransformPositionTweener.TweenBuilder.Create()
                    .WithBaseSpeed(5f)
                    .WithTarget(pos)
                    .Build(ref tweenDataRef.ValueRW, canTweenTag);
            }

        }

    }

}