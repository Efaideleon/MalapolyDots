using System.Collections.Generic;
using Assets.Scripts.DOTS.GamePlay;
using DOTS.DataComponents;
using DOTS.GameSpaces;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;

namespace DOTS.GamePlay
{
    // Presentation follows the server's replicated monopoly flag, including sales and trades.
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class MonopolyCrownSystem : SystemBase
    {
        private Mesh crownMesh;
        private Material material;
        private MaterialPropertyBlock properties;
        private readonly MonopolyPropertyGlow glow = new MonopolyPropertyGlow();
        private static readonly int GroupTint = Shader.PropertyToID("_GroupColor");

        protected override void OnCreate()
        {
            RequireForUpdate<NetworkStreamInGame>();
            RequireForUpdate<PropertySpaceTag>();
        }

        protected override void OnUpdate()
        {
            if (!SystemAPI.TryGetSingleton<GameStateComponent>(out var game) ||
                !game.AllPlacesInstantiated || game.State == GameState.GameOver)
                return;

            var camera = Camera.main;
            if (camera == null) return;

            foreach (var (monopoly, color, bounds, transform, id, houses) in
                     SystemAPI.Query<RefRO<MonopolyFlagComponent>, RefRO<ColorCodeComponent>,
                         RefRO<RenderBounds>, RefRO<LocalToWorld>, RefRO<SpaceIDComponent>,
                         RefRO<HouseCount>>().WithAll<PropertySpaceTag>())
            {
                if (!monopoly.ValueRO.Value || color.ValueRO.Value < PropertyColor.Brown ||
                    color.ValueRO.Value > PropertyColor.Blue)
                    continue;
                if (material == null && !Initialize()) return;

                var matrix = transform.ValueRO.Value;
                var localBounds = bounds.ValueRO.Value;
                float3 center = math.transform(matrix, localBounds.Center);
                float3 extents = math.abs(matrix.c0.xyz) * localBounds.Extents.x +
                                 math.abs(matrix.c1.xyz) * localBounds.Extents.y +
                                 math.abs(matrix.c2.xyz) * localBounds.Extents.z;
                var groupColor = GroupColor(color.ValueRO.Value);
                glow.Draw(id.ValueRO.Value, houses.ValueRO.Value, (Matrix4x4)matrix,
                    groupColor, UnityEngine.Time.unscaledTime, camera);
                var roof = new Vector3(center.x, center.y + extents.y, center.z);
                float depth = Vector3.Dot(roof - camera.transform.position, camera.transform.forward);
                if (depth <= camera.nearClipPlane) continue;
                // Keep the crown legible while zooming, without depending on the roof shape.
                float viewHeight = camera.orthographic ? camera.orthographicSize * 2f :
                    2f * depth * Mathf.Tan(camera.fieldOfView * Mathf.Deg2Rad * 0.5f);
                float size = viewHeight * 42f / Mathf.Max(1, camera.pixelHeight);
                float bob = Mathf.Sin(UnityEngine.Time.unscaledTime * 2f) * size * 0.07f;
                var position = roof + Vector3.up * (size * 0.75f + 0.35f + bob);
                properties.SetColor(GroupTint, groupColor);
                Graphics.DrawMesh(crownMesh, Matrix4x4.TRS(position, camera.transform.rotation,
                    Vector3.one * size), material, 0, camera, 0, properties,
                    ShadowCastingMode.Off, false, null, LightProbeUsage.Off);
            }
        }

        private bool Initialize()
        {
            var shader = Resources.Load<Shader>("MonopolyCrown");
            if (shader == null) return false;
            material = new Material(shader) { name = "Completed property group crown (runtime)" };
            properties = new MaterialPropertyBlock();
            var vertices = new List<Vector3>();
            var colors = new List<Color>();
            var uv = new List<Vector2>();
            var triangles = new List<int>();
            var silhouette = new[]
            {
                new Vector2(-0.5f, 0.3f), new Vector2(-0.22f, 0.07f),
                new Vector2(0, 0.48f), new Vector2(0.22f, 0.07f),
                new Vector2(0.5f, 0.3f), new Vector2(0.38f, -0.30f),
                new Vector2(-0.38f, -0.30f)
            };
            AddShape(1.10f, new Color(0.07f, 0.035f, 0.01f), false);
            AddShape(1f, new Color(1.8f, 1.25f, 0.22f), false);
            AddShape(0.77f, Color.white, true);
            crownMesh = new Mesh { name = "Completed set crown badge" };
            crownMesh.SetVertices(vertices);
            crownMesh.SetColors(colors);
            crownMesh.SetUVs(0, uv);
            crownMesh.SetTriangles(triangles, 0);
            crownMesh.RecalculateBounds();
            return true;

            void AddShape(float scale, Color tint, bool groupColor)
            {
                int start = vertices.Count;
                vertices.Add(new Vector3(0, -0.12f, 0));
                colors.Add(tint);
                uv.Add(new Vector2(groupColor ? 1 : 0, 0));
                foreach (var point in silhouette)
                {
                    vertices.Add(new Vector3(point.x * scale, (point.y + 0.12f) * scale - 0.12f, 0));
                    colors.Add(tint);
                    uv.Add(new Vector2(groupColor ? 1 : 0, 0));
                }
                for (int i = 0; i < silhouette.Length; i++)
                {
                    triangles.Add(start);
                    triangles.Add(start + 1 + i);
                    triangles.Add(start + 1 + (i + 1) % silhouette.Length);
                }
            }
        }

        public static Color GroupColor(PropertyColor color)
        {
            switch (color)
            {
                case PropertyColor.Brown: return new Color(0.68f, 0.30f, 0.10f);
                case PropertyColor.LightBlue: return new Color(0.15f, 0.78f, 1f);
                case PropertyColor.Purple: return new Color(0.77f, 0.16f, 1f);
                case PropertyColor.Orange: return new Color(1f, 0.38f, 0.035f);
                case PropertyColor.Red: return new Color(1f, 0.045f, 0.08f);
                case PropertyColor.Yellow: return new Color(1f, 0.85f, 0.035f);
                case PropertyColor.Green: return new Color(0.06f, 0.90f, 0.24f);
                case PropertyColor.Blue: return new Color(0.075f, 0.22f, 1f);
                default: return Color.clear;
            }
        }

        protected override void OnDestroy()
        {
            glow.Dispose();
            if (material != null) Object.Destroy(material);
            if (crownMesh != null) Object.Destroy(crownMesh);
        }
    }
}
