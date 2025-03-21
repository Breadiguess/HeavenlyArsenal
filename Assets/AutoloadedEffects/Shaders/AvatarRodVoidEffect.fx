sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float time;
float2 noiseScale;
float noiseStrength;
float outlineThickness;
float4 edgeColor;

float4 PixelShaderFunction(float2 coords : TEXCOORD0, float4 baseColor : COLOR0) : COLOR0
{
    float noise = tex2D(uImage0, float2(coords.x, coords.y + frac(time) / noiseScale.y) * noiseScale);
    float noise2 = tex2D(uImage1, float2(coords.x * 2, coords.y * 2 + frac(time * 2) / noiseScale.y + noise * 0.4) * noiseScale);
    
    float offset = (coords.y + (noise2 - 0.5) * noiseStrength + pow(coords.y, 3)) * sin(coords.x * 3.14);
    float curve = (coords.y - abs(coords.x * 2 - 1)) * smoothstep(1.05, 0.89, coords.y + (1 - sin(coords.x * 3.14)) * 0.1);

    float image = offset > 1 - curve + outlineThickness;
    float outline = offset > 1 - curve;

    if (outline > image)
        return outline * edgeColor;
    
    return image * baseColor;
}

technique Technique1
{
    pass AutoloadPass // Interesting.
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}