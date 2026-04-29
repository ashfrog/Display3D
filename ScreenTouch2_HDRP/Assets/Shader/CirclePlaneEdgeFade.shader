Shader "Custom/CirclePlaneEdgeFade"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _FadeWidth ("Edge Fade Width", Range(0,0.5)) = 0.1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float _FadeWidth;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 center = float2(0.5, 0.5);
                float dist = distance(i.uv, center);

                // 圆形半径，0.5为整张UV图的最大半径
                float radius = 0.5;
                float alpha = smoothstep(radius, radius - _FadeWidth, dist);

                fixed4 col = tex2D(_MainTex, i.uv);
                col.rgb *= alpha; // 边缘乘黑色
                col.a = alpha;
                return col;
            }
            ENDCG
        }
    }
}