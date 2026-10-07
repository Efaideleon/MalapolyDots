using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

namespace DOTS.GameSpaces
{
    [DisallowMultipleComponent]
    public sealed class PropertyBaseGlowAuthoring : MonoBehaviour
    {
        private sealed class Baker : Baker<PropertyBaseGlowAuthoring>
        {
            public override void Bake(PropertyBaseGlowAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Renderable);
                AddComponent(entity, new PropertyBaseGlowState());
                AddComponent(entity, new PropertyBaseGlowAnimation());
            }
        }
    }

    [MaterialProperty("_BaseGlowState")]
    public struct PropertyBaseGlowState : IComponentData
    {
        public float4 Value;
    }

    // Presentation state stays local; the source monopoly flag and house count are replicated.
    public struct PropertyBaseGlowAnimation : IComponentData
    {
        public const float PurchaseDuration = 2.5f;
        public int LastHouseCount;
        public bool Initialized;
        public float PurchaseRemaining;

        public float Advance(int houseCount, float deltaTime)
        {
            if (!Initialized)
            {
                Initialized = true;
                LastHouseCount = houseCount;
                return 0f;
            }
            if (houseCount > LastHouseCount) PurchaseRemaining = PurchaseDuration;
            else if (houseCount < LastHouseCount) PurchaseRemaining = 0f;
            else PurchaseRemaining = math.max(0f, PurchaseRemaining - math.max(0f, deltaTime));
            LastHouseCount = houseCount;
            float remaining = PurchaseRemaining / PurchaseDuration;
            // A quick bright response followed by a smooth fade.
            return remaining * remaining;
        }
    }
}
