Shader "Custom/BrightGroundGlow"
{
    Properties
    {
        _Color ("Glow Color", Color) = (1, 1, 0, 1)
        _Intensity ("Intensity", Float) = 5
        _Scale ("Dot Scale", Float) = 20
        _Speed ("Pulse Speed", Float) = 2
    }
    
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 200
        
        Blend One One
        ZWrite Off
        Cull Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile_fog
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
                UNITY_FOG_COORDS(1)
            };
            
            float4 _Color;
            float _Intensity;
            float _Scale;
            float _Speed;
            
            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }
            
            float circle(float2 p, float r)
            {
                return smoothstep(r, r * 0.7, length(p));
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                // Center UV
                float2 uv = i.uv - 0.5;
                float dist = length(uv);
                
                // Pulsing radius
                float pulse = sin(_Time.y * _Speed) * 0.25 + 1.0;
                
                // Fade at edges
                float edgeFade = 1.0 - smoothstep(0.35, 0.5, dist / pulse);
                if (edgeFade <= 0.0) return fixed4(0, 0, 0, 0);
                
                // Grid
                float2 grid = uv * _Scale * 2.0;
                float2 id = floor(grid);
                float2 gv = frac(grid) - 0.5;
                
                // Hex pattern
                float hexOffset = step(0.5, frac(id.y * 0.5));
                gv.x += hexOffset * 0.5;
                gv.x = frac(gv.x + 0.5) - 0.5;
                
                // Dot size based on distance
                float sizeFade = 1.0 - (dist / 0.45);
                sizeFade = pow(saturate(sizeFade), 0.6);
                
                // Pulsing dots
                float dotPulse = sin(_Time.y * _Speed * 1.2) * 0.15 + 0.85;
                float dotSize = 0.25 * sizeFade * dotPulse * pulse;
                
                // Draw dot
                float dot = circle(gv, dotSize);
                
                // Apply fades
                dot *= sizeFade * edgeFade;
                
                // Bright color
                float3 col = _Color.rgb * dot * _Intensity;
                
                fixed4 finalColor = fixed4(col, 1);
                UNITY_APPLY_FOG(i.fogCoord, finalColor);
                return finalColor;
            }
            ENDCG
        }
    }
    
    FallBack "Particles/Additive"
}
