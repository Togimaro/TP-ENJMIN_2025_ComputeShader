using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class RenderParticlePass : ScriptableRenderPass {
    Material mat;
    int numParticles;

    public RenderParticlePass(int numParticles) {
        this.numParticles = numParticles;
    }

    public void Setup(Material material) {
        mat = material;
    }

    class PassData {
        public Material mat;
        public BufferHandle particleBufferHandle;
        public int numParticles;
    };

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData) {
        using (var builder = renderGraph.AddRasterRenderPass("Render Particle", out PassData data)) {
            var particleRenderData = frameData.Get<ParticleRenderData>();

            data.mat = mat;
            data.particleBufferHandle = particleRenderData.particleBuffer;
            data.numParticles = numParticles;

            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            builder.SetRenderAttachment(resourceData.activeColorTexture, 0, AccessFlags.Write);
            builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture, AccessFlags.Write);

            builder.UseBuffer(particleRenderData.particleBuffer, AccessFlags.Read);
            builder.SetRenderFunc<PassData>(ExecutePass);
        }
    }

    static void ExecutePass(PassData data, RasterGraphContext ctx) {
        data.mat.SetBuffer("particleBuffer", data.particleBufferHandle);
        ctx.cmd.DrawProcedural(Matrix4x4.identity, data.mat, 0, MeshTopology.Points, data.numParticles);
    }
}
