using TweenLib.ShakeTween.Data;
using TweenLib.Timer.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace TweenLib.ShakeTween.Logic
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [BurstCompile]
    public partial struct ShakeSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimerList>();
            state.RequireForUpdate<TimerIdPool>();
            state.RequireForUpdate<ShakeDataList>();
            state.RequireForUpdate<ShakeDataIdPool>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.EntityManager.CompleteDependencyBeforeRW<TimerList>(); // BUG: When make this RO, an exception will be thrown.
            state.EntityManager.CompleteDependencyBeforeRO<ShakeDataList>();

            var timerList = SystemAPI.GetSingleton<TimerList>();
            var shakeDataList = SystemAPI.GetSingleton<ShakeDataList>();

            state.Dependency = new ShakeJob
            {
                TimerList = timerList,
                ShakeDataList = shakeDataList,
            }.ScheduleParallel(state.Dependency);

        }

        [BurstCompile]
        private partial struct ShakeJob : IJobEntity
        {
            [ReadOnly] public TimerList TimerList;
            [ReadOnly] public ShakeDataList ShakeDataList;

            void Execute(
                ref LocalTransform transform
                , ref ShakeDataIdHolder shakeDataIdHolder)
            {
                if (shakeDataIdHolder == ShakeDataIdHolder.Invalid) return;

                var shakeData = this.ShakeDataList.Value[shakeDataIdHolder.Value];
                if (shakeData.IsEnabled == 0)
                {
                    transform.Position = shakeData.TargetPos;
                    shakeDataIdHolder = ShakeDataIdHolder.Invalid;
                    return;
                }

                this.Shake(ref transform, this.TimerList.Value[shakeData.TimerId].Counter, shakeData);

            }

            [BurstCompile]
            private void Shake(ref LocalTransform transform, in float t, in ShakeData shakeData)
            {
                float scale = 1f - math.saturate(t / shakeData.Duration);
                float time = t * shakeData.Frequency;

                float offsetX = noise.cnoise(new float2(time, 0f));
                float offsetY = noise.cnoise(new float2(0f, time));
                float offsetZ = noise.cnoise(new float2(time, time));
                float3 offset = scale * shakeData.Intensity * new float3(offsetX, offsetY, offsetZ);

                transform.Position = shakeData.TargetPos + offset;
            }

        }

    }

}