using UnityEngine;
using Unity.Entities;

public class LiraAuthoring : MonoBehaviour
{
    [SerializeField] public string Name;

    public class LiraBaker : Baker<LiraAuthoring>
    {
        public override void Bake(LiraAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new NameDataComponent { Name = authoring.Name});
        }
    }
}
