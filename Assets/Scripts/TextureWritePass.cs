using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class TextureWritePass : ScriptableRenderPass {
    ComputeShader cs;
    RenderTexture rt;

    public TextureWritePass() {
    }

    public void Setup(ComputeShader computeShader, RenderTexture rt) {
        this.cs = computeShader;
        this.rt = rt;
    }

    class PassData {
        public ComputeShader cs;
        public TextureHandle textureHandle;
    };

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData) {
        using (var builder = renderGraph.AddComputePass("Write Texture", out PassData data)) {
            data.cs = cs;

            var rtHandle = RTHandles.Alloc(rt);
            data.textureHandle = renderGraph.ImportTexture(rtHandle);

            builder.UseTexture(data.textureHandle, AccessFlags.ReadWrite);
            builder.SetRenderFunc<PassData>(ExecutePass);
        }
    }

    static void ExecutePass(PassData data, ComputeGraphContext ctx) {
        if (!Application.isPlaying || EditorApplication.isPaused) return;
        int kernelId = data.cs.FindKernel("CSMain");

        ctx.cmd.SetComputeTextureParam(data.cs, kernelId, "tex", data.textureHandle);
        data.cs.GetKernelThreadGroupSizes(kernelId, out uint threadCountX, out uint threadCountY, out uint threadCountZ);
        ctx.cmd.DispatchCompute(data.cs, kernelId, (int)(256 / threadCountX), (int)(256 / threadCountY), 1);
    }
}
