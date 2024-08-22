Shader"Universal Render Pipeline/LowPolyWaterShaderGraph"
{
    Properties
    {
        _BaseColor ("Base color", Color)  = ( .54, .95, .99, 0.5) 
        _SpecColor ("Specular Material Color", Color) = (1,1,1,1) 
        _Shininess ("Shininess", Float) = 10
        _ShoreTex ("Shore & Foam texture", 2D) = "black" {} 
        _InvFadeParemeter ("Auto blend parameter (Edge, Shore, Distance scale)", Vector) = (0.2 ,0.39, 0.5, 1.0)
        _BumpTiling ("Foam Tiling", Vector) = (1.0 ,1.0, -2.0, 3.0)
        _BumpDirection ("Foam movement", Vector) = (1.0 ,1.0, -1.0, 1.0) 
        _Foam ("Foam (intensity, cutoff)", Vector) = (0.1, 0.375, 0.0, 0.0) 
        _isInnerAlphaBlendOrColor ("Fade inner to color or alpha?", Float) = 0 
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
Blend SrcAlpha
OneMinusSrcAlpha
        ZTest
LEqual
        ZWrite
Off
        CullOff

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile_fog
            #pragma multi_compile WATER_EDGEBLEND_ON WATER_EDGEBLEND_OFF 

#include "UnityCG.cginc"

struct appdata
{
    float4 vertex : POSITION;
    float3 normal : NORMAL;
};

struct v2f
{
    float4 pos : SV_POSITION;
    float4 normalInterpolator : TEXCOORD0;
    float4 viewInterpolator : TEXCOORD1;
    float4 bumpCoords : TEXCOORD2;
    float4 screenPos : TEXCOORD3;
    half3 worldRefl : TEXCOORD6;
    float4 posWorld : TEXCOORD7;
    float3 normalDir : TEXCOORD8;
                UNITY_FOG_COORDS(5)
};

sampler2D _ShoreTex;
float4 _BaseColor;
float4 _InvFadeParemeter;
float4 _BumpTiling;
float4 _BumpDirection;
float4 _Foam;
float _isInnerAlphaBlendOrColor;

inline half4 Foam(sampler2D shoreTex, half4 coords)
{
    half4 foam = (tex2D(shoreTex, coords.xy) * tex2D(shoreTex, coords.zw)) - 0.125;
    return foam;
}

v2f vert(appdata v)
{
    v2f o;
    o.normalInterpolator.xyz = v.normal;
    o.normalInterpolator.w = 1;
    o.bumpCoords.xyzw = (v.vertex.xyxy + _Time.xyxy * _BumpDirection.xyxy) * _BumpTiling.xyxy;
    o.pos = UnityObjectToClipPos(v.vertex);
    o.viewInterpolator.xyz = ObjSpaceViewDir(v.vertex.xyz);
    o.screenPos = ComputeScreenPos(o.pos);
    o.posWorld = mul(unity_ObjectToWorld, v.vertex);
    o.normalDir = mul((float3x3) unity_WorldToObject, v.normal);
    UNITY_TRANSFER_FOG(o, o.pos);
    return o;
}

half4 frag(v2f i) : SV_Target
{
    half4 edgeBlendFactors = half4(1.0, 0.0, 0.0, 0.0);
#ifdef WATER_EDGEBLEND_ON
                    half depth = LinearEyeDepth(TEXTURE2D(_CameraDepthTexture,UNITY_PROJ_COORD(i.screenPos)).r);
                    edgeBlendFactors = saturate(_InvFadeParemeter * (depth - i.screenPos.w));
                    edgeBlendFactors.y = 1.0 - edgeBlendFactors.y;
#endif

    half4 baseColor = _BaseColor;

    half4 foam = Foam(_ShoreTex, i.bumpCoords * 2.0);
    baseColor.rgb += foam.rgb * _Foam.x * (edgeBlendFactors.y + saturate(i.viewInterpolator.w - _Foam.y));

    if (_isInnerAlphaBlendOrColor == 0)
        baseColor.rgb += 1.0 - edgeBlendFactors.x;

    if (_isInnerAlphaBlendOrColor == 1.0)
        baseColor.a = edgeBlendFactors.x;

    UNITY_APPLY_FOG(i.fogCoord, baseColor);
    return baseColor;
}
            ENDCG
        }
    }
}
