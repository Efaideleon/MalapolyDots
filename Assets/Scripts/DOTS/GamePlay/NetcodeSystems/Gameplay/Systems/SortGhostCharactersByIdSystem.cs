using System;
using Assets.Scripts.DOTS.Characters;
using DOTS.DataComponents;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

namespace Assets.Scripts.DOTS.GamePlay.NetcodeSystems.Gameplay.Systems
{ 
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct SortGhostCharactersByIdSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GeneralGhostStates>();
            state.RequireForUpdate<PlayersSortedByNetId>();
            state.RequireForUpdate<NetworkStreamInGame>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var generalState = SystemAPI.GetSingleton<GeneralGhostStates>();
            if (!generalState.AllGhostCharactersSpawned || generalState.PlayerNamesSorted)
                return;

            NativeList<PlayerByName> players = new(Allocator.Temp);
            foreach (var (ghostOwner, _, entity) in SystemAPI.Query<RefRO<GhostOwner>, RefRO<CharacterFlag>>().WithEntityAccess())
            {
                players.Add(new PlayerByName { Entity = entity, id = ghostOwner.ValueRO.NetworkId});
            }
            players.Sort();

            var buffer = SystemAPI.GetSingletonBuffer<PlayersSortedByNetId>();

            foreach (var player in players)
            {
                if (SystemAPI.HasComponent<NameComponent>(player.Entity))
                {
                    var name = SystemAPI.GetComponent<NameComponent>(player.Entity).Value;
                    UnityEngine.Debug.Log($"[SortGhostCharactersByIdSystem] | player: {name} id: {player.id}");
                    buffer.Add(new PlayersSortedByNetId { Name = name});
                }
            }

            SystemAPI.GetSingletonRW<GeneralGhostStates>().ValueRW.PlayerNamesSorted = true;

            players.Dispose();
            state.Enabled = false;
        }
    }

    public struct PlayerByName : IComponentData, IComparable<PlayerByName>
    {
        public Entity Entity;
        public int id;

        public int CompareTo(PlayerByName other)
        {
            return id.CompareTo(other.id);
        }
    }
}
