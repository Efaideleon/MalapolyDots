using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class CustomRenderPassFeature : ScriptableRendererFeature
{
    public override void Create()
    {
    }
    
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // Create and configure your custom render pass here
        //var customPass = new CustomRenderPass();
        //  renderer.EnqueuePass(customPass);
    }
}
