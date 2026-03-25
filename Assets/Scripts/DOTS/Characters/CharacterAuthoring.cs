using DOTS.Characters.CharactersMaterialAuthoring;
using DOTS.DataComponents;
using DOTS.GameData;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DOTS.Characters
{
    public class CharacterAuthoring : MonoBehaviour
    {
        [SerializeField] public string charName;
        [SerializeField] public float moveSpeed;
        [SerializeField] public CharactersEnum charactersEnum;

        class CharactersBaker : Baker<CharacterAuthoring>
        {
            public override void Bake(CharacterAuthoring authoring)
            {
                // character entity
                var authoringEntity = GetEntity(authoring, TransformUsageFlags.None);

                AddComponent(authoringEntity, new NameComponent { Value = authoring.charName });
                AddComponent(authoringEntity, new MoneyComponent { Value = 500_000 });
                AddComponent(authoringEntity, new PrefabTag());
                AddComponent(authoringEntity, new PlayerWaypointIndex { Value = 0 });
                AddComponent(authoringEntity, new CharacterFlag { });
                AddComponent(authoringEntity, new PlayerMovementState { Value = MoveState.Idle });
                AddComponent(authoringEntity, new PlayerID { Value = 0 });
                AddComponent(authoringEntity, new CurrentPivotRotation { Value = quaternion.identity });
                AddComponent(authoringEntity, new FinalArrived { Value = false });
                AddComponent(authoringEntity, new RemainingMoves { Value = 0 });
                AddComponent(authoringEntity, new ReachedTargetPosition { Value = false });
                AddComponent(authoringEntity, new TargetPosition { Value = default });
                AddComponent(authoringEntity, new MoveSpeed { Value = authoring.moveSpeed });
                AddComponent(authoringEntity, new PlayerBoardIndex { Value = 0 });
                AddComponent(authoringEntity, new SpaceLandedOn { entity = Entity.Null });
                AddComponent(authoringEntity, new CharactersEnumComponent { Value = authoring.charactersEnum });

            }
        }
    }

    public enum CharactersEnum
    {
        Default,
        Avocado,
        Bird,
        Coin,
        Coffee,
        Tuctuc,
        Lira
    }

    public struct CharactersEnumComponent : IComponentData
    {
        public CharactersEnum Value;
    }

    public struct SpaceLandedOn : IComponentData
    {
        public Entity entity;
    }

    public struct PlayerBoardIndex : IComponentData
    {
        public int Value;
    }

    public struct MoveSpeed : IComponentData
    {
        public float Value;
    }

    public struct RemainingMoves : IComponentData
    {
        public int Value;
    }

    public struct PlayerID : IComponentData
    {
        public int Value;
    }

    public struct FinalArrived : IComponentData
    {
        public bool Value;
    }

    public struct ReachedTargetPosition : IComponentData
    {
        public bool Value;
    }

    public struct TargetPosition : IComponentData
    {
        public float3 Value;
    }

    public struct PlayerTurnComponent : IComponentData
    {
        public bool IsActive;
    }

    public struct PlayerWaypointIndex : IComponentData
    {
        public int Value;
    }

    public struct PlayerMovementState : IComponentData
    {
        public MoveState Value;
    }

    public struct CharacterFlag : IComponentData
    { }

    public enum MoveState
    {
        Walking,
        Idle
    }

    public struct CurrentPivotRotation : IComponentData
    {
        public quaternion Value;
    }
}
