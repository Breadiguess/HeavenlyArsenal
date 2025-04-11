sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
float3 uColor;
float2 uNoiseOffset;
float2 uOffset;
float uProgress;
float uProgressInside;
float uNoiseStrength;
bool useDissolve;

float4 PixelFunction(float2 coords : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
    float2 centered = coords * 2 - 1;
    float4 noise = tex2D(uImage0, coords + uNoiseOffset);
    float4 noise2 = tex2D(uImage1, coords + uNoiseOffset);
    
    float distance = length(centered);
    float distanceShifted = length(centered + uOffset);
    float radial = smoothstep(0.9, 0.89, distance + length(noise.rgb) * 0.1 * uNoiseStrength) * smoothstep(0.02, 0.03, distanceShifted + length(noise.rgb) * 0.2 * uNoiseStrength - sqrt(uProgressInside));
    color.rgb *= (noise2.rgb * 0.2 + 0.8);
    
    if (useDissolve)
    {
        return radial * color * smoothstep(0, 0.04, length(noise2.rgb) - uProgressInside);
    }
    
    return radial * color;
}
technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_2_0 PixelFunction();
    }
}