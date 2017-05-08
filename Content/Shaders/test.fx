sampler TextureSampler : register(s0);

sampler DisplaceMap;

float maximum;
float time;

float4 PixelShaderFunction(float4 pos : SV_POSITION, float4 color1 : COLOR0, float2 texCoord: TEXCOORD0) : COLOR0
{
	float time_e = time * 0.001;

	float2 dmCoord_uv = float2(texCoord.x + time_e, texCoord.y + time_e);
	float4 displace = tex2D(DisplaceMap, dmCoord_uv);

	float displace_k = displace.g * maximum;
	float2 uv_diplaced = float2(texCoord.x + displace_k,
								texCoord.y + displace_k);

	return tex2D(TextureSampler, uv_diplaced);
}

technique BlackAndWhite
{
	pass Pass1
	{
		PixelShader = compile ps_4_0_level_9_3 PixelShaderFunction();
	}
}