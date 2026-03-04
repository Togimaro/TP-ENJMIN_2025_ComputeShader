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
    int numParticles;

    struct ParticleData {
        public Vector3 position;
        public Vector3 velocity;
    };

    public UpdateParticlePass(int numParticles) {
        this.numParticles = numParticles;
        particleBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, numParticles, Marshal.SizeOf(typeof(ParticleData)));

        var list = new List<ParticleData>();
        for (int i = 0; i < numParticles; i++) {
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
        public int numParticles;
    };

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData) {
        var bufferHandle = renderGraph.ImportBuffer(particleBuffer);

        using (var builder = renderGraph.AddComputePass("Update Particle", out PassData data)) {
            var customData = frameData.GetOrCreate<ParticleRenderData>();
            customData.particleBuffer = bufferHandle;

            data.cs = cs;
            data.particleBufferHandle = bufferHandle;
            data.numParticles = numParticles;

            builder.UseBuffer(bufferHandle, AccessFlags.ReadWrite);
            builder.SetRenderFunc<PassData>(ExecutePass);
        }
    }

    static void ExecutePass(PassData data, ComputeGraphContext ctx) {
        if (!Application.isPlaying || EditorApplication.isPaused) return;
        int kernelId = data.cs.FindKernel("CSMain");
        ctx.cmd.SetComputeBufferParam(data.cs, kernelId, "particleBuffer", data.particleBufferHandle);
        ctx.cmd.SetComputeVectorParam(data.cs, "unity_DeltaTime", Shader.GetGlobalVector("unity_DeltaTime"));
        ctx.cmd.SetComputeFloatParam(data.cs, "simulationSpeed", 0.1f);

        data.cs.GetKernelThreadGroupSizes(kernelId, out uint threadCountX, out uint threadCountY, out uint threadCountZ);
        ctx.cmd.DispatchCompute(data.cs, kernelId, (int)(data.numParticles / threadCountX), 1, 1);
    }
}
