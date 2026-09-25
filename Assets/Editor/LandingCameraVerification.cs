using System;
using System.Reflection;
using DOTS.GamePlay.CameraSystems;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;

public static class LandingCameraVerification
{
    [MenuItem("Tools/Malapoly/Verify Landing Camera Stability")]
    public static void Run()
    {
        if (Application.isPlaying) throw new InvalidOperationException("Run in Edit Mode.");
        var manager = UnityEngine.Object.FindFirstObjectByType<CinemachineCameraManager>();
        if (manager == null) throw new InvalidOperationException("Open MainScene first.");
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var source = (CinemachineCamera)typeof(CinemachineCameraManager)
            .GetField("landingCamera", flags).GetValue(manager);
        float originalDrift = 0, fixedDrift = 0;
        int cases = 0;
        foreach (int fps in new[] { 30, 60, 120 })
        foreach (float heading in new[] { 0f, 90f, 180f, 270f })
        foreach (bool fix in new[] { false, true })
        {
            var target = new GameObject("Landing verification target") { hideFlags = HideFlags.HideAndDontSave };
            var shot = UnityEngine.Object.Instantiate(source.gameObject);
            shot.hideFlags = HideFlags.HideAndDontSave;
            try
            {
                // ElMercado's authored look-at point, plus all four board headings.
                target.transform.SetPositionAndRotation(new Vector3(56.54f, 3.15f, 45.53f), Quaternion.Euler(0, heading, 0));
                var camera = shot.GetComponent<CinemachineCamera>();
                var initialOffset = Quaternion.Euler(0, heading, 0) * new Vector3(0, 12, -16);
                camera.transform.SetPositionAndRotation(target.transform.position + initialOffset,
                    Quaternion.LookRotation(-initialOffset, Vector3.up));
                camera.Follow = target.transform;
                camera.LookAt = target.transform;
                typeof(CinemachineCameraManager).GetMethod("ConfigureCamera", flags).Invoke(manager, new object[] { camera });
                if (fix)
                    typeof(CinemachineCameraManager).GetMethod("ConfigureLandingCamera", flags).Invoke(manager, new object[] { camera });
                camera.PreviousStateIsValid = false;
                Vector3 previous = Vector3.zero;
                Quaternion previousRotation = Quaternion.identity;
                float maxStep = 0, maxAngle = 0;
                for (int frame = 0; frame < fps * 15; frame++)
                {
                    camera.InternalUpdateCameraState(Vector3.up, 1f / fps);
                    var position = camera.State.GetFinalPosition();
                    var rotation = camera.State.GetFinalOrientation();
                    if (frame >= fps * 12)
                    {
                        maxStep = Mathf.Max(maxStep, Vector3.Distance(previous, position));
                        maxAngle = Mathf.Max(maxAngle, Quaternion.Angle(previousRotation, rotation));
                    }
                    if (fix && (float.IsNaN(position.y) || position.y < 2.999f))
                        throw new Exception("Landing camera violated board clearance.");
                    previous = position;
                    previousRotation = rotation;
                }
                if (fix)
                {
                    fixedDrift = Mathf.Max(fixedDrift, maxStep);
                    if (maxStep > 0.001f || maxAngle > 0.1f)
                        throw new Exception($"Landing camera did not settle: {fps} FPS, heading {heading}: step={maxStep}, angle={maxAngle}");
                    cases++;
                }
                else originalDrift = Mathf.Max(originalDrift, maxStep);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(shot);
                UnityEngine.Object.DestroyImmediate(target);
            }
        }
        Debug.Log($"Landing camera verification PASS: {cases} cases. Original maximum settled-frame movement={originalDrift:F6}; fixed={fixedDrift:F6}.");
    }
}
