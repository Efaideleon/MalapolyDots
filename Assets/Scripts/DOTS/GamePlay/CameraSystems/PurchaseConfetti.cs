using UnityEngine;
using UnityEngine.Rendering;

namespace DOTS.GamePlay.CameraSystems
{
    // Created only on clients, after the server confirms a property purchase.
    public sealed class PurchaseConfetti : MonoBehaviour
    {
        ParticleSystem particles;
        Material material;
        Mesh paper;
        static readonly Color[] Colors =
        {
            new Color(1f, 0.08f, 0.16f), // Red
            new Color(1f, 0.4f, 0.05f),  // Orange
            new Color(1f, 0.9f, 0.05f),  // Yellow
            new Color(0.1f, 1f, 0.3f),   // Green
            new Color(0.05f, 0.9f, 1f),  // Cyan
            new Color(0.2f, 0.3f, 1f),   // Blue
            new Color(1f, 0.1f, 0.8f)    // Pink
        };

        public void Play(Vector3 position)
        {
            if (particles == null && !Initialize()) return;
            // World-space particles let successive purchases overlap without moving old confetti.
            particles.transform.position = position;
            particles.Play();
            for (int i = 0; i < 100; i++)
            {
                float angle = Random.Range(0f, Mathf.PI * 2f);
                float speed = Random.Range(1.5f, 4f);
                particles.Emit(new ParticleSystem.EmitParams
                {
                    position = position + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * 0.3f,
                    velocity = new Vector3(Mathf.Cos(angle) * speed, Random.Range(3f, 6f), Mathf.Sin(angle) * speed),
                    startColor = Colors[i % Colors.Length],
                    startSize = Random.Range(0.4f, 0.7f),
                    startLifetime = Random.Range(1.7f, 2.8f),
                    rotation3D = new Vector3(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f))
                }, 1);
            }
        }

        bool Initialize()
        {
            var shader = Resources.Load<Shader>("PurchaseConfetti");
            if (shader == null) return false;
            material = new Material(shader) { name = "Purchase confetti (runtime)" };
            paper = new Mesh
            {
                name = "Confetti paper",
                vertices = new[] { new Vector3(-0.5f, -0.3f, 0), new Vector3(0.5f, -0.3f, 0),
                    new Vector3(0.5f, 0.3f, 0), new Vector3(-0.5f, 0.3f, 0) },
                colors = new[] { Color.white, Color.white, Color.white, Color.white },
                triangles = new[] { 0, 1, 2, 0, 2, 3 }
            };
            paper.RecalculateBounds();
            var go = new GameObject("Property purchase confetti");
            go.transform.SetParent(transform, false);
            particles = go.AddComponent<ParticleSystem>();
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = particles.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = 3f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.useUnscaledTime = true;
            main.gravityModifier = 0.65f;
            main.maxParticles = 400;
            main.startRotation3D = true;
            var emission = particles.emission;
            emission.enabled = false;
            var shape = particles.shape;
            shape.enabled = false;
            var rotation = particles.rotationOverLifetime;
            rotation.enabled = true;
            rotation.separateAxes = true;
            rotation.x = new ParticleSystem.MinMaxCurve(-5f, 5f);
            rotation.y = new ParticleSystem.MinMaxCurve(-4f, 4f);
            rotation.z = new ParticleSystem.MinMaxCurve(-6f, 6f);
            var color = particles.colorOverLifetime;
            color.enabled = true;
            var fade = new Gradient();
            fade.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
                new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 0.7f), new GradientAlphaKey(0, 1) });
            color.color = fade;
            var renderer = particles.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Mesh;
            // This lightweight shader consumes CPU-expanded particle vertices, not
            // Unity's procedural GPU particle-instancing buffer.
            renderer.enableGPUInstancing = false;
            renderer.SetActiveVertexStreams(new System.Collections.Generic.List<ParticleSystemVertexStream>
            {
                ParticleSystemVertexStream.Position,
                ParticleSystemVertexStream.Color
            });
            renderer.mesh = paper;
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return true;
        }

        public void Clear()
        {
            if (particles != null) particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        void OnDestroy()
        {
            if (particles != null) Destroy(particles.gameObject);
            if (material != null) Destroy(material);
            if (paper != null) Destroy(paper);
        }
    }
}
