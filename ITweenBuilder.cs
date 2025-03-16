using Unity.Entities;

namespace TweenLib
{
    public interface ITweenBuilder<Target, CanTweenTag, TweenData>
        where Target : unmanaged
        where CanTweenTag : unmanaged, IComponentData, IEnableableComponent
        where TweenData : unmanaged, IComponentData
    {
    }

}
