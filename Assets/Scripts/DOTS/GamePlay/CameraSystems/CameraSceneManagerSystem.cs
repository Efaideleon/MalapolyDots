using Unity.Entities;

namespace DOTS.GamePlay.CameraSystems
{
    public partial struct CameraSceneManagerSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CameraSceneTag>();
        }

        public void OnUpdate(ref SystemState state)
        {

        }
    }
}
