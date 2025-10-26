Shader "Custom/ItemHighlightGlow"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Base Color", Color) = (1, 1, 1, 1)
        _GlowColor ("Glow Color", Color) = (1, 0.8, 0, 1)
        _GlowIntensity ("Glow Intensity", Range(0, 5)) = 2.0
        _RimPower ("Rim Power", Range(0.1, 8.0)) = 3.0
        _HalftoneScale ("Halftone Scale", Range(5, 50)) = 20.0
        _HalftoneRadius ("Halftone Radius", Range(0.5, 5)) = 2.0
        _PulseSpeed ("Pulse Speed", Range(0, 5)) = 2.0
        _DotSize ("Dot Size", Range(0.1, 1)) = 0.4
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 200
        
        // First pass - render the object normally
        Pass
        {
            Name "BASE"
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
                float3 viewDir : TEXCOORD3;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float4 _GlowColor;
            float _GlowIntensity;
            float _RimPower;
            float _HalftoneScale;
            float _HalftoneRadius;
            float _PulseSpeed;
            float _DotSize;
            
            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = normalize(_WorldSpaceCameraPos - o.worldPos);
                return o;
            }
            
            // Halftone dot function
            float HalftoneDot(float2 uv, float2 center, float radius)
            {
                float2 coords = uv - center;
                float dist = length(coords);
                return smoothstep(radius, radius * 0.8, dist);
            }
            
            // Generate halftone pattern around object
            float HalftonePattern(float2 worldPos, float2 objectCenter)
            {
                float2 uv = worldPos - objectCenter;
                float distFromCenter = length(uv);
                
                // Pulsing radius
                float pulse = sin(_Time.y * _PulseSpeed) * 0.3 + 1.0;
                float maxRadius = _HalftoneRadius * pulse;
                
                // Don't draw dots beyond max radius
                if (distFromCenter > maxRadius)
                    return 0.0;
                
                // Create grid
                float2 gridPos = uv * _HalftoneScale;
                float2 gridCell = floor(gridPos);
                float2 gridUV = frac(gridPos);
                
                // Offset every other row (hexagonal pattern)
                if (fmod(gridCell.y, 2.0) > 0.5)
                    gridUV.x += 0.5;
                
                gridUV.x = frac(gridUV.x); // Wrap the offset
                
                // Dot center
                float2 dotCenter = float2(0.5, 0.5);
                
                // Scale dot size based on distance
                float distanceScale = 1.0 - (distFromCenter / maxRadius);
                distanceScale = pow(distanceScale, 1.2);
                
                // Animated dot size
                float dotPulse = sin(_Time.y * _PulseSpeed * 1.5) * 0.15 + 0.85;
                float dotSize = _DotSize * distanceScale * dotPulse;
                
                // Draw dot
                float dot = HalftoneDot(gridUV, dotCenter, dotSize);
                
                // Fade based on distance
                dot *= distanceScale;
                
                return dot;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                // Base color from texture and tint (this should stay as-is)
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                
                // Calculate rim - this should be 0 when facing camera, 1 at edges
                float3 normalizedViewDir = normalize(i.viewDir);
                float3 normalizedNormal = normalize(i.worldNormal);
                float rim = 1.0 - saturate(dot(normalizedViewDir, normalizedNormal));
                rim = pow(rim, _RimPower);
                
                // Apply rim glow (only affects edges based on rim value)
                float3 rimGlow = _GlowColor.rgb * rim * _GlowIntensity;
                
                // Add rim to the base color
                col.rgb += rimGlow;
                
                return col;
            }
            ENDCG
        }
        
        // Second pass - ground projection (halftone dots on ground around item)
        Pass
        {
            Name "GROUND_PROJECTION"
            Tags { "Queue"="Transparent" "RenderType"="Transparent" }
            
            Blend SrcAlpha One // Additive blending for bright glow
            ZWrite Off
            ZTest LEqual // Changed from Always
            Offset -1, -1
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 worldPosXZ : TEXCOORD0;
                float2 objectPosXZ : TEXCOORD1;
            };
            
            float4 _GlowColor;
            float _GlowIntensity;
            float _HalftoneScale;
            float _HalftoneRadius;
            float _PulseSpeed;
            float _DotSize;
            
            v2f vert(appdata v)
            {
                v2f o;
                
                // Project onto ground plane (y = 0 in world space)
                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                float3 objectCenter = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
                
                // Flatten to ground
                worldPos.y = 0.01; // Slightly above ground to avoid z-fighting
                
                o.pos = mul(UNITY_MATRIX_VP, worldPos);
                o.worldPosXZ = worldPos.xz;
                o.objectPosXZ = objectCenter.xz;
                
                return o;
            }
            
            float HalftoneDot(float2 uv, float2 center, float radius)
            {
                float dist = length(uv - center);
                return smoothstep(radius, radius * 0.8, dist);
            }
            
            float HalftonePattern(float2 worldPos, float2 objectCenter)
            {
                float2 uv = worldPos - objectCenter;
                float distFromCenter = length(uv);
                
                float pulse = sin(_Time.y * _PulseSpeed) * 0.3 + 1.0;
                float maxRadius = _HalftoneRadius * pulse;
                
                if (distFromCenter > maxRadius)
                    return 0.0;
                
                float2 gridPos = uv * _HalftoneScale;
                float2 gridCell = floor(gridPos);
                float2 gridUV = frac(gridPos);
                
                if (fmod(gridCell.y, 2.0) > 0.5)
                    gridUV.x += 0.5;
                gridUV.x = frac(gridUV.x);
                
                float distanceScale = 1.0 - (distFromCenter / maxRadius);
                distanceScale = pow(distanceScale, 1.2);
                
                float dotPulse = sin(_Time.y * _PulseSpeed * 1.5) * 0.15 + 0.85;
                float dotSize = _DotSize * distanceScale * dotPulse;
                
                float dot = HalftoneDot(gridUV, float2(0.5, 0.5), dotSize);
                dot *= distanceScale;
                
                return dot;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                float halftone = HalftonePattern(i.worldPosXZ, i.objectPosXZ);
                float3 color = _GlowColor.rgb * halftone * _GlowIntensity * 2.0; // Doubled intensity
                
                return fixed4(color, halftone * 0.9); // Increased alpha
            }
            ENDCG
        }
    }
    
    FallBack "Diffuse"
}
