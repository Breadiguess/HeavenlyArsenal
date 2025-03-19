sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float time;
float brightness;
float spin;
matrix uWorldViewProjection;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float3 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float3 TextureCoordinates : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;
    float4 pos = mul(input.Position, uWorldViewProjection);
    output.Position = pos;
    
    output.Color = input.Color;
    output.TextureCoordinates = input.TextureCoordinates;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TextureCoordinates;   
    // Apply smoothening to the visual.
    coords.y = (coords.y - 0.5) / input.TextureCoordinates.z + 0.5;
    float4 color = tex2D(uImage0, float2(coords.x - frac(time), coords.y - coords.x * spin));
    float4 glow = tex2D(uImage1, float2(coords.x - frac(time * 2), coords.y - frac(time) * spin));
    float mainColor = smoothstep(0.1, 0.22, length(color.rgb) * pow((1 - coords.x), 4) * (1 - coords.x));
    float glowColor = pow(length(glow.rgb), 0.5 + coords.x * 3) * (1 - coords.x);
    return (pow((mainColor + glowColor), 2) + sin(coords.y * 3.14) * (1 - coords.x * 1.5)) * input.Color * brightness;
}

technique Technique1
{
    pass AutoloadPass
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
