Shader "Relive/InstancedLines"
{
    Properties
    {
        _lineWidth("LineWidth", Range(0., 0.1)) = 0.007

        [PerRendererData] _texWidth("INTERNAL", Int) = 2048
        [PerRendererData] _texHeight("INTERNAL", Int) = 1
        [PerRendererData] _transparencyMultiplier("Transparency Multiplier", Range(0., 1.)) = 1.
        [PerRendererData] _localToWorld("INTERNAL", Float) = 0
        [PerRendererData] _positions("INTERNAL", 2D) = "white" {}
        [PerRendererData] _color("INTERNAL", 2D) = "white" {}

        [PerRendererData] _startIndex("INTERNAL", Int) = 0
        [PerRendererData] _endIndex("INTERNAL", Int) = 0
    }


    CGINCLUDE

    #include "UnityCG.cginc"

    uniform uint _texWidth;
    uniform uint _texHeight;
    uniform float _lineWidth;
    uniform float _transparencyMultiplier;

    uniform float4x4 _localToWorld;

    uniform sampler2D _positions;
    uniform sampler2D _color;
    float4 _color_ST;

    uniform uint _startIndex;
    uniform uint _endIndex;


    struct Input
    {
        float4 vertex : POSITION;
        uint vertexId: SV_VertexID;
        float2 uv : TEXCOORD0;
    };

    struct v2f
    {
        float4 pos : SV_POSITION;
        float2 worldPos: TEXCOORD0;
        float4 color: TEXCOORD1;
        float instanceID: TEXCOORD2;
        float2 uv: TEXCOORD3;
    };



    v2f vert(Input input, uint instanceID : SV_InstanceID)
    {
        // *must* be integer division!
        float texRowStart = floor(instanceID / _texWidth);
        float texRowEnd = floor((instanceID + 1) / _texWidth);
        float adjTexHeight = max(_texHeight - 1, 1);
        float adjTexWidth = max(_texWidth - 1, 1);

        float2 texPosStart = float2((instanceID % _texWidth) / adjTexWidth, texRowStart / adjTexHeight);
        float2 texPosEnd = float2(((instanceID + 1) % _texWidth) / adjTexWidth, texRowEnd / adjTexHeight);

        float4 startPos = tex2Dlod(_positions, fixed4(texPosStart.x, texPosStart.y, 0, 0));
        float4 endPos = tex2Dlod(_positions, fixed4(texPosEnd.x, texPosEnd.y, 0, 0));

        float4 color = tex2Dlod(_color, fixed4(texPosStart.x, texPosStart.y, 0, 0));
        float transparency = color.a * _transparencyMultiplier;

        // vertices 0/2 -> start, 1/3 -> end
        uint pos = input.vertexId % 2;

        // => pos = 0 <=> start, pos = 1 <=> end
        float3 linePosition = (1 - pos) * startPos + pos * endPos;

        // move vertices up/down to span up a line
        float widthIndex = ((input.vertexId % 4) / 2 - 0.5) * 2;

        float3 lineWidthOffset = float3(1, 1, 1) * widthIndex * _lineWidth * transparency;

        float4 worldPos = mul(_localToWorld, float4(linePosition, 1));

        color.a = color.a * _transparencyMultiplier;

        v2f output;
        output.color = color;
        output.pos = UnityObjectToClipPos(worldPos) + float4(lineWidthOffset, 0);
        output.worldPos = worldPos;
        output.uv = TRANSFORM_TEX(input.uv, _color);
        output.instanceID = instanceID;
        return output;
    }



    ENDCG

        SubShader
    {
        Cull Off
        Lighting Off
        ZWrite On

        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Tags
            {
                "Queue" = "Geometry"
                "RenderType" = "Opaque"
                "IgnoreProjectors" = "True"
            }

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
            #pragma multi_compile_instancing
            #pragma target 4.0

            fixed4 frag(v2f i) : SV_Target
            {
                // add little offset to render subsequent/preceding line segment properly
				clip(i.instanceID - _startIndex + 0.001);
				clip(_endIndex - i.instanceID + 0.001);
                return fixed4(i.color.xyz, 1);
            }

            ENDCG
        }
    }
}