Shader "Custom/StylizedGradientSkybox"
{
    Properties
    {
        _SkyColor ("Sky Color (Top)", Color) = (0.05, 0.05, 0.2, 1)
        _HorizonColor ("Horizon Color (Middle)", Color) = (0.4, 0.2, 0.4, 1)
        _GroundColor ("Ground Color (Bottom)", Color) = (0.1, 0.05, 0.1, 1)
        _Exponent1 ("Sky-to-Horizon Sharpness", Float) = 3.0
        _Exponent2 ("Horizon-to-Ground Sharpness", Float) = 3.0
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off

        Pass
        {
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
                float3 viewDir      : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _SkyColor;
                half4 _HorizonColor;
                half4 _GroundColor;
                float _Exponent1;
                float _Exponent2;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                
                #if UNITY_REVERSED_Z
                    output.positionCS.z = 0.0; 
                #else
                    output.positionCS.z = output.positionCS.w; 
                #endif
                
                output.viewDir = input.positionOS.xyz;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 d = normalize(input.viewDir);
                float y = d.y; 

                half3 finalColor;

                if (y >= 0.0)
                {
                    float blend = pow(1.0 - y, _Exponent1);
                    finalColor = lerp(_SkyColor.rgb, _HorizonColor.rgb, blend);
                }
                else
                {
                    float blend = pow(1.0 + y, _Exponent2);
                    finalColor = lerp(_GroundColor.rgb, _HorizonColor.rgb, blend);
                }

                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}