sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
sampler uImage2 : register(s2);
sampler uImage3 : register(s3);
float4 reflects[8]; // xy - position, z - radius, w - lifetime
float3 reflectColors[8]; // color for each reflection
float2 textureSize;
float globalTime;
int reflectCount;

float4 GetReflects(float2 coords)
{
    float4 totalColor = float4(0, 0, 0, 0);

    for (int i = 0; i < reflectCount; i++)
    {
        float2 reflectPos = reflects[i].xy;     // Position
        float radius = reflects[i].z / 11.0;    // Radius divided by 11

        float2 diff = coords - reflectPos;
        float adx = abs(diff.x);
        float ady = abs(diff.y);
        
        if (adx > radius)
            continue;
        
        float invRadius = 1.0 / radius;
        float strengthX = saturate(1.0 - adx * invRadius);
        float yBias = radius + saturate(1.0 - diff.x * invRadius) * 0.5; // Adds a bit bias to the X center
        float strengthY = saturate(1.0 - ady / yBias);
        float strength = strengthX * strengthY;
        
        float lifetime = abs(reflects[i].w);
        strength *= lifetime * lifetime;

        totalColor += float4(reflectColors[i] * strength, strength);
    }

    return totalColor;
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 noiseCoords = coords * float2(1.0, 0.5);                     // Coords with double X size since the body sprite X size is [at least seemingly] twice as large
    float4 tex = tex2D(uImage0, coords);                                // Original body texture
    float4 mask = tex2D(uImage1, coords);                               // The mask for the body, red gives fill & outline while yellow gives outline
    float4 wavy = tex2D(uImage2, noiseCoords);                          // Cool noise number one
    float4 noise = tex2D(uImage3, noiseCoords);                         // Cool noise number two (generic name warning)

    float4 refl = GetReflects(coords) * 6.5;                            // Get reflections and multiply its strength by 6.5
    float3 reflEffect = float3(refl.r, refl.g, refl.b) * mask.r;        // Use mask on the reflections
    reflEffect *= (wavy.r * 0.8) + (noise.r * 0.15) + 0.05;             // Apply noise to the reflection fill
    reflEffect = reflEffect / (1.0 + reflEffect);                       // Normalize the color (don't make it white)
    reflEffect *= 1.0 + mask.g * 2.5;                                   // Add outline

    float reflAlpha = saturate(refl.a);
    float flash = reflAlpha * reflAlpha;
    reflEffect *= 0.4 + flash * 0.6;                                    // Add flash effect
    
    float wrap = 0.75 + smoothstep(0.0, 0.25, mask.g * reflAlpha);      // Make the fill darker than the outline
    reflEffect *= wrap;
    
    float3 darken = tex.rgb * (1.0 - reflAlpha * 0.25);                 // Darken the areas where the reflections happen to make them look stronger
    float3 finalColor = darken + reflEffect;
    return float4(finalColor, tex.a);
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}