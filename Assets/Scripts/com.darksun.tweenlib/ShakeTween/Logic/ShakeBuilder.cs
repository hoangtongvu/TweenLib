using TweenLib.ShakeTween.Data;
using TweenLib.Timer.Data;
using TweenLib.Timer.Logic;
using Unity.Burst;
using Unity.Mathematics;

namespace TweenLib.ShakeTween.Logic
{
    [BurstCompile]
    public struct ShakeBuilder
    {
        private float3 targetPos;
        private float intensity;
        private float frequency;
        private float duration;

        public static ShakeBuilder Create(float intensity, float frequency, float duration, in float3 targetPos)
        {
            return new()
            {
                intensity = intensity,
                frequency = frequency,
                duration = duration,
                targetPos = targetPos,
            };
        }

        [BurstCompile]
        public void Build(
            ref TimerList timerList
            , in TimerIdPool timerIdPool
            , ref ShakeDataList shakeDataList
            , in ShakeDataIdPool shakeDataIdPool
            , ref ShakeDataIdHolder shakeDataIdHolder)
        {
            var newShakeData = new ShakeData()
            {
                IsEnabled = 1,
                Intensity = intensity,
                Frequency = frequency,
                Duration = duration,
                TargetPos = targetPos,
            };

            if (shakeDataIdHolder != ShakeDataIdHolder.Invalid)
            {
                ref var shakeData = ref shakeDataList.Value.ElementAt(shakeDataIdHolder.Value);
                if (shakeData.IsEnabled == 1)
                {
                    ref var timerSeconds = ref timerList.Value.ElementAt(shakeData.TimerId);
                    timerSeconds.Counter = 0f;

                    newShakeData.TimerId = shakeData.TimerId;
                    shakeData = newShakeData;
                    return;
                }
            }

            newShakeData.TimerId = TimerHelper.AddTimer(ref timerList, in timerIdPool);
            shakeDataIdHolder.Value = ShakeDataHelper.AddToList(ref shakeDataList, in shakeDataIdPool, in newShakeData);

        }

    }

}