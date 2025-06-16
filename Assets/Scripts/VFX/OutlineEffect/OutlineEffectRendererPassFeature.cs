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
        private Material _colorBlurMaterial;
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
            internal TextureHandle sceneMaskDST;
            internal TextureHandle originalCameraColor;
            internal TextureHandle finalTextureComposite;
            internal TextureHandle cameraDepth;
            internal TextureHandle maskedObjectsDepth;
            internal TextureHandle smoothSteppedOutline;
            internal TextureHandle smoothSteppedSceneMask;
            internal TextureHandle blurredObjectDepthMask;
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
                LayerMask outlineLayerMask,
                Material colorBlurMaterial
        )
        {
            _maskMaterial = maskMaterial;
            _blurMaterial = blurMaterial;
            _outlineMaterial = outlineMaterial;
            _smoothstepMaterial = smoothstepMaterial;
            _compositeMaterial = compositeMaterial;
            _colorBlurMaterial = colorBlurMaterial;
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

        private void UpdateSolidBlurSettings()
        {
            if (_colorBlurMaterial == null) return;


            float horizontalBlur = _blurSettings.horizontalBlur;
            float verticalBlur =  _blurSettings.verticalBlur;

            _colorBlurMaterial.SetFloat(horizontalBlurId, horizontalBlur);
            _colorBlurMaterial.SetFloat(verticalBlurId, verticalBlur);
        }
        
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalRenderingData universalRenderingData = frameData.Get<UniversalRenderingData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            UniversalLightData lightData = frameData.Get<UniversalLightData>();

            if (resourceData.isActiveTargetBackBuffer) return;
            if (!resourceData.activeColorTexture.IsValid()) return;
            if (!resourceData.cameraDepth.IsValid()) return;

            if (_maskMaterial == null) return;
            if (_blurMaterial == null) return;
            if (_smoothstepMaterial == null) return;
            if (_outlineMaterial == null) return;
            if (_compositeMaterial == null) return;

            TextureHandle depthTexture = resourceData.cameraDepth;

            // solidBlurTexture
            TextureDesc solidBlurDesc = resourceData.activeColorTexture.GetDescriptor(renderGraph);
            solidBlurDesc.colorFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_UNorm;
            //solidBlurDesc.depthBufferBits = DepthBits.Depth8;
            solidBlurDesc.name = _blurTextureName;
            TextureHandle solidBlurTexture = renderGraph.CreateTexture(solidBlurDesc);

            // solidIntermediateBlurTexture
            TextureDesc solidIntermediateBlurDesc = resourceData.activeColorTexture.GetDescriptor(renderGraph);
            solidIntermediateBlurDesc.colorFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm;
            solidIntermediateBlurDesc.name = _blurTextureName;
            TextureHandle solidIntermediateBlurTexture = renderGraph.CreateTexture(solidIntermediateBlurDesc);

            // intermediateTexture
            TextureDesc intermediateDesc = resourceData.activeColorTexture.GetDescriptor(renderGraph);
            intermediateDesc.colorFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm;
            TextureHandle intermediateTexture = renderGraph.CreateTexture(intermediateDesc);

            // intermediateSmoothSteppedTexture
            TextureDesc intermediateSmoothSteppedDesc = resourceData.activeColorTexture.GetDescriptor(renderGraph);
            intermediateSmoothSteppedDesc.colorFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm;
            TextureHandle intermediateSmoothSteppedTexture = renderGraph.CreateTexture(intermediateSmoothSteppedDesc);

            // blurOutlineTexture with the cropped piece black and white
            var _blurOutlineTextureDescriptor = resourceData.activeColorTexture.GetDescriptor(renderGraph);
            _blurOutlineTextureDescriptor.name = _blurTextureName;
            _blurOutlineTextureDescriptor.colorFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm;
            var blurOutlineTextureDST = renderGraph.CreateTexture(_blurOutlineTextureDescriptor);

            // blurTexture
            _blurTextureDescriptor = resourceData.activeColorTexture.GetDescriptor(renderGraph);
            _blurTextureDescriptor.name = _blurTextureName;
            _blurTextureDescriptor.colorFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm;
            var dst = renderGraph.CreateTexture(_blurTextureDescriptor);

            // maskedTexture
            TextureDesc maskedTextureDesc = resourceData.activeColorTexture.GetDescriptor(renderGraph);
            maskedTextureDesc.colorFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm;
            maskedTextureDesc.clearColor = Color.black;
            maskedTextureDesc.name = "Masked Objects Texture";
            TextureHandle maskedTexture = renderGraph.CreateTexture(maskedTextureDesc);

            // maskedTextureDepth
            TextureDesc maskedObjectsDepthDesc = resourceData.cameraDepth.GetDescriptor(renderGraph);
            maskedObjectsDepthDesc.name = "Masked Objects Depth Texture";
            maskedObjectsDepthDesc.depthBufferBits = DepthBits.Depth8;
            TextureHandle maskedObjectsDepthTexture = renderGraph.CreateTexture(maskedObjectsDepthDesc);

            // smoothSteppedOutline
            TextureDesc smoothsteppedDesc = resourceData.activeColorTexture.GetDescriptor(renderGraph);
            smoothsteppedDesc.colorFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm;
            smoothsteppedDesc.clearColor = Color.black;
            smoothsteppedDesc.name = "Masked Objects Texture";
            TextureHandle smoothsteppedTexture = renderGraph.CreateTexture(smoothsteppedDesc);

            // intermediateSmoothSteppedSceneMaskTexture
            TextureDesc intermediateSmoothSteppedSceneMaskDesc = resourceData.activeColorTexture.GetDescriptor(renderGraph);
            intermediateSmoothSteppedSceneMaskDesc.colorFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm;
            TextureHandle intermediateSmoothSteppedSceneMaskTexture = renderGraph.CreateTexture(intermediateSmoothSteppedDesc);

            // blurSceneMaskTexture
            var _blurSceneMaskTextureDescriptor = resourceData.activeColorTexture.GetDescriptor(renderGraph);
            _blurSceneMaskTextureDescriptor.name = "Blur Scene Mask Texture";
            _blurSceneMaskTextureDescriptor.colorFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm;
            var blurSceneMaskDst = renderGraph.CreateTexture(_blurSceneMaskTextureDescriptor);

            // Scene smooth mask
            TextureDesc smoothsteppedSceneMaskDesc = resourceData.activeColorTexture.GetDescriptor(renderGraph);
            smoothsteppedDesc.colorFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm;
            smoothsteppedDesc.clearColor = Color.black;
            smoothsteppedDesc.name = "Masked Scene Objects Texture";
            TextureHandle smoothsteppedSceneMaskTexture = renderGraph.CreateTexture(smoothsteppedSceneMaskDesc);

            // Drawing filtered objects settings 
            var sortFlags = cameraData.defaultOpaqueSortFlags;
            RenderQueueRange renderQueueRange = RenderQueueRange.all;
            FilteringSettings filteringSettings = new FilteringSettings(renderQueueRange, _outlineLayerMask);
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
            TextureHandle sceneMaskTexture = renderGraph.CreateTexture(outlineDesc);

            UpdateBlurSettings();
            UpdateSolidBlurSettings();

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Build Depth Mask", out var passData) )
            {
                passData.rendererListHandle = rendererListHandle;

                builder.UseRendererList(passData.rendererListHandle);

                builder.SetRenderAttachmentDepth(maskedObjectsDepthTexture, AccessFlags.Write);
                builder.SetRenderAttachment(maskedTexture, 0, AccessFlags.Write);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecuteRendererListToMaskPass(data, context));
            }

            // renderGraph.AddBlitPass(
            //         resourceData.cameraDepth,
            //         resourceData.activeColorTexture,
            //         new Vector2(1, 1),
            //         new Vector2(0, 0),
            //         passName: "Testing Depth"
            // );

            //renderGraph.AddBlitPass(resourceData.cameraDepth, resourceData.activeColorTexture, new Vector2(1, 1), new Vector2(0, 0), passName: "Testing Depth");

            // Regular Orignal masked object outline
            renderGraph.AddBlitPass(
                    maskedTexture,
                    intermediateTexture,
                    new Vector2(1, 1),
                    new Vector2(0, 0),
                    passName: "Copy Mask To Camera"
            );

            // SOLID BLUR PASS
            var solidVerticalBlurParams = new RenderGraphUtils.BlitMaterialParameters(
                    maskedObjectsDepthTexture,
                    solidIntermediateBlurTexture,
                    _colorBlurMaterial,
                    0
            );
            renderGraph.AddBlitPass(
                    solidVerticalBlurParams,
                    passName: "YYYYYYY Blur Vertical Solid Blur"
            );

            var solidHorizontalBlurParams = new RenderGraphUtils.BlitMaterialParameters(
                    solidIntermediateBlurTexture,
                    solidBlurTexture,
                    _colorBlurMaterial,
                    1
            );
            renderGraph.AddBlitPass(
                    solidHorizontalBlurParams,
                    passName: "YYYYYYY Blur Horizontal Solid Blur"
            );

            // Blur for regular outline
            var verticalBlurPara = new RenderGraphUtils.BlitMaterialParameters(
                    intermediateTexture,
                    dst,
                    _blurMaterial,
                    0
            );
            renderGraph.AddBlitPass(
                    verticalBlurPara,
                    "Add Blur Effect Vertical"
            );

            var horizontalBlurPara = new RenderGraphUtils.BlitMaterialParameters(
                    dst,
                    intermediateTexture,
                    _blurMaterial,
                    1
            );
            renderGraph.AddBlitPass(
                    horizontalBlurPara,
                    "Add Blur Effect Horizontal"
            );

            var smoothstepPara = new RenderGraphUtils.BlitMaterialParameters(
                    intermediateTexture,
                    smoothstepDST,
                    _smoothstepMaterial,
                    0
            );
            renderGraph.AddBlitPass(
                    smoothstepPara,
                    "Add Smoothstep Effect"
            );

            // Final Blit pass
            renderGraph.AddBlitPass(
                    smoothstepDST,
                    intermediateTexture,
                    new Vector2(1, 1),
                    new Vector2(0, 0),
                    passName: "Add Smoothstep Blit Pass"
            );

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("OutlinePass", out var passData))
            {
                passData.orignal = maskedTexture;
                passData.smoothstepped = smoothstepDST;
                passData.sceneMaskDST = sceneMaskTexture;
                passData.outlineMaterial = _outlineMaterial;
                passData.cameraDepth = depthTexture;
                passData.maskedObjectsDepth = maskedObjectsDepthTexture;
                passData.blurredObjectDepthMask = solidBlurTexture;

                if (passData.outlineMaterial == null)
                {
                    Debug.LogWarning("passData.outlineMaterial is null");
                    return;
                }
                
                builder.UseTexture(passData.orignal, AccessFlags.Read);
                builder.UseTexture(passData.smoothstepped, AccessFlags.Read);
                //builder.UseTexture(passData.sceneMaskDST, AccessFlags.Read);
                builder.UseTexture(passData.cameraDepth, AccessFlags.Read);
                builder.UseTexture(passData.maskedObjectsDepth, AccessFlags.Read);
                builder.UseTexture(passData.blurredObjectDepthMask, AccessFlags.Read);

                builder.SetRenderAttachment(passData.sceneMaskDST, 0, AccessFlags.Write);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));
            }

            // ================ Smoothstepping scene mask ================
            var verticalBlurSceneMaskPara = new RenderGraphUtils.BlitMaterialParameters(
                    maskedTexture,
                    intermediateSmoothSteppedSceneMaskTexture,
                    _blurMaterial,
                    0
            );
            renderGraph.AddBlitPass(
                    verticalBlurSceneMaskPara,
                    "Add Blur Effect Vertical To Scene Mask"
            );

            var horizontalBlurSceneMaskPara = new RenderGraphUtils.BlitMaterialParameters(
                    intermediateSmoothSteppedSceneMaskTexture,
                    blurSceneMaskDst,
                    _blurMaterial,
                    1
            );
            renderGraph.AddBlitPass(
                    horizontalBlurSceneMaskPara,
                    "Add Blur Effect Horizontal To Scene Mask"
            );

            var smoothstepSceneMaskPara = new RenderGraphUtils.BlitMaterialParameters(
                    blurSceneMaskDst,
                    smoothsteppedSceneMaskTexture,
                    _smoothstepMaterial,
                    0
            );
            renderGraph.AddBlitPass(
                    smoothstepSceneMaskPara,
                    "Add Smoothstep Effect To Scene Mask"
            );

            // ================ Smoothstepping cropped texture ================
            renderGraph.AddBlitPass(
                    sceneMaskTexture,
                    intermediateSmoothSteppedTexture,
                    new Vector2(1, 1),
                    new Vector2(0, 0),
                    passName: "Copy SmoothStepped Outline To intermediateSmoothSteppedTexture"
            );

            var verticalOutlineBlurPara = new RenderGraphUtils.BlitMaterialParameters(
                    intermediateSmoothSteppedTexture,
                    blurOutlineTextureDST,
                    _blurMaterial,
                    0
            );
            renderGraph.AddBlitPass(
                    verticalOutlineBlurPara,
                    "Add Blur Effect Vertical Outline"
            );

            var horizontalOutlineBlurPara = new RenderGraphUtils.BlitMaterialParameters(
                    blurOutlineTextureDST,
                    intermediateSmoothSteppedTexture,
                    _blurMaterial,
                    1
            );
            renderGraph.AddBlitPass(
                    horizontalOutlineBlurPara,
                    "Add Blur Effect Horizontal Outline!!!!"
            );

            var smoothSteppedOutlinePara = new RenderGraphUtils.BlitMaterialParameters(
                    intermediateSmoothSteppedTexture,
                    smoothsteppedTexture,
                    _smoothstepMaterial,
                    0
            );
            renderGraph.AddBlitPass(
                    smoothSteppedOutlinePara,
                    "Add Smoothstep Effect To Outline Cropped"
            );

            // ================ Composite Logic ================
            TextureDesc compositeDesc = resourceData.activeColorTexture.GetDescriptor(renderGraph);
            TextureHandle compositeTexture = renderGraph.CreateTexture(compositeDesc);

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("CompositePass", out var passData))
            {
                passData.sceneMaskDST = sceneMaskTexture;
                passData.originalCameraColor = resourceData.activeColorTexture;
                passData.compositeMaterial = _compositeMaterial;
                passData.finalTextureComposite = compositeTexture;
                passData.cameraDepth = resourceData.cameraDepth;
                passData.maskedObjectsDepth = maskedObjectsDepthTexture;
                passData.smoothstepped = smoothstepDST;
                passData.smoothSteppedOutline = smoothsteppedTexture;
                passData.smoothSteppedSceneMask = smoothsteppedSceneMaskTexture;
                passData.orignal = maskedTexture;

                if (passData.compositeMaterial == null)
                {
                    Debug.LogWarning("passData.compositeMaterial is null");
                    return;
                }

                builder.UseTexture(passData.sceneMaskDST, AccessFlags.Read);
                builder.UseTexture(passData.originalCameraColor, AccessFlags.Read);
                builder.UseTexture(passData.cameraDepth, AccessFlags.Read);
                builder.UseTexture(passData.maskedObjectsDepth, AccessFlags.Read);
                builder.UseTexture(passData.smoothstepped, AccessFlags.Read);
                builder.UseTexture(passData.smoothSteppedOutline, AccessFlags.Read);
                builder.UseTexture(passData.smoothSteppedSceneMask, AccessFlags.Read);
                builder.UseTexture(passData.orignal, AccessFlags.Read);

                builder.SetRenderAttachment(compositeTexture, 0, AccessFlags.Write);

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
            // which SetTexture expects.

            if (data.outlineMaterial == null)
            {
                Debug.LogWarning("data.outLineMater is null in ExecutePass");
                return;
            }

            data.outlineMaterial.SetTexture("_CameraDepth", data.cameraDepth);
            data.outlineMaterial.SetTexture("_MaskedObjectsDepth", data.maskedObjectsDepth);
            data.outlineMaterial.SetTexture("_BlurredObjectDepthMask", data.blurredObjectDepthMask);
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

            data.compositeMaterial.SetTexture("_Outline", data.orignal);
            data.compositeMaterial.SetTexture("_CameraColor", data.originalCameraColor);
            data.compositeMaterial.SetTexture("_CameraDepth", data.cameraDepth);
            data.compositeMaterial.SetTexture("_MaskedObjectsDepth", data.maskedObjectsDepth);
            data.compositeMaterial.SetTexture("_Smoothstepped", data.smoothstepped);
            data.compositeMaterial.SetTexture("_SmoothsteppedOutline", data.smoothSteppedOutline);
            data.compositeMaterial.SetTexture("_SmoothsteppedSceneMask", data.smoothSteppedSceneMask);
            data.compositeMaterial.SetTexture("_OutlineOutput", data.sceneMaskDST);

            Blitter.BlitTexture(context.cmd, data.sceneMaskDST, new Vector4(1, 1, 0, 0), data.compositeMaterial, 0);
        }

        static void ExecuteRendererListToMaskPass(PassData data, RasterGraphContext context)
        {
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
    [SerializeField] Shader colorBlurShader;
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
        var colorBlurMaterial = new Material(colorBlurShader);

        _outlineRenderPass = new OutlineRenderPass(
                maskMaterial,
                blurMaterial,
                smoothstepMaterial,
                outlineMaterial,
                compositeMaterial,
                blurSettings,
                outlineLayerMask,
                colorBlurMaterial
        );

        _outlineRenderPass.renderPassEvent = RenderPassEvent.AfterRenderingSkybox;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(_outlineRenderPass);
    }
}
