#ifndef FRAME_EFFECTS_INCLUDED
#define FRAME_EFFECTS_INCLUDED

// --- helpers (simple but effective for found-footage style) ---
float3 RGBtoYUV(float3 rgb)
{
    float Y = dot(rgb, float3(0.299, 0.587, 0.114));
    float U = dot(rgb, float3(-0.14713, -0.28886, 0.436));
    float V = dot(rgb, float3(0.615, -0.51499, -0.10001));
    return float3(Y, U, V);
}

float3 YUVtoRGB(float3 yuv)
{
    float Y = yuv.x, U = yuv.y, V = yuv.z;
    return float3(
        Y + 1.13983 * V,
        Y - 0.39465 * U - 0.58060 * V,
        Y + 2.03211 * U
    );
}

// --- Effect 1: "denoiser" (intentionally broken = smear + chroma bleed) ---
float3 DenoiseLike(Texture2D currentFrame, SamplerState samplerCurrentFrame,
                   Texture2D historyTex, SamplerState samplerHistoryTex,
                   float2 uv, float smear, float chroma)
{
    float3 cur = currentFrame.Sample(samplerCurrentFrame, uv).rgb;
    float3 prev = historyTex.Sample(samplerHistoryTex, uv).rgb;
    
    // temporal smear (this is the "denoiser" that behaves like bad NR)
    float3 t = lerp(cur, prev, saturate(smear));
    
    // chroma bleed (VHS-like)
    float3 yt = RGBtoYUV(t);
    float3 yp = RGBtoYUV(prev);
    yt.yz = lerp(yt.yz, yp.yz, saturate(chroma));
    
    return YUVtoRGB(yt);
}

// --- Effect 2: lightweight "datamosh" (blocky frame mixing) ---
float Hash1(float2 p)
{
    return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
}

float3 Datamosh(Texture2D currentFrame, SamplerState samplerCurrentFrame,
                Texture2D historyTex, SamplerState samplerHistoryTex,
                float2 uv, float intensity, float block)
{
    // Block grid (bigger block => more obvious "macro-block" corruption)
    float2 cell = floor(uv / max(block, 1e-5));
    float r = Hash1(cell);
    
    // Choose a remapped UV that jumps inside the block (simulates bad reference frame fetch)
    float2 local = frac(uv / max(block, 1e-5));
    float2 jump = float2(frac(local.x + r), frac(local.y + (1.0 - r)));
    
    float3 cur = currentFrame.Sample(samplerCurrentFrame, uv).rgb;
    float3 prev = historyTex.Sample(samplerHistoryTex, uv).rgb;
    float3 prevJump = historyTex.Sample(samplerHistoryTex, cell * block + jump * block).rgb;
    
    // Mix: keep some current, add "wrong" previous blocks
    float w = saturate(intensity);
    return lerp(cur, lerp(prev, prevJump, 0.65), w);
}

// --- Single entry point for Shader Graph (mode selects which effect runs) ---
void ProcessFrame_float(Texture2D currentFrame, SamplerState samplerCurrentFrame,
                        Texture2D historyTex, SamplerState samplerHistoryTex,
                        float2 uv, float effectMode, float strength, float chromaOrBlock,
                        out float3 outColor)
{
    if (effectMode < 0.5)
    {
        outColor = DenoiseLike(currentFrame, samplerCurrentFrame, historyTex, samplerHistoryTex,
                               uv, strength, chromaOrBlock);
    }
    else
    {
        outColor = Datamosh(currentFrame, samplerCurrentFrame, historyTex, samplerHistoryTex,
                            uv, strength, chromaOrBlock);
    }
}

#endif // FRAME_EFFECTS_INCLUDED