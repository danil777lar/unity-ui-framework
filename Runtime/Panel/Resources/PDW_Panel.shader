Shader "Hidden/PDW/Panel"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)


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
            Name "LarjeUI_Panel"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            
            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float4 tangent : TANGENT;

                float2 texcoord : TEXCOORD0;
                float4 texcoord1 : TEXCOORD1;
                float4 texcoord2 : TEXCOORD2;
                float4 texcoord3 : TEXCOORD3;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex         : SV_POSITION;
                fixed4 color          : COLOR;
                float2 uv             : TEXCOORD0;

                float4 fillColor      : TEXCOORD1;
                float4 borderColor    : TEXCOORD2;
                float4 borderSoftSize : TEXCOORD3;
                float4 radius : TANGENT;

                float4 worldPos : TEXCOORD7;

                UNITY_VERTEX_OUTPUT_STEREO
            };



            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;

            v2f vert(appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = v.vertex;
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);

                o.color = v.color * _Color;

                o.fillColor = v.texcoord1;
                o.borderColor = v.texcoord2;
                o.borderSoftSize = v.texcoord3;
                o.radius = v.tangent;

                return o;
            }

            float sdRoundedBox(float2 p, float2 halfSize, float radius)
            {
                float2 q = abs(p) - (halfSize - radius);
                return length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - radius;
            }

            float getActualRadius(float2 p, float4 radius) 
            {
                float r;
                if (p.x < 0)
                {
                    r = (p.y < 0) ? radius.x : radius.y;
                }
                else
                {
                    r = (p.y < 0) ? radius.w : radius.z;
                }

                return r;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 fillColor = i.fillColor;
                fixed4 borderColor = i.borderColor;
                fixed4 radiusPx = i.radius;
                fixed borderPx = i.borderSoftSize.x;
                fixed softnessPx = i.borderSoftSize.y;
                fixed2 rectSize = i.borderSoftSize.zw;

                fixed4 tex = tex2D(_MainTex, i.uv);

                float w = max(1.0, rectSize.x);
                float h = max(1.0, rectSize.y);
                float2 p = (i.uv - 0.5) * float2(w, h);
                float r = getActualRadius(p, radiusPx);
                float b = clamp(borderPx, 0.0, 0.5 * min(w, h));

                float2 halfSize = 0.5 * float2(w, h);

                float d = sdRoundedBox(p, halfSize, r);
                float aa = max(max(0.001, softnessPx), fwidth(d));
                float insideA = smoothstep(0.0, aa, -d);
                float deepA = (b > 0.0) ? smoothstep(b, b + aa, -d) : insideA;
                float borderA = (b > 0.0) ? saturate(insideA - deepA) : 0.0;
                float fillA = (b > 0.0) ? deepA : insideA;

                fixed4 col;
                col.rgb = fillColor.rgb * fillA + borderColor.rgb * borderA;
                col.a = fillColor.a * fillA + borderColor.a * borderA;

                col *= tex;
                col *= i.color;

                #ifdef UNITY_UI_CLIP_RECT
                col.a *= UnityGet2DClipping(i.worldPos.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(col.a - 0.001);
                #endif

                return col;
            }

            ENDHLSL
        }
    }
}
