// includes סטנדרטיים של URP
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

// Toon "רך": סף ו"נוצות" (אפשר להחליף בדגימת Ramp Texture)
inline float ToonStep(float ndotl, float threshold, float feather)
{
	// ndotl ∈ [0,1]
	return smoothstep(threshold - feather, threshold + feather, saturate(ndotl));
}

// חישוב אור טוני לאור יחיד
inline float3 EvalToonLight(in float3 N, in Light L, float threshold, float feather)
{
	float ndl   = saturate(dot(N, L.direction));
	float step  = ToonStep(ndl, threshold, feather);      // << כאן הטון
	float atten = L.distanceAttenuation * L.shadowAttenuation;
	return step * L.color * atten;
}

// סכימת האור הראשי + אורות נוספים – כולם עוברים toon
inline float3 AccumulateToonLighting(float3 worldPos, float3 N, float threshold, float feather)
{
	float3 sumCol = 0;

	// Main Light
	float4 shadowCoord = TransformWorldToShadowCoord(worldPos);
	Light mainLight = GetMainLight(shadowCoord);
	sumCol += EvalToonLight(N, mainLight, threshold, feather);

	// Additional Lights
	#if defined(_ADDITIONAL_LIGHTS)
	int count = GetAdditionalLightsCount();
	[loop] for (int i = 0; i < count; i++)
	{
		Light l = GetAdditionalLight(i, worldPos);
		sumCol += EvalToonLight(N, l, threshold, feather);
	}
	#endif

	return sumCol; // להכות בזה את ה-albedo שלך בסוף
}
