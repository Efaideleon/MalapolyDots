using System;
using System.Collections.Generic;
using DOTS.DataComponents;
using DOTS.GameSpaces;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;

namespace DOTS.GamePlay
{
    public sealed class PropertyTapGlow : IDisposable
    {
        readonly MonopolyPropertyGlow contours = new();
        readonly Dictionary<int, Mesh> fallbackMeshes = new();
        readonly MaterialPropertyBlock properties = new();
        Material material;
        static readonly int EffectTime = Shader.PropertyToID("_EffectTime");
        static readonly int Hovered = Shader.PropertyToID("_Hovered");

        public Mesh GetMesh(EntityManager manager, Entity property, out Matrix4x4 matrix, out Vector3 anchor)
        {
            matrix = Matrix4x4.identity;
            anchor = Vector3.zero;
            if (!manager.HasComponent<LocalToWorld>(property) || !manager.HasComponent<RenderBounds>(property) ||
                !manager.HasComponent<SpaceIDComponent>(property)) return null;
            var transform = manager.GetComponentData<LocalToWorld>(property).Value;
            matrix = (Matrix4x4)transform;
            var renderBounds = manager.GetComponentData<RenderBounds>(property).Value;
            var bounds = new Bounds((Vector3)renderBounds.Center, (Vector3)renderBounds.Extents * 2);
            int id = manager.GetComponentData<SpaceIDComponent>(property).Value;
            int houses = manager.HasComponent<HouseCount>(property) ? manager.GetComponentData<HouseCount>(property).Value : 0;
            var mesh = GetMesh(id, houses, bounds);
            var roof = fallbackMeshes.ContainsKey(id) ? bounds.center : mesh.bounds.center;
            roof.y = (fallbackMeshes.ContainsKey(id) ? bounds.max.y : mesh.bounds.max.y) + 0.5f;
            anchor = math.transform(transform, (float3)roof);
            return mesh;
        }

        public Mesh GetMesh(int id, int houses, Bounds bounds)
        {
            var mesh = contours.GetMesh(id, houses, false);
            if (mesh != null) return mesh;
            if (fallbackMeshes.TryGetValue(id, out mesh)) return mesh;
            var vertices = new List<Vector3>();
            var uv = new List<Vector3>();
            var triangles = new List<int>();
            float y = bounds.min.y + 0.12f;
            var corners = new[] {
                new Vector3(bounds.min.x, y, bounds.min.z), new Vector3(bounds.max.x, y, bounds.min.z),
                new Vector3(bounds.max.x, y, bounds.max.z), new Vector3(bounds.min.x, y, bounds.max.z)
            };
            for (int i = 0; i < 4; i++)
            {
                var a = corners[i]; var b = corners[(i + 1) % 4];
                var normal = Vector3.Cross((b - a).normalized, Vector3.up) * 0.2f;
                int start = vertices.Count;
                vertices.AddRange(new[] { a - normal, b - normal, b + normal, a + normal });
                uv.AddRange(new[] { new Vector3(0, -1, 0), new Vector3(1, -1, 0), new Vector3(1, 1, 0), new Vector3(0, 1, 0) });
                triangles.AddRange(new[] { start, start + 1, start + 2, start, start + 2, start + 3 });
            }
            mesh = new Mesh { name = "Property tap base " + id, hideFlags = HideFlags.HideAndDontSave };
            mesh.SetVertices(vertices); mesh.SetUVs(0, uv); mesh.SetTriangles(triangles, 0); mesh.RecalculateBounds();
            fallbackMeshes.Add(id, mesh);
            return mesh;
        }

        public void Draw(Mesh mesh, Matrix4x4 matrix, bool hovered, Camera camera)
        {
            if (material == null)
            {
                var shader = Resources.Load<Shader>("PropertyTapGlow");
                if (shader == null) return;
                material = new Material(shader) { name = "Property tap pulse (runtime)", hideFlags = HideFlags.HideAndDontSave };
            }
            properties.SetFloat(EffectTime, Time.unscaledTime);
            properties.SetFloat(Hovered, hovered ? 1 : 0);
            Graphics.DrawMesh(mesh, matrix, material, 0, camera, 0, properties,
                ShadowCastingMode.Off, false, null, LightProbeUsage.Off);
        }

        public void Dispose()
        {
            contours.Dispose();
            if (material != null) UnityEngine.Object.Destroy(material);
            foreach (var mesh in fallbackMeshes.Values) UnityEngine.Object.Destroy(mesh);
            fallbackMeshes.Clear();
        }
    }
}
