using TweenLib.StandardTweeners;
using TweenLib.Timer.Data;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Systems.Initialization
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [BurstCompile]
    public partial struct CubeTweenCreateSystem : ISystem
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

            state.EntityManager.CompleteDependencyBeforeRW<TimerList>();
            state.EntityManager.CompleteDependencyBeforeRW<TimerIdPool>();

            var timerList = SystemAPI.GetSingleton<TimerList>();
            var timerIdPool = SystemAPI.GetSingleton<TimerIdPool>();

            foreach (var (transformRef, canTweenTag, tweenDataRef) in
                SystemAPI.Query<
                    RefRO<LocalTransform>
                    , EnabledRefRW<Can_TransformPositionTweener_TweenTag>
                    , RefRW<TransformPositionTweener_TweenData>>()
                    .WithOptions(EntityQueryOptions.IgnoreComponentEnabledState))
            {
                UnityEngine.Debug.Log("Create TWEEN");

                var pos = transformRef.ValueRO.Position;
                pos.x += 3f;
                
                TransformPositionTweener.TweenBuilder.Create(0.8f, pos)
                    .WithEase(TweenLib.EasingType.EaseOutBounce)
                    .Build(ref timerList, in timerIdPool, ref tweenDataRef.ValueRW, canTweenTag);
            }

        }

    }

}