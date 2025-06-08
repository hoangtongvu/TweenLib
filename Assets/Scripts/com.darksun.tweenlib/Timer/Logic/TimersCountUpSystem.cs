using TweenLib.Timer.Data;
using Unity.Burst;
using Unity.Burst.CompilerServices;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace TweenLib.Timer.Logic
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [BurstCompile]
    public partial struct TimersCountUpSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimerList>();
            state.RequireForUpdate<TimerIdPool>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.EntityManager.CompleteDependencyBeforeRW<TimerList>();
            var timerList = SystemAPI.GetSingleton<TimerList>();

            state.Dependency = new CountUpJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
                TimerList = timerList,
            }.ScheduleParallel(timerList.Value.Length, 64, state.Dependency);

        }

        [BurstCompile]
        private partial struct CountUpJob : IJobParallelForBatch
        {
            [ReadOnly] public float DeltaTime;
            [NativeDisableParallelForRestriction] public TimerList TimerList;

            public void Execute(int startIndex, int count)
            {
                int upperBound = startIndex + count;

                for (int i = startIndex; i < upperBound; i++)
                {
                    ref var timerSeconds = ref this.TimerList.Value.ElementAt(i);

                    if (Hint.Likely(timerSeconds.IsEnabled))
                        timerSeconds.Counter += this.DeltaTime;

                }

            }

        }

    }

}