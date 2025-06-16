using TweenLib.Commons;
using Unity.Burst;

namespace TweenLib.Utilities.Extensions
{
    [BurstCompile]
    public static class TweenTimerExtensions
    {
        [BurstCompile]
        public static BoolByte TimedOut(in this TweenTimer tweenTimer)
            => tweenTimer.ElapsedSeconds >= tweenTimer.DurationSeconds;

        [BurstCompile]
        public static float GetNormalizedTime(in this TweenTimer tweenTimer)
            => tweenTimer.ElapsedSeconds / tweenTimer.DurationSeconds;

    }

}
