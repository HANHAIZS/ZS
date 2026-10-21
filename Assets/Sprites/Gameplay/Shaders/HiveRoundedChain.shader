Shader "BeeHarvest/HiveRoundedChain"
{
    Properties
    {
        _ColorA ("Endpoint A", Color) = (1, .4, 0, 1)
        _ColorB ("Endpoint B", Color) = (1, .4, 0, 1)
        _BodyAspect ("Distance / diameter", Float) = 8
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent+15" }
        Pass
        {
            Name "RoundedChain"
            Tags { "LightMode"="SRPDefaultUnlit" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 3.0
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                float4 _ColorA;
                float4 _ColorB;
                float _BodyAspect;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }
            float CapsuleDistance(float2 p)
            {
                return length(float2(max(abs(p.x) - _BodyAspect * .5, 0), p.y)) - .5;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                float2 p = (input.uv - .5) * float2(_BodyAspect + 1.2, 1.2);
                float distance = CapsuleDistance(p);
                float aa = max(fwidth(distance), .006);
                float coverage = 1 - smoothstep(-aa, aa, distance);
                float shadow = (1 - smoothstep(-.01, .065, CapsuleDistance(p + float2(0, .055)))) * .3;
                // Color identities meet at the geometric midpoint, without a gradient or seam.
                half3 tint = input.uv.x < .5 ? _ColorA.rgb : _ColorB.rgb;
                half3 shade = tint * lerp(.43, 1.0, smoothstep(-.46, .16, p.y));
                float highlight = exp(-pow((p.y - .18) / .12, 2)) * .42;
                shade = lerp(shade, half3(1, 1, 1), highlight);
                float bevel = 1 - smoothstep(-.115, -.045, distance);
                half3 rim = lerp(half3(.025, .015, .025), tint * .4, .55);
                half3 color = lerp(rim, shade, bevel);
                float alpha = coverage + shadow * (1 - coverage);
                color = color * coverage / max(alpha, .001);
                return half4(color, alpha);
            }
            ENDHLSL
        }
    }
}
