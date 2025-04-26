sampler baseTexture : register(s0);
sampler noiseTexture : register(s1);

float opacity;
float globalTime;
float swirlVariance;
float windDirection;
float4 additiveLight;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Calculate polar coordinates in advance for upcoming calculations.
    float distanceFromCenter = distance(coords, 0.5);
    float2 polar = float2(atan2(coords.y - 0.5, coords.x - 0.5) / 6.283 + 0.5, distanceFromCenter);
    
    // Layered warp noise makes the world go round!
    float noise = tex2D(noiseTexture, polar * 2 - globalTime * 0.3);
    noise = tex2D(noiseTexture, coords * 3 + globalTime * 0.2 + noise * 0.4);
    noise = tex2D(noiseTexture, polar * 2 + globalTime * 0.2 + noise * 0.2);
    
    // Calculate the influence of wind.
    float windNoise = tex2D(noiseTexture, coords * float2(windDirection * 0.2, 2) + globalTime * float2(0.6, 0));
    windNoise = tex2D(noiseTexture, coords * float2(windDirection * 0.1, 1.5) + globalTime * float2(1.5, 0) + windNoise * 0.2);
    
    // Use aforementioned noise values to detemrine the glow factor on the overall additive light.
    // This is used to give mild accents in the results, rather than being a monotone glow.
    float glowInterpolant = lerp(noise, 0.5, smoothstep(0.5, 0.15, distanceFromCenter)) + windNoise;
    float glowFactor = lerp(1 - swirlVariance, 1 + swirlVariance, glowInterpolant);
    
    return tex2D(baseTexture, coords) + opacity * additiveLight * glowFactor;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}