sampler paperTexture : register(s1);
sampler textTexture : register(s2);
sampler lightTargetTexture : register(s3);

float opacity;
float2 gameZoom;
float2 screenSize;
matrix transform;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
    float3 Normal : NORMAL0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
    float3 Normal : TEXCOORD1;
};

VertexShaderOutput VertexShaderFunction(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;
    float4 pos = mul(input.Position, transform);
    output.Position = pos;
    
    output.Color = input.Color;
    output.TextureCoordinates = input.TextureCoordinates;
    output.Normal = input.Normal;
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TextureCoordinates;
    float2 screenCoords = (input.Position.xy / screenSize - 0.5) / gameZoom + 0.5;
    float4 paperColor = pow(tex2D(paperTexture, coords * 0.5), 1);
    float4 textColor = tex2D(textTexture, float2(coords.y * 1.1, 1 - coords.x));
    
    float4 color = (paperColor * textColor + paperColor * (1 - textColor.a)) * tex2D(lightTargetTexture, screenCoords);
    
    return color * opacity;
}

technique Technique1
{
    pass AutoloadPass
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}