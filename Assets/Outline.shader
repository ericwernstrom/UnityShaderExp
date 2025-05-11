Shader "Assets/Outline"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _EdgeThreshold("Edge Threshold", Float) = 1.0
    }
    SubShader
    {
        Tags 
        {
            "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" 
        }

            //Tags { "LightMode" = "UniversalForward" }

        HLSLINCLUDE
        #pragma vertex vert
        #pragma fragment frag
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

        struct Attributes
        {
            float4 positionOS : POSITION;
            float2 uv : TEXCOORD0;
        };

        struct Varyings
        {
            float4 positionHCS : SV_POSITION;
            float2 uv : TEXCOORD0;
        };

        SamplerState sampler_point_clamp;

        half4 _OutlineColor;
        float _EdgeThreshold;

        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);

        Varyings vert(Attributes IN)
        {
            Varyings OUT;
            OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz); //UnityObjectToClipPos(IN.positionOS)
            OUT.uv = IN.uv;
            return OUT;
        }
        ENDHLSL
        
        Pass
        {
            Name "Outline"

            HLSLPROGRAM
            float SobelDetection(float2 uv, float2 texelSize)
            {
                float kernelX[3][3] = {
                    { -1, 0, 1 },
                    { -2, 0, 2 },
                    { -1, 0, 1 }
                };

                float kernelY[3][3] = {
                    { -1, -2, -1 },
                    {  0,  0,  0 },
                    {  1,  2,  1 }
                };

                float edgeX = 0.0;
                float edgeY = 0.0;
                
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        float2 sampleUV = uv + float2(i, j) * texelSize;
                        sampleUV = clamp(sampleUV, 0.0, 1.0);
                        
                        float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, sampleUV);
                        //depth = LinearEyeDepth(depth);

                        edgeX += depth * kernelX[i + 1][j + 1];
                        edgeY += depth * kernelY[i + 1][j + 1];
                    }
                }

                float edgeStrength = sqrt(edgeX * edgeX + edgeY * edgeY);
                return edgeStrength;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 texelSize = float2(1.0 / _ScreenParams.x, 1.0/_ScreenParams.y);
                float edge = SobelDetection(IN.uv, texelSize);
                edge *= 300;

                half4 sceneColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);

                if (edge > _EdgeThreshold)
                {
                    return _OutlineColor;
                }

                return sceneColor;
                //return half4(1.0, 0.0, 0.0, 1.0);
            }
            ENDHLSL
        }
    }
}
