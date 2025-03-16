using Unity.Entities;

namespace TweenLib
{
    public interface ITweener<Component, Target>
        where Component : unmanaged, IComponentData
        where Target : unmanaged
    {
        void Tween(ref Component componentData, in float baseSpeed, in Target target);

        bool CanStop(in Component componentData, in float lifeTimeSecond, in float baseSpeed, in Target target);
    }

}