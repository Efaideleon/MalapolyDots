using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using System;
using System.Collections.Generic;

public class OutlineEffectRendererPassFeature : ScriptableRendererFeature
{
    class OutlineRenderPass : ScriptableRenderPass
    {
        private Material _maskMaterial;
        private Material _blurMaterial;
        private Material _smoothstepMaterial;
        private Material _outlineMaterial;
        private Material _compositeMaterial;
        private BlurSettings _blurSettings;
        private TextureDesc _blurTextureDescriptor;
        private LayerMask _outlineLayerMask;

        private static readonly int horizontalBlurId = Shader.PropertyToID("_HorizontalBlur");
        private static readonly int verticalBlurId = Shader.PropertyToID("_VerticalBlur");

        private static readonly int outlineOriginalID = Shader.PropertyToID("_Original");
        private static readonly int outlineSmoothsteppedID = Shader.PropertyToID("_Smoothstepped");
        private const string _blurTextureName = "_BlurTexture";

        private List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>();


        private class PassData
        {
            internal TextureHandle orignal;
            internal TextureHandle smoothstepped;
            internal TextureHandle outlineDST;
            internal TextureHandle originalCameraColor;
            internal TextureHandle finalTextureComposite;
            internal Material outlineMaterial;
            internal Material compositeMaterial;
            internal RendererListHandle rendererListHandle;
        }

        public OutlineRenderPass(
                Material maskMaterial,
                Material blurMaterial,
                Material smoothstepMaterial,
                Material outlineMaterial,
                Material compositeMaterial,
                BlurSettings blurSettings,
                LayerMask outlineLayerMask
        )
        {
            _maskMaterial = maskMaterial;
            _blurMaterial = blurMaterial;
            _outlineMaterial = outlineMaterial;
            _smoothstepMaterial = smoothstepMaterial;
            _compositeMaterial = compositeMaterial;
            _blurSettings = blurSettings;
            _outlineLayerMask = outlineLayerMask;
        }

        private void UpdateBlurSettings()
        {
            if (_blurMaterial == null) return;


            float horizontalBlur = _blurSettings.horizontalBlur;
            float verticalBlur =  _blurSettings.verticalBlur;

            _blurMaterial.SetFloat(horizontalBlurId, horizontalBlur);
            _blurMaterial.SetFloat(verticalBlurId, verticalBlur);
        }
        
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            string passName = "OutlinePass";

            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalRenderingData universalRenderingData = frameData.Get<UniversalRenderingData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            UniversalLightData lightData = frameData.Get<UniversalLightData>();

            if (resourceData.isActiveTargetBackBuffer) return;
            if (!resourceData.activeColorTexture.IsValid()) return;

            if (_maskMaterial == null) return;
            if (_blurMaterial == null) return;
            if (_smoothstepMaterial == null) return;
            if (_outlineMaterial == null) return;
            if (_compositeMaterial == null) return;

            TextureHandle depthTexture = resourceData.cameraDepth;

            TextureDesc intermediateDesc = resourceData.activeColorTexture.GetDescriptor(renderGraph);
            intermediateDesc.colorFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm;
            TextureHandle intermediateTexture = renderGraph.CreateTexture(intermediateDesc);

            _blurTextureDescriptor = resourceData.activeColorTexture.GetDescriptor(renderGraph);
            _blurTextureDescriptor.name = _blurTextureName;
            _blurTextureDescriptor.depthBufferBits = 0;
            _blurTextureDescriptor.colorFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm;
            var dst = renderGraph.CreateTexture(_blurTextureDescriptor);

            TextureDesc maskedTextureDesc = resourceData.activeColorTexture.GetDescriptor(renderGraph);
            maskedTextureDesc.colorFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm;
            maskedTextureDesc.clearColor = Color.black;
            TextureHandle maskedTexture = renderGraph.CreateTexture(maskedTextureDesc);

            var sortFlags = cameraData.defaultOpaqueSortFlags;
            RenderQueueRange renderQueueRange = RenderQueueRange.all;
            FilteringSettings filteringSettings = new FilteringSettings(renderQueueRange, _outlineLayerMask);
            // Building the RendererListHandle
            ShaderTagId[] forwardOnlyShaderTagIds = new ShaderTagId[]
            {
                new ShaderTagId("UniversalForwardOnly"),
                new ShaderTagId("UniversalForward"),
                new ShaderTagId("SRPDefaultUnlit"), 
                new ShaderTagId("LightweightForward"), 
                new ShaderTagId("Always") 
            };

