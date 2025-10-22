using Unity.Entities;
using UnityEngine;

public struct ABLESTUFF
{
    public string PetName;
}

#nullable enable
namespace DOTS.Characters
{
    public class CharactersAuthoring : MonoBehaviour
    {
        private ABLESTUFF a = new ABLESTUFF { PetName = null };

        public void Func()
        {
            var newPetName = a.PetName?.Trim('a');
        }

        [SerializeField] public GameObject avocadoPrefab;
        [SerializeField] public GameObject birdPrefab;
        [SerializeField] public GameObject coinPrefab;
        [SerializeField] public GameObject coffeePrefab;
        [SerializeField] public GameObject liraPrefab;
        [SerializeField] public GameObject tuctucPrefab;

        class CharactersBaker : Baker<CharactersAuthoring>
        {
            public override void Bake(CharactersAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);

                var avocadoEntity = GetEntity(authoring.avocadoPrefab, TransformUsageFlags.Dynamic);
                var birdEntity = GetEntity(authoring.birdPrefab, TransformUsageFlags.Dynamic);
                var coinEntity = GetEntity(authoring.coinPrefab, TransformUsageFlags.Dynamic);
                var coffeeEntity = GetEntity(authoring.coffeePrefab, TransformUsageFlags.Dynamic);
                var liraEntity = GetEntity(authoring.liraPrefab, TransformUsageFlags.Dynamic);
                var tuctucEntity = GetEntity(authoring.tuctucPrefab, TransformUsageFlags.Dynamic);

                var buffer = AddBuffer<CharacterEntityBuffer>(entity);
                buffer.Add(new CharacterEntityBuffer { Prefab = avocadoEntity});
                buffer.Add(new CharacterEntityBuffer { Prefab = birdEntity});
                buffer.Add(new CharacterEntityBuffer { Prefab = coinEntity});
                buffer.Add(new CharacterEntityBuffer { Prefab = coffeeEntity});
                buffer.Add(new CharacterEntityBuffer { Prefab = liraEntity});
                buffer.Add(new CharacterEntityBuffer { Prefab = tuctucEntity});

                AddComponent(entity, new CharactersBufferTag{});
            }
        }
    }

    public struct CharacterEntityBuffer : IBufferElementData
    {
        public Entity Prefab;
    }

    public struct CharactersBufferTag : IComponentData
    {}
}
