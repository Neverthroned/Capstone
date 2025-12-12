using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

[System.Serializable]
public class DatamoshPass : CustomPass
{
    [SerializeField] public ComputeShader datamoshCompute;
    [Range(0f, 2f)][SerializeField] public float decay = 0.98f;
    [SerializeField] public float motionScale = 1.0f;
    [SerializeField] public float noiseScale = 0.5f;
    [SerializeField] public float noiseSpeed = 1.0f;
    [SerializeField] public Color tint = Color.white;
    [Range(1, 5)][SerializeField] int kernelRadius = 2;
    [SerializeField] float motionDiffThreshold = 0.03f;

    [SerializeField] Material datamoshMat;

    // RTHandles
    RTHandle snapshotRT = null;
    RTHandle tempRT = null;

    bool snapshotTaken = false;
    bool snapshotCopied = false;

    int kernelCS = -1;

    protected override void Setup(ScriptableRenderContext ctx, CommandBuffer cmd)
    {
        if (datamoshCompute != null)
            kernelCS = datamoshCompute.FindKernel("CSMain");
    }

    protected override void Execute(CustomPassContext ctx)
    {
        if (datamoshCompute == null)
        {
            Debug.LogError("DatamoshPass: Compute shader not assigned!");
            return;
        }

        if (kernelCS < 0)
        {
            Debug.LogError("DatamoshPass: Invalid kernel index!");
            return;
        }

        if (!snapshotTaken) return;

        EnsureRTsMatchCamera(ctx);

        var camRT = ctx.cameraColorBuffer.rt;
        int w = camRT.width;
        int h = camRT.height;

        // 1. Copy camera buffer to tempRT
        HDUtils.BlitCameraTexture(ctx.cmd, ctx.cameraColorBuffer, tempRT);

        // 2. Initialize snapshotRT if first frame
        if (!snapshotCopied)
        {
            ctx.cmd.CopyTexture(tempRT.rt, snapshotRT.rt);
            snapshotCopied = true;
        }

        // 3. Dispatch compute shader
        ctx.cmd.SetComputeTextureParam(datamoshCompute, kernelCS, "_SnapshotTex", snapshotRT.rt);
        ctx.cmd.SetComputeTextureParam(datamoshCompute, kernelCS, "_CurrentFrame", tempRT.rt);
        ctx.cmd.SetComputeFloatParam(datamoshCompute, "_Decay", decay);
        ctx.cmd.SetComputeFloatParam(datamoshCompute, "_NoiseScale", noiseScale);
        ctx.cmd.SetComputeFloatParam(datamoshCompute, "_NoiseSpeed", noiseSpeed);
        ctx.cmd.SetComputeFloatParam(datamoshCompute, "_MotionDiffThreshold", motionDiffThreshold);
        ctx.cmd.SetComputeVectorParam(datamoshCompute, "_Tint", tint); // Use SetComputeVectorParam for color
        ctx.cmd.SetComputeFloatParam(datamoshCompute, "_Time", Time.time);
        ctx.cmd.SetComputeIntParam(datamoshCompute, "_KernelRadius", 2);

        ctx.cmd.DispatchCompute(datamoshCompute, kernelCS, Mathf.CeilToInt(w / 8.0f), Mathf.CeilToInt(h / 8.0f), 1);

        // 4. Draw fullscreen using Datamosh material
        if (datamoshMat != null)
        {
            datamoshMat.SetTexture("_SnapshotTex", snapshotRT);
            datamoshMat.SetTexture("_CurrentFrame", tempRT);
            CoreUtils.SetRenderTarget(ctx.cmd, ctx.cameraColorBuffer, ClearFlag.None);
            CoreUtils.DrawFullScreen(ctx.cmd, datamoshMat, ctx.cameraColorBuffer);
        }
    }

    protected override void Cleanup()
    {
        snapshotRT?.Release(); snapshotRT = null;
        tempRT?.Release(); tempRT = null;
    }

    void EnsureRTsMatchCamera(CustomPassContext ctx)
    {
        var camRT = ctx.cameraColorBuffer.rt;
        if (camRT == null)
        {
            Debug.LogError("DatamoshPass: Camera RT null");
            return;
        }

        int w = camRT.width;
        int h = camRT.height;

        if (snapshotRT == null || snapshotRT.rt.width != w || snapshotRT.rt.height != h)
        {
            snapshotRT?.Release();
            tempRT?.Release();

            // Use high precision format for RGBA
            snapshotRT = RTHandles.Alloc(w, h, colorFormat: GraphicsFormat.R32G32B32A32_SFloat, enableRandomWrite: true);
            tempRT = RTHandles.Alloc(w, h, colorFormat: GraphicsFormat.R32G32B32A32_SFloat, enableRandomWrite: true);


            snapshotCopied = false;
            Debug.Log($"DatamoshPass: allocated RTs W:{w} H:{h}");
        }
    }

    public void TakeSnapshot()
    {
        snapshotTaken = true;
        snapshotCopied = false;
        Debug.Log("DatamoshPass: TakeSnapshot called");
    }

    public void EndEffect()
    {
        snapshotTaken = false;
        snapshotCopied = false;
        snapshotRT?.Release(); snapshotRT = null;
        tempRT?.Release(); tempRT = null;
        Debug.Log("DatamoshPass: EndEffect called and snapshot cleared");
    }
}