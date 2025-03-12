Shader "Projector/Light_Flipped_SelfIllum" {
    Properties {
        _Color ("Main Color", Color) = (1,1,1,1)
        _ShadowTex ("Cookie", 2D) = "" {}
        _FalloffTex ("FallOff", 2D) = "" {}
        [Toggle]_FlipY("Flip Y", Float) = 0
    }

    SubShader {
        Tags {
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        Pass {
            ZWrite Off
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // Remove multi_compile_fog and any built-in lighting passes:
            #pragma shader_feature _               

            #include "UnityCG.cginc"

            struct v2f {
                float4 uvShadow : TEXCOORD0;
                float4 uvFalloff : TEXCOORD1;
                float4 pos : SV_POSITION;
            };

            float4x4 unity_Projector;
            float4x4 unity_ProjectorClip;
            float _FlipY;

            v2f vert (float4 vertex : POSITION)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(vertex);

                o.uvShadow  = mul(unity_Projector, vertex);
                o.uvFalloff = mul(unity_ProjectorClip, vertex);

                // Flip Y if _FlipY is 1
                o.uvShadow.y  = lerp(o.uvShadow.y,  -o.uvShadow.y  + o.uvShadow.w,  _FlipY);
                o.uvFalloff.y = lerp(o.uvFalloff.y, -o.uvFalloff.y + o.uvFalloff.w, _FlipY);

                return o;
            }

            sampler2D _ShadowTex;
            sampler2D _FalloffTex;
            fixed4 _Color;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 mainTexColor = tex2Dproj(_ShadowTex, UNITY_PROJ_COORD(i.uvShadow)) * _Color;
                fixed4 falloffTex   = tex2Dproj(_FalloffTex, UNITY_PROJ_COORD(i.uvFalloff));
                // Combine main color and falloff alpha without fog or extra lighting
                fixed4 finalColor   = fixed4(mainTexColor.rgb, mainTexColor.a * falloffTex.a);
                return finalColor;
            }
            ENDCG
        }
    }
}