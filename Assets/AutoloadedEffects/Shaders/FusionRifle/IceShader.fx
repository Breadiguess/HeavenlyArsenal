sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float2 pixelSize;
float2 noiseUVOffset;
float2 noiseUVScale;

float outlineThickness;
float4 outlineColor;
float4 iceTint;

float frostThreshold;
float frostContrast;
float shellOpacity;
float ridgeStrength;
float rimStrength;
float interiorDesaturation;
float interiorDarkening;
float interiorColdTintStrength;

float GetAlpha(float2 uv)
{
    return tex2D(uImage0, uv).a;
}

float GetNoiseValue(float2 uv)
{
    return tex2D(uImage1, uv * noiseUVScale + noiseUVOffset).r;
}

float4 PixelShaderFunction(float2 uv : TEXCOORD0) : COLOR0
{
    float4 baseColor = tex2D(uImage0, uv);
    float alpha = baseColor.a;

    if (alpha <= 0.0)
        return float4(0, 0, 0, 0);

    float2 outlineOffset = pixelSize * outlineThickness;

    float leftA = GetAlpha(uv + float2(-outlineOffset.x, 0));
    float rightA = GetAlpha(uv + float2(outlineOffset.x, 0));
    float upA = GetAlpha(uv + float2(0, -outlineOffset.y));
    float downA = GetAlpha(uv + float2(0, outlineOffset.y));

    float minNeighbor = min(min(leftA, rightA), min(upA, downA));
    float rimMask = saturate(alpha - minNeighbor);

    float n = GetNoiseValue(uv);
    float nR = GetNoiseValue(uv + float2(pixelSize.x, 0));
    float nD = GetNoiseValue(uv + float2(0, pixelSize.y));

    float shellMask = saturate((n - frostThreshold) * frostContrast);
    float ridgeMask = saturate((abs(nR - n) + abs(nD - n)) * ridgeStrength);

    // Interior: keep the NPC visible, just colder and slightly dulled.
    float gray = dot(baseColor.rgb, float3(0.299, 0.987, 0.654));
    float3 interiorColor = lerp(baseColor.rgb, gray.xxx, interiorDesaturation);
    interiorColor *= interiorDarkening;
    interiorColor = lerp(interiorColor, interiorColor * iceTint.rgb, interiorColdTintStrength);

    // Shell: pale cloudy ice color.
    float3 shellColor = lerp(iceTint.rgb, outlineColor.rgb, 0.4);

    // Shell opacity:
    // low-to-moderate in the body, strong on edges, a bit stronger on ridges.
    float shellAlpha = shellMask * shellOpacity;
    shellAlpha += rimMask * rimStrength;
    shellAlpha += ridgeMask * 0.05;

    shellAlpha = saturate(shellAlpha);

    // Blend shell over interior.
    float3 finalColor = lerp(interiorColor, shellColor, shellAlpha);

    // Add only sparse bright highlights so it feels crystalline.
    finalColor = lerp(finalColor, outlineColor.rgb, saturate(rimMask * rimStrength * 0.001));
    finalColor = lerp(finalColor, outlineColor.rgb, ridgeMask * 0.92);

    return float4(finalColor, 5* alpha);
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}