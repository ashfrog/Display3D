Shader "Custom/PreciseNineSliceShader" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _CornerSize ("Corner Size", Vector) = (0.2, 0.2, 0.2, 0.2)
        // x: left corner width, y: right corner width, 
        // z: top corner height, w: bottom corner height
    }
    SubShader {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        LOD 100

        CGPROGRAM
        #pragma surface surf Lambert alpha

        sampler2D _MainTex;
        float4 _CornerSize;

        struct Input {
            float2 uv_MainTex;
        };

        void surf (Input IN, inout SurfaceOutput o) {
            float2 uv = IN.uv_MainTex;
            float2 scaledUV;

            // 左下角
            if (uv.x < _CornerSize.x && uv.y < _CornerSize.w) {
                scaledUV = uv;
            }
            // 右下角
            else if (uv.x > (1.0 - _CornerSize.y) && uv.y < _CornerSize.w) {
                scaledUV = float2(1.0 - _CornerSize.y + (uv.x - (1.0 - _CornerSize.y)) * (_CornerSize.y / _CornerSize.y), uv.y);
            }
            // 左上角
            else if (uv.x < _CornerSize.x && uv.y > (1.0 - _CornerSize.z)) {
                scaledUV = float2(uv.x, 1.0 - _CornerSize.z + (uv.y - (1.0 - _CornerSize.z)) * (_CornerSize.z / _CornerSize.z));
            }
            // 右上角
            else if (uv.x > (1.0 - _CornerSize.y) && uv.y > (1.0 - _CornerSize.z)) {
                scaledUV = float2(
                    1.0 - _CornerSize.y + (uv.x - (1.0 - _CornerSize.y)) * (_CornerSize.y / _CornerSize.y),
                    1.0 - _CornerSize.z + (uv.y - (1.0 - _CornerSize.z)) * (_CornerSize.z / _CornerSize.z)
                );
            }
            // 左边
            else if (uv.x < _CornerSize.x) {
                scaledUV = float2(uv.x, 
                    _CornerSize.w + (uv.y - _CornerSize.w) * ((1.0 - _CornerSize.w - _CornerSize.z) / (1.0 - _CornerSize.w - _CornerSize.z))
                );
            }
            // 右边
            else if (uv.x > (1.0 - _CornerSize.y)) {
                scaledUV = float2(
                    1.0 - _CornerSize.y + (uv.x - (1.0 - _CornerSize.y)) * (_CornerSize.y / _CornerSize.y),
                    _CornerSize.w + (uv.y - _CornerSize.w) * ((1.0 - _CornerSize.w - _CornerSize.z) / (1.0 - _CornerSize.w - _CornerSize.z))
                );
            }
            // 底部
            else if (uv.y < _CornerSize.w) {
                scaledUV = float2(
                    _CornerSize.x + (uv.x - _CornerSize.x) * ((1.0 - _CornerSize.x - _CornerSize.y) / (1.0 - _CornerSize.x - _CornerSize.y)), 
                    uv.y
                );
            }
            // 顶部
            else if (uv.y > (1.0 - _CornerSize.z)) {
                scaledUV = float2(
                    _CornerSize.x + (uv.x - _CornerSize.x) * ((1.0 - _CornerSize.x - _CornerSize.y) / (1.0 - _CornerSize.x - _CornerSize.y)),
                    1.0 - _CornerSize.z + (uv.y - (1.0 - _CornerSize.z)) * (_CornerSize.z / _CornerSize.z)
                );
            }
            // 中间区域
            else {
                scaledUV = float2(
                    _CornerSize.x + (uv.x - _CornerSize.x) * ((1.0 - _CornerSize.x - _CornerSize.y) / (1.0 - _CornerSize.x - _CornerSize.y)),
                    _CornerSize.w + (uv.y - _CornerSize.w) * ((1.0 - _CornerSize.w - _CornerSize.z) / (1.0 - _CornerSize.w - _CornerSize.z))
                );
            }
            
            // 采样纹理
            fixed4 c = tex2D(_MainTex, scaledUV);
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}