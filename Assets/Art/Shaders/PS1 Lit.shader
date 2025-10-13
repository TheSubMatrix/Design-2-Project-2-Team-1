Shader "Custom/URP PS1 Lit Tessellation" {
	Properties {
		[MainTexture] _BaseMap("Base Map (RGB) Smoothness / Alpha (A)", 2D) = "white" {}
		[MainColor]   _BaseColor("Base Color", Color) = (1, 1, 1, 1)

		[Toggle(_NORMALMAP)] _NormalMapToggle ("Normal Mapping", Float) = 0
		[NoScaleOffset] _BumpMap("Normal Map", 2D) = "bump" {}

		[HDR] _EmissionColor("Emission Color", Color) = (0,0,0)
		[Toggle(_EMISSION)] _Emission ("Emission", Float) = 0
		[NoScaleOffset]_EmissionMap("Emission Map", 2D) = "white" {}

		[Toggle(_ALPHATEST_ON)] _AlphaTestToggle ("Alpha Clipping", Float) = 0
		_Cutoff ("Alpha Cutoff", Float) = 0.5

		[Toggle(_SURFACE_TYPE_TRANSPARENT)] _SurfaceType ("Transparent", Float) = 0
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 5
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Float) = 10
		[Enum(Off, 0, On, 1)] _ZWrite ("Z Write", Float) = 1

		[Toggle(_SPECGLOSSMAP)] _SpecGlossMapToggle ("Use Specular Gloss Map", Float) = 0
		_SpecColor("Specular Color", Color) = (0.5, 0.5, 0.5, 0.5)
		_SpecGlossMap("Specular Map", 2D) = "white" {}
		_Smoothness("Smoothness", Range(0.0, 1.0)) = 0.5

		_TessellationFactor("Tessellation Factor", Range(1, 64)) = 4
		_TessellationMaxDistance("Max Tessellation Distance", Float) = 50
		[Toggle(_AFFINE_TEXTURE_MAPPING)] _AffineTextureMappingToggle ("Affine Texture Mapping", Float) = 1
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
		float _TessellationFactor;
		float _TessellationMaxDistance;
		float _CustomShadowBias;
		float _CustomShadowNormalBias;
		CBUFFER_END

		// Shared vertex snapping function
		float4 SnapToPixelGrid(float4 positionCS) {
			float4 snappedPos = positionCS;
			snappedPos.xy = positionCS.xy / positionCS.w;
			snappedPos.xy = floor(snappedPos.xy * float2(480, 640) + 0.5) / float2(480, 640);
			snappedPos.xy *= positionCS.w;
			return snappedPos;
		}

		// Shared tessellation factor calculation
		float CalcDistanceTessFactor(float4 vertex, float minDist, float maxDist, float tess) {
			float3 worldPosition = TransformObjectToWorld(vertex.xyz);
			float dist = distance(worldPosition, _WorldSpaceCameraPos);
			float f = clamp(1.0 - (dist - minDist) / (maxDist - minDist), 0.01, 1.0);
			float tessLevel = f * tess;
			tessLevel = pow(2.0, round(log2(max(tessLevel, 1.0))));
			return clamp(tessLevel, 1.0, tess);
		}

		// Generic tessellation structures
		struct TessellationFactors {
			float edge[3] : SV_TessFactor;
			float inside  : SV_InsideTessFactor;
		};

		// Shared patch constant function
		#define _PATCH_CONSTANT_FUNCTION(InputType) \
		TessellationFactors PatchConstant(InputPatch<InputType, 3> patch) { \
			TessellationFactors f; \
			float minDist = 0.0; \
			float maxDist = _TessellationMaxDistance; \
			f.edge[0] = CalcDistanceTessFactor((patch[1].PositionOS + patch[2].PositionOS) * 0.5, minDist, maxDist, _TessellationFactor); \
			f.edge[1] = CalcDistanceTessFactor((patch[2].PositionOS + patch[0].PositionOS) * 0.5, minDist, maxDist, _TessellationFactor); \
			f.edge[2] = CalcDistanceTessFactor((patch[0].PositionOS + patch[1].PositionOS) * 0.5, minDist, maxDist, _TessellationFactor); \
			f.inside = (f.edge[0] + f.edge[1] + f.edge[2]) / 3.0; \
			return f; \
		}

		// Domain interpolation macro
		#define _DOMAIN_INTERPOLATE(fieldName) v.fieldName = \
			patch[0].fieldName * barycentricCoordinates.x + \
			patch[1].fieldName * barycentricCoordinates.y + \
			patch[2].fieldName * barycentricCoordinates.z;

		ENDHLSL

		Pass {
			Name "ForwardLit"
			Tags { "LightMode"="UniversalForward" }

			Blend [_SrcBlend] [_DstBlend]
			ZWrite [_ZWrite]

			HLSLPROGRAM
			#pragma target 4.6
			#pragma require tessellation
			
			#pragma vertex TessellationVertex
			#pragma hull Hull
			#pragma domain Domain
			#pragma fragment LitPassFragment

			#pragma shader_feature_local _NORMALMAP
			#pragma shader_feature_local_fragment _EMISSION
			#pragma shader_feature_local _RECEIVE_SHADOWS_OFF
			#pragma shader_feature_local_fragment _SURFACE_TYPE_TRANSPARENT
			#pragma shader_feature_local_fragment _ALPHATEST_ON
			#pragma shader_feature_local_fragment _ALPHAPREMULTIPLY_ON
			#pragma shader_feature_local_fragment _ _SPECGLOSSMAP
			#pragma shader_feature_local _AFFINE_TEXTURE_MAPPING
			#define _SPECULAR_COLOR
			
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
			#pragma multi_compile _ _SHADOWS_SOFT
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			#pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
			#pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
			#pragma multi_compile _ SHADOWS_SHADOWMASK
			#pragma multi_compile _ LIGHTMAP_ON
			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
			#pragma multi_compile_fog

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

			struct Attributes {
				float4 PositionOS	: POSITION;
				float4 NormalOS		: NORMAL;
				#ifdef _NORMALMAP
					float4 TangentOS : TANGENT;
				#endif
				float2 UV		    : TEXCOORD0;
				float2 LightmapUV	: TEXCOORD1;
				float4 Color		: COLOR;
			};

			struct TessellationControlPoint {
				float4 PositionOS	: INTERNALTESSPOS;
				float4 NormalOS		: NORMAL;
				#ifdef _NORMALMAP
					float4 TangentOS : TANGENT;
				#endif
				float2 UV		    : TEXCOORD0;
				float2 LightmapUV	: TEXCOORD1;
				float4 Color		: COLOR;
			};

			struct Varyings {
				float4 PositionCS : SV_POSITION;
				#ifdef _AFFINE_TEXTURE_MAPPING
				noperspective float2 UV : TEXCOORD0;
				#else
				float2 UV : TEXCOORD0;
				#endif
				
				DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 1);
				float3 PositionWS : TEXCOORD2;

				#ifdef _NORMALMAP
					half4 NormalWS : TEXCOORD3;
					half4 TangentWS : TEXCOORD4;
					half4 BitangentWS : TEXCOORD5;
				#else
					half3 NormalWS : TEXCOORD3;
				#endif
				
				#ifdef _ADDITIONAL_LIGHTS_VERTEX
					half4 FogFactorAndVertexLight : TEXCOORD6;
				#else
					half FogFactor : TEXCOORD6;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
					float4 shadowCoord : TEXCOORD7;
				#endif

				float4 Color : COLOR;
			};

			TEXTURE2D(_SpecGlossMap); SAMPLER(sampler_SpecGlossMap);

			TessellationControlPoint TessellationVertex(Attributes v) {
				TessellationControlPoint o;
				o.PositionOS = v.PositionOS;
				o.NormalOS = v.NormalOS;
				#ifdef _NORMALMAP
					o.TangentOS = v.TangentOS;
				#endif
				o.UV = v.UV;
				o.LightmapUV = v.LightmapUV;
				o.Color = v.Color;
				return o;
			}

			_PATCH_CONSTANT_FUNCTION(TessellationControlPoint)

			[domain("tri")]
			[outputcontrolpoints(3)]
			[outputtopology("triangle_cw")]
			[partitioning("integer")]
			[patchconstantfunc("PatchConstant")]
			TessellationControlPoint Hull(InputPatch<TessellationControlPoint, 3> patch, uint id : SV_OutputControlPointID) {
				return patch[id];
			}

			Varyings LitPassVertex(Attributes IN) {
				Varyings OUT;

				VertexPositionInputs positionInputs = GetVertexPositionInputs(IN.PositionOS.xyz);
				#ifdef _NORMALMAP
					VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.NormalOS.xyz, IN.TangentOS);
				#else
					VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.NormalOS.xyz);
				#endif
				
				OUT.PositionCS = SnapToPixelGrid(positionInputs.positionCS);
				OUT.PositionWS = positionInputs.positionWS;
				
				half3 viewDirWS = GetWorldSpaceViewDir(positionInputs.positionWS);
				half3 vertexLight = VertexLighting(positionInputs.positionWS, normalInputs.normalWS);
				half fogFactor = ComputeFogFactor(positionInputs.positionCS.z);
				
				#ifdef _NORMALMAP
					OUT.NormalWS = half4(normalInputs.normalWS, viewDirWS.x);
					OUT.TangentWS = half4(normalInputs.tangentWS, viewDirWS.y);
					OUT.BitangentWS = half4(normalInputs.bitangentWS, viewDirWS.z);
				#else
					OUT.NormalWS = NormalizeNormalPerVertex(normalInputs.normalWS);
				#endif

				OUTPUT_LIGHTMAP_UV(IN.lightmapUV, unity_LightmapST, OUT.lightmapUV);
				OUTPUT_SH(OUT.NormalWS.xyz, OUT.vertexSH);

				#ifdef _ADDITIONAL_LIGHTS_VERTEX
					OUT.FogFactorAndVertexLight = half4(fogFactor, vertexLight);
				#else
					OUT.FogFactor = fogFactor;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
					OUT.shadowCoord = GetShadowCoord(positionInputs);
				#endif

				OUT.UV = TRANSFORM_TEX(IN.UV, _BaseMap);
				OUT.Color = IN.Color;
				return OUT;
			}

			[domain("tri")]
			Varyings Domain(TessellationFactors factors, OutputPatch<TessellationControlPoint, 3> patch, float3 barycentricCoordinates : SV_DomainLocation) {
				Attributes v;
				_DOMAIN_INTERPOLATE(PositionOS)
				_DOMAIN_INTERPOLATE(NormalOS)
				#ifdef _NORMALMAP
					_DOMAIN_INTERPOLATE(TangentOS)
				#endif
				_DOMAIN_INTERPOLATE(UV)
				_DOMAIN_INTERPOLATE(LightmapUV)
				_DOMAIN_INTERPOLATE(Color)
				return LitPassVertex(v);
			}

			void InitalizeSurfaceData(Varyings IN, out SurfaceData surfaceData){
				surfaceData = (SurfaceData)0;

				half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.UV);

				#ifdef _ALPHATEST_ON
					clip(baseMap.a - _Cutoff);
				#endif

				half4 diffuse = baseMap * _BaseColor * IN.Color;
				surfaceData.albedo = diffuse.rgb;
				surfaceData.alpha = diffuse.a;
				surfaceData.normalTS = SampleNormal(IN.UV, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap));
				surfaceData.emission = SampleEmission(IN.UV, _EmissionColor.rgb, TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));
				surfaceData.occlusion = 1.0;

				half4 specGlossMap = SAMPLE_TEXTURE2D(_SpecGlossMap, sampler_SpecGlossMap, IN.UV);
				surfaceData.specular = specGlossMap.rgb * _SpecColor.rgb;
				surfaceData.smoothness = specGlossMap.a * _Smoothness;
			}

			void InitializeInputData(Varyings input, half3 normalTS, out InputData inputData) {
				inputData = (InputData)0;
				inputData.positionWS = input.PositionWS;

				#ifdef _NORMALMAP
					half3 viewDirWS = half3(input.NormalWS.w, input.TangentWS.w, input.BitangentWS.w);
					inputData.normalWS = TransformTangentToWorld(normalTS, half3x3(input.TangentWS.xyz, input.BitangentWS.xyz, input.NormalWS.xyz));
				#else
					half3 viewDirWS = GetWorldSpaceNormalizeViewDir(inputData.positionWS);
					inputData.normalWS = input.NormalWS;
				#endif

				inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
				inputData.viewDirectionWS = SafeNormalize(viewDirWS);

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
					inputData.shadowCoord = input.shadowCoord;
				#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
					inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
				#else
					inputData.shadowCoord = float4(0, 0, 0, 0);
				#endif
				
				#ifdef _ADDITIONAL_LIGHTS_VERTEX
					inputData.fogCoord = InitializeInputDataFog(float4(inputData.positionWS, 1.0), input.FogFactorAndVertexLight.x);
					inputData.vertexLighting = input.FogFactorAndVertexLight.yzw;
				#else
					inputData.fogCoord = InitializeInputDataFog(float4(inputData.positionWS, 1.0), input.FogFactor);
					inputData.vertexLighting = half3(0, 0, 0);
				#endif

				inputData.bakedGI = SAMPLE_GI(input.lightmapUV, input.vertexSH, inputData.normalWS);
				inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.PositionCS);
				inputData.shadowMask = SAMPLE_SHADOWMASK(input.lightmapUV);
			}

			half4 LitPassFragment(Varyings IN) : SV_Target {
				SurfaceData surfaceData;
				InitalizeSurfaceData(IN, surfaceData);

				InputData inputData;
				InitializeInputData(IN, surfaceData.normalTS, inputData);

				ApplyDecalToSurfaceData(IN.PositionCS, surfaceData, inputData);
				
				half4 color = UniversalFragmentBlinnPhong(inputData, surfaceData.albedo, half4(surfaceData.specular, 1), surfaceData.smoothness, surfaceData.emission, surfaceData.alpha, surfaceData.normalTS);
				color.rgb = MixFog(color.rgb, inputData.fogCoord);
				return color;
			}
			ENDHLSL
		}

		Pass {
			Name "ShadowCaster"
			Tags { "LightMode"="ShadowCaster" }

			ZWrite On
			ZTest LEqual
			ColorMask 0
			Cull Back

			HLSLPROGRAM
			#pragma target 4.6
			#pragma require tessellation
			
			#pragma vertex TessellationVertexShadow
			#pragma hull HullShadow
			#pragma domain DomainShadow
			#pragma fragment ShadowPassFragment

			#pragma shader_feature_local_fragment _ALPHATEST_ON
			#pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

			float3 _LightDirection;
			float3 _LightPosition;

			struct AttributesShadow {
				float4 PositionOS : POSITION;
				float3 NormalOS : NORMAL;
				float2 Texcoord : TEXCOORD0;
			};

			struct TessellationControlPointShadow {
				float4 PositionOS : INTERNALTESSPOS;
				float3 NormalOS : NORMAL;
				float2 Texcoord : TEXCOORD0;
			};

			struct VaryingsShadow {
				float2 UV : TEXCOORD0;
				float4 PositionCS : SV_POSITION;
			};

			TessellationControlPointShadow TessellationVertexShadow(AttributesShadow v) {
				TessellationControlPointShadow o;
				o.PositionOS = v.PositionOS;
				o.NormalOS = v.NormalOS;
				o.Texcoord = v.Texcoord;
				return o;
			}

			_PATCH_CONSTANT_FUNCTION(TessellationControlPointShadow)

			[domain("tri")]
			[outputcontrolpoints(3)]
			[outputtopology("triangle_cw")]
			[partitioning("integer")]
			[patchconstantfunc("PatchConstant")]
			TessellationControlPointShadow HullShadow(InputPatch<TessellationControlPointShadow, 3> patch, uint id : SV_OutputControlPointID) {
				return patch[id];
			}

			float4 GetShadowPositionHClip(AttributesShadow input) {
				float3 positionWS = TransformObjectToWorld(input.PositionOS.xyz);
				float3 normalWS = TransformObjectToWorldNormal(input.NormalOS);

				#if _CASTING_PUNCTUAL_LIGHT_SHADOW
					float3 lightDirectionWS = normalize(_LightPosition - positionWS);
				#else
					float3 lightDirectionWS = _LightDirection;
				#endif

				positionWS = positionWS + normalWS * _CustomShadowNormalBias * 0.001;
				positionWS = positionWS + lightDirectionWS * _CustomShadowBias * 0.001;

				float4 positionCS = TransformWorldToHClip(positionWS);

				#if UNITY_REVERSED_Z
					positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
				#else
					positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
				#endif

				return positionCS;
			}

			VaryingsShadow ShadowPassVertex(AttributesShadow input) {
				VaryingsShadow output;
				output.UV = TRANSFORM_TEX(input.Texcoord, _BaseMap);
				output.PositionCS = GetShadowPositionHClip(input);
				return output;
			}

			[domain("tri")]
			VaryingsShadow DomainShadow(TessellationFactors factors, OutputPatch<TessellationControlPointShadow, 3> patch, float3 barycentricCoordinates : SV_DomainLocation) {
				AttributesShadow v;
				_DOMAIN_INTERPOLATE(PositionOS)
				_DOMAIN_INTERPOLATE(NormalOS)
				_DOMAIN_INTERPOLATE(Texcoord)
				return ShadowPassVertex(v);
			}

			half4 ShadowPassFragment(VaryingsShadow input) : SV_TARGET {
				#ifdef _ALPHATEST_ON
					half alpha = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.UV).a;
					clip(alpha - _Cutoff);
				#endif
				return 0;
			}
			ENDHLSL
		}

		Pass {
			Name "DepthOnly"
			Tags { "LightMode"="DepthOnly" }

			ColorMask 0
			ZWrite On
			ZTest LEqual

			HLSLPROGRAM
			#pragma target 4.6
			#pragma require tessellation
			
			#pragma vertex TessellationVertexDepth
			#pragma hull HullDepth
			#pragma domain DomainDepth
			#pragma fragment DepthOnlyFragment

			#pragma shader_feature_local_fragment _ALPHATEST_ON

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

			struct AttributesDepth {
				float4 PositionOS : POSITION;
				float2 Texcoord : TEXCOORD0;
			};

			struct TessellationControlPointDepth {
				float4 PositionOS : INTERNALTESSPOS;
				float2 Texcoord : TEXCOORD0;
			};

			struct VaryingsDepth {
				float2 UV : TEXCOORD0;
				float4 PositionCS : SV_POSITION;
			};

			TessellationControlPointDepth TessellationVertexDepth(AttributesDepth v) {
				TessellationControlPointDepth o;
				o.PositionOS = v.PositionOS;
				o.Texcoord = v.Texcoord;
				return o;
			}

			_PATCH_CONSTANT_FUNCTION(TessellationControlPointDepth)

			[domain("tri")]
			[outputcontrolpoints(3)]
			[outputtopology("triangle_cw")]
			[partitioning("integer")]
			[patchconstantfunc("PatchConstant")]
			TessellationControlPointDepth HullDepth(InputPatch<TessellationControlPointDepth, 3> patch, uint id : SV_OutputControlPointID) {
				return patch[id];
			}

			VaryingsDepth DepthOnlyVertex(AttributesDepth input) {
				VaryingsDepth output;
				output.UV = TRANSFORM_TEX(input.Texcoord, _BaseMap);
				output.PositionCS = TransformObjectToHClip(input.PositionOS.xyz);
				return output;
			}

			[domain("tri")]
			VaryingsDepth DomainDepth(TessellationFactors factors, OutputPatch<TessellationControlPointDepth, 3> patch, float3 barycentricCoordinates : SV_DomainLocation) {
				AttributesDepth v;
				_DOMAIN_INTERPOLATE(PositionOS)
				_DOMAIN_INTERPOLATE(Texcoord)
				return DepthOnlyVertex(v);
			}

			half4 DepthOnlyFragment(VaryingsDepth input) : SV_TARGET {
				#ifdef _ALPHATEST_ON
					half alpha = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.UV).a;
					clip(alpha - _Cutoff);
				#endif
				return 0;
			}
			ENDHLSL
		}

		Pass {
			Name "DepthNormals"
			Tags { "LightMode"="DepthNormals" }

			ZWrite On
			ZTest LEqual

			HLSLPROGRAM
			#pragma target 4.6
			#pragma require tessellation
			
			#pragma vertex TessellationVertexDepthNormals
			#pragma hull HullDepthNormals
			#pragma domain DomainDepthNormals
			#pragma fragment DepthNormalsFragment

			#pragma shader_feature_local _NORMALMAP
			#pragma shader_feature_local_fragment _ALPHATEST_ON

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

			struct AttributesDepthNormals {
				float4 PositionOS : POSITION;
				float4 TangentOS : TANGENT;
				float3 NormalOS : NORMAL;
				float2 Texcoord : TEXCOORD0;
			};

			struct TessellationControlPointDepthNormals {
				float4 PositionOS : INTERNALTESSPOS;
				float4 TangentOS : TANGENT;
				float3 NormalOS : NORMAL;
				float2 Texcoord : TEXCOORD0;
			};

			struct VaryingsDepthNormals {
				float4 PositionCS : SV_POSITION;
				float2 UV : TEXCOORD1;
				#ifdef _NORMALMAP
					half4 NormalWS : TEXCOORD2;
					half4 TangentWS : TEXCOORD3;
					half4 BitangentWS : TEXCOORD4;
				#else
					half3 NormalWS : TEXCOORD2;
				#endif
			};

			TessellationControlPointDepthNormals TessellationVertexDepthNormals(AttributesDepthNormals v) {
				TessellationControlPointDepthNormals o;
				o.PositionOS = v.PositionOS;
				o.TangentOS = v.TangentOS;
				o.NormalOS = v.NormalOS;
				o.Texcoord = v.Texcoord;
				return o;
			}

			_PATCH_CONSTANT_FUNCTION(TessellationControlPointDepthNormals)

			[domain("tri")]
			[outputcontrolpoints(3)]
			[outputtopology("triangle_cw")]
			[partitioning("integer")]
			[patchconstantfunc("PatchConstant")]
			TessellationControlPointDepthNormals HullDepthNormals(InputPatch<TessellationControlPointDepthNormals, 3> patch, uint id : SV_OutputControlPointID) {
				return patch[id];
			}

			VaryingsDepthNormals DepthNormalsVertex(AttributesDepthNormals input) {
				VaryingsDepthNormals output;

				output.UV = TRANSFORM_TEX(input.Texcoord, _BaseMap);
				output.PositionCS = TransformObjectToHClip(input.PositionOS.xyz);

				VertexPositionInputs vertexInput = GetVertexPositionInputs(input.PositionOS.xyz);
				#ifdef _NORMALMAP
					VertexNormalInputs normalInput = GetVertexNormalInputs(input.NormalOS, input.TangentOS);
					half3 viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
					output.NormalWS = half4(normalInput.normalWS, viewDirWS.x);
					output.TangentWS = half4(normalInput.tangentWS, viewDirWS.y);
					output.BitangentWS = half4(normalInput.bitangentWS, viewDirWS.z);
				#else
					output.NormalWS = TransformObjectToWorldNormal(input.NormalOS);
				#endif

				return output;
			}

			[domain("tri")]
			VaryingsDepthNormals DomainDepthNormals(TessellationFactors factors, OutputPatch<TessellationControlPointDepthNormals, 3> patch, float3 barycentricCoordinates : SV_DomainLocation) {
				AttributesDepthNormals v;
				_DOMAIN_INTERPOLATE(PositionOS)
				_DOMAIN_INTERPOLATE(TangentOS)
				_DOMAIN_INTERPOLATE(NormalOS)
				_DOMAIN_INTERPOLATE(Texcoord)
				return DepthNormalsVertex(v);
			}

			half4 DepthNormalsFragment(VaryingsDepthNormals input) : SV_TARGET {
				#ifdef _ALPHATEST_ON
					half alpha = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.UV).a;
					clip(alpha - _Cutoff);
				#endif

				#ifdef _NORMALMAP
					half3 normalTS = SampleNormal(input.UV, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap));
					half3 normalWS = TransformTangentToWorld(normalTS, half3x3(input.TangentWS.xyz, input.BitangentWS.xyz, input.NormalWS.xyz));
				#else
					half3 normalWS = input.NormalWS;
				#endif

				return half4(NormalizeNormalPerPixel(normalWS), 0.0);
			}
			ENDHLSL
		}
	}
}