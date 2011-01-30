float fade = 1.0f;

sampler texSampler;

float4 PS_TITLE_SCREEN(float2 texCoord : TEXCOORD0) : COLOR0
{		
    float4 color = tex2D(texSampler, texCoord);
	return fade * color;
}

technique TechniqueTitle
{
    pass Pass0
    {     
        PixelShader = compile ps_2_0 PS_TITLE_SCREEN();
    }
}
