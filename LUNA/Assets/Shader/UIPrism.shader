Shader "UI/PrismButton"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _PrismStrength ("Prism Strength", Range(0,1)) = 0.6
        _Speed ("Speed", Float) = 1.5
        _CornerRadius ("Corner Radius", Range(0,0.5)) = 0.15
    }

    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
            "RenderPipeline"="UniversalPipeline"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "UIPrism"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                float4 positionOS : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _Color;
            float _PrismStrength;
            float _Speed;
            float _CornerRadius;

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color * _Color;
                output.positionOS = input.positionOS;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Rounded corner
                float2 uv = input.uv * 2.0 - 1.0;
                float2 absUV = abs(uv);
                float corner = length(max(absUV - (1.0 - _CornerRadius * 2.0), 0.0)) - _CornerRadius;
                float mask = 1.0 - smoothstep(0.0, 0.02, corner);

                // Prism effect
                float t = _Time.y * _Speed;
                float3 prism = sin(float3(input.uv.x * 8.0 + t, input.uv.y * 6.0 - t * 0.7, t * 1.3)) * 0.5 + 0.5;
                prism = prism * float3(0.6, 0.9, 1.2) + float3(0.3, 0.2, 0.5);

                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * input.color;
                col.rgb = lerp(col.rgb, col.rgb * prism + prism * 0.3, _PrismStrength);
                col.a *= mask;

                return col;
            }
            ENDHLSL
        }
    }
}