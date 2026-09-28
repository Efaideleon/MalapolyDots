using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace DOTS.GamePlay.CameraSystems
{
    // Created only on clients,
    // after the server confirms a property purchase.
    public sealed class PurchaseConfetti : MonoBehaviour
    {
        private readonly List<ParticleSystem> systems = new();
        private readonly List<Material> materials = new();
        private Mesh paper;

        // Different colors available to each individual
        // piece of confetti.
        private static readonly Color[] Colors =
        {
            new Color(1.00f, 0.12f, 0.20f), // Ruby Red
            new Color(1.00f, 0.45f, 0.05f), // Tangerine Orange
            new Color(1.00f, 0.85f, 0.05f), // Gold / Yellow
            new Color(0.15f, 0.95f, 0.35f), // Emerald / Lime
            new Color(0.05f, 0.85f, 1.00f), // Sky Blue / Cyan
            new Color(0.25f, 0.35f, 1.00f), // Royal Blue
            new Color(0.65f, 0.15f, 0.95f), // Violet / Purple
            new Color(1.00f, 0.15f, 0.75f), // Hot Pink
            new Color(0.10f, 1.00f, 0.75f), // Mint / Turquoise
            new Color(1.00f, 0.95f, 0.85f)  // Champagne White
        };

        public bool Play()
        {
            Camera camera = Camera.main;

            if (camera == null)
                return false;

            if (systems.Count == 0 && !Initialize())
                return false;

            foreach (var particles in systems)
            {
                particles.transform.SetParent(camera.transform, false);
                particles.transform.localPosition = Vector3.zero;
                particles.transform.localRotation = Quaternion.identity;
                particles.transform.localScale = Vector3.one;
                particles.Play();
            }

            // Put the confetti in front of the camera.
            float depth =
                camera.nearClipPlane + 1f;

            Vector3 bottomLeft =
                camera.transform.InverseTransformPoint(
                    camera.ViewportToWorldPoint(
                        new Vector3(
                            0f,
                            0f,
                            depth
                        )
                    )
                );

            Vector3 topRight =
                camera.transform.InverseTransformPoint(
                    camera.ViewportToWorldPoint(
                        new Vector3(
                            1f,
                            1f,
                            depth
                        )
                    )
                );

            float height =
                topRight.y - bottomLeft.y;

            // Emit 100 individual pieces of confetti.
            for (int i = 0; i < 100; i++)
            {
                float y =
                    topRight.y +
                    height *
                    Random.Range(
                        0.04f,
                        0.55f
                    );

                float speed =
                    height *
                    Random.Range(
                        0.32f,
                        0.48f
                    );

                // Equal counts guarantee every palette color appears in each shower.
                var particles = systems[i % systems.Count];

                ParticleSystem.EmitParams emitParams =
                    new ParticleSystem.EmitParams
                    {
                        position =
                            new Vector3(
                                Random.Range(
                                    bottomLeft.x,
                                    topRight.x
                                ),
                                y,
                                depth
                            ),

                        velocity =
                            new Vector3(
                                height *
                                Random.Range(
                                    -0.025f,
                                    0.025f
                                ),
                                -speed,
                                0f
                            ),

                        // Hue comes from this emitter's own material.
                        startColor = Color.white,

                        startSize =
                            height *
                            Random.Range(
                                0.025f,
                                0.04f
                            ),

                        startLifetime =
                            (
                                y -
                                bottomLeft.y +
                                height * 0.12f
                            )
                            / speed,

                        rotation3D =
                            new Vector3(
                                Random.Range(
                                    0f,
                                    360f
                                ),
                                Random.Range(
                                    0f,
                                    360f
                                ),
                                Random.Range(
                                    0f,
                                    360f
                                )
                            )
                    };

                particles.Emit(
                    emitParams,
                    1
                );
            }

            return true;
        }

        private bool Initialize()
        {
            Shader shader =
                Resources.Load<Shader>(
                    "PurchaseConfetti"
                );

            if (shader == null)
            {
                Debug.LogError(
                    "PurchaseConfetti shader could not be found " +
                    "inside a Resources folder."
                );

                return false;
            }

            // Create a small rectangular piece of paper.
            paper =
                new Mesh
                {
                    name = "Confetti paper",

                    vertices =
                        new[]
                        {
                            new Vector3(
                                -0.5f,
                                -0.3f,
                                0f
                            ),

                            new Vector3(
                                0.5f,
                                -0.3f,
                                0f
                            ),

                            new Vector3(
                                0.5f,
                                0.3f,
                                0f
                            ),

                            new Vector3(
                                -0.5f,
                                0.3f,
                                0f
                            )
                        },

                    // Keep the mesh itself white.
                    //
                    // Particle color will be supplied separately
                    // through ParticleSystemVertexStream.Color.
                    colors =
                        new[]
                        {
                            Color.white,
                            Color.white,
                            Color.white,
                            Color.white
                        },

                    triangles =
                        new[]
                        {
                            0, 1, 2,
                            0, 2, 3
                        }
                };

            paper.RecalculateBounds();

            for (int i = 0; i < Colors.Length; i++)
            {
                var material = new Material(shader) { name = $"Confetti color {i + 1} (runtime)" };
                material.SetColor("_ConfettiColor", Colors[i]);
                materials.Add(material);
                var go = new GameObject($"Purchase confetti color {i + 1}");
                go.transform.SetParent(transform, false);
                var particles = go.AddComponent<ParticleSystem>();
                particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                systems.Add(particles);
                ConfigureMainModule(particles);
                ConfigureEmission(particles);
                ConfigureShape(particles);
                ConfigureRotation(particles);
                ConfigureRenderer(particles, material);
            }

            return true;
        }

        private void ConfigureMainModule(ParticleSystem particles)
        {
            ParticleSystem.MainModule main =
                particles.main;

            main.playOnAwake = false;
            main.loop = false;
            main.duration = 6f;

            main.simulationSpace =
                ParticleSystemSimulationSpace.Local;

            main.useUnscaledTime = true;
            main.gravityModifier = 0f;

            main.cullingMode =
                ParticleSystemCullingMode
                    .AlwaysSimulate;

            main.maxParticles = 40;

            main.startRotation3D = true;

            // The material supplies the hue; keep the particle multiplier neutral.
            main.startColor = Color.white;
        }

        private void ConfigureEmission(ParticleSystem particles)
        {
            ParticleSystem.EmissionModule emission =
                particles.emission;

            // We manually emit particles in Play().
            emission.enabled = false;
        }

        private void ConfigureShape(ParticleSystem particles)
        {
            ParticleSystem.ShapeModule shape =
                particles.shape;

            // Positions are generated manually.
            shape.enabled = false;
        }

        private void ConfigureRotation(ParticleSystem particles)
        {
            ParticleSystem.RotationOverLifetimeModule rotation =
                particles.rotationOverLifetime;

            rotation.enabled = true;
            rotation.separateAxes = true;

            rotation.x =
                new ParticleSystem.MinMaxCurve(
                    -5f,
                    5f
                );

            rotation.y =
                new ParticleSystem.MinMaxCurve(
                    -4f,
                    4f
                );

            rotation.z =
                new ParticleSystem.MinMaxCurve(
                    -6f,
                    6f
                );
        }

        private void ConfigureRenderer(ParticleSystem particles, Material material)
        {
            ParticleSystemRenderer renderer =
                particles.GetComponent<
                    ParticleSystemRenderer
                >();

            renderer.renderMode =
                ParticleSystemRenderMode.Mesh;

            renderer.enableGPUInstancing = false;

            // Color comes from the material, so only positions are required.
            renderer.SetActiveVertexStreams(
                new List<
                    ParticleSystemVertexStream
                >
                {
                    ParticleSystemVertexStream.Position
                }
            );

            renderer.mesh = paper;

            renderer.sharedMaterial =
                material;

            renderer.shadowCastingMode =
                ShadowCastingMode.Off;

            renderer.receiveShadows = false;
        }

        public void Clear()
        {
            foreach (var particles in systems)
                if (particles != null)
                    particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private void OnDestroy()
        {
            foreach (var particles in systems)
                if (particles != null) Destroy(particles.gameObject);
            foreach (var material in materials)
                if (material != null) Destroy(material);
            if (paper != null) Destroy(paper);
        }
    }
}
