using Components;
using TweenLib.StandardTweeners.ShakePositionTweeners;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Systems.Initialization.ShakeCreateSystems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [BurstCompile]
    public partial struct CubeShakePositionXZCreateSystem : ISystem
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

            var shakePositionConfigs = SystemAPI.GetSingleton<ShakePositionConfigs>();
            if (shakePositionConfigs.ShakePositionType != ShakePositionType.XZ) return;

            foreach (var (transformRef, canTweenXTag, tweenDataXRef) in
                SystemAPI.Query<
                    RefRO<LocalTransform>
                    , EnabledRefRW<Can_ShakePositionXZTweener_TweenTag>
                    , RefRW<ShakePositionXZTweener_TweenData>>()
                    .WithOptions(EntityQueryOptions.IgnoreComponentEnabledState))
            {
                UnityEngine.Debug.Log("Create SHAKE");

                var tweenBuilder = ShakePositionXZTweener.TweenBuilder
                    .Create(
                        shakePositionConfigs.Duration
                        , new(shakePositionConfigs.Frequency
                        , shakePositionConfigs.Intensity, 0f))
                    .WithDelay(shakePositionConfigs.DelaySeconds);

                if (shakePositionConfigs.UseCustomStartValue)
                    tweenBuilder = tweenBuilder.WithStartValue(shakePositionConfigs.StartValue);

                tweenBuilder.Build(ref tweenDataXRef.ValueRW, canTweenXTag);

            }

        }

    }

}