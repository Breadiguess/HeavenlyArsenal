sampler baseTexture : register(s0);
sampler blackHoleTargetTexture : register(s1);
sampler noiseTexture : register(s2);

float globalTime;
float sourceRadii[5];
float2 sourcePositions[5];
float2 aspectRatioCorrectionFactor;
float2 zoom;
float3 burnColor;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float shredBurn = 0;
    float disappearance = 0;
    float2 distortedCoords = coords;
    for (int i = 0; i < 5; i++)
    {
        float radius = sourceRadii[i];
        float distanceFromDistortion = distance((distortedCoords - 0.5) * aspectRatioCorrectionFactor + 0.5, sourcePositions[i]);
        float localDistortionIntensity = smoothstep(radius, 0, distanceFromDistortion);
        
        // Apply the distortion.
        distortedCoords = lerp(distortedCoords, sourcePositions[i], -localDistortionIntensity * 0.1);
        
        // Calculate the influence of redshift and disappearance, in accordance with real-world black hole effects.
        shredBurn += smoothstep(4, 2, distanceFromDistortion / radius);
        disappearance += smoothstep(1.5, 0.75, distanceFromDistortion / radius);
    }
    
    float burnNoise = 0;
    float2 polar = float2(atan2(coords.y - 0.5, coords.x - 0.5) / 6.283 + 0.5, distance(coords, 0.5));
    for (i = 1; i < 3; i++)
    {
        burnNoise = tex2D(noiseTexture, polar * (i + 1) + burnNoise * 0.12);
    }
    
    burnNoise = cos(burnNoise * 9.3 - globalTime * 6.7) * 0.5 + 0.5;
    float burnGlow = 0.085 / distance(burnNoise, 0.6) * smoothstep(0.35, 0.6, burnNoise);
    float4 distortedColor = tex2D(baseTexture, distortedCoords);
    float4 burnColorInfluence = float4(burnColor, 1) * burnGlow * smoothstep(0.3, 1, shredBurn) * distortedColor.a;
    return (distortedColor + float4(0.7, -0.5, -0.5, 0) * distortedColor.a * shredBurn) * saturate(1 - disappearance) + burnColorInfluence;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}