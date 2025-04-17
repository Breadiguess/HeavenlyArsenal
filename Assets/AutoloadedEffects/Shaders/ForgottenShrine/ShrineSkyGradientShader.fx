sampler noiseTexture : register(s1);
sampler gradientTexture : register(s2);

float globalTime;
float gradientYOffset;
float gradientSteepness;
float4 gradientTop;
float4 gradientBottom;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float gradientOffsetNoise = tex2D(noiseTexture, coords * float2(2, 0.1)) * 0.004;
    float y = saturate(gradientOffsetNoise + coords.y + gradientYOffset);
    y = smoothstep(0, 1, y);
    
    float noise = tex2D(noiseTexture, coords) * 4;
    return tex2D(gradientTexture, 1 - pow(y, gradientSteepness));
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}