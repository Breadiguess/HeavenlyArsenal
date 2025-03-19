sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float time;
float3 edgeColor;

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    return tex2D(uImage0, coords);
}

technique Technique1
{
    pass PixelShaderFunction
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}