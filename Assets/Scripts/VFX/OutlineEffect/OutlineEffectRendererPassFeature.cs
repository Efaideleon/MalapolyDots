// ===========================================================================================
// Naming Conventions
// 
// * Suffix "Dst" on TextureHandle indicates the destination TextureHandle for the renderPass.
// * Suffix "Desc" on TextureDesc stands for Descriptor.
// * Suffix "RenderPassName" indicates the pass names shown in the frameDebugger. 
// ===========================================================================================

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;

using System;
using System.Collections.Generic;

using GraphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat;
using BlitMaterialParameters = UnityEngine.Rendering.RenderGraphModule.Util.RenderGraphUtils.BlitMaterialParameters;

public class OutlineEffectRendererPassFeature : ScriptableRendererFeature
{
    class OutlineRenderPass : ScriptableRenderPass
    {
        private Materials _materials;
        private BlurSettings _blurSettings;
        private LayerMask _outlineLayerMask;
        private Color _color;

        #region Shader Ids
        private static readonly int _horizontalBlurID = Shader.PropertyToID("_HorizontalBlur");
        private static readonly int _verticalBlurID = Shader.PropertyToID("_VerticalBlur");
        private static readonly int _outlineOriginalID = Shader.PropertyToID("_Original");
        private static readonly int _outlineSmoothsteppedID = Shader.PropertyToID("_Smoothstepped");
        private static readonly int _cameralColorID = Shader.PropertyToID("_CameraColor");
        private static readonly int _customDepthTextureID = Shader.PropertyToID("_CustomDepthTexture");
        private static readonly int _outlineOutputID = Shader.PropertyToID("_OutlineOutput");
        private static readonly int _outlineColorID = Shader.PropertyToID("_OutlineColor");
        #endregion

        private const string BlurTextureName = "_BlurTexture";

        static readonly ShaderTagId[] forwardOnlyShaderTagIds = new ShaderTagId[]
        {
            new("UniversalForwardOnly"),
            new("UniversalForward"),
            new("SRPDefaultUnlit"),
            new("LightweightForward"),
            new("Always")
        };

        private List<ShaderTagId> _shaderTagIdList = new(forwardOnlyShaderTagIds);

        #region PassNames 
        private const string CustomDepthTextureRenderPassName = "Build Custom Depth Texture";
        private const string DepthMaskRenderPassName = "Build Depth Mask";
        private const string TempTextureSetupRenderPassName = "Copy Mask To intermediateTexture";
        private const string VerticalBlurRenderPassName = "Add Blur Effect Vertical";
        private const string HorizontalBlurRenderPassName = "Add Blur Effect Horizontal";
        private const string SmoothstepRenderPassName = "Add Smoothstep Effect";
        private const string OutlineRenderPassName = "OutlinePass";
        private const string CompositeRenderPassName = "CompositePass";
        private const string CompositeToColorPassName = "Blit Composite To Camera";
        #endregion

        private class PassData
        {
            internal TextureHandle Original;
            internal TextureHandle Smoothstepped;
            internal TextureHandle OutlineTextureDst;
            internal TextureHandle OriginalCameraColor;
            internal TextureHandle FinalTextureComposite;
            internal TextureHandle CustomDepthTexture;
            internal Material OutlineMaterial;
            internal Material CompositeMaterial;
            internal RendererListHandle RendererListHandle;
            internal RendererListHandle CustomDepthRendererListHandle;
        }

        public OutlineRenderPass(Materials materials, BlurSettings blurSettings, LayerMask outlineLayerMask, Color color)
        {
            _materials = materials;
            _blurSettings = blurSettings;
            _outlineLayerMask = outlineLayerMask;
            _color = color;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            UniversalLightData lightData = frameData.Get<UniversalLightData>();

            if (resourceData.isActiveTargetBackBuffer) { return; }
            if (!resourceData.activeColorTexture.IsValid()) { return; }
            if (!resourceData.cameraDepth.IsValid()) { return; }

            if (_materials.Mask == null) { return; }
            if (_materials.Blur == null) { return; }
            if (_materials.Smoothstep == null) { return; }
            if (_materials.Outline == null) { return; }
            if (_materials.Composite == null) { return; }

            #region TextureDescriptors and TextureHandles
            TextureHandle cameraDepth = resourceData.cameraDepth;

            // maskedTexture
            TextureDesc maskedTextureDesc = resourceData.activeColorTexture.GetDescriptor(renderGraph);
            maskedTextureDesc.colorFormat = GraphicsFormat.R8G8B8A8_UNorm;
            maskedTextureDesc.clearColor = Color.black;
            maskedTextureDesc.name = "Masked Objects Texture";
            TextureHandle maskedTextureDst = renderGraph.CreateTexture(maskedTextureDesc);

            // customDepthTexture
            TextureDesc customDepthTextureDesc = new(cameraDepth.GetDescriptor(renderGraph));
            customDepthTextureDesc.depthBufferBits = DepthBits.Depth32;
            customDepthTextureDesc.clearBuffer = true;
            customDepthTextureDesc.clearColor = Color.clear;
            customDepthTextureDesc.name = "Custom Depth Texture";
            TextureHandle customDepthTexture = renderGraph.CreateTexture(customDepthTextureDesc);

            // intermediateTexture
            TextureDesc tempBlurTextureDesc = resourceData.activeColorTexture.GetDescriptor(renderGraph);
            tempBlurTextureDesc.colorFormat = GraphicsFormat.R8G8B8A8_UNorm;
            TextureHandle tempBlurTextureDst = renderGraph.CreateTexture(tempBlurTextureDesc);

            // blurTexture
            TextureDesc blurTextureDesc = resourceData.activeColorTexture.GetDescriptor(renderGraph);
            blurTextureDesc.name = BlurTextureName;
            blurTextureDesc.colorFormat = GraphicsFormat.R8G8B8A8_UNorm;
            TextureHandle blurTextureDst = renderGraph.CreateTexture(blurTextureDesc);

            // maskedTextureDepth
            TextureDesc maskedObjectsDepthTextureDesc = cameraDepth.GetDescriptor(renderGraph);
            maskedObjectsDepthTextureDesc.name = "Masked Objects Depth Texture";
            maskedObjectsDepthTextureDesc.depthBufferBits = DepthBits.Depth8;
            TextureHandle maskedObjectsDepthTextureDst = renderGraph.CreateTexture(maskedObjectsDepthTextureDesc);

            // smoothstepTexture
            TextureDesc smoothstepTextureDesc = resourceData.activeColorTexture.GetDescriptor(renderGraph);
            smoothstepTextureDesc.colorFormat = GraphicsFormat.R8G8B8A8_UNorm;
            TextureHandle smoothstepTextureDst = renderGraph.CreateTexture(smoothstepTextureDesc);

            // outlineTexture
            TextureDesc outlineTextureDesc = resourceData.activeColorTexture.GetDescriptor(renderGraph);
            TextureHandle outlineTextureDst = renderGraph.CreateTexture(outlineTextureDesc);

            // compositeTexture
            TextureDesc compositeTextureDesc = resourceData.activeColorTexture.GetDescriptor(renderGraph);
            TextureHandle compositeTexture = renderGraph.CreateTexture(compositeTextureDesc);
            #endregion

            // Drawing filtered objects settings 
            SortingCriteria sortFlags = cameraData.defaultOpaqueSortFlags;

            UpdateBlurSettings();
            UpdateOutlineColor();

            #region Custom Depth Texture Pass
            using (var builder = renderGraph.AddRasterRenderPass<PassData>(CustomDepthTextureRenderPassName, out var passData))
            {
                passData.CustomDepthTexture = customDepthTexture;
                builder.SetRenderAttachmentDepth(passData.CustomDepthTexture, AccessFlags.Write);

                FilteringSettings filtering = new(RenderQueueRange.opaque, ~LayerMask.GetMask("IgnoreDepth"));

                DrawingSettings drawingSettings = CreateDrawingSettings(
                        _shaderTagIdList,
                        renderingData,
                        cameraData,
                        lightData,
                        sortFlags
                );
                var rendererListParams = new RendererListParams(renderingData.cullResults, drawingSettings, filtering);

                RendererListHandle rendererListHandle = renderGraph.CreateRendererList(rendererListParams);

                passData.CustomDepthRendererListHandle = rendererListHandle;

                builder.UseRendererList(rendererListHandle);
                builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecuteCustomDepthPass(data, context));
            }
            #endregion

            #region Selected Objects and their Depth Render Pass.
            using (var builder = renderGraph.AddRasterRenderPass<PassData>(DepthMaskRenderPassName, out var passData))
            {
                DrawingSettings drawingSettings = CreateDrawingSettings(
                        _shaderTagIdList,
                        renderingData,
                        cameraData,
                        lightData,
                        sortFlags
                );

                drawingSettings.overrideMaterial = _materials.Mask;
                drawingSettings.overrideMaterialPassIndex = 0;

                RenderQueueRange renderQueueRange = RenderQueueRange.all;
                var filteringSettings = new FilteringSettings(renderQueueRange, _outlineLayerMask);
                var rendererListParams = new RendererListParams(renderingData.cullResults, drawingSettings, filteringSettings);

                var rendererListHandle = renderGraph.CreateRendererList(rendererListParams);
                passData.RendererListHandle = rendererListHandle;

                builder.UseRendererList(passData.RendererListHandle);

                builder.SetRenderAttachmentDepth(maskedObjectsDepthTextureDst, AccessFlags.Write);
                builder.SetRenderAttachment(maskedTextureDst, 0, AccessFlags.Write);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecuteRendererListToMaskPass(data, context));
            }
            #endregion

            #region Vertical and Horizontal Blur Effect with Smoothstep Render Pass
            renderGraph.AddBlitPass(maskedTextureDst, tempBlurTextureDst, Vector2.one, Vector2.zero,
                    passName: TempTextureSetupRenderPassName);

            var verticalBlurPara = new BlitMaterialParameters(tempBlurTextureDst, blurTextureDst, _materials.Blur, 0);
            renderGraph.AddBlitPass(verticalBlurPara, VerticalBlurRenderPassName);

            var horizontalBlurPara = new BlitMaterialParameters(blurTextureDst, tempBlurTextureDst, _materials.Blur, 1);
            renderGraph.AddBlitPass(horizontalBlurPara, HorizontalBlurRenderPassName);

            var smoothstepPara = new BlitMaterialParameters(tempBlurTextureDst, smoothstepTextureDst, _materials.Smoothstep, 0);
            renderGraph.AddBlitPass(smoothstepPara, SmoothstepRenderPassName);
            #endregion

            #region Outline Render Pass
            using (var builder = renderGraph.AddRasterRenderPass<PassData>(OutlineRenderPassName, out var passData))
            {
                passData.Original = maskedTextureDst;
                passData.Smoothstepped = smoothstepTextureDst;

                passData.OutlineTextureDst = outlineTextureDst;
                passData.OutlineMaterial = _materials.Outline;

                builder.UseTexture(passData.Original, AccessFlags.Read);
                builder.UseTexture(passData.Smoothstepped, AccessFlags.Read);

                builder.SetRenderAttachment(passData.OutlineTextureDst, 0, AccessFlags.Write);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));
            }
            #endregion

            #region Composite Render Pass
            using (var builder = renderGraph.AddRasterRenderPass<PassData>(CompositeRenderPassName, out var passData))
            {
                passData.CompositeMaterial = _materials.Composite;
                passData.FinalTextureComposite = compositeTexture;

                passData.OutlineTextureDst = outlineTextureDst;
                passData.OriginalCameraColor = resourceData.activeColorTexture;
                passData.CustomDepthTexture = customDepthTexture;

                builder.UseTexture(passData.OutlineTextureDst, AccessFlags.Read);
                builder.UseTexture(passData.OriginalCameraColor, AccessFlags.Read);
                builder.UseTexture(passData.CustomDepthTexture, AccessFlags.Read);

                builder.SetRenderAttachment(compositeTexture, 0, AccessFlags.Write);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecuteFinalCompositePass(data, context));
            }

            renderGraph.AddBlitPass(compositeTexture, resourceData.activeColorTexture, Vector2.one, Vector2.zero,
                    passName: CompositeToColorPassName);
        }
        #endregion

        #region Execute Methods
        static void ExecutePass(PassData data, RasterGraphContext context)
        {
            data.OutlineMaterial.SetTexture(_outlineOriginalID, data.Original);
            data.OutlineMaterial.SetTexture(_outlineSmoothsteppedID, data.Smoothstepped);

            context.cmd.ClearRenderTarget(clearDepth: false, clearColor: true, backgroundColor: Color.black);
            Blitter.BlitTexture(context.cmd, data.Original, new Vector4(1, 1, 0, 0), data.OutlineMaterial, 0);
        }

        static void ExecuteFinalCompositePass(PassData data, RasterGraphContext context)
        {
            data.CompositeMaterial.SetTexture(_cameralColorID, data.OriginalCameraColor);
            data.CompositeMaterial.SetTexture(_customDepthTextureID, data.CustomDepthTexture);
            data.CompositeMaterial.SetTexture(_outlineOutputID, data.OutlineTextureDst);

            Blitter.BlitTexture(context.cmd, data.OutlineTextureDst, new Vector4(1, 1, 0, 0), data.CompositeMaterial, 0);
        }

        static void ExecuteRendererListToMaskPass(PassData data, RasterGraphContext context)
        {
            context.cmd.DrawRendererList(data.RendererListHandle);
        }

        static void ExecuteCustomDepthPass(PassData data, RasterGraphContext context)
        {
            context.cmd.DrawRendererList(data.CustomDepthRendererListHandle);
        }
        #endregion

        private void UpdateBlurSettings()
        {
            if (_materials.Blur == null) { return; }

            float horizontalBlur = _blurSettings.HorizontalBlur;
            float verticalBlur = _blurSettings.VerticalBlur;

            _materials.Blur.SetFloat(_horizontalBlurID, horizontalBlur);
            _materials.Blur.SetFloat(_verticalBlurID, verticalBlur);
        }

        private void UpdateOutlineColor()
        {
            if (_materials.Composite == null) { return; }

            _materials.Composite.SetColor(_outlineColorID, _color);
        }
    }

    [Serializable]
    public class BlurSettings
    {
        [Range(0, 0.4f)] public float HorizontalBlur;
        [Range(0, 0.4f)] public float VerticalBlur;
    }

    private class Materials
    {
        public Material Mask;
        public Material Blur;
        public Material Smoothstep;
        public Material Outline;
        public Material Composite;
    }

    [SerializeField] private BlurSettings _blurSettings;
    [SerializeField] private Shader _maskShader;
    [SerializeField] private Shader _smoothstepShader;
    [SerializeField] private Shader _outlineShader;
    [SerializeField] private Shader _compositeShader;
    [SerializeField] private Shader _blurShader;
    [SerializeField] private LayerMask _outlineLayerMask;
    [SerializeField] private Color _outlineColor;

    private Materials _materials = new();

    OutlineRenderPass _outlineRenderPass;

    public override void Create()
    {
        if (_maskShader == null)
        {
            Debug.Log("maskShader is null");
            return;
        }

        if (_smoothstepShader == null)
        {
            Debug.Log("smoothstepShader is null");
            return;
        }

        if (_outlineShader == null)
        {
            Debug.Log("outlineShader is null");
            return;
        }

        if (_compositeShader == null)
        {
            Debug.Log("compositeShader is null");
            return;
        }

        if (_blurShader == null)
        {
            Debug.Log("blurShader is null");
            return;
        }

        _materials.Mask = new Material(_maskShader);
        _materials.Blur = new Material(_blurShader);
        _materials.Smoothstep = new Material(_smoothstepShader);
        _materials.Outline = new Material(_outlineShader);
        _materials.Composite = new Material(_compositeShader);

        _outlineRenderPass = new OutlineRenderPass(
                _materials,
                _blurSettings,
                _outlineLayerMask,
                _outlineColor
        );

        _outlineRenderPass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(_outlineRenderPass);
    }
}
