Shader "ZombieWar/ZombieDissolve"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.22, 0.48, 0.16, 1)
        _HitFlash ("Hit Flash", Range(0, 1)) = 0
        _DissolveAmount ("Dissolve", Range(0, 1)) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half _HitFlash;
                half _DissolveAmount;
            CBUFFER_END

            Varyings Vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs position = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = position.positionCS;
                output.positionWS = position.positionWS;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half noise = frac(sin(dot(input.positionWS.xz * 5.7, half2(12.9898, 78.233))) * 43758.5453);
                clip(noise - _DissolveAmount);
                half3 color = lerp(_BaseColor.rgb, half3(1, 0.92, 0.75), _HitFlash);
                return half4(color, 1);
            }
            ENDHLSL
        }
    }
}
