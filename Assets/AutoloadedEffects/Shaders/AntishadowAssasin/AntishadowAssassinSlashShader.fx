sampler noiseTexture : register(s1);
sampler slashOpacityTexture : register(s2);

float globalTime;
float lifetimeRatio;
float noiseSlant;
float noiseInfluenceFactor;
float opacityFadeExponent;
float4 sheenEdgeColorWeak;
float4 sheenEdgeColorStrong;
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
    float4 pos = mul(float4(input.Position.xyz, 1), uWorldViewProjection);
    output.Position = pos;
    
    output.Color = input.Color;
    output.TextureCoordinates = input.TextureCoordinates;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float4 color = input.Color;
    float2 coords = input.TextureCoordinates;
    coords.y = (coords.y - 0.5) / input.TextureCoordinates.z + 0.5;
    
    // Make the top of the slash have two-toned color variances.
    color = lerp(color, sheenEdgeColorWeak, smoothstep(0.56, 1, coords.y));
    color = lerp(color, sheenEdgeColorStrong, smoothstep(0.85, 1, coords.y));
    
    // Apply some noise to the results, to add texturing.
    color.rgb += (tex2D(noiseTexture, coords + float2(globalTime * -1.3, coords.x * noiseSlant)) - 0.5) * coords.y * noiseInfluenceFactor;
    
    // Make the top and bottom of the slash fade out.
    color *= pow(smoothstep(0, 0.4, coords.y) * smoothstep(1, 0.95, coords.y), opacityFadeExponent);
    color *= tex2D(slashOpacityTexture, coords).r;
    
    // Make the front of the slash fade out, so that it doesn't have a noticeable harsh edge.
    color *= pow(smoothstep(0, 0.1, coords.x), 2);
    
    return saturate(color) * input.Color.a;
}

technique Technique1
{
    pass AutoloadPass
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
