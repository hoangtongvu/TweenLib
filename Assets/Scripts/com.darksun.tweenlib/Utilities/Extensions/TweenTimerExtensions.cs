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
        public static void Tick(ref this TweenTimer tweenTimer, in float deltaTime)
            => tweenTimer.ElapsedSeconds += deltaTime;

		[BurstCompile]
        public static void ResetTimeCounter(ref this TweenTimer tweenTimer)
            => tweenTimer.ElapsedSeconds = 0f;

        [BurstCompile]
        public static float GetNormalizedTime(in this TweenTimer tweenTimer)
        {
            float rawNormalizedTime = tweenTimer.ElapsedSeconds / tweenTimer.DurationSeconds;

            return tweenTimer.NormalizedTimeDirection switch
            {
                NormalizedTimeDirection.Forward => rawNormalizedTime,
                NormalizedTimeDirection.Backward => 1 - rawNormalizedTime,
                _ => throw new System.NotImplementedException(),
            };
        }

        [BurstCompile]
        public static void ToggleNormalizedTimeDirection(ref this TweenTimer tweenTimer)
            => tweenTimer.NormalizedTimeDirection = (NormalizedTimeDirection)(1 - (byte)tweenTimer.NormalizedTimeDirection);

        [BurstCompile]
        public static void IncreaseLoopCounter(ref this TweenTimer tweenTimer)
            => tweenTimer.LoopCounter++;

        [BurstCompile]
        public static BoolByte LoopCounterExceeded(in this TweenTimer tweenTimer)
            => tweenTimer.LoopCounter > tweenTimer.LoopCountLimit;

        [BurstCompile]
        public static BoolByte IsInfiniteLoop(in this TweenTimer tweenTimer)
            => tweenTimer.LoopCountLimit == byte.MinValue;

    }

}
