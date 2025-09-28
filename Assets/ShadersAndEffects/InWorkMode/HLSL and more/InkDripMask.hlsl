// InkDripMask.hlsl
// Minimal, texture-free drip columns + falling heads, designed for Shader Graph Custom Function (File mode).
// Axis-agnostic: you feed posWS and an upDir; it will fall along that upDir.
// Returns a 0..1 mask you can plug into Emission/Lerp/etc.

// ------------------------------
// tiny hashing helpers (deterministic, no Unity includes)
// ------------------------------
inline float2 hash22(float2 p)
{
    // https://www.shadertoy.com/view/4djSRW style hash
    float3 p3 = frac(float3(p.x, p.y, p.x) * 0.1031);
    p3 += dot(p3, p3.yzx + 33.33);
    return frac((p3.xx + p3.yz) * p3.zy);
}

inline float hash21(float2 p)
{
    float2 h = hash22(p);
    return frac(h.x + h.y * 0.61803398875); // golden ratio mix
}

// soft disk profile (0 at center → 1 at radius, then clamp)
inline float softDisk(float2 uv, float radius, float softness)
{
    float d = length(uv);
    float t = saturate((d - (radius - softness)) / max(1e-5, softness));
    return t; // 0 center → 1 outside (use 1 - to get solid disk)
}

// smoothstep helper that guards edge order
inline float sstep(float e1, float e2, float x)
{
    float a = min(e1, e2);
    float b = max(e1, e2);
    return smoothstep(a, b, x);
}

// ------------------------------------------------------------
// MAIN
// ------------------------------------------------------------
// INPUTS (keep these names & order exactly when you wire the Custom Function node):
//   float3 posWS      : world-space position of the pixel (Shader Graph → Position (World))
//   float  time       : current time (Shader Graph → Time node, use Time.y or Time.x)
//   float3 upDir      : which direction is "up" (usually (0,1,0). Normalize is done inside)
//   float  dripDensity: average columns per world unit across (bigger = more columns)
//   float  dripSpeed  : world units per second (positive always falls top → bottom)
//   float  trailWidth : vertical thickness of the wet band behind the head (world units)
//   float  dropSize   : head blob radius (world units)
//   float  yBase      : bottom of the dripping region (world height)
//   float  yHeight    : height of the dripping region (world units)
//   float  jitter     : small 0..1 to vary head phase/speed (use 0.1..0.5)
// OUTPUT:
//   float  dripMask   : 0..1 mask (columns * trail + head blob). Use as Emission/Lerp mask.
//
void InkDripMask_float(
    float3 posWS,
    float  time,
    float3 upDir,
    float  dripDensity,
    float  dripSpeed,
    float  trailWidth,
    float  dropSize,
    float  yBase,
    float  yHeight,
    float  jitter,
    out float dripMask
)
{
    // --- set up a coordinate frame: Up (normalized), and a lateral axis X to lay columns across ---
    float3 U = normalize(upDir);
    // choose any vector not parallel to U to build a tangent
    float3 any = (abs(U.y) < 0.99) ? float3(0,1,0) : float3(1,0,0);
    float3 X = normalize(cross(any, U));   // one lateral axis
    float3 Z = normalize(cross(U, X));     // the other lateral axis

    // world "height" along up
    float y = dot(posWS, U);

    // pick one lateral axis to distribute columns across (X is fine; swap to Z if you prefer)
    float x = dot(posWS, X);

    // --- region clamp & normalized height (0 at yBase → 1 at yBase+yHeight) ---
    float y01 = (y - yBase) / max(1e-5, yHeight);
    y01 = saturate(y01);

    // --- time motion: always top→bottom ---
    float speed = abs(dripSpeed);     // force downward (positive speed means moving toward -U visually)
    float t = time * speed;

    // --- column grid in x: cell width from density ---
    // columns spaced roughly by 1/dripDensity world units
    float cellW = max(1e-4, 1.0 / max(1e-5, dripDensity));
    float xCell = floor(x / cellW);

    // random per-column seeds (stable per xCell)
    float2 rnd = hash22(float2(xCell, 123.456));
    float phase   = rnd.x;                 // 0..1 phase offset (per column)
    float colJit  = (rnd.y - 0.5) * jitter; // small per-column offset

    // vertical head position for this column (in world Y units)
    float head = yBase + yHeight * frac(phase + t / max(1e-5, yHeight) + colJit);

    // --- trail band: 1 above head, fades within trailWidth ---
    // band = 1 when y <= head and within [head - trailWidth, head]
    float band = 1.0 - sstep(head - trailWidth, head, y);

    // --- per-column lateral mask (soft column center) ---
    // local x distance from the current column center
    float xCenter = (xCell + 0.5) * cellW;
    float dx = x - xCenter;

    // soft column profile: radius ≈ 0.35*cellW, softness ≈ 0.35*cellW
    float colRadius   = 0.35 * cellW;
    float colSoftness = 0.35 * cellW;
    float colSoft = softDisk(float2(dx, 0), colRadius, colSoftness); // 0 center → 1 outside
    float colMask = 1.0 - colSoft; // 1 at center, fades outward

    // --- head blob (round drop) sitting at the head line ---
    // we want a round blob in the lateral plane; use distance in (x vs column center) and (y vs head)
    float2 hv = float2(dx, y - head);
    float  diskSoft = softDisk(hv, dropSize, dropSize * 0.5); // 0 center → 1 outside
    float  headBlob = 1.0 - diskSoft;

    // --- combine: columns * band + head blob; clamp to [0,1] ---
    float trail = colMask * band;
    float mask  = saturate(trail + headBlob);

    // limit to our vertical region only (hard clip outside)
    mask *= step(0.0, y01) * step(y01, 1.0);

    dripMask = mask;
}
