using TweenLib.Timer.Data;
using Unity.Burst;

namespace TweenLib.Timer.Logic
{
    [BurstCompile]
    public static class TimerSecondsExtensions
    {
        [BurstCompile]
        public static float GetNormalizedTime(in this TimerSeconds timerSeconds, in float duration)
            => timerSeconds.Counter / duration;

    }

}
