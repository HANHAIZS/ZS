Shader "BeeHarvest/VoxelCavity"
{
 Properties
 {
  _CavityStrength ("Local cavity strength", Range(0,1)) = 1
  [HideInInspector] _CavityData ("Face corner occlusion cache", 2D) = "black" {}
  _BaseColor ("Original palette", Color) = (1,1,1,1)
  _BaseMap ("Original surface", 2D) = "white" {}
  [Normal] _BumpMap ("Original normal", 2D) = "bump" {}
  _BumpScale ("Normal strength", Float) = 1
  _Metallic ("Metallic", Range(0,1)) = 0
  _Smoothness ("Smoothness", Range(0,1)) = .4
  _EdgeWidth ("Black edge / half block", Range(0,.15)) = 0
  _EdgeColor ("Edge color", Color) = (0,0,0,1)
  _VoxelCenter ("Mesh bounds center", Vector) = (0,0,0,0)
  _VoxelExtents ("Mesh bounds extents", Vector) = (.5,.5,.5,0)
 }
 SubShader
 {
  Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
  HLSLINCLUDE
  #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
  #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
  CBUFFER_START(UnityPerMaterial)
   half4 _BaseColor, _EdgeColor;
   half _Metallic, _Smoothness, _BumpScale;
   float4 _BaseMap_ST;
   float _EdgeWidth, _CavityStrength;
   float4 _CavityGridMin, _CavityGridSize, _CavityTextureSize;
   float4x4 _CavityWorldToGrid;
   float4 _VoxelCenter, _VoxelExtents;
  CBUFFER_END
  TEXTURE2D(_CavityData);SAMPLER(sampler_CavityData);
  TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
  TEXTURE2D(_BumpMap);SAMPLER(sampler_BumpMap);
  struct Attributes {
   float4 positionOS:POSITION;
   float3 normalOS:NORMAL;
   float4 tangentOS:TANGENT;
   float2 uv:TEXCOORD0;
   UNITY_VERTEX_INPUT_INSTANCE_ID
  };
  ENDHLSL
  Pass
  {
   Name "ForwardLit"
   Tags { "LightMode"="UniversalForward" }
   ZWrite On
   Cull Back
   HLSLPROGRAM
   #pragma target 3.0
   #pragma vertex Vert
   #pragma fragment Frag
   #pragma multi_compile_local _ _VOXEL_SURFACE
   #pragma multi_compile_local _ _NORMALMAP
   #pragma multi_compile_instancing
   #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
   #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
   #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
   #pragma multi_compile_fragment _ _SHADOWS_SOFT
   #pragma multi_compile_fog
   struct Varyings {
    float4 positionCS:SV_POSITION;
    float3 positionWS:TEXCOORD0;
    half3 normalWS:TEXCOORD1;
    float3 boxPosition:TEXCOORD2;
    half fog:TEXCOORD3;
    half3 vertexLight:TEXCOORD4;
    float3 gridOrigin:TEXCOORD5;
    #if defined(_VOXEL_SURFACE)
     float2 uv:TEXCOORD6;
    #endif
    #if defined(_NORMALMAP)
     half4 tangentWS:TEXCOORD7;
    #endif
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
   };
   Varyings Vert(Attributes input)
   {
    Varyings output=(Varyings)0;
    UNITY_SETUP_INSTANCE_ID(input);UNITY_TRANSFER_INSTANCE_ID(input,output);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
    VertexPositionInputs position=GetVertexPositionInputs(input.positionOS.xyz);
    output.positionCS=position.positionCS;output.positionWS=position.positionWS;
    output.normalWS=TransformObjectToWorldNormal(input.normalOS);
    #if defined(_VOXEL_SURFACE)
     output.uv=TRANSFORM_TEX(input.uv,_BaseMap);
    #endif
    #if defined(_NORMALMAP)
     output.tangentWS=half4(TransformObjectToWorldDir(input.tangentOS.xyz),
      input.tangentOS.w*GetOddNegativeScale());
    #endif
    output.boxPosition=(input.positionOS.xyz-_VoxelCenter.xyz)/max(_VoxelExtents.xyz,.0001);
    output.gridOrigin=mul(_CavityWorldToGrid,float4(TransformObjectToWorld(float3(0,0,0)),1)).xyz;
    output.fog=ComputeFogFactor(position.positionCS.z);
    output.vertexLight=VertexLighting(position.positionWS,output.normalWS);
    return output;
   }
   // One cached RGBA texel per voxel face; no neighbor searches in the fragment shader.
   half CavityShade(Varyings input)
   {
    float3 cell=floor(input.gridOrigin+.5);
    float3 index=cell-_CavityGridMin.xyz;
    if(any(index<0)||any(index>=_CavityGridSize.xyz))return 1;
    float3 normal=mul((float3x3)_CavityWorldToGrid,input.normalWS);
    float3 absolute=abs(normal);
    float3 position=mul(_CavityWorldToGrid,float4(input.positionWS,1)).xyz-cell+.5;
    float face;float2 uv;
    if(absolute.x>=absolute.y&&absolute.x>=absolute.z){face=normal.x>0?0:1;uv=position.zy;}
    else if(absolute.y>=absolute.z){face=normal.y>0?2:3;uv=position.xz;}
    else {face=normal.z>0?4:5;uv=position.xy;}
    float pixel=(index.x+_CavityGridSize.x*(index.y+_CavityGridSize.y*index.z))*6+face;
    float2 address=float2(fmod(pixel,_CavityTextureSize.x),floor(pixel/_CavityTextureSize.x));
    half4 corners=SAMPLE_TEXTURE2D(_CavityData,sampler_CavityData,(address+.5)/_CavityTextureSize.xy);
    half average=dot(corners,half4(.25,.25,.25,.25));
    half4 sides=half4(max(corners.r,corners.b),max(corners.g,corners.a),max(corners.r,corners.g),max(corners.b,corners.a));
    half4 proximity=saturate(1-half4(uv.x,1-uv.x,uv.y,1-uv.y)/.22);
    half4 contact=sides*proximity*proximity;
    half rim=max(max(contact.x,contact.y),max(contact.z,contact.w));
    return 1-_CavityStrength*min(.55,average*.22+rim*.28);
   }
   half4 Frag(Varyings input):SV_Target
   {
    UNITY_SETUP_INSTANCE_ID(input);UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    InputData lighting=(InputData)0;
    lighting.positionWS=input.positionWS;
    lighting.normalWS=NormalizeNormalPerPixel(input.normalWS);
    #if defined(_NORMALMAP) && defined(_VOXEL_SURFACE)
     half3 normalTS=UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap,sampler_BumpMap,input.uv),_BumpScale);
     half3 bitangentWS=input.tangentWS.w*cross(input.normalWS,input.tangentWS.xyz);
     lighting.normalWS=NormalizeNormalPerPixel(TransformTangentToWorld(normalTS,
      half3x3(input.tangentWS.xyz,bitangentWS,input.normalWS)));
    #endif
    lighting.viewDirectionWS=GetWorldSpaceNormalizeViewDir(input.positionWS);
    #if defined(_MAIN_LIGHT_SHADOWS_SCREEN)
     lighting.shadowCoord=ComputeScreenPos(TransformWorldToHClip(input.positionWS));
    #else
     lighting.shadowCoord=TransformWorldToShadowCoord(input.positionWS);
    #endif
    lighting.bakedGI=SampleSH(lighting.normalWS);
    lighting.vertexLighting=input.vertexLight;
    lighting.normalizedScreenSpaceUV=GetNormalizedScreenSpaceUV(input.positionCS);
    lighting.shadowMask=half4(1,1,1,1);
    SurfaceData surface=(SurfaceData)0;
    surface.albedo=_BaseColor.rgb;surface.metallic=_Metallic;surface.smoothness=_Smoothness;
    #if defined(_VOXEL_SURFACE)
     surface.albedo*=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,input.uv).rgb;
    #endif
    surface.normalTS=half3(0,0,1);surface.occlusion=1;surface.alpha=1;
    half4 color=UniversalFragmentPBR(lighting,surface);
    // The second-nearest box boundary identifies the twelve true edges, never mesh diagonals.
    float3 d=abs(1-abs(input.boxPosition));
    float edge=min(max(d.x,d.y),min(max(d.y,d.z),max(d.z,d.x)));
    float aa=max(fwidth(edge),.0001);
    float face=smoothstep(_EdgeWidth-aa*.5,_EdgeWidth+aa*.5,edge);
    color.rgb=MixFog((_EdgeWidth>0?lerp(_EdgeColor.rgb,color.rgb,face):color.rgb)*CavityShade(input),input.fog);
    return half4(color.rgb,1);
   }
   ENDHLSL
  }
  Pass
  {
   Name "ShadowCaster"
   Tags { "LightMode"="ShadowCaster" }
   ZWrite On ZTest LEqual ColorMask 0 Cull Back
   HLSLPROGRAM
   #pragma target 3.0
   #pragma vertex ShadowVert
   #pragma fragment EmptyFrag
   #pragma multi_compile_instancing
   #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
   float3 _LightDirection,_LightPosition;
   float4 ShadowVert(Attributes input):SV_POSITION
   {
    UNITY_SETUP_INSTANCE_ID(input);
    float3 position=TransformObjectToWorld(input.positionOS.xyz);
    float3 normal=TransformObjectToWorldNormal(input.normalOS);
    #if _CASTING_PUNCTUAL_LIGHT_SHADOW
     float3 direction=normalize(_LightPosition-position);
    #else
     float3 direction=_LightDirection;
    #endif
    float4 clipPosition=TransformWorldToHClip(ApplyShadowBias(position,normal,direction));
    #if UNITY_REVERSED_Z
     clipPosition.z=min(clipPosition.z,clipPosition.w*UNITY_NEAR_CLIP_VALUE);
    #else
     clipPosition.z=max(clipPosition.z,clipPosition.w*UNITY_NEAR_CLIP_VALUE);
    #endif
    return clipPosition;
   }
   half4 EmptyFrag():SV_Target { return 0; }
   ENDHLSL
  }
  Pass
  {
   Name "DepthOnly"
   Tags { "LightMode"="DepthOnly" }
   ZWrite On ColorMask R Cull Back
   HLSLPROGRAM
   #pragma target 3.0
   #pragma vertex DepthVert
   #pragma fragment DepthFrag
   #pragma multi_compile_instancing
   float4 DepthVert(Attributes input):SV_POSITION { UNITY_SETUP_INSTANCE_ID(input);return TransformObjectToHClip(input.positionOS.xyz); }
   half DepthFrag(float4 position:SV_POSITION):SV_Target { return position.z; }
   ENDHLSL
  }
 }
 FallBack Off
}
