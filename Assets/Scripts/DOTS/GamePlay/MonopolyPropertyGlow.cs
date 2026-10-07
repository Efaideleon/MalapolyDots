using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace DOTS.GamePlay
{
    // Contours come from the Blender meshes in property-local coordinates.
    public sealed class MonopolyPropertyGlow : IDisposable
    {
        [Serializable] private class Catalog { public Footprint[] properties; }
        [Serializable] private class Footprint { public int id; public string mesh; public Level[] levels; }
        [Serializable] private class Level { public Loop[] loops; }
        [Serializable] private class Loop { public float elevation, height; public Vector2[] points; }

        private readonly Dictionary<int, Footprint> footprints = new Dictionary<int, Footprint>();
        private readonly Dictionary<(int, int), Mesh> meshes = new Dictionary<(int, int), Mesh>();
        private readonly HashSet<int> missing = new HashSet<int>();
        private bool initialized;
        private Material material;
        private readonly MaterialPropertyBlock properties = new MaterialPropertyBlock();
        private static readonly int GroupColor = Shader.PropertyToID("_GroupColor");
        private static readonly int EffectTime = Shader.PropertyToID("_EffectTime");

        public void Draw(int propertyId, int houseCount, Matrix4x4 localToWorld, Color color, float time, Camera camera)
        {
            var mesh = GetMesh(propertyId, houseCount);
            if (mesh == null || material == null) return;
            properties.SetColor(GroupColor, color);
            properties.SetFloat(EffectTime, time);
            Graphics.DrawMesh(mesh, localToWorld, material,
                0, camera, 0, properties, ShadowCastingMode.Off, false, null, LightProbeUsage.Off);
        }

        public Mesh GetMesh(int propertyId, int houseCount, bool warnIfMissing = true)
        {
            if (!initialized) Initialize();
            if (!footprints.TryGetValue(propertyId, out var footprint))
            {
                if (warnIfMissing && missing.Add(propertyId)) Debug.LogWarning($"Missing Blender glow contour for property {propertyId}.");
                return null;
            }
            // The current hotel presentation retains all four house meshes.
            int level = Mathf.Clamp(houseCount, 0, footprint.levels.Length - 1);
            var key = (propertyId, level);
            if (!meshes.TryGetValue(key, out var mesh))
            {
                mesh = BuildMesh(footprint.levels[level], footprint.mesh + " monopoly glow " + level);
                meshes.Add(key, mesh);
            }
            return mesh;
        }

        private void Initialize()
        {
            initialized = true;
            var shader = Resources.Load<Shader>("MonopolyPropertyGlow");
            var data = Resources.Load<TextAsset>("MonopolyFootprints");
            if (shader == null || data == null)
            {
                Debug.LogError("Monopoly glow shader or Blender footprint data is missing.");
                return;
            }
            foreach (var footprint in JsonUtility.FromJson<Catalog>(data.text).properties)
                footprints.Add(footprint.id, footprint);
            material = new Material(shader)
            {
                name = "Completed monopoly property glow (runtime)",
                hideFlags = HideFlags.HideAndDontSave
            };
        }

        private static Mesh BuildMesh(Level level, string name)
        {
            const float band = 0.32f;
            const float clearance = 0.10f;
            var vertices = new List<Vector3>();
            var uv = new List<Vector3>();
            var triangles = new List<int>();
            foreach (var loop in level.loops)
            {
                float distance = 0;
                for (int i = 0; i < loop.points.Length; i++)
                {
                    int next = (i + 1) % loop.points.Length;
                    float end = distance + Vector2.Distance(loop.points[i], loop.points[next]);
                    Vector3 a = Position(i), b = Position(next);
                    Vector3 na = Outward(i), nb = Outward(next);
                    AddQuad(a + na * (clearance - band), b + nb * (clearance - band),
                        b + nb * (clearance + band), a + na * (clearance + band), distance, end, false);
                    a += na * clearance;
                    b += nb * clearance;
                    AddQuad(a, b, b + Vector3.up * loop.height, a + Vector3.up * loop.height, distance, end, true);
                    distance = end;
                }

                Vector3 Position(int i) => new Vector3(loop.points[i].x, loop.elevation, loop.points[i].y);
                Vector3 Outward(int i)
                {
                    var previous = loop.points[(i + loop.points.Length - 1) % loop.points.Length];
                    var next = loop.points[(i + 1) % loop.points.Length];
                    var incoming = (loop.points[i] - previous).normalized;
                    var outgoing = (next - loop.points[i]).normalized;
                    var normal = new Vector2(outgoing.y, -outgoing.x);
                    var bisector = (normal + new Vector2(incoming.y, -incoming.x)).normalized;
                    // Bounded miters avoid long spikes at sharp, concave mesh details.
                    var offset = bisector / Mathf.Max(0.5f, Vector2.Dot(bisector, normal));
                    return new Vector3(offset.x, 0, offset.y);
                }
            }
            var mesh = new Mesh { name = name, hideFlags = HideFlags.HideAndDontSave };
            if (vertices.Count > 65535) mesh.indexFormat = IndexFormat.UInt32;
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uv);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            return mesh;

            void AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float startDistance, float endDistance, bool wall)
            {
                int start = vertices.Count;
                vertices.AddRange(new[] { a, b, c, d });
                float mode = wall ? 1 : 0;
                float lower = wall ? 0 : -1;
                uv.Add(new Vector3(startDistance, lower, mode));
                uv.Add(new Vector3(endDistance, lower, mode));
                uv.Add(new Vector3(endDistance, 1, mode));
                uv.Add(new Vector3(startDistance, 1, mode));
                triangles.AddRange(new[] { start, start + 1, start + 2, start, start + 2, start + 3 });
            }
        }

        public void Dispose()
        {
            Destroy(material);
            foreach (var mesh in meshes.Values) Destroy(mesh);
            meshes.Clear();
            footprints.Clear();
            missing.Clear();
            initialized = false;
            material = null;
        }

        private static void Destroy(UnityEngine.Object value)
        {
            if (value == null) return;
            if (Application.isPlaying) UnityEngine.Object.Destroy(value);
            else UnityEngine.Object.DestroyImmediate(value);
        }
    }
}
