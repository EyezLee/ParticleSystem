Shader "Unlit/TempRend"
{
	Properties{
		_MainTex("Texture", 2D) = "white" {}
		//_syncPace("SyncPace", Range(0, 0.5)) = 0.5
	}
	SubShader{
		 Pass {
		 Tags{ "RenderType" = "Transparent" }
		 LOD 200
		 ZWrite Off
		 Blend SrcAlpha OneMinusSrcAlpha

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
			float lifeOffset;
			float scale;
		};

		struct appData
		{
			float3 vertex : POSITION;
			float2 uv : TEXCOORD0;
		};

		struct FB_INPUT {
			float4 position : SV_POSITION;
			float4 color : COLOR;
			float brightness : BRIGHTNESS;
			float3 vel : VEL;
			float scale : SCALE;
			float2 uv : TEXCOORD0;
		};

		StructuredBuffer<Firefly> FireflyBuffer;
		sampler2D _MainTex;
		float4 _MainTex_ST;

		float _shineSpeed;

		FB_INPUT vert(uint vertex_id : SV_VertexID, uint instance_id : SV_InstanceID, appData v)
		{
			FB_INPUT o = (FB_INPUT)0;

			// Color
			float life = FireflyBuffer[instance_id].lifeOffset;
			float brightness = sin((life + _Time.y) * _shineSpeed) / 2 + 0.5;
			o.color = float4(FireflyBuffer[instance_id].col * brightness);
			o.uv = TRANSFORM_TEX(v.uv, _MainTex); // uv

			// Position
			float3 worldPos = FireflyBuffer[instance_id].pos;

			// vertices
			o.position = UnityObjectToClipPos(
				float4(v.vertex * FireflyBuffer[instance_id].scale + worldPos, 1));
			return o;
		}

		fixed4 frag(FB_INPUT i) : COLOR
		{
			fixed4 col = tex2D(_MainTex, i.uv) * i.color;
			return fixed4(1, 1, 1, 1);
		}

		ENDCG
		}
	}
		FallBack Off
}
