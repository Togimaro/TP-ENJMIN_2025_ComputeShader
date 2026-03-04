Shader "RenderParticleShader"
{
    Properties
    {
        [HDR] _Color("Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { 
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Opaque"
            "UniversalMaterialType" = "Lit"
            "Queue"="Geometry"
            "DisableBatching"="False"
        }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vertexShader
            #pragma fragment pixelShader
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct ParticleData
            {
                float3 position;
            };
            StructuredBuffer<ParticleData> particleBuffer;

            struct VertexOutput {
                float4 position : SV_POSITION;
            };

            float4 _Color;

            VertexOutput vertexShader(uint vertexId : SV_VertexID) {
                VertexOutput o = (VertexOutput)0;
                o.position = TransformObjectToHClip(particleBuffer[vertexId].position);
                return o;
            }

            float4 pixelShader(VertexOutput i) : SV_Target {
                return _Color;
            }
            ENDHLSL
        }
    }
}
