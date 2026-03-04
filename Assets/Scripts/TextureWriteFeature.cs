using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class TextureWriteFeature : ScriptableRendererFeature {
    [SerializeField]
    ComputeShader textureWriteComputeShader;
    [SerializeField]
    RenderTexture rt;

    TextureWritePass textureWritePass;

    public override void Create() {
        textureWritePass = new TextureWritePass();
        textureWritePass.renderPassEvent = RenderPassEvent.BeforeRendering;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
        textureWritePass.Setup(textureWriteComputeShader, rt);
        renderer.EnqueuePass(textureWritePass);
    }
}
