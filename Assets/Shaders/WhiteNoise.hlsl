#ifndef WHITENOISE_INCLUDED
#define WHITENOISE_INCLUDED

// HDRP Shader Graph compatible 2D white noise
// Note the change to 'void' and the 'out float Out' parameter.
inline void WhiteNoise_float(float2 uv, out float Out)
{
    // Mix coordinates into a pseudo-random number
    float dotProduct = dot(uv, float2(12.9898, 78.233));
    
    // Assign the result to the 'Out' parameter instead of returning it
    Out = frac(sin(dotProduct) * 43758.5453);
}

#endif