#ifndef WHITENOISE_INCLUDED
#define WHITENOISE_INCLUDED

// HDRP Shader Graph compatible 2D white noise
// Note the change to 'void' and the 'out float Out' parameter.
inline void WhiteNoise2D_float(float2 uv, out float Out)
{
    // Mix coordinates into a pseudo-random number
    float dotProduct = dot(uv, float2(12.9898, 78.233));
    
    // Assign the result to the 'Out' parameter instead of returning it
    Out = frac(sin(dotProduct) * 43758.5453);
}

inline void WhiteNoise3D_float(float3 p, out float Out)
{
    float dotProduct = dot(p, float3(12.9898, 78.233, 37.719));
    Out = frac(sin(dotProduct) * 43758.5453);
}

inline void WhiteNoise3D_RGB_float(float3 p, out float3 Out)
{
    float n1 = frac(sin(dot(p, float3(12.9898, 78.233, 37.719))) * 43758.5453);
    float n2 = frac(sin(dot(p, float3(39.3468, 11.135, 83.155))) * 24634.6345);
    float n3 = frac(sin(dot(p, float3(73.156, 52.235, 09.151))) * 56445.2345);

    Out = float3(n1, n2, n3);
}
#endif