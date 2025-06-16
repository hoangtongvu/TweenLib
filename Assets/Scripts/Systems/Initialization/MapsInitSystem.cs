using TweenLib.Timer.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Systems.Initialization
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [BurstCompile]
    public partial struct MapsInitSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            var em = state.EntityManager;
            var entity = em.CreateEntity();

            em.AddComponentData(entity, new TimerList
            {
                Value = new(100, Allocator.Persistent),
            });

            em.AddComponentData(entity, new TimerIdPool
            {
                Value = new(Allocator.Persistent),
            });

            state.Enabled = false;
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
        }

    }

}