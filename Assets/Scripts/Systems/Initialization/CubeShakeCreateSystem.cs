using TweenLib.ShakeTween.Data;
using TweenLib.ShakeTween.Logic;
using TweenLib.Timer.Data;
using TweenLib.Timer.Logic;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Systems.Initialization
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [BurstCompile]
    public partial struct CubeShakeCreateSystem : ISystem
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
            if (!Input.GetKeyDown(KeyCode.F)) return;

            var em = state.EntityManager;

            TimerHelper.CompleteDependencesBeforeRW(in em);
            em.CompleteDependencyBeforeRW<ShakeDataList>();
            em.CompleteDependencyBeforeRW<ShakeDataIdPool>();

            var timerList = SystemAPI.GetSingleton<TimerList>();
            var timerIdPool = SystemAPI.GetSingleton<TimerIdPool>();
            var shakeDataList = SystemAPI.GetSingleton<ShakeDataList>();
            var shakeDataIdPool = SystemAPI.GetSingleton<ShakeDataIdPool>();

            foreach (var (transformRef, shakeDataIdHolderRef) in
                SystemAPI.Query<
                    RefRO<LocalTransform>
                    , RefRW<ShakeDataIdHolder>>()
                    .WithOptions(EntityQueryOptions.IgnoreComponentEnabledState))
            {
                UnityEngine.Debug.Log("Create SHAKE");

                ShakeBuilder.Create(0.5f, 15f, 2f, in transformRef.ValueRO.Position)
                    .Build(ref timerList, in timerIdPool, ref shakeDataList, in shakeDataIdPool, ref shakeDataIdHolderRef.ValueRW);
                    
            }

        }

    }

}