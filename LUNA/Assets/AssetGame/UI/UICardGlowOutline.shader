Shader "UI/CardGlowOutline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _OutlineColor ("Outline Color", Color) = (1, 0.8, 0.2, 1)
        _OutlineThickness ("Outline Thickness", Range(0, 10)) = 2
        _OutlineSoftness ("Outline Softness", Range(0, 5)) = 1.5
        _GlowIntensity ("Glow Intensity", Range(0, 5)) = 1.8

        // UI required
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
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
            Name "OUTLINE"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            fixed4 _OutlineColor;
            float _OutlineThickness;
            float _OutlineSoftness;
            float _GlowIntensity;
            float4 _ClipRect;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = v.texcoord;
                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.texcoord;
                float2 texel = _MainTex_TexelSize.xy * _OutlineThickness;

                // Sample alpha around pixel (8 directions)
                float a =
                    tex2D(_MainTex, uv + float2( texel.x,  0)).a +
                    tex2D(_MainTex, uv + float2(-texel.x,  0)).a +
                    tex2D(_MainTex, uv + float2( 0,  texel.y)).a +
                    tex2D(_MainTex, uv + float2( 0, -texel.y)).a +
                    tex2D(_MainTex, uv + float2( texel.x,  texel.y)).a +
                    tex2D(_MainTex, uv + float2(-texel.x,  texel.y)).a +
                    tex2D(_MainTex, uv + float2( texel.x, -texel.y)).a +
                    tex2D(_MainTex, uv + float2(-texel.x, -texel.y)).a;

                a = saturate(a);

                float center = tex2D(_MainTex, uv).a;

                // Outline mask
                float outline = saturate(a - center);
                outline = smoothstep(0, _OutlineSoftness * 0.1 + 0.01, outline);

                fixed4 outlineCol = _OutlineColor;
                outlineCol.a *= outline * _GlowIntensity * IN.color.a;

                // Original sprite
                fixed4 mainCol = tex2D(_MainTex, uv) * IN.color;

                // Clip UI
                outlineCol.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                mainCol.a   *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);

                // Combine: outline behind / around
                fixed4 finalCol = lerp(outlineCol, mainCol, mainCol.a);
                finalCol.a = max(outlineCol.a, mainCol.a);

                return finalCol;
            }
            ENDCG
        }
    }
}