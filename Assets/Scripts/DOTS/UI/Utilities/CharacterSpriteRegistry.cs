using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace Assets.Scripts.DOTS.Mediator
{
    public interface ISpriteRegistry
    {
        public Sprite Get(FixedString64Bytes id);
        public bool TryGet(FixedString64Bytes id, out Sprite sprite);
    }

    public sealed class CharacterSpriteRegistry : ISpriteRegistry
    {
        private readonly Dictionary<FixedString64Bytes, Sprite> _sprites;

        public CharacterSpriteRegistry(Sprite[] sprites)
        {
            _sprites = new();
            for (int i = 0; i < sprites.Length; i++)
            {
                _sprites.TryAdd(sprites[i].name, sprites[i]);
            }
        }

        public Sprite Get(FixedString64Bytes id)
        {
            return _sprites[id];
        }

        public bool TryGet(FixedString64Bytes id, out Sprite sprite)
        {
            return _sprites.TryGetValue(id, out sprite);
        }
    }
}
