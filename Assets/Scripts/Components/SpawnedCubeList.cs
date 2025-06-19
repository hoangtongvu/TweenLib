using Unity.Collections;
using Unity.Entities;

namespace Components
{
    public struct SpawnedCubeList : IComponentData
    {
        public NativeList<Entity> Value;
    }

}
