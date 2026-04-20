sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float dissolveProgress;
float edgeWidth;
float opacity;
float4 edgeColor;


float4 MainPS(float2 uv : TEXCOORD0) : COLOR
{
    float4 texColor = tex2D(uImage0, uv);
    float noise = tex2D(uImage1, uv).r;

    if (texColor.a <= 0.03f)
        discard;
   
    
    float threshold = dissolveProgress;
    
    float edge;
    float alpha;
    edge = 0.03;
    alpha = smoothstep(threshold - edge, threshold + edge, noise);
    
    // Edge band
    float edgeFactor = smoothstep(dissolveProgress, dissolveProgress + edgeWidth, noise);

    float3 finalColor = lerp(edgeColor.rgb, texColor.rgb, edgeFactor);
    float finalAlpha = texColor.a * edgeFactor * opacity;

    return float4(finalColor, finalAlpha) * alpha;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_2_0 MainPS();
    }
}
