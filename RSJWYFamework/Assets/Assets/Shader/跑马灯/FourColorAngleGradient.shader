Shader "Custom/FourColorAngleGradient"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Color0("Color 0", Color) = (1, 0, 0, 1)
        _Color90("Color 90", Color) = (0, 1, 0, 1)
        _Color180("Color 180", Color) = (0, 0, 1, 1)
        _Color270("Color 270", Color) = (1, 1, 0, 1)
        _GradientAngle("Gradient Angle", Range(-180, 180)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
 
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
 
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
            float4 _Color0;
            float4 _Color90;
            float4 _Color180;
            float4 _Color270;
            float _GradientAngle;
 
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
 
            float4 frag(v2f i) : SV_Target
            {
                // Sample the texture
                float4 texColor = tex2D(_MainTex, i.uv);
                if (texColor.a == 0)
                    discard;
 
                // Calculate angle
                float2 coord = i.uv - 0.5; // Offset to center
                coord.y *= -1; // Invert y for correct angle calculation
                float angle = atan2(coord.y, coord.x) * (180.0 / 3.14159265);
                angle += 180.0; // Offset to make angle positive
                angle -= _GradientAngle; // Apply gradient angle offset
                if (angle < 0) angle += 360.0;
                if (angle >= 360) angle -= 360.0;
 
                // Determine color based on angle
                float4 color;
                if (angle < 90)
                {
                    color = lerp(_Color0, _Color90, angle / 90);
                }
                else if (angle < 180)
                {
                    color = lerp(_Color90, _Color180, (angle - 90) / 90);
                }
                else if (angle < 270)
                {
                    color = lerp(_Color180, _Color270, (angle - 180) / 90);
                }
                else
                {
                    color = lerp(_Color270, _Color0, (angle - 270) / 90);
                }
 
                // Combine texture and gradient color
                return color * texColor;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}