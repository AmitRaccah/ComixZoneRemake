SamplerState sampler_paletteTex;

inline float Luma(float3 c) { return dot(c, float3(0.2126, 0.7152, 0.0722)); }
inline half Luma(half3 c) { return dot(c, half3(0.2126, 0.7152, 0.0722)); }

void PosterizePalette_float(
    float3 sceneRGB,
    float  steps,
    Texture2D paletteTex,
    float2 paletteUV,
    out float3 outRGB)
{
    float l = saturate(Luma(sceneRGB));
    float s = max(1.0, steps);
    float q = floor(l * s) / s;
    outRGB = paletteTex.Sample(sampler_paletteTex, float2(q, paletteUV.y)).rgb;
}

void PosterizePalette_half(
    half3 sceneRGB,
    half  steps,
    Texture2D paletteTex,
    half2 paletteUV,
    out half3 outRGB)
{
    half l = saturate(Luma(sceneRGB));
    half s = max((half)1.0, steps);
    half q = floor(l * s) / s;
    outRGB = paletteTex.Sample(sampler_paletteTex, float2(q, paletteUV.y)).rgb;
}