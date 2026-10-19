Shader "UI/SoftPanel"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Color", Color) = (0.28, 0.14, 0.5, 0.88)
        _Corner ("Corner", Range(0, 0.5)) = 0.22
        _Glow ("Glow", Range(0, 1)) = 0.4
        _GlowColor ("Glow Color", Color) = (0.75, 0.55, 1, 1)
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Cull Off ZWrite Off ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; float4 color : COLOR; };
            struct v2f { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; float4 color : COLOR; };

            float4 _Color, _GlowColor;
            float _Corner, _Glow;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                o.color = v.color * _Color;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                float2 uv = abs(i.uv * 2.0 - 1.0);
                float2 c = max(uv - (1.0 - _Corner * 2.0), 0.0);
                float dist = length(c) - _Corner;
                float mask = 1.0 - smoothstep(0.0, 0.016, dist);

                float edgeGlow = smoothstep(0.12, 0.0, abs(dist)) * _Glow;
                half4 col = i.color;
                col.rgb += _GlowColor.rgb * edgeGlow;
                col.a *= mask;
                return col;
            }
            ENDHLSL
        }
    }
}