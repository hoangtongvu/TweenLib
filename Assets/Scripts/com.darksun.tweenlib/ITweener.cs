using Unity.Entities;

namespace TweenLib
{
    public interface ITweener<Component, Target>
        where Component : unmanaged, IComponentData
        where Target : unmanaged
    {
        Target GetDefaultStartValue(in Component componentData);

        void Tween(ref Component componentData, in float normalizedTime, EasingType easingType, in Target startValue, in Target target);

    }

}