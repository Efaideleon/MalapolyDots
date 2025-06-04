#ifndef SAMPLE_MAIN_LIGHT_INCLUDED
#define SAMPLE_MAIN_LIGHT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

void SampleAllLightsFcn_float(float3 worldPos, float3 worldNormal, out float3 lightColor)
{
    // Initialize output
    lightColor = float3(0, 0, 0);
    
    // Sample main light
    float4 shadowCoord = TransformWorldToShadowCoord(worldPos);
    Light mainLight = GetMainLight(shadowCoord);
    
    float3 lightDir = normalize(mainLight.direction);
    float NdotL = saturate(dot(worldNormal, lightDir));
    
    lightColor += mainLight.color * NdotL * mainLight.distanceAttenuation * mainLight.shadowAttenuation;
    
    // Sample additional lights
    uint pixelLightCount = GetAdditionalLightsCount();
    
    LIGHT_LOOP_BEGIN(pixelLightCount)
        Light light = GetAdditionalLight(lightIndex, worldPos);
        
    float3 addLightDir = normalize(light.direction);
    float addNdotL = saturate(dot(worldNormal, addLightDir));
        
    lightColor += light.color * addNdotL * light.distanceAttenuation * light.shadowAttenuation;
    LIGHT_LOOP_END
}
#endif
