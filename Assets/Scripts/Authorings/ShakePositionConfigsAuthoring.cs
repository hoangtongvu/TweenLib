using Components;
using Unity.Entities;
using UnityEngine;

namespace Authoring
{
    public class ShakePositionConfigsAuthoring : MonoBehaviour
    {
        public ShakePositionConfigs ShakePositionConfigs = new()
        {
            Duration = 2f,
            UseCustomStartValue = true,
            StartValue = new(-3f, 0f, 0f),
            Frequency = 15f,
            Intensity = 1,
            ShakePositionType = ShakePositionType.XYZ,
        };

        private class Baker : Baker<ShakePositionConfigsAuthoring>
        {
            public override void Bake(ShakePositionConfigsAuthoring authoring)
            {
                Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

                AddComponent(entity, authoring.ShakePositionConfigs);

            }
        }
    }
}
