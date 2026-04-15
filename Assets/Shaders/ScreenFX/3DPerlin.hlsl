#ifndef THREED_PERLIN
#define THREED_PERLIN

// -----------------------------
// Fade function
inline float Fade(float t)
{
    return t * t * t * (t * (t * 6 - 15) + 10);
}

// -----------------------------
// Hash function (3D)
inline float Hash(float3 p)
{
    p = frac(p * float3(0.1031, 0.1030, 0.0973));
    p += dot(p, p.yxz + 33.33);
    return frac((p.x + p.y) * p.z);
}

// -----------------------------
// Gradient function (3D)
inline float Grad(float3 ip, float3 fp)
{
    float h = Hash(ip);
    float3 g = float3(
        frac(h * 7.0) * 2.0 - 1.0,
        frac(h * 13.0) * 2.0 - 1.0,
        frac(h * 17.0) * 2.0 - 1.0
    );
    return dot(g, fp);
}

// -----------------------------
// 3D Perlin noise
inline void Perlin3D_float(float3 p, out float Out)
{
    float3 ip = floor(p);
    float3 fp = frac(p);

    float3 u = float3(
        Fade(fp.x),
        Fade(fp.y),
        Fade(fp.z)
    );

    float n000 = Grad(ip + float3(0, 0, 0), fp - float3(0, 0, 0));
    float n100 = Grad(ip + float3(1, 0, 0), fp - float3(1, 0, 0));
    float n010 = Grad(ip + float3(0, 1, 0), fp - float3(0, 1, 0));
    float n110 = Grad(ip + float3(1, 1, 0), fp - float3(1, 1, 0));

    float n001 = Grad(ip + float3(0, 0, 1), fp - float3(0, 0, 1));
    float n101 = Grad(ip + float3(1, 0, 1), fp - float3(1, 0, 1));
    float n011 = Grad(ip + float3(0, 1, 1), fp - float3(0, 1, 1));
    float n111 = Grad(ip + float3(1, 1, 1), fp - float3(1, 1, 1));

    float nx00 = lerp(n000, n100, u.x);
    float nx10 = lerp(n010, n110, u.x);
    float nx01 = lerp(n001, n101, u.x);
    float nx11 = lerp(n011, n111, u.x);

    float nxy0 = lerp(nx00, nx10, u.y);
    float nxy1 = lerp(nx01, nx11, u.y);

    Out = lerp(nxy0, nxy1, u.z);
}

// -----------------------------
// fBm using 3D noise (time = Z)
inline void FBM3D_float(float2 p, float time, float octaves, out float Out)
{
    float value = 0.0;
    float amplitude = 0.5;
    float frequency = 1.0;
    float maxValue = 0.0;

    for (int i = 0; i < 10; i++)
    {
        if (i >= (int) octaves)
            break;

        float3 p3 = float3(p * frequency, time); // only scale XY, not time
        float n;
        Perlin3D_float(p3, n);
        value += amplitude * n;
        maxValue += amplitude;
        frequency *= 2.0;
        amplitude *= 0.5;
    }

    Out = (value / maxValue) * 0.5 + 0.5;
}

#endif