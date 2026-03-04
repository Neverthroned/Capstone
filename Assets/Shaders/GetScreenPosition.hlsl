void GetScreenPosition_float(float3 Position, out float2 ScreenPosition, out float2 ScreenPositionAspectRatio)
{
    float4 screen = ComputeScreenPos(TransformWorldToHClip(Position), _ProjectionParams.x);
    ScreenPosition = screen.xy / abs(screen.w);
    
    float aspectRatio = _ScreenParams.y / _ScreenParams.x;
    ScreenPositionAspectRatio = float2(ScreenPosition.x, ScreenPosition.y * aspectRatio);
}
void MirrorUVCoordinates_float(float2 UVs, out float2 NewUVs)
{
    // Mirror U coordinate
    if (UVs.x < 0.0 || UVs.x > 1.0)
        NewUVs.x = 1.0 - abs((UVs.x - 2.0 * floor(UVs.x / 2.0)) - 1.0);
    else
        NewUVs.x = UVs.x;

    // Mirror V coordinate
    if (UVs.y < 0.0 || UVs.y > 1.0)
        NewUVs.y = 1.0 - abs((UVs.y - 2.0 * floor(UVs.y / 2.0)) - 1.0);
    else
        NewUVs.y = UVs.y;

}