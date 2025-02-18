using UnityEngine;
using Unity.Entities;

public class CoinAuthoring : MonoBehaviour
{
    [SerializeField] public string Name;

    public class CoinBaker : Baker<CoinAuthoring>
    {
        public override void Bake(CoinAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new NameDataComponent { Name = authoring.Name});
        }
    }
}
