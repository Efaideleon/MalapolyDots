using Unity.Entities;
using UnityEngine;

namespace DOTS.GamePlay.CameraSystems
{
    public class CameraSceneAuthoring : MonoBehaviour
    {
        [SerializeField] int RotationAngle;
        [SerializeField] int RotationSpeed;
        //TODO: scenes can choose between rotating the shortets or longest path.
        public class CameraSceneBaker : Baker<CameraSceneAuthoring>
        {
            public override void Bake(CameraSceneAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
                AddComponent(entity, new CameraSceneTag { });
                AddComponent(entity, new CameraSceneData
                {
                    RotationAngleY = authoring.RotationAngle,
                    RotationSpeed = authoring.RotationSpeed
                });
            }
        }
    }

    public struct CameraSceneTag : IComponentData
    { }

    public struct CameraSceneData : IComponentData
    {
        public int RotationAngleY;
        public int RotationSpeed;
    }
}
