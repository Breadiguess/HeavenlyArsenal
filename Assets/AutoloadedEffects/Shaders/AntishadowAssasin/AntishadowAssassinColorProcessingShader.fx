sampler baseTexture : register(s0);
sampler noiseTexture : register(s1);

float globalTime;
float freezeInterpolant;
float gradientCount;
float eyeScale;
float2 textureSize;
float3 gradient[6];
float4 edgeColor;

float3 PaletteLerp(float interpolant)
{
    int startIndex = clamp(interpolant * gradientCount, 0, gradientCount - 1);
    int endIndex = clamp(startIndex + 1, 0, gradientCount - 1);
    return lerp(gradient[startIndex], gradient[endIndex], frac(interpolant * gradientCount));
}

float4 Sample(float2 coords)
{
    return tex2D(baseTexture, coords);
}

bool AtEdge(float2 coords)
{
    float2 screenCoords = coords * textureSize;
    float left = Sample((screenCoords + float2(-2, 0)) / textureSize).a;
    float right = Sample((screenCoords + float2(2, 0)) / textureSize).a;
    float top = Sample((screenCoords + float2(0, -2)) / textureSize).a;
    float bottom = Sample((screenCoords + float2(0, 2)) / textureSize).a;
    float4 color = Sample(coords);
    bool anyEmptyEdge = !any(left) || !any(right) || !any(top) || !any(bottom);
    
    return anyEmptyEdge && any(color.a);
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 result = sampleColor;
    
    float4 colorData = tex2D(baseTexture, coords);
    bool eyeInUse = colorData.r > 0;

    // Calculate the eye color based on noise offset by the red channel, and faded in opacity by the blue channel.
    float eyeOffsetNoise = tex2D(noiseTexture, coords * 0.2 + colorData.r + globalTime) * eyeInUse;
    float eyeCenterFade = smoothstep(0.1, 0.67, colorData.b) * smoothstep(1, 0.5, colorData.b * eyeScale);
    float4 eyeColor = float4(PaletteLerp(cos(globalTime * 4 + eyeOffsetNoise * 2.5) * 0.5 + 0.5), 1) * colorData.a * 2;
    result += tex2D(noiseTexture, coords + eyeOffsetNoise * 0.03) * eyeInUse * eyeCenterFade * eyeColor;
    
    // Make the edges a distinct color.
    result += edgeColor * AtEdge(coords);
    
    float opacity = colorData.a * (1 - colorData.g);    
    return saturate(result) * opacity;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}