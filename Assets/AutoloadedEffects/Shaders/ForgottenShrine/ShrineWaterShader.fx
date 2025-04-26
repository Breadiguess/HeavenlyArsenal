sampler baseTexture : register(s0);
sampler distortionTexture : register(s1);
sampler waterTexture : register(s2);
sampler noiseTexture : register(s3);

float globalTime;
float perturbationStrength;
float2 perturbationScroll;
float2 zoom;
float2 screenOffset;
float2 targetSize;
float2 screenPosition;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 liquidTextureCoords = (coords - 0.5) / zoom + 0.5 + screenOffset;
    float2 worldStableCoords = (coords - 0.5) / zoom + 0.5 + screenPosition / targetSize;
    float4 waterData = tex2D(waterTexture, liquidTextureCoords);
    
    // Calculate the effects of distortion.
    float4 distortionData = tex2D(distortionTexture, liquidTextureCoords);
    float2 distortionOffset = float2(distortionData.r - 0.5, distortionData.g - 0.5);
    float2 distortionDirection = normalize(distortionOffset);
    float distortionIntensity = length(distortionOffset) * 0.05;
    float2 distortion = distortionDirection * smoothstep(0.05, 0.1, length(distortionOffset)) * distortionIntensity;
    
    float perturbationNoise = tex2D(noiseTexture, worldStableCoords * 4 + perturbationScroll);
    perturbationNoise += pow(tex2D(noiseTexture, worldStableCoords * 6.1 + perturbationScroll * 1.3 + perturbationNoise * 0.1), 2.5);
    distortion += perturbationNoise * perturbationStrength;
    
    // Ensure that distortion only affects water.
    distortion *= any(tex2D(waterTexture, liquidTextureCoords)) || any(tex2D(waterTexture, liquidTextureCoords + distortion));
    
    return tex2D(baseTexture, coords + distortion);
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}