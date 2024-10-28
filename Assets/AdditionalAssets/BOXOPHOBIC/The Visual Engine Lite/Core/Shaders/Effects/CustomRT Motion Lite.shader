// Made with Amplify Shader Editor v1.9.6.3
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "BOXOPHOBIC/The Visual Engine Lite/Effects/CustomRT Motion Lite"
{
    Properties
    {
		[StyledBanner(CustomRT Motion)]_BANNER("[ BANNER ]", Float) = 0
		[NoScaleOffset][StyledTextureSingleLine]_MotionNoiseTex("Motion Texture", 2D) = "white" {}
		[Space(10)]_MotionSpeedMinValue("Motion Wind Speed Min", Range( 0 , 40)) = 4
		_MotionSpeedMaxValue("Motion Wind Speed Max", Range( 0 , 40)) = 8
		_MotionTinySpeedMinValue("Motion Flutter Speed Min", Range( 0 , 1)) = 0.1
		_MotionTinySpeedMaxValue("Motion Flutter Speed Max", Range( 0 , 1)) = 0.3

    }

	SubShader
	{
		LOD 0

		
		CGINCLUDE
		#pragma target 3.0
		ENDCG
		Blend Off
		AlphaToMask Off
		Cull Back
		ColorMask RGBA
		ZWrite On
		ZTest LEqual
		Offset 0 , 0
		
		
        Pass
        {
			Name "Custom RT Update"
            CGPROGRAM
            #define ASE_USING_SAMPLING_MACROS 1

            #include "UnityCustomRenderTexture.cginc"
            #pragma vertex ASECustomRenderTextureVertexShader
            #pragma fragment frag
            #pragma target 3.0
			#include "UnityShaderVariables.cginc"
			#if defined(SHADER_API_D3D11) || defined(SHADER_API_XBOXONE) || defined(UNITY_COMPILER_HLSLCC) || defined(SHADER_API_PSSL) || (defined(SHADER_TARGET_SURFACE_ANALYSIS) && !defined(SHADER_TARGET_SURFACE_ANALYSIS_MOJOSHADER))//ASE Sampler Macros
			#define SAMPLE_TEXTURE2D(tex,samplerTex,coord) tex.Sample(samplerTex,coord)
			#else//ASE Sampling Macros
			#define SAMPLE_TEXTURE2D(tex,samplerTex,coord) tex2D(tex,coord)
			#endif//ASE Sampling Macros
			


			struct ase_appdata_customrendertexture
			{
				uint vertexID : SV_VertexID;
				
			};

			struct ase_v2f_customrendertexture
			{
				float4 vertex           : SV_POSITION;
				float3 localTexcoord    : TEXCOORD0;    // Texcoord local to the update zone (== globalTexcoord if no partial update zone is specified)
				float3 globalTexcoord   : TEXCOORD1;    // Texcoord relative to the complete custom texture
				uint primitiveID        : TEXCOORD2;    // Index of the update zone (correspond to the index in the updateZones of the Custom Texture)
				float3 direction        : TEXCOORD3;    // For cube textures, direction of the pixel being rendered in the cubemap
				
			};

			uniform float _BANNER;
			UNITY_DECLARE_TEX2D_NOSAMPLER(_MotionNoiseTex);
			uniform half4 TVE_MotionParams;
			uniform half TVE_IsEnabled;
			uniform half4 TVE_TimeParams;
			uniform half _MotionSpeedMinValue;
			SamplerState sampler_Linear_Repeat;
			uniform half _MotionSpeedMaxValue;
			uniform float _MotionTinySpeedMinValue;
			uniform float _MotionTinySpeedMaxValue;


			ase_v2f_customrendertexture ASECustomRenderTextureVertexShader(ase_appdata_customrendertexture IN  )
			{
				ase_v2f_customrendertexture OUT;
				
			#if UNITY_UV_STARTS_AT_TOP
				const float2 vertexPositions[6] =
				{
					{ -1.0f,  1.0f },
					{ -1.0f, -1.0f },
					{  1.0f, -1.0f },
					{  1.0f,  1.0f },
					{ -1.0f,  1.0f },
					{  1.0f, -1.0f }
				};

				const float2 texCoords[6] =
				{
					{ 0.0f, 0.0f },
					{ 0.0f, 1.0f },
					{ 1.0f, 1.0f },
					{ 1.0f, 0.0f },
					{ 0.0f, 0.0f },
					{ 1.0f, 1.0f }
				};
			#else
				const float2 vertexPositions[6] =
				{
					{  1.0f,  1.0f },
					{ -1.0f, -1.0f },
					{ -1.0f,  1.0f },
					{ -1.0f, -1.0f },
					{  1.0f,  1.0f },
					{  1.0f, -1.0f }
				};

				const float2 texCoords[6] =
				{
					{ 1.0f, 1.0f },
					{ 0.0f, 0.0f },
					{ 0.0f, 1.0f },
					{ 0.0f, 0.0f },
					{ 1.0f, 1.0f },
					{ 1.0f, 0.0f }
				};
			#endif

				uint primitiveID = IN.vertexID / 6;
				uint vertexID = IN.vertexID % 6;
				float3 updateZoneCenter = CustomRenderTextureCenters[primitiveID].xyz;
				float3 updateZoneSize = CustomRenderTextureSizesAndRotations[primitiveID].xyz;
				float rotation = CustomRenderTextureSizesAndRotations[primitiveID].w * UNITY_PI / 180.0f;

			#if !UNITY_UV_STARTS_AT_TOP
				rotation = -rotation;
			#endif

				// Normalize rect if needed
				if (CustomRenderTextureUpdateSpace > 0.0) // Pixel space
				{
					// Normalize xy because we need it in clip space.
					updateZoneCenter.xy /= _CustomRenderTextureInfo.xy;
					updateZoneSize.xy /= _CustomRenderTextureInfo.xy;
				}
				else // normalized space
				{
					// Un-normalize depth because we need actual slice index for culling
					updateZoneCenter.z *= _CustomRenderTextureInfo.z;
					updateZoneSize.z *= _CustomRenderTextureInfo.z;
				}

				// Compute rotation

				// Compute quad vertex position
				float2 clipSpaceCenter = updateZoneCenter.xy * 2.0 - 1.0;
				float2 pos = vertexPositions[vertexID] * updateZoneSize.xy;
				pos = CustomRenderTextureRotate2D(pos, rotation);
				pos.x += clipSpaceCenter.x;
			#if UNITY_UV_STARTS_AT_TOP
				pos.y += clipSpaceCenter.y;
			#else
				pos.y -= clipSpaceCenter.y;
			#endif

				// For 3D texture, cull quads outside of the update zone
				// This is neeeded in additional to the preliminary minSlice/maxSlice done on the CPU because update zones can be disjointed.
				// ie: slices [1..5] and [10..15] for two differents zones so we need to cull out slices 0 and [6..9]
				if (CustomRenderTextureIs3D > 0.0)
				{
					int minSlice = (int)(updateZoneCenter.z - updateZoneSize.z * 0.5);
					int maxSlice = minSlice + (int)updateZoneSize.z;
					if (_CustomRenderTexture3DSlice < minSlice || _CustomRenderTexture3DSlice >= maxSlice)
					{
						pos.xy = float2(1000.0, 1000.0); // Vertex outside of ncs
					}
				}

				OUT.vertex = float4(pos, 0.0, 1.0);
				OUT.primitiveID = asuint(CustomRenderTexturePrimitiveIDs[primitiveID]);
				OUT.localTexcoord = float3(texCoords[vertexID], CustomRenderTexture3DTexcoordW);
				OUT.globalTexcoord = float3(pos.xy * 0.5 + 0.5, CustomRenderTexture3DTexcoordW);
			#if UNITY_UV_STARTS_AT_TOP
				OUT.globalTexcoord.y = 1.0 - OUT.globalTexcoord.y;
			#endif
				OUT.direction = CustomRenderTextureComputeCubeDirection(OUT.globalTexcoord.xy);

				return OUT;
			}

            float4 frag(ase_v2f_customrendertexture IN ) : COLOR
            {
				float4 finalColor;
				float2 temp_output_3_0_g76794 = IN.localTexcoord.xy;
				float4 lerpResult136_g76785 = lerp( half4(0,1,1,0) , TVE_MotionParams , TVE_IsEnabled);
				half2 Global_WindDirection141_g76785 = (lerpResult136_g76785).xy;
				float2 temp_output_21_0_g76794 = (Global_WindDirection141_g76785*2.0 + -1.0);
				float lerpResult128_g76793 = lerp( _Time.y , ( ( _Time.y * TVE_TimeParams.x ) + TVE_TimeParams.y ) , TVE_TimeParams.w);
				half Noise_SpeedA144_g76785 = ( lerpResult128_g76793 * 0.02 * _MotionSpeedMinValue );
				float temp_output_15_0_g76794 = Noise_SpeedA144_g76785;
				float temp_output_23_0_g76794 = frac( temp_output_15_0_g76794 );
				float4 lerpResult39_g76794 = lerp( SAMPLE_TEXTURE2D( _MotionNoiseTex, sampler_Linear_Repeat, ( temp_output_3_0_g76794 + ( temp_output_21_0_g76794 * temp_output_23_0_g76794 ) ) ) , SAMPLE_TEXTURE2D( _MotionNoiseTex, sampler_Linear_Repeat, ( temp_output_3_0_g76794 + ( temp_output_21_0_g76794 * frac( ( temp_output_15_0_g76794 + 0.5 ) ) ) ) ) , ( abs( ( temp_output_23_0_g76794 - 0.5 ) ) / 0.5 ));
				half4 Noise_TexA152_g76785 = lerpResult39_g76794;
				float2 temp_output_3_0_g76797 = IN.localTexcoord.xy;
				float2 temp_output_21_0_g76797 = (Global_WindDirection141_g76785*2.0 + -1.0);
				float lerpResult128_g76796 = lerp( _Time.y , ( ( _Time.y * TVE_TimeParams.x ) + TVE_TimeParams.y ) , TVE_TimeParams.w);
				half Noise_SpeedB244_g76785 = ( lerpResult128_g76796 * 0.02 * _MotionSpeedMaxValue );
				float temp_output_15_0_g76797 = Noise_SpeedB244_g76785;
				float temp_output_23_0_g76797 = frac( temp_output_15_0_g76797 );
				float4 lerpResult39_g76797 = lerp( SAMPLE_TEXTURE2D( _MotionNoiseTex, sampler_Linear_Repeat, ( temp_output_3_0_g76797 + ( temp_output_21_0_g76797 * temp_output_23_0_g76797 ) ) ) , SAMPLE_TEXTURE2D( _MotionNoiseTex, sampler_Linear_Repeat, ( temp_output_3_0_g76797 + ( temp_output_21_0_g76797 * frac( ( temp_output_15_0_g76797 + 0.5 ) ) ) ) ) , ( abs( ( temp_output_23_0_g76797 - 0.5 ) ) / 0.5 ));
				half4 Noise_TexB245_g76785 = lerpResult39_g76797;
				half Global_WindIntensity153_g76785 = (lerpResult136_g76785).z;
				float2 lerpResult248_g76785 = lerp( (Noise_TexA152_g76785).rg , (Noise_TexB245_g76785).rg , Global_WindIntensity153_g76785);
				float2 lerpResult166_g76785 = lerp( lerpResult248_g76785 , Global_WindDirection141_g76785 , Global_WindIntensity153_g76785);
				float2 lerpResult172_g76785 = lerp( float2( 0.5,0.5 ) , lerpResult166_g76785 , Global_WindIntensity153_g76785);
				half2 Wind_Direction192_g76785 = lerpResult172_g76785;
				float lerpResult254_g76785 = lerp( (Noise_TexA152_g76785).b , (Noise_TexB245_g76785).b , Global_WindIntensity153_g76785);
				half Wind_Noise256_g76785 = lerpResult254_g76785;
				float lerpResult128_g76799 = lerp( _Time.y , ( ( _Time.y * TVE_TimeParams.x ) + TVE_TimeParams.y ) , TVE_TimeParams.w);
				float2 appendResult202_g76785 = (float2(0.0 , _MotionTinySpeedMinValue));
				float2 panner164_g76785 = ( lerpResult128_g76799 * appendResult202_g76785 + IN.localTexcoord.xy);
				float lerpResult128_g76801 = lerp( _Time.y , ( ( _Time.y * TVE_TimeParams.x ) + TVE_TimeParams.y ) , TVE_TimeParams.w);
				float2 appendResult211_g76785 = (float2(0.0 , _MotionTinySpeedMaxValue));
				float2 panner209_g76785 = ( lerpResult128_g76801 * appendResult211_g76785 + IN.localTexcoord.xy);
				float lerpResult128_g76790 = lerp( _Time.y , ( ( _Time.y * TVE_TimeParams.x ) + TVE_TimeParams.y ) , TVE_TimeParams.w);
				float temp_output_171_0_g76785 = sin( ( lerpResult128_g76790 * 1.0 ) );
				float lerpResult174_g76785 = lerp( SAMPLE_TEXTURE2D( _MotionNoiseTex, sampler_Linear_Repeat, panner164_g76785 ).r , SAMPLE_TEXTURE2D( _MotionNoiseTex, sampler_Linear_Repeat, panner209_g76785 ).g , (temp_output_171_0_g76785*0.5 + 0.5));
				half Wind_Flutter194_g76785 = lerpResult174_g76785;
				float4 appendResult188_g76785 = (float4(Wind_Direction192_g76785 , Wind_Noise256_g76785 , Wind_Flutter194_g76785));
				
                finalColor = appendResult188_g76785;
				return finalColor;
            }
            ENDCG
		}
    }
	
	
	Fallback Off
}
/*ASEBEGIN
Version=19603
Node;AmplifyShaderEditor.FunctionNode;299;2176,2816;Inherit;False;Compute Global Wind;0;;76785;f4994c96c48c75e4ab5e1dd842e8376b;0;0;1;FLOAT4;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;2560,2816;Float;False;True;-1;2;;0;2;BOXOPHOBIC/The Visual Engine Lite/Effects/CustomRT Motion Lite;32120270d1b3a8746af2aca8bc749736;True;Custom RT Update;0;0;Custom RT Update;1;False;True;0;1;False;;0;False;;0;1;False;;0;False;;True;0;False;;0;False;;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;0;True;2;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;0;;0;0;Standard;0;0;1;True;False;;True;0
WireConnection;0;0;299;0
ASEEND*/
//CHKSM=020887F74E5063663524D509C4154A4BF95F60C8