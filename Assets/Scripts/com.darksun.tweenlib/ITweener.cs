using TweenLib.Utilities;
using Unity.Entities;

namespace TweenLib
{
    public interface ITweener<Component, Target>
        where Component : unmanaged, IComponentData
        where Target : unmanaged
    {
        /// <summary>
        /// Default start value in case the user does not specify it when build the tween.
        /// </summary>
        void GetDefaultStartValue(in Component componentData, out Target defaultStartValue);

        /// <summary>
        /// result = a + b.
        /// </summary>
        void GetSum(in Target a, in Target b, out Target result);

        /// <summary>
        /// result = a - b.
        /// </summary>
        void GetDifference(in Target a, in Target b, out Target result);

        /// <summary>
        /// Main tweening method.
        /// </summary>
        void Tween(ref Component componentData, in float normalizedTime, EasingType easingType, in Target startValue, in Target target);

    }

}