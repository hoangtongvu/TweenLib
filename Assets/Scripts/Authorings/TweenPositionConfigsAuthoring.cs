using Components;
using TweenLib.Commons;
using TweenLib.Utilities;
using Unity.Entities;
using UnityEngine;

namespace Authoring
{
    public class TweenPositionConfigsAuthoring : MonoBehaviour
    {
        public TweenPositionConfigs TweenPositionConfigs = new()
        {
            Duration = 1f,
            UseCustomStartValue = true,
            StartValue = new(-3f, 0f, 0f),
            TargetValue = new(3f, 0f, 0f),
            TweenPositionType = TweenPositionType.XYZ,
            EasingType = EasingType.Linear,
            LoopCount = 1,
            LoopType = LoopType.Restart,
        };

        private class Baker : Baker<TweenPositionConfigsAuthoring>
        {
            public override void Bake(TweenPositionConfigsAuthoring authoring)
            {
                Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

                AddComponent(entity, authoring.TweenPositionConfigs);

            }
        }
    }
}
