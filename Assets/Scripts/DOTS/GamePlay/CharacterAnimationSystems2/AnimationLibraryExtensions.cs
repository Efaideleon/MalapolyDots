using Assets.Scripts.DOTS.Characters.CharactersMaterialAuthoring;
using DOTS.Characters.CharactersMaterialAuthoring;

namespace Assets.Scripts.DOTS.GamePlay.CharacterAnimationSystems2
{
    public static class AnimationLibraryExtensions
    {
        public static ref AnimationData GetClip(this in AnimationDataLibrary library, CharacterAnimationEnum animation, AnimationPhase phase)
        {
            ref var group = ref library.AnimationDataBlobRef.Value.Clips[(int)animation];

            switch (phase)
            {
                case AnimationPhase.Start:
                    return ref group.Start;
                case AnimationPhase.Middle:
                    return ref group.Middle;
                case AnimationPhase.End:
                    return ref group.End;
            }

            return ref group.Start;
        }
    }
}
