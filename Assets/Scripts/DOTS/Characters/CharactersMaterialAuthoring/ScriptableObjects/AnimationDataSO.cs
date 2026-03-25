using UnityEngine;

namespace DOTS.Characters.CharactersMaterialAuthoring.ScriptableObjects
{
    [CreateAssetMenu(fileName = "AnimationData", menuName = "Scriptable Objects/Animation/AnimationData")]
    public class AnimationDataSO : ScriptableObject
    {
        [Tooltip("The frame rate for the animation.")]
        public float FrameRate;
        [Tooltip("The frame range for the animation.")]
        public FrameRange FrameRange;
        [Tooltip("The animation should loop or not.")]
        public bool Loops;
        [Tooltip("The animation enum type.")]
        public CharacterAnimation AnimationEnum;

        public AnimationData ToAnimationData()
        {
            return new AnimationData 
            { 
                FrameRate = FrameRate,
                FrameRange = FrameRange,
                Loops = Loops,
                HasClip = true
            };
        }
    }
}
