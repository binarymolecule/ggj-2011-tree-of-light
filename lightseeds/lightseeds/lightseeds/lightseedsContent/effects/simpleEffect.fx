
// Input parameters.
float2   ViewportSize    : register(c0);
float2   TextureSize     : register(c1);
float4x4 MatrixTransform : register(c2);
sampler  TextureSampler  : register(s0);


// Vertex shader for rendering sprites on Windows.
void VertexShaderFunction(inout float4 position : POSITION0,
		  				inout float4 color    : COLOR0,
						inout float2 texCoord : TEXCOORD0)
{
    // Apply the matrix transform.
    position = mul(position, transpose(MatrixTransform));
	texCoord = position;
}


// Pixel shader for rendering sprites (shared between Windows and Xbox).
void PixelShaderFunction(inout float4 color : COLOR0, float2 texCoord : TEXCOORD0)
{
    //color *= tex2D(TextureSampler, texCoord);
	color = float4(1,0,0,1);
}


technique Technique1
{
    pass Pass1
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
