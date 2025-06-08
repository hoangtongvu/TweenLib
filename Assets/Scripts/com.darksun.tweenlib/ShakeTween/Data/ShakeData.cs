using Unity.Mathematics;

namespace TweenLib.ShakeTween.Data
{
    public struct ShakeData
    {
        public byte IsEnabled; // 0 = false, 1 = true
        public int TimerId;

        public float Intensity;
        public float Frequency;
        public float Duration;
        public float3 TargetPos;
    }

}