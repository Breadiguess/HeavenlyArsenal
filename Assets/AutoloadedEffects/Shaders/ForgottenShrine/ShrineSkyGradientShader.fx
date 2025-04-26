sampler noiseTexture : register(s1);

float globalTime;
float gradientYOffset;
float gradientSteepness;
float gradientCount;
float4 gradientTop;
float4 gradientBottom;
float3 gradient[4];

float3 PaletteLerp(float interpolant)
{
    int startIndex = clamp(frac(interpolant) * gradientCount, 0, gradientCount - 1);
    int endIndex = (startIndex + 1) % gradientCount;
    return lerp(gradient[startIndex], gradient[endIndex], frac(interpolant * gradientCount));
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float gradientOffsetNoise = tex2D(noiseTexture, coords * float2(2, 0.1)) * 0.004;
    float y = saturate(gradientOffsetNoise + coords.y + gradientYOffset);
    y = smoothstep(0, 1, y);
    
    float noise = tex2D(noiseTexture, coords) * 4;
    float paletteInterpolant = saturate(1 - pow(y, gradientSteepness)) * (1 - 1.0 / gradientCount);
    return float4(PaletteLerp(paletteInterpolant), 1);
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}