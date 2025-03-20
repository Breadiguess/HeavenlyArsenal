sampler baseTexture : register(s1);
sampler normalMapTexture : register(s2);
sampler lightMapTexture : register(s3);

float globalTime;
float diffuseLightExponent;
float2 textureLookupZoom;
float2 screenSize;
float2 gameZoom;
matrix uWorldViewProjection;

struct VertexShaderInput
{
    float3 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
    float3 Normal : NORMAL0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
    float3 Normal : NORMAL0;
};

VertexShaderOutput VertexShaderFunction(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;
    float4 pos = mul(float4(input.Position, 1), uWorldViewProjection);
    output.Position = pos;
    
    output.Color = input.Color;
    output.TextureCoordinates = input.TextureCoordinates;
    output.Normal = input.Normal;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TextureCoordinates * textureLookupZoom;
    float4 color = input.Color * pow(tex2D(baseTexture, coords), 0.75) * 1.7;
    float3 normal = tex2D(normalMapTexture, coords).xyz;
    
    // Calculate light based on two normals: The normal map and the normals generated on the vertices.
    float4 lightData = tex2D(lightMapTexture, (input.Position.xy / screenSize - 0.5) / gameZoom + 0.5);
    float diffuse = saturate(dot(normal, float3(0, 0, 1)) * dot(input.Normal, float3(0, 0, -1)));
    float3 light = pow(diffuse, diffuseLightExponent) * lightData.rgb;
    
    // Combine the base color with light.
    float4 result = float4(light, 1) * color;    
    return result;
}

technique Technique1
{
    pass AutoloadPass
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
