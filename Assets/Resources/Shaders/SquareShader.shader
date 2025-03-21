Shader "Custom/SquareShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _GlowColor ("Glow Color", Color) = (0.1,1,0.3,1)
        _GlowIntensity ("Glow Intensity", Range(0, 1)) = 0.5
        _GlowWidth ("Glow Width", Range(0, 0.5)) = 0.1
        _PulseSpeed ("Pulse Speed", Range(0, 10)) = 1
        _PulseAmount ("Pulse Amount", Range(0, 1)) = 0.2
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        
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
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float4 _GlowColor;
            float _GlowIntensity;
            float _GlowWidth;
            float _PulseSpeed;
            float _PulseAmount;
            
            // Calculate distance to edge of square
            float calculateEdgeDistance(float2 uv)
            {
                // Map UVs to -1 to 1 range
                float2 centered = abs((uv - 0.5) * 2.0);
                
                // For a square, the distance to edge is the distance to the nearest edge
                float distToEdge = max(centered.x, centered.y);
                float edgeFactor = smoothstep(0.8, 1.0, distToEdge);
                
                return edgeFactor;
            }
            
            v2f vert (appdata v)
            {
                v2f o;
                
                // Apply pulsing effect
                float pulse = sin(_Time.y * _PulseSpeed) * 0.5 + 0.5; // 0 to 1
                float scale = 1.0 + (pulse * _PulseAmount);
                v.vertex.xy *= scale;
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;
                return o;
            }
            
            float4 frag (v2f i) : SV_Target
            {
                // Sample texture
                float4 col = tex2D(_MainTex, i.uv) * i.color;
                
                // Calculate edge glow
                float edgeFactor = calculateEdgeDistance(i.uv);
                float glowAmount = smoothstep(0.0, _GlowWidth * 3.0, edgeFactor) * _GlowIntensity;
                
                // Add pulsing to the glow
                float pulse = sin(_Time.y * _PulseSpeed) * 0.5 + 0.5;
                glowAmount *= pulse;
                
                // Apply glow
                col.rgb = lerp(col.rgb, _GlowColor.rgb, glowAmount);
                
                return col;
            }
            ENDCG
        }
    }
    
    Fallback "Sprites/Default"
} 