Shader "Hidden/Custom/Pixel Effect"
{
    Properties
    {
        _Curvature("Curvature Width", float) = 0
        _VignetteWidth("Vignette Width", float) = 0
    }

    HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
        SamplerState sampler_BlitTexture;
        float _Curvature;
        float _VignetteWidth;
        
        float4 frag (Varyings i) : SV_Target
        {
            float2 uv = i.texcoord * 2.0f - 1.0f;
            float2 offset = uv.yx / _Curvature;
            uv = uv + uv * offset * offset;
            uv = uv * 0.5f + 0.5f;
            float4 col = _BlitTexture.Sample(sampler_BlitTexture, uv);
            if (uv.x <= 0.0f || 1.0f <= uv.x || uv.y <= 0.0f || 1.0f <= uv.y){ col = 0; }
            uv = uv * 2.0f - 1.0f;
            float2 vignette = _VignetteWidth / _ScreenParams.xy;
            vignette = smoothstep(0.0f, vignette, 1.0f - abs(uv));
            vignette = saturate(vignette);
            col.g *= (sin(i.texcoord.y * _ScreenParams.y * 2.0f) + 1.0f) * 0.15f + 1.0f;
            col.rb *= (cos(i.texcoord.y * _ScreenParams.y * 2.0f) + 1.0f) * 0.135f + 1.0f;
            return saturate(col) * vignette.x * vignette.y;
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
            #pragma fragment frag
            ENDHLSL
        }
    }
}