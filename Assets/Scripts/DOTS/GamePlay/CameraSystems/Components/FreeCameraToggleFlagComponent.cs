using Unity.Entities; 

namespace DOTS.GamePlay.CameraSystems
{
    /// <summary>
    /// True: FreeCam is on, False: FreeCam is false
    /// </summary>
    public struct FreeCameraToggleFlag : IComponentData
    {
        public bool Value;
    }
}
