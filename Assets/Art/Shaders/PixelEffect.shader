Shader "Hidden/Custom/Pixel Effect"
{
    Properties
    {
        _SampleAmount("Sample Amount", int) = 100
        _DitherSpread("Dither Spread", float) = .1
    }

    HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
        SamplerState _Sampler_BlitTexture_Point_Clamp;
        int _SampleAmount;
        float4 _QuantizationAmounts;
        float _DitherSpread;

        static const int PS1Dither[16] = 
        {
            -4, 0, -3, 1,
            2, -2, 3, -1,
            -3, 1, -4, 0,
            3, -1, 2, -2
        };
        static const int DitherPatternSize = 4;


        float GetDither(float2 uvs)
        {
            // Cast to integer pixel coordinates before indexing the matrix
            int2 p = int2(uvs) % DitherPatternSize;
            return (float)PS1Dither[p.x + p.y * DitherPatternSize];
        }
        float CalculateQuatization(float incomingColor, float2 uvs)
        {
            // Apply spread to the dither, quantize to 5-bit (0..31), then normalize back to 0..1
            float v = (incomingColor * 255.0) + (GetDither(uvs) * _DitherSpread);
            float q = floor(v / 8.0);        // 256/32 = 8 -> 0..31
            return saturate(q / 31.0);
        }
        float4 Frag (Varyings i) : SV_Target
        {
            float2 pixelRatio = float2(_SampleAmount, _SampleAmount * (_ScreenParams.y / _ScreenParams.x));
            float2 newTexUVs = i.texcoord * pixelRatio;
            newTexUVs = floor(newTexUVs);
            newTexUVs /= pixelRatio;

            float2 ps1DitherCoords = newTexUVs * pixelRatio;
            // sample the texture
            float4 col = _BlitTexture.Sample(_Sampler_BlitTexture_Point_Clamp, newTexUVs);
            col = float4(
                CalculateQuatization(col.r, ps1DitherCoords),
                CalculateQuatization(col.g, ps1DitherCoords),
                CalculateQuatization(col.b, ps1DitherCoords),
                col.a
            );
            return col;
        }

    ENDHLSL

    SubShader
    {
        Tags{ "RenderPipeline" = "UniversalPipeline"}
        ZWrite Off ZTest Always Blend Off Cull Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            ENDHLSL
        }
    }
}