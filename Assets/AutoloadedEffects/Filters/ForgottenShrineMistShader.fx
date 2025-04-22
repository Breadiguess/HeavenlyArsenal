sampler baseTexture : register(s0);
sampler noiseTexture : register(s1);
sampler liquidTexture : register(s2);
sampler lightTexture : register(s3);
sampler lightDistanceTexture : register(s4);

float globalTime;
float noiseAppearanceThreshold;
float mistHeight;
float2 zoom;
float2 mistCoordinatesZoom;
float2 screenPosition;
float2 oldScreenPosition;
float2 targetSize;
float4 mistColor;

float DistanceToLiquidPixel(float2 coords)
{
    return tex2D(lightDistanceTexture, coords).r * targetSize.y;
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Calculate coordinates.
    float2 screenOffset = (screenPosition - oldScreenPosition) / targetSize;
    float2 worldStableCoords = (coords - 0.5) / zoom + 0.5 + screenPosition / targetSize;
    float2 liquidTextureCoords = (coords - 0.5) / zoom + 0.5 + screenOffset;
    
    // Determine how much light is assigned to this pixel.
    float light = tex2D(lightTexture, liquidTextureCoords);
    
    // Determine how much mist should be present. Only pixels above liquid may receive mist.
    float distanceToLiquid = DistanceToLiquidPixel(liquidTextureCoords);
    float modulatedDistanceToLiquid = distanceToLiquid + cos(globalTime * 1.1 + worldStableCoords.x * 20) * 10;
    float mistInterpolant = smoothstep(0, 30, modulatedDistanceToLiquid) * smoothstep(mistHeight, mistHeight * 0.45, modulatedDistanceToLiquid);
    
    // Make mist dissipate if light is low.
    mistInterpolant *= smoothstep(0, 0.5, pow(light, 1.6));
    
    // Make mist dissipate if the water is shallow.
    mistInterpolant *= smoothstep(0.1, 0.3, tex2D(lightDistanceTexture, liquidTextureCoords).g);
    
    // Do some standard noise calculations to determine the shape of the mist.
    float time = globalTime * 0.3;
    float2 noiseCoords = worldStableCoords * mistCoordinatesZoom;
    float warpNoise = tex2D(noiseTexture, noiseCoords * float2(0.3, 2.76) + float2(time * 0.02, 0)) * 0.045;
    float mistNoiseA = tex2D(noiseTexture, noiseCoords * float2(0.6, 1.1) + float2(time * -0.03, 0.3) - warpNoise);
    float mistNoiseB = tex2D(noiseTexture, noiseCoords * float2(0.2, 1.4) + float2(time * 0.02, 0.5) + warpNoise + mistNoiseA * 0.1);
    float mistNoise = smoothstep(0, 0.6, sqrt(mistNoiseA * mistNoiseB) - noiseAppearanceThreshold);
    
    return tex2D(baseTexture, coords) + mistColor * mistInterpolant * mistNoise;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}