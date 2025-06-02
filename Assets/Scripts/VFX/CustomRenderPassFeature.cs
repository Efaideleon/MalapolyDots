using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine;
using System;
using UnityEngine.Rendering.RenderGraphModule.Util;

public class CustomRenderPassFeature : ScriptableRendererFeature
{
    [Serializable]
    public class BlurSettings
    {
        [Range(0,0.4f)] public float horizontalBlur;
        [Range(0,0.4f)] public float verticalBlur;
    }

    class CustomRenderPass : ScriptableRenderPass
    {
        private BlurSettings defaultSettings;
        private Material material;
        private TextureDesc blurTextureDescriptor;

        private static readonly int horizontalBlurId = Shader.PropertyToID("_HorizontalBlur");
        private static readonly int verticalBlurId = Shader.PropertyToID("_VerticalBlur");
        private const string _blurTextureName = "_BlurTexture";
        private const string _verticalPassName = "VerticalBlurRenderPass";
        private const string _horizontalPassName = "horizontalBlurRenderPass";

        public CustomRenderPass(Material material, BlurSettings defaultSettings)
        { 
            this.defaultSettings = defaultSettings;
            this.material = material;
        }

        private void UpdateBlurSettings()
        {
            if (material == null) return;

            var volumeComponent = VolumeManager.instance.stack.GetComponent<CustomVolumeBlurComponent>();

            float horizontalBlur = volumeComponent.horizontalBlur.overrideState 
                ? volumeComponent.horizontalBlur.value 
                : defaultSettings.horizontalBlur;

            float verticalBlur = volumeComponent.verticalBlur.overrideState 
                ? volumeComponent.verticalBlur.value 
                : defaultSettings.verticalBlur;

            material.SetFloat(horizontalBlurId, horizontalBlur);
            material.SetFloat(verticalBlurId, verticalBlur);
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            const string passName = "Render Custom Pass";

            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

            if (resourceData.isActiveTargetBackBuffer)
                return;

            UpdateBlurSettings();

            TextureHandle srcCamColor = resourceData.activeColorTexture;
            blurTextureDescriptor = srcCamColor.GetDescriptor(renderGraph);
            blurTextureDescriptor.name = _blurTextureName;
            blurTextureDescriptor.depthBufferBits = 0;
            var dst = renderGraph.CreateTexture(blurTextureDescriptor);

            if (!srcCamColor.IsValid() || !dst.IsValid())
                return;

            RenderGraphUtils.BlitMaterialParameters paraVeritical = new(srcCamColor, dst, material, 0);
            renderGraph.AddBlitPass(paraVeritical, _verticalPassName);

            RenderGraphUtils.BlitMaterialParameters paraHorizontal = new(dst, srcCamColor, material, 1);
            renderGraph.AddBlitPass(paraHorizontal, _horizontalPassName);
        }
    }

    [SerializeField] private BlurSettings settings;
    [SerializeField] private Shader shader;
    private Material material;
    private CustomRenderPass m_ScriptablePass;

    public override void Create()
    {
        if (shader == null)
        {
            return;
        }

        material = new Material(shader);
        m_ScriptablePass = new CustomRenderPass(material, settings);
        m_ScriptablePass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType == CameraType.Game)
        {
            renderer.EnqueuePass(m_ScriptablePass);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (Application.isPlaying)
        {
            Destroy(material);
        }
        else
        {
            DestroyImmediate(material);
        }
    }
}
