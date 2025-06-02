using System;
using UnityEngine.Rendering;

[Serializable]
[VolumeComponentMenu("Custom/CustomVolumeBlurComponent")]
public class CustomVolumeBlurComponent : VolumeComponent
{
    public ClampedFloatParameter horizontalBlur = new ClampedFloatParameter(0.05f, 0, 0.5f, true);
    public ClampedFloatParameter verticalBlur = new ClampedFloatParameter(0.05f, 0, 0.5f, true);
}
