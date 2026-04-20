sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float fragmentProgress;
float fragmentStrength;
float2 fragmentDirection;
float edgeWidth;

float noiseScale;
float4 edgeColor;




float4 PixelShaderFunction(float2 uv : TEXCOORD0) : COLOR0
{
    float4 baseColor = tex2D(uImage0, uv);
    float alpha;
    float edge;
    
    if(baseColor.a <= 0.0001f)
        discard;
    
    float threshold = saturate(fragmentProgress);
    
    
    
    float2 noiseUV = uv * noiseScale;
    float noise = tex2D(uImage1, noiseUV).r;
    
    edge = 0.03;
    alpha = smoothstep(threshold- edge, threshold + edge, noise);
    
    float edgeMask = 1.0f - saturate(abs(noise - threshold) / max(edgeWidth, 0.0001f));
    
    float dir = fragmentDirection;
    
    float dirLen = length(dir);
    dir = dirLen > 0.0001f ? dir / dirLen : float2(0.0f, -1.0f);
    
    float2 offsetUv = uv + dir * edgeMask * fragmentStrength * 0.01f;
    float4 shiftedColor = tex2D(uImage0, offsetUv);
    
    
    
    float4 finalColor = lerp(baseColor, shiftedColor, edgeMask * 0.6f);
    finalColor.rgb = lerp(finalColor.rgb, edgeColor.rgb, edgeMask * edgeColor.a);
    
    return finalColor * baseColor.r * alpha;

}




technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();

    }
}