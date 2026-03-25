using Assets.Scripts.DOTS.DataComponents;
using DOTS.Characters.CharactersMaterialAuthoring;
using DOTS.DataComponents;
using DOTS.GameData;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

namespace Assets.Scripts.DOTS.Characters
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

                // There is a ghost onwer comp attached when its spawned.
                AddComponent(authoringEntity, new NameComponent { Value = authoring.charName });
                AddComponent(authoringEntity, new MoneyComponent { Value = 500_000 });
                AddComponent(authoringEntity, new GhostMoneyComponet { Value = 500_000 });
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
                AddComponent<TouchRayCastDataInput>(authoringEntity);
                AddComponent<TouchPositionInput>(authoringEntity);
                AddComponent<TouchStartedInput>(authoringEntity);
                AddComponent<TouchCanceledInput>(authoringEntity);
                AddComponent<ClickedPropertyComponent>(authoringEntity);
                AddComponent<HitCastResult>(authoringEntity);
                AddBuffer<BackDropEventBus>(authoringEntity);
                AddComponent<UITappedPropertyEvent>(authoringEntity);
                AddComponent<ActivePlayer>(authoringEntity);
                AddComponent<GhostChanceCardPicked>(authoringEntity);
            }
        }
    }

    [GhostComponent]
    [GhostEnabledBit]
    public struct ActivePlayer : IComponentData, IEnableableComponent { }

    [GhostComponent]
    public struct GhostChanceCardPicked : IComponentData
    {
        [GhostField]
        public int id;

        [GhostField]
        public FixedString64Bytes msg;
    }

    [GhostComponent]
    public struct GhostMoneyComponet : IComponentData
    {
        [GhostField]
        public int Value;
    }

    public struct CharactersEnumComponent : IComponentData
    {
        public CharactersEnum Value;
    }

    [GhostComponent]
    public struct SpaceLandedOn : IComponentData
    {
        [GhostField]
        public Entity entity;
    }

    [GhostComponent]
    public struct PlayerBoardIndex : IComponentData
    {
        [GhostField]
        public int Value;
    }

    public struct MoveSpeed : IComponentData
    {
        public float Value;
    }

    [GhostComponent]
    public struct RemainingMoves : IComponentData
    {
        [GhostField]
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

    // TODO: Might need to combine some of this.
    public struct TouchRayCastDataInput : IInputComponentData
    {
        public float3 RayOrigin;
        public float3 RayDirection;
        public float3 RayEnd;
    }

    public struct TouchPositionInput : IInputComponentData
    {
        public InputEvent IsHeld;
        public float2 Position;
    }

    public struct TouchStartedInput : IInputComponentData
    {
        public InputEvent IsTapped;
    }

    public struct TouchCanceledInput : IInputComponentData
    {
        public InputEvent IsTapped;
    }

    public struct HitData
    {
        public Entity Entity;
        public float3 Position;
    }

    // Should this be a ghost component?
    [GhostComponent]
    public struct HitCastResult : IComponentData
    {
        [GhostField]
        public HitData FloorHit;

        [GhostField]
        public HitData ObjectHit;
    }

    [GhostComponent]
    public struct ClickedPropertyComponent : IComponentData
    {
        [GhostField]
        public Entity entity;
    }

    public struct BackDropEventBus : IBufferElementData
    { }

    [GhostComponent]
    public struct UITappedPropertyEvent : IComponentData
    {
        [GhostField]
        public Entity entity;

        [GhostField]
        public uint EventTick;
    }
}
