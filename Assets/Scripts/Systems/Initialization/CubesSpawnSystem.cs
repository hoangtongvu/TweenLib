using Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Systems.Initialization
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [BurstCompile]
    public partial struct CubesSpawnSystem : ISystem, ISystemStartStop
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            this.CreateSpawnedCubeListComponent(state.EntityManager);
            
            var query0 = SystemAPI.QueryBuilder()
                .WithAll<
                    LocalTransform>()
                .Build();

            state.RequireForUpdate(query0);
            state.RequireForUpdate<CubeSpawningConfigs>();

        }

        [BurstCompile]
        public void OnStartRunning(ref SystemState state)
        {
            var prefabEntity = SystemAPI.GetSingleton<CubeSpawningConfigs>().SpawnPrefab;
            var spawnedCubeList = SystemAPI.GetSingleton<SpawnedCubeList>();
            this.CreateTheFirstCube(state.EntityManager, in spawnedCubeList, in prefabEntity);
        }

        [BurstCompile]
        public void OnStopRunning(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!Input.GetKeyDown(KeyCode.D)) return;

            var cubeSpawningConfigs = SystemAPI.GetSingleton<CubeSpawningConfigs>();
            var spawnedCubeList = SystemAPI.GetSingleton<SpawnedCubeList>();

            var em = state.EntityManager;

            em.DestroyEntity(spawnedCubeList.Value.ToArray(Allocator.Temp));
            spawnedCubeList.Value.Clear();

            var spawnedEntities = em.Instantiate(
                cubeSpawningConfigs.SpawnPrefab
                , cubeSpawningConfigs.SpawnCount
                , Allocator.TempJob);

            spawnedCubeList.Value.AddRange(spawnedEntities);

            state.Dependency = new SetPositionsJob
            {
                Origin = cubeSpawningConfigs.Origin,
                Spacing = cubeSpawningConfigs.Spacing,
                SpawnedEntities = spawnedEntities,
                TransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(),
            }.ScheduleParallel(spawnedEntities.Length, 64, state.Dependency);

        }

        [BurstCompile]
        private void CreateSpawnedCubeListComponent(in EntityManager em)
        {
            var spawnedCubeListEntity = em.CreateEntity();
            em.AddComponentData(spawnedCubeListEntity, new SpawnedCubeList
            {
                Value = new(50000, Allocator.Persistent),
            });
        }

        [BurstCompile]
        private void CreateTheFirstCube(
            in EntityManager em
            , in SpawnedCubeList spawnedCubeList
            , in Entity prefabEntity)
        {
            spawnedCubeList.Value.Add(em.Instantiate(prefabEntity));
        }

        [BurstCompile]
        public struct SetPositionsJob : IJobParallelForBatch
        {
            [ReadOnly] public float Spacing;
            [ReadOnly] public float3 Origin;

            [DeallocateOnJobCompletion]
            public NativeArray<Entity> SpawnedEntities;

            [NativeDisableParallelForRestriction]
            public ComponentLookup<LocalTransform> TransformLookup;

            public void Execute(int startIndex, int count)
            {
                int gridSize = (int)math.ceil(math.sqrt(SpawnedEntities.Length));

                for (int i = 0; i < count; i++)
                {
                    int index = startIndex + i;
                    if (index >= this.SpawnedEntities.Length) return;

                    int x = index % gridSize;
                    int z = index / gridSize;

                    float3 position = this.Origin + new float3(x * this.Spacing, 0f, z * this.Spacing);

                    if (this.TransformLookup.HasComponent(this.SpawnedEntities[index]))
                    {
                        this.TransformLookup[this.SpawnedEntities[index]] = LocalTransform.FromPosition(position);
                    }

                }

            }

        }

    }

}