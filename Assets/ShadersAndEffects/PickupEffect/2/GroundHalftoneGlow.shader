Shader "Custom/GroundHalftoneGlow"
{
    Properties
    {
        _GlowColor ("Glow Color", Color) = (1, 0.95, 0, 1)
        _GlowIntensity ("Glow Intensity", Range(0, 5)) = 2.5
        _HalftoneScale ("Halftone Scale", Range(5, 50)) = 20.0
        _PulseSpeed ("Pulse Speed", Range(0, 5)) = 2.0
        _DotSize ("Dot Size", Range(0.1, 1)) = 0.45
        _Radius ("Effect Radius", Range(0.5, 5)) = 2.5
    }
    
    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "RenderType"="Transparent" 
            "IgnoreProjector"="True"
        }
        
        LOD 100
        
        Blend SrcAlpha One // Additive blending for bright glow
        ZWrite Off
        ZTest LEqual
        Cull Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
            };
            
            float4 _GlowColor;
            float _GlowIntensity;
            float _HalftoneScale;
            float _PulseSpeed;
            float _DotSize;
            float _Radius;
            
            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }
            
            // Halftone dot function
            float HalftoneDot(float2 uv, float2 center, float radius)
            {
                float dist = length(uv - center);
                return smoothstep(radius, radius * 0.7, dist);
            }
            
            // Generate radial halftone pattern
            float HalftonePattern(float2 uv)
            {
                // Convert UV to centered coordinates (-0.5 to 0.5)
                float2 centered = uv - 0.5;
                float distFromCenter = length(centered) * 2.0; // Scale to 0-1 range
                
                // Pulsing effect
                float pulse = sin(_Time.y * _PulseSpeed) * 0.3 + 1.0;
                
                // Don't draw dots beyond radius (in UV space)
                if (distFromCenter > pulse)
                    return 0.0;
                
                // Create hexagonal grid
                float2 gridPos = centered * _HalftoneScale;
                float2 gridCell = floor(gridPos);
                float2 gridUV = frac(gridPos);
                
                // Offset every other row for hexagonal pattern
                if (fmod(gridCell.y, 2.0) > 0.5)
                    gridUV.x += 0.5;
                
                gridUV.x = frac(gridUV.x);
                
                // Dot center
                float2 dotCenter = float2(0.5, 0.5);
                
                // Scale dot size based on distance from center
                float distanceScale = 1.0 - distFromCenter;
                distanceScale = pow(max(distanceScale, 0.0), 1.2);
                
                // Animated dot size
                float dotPulse = sin(_Time.y * _PulseSpeed * 1.5) * 0.15 + 0.85;
                float dotSize = _DotSize * distanceScale * dotPulse;
                
                // Draw the dot
                float dot = HalftoneDot(gridUV, dotCenter, dotSize);
                
                // Fade based on distance from center
                dot *= distanceScale;
                
                // Extra falloff at edges
                float edgeFade = 1.0 - smoothstep(0.7, 1.0, distFromCenter / pulse);
                dot *= edgeFade;
                
                return dot;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                // Generate halftone pattern
                float halftone = HalftonePattern(i.uv);
                
                // Apply glow color and intensity
                float3 color = _GlowColor.rgb * halftone * _GlowIntensity;
                
                // Alpha for blending
                float alpha = halftone * 0.9;
                
                return fixed4(color, alpha);
            }
            ENDCG
        }
    }
    
    FallBack "Transparent/Diffuse"
}
