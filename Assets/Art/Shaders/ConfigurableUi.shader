Shader "Configurable/UI"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		[HDR] _Color ("Tint", Color) = (1,1,1,1)
		[SimpleToggle] _UseVertexColor("Vertex color", Float) = 1.0
		[SimpleToggle] _AlphaBrightnessControl("Alpha controls Brightness", Float) = 0.0
		
		[HeaderHelpURL(Rendering, https, github.com supyrb ConfigurableShaders wiki Rendering)]
		[Enum(None,0,Alpha,1,Red,8,Green,4,Blue,2,RGB,14,RGBA,15)]_ColorMask ("Color Mask", Float) = 14
		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 1
		
		[Enum(UnityEngine.Rendering.BlendMode)] _BlendSrc ("Blend mode Source", Int) = 5
		[Enum(UnityEngine.Rendering.BlendMode)] _BlendDst ("Blend mode Destination", Int) = 10
		
		[EightBit] _Stencil ("Stencil ID", Int) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp ("Stencil Comparison", Int) = 0
		[Enum(UnityEngine.Rendering.StencilOp)] _StencilOp ("Stencil Operation", Int) = 0
		[Enum(UnityEngine.Rendering.StencilOp)] _StencilFail ("Stencil Fail", Int) = 0
		[Enum(UnityEngine.Rendering.StencilOp)] _StencilZFail ("Stencil ZFail", Int) = 0
		[EightBit] _StencilReadMask ("Stencil Read Mask", Int) = 255
		[EightBit] _StencilWriteMask ("Stencil Write Mask", Int) = 255
	}

	CGINCLUDE
	#include "UnityCG.cginc"
	#include "UnityUI.cginc"
	
	half4 _Color;
	half _UseVertexColor;
	half _AlphaBrightnessControl;
	fixed4 _TextureSampleAdd;
	float4 _ClipRect;
	
	struct appdata_t
	{
		float4 vertex	: POSITION;
		float4 color	: COLOR;
		float2 texcoord : TEXCOORD0;
		UNITY_VERTEX_INPUT_INSTANCE_ID
	};

	struct v2f
	{
		float4 vertex	: SV_POSITION;
		half4 color	: COLOR;
		float2 texcoord	 : TEXCOORD0;
		float4 worldPosition : TEXCOORD1;
		UNITY_VERTEX_OUTPUT_STEREO
	};
 

	v2f vert(appdata_t IN)
	{
		v2f OUT;
		UNITY_SETUP_INSTANCE_ID(IN);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
		OUT.worldPosition = IN.vertex;
		OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

		OUT.texcoord = IN.texcoord;
	 
		half4 color = lerp(_Color, IN.color * _Color, _UseVertexColor);
		color.rgb *= lerp(1.0, color.a, _AlphaBrightnessControl);
		OUT.color = color;
		return OUT;
	}

	sampler2D _MainTex;

	half4 frag(v2f IN) : SV_Target
	{
		half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color * _Color;
	 
		color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
	 
		#ifdef UNITY_UI_ALPHACLIP
		clip (color.a - 0.001);
		#endif

		return color;
	}
	ENDCG
	
	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}
		
		Cull Off
		Lighting Off
		ZWrite Off
		ZTest Always
		Blend [_BlendSrc] [_BlendDst]
		ColorMask [_ColorMask]
		
		Stencil
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp]
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}
		
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile __ UNITY_UI_ALPHACLIP
			ENDCG
		}
	}
}