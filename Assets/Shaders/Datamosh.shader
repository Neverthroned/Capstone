Shader "Unlit/Datamosh"
{
    Properties
    {
        _SnapshotTex ("Snapshot (RGBA)", 2D) = "white" {}
        _CurrentFrame ("Current Frame", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderPipeline"="HDRenderPipeline" }
        Pass
        {
            Name "CompositePass"
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

            TEXTURE2D(_SnapshotTex);  SAMPLER(sampler_SnapshotTex);
            TEXTURE2D(_CurrentFrame); SAMPLER(sampler_CurrentFrame);

            struct Attributes { uint vertexID : SV_VertexID; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 texcoord : TEXCOORD0; };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
                output.texcoord   = GetFullScreenTriangleTexCoord(input.vertexID);
                return output;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.texcoord;

                float4 live = SAMPLE_TEXTURE2D(_CurrentFrame, sampler_CurrentFrame, uv);
                float4 snap = SAMPLE_TEXTURE2D(_SnapshotTex, sampler_SnapshotTex, uv);

                // Composite: snapshot drives blending (alpha pre-updated by compute)
                float3 outCol = lerp(live.rgb, snap.rgb, snap.a);

                return float4(outCol, 1.0);
            }
            ENDHLSL
        }
    }
    FallBack Off
}