Shader "Unlit/Glass"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_width("Width", range(0.01, 0.2)) = 0.1
		_angle("Angle", range(0, 2)) = 1
		_speed("Speed", range(0, 1)) = 0.1
		_max("Max Light Width", range(1, 1.5)) = 1
		_light("Light", range(1, 100)) = 10
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
			float _width;
			float _angle;
			float _speed;
			float _max;
			float _light;
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
				half x = saturate(fmod(_speed * _Time.y, _loopTime))*(1+xlength + _width);

				//half x = _speed;
				half v = _light * saturate(_width - abs(i.uv.x - clamp(x - ylength, - xlength - _width, 1 + xlength+ _width)));

				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				half4 dest = col + half4(1,1,1,1);
				col = lerp(col, dest, v);

				return col; 
			}
			ENDCG
		}
	}
}
