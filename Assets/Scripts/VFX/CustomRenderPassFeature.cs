using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine;

public class CustomRenderPassFeature : ScriptableRendererFeature
{
    class CustomRenderPass : ScriptableRenderPass
    {
        private LayerMask _layerMask;

        public CustomRenderPass(LayerMask layerMask)
        {
            _layerMask = layerMask;
        }
        // This class stores the data needed by the RenderGraph pass.
        // It is passed as a parameter to the delegate function that executes the RenderGraph pass.
        private class PassData
        {
            public RendererListHandle rendererListHandle;
        }

        private void InitRendererLists(ContextContainer frameData, ref PassData passData, RenderGraph renderGraph)
        {
            UniversalRenderingData universalRenderingData = frameData.Get<UniversalRenderingData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

            RenderQueueRange renderQueueRange = new();
            FilteringSettings filteringSettings = new(renderQueueRange, _layerMask);
            DrawingSettings drawingSettings = new();

            RendererListParams param = new(universalRenderingData.cullResults, drawingSettings, filteringSettings);
            passData.rendererListHandle = renderGraph.CreateRendererList(param);
        }

        // This static method is passed as the RenderFunc delegate to the RenderGraph render pass.
        // It is used to execute draw commands.
        static void ExecutePass(PassData data, RasterGraphContext context)
        {
        }

        // RecordRenderGraph is where the RenderGraph handle can be accessed, through which render passes can be added to the graph.
        // FrameData is a context container through which URP resources can be accessed and managed.
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            const string passName = "Render Custom Pass";

            // This adds a raster render pass to the graph, specifying the name and the data type that will be passed to the ExecutePass function.
            using (var builder = renderGraph.AddRasterRenderPass<PassData>(passName, out var passData))
            {
                // Use this scope to set the required inputs and outputs of the pass and to
                // setup the passData with the required properties needed at pass execution time.

                // Make use of frameData to access resources and camera data through the dedicated containers.
                // Eg:
                // UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
                UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

                InitRendererLists(frameData, ref passData, renderGraph);

                builder.UseRendererList(passData.rendererListHandle);

                // Setup pass inputs and outputs through the builder interface.
                // Eg:
                // builder.UseTexture(sourceTexture);
                // TextureHandle destination = UniversalRenderer.CreateRenderGraphTexture(renderGraph, cameraData.cameraTargetDescriptor, "Destination Texture", false);

                // This sets the render target of the pass to the active color texture. Change it to your own render target as needed.
                builder.SetRenderAttachment(resourceData.activeColorTexture, 0);

                // Assigns the ExecutePass function to the render pass delegate. This will be called by the render graph when executing the pass.
                builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));
            }
        }
    }

    public LayerMask _layerMask;

    CustomRenderPass m_ScriptablePass;

    /// <inheritdoc/>
    public override void Create()
    {
        m_ScriptablePass = new CustomRenderPass(_layerMask);

        // Configures where the render pass should be injected.
        m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }
}
