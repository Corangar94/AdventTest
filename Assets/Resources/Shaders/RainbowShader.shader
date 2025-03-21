Shader "Custom/RainbowShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Speed ("Animation Speed", Range(0, 10)) = 1
        _Frequency ("Color Frequency", Range(0.1, 10)) = 0.5
        _Saturation ("Color Saturation", Range(0, 1)) = 0.8
        _Brightness ("Color Brightness", Range(0, 1)) = 0.8
        _EmissionStrength ("Emission Strength", Range(0, 5)) = 1.0
        _Direction ("Direction (0=Horizontal, 1=Vertical)", Range(0, 1)) = 0
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        
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
                float4 color : COLOR;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float3 worldPos : TEXCOORD1;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Speed;
            float _Frequency;
            float _Saturation;
            float _Brightness;
            float _EmissionStrength;
            float _Direction;
            
            // RGB to HSV conversion
            float3 rgb2hsv(float3 c)
            {
                float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
                float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
                float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));
                
                float d = q.x - min(q.w, q.y);
                float e = 1.0e-10;
                return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
            }
            
            // HSV to RGB conversion
            float3 hsv2rgb(float3 c)
            {
                float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
                float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
                return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
            }
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }
            
            float4 frag (v2f i) : SV_Target
            {
                // Sample base texture
                float4 tex = tex2D(_MainTex, i.uv);
                
                // Get rainbow color based on position and time
                float offset;
                if (_Direction < 0.5)
                    offset = i.uv.x; // Horizontal
                else
                    offset = i.uv.y; // Vertical
                
                // Animated rainbow effect
                float hue = frac(offset * _Frequency + _Time.y * _Speed);
                float3 hsv = float3(hue, _Saturation, _Brightness);
                float3 rgb = hsv2rgb(hsv);
                
                // Combine with texture and add emission
                float4 col = tex * i.color;
                col.rgb = lerp(col.rgb, rgb, tex.a) * _EmissionStrength;
                
                return col;
            }
            ENDCG
        }
    }
    
    Fallback "Sprites/Default"
} 