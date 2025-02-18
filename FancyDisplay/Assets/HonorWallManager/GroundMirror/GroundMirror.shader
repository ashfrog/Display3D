Shader "Custom/GroundMirror" {
    Properties {
        _MainTex ("Main Texture", 2D) = "white" {}
        _ReflectionTex ("Reflection Texture", 2D) = "white" {}
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _ReflectionStrength ("Reflection Strength", Range(0,1)) = 0.5
    }
    
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
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
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}