Shader "QSTXRendererFeature/NormalAlbedoShow"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        //https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Shader.SetGlobalFloat.html
        //https://blog.csdn.net/weixin_37417198/article/details/121260036
        //_NormalScale("NormalScale",Range(0.0,2.0)) = 1.0
    }
    SubShader
    {
        HLSLINCLUDE
        #pragma multi_compile _ _NORMAL_UNCORRECTED
        #pragma multi_compile _ _COLOR_REMAP
        ENDHLSL
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
                float3 normalDir : NORMAL;
                float2 uv : TEXCOORD0;
                float4 positionHS : SV_POSITION;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            Varyings vert (Attributes input)
            {
                Varyings output = (Varyings)0;
                output.positionHS = TransformObjectToHClip(input.positionOS.xyz);
                #ifdef _NORMAL_UNCORRECTED
                output.normalDir = TransformObjectToWorldDir(input.normalOS.xyz,true);
                #else
                output.normalDir = TransformObjectToWorldNormal(input.normalOS.xyz,true);
                #endif
                output.uv = input.uv;
                return output;
            }

            float4 frag (Varyings input) : SV_Target
            {
                
                #ifdef _COLOR_REMAP
                input.normalDir=(input.normalDir+float3(1.0f,1.0f,1.0f)) * 0.5f;
                #endif
                float3 color = input.normalDir;
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
                "RenderPipeline" = "UniversalRenderPipeline"
            }
            ZWrite On
            ZTest LEqual
            Cull Back
            Blend Off
            
            HLSLPROGRAM
            #pragma target 2.0//据说需要大于等于4，不然无法支持几何着色器
            #pragma require geometry
            #pragma enable_d3d11_debug_symbols
            #pragma multi_compile _ _NORMAL_UNCORRECTED
            #pragma multi_compile _ _COLOR_REMAP
            
            #include "Packages/com.qstx.rendererfeature/Runtime/NormalDebug/NormalDebugPass.hlsl"
 
            #pragma vertex NormalDebugVertex
            #pragma geometry NormalDebugGeometry
            #pragma fragment NormalDebugFragment

            ENDHLSL
        }
    }
}
