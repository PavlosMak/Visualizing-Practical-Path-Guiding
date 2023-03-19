Shader "Custom/Isosurface" {
    Properties {
        _MainTex ("Texture3D", 3D) = "white" {}
        _IsoLevel ("Iso Level", Range(0.0, 1.0)) = 0.5
    }
 
    SubShader {
        Tags { "RenderType"="Opaque" }
 
        CGPROGRAM
        #pragma surface surf Standard
 
        sampler3D _MainTex;
        float _IsoLevel;
 
        struct Input {
            float3 worldPos;
        };
 
        void surf (Input IN, inout SurfaceOutputStandard o) {
            float3 texCoord = IN.worldPos;
            float value = tex3D(_MainTex, texCoord).r;
            float4 color = float4(1, 1, 1, 0); // default color is transparent
 
            if (value >= _IsoLevel) {
                color.a = 1; // set alpha to opaque
            }
 
            o.Albedo = color.rgb;
            o.Alpha = color.a;
        }
        ENDCG
    }
 
    FallBack "Diffuse"
}