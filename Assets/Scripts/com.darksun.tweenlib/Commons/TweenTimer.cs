
namespace TweenLib.Commons
{
    [System.Serializable]
    public struct TweenTimer
    {
        public float ElapsedSeconds;
        public float DurationSeconds;
        public NormalizedTimeDirection NormalizedTimeDirection;

        public byte LoopCounter;
        public byte LoopCountLimit;
        public LoopType LoopType;

        public BoolByte DelayEnded;
        public float DelaySeconds;
    }

    public enum NormalizedTimeDirection : byte
    {
        Forward = 0,
        Backward = 1,
    }

}
