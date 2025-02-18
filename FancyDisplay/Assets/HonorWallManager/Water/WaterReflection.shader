// 水面着色器
Shader "Custom/WaterSurface" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _ReflectionTex ("Reflection", 2D) = "white" {}
        _WaveScale ("Wave Scale", Range(0.02,0.15)) = 0.063
        _ReflDistort ("Reflection Distort", Range(0,1.5)) = 0.44
        _RefractionTex ("Refraction", 2D) = "white" {}
        _Fresnel ("Fresnel", Range(0,1)) = 0.7
    }
    
    SubShader {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200
        
        CGPROGRAM
        #pragma surface surf Lambert alpha
        #pragma target 3.0
        
        sampler2D _ReflectionTex;
        sampler2D _RefractionTex;
        float4 _Color;
        float _WaveScale;
        float _ReflDistort;
        float _Fresnel;
        
        struct Input {
            float2 uv_ReflectionTex;
            float3 worldPos;
            float3 worldRefl;
            float3 viewDir;
            INTERNAL_DATA
        };
        
        void surf (Input IN, inout SurfaceOutput o) {
            // 计算波纹UV偏移
            float2 waveUV = IN.uv_ReflectionTex;
            waveUV += float2(_WaveScale * sin(_Time.x + IN.worldPos.x), 
                            _WaveScale * cos(_Time.x + IN.worldPos.z));
            
            // 获取反射贴图
            float3 reflection = tex2D(_ReflectionTex, 
                                    waveUV + _ReflDistort * IN.worldRefl.xz).rgb;
            
            // 获取折射贴图
            float3 refraction = tex2D(_RefractionTex, waveUV).rgb;
            
            // 计算菲涅尔系数
            float fresnel = _Fresnel * pow(1 - dot(IN.viewDir, float3(0,1,0)), 5);
            
            // 混合反射和折射
            o.Albedo = lerp(refraction, reflection, fresnel) * _Color.rgb;
            o.Alpha = _Color.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}