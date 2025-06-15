
namespace TweenLib.Commons
{
    public struct BoolByte
    {
        public byte Value;

        public static implicit operator bool(BoolByte b) => b.Value != 0;

        public static implicit operator BoolByte(bool b) => new() { Value = b ? (byte)1 : (byte)0 };
    }

}
