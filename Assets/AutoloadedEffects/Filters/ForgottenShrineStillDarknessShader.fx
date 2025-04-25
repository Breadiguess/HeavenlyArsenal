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

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Calculate coordinates.
    float2 worldPosition = mul(float4(coords, 0, 1), uvToWorld).xy;
    float glowInterpolant = smoothstep(0, 0.7, dot(tex2D(glowTargetTexture, coords + screenOffset).rgb, 0.333));
    
    float darknessInterpolant = smoothstep(islandLeft, islandLeft + darknessTaperDistance, worldPosition.x) *
                                smoothstep(islandRight, islandRight - darknessTaperDistance, worldPosition.x);
    float darkness = lerp(1, baseDarkness, darknessInterpolant * opacity);
    float brightness = lerp(darkness, 1, glowInterpolant * 1.75);
    
    return tex2D(baseTexture, coords) * brightness;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}