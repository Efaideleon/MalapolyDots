using Unity.Mathematics;
using UnityEngine;

namespace DOTS.GamePlay.CameraSystems
{
    [CreateAssetMenu(fileName = "CameraConfig", menuName = "Scriptable Objects/Camera/CameraConfig")]
    public class CameraConfig : ScriptableObject
    {
        public float3 offset;
        public float angle;
        public float fieldOfView;
    }
}

