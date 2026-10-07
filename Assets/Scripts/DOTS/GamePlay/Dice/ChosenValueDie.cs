using System;
using UnityEngine;

namespace DOTS.GamePlay
{
    public sealed class ChosenValueDie : MonoBehaviour
    {
        [Tooltip("Face-center markers from Blender, in order 1 through 6.")]
        public Transform[] Faces = new Transform[6];
        public Transform ContactShadow;
        Quaternion target;
        Vector3 spin, landing;
        float direction;
        public int Value { get; private set; }

        public Vector3 FaceNormal(int value)
        {
            if (value < 1 || value > 6) throw new ArgumentOutOfRangeException(nameof(value));
            return transform.InverseTransformPoint(Faces[value - 1].position).normalized;
        }

        public Quaternion RotationFor(int value, float yaw) =>
            Quaternion.AngleAxis(yaw, Vector3.up) * Quaternion.FromToRotation(FaceNormal(value), Vector3.up);

        public void Begin(int value, Vector3 restingPosition, uint seed, float side)
        {
            Value = value;
            var random = new System.Random(unchecked((int)seed));
            target = RotationFor(value, (float)random.NextDouble() * 50 - 25);
            spin = new Vector3(720 + random.Next(0, 360), side * (540 + random.Next(0, 360)), 360 + random.Next(0, 180));
            landing = restingPosition;
            direction = side;
            Sample(0);
        }

        // Deterministic tumbling and diminishing bounces end at the requested orientation.
        public void Sample(float progress)
        {
            float t = Mathf.Clamp01(progress);
            float remaining = (1 - t) * (1 - t);
            var rotation = Quaternion.Euler(spin * remaining) * target;
            transform.localRotation = rotation;
            float support = .5f * (Mathf.Abs((rotation * Vector3.right).y) +
                                  Mathf.Abs((rotation * Vector3.up).y) +
                                  Mathf.Abs((rotation * Vector3.forward).y));
            float bounce;
            if (t < .45f) bounce = 3f * (1 - Mathf.Pow(t / .45f, 2));
            else if (t < .7f) bounce = Bounce((t - .45f) / .25f, .6f);
            else if (t < .87f) bounce = Bounce((t - .7f) / .17f, .22f);
            else bounce = Bounce((t - .87f) / .13f, .06f);
            transform.localPosition = landing + new Vector3(direction * .22f * Mathf.Sin(t * Mathf.PI) * remaining,
                support + bounce, -1.1f * remaining);
            if (ContactShadow != null)
            {
                ContactShadow.localPosition = new Vector3(transform.localPosition.x, .006f, transform.localPosition.z);
                ContactShadow.localScale = Vector3.one * (1.55f + bounce * .3f);
            }
        }

        static float Bounce(float t, float height) => 4 * height * t * (1 - t);
    }
}
