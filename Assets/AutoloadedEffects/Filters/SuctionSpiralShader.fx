sampler baseTexture : register(s0);
sampler spiralTexture : register(s1);

float globalTime;
float suctionOpacity;
float zoom;
float suctionBaseRange;
float suctionFadeOutRange;
float2 screenSize;
float2 zoomedScreenSize;
float2 suctionCenter;

float2 RotatedBy(float2 v, float theta)
{
    float s = sin(theta);
    float c = cos(theta);
    return float2(v.x * c - v.y * s, v.x * s + v.y * c);
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float4 position : SV_Position) : COLOR0
{
    // Calculate the base color for the screen.
    float4 baseColor = tex2D(baseTexture, coords);
    
    // Calculate the centered screen coords, ensuring that the results are unaffected by screen resolution.
    float2 centeredCoords = float2((coords.x - 0.5) * screenSize.x / screenSize.y + 0.5, coords.y);
    
    // Calculate how far away the given pixel is from the center of the suction effect.
    float distanceFromSource = distance(position.xy, suctionCenter);
    float2 suctionCenterUV = suctionCenter / zoomedScreenSize;
    
    // Use the aforementioned calculations to calculate an angle and coordinates for a dual-sampled noise value,
    // This spiral noise value will be used below to create the suction colors.
    float spiralAngle = distanceFromSource * 0.007 + globalTime * -0.36;
    float2 spiralCoords = RotatedBy(centeredCoords - suctionCenterUV, spiralAngle) + (1 - suctionCenterUV);
    float spiralNoise = (tex2D(spiralTexture, spiralCoords) + tex2D(spiralTexture, spiralCoords * 1.6)) * 0.5;
    
    // Calculate opacity, taking into account the suctionOpacity variable and how far away the given pixel is from the source of the suction effect.
    float opacityDistanceFadeoff = pow(smoothstep(suctionFadeOutRange, 0, distanceFromSource / zoom - suctionBaseRange), 2);
    float localOpacity = opacityDistanceFadeoff * suctionOpacity;
    
    // Combine everything together into a subtractive noise color.
    // This can cause black or red colors, depending a secondary noise value (whose coordinate are influenced by the original spiral coordinates to help give it a swirly feel).
    float spiralDarkness = smoothstep(0.25, 1, spiralNoise) * localOpacity;
    float redInfluence = smoothstep(0.7, 1, tex2D(spiralTexture, coords * 0.6 - spiralCoords * 0.1));
    float4 spiralColor = float4(redInfluence * 4, 0, 0, 1);
    float4 subtractiveNoise = (1 - spiralColor) * spiralDarkness;
    
    return baseColor - subtractiveNoise;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}