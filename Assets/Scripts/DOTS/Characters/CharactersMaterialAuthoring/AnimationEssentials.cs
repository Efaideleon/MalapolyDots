using Unity.Entities;
using UnityEngine;

namespace DOTS.Characters.CharactersMaterialAuthoring
{
    public class AnimationEssentials : MonoBehaviour
    {
        public class AnimationEssentialsBaker : Baker<AnimationEssentials>
        {
            public override void Bake(AnimationEssentials authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Renderable);
                AddComponent(entity, new AnimationNumberMO { Value = 0 });
                AddComponent(entity, new AnimationPlayState { Value = PlayState.NotPlaying });
                AddComponent(entity, new CurrentAnimation { Value = Animations.Idle });
            }
        }
    }
}
