using Unity.Entities;
using Unity.Rendering;

namespace DOTS.Characters.CharactersMaterialAuthoring
{
    [MaterialProperty("_Animation_number")]
    public struct MaterialOverrideAnimationNumber : IComponentData
    {
        public float Value;
    }

    /// <summary> Store the idle animation number for the character animation</summary>
    public struct IdleComponent : IComponentData
    {
        public float Value;
    }

    /// <summary> Store the walking animation number for the character animation</summary>
    public struct WalkingComponent : IComponentData
    {
        public float Value;
    }
}
