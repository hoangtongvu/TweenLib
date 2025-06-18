# How to use Shake tween

As Shake tweens do not have the `TargetValue` and require additional data like `Frequency` and `Intensity` to operate, we can repurpose the the `TargetValue` to store these values.

**ShakeTweener Template**:
```cs
[BurstCompile]
public partial struct ShakePositionTweener : ITweener<LocalTransform, float3>
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
    public void Tween(ref LocalTransform componentData, in float normalizedTime, EasingType easingType, in float3 startValue, in float3 shakeData)
    {
        // shakeData.x = frequency, shakeData.y = intensity
        ShakeHelper.Float3Shake(in normalizedTime, shakeData.x, shakeData.y, in startValue, out componentData.Position);
    }
}
```

```cs
[BurstCompile]
public static class ShakeHelper
{
    [BurstCompile]
    public static void Float3Shake(
        in float normalizedTime
        , in float frequency
        , in float intensity
        , in float3 startValue
        , out float3 value)
    {
        float time = normalizedTime * frequency;

        float offsetX = noise.cnoise(new float2(time, 0f));
        float offsetY = noise.cnoise(new float2(0f, time));
        float offsetZ = noise.cnoise(new float2(time, time));
        float3 offset = (1 - normalizedTime) * intensity * new float3(offsetX, offsetY, offsetZ);

        value = startValue + offset;
    }

}
```

**Build Shake Tween in your System**:
```cs
foreach (var (transformRef, canTweenXTag, tweenDataXRef) in
    SystemAPI.Query<
        RefRO<LocalTransform>
        , EnabledRefRW<Can_ShakePositionTweener_TweenTag>
        , RefRW<ShakePositionTweener_TweenData>>()
        .WithOptions(EntityQueryOptions.IgnoreComponentEnabledState))
{
    float duration = 2f;
    float frequency = 15f;
    float intensity = 1f;
    
    // Create shake tween just like a normal tween,
    // but use the TargetValue to store Frequency and Intensity,
    // WithEase() won't work as EasingType is not used for shake tween
    ShakePositionTweener.TweenBuilder
        .Create(
            duration
            , new(frequency, intensity, 0f))
        .WithDelay(0.2f)
        .Build(ref tweenDataXRef.ValueRW, canTweenXTag);

}
```

[ShakeHelper](../../com.darksun.tweenlib/Utilities/Helpers/ShakeHelper.cs) also supports helper methods `FloatShake()` & `Float2Shake()` which are used in:
- [ShakePositionXTweener](../../com.darksun.tweenlib/StandardTweeners/ShakePositionTweeners/ShakePositionXTweener.cs)
- [ShakePositionXYTweener](../../com.darksun.tweenlib/StandardTweeners/ShakePositionTweeners/ShakePositionXYTweener.cs)
- [ShakePositionXZTweener](../../com.darksun.tweenlib/StandardTweeners/ShakePositionTweeners/ShakePositionXZTweener.cs)