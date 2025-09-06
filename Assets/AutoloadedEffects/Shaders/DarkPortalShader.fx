sampler baseTexture : register(s0);
sampler noiseTextureA : register(s1);
sampler noiseTextureB : register(s2);

float time;
float baseCutoffRadius;
float swirlOutwardnessExponent;
float swirlOutwardnessFactor;
float vanishInterpolant;
float edgeColorBias;
float4 edgeColor;

float globalTime;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Calculate polar coordinates.
    float angleFromCenter = atan2(coords.y - 0.5, coords.x - 0.5);
    float distanceToCenter = distance(coords, 0.5);
    float2 polar = float2(distanceToCenter, angleFromCenter / 6.283 + 0.5);
    
    float2 polarA = polar;
    polarA.y = fmod(polarA.y + time * 1.91, 1);
    
    float2 polarB = polar;
    polarB.y = fmod(polarB.y + time * -1.91, 1);
    
    // Calculate noise textures and combine them together.
    float noiseA = tex2D(noiseTextureA, polarA * float2(3, 1) + float2(-3.8, 0) * time);
    float noiseB = tex2D(noiseTextureB, polarB * float2(7, 1) + float2(-2.78, 0) * time);
    float combinedNoise = dot(float2(noiseB, noiseA), float2(0.582, 0.813)) - 0.74;
    
    // Determine the erasure value.
    // Once this meets or exceeds 1, a pixel is erased.
    float erasureValue = distanceToCenter / baseCutoffRadius;
    erasureValue += pow(erasureValue, swirlOutwardnessExponent) * combinedNoise * saturate(0.15 / distanceToCenter) * swirlOutwardnessFactor * (1 - vanishInterpolant);
    erasureValue += vanishInterpolant * 1.6;
    
    // Calculate the base color and ensure that edges receive a good amount of red.
    float4 baseColor = tex2D(baseTexture, polarA + coords * 0.2) * (erasureValue < 1);
    baseColor.rgb *= smoothstep(0.05, 0.187, distanceToCenter);
    baseColor += edgeColor * smoothstep(0.72, 0.9, erasureValue + edgeColorBias) * baseColor.a;
    
    return baseColor;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}