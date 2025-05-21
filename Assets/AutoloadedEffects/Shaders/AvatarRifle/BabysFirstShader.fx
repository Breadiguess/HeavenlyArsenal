sampler2D NoiseTex : register(s0);
float time;
float4 FlameColor;

struct VS_INPUT
{
    float4 Position : POSITION; 
    float2 TexCoord : TEXCOORD0;
};
struct PS_INPUT
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
};

PS_INPUT VSMain(VS_INPUT input)
{
    PS_INPUT output;
    output.Position = mul(input.Position.x, input.Position.y);
    output.TexCoord = input.TexCoord;
    return output;
}

float4 PSMain(PS_INPUT input) : SV_Target
{
    float2 uv = input.TexCoord;

    float speed = 0.5;
    float2 noiseUV = float2(uv.x, uv.y + time * speed); // should be going upwards?

    float noiseValue = tex2D(NoiseTex, noiseUV).r;

    float intensity = saturate(noiseValue * 1.2);

    return FlameColor * intensity;
}

technique FlameTech
{
    pass P0
    {
        VertexShader = compile vs_3_0 VSMain();
        PixelShader = compile ps_3_0 PSMain();
    }
}
