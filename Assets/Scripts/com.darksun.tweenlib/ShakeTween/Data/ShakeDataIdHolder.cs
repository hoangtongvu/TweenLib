using System;
using Unity.Entities;

namespace TweenLib.ShakeTween.Data
{
    public struct ShakeDataIdHolder : IComponentData, IEquatable<ShakeDataIdHolder>
    {
        public int Value;
        public static readonly ShakeDataIdHolder Invalid = new()
        {
            Value = -1,
        };

        public override bool Equals(object obj)
        {
            if (obj is ShakeDataIdHolder shakeDataIdHolder)
            {
                return Equals(shakeDataIdHolder);
            }

            return base.Equals(obj);
        }

        public static bool operator ==(ShakeDataIdHolder first, ShakeDataIdHolder second) => first.Equals(second);

        public static bool operator !=(ShakeDataIdHolder first, ShakeDataIdHolder second) => !(first == second);

        public bool Equals(ShakeDataIdHolder other) => Value.Equals(other.Value);

        public override int GetHashCode() => Value.GetHashCode();

    }

}