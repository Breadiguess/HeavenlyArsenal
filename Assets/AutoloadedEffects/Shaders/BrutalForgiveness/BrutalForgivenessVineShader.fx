sampler baseTexture : register(s1);
sampler normalMapTexture : register(s2);
sampler lightMapTexture : register(s3);

float globalTime;
float diffuseLightExponent;
float2 textureLookupZoom;
float2 screenSize;
float2 gameZoom;
float3 lightPosition;
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
    float3 color = input.Color.rgb * pow(tex2D(baseTexture, coords).rgb, 0.75);
    float3 normal = tex2D(normalMapTexture, coords).xyz;
    
    // Calculate light based on the normal map.
    float2 screenCoords = (input.Position.xy / screenSize - 0.5) / gameZoom + 0.5;
    float3 ambientLight = tex2D(lightMapTexture, screenCoords).rgb;
    
    float diffuseInterpolant = smoothstep(0, 1, dot(input.Normal, float3(0, 0, -1)));
    float3 lightDirection = normalize(float3(screenCoords, 0) - lightPosition);
    float3 diffuse = pow(saturate(dot(normal, lightDirection) * lerp(0.5, 1, diffuseInterpolant)), diffuseLightExponent);
    
    float3 result = min(color * ambientLight + color * diffuse, ambientLight);
    
    return float4(result, 1);
}

technique Technique1
{
    pass AutoloadPass
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
