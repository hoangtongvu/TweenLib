using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Components
{
    [System.Serializable]
    public class CubeSpawningConfigsManaged
    {
        public GameObject SpawnPrefabGO;
        public int SpawnCount;
        public float Spacing;
        public float3 Origin;

        public CubeSpawningConfigs ToUnmanagedVersion(in Entity entityPrefab)
        {
            return new()
            {
                SpawnPrefab = entityPrefab,
                SpawnCount = SpawnCount,
                Spacing = Spacing,
                Origin = Origin,
            };
        }

    }

    [System.Serializable]
    public struct CubeSpawningConfigs : IComponentData
    {
        public Entity SpawnPrefab;
        public int SpawnCount;
        public float Spacing;
        public float3 Origin;
    }

}
