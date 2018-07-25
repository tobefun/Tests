Shader "Unlit/ParticleExp_Beta"
{
    Properties
    {
        //细分相关变量
        _Level("Level",Range(1, 4))=0
        _DispDir("Displacement Direction",Vector)=(0,0,0)
        _uVelScale("VelScale",float)=2
        //粒子化特效相关变量
        _Speed("Speed",Range(0,5))=1
        _splitSpeed("Split Speed",Range(0,5))=1
        _fadeSpeed("Fade Speed",Range(0,1))=1
        _ShaderStartTime("Shader Start Time",float)=0
        _FinalColor("Final Color",color)=(1,1,1,1)
    }

    SubShader
    {
        Tags{"RenderType"="Transparent" "Queue" = "Transparent"}
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha // use alpha blending
            cull off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma geometry geom

            #include "UnityCG.cginc"
            //CPU输入变量
            ////细分相关变量
            uniform int _Level;
            uniform float3 _DispDir;
            uniform float _uVelScale;
            ////粒子化特效相关变量
            uniform float _Speed;           //粒子位移速度
			float _splitSpeed;
            float _fadeSpeed;
            uniform float _ShaderStartTime; //粒子化起始时间
            uniform fixed4 _FinalColor;     //粒子颜色

            //内部变量
            float3 V0, V1, V2;
            float3 CG;


            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal:NORMAL;
            };

            struct v2g
            {
                float4 vertex : SV_POSITION;
                fixed4 color:COLOR;
                float3 normal:NORMAL;
            };

            struct g2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color:COLOR;
            };

            v2g vert (appdata v)
            {
                v2g o;
                o.vertex = v.vertex;
                o.normal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

           [maxvertexcount(120)]//v2g input[3]
           void geom(inout PointStream<g2f> OutputStream,triangle v2g input[3])
           {
              float time_SinceBirth = (_Time.y - _ShaderStartTime) * _splitSpeed;
              g2f o = (g2f)0;
              V1 = (input[1].vertex - input[0].vertex).xyz;
              V2 = (input[2].vertex - input[0].vertex).xyz;
              V0 = input[0].vertex.xyz;
              CG = (input[0].vertex.xyz + input[1].vertex.xyz + input[2].vertex.xyz)/3.0f;

              int numLayers = 1<<_Level;     //2^_Level
              float dt = 1.0f / float( numLayers );
              float t = 1.0f;
              for( int it = 0; it < numLayers; it++ )
              {
				  float smax = 1.0f - t;
				  int nums = it + 1;
				  float ds = smax / float( nums - 1 );
				  float s = 0;
				  for( int is = 0; is < nums; is++ )
				  {
				      float3 v = V0 + s*V1 + t*V2;
				      float3 vel = _uVelScale * ( v - CG );
				      v = CG + vel*(_Speed*time_SinceBirth+1.0f) + 0.5f*_DispDir.xyz*sin(it*is)*(_Speed*time_SinceBirth)*(_Speed*time_SinceBirth);
				      o.vertex = mul(UNITY_MATRIX_MVP, float4( v, 1.0f ));//UnityObjectToClipPos(float4( v, 1.0f ));
				      o.color =_FinalColor;
				      o.color.w = 1.0f-smoothstep(0,1.0f,time_SinceBirth * _fadeSpeed);
				      OutputStream.Append(o);
				      s += ds;
				  } 
				  t -= dt;
              }
           }

            fixed4 frag (g2f i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }
    }
}