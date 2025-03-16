sampler bloodBlobTexture : register(s0);
sampler accentNoise : register(s1);

float dissolveThreshold;
float localTime;
float4 accentColor;
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
    float4 pos = mul(input.Position, uWorldViewProjection);
    output.Position = pos;
    
    output.Color = input.Color;
    output.TextureCoordinates = input.TextureCoordinates;

    return output;
}

float QuadraticBump(float x)
{
    return x * (4 - x * 4);
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float4 color = input.Color;
    float2 coords = input.TextureCoordinates;
    
    // Apply smoothening to the visual.
    coords.y = (coords.y - 0.5) / input.TextureCoordinates.z + 0.5;

    // Calculate the horizontal bump in advance.
    float horizontalBump = QuadraticBump(coords.y);
    
    // Calculate the general-purpose scroll value.
    // This simultaneously flows backwards perpetually and has an in-built wavy horizontal motion to it, to make the effect a bit more dynamic.
    float2 scroll = float2(-localTime * 0.81, sin(coords.x * -12 + localTime * -3) * (1 - coords.x) * 0.19);
    
    // Determine how much the color should be dissolved by noise.
    // The threshold for this becomes more lenient at the back of the blob, making it naturally fade away at the tail.
    float dissolveValue = tex2D(accentNoise, (coords + scroll) * 0.5) - pow(coords.x, 2) * 1.8 - (1 - horizontalBump) * 0.2;
    float dissolveOpacity = smoothstep(-0.1, 0, dissolveValue - dissolveThreshold);
    
    // Accent the colors a bit based on noise.
    color = saturate(color + accentColor * tex2D(bloodBlobTexture, coords + scroll).r);
    
    // Darken near the dissolve thresholds.
    float darkening = smoothstep(0, 0.2, dissolveValue - dissolveThreshold);
    
    // Dark near the edges.
    darkening -= smoothstep(1, 0, QuadraticBump(coords.y));
    
    color.rgb *= lerp(0.4, 1, saturate(darkening));
    
    float edgeOpacity = smoothstep(0, 0.9, horizontalBump);
    float frontOpacity = smoothstep(0.1, 0.35, coords.x);
    
    // Make the inner horizontal part of the texture a bit brigther towards red.
    color = saturate(color + smoothstep(0.85, 1, horizontalBump) * float4(0.85, 0.13, 0, 0) * coords.x);
    
    return color * dissolveOpacity * edgeOpacity * frontOpacity;
}

technique Technique1
{
    pass AutoloadPass
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
