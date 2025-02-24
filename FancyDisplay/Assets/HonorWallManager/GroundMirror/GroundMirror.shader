Shader "Custom/GroundMirror" {
    Properties {
        _MainTex ("Main Texture", 2D) = "white" {}
        _ReflectionTex ("Reflection Texture", 2D) = "white" {}
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _ReflectionStrength ("Reflection Strength", Range(0,1)) = 0.5
        _Transparency ("Transparency", Range(0,1)) = 0.5  // 新增透明度控制
    }
    
    SubShader {
        Tags { 
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }
        LOD 200

        // 开启透明混合
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows alpha:fade
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _ReflectionTex;
        
        struct Input {
            float2 uv_MainTex;
            float4 screenPos;
        };

        half _Metallic;
        half _Glossiness;
        half _ReflectionStrength;
        half _Transparency;  // 新增透明度变量

        void surf (Input IN, inout SurfaceOutputStandard o) {
            // 基础纹理
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
            
            // 计算反射UV坐标
            float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
            
            // 采样反射纹理
            fixed4 refl = tex2D(_ReflectionTex, screenUV);
            
            // 混合基础颜色和反射
            o.Albedo = lerp(c.rgb, refl.rgb, _ReflectionStrength);
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = _Transparency;  // 使用透明度参数
        }
        ENDCG
    }
    FallBack "Transparent/Diffuse"
}