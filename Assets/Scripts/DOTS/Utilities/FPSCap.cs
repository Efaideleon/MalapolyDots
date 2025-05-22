using UnityEngine;

public class SetFPS : MonoBehaviour
{
    void Awake()
    {
// #if UNITY_EDITOR
//         Application.targetFrameRate = 15;
//         QualitySettings.vSyncCount = 0;
// #endif
        // QualitySettings.vSyncCount = 0;
        // Application.targetFrameRate = 60;
    }
}
