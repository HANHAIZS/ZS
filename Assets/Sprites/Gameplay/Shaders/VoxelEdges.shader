Shader "BeeHarvest/VoxelEdges"
{
 Properties
 {
  _BaseColor ("Original palette", Color) = (1,1,1,1)
  _Metallic ("Metallic", Range(0,1)) = 0
  _Smoothness ("Smoothness", Range(0,1)) = .4
  _EdgeWidth ("Black edge / half block", Range(.005,.15)) = .11
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
   half4 _BaseColor;
   half _Metallic, _Smoothness;
   float _EdgeWidth;
   float4 _VoxelCenter, _VoxelExtents;
  CBUFFER_END
  struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; UNITY_VERTEX_INPUT_INSTANCE_ID };
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
    output.boxPosition=(input.positionOS.xyz-_VoxelCenter.xyz)/max(_VoxelExtents.xyz,.0001);
    output.fog=ComputeFogFactor(position.positionCS.z);
    output.vertexLight=VertexLighting(position.positionWS,output.normalWS);
    return output;
   }
   half4 Frag(Varyings input):SV_Target
   {
    UNITY_SETUP_INSTANCE_ID(input);UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    InputData lighting=(InputData)0;
    lighting.positionWS=input.positionWS;
    lighting.normalWS=NormalizeNormalPerPixel(input.normalWS);
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
    surface.normalTS=half3(0,0,1);surface.occlusion=1;surface.alpha=1;
    half4 color=UniversalFragmentPBR(lighting,surface);
    // The second-nearest box boundary identifies the twelve true edges, never mesh diagonals.
    float3 d=abs(1-abs(input.boxPosition));
    float edge=min(max(d.x,d.y),min(max(d.y,d.z),max(d.z,d.x)));
    float aa=max(fwidth(edge),.0001);
    float face=smoothstep(_EdgeWidth-aa*.5,_EdgeWidth+aa*.5,edge);
    color.rgb=MixFog(color.rgb*face,input.fog);
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
