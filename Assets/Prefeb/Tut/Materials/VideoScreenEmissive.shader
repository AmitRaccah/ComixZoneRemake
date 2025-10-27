Shader "Custom/VideoScreenEmissive"
{
    Properties
    {
        _MainTex ("Video Texture", 2D) = "white" {}
        _Brightness ("Brightness", Range(0, 2)) = 1
        _EmissionStrength ("Emission Strength", Range(0, 10)) = 3
        _EmissionThreshold ("Emission Threshold", Range(0, 1)) = 0.2
        _Color ("Screen Tint", Color) = (1,1,1,1)
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        
        sampler2D _MainTex;
        float _Brightness;
        float _EmissionStrength;
        float _EmissionThreshold;
        fixed4 _Color;
        
        struct Input
        {
            float2 uv_MainTex;
        };
        
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Sample video texture
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            
            // Calculate luminance (brightness) of this pixel
            float luminance = dot(c.rgb, float3(0.299, 0.587, 0.114));
            
            // Base color
            o.Albedo = c.rgb * _Brightness;
            
            // Emission based on brightness
            // Brighter parts of video = more emission
            float emissionMask = saturate((luminance - _EmissionThreshold) / (1.0 - _EmissionThreshold));
            emissionMask = pow(emissionMask, 1.5); // Make it more dramatic
            
            // Emit the original color, scaled by brightness
            o.Emission = c.rgb * emissionMask * _EmissionStrength;
            
            o.Metallic = 0;
            o.Smoothness = 0.5;
            o.Alpha = c.a;
        }
        ENDCG
    }
    
    FallBack "Diffuse"
}
