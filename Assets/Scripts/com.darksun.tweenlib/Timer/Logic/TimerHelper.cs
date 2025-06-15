using TweenLib.Timer.Data;
using Unity.Burst;
using Unity.Entities;

namespace TweenLib.Timer.Logic
{
    [BurstCompile]
    public static class TimerHelper
    {
        /// <summary>
        /// Add a new timer to TimerList
        /// </summary>
        /// <returns>Id of timer</returns>
        [BurstCompile]
        public static int AddTimer(
            ref TimerList timerList
            , in TimerIdPool timerIdPool)
        {
            bool hasReusableId = timerIdPool.Value.TryDequeue(out int id);

            if (hasReusableId)
            {
                timerList.Value[id] = TimerSeconds.Default;
            }
            else
            {
                id = timerList.Value.Length;
                timerList.Value.Add(TimerSeconds.Default);
            }

            return id;
        }

        /// <summary>
        /// Remove a timer from id
        /// </summary>
        [BurstCompile]
        public static void RemoveTimer(
            in TimerList timerList
            , in TimerIdPool timerIdPool
            , in int id)
        {
            ref var timerSeconds = ref timerList.Value.ElementAt(id);
            timerSeconds.IsEnabled = false;

            timerIdPool.Value.Enqueue(id);
        }

        [BurstCompile]
        public static void CompleteDependencesBeforeRW(in EntityManager em)
        {
            em.CompleteDependencyBeforeRW<TimerList>();
            em.CompleteDependencyBeforeRW<TimerIdPool>();
        }

    }

}
