using TweenLib.StandardTweeners;
using TweenLib.StandardTweeners.ShakePositionTweeners;
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

                ShakePositionTweener.AddTweenComponents(this, entity);
                ShakePositionXYTweener.AddTweenComponents(this, entity);
                ShakePositionXZTweener.AddTweenComponents(this, entity);
                ShakePositionXTweener.AddTweenComponents(this, entity);

            }
        }
    }
}
