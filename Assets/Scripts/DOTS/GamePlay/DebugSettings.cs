using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

namespace Assets.Scripts.DOTS.Mediator.Systems.DebugScreenSystem
{
    public class DebugSettings : MonoBehaviour
    {
        public class DebugSettingsBaker : Baker<DebugSettings>
        {
            public override void Bake(DebugSettings authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);
                AddComponent<RollConfig>(entity);
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
