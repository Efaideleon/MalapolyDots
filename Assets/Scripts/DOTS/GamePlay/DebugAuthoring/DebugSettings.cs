using Unity.Entities;
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

    public struct RollConfig : IComponentData
    {
        public bool isCustomEnabled;
        public int customRollValue;
    }
}
