// Item Highlight Effect with Halftone Glow
// Based on the visual style shown in the reference images

// Shader properties
float4x4 WorldViewProjection;
float4x4 World;
float3 CameraPosition;
float Time;

// Item-specific properties
float3 GlowColor = float3(1.0, 0.8, 0.0); // Default yellow
float GlowIntensity = 2.0;
float HalftoneScale = 20.0;
float HalftoneRadius = 2.0;
float PulseSpeed = 2.0;
float OutlineThickness = 0.05;

Texture2D ObjectTexture;
sampler ObjectSampler = sampler_state
{
    Texture = <ObjectTexture>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
};

struct VS_INPUT
{
    float4 Position : POSITION;
    float3 Normal : NORMAL;
    float2 TexCoord : TEXCOORD0;
};

struct VS_OUTPUT
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
    float3 WorldPos : TEXCOORD1;
    float3 Normal : TEXCOORD2;
    float3 ViewDir : TEXCOORD3;
};

// Vertex Shader
VS_OUTPUT VS_Main(VS_INPUT input)
{
    VS_OUTPUT output;
    
    output.Position = mul(input.Position, WorldViewProjection);
    output.TexCoord = input.TexCoord;
    output.WorldPos = mul(input.Position, World).xyz;
    output.Normal = normalize(mul(input.Normal, (float3x3)World));
    output.ViewDir = normalize(CameraPosition - output.WorldPos);
    
    return output;
}

// Halftone dot pattern function
float HalftoneDot(float2 uv, float2 center, float radius)
{
    float2 coords = uv - center;
    float dist = length(coords);
    return smoothstep(radius, radius * 0.8, dist);
}

// Generate halftone pattern
float HalftonePattern(float2 worldPos, float2 center, float maxRadius)
{
    float2 uv = worldPos - center;
    float distFromCenter = length(uv);
    
    // Don't draw dots beyond max radius
    if (distFromCenter > maxRadius)
        return 0.0;
    
    // Create grid of dots
    float2 gridPos = uv * HalftoneScale;
    float2 gridCell = floor(gridPos);
    float2 gridUV = frac(gridPos);
    
    // Offset every other row for better coverage
    if (fmod(gridCell.y, 2.0) > 0.5)
        gridUV.x += 0.5;
    
    // Center the dot in the cell
    float2 dotCenter = float2(0.5, 0.5);
    
    // Calculate dot size based on distance from center (bigger near center)
    float distanceScale = 1.0 - (distFromCenter / maxRadius);
    distanceScale = pow(distanceScale, 1.5); // Exponential falloff
    
    // Animate the dots
    float pulse = sin(Time * PulseSpeed) * 0.2 + 0.8;
    float dotSize = 0.3 * distanceScale * pulse;
    
    // Draw the dot
    float dot = HalftoneDot(gridUV, dotCenter, dotSize);
    
    // Fade based on distance from center
    dot *= distanceScale;
    
    return dot;
}

// Fresnel effect for rim lighting
float Fresnel(float3 normal, float3 viewDir, float power)
{
    return pow(1.0 - saturate(dot(normal, viewDir)), power);
}

// Pixel Shader
float4 PS_Main(VS_OUTPUT input) : SV_TARGET
{
    // Sample base texture
    float4 baseColor = ObjectTexture.Sample(ObjectSampler, input.TexCoord);
    
    // Calculate fresnel for outline
    float fresnel = Fresnel(input.Normal, input.ViewDir, 3.0);
    
    // Rim glow
    float3 rimGlow = GlowColor * fresnel * GlowIntensity;
    
    // Halftone pattern (in screen/world space around object)
    // For simplicity, using world XZ plane position
    float2 worldPosXZ = input.WorldPos.xz;
    float2 objectCenterXZ = float2(0, 0); // This should be passed from the object's world position
    
    // Calculate halftone with pulsing radius
    float pulse = sin(Time * PulseSpeed) * 0.3 + 1.0;
    float halftone = HalftonePattern(worldPosXZ, objectCenterXZ, HalftoneRadius * pulse);
    
    // Combine halftone with glow color
    float3 halftoneGlow = GlowColor * halftone * GlowIntensity * 0.5;
    
    // Combine all effects
    float3 finalColor = baseColor.rgb + rimGlow + halftoneGlow;
    
    // Boost brightness
    finalColor = saturate(finalColor);
    
    return float4(finalColor, baseColor.a);
}

// Technique
technique ItemHighlight
{
    pass P0
    {
        VertexShader = compile vs_4_0 VS_Main();
        PixelShader = compile ps_4_0 PS_Main();
    }
}
