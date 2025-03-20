sampler baseTexture : register(s0);
sampler lightMapTexture : register(s1);

float2 screenSize;
float2 zoom;

float4 PixelShaderFunction(float2 coords : TEXCOORD0, float4 sampleColor : COLOR0, float4 position : SV_Position) : COLOR0
{
    // Just a simple shader that makes light from the light map texture affect a given texture. Easy peasy.
    float4 color = tex2D(baseTexture, coords) * sampleColor;
    float4 lightData = tex2D(lightMapTexture, (position.xy / screenSize - 0.5) / zoom + 0.5);    
    return color * float4(lightData.rgb, 1);
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
