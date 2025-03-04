Shader "Custom/AVProVideoProjector"
{
    Properties
    {
        _AVProVideoMainTex ("AVPro Video Texture", 2D) = "white" {}
        _Tint ("Tint Color", Color) = (1, 1, 1, 1)
        _Intensity ("Intensity", Range(0, 10)) = 1.0
        _FalloffTex ("Falloff Texture", 2D) = "white" {}
    }
    
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 100
        
        ZWrite Off
        Blend DstColor One
        Offset -1, -1
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 projUV : TEXCOORD0;
                float2 uv : TEXCOORD1;
                UNITY_FOG_COORDS(2)
            };
            
            sampler2D _AVProVideoMainTex;
            float4 _AVProVideoMainTex_ST;
            float4 _Tint;
            float _Intensity;
            sampler2D _FalloffTex;
            float4x4 unity_Projector;
            float4x4 unity_ProjectorClip;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.projUV = mul(unity_Projector, v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _AVProVideoMainTex);
                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // 计算投影坐标
                float2 projUV = i.projUV.xy / i.projUV.w;
                
                // 检查投影是否在有效范围内
                float falloff = tex2D(_FalloffTex, projUV).a;
                
                // 采样AVPro视频纹理
                // 注意：AVPro视频纹理需要正确处理
                // 1. 视频可能是颠倒的，需要翻转Y轴
                // 2. 视频可能使用不同的色彩空间，需要进行适当转换
                float2 videoUV = float2(projUV.x, 1.0 - projUV.y); // 翻转Y轴
                fixed4 videoColor = tex2D(_AVProVideoMainTex, videoUV);
                
                // 应用Tint颜色和强度
                fixed4 finalColor = videoColor * _Tint * _Intensity;
                
                // 应用衰减
                finalColor.a *= falloff;
                
                // 应用雾效果
                UNITY_APPLY_FOG_COLOR(i.fogCoord, finalColor, fixed4(1, 1, 1, 1));
                
                return finalColor;
            }
            ENDCG
        }
    }
    
    Fallback "Projector/Light"
}