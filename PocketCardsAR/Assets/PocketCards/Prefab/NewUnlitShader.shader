Shader "Custom/LiquidFlow"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _FlowSpeed ("Flow Speed", Float) = 1.0
        _FillAmount ("Fill Amount", Range(0,1)) = 1.0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

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

            float4 _Color;
            float _FlowSpeed;
            float _FillAmount;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS);
                OUT.uv = IN.uv;
                return OUT;
            }

            float4 frag (Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;

                // Scroll the UV vertically for flow effect
                uv.y += _Time.y * _FlowSpeed;

                // Fill mask (hide part of the line)
                if (IN.uv.x > _FillAmount)   // horizontal mask
                    return float4(0,0,0,0);  // fully transparent

                // Final color
                float4 col = _Color;

                return col;
            }
            ENDHLSL
        }
    }
}
