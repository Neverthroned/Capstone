#ifndef PROCEDURAL_NOISE_INCLUDED
#define PROCEDURAL_NOISE_INCLUDED

// -----------------------------
// Fade function
inline void Fade_float(float t, out float Out)
{
    Out = t * t * t * (t * (t * 6 - 15) + 10);
}

// -----------------------------
// Hash function
inline void Hash_float(float2 p, out float Out)
{
    Out = frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453123);
}

// -----------------------------
// Gradient function
inline void Grad_float(float2 fp, float2 ip, out float Out)
{
    float h;
    Hash_float(ip, h);
    float angle = h * 6.2831853; // 2ƒÎ
    float2 g = float2(cos(angle), sin(angle));
    Out = dot(g, fp);
}

// -----------------------------
// 2D Perlin noise
inline void Perlin2D_float(float2 p, out float Out)
{
    float2 ip = floor(p);
    float2 fp = frac(p);

    float uX, uY;
    Fade_float(fp.x, uX);
    Fade_float(fp.y, uY);
    float2 u = float2(uX, uY);

    float n00, n10, n01, n11;
    Grad_float(fp - float2(0, 0), ip + float2(0, 0), n00);
    Grad_float(fp - float2(1, 0), ip + float2(1, 0), n10);
    Grad_float(fp - float2(0, 1), ip + float2(0, 1), n01);
    Grad_float(fp - float2(1, 1), ip + float2(1, 1), n11);

    float nx0 = lerp(n00, n10, u.x);
    float nx1 = lerp(n01, n11, u.x);
    Out = lerp(nx0, nx1, u.y);
}

// -----------------------------
// fBm (adjustable octaves, optional timeOffset)
inline void FBM2D_float(float2 p, float octaves, float2 timeOffset, out float Out)
{
    p += timeOffset;

    float value = 0.0;
    float amplitude = 0.5;
    float frequency = 1.0;

    for (int i = 0; i < 10; i++) // max octaves
    {
        if (i >= (int) octaves)
            break;
        float n;
        Perlin2D_float(p * frequency, n);
        value += amplitude * n;
        frequency *= 2.0;
        amplitude *= 0.5;
    }

    // remap -1..1 -> 0..1
    Out = value * 0.5 + 0.5;
}

#endif