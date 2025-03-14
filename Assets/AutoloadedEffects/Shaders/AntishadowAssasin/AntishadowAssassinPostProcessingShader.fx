sampler baseTexture : register(s0);
sampler disappearanceNoise : register(s1);

float blurOffset;
float disappearanceInterpolant;
float direction;
float2 blurDirection;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float4 position : SV_Position) : COLOR0
{
    float2 pixelatedCoords = round(coords * 150) / 150;
    
    float disappearanceNoiseValue = tex2D(disappearanceNoise, pixelatedCoords * float2(0, 2) + float2(disappearanceInterpolant * 0.05, 0.15)) - 0.5;
    coords.x += disappearanceInterpolant * pow(disappearanceNoiseValue, 2) * direction * 2;
    
    float4 blurredColor = 0;
    for (int i = 0; i < 40; i++)
        blurredColor += tex2D(baseTexture, coords + blurDirection * blurOffset * i * 0.025) * 0.025;
    
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