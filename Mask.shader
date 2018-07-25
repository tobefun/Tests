Shader "Unlit/Mask"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_angle("Angle", range(0, 2)) = 1
		_speed("Speed", range(0, 1)) = 0.1
		_loopTime("Loop Time", Range(1, 10)) = 1
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _angle;
			float _speed;
			float _loopTime;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				half xlength = 1/tan(_angle);
				half ylength = i.uv.y * xlength;
				//half x = saturate(fmod(_speed * _Time.y, _loopTime))*(1+xlength);

				half x = _speed;
				half v = i.uv.x - clamp(x + ylength, - xlength, 1 + xlength);
				clip(v);
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				half4 dest = col;

				return col; 
			}
			ENDCG
		}
	}
}
