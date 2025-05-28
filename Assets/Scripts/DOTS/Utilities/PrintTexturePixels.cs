using UnityEngine;

namespace DOTS.Utilities
{
    public class PrintTexturePixels : MonoBehaviour
    {
        [Tooltip("Material that has the texture you want to dump")]
        public Material sourceMaterial;

        [Tooltip("Name of the texture property on the material (e.g. _MainTex)")]
        public string textureProperty = "_MyTex";

        void Start()
        {
            if (sourceMaterial == null)
            {
                Debug.LogError("TexturePixelDumper: No sourceMaterial assigned!");
                return;
            }

            var tex = sourceMaterial.GetTexture(textureProperty) as Texture2D;
            if (tex == null)
            {
                Debug.LogError($"TexturePixelDumper: Could not get a Texture2D from '{textureProperty}'");
                return;
            }

            // Make sure the texture is readable in import settings
            if (!tex.isReadable)
            {
                Debug.LogError("TexturePixelDumper: Texture is not marked as readable in importer settings.");
                return;
            }

            Color[] pixels = tex.GetPixels();
            int w = tex.width;
            int h = tex.height;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    Color c = pixels[y * w + x];
                    Debug.Log($"Pixel[{x},{y}] = R{c.r:F3}, G{c.g:F3}, B{c.b:F3}");
                }
            }
        }
    }
}
