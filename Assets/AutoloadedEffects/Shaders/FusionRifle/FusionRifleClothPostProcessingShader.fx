sampler baseTexture : register(s0);

float2 textureSize;
float4 edgeColor;

// Returns the sampled texture color
float4 Sample(float2 coords)
{
    return tex2D(baseTexture, coords);
}

// Determines if the given coordinate is near an edge (using neighboring alpha values)
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
    // Preserve the original texture coordinate for checking the vertical position.
    float2 originalCoords = coords;
    
    // Pixelate the texture coordinates.
    float2 pixelationFactor = 1.5 / textureSize;
    float2 pixelatedCoords = floor(coords / pixelationFactor) * pixelationFactor;
    
    // Compute the basic cloth color using the pixelated coordinate and add edge color if necessary.
    float4 clothColor = tex2D(baseTexture, pixelatedCoords) * sampleColor
                        + (AtEdge(pixelatedCoords) ? edgeColor * sampleColor.a : 0);
    
    // If the original texture coordinate indicates the bottom of the cloth,
    // overlay the base texture (using the unpixelated coordinate).
    // Adjust the threshold (0.8) as needed.
    if (originalCoords.y > 0.8)
    {
        // This simply replaces the clothColor with the base texture sample.
        // Alternatively, you can blend the two if you wish.
        clothColor = tex2D(baseTexture, originalCoords);
    }
    
    return clothColor;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
