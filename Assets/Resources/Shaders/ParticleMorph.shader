Shader "Custom/ParticleMorph"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _ParticleSize ("Particle Size", Range(0.001, 0.1)) = 0.01
        _ParticleCount ("Particle Count", Range(1, 100)) = 15
        _ParticleSpread ("Particle Spread", Range(0.1, 5.0)) = 2.5
        _NoiseScale ("Noise Scale", Range(1, 50)) = 20
        _NoiseStrength ("Noise Strength", Range(0, 1)) = 0.5
        _SpinSpeed ("Spin Speed", Range(0, 10)) = 2
        _DissolveTexture ("Dissolve Texture", 2D) = "white" {}
        _DissolveAmount ("Dissolve Amount", Range(0, 1)) = 0
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
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            sampler2D _MainTex;
            sampler2D _DissolveTexture;
            float4 _MainTex_ST;
            float4 _Color;
            float _ParticleSize;
            float _ParticleCount;
            float _ParticleSpread;
            float _NoiseScale;
            float _NoiseStrength;
            float _SpinSpeed;
            float _DissolveAmount;
            
            // Simple noise function
            float noise(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
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
                
                // Rotate around center based on time
                float2 vertPos = v.vertex.xy;
                float angle = _Time.y * _SpinSpeed;
                vertPos = rotate2D(vertPos, angle);
                
                // Apply noise based on position and time
                float noiseVal = noise(vertPos * _NoiseScale + _Time.y);
                vertPos += noiseVal * _NoiseStrength;
                
                v.vertex.xy = vertPos;
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = _Color;
                
                return o;
            }
            
            float4 frag (v2f i) : SV_Target
            {
                // Sample main texture
                float4 col = tex2D(_MainTex, i.uv) * i.color;
                
                // Apply dissolve effect
                float dissolve = tex2D(_DissolveTexture, i.uv).r;
                clip(dissolve - _DissolveAmount);
                
                // Add dissolve edge glow
                if (dissolve - _DissolveAmount < 0.05)
                {
                    col.rgb += float3(1.0, 0.5, 0.0) * (1.0 - (dissolve - _DissolveAmount) / 0.05);
                }
                
                return col;
            }
            ENDCG
        }
    }
    
    Fallback "Sprites/Default"
} 