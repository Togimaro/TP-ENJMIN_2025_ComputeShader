Shader "RenderParticleShader"
{
    Properties
    {
        [HDR] _ColorSlow("Color Slow", Color) = (1, 1, 1, 1)
        [HDR] _ColorFast("Color Fast", Color) = (1, 1, 1, 1)
        _MinVel("Minimum Velocity", Float) = 0
        _VelScale("Velocity Scale", Float) = 1
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
            #pragma enable_d3d11_debug_symbols
            #pragma vertex vertexShader
            #pragma fragment pixelShader
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct ParticleData {
                float3 position;
                float3 velocity;
            };
            StructuredBuffer<ParticleData> particleBuffer;

            struct VertexOutput {
                float4 position : SV_POSITION;
                float4 color : COLOR0;
            };

            float4 _ColorSlow;
            float4 _ColorFast;
            float _MinVel;
            float _VelScale;

            VertexOutput vertexShader(uint vertexId : SV_VertexID) {
                VertexOutput o = (VertexOutput)0;
                o.position = TransformObjectToHClip(particleBuffer[vertexId].position);

                float speed = length(particleBuffer[vertexId].velocity);
                float speedClamped = saturate((speed - _MinVel) / _VelScale);
                o.color = lerp(_ColorSlow, _ColorFast, speedClamped);
                return o;
            }

            float4 pixelShader(VertexOutput input) : SV_Target {
                return input.color;
            }
            ENDHLSL
        }
    }
}
