using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using System;

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

        private static readonly int horizontalBlurId = Shader.PropertyToID("_HorizontalBlur");
        private static readonly int verticalBlurId = Shader.PropertyToID("_VerticalBlur");

        private static readonly int outlineOriginalID = Shader.PropertyToID("_Original");
        private static readonly int outlineSmoothsteppedID = Shader.PropertyToID("_Smoothstepped");
        private const string _blurTextureName = "_BlurTexture";

        private class PassData
        {
            internal TextureHandle orignal;
            internal TextureHandle smoothstepped;
            internal TextureHandle outlineDST;
            internal TextureHandle originalCameraColor;
            internal TextureHandle finalTextureComposite;
            internal Material outlineMaterial;
            internal Material compositeMaterial;
        }

        public OutlineRenderPass(
                Material maskMaterial,
                Material blurMaterial,
                Material smoothstepMaterial,
                Material outlineMaterial,
                Material compositeMaterial,
                BlurSettings blurSettings)
        {
            _maskMaterial = maskMaterial;
            _blurMaterial = blurMaterial;
            _outlineMaterial = outlineMaterial;
            _smoothstepMaterial = smoothstepMaterial;
            _compositeMaterial = compositeMaterial;
            _blurSettings = blurSettings;
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
            TextureHandle depthTexture = resourceData.cameraDepth;

            TextureDesc intermediateDesc = resourceData.activeColorTexture.GetDescriptor(renderGraph);
            TextureHandle intermediateTexture = renderGraph.CreateTexture(intermediateDesc);

            _blurTextureDescriptor = resourceData.activeColorTexture.GetDescriptor(renderGraph);
            _blurTextureDescriptor.name = _blurTextureName;
            _blurTextureDescriptor.depthBufferBits = 0;
            var dst = renderGraph.CreateTexture(_blurTextureDescriptor);

            TextureDesc maskedTextureDesc = resourceData.activeColorTexture.GetDescriptor(renderGraph);
            TextureHandle maskedTexture = renderGraph.CreateTexture(maskedTextureDesc);

            TextureDesc smoothstepDesc = resourceData.activeColorTexture.GetDescriptor(renderGraph);
            TextureHandle smoothstepDST = renderGraph.CreateTexture(smoothstepDesc);

            TextureDesc outlineDesc = resourceData.activeColorTexture.GetDescriptor(renderGraph);
            TextureHandle outlineTexture = renderGraph.CreateTexture(outlineDesc);

            UpdateBlurSettings();

            RenderGraphUtils.BlitMaterialParameters maskPara = new(depthTexture, maskedTexture, _maskMaterial, 0);
            renderGraph.AddBlitPass(maskPara, "Build Depth Mask");

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

                builder.UseTexture(passData.outlineDST, AccessFlags.Read);
                builder.UseTexture(passData.originalCameraColor, AccessFlags.Read);

                builder.SetRenderAttachment(passData.finalTextureComposite, 0, AccessFlags.Write);
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
            data.outlineMaterial.SetTexture(outlineOriginalID, data.orignal);
            data.outlineMaterial.SetTexture(outlineSmoothsteppedID, data.smoothstepped);

            context.cmd.ClearRenderTarget(clearDepth: false, clearColor: true, backgroundColor: Color.clear);
            Blitter.BlitTexture(context.cmd, data.orignal, new Vector4(1, 1, 0, 0), data.outlineMaterial, 0);
        }

        static void ExecuteFinalCompositePass(PassData data, RasterGraphContext context)
        {
            // SetTexture is called here because when the ExecutePass is called when the pass is being executed 
            // and the TextureHandle has been Registered as a Texture
            // which SetTexture() expects.
            data.compositeMaterial.SetTexture("_Outline", data.outlineDST);
            data.compositeMaterial.SetTexture("_CameraColor", data.originalCameraColor);

            Blitter.BlitTexture(context.cmd, data.outlineDST, new Vector4(1, 1, 0, 0), data.compositeMaterial, 0);
        }
    }

    [Serializable]
    public class BlurSettings
    {
        [Range(0,0.4f)] public float horizontalBlur;
        [Range(0,0.4f)] public float verticalBlur;
    }

    [SerializeField] private BlurSettings blurSettings;
    [SerializeField] Material maskMaterial;
    [SerializeField] Material smoothstepMaterial;
    [SerializeField] Material outlineMaterial;
    [SerializeField] Material compositeMaterial;
    [SerializeField] Shader blurShader;

    OutlineRenderPass _outlineRenderPass;

    public override void Create()
    {
        if (blurShader == null)
            return;

        if (smoothstepMaterial == null)
            return;

        var blurMaterial = new Material(blurShader);
        _outlineRenderPass = new OutlineRenderPass(
                maskMaterial,
                blurMaterial,
                smoothstepMaterial,
                outlineMaterial,
                compositeMaterial,
                blurSettings);

        _outlineRenderPass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (maskMaterial == null)
            return;
        renderer.EnqueuePass(_outlineRenderPass);
    }
}