            m_ShaderTagIdList.Clear();
            
            foreach (ShaderTagId sid in forwardOnlyShaderTagIds)
                m_ShaderTagIdList.Add(sid);

            DrawingSettings drawingSettings = RenderingUtils.CreateDrawingSettings(
                    m_ShaderTagIdList,
                    universalRenderingData,
                    cameraData,
                    lightData,
                    sortFlags
            );

            drawingSettings.overrideMaterial = _maskMaterial;
            drawingSettings.overrideMaterialPassIndex = 0;

            var rendererListParams = new RendererListParams(
                    universalRenderingData.cullResults,
                    drawingSettings,
                    filteringSettings
            );

            var rendererListHandle = renderGraph.CreateRendererList(rendererListParams);

            TextureDesc smoothstepDesc = resourceData.activeColorTexture.GetDescriptor(renderGraph);
            smoothstepDesc.colorFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm;
            TextureHandle smoothstepDST = renderGraph.CreateTexture(smoothstepDesc);

            TextureDesc outlineDesc = resourceData.activeColorTexture.GetDescriptor(renderGraph);
            TextureHandle outlineTexture = renderGraph.CreateTexture(outlineDesc);

            UpdateBlurSettings();

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Build Depth Mask", out var passData) )
            {
                passData.rendererListHandle = rendererListHandle;

                //builder.SetRenderAttachment(maskedTexture, 0, AccessFlags.Write);
                builder.UseRendererList(passData.rendererListHandle);
                builder.SetRenderAttachment(maskedTexture, 0, AccessFlags.Write);
                builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture, AccessFlags.Write);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecuteRendererListToMaskPass(data, context));
            }

            //renderGraph.AddBlitPass(maskedTexture, resourceData.activeColorTexture, new Vector2(1, 1), new Vector2(0, 0), passName: "Copy Mask To Camera 1");
            renderGraph.AddBlitPass(maskedTexture, intermediateTexture, new Vector2(1, 1), new Vector2(0, 0), passName: "Copy Mask To Camera");

            RenderGraphUtils.BlitMaterialParameters verticalBlurPara = new(intermediateTexture, dst, _blurMaterial, 0);
            renderGraph.AddBlitPass(verticalBlurPara, "Add Blur Effect Vertical");

            RenderGraphUtils.BlitMaterialParameters horizontalBlurPara = new(dst, intermediateTexture, _blurMaterial, 1);
            renderGraph.AddBlitPass(horizontalBlurPara, "Add Blur Effect Horizontal");

            RenderGraphUtils.BlitMaterialParameters smoothstepPara = new(intermediateTexture, smoothstepDST, _smoothstepMaterial, 0);
            renderGraph.AddBlitPass(smoothstepPara, "Add Smoothstep Effect");

            // Final Blit pass
            renderGraph.AddBlitPass(smoothstepDST, intermediateTexture, new Vector2(1, 1), new Vector2(0, 0), passName: "Add Smoothstep Blit Pass");

            using (var builder = renderGraph.AddRasterRenderPass<PassData>(passName, out var passData))
            {
                passData.orignal = maskedTexture;
                passData.smoothstepped = smoothstepDST;
                passData.outlineDST = outlineTexture;
                passData.outlineMaterial = _outlineMaterial;

                if (passData.outlineMaterial == null)
                {
                    Debug.LogWarning("passData.outlineMaterial is null");
                    return;
                }
                
                builder.UseTexture(passData.orignal, AccessFlags.Read);
                builder.UseTexture(passData.smoothstepped, AccessFlags.Read);

                builder.SetRenderAttachment(passData.outlineDST, 0, AccessFlags.Write);
                //builder.SetRenderAttachmentDepth(passData.outlineDST, AccessFlags.Write);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));
            }

            // Composite Logic
            TextureDesc compositeDesc = resourceData.activeColorTexture.GetDescriptor(renderGraph);
            TextureHandle compositeTexture = renderGraph.CreateTexture(compositeDesc);

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("CompositePass", out var passData))
            {
                passData.outlineDST = outlineTexture;
                passData.originalCameraColor = resourceData.activeColorTexture;
                passData.compositeMaterial = _compositeMaterial;
                passData.finalTextureComposite = compositeTexture;

                if (passData.compositeMaterial == null)
                {
                    Debug.LogWarning("passData.compositeMaterial is null");
                    return;
                }

                builder.UseTexture(passData.outlineDST, AccessFlags.Read);
                builder.UseTexture(passData.originalCameraColor, AccessFlags.Read);

                builder.SetRenderAttachment(compositeTexture, 0, AccessFlags.Write);
                //builder.SetRenderAttachmentDepth(passData.outlineDST, AccessFlags.Write);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecuteFinalCompositePass(data, context));
            }

            renderGraph.AddBlitPass(
                    compositeTexture,
                    resourceData.activeColorTexture,
                    new Vector2(1, 1),
                    new Vector2(0, 0),
                    passName: "Blit Composite To Camera"
            );
        }

        static void ExecutePass(PassData data, RasterGraphContext context)
        {
            // SetTexture is called here because when the ExecutePass is called when the pass is being executed 
            // and the TextureHandle has been Registered as a Texture
            // which SetTexture() expects.

            if (data.outlineMaterial == null)
            {
                Debug.LogWarning("data.outLineMater is null in ExecutePass");
                return;
            }

            data.outlineMaterial.SetTexture(outlineOriginalID, data.orignal);
            data.outlineMaterial.SetTexture(outlineSmoothsteppedID, data.smoothstepped);

            context.cmd.ClearRenderTarget(clearDepth: false, clearColor: true, backgroundColor: Color.black);
            Blitter.BlitTexture(context.cmd, data.orignal, new Vector4(1, 1, 0, 0), data.outlineMaterial, 0);
        }

        static void ExecuteFinalCompositePass(PassData data, RasterGraphContext context)
        {
            if (data.compositeMaterial == null)
            {
                Debug.Log("data.CompositeMaterial is null in ExecuteFinalCompositePass");
                return;
            }

            data.compositeMaterial.SetTexture("_Outline", data.outlineDST);
            data.compositeMaterial.SetTexture("_CameraColor", data.originalCameraColor);

            Blitter.BlitTexture(context.cmd, data.outlineDST, new Vector4(1, 1, 0, 0), data.compositeMaterial, 0);
        }

        static void ExecuteRendererListToMaskPass(PassData data, RasterGraphContext context)
        {
            context.cmd.ClearRenderTarget(RTClearFlags.ColorDepth, Color.black, 1,0);
            context.cmd.DrawRendererList(data.rendererListHandle);

        }
    }

    [Serializable]
    public class BlurSettings
    {
        [Range(0,0.4f)] public float horizontalBlur;
        [Range(0,0.4f)] public float verticalBlur;
    }

    [SerializeField] private BlurSettings blurSettings;
    [SerializeField] Shader maskShader;
    [SerializeField] Shader smoothstepShader;
    [SerializeField] Shader outlineShader;
    [SerializeField] Shader compositeShader;
    [SerializeField] Shader blurShader;
    [SerializeField] LayerMask outlineLayerMask;

    OutlineRenderPass _outlineRenderPass;

    public override void Create()
    {
        if (maskShader == null)
        {
            Debug.Log("markShader is null");
            return;
        }

        if (smoothstepShader == null)
        {
            Debug.Log("smoothstepShader is null");
            return;
        }

        if (outlineShader == null)
        {
            Debug.Log("outlineShader is null");
            return;
        }

        if (compositeShader == null)
        {
            Debug.Log("compositeShader is null");
            return;
        }

        if (blurShader == null)
        {
            Debug.Log("blurShader is null");
            return;
        }

        var blurMaterial = new Material(blurShader);
        var smoothstepMaterial = new Material(smoothstepShader);
        var outlineMaterial = new Material(outlineShader);
        var compositeMaterial = new Material(compositeShader);
        var maskMaterial = new Material(maskShader);

        _outlineRenderPass = new OutlineRenderPass(
                maskMaterial,
                blurMaterial,
                smoothstepMaterial,
                outlineMaterial,
                compositeMaterial,
                blurSettings,
                outlineLayerMask
        );

        _outlineRenderPass.renderPassEvent = RenderPassEvent.AfterRenderingSkybox;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(_outlineRenderPass);
    }
}
