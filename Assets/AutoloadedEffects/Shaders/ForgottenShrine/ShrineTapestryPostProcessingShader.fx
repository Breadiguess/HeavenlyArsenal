sampler baseTexture : register(s0);
sampler lightMapTexture : register(s1);

float2 pixelationFactor;
float2 screenSize;
float2 zoom;


float4 Sample(float2 coords)
{
    return tex2D(baseTexture, coords);
}

bool AtEdge(float2 coords)
{
    float2 screenCoords = coords;
    float left = Sample((coords + float2(-1, 0) * pixelationFactor)).a;
    float right = Sample((coords + float2(1, 0) * pixelationFactor)).a;
    float top = Sample((coords + float2(0, -1) * pixelationFactor)).a;
    float bottom = Sample((coords + float2(0, 1) * pixelationFactor)).a;
    float4 color = Sample(coords);
    bool anyEmptyEdge = !any(left) || !any(right) || !any(top) || !any(bottom);
    
    return anyEmptyEdge && any(color.a);
}

float4 PixelFunction(float2 coords : TEXCOORD0, float4 color : COLOR0, float4 position : SV_Position) : COLOR0
{
    float2 pixelatedCoords = round(coords / pixelationFactor) * pixelationFactor;
    float4 baseColor = Sample(pixelatedCoords);
    float4 lightData = tex2D(lightMapTexture, (position.xy / screenSize - 0.5) / zoom + 0.5);
    
    bool atEdge = AtEdge(pixelatedCoords);
    baseColor.rgb *= lerp(1, 0.4, atEdge);
    
    return baseColor * float4(lightData.rgb, 1);
}
technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelFunction();
    }
}