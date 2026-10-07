using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// The coin VAT was exported in Blender coordinates. Keep its base mesh in that
// same space so OpenVAT can add the exported vertex offsets without distortion.
public sealed class CoinVatModelImport : AssetPostprocessor
{
    const string CoinModel = "Assets/FBX/characters/coin.fbx";

    public override uint GetVersion() => 2;

    void OnPostprocessModel(GameObject model)
    {
        if (assetPath != CoinModel) return;
        var processed = new HashSet<Mesh>();
        foreach (var filter in model.GetComponentsInChildren<MeshFilter>(true))
        {
            var mesh = filter.sharedMesh;
            if (mesh == null || !processed.Add(mesh)) continue;

            var vertices = mesh.vertices;
            for (int i = 0; i < vertices.Length; i++) vertices[i] = VatSpace(vertices[i]);
            mesh.vertices = vertices;

            var normals = mesh.normals;
            for (int i = 0; i < normals.Length; i++) normals[i] = VatSpace(normals[i]);
            mesh.normals = normals;

            var tangents = mesh.tangents;
            for (int i = 0; i < tangents.Length; i++)
            {
                var direction = VatSpace(tangents[i]);
                tangents[i] = new Vector4(direction.x, direction.y, direction.z, -tangents[i].w);
            }
            mesh.tangents = tangents;

            // Undoing the FBX handedness conversion also reverses winding.
            for (int submesh = 0; submesh < mesh.subMeshCount; submesh++)
            {
                var triangles = mesh.GetTriangles(submesh);
                for (int i = 0; i < triangles.Length; i += 3)
                    (triangles[i], triangles[i + 1]) = (triangles[i + 1], triangles[i]);
                mesh.SetTriangles(triangles, submesh, false);
            }

            // OpenVAT's decoder reads UV2; the FBX stores the atlas lookup in UV1.
            var atlas = new List<Vector2>();
            mesh.GetUVs(1, atlas);
            mesh.SetUVs(2, atlas);

            mesh.RecalculateBounds();
            var bounds = mesh.bounds;
            // GPU animation can move vertices outside the unanimated mesh bounds.
            bounds.Encapsulate(mesh.bounds.min + new Vector3(-1.5f, -0.8f, -1.4f));
            bounds.Encapsulate(mesh.bounds.max + new Vector3(1.5f, 0.8f, 1.4f));
            mesh.bounds = bounds;
        }
    }

    static Vector3 VatSpace(Vector3 value) => new Vector3(-value.x, value.y, value.z);
}
