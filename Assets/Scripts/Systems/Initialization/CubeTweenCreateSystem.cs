using Components;
using TweenLib.StandardTweeners;
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

            var tweenPositionConfigs = SystemAPI.GetSingleton<TweenPositionConfigs>();
            if (tweenPositionConfigs.TweenPositionType != TweenPositionType.XYZ) return;

            foreach (var (transformRef, canTweenTag, tweenDataRef) in
                SystemAPI.Query<
                    RefRO<LocalTransform>
                    , EnabledRefRW<Can_TransformPositionTweener_TweenTag>
                    , RefRW<TransformPositionTweener_TweenData>>()
                    .WithOptions(EntityQueryOptions.IgnoreComponentEnabledState))
            {
                UnityEngine.Debug.Log("Create TWEEN");

                var tweenBuilder = TransformPositionTweener.TweenBuilder
                    .Create(tweenPositionConfigs.Duration, tweenPositionConfigs.TargetValue)
                    .WithEase(tweenPositionConfigs.EasingType);

                if (tweenPositionConfigs.UseCustomStartValue)
                    tweenBuilder = tweenBuilder.WithStartValue(tweenPositionConfigs.StartValue);

                tweenBuilder.Build(ref tweenDataRef.ValueRW, canTweenTag);
            }

        }

    }

}