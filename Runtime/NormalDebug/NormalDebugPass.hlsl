#ifndef UNIVERSAL_FORWARD_LIT_PASS_INCLUDED
#define UNIVERSAL_FORWARD_LIT_PASS_INCLUDED

//CBUFFER_START(UnityPerMaterial)
float _NormalScale;
//CBUFFER_END

struct Attributes
{
    float3 normalOS   : NORMAL;
    float4 tangentOS  : TANGENT;
    float3 positionOS : POSITION;
    float2 uv         : TEXCOORD0;
};

struct Varyings
{
    float3 normalDir  : NORMAL;
    float3 positionWS : TEXCOORD0;
};

struct GeomOutputs
{
    float3 normalDir  : NORMAL;
    float4 positionHS : SV_POSITION;
};

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

Varyings NormalDebugVertex(Attributes input)
{
    Varyings output = (Varyings)0;
    output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
    #ifdef _NORMAL_UNCORRECTED
    output.normalDir = TransformObjectToWorldDir(input.normalOS.xyz, true);
    #else
    output.normalDir = TransformObjectToWorldNormal(input.normalOS.xyz, true);
    #endif

    return output;
}

[maxvertexcount(6)]
void NormalDebugGeometry(triangle Varyings IN[3], inout LineStream<GeomOutputs> stream)
{
    for (int i = 0; i < 3; ++i)
    {
        float3 p0 = IN[i].positionWS;
        float3 p1 = p0 + IN[i].normalDir * _NormalScale;

        GeomOutputs o = (GeomOutputs)0;
        o.positionHS = TransformWorldToHClip(p0);
        o.normalDir = IN[i].normalDir;
        stream.Append(o);

        o.positionHS = TransformWorldToHClip(p1);
        o.normalDir = IN[i].normalDir;
        stream.Append(o);

        stream.RestartStrip();
    }
}

float4 NormalDebugFragment(GeomOutputs input) : SV_Target
{
    #ifdef _COLOR_REMAP
    input.normalDir=(input.normalDir+float3(1.0f,1.0f,1.0f)) * 0.5f;
    #endif
    float3 color = input.normalDir;
    return float4(color, 1.0f);
}
#endif
