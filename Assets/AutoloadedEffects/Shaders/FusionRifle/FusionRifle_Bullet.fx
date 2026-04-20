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
    
    // Transform the vertex position by the world-view-projection matrix
    float4 pos = mul(input.Position, uWorldViewProjection);
    output.Position = pos;
    
    // Pass the vertex color to the output
    output.Color = input.Color;
    
    // Pass the texture coordinates to the output
    output.TextureCoordinates = input.TextureCoordinates;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TextureCoordinates.xy;
    coords.y = (coords.y - 0.5) / input.TextureCoordinates.z + 0.5;

    float4 color = tex2D(uImage0, float2(coords.x - frac(time), coords.y - coords.x * spin));
    float4 glow = tex2D(uImage1, float2(coords.x - frac(time * 2), coords.y - frac(time) * spin));

    float mainColor = smoothstep(0.1, 0.62, length(color.rgb) * pow((1 - coords.x), 4) * (1 - coords.x));
    float glowColor = pow(length(glow.rgb), 0.5 + coords.x * 3) * (1 - coords.x);

    float trail = (pow(mainColor + glowColor, 2) + sin(coords.y * 3.14159) * (1 - coords.x * 1.5));

    float alphaMask = 1.0;

    float capStart = 0.18;
    if (coords.x > capStart)
    {
        float localX = (coords.x - capStart) / (1.0 - capStart); // 0 to 1 across cap
        float2 capUV = float2(localX, coords.y);

        float2 p = float2(capUV.x - 0.0, capUV.y - 0.5);
        float dist = length(p);

        // semicircle centered on left-middle of cap region
        alphaMask *= 1.0 - smoothstep(0.5, 0.52, dist);
    }

    return trail * input.Color * brightness * alphaMask;
}
technique Technique1
{
    pass AutoloadPass
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
