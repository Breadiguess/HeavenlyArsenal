    sampler baseTexture : register(s0);
    sampler psychedelicTexture : register(s1);
    sampler noiseTexture : register(s2);

    float globalTime;
    float opacity;
    float intensityFactor;
    float psychedelicExponent;
    float colorAccentuationFactor;
    float3 colorToAccentuate;
    float4 goldColor;
    float4 psychedelicColorTint;

    float CalculateStarBrightness(float2 coords, float cutoffThreshold)
    {
        // Generate a random value from the input coordinates, and determine whether it can twinkle based on whether it exceeds a given inputted threshold.
        float randomValue = frac(sin(dot(coords + cutoffThreshold, float2(12.9898, 78.233))) * 97000);
        bool createTwinkle = randomValue >= cutoffThreshold;
    
        // Assuming it does exceed said threshold, SIGNIFICANTLY squash down the spectrum of values that can result in a twinkle, first applying an inverse lerp starting
        // at the threshold, and then applying a harsh exponentiation.
        float brightness = pow(smoothstep(cutoffThreshold, 1, randomValue), 17);
    
        // Multiply the brightness value by a shining twinkle factor, to make it give the twinkly look to the stars.
        // This varies strongly based on brightness and the cutoff threshold to ensure random twinkling timers for each star.
        float twinkleRate = 0.7;
        float twinkle = cos(globalTime * twinkleRate * 6.283 + randomValue * 400 + cutoffThreshold * 400) * 0.5 + 0.5;
        brightness *= twinkle;
    
        return brightness * createTwinkle;
    }

    float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
    {
        float2 originalCoords = coords;
    
        float intensity = opacity * intensityFactor;
    
        // Apply a general microdistortion to everything.
        float microdistortionTime = globalTime * 0.3;
        float warpNoise = tex2D(noiseTexture, coords * 1.3 + globalTime * 0.1) * 0.05;
        float microdistortionX = tex2D(noiseTexture, coords * 0.8 + microdistortionTime * float2(0.1, 0.1) + warpNoise);
        float microdistortionY = tex2D(noiseTexture, coords * 0.9 + microdistortionTime * float2(0.05, -0.1) - warpNoise);
        float2 microdistortion = float2(microdistortionX, microdistortionY) * intensity * 0.01;
        coords += microdistortion;
    
        // Distort the edges of the screen.
        float2 polar = float2(atan2(coords.y - 0.5, coords.x - 0.5), distance(coords, 0.5));
        float edgeDistortion = sin(polar.x * 2 + polar.y * 50 + globalTime * 0.6) * smoothstep(0.4, 0.5, polar.y) * 0.03;
        float2 edgeDistortedPolar = polar + float2(edgeDistortion * intensity, 0);
        float2 edgeDistortedCoords = 0.5 + float2(cos(edgeDistortedPolar.x), sin(edgeDistortedPolar.x)) * edgeDistortedPolar.y;
    
        // Calculate a psychedelic color based on polar coordinates.
        edgeDistortedPolar.x = edgeDistortedPolar.x / 6.283 + 0.5 - globalTime * 0.01;
        float edgePsychedelicInfluence = pow(smoothstep(0.1, 0.6, polar.y), psychedelicExponent) * intensity;
        float2 psychedelicCoords = (edgeDistortedPolar + float2(0, globalTime * -0.2)) * float2(2, 0.1);
        float4 edgeColor = (tex2D(psychedelicTexture, psychedelicCoords) + psychedelicColorTint) * edgePsychedelicInfluence * 0.5;
    
        // Calculate a twinkle value.
        float2 twinkleCoords = round(originalCoords * 1000) / 1000;
        float4 twinkleColor = CalculateStarBrightness(twinkleCoords, 0.95) * pow(edgePsychedelicInfluence, 4);
    
        float4 color = tex2D(baseTexture, edgeDistortedCoords);
    
        // Accentuate a target color.
        float3 distanceFromTargetColor = abs(color.rgb - colorToAccentuate);
        float closenessToTargetColor = 1 - dot(distanceFromTargetColor, 1);
        float targetColorAccentuation = pow(closenessToTargetColor, 3.5) * colorAccentuationFactor;
        color *= 1 + targetColorAccentuation;
    
        float2 blurOffset = 0.001;
        float4 glow = (tex2D(baseTexture, coords + float2(0, 1) * blurOffset) + tex2D(baseTexture, coords + float2(0, -1) * blurOffset) +
                       tex2D(baseTexture, coords + float2(1, 0) * blurOffset) + tex2D(baseTexture, coords + float2(-1, 0) * blurOffset)) * intensity * 0.2;
    
        float4 baseResult = color + edgeColor + twinkleColor + glow;
        float fadeToGold = lerp(0.1, 0.35, baseResult.r) + microdistortionX * 0.1;
        return lerp(baseResult, goldColor, intensity * fadeToGold);
    }

    technique Technique1
    {
        pass AutoloadPass
        {
            PixelShader = compile ps_3_0 PixelShaderFunction();
        }
    }