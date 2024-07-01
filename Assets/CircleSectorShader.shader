Shader "Custom/CircleSectorShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _SectorColor ("Sector Color", Color) = (1,0,0,1)
        _Angle ("Angle", Range(0, 360)) = 0
        _SectorWidth ("Sector Width", Range(0, 360)) = 15
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float4 _SectorColor;
            float _Angle;
            float _SectorWidth;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                float angle = atan2(i.uv.y - 0.5, i.uv.x - 0.5) * 57.2958; // Convert to degrees
                if (angle < 0) angle += 360;

                if (angle >= _Angle && angle <= (_Angle + _SectorWidth))
                {
                    col = _SectorColor;
                }
                return col;
            }
            ENDCG
        }
    }
}
