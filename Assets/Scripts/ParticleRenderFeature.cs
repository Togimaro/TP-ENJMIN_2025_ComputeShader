using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class ParticleRenderData : ContextItem {
    public BufferHandle particleBuffer;

    public override void Reset() {
        particleBuffer = BufferHandle.nullHandle;
    }
}

public class ParticleRenderFeature : ScriptableRendererFeature {
    const int NUM_PARTICLE = 0x8000;

    [SerializeField]
    ComputeShader updateComputeShader;
    [SerializeField]
    Material particleMaterial;

    UpdateParticlePass updatePass;
    RenderParticlePass renderPass;

    public override void Create() {
        updatePass = new UpdateParticlePass(NUM_PARTICLE);
        updatePass.renderPassEvent = RenderPassEvent.BeforeRendering;

        renderPass = new RenderParticlePass(NUM_PARTICLE);
        renderPass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
        updatePass.Setup(updateComputeShader);
        renderer.EnqueuePass(updatePass);
        renderPass.Setup(particleMaterial);
        renderer.EnqueuePass(renderPass);
    }
}
