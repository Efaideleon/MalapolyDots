using Unity.Entities;
using UnityEngine;

namespace DOTS.GamePlay.CameraSystems
{
    public class CameraPanAuthoring : MonoBehaviour
    {

        [SerializeField] uint FloorLayerBitMask = 1u << 8;
        [SerializeField] float MaxFlingSpeed = 30f;
        [SerializeField] float DampingPerSecond = 5f;
        [SerializeField] float StartPanningThreshold = 0;

        public class CameraPanBaker : Baker<CameraPanAuthoring>
        {
            public override void Bake(CameraPanAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);
                AddComponent(entity, new CameraPanSettings 
                {
                    FloorLayerBitMask = authoring.FloorLayerBitMask,
                    MaxFlingSpeed = authoring.MaxFlingSpeed,
                    DampingPerSecond = authoring.DampingPerSecond,
                    StartPanningThreshold = authoring.StartPanningThreshold
                });
            }
        }
    }

    public struct CameraPanSettings : IComponentData
    {
        public uint FloorLayerBitMask;
        public float MaxFlingSpeed;
        public float DampingPerSecond;
        public float StartPanningThreshold;
    }
}
