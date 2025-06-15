using TweenLib.ShakeTween.Data;
using TweenLib.StandardTweeners;
using Unity.Entities;
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

                TransformPositionTweener.AddTweenComponents(this, entity);

                AddComponent(entity, ShakeDataIdHolder.Invalid);

            }
        }
    }
}
