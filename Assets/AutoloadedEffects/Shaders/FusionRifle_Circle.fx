sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
float3 uColor;
float3 uSecondaryColor;
float uOpacity : register(C0);
float uSaturation;
float uCircularRotation;
float uRotation;
float uTime;
float4 uSourceRect;
float2 uWorldPosition;
float uDirection;
float3 uLightSource;
float2 uImageSize0;
float2 uImageSize1;
float2 overallImageSize;
matrix uWorldViewProjection;
float4 uShaderSpecificData;

bool TrimLeft;




float4 PixelFunction(float2 coords : TEXCOORD0) : COLOR0
{
    // Center the coordinates
    float2 centeredCoords = coords - 0.5;
    
    // Make the rotation matrix we learned in linear algebra
    float2x2 rotationMatrix = float2x2(cos(uCircularRotation), sin(uCircularRotation), -sin(uCircularRotation), cos(uCircularRotation));
    
    // Multiply the centered coordinates by the matrix to rotate the coordinates
    // We add 0.5 back so the coordinates go back to being 0 to 1 instead of -0.5 to 0.5
    // These are your new "default" coordinates,
    float2 rotatedCoords = mul(centeredCoords, rotationMatrix) + 0.5;
    float uCircularRotation = uTime;
    // This is basic texture sampling, anything from here can be done like a regular shader
    // You would also do your side trimming here, using the original coords instead of hte rotated ones
    float4 finalImage = tex2D(uImage0, rotatedCoords);
    
}
technique Technique1
{
    pass ShieldPass
    {
        //VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelFunction();
    }
}