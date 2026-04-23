using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

namespace DOTS.GamePlay.DebugAuthoring
{
    public class DebugSettings : MonoBehaviour
    {
        [Header("Roll System")]
        [Tooltip("Stops using random generated number.")]
        public bool isCustomEnabled = false;
        public int customRollValue;

        public class DebugBaker : Baker<DebugSettings>
        {
            public override void Bake(DebugSettings authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);
                AddComponent(entity, new RollConfig
                {
                    isCustomEnabled = authoring.isCustomEnabled,
                    customRollValue = authoring.customRollValue
                });
            }
        }
    }

    [GhostComponent]
    public struct RollConfig : IComponentData
    {
        [GhostField]
        public bool isCustomEnabled;

        [GhostField]
        public int customRollValue;
    }
}

