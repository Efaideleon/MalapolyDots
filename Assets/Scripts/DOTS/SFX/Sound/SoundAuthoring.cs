using Unity.Entities;
using UnityEngine;

namespace DOTS.SFX.Sound
{
    public class ClickSoundClipComponent : IComponentData
    {
        public AudioClip Value;
    }

    public class AudioSourceComponent : IComponentData
    {
        public GameObject AudioSourceGO;
    }

    public class SoundAuthoring : MonoBehaviour
    {
        [SerializeField] public AudioClip ClickSound;
        [SerializeField] public AudioSource AudioSource;

        public class SoundBaker : Baker<SoundAuthoring>
        {
            public override void Bake(SoundAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);
                AddComponentObject(entity, new ClickSoundClipComponent { Value = authoring.ClickSound });
                AddComponentObject(entity, new AudioSourceComponent { AudioSourceGO = authoring.AudioSource.gameObject });
            }
        }
    }
}
