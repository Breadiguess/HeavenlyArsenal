sampler baseTexture : register(s0);
sampler waterTexture : register(s1);

float globalTime;
float underwaterOpacity;
float2 zoom;
float2 screenOffset;
float2 targetSize;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float4 position : SV_Position) : COLOR0
{
    float2 liquidTextureCoords = (position.xy / targetSize - 0.5) / zoom + 0.5;
    float4 waterData = tex2D(waterTexture, liquidTextureCoords);
    float opacity = lerp(underwaterOpacity, 1, smoothstep(0.4, 0, waterData.a));

    return tex2D(baseTexture, coords) * opacity * sampleColor;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}