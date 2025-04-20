sampler baseTexture : register(s0);
sampler noiseTexture : register(s1);
sampler liquidTexture : register(s2);

float globalTime;
float reflectionMaxDepth;
float reflectionStrength;
float reflectionWaviness;
float2 zoom;
float2 screenPosition;
float2 oldScreenPosition;
float2 targetSize;

float CalculateLiquidPixelLineY(float2 coords)
{
    float top = 0;
    float bottom = 1;

    // Perform binary search to find the first Y with alpha > 0.
    for (int i = 0; i < 15; i++)
    {
        float midpoint = (top + bottom) * 0.5;
        float alpha = tex2D(liquidTexture, float2(coords.x, midpoint)).a;

        // If transparent, look further down.
        bool lookFurtherDown = alpha < 0.01;
        top = lerp(top, midpoint, lookFurtherDown);
        bottom = lerp(bottom, midpoint, !lookFurtherDown);
    }
    
    return bottom;
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Calculate coordinates.
    float2 screenOffset = (screenPosition - oldScreenPosition) / targetSize;
    float2 worldStableCoords = (coords - 0.5) / zoom + 0.5 + screenPosition / targetSize;
    float2 liquidTextureCoords = (coords - 0.5) / zoom + 0.5 + screenOffset;
    
    // Calculate the reflection line Y position.
    float reflectionLineY = CalculateLiquidPixelLineY(liquidTextureCoords);
    float reconvertedReflectionY = (reflectionLineY - screenOffset.y - 0.5) * zoom.y + 0.5;
    float4 baseColor = tex2D(baseTexture, coords);
    
    // Use the reflection line to determine how deep in the water the current pixel is.
    float depth = (liquidTextureCoords.y - reflectionLineY) * targetSize.y;
    
    // Determine how strong reflections should be at this pixel.
    // This effect is strongest at the top of water and tapers off from there.
    float reflectionInterpolant = smoothstep(reflectionMaxDepth, 0, depth) * smoothstep(-1, 0, depth);
    
    // Determine how much coordinates should be stretched vertically as a result of reflections.
    float stretch = 1 + (1 - reflectionInterpolant) * -0.4;
    
    // Use reflection math to determine the color of the pixel when reflected along the reflection line.
    float reflectedY = reconvertedReflectionY * 2 - coords.y;
    float reflectionWave = sin(worldStableCoords.y * 300 + globalTime * 3) * (1 - reflectionInterpolant) * reflectionWaviness;
    float2 reflectionCoords = float2(coords.x + reflectionWave, (reflectedY - 0.5) * stretch + 0.5);
    float edgeOfScreenTaper = smoothstep(0, 0.1, reflectionCoords.y) * smoothstep(1, 0.9, reflectionCoords.y);
    float4 reflectedColor = tex2D(baseTexture, reflectionCoords);
    
    // Make reflections brighter in spots where it's already bright.
    float reflectionBrightness = dot(reflectedColor.rgb, float3(0.3, 0.6, 0.1));
    reflectedColor *= 1 + smoothstep(0.5, 1, reflectionBrightness) * 0.93;
    
    // Combine things together.
    return baseColor + reflectionInterpolant * reflectedColor * reflectionStrength * edgeOfScreenTaper;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}