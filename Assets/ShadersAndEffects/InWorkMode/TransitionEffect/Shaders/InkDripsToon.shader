Shader "Shader Graphs/InkToonBase"
{
    Properties
    {
        _AmbientStrength("Ambient Strength", Range(0, 0.2)) = 0.02
        _DiffuseColor("Diffuse Color", Color) = (1, 1, 1, 1)
        [NoScaleOffset]_MainTex("Diffuse Texture", 2D) = "white" {}
        _Step("Step", Float) = 3
        _SpecWidth("SpecWidth", Range(0.06, 0.12)) = 0.06
        _SpecPower("SpecPower", Range(48, 96)) = 48
        _SpecIntensity("SpecIntensity", Range(0.7, 1.2)) = 0.7
        [HDR]_HighlightColor("HighlightColor", Color) = (0.9150943, 0.9150943, 0.9150943, 0)
        _YBase("YBase", Float) = 1
        _YHeight("YHeight", Float) = 2
        _DripDensity("DripDensity", Float) = 4.5
        _DripSpeed("DripSpeed", Float) = 0.6
        _TrailWidth("TrailWidth", Float) = 0.06
        _DropSize("DropSize", Float) = 0.09
        [NoScaleOffset]_NoiseTex("NoiseTex", 2D) = "white" {}
        _NoiseScale("NoiseScale", Float) = 3.5
        _WettDark("WettDark", Range(0, 0.85)) = 0
        _WetBoost("WetBoost", Float) = 0.8
        [HideInInspector]_QueueOffset("_QueueOffset", Float) = 0
        [HideInInspector]_QueueControl("_QueueControl", Float) = -1
        [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "UniversalMaterialType" = "Lit"
            "Queue"="Transparent"
            "DisableBatching"="False"
            "ShaderGraphShader"="true"
            "ShaderGraphTargetId"="UniversalLitSubTarget"
        }
        Pass
        {
            Name "Universal Forward"
            Tags
            {
                "LightMode" = "UniversalForward"
            }
        
        // Render State
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite Off
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma multi_compile_instancing
        #pragma multi_compile_fog
        #pragma instancing_options renderinglayer
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
        #pragma multi_compile _ LIGHTMAP_ON
        #pragma multi_compile _ DYNAMICLIGHTMAP_ON
        #pragma multi_compile _ DIRLIGHTMAP_COMBINED
        #pragma multi_compile _ USE_LEGACY_LIGHTMAPS
        #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
        #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
        #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
        #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
        #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
        #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
        #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
        #pragma multi_compile _ SHADOWS_SHADOWMASK
        #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
        #pragma multi_compile_fragment _ _LIGHT_LAYERS
        #pragma multi_compile_fragment _ DEBUG_DISPLAY
        #pragma multi_compile_fragment _ _LIGHT_COOKIES
        #pragma multi_compile _ _FORWARD_PLUS
        #pragma multi_compile _ EVALUATE_SH_MIXED EVALUATE_SH_VERTEX
        // GraphKeywords: <None>
        
        // Defines
        
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_TEXCOORD1
        #define ATTRIBUTES_NEED_TEXCOORD2
        #define FEATURES_GRAPH_VERTEX_NORMAL_OUTPUT
        #define FEATURES_GRAPH_VERTEX_TANGENT_OUTPUT
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TANGENT_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
        #define VARYINGS_NEED_SHADOW_COORD
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_FORWARD
        #define _FOG_FRAGMENT 1
        #define _SURFACE_TYPE_TRANSPARENT 1
        #define _RECEIVE_SHADOWS_OFF 1
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ProbeVolumeVariants.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 uv1 : TEXCOORD1;
             float4 uv2 : TEXCOORD2;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float3 normalWS;
             float4 tangentWS;
             float4 texCoord0;
            #if defined(LIGHTMAP_ON)
             float2 staticLightmapUV;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
             float2 dynamicLightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
             float3 sh;
            #endif
            #if defined(USE_APV_PROBE_OCCLUSION)
             float4 probeOcclusion;
            #endif
             float4 fogFactorAndVertexLight;
            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
             float4 shadowCoord;
            #endif
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 WorldSpaceNormal;
             float3 TangentSpaceNormal;
             float3 WorldSpaceViewDirection;
             float3 WorldSpacePosition;
             float4 uv0;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
            #if defined(LIGHTMAP_ON)
             float2 staticLightmapUV : INTERP0;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
             float2 dynamicLightmapUV : INTERP1;
            #endif
            #if !defined(LIGHTMAP_ON)
             float3 sh : INTERP2;
            #endif
            #if defined(USE_APV_PROBE_OCCLUSION)
             float4 probeOcclusion : INTERP3;
            #endif
            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
             float4 shadowCoord : INTERP4;
            #endif
             float4 tangentWS : INTERP5;
             float4 texCoord0 : INTERP6;
             float4 fogFactorAndVertexLight : INTERP7;
             float3 positionWS : INTERP8;
             float3 normalWS : INTERP9;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            #if defined(LIGHTMAP_ON)
            output.staticLightmapUV = input.staticLightmapUV;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
            output.dynamicLightmapUV = input.dynamicLightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.sh = input.sh;
            #endif
            #if defined(USE_APV_PROBE_OCCLUSION)
            output.probeOcclusion = input.probeOcclusion;
            #endif
            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
            output.shadowCoord = input.shadowCoord;
            #endif
            output.tangentWS.xyzw = input.tangentWS;
            output.texCoord0.xyzw = input.texCoord0;
            output.fogFactorAndVertexLight.xyzw = input.fogFactorAndVertexLight;
            output.positionWS.xyz = input.positionWS;
            output.normalWS.xyz = input.normalWS;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            #if defined(LIGHTMAP_ON)
            output.staticLightmapUV = input.staticLightmapUV;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
            output.dynamicLightmapUV = input.dynamicLightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.sh = input.sh;
            #endif
            #if defined(USE_APV_PROBE_OCCLUSION)
            output.probeOcclusion = input.probeOcclusion;
            #endif
            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
            output.shadowCoord = input.shadowCoord;
            #endif
            output.tangentWS = input.tangentWS.xyzw;
            output.texCoord0 = input.texCoord0.xyzw;
            output.fogFactorAndVertexLight = input.fogFactorAndVertexLight.xyzw;
            output.positionWS = input.positionWS.xyz;
            output.normalWS = input.normalWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float _AmbientStrength;
        float4 _DiffuseColor;
        float4 _MainTex_TexelSize;
        float _Step;
        float _SpecPower;
        float _SpecIntensity;
        float _SpecWidth;
        float4 _HighlightColor;
        float _YHeight;
        float _DripDensity;
        float _DripSpeed;
        float _TrailWidth;
        float _DropSize;
        float _NoiseScale;
        float _YBase;
        float4 _NoiseTex_TexelSize;
        float _WettDark;
        float _WetBoost;
        UNITY_TEXTURE_STREAMING_DEBUG_VARS;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_NoiseTex);
        SAMPLER(sampler_NoiseTex);
        float _LerpB;
        float _LerpA;
        
        // Graph Includes
        #include_with_pragmas "Assets/ShadersAndEffects/InWorkMode/HLSL and more/Lighting.hlsl"
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Saturate_float(float In, out float Out)
        {
            Out = saturate(In);
        }
        
        void Unity_Lerp_float(float A, float B, float T, out float Out)
        {
            Out = lerp(A, B, T);
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
        Out = A * B;
        }
        
        struct Bindings_GetMainLight_d6b14a2e8b6f3554b8459648535f697e_float
        {
        };
        
        void SG_GetMainLight_d6b14a2e8b6f3554b8459648535f697e_float(float3 Vector3_b21c75b9b8514ef286d5e6dc199fa9af, Bindings_GetMainLight_d6b14a2e8b6f3554b8459648535f697e_float IN, out float3 Direction_0, out float3 Color_1, out float Attenuation_2)
        {
        float3 _Property_923162a64885457196b5ccbf7a2aaac7_Out_0_Vector3 = Vector3_b21c75b9b8514ef286d5e6dc199fa9af;
        float3 _MainLightCustomFunction_93e39954c04146b5ae1be272ea9d714b_Direction_0_Vector3;
        float3 _MainLightCustomFunction_93e39954c04146b5ae1be272ea9d714b_Color_2_Vector3;
        float _MainLightCustomFunction_93e39954c04146b5ae1be272ea9d714b_DistanceAtten_3_Float;
        float _MainLightCustomFunction_93e39954c04146b5ae1be272ea9d714b_ShadowAtten_4_Float;
        MainLight_float(_Property_923162a64885457196b5ccbf7a2aaac7_Out_0_Vector3, _MainLightCustomFunction_93e39954c04146b5ae1be272ea9d714b_Direction_0_Vector3, _MainLightCustomFunction_93e39954c04146b5ae1be272ea9d714b_Color_2_Vector3, _MainLightCustomFunction_93e39954c04146b5ae1be272ea9d714b_DistanceAtten_3_Float, _MainLightCustomFunction_93e39954c04146b5ae1be272ea9d714b_ShadowAtten_4_Float);
        float _Multiply_6d878674c9b447478a9564442340c0a2_Out_2_Float;
        Unity_Multiply_float_float(_MainLightCustomFunction_93e39954c04146b5ae1be272ea9d714b_DistanceAtten_3_Float, _MainLightCustomFunction_93e39954c04146b5ae1be272ea9d714b_ShadowAtten_4_Float, _Multiply_6d878674c9b447478a9564442340c0a2_Out_2_Float);
        Direction_0 = _MainLightCustomFunction_93e39954c04146b5ae1be272ea9d714b_Direction_0_Vector3;
        Color_1 = _MainLightCustomFunction_93e39954c04146b5ae1be272ea9d714b_Color_2_Vector3;
        Attenuation_2 = _Multiply_6d878674c9b447478a9564442340c0a2_Out_2_Float;
        }
        
        void Unity_DotProduct_float3(float3 A, float3 B, out float Out)
        {
            Out = dot(A, B);
        }
        
        void Unity_Floor_float(float In, out float Out)
        {
            Out = floor(In);
        }
        
        void Unity_Divide_float(float A, float B, out float Out)
        {
            Out = A / B;
        }
        
        void Unity_Maximum_float(float A, float B, out float Out)
        {
            Out = max(A, B);
        }
        
        struct Bindings_GetAdditionalLight_b5516a5008f7d104abebe27210c42de8_float
        {
        };
        
        void SG_GetAdditionalLight_b5516a5008f7d104abebe27210c42de8_float(float3 Vector3_1e07dec0084a48e38c95166c3cdc688d, float Vector1_0ce9574e837f408991312a6c71473833, Bindings_GetAdditionalLight_b5516a5008f7d104abebe27210c42de8_float IN, out float3 Direction_0, out float3 Color_1, out float Attenuation_2)
        {
        float3 _Property_e9927ccc4a684aeabdaa70dbe76689f0_Out_0_Vector3 = Vector3_1e07dec0084a48e38c95166c3cdc688d;
        float _Property_c6fa454cb94449b6923e1c90fafaf929_Out_0_Float = Vector1_0ce9574e837f408991312a6c71473833;
        float3 _AdditionalLightCustomFunction_c6ec52ed99634926b12dc96d48e96f1b_Direction_1_Vector3;
        float3 _AdditionalLightCustomFunction_c6ec52ed99634926b12dc96d48e96f1b_Color_2_Vector3;
        float _AdditionalLightCustomFunction_c6ec52ed99634926b12dc96d48e96f1b_DistanceAtten_3_Float;
        float _AdditionalLightCustomFunction_c6ec52ed99634926b12dc96d48e96f1b_ShadowAtten_4_Float;
        AdditionalLight_float(_Property_e9927ccc4a684aeabdaa70dbe76689f0_Out_0_Vector3, _Property_c6fa454cb94449b6923e1c90fafaf929_Out_0_Float, _AdditionalLightCustomFunction_c6ec52ed99634926b12dc96d48e96f1b_Direction_1_Vector3, _AdditionalLightCustomFunction_c6ec52ed99634926b12dc96d48e96f1b_Color_2_Vector3, _AdditionalLightCustomFunction_c6ec52ed99634926b12dc96d48e96f1b_DistanceAtten_3_Float, _AdditionalLightCustomFunction_c6ec52ed99634926b12dc96d48e96f1b_ShadowAtten_4_Float);
        float _Multiply_0b8ec7bbb926441e8c14c7c32d369adf_Out_2_Float;
        Unity_Multiply_float_float(_AdditionalLightCustomFunction_c6ec52ed99634926b12dc96d48e96f1b_DistanceAtten_3_Float, _AdditionalLightCustomFunction_c6ec52ed99634926b12dc96d48e96f1b_ShadowAtten_4_Float, _Multiply_0b8ec7bbb926441e8c14c7c32d369adf_Out_2_Float);
        Direction_0 = _AdditionalLightCustomFunction_c6ec52ed99634926b12dc96d48e96f1b_Direction_1_Vector3;
        Color_1 = _AdditionalLightCustomFunction_c6ec52ed99634926b12dc96d48e96f1b_Color_2_Vector3;
        Attenuation_2 = _Multiply_0b8ec7bbb926441e8c14c7c32d369adf_Out_2_Float;
        }
        
        void Unity_Multiply_float3_float3(float3 A, float3 B, out float3 Out)
        {
        Out = A * B;
        }
        
        void Unity_Add_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A + B;
        }
        
        struct Bindings_CalcAdditionalDiffuse_e85823578a3493d41ade631af12166aa_float
        {
        float3 WorldSpaceNormal;
        float3 WorldSpacePosition;
        };
        
        void SG_CalcAdditionalDiffuse_e85823578a3493d41ade631af12166aa_float(Bindings_CalcAdditionalDiffuse_e85823578a3493d41ade631af12166aa_float IN, out float3 Diffuse_1)
        {
        Bindings_GetAdditionalLight_b5516a5008f7d104abebe27210c42de8_float _GetAdditionalLight_e6c10dfa199649fca18a0cb673fcba44;
        float3 _GetAdditionalLight_e6c10dfa199649fca18a0cb673fcba44_Direction_0_Vector3;
        float3 _GetAdditionalLight_e6c10dfa199649fca18a0cb673fcba44_Color_1_Vector3;
        float _GetAdditionalLight_e6c10dfa199649fca18a0cb673fcba44_Attenuation_2_Float;
        SG_GetAdditionalLight_b5516a5008f7d104abebe27210c42de8_float(IN.WorldSpacePosition, float(0), _GetAdditionalLight_e6c10dfa199649fca18a0cb673fcba44, _GetAdditionalLight_e6c10dfa199649fca18a0cb673fcba44_Direction_0_Vector3, _GetAdditionalLight_e6c10dfa199649fca18a0cb673fcba44_Color_1_Vector3, _GetAdditionalLight_e6c10dfa199649fca18a0cb673fcba44_Attenuation_2_Float);
        float _DotProduct_aee5f4b498024e2da9796ed342f8a43a_Out_2_Float;
        Unity_DotProduct_float3(_GetAdditionalLight_e6c10dfa199649fca18a0cb673fcba44_Direction_0_Vector3, IN.WorldSpaceNormal, _DotProduct_aee5f4b498024e2da9796ed342f8a43a_Out_2_Float);
        float _Saturate_2080d664f6004dc9ad574ba7e91917b2_Out_1_Float;
        Unity_Saturate_float(_DotProduct_aee5f4b498024e2da9796ed342f8a43a_Out_2_Float, _Saturate_2080d664f6004dc9ad574ba7e91917b2_Out_1_Float);
        float3 _Multiply_57fcc7dc0fb845cf924c855eadbb7792_Out_2_Vector3;
        Unity_Multiply_float3_float3((_Saturate_2080d664f6004dc9ad574ba7e91917b2_Out_1_Float.xxx), _GetAdditionalLight_e6c10dfa199649fca18a0cb673fcba44_Color_1_Vector3, _Multiply_57fcc7dc0fb845cf924c855eadbb7792_Out_2_Vector3);
        float3 _Multiply_59016ca5433045d0a06ff03a13ec13b6_Out_2_Vector3;
        Unity_Multiply_float3_float3(_Multiply_57fcc7dc0fb845cf924c855eadbb7792_Out_2_Vector3, (_GetAdditionalLight_e6c10dfa199649fca18a0cb673fcba44_Attenuation_2_Float.xxx), _Multiply_59016ca5433045d0a06ff03a13ec13b6_Out_2_Vector3);
        Bindings_GetAdditionalLight_b5516a5008f7d104abebe27210c42de8_float _GetAdditionalLight_a054a4cd5b484e1492208dc9ca4a85f5;
        float3 _GetAdditionalLight_a054a4cd5b484e1492208dc9ca4a85f5_Direction_0_Vector3;
        float3 _GetAdditionalLight_a054a4cd5b484e1492208dc9ca4a85f5_Color_1_Vector3;
        float _GetAdditionalLight_a054a4cd5b484e1492208dc9ca4a85f5_Attenuation_2_Float;
        SG_GetAdditionalLight_b5516a5008f7d104abebe27210c42de8_float(IN.WorldSpacePosition, float(1), _GetAdditionalLight_a054a4cd5b484e1492208dc9ca4a85f5, _GetAdditionalLight_a054a4cd5b484e1492208dc9ca4a85f5_Direction_0_Vector3, _GetAdditionalLight_a054a4cd5b484e1492208dc9ca4a85f5_Color_1_Vector3, _GetAdditionalLight_a054a4cd5b484e1492208dc9ca4a85f5_Attenuation_2_Float);
        float _DotProduct_00e581a08ce7465588597fd11859eea2_Out_2_Float;
        Unity_DotProduct_float3(_GetAdditionalLight_a054a4cd5b484e1492208dc9ca4a85f5_Direction_0_Vector3, IN.WorldSpaceNormal, _DotProduct_00e581a08ce7465588597fd11859eea2_Out_2_Float);
        float _Saturate_58b5f4d3dcaf45e6bda212ef43edc4ea_Out_1_Float;
        Unity_Saturate_float(_DotProduct_00e581a08ce7465588597fd11859eea2_Out_2_Float, _Saturate_58b5f4d3dcaf45e6bda212ef43edc4ea_Out_1_Float);
        float3 _Multiply_6ff1cb7ecc8542848d8d721f284a3b3c_Out_2_Vector3;
        Unity_Multiply_float3_float3((_Saturate_58b5f4d3dcaf45e6bda212ef43edc4ea_Out_1_Float.xxx), _GetAdditionalLight_a054a4cd5b484e1492208dc9ca4a85f5_Color_1_Vector3, _Multiply_6ff1cb7ecc8542848d8d721f284a3b3c_Out_2_Vector3);
        float3 _Multiply_8337e9ec92b74cefb421776f67b29661_Out_2_Vector3;
        Unity_Multiply_float3_float3(_Multiply_6ff1cb7ecc8542848d8d721f284a3b3c_Out_2_Vector3, (_GetAdditionalLight_a054a4cd5b484e1492208dc9ca4a85f5_Attenuation_2_Float.xxx), _Multiply_8337e9ec92b74cefb421776f67b29661_Out_2_Vector3);
        float3 _Add_8d5fe4674e364518a5d8f4559669df92_Out_2_Vector3;
        Unity_Add_float3(_Multiply_59016ca5433045d0a06ff03a13ec13b6_Out_2_Vector3, _Multiply_8337e9ec92b74cefb421776f67b29661_Out_2_Vector3, _Add_8d5fe4674e364518a5d8f4559669df92_Out_2_Vector3);
        Bindings_GetAdditionalLight_b5516a5008f7d104abebe27210c42de8_float _GetAdditionalLight_ee2152ac3ed64138932b3a2e4b8e3f14;
        float3 _GetAdditionalLight_ee2152ac3ed64138932b3a2e4b8e3f14_Direction_0_Vector3;
        float3 _GetAdditionalLight_ee2152ac3ed64138932b3a2e4b8e3f14_Color_1_Vector3;
        float _GetAdditionalLight_ee2152ac3ed64138932b3a2e4b8e3f14_Attenuation_2_Float;
        SG_GetAdditionalLight_b5516a5008f7d104abebe27210c42de8_float(IN.WorldSpacePosition, float(2), _GetAdditionalLight_ee2152ac3ed64138932b3a2e4b8e3f14, _GetAdditionalLight_ee2152ac3ed64138932b3a2e4b8e3f14_Direction_0_Vector3, _GetAdditionalLight_ee2152ac3ed64138932b3a2e4b8e3f14_Color_1_Vector3, _GetAdditionalLight_ee2152ac3ed64138932b3a2e4b8e3f14_Attenuation_2_Float);
        float _DotProduct_7a3fce316c4a453e9b9e18226fc2d066_Out_2_Float;
        Unity_DotProduct_float3(_GetAdditionalLight_ee2152ac3ed64138932b3a2e4b8e3f14_Direction_0_Vector3, IN.WorldSpaceNormal, _DotProduct_7a3fce316c4a453e9b9e18226fc2d066_Out_2_Float);
        float _Saturate_c9919732b14f4622aa82e1eec5cb383d_Out_1_Float;
        Unity_Saturate_float(_DotProduct_7a3fce316c4a453e9b9e18226fc2d066_Out_2_Float, _Saturate_c9919732b14f4622aa82e1eec5cb383d_Out_1_Float);
        float3 _Multiply_013e04c47fdf4f46a7f2a4868ec83bed_Out_2_Vector3;
        Unity_Multiply_float3_float3((_Saturate_c9919732b14f4622aa82e1eec5cb383d_Out_1_Float.xxx), _GetAdditionalLight_ee2152ac3ed64138932b3a2e4b8e3f14_Color_1_Vector3, _Multiply_013e04c47fdf4f46a7f2a4868ec83bed_Out_2_Vector3);
        float3 _Multiply_8d16da0d689d4d49909909e6c2cc2da6_Out_2_Vector3;
        Unity_Multiply_float3_float3(_Multiply_013e04c47fdf4f46a7f2a4868ec83bed_Out_2_Vector3, (_GetAdditionalLight_ee2152ac3ed64138932b3a2e4b8e3f14_Attenuation_2_Float.xxx), _Multiply_8d16da0d689d4d49909909e6c2cc2da6_Out_2_Vector3);
        Bindings_GetAdditionalLight_b5516a5008f7d104abebe27210c42de8_float _GetAdditionalLight_da7753016bd64b489b897349d7f3fc0f;
        float3 _GetAdditionalLight_da7753016bd64b489b897349d7f3fc0f_Direction_0_Vector3;
        float3 _GetAdditionalLight_da7753016bd64b489b897349d7f3fc0f_Color_1_Vector3;
        float _GetAdditionalLight_da7753016bd64b489b897349d7f3fc0f_Attenuation_2_Float;
        SG_GetAdditionalLight_b5516a5008f7d104abebe27210c42de8_float(IN.WorldSpacePosition, float(3), _GetAdditionalLight_da7753016bd64b489b897349d7f3fc0f, _GetAdditionalLight_da7753016bd64b489b897349d7f3fc0f_Direction_0_Vector3, _GetAdditionalLight_da7753016bd64b489b897349d7f3fc0f_Color_1_Vector3, _GetAdditionalLight_da7753016bd64b489b897349d7f3fc0f_Attenuation_2_Float);
        float _DotProduct_be3ed23b71d142e3a88f9f21ca2b9bb1_Out_2_Float;
        Unity_DotProduct_float3(_GetAdditionalLight_da7753016bd64b489b897349d7f3fc0f_Direction_0_Vector3, IN.WorldSpaceNormal, _DotProduct_be3ed23b71d142e3a88f9f21ca2b9bb1_Out_2_Float);
        float _Saturate_b3794fe44470430e954d43bb481a3d9a_Out_1_Float;
        Unity_Saturate_float(_DotProduct_be3ed23b71d142e3a88f9f21ca2b9bb1_Out_2_Float, _Saturate_b3794fe44470430e954d43bb481a3d9a_Out_1_Float);
        float3 _Multiply_fcf9bc551f0e42f39634ac95fab7cae5_Out_2_Vector3;
        Unity_Multiply_float3_float3((_Saturate_b3794fe44470430e954d43bb481a3d9a_Out_1_Float.xxx), _GetAdditionalLight_da7753016bd64b489b897349d7f3fc0f_Color_1_Vector3, _Multiply_fcf9bc551f0e42f39634ac95fab7cae5_Out_2_Vector3);
        float3 _Multiply_0cc8c00775354d6a9f91977c3e357ff8_Out_2_Vector3;
        Unity_Multiply_float3_float3(_Multiply_fcf9bc551f0e42f39634ac95fab7cae5_Out_2_Vector3, (_GetAdditionalLight_da7753016bd64b489b897349d7f3fc0f_Attenuation_2_Float.xxx), _Multiply_0cc8c00775354d6a9f91977c3e357ff8_Out_2_Vector3);
        float3 _Add_0992646b9a764f6eb220517d0e9fc815_Out_2_Vector3;
        Unity_Add_float3(_Multiply_8d16da0d689d4d49909909e6c2cc2da6_Out_2_Vector3, _Multiply_0cc8c00775354d6a9f91977c3e357ff8_Out_2_Vector3, _Add_0992646b9a764f6eb220517d0e9fc815_Out_2_Vector3);
        float3 _Add_d239c6ae017b45cba8fda7bfa29dc880_Out_2_Vector3;
        Unity_Add_float3(_Add_8d5fe4674e364518a5d8f4559669df92_Out_2_Vector3, _Add_0992646b9a764f6eb220517d0e9fc815_Out_2_Vector3, _Add_d239c6ae017b45cba8fda7bfa29dc880_Out_2_Vector3);
        Diffuse_1 = _Add_d239c6ae017b45cba8fda7bfa29dc880_Out_2_Vector3;
        }
        
        void Unity_OneMinus_float(float In, out float Out)
        {
            Out = 1 - In;
        }
        
        void Unity_Normalize_float3(float3 In, out float3 Out)
        {
            Out = normalize(In);
        }
        
        void Unity_Power_float(float A, float B, out float Out)
        {
            Out = pow(A, B);
        }
        
        void Unity_Smoothstep_float(float Edge1, float Edge2, float In, out float Out)
        {
            Out = smoothstep(Edge1, Edge2, In);
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float3 BaseColor;
            float3 NormalTS;
            float3 Emission;
            float Metallic;
            float Smoothness;
            float Occlusion;
            float Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Property_58ffbe2cdb974e6eb51f08eb2773b719_Out_0_Float = _WettDark;
            float _Saturate_9a6eaf9061454bb68af056e5c611fc32_Out_1_Float;
            Unity_Saturate_float(float(0), _Saturate_9a6eaf9061454bb68af056e5c611fc32_Out_1_Float);
            float _Lerp_07deac0a40074d41b62045f5fb57c3f6_Out_3_Float;
            Unity_Lerp_float(float(1), _Property_58ffbe2cdb974e6eb51f08eb2773b719_Out_0_Float, _Saturate_9a6eaf9061454bb68af056e5c611fc32_Out_1_Float, _Lerp_07deac0a40074d41b62045f5fb57c3f6_Out_3_Float);
            Bindings_GetMainLight_d6b14a2e8b6f3554b8459648535f697e_float _GetMainLight_66f3629103c443e29b0bf77e7318459a;
            float3 _GetMainLight_66f3629103c443e29b0bf77e7318459a_Direction_0_Vector3;
            float3 _GetMainLight_66f3629103c443e29b0bf77e7318459a_Color_1_Vector3;
            float _GetMainLight_66f3629103c443e29b0bf77e7318459a_Attenuation_2_Float;
            SG_GetMainLight_d6b14a2e8b6f3554b8459648535f697e_float(float3 (0, 0, 0), _GetMainLight_66f3629103c443e29b0bf77e7318459a, _GetMainLight_66f3629103c443e29b0bf77e7318459a_Direction_0_Vector3, _GetMainLight_66f3629103c443e29b0bf77e7318459a_Color_1_Vector3, _GetMainLight_66f3629103c443e29b0bf77e7318459a_Attenuation_2_Float);
            float4 _Property_5bce803f7b1342118efa64dfd7a5c5af_Out_0_Vector4 = _DiffuseColor;
            Bindings_GetMainLight_d6b14a2e8b6f3554b8459648535f697e_float _GetMainLight_41b37094e05a429f8266b6602caf8242;
            float3 _GetMainLight_41b37094e05a429f8266b6602caf8242_Direction_0_Vector3;
            float3 _GetMainLight_41b37094e05a429f8266b6602caf8242_Color_1_Vector3;
            float _GetMainLight_41b37094e05a429f8266b6602caf8242_Attenuation_2_Float;
            SG_GetMainLight_d6b14a2e8b6f3554b8459648535f697e_float(IN.WorldSpacePosition, _GetMainLight_41b37094e05a429f8266b6602caf8242, _GetMainLight_41b37094e05a429f8266b6602caf8242_Direction_0_Vector3, _GetMainLight_41b37094e05a429f8266b6602caf8242_Color_1_Vector3, _GetMainLight_41b37094e05a429f8266b6602caf8242_Attenuation_2_Float);
            float _DotProduct_8d4bdec92ac948559d5a94572dd8cb4d_Out_2_Float;
            Unity_DotProduct_float3(IN.WorldSpaceNormal, _GetMainLight_41b37094e05a429f8266b6602caf8242_Direction_0_Vector3, _DotProduct_8d4bdec92ac948559d5a94572dd8cb4d_Out_2_Float);
            float _Saturate_568447dc078f4cdaa9df59ff3d9456d3_Out_1_Float;
            Unity_Saturate_float(_DotProduct_8d4bdec92ac948559d5a94572dd8cb4d_Out_2_Float, _Saturate_568447dc078f4cdaa9df59ff3d9456d3_Out_1_Float);
            float _Property_2e6e3ebe260e41cda8b744c9ef183653_Out_0_Float = _Step;
            float _Multiply_89ea6b707c044fb5a105c8ecc0f663cc_Out_2_Float;
            Unity_Multiply_float_float(_Saturate_568447dc078f4cdaa9df59ff3d9456d3_Out_1_Float, _Property_2e6e3ebe260e41cda8b744c9ef183653_Out_0_Float, _Multiply_89ea6b707c044fb5a105c8ecc0f663cc_Out_2_Float);
            float _Floor_a3099d1c1e92444786c2d86607f308d1_Out_1_Float;
            Unity_Floor_float(_Multiply_89ea6b707c044fb5a105c8ecc0f663cc_Out_2_Float, _Floor_a3099d1c1e92444786c2d86607f308d1_Out_1_Float);
            float _Divide_bf2c7eeba1dd479aaab53317c575a0ea_Out_2_Float;
            Unity_Divide_float(_Floor_a3099d1c1e92444786c2d86607f308d1_Out_1_Float, _Property_2e6e3ebe260e41cda8b744c9ef183653_Out_0_Float, _Divide_bf2c7eeba1dd479aaab53317c575a0ea_Out_2_Float);
            float _Property_68149cd9c2eb40a79d0105acc016d1c4_Out_0_Float = _AmbientStrength;
            float _Maximum_c73c7cbcf6c4463a865782d13a0c4101_Out_2_Float;
            Unity_Maximum_float(_Divide_bf2c7eeba1dd479aaab53317c575a0ea_Out_2_Float, _Property_68149cd9c2eb40a79d0105acc016d1c4_Out_0_Float, _Maximum_c73c7cbcf6c4463a865782d13a0c4101_Out_2_Float);
            Bindings_CalcAdditionalDiffuse_e85823578a3493d41ade631af12166aa_float _CalcAdditionalDiffuse_f61ebb0caffa49798669b455d28ddc2f;
            _CalcAdditionalDiffuse_f61ebb0caffa49798669b455d28ddc2f.WorldSpaceNormal = IN.WorldSpaceNormal;
            _CalcAdditionalDiffuse_f61ebb0caffa49798669b455d28ddc2f.WorldSpacePosition = IN.WorldSpacePosition;
            float3 _CalcAdditionalDiffuse_f61ebb0caffa49798669b455d28ddc2f_Diffuse_1_Vector3;
            SG_CalcAdditionalDiffuse_e85823578a3493d41ade631af12166aa_float(_CalcAdditionalDiffuse_f61ebb0caffa49798669b455d28ddc2f, _CalcAdditionalDiffuse_f61ebb0caffa49798669b455d28ddc2f_Diffuse_1_Vector3);
            float3 _Add_0f6ab824bf464526bbf3116b8d1e102f_Out_2_Vector3;
            Unity_Add_float3((_Maximum_c73c7cbcf6c4463a865782d13a0c4101_Out_2_Float.xxx), _CalcAdditionalDiffuse_f61ebb0caffa49798669b455d28ddc2f_Diffuse_1_Vector3, _Add_0f6ab824bf464526bbf3116b8d1e102f_Out_2_Vector3);
            float3 _Multiply_d622b247c4a84f8bbebf6a902f0b0fcb_Out_2_Vector3;
            Unity_Multiply_float3_float3((_Property_5bce803f7b1342118efa64dfd7a5c5af_Out_0_Vector4.xyz), _Add_0f6ab824bf464526bbf3116b8d1e102f_Out_2_Vector3, _Multiply_d622b247c4a84f8bbebf6a902f0b0fcb_Out_2_Vector3);
            UnityTexture2D _Property_a111b087fff64719b899025161b92d31_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_MainTex);
            float4 _SampleTexture2D_68405c1067a64b7f874afd25ede0e0c8_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_a111b087fff64719b899025161b92d31_Out_0_Texture2D.tex, _Property_a111b087fff64719b899025161b92d31_Out_0_Texture2D.samplerstate, _Property_a111b087fff64719b899025161b92d31_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_68405c1067a64b7f874afd25ede0e0c8_R_4_Float = _SampleTexture2D_68405c1067a64b7f874afd25ede0e0c8_RGBA_0_Vector4.r;
            float _SampleTexture2D_68405c1067a64b7f874afd25ede0e0c8_G_5_Float = _SampleTexture2D_68405c1067a64b7f874afd25ede0e0c8_RGBA_0_Vector4.g;
            float _SampleTexture2D_68405c1067a64b7f874afd25ede0e0c8_B_6_Float = _SampleTexture2D_68405c1067a64b7f874afd25ede0e0c8_RGBA_0_Vector4.b;
            float _SampleTexture2D_68405c1067a64b7f874afd25ede0e0c8_A_7_Float = _SampleTexture2D_68405c1067a64b7f874afd25ede0e0c8_RGBA_0_Vector4.a;
            float3 _Multiply_8ac74964cd004464a81865818d5afcc4_Out_2_Vector3;
            Unity_Multiply_float3_float3(_Multiply_d622b247c4a84f8bbebf6a902f0b0fcb_Out_2_Vector3, (_SampleTexture2D_68405c1067a64b7f874afd25ede0e0c8_RGBA_0_Vector4.xyz), _Multiply_8ac74964cd004464a81865818d5afcc4_Out_2_Vector3);
            float _Property_0e2d1f788f574a2f8b34d846bbd7263c_Out_0_Float = _SpecWidth;
            float _Multiply_c17e567eb6d844efb410cd9a33a05375_Out_2_Float;
            Unity_Multiply_float_float(_Property_0e2d1f788f574a2f8b34d846bbd7263c_Out_0_Float, 2, _Multiply_c17e567eb6d844efb410cd9a33a05375_Out_2_Float);
            float _OneMinus_d3c0cdcde3e14887b51750055ea97bb8_Out_1_Float;
            Unity_OneMinus_float(_Multiply_c17e567eb6d844efb410cd9a33a05375_Out_2_Float, _OneMinus_d3c0cdcde3e14887b51750055ea97bb8_Out_1_Float);
            float _OneMinus_a315df87ea3b4fdebec40e5a9022411a_Out_1_Float;
            Unity_OneMinus_float(_Property_0e2d1f788f574a2f8b34d846bbd7263c_Out_0_Float, _OneMinus_a315df87ea3b4fdebec40e5a9022411a_Out_1_Float);
            float3 _Normalize_048a70be3e7e48d79a6a6d5115dfa858_Out_1_Vector3;
            Unity_Normalize_float3(_GetMainLight_66f3629103c443e29b0bf77e7318459a_Direction_0_Vector3, _Normalize_048a70be3e7e48d79a6a6d5115dfa858_Out_1_Vector3);
            float3 _Normalize_d0e7859ef3784895af4ebfa092cea7a8_Out_1_Vector3;
            Unity_Normalize_float3(IN.WorldSpaceViewDirection, _Normalize_d0e7859ef3784895af4ebfa092cea7a8_Out_1_Vector3);
            float3 _Add_155e7c4d78f949c48564160410bc4048_Out_2_Vector3;
            Unity_Add_float3(_Normalize_048a70be3e7e48d79a6a6d5115dfa858_Out_1_Vector3, _Normalize_d0e7859ef3784895af4ebfa092cea7a8_Out_1_Vector3, _Add_155e7c4d78f949c48564160410bc4048_Out_2_Vector3);
            float3 _Normalize_47d11df4961b4fb8a702583e4b0a9fc7_Out_1_Vector3;
            Unity_Normalize_float3(_Add_155e7c4d78f949c48564160410bc4048_Out_2_Vector3, _Normalize_47d11df4961b4fb8a702583e4b0a9fc7_Out_1_Vector3);
            float _DotProduct_6141d4c6d0ac402596caf047be9dbc14_Out_2_Float;
            Unity_DotProduct_float3(IN.WorldSpaceNormal, _Normalize_47d11df4961b4fb8a702583e4b0a9fc7_Out_1_Vector3, _DotProduct_6141d4c6d0ac402596caf047be9dbc14_Out_2_Float);
            float _Saturate_6d73ca1db00946ebb16bc3ae45eec3e3_Out_1_Float;
            Unity_Saturate_float(_DotProduct_6141d4c6d0ac402596caf047be9dbc14_Out_2_Float, _Saturate_6d73ca1db00946ebb16bc3ae45eec3e3_Out_1_Float);
            float _Property_cffee28cd20846c294a39928e843e022_Out_0_Float = _SpecPower;
            float _Power_ed38d16ded9843b997ddef76e0b234c9_Out_2_Float;
            Unity_Power_float(_Saturate_6d73ca1db00946ebb16bc3ae45eec3e3_Out_1_Float, _Property_cffee28cd20846c294a39928e843e022_Out_0_Float, _Power_ed38d16ded9843b997ddef76e0b234c9_Out_2_Float);
            float _Smoothstep_1415711804e74f72af642ca650f88631_Out_3_Float;
            Unity_Smoothstep_float(_OneMinus_d3c0cdcde3e14887b51750055ea97bb8_Out_1_Float, _OneMinus_a315df87ea3b4fdebec40e5a9022411a_Out_1_Float, _Power_ed38d16ded9843b997ddef76e0b234c9_Out_2_Float, _Smoothstep_1415711804e74f72af642ca650f88631_Out_3_Float);
            float _Property_e24c9b5c87334203ae45a0f4524092cf_Out_0_Float = _SpecIntensity;
            float _Multiply_bd9542e81f61443c93d68d6effc8d884_Out_2_Float;
            Unity_Multiply_float_float(_Smoothstep_1415711804e74f72af642ca650f88631_Out_3_Float, _Property_e24c9b5c87334203ae45a0f4524092cf_Out_0_Float, _Multiply_bd9542e81f61443c93d68d6effc8d884_Out_2_Float);
            float4 _Property_17f80f6fd3d648edbac72436c2bf4f2d_Out_0_Vector4 = IsGammaSpace() ? LinearToSRGB(_HighlightColor) : _HighlightColor;
            float4 _Multiply_fb7a787882f34251bf81a170c440b704_Out_2_Vector4;
            Unity_Multiply_float4_float4((_Multiply_bd9542e81f61443c93d68d6effc8d884_Out_2_Float.xxxx), _Property_17f80f6fd3d648edbac72436c2bf4f2d_Out_0_Vector4, _Multiply_fb7a787882f34251bf81a170c440b704_Out_2_Vector4);
            float3 _Add_6b9391937ee345d693bb8ca62983e59c_Out_2_Vector3;
            Unity_Add_float3(_Multiply_8ac74964cd004464a81865818d5afcc4_Out_2_Vector3, (_Multiply_fb7a787882f34251bf81a170c440b704_Out_2_Vector4.xyz), _Add_6b9391937ee345d693bb8ca62983e59c_Out_2_Vector3);
            float3 _Multiply_9ead612961534f23adf829877ef2f3da_Out_2_Vector3;
            Unity_Multiply_float3_float3(_GetMainLight_66f3629103c443e29b0bf77e7318459a_Color_1_Vector3, _Add_6b9391937ee345d693bb8ca62983e59c_Out_2_Vector3, _Multiply_9ead612961534f23adf829877ef2f3da_Out_2_Vector3);
            float3 _Multiply_96dcb3634feb420a982106a37e9fb39f_Out_2_Vector3;
            Unity_Multiply_float3_float3(_Multiply_9ead612961534f23adf829877ef2f3da_Out_2_Vector3, (_GetMainLight_66f3629103c443e29b0bf77e7318459a_Attenuation_2_Float.xxx), _Multiply_96dcb3634feb420a982106a37e9fb39f_Out_2_Vector3);
            float3 _Multiply_9f64f4291cc242d993083fefef541c9e_Out_2_Vector3;
            Unity_Multiply_float3_float3((_Lerp_07deac0a40074d41b62045f5fb57c3f6_Out_3_Float.xxx), _Multiply_96dcb3634feb420a982106a37e9fb39f_Out_2_Vector3, _Multiply_9f64f4291cc242d993083fefef541c9e_Out_2_Vector3);
            float _Property_e7835e37fa084c21b3fe49ad42fed15e_Out_0_Float = _WetBoost;
            float _Multiply_3841a77e87a5481d90edb0ddd7665098_Out_2_Float;
            Unity_Multiply_float_float(_Saturate_9a6eaf9061454bb68af056e5c611fc32_Out_1_Float, _Property_e7835e37fa084c21b3fe49ad42fed15e_Out_0_Float, _Multiply_3841a77e87a5481d90edb0ddd7665098_Out_2_Float);
            float _Add_52bbe1adc27b4277bf2662761ef4207c_Out_2_Float;
            Unity_Add_float(float(1), _Multiply_3841a77e87a5481d90edb0ddd7665098_Out_2_Float, _Add_52bbe1adc27b4277bf2662761ef4207c_Out_2_Float);
            float3 _Multiply_f5424bb1ec264175bae4da44f3dc0f2f_Out_2_Vector3;
            Unity_Multiply_float3_float3((_Add_52bbe1adc27b4277bf2662761ef4207c_Out_2_Float.xxx), _Multiply_96dcb3634feb420a982106a37e9fb39f_Out_2_Vector3, _Multiply_f5424bb1ec264175bae4da44f3dc0f2f_Out_2_Vector3);
            float3 _Add_ade479e6605d417d9ff3e0e1696825dd_Out_2_Vector3;
            Unity_Add_float3(_Multiply_9f64f4291cc242d993083fefef541c9e_Out_2_Vector3, _Multiply_f5424bb1ec264175bae4da44f3dc0f2f_Out_2_Vector3, _Add_ade479e6605d417d9ff3e0e1696825dd_Out_2_Vector3);
            surface.BaseColor = IsGammaSpace() ? float3(0, 0, 0) : SRGBToLinear(float3(0, 0, 0));
            surface.NormalTS = IN.TangentSpaceNormal;
            surface.Emission = _Add_ade479e6605d417d9ff3e0e1696825dd_Out_2_Vector3;
            surface.Metallic = float(0);
            surface.Smoothness = float(0);
            surface.Occlusion = float(1);
            surface.Alpha = float(1);
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
            // must use interpolated tangent, bitangent and normal before they are normalized in the pixel shader.
            float3 unnormalizedNormalWS = input.normalWS;
            const float renormFactor = 1.0 / length(unnormalizedNormalWS);
        
        
            output.WorldSpaceNormal = renormFactor * input.normalWS.xyz;      // we want a unit length Normal Vector node in shader graph
            output.TangentSpaceNormal = float3(0.0f, 0.0f, 1.0f);
        
        
            output.WorldSpaceViewDirection = GetWorldSpaceNormalizeViewDir(input.positionWS);
            output.WorldSpacePosition = input.positionWS;
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
            output.uv0 = input.texCoord0;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBRForwardPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "GBuffer"
            Tags
            {
                "LightMode" = "UniversalGBuffer"
            }
        
        // Render State
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite Off
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 4.5
        #pragma exclude_renderers gles3 glcore
        #pragma multi_compile_instancing
        #pragma multi_compile_fog
        #pragma instancing_options renderinglayer
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        #pragma multi_compile _ LIGHTMAP_ON
        #pragma multi_compile _ DYNAMICLIGHTMAP_ON
        #pragma multi_compile _ DIRLIGHTMAP_COMBINED
        #pragma multi_compile _ USE_LEGACY_LIGHTMAPS
        #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
        #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
        #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
        #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
        #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
        #pragma multi_compile _ SHADOWS_SHADOWMASK
        #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
        #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
        #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
        #pragma multi_compile_fragment _ _RENDER_PASS_ENABLED
        #pragma multi_compile_fragment _ DEBUG_DISPLAY
        // GraphKeywords: <None>
        
        // Defines
        
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_TEXCOORD1
        #define ATTRIBUTES_NEED_TEXCOORD2
        #define FEATURES_GRAPH_VERTEX_NORMAL_OUTPUT
        #define FEATURES_GRAPH_VERTEX_TANGENT_OUTPUT
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TANGENT_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
        #define VARYINGS_NEED_SHADOW_COORD
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_GBUFFER
        #define _FOG_FRAGMENT 1
        #define _SURFACE_TYPE_TRANSPARENT 1
        #define _RECEIVE_SHADOWS_OFF 1
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ProbeVolumeVariants.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 uv1 : TEXCOORD1;
             float4 uv2 : TEXCOORD2;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float3 normalWS;
             float4 tangentWS;
             float4 texCoord0;
            #if defined(LIGHTMAP_ON)
             float2 staticLightmapUV;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
             float2 dynamicLightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
             float3 sh;
            #endif
            #if defined(USE_APV_PROBE_OCCLUSION)
             float4 probeOcclusion;
            #endif
             float4 fogFactorAndVertexLight;
            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
             float4 shadowCoord;
            #endif
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 WorldSpaceNormal;
             float3 TangentSpaceNormal;
             float3 WorldSpaceViewDirection;
             float3 WorldSpacePosition;
             float4 uv0;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
            #if defined(LIGHTMAP_ON)
             float2 staticLightmapUV : INTERP0;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
             float2 dynamicLightmapUV : INTERP1;
            #endif
            #if !defined(LIGHTMAP_ON)
             float3 sh : INTERP2;
            #endif
            #if defined(USE_APV_PROBE_OCCLUSION)
             float4 probeOcclusion : INTERP3;
            #endif
            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
             float4 shadowCoord : INTERP4;
            #endif
             float4 tangentWS : INTERP5;
             float4 texCoord0 : INTERP6;
             float4 fogFactorAndVertexLight : INTERP7;
             float3 positionWS : INTERP8;
             float3 normalWS : INTERP9;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            #if defined(LIGHTMAP_ON)
            output.staticLightmapUV = input.staticLightmapUV;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
            output.dynamicLightmapUV = input.dynamicLightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.sh = input.sh;
            #endif
            #if defined(USE_APV_PROBE_OCCLUSION)
            output.probeOcclusion = input.probeOcclusion;
            #endif
            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
            output.shadowCoord = input.shadowCoord;
            #endif
            output.tangentWS.xyzw = input.tangentWS;
            output.texCoord0.xyzw = input.texCoord0;
            output.fogFactorAndVertexLight.xyzw = input.fogFactorAndVertexLight;
            output.positionWS.xyz = input.positionWS;
            output.normalWS.xyz = input.normalWS;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            #if defined(LIGHTMAP_ON)
            output.staticLightmapUV = input.staticLightmapUV;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
            output.dynamicLightmapUV = input.dynamicLightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.sh = input.sh;
            #endif
            #if defined(USE_APV_PROBE_OCCLUSION)
            output.probeOcclusion = input.probeOcclusion;
            #endif
            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
            output.shadowCoord = input.shadowCoord;
            #endif
            output.tangentWS = input.tangentWS.xyzw;
            output.texCoord0 = input.texCoord0.xyzw;
            output.fogFactorAndVertexLight = input.fogFactorAndVertexLight.xyzw;
            output.positionWS = input.positionWS.xyz;
            output.normalWS = input.normalWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float _AmbientStrength;
        float4 _DiffuseColor;
        float4 _MainTex_TexelSize;
        float _Step;
        float _SpecPower;
        float _SpecIntensity;
        float _SpecWidth;
        float4 _HighlightColor;
        float _YHeight;
        float _DripDensity;
        float _DripSpeed;
        float _TrailWidth;
        float _DropSize;
        float _NoiseScale;
        float _YBase;
        float4 _NoiseTex_TexelSize;
        float _WettDark;
        float _WetBoost;
        UNITY_TEXTURE_STREAMING_DEBUG_VARS;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_NoiseTex);
        SAMPLER(sampler_NoiseTex);
        float _LerpB;
        float _LerpA;
        
        // Graph Includes
        #include_with_pragmas "Assets/ShadersAndEffects/InWorkMode/HLSL and more/Lighting.hlsl"
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Saturate_float(float In, out float Out)
        {
            Out = saturate(In);
        }
        
        void Unity_Lerp_float(float A, float B, float T, out float Out)
        {
            Out = lerp(A, B, T);
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
        Out = A * B;
        }
        
        struct Bindings_GetMainLight_d6b14a2e8b6f3554b8459648535f697e_float
        {
        };
        
        void SG_GetMainLight_d6b14a2e8b6f3554b8459648535f697e_float(float3 Vector3_b21c75b9b8514ef286d5e6dc199fa9af, Bindings_GetMainLight_d6b14a2e8b6f3554b8459648535f697e_float IN, out float3 Direction_0, out float3 Color_1, out float Attenuation_2)
        {
        float3 _Property_923162a64885457196b5ccbf7a2aaac7_Out_0_Vector3 = Vector3_b21c75b9b8514ef286d5e6dc199fa9af;
        float3 _MainLightCustomFunction_93e39954c04146b5ae1be272ea9d714b_Direction_0_Vector3;
        float3 _MainLightCustomFunction_93e39954c04146b5ae1be272ea9d714b_Color_2_Vector3;
        float _MainLightCustomFunction_93e39954c04146b5ae1be272ea9d714b_DistanceAtten_3_Float;
        float _MainLightCustomFunction_93e39954c04146b5ae1be272ea9d714b_ShadowAtten_4_Float;
        MainLight_float(_Property_923162a64885457196b5ccbf7a2aaac7_Out_0_Vector3, _MainLightCustomFunction_93e39954c04146b5ae1be272ea9d714b_Direction_0_Vector3, _MainLightCustomFunction_93e39954c04146b5ae1be272ea9d714b_Color_2_Vector3, _MainLightCustomFunction_93e39954c04146b5ae1be272ea9d714b_DistanceAtten_3_Float, _MainLightCustomFunction_93e39954c04146b5ae1be272ea9d714b_ShadowAtten_4_Float);
        float _Multiply_6d878674c9b447478a9564442340c0a2_Out_2_Float;
        Unity_Multiply_float_float(_MainLightCustomFunction_93e39954c04146b5ae1be272ea9d714b_DistanceAtten_3_Float, _MainLightCustomFunction_93e39954c04146b5ae1be272ea9d714b_ShadowAtten_4_Float, _Multiply_6d878674c9b447478a9564442340c0a2_Out_2_Float);
        Direction_0 = _MainLightCustomFunction_93e39954c04146b5ae1be272ea9d714b_Direction_0_Vector3;
        Color_1 = _MainLightCustomFunction_93e39954c04146b5ae1be272ea9d714b_Color_2_Vector3;
        Attenuation_2 = _Multiply_6d878674c9b447478a9564442340c0a2_Out_2_Float;
        }
        
        void Unity_DotProduct_float3(float3 A, float3 B, out float Out)
        {
            Out = dot(A, B);
        }
        
        void Unity_Floor_float(float In, out float Out)
        {
            Out = floor(In);
        }
        
        void Unity_Divide_float(float A, float B, out float Out)
        {
            Out = A / B;
        }
        
        void Unity_Maximum_float(float A, float B, out float Out)
        {
            Out = max(A, B);
        }
        
        struct Bindings_GetAdditionalLight_b5516a5008f7d104abebe27210c42de8_float
        {
        };
        
        void SG_GetAdditionalLight_b5516a5008f7d104abebe27210c42de8_float(float3 Vector3_1e07dec0084a48e38c95166c3cdc688d, float Vector1_0ce9574e837f408991312a6c71473833, Bindings_GetAdditionalLight_b5516a5008f7d104abebe27210c42de8_float IN, out float3 Direction_0, out float3 Color_1, out float Attenuation_2)
        {
        float3 _Property_e9927ccc4a684aeabdaa70dbe76689f0_Out_0_Vector3 = Vector3_1e07dec0084a48e38c95166c3cdc688d;
        float _Property_c6fa454cb94449b6923e1c90fafaf929_Out_0_Float = Vector1_0ce9574e837f408991312a6c71473833;
        float3 _AdditionalLightCustomFunction_c6ec52ed99634926b12dc96d48e96f1b_Direction_1_Vector3;
        float3 _AdditionalLightCustomFunction_c6ec52ed99634926b12dc96d48e96f1b_Color_2_Vector3;
        float _AdditionalLightCustomFunction_c6ec52ed99634926b12dc96d48e96f1b_DistanceAtten_3_Float;
        float _AdditionalLightCustomFunction_c6ec52ed99634926b12dc96d48e96f1b_ShadowAtten_4_Float;
        AdditionalLight_float(_Property_e9927ccc4a684aeabdaa70dbe76689f0_Out_0_Vector3, _Property_c6fa454cb94449b6923e1c90fafaf929_Out_0_Float, _AdditionalLightCustomFunction_c6ec52ed99634926b12dc96d48e96f1b_Direction_1_Vector3, _AdditionalLightCustomFunction_c6ec52ed99634926b12dc96d48e96f1b_Color_2_Vector3, _AdditionalLightCustomFunction_c6ec52ed99634926b12dc96d48e96f1b_DistanceAtten_3_Float, _AdditionalLightCustomFunction_c6ec52ed99634926b12dc96d48e96f1b_ShadowAtten_4_Float);
        float _Multiply_0b8ec7bbb926441e8c14c7c32d369adf_Out_2_Float;
        Unity_Multiply_float_float(_AdditionalLightCustomFunction_c6ec52ed99634926b12dc96d48e96f1b_DistanceAtten_3_Float, _AdditionalLightCustomFunction_c6ec52ed99634926b12dc96d48e96f1b_ShadowAtten_4_Float, _Multiply_0b8ec7bbb926441e8c14c7c32d369adf_Out_2_Float);
        Direction_0 = _AdditionalLightCustomFunction_c6ec52ed99634926b12dc96d48e96f1b_Direction_1_Vector3;
        Color_1 = _AdditionalLightCustomFunction_c6ec52ed99634926b12dc96d48e96f1b_Color_2_Vector3;
        Attenuation_2 = _Multiply_0b8ec7bbb926441e8c14c7c32d369adf_Out_2_Float;
        }
        
        void Unity_Multiply_float3_float3(float3 A, float3 B, out float3 Out)
        {
        Out = A * B;
        }
        
        void Unity_Add_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A + B;
        }
        
        struct Bindings_CalcAdditionalDiffuse_e85823578a3493d41ade631af12166aa_float
        {
        float3 WorldSpaceNormal;
        float3 WorldSpacePosition;
        };
        
        void SG_CalcAdditionalDiffuse_e85823578a3493d41ade631af12166aa_float(Bindings_CalcAdditionalDiffuse_e85823578a3493d41ade631af12166aa_float IN, out float3 Diffuse_1)
        {
        Bindings_GetAdditionalLight_b5516a5008f7d104abebe27210c42de8_float _GetAdditionalLight_e6c10dfa199649fca18a0cb673fcba44;
        float3 _GetAdditionalLight_e6c10dfa199649fca18a0cb673fcba44_Direction_0_Vector3;
        float3 _GetAdditionalLight_e6c10dfa199649fca18a0cb673fcba44_Color_1_Vector3;
        float _GetAdditionalLight_e6c10dfa199649fca18a0cb673fcba44_Attenuation_2_Float;
        SG_GetAdditionalLight_b5516a5008f7d104abebe27210c42de8_float(IN.WorldSpacePosition, float(0), _GetAdditionalLight_e6c10dfa199649fca18a0cb673fcba44, _GetAdditionalLight_e6c10dfa199649fca18a0cb673fcba44_Direction_0_Vector3, _GetAdditionalLight_e6c10dfa199649fca18a0cb673fcba44_Color_1_Vector3, _GetAdditionalLight_e6c10dfa199649fca18a0cb673fcba44_Attenuation_2_Float);
        float _DotProduct_aee5f4b498024e2da9796ed342f8a43a_Out_2_Float;
        Unity_DotProduct_float3(_GetAdditionalLight_e6c10dfa199649fca18a0cb673fcba44_Direction_0_Vector3, IN.WorldSpaceNormal, _DotProduct_aee5f4b498024e2da9796ed342f8a43a_Out_2_Float);
        float _Saturate_2080d664f6004dc9ad574ba7e91917b2_Out_1_Float;
        Unity_Saturate_float(_DotProduct_aee5f4b498024e2da9796ed342f8a43a_Out_2_Float, _Saturate_2080d664f6004dc9ad574ba7e91917b2_Out_1_Float);
        float3 _Multiply_57fcc7dc0fb845cf924c855eadbb7792_Out_2_Vector3;
        Unity_Multiply_float3_float3((_Saturate_2080d664f6004dc9ad574ba7e91917b2_Out_1_Float.xxx), _GetAdditionalLight_e6c10dfa199649fca18a0cb673fcba44_Color_1_Vector3, _Multiply_57fcc7dc0fb845cf924c855eadbb7792_Out_2_Vector3);
        float3 _Multiply_59016ca5433045d0a06ff03a13ec13b6_Out_2_Vector3;
        Unity_Multiply_float3_float3(_Multiply_57fcc7dc0fb845cf924c855eadbb7792_Out_2_Vector3, (_GetAdditionalLight_e6c10dfa199649fca18a0cb673fcba44_Attenuation_2_Float.xxx), _Multiply_59016ca5433045d0a06ff03a13ec13b6_Out_2_Vector3);
        Bindings_GetAdditionalLight_b5516a5008f7d104abebe27210c42de8_float _GetAdditionalLight_a054a4cd5b484e1492208dc9ca4a85f5;
        float3 _GetAdditionalLight_a054a4cd5b484e1492208dc9ca4a85f5_Direction_0_Vector3;
        float3 _GetAdditionalLight_a054a4cd5b484e1492208dc9ca4a85f5_Color_1_Vector3;
        float _GetAdditionalLight_a054a4cd5b484e1492208dc9ca4a85f5_Attenuation_2_Float;
        SG_GetAdditionalLight_b5516a5008f7d104abebe27210c42de8_float(IN.WorldSpacePosition, float(1), _GetAdditionalLight_a054a4cd5b484e1492208dc9ca4a85f5, _GetAdditionalLight_a054a4cd5b484e1492208dc9ca4a85f5_Direction_0_Vector3, _GetAdditionalLight_a054a4cd5b484e1492208dc9ca4a85f5_Color_1_Vector3, _GetAdditionalLight_a054a4cd5b484e1492208dc9ca4a85f5_Attenuation_2_Float);
        float _DotProduct_00e581a08ce7465588597fd11859eea2_Out_2_Float;
        Unity_DotProduct_float3(_GetAdditionalLight_a054a4cd5b484e1492208dc9ca4a85f5_Direction_0_Vector3, IN.WorldSpaceNormal, _DotProduct_00e581a08ce7465588597fd11859eea2_Out_2_Float);
        float _Saturate_58b5f4d3dcaf45e6bda212ef43edc4ea_Out_1_Float;
        Unity_Saturate_float(_DotProduct_00e581a08ce7465588597fd11859eea2_Out_2_Float, _Saturate_58b5f4d3dcaf45e6bda212ef43edc4ea_Out_1_Float);
        float3 _Multiply_6ff1cb7ecc8542848d8d721f284a3b3c_Out_2_Vector3;
        Unity_Multiply_float3_float3((_Saturate_58b5f4d3dcaf45e6bda212ef43edc4ea_Out_1_Float.xxx), _GetAdditionalLight_a054a4cd5b484e1492208dc9ca4a85f5_Color_1_Vector3, _Multiply_6ff1cb7ecc8542848d8d721f284a3b3c_Out_2_Vector3);
        float3 _Multiply_8337e9ec92b74cefb421776f67b29661_Out_2_Vector3;
        Unity_Multiply_float3_float3(_Multiply_6ff1cb7ecc8542848d8d721f284a3b3c_Out_2_Vector3, (_GetAdditionalLight_a054a4cd5b484e1492208dc9ca4a85f5_Attenuation_2_Float.xxx), _Multiply_8337e9ec92b74cefb421776f67b29661_Out_2_Vector3);
        float3 _Add_8d5fe4674e364518a5d8f4559669df92_Out_2_Vector3;
        Unity_Add_float3(_Multiply_59016ca5433045d0a06ff03a13ec13b6_Out_2_Vector3, _Multiply_8337e9ec92b74cefb421776f67b29661_Out_2_Vector3, _Add_8d5fe4674e364518a5d8f4559669df92_Out_2_Vector3);
        Bindings_GetAdditionalLight_b5516a5008f7d104abebe27210c42de8_float _GetAdditionalLight_ee2152ac3ed64138932b3a2e4b8e3f14;
        float3 _GetAdditionalLight_ee2152ac3ed64138932b3a2e4b8e3f14_Direction_0_Vector3;
        float3 _GetAdditionalLight_ee2152ac3ed64138932b3a2e4b8e3f14_Color_1_Vector3;
        float _GetAdditionalLight_ee2152ac3ed64138932b3a2e4b8e3f14_Attenuation_2_Float;
        SG_GetAdditionalLight_b5516a5008f7d104abebe27210c42de8_float(IN.WorldSpacePosition, float(2), _GetAdditionalLight_ee2152ac3ed64138932b3a2e4b8e3f14, _GetAdditionalLight_ee2152ac3ed64138932b3a2e4b8e3f14_Direction_0_Vector3, _GetAdditionalLight_ee2152ac3ed64138932b3a2e4b8e3f14_Color_1_Vector3, _GetAdditionalLight_ee2152ac3ed64138932b3a2e4b8e3f14_Attenuation_2_Float);
        float _DotProduct_7a3fce316c4a453e9b9e18226fc2d066_Out_2_Float;
        Unity_DotProduct_float3(_GetAdditionalLight_ee2152ac3ed64138932b3a2e4b8e3f14_Direction_0_Vector3, IN.WorldSpaceNormal, _DotProduct_7a3fce316c4a453e9b9e18226fc2d066_Out_2_Float);
        float _Saturate_c9919732b14f4622aa82e1eec5cb383d_Out_1_Float;
        Unity_Saturate_float(_DotProduct_7a3fce316c4a453e9b9e18226fc2d066_Out_2_Float, _Saturate_c9919732b14f4622aa82e1eec5cb383d_Out_1_Float);
        float3 _Multiply_013e04c47fdf4f46a7f2a4868ec83bed_Out_2_Vector3;
        Unity_Multiply_float3_float3((_Saturate_c9919732b14f4622aa82e1eec5cb383d_Out_1_Float.xxx), _GetAdditionalLight_ee2152ac3ed64138932b3a2e4b8e3f14_Color_1_Vector3, _Multiply_013e04c47fdf4f46a7f2a4868ec83bed_Out_2_Vector3);
        float3 _Multiply_8d16da0d689d4d49909909e6c2cc2da6_Out_2_Vector3;
        Unity_Multiply_float3_float3(_Multiply_013e04c47fdf4f46a7f2a4868ec83bed_Out_2_Vector3, (_GetAdditionalLight_ee2152ac3ed64138932b3a2e4b8e3f14_Attenuation_2_Float.xxx), _Multiply_8d16da0d689d4d49909909e6c2cc2da6_Out_2_Vector3);
        Bindings_GetAdditionalLight_b5516a5008f7d104abebe27210c42de8_float _GetAdditionalLight_da7753016bd64b489b897349d7f3fc0f;
        float3 _GetAdditionalLight_da7753016bd64b489b897349d7f3fc0f_Direction_0_Vector3;
        float3 _GetAdditionalLight_da7753016bd64b489b897349d7f3fc0f_Color_1_Vector3;
        float _GetAdditionalLight_da7753016bd64b489b897349d7f3fc0f_Attenuation_2_Float;
        SG_GetAdditionalLight_b5516a5008f7d104abebe27210c42de8_float(IN.WorldSpacePosition, float(3), _GetAdditionalLight_da7753016bd64b489b897349d7f3fc0f, _GetAdditionalLight_da7753016bd64b489b897349d7f3fc0f_Direction_0_Vector3, _GetAdditionalLight_da7753016bd64b489b897349d7f3fc0f_Color_1_Vector3, _GetAdditionalLight_da7753016bd64b489b897349d7f3fc0f_Attenuation_2_Float);
        float _DotProduct_be3ed23b71d142e3a88f9f21ca2b9bb1_Out_2_Float;
        Unity_DotProduct_float3(_GetAdditionalLight_da7753016bd64b489b897349d7f3fc0f_Direction_0_Vector3, IN.WorldSpaceNormal, _DotProduct_be3ed23b71d142e3a88f9f21ca2b9bb1_Out_2_Float);
        float _Saturate_b3794fe44470430e954d43bb481a3d9a_Out_1_Float;
        Unity_Saturate_float(_DotProduct_be3ed23b71d142e3a88f9f21ca2b9bb1_Out_2_Float, _Saturate_b3794fe44470430e954d43bb481a3d9a_Out_1_Float);
        float3 _Multiply_fcf9bc551f0e42f39634ac95fab7cae5_Out_2_Vector3;
        Unity_Multiply_float3_float3((_Saturate_b3794fe44470430e954d43bb481a3d9a_Out_1_Float.xxx), _GetAdditionalLight_da7753016bd64b489b897349d7f3fc0f_Color_1_Vector3, _Multiply_fcf9bc551f0e42f39634ac95fab7cae5_Out_2_Vector3);
        float3 _Multiply_0cc8c00775354d6a9f91977c3e357ff8_Out_2_Vector3;
        Unity_Multiply_float3_float3(_Multiply_fcf9bc551f0e42f39634ac95fab7cae5_Out_2_Vector3, (_GetAdditionalLight_da7753016bd64b489b897349d7f3fc0f_Attenuation_2_Float.xxx), _Multiply_0cc8c00775354d6a9f91977c3e357ff8_Out_2_Vector3);
        float3 _Add_0992646b9a764f6eb220517d0e9fc815_Out_2_Vector3;
        Unity_Add_float3(_Multiply_8d16da0d689d4d49909909e6c2cc2da6_Out_2_Vector3, _Multiply_0cc8c00775354d6a9f91977c3e357ff8_Out_2_Vector3, _Add_0992646b9a764f6eb220517d0e9fc815_Out_2_Vector3);
        float3 _Add_d239c6ae017b45cba8fda7bfa29dc880_Out_2_Vector3;
        Unity_Add_float3(_Add_8d5fe4674e364518a5d8f4559669df92_Out_2_Vector3, _Add_0992646b9a764f6eb220517d0e9fc815_Out_2_Vector3, _Add_d239c6ae017b45cba8fda7bfa29dc880_Out_2_Vector3);
        Diffuse_1 = _Add_d239c6ae017b45cba8fda7bfa29dc880_Out_2_Vector3;
        }
        
        void Unity_OneMinus_float(float In, out float Out)
        {
            Out = 1 - In;
        }
        
        void Unity_Normalize_float3(float3 In, out float3 Out)
        {
            Out = normalize(In);
        }
        
        void Unity_Power_float(float A, float B, out float Out)
        {
            Out = pow(A, B);
        }
        
        void Unity_Smoothstep_float(float Edge1, float Edge2, float In, out float Out)
        {
            Out = smoothstep(Edge1, Edge2, In);
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float3 BaseColor;
            float3 NormalTS;
            float3 Emission;
            float Metallic;
            float Smoothness;
            float Occlusion;
            float Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Property_58ffbe2cdb974e6eb51f08eb2773b719_Out_0_Float = _WettDark;
            float _Saturate_9a6eaf9061454bb68af056e5c611fc32_Out_1_Float;
            Unity_Saturate_float(float(0), _Saturate_9a6eaf9061454bb68af056e5c611fc32_Out_1_Float);
            float _Lerp_07deac0a40074d41b62045f5fb57c3f6_Out_3_Float;
            Unity_Lerp_float(float(1), _Property_58ffbe2cdb974e6eb51f08eb2773b719_Out_0_Float, _Saturate_9a6eaf9061454bb68af056e5c611fc32_Out_1_Float, _Lerp_07deac0a40074d41b62045f5fb57c3f6_Out_3_Float);
            Bindings_GetMainLight_d6b14a2e8b6f3554b8459648535f697e_float _GetMainLight_66f3629103c443e29b0bf77e7318459a;
            float3 _GetMainLight_66f3629103c443e29b0bf77e7318459a_Direction_0_Vector3;
            float3 _GetMainLight_66f3629103c443e29b0bf77e7318459a_Color_1_Vector3;
            float _GetMainLight_66f3629103c443e29b0bf77e7318459a_Attenuation_2_Float;
            SG_GetMainLight_d6b14a2e8b6f3554b8459648535f697e_float(float3 (0, 0, 0), _GetMainLight_66f3629103c443e29b0bf77e7318459a, _GetMainLight_66f3629103c443e29b0bf77e7318459a_Direction_0_Vector3, _GetMainLight_66f3629103c443e29b0bf77e7318459a_Color_1_Vector3, _GetMainLight_66f3629103c443e29b0bf77e7318459a_Attenuation_2_Float);
            float4 _Property_5bce803f7b1342118efa64dfd7a5c5af_Out_0_Vector4 = _DiffuseColor;
            Bindings_GetMainLight_d6b14a2e8b6f3554b8459648535f697e_float _GetMainLight_41b37094e05a429f8266b6602caf8242;
            float3 _GetMainLight_41b37094e05a429f8266b6602caf8242_Direction_0_Vector3;
            float3 _GetMainLight_41b37094e05a429f8266b6602caf8242_Color_1_Vector3;
            float _GetMainLight_41b37094e05a429f8266b6602caf8242_Attenuation_2_Float;
            SG_GetMainLight_d6b14a2e8b6f3554b8459648535f697e_float(IN.WorldSpacePosition, _GetMainLight_41b37094e05a429f8266b6602caf8242, _GetMainLight_41b37094e05a429f8266b6602caf8242_Direction_0_Vector3, _GetMainLight_41b37094e05a429f8266b6602caf8242_Color_1_Vector3, _GetMainLight_41b37094e05a429f8266b6602caf8242_Attenuation_2_Float);
            float _DotProduct_8d4bdec92ac948559d5a94572dd8cb4d_Out_2_Float;
            Unity_DotProduct_float3(IN.WorldSpaceNormal, _GetMainLight_41b37094e05a429f8266b6602caf8242_Direction_0_Vector3, _DotProduct_8d4bdec92ac948559d5a94572dd8cb4d_Out_2_Float);
            float _Saturate_568447dc078f4cdaa9df59ff3d9456d3_Out_1_Float;
            Unity_Saturate_float(_DotProduct_8d4bdec92ac948559d5a94572dd8cb4d_Out_2_Float, _Saturate_568447dc078f4cdaa9df59ff3d9456d3_Out_1_Float);
            float _Property_2e6e3ebe260e41cda8b744c9ef183653_Out_0_Float = _Step;
            float _Multiply_89ea6b707c044fb5a105c8ecc0f663cc_Out_2_Float;
            Unity_Multiply_float_float(_Saturate_568447dc078f4cdaa9df59ff3d9456d3_Out_1_Float, _Property_2e6e3ebe260e41cda8b744c9ef183653_Out_0_Float, _Multiply_89ea6b707c044fb5a105c8ecc0f663cc_Out_2_Float);
            float _Floor_a3099d1c1e92444786c2d86607f308d1_Out_1_Float;
            Unity_Floor_float(_Multiply_89ea6b707c044fb5a105c8ecc0f663cc_Out_2_Float, _Floor_a3099d1c1e92444786c2d86607f308d1_Out_1_Float);
            float _Divide_bf2c7eeba1dd479aaab53317c575a0ea_Out_2_Float;
            Unity_Divide_float(_Floor_a3099d1c1e92444786c2d86607f308d1_Out_1_Float, _Property_2e6e3ebe260e41cda8b744c9ef183653_Out_0_Float, _Divide_bf2c7eeba1dd479aaab53317c575a0ea_Out_2_Float);
            float _Property_68149cd9c2eb40a79d0105acc016d1c4_Out_0_Float = _AmbientStrength;
            float _Maximum_c73c7cbcf6c4463a865782d13a0c4101_Out_2_Float;
            Unity_Maximum_float(_Divide_bf2c7eeba1dd479aaab53317c575a0ea_Out_2_Float, _Property_68149cd9c2eb40a79d0105acc016d1c4_Out_0_Float, _Maximum_c73c7cbcf6c4463a865782d13a0c4101_Out_2_Float);
            Bindings_CalcAdditionalDiffuse_e85823578a3493d41ade631af12166aa_float _CalcAdditionalDiffuse_f61ebb0caffa49798669b455d28ddc2f;
            _CalcAdditionalDiffuse_f61ebb0caffa49798669b455d28ddc2f.WorldSpaceNormal = IN.WorldSpaceNormal;
            _CalcAdditionalDiffuse_f61ebb0caffa49798669b455d28ddc2f.WorldSpacePosition = IN.WorldSpacePosition;
            float3 _CalcAdditionalDiffuse_f61ebb0caffa49798669b455d28ddc2f_Diffuse_1_Vector3;
            SG_CalcAdditionalDiffuse_e85823578a3493d41ade631af12166aa_float(_CalcAdditionalDiffuse_f61ebb0caffa49798669b455d28ddc2f, _CalcAdditionalDiffuse_f61ebb0caffa49798669b455d28ddc2f_Diffuse_1_Vector3);
            float3 _Add_0f6ab824bf464526bbf3116b8d1e102f_Out_2_Vector3;
            Unity_Add_float3((_Maximum_c73c7cbcf6c4463a865782d13a0c4101_Out_2_Float.xxx), _CalcAdditionalDiffuse_f61ebb0caffa49798669b455d28ddc2f_Diffuse_1_Vector3, _Add_0f6ab824bf464526bbf3116b8d1e102f_Out_2_Vector3);
            float3 _Multiply_d622b247c4a84f8bbebf6a902f0b0fcb_Out_2_Vector3;
            Unity_Multiply_float3_float3((_Property_5bce803f7b1342118efa64dfd7a5c5af_Out_0_Vector4.xyz), _Add_0f6ab824bf464526bbf3116b8d1e102f_Out_2_Vector3, _Multiply_d622b247c4a84f8bbebf6a902f0b0fcb_Out_2_Vector3);
            UnityTexture2D _Property_a111b087fff64719b899025161b92d31_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_MainTex);
            float4 _SampleTexture2D_68405c1067a64b7f874afd25ede0e0c8_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_a111b087fff64719b899025161b92d31_Out_0_Texture2D.tex, _Property_a111b087fff64719b899025161b92d31_Out_0_Texture2D.samplerstate, _Property_a111b087fff64719b899025161b92d31_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_68405c1067a64b7f874afd25ede0e0c8_R_4_Float = _SampleTexture2D_68405c1067a64b7f874afd25ede0e0c8_RGBA_0_Vector4.r;
            float _SampleTexture2D_68405c1067a64b7f874afd25ede0e0c8_G_5_Float = _SampleTexture2D_68405c1067a64b7f874afd25ede0e0c8_RGBA_0_Vector4.g;
            float _SampleTexture2D_68405c1067a64b7f874afd25ede0e0c8_B_6_Float = _SampleTexture2D_68405c1067a64b7f874afd25ede0e0c8_RGBA_0_Vector4.b;
            float _SampleTexture2D_68405c1067a64b7f874afd25ede0e0c8_A_7_Float = _SampleTexture2D_68405c1067a64b7f874afd25ede0e0c8_RGBA_0_Vector4.a;
            float3 _Multiply_8ac74964cd004464a81865818d5afcc4_Out_2_Vector3;
            Unity_Multiply_float3_float3(_Multiply_d622b247c4a84f8bbebf6a902f0b0fcb_Out_2_Vector3, (_SampleTexture2D_68405c1067a64b7f874afd25ede0e0c8_RGBA_0_Vector4.xyz), _Multiply_8ac74964cd004464a81865818d5afcc4_Out_2_Vector3);
            float _Property_0e2d1f788f574a2f8b34d846bbd7263c_Out_0_Float = _SpecWidth;
            float _Multiply_c17e567eb6d844efb410cd9a33a05375_Out_2_Float;
            Unity_Multiply_float_float(_Property_0e2d1f788f574a2f8b34d846bbd7263c_Out_0_Float, 2, _Multiply_c17e567eb6d844efb410cd9a33a05375_Out_2_Float);
            float _OneMinus_d3c0cdcde3e14887b51750055ea97bb8_Out_1_Float;
            Unity_OneMinus_float(_Multiply_c17e567eb6d844efb410cd9a33a05375_Out_2_Float, _OneMinus_d3c0cdcde3e14887b51750055ea97bb8_Out_1_Float);
            float _OneMinus_a315df87ea3b4fdebec40e5a9022411a_Out_1_Float;
            Unity_OneMinus_float(_Property_0e2d1f788f574a2f8b34d846bbd7263c_Out_0_Float, _OneMinus_a315df87ea3b4fdebec40e5a9022411a_Out_1_Float);
            float3 _Normalize_048a70be3e7e48d79a6a6d5115dfa858_Out_1_Vector3;
            Unity_Normalize_float3(_GetMainLight_66f3629103c443e29b0bf77e7318459a_Direction_0_Vector3, _Normalize_048a70be3e7e48d79a6a6d5115dfa858_Out_1_Vector3);
            float3 _Normalize_d0e7859ef3784895af4ebfa092cea7a8_Out_1_Vector3;
            Unity_Normalize_float3(IN.WorldSpaceViewDirection, _Normalize_d0e7859ef3784895af4ebfa092cea7a8_Out_1_Vector3);
            float3 _Add_155e7c4d78f949c48564160410bc4048_Out_2_Vector3;
            Unity_Add_float3(_Normalize_048a70be3e7e48d79a6a6d5115dfa858_Out_1_Vector3, _Normalize_d0e7859ef3784895af4ebfa092cea7a8_Out_1_Vector3, _Add_155e7c4d78f949c48564160410bc4048_Out_2_Vector3);
            float3 _Normalize_47d11df4961b4fb8a702583e4b0a9fc7_Out_1_Vector3;
            Unity_Normalize_float3(_Add_155e7c4d78f949c48564160410bc4048_Out_2_Vector3, _Normalize_47d11df4961b4fb8a702583e4b0a9fc7_Out_1_Vector3);
            float _DotProduct_6141d4c6d0ac402596caf047be9dbc14_Out_2_Float;
            Unity_DotProduct_float3(IN.WorldSpaceNormal, _Normalize_47d11df4961b4fb8a702583e4b0a9fc7_Out_1_Vector3, _DotProduct_6141d4c6d0ac402596caf047be9dbc14_Out_2_Float);
            float _Saturate_6d73ca1db00946ebb16bc3ae45eec3e3_Out_1_Float;
            Unity_Saturate_float(_DotProduct_6141d4c6d0ac402596caf047be9dbc14_Out_2_Float, _Saturate_6d73ca1db00946ebb16bc3ae45eec3e3_Out_1_Float);
            float _Property_cffee28cd20846c294a39928e843e022_Out_0_Float = _SpecPower;
            float _Power_ed38d16ded9843b997ddef76e0b234c9_Out_2_Float;
            Unity_Power_float(_Saturate_6d73ca1db00946ebb16bc3ae45eec3e3_Out_1_Float, _Property_cffee28cd20846c294a39928e843e022_Out_0_Float, _Power_ed38d16ded9843b997ddef76e0b234c9_Out_2_Float);
            float _Smoothstep_1415711804e74f72af642ca650f88631_Out_3_Float;
            Unity_Smoothstep_float(_OneMinus_d3c0cdcde3e14887b51750055ea97bb8_Out_1_Float, _OneMinus_a315df87ea3b4fdebec40e5a9022411a_Out_1_Float, _Power_ed38d16ded9843b997ddef76e0b234c9_Out_2_Float, _Smoothstep_1415711804e74f72af642ca650f88631_Out_3_Float);
            float _Property_e24c9b5c87334203ae45a0f4524092cf_Out_0_Float = _SpecIntensity;
            float _Multiply_bd9542e81f61443c93d68d6effc8d884_Out_2_Float;
            Unity_Multiply_float_float(_Smoothstep_1415711804e74f72af642ca650f88631_Out_3_Float, _Property_e24c9b5c87334203ae45a0f4524092cf_Out_0_Float, _Multiply_bd9542e81f61443c93d68d6effc8d884_Out_2_Float);
            float4 _Property_17f80f6fd3d648edbac72436c2bf4f2d_Out_0_Vector4 = IsGammaSpace() ? LinearToSRGB(_HighlightColor) : _HighlightColor;
            float4 _Multiply_fb7a787882f34251bf81a170c440b704_Out_2_Vector4;
            Unity_Multiply_float4_float4((_Multiply_bd9542e81f61443c93d68d6effc8d884_Out_2_Float.xxxx), _Property_17f80f6fd3d648edbac72436c2bf4f2d_Out_0_Vector4, _Multiply_fb7a787882f34251bf81a170c440b704_Out_2_Vector4);
            float3 _Add_6b9391937ee345d693bb8ca62983e59c_Out_2_Vector3;
            Unity_Add_float3(_Multiply_8ac74964cd004464a81865818d5afcc4_Out_2_Vector3, (_Multiply_fb7a787882f34251bf81a170c440b704_Out_2_Vector4.xyz), _Add_6b9391937ee345d693bb8ca62983e59c_Out_2_Vector3);
            float3 _Multiply_9ead612961534f23adf829877ef2f3da_Out_2_Vector3;
            Unity_Multiply_float3_float3(_GetMainLight_66f3629103c443e29b0bf77e7318459a_Color_1_Vector3, _Add_6b9391937ee345d693bb8ca62983e59c_Out_2_Vector3, _Multiply_9ead612961534f23adf829877ef2f3da_Out_2_Vector3);
            float3 _Multiply_96dcb3634feb420a982106a37e9fb39f_Out_2_Vector3;
            Unity_Multiply_float3_float3(_Multiply_9ead612961534f23adf829877ef2f3da_Out_2_Vector3, (_GetMainLight_66f3629103c443e29b0bf77e7318459a_Attenuation_2_Float.xxx), _Multiply_96dcb3634feb420a982106a37e9fb39f_Out_2_Vector3);
            float3 _Multiply_9f64f4291cc242d993083fefef541c9e_Out_2_Vector3;
            Unity_Multiply_float3_float3((_Lerp_07deac0a40074d41b62045f5fb57c3f6_Out_3_Float.xxx), _Multiply_96dcb3634feb420a982106a37e9fb39f_Out_2_Vector3, _Multiply_9f64f4291cc242d993083fefef541c9e_Out_2_Vector3);
            float _Property_e7835e37fa084c21b3fe49ad42fed15e_Out_0_Float = _WetBoost;
            float _Multiply_3841a77e87a5481d90edb0ddd7665098_Out_2_Float;
            Unity_Multiply_float_float(_Saturate_9a6eaf9061454bb68af056e5c611fc32_Out_1_Float, _Property_e7835e37fa084c21b3fe49ad42fed15e_Out_0_Float, _Multiply_3841a77e87a5481d90edb0ddd7665098_Out_2_Float);
            float _Add_52bbe1adc27b4277bf2662761ef4207c_Out_2_Float;
            Unity_Add_float(float(1), _Multiply_3841a77e87a5481d90edb0ddd7665098_Out_2_Float, _Add_52bbe1adc27b4277bf2662761ef4207c_Out_2_Float);
            float3 _Multiply_f5424bb1ec264175bae4da44f3dc0f2f_Out_2_Vector3;
            Unity_Multiply_float3_float3((_Add_52bbe1adc27b4277bf2662761ef4207c_Out_2_Float.xxx), _Multiply_96dcb3634feb420a982106a37e9fb39f_Out_2_Vector3, _Multiply_f5424bb1ec264175bae4da44f3dc0f2f_Out_2_Vector3);
            float3 _Add_ade479e6605d417d9ff3e0e1696825dd_Out_2_Vector3;
            Unity_Add_float3(_Multiply_9f64f4291cc242d993083fefef541c9e_Out_2_Vector3, _Multiply_f5424bb1ec264175bae4da44f3dc0f2f_Out_2_Vector3, _Add_ade479e6605d417d9ff3e0e1696825dd_Out_2_Vector3);
            surface.BaseColor = IsGammaSpace() ? float3(0, 0, 0) : SRGBToLinear(float3(0, 0, 0));
            surface.NormalTS = IN.TangentSpaceNormal;
            surface.Emission = _Add_ade479e6605d417d9ff3e0e1696825dd_Out_2_Vector3;
            surface.Metallic = float(0);
            surface.Smoothness = float(0);
            surface.Occlusion = float(1);
            surface.Alpha = float(1);
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
            // must use interpolated tangent, bitangent and normal before they are normalized in the pixel shader.
            float3 unnormalizedNormalWS = input.normalWS;
            const float renormFactor = 1.0 / length(unnormalizedNormalWS);
        
        
            output.WorldSpaceNormal = renormFactor * input.normalWS.xyz;      // we want a unit length Normal Vector node in shader graph
            output.TangentSpaceNormal = float3(0.0f, 0.0f, 1.0f);
        
        
            output.WorldSpaceViewDirection = GetWorldSpaceNormalizeViewDir(input.positionWS);
            output.WorldSpacePosition = input.positionWS;
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
            output.uv0 = input.texCoord0;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityGBuffer.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBRGBufferPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "MotionVectors"
            Tags
            {
                "LightMode" = "MotionVectors"
            }
        
        // Render State
        Cull Off
        ZTest LEqual
        ZWrite On
        ColorMask RG
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 3.5
        #pragma multi_compile_instancing
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        // PassKeywords: <None>
        // GraphKeywords: <None>
        
        // Defines
        
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_MOTION_VECTORS
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float _AmbientStrength;
        float4 _DiffuseColor;
        float4 _MainTex_TexelSize;
        float _Step;
        float _SpecPower;
        float _SpecIntensity;
        float _SpecWidth;
        float4 _HighlightColor;
        float _YHeight;
        float _DripDensity;
        float _DripSpeed;
        float _TrailWidth;
        float _DropSize;
        float _NoiseScale;
        float _YBase;
        float4 _NoiseTex_TexelSize;
        float _WettDark;
        float _WetBoost;
        UNITY_TEXTURE_STREAMING_DEBUG_VARS;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_NoiseTex);
        SAMPLER(sampler_NoiseTex);
        float _LerpB;
        float _LerpA;
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        // GraphFunctions: <None>
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            surface.Alpha = float(1);
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpacePosition =                        input.positionOS;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/MotionVectorPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "DepthNormals"
            Tags
            {
                "LightMode" = "DepthNormals"
            }
        
        // Render State
        Cull Off
        ZTest LEqual
        ZWrite On
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma multi_compile_instancing
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        // PassKeywords: <None>
        // GraphKeywords: <None>
        
        // Defines
        
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD1
        #define FEATURES_GRAPH_VERTEX_NORMAL_OUTPUT
        #define FEATURES_GRAPH_VERTEX_TANGENT_OUTPUT
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TANGENT_WS
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_DEPTHNORMALS
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv1 : TEXCOORD1;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 normalWS;
             float4 tangentWS;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 TangentSpaceNormal;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 tangentWS : INTERP0;
             float3 normalWS : INTERP1;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.tangentWS.xyzw = input.tangentWS;
            output.normalWS.xyz = input.normalWS;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.tangentWS = input.tangentWS.xyzw;
            output.normalWS = input.normalWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float _AmbientStrength;
        float4 _DiffuseColor;
        float4 _MainTex_TexelSize;
        float _Step;
        float _SpecPower;
        float _SpecIntensity;
        float _SpecWidth;
        float4 _HighlightColor;
        float _YHeight;
        float _DripDensity;
        float _DripSpeed;
        float _TrailWidth;
        float _DropSize;
        float _NoiseScale;
        float _YBase;
        float4 _NoiseTex_TexelSize;
        float _WettDark;
        float _WetBoost;
        UNITY_TEXTURE_STREAMING_DEBUG_VARS;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_NoiseTex);
        SAMPLER(sampler_NoiseTex);
        float _LerpB;
        float _LerpA;
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        // GraphFunctions: <None>
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float3 NormalTS;
            float Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            surface.NormalTS = IN.TangentSpaceNormal;
            surface.Alpha = float(1);
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
            output.TangentSpaceNormal = float3(0.0f, 0.0f, 1.0f);
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/DepthNormalsOnlyPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "Meta"
            Tags
            {
                "LightMode" = "Meta"
            }
        
        // Render State
        Cull Off
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        #pragma shader_feature _ EDITOR_VISUALIZATION
        // GraphKeywords: <None>
        
        // Defines
        
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_TEXCOORD1
        #define ATTRIBUTES_NEED_TEXCOORD2
        #define ATTRIBUTES_NEED_INSTANCEID
        #define FEATURES_GRAPH_VERTEX_NORMAL_OUTPUT
        #define FEATURES_GRAPH_VERTEX_TANGENT_OUTPUT
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_TEXCOORD1
        #define VARYINGS_NEED_TEXCOORD2
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_META
        #define _FOG_FRAGMENT 1
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 uv1 : TEXCOORD1;
             float4 uv2 : TEXCOORD2;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float3 normalWS;
             float4 texCoord0;
             float4 texCoord1;
             float4 texCoord2;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 WorldSpaceNormal;
             float3 WorldSpaceViewDirection;
             float3 WorldSpacePosition;
             float4 uv0;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0 : INTERP0;
             float4 texCoord1 : INTERP1;
             float4 texCoord2 : INTERP2;
             float3 positionWS : INTERP3;
             float3 normalWS : INTERP4;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.texCoord0.xyzw = input.texCoord0;
            output.texCoord1.xyzw = input.texCoord1;
            output.texCoord2.xyzw = input.texCoord2;
            output.positionWS.xyz = input.positionWS;
            output.normalWS.xyz = input.normalWS;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.texCoord0.xyzw;
            output.texCoord1 = input.texCoord1.xyzw;
            output.texCoord2 = input.texCoord2.xyzw;
            output.positionWS = input.positionWS.xyz;
            output.normalWS = input.normalWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float _AmbientStrength;
        float4 _DiffuseColor;
        float4 _MainTex_TexelSize;
        float _Step;
        float _SpecPower;
        float _SpecIntensity;
        float _SpecWidth;
        float4 _HighlightColor;
        float _YHeight;
        float _DripDensity;
        float _DripSpeed;
        float _TrailWidth;
        float _DropSize;
        float _NoiseScale;
        float _YBase;
        float4 _NoiseTex_TexelSize;
        float _WettDark;
        float _WetBoost;
        UNITY_TEXTURE_STREAMING_DEBUG_VARS;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_NoiseTex);
        SAMPLER(sampler_NoiseTex);
        float _LerpB;
        float _LerpA;
        
        // Graph Includes
        #include_with_pragmas "Assets/ShadersAndEffects/InWorkMode/HLSL and more/Lighting.hlsl"
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Saturate_float(float In, out float Out)
        {
            Out = saturate(In);
        }
        
        void Unity_Lerp_float(float A, float B, float T, out float Out)
        {
            Out = lerp(A, B, T);
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
        Out = A * B;
        }
        
        struct Bindings_GetMainLight_d6b14a2e8b6f3554b8459648535f697e_float
        {
        };
        
        void SG_GetMainLight_d6b14a2e8b6f3554b8459648535f697e_float(float3 Vector3_b21c75b9b8514ef286d5e6dc199fa9af, Bindings_GetMainLight_d6b14a2e8b6f3554b8459648535f697e_float IN, out float3 Direction_0, out float3 Color_1, out float Attenuation_2)
        {
        float3 _Property_923162a64885457196b5ccbf7a2aaac7_Out_0_Vector3 = Vector3_b21c75b9b8514ef286d5e6dc199fa9af;
        float3 _MainLightCustomFunction_93e39954c04146b5ae1be272ea9d714b_Direction_0_Vector3;
        float3 _MainLightCustomFunction_93e39954c04146b5ae1be272ea9d714b_Color_2_Vector3;
        float _MainLightCustomFunction_93e39954c04146b5ae1be272ea9d714b_DistanceAtten_3_Float;
        float _MainLightCustomFunction_93e39954c04146b5ae1be272ea9d714b_ShadowAtten_4_Float;
        MainLight_float(_Property_923162a64885457196b5ccbf7a2aaac7_Out_0_Vector3, _MainLightCustomFunction_93e39954c04146b5ae1be272ea9d714b_Direction_0_Vector3, _MainLightCustomFunction_93e39954c04146b5ae1be272ea9d714b_Color_2_Vector3, _MainLightCustomFunction_93e39954c04146b5ae1be272ea9d714b_DistanceAtten_3_Float, _MainLightCustomFunction_93e39954c04146b5ae1be272ea9d714b_ShadowAtten_4_Float);
        float _Multiply_6d878674c9b447478a9564442340c0a2_Out_2_Float;
        Unity_Multiply_float_float(_MainLightCustomFunction_93e39954c04146b5ae1be272ea9d714b_DistanceAtten_3_Float, _MainLightCustomFunction_93e39954c04146b5ae1be272ea9d714b_ShadowAtten_4_Float, _Multiply_6d878674c9b447478a9564442340c0a2_Out_2_Float);
        Direction_0 = _MainLightCustomFunction_93e39954c04146b5ae1be272ea9d714b_Direction_0_Vector3;
        Color_1 = _MainLightCustomFunction_93e39954c04146b5ae1be272ea9d714b_Color_2_Vector3;
        Attenuation_2 = _Multiply_6d878674c9b447478a9564442340c0a2_Out_2_Float;
        }
        
        void Unity_DotProduct_float3(float3 A, float3 B, out float Out)
        {
            Out = dot(A, B);
        }
        
        void Unity_Floor_float(float In, out float Out)
        {
            Out = floor(In);
        }
        
        void Unity_Divide_float(float A, float B, out float Out)
        {
            Out = A / B;
        }
        
        void Unity_Maximum_float(float A, float B, out float Out)
        {
            Out = max(A, B);
        }
        
        struct Bindings_GetAdditionalLight_b5516a5008f7d104abebe27210c42de8_float
        {
        };
        
        void SG_GetAdditionalLight_b5516a5008f7d104abebe27210c42de8_float(float3 Vector3_1e07dec0084a48e38c95166c3cdc688d, float Vector1_0ce9574e837f408991312a6c71473833, Bindings_GetAdditionalLight_b5516a5008f7d104abebe27210c42de8_float IN, out float3 Direction_0, out float3 Color_1, out float Attenuation_2)
        {
        float3 _Property_e9927ccc4a684aeabdaa70dbe76689f0_Out_0_Vector3 = Vector3_1e07dec0084a48e38c95166c3cdc688d;
        float _Property_c6fa454cb94449b6923e1c90fafaf929_Out_0_Float = Vector1_0ce9574e837f408991312a6c71473833;
        float3 _AdditionalLightCustomFunction_c6ec52ed99634926b12dc96d48e96f1b_Direction_1_Vector3;
        float3 _AdditionalLightCustomFunction_c6ec52ed99634926b12dc96d48e96f1b_Color_2_Vector3;
        float _AdditionalLightCustomFunction_c6ec52ed99634926b12dc96d48e96f1b_DistanceAtten_3_Float;
        float _AdditionalLightCustomFunction_c6ec52ed99634926b12dc96d48e96f1b_ShadowAtten_4_Float;
        AdditionalLight_float(_Property_e9927ccc4a684aeabdaa70dbe76689f0_Out_0_Vector3, _Property_c6fa454cb94449b6923e1c90fafaf929_Out_0_Float, _AdditionalLightCustomFunction_c6ec52ed99634926b12dc96d48e96f1b_Direction_1_Vector3, _AdditionalLightCustomFunction_c6ec52ed99634926b12dc96d48e96f1b_Color_2_Vector3, _AdditionalLightCustomFunction_c6ec52ed99634926b12dc96d48e96f1b_DistanceAtten_3_Float, _AdditionalLightCustomFunction_c6ec52ed99634926b12dc96d48e96f1b_ShadowAtten_4_Float);
        float _Multiply_0b8ec7bbb926441e8c14c7c32d369adf_Out_2_Float;
        Unity_Multiply_float_float(_AdditionalLightCustomFunction_c6ec52ed99634926b12dc96d48e96f1b_DistanceAtten_3_Float, _AdditionalLightCustomFunction_c6ec52ed99634926b12dc96d48e96f1b_ShadowAtten_4_Float, _Multiply_0b8ec7bbb926441e8c14c7c32d369adf_Out_2_Float);
        Direction_0 = _AdditionalLightCustomFunction_c6ec52ed99634926b12dc96d48e96f1b_Direction_1_Vector3;
        Color_1 = _AdditionalLightCustomFunction_c6ec52ed99634926b12dc96d48e96f1b_Color_2_Vector3;
        Attenuation_2 = _Multiply_0b8ec7bbb926441e8c14c7c32d369adf_Out_2_Float;
        }
        
        void Unity_Multiply_float3_float3(float3 A, float3 B, out float3 Out)
        {
        Out = A * B;
        }
        
        void Unity_Add_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A + B;
        }
        
        struct Bindings_CalcAdditionalDiffuse_e85823578a3493d41ade631af12166aa_float
        {
        float3 WorldSpaceNormal;
        float3 WorldSpacePosition;
        };
        
        void SG_CalcAdditionalDiffuse_e85823578a3493d41ade631af12166aa_float(Bindings_CalcAdditionalDiffuse_e85823578a3493d41ade631af12166aa_float IN, out float3 Diffuse_1)
        {
        Bindings_GetAdditionalLight_b5516a5008f7d104abebe27210c42de8_float _GetAdditionalLight_e6c10dfa199649fca18a0cb673fcba44;
        float3 _GetAdditionalLight_e6c10dfa199649fca18a0cb673fcba44_Direction_0_Vector3;
        float3 _GetAdditionalLight_e6c10dfa199649fca18a0cb673fcba44_Color_1_Vector3;
        float _GetAdditionalLight_e6c10dfa199649fca18a0cb673fcba44_Attenuation_2_Float;
        SG_GetAdditionalLight_b5516a5008f7d104abebe27210c42de8_float(IN.WorldSpacePosition, float(0), _GetAdditionalLight_e6c10dfa199649fca18a0cb673fcba44, _GetAdditionalLight_e6c10dfa199649fca18a0cb673fcba44_Direction_0_Vector3, _GetAdditionalLight_e6c10dfa199649fca18a0cb673fcba44_Color_1_Vector3, _GetAdditionalLight_e6c10dfa199649fca18a0cb673fcba44_Attenuation_2_Float);
        float _DotProduct_aee5f4b498024e2da9796ed342f8a43a_Out_2_Float;
        Unity_DotProduct_float3(_GetAdditionalLight_e6c10dfa199649fca18a0cb673fcba44_Direction_0_Vector3, IN.WorldSpaceNormal, _DotProduct_aee5f4b498024e2da9796ed342f8a43a_Out_2_Float);
        float _Saturate_2080d664f6004dc9ad574ba7e91917b2_Out_1_Float;
        Unity_Saturate_float(_DotProduct_aee5f4b498024e2da9796ed342f8a43a_Out_2_Float, _Saturate_2080d664f6004dc9ad574ba7e91917b2_Out_1_Float);
        float3 _Multiply_57fcc7dc0fb845cf924c855eadbb7792_Out_2_Vector3;
        Unity_Multiply_float3_float3((_Saturate_2080d664f6004dc9ad574ba7e91917b2_Out_1_Float.xxx), _GetAdditionalLight_e6c10dfa199649fca18a0cb673fcba44_Color_1_Vector3, _Multiply_57fcc7dc0fb845cf924c855eadbb7792_Out_2_Vector3);
        float3 _Multiply_59016ca5433045d0a06ff03a13ec13b6_Out_2_Vector3;
        Unity_Multiply_float3_float3(_Multiply_57fcc7dc0fb845cf924c855eadbb7792_Out_2_Vector3, (_GetAdditionalLight_e6c10dfa199649fca18a0cb673fcba44_Attenuation_2_Float.xxx), _Multiply_59016ca5433045d0a06ff03a13ec13b6_Out_2_Vector3);
        Bindings_GetAdditionalLight_b5516a5008f7d104abebe27210c42de8_float _GetAdditionalLight_a054a4cd5b484e1492208dc9ca4a85f5;
        float3 _GetAdditionalLight_a054a4cd5b484e1492208dc9ca4a85f5_Direction_0_Vector3;
        float3 _GetAdditionalLight_a054a4cd5b484e1492208dc9ca4a85f5_Color_1_Vector3;
        float _GetAdditionalLight_a054a4cd5b484e1492208dc9ca4a85f5_Attenuation_2_Float;
        SG_GetAdditionalLight_b5516a5008f7d104abebe27210c42de8_float(IN.WorldSpacePosition, float(1), _GetAdditionalLight_a054a4cd5b484e1492208dc9ca4a85f5, _GetAdditionalLight_a054a4cd5b484e1492208dc9ca4a85f5_Direction_0_Vector3, _GetAdditionalLight_a054a4cd5b484e1492208dc9ca4a85f5_Color_1_Vector3, _GetAdditionalLight_a054a4cd5b484e1492208dc9ca4a85f5_Attenuation_2_Float);
        float _DotProduct_00e581a08ce7465588597fd11859eea2_Out_2_Float;
        Unity_DotProduct_float3(_GetAdditionalLight_a054a4cd5b484e1492208dc9ca4a85f5_Direction_0_Vector3, IN.WorldSpaceNormal, _DotProduct_00e581a08ce7465588597fd11859eea2_Out_2_Float);
        float _Saturate_58b5f4d3dcaf45e6bda212ef43edc4ea_Out_1_Float;
        Unity_Saturate_float(_DotProduct_00e581a08ce7465588597fd11859eea2_Out_2_Float, _Saturate_58b5f4d3dcaf45e6bda212ef43edc4ea_Out_1_Float);
        float3 _Multiply_6ff1cb7ecc8542848d8d721f284a3b3c_Out_2_Vector3;
        Unity_Multiply_float3_float3((_Saturate_58b5f4d3dcaf45e6bda212ef43edc4ea_Out_1_Float.xxx), _GetAdditionalLight_a054a4cd5b484e1492208dc9ca4a85f5_Color_1_Vector3, _Multiply_6ff1cb7ecc8542848d8d721f284a3b3c_Out_2_Vector3);
        float3 _Multiply_8337e9ec92b74cefb421776f67b29661_Out_2_Vector3;
        Unity_Multiply_float3_float3(_Multiply_6ff1cb7ecc8542848d8d721f284a3b3c_Out_2_Vector3, (_GetAdditionalLight_a054a4cd5b484e1492208dc9ca4a85f5_Attenuation_2_Float.xxx), _Multiply_8337e9ec92b74cefb421776f67b29661_Out_2_Vector3);
        float3 _Add_8d5fe4674e364518a5d8f4559669df92_Out_2_Vector3;
        Unity_Add_float3(_Multiply_59016ca5433045d0a06ff03a13ec13b6_Out_2_Vector3, _Multiply_8337e9ec92b74cefb421776f67b29661_Out_2_Vector3, _Add_8d5fe4674e364518a5d8f4559669df92_Out_2_Vector3);
        Bindings_GetAdditionalLight_b5516a5008f7d104abebe27210c42de8_float _GetAdditionalLight_ee2152ac3ed64138932b3a2e4b8e3f14;
        float3 _GetAdditionalLight_ee2152ac3ed64138932b3a2e4b8e3f14_Direction_0_Vector3;
        float3 _GetAdditionalLight_ee2152ac3ed64138932b3a2e4b8e3f14_Color_1_Vector3;
        float _GetAdditionalLight_ee2152ac3ed64138932b3a2e4b8e3f14_Attenuation_2_Float;
        SG_GetAdditionalLight_b5516a5008f7d104abebe27210c42de8_float(IN.WorldSpacePosition, float(2), _GetAdditionalLight_ee2152ac3ed64138932b3a2e4b8e3f14, _GetAdditionalLight_ee2152ac3ed64138932b3a2e4b8e3f14_Direction_0_Vector3, _GetAdditionalLight_ee2152ac3ed64138932b3a2e4b8e3f14_Color_1_Vector3, _GetAdditionalLight_ee2152ac3ed64138932b3a2e4b8e3f14_Attenuation_2_Float);
        float _DotProduct_7a3fce316c4a453e9b9e18226fc2d066_Out_2_Float;
        Unity_DotProduct_float3(_GetAdditionalLight_ee2152ac3ed64138932b3a2e4b8e3f14_Direction_0_Vector3, IN.WorldSpaceNormal, _DotProduct_7a3fce316c4a453e9b9e18226fc2d066_Out_2_Float);
        float _Saturate_c9919732b14f4622aa82e1eec5cb383d_Out_1_Float;
        Unity_Saturate_float(_DotProduct_7a3fce316c4a453e9b9e18226fc2d066_Out_2_Float, _Saturate_c9919732b14f4622aa82e1eec5cb383d_Out_1_Float);
        float3 _Multiply_013e04c47fdf4f46a7f2a4868ec83bed_Out_2_Vector3;
        Unity_Multiply_float3_float3((_Saturate_c9919732b14f4622aa82e1eec5cb383d_Out_1_Float.xxx), _GetAdditionalLight_ee2152ac3ed64138932b3a2e4b8e3f14_Color_1_Vector3, _Multiply_013e04c47fdf4f46a7f2a4868ec83bed_Out_2_Vector3);
        float3 _Multiply_8d16da0d689d4d49909909e6c2cc2da6_Out_2_Vector3;
        Unity_Multiply_float3_float3(_Multiply_013e04c47fdf4f46a7f2a4868ec83bed_Out_2_Vector3, (_GetAdditionalLight_ee2152ac3ed64138932b3a2e4b8e3f14_Attenuation_2_Float.xxx), _Multiply_8d16da0d689d4d49909909e6c2cc2da6_Out_2_Vector3);
        Bindings_GetAdditionalLight_b5516a5008f7d104abebe27210c42de8_float _GetAdditionalLight_da7753016bd64b489b897349d7f3fc0f;
        float3 _GetAdditionalLight_da7753016bd64b489b897349d7f3fc0f_Direction_0_Vector3;
        float3 _GetAdditionalLight_da7753016bd64b489b897349d7f3fc0f_Color_1_Vector3;
        float _GetAdditionalLight_da7753016bd64b489b897349d7f3fc0f_Attenuation_2_Float;
        SG_GetAdditionalLight_b5516a5008f7d104abebe27210c42de8_float(IN.WorldSpacePosition, float(3), _GetAdditionalLight_da7753016bd64b489b897349d7f3fc0f, _GetAdditionalLight_da7753016bd64b489b897349d7f3fc0f_Direction_0_Vector3, _GetAdditionalLight_da7753016bd64b489b897349d7f3fc0f_Color_1_Vector3, _GetAdditionalLight_da7753016bd64b489b897349d7f3fc0f_Attenuation_2_Float);
        float _DotProduct_be3ed23b71d142e3a88f9f21ca2b9bb1_Out_2_Float;
        Unity_DotProduct_float3(_GetAdditionalLight_da7753016bd64b489b897349d7f3fc0f_Direction_0_Vector3, IN.WorldSpaceNormal, _DotProduct_be3ed23b71d142e3a88f9f21ca2b9bb1_Out_2_Float);
        float _Saturate_b3794fe44470430e954d43bb481a3d9a_Out_1_Float;
        Unity_Saturate_float(_DotProduct_be3ed23b71d142e3a88f9f21ca2b9bb1_Out_2_Float, _Saturate_b3794fe44470430e954d43bb481a3d9a_Out_1_Float);
        float3 _Multiply_fcf9bc551f0e42f39634ac95fab7cae5_Out_2_Vector3;
        Unity_Multiply_float3_float3((_Saturate_b3794fe44470430e954d43bb481a3d9a_Out_1_Float.xxx), _GetAdditionalLight_da7753016bd64b489b897349d7f3fc0f_Color_1_Vector3, _Multiply_fcf9bc551f0e42f39634ac95fab7cae5_Out_2_Vector3);
        float3 _Multiply_0cc8c00775354d6a9f91977c3e357ff8_Out_2_Vector3;
        Unity_Multiply_float3_float3(_Multiply_fcf9bc551f0e42f39634ac95fab7cae5_Out_2_Vector3, (_GetAdditionalLight_da7753016bd64b489b897349d7f3fc0f_Attenuation_2_Float.xxx), _Multiply_0cc8c00775354d6a9f91977c3e357ff8_Out_2_Vector3);
        float3 _Add_0992646b9a764f6eb220517d0e9fc815_Out_2_Vector3;
        Unity_Add_float3(_Multiply_8d16da0d689d4d49909909e6c2cc2da6_Out_2_Vector3, _Multiply_0cc8c00775354d6a9f91977c3e357ff8_Out_2_Vector3, _Add_0992646b9a764f6eb220517d0e9fc815_Out_2_Vector3);
        float3 _Add_d239c6ae017b45cba8fda7bfa29dc880_Out_2_Vector3;
        Unity_Add_float3(_Add_8d5fe4674e364518a5d8f4559669df92_Out_2_Vector3, _Add_0992646b9a764f6eb220517d0e9fc815_Out_2_Vector3, _Add_d239c6ae017b45cba8fda7bfa29dc880_Out_2_Vector3);
        Diffuse_1 = _Add_d239c6ae017b45cba8fda7bfa29dc880_Out_2_Vector3;
        }
        
        void Unity_OneMinus_float(float In, out float Out)
        {
            Out = 1 - In;
        }
        
        void Unity_Normalize_float3(float3 In, out float3 Out)
        {
            Out = normalize(In);
        }
        
        void Unity_Power_float(float A, float B, out float Out)
        {
            Out = pow(A, B);
        }
        
        void Unity_Smoothstep_float(float Edge1, float Edge2, float In, out float Out)
        {
            Out = smoothstep(Edge1, Edge2, In);
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float3 BaseColor;
            float3 Emission;
            float Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Property_58ffbe2cdb974e6eb51f08eb2773b719_Out_0_Float = _WettDark;
            float _Saturate_9a6eaf9061454bb68af056e5c611fc32_Out_1_Float;
            Unity_Saturate_float(float(0), _Saturate_9a6eaf9061454bb68af056e5c611fc32_Out_1_Float);
            float _Lerp_07deac0a40074d41b62045f5fb57c3f6_Out_3_Float;
            Unity_Lerp_float(float(1), _Property_58ffbe2cdb974e6eb51f08eb2773b719_Out_0_Float, _Saturate_9a6eaf9061454bb68af056e5c611fc32_Out_1_Float, _Lerp_07deac0a40074d41b62045f5fb57c3f6_Out_3_Float);
            Bindings_GetMainLight_d6b14a2e8b6f3554b8459648535f697e_float _GetMainLight_66f3629103c443e29b0bf77e7318459a;
            float3 _GetMainLight_66f3629103c443e29b0bf77e7318459a_Direction_0_Vector3;
            float3 _GetMainLight_66f3629103c443e29b0bf77e7318459a_Color_1_Vector3;
            float _GetMainLight_66f3629103c443e29b0bf77e7318459a_Attenuation_2_Float;
            SG_GetMainLight_d6b14a2e8b6f3554b8459648535f697e_float(float3 (0, 0, 0), _GetMainLight_66f3629103c443e29b0bf77e7318459a, _GetMainLight_66f3629103c443e29b0bf77e7318459a_Direction_0_Vector3, _GetMainLight_66f3629103c443e29b0bf77e7318459a_Color_1_Vector3, _GetMainLight_66f3629103c443e29b0bf77e7318459a_Attenuation_2_Float);
            float4 _Property_5bce803f7b1342118efa64dfd7a5c5af_Out_0_Vector4 = _DiffuseColor;
            Bindings_GetMainLight_d6b14a2e8b6f3554b8459648535f697e_float _GetMainLight_41b37094e05a429f8266b6602caf8242;
            float3 _GetMainLight_41b37094e05a429f8266b6602caf8242_Direction_0_Vector3;
            float3 _GetMainLight_41b37094e05a429f8266b6602caf8242_Color_1_Vector3;
            float _GetMainLight_41b37094e05a429f8266b6602caf8242_Attenuation_2_Float;
            SG_GetMainLight_d6b14a2e8b6f3554b8459648535f697e_float(IN.WorldSpacePosition, _GetMainLight_41b37094e05a429f8266b6602caf8242, _GetMainLight_41b37094e05a429f8266b6602caf8242_Direction_0_Vector3, _GetMainLight_41b37094e05a429f8266b6602caf8242_Color_1_Vector3, _GetMainLight_41b37094e05a429f8266b6602caf8242_Attenuation_2_Float);
            float _DotProduct_8d4bdec92ac948559d5a94572dd8cb4d_Out_2_Float;
            Unity_DotProduct_float3(IN.WorldSpaceNormal, _GetMainLight_41b37094e05a429f8266b6602caf8242_Direction_0_Vector3, _DotProduct_8d4bdec92ac948559d5a94572dd8cb4d_Out_2_Float);
            float _Saturate_568447dc078f4cdaa9df59ff3d9456d3_Out_1_Float;
            Unity_Saturate_float(_DotProduct_8d4bdec92ac948559d5a94572dd8cb4d_Out_2_Float, _Saturate_568447dc078f4cdaa9df59ff3d9456d3_Out_1_Float);
            float _Property_2e6e3ebe260e41cda8b744c9ef183653_Out_0_Float = _Step;
            float _Multiply_89ea6b707c044fb5a105c8ecc0f663cc_Out_2_Float;
            Unity_Multiply_float_float(_Saturate_568447dc078f4cdaa9df59ff3d9456d3_Out_1_Float, _Property_2e6e3ebe260e41cda8b744c9ef183653_Out_0_Float, _Multiply_89ea6b707c044fb5a105c8ecc0f663cc_Out_2_Float);
            float _Floor_a3099d1c1e92444786c2d86607f308d1_Out_1_Float;
            Unity_Floor_float(_Multiply_89ea6b707c044fb5a105c8ecc0f663cc_Out_2_Float, _Floor_a3099d1c1e92444786c2d86607f308d1_Out_1_Float);
            float _Divide_bf2c7eeba1dd479aaab53317c575a0ea_Out_2_Float;
            Unity_Divide_float(_Floor_a3099d1c1e92444786c2d86607f308d1_Out_1_Float, _Property_2e6e3ebe260e41cda8b744c9ef183653_Out_0_Float, _Divide_bf2c7eeba1dd479aaab53317c575a0ea_Out_2_Float);
            float _Property_68149cd9c2eb40a79d0105acc016d1c4_Out_0_Float = _AmbientStrength;
            float _Maximum_c73c7cbcf6c4463a865782d13a0c4101_Out_2_Float;
            Unity_Maximum_float(_Divide_bf2c7eeba1dd479aaab53317c575a0ea_Out_2_Float, _Property_68149cd9c2eb40a79d0105acc016d1c4_Out_0_Float, _Maximum_c73c7cbcf6c4463a865782d13a0c4101_Out_2_Float);
            Bindings_CalcAdditionalDiffuse_e85823578a3493d41ade631af12166aa_float _CalcAdditionalDiffuse_f61ebb0caffa49798669b455d28ddc2f;
            _CalcAdditionalDiffuse_f61ebb0caffa49798669b455d28ddc2f.WorldSpaceNormal = IN.WorldSpaceNormal;
            _CalcAdditionalDiffuse_f61ebb0caffa49798669b455d28ddc2f.WorldSpacePosition = IN.WorldSpacePosition;
            float3 _CalcAdditionalDiffuse_f61ebb0caffa49798669b455d28ddc2f_Diffuse_1_Vector3;
            SG_CalcAdditionalDiffuse_e85823578a3493d41ade631af12166aa_float(_CalcAdditionalDiffuse_f61ebb0caffa49798669b455d28ddc2f, _CalcAdditionalDiffuse_f61ebb0caffa49798669b455d28ddc2f_Diffuse_1_Vector3);
            float3 _Add_0f6ab824bf464526bbf3116b8d1e102f_Out_2_Vector3;
            Unity_Add_float3((_Maximum_c73c7cbcf6c4463a865782d13a0c4101_Out_2_Float.xxx), _CalcAdditionalDiffuse_f61ebb0caffa49798669b455d28ddc2f_Diffuse_1_Vector3, _Add_0f6ab824bf464526bbf3116b8d1e102f_Out_2_Vector3);
            float3 _Multiply_d622b247c4a84f8bbebf6a902f0b0fcb_Out_2_Vector3;
            Unity_Multiply_float3_float3((_Property_5bce803f7b1342118efa64dfd7a5c5af_Out_0_Vector4.xyz), _Add_0f6ab824bf464526bbf3116b8d1e102f_Out_2_Vector3, _Multiply_d622b247c4a84f8bbebf6a902f0b0fcb_Out_2_Vector3);
            UnityTexture2D _Property_a111b087fff64719b899025161b92d31_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_MainTex);
            float4 _SampleTexture2D_68405c1067a64b7f874afd25ede0e0c8_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_a111b087fff64719b899025161b92d31_Out_0_Texture2D.tex, _Property_a111b087fff64719b899025161b92d31_Out_0_Texture2D.samplerstate, _Property_a111b087fff64719b899025161b92d31_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_68405c1067a64b7f874afd25ede0e0c8_R_4_Float = _SampleTexture2D_68405c1067a64b7f874afd25ede0e0c8_RGBA_0_Vector4.r;
            float _SampleTexture2D_68405c1067a64b7f874afd25ede0e0c8_G_5_Float = _SampleTexture2D_68405c1067a64b7f874afd25ede0e0c8_RGBA_0_Vector4.g;
            float _SampleTexture2D_68405c1067a64b7f874afd25ede0e0c8_B_6_Float = _SampleTexture2D_68405c1067a64b7f874afd25ede0e0c8_RGBA_0_Vector4.b;
            float _SampleTexture2D_68405c1067a64b7f874afd25ede0e0c8_A_7_Float = _SampleTexture2D_68405c1067a64b7f874afd25ede0e0c8_RGBA_0_Vector4.a;
            float3 _Multiply_8ac74964cd004464a81865818d5afcc4_Out_2_Vector3;
            Unity_Multiply_float3_float3(_Multiply_d622b247c4a84f8bbebf6a902f0b0fcb_Out_2_Vector3, (_SampleTexture2D_68405c1067a64b7f874afd25ede0e0c8_RGBA_0_Vector4.xyz), _Multiply_8ac74964cd004464a81865818d5afcc4_Out_2_Vector3);
            float _Property_0e2d1f788f574a2f8b34d846bbd7263c_Out_0_Float = _SpecWidth;
            float _Multiply_c17e567eb6d844efb410cd9a33a05375_Out_2_Float;
            Unity_Multiply_float_float(_Property_0e2d1f788f574a2f8b34d846bbd7263c_Out_0_Float, 2, _Multiply_c17e567eb6d844efb410cd9a33a05375_Out_2_Float);
            float _OneMinus_d3c0cdcde3e14887b51750055ea97bb8_Out_1_Float;
            Unity_OneMinus_float(_Multiply_c17e567eb6d844efb410cd9a33a05375_Out_2_Float, _OneMinus_d3c0cdcde3e14887b51750055ea97bb8_Out_1_Float);
            float _OneMinus_a315df87ea3b4fdebec40e5a9022411a_Out_1_Float;
            Unity_OneMinus_float(_Property_0e2d1f788f574a2f8b34d846bbd7263c_Out_0_Float, _OneMinus_a315df87ea3b4fdebec40e5a9022411a_Out_1_Float);
            float3 _Normalize_048a70be3e7e48d79a6a6d5115dfa858_Out_1_Vector3;
            Unity_Normalize_float3(_GetMainLight_66f3629103c443e29b0bf77e7318459a_Direction_0_Vector3, _Normalize_048a70be3e7e48d79a6a6d5115dfa858_Out_1_Vector3);
            float3 _Normalize_d0e7859ef3784895af4ebfa092cea7a8_Out_1_Vector3;
            Unity_Normalize_float3(IN.WorldSpaceViewDirection, _Normalize_d0e7859ef3784895af4ebfa092cea7a8_Out_1_Vector3);
            float3 _Add_155e7c4d78f949c48564160410bc4048_Out_2_Vector3;
            Unity_Add_float3(_Normalize_048a70be3e7e48d79a6a6d5115dfa858_Out_1_Vector3, _Normalize_d0e7859ef3784895af4ebfa092cea7a8_Out_1_Vector3, _Add_155e7c4d78f949c48564160410bc4048_Out_2_Vector3);
            float3 _Normalize_47d11df4961b4fb8a702583e4b0a9fc7_Out_1_Vector3;
            Unity_Normalize_float3(_Add_155e7c4d78f949c48564160410bc4048_Out_2_Vector3, _Normalize_47d11df4961b4fb8a702583e4b0a9fc7_Out_1_Vector3);
            float _DotProduct_6141d4c6d0ac402596caf047be9dbc14_Out_2_Float;
            Unity_DotProduct_float3(IN.WorldSpaceNormal, _Normalize_47d11df4961b4fb8a702583e4b0a9fc7_Out_1_Vector3, _DotProduct_6141d4c6d0ac402596caf047be9dbc14_Out_2_Float);
            float _Saturate_6d73ca1db00946ebb16bc3ae45eec3e3_Out_1_Float;
            Unity_Saturate_float(_DotProduct_6141d4c6d0ac402596caf047be9dbc14_Out_2_Float, _Saturate_6d73ca1db00946ebb16bc3ae45eec3e3_Out_1_Float);
            float _Property_cffee28cd20846c294a39928e843e022_Out_0_Float = _SpecPower;
            float _Power_ed38d16ded9843b997ddef76e0b234c9_Out_2_Float;
            Unity_Power_float(_Saturate_6d73ca1db00946ebb16bc3ae45eec3e3_Out_1_Float, _Property_cffee28cd20846c294a39928e843e022_Out_0_Float, _Power_ed38d16ded9843b997ddef76e0b234c9_Out_2_Float);
            float _Smoothstep_1415711804e74f72af642ca650f88631_Out_3_Float;
            Unity_Smoothstep_float(_OneMinus_d3c0cdcde3e14887b51750055ea97bb8_Out_1_Float, _OneMinus_a315df87ea3b4fdebec40e5a9022411a_Out_1_Float, _Power_ed38d16ded9843b997ddef76e0b234c9_Out_2_Float, _Smoothstep_1415711804e74f72af642ca650f88631_Out_3_Float);
            float _Property_e24c9b5c87334203ae45a0f4524092cf_Out_0_Float = _SpecIntensity;
            float _Multiply_bd9542e81f61443c93d68d6effc8d884_Out_2_Float;
            Unity_Multiply_float_float(_Smoothstep_1415711804e74f72af642ca650f88631_Out_3_Float, _Property_e24c9b5c87334203ae45a0f4524092cf_Out_0_Float, _Multiply_bd9542e81f61443c93d68d6effc8d884_Out_2_Float);
            float4 _Property_17f80f6fd3d648edbac72436c2bf4f2d_Out_0_Vector4 = IsGammaSpace() ? LinearToSRGB(_HighlightColor) : _HighlightColor;
            float4 _Multiply_fb7a787882f34251bf81a170c440b704_Out_2_Vector4;
            Unity_Multiply_float4_float4((_Multiply_bd9542e81f61443c93d68d6effc8d884_Out_2_Float.xxxx), _Property_17f80f6fd3d648edbac72436c2bf4f2d_Out_0_Vector4, _Multiply_fb7a787882f34251bf81a170c440b704_Out_2_Vector4);
            float3 _Add_6b9391937ee345d693bb8ca62983e59c_Out_2_Vector3;
            Unity_Add_float3(_Multiply_8ac74964cd004464a81865818d5afcc4_Out_2_Vector3, (_Multiply_fb7a787882f34251bf81a170c440b704_Out_2_Vector4.xyz), _Add_6b9391937ee345d693bb8ca62983e59c_Out_2_Vector3);
            float3 _Multiply_9ead612961534f23adf829877ef2f3da_Out_2_Vector3;
            Unity_Multiply_float3_float3(_GetMainLight_66f3629103c443e29b0bf77e7318459a_Color_1_Vector3, _Add_6b9391937ee345d693bb8ca62983e59c_Out_2_Vector3, _Multiply_9ead612961534f23adf829877ef2f3da_Out_2_Vector3);
            float3 _Multiply_96dcb3634feb420a982106a37e9fb39f_Out_2_Vector3;
            Unity_Multiply_float3_float3(_Multiply_9ead612961534f23adf829877ef2f3da_Out_2_Vector3, (_GetMainLight_66f3629103c443e29b0bf77e7318459a_Attenuation_2_Float.xxx), _Multiply_96dcb3634feb420a982106a37e9fb39f_Out_2_Vector3);
            float3 _Multiply_9f64f4291cc242d993083fefef541c9e_Out_2_Vector3;
            Unity_Multiply_float3_float3((_Lerp_07deac0a40074d41b62045f5fb57c3f6_Out_3_Float.xxx), _Multiply_96dcb3634feb420a982106a37e9fb39f_Out_2_Vector3, _Multiply_9f64f4291cc242d993083fefef541c9e_Out_2_Vector3);
            float _Property_e7835e37fa084c21b3fe49ad42fed15e_Out_0_Float = _WetBoost;
            float _Multiply_3841a77e87a5481d90edb0ddd7665098_Out_2_Float;
            Unity_Multiply_float_float(_Saturate_9a6eaf9061454bb68af056e5c611fc32_Out_1_Float, _Property_e7835e37fa084c21b3fe49ad42fed15e_Out_0_Float, _Multiply_3841a77e87a5481d90edb0ddd7665098_Out_2_Float);
            float _Add_52bbe1adc27b4277bf2662761ef4207c_Out_2_Float;
            Unity_Add_float(float(1), _Multiply_3841a77e87a5481d90edb0ddd7665098_Out_2_Float, _Add_52bbe1adc27b4277bf2662761ef4207c_Out_2_Float);
            float3 _Multiply_f5424bb1ec264175bae4da44f3dc0f2f_Out_2_Vector3;
            Unity_Multiply_float3_float3((_Add_52bbe1adc27b4277bf2662761ef4207c_Out_2_Float.xxx), _Multiply_96dcb3634feb420a982106a37e9fb39f_Out_2_Vector3, _Multiply_f5424bb1ec264175bae4da44f3dc0f2f_Out_2_Vector3);
            float3 _Add_ade479e6605d417d9ff3e0e1696825dd_Out_2_Vector3;
            Unity_Add_float3(_Multiply_9f64f4291cc242d993083fefef541c9e_Out_2_Vector3, _Multiply_f5424bb1ec264175bae4da44f3dc0f2f_Out_2_Vector3, _Add_ade479e6605d417d9ff3e0e1696825dd_Out_2_Vector3);
            surface.BaseColor = IsGammaSpace() ? float3(0, 0, 0) : SRGBToLinear(float3(0, 0, 0));
            surface.Emission = _Add_ade479e6605d417d9ff3e0e1696825dd_Out_2_Vector3;
            surface.Alpha = float(1);
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
            // must use interpolated tangent, bitangent and normal before they are normalized in the pixel shader.
            float3 unnormalizedNormalWS = input.normalWS;
            const float renormFactor = 1.0 / length(unnormalizedNormalWS);
        
        
            output.WorldSpaceNormal = renormFactor * input.normalWS.xyz;      // we want a unit length Normal Vector node in shader graph
        
        
            output.WorldSpaceViewDirection = GetWorldSpaceNormalizeViewDir(input.positionWS);
            output.WorldSpacePosition = input.positionWS;
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
            output.uv0 = input.texCoord0;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/LightingMetaPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "SceneSelectionPass"
            Tags
            {
                "LightMode" = "SceneSelectionPass"
            }
        
        // Render State
        Cull Off
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        // PassKeywords: <None>
        // GraphKeywords: <None>
        
        // Defines
        
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define FEATURES_GRAPH_VERTEX_NORMAL_OUTPUT
        #define FEATURES_GRAPH_VERTEX_TANGENT_OUTPUT
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_DEPTHONLY
        #define SCENESELECTIONPASS 1
        #define ALPHA_CLIP_THRESHOLD 1
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float _AmbientStrength;
        float4 _DiffuseColor;
        float4 _MainTex_TexelSize;
        float _Step;
        float _SpecPower;
        float _SpecIntensity;
        float _SpecWidth;
        float4 _HighlightColor;
        float _YHeight;
        float _DripDensity;
        float _DripSpeed;
        float _TrailWidth;
        float _DropSize;
        float _NoiseScale;
        float _YBase;
        float4 _NoiseTex_TexelSize;
        float _WettDark;
        float _WetBoost;
        UNITY_TEXTURE_STREAMING_DEBUG_VARS;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_NoiseTex);
        SAMPLER(sampler_NoiseTex);
        float _LerpB;
        float _LerpA;
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        // GraphFunctions: <None>
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            surface.Alpha = float(1);
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/SelectionPickingPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "ScenePickingPass"
            Tags
            {
                "LightMode" = "Picking"
            }
        
        // Render State
        Cull Off
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        // PassKeywords: <None>
        // GraphKeywords: <None>
        
        // Defines
        
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define FEATURES_GRAPH_VERTEX_NORMAL_OUTPUT
        #define FEATURES_GRAPH_VERTEX_TANGENT_OUTPUT
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_DEPTHONLY
        #define SCENEPICKINGPASS 1
        #define ALPHA_CLIP_THRESHOLD 1
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float _AmbientStrength;
        float4 _DiffuseColor;
        float4 _MainTex_TexelSize;
        float _Step;
        float _SpecPower;
        float _SpecIntensity;
        float _SpecWidth;
        float4 _HighlightColor;
        float _YHeight;
        float _DripDensity;
        float _DripSpeed;
        float _TrailWidth;
        float _DropSize;
        float _NoiseScale;
        float _YBase;
        float4 _NoiseTex_TexelSize;
        float _WettDark;
        float _WetBoost;
        UNITY_TEXTURE_STREAMING_DEBUG_VARS;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_NoiseTex);
        SAMPLER(sampler_NoiseTex);
        float _LerpB;
        float _LerpA;
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        // GraphFunctions: <None>
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float3 BaseColor;
            float Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            surface.BaseColor = IsGammaSpace() ? float3(0, 0, 0) : SRGBToLinear(float3(0, 0, 0));
            surface.Alpha = float(1);
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/SelectionPickingPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "Universal 2D"
            Tags
            {
                "LightMode" = "Universal2D"
            }
        
        // Render State
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite Off
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        // PassKeywords: <None>
        // GraphKeywords: <None>
        
        // Defines
        
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define FEATURES_GRAPH_VERTEX_NORMAL_OUTPUT
        #define FEATURES_GRAPH_VERTEX_TANGENT_OUTPUT
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_2D
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float _AmbientStrength;
        float4 _DiffuseColor;
        float4 _MainTex_TexelSize;
        float _Step;
        float _SpecPower;
        float _SpecIntensity;
        float _SpecWidth;
        float4 _HighlightColor;
        float _YHeight;
        float _DripDensity;
        float _DripSpeed;
        float _TrailWidth;
        float _DropSize;
        float _NoiseScale;
        float _YBase;
        float4 _NoiseTex_TexelSize;
        float _WettDark;
        float _WetBoost;
        UNITY_TEXTURE_STREAMING_DEBUG_VARS;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_NoiseTex);
        SAMPLER(sampler_NoiseTex);
        float _LerpB;
        float _LerpA;
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        // GraphFunctions: <None>
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float3 BaseColor;
            float Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            surface.BaseColor = IsGammaSpace() ? float3(0, 0, 0) : SRGBToLinear(float3(0, 0, 0));
            surface.Alpha = float(1);
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBR2DPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
    }
    CustomEditor "UnityEditor.ShaderGraph.GenericShaderGraphMaterialGUI"
    CustomEditorForRenderPipeline "UnityEditor.ShaderGraphLitGUI" "UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset"
    FallBack "Hidden/Shader Graph/FallbackError"
}