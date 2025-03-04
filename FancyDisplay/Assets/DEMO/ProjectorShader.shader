Shader "Projector/Light_Flipped" {
    Properties {
        _Color ("Main Color", Color) = (1,1,1,1)
        _ShadowTex ("Cookie", 2D) = "" {}
        _FalloffTex ("FallOff", 2D) = "" {}
    }
    
    SubShader {
        Tags {"Queue"="Transparent"}
        Pass {
            ZWrite Off
            ColorMask RGB
            Blend DstColor One
            Offset -1, -1
    
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
            
            v2f vert (float4 vertex : POSITION)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(vertex);
                
                // Projective transforms
                o.uvShadow = mul(unity_Projector, vertex);
                o.uvFalloff = mul(unity_ProjectorClip, vertex);
                
                // Flip the y-coordinate for both projected UVs
                o.uvShadow.y = -o.uvShadow.y + o.uvShadow.w;
                o.uvFalloff.y = -o.uvFalloff.y + o.uvFalloff.w;
                
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            
            fixed4 _Color;
            sampler2D _ShadowTex;
            sampler2D _FalloffTex;
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 texS = tex2Dproj(_ShadowTex, UNITY_PROJ_COORD(i.uvShadow));
                texS.rgb *= _Color.rgb * 30;
                texS.a = 1.0 - texS.a;
    
                fixed4 texF = tex2Dproj(_FalloffTex, UNITY_PROJ_COORD(i.uvFalloff));
                fixed4 res = texS * texF.a;

                UNITY_APPLY_FOG_COLOR(i.fogCoord, res, fixed4(0,0,0,0));
                return res;
            }
            ENDCG
        }
    }
}