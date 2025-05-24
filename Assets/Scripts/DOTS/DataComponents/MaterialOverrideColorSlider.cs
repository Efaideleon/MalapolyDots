using Unity.Entities;
using Unity.Rendering;

namespace DOTS.DataComponents
{
    [MaterialProperty("_ColorSlider")]
    public struct MaterialOverrideColorSlider : IComponentData
    {
        public float Value;
    }
}
