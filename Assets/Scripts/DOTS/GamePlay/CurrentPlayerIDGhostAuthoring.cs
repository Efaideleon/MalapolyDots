using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

namespace Assets.Scripts.DOTS.GamePlay
{
    public class CurrentPlayerIDGhostAuthoring : MonoBehaviour
    {
        public class CurrentPlayerIDGhostBaker : Baker<CurrentPlayerIDGhostAuthoring>
        {
            public override void Bake(CurrentPlayerIDGhostAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);
                AddComponent(entity, new CurrentPlayerID { Value = -1 });
            }
        }
    }

    [GhostComponent]
    public struct CurrentPlayerID : IComponentData
    {
        [GhostField]
        public int Value;
    }

}
