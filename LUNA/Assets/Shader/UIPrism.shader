Shader "UI/SoftPanel"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (0.1, 0.05, 0.2, 0.75)
        _CornerRadius ("Corner Radius", Range(0, 0.5)) = 0.2
        _GlowStrength ("Glow Strength", Range(0, 0.5)) = 0.12
        _GlowColor ("Glow Color", Color) = (0.6, 0.8, 1, 1)
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

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; float4 color : COLOR; };
            struct Varyings { float4 positionHCS : SV_POSITION; float2 uv : TEXCOORD0; float4 color : COLOR; };

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            float4 _Color, _GlowColor;
            float _CornerRadius, _GlowStrength;

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;
                o.color = v.color * _Color;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float2 uv = i.uv * 2 - 1;
                float2 d = abs(uv) - (1 - _CornerRadius * 2);
                float dist = length(max(d, 0)) - _CornerRadius;
                float mask = 1 - smoothstep(0, 0.015, dist);

                float glow = smoothstep(0.25, 0.0, dist) * _GlowStrength;
                half4 col = i.color;
                col.rgb += _GlowColor.rgb * glow;
                col.a *= mask;
                return col;
            }
            ENDHLSL
        }
    }
}