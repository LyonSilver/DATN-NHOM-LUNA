Shader "UI/PrismButton"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _PrismStrength ("Prism Strength", Range(0, 1)) = 0.6
        _PrismSpeed ("Prism Speed", Range(0, 5)) = 1.2
        _Chromatic ("Chromatic Aberration", Range(0, 0.05)) = 0.015
        _EdgeGlow ("Edge Glow", Range(0, 2)) = 0.8
        _Shine ("Shine Intensity", Range(0, 2)) = 1.0

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
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
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;
            float _PrismStrength;
            float _PrismSpeed;
            float _Chromatic;
            float _EdgeGlow;
            float _Shine;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color = v.color * _Color;
                return OUT;
            }

            // Rainbow / Prism spectrum
            fixed3 PrismColor(float2 uv, float time)
            {
                float x = uv.x + uv.y * 0.5 + time * _PrismSpeed;
                float3 c;
                c.r = 0.5 + 0.5 * sin(x * 6.283 + 0.0);
                c.g = 0.5 + 0.5 * sin(x * 6.283 + 2.094);
                c.b = 0.5 + 0.5 * sin(x * 6.283 + 4.188);
                return c;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.texcoord;
                float time = _Time.y;

                // Chromatic aberration (lăng kính tách màu)
                float2 offset = (uv - 0.5) * _Chromatic;
                fixed4 r = tex2D(_MainTex, uv + offset) + _TextureSampleAdd;
                fixed4 g = tex2D(_MainTex, uv) + _TextureSampleAdd;
                fixed4 b = tex2D(_MainTex, uv - offset) + _TextureSampleAdd;

                fixed4 color = fixed4(r.r, g.g, b.b, (r.a + g.a + b.a) / 3.0);

                // Prism rainbow overlay
                fixed3 prism = PrismColor(uv, time);
                color.rgb = lerp(color.rgb, color.rgb * prism * 1.8, _PrismStrength);

                // Edge glow + shine
                float edge = 1.0 - saturate(length(uv - 0.5) * 1.8);
                float shine = pow(edge, 2.0) * _Shine * (0.6 + 0.4 * sin(time * 3.0 + uv.x * 8.0));
                color.rgb += prism * edge * _EdgeGlow + shine;

                color *= IN.color;

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif

                return color;
            }
            ENDCG
        }
    }
}