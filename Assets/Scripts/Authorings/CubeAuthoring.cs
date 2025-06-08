using TweenLib.ShakeTween.Data;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Authoring
{
    public class CubeAuthoring : MonoBehaviour
    {
        private class Baker : Baker<CubeAuthoring>
        {
            public override void Bake(CubeAuthoring authoring)
            {
                Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

                AddComponent<Can_TransformPositionTweener_TweenTag>(entity);
                SetComponentEnabled<Can_TransformPositionTweener_TweenTag>(entity, false);
                AddComponent<TransformPositionTweener_TweenData>(entity);

                AddComponent(entity, ShakeDataIdHolder.Invalid);

            }
        }
    }
}
