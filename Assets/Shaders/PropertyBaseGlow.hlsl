#include "ShaderApiReflectionSupport.hlsl"

#ifndef MALAPOLY_PROPERTY_BASE_GLOW_INCLUDED
#define MALAPOLY_PROPERTY_BASE_GLOW_INCLUDED

/// <funchints>
/// <sg:ProviderKey>MalapolyPropertyBaseGlow</sg:ProviderKey>
/// <sg:DisplayName>Property Base Glow</sg:DisplayName>
/// <sg:SearchCategory>Malapoly/Properties</sg:SearchCategory>
/// <sg:SearchTerms>Monopoly, House, Purchase, Shine, Emission</sg:SearchTerms>
/// </funchints>
UNITY_EXPORT_REFLECTION void PropertyBaseGlow_float(
    float3 Position, float3 Normal, float3 ViewDirection,
    float4 Tint, float4 State, float MonopolyStrength, float PurchaseStrength, float OriginalSmoothness,
    out float3 Emission, out float Shine)
{
    // State: monopoly, fading purchase flash, purchase age, animation time.
    float monopoly = saturate(State.x);
    float purchase = saturate(State.y);
    float rim = pow(1.0 - saturate(dot(normalize(Normal), normalize(ViewDirection))), 3.0);
    float pulse = 0.82 + 0.18 * sin(State.w * 2.2);
    float sweep = pow(saturate(0.5 + 0.5 * sin(dot(Position.xz, float2(0.55, 0.38))
        - State.w * 2.8)), 18.0);
    float glow = monopoly * MonopolyStrength * pulse * (0.18 + 0.82 * rim + 0.3 * sweep);
    float purchaseSweep = pow(saturate(0.5 + 0.5 * sin(dot(Position.xz, float2(0.55, 0.38))
        - State.z * 5.0)), 12.0);
    float flash = purchase * PurchaseStrength * (0.12 + 0.65 * purchaseSweep + 0.25 * rim);
    Emission = Tint.rgb * glow + lerp(Tint.rgb, float3(1.0, 0.88, 0.55), 0.65) * flash;
    Shine = saturate(OriginalSmoothness + monopoly * 0.15 + purchase * 0.45);
}

void PropertyBaseGlow_half(
    half3 Position, half3 Normal, half3 ViewDirection,
    half4 Tint, half4 State, half MonopolyStrength, half PurchaseStrength, half OriginalSmoothness,
    out half3 Emission, out half Shine)
{
    float3 emission;
    float shine;
    PropertyBaseGlow_float(Position, Normal, ViewDirection, Tint, State,
        MonopolyStrength, PurchaseStrength, OriginalSmoothness, emission, shine);
    Emission = emission;
    Shine = shine;
}
#endif
