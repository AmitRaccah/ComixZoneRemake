// --- Posterize -> Palette ---
void PosterizePalette_float(
    float3 sceneRGB,
    float steps,
    Texture2D paletteTex,
    SamplerState sampler_paletteTex,
    float2 paletteUV,                 // pass (0.5, 0.5)
    out float3 outRGB)
{
    float l = dot(sceneRGB, float3(0.2126, 0.7152, 0.0722));
    l = saturate(l);
    float s = max(1.0, steps);
    float q = floor(l * s) / s;       // 0..1 stepped
    outRGB = paletteTex.Sample(sampler_paletteTex, float2(q, paletteUV.y)).rgb;
}

// --- Shard mask ---
void ComicShards_float(float2 uv, float t, float softness, out float m)
{
    float2 p = (uv - 0.5) * 2.0;

    float2 v0 = float2(cos(radians(-10.0)), sin(radians(-10.0)));
    float2 v1 = float2(cos(radians( 25.0)), sin(radians( 25.0)));
    float2 v2 = float2(cos(radians( 65.0)), sin(radians( 65.0)));
    float2 v3 = float2(cos(radians(-55.0)), sin(radians(-55.0)));
    float2 v4 = float2(cos(radians(115.0)), sin(radians(115.0)));

    float d0 = dot(p, v0) - lerp(-1.2, 1.2, t);
    float d1 = dot(p, v1) - lerp(-1.0, 1.0, t);
    float d2 = dot(p, v2) - lerp(-1.3, 1.1, t);
    float d3 = dot(p, v3) - lerp(-0.9, 1.3, t);
    float d4 = dot(p, v4) - lerp(-1.1, 1.0, t);

    float h0 = 1.0 - smoothstep(0.0, softness, d0);
    float h1 = 1.0 - smoothstep(0.0, softness, d1);
    float h2 = 1.0 - smoothstep(0.0, softness, d2);
    float h3 = 1.0 - smoothstep(0.0, softness, d3);
    float h4 = 1.0 - smoothstep(0.0, softness, d4);

    m = saturate(max(h0, max(h1, max(h2, max(h3, h4)))));
}

// --- (optional) Halftone ---
void HalftoneDots_float(float2 uv, float scale, float darkness, out float mask)
{
    float2 g = frac(uv * scale) - 0.5;
    float d = length(g);
    float r = lerp(0.15, 0.45, saturate(darkness));
    mask = step(r, d); // 1 outside dots, 0 inside
}

// --- (optional) Speed lines ---
void SpeedLines_float(float2 uv, float2 center, float density, float width, float t, float speed, out float bands)
{
    float2 p = uv - center;
    float a = atan2(p.y, p.x) * (1.0 / (2.0 * 3.14159265)); // 0..1
    float u = frac(a * density + t * speed);
    bands = step(u, width); // 1 = line
}
