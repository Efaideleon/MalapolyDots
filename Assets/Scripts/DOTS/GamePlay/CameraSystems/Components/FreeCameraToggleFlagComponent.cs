using Unity.Entities;

namespace DOTS.GamePlay.CameraSystems.Components
{
    /// <summary>
    /// True: FreeCam is on, False: FreeCam is false
    /// </summary>
    public struct FreeCameraToggleFlag : IComponentData
    {
        public bool Value;
    }
}
