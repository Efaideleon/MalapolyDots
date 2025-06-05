
void GetLight_float(float3 WorldPos, out float3 Direction, out float3 Color, out float Attenuation)
{
#if defined(SHADERGRAPH_PREVIEW)
    Direction = float3(0.5, 0.5, 0);
    Color = 1;
    Attenuation = 0.5;
#else 
    float4 sCoord = TransformWorldToShadowCoord(WorldPos);
    Light mainLight = GetMainLight(sCoord);
    Direction = mainLight.direction;
    Color = mainLight.color;
    Attenuation = mainLight.shadowAttenuation;
#endif
}