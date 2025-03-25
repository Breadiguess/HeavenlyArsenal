sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
sampler uImage2 : register(s2);



float time;
float brightness;
float spin;
matrix uWorldViewProjection;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float3 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float3 TextureCoordinates : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;
    
    // Transform the vertex position by the world-view-projection matrix
    float4 pos = mul(input.Position, uWorldViewProjection);
    output.Position = pos;
    
    // Pass the vertex color to the output
    output.Color = input.Color;
    
    // Pass the texture coordinates to the output
    output.TextureCoordinates = input.TextureCoordinates;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TextureCoordinates;
    
    // Apply smoothening to the visual by adjusting the y-coordinate
    coords.y = (coords.y - 0.5) / input.TextureCoordinates.z + 0.5;
    
    // Sample the first texture with adjusted coordinates
    float4 color = tex2D(uImage0, float2(coords.x - frac(time), coords.y - coords.x * spin));
    
    // Sample the second texture to create a glow effect
    float4 glow = tex2D(uImage1, float2(coords.x - frac(time * 2), coords.y - frac(time) * spin));
    
     // Sample the new texture for the tip region
    float4 tip = tex2D(uImage2, float2(coords.x, coords.y));

    
    
     // Blend the textures for the tip (e.g., use a linear interpolation based on the x-coordinate)
    float tipBlendFactor = smoothstep(0.8, 1.0, coords.x); // Blend at the tip region
    float4 blendedColor = lerp(color + glow, tip, tipBlendFactor);


    
    // Calculate the main color intensity using smoothstep and other factors
    float mainColor = smoothstep(0.1, 0.22, length(color.rgb) * pow((1 - coords.x), 4) * (1 - coords.x));
    
    // Calculate the glow color intensity
    float glowColor = pow(length(glow.rgb), 0.5 + coords.x * 3) * (1 - coords.x);
    
    // Combine the main color and glow color, apply a sine wave modulation, and adjust brightness
    return (pow((mainColor + glowColor), 2) + sin(coords.y * 3.14) * (1 - coords.x * 1.5)) * input.Color * brightness;
}
technique Technique1
{
    pass AutoloadPass
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
