sampler baseTexture : register(s0);
sampler noiseTexture : register(s1);

float time;
float2 textureSize;

// Control parameters
float dropletFrequency = 5.0; // density of potential droplets across the bar
float dropletThreshold = 0.8; // noise threshold to spawn a droplet
float bleedSpeed = 0.2; // speed at which blood drips downward
float minDropletRadius = 0.01; // minimum radius of a droplet
float maxDropletRadius = 0.03; // maximum radius of a droplet

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 uv = coords;
    float2 pixelStep = 1.0 / textureSize;

// Generate a noise-based value to decide drop center and size
    float2 noiseUV = uv * dropletFrequency + float2(0, time * 0.1);
    float noiseVal = tex2D(noiseTexture, noiseUV).r;

// Determine if a droplet spawns here
    float spawn = step(dropletThreshold, noiseVal);

// Choose a random center along x based on noise
    float cx = frac(noiseVal * dropletFrequency + 0.23);
    float cy = frac(noiseVal * 0.73);
    float2 dropletCenter = float2(cx, cy);

// Randomize droplet radius
    float dropletRadius = lerp(minDropletRadius, maxDropletRadius, frac(noiseVal * 1.37));

// Current pixel distance to droplet center
    float dist0 = distance(uv, dropletCenter);
    float mask0 = smoothstep(dropletRadius, dropletRadius * 0.8, dist0);

// Compute bleeding: move center downward by bleedSpeed*time, wrap when off-screen
    float cyBleed = dropletCenter.y - frac(time * bleedSpeed);
    float2 bleedCenter = float2(dropletCenter.x, cyBleed);
    float dist1 = distance(uv, bleedCenter);
    float smearRadius = dropletRadius * 1.5;
    float mask1 = smoothstep(smearRadius, smearRadius * 0.8, dist1);

// Combine spawn, droplet, and smear
    float bloodMask = max(mask0, mask1) * spawn;

// Sample original scene with nearest-neighbor pixelation
    float4 orig = tex2D(baseTexture, round(uv * textureSize) / textureSize);

// Blood color (deep red) with alpha from mask
    float4 bloodColor = float4(0.6, 0.0, 0.0, bloodMask);

// Composite blood over scene using premultiplied alpha blend
    float4 final = orig * (1 - bloodMask) + bloodColor;

    return final * sampleColor;

}

technique Technique1
{
    pass BloodBleedPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}