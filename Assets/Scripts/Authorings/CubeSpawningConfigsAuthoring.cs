using Components;
using Unity.Entities;
using UnityEngine;

namespace Authoring
{
    public class CubeSpawningConfigsAuthoring : MonoBehaviour
    {
        public CubeSpawningConfigsManaged CubeSpawningConfigsManaged = new()
        {
            SpawnCount = 10000,
            Spacing = 2f,
        };

        private class Baker : Baker<CubeSpawningConfigsAuthoring>
        {
            public override void Bake(CubeSpawningConfigsAuthoring authoring)
            {
                Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
                Entity prefabEntity = GetEntity(authoring.CubeSpawningConfigsManaged.SpawnPrefabGO, TransformUsageFlags.Dynamic);

                AddComponent(entity, authoring.CubeSpawningConfigsManaged.ToUnmanagedVersion(in prefabEntity));

            }
        }
    }
}
