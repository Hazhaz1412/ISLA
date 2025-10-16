Shader "UI/BloodDripGradient"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (0.05, 0, 0, 0.95)
        
        // Gradient Settings
        _GradientTop ("Gradient Top", Color) = (0.1, 0, 0, 1)
        _GradientBottom ("Gradient Bottom", Color) = (0.02, 0, 0, 0.8)
        _GradientPower ("Gradient Power", Range(0.1, 5)) = 1.5
        
        // Glossiness
        _Glossiness ("Glossiness", Range(0,1)) = 0.85
        _SpecularPower ("Specular Power", Range(1,100)) = 35
        _SpecularColor ("Specular Color", Color) = (0.2,0.05,0.05,1)
        
        // Glow/Wetness
        _GlowIntensity ("Glow Intensity", Range(0, 1)) = 0.3
        _Fresnel ("Fresnel Power", Range(0, 5)) = 2
        
        // Distortion
        _DistortionAmount ("Distortion", Range(0, 0.1)) = 0.02
        _DistortionSpeed ("Distortion Speed", Range(0, 5)) = 1
        
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "BloodDrip"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                float2 screenPos : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _GradientTop;
            fixed4 _GradientBottom;
            float _GradientPower;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;
            float _Glossiness;
            float _SpecularPower;
            fixed4 _SpecularColor;
            float _GlowIntensity;
            float _Fresnel;
            float _DistortionAmount;
            float _DistortionSpeed;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color = v.color * _Color;
                OUT.screenPos = ComputeScreenPos(OUT.vertex).xy;
                
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // Distortion effect (méo mó như chất lỏng)
                float2 distortedUV = IN.texcoord;
                distortedUV.x += sin(IN.texcoord.y * 10.0 + _Time.y * _DistortionSpeed) * _DistortionAmount;
                
                half4 color = (tex2D(_MainTex, distortedUV) + _TextureSampleAdd) * IN.color;

                // Gradient từ trên xuống (đậm ở trên, nhạt ở dưới)
                float gradientFactor = pow(1.0 - IN.texcoord.y, _GradientPower);
                fixed4 gradientColor = lerp(_GradientBottom, _GradientTop, gradientFactor);
                color.rgb = gradientColor.rgb;
                color.a *= gradientColor.a;

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                // Specular highlight (ánh bóng)
                float3 viewDir = float3(0, 0, -1);
                float3 lightDir = normalize(float3(0.3, 0.7, -0.5));
                float3 halfDir = normalize(lightDir + viewDir);
                float3 normal = float3(sin(IN.texcoord.y * 20.0) * 0.1, 0, 1);
                normal = normalize(normal);
                
                float NdotH = max(0, dot(normal, halfDir));
                float specular = pow(NdotH, _SpecularPower) * _Glossiness;
                
                // Edge glow (viền sáng)
                float edgeFactor = abs(IN.texcoord.x - 0.5) * 2.0;
                float fresnel = pow(1.0 - edgeFactor, _Fresnel);
                
                // Combine specular + glow
                color.rgb += _SpecularColor.rgb * specular;
                color.rgb += fresnel * _GlowIntensity * float3(0.3, 0.1, 0.1);
                
                // Wet shine effect (hiệu ứng ướt)
                float wetness = sin(IN.texcoord.y * 15.0 + _Time.y * 2.0) * 0.5 + 0.5;
                wetness = pow(wetness, 3.0);
                color.rgb += wetness * 0.15 * float3(0.2, 0.05, 0.05);

                return color;
            }
        ENDCG
        }
    }
    
    Fallback "UI/Default"
}
