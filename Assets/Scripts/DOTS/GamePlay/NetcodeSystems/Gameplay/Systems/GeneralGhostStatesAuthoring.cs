using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

namespace Assets.Scripts.DOTS.GamePlay.NetcodeSystems.Gameplay.Systems
{
    public class GeneralGhostStatesAuthoring : MonoBehaviour
    {
        public class GeneralGhostStatesAuthoringBaker : Baker<GeneralGhostStatesAuthoring>
        {
            public override void Bake(GeneralGhostStatesAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);
                AddComponent(entity, new GeneralGhostStates { AllGhostCharactersSpawned = false });
                AddBuffer<PlayersSortedByNetId>(entity);
            }
        }
    }

    [GhostComponent]
    public struct GeneralGhostStates : IComponentData
    {
        [GhostField]
        public bool AllGhostCharactersSpawned;

        [GhostField]
        public int TotalNumberOfCharSpawned;

        [GhostField]
        public bool PlayerNamesSorted;
    }

    [GhostComponent]
    public struct PlayersSortedByNetId : IBufferElementData
    {
        [GhostField]
        public FixedString64Bytes Name;
    }
}
