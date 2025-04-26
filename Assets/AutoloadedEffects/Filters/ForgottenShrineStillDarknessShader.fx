sampler baseTexture : register(s0);
sampler glowTargetTexture : register(s1);

float opacity;
float globalTime;
float baseDarkness;
float islandLeft;
float islandRight;
float darknessTaperDistance;
float2 zoom;
float2 screenPosition;
float2 screenOffset;
float2 targetSize;
matrix uvToWorld;

float CalculateColorSaturation(float3 color)
{
    float brightness = dot(color, float3(0.3, 0.6, 0.1));
    return (distance(color.r, brightness) + distance(color.g, brightness) + distance(color.b, brightness)) * 0.333;
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseColor = tex2D(baseTexture, coords);
    float4 glowColor = tex2D(glowTargetTexture, coords + screenOffset);
    
    // Calculate world coordinates.
    float2 worldPosition = mul(float4(coords, 0, 1), uvToWorld).xy;
    
    // Without this the moon will "reveal" parts of mountains that it's supposed to be covered by.
    // Scuffed, but it works, and the alternatives are considerably more complicated.
    float redness = smoothstep(0.1, 0.24, glowColor.r - glowColor.g - glowColor.b);
    float moonLayeringMask = CalculateColorSaturation(glowColor.rgb) <= 0.7 && CalculateColorSaturation(baseColor.rgb) >= 0.06;
    moonLayeringMask = lerp(moonLayeringMask, 1, 1 - redness);
    
    // Determine how much of the original view should be revealed based on the brightness of the glow data.
    float glowInterpolant = smoothstep(0, 0.7, dot(glowColor.rgb, 0.333)) * moonLayeringMask;
    
    // Determine how much darkness should appear based on world position.
    float darknessPositionInterpolant = smoothstep(islandLeft, islandLeft + darknessTaperDistance, worldPosition.x) *
                                        smoothstep(islandRight, islandRight - darknessTaperDistance, worldPosition.x);
    darknessPositionInterpolant = pow(darknessPositionInterpolant, 1.75);
    
    float darkness = lerp(1, baseDarkness, darknessPositionInterpolant * opacity);
    float brightness = max(darkness, lerp(darkness, 1, glowInterpolant * 1.75));
    
    return baseColor * brightness;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}