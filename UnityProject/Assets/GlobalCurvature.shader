Shader "Custom/GlobalCurvature"
{
    Properties
    {
        _CurveStrength ("Curve Strength", Float) = 0.001
        _CurveStartDistance ("Curve Start Distance", Float) = 10.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100

        Pass
        {
            Name "ForwardLit"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
            };

            CBUFFER_START(UnityPerMaterial)
                float _CurveStrength;
                float _CurveStartDistance;
            CBUFFER_END

            Varyings vert(Attributes input) 
            {
                Varyings output;
                
                float3 worldPos = TransformObjectToWorld(input.positionOS.xyz);
                float3 cameraPos = _WorldSpaceCameraPos;
                float distZ = max(0.0, abs(worldPos.z - cameraPos.z) - _CurveStartDistance);

                worldPos.y -= _CurveStrength * (distZ * distZ);

                output.positionCS = TransformWorldToHClip(worldPos);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                return half4(1,1,1,1);
            }
            ENDHLSL
        }
    }
}