// === Inputs ===
// s0: Shape A (grayscale mask; use alpha or luminance)
// s1: Noise texture (tileable noise)
// s2: Shape B (grayscale mask)
// s3: Shape C (grayscale mask)

sampler2D ShapeA : register(s0);
sampler2D NoiseTexture : register(s1);
sampler2D ShapeB : register(s2);
sampler2D ShapeC : register(s3);

float4 Color; 
float Time; // Seconds; drive from C# via Shader.Parameters["Time"]

float MorphSpeed = 0.25; 
float EdgeWidth = 0.08; 
float Threshold = 0.5; 
float2 NoiseScale = float2(1.0, 1.0); // UV scale for noise sampling
float WarpStrength = 0.03; // how much noise warps the UVs
float NoiseSpeed = 0.15; // how fast noise scrolls

float luminance(float3 c)
{
    return dot(c, float3(0.299, 0.587, 0.114));
}

// Sample a mask (prefers alpha; falls back to luminance)
float sampleMask(sampler2D s, float2 uv)
{
    float4 c = tex2D(s, uv);
    float a = c.a;
    // In case textures don’t have alpha, derive from RGB:
    return (a > 0.001) ? a : luminance(c.rgb);
}

float3 triWeights(float p)
{
    float x = p * 3.0; // [0,3)
    float wA = saturate(1.0 - abs(x - 0.0));
    float wB = saturate(1.0 - abs(x - 1.0));
    float wC = saturate(1.0 - abs(x - 2.0));
    float sum = max(wA + wB + wC, 1e-4);
    return float3(wA, wB, wC) / sum;
}

float4 PixelShaderFunction(float2 uv : TEXCOORD0) : COLOR0
{
    float2 nUV = uv * NoiseScale + float2(Time * NoiseSpeed, -Time * NoiseSpeed);
    float n = tex2D(NoiseTexture, nUV).r; 
    float2 warp = (n - 0.5) * WarpStrength; // center on 0
    float2 wuv = uv + warp;

    
    float phase = frac(Time * MorphSpeed); // [0,1)
    float3 w = triWeights(phase); // weights for A,B, C
    
    float aMask = sampleMask(ShapeA, wuv);
    float bMask = sampleMask(ShapeB, wuv);
    float cMask = sampleMask(ShapeC, wuv);
    //this sucks ass but im also only just starting out so i don't feel awful
    float mask = dot(float3(aMask, bMask, cMask), w);
    
    // Think of Threshold as the isosurface; EdgeWidth controls falloff.
    float alpha = smoothstep(Threshold - EdgeWidth, Threshold + EdgeWidth, mask);
    
    float shade = mask; 

    return float4(Color.rgb * shade, Color.a * alpha);
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}

