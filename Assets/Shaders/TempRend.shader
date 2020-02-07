Shader "Unlit/TempRend"
{
	SubShader{
		 Pass {
		 Tags{ "RenderType" = "Opaque" }
		 LOD 200
		 Blend SrcAlpha one

		 CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma vertex vert
		#pragma fragment frag

		#include "UnityCG.cginc"

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 5.0

		struct Firefly {
			float3 pos;
			float3 vel;
			float4 col;
			float brightness;
			float scale;
		};

		struct FB_INPUT {
			float4 position : SV_POSITION;
			float4 color : COLOR;
			float brightness : BRIGHTNESS;
			float3 vel : VEL;
			float scale : SCALE;
		};

		StructuredBuffer<Firefly> FireflyBuffer;

		FB_INPUT vert(uint vertex_id : SV_VertexID, uint instance_id : SV_InstanceID)
		{
			FB_INPUT o = (FB_INPUT)0;

			// Color
			o.color = float4(FireflyBuffer[instance_id].col.rgb * FireflyBuffer[instance_id].brightness, 1.0);

			// Position
			o.position = UnityObjectToClipPos(float4(FireflyBuffer[instance_id].pos, 1.0f));

			return o;
		}

		float4 frag(FB_INPUT i) : COLOR
		{
			return i.color;
		}

		ENDCG
		}
	}
		FallBack Off
}
