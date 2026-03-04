using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class RenderParticlePass : ScriptableRenderPass {
    Material mat;

    public void Setup(Material material) {
        mat = material;
    }

    class PassData
    {
        public Material mat;
        public BufferHandle particleBufferHandle;
    };

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData) {
        using (var builder = renderGraph.AddRasterRenderPass("Render Particle", out PassData data)) {
            var particleRenderData = frameData.Get<ParticleRenderData>();

            data.mat = mat;
            data.particleBufferHandle = particleRenderData.particleBuffer;

            builder.UseBuffer(particleRenderData.particleBuffer, AccessFlags.Read);
            builder.SetRenderFunc<PassData>(ExecutePass);
        }
    }

    static void ExecutePass(PassData data, RasterGraphContext renderGraphContext)
    {
        
    }
}
