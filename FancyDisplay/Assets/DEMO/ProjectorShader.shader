Shader "Projector/Light_Flipped_SelfIllum" {
    Properties {
        _Color ("Main Color", Color) = (1,1,1,1)
        _ShadowTex ("Cookie", 2D) = "" {}
        _FalloffTex ("FallOff", 2D) = "" {}
        _FlipY ("Flip Y", Float) = 0.0
    }

    SubShader {
        // 使用Transparent队列且禁用深度写入，让材质表现更像自发光UI
        Tags {
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        Pass {
            // 关闭深度写入，关闭裁剪
            ZWrite Off
            Cull Off
            
            // 对颜色使用常见的UI型Alpha混合
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "UnityCG.cginc"

            struct v2f {
                float4 uvShadow : TEXCOORD0;
                float4 uvFalloff : TEXCOORD1;
                UNITY_FOG_COORDS(2)
                float4 pos : SV_POSITION;
            };

            float4x4 unity_Projector;
            float4x4 unity_ProjectorClip;
            float _FlipY;

            v2f vert (float4 vertex : POSITION)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(vertex);

                // 计算Projector贴图坐标
                float4 uvShadow  = mul(unity_Projector, vertex);
                float4 uvFalloff = mul(unity_ProjectorClip, vertex);

                // 默认（_FlipY=0）时使用原有投影公式: y = -y + w
                // 如果开启翻转（_FlipY=1），则使用原本的 y，不执行反转
                // 通过线性插值实现可切换：当 _FlipY=0 => -uv.y + uv.w；当 _FlipY=1 => uv.y
                float flippedShadowY  = -uvShadow.y  + uvShadow.w;
                float flippedFalloffY = -uvFalloff.y + uvFalloff.w;

                uvShadow.y  = lerp(flippedShadowY,  uvShadow.y,  _FlipY);
                uvFalloff.y = lerp(flippedFalloffY, uvFalloff.y, _FlipY);

                o.uvShadow  = uvShadow;
                o.uvFalloff = uvFalloff;

                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }

            sampler2D _ShadowTex;
            sampler2D _FalloffTex;
            fixed4 _Color;

            // 简单的自发光式片段着色器，不做任何Lighting计算
            fixed4 frag (v2f i) : SV_Target
            {
                // 取主贴图，并乘以颜色
                fixed4 mainTexColor = tex2Dproj(_ShadowTex, UNITY_PROJ_COORD(i.uvShadow)) * _Color;

                // 取衰减贴图，使用其Alpha作为边缘衰减
                fixed4 falloffTex = tex2Dproj(_FalloffTex, UNITY_PROJ_COORD(i.uvFalloff));
                fixed4 finalColor = fixed4(mainTexColor.rgb, mainTexColor.a * falloffTex.a);

                UNITY_APPLY_FOG_COLOR(i.fogCoord, finalColor, fixed4(0,0,0,0));
                return finalColor;
            }
            ENDCG
        }
    }
}