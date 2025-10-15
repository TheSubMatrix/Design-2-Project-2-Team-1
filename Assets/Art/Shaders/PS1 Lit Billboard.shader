Shader "Custom/URP PS1 Lit Billboard" {
	Properties {
		[MainTexture] _BaseMap("Base Map (RGB) Smoothness / Alpha (A)", 2D) = "white" {}
		[MainColor]   _BaseColor("Base Color", Color) = (1, 1, 1, 1)

		// Billboard control properties
		_VerticalOffset("Vertical Offset", Float) = 0.0
		_HorizontalOffset("Horizontal Offset", Float) = 0.0
		_VerticalRotation("Vertical Rotation (Degrees)", Range(-180, 180)) = 0.0

		[Toggle(_NORMALMAP)] _NormalMapToggle ("Normal Mapping", Float) = 0
		[NoScaleOffset] _BumpMap("Normal Map", 2D) = "bump" {}

		[HDR] _EmissionColor("Emission Color", Color) = (0,0,0)
		[Toggle(_EMISSION)] _Emission ("Emission", Float) = 0
		[NoScaleOffset]_EmissionMap("Emission Map", 2D) = "white" {}

		[Toggle(_ALPHATEST_ON)] _AlphaTestToggle ("Alpha Clipping", Float) = 0
		_Cutoff ("Alpha Cutoff", Float) = 0.5

		[Toggle(_SPECGLOSSMAP)] _SpecGlossMapToggle ("Use Specular Gloss Map", Float) = 0
		_SpecColor("Specular Color", Color) = (0.5, 0.5, 0.5, 0.5)
		_SpecGlossMap("Specular Map", 2D) = "white" {}
		_Smoothness("Smoothness", Range(0.0, 1.0)) = 0.5
	}
	SubShader {
		Tags {
			"RenderPipeline"="UniversalPipeline"
			"RenderType"="Opaque"
			"Queue"="Geometry"
		}

		HLSLINCLUDE
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

		CBUFFER_START(UnityPerMaterial)
		float4 _BaseMap_ST;
		float4 _BaseColor;
		float4 _EmissionColor;
		float4 _SpecColor;
		float _Cutoff;
		float _Smoothness;
		float _VerticalOffset;
		float _HorizontalOffset;
		float _VerticalRotation;
		CBUFFER_END
		ENDHLSL

		Pass {
			Name "ForwardLit"
			Tags { "LightMode"="UniversalForward" }

			HLSLPROGRAM
			#pragma vertex LitPassVertex
			#pragma fragment LitPassFragment

			// Material Keywords
			#pragma shader_feature_local _NORMALMAP
			#pragma shader_feature_local_fragment _EMISSION
			#pragma shader_feature_local _RECEIVE_SHADOWS_OFF
			#pragma shader_feature_local_fragment _ALPHATEST_ON
			#pragma shader_feature_local_fragment _ALPHAPREMULTIPLY_ON
			#pragma shader_feature_local_fragment _ _SPECGLOSSMAP
			#define _SPECULAR_COLOR

			// URP Keywords
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
			#pragma multi_compile _ _SHADOWS_SOFT
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			#pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
			#pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
			#pragma multi_compile _ SHADOWS_SHADOWMASK

			// Unity Keywords
			#pragma multi_compile _ LIGHTMAP_ON
			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
			#pragma multi_compile_fog

			// Includes
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

			// Structs
			struct Attributes {
				float4 positionOS	: POSITION;
				float4 normalOS		: NORMAL;
				#ifdef _NORMALMAP
					float4 tangentOS 	: TANGENT;
				#endif
				float2 uv		    : TEXCOORD0;
				float2 lightmapUV	: TEXCOORD1;
				float4 color		: COLOR;
			};

			struct Varyings {
				float4 positionCS 					: SV_POSITION;
				float2 uv		    	: TEXCOORD0;
				DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 1);
				float3 positionWS					: TEXCOORD2;

				#ifdef _NORMALMAP
					half4 normalWS					: TEXCOORD3;
					half4 tangentWS					: TEXCOORD4;
					half4 bitangentWS				: TEXCOORD5;
				#else
					half3 normalWS					: TEXCOORD3;
				#endif
				
				#ifdef _ADDITIONAL_LIGHTS_VERTEX
					half4 fogFactorAndVertexLight	: TEXCOORD6;
				#else
					half  fogFactor					: TEXCOORD6;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
					float4 shadowCoord 				: TEXCOORD7;
				#endif

				float4 color						: COLOR;
			};

			// Textures, Samplers & Global Properties
			TEXTURE2D(_SpecGlossMap); 	SAMPLER(sampler_SpecGlossMap);

			// Helper function to create Y-axis rotation matrix
			float3x3 GetYRotationMatrix(float angleDegrees) {
				float angleRad = radians(angleDegrees);
				float c = cos(angleRad);
				float s = sin(angleRad);
				return float3x3(
					c, 0, s,
					0, 1, 0,
					-s, 0, c
				);
			}

			// Helper function to create billboard transformation
			float3x3 GetBillboardMatrix(float verticalRotation) {
				float3 originWS = TransformObjectToWorld(float3(0,0,0));
				originWS -= _WorldSpaceCameraPos;
				originWS.y = 0;
				float3 direction = normalize(TransformWorldToObjectDir(originWS));
				float3 topRow = normalize(cross(direction, float3(0,1,0)));
				float3 middleRow = normalize(cross(direction, topRow));
				float3x3 billboardMatrix = float3x3(topRow, middleRow, direction);
				
				// Apply vertical rotation
				if (abs(verticalRotation) > 0.001) {
					float3x3 rotationMatrix = GetYRotationMatrix(verticalRotation);
					billboardMatrix = mul(rotationMatrix, billboardMatrix);
				}
				
				return billboardMatrix;
			}

			// Functions
			half4 SampleSpecularSmoothness(float2 uv, half alpha, half4 specColor, TEXTURE2D_PARAM(specMap, sampler_specMap)) {
				half4 specularSmoothness = half4(0.0h, 0.0h, 0.0h, 1.0h);
				#ifdef _SPECGLOSSMAP
					specularSmoothness = SAMPLE_TEXTURE2D(specMap, sampler_specMap, uv) * specColor;
				#elif defined(_SPECULAR_COLOR)
					specularSmoothness = specColor;
				#endif
				return specularSmoothness;
			}

			//  SurfaceData & InputData
			void InitalizeSurfaceData(Varyings IN, out SurfaceData surfaceData){
				surfaceData = (SurfaceData)0;

				half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);

				#ifdef _ALPHATEST_ON
					clip(baseMap.a - _Cutoff);
				#endif

				half4 diffuse = baseMap * _BaseColor * IN.color;
				surfaceData.albedo = diffuse.rgb;
				surfaceData.normalTS = SampleNormal(IN.uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap));
				surfaceData.emission = SampleEmission(IN.uv, _EmissionColor.rgb, TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));
				surfaceData.occlusion = 1.0;

				half4 specular = SampleSpecularSmoothness(IN.uv, diffuse.a, _SpecColor, TEXTURE2D_ARGS(_SpecGlossMap, sampler_SpecGlossMap));
				#ifdef _SPECGLOSSMAP
				surfaceData.specular = specular.rgb;
				surfaceData.smoothness = specular.a;
				#else
				surfaceData.specular = _SpecColor;
				surfaceData.smoothness = _Smoothness;
				#endif
				
			}

			void InitializeInputData(Varyings input, half3 normalTS, out InputData inputData) {
				inputData = (InputData)0;

				inputData.positionWS = input.positionWS;

				#ifdef _NORMALMAP
					half3 viewDirWS = half3(input.normalWS.w, input.tangentWS.w, input.bitangentWS.w);
					inputData.normalWS = TransformTangentToWorld(normalTS,half3x3(input.tangentWS.xyz, input.bitangentWS.xyz, input.normalWS.xyz));
				#else
					half3 viewDirWS = GetWorldSpaceNormalizeViewDir(inputData.positionWS);
					inputData.normalWS = input.normalWS;
				#endif

				inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);

				viewDirWS = SafeNormalize(viewDirWS);
				inputData.viewDirectionWS = viewDirWS;

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
					inputData.shadowCoord = input.shadowCoord;
				#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
					inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
				#else
					inputData.shadowCoord = float4(0, 0, 0, 0);
				#endif
				
				#ifdef _ADDITIONAL_LIGHTS_VERTEX
					inputData.fogCoord = InitializeInputDataFog(float4(inputData.positionWS, 1.0), input.fogFactorAndVertexLight.x);
					inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
				#else
					inputData.fogCoord = InitializeInputDataFog(float4(inputData.positionWS, 1.0), input.fogFactor);
					inputData.vertexLighting = half3(0, 0, 0);
				#endif

				inputData.bakedGI = SAMPLE_GI(input.lightmapUV, input.vertexSH, inputData.normalWS);
				inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
				inputData.shadowMask = SAMPLE_SHADOWMASK(input.lightmapUV);
			}

			// Vertex Shader
			Varyings LitPassVertex(Attributes IN)
			{
				Varyings OUT;

				// Apply offsets first (in local space before billboard transformation)
				float3 offsetPosition = IN.positionOS.xyz;
				offsetPosition.x += _HorizontalOffset;
				offsetPosition.y += _VerticalOffset;
				
				// Get billboard transformation matrix and apply to offset position
				float3x3 billboardMatrix = GetBillboardMatrix(_VerticalRotation);
				float3 positionOS = mul(billboardMatrix, offsetPosition);
				
				#ifdef _NORMALMAP
					VertexNormalInputs normalInputs = GetVertexNormalInputs(mul(billboardMatrix, IN.normalOS.xyz), IN.tangentOS);
				#else
					VertexNormalInputs normalInputs = GetVertexNormalInputs(mul(billboardMatrix, IN.normalOS.xyz));
				#endif
				VertexPositionInputs positionInputs = GetVertexPositionInputs(positionOS);
				
				// Store the correct world position BEFORE quantization
				OUT.positionWS = positionInputs.positionWS;
				
				// Apply PS1-style vertex quantization to clip space position
				OUT.positionCS = float4(
					(round(positionInputs.positionCS.xy / positionInputs.positionCS.w * float2(480, 640)) / float2(480, 640) * positionInputs.positionCS.w),
					positionInputs.positionCS.z,
					positionInputs.positionCS.w
				);
				
				half3 viewDirWS = GetWorldSpaceViewDir(OUT.positionWS);
				half3 vertexLight = VertexLighting(OUT.positionWS, normalInputs.normalWS);
				half fogFactor = ComputeFogFactor(OUT.positionCS.z);
				
				#ifdef _NORMALMAP
					OUT.normalWS = half4(normalInputs.normalWS, viewDirWS.x);
					OUT.tangentWS = half4(normalInputs.tangentWS, viewDirWS.y);
					OUT.bitangentWS = half4(normalInputs.bitangentWS, viewDirWS.z);
				#else
					OUT.normalWS = NormalizeNormalPerVertex(normalInputs.normalWS);
				#endif

				OUTPUT_LIGHTMAP_UV(IN.lightmapUV, unity_LightmapST, OUT.lightmapUV);
				OUTPUT_SH(OUT.normalWS.xyz, OUT.vertexSH);

				#ifdef _ADDITIONAL_LIGHTS_VERTEX
					OUT.fogFactorAndVertexLight = half4(fogFactor, vertexLight);
				#else
					OUT.fogFactor = fogFactor;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
					OUT.shadowCoord = GetShadowCoord(positionInputs);
				#endif

				OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
				OUT.uv = 1 - OUT.uv;
				OUT.color = IN.color;
				return OUT;
			}

			// Fragment Shader
			half4 LitPassFragment(Varyings IN) : SV_Target {
				SurfaceData surfaceData;
				InitalizeSurfaceData(IN, surfaceData);

				InputData inputData;
				InitializeInputData(IN, surfaceData.normalTS, inputData);

				half4 color = UniversalFragmentBlinnPhong(inputData, surfaceData.albedo, half4(surfaceData.specular, 1), surfaceData.smoothness, surfaceData.emission, surfaceData.alpha, surfaceData.normalTS);

				color.rgb = MixFog(color.rgb, inputData.fogCoord);
				return color;
			}
			ENDHLSL
		}

		// ShadowCaster Pass
		Pass {
			Name "ShadowCaster"
			Tags { "LightMode"="ShadowCaster" }

			ZWrite On
			ZTest LEqual
			ColorMask 0
			Cull Back

			HLSLPROGRAM
			#pragma target 2.0
			#pragma vertex ShadowPassVertex 
			#pragma fragment ShadowPassFragment

			#pragma shader_feature_local_fragment _ALPHATEST_ON
			#pragma multi_compile_instancing
			#pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"

			float3 _LightDirection;
			float3 _LightPosition;
			float4 _ShadowBias;

			struct Attributes
			{
				float4 positionOS   : POSITION;
				float3 normalOS     : NORMAL;
				float2 texcoord     : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct Varyings
			{
				float4 positionCS   : SV_POSITION;
				float2 uv       : TEXCOORD0;
			};

			TEXTURE2D(_BaseMap);
			SAMPLER(sampler_BaseMap);

			float3x3 GetYRotationMatrix(float angleDegrees) {
				float angleRad = radians(angleDegrees);
				float c = cos(angleRad);
				float s = sin(angleRad);
				return float3x3(c, 0, s, 0, 1, 0, -s, 0, c);
			}

			float3x3 GetBillboardMatrix(float verticalRotation) {
				float3 originWS = TransformObjectToWorld(float3(0,0,0));
				originWS -= _WorldSpaceCameraPos;
				originWS.y = 0;
				float3 direction = normalize(TransformWorldToObjectDir(originWS));
				float3 topRow = normalize(cross(direction, float3(0,1,0)));
				float3 middleRow = normalize(cross(direction, topRow));
				float3x3 billboardMatrix = float3x3(topRow, middleRow, direction);
				
				if (abs(verticalRotation) > 0.001) {
					float3x3 rotationMatrix = GetYRotationMatrix(verticalRotation);
					billboardMatrix = mul(rotationMatrix, billboardMatrix);
				}
				
				return billboardMatrix;
			}

			float3 ApplyShadowBias(float3 positionWS, float3 normalWS, float3 lightDirection)
			{
				float invNdotL = 1.0 - saturate(dot(lightDirection, normalWS));
				float scale = invNdotL * _ShadowBias.y;
				positionWS = lightDirection * _ShadowBias.xxx + positionWS;
				positionWS = normalWS * scale.xxx + positionWS;
				return positionWS;
			}

			float4 GetShadowPositionHClip(Attributes input, float3x3 billboardMatrix)
			{
				// Apply offsets first (in local space before billboard transformation)
				float3 offsetPosition = input.positionOS.xyz;
				offsetPosition.x += _HorizontalOffset;
				offsetPosition.y += _VerticalOffset;
				
				float3 positionOS = mul(billboardMatrix, offsetPosition);
				
				float3 positionWS = TransformObjectToWorld(positionOS);
				float3 normalWS = TransformObjectToWorldNormal(mul(billboardMatrix, input.normalOS));

				#if _CASTING_PUNCTUAL_LIGHT_SHADOW
					float3 lightDirectionWS = normalize(_LightPosition - positionWS);
				#else
					float3 lightDirectionWS = _LightDirection;
				#endif

				float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));

				#if UNITY_REVERSED_Z
					positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
				#else
					positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
				#endif

				return positionCS;
			}
			
			Varyings ShadowPassVertex(Attributes input)
			{
				Varyings output;
				UNITY_SETUP_INSTANCE_ID(input);

				float3x3 billboardMatrix = GetBillboardMatrix(_VerticalRotation);
				output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
				output.uv = 1 - output.uv;
				output.positionCS = GetShadowPositionHClip(input, billboardMatrix);
				return output;
			}

			half4 ShadowPassFragment(Varyings input) : SV_TARGET
			{
				half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
				half alpha = baseMap.a * _BaseColor.a;
				clip(alpha - _Cutoff);
				return 0;
			}
			
			ENDHLSL
		}

		// DepthOnly Pass
		Pass {
			Name "DepthOnly"
			Tags { "LightMode"="DepthOnly" }

			ColorMask 0
			ZWrite On
			ZTest LEqual

			HLSLPROGRAM
			#pragma vertex DisplacedDepthOnlyVertex
			#pragma fragment DepthOnlyFragment

			#pragma shader_feature_local_fragment _ALPHATEST_ON
			#pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
			#pragma multi_compile_instancing

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
			
			float3x3 GetYRotationMatrix(float angleDegrees) {
				float angleRad = radians(angleDegrees);
				float c = cos(angleRad);
				float s = sin(angleRad);
				return float3x3(c, 0, s, 0, 1, 0, -s, 0, c);
			}

			float3x3 GetBillboardMatrix(float verticalRotation) {
				float3 originWS = TransformObjectToWorld(float3(0,0,0));
				originWS -= _WorldSpaceCameraPos;
				originWS.y = 0;
				float3 direction = normalize(TransformWorldToObjectDir(originWS));
				float3 topRow = normalize(cross(direction, float3(0,1,0)));
				float3 middleRow = normalize(cross(direction, topRow));
				float3x3 billboardMatrix = float3x3(topRow, middleRow, direction);
				
				if (abs(verticalRotation) > 0.001) {
					float3x3 rotationMatrix = GetYRotationMatrix(verticalRotation);
					billboardMatrix = mul(rotationMatrix, billboardMatrix);
				}
				
				return billboardMatrix;
			}
			
			Varyings DisplacedDepthOnlyVertex(Attributes input) {
				Varyings OUT;

				// Apply offsets first (in local space before billboard transformation)
				float3 offsetPosition = input.position.xyz;
				offsetPosition.x += _HorizontalOffset;
				offsetPosition.y += _VerticalOffset;
				
				float3x3 billboardMatrix = GetBillboardMatrix(_VerticalRotation);
				float3 positionOS = mul(billboardMatrix, offsetPosition);

				#if defined(_ALPHATEST_ON)
				OUT.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
				OUT.uv = 1 - OUT.uv;
				#endif
				
				OUT.positionCS = TransformObjectToHClip(positionOS.xyz);
				return OUT;
			}
			
			ENDHLSL
		}

		// DepthNormals Pass
		Pass {
			Name "DepthNormals"
			Tags { "LightMode"="DepthNormals" }

			ZWrite On
			ZTest LEqual

			HLSLPROGRAM
			#pragma vertex DisplacedDepthNormalsVertex
			#pragma fragment DepthNormalsFragment

			#pragma shader_feature_local _NORMALMAP
			#pragma shader_feature_local_fragment _ALPHATEST_ON
			#pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
			#pragma multi_compile_instancing

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Shaders/DepthNormalsPass.hlsl"

			float3x3 GetYRotationMatrix(float angleDegrees) {
				float angleRad = radians(angleDegrees);
				float c = cos(angleRad);
				float s = sin(angleRad);
				return float3x3(c, 0, s, 0, 1, 0, -s, 0, c);
			}

			float3x3 GetBillboardMatrix(float verticalRotation) {
				float3 originWS = TransformObjectToWorld(float3(0,0,0));
				originWS -= _WorldSpaceCameraPos;
				originWS.y = 0;
				float3 direction = normalize(TransformWorldToObjectDir(originWS));
				float3 topRow = normalize(cross(direction, float3(0,1,0)));
				float3 middleRow = normalize(cross(direction, topRow));
				float3x3 billboardMatrix = float3x3(topRow, middleRow, direction);
				
				if (abs(verticalRotation) > 0.001) {
					float3x3 rotationMatrix = GetYRotationMatrix(verticalRotation);
					billboardMatrix = mul(rotationMatrix, billboardMatrix);
				}
				
				return billboardMatrix;
			}

			Varyings DisplacedDepthNormalsVertex(Attributes input) {
				Varyings OUT;

				// Apply offsets first (in local space before billboard transformation)
				float3 offsetPosition = input.positionOS.xyz;
				offsetPosition.x += _HorizontalOffset;
				offsetPosition.y += _VerticalOffset;
				
				float3x3 billboardMatrix = GetBillboardMatrix(_VerticalRotation);
				float3 positionOS = mul(billboardMatrix, offsetPosition);

				#if defined(_ALPHATEST_ON)
				OUT.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
				OUT.uv = 1 - OUT.uv;
				#endif
				
				OUT.positionCS = TransformObjectToHClip(positionOS.xyz);
				OUT.normalWS = TransformObjectToWorldNormal(mul(billboardMatrix, input.normal));
				return OUT;
			}
			
			ENDHLSL
		}

	}
}