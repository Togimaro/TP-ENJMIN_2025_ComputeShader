using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class UpdateParticlePass : ScriptableRenderPass {
    ComputeShader cs;
    GraphicsBuffer particleBuffer;

    struct ParticleData
    {
        public Vector3 position;
    };

    public UpdateParticlePass()
    {
        particleBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 1024, Marshal.SizeOf(typeof(ParticleData)));

        var list = new List<ParticleData>();
        for (int i = 0; i < 1024; i++)
        {
            var data = new ParticleData();
            data.position = UnityEngine.Random.insideUnitSphere * 5.0f;
            list.Add(data);
        }
        particleBuffer.SetData(list);
    }

    public void Setup(ComputeShader updateComputeShader) {
        cs = updateComputeShader;
    }

    class PassData {
        public ComputeShader cs;
        public BufferHandle particleBufferHandle;
    };

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData) {
        var bufferHandle = renderGraph.ImportBuffer(particleBuffer);

        using (var builder = renderGraph.AddComputePass("Update Particle", out PassData data)) {
            var customData = frameData.GetOrCreate<ParticleRenderData>();
            customData.particleBuffer = bufferHandle;

            data.cs = cs;
            data.particleBufferHandle = bufferHandle;

            builder.UseBuffer(bufferHandle, AccessFlags.ReadWrite);
            builder.SetRenderFunc<PassData>(ExecutePass);
        }
    }

    static void ExecutePass(PassData data, ComputeGraphContext ctx) {
        if (!Application.isPlaying || EditorApplication.isPaused) return;
        int kernelId = data.cs.FindKernel("CSMain");
        ctx.cmd.SetComputeBufferParam(data.cs, kernelId, "particleBuffer", data.particleBufferHandle);
        ctx.cmd.DispatchCompute(data.cs, kernelId, 1, 1, 1);
    }
}
