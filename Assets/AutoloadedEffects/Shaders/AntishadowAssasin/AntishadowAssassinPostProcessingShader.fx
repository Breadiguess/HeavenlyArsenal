sampler baseTexture : register(s0);
sampler disappearanceNoise : register(s1);

float blurOffset;
float disappearanceInterpolant;
float direction;
float2 blurDirection;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float4 position : SV_Position) : COLOR0
{
    float2 pixelatedCoords = round(coords * 150) / 150;
    
    // Horizontally offset coords based on noise. This uses the pixelated coordinates from above, and only a 1D slice of noise is taken, so this results
    // in what looks like horizontally offset bars that reach out as the assassin disappears, kind of like the disappearance animation of Erazor from SoA.
    float disappearanceNoiseValue = tex2D(disappearanceNoise, pixelatedCoords * float2(0, 2) + float2(disappearanceInterpolant * 0.05, 0.15)) - 0.5;
    coords.x += disappearanceInterpolant * pow(disappearanceNoiseValue, 2) * direction * 2;
    
    // Apply motion blur.
    float4 blurredColor = 0;
    for (int i = 0; i < 40; i++)
        blurredColor += tex2D(baseTexture, coords + blurDirection * blurOffset * i * 0.025) * 0.025;
    
    // Fade to black when disappearing.
    blurredColor = lerp(blurredColor, float4(0, 0, 0, blurredColor.a), disappearanceInterpolant);
    
    return blurredColor * sampleColor * sqrt(1 - disappearanceInterpolant);
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}