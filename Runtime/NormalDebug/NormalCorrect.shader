Shader "Unlit/NormalCorrect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NormalScale("NormalScale",Range(0.0,2.0)) = 1.0
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "UniversalMaterialType" = "Lit"
            "IgnoreProjector" = "True"
        }
        LOD 100

        Pass
        {
            Name "ForwardLit"
            Tags
            {
                "LightMode" = "UniversalForward"
            }
            Cull Back
            ZTest LEqual
            ZWrite On
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma enable_d3d11_debug_symbols


            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes
            {
                float3 normalOS : NORMAL;
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float3 normalWS : NORMAL;
                float2 uv : TEXCOORD0;
                float4 positionHS : SV_POSITION;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            Varyings vert (Attributes input)
            {
                Varyings output = (Varyings)0;
                output.positionHS = TransformObjectToHClip(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS.xyz);
                //output.normalWS = TransformObjectToWorldDir(input.normalOS.xyz);
                output.uv = input.uv;
                return output;
            }

            float4 frag (Varyings input) : SV_Target
            {
                float3 color = input.normalWS;
                return float4(color, 1.0f);
            }
            ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back
            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            // -------------------------------------
            // Universal Pipeline keywords

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile_fragment _ LOD_FADE_CROSSFADE

            // This is used during shadow map generation to differentiate between directional and punctual light shadows, as they use different formulas to apply Normal Bias
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            // -------------------------------------
            // Includes
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
        Pass
        {
            Name "NormalDebug"
            Tags
            {
                "LightMode" = "NormalDebug"
            }
            ZWrite On
            ZTest LEqual
            Cull Off
            Blend Off
            
            HLSLPROGRAM
            #pragma target 2.0//据说需要大于等于4
            #pragma require geometry
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
            float _NormalScale;
            CBUFFER_END
            
            struct Attributes
            {
                float3 normalOS : NORMAL;
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float3 normalWS : NORMAL1;
                float3 positionWS : POSITION2;
            };

            struct GeomOutputs
            {
                float3 normalWS : NORMAL3;
                float4 positionHS : SV_POSITION;
            };
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            #pragma enable_d3d11_debug_symbols

            Varyings vert (Attributes input)
            {
                Varyings output = (Varyings)0;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS.xyz);
                //output.normalWS = TransformObjectToWorldDir(input.normalOS.xyz);
                return output;
            }

            [maxvertexcount(2)]
            void geom(point Varyings IN[1], inout LineStream<GeomOutputs> stream)
            {
                float3 p0 = IN[0].positionWS;
                float3 p1 = p0 + IN[0].normalWS * _NormalScale;

                GeomOutputs o = (GeomOutputs)0;
                o.positionHS = TransformWorldToHClip(p0);
                o.normalWS = IN[0].normalWS;
                stream.Append(o);

                o.positionHS = TransformWorldToHClip(p1);
                o.normalWS = IN[0].normalWS;
                stream.Append(o);
            }

            float4 frag (GeomOutputs input) : SV_Target
            {
                float3 color = input.normalWS;
                return float4(color, 1.0f);
            }
            ENDHLSL
        }
    }
}
