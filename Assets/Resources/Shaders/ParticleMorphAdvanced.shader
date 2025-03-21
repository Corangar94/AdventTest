Shader "Custom/ParticleMorphAdvanced"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _EdgeColor ("Edge Color", Color) = (1,0.5,0,1)
        _EdgeWidth ("Edge Width", Range(0, 0.2)) = 0.05
        _DissolveTexture ("Dissolve Texture", 2D) = "white" {}
        _DissolveAmount ("Dissolve Amount", Range(0, 1)) = 0
        _EmissionStrength ("Emission Strength", Range(0, 5)) = 1.0
        _ParticleSize ("Particle Size", Range(0.001, 0.1)) = 0.01
        _ParticleCount ("Particle Count", Range(1, 100)) = 15
        _NoiseScale ("Noise Scale", Range(1, 50)) = 20
        _NoiseSpeed ("Noise Speed", Range(0, 10)) = 2
        _DistortionAmount ("Distortion Amount", Range(0, 1)) = 0.5
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
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float2 noiseUV : TEXCOORD1;
                float4 worldPosition : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            sampler2D _MainTex;
            sampler2D _DissolveTexture;
            float4 _MainTex_ST;
            float4 _Color;
            float4 _EdgeColor;
            float _EdgeWidth;
            float _DissolveAmount;
            float _EmissionStrength;
            float _ParticleSize;
            float _ParticleCount;
            float _NoiseScale;
            float _NoiseSpeed;
            float _DistortionAmount;
            
            // Better noise function using perlin-like behavior
            float2 hash(float2 p)
            {
                p = float2(dot(p, float2(127.1, 311.7)),
                          dot(p, float2(269.5, 183.3)));
                return -1.0 + 2.0 * frac(sin(p) * 43758.5453123);
            }
            
            float noise(float2 p)
            {
                const float K1 = 0.366025404; // (sqrt(3)-1)/2
                const float K2 = 0.211324865; // (3-sqrt(3))/6
                
                float2 i = floor(p + (p.x + p.y) * K1);
                float2 a = p - i + (i.x + i.y) * K2;
                float2 o = (a.x > a.y) ? float2(1.0, 0.0) : float2(0.0, 1.0);
                float2 b = a - o + K2;
                float2 c = a - 1.0 + 2.0 * K2;
                
                float3 h = max(0.5 - float3(dot(a, a), dot(b, b), dot(c, c)), 0.0);
                float3 n = h * h * h * h * float3(dot(a, hash(i + 0.0)), dot(b, hash(i + o)), dot(c, hash(i + 1.0)));
                
                return dot(n, float3(70.0, 70.0, 70.0));
            }
            
            // Simple 2D rotation function
            float2 rotate2D(float2 pos, float angle)
            {
                float s = sin(angle);
                float c = cos(angle);
                float2x2 rot = float2x2(c, -s, s, c);
                return mul(rot, pos);
            }
            
            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                
                // Animated distortion based on noise
                float2 uv = v.uv * _NoiseScale;
                float timeValue = _Time.y * _NoiseSpeed;
                
                // Get noise value
                float2 offset = float2(
                    noise(uv + timeValue * 0.5),
                    noise(uv + float2(1.0, 1.0) + timeValue * 0.5)
                );
                
                // Apply distortion based on dissolve amount
                // Increase distortion as dissolve increases (0.5 is max, then decrease)
                float dissolveFactor = 1.0 - abs(_DissolveAmount * 2.0 - 1.0);
                float2 distortedPosition = v.vertex.xy + offset * _DistortionAmount * dissolveFactor;
                
                // Apply final position
                float4 vertexPosition = float4(distortedPosition, v.vertex.zw);
                
                // Transform to clip space
                o.vertex = UnityObjectToClipPos(vertexPosition);
                o.worldPosition = mul(unity_ObjectToWorld, vertexPosition);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.noiseUV = uv;
                o.color = v.color * _Color;
                
                return o;
            }
            
            float4 frag (v2f i) : SV_Target
            {
                // Sample main texture
                float4 col = tex2D(_MainTex, i.uv) * i.color;
                
                // Apply dissolve effect using the dissolve texture
                float dissolveValue = tex2D(_DissolveTexture, i.uv).r;
                
                // Add some variation based on noise and world position
                float noise1 = noise(i.worldPosition.xy * 0.1 + _Time.y * 0.1);
                float noise2 = noise(i.noiseUV * 0.2 - _Time.y * 0.15);
                dissolveValue = dissolveValue * 0.8 + noise1 * 0.1 + noise2 * 0.1;
                
                // Calculate difference for edge detection
                float edgeDelta = dissolveValue - _DissolveAmount;
                
                // Clip pixels outside dissolve threshold
                clip(edgeDelta);
                
                // Apply glowing edge effect near the dissolve boundary
                if (edgeDelta < _EdgeWidth)
                {
                    // Calculate edge intensity - more intense closer to the edge
                    float edgeIntensity = 1.0 - (edgeDelta / _EdgeWidth);
                    
                    // Add emission to the edge
                    col.rgb = lerp(col.rgb, _EdgeColor.rgb * _EmissionStrength, edgeIntensity);
                    
                    // Make edge more opaque to emphasize the effect
                    col.a = lerp(col.a, 1.0, edgeIntensity * 0.5);
                }
                
                // Add overall emission boost during transition
                float transitionBoost = 1.0 - abs(_DissolveAmount * 2.0 - 1.0);
                col.rgb *= 1.0 + transitionBoost * 0.5;
                
                return col;
            }
            ENDCG
        }
    }
    
    Fallback "Sprites/Default"
} 