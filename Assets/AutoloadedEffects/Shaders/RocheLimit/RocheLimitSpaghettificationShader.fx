sampler baseTexture : register(s0);
sampler blackHoleTargetTexture : register(s1);

float globalTime;
float sourceRadii[5];
float2 sourcePositions[5];
float2 aspectRatioCorrectionFactor;
float2 zoom;

float2 ModifyForZoom(float2 coords)
{
    return (coords - 0.5) * zoom + 0.5;
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float lensingAngles[5];
    float redshift = 0;
    float disappearance = 0;
    float2 distortedCoords = coords;
    for (int i = 0; i < 5; i++)
    {
        float radius = sourceRadii[i];
        float distanceFromDistortion = max(distance((distortedCoords - 0.5) * aspectRatioCorrectionFactor + 0.5, sourcePositions[i]), 0);
        float localDistortionIntensity = smoothstep(radius, 0, distanceFromDistortion);
        
        // Apply the distortion.
        distortedCoords = lerp(distortedCoords, sourcePositions[i], -localDistortionIntensity * 0.1);
        
        // Calculate the influence of redshift and disappearance, in accordance with real-world black hole effects.
        redshift += smoothstep(3.2, 2, distanceFromDistortion / radius);
        disappearance += smoothstep(1.5, 0.75, distanceFromDistortion / radius);
    }
    
    float4 distortedColor = tex2D(baseTexture, distortedCoords);
    return (distortedColor + float4(0.7, -0.5, -0.5, 0) * distortedColor.a * redshift) * saturate(1 - disappearance);
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}