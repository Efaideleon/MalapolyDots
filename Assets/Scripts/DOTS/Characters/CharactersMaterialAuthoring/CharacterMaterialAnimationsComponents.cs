using Unity.Entities;
using Unity.Rendering;

namespace DOTS.Characters.CharactersMaterialAuthoring
{
    [MaterialProperty("_Animation_number")]
    public struct MaterialOverrideAnimationNumber : IComponentData
    {
        public float Value;
    }

    [MaterialProperty("_1_frame")]
    public struct MaterialOverrideFrameNumber : IComponentData
    {
        public float Value;
    }

    [MaterialProperty("_1_use_time")]
    public struct MaterialOverrideUseTime : IComponentData
    {
        public float Value;
    }

    [MaterialProperty("_1_spped")]
    public struct MaterialOverrideIdleSpeed : IComponentData
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
