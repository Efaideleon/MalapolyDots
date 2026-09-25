using System.Collections.Generic;
using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.GamePlay;
using DOTS.GameSpaces;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Graphics;
using Unity.NetCode;
using UnityEngine;

namespace DOTS.GamePlay
{
    public struct LastPropertyOutlined : IComponentData
    {
        public Entity Entity;
    }

    // Every client follows the same replicated turn, regardless of local clicks or ownership.
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    [UpdateInGroup(typeof(PresentationSystemGroup), OrderFirst = true)]
    public partial class OutlineSelectionSystem : SystemBase
    {
        readonly Dictionary<Entity, int> originalLayers = new();
        readonly List<Entity> restored = new();
        EntityQuery renderers;
        int outlineLayer;

        protected override void OnCreate()
        {
            EntityManager.CreateSingleton<LastPropertyOutlined>();
            outlineLayer = LayerMask.NameToLayer("Outline");
            renderers = GetEntityQuery(new EntityQueryDesc
            {
                All = new[] { ComponentType.ReadOnly<RenderFilterSettings>() },
                Any = new[] { ComponentType.ReadOnly<PropertySpaceTag>(), ComponentType.ReadOnly<PropertyLodSource>() }
            });
        }

        protected override void OnUpdate()
        {
            Entity property = Entity.Null;
            if (outlineLayer >= 0 &&
                !SystemAPI.QueryBuilder().WithAll<NetworkStreamInGame>().Build().IsEmptyIgnoreFilter &&
                SystemAPI.TryGetSingleton<CurrentActivePlayer>(out var active) &&
                SystemAPI.TryGetSingleton<GameStateComponent>(out var game) &&
                game.State != GameState.Walking && game.State != GameState.GameOver &&
                EntityManager.HasComponent<SpaceLandedOn>(active.Entity))
            {
                var landed = EntityManager.GetComponentData<SpaceLandedOn>(active.Entity).entity;
                if (EntityManager.HasComponent<PropertySpaceTag>(landed)) property = landed;
            }

            Dependency.Complete();
            restored.Clear();
            foreach (var pair in originalLayers)
            {
                if (property != Entity.Null && PropertyFor(pair.Key) == property) continue;
                if (EntityManager.HasComponent<RenderFilterSettings>(pair.Key)) SetLayer(pair.Key, pair.Value);
                restored.Add(pair.Key);
            }
            foreach (var entity in restored) originalLayers.Remove(entity);

            if (property != Entity.Null)
            {
                // Include all LODs, even when currently culled, and pick up newly streamed renderers.
                using var entities = renderers.ToEntityArray(Allocator.Temp);
                foreach (var entity in entities)
                {
                    if (PropertyFor(entity) != property) continue;
                    if (!originalLayers.ContainsKey(entity))
                        originalLayers.Add(entity, EntityManager.GetSharedComponentManaged<RenderFilterSettings>(entity).Layer);
                    SetLayer(entity, outlineLayer);
                }
            }
            SystemAPI.SetSingleton(new LastPropertyOutlined { Entity = property });
        }

        Entity PropertyFor(Entity entity)
        {
            if (!EntityManager.Exists(entity)) return Entity.Null;
            if (EntityManager.HasComponent<PropertyLodSource>(entity))
                return EntityManager.GetComponentData<PropertyLodSource>(entity).Value;
            return EntityManager.HasComponent<PropertySpaceTag>(entity) ? entity : Entity.Null;
        }

        void SetLayer(Entity entity, int layer)
        {
            var settings = EntityManager.GetSharedComponentManaged<RenderFilterSettings>(entity);
            if (settings.Layer == layer) return;
            settings.Layer = layer;
            EntityManager.SetSharedComponentManaged(entity, settings);
        }
    }
}
