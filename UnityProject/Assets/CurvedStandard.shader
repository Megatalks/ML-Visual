Shader "Custom/CurvedStandard"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BaseColor ("Color Tint", Color) = (1, 1, 1, 1)
        _CurveStrength ("Curve Strength", Float) = 0.0008
        _CurveStartDistance ("Curve Start Distance", Float) = 15.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float3 normalWS     : TEXCOORD1;
                float3 worldPos     : TEXCOORD3;
                float2 uv           : TEXCOORD0;
            };

            Texture2D _MainTex;
            SamplerState sampler_MainTex;

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _BaseColor;
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

                output.worldPos = worldPos;
                output.positionCS = TransformWorldToHClip(worldPos);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = input.uv;
                
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 albedo = _MainTex.Sample(sampler_MainTex, input.uv) * _BaseColor;

                float4 shadowCoord = TransformWorldToShadowCoord(input.worldPos);

                Light mainLight = GetMainLight(shadowCoord);

                float3 normal = normalize(input.normalWS);
                float NdotL = saturate(dot(normal, mainLight.direction));

                float3 ambientLight = SampleSH(normal);

                float3 lightColor = mainLight.color * (NdotL * mainLight.shadowAttenuation);
                float3 finalColor = albedo.rgb * (lightColor + ambientLight);

                return half4(finalColor, albedo.a);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }

            ColorMask 0

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
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

                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                #if _MAIN_LIGHT_SHADOWS
                    float3 lightDir = _MainLightPosition.xyz;
                    worldPos = ApplyShadowBias(worldPos, normalWS, lightDir);
                #endif

                output.positionCS = TransformWorldToHClip(worldPos);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }
}