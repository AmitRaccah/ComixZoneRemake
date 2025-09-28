// TestTiny.hlsl
// Provide BOTH precision variants.
// If you don't need the Time input, remove it here AND from the node.

void TestTiny_float(float3 posWS, float Time, out float mask)
{
    mask = posWS.y; // simple test
}

void TestTiny_half(half3 posWS, half Time, out half mask)
{
    mask = posWS.y;
}
