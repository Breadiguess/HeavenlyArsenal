sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float dissolveProgress;
float edgeWidth;
float opacity;
float4 edgeColor;

float2 dissolveDirection;

float directionalStrength;

float noiseStrength;
float gradientStrength;

float4 MainPS(float2 uv : TEXCOORD0) : COLOR
{
    float4 texColor = tex2D(uImage0, uv);

    if (texColor.a <= 0.001f)
        discard;

    float noise = tex2D(uImage1, uv).r;
    float2 dir = dissolveDirection;
    
    float dirLen = length(dir);
    
    if(dirLen > 0.00001f)
        dir /= dirLen;
    else
        dir = float2(1.0f, 0.0f);
    
    float2 centeredUv = uv * 2.0f - 1.0f;
    float directionalGradient = dot(centeredUv, dir);
    directionalGradient = directionalGradient * 0.5f + 0.5f;
    
    float dissolveValue = noise * noiseStrength + directionalGradient + gradientStrength;
    
    float alphaAwareProgress = saturate(dissolveProgress / max(texColor.a, 0.001f));
    
    float remaining = saturate((dissolveValue - alphaAwareProgress) / max(edgeWidth, 0.0001f));
    
    if (remaining <= 0.001f)
        discard;
    float edgeMask = saturate(texColor.a * 4.0f);
    
    float3 finalColor = lerp(edgeColor.rgb, texColor.rgb, remaining * edgeMask);
    
    float finalAlpha = texColor.a * remaining * opacity;
    
    return float4(finalColor, finalAlpha);

}


technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_2_0 MainPS();
    }
}