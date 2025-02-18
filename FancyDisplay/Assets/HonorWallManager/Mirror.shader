// Mirror.shader
Shader "Custom/Mirror"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ReflectionStrength ("Reflection Strength", Range(0,1)) = 0.5
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            struct appdata
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
            float4 _MainTex_ST;
            float _ReflectionStrength;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // 采样原始纹理
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // 创建镜面效果
                float2 reflectionUV = float2(i.uv.x, 1 - i.uv.y);
                fixed4 reflection = tex2D(_MainTex, reflectionUV);
                
                // 根据Y坐标计算渐变alpha
                float gradientAlpha = 1 - i.uv.y;
                
                // 混合原始图像和反射
                col = lerp(col, reflection, _ReflectionStrength * gradientAlpha);
                
                return col;
            }
            ENDCG
        }
    }
}