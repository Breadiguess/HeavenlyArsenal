sampler baseTexture : register(s1);

float opacity;
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


// The vertex shader function
VertexShaderOutput VertexShaderFunction(in VertexShaderInput input)
{
    float2 coords = input.TextureCoordinates;
    
    
   
    // Initialize the output structure
    VertexShaderOutput output = (VertexShaderOutput) 0;
    
    // Transform the input position by the transformation matrix
    float4 pos = mul(input.Position, transform);
    output.Position = pos; // Set the transformed position in the output
   
    // Pass through the color, texture coordinates, and normal from input to output
    output.Color = input.Color;
    output.TextureCoordinates = input.TextureCoordinates;
    output.Normal = input.Normal;
    
    // Return the output structure
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    return float4(100, 0, 0, opacity);
}

technique Technique1
{
    pass AutoloadPass
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}