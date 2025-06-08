using TweenLib.ShakeTween.Data;
using TweenLib.Timer.Data;
using TweenLib.Timer.Logic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace TweenLib.ShakeTween.Logic
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [BurstCompile]
    public partial struct DisableShakeSystem : ISystem
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
            state.EntityManager.CompleteDependencyBeforeRW<TimerList>();
            state.EntityManager.CompleteDependencyBeforeRW<TimerIdPool>();
            state.EntityManager.CompleteDependencyBeforeRW<ShakeDataList>();
            state.EntityManager.CompleteDependencyBeforeRW<ShakeDataIdPool>();

            var timerList = SystemAPI.GetSingleton<TimerList>();
            var timerIdPool = SystemAPI.GetSingleton<TimerIdPool>();
            var shakeDataList = SystemAPI.GetSingleton<ShakeDataList>();
            var shakeDataIdPool = SystemAPI.GetSingleton<ShakeDataIdPool>();

            state.Dependency = new CheckAndDisableShakeJob
            {
                TimerList = timerList,
                TimerIdPool = timerIdPool,
                ShakeDataList = shakeDataList,
                ShakeDataIdPool = shakeDataIdPool,
            }.ScheduleParallel(timerList.Value.Length, 64, state.Dependency);

        }

        [BurstCompile]
        private partial struct CheckAndDisableShakeJob : IJobParallelForBatch
        {
            [NativeDisableParallelForRestriction] public TimerList TimerList;
            [NativeDisableParallelForRestriction] public TimerIdPool TimerIdPool;
            [NativeDisableParallelForRestriction] public ShakeDataList ShakeDataList;
            [NativeDisableParallelForRestriction] public ShakeDataIdPool ShakeDataIdPool;

            public void Execute(int startIndex, int count)
            {
                int upperBound = startIndex + count;

                for (int i = startIndex; i < upperBound; i++)
                {
                    var shakeData = this.ShakeDataList.Value[i];
                    if (shakeData.IsEnabled == 0) continue;

                    float timeCounterSeconds = this.TimerList.Value[shakeData.TimerId].Counter;
                    if (timeCounterSeconds < shakeData.Duration) continue;

                    TimerHelper.RemoveTimer(in this.TimerList, in this.TimerIdPool, in shakeData.TimerId);
                    ShakeDataHelper.RemoveFromList(in this.ShakeDataList, in ShakeDataIdPool, in i);

                }

            }

        }

    }

}