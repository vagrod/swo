Shader "Custom/PaperDistort"{
Properties {
        _MainTex ("MainTex", 2D) = "white" {}
        _Distort ("Distort", 2D) = "white" {}
        _Scalar ("Scalar", Float ) = 0.01
        _EffectStartPosX ("EffectStartPosX", Float ) = 0
        _EffectStartPosY ("EffectStartPosY", Float ) = 0
        _EffectSizeX ("EffectSizeX", Float ) = 0
        _EffectSizeY ("EffectSizeY", Float ) = 0
        _ScreenSizeX ("ScreenSizeX", Float ) = 0
        _ScreenSizeY ("ScreenSizeY", Float ) = 0
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Overlay+1"
            "RenderType"="Overlay"
            "PreviewType"="Plane"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            ZTest Always
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #define _GLOSSYENV 1
            #include "UnityCG.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma multi_compile_fwdbase
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal d3d11_9x 
            #pragma target 3.0
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform sampler2D _Distort; uniform float4 _Distort_ST;
            uniform float _Scalar;
            uniform float _EffectStartPosX;
            uniform float _EffectStartPosY;
            uniform float _EffectSizeX;
            uniform float _EffectSizeY;
            uniform float _ScreenSizeX;
            uniform float _ScreenSizeY;

            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 screenPos:TEXCOORD1;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.pos = UnityObjectToClipPos( v.vertex );
                o.screenPos = ComputeScreenPos(o.pos);

                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
////// Lighting:
////// Emissive:
                float currentPosX = i.screenPos.x * _ScreenSizeX;
                float currentPosY = i.screenPos.y * _ScreenSizeY;

            	if(currentPosX>_EffectStartPosX && currentPosX<=_EffectStartPosX +_EffectSizeX &&
                   currentPosY>_EffectStartPosY && currentPosY<=_EffectStartPosY +_EffectSizeY ){
                    float distortU = (currentPosX - _EffectStartPosX) / _EffectSizeX;
                    float distortV = (currentPosY - _EffectStartPosY) / _EffectSizeY;
                    float4 distortTexture = tex2D(_Distort,TRANSFORM_TEX(fixed2(distortU, distortV), _Distort));
                    float displacementStrength = _Scalar*distortTexture.rgb.r; // black color = no displacement; white = maximum displacement
                    float2 displacementUV = (i.uv0 + distortTexture.rgb.rg * displacementStrength);
                    float4 resultedTex = tex2D(_MainTex,TRANSFORM_TEX(displacementUV, _MainTex));
                    float3 emissive = resultedTex.rgb;

	                float3 finalColor = emissive;
	                return fixed4(finalColor,1);
            	}

                return tex2Dlod (_MainTex, float4(i.uv0.xy,0,0));
            }
            ENDCG
        }
    }
    CustomEditor "ShaderForgeMaterialInspector"
}
