sampler baseTexture : register(s0);

float2 textureSize;
float4 edgeColor;

float4 Sample(float2 coords)
{
    return tex2D(baseTexture, coords);
}

bool AtEdge(float2 coords)
{
    float2 screenCoords = coords * textureSize;
    float left = Sample((screenCoords + float2(-2, 0)) / textureSize).a;
    float right = Sample((screenCoords + float2(2, 0)) / textureSize).a;
    float top = Sample((screenCoords + float2(0, -2)) / textureSize).a;
    float bottom = Sample((screenCoords + float2(0, 2)) / textureSize).a;
    float4 color = Sample(coords);
    bool anyEmptyEdge = !any(left) || !any(right) || !any(top) || !any(bottom);
    
    return anyEmptyEdge && any(color.a);
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 pixelationFactor = 1.5 / textureSize;
    coords = floor(coords / pixelationFactor) * pixelationFactor;
    
    return tex2D(baseTexture, coords) * sampleColor + AtEdge(coords) * edgeColor * sampleColor.a;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}