using Unity.Entities;
using UnityEngine;

public class TaxSpaceAuthoring : MonoBehaviour
{
    [SerializeField] int ID;
    [SerializeField] string Name;
    [SerializeField] int BoardIndex;

    class TaxSpaceAuthoringBaker : Baker<TaxSpaceAuthoring>
    {
        public override void Bake(TaxSpaceAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new NameComponent { Value = authoring.Name });
            AddComponent(entity, new SpaceIDComponent { Value = authoring.ID });
            AddComponent(entity, new BoardIndexComponent { Value = authoring.BoardIndex });
        }
    }
}
