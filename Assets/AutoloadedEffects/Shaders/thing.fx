
float2 Resolution;

// This is the “time” uniform (in seconds). tModLoader will update this each frame.
// You can adjust the speed by multiplying/divider if you want.
float uTime;

// The input texture (the current back buffer). If you want to blend your tunnel
// on top of an existing screen, you sample from this. If you only want the tunnel,
// you can ignore it or clear it to black first.
sampler2D inputTexture : register(s0);

// ------------------------------------------------------
// Helper: A simple pseudo‐random float between [0,1]. Not used here.
// float rand(float2 co) { return frac(sin(dot(co.xy, float2(12.9898,78.233))) * 43758.5453); }

// ------------------------------------------------------
// Our pixel shader entry point. We’ll render to the full screen.
// The "PS_INPUT" struct must have at least a TEXCOORD0 for screen‐space UV.
struct PS_INPUT
{
    float2 TexCoord0 : TEXCOORD0; // uv ∈ [0,1] over the screen.
};


float4 MainPS(PS_INPUT input) : SV_TARGET
{
    // 1) Re‐create the “u = (fragCoord - iResolution/2)/iResolution.y” part:
    //    fragCoord.x = input.TexCoord0.x * Resolution.x
    //    fragCoord.y = input.TexCoord0.y * Resolution.y
    //    So:
    float2 fragCoord = input.TexCoord0 * Resolution;
    float2 u = (fragCoord - (Resolution * 0.5)) / Resolution.y;

    // 2) Set up our running accumulators:
    //    - o: color accumulator (float4)
    //    - d: total distance marched
    //    - i: loop iterator counter
    //    - s: signed distance to tunnel
    //    - n: noise‐loop iterator
    //    - t: “very slow time” = Time * 0.05
    float4 o = float4(0, 0, 0, 0);
    float d = 0.0;
    float i = 0.0;
    float s = 0.0;
    float n = 0.0;
    float t = uTime * 0.05;

    // 3) The main raymarch loop: “for(o *= i; i++ < 1e2; ) { … }”
    //    We will do exactly 100 iterations (i goes from 0 -> 100).
    //    In each step, we:
    //      a) compute p = vec3(u * d, d + t*4)
    //      b) perturb p by “turbulence”
    //      c) compute signed‐distance “s” to inside‐of‐cylinder
    //      d) subtract layered sine‐noise for “cloud” look
    //      e) advance d by “0.02 + abs(s)*0.1”
    //      f) accumulate color o += 1 / s
    for (i = 0.0; i < 100.0; i += 1.0)
    {
        // a) p = vec3(u * d, d + t*4)
        float3 p;
        p.xy = u * d;
        p.z = d + t * 4.0;

        // b) “turbulence” step:
        //    p += cos(p.z + t + p.yzx * .5) * .5;
        // In HLSL: sin(), cos() and mul/swizzle all exist. p.yzx is simply p.y, p.z, p.x
        float3 swz = float3(p.y, p.z, p.x) * 0.5;
        float3 arg = float3(p.z + t, p.z + t, p.z + t) + swz;
        p += cos(arg) * 0.5;

        // c) signed‐distance to inside of a radius-5 cylinder:
        //    s = 5.0 - length(p.xy)
        s = 5.0 - length(p.xy);

        // d) subtract layered sine noise from ‘s’ to create “cloud‐like” edges
        n = 0.06;
        while (n < 2.0)
        {
            // rotate p.xy by a “pseudo‐mat2” trick:
            // GLSL: p.xy *= mat2(cos(t*.1 + vec4(0,33,11,0)));
            //
            // In GLSL that was shorthand: 
            //    float4 c = cos(t*0.1 + float4(0,33,11,0));
            //    mat2 M = [ c.x  c.y ]
            //             [ c.z  c.w ]  (column-major)
            //
            // In HLSL: we do the same by building a float2x2. 
            float4 c = cos(float4(t * 0.1 + 0.0,
                                  t * 0.1 + 33.0,
                                  t * 0.1 + 11.0,
                                  t * 0.1 + 0.0));
            // HLSL’s float2x2 is row‐major by default, but mul(p.xy, M) 
            // expects column‐major layout. So we build M so that
            // p.xy = mul(p.xy, float2x2( c.x, c.z,   c.y, c.w ));
            float2x2 rot = float2x2(c.x, c.z,
                                     c.y, c.w);
            p.xy = mul(p.xy, rot);

            // Now subtract noise: 
            //    s -= abs(dot(sin(p.z+t + p * n * 20.), vec3(.05))) / n;
            float3 sinArg = float3(p.z + t, p.z + t, p.z + t)
                            + (p * n * 20.0);
            float noise = abs(dot(sin(sinArg), float3(0.05, 0.05, 0.05))) / n;
            s -= noise;

            // advance n:
            n += n; // same as n *= 2
        }

        // e) advance “d”:  
        //    d += sStep = 0.02 + abs(s)*0.1;
        float sStep = 0.02 + abs(s) * 0.1;
        d += sStep;

        // f) accumulate color:  o += 1.0 / sStep
        //    (They used “o += 1. / s;” in GLSL, but note they wrote d += s = .02 + abs(s)*.1;
        //     so sStep = that same value.)
        o += float4(1.0, 1.0, 1.0, 1.0) * (1.0 / sStep);
    }

    // 4) After raymarching, we do tone‐mapping in GLSL:
    //    o = tanh( o / d / 9e2 / length(u) );
    // 
    // In HLSL: tanh() is available. length(u) gives same behavior.
    float lum = length(u);
    // Avoid divide by zero if the pixel is exactly at center (u=0). 
    // We can safely clamp lum to something small:
    lum = max(lum, 0.0001);
    float4 color = tanh((o / d / 900.0) / lum);

    // If you want to overlay on top of the existing backbuffer:
    // float4 prev = tex2D(inputTexture, input.TexCoord0);
    // return lerp(prev, color, color.a);
    //
    // But if you just want the tunnel alone, return color directly:
    return color;
}

// We need to tell the FX framework which function is our pixel‐shader entry.
// In tModLoader’s convention, we declare a “technique” that uses MainPS.
technique Technique1
{
    pass P0
    {
        PixelShader = compile ps_3_0 MainPS();
    }
}
