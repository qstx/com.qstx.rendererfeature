Shader "QSTXRendererFeature/Occluders"
{
    Properties
    {
        _Color("Main Color",Color)=(0.0,0.0,0.0,0.0)
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        Fog{Mode Off}
        Color[_Color]
        LOD 100

        Pass
        {
        }
    }
}