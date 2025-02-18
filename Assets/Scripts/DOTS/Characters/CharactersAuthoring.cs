using Unity.Entities;
using UnityEngine;

public class CharactersAuthoring : MonoBehaviour
{
    [Header("Character Prefabs")]
    [SerializeField] public GameObject avocadoPrefab;
    [SerializeField] public GameObject birdPrefab;
    [SerializeField] public GameObject liraPrefab;
    [SerializeField] public GameObject coinPrefab;
    [SerializeField] public GameObject mugPrefab;
    [SerializeField] public GameObject tuctucPrefab;

    class CharactersBaker : Baker<CharactersAuthoring>
    {
        public override void Bake(CharactersAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.None);
            AddComponent(entity, new CharactersComponent
            {
                avocado = GetEntity(authoring.avocadoPrefab, TransformUsageFlags.Dynamic),
                bird = GetEntity(authoring.birdPrefab, TransformUsageFlags.Dynamic),
                lira = GetEntity(authoring.liraPrefab, TransformUsageFlags.Dynamic),
                coin = GetEntity(authoring.coinPrefab, TransformUsageFlags.Dynamic),
                mug = GetEntity(authoring.mugPrefab, TransformUsageFlags.Dynamic),
                tuctuc = GetEntity(authoring.tuctucPrefab, TransformUsageFlags.Dynamic),
            });
        }
    }
}

public struct CharactersComponent : IComponentData
{
    public Entity avocado;
    public Entity bird;
    public Entity lira;
    public Entity coin;
    public Entity mug;
    public Entity tuctuc;
}


