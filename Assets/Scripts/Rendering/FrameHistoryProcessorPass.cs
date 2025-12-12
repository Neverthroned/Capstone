using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class FrameHistoryProcessorPass : CustomPass
{
    [Header("Output (optional): if set, RawImage can read this")]
    public RenderTexture uiOutput;          // assign if you want RawImage to show the processed frame

    [Header("Material (Shader Graph)")]
    public Material fullscreenMaterial;     // must expect _CurrentFrame and _HistoryTex

    RTHandle historyA;
    RTHandle historyB;
    bool initialized;

    protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
    {
        initialized = false;
    }

    void EnsureHistoryBuffers(CustomPassContext ctx)
    {
        if (initialized) return;

        var desc = ctx.cameraColorBuffer;
        // Allocate two history buffers matching the camera color buffer (size/format)
        historyA = RTHandles.Alloc(desc, name: "HistoryA");
        historyB = RTHandles.Alloc(desc, name: "HistoryB");

        // Initialize history with the first frame so you don't get a black frame on start
        ctx.cmd.Blit(ctx.cameraColorBuffer, historyA);
        initialized = true;
    }

    protected override void Execute(CustomPassContext ctx)
    {
        if (fullscreenMaterial == null) return;

        EnsureHistoryBuffers(ctx);

        // Read targets for this frame
        var current = ctx.cameraColorBuffer;
        var previous = historyA; // "prev" this frame

        // Bind inputs expected by your Shader Graph material (via Custom Function HLSL)
        ctx.propertyBlock.SetTexture("_CurrentFrame", current);
        ctx.propertyBlock.SetTexture("_HistoryTex", previous);

        // (Optional) you can also push effect selectors/strengths here:
        // ctx.propertyBlock.SetFloat("_EffectMode", 0); // 0=denoise, 1=datamosh
        // ctx.propertyBlock.SetFloat("_Strength", 0.75f);

        // Render full-screen into the other history buffer (this frame's processed output)
        CoreUtils.SetRenderTarget(ctx.cmd, historyB, ClearFlag.None);
        CoreUtils.DrawFullScreen(ctx.cmd, fullscreenMaterial, ctx.propertyBlock, shaderPassId: 0);

        // If you want the UI RawImage to display the processed frame, push it here:
        if (uiOutput != null)
        {
            ctx.cmd.Blit(historyB, uiOutput);
        }

        // Ping-pong swap: next frame, historyB becomes "previous"
        var tmp = historyA;
        historyA = historyB;
        historyB = tmp;
    }

    protected override void Cleanup()
    {
        if (historyA != null) historyA.Release();
        if (historyB != null) historyB.Release();
    }
}