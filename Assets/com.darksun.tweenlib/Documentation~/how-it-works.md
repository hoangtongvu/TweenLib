# How it works

Suppose we define a Tweener **manually**:

```cs
[BurstCompile]
public partial struct TransformPositionTweener : ITweener<LocalTransform, float3>
{
    [BurstCompile]
    public void GetDefaultStartValue(in LocalTransform componentData, out float3 defaultStartValue)
        => defaultStartValue = componentData.Position;

    [BurstCompile]
    public void GetSum(in float3 a, in float3 b, out float3 result)
        => result = a + b;

    [BurstCompile]
    public void GetDifference(in float3 a, in float3 b, out float3 result)
        => result = a - b;

    [BurstCompile]
    public void Tween(ref LocalTransform componentData, in float normalizedTime, EasingType easingType, in float3 startValue, in float3 target)
    {
        TweenHelper.Float3Tween(in normalizedTime, easingType, in startValue, in target, out componentData.Position);
    }
}
```

After that, the following code will be generated to the same assembly as the defined Tweener by SourceGenerator:

1. `{tweenerName}_TweenData` (IComponentData)
2. `Can_{tweenerName}_TweenTag` (IComponentData)
3. Tween Components Baking Helper
4. Tweener Static Methods
5. Tween System (ISystem) + TweenJob (IJobChunk)
6. Tween Builder

## 1. `{tweenerName}_TweenData`

The generated TweenData component contains all tweening related data. It will be in the same namespace as the `TComponent` defined in the Tweener (in this case: `Unity.Transforms.LocalTransform`):

```cs
namespace Unity.Transforms
{
    public struct TransformPositionTweener_TweenData : IComponentData
    {
        public TweenTimer TweenTimer;
        public Unity.Mathematics.float3 Target;

        public bool StartValueInitialized;
        public bool UseCustomStartValue;
        public Unity.Mathematics.float3 StartValue;

        public EasingType EasingType;
    }
}
```

## 2. `Can_{tweenerName}_TweenTag`

`Can_{tweenerName}_TweenTag` will be used to trigger the Tween, it also stays in the same namespace as `TComponent`:

```cs
namespace Unity.Transforms
{
    public struct Can_TransformPositionTweener_TweenTag : IComponentData, IEnableableComponent
    {
    }
}
```

## 3. Tween Components Baking Helper

This generated baking helper is a method to quickly add Tween components to the entity in the Authoring phase:

```cs
namespace TweenLib.StandardTweeners
{
    public partial struct TransformPositionTweener
    {
        public static void AddTweenComponents(IBaker baker, Entity entity)
        {
            baker.AddComponent<Can_TransformPositionTweener_TweenTag>(entity);
            baker.SetComponentEnabled<Can_TransformPositionTweener_TweenTag>(entity, false);
            baker.AddComponent<TransformPositionTweener_TweenData>(entity);
        }
    }
}
```

## 4. Tweener Static Methods

Since our Tweeners do not contain any data, SourceGenerator will generate a static version for each Tweener's method for performance purposes:

```cs
namespace TweenLib.StandardTweeners
{
    public partial struct TransformPositionTweener
    {
        [BurstCompile]
        static public void GetDefaultStartValue_Static(in LocalTransform componentData, out float3 defaultStartValue)
            => defaultStartValue = componentData.Position;

        [BurstCompile]
        static public void GetSum_Static(in float3 a, in float3 b, out float3 result)
            => result = a + b;

        [BurstCompile]
        static public void GetDifference_Static(in float3 a, in float3 b, out float3 result)
            => result = a - b;

        [BurstCompile]
        static public void Tween_Static(ref LocalTransform componentData, in float normalizedTime, EasingType easingType, in float3 startValue, in float3 target)
        {
            TweenHelper.Float3Tween(in normalizedTime, easingType, in startValue, in target, out componentData.Position);
        }
    }
}
```

## 5. Tween System (ISystem) + TweenJob (IJobChunk)

All Tween systems (main systems that do the Tweens) will be in the same namespace `TweenLib.Systems` and update in [TweenLib.TweenSystemGroup](../TweenSystemGroup.cs):

```cs
[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class TweenSystemGroup : ComponentSystemGroup { }
```

The `TweenJob` will do the following things:
- Tick the `TweenTimer`
- Do the delay checking
- If `TweenTimer` timed out:
    - Increase `loopCounter` by 1
    - Reset the `timeCounter`
    - Do some actions based on Tween's `LoopType`
- When `TweenTimer`'s `loopCounter` exceeds the `loopCountLimit`:
    - Assign final value to the component's value
    - Stop tweening
- Try initializing the `StartValue`
- Do the Tween

An example of a generated **TweenSystem**:

<details>

<summary>It's long, click to view</summary>

```cs
namespace TweenLib.Systems
{
    [UpdateInGroup(typeof(TweenLib.TweenSystemGroup))]
    [BurstCompile]
    public partial struct TransformPositionTweener_TweenSystem : ISystem
    {
        private EntityQuery query;
        private ComponentTypeHandle<LocalTransform> componentTypeHandle;
        private ComponentTypeHandle<Can_TransformPositionTweener_TweenTag> canTweenTagTypeHandle;
        private ComponentTypeHandle<TransformPositionTweener_TweenData> tweenDataTypeHandle;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            var queryBuilder = new EntityQueryBuilder(Allocator.Temp);
            this.query = queryBuilder
                .WithAllRW<LocalTransform>()
                .WithAllRW<TransformPositionTweener_TweenData>()
                .WithAll<Can_TransformPositionTweener_TweenTag>()
                .Build(ref state);

            this.componentTypeHandle = state.GetComponentTypeHandle<LocalTransform>(false);
            this.canTweenTagTypeHandle = state.GetComponentTypeHandle<Can_TransformPositionTweener_TweenTag>(false);
            this.tweenDataTypeHandle = state.GetComponentTypeHandle<TransformPositionTweener_TweenData>(false);

            state.RequireForUpdate(this.query);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            this.componentTypeHandle.Update(ref state);
            this.canTweenTagTypeHandle.Update(ref state);
            this.tweenDataTypeHandle.Update(ref state);

            state.Dependency = new TweenIJC
            {
                DeltaTime = state.WorldUnmanaged.Time.DeltaTime,
                ComponentTypeHandle = this.componentTypeHandle,
                CanTweenTagTypeHandle = this.canTweenTagTypeHandle,
                TweenDataTypeHandle = this.tweenDataTypeHandle,
            }.ScheduleParallel(this.query, state.Dependency);
                
        }

        [BurstCompile]
        public struct TweenIJC : IJobChunk
        {
            [ReadOnly] public float DeltaTime;

            public ComponentTypeHandle<LocalTransform> ComponentTypeHandle;
            public ComponentTypeHandle<Can_TransformPositionTweener_TweenTag> CanTweenTagTypeHandle;
            public ComponentTypeHandle<TransformPositionTweener_TweenData> TweenDataTypeHandle;

            [BurstCompile]
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                var canTweenTagEnabledMask_RW = chunk.GetEnabledMask(ref this.CanTweenTagTypeHandle);
                var componentArray = chunk.GetNativeArray(ref this.ComponentTypeHandle);
                var tweenDataArray = chunk.GetNativeArray(ref this.TweenDataTypeHandle);

                var enumerator = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);

                while (enumerator.NextEntityIndex(out var i))
                {
                    ref var component = ref componentArray.ElementAt(i);
                    ref var tweenData = ref tweenDataArray.ElementAt(i);
                    var canTweenTag = canTweenTagEnabledMask_RW.GetEnabledRefRW<Can_TransformPositionTweener_TweenTag>(i);

                    tweenData.TweenTimer.Tick(in this.DeltaTime);

                    if (!tweenData.TweenTimer.DelayEnded)
                    {
                        if (!tweenData.TweenTimer.TimeCounterReachedDelayLimit()) continue;
                        tweenData.TweenTimer.DelayEnded = true;
                        tweenData.TweenTimer.ResetTimeCounter();
                    }
    
                    float finalNormalizedTime = 1f;

                    if (tweenData.TweenTimer.TimedOut())
                    {
                        tweenData.TweenTimer.IncreaseLoopCounter();
                        tweenData.TweenTimer.ResetTimeCounter();
                        
                        switch (tweenData.TweenTimer.LoopType)
                        {
                            case LoopType.Restart:
                                finalNormalizedTime = 1f;
                                break;
                            case LoopType.Flip:
                                finalNormalizedTime = 0f;

                                var temp = tweenData.StartValue;
                                tweenData.StartValue = tweenData.Target;
                                tweenData.Target = temp;
                                break;
                            case LoopType.Incremental:
                                finalNormalizedTime = 0f;

                                TransformPositionTweener.GetDifference_Static(in tweenData.Target, in tweenData.StartValue, out var difference);
                                TransformPositionTweener.GetSum_Static(in tweenData.StartValue, in difference, out tweenData.StartValue);
                                TransformPositionTweener.GetSum_Static(in tweenData.Target, in difference, out tweenData.Target);
                                break;
                            case LoopType.Yoyo:
                                finalNormalizedTime = 1 - tweenData.TweenTimer.LoopCounter % 2;
                                tweenData.TweenTimer.ToggleNormalizedTimeDirection();
                                break;
                        }
                    }

                    if (!tweenData.TweenTimer.IsInfiniteLoop() && tweenData.TweenTimer.LoopCounterExceeded())
                    {
                        // Stop tweening
                        canTweenTag.ValueRW = false;
                        
                        // Finalize the component on tween stop
                        TransformPositionTweener.Tween_Static(
                            ref component
                            , finalNormalizedTime
                            , tweenData.EasingType
                            , in tweenData.StartValue
                            , in tweenData.Target);

                        continue;
                    }

                    if (!tweenData.StartValueInitialized)
                    {
                        if (!tweenData.UseCustomStartValue)
                            TransformPositionTweener.GetDefaultStartValue_Static(in component, out tweenData.StartValue);

                        tweenData.StartValueInitialized = true;
                    }
    
                    TransformPositionTweener.Tween_Static(
                        ref component
                        , tweenData.TweenTimer.GetNormalizedTime()
                        , tweenData.EasingType
                        , in tweenData.StartValue
                        , in tweenData.Target);
                }
            }
        }
    }
}
```

</details>

## 6. Tween Builder

`TweenBuilder` will be generated as a nested struct of the defined Tweener. It is used at runtime to fluently configure and build (schedule) Tweens on components.

An example of a generated **TweenBuilder**:

<details>

<summary>It's long, click to view</summary>

```cs
namespace TweenLib.StandardTweeners
{
    public partial struct TransformPositionTweener
    {
        [BurstCompile]
        public struct TweenBuilder :
            ITweenBuilder<
                float3
                , Can_TransformPositionTweener_TweenTag
                , TransformPositionTweener_TweenData>
        {
            private TransformPositionTweener_TweenData tweenData;

            public static TweenBuilder Create(float durationSeconds, in float3 target) => new(durationSeconds, in target);

            public TweenBuilder(float durationSeconds, in float3 target)
            {
                this.tweenData = new()
                {
                    TweenTimer = new()
                    {
                        DurationSeconds = durationSeconds,
                        LoopCounter = 1,
                        LoopCountLimit = 1,
                    },
                    Target = target,
                    EasingType = EasingType.Linear,
                };
                
            }

            [BurstCompile]
            public TweenBuilder WithStartValue(in float3 startValue)
            {
	            this.tweenData.StartValue = startValue;
	            this.tweenData.UseCustomStartValue = true;
                return this;
            }

            [BurstCompile]
            public TweenBuilder WithEase(EasingType easingType)
            {
	            this.tweenData.EasingType = easingType;
                return this;
            }

            [BurstCompile]
            public TweenBuilder WithLoops(LoopType loopType, byte loopCount = byte.MinValue)
            {
	            this.tweenData.TweenTimer.LoopCountLimit = loopCount;
	            this.tweenData.TweenTimer.LoopType = loopType;
                return this;
            }

            [BurstCompile]
            public TweenBuilder WithDelay(float delaySeconds)
            {
	            this.tweenData.TweenTimer.DelaySeconds = delaySeconds;
                return this;
            }

            [BurstCompile]
            public void Build(
                ref TransformPositionTweener_TweenData tweenData
                , in EnabledRefRW<Can_TransformPositionTweener_TweenTag> canTweenTag)
            {
                tweenData = this.tweenData;
                canTweenTag.ValueRW = true;
            }
        }
    }
}
```

</details>