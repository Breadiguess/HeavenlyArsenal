sampler baseTexture : register(s0);
sampler lightMapTexture : register(s1);

float2 screenSize;
float2 zoom;

float4 PixelFunction(float2 coords : TEXCOORD0, float4 color : COLOR0, float4 position : SV_Position) : COLOR0
{
    float4 baseColor = tex2D(baseTexture, coords);
    float4 lightData = tex2D(lightMapTexture, (position.xy / screenSize - 0.5) / zoom + 0.5);
    return baseColor * float4(lightData.rgb, 1);
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelFunction();
    }
}