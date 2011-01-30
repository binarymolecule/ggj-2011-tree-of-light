// Effect dynamically changes color saturation.

sampler TextureSampler : register(s0);

texture Darkness;
sampler2D Extra = sampler_state {
    Texture = (Darkness);
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

float PI=3.14f;

float Percent;

float4 main(float4 color : COLOR0, float2 texCoord : TEXCOORD0) : COLOR0
{
    // Look up the texture color.
    float4 tex = tex2D(TextureSampler, texCoord);
		float4 tex2 = tex2D(Extra, texCoord);

		float s = sin(PI*Percent);
		float c = cos(PI*Percent);

		float2 coord = texCoord - (0.5f, 0.5f);

		float a = s * (coord.y) - c * (coord.x); 
		float b = s * (coord.y) + c * (coord.x); 

		if(((coord.x>0.0f)&&(a>0.0f))||((coord.x<0.0f)&&(b>0.0f)))
			tex.rgb = tex2.rgb;
    
    return tex;
}


technique Desaturate
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 main();
    }
}
