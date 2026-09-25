using System.IO;
using UnityEditor;
using UnityEngine;

// VAT pixels contain vertex positions, not display colors. Never filter or compress them.
public sealed class CharacterVatImportSettings : AssetPostprocessor
{
    const string Folder = "Assets/OpenVAT/Characters";
    static bool IsVat(string path) => path.StartsWith(Folder + "/", System.StringComparison.Ordinal) && path.EndsWith("_vat.exr", System.StringComparison.OrdinalIgnoreCase);
    void OnPreprocessTexture()
    {
        if (IsVat(assetPath)) Configure((TextureImporter)assetImporter);
    }
    static bool Configure(TextureImporter importer)
    {
        bool changed = importer.sRGBTexture || importer.mipmapEnabled || importer.filterMode != FilterMode.Point ||
            importer.wrapMode != TextureWrapMode.Clamp || importer.npotScale != TextureImporterNPOTScale.None ||
            importer.textureCompression != TextureImporterCompression.Uncompressed;
        importer.sRGBTexture = false;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Point;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        // Preserve floating-point coordinates on desktop and mobile, including existing overrides.
        foreach (string platform in new[] { "DefaultTexturePlatform", "Standalone", "iPhone", "Android", "WebGL" })
        {
            var settings = importer.GetPlatformTextureSettings(platform);
            bool isDefault = platform == "DefaultTexturePlatform";
            changed |= settings.format != TextureImporterFormat.RGBAHalf || settings.textureCompression != TextureImporterCompression.Uncompressed ||
                (!isDefault && !settings.overridden) || settings.maxTextureSize < 2048;
            settings.format = TextureImporterFormat.RGBAHalf;
            settings.textureCompression = TextureImporterCompression.Uncompressed;
            settings.crunchedCompression = false;
            settings.maxTextureSize = System.Math.Max(2048, settings.maxTextureSize);
            if (!isDefault) settings.overridden = true;
            importer.SetPlatformTextureSettings(settings);
        }
        return changed;
    }
    [InitializeOnLoadMethod]
    static void ScheduleRepair() => EditorApplication.delayCall += RepairExisting;
    static void RepairExisting()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorApplication.delayCall += RepairExisting;
            return;
        }
        if (!Directory.Exists(Folder)) return;
        int changed = 0;
        foreach (string path in Directory.GetFiles(Folder, "*_vat.exr", SearchOption.AllDirectories))
        {
            var importer = AssetImporter.GetAtPath(path.Replace('\\', '/')) as TextureImporter;
            if (importer != null && Configure(importer)) { importer.SaveAndReimport(); changed++; }
        }
        if (changed > 0) Debug.Log($"Character VAT: repaired {changed} animation textures (linear, point filtered, no mipmaps, uncompressed half float).");
    }
}
