Shader "Custom/ShapeGlowShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _GlowColor ("Glow Color", Color) = (1,0.5,0,1)
        _GlowIntensity ("Glow Intensity", Range(0, 1)) = 0.5
        _GlowWidth ("Glow Width", Range(0, 0.5)) = 0.1
        _PulseSpeed ("Pulse Speed", Range(0, 10)) = 1
        _PulseAmount ("Pulse Amount", Range(0, 1)) = 0.2
        _ShapeType ("Shape Type (0=Hex, 1=Square, 2=Triangle)", Range(0, 2)) = 0
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
            
            // Constants
            #define PI 3.14159265359
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float4 _GlowColor;
            float _GlowIntensity;
            float _GlowWidth;
            float _PulseSpeed;
            float _PulseAmount;
            float _ShapeType;
            
            // Calculate distance to edge for hexagon - no outline, just full glow
            float calculateHexagonEdgeDistance(float2 uv)
            {
                // Map UVs to -1 to 1 range
                float2 p = (uv - 0.5) * 2.0;
                
                // Calculate distance from center as glow factor
                float dist = length(p);
                
                // Return full shape glow instead of just edge
                return smoothstep(0.0, 1.0, 1.0 - dist);
            }
            
            // Calculate distance to edge for square - no outline, just full glow
            float calculateSquareEdgeDistance(float2 uv)
            {
                // Map UVs to -1 to 1 range
                float2 centered = (uv - 0.5) * 2.0;
                
                // For a square, use distance from center with square shape
                float squareDist = max(abs(centered.x), abs(centered.y));
                
                // Return full shape glow instead of just edge
                return smoothstep(0.0, 1.0, 1.0 - squareDist);
            }
            
            // Calculate distance to edge for triangle - no outline, just full glow
            float calculateTriangleEdgeDistance(float2 uv)
            {
                // Map UVs to -1 to 1 range with y-axis flipped
                float2 p = (uv - 0.5) * 2.0;
                p.y = -p.y; // Flip y to match our triangle orientation (point up)
                
                // Create factor based on position within triangle
                const float triangleHeight = 1.732; // sqrt(3)
                
                // Return full shape glow instead of just edge
                return smoothstep(0.0, 1.0, 1.0 - length(p) * 0.8);
            }
            
            // Calculate distance to edge based on shape type
            float calculateEdgeDistance(float2 uv)
            {
                if (_ShapeType < 0.5) {
                    // Hexagon
                    return calculateHexagonEdgeDistance(uv);
                } 
                else if (_ShapeType < 1.5) {
                    // Square
                    return calculateSquareEdgeDistance(uv);
                } 
                else {
                    // Triangle
                    return calculateTriangleEdgeDistance(uv);
                }
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
                
                // Calculate glow factor based on shape type
                float glowFactor = calculateEdgeDistance(i.uv);
                
                // Skip if no glow needed
                if (glowFactor <= 0.01)
                    return col;
                
                // Calculate glow intensity with shape-specific adjustments
                float finalGlowIntensity = _GlowIntensity;
                if (_ShapeType < 0.5) { // Hexagon
                    finalGlowIntensity *= 0.8;
                } 
                else if (_ShapeType < 1.5) { // Square
                    finalGlowIntensity *= 0.7;
                }
                else { // Triangle
                    finalGlowIntensity *= 0.85;
                }
                
                // Apply pulsing effect
                float pulse = sin(_Time.y * _PulseSpeed) * 0.5 + 0.5;
                
                // Adjust glow based on distance from center - creates a gradient effect
                float glowAmount = glowFactor * finalGlowIntensity * (0.8 + pulse * 0.4);
                
                // Apply glow as overlay on the shape
                col.rgb = lerp(col.rgb, _GlowColor.rgb, glowAmount * 0.5);
                
                return col;
            }
            ENDCG
        }
    }
    
    Fallback "Sprites/Default"
} 