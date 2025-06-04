#ifndef SAMPLE_ADDITIONAL_LIGHTS_INCLUDED
#define SAMPLE_ADDITIONAL_LIGHTS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

// This function accumulates additional light intensity at a world position
void SampleAdditionalLights_float(float3 worldPos, out float3 lightColor)
{
    lightColor = float3(0, 0, 0);

    int additionalLightCount = GetAdditionalLightsCount();

    for (int i = 0; i < additionalLightCount; ++i)
    {
        Light light = GetAdditionalLight(i, worldPos);
        float3 lightDir = normalize(light.direction);
        float NdotL = saturate(dot(float3(0, 1, 0), lightDir)); // assuming a flat surface facing up
        lightColor += light.color * NdotL * light.distanceAttenuation;
    }
}
#endif
