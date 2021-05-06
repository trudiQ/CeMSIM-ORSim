////////////////////////////////////////
// Generated with Better Shaders
//
// Auto-generated shader code, don't hand edit!
//
//   Unity Version: 2019.4.12f1
//   Render Pipeline: HDRP2019
//   Platform: WindowsEditor
////////////////////////////////////////


Shader "Paint in 3D/Alpha"
{
   Properties
   {
      
	[NoScaleOffset]_MainTex("Albedo (RGB) Alpha (A)", 2D) = "white" {}
	[NoScaleOffset][Normal]_BumpMap("Normal (RGBA)", 2D) = "bump" {}
	[NoScaleOffset]_MetallicGlossMap("Metallic (R) Occlusion (G) Smoothness (B)", 2D) = "white" {}
	[NoScaleOffset]_EmissionMap("Emission (RGB)", 2D) = "white" {}

	_Color("Color", Color) = (1,1,1,1)
	_BumpScale("Normal Map Strength", Range(0,5)) = 1
	_Metallic("Metallic", Range(0,1)) = 0
	_GlossMapScale("Smoothness", Range(0,1)) = 1
	_Emission("Emission", Color) = (0,0,0)
	_Tiling("Tiling", Float) = 1.0
	[Toggle(_USE_UV2)] _UseUV2("Use Second UV", Float) = 0

	[Header(OVERRIDE SETTINGS)]
	[Toggle(_USE_UV2_ALT)] _UseUV2Alt("	Use Second UV", Float) = 1
	[Toggle(_OVERRIDE_OPACITY)] _EnableOpacity("	Enable Opacity", Float) = 0
	[Toggle(_OVERRIDE_NORMAL)] _EnableNormal("	Enable Normal", Float) = 0
	[Toggle(_OVERRIDE_MOS)] _EnableMos("	Enable MOS", Float) = 0
	[Toggle(_OVERRIDE_EMISSION)] _EnableEmission("	Enable Emission", Float) = 0

	[Header(OVERRIDES)]
	[NoScaleOffset]_AlbedoTex("	Premultiplied Albedo (RGB) Weight (A)", 2D) = "black" {}
	[NoScaleOffset]_OpacityTex("	Premultiplied Opacity (R) Weight (A)", 2D) = "black" {}
	[NoScaleOffset]_NormalTex("	Premultiplied Normal (RG) Weight (A)", 2D) = "black" {}
	[NoScaleOffset]_MosTex("	Premultiplied Metallic (R) Occlusion (G) Smoothness (B) Weight (A)", 2D) = "black" {}
	[NoScaleOffset]_EmissionTex("	Premultiplied Emission (RGB) Weight (A)", 2D) = "black" {}





      [HideInInspector] _StencilRef("Vector1 ", Int) = 0
      [HideInInspector] _StencilWriteMask("Vector1 ", Int) = 3
      [HideInInspector] _StencilRefDepth("Vector1 ", Int) = 0
      [HideInInspector] _StencilWriteMaskDepth("Vector1 ", Int) = 32
      [HideInInspector] _StencilRefMV("Vector1 ", Int) = 128
      [HideInInspector] _StencilWriteMaskMV("Vector1 ", Int) = 128
      [HideInInspector] _StencilRefDistortionVec("Vector1 ", Int) = 64
      [HideInInspector] _StencilWriteMaskDistortionVec("Vector1 ", Int) = 64
      [HideInInspector] _StencilWriteMaskGBuffer("Vector1 ", Int) = 3
      [HideInInspector] _StencilRefGBuffer("Vector1 ", Int) = 2
      [HideInInspector] _ZTestGBuffer("Vector1 ", Int) = 4
      [HideInInspector] [ToggleUI] _RequireSplitLighting("Boolean", Float) = 0
      [HideInInspector] [ToggleUI] _ReceivesSSR("Boolean", Float) = 1
      [HideInInspector] _SurfaceType("Vector1 ", Float) = 0
      [HideInInspector] [ToggleUI] _ZWrite("Boolean", Float) = 0
      [HideInInspector] _TransparentSortPriority("Vector1 ", Int) = 0
      [HideInInspector] _ZTestDepthEqualForOpaque("Vector1 ", Int) = 4
      [HideInInspector] [Enum(UnityEngine.Rendering.CompareFunction)] _ZTestTransparent("Vector1", Float) = 4
      [HideInInspector] [ToggleUI] _TransparentBackfaceEnable("Boolean", Float) = 0
      [HideInInspector] [ToggleUI] _AlphaCutoffEnable("Boolean", Float) = 0
      [HideInInspector] [ToggleUI] _UseShadowThreshold("Boolean", Float) = 0
      [HideInInspector] _BlendMode("Float", Float) = 0
   }
   SubShader
   {
      Tags { "RenderPipeline"="HDRenderPipeline" "RenderPipeline" = "HDRenderPipeline" "RenderType"="Transparent" "Queue"="Transparent" }

      
              Pass
        {
            // based on HDLitPass.template
            Name "Forward"
            Tags { "LightMode" = "Forward" }

            Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
Cull Back
 ZTest LEqual
ZWrite Off
            
        
            
            // Stencil setup
        Stencil
        {
           WriteMask [_StencilWriteMask]
           Ref [_StencilRef]
           Comp Always
           Pass Replace
        }
        
            ColorMask [_ColorMaskTransparentVel] 1

            
            
            //-------------------------------------------------------------------------------------
            // End Render Modes
            //-------------------------------------------------------------------------------------
        
            HLSLPROGRAM
        
            #pragma target 4.5
            #pragma only_renderers d3d11 ps4 xboxone vulkan metal switch
            //#pragma enable_d3d11_debug_symbols
        
            #pragma multi_compile_instancing
        
            #pragma multi_compile_local _ _ALPHATEST_ON
        
            // #pragma multi_compile _ LOD_FADE_CROSSFADE
        
            //#pragma shader_feature _SURFACE_TYPE_TRANSPARENT
            //#pragma shader_feature_local _BLENDMODE_OFF _BLENDMODE_ALPHA _BLENDMODE_ADDITIVE _BLENDMODE_PRE_MULTIPLY
        
            //-------------------------------------------------------------------------------------
            // Variant Definitions (active field translations to HDRP defines)
            //-------------------------------------------------------------------------------------
            // #define _MATERIAL_FEATURE_SUBSURFACE_SCATTERING 1
            // #define _MATERIAL_FEATURE_TRANSMISSION 1
            // #define _MATERIAL_FEATURE_ANISOTROPY 1
            // #define _MATERIAL_FEATURE_IRIDESCENCE 1
            // #define _MATERIAL_FEATURE_SPECULAR_COLOR 1
            // #define _ENABLE_FOG_ON_TRANSPARENT 1
            // #define _AMBIENT_OCCLUSION 1
            // #define _SPECULAR_OCCLUSION_FROM_AO 1
            // #define _SPECULAR_OCCLUSION_FROM_AO_BENT_NORMAL 1
            // #define _SPECULAR_OCCLUSION_CUSTOM 1
            // #define _ENERGY_CONSERVING_SPECULAR 1
            // #define _ENABLE_GEOMETRIC_SPECULAR_AA 1
            // #define _HAS_REFRACTION 1
            // #define _REFRACTION_PLANE 1
            // #define _REFRACTION_SPHERE 1
            // #define _DISABLE_DECALS 1
            // #define _DISABLE_SSR 1
            // #define _ADD_PRECOMPUTED_VELOCITY
            // #define _WRITE_TRANSPARENT_MOTION_VECTOR 1
            // #define _DEPTHOFFSET_ON 1
            // #define _BLENDMODE_PRESERVE_SPECULAR_LIGHTING 1

            #define SHADERPASS SHADERPASS_FORWARD
            #define _PASSFORWARD 1
            
            
   #pragma shader_feature_local _ _OVERRIDE_OPACITY
   #pragma shader_feature_local _ _OVERRIDE_NORMAL
   #pragma shader_feature_local _ _OVERRIDE_MOS
   #pragma shader_feature_local _ _OVERRIDE_EMISSION


	#define _HAS_ALPHA_BLEND 1


   #define _HDRP 1
#define _BLENDMODE_ALPHA 1
#define _SURFACE_TYPE_TRANSPARENT 1


               #pragma vertex Vert
   #pragma fragment Frag

            
            #pragma multi_compile _ DEBUG_DISPLAY
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile DECALS_OFF DECALS_3RT DECALS_4RT
            #pragma multi_compile USE_FPTL_LIGHTLIST USE_CLUSTERED_LIGHTLIST
            #pragma multi_compile SHADOW_LOW SHADOW_MEDIUM SHADOW_HIGH
            #define REQUIRE_DEPTH_TEXTURE
            


                  // useful conversion functions to make surface shader code just work

      #define UNITY_DECLARE_TEX2D(name) TEXTURE2D(name); SAMPLER(sampler##name);
      #define UNITY_DECLARE_TEX2D_NOSAMPLER(name) TEXTURE2D(name);
      #define UNITY_DECLARE_TEX2DARRAY(name) TEXTURE2D_ARRAY(name); SAMPLER(sampler##name);
      #define UNITY_DECLARE_TEX2DARRAY_NOSAMPLER(tex) TEXTURE2D_ARRAY(tex);

      #define UNITY_SAMPLE_TEX2DARRAY(tex,coord)            SAMPLE_TEXTURE2D_ARRAY(tex, sampler##tex, coord.xy, coord.z)
      #define UNITY_SAMPLE_TEX2DARRAY_LOD(tex,coord,lod)    SAMPLE_TEXTURE2D_ARRAY_LOD(tex, sampler##tex, coord.xy, coord.z, lod)
      #define UNITY_SAMPLE_TEX2D(tex, coord)                SAMPLE_TEXTURE2D(tex, sampler##tex, coord)
      #define UNITY_SAMPLE_TEX2D_SAMPLER(tex, samp, coord)  SAMPLE_TEXTURE2D(tex, sampler##samp, coord)

      #define UNITY_SAMPLE_TEX2D_LOD(tex,coord, lod)   SAMPLE_TEXTURE2D_LOD(tex, sampler_##tex, coord, lod)
      #define UNITY_SAMPLE_TEX2D_SAMPLER_LOD(tex,samplertex,coord, lod) SAMPLE_TEXTURE2D_LOD (tex, sampler##samplertex,coord, lod)

      #if defined(UNITY_COMPILER_HLSL)
         #define UNITY_INITIALIZE_OUTPUT(type,name) name = (type)0;
      #else
         #define UNITY_INITIALIZE_OUTPUT(type,name)
      #endif

      #define sampler2D_float sampler2D
      #define sampler2D_half sampler2D

      #undef WorldNormalVector
      #define WorldNormalVector(data, normal) mul(normal, data.TBNMatrix)

      #define UnityObjectToWorldNormal(normal) mul(GetObjectToWorldMatrix(), normal)




// HDRP Adapter stuff


            // If we use subsurface scattering, enable output split lighting (for forward pass)
            #if defined(_MATERIAL_FEATURE_SUBSURFACE_SCATTERING) && !defined(_SURFACE_TYPE_TRANSPARENT)
            #define OUTPUT_SPLIT_LIGHTING
            #endif

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Version.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
        
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/NormalSurfaceGradient.hlsl"
        
            // define FragInputs structure
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"
            #if (SHADER_LIBRARY_VERSION_MAJOR >= 10)
               #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/Functions.hlsl"
            #endif


        

            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
        #ifdef DEBUG_DISPLAY
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Debug/DebugDisplay.hlsl"
        #endif
        
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
        
        #if (SHADERPASS == SHADERPASS_FORWARD)
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/Lighting.hlsl"
        
            #define HAS_LIGHTLOOP
        
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/LightLoop/LightLoopDef.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/Lit.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/LightLoop/LightLoop.hlsl"
        #else
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/Lit.hlsl"
        #endif
        
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/BuiltinUtilities.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/MaterialUtilities.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Decal/DecalUtilities.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/LitDecalData.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphFunctions.hlsl"
        
            // Used by SceneSelectionPass
            int _ObjectId;
            int _PassValue;
        
           
            // data across stages, stripped like the above.
            struct VertexToPixel
            {
               float4 pos : SV_POSITION;
               float3 worldPos : TEXCOORD0;
               float3 worldNormal : TEXCOORD1;
               float4 worldTangent : TEXCOORD2;
               float4 texcoord0 : TEXCCOORD3;
               float4 texcoord1 : TEXCCOORD4;
               float4 texcoord2 : TEXCCOORD5;
               // float4 texcoord3 : TEXCCOORD6;
               // float4 screenPos : TEXCOORD7;
               // float4 vertexColor : COLOR;

               // float4 extraV2F0 : TEXCOORD8;
               // float4 extraV2F1 : TEXCOORD9;
               // float4 extraV2F2 : TEXCOORD10;
               // float4 extraV2F3 : TEXCOORD11;
               // float4 extraV2F4 : TEXCOORD12;
               // float4 extraV2F5 : TEXCOORD13;
               // float4 extraV2F6 : TEXCOORD14;
               // float4 extraV2F7 : TEXCOORD15;

               #if UNITY_ANY_INSTANCING_ENABLED
                  uint instanceID : INSTANCEID_SEMANTIC;
               #endif // UNITY_ANY_INSTANCING_ENABLED

               UNITY_VERTEX_OUTPUT_STEREO
            };



            
            
            // data describing the user output of a pixel
            struct Surface
            {
               half3 Albedo;
               half Height;
               half3 Normal;
               half Smoothness;
               half3 Emission;
               half Metallic;
               half3 Specular;
               half Occlusion;
               half SpecularPower; // for simple lighting
               half Alpha;
               float outputDepth; // if written, SV_Depth semantic is used. ShaderData.clipPos.z is unused value
               // HDRP Only
               half SpecularOcclusion;
               half SubsurfaceMask;
               half Thickness;
               half CoatMask;
               half CoatSmoothness;
               half Anisotropy;
               half IridescenceMask;
               half IridescenceThickness;
               int DiffusionProfileHash;
            };

            // Data the user declares in blackboard blocks
            struct Blackboard
            {
                
                float blackboardDummyData;
            };

            // data the user might need, this will grow to be big. But easy to strip
            struct ShaderData
            {
               float4 clipPos; // SV_POSITION
               float3 localSpacePosition;
               float3 localSpaceNormal;
               float3 localSpaceTangent;
        
               float3 worldSpacePosition;
               float3 worldSpaceNormal;
               float3 worldSpaceTangent;
               float tangentSign;

               float3 worldSpaceViewDir;
               float3 tangentSpaceViewDir;

               float4 texcoord0;
               float4 texcoord1;
               float4 texcoord2;
               float4 texcoord3;

               float2 screenUV;
               float4 screenPos;

               float4 vertexColor;
               bool isFrontFace;

               float4 extraV2F0;
               float4 extraV2F1;
               float4 extraV2F2;
               float4 extraV2F3;
               float4 extraV2F4;
               float4 extraV2F5;
               float4 extraV2F6;
               float4 extraV2F7;

               float3x3 TBNMatrix;
               Blackboard blackboard;
            };

            struct VertexData
            {
               #if SHADER_TARGET > 30
               // uint vertexID : SV_VertexID;
               #endif
               float4 vertex : POSITION;
               float3 normal : NORMAL;
               float4 tangent : TANGENT;
               float4 texcoord0 : TEXCOORD0;
               float4 texcoord1 : TEXCOORD1;
               float4 texcoord2 : TEXCOORD2;
               // float4 texcoord3 : TEXCOORD3;
               // float4 vertexColor : COLOR;

               #if _HDRP && (_PASSMOTIONVECTOR || (_PASSFORWARD && defined(_WRITE_TRANSPARENT_MOTION_VECTOR)))
                  float3 previousPositionOS : TEXCOORD4; // Contain previous transform position (in case of skinning for example)
                  #if defined (_ADD_PRECOMPUTED_VELOCITY)
                     float3 precomputedVelocity    : TEXCOORD5; // Add Precomputed Velocity (Alembic computes velocities on runtime side).
                  #endif
               #endif

               UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct TessVertex 
            {
               float4 vertex : INTERNALTESSPOS;
               float3 normal : NORMAL;
               float4 tangent : TANGENT;
               float4 texcoord0 : TEXCOORD0;
               float4 texcoord1 : TEXCOORD1;
               float4 texcoord2 : TEXCOORD2;
               // float4 texcoord3 : TEXCOORD3;
               // float4 vertexColor : COLOR;

               
               // float4 extraV2F0 : TEXCOORD4;
               // float4 extraV2F1 : TEXCOORD5;
               // float4 extraV2F2 : TEXCOORD6;
               // float4 extraV2F3 : TEXCOORD7;
               // float4 extraV2F4 : TEXCOORD8;
               // float4 extraV2F5 : TEXCOORD9;
               // float4 extraV2F6 : TEXCOORD10;
               // float4 extraV2F7 : TEXCOORD11;

               #if _HDRP && (_PASSMOTIONVECTOR || (_PASSFORWARD && defined(_WRITE_TRANSPARENT_MOTION_VECTOR)))
                  float3 previousPositionOS : TEXCOORD12; // Contain previous transform position (in case of skinning for example)
                  #if defined (_ADD_PRECOMPUTED_VELOCITY)
                     float3 precomputedVelocity : TEXCOORD13;
                  #endif
               #endif

               UNITY_VERTEX_INPUT_INSTANCE_ID
               UNITY_VERTEX_OUTPUT_STEREO
            };

            struct ExtraV2F
            {
               float4 extraV2F0;
               float4 extraV2F1;
               float4 extraV2F2;
               float4 extraV2F3;
               float4 extraV2F4;
               float4 extraV2F5;
               float4 extraV2F6;
               float4 extraV2F7;
               Blackboard blackboard;
            };


            float3 WorldToTangentSpace(ShaderData d, float3 normal)
            {
               return mul(d.TBNMatrix, normal);
            }

            float3 TangentToWorldSpace(ShaderData d, float3 normal)
            {
               return mul(normal, d.TBNMatrix);
            }

            // in this case, make standard more like SRPs, because we can't fix
            // unity_WorldToObject in HDRP, since it already does macro-fu there

            #if _STANDARD
               float3 TransformWorldToObject(float3 p) { return mul(unity_WorldToObject, float4(p, 1)); };
               float3 TransformObjectToWorld(float3 p) { return mul(unity_ObjectToWorld, float4(p, 1)); };
               float4 TransformWorldToObject(float4 p) { return mul(unity_WorldToObject, p); };
               float4 TransformObjectToWorld(float4 p) { return mul(unity_ObjectToWorld, p); };
               float4x4 GetWorldToObjectMatrix() { return unity_WorldToObject; }
               float4x4 GetObjectToWorldMatrix() { return unity_ObjectToWorld; }
               #if (defined(SHADER_API_D3D11) || defined(SHADER_API_XBOXONE) || defined(UNITY_COMPILER_HLSLCC) || defined(SHADER_API_PSSL) || (SHADER_TARGET_SURFACE_ANALYSIS && !SHADER_TARGET_SURFACE_ANALYSIS_MOJOSHADER))
                 #define UNITY_SAMPLE_TEX2D_LOD(tex,coord, lod) tex.SampleLevel (sampler##tex,coord, lod)
                 #define UNITY_SAMPLE_TEX2D_SAMPLER_LOD(tex,samplertex,coord, lod) tex.SampleLevel (sampler##samplertex,coord, lod)
              #else
                 #define UNITY_SAMPLE_TEX2D_LOD(tex,coord,lod) tex2D (tex,coord,0,lod)
                 #define UNITY_SAMPLE_TEX2D_SAMPLER_LOD(tex,samplertex,coord,lod) tex2D (tex,coord,0,lod)
              #endif

               #undef GetObjectToWorldMatrix()
               #undef GetWorldToObjectMatrix()
               #undef GetWorldToViewMatrix()
               #undef UNITY_MATRIX_I_V
               #undef UNITY_MATRIX_P
               #undef GetWorldToHClipMatrix()
               #undef GetObjectToWorldMatrix()V
               #undef UNITY_MATRIX_T_MV
               #undef UNITY_MATRIX_IT_MV
               #undef GetObjectToWorldMatrix()VP

               #define GetObjectToWorldMatrix()     unity_ObjectToWorld
               #define GetWorldToObjectMatrix()   unity_WorldToObject
               #define GetWorldToViewMatrix()     unity_MatrixV
               #define UNITY_MATRIX_I_V   unity_MatrixInvV
               #define GetViewToHClipMatrix()     OptimizeProjectionMatrix(glstate_matrix_projection)
               #define GetWorldToHClipMatrix()    unity_MatrixVP
               #define GetObjectToWorldMatrix()V    mul(GetWorldToViewMatrix(), GetObjectToWorldMatrix())
               #define UNITY_MATRIX_T_MV  transpose(GetObjectToWorldMatrix()V)
               #define UNITY_MATRIX_IT_MV transpose(mul(GetWorldToObjectMatrix(), UNITY_MATRIX_I_V))
               #define GetObjectToWorldMatrix()VP   mul(GetWorldToHClipMatrix(), GetObjectToWorldMatrix())


            #endif

            float3 GetCameraWorldPosition()
            {
               #if _HDRP
                  return GetCameraRelativePositionWS(_WorldSpaceCameraPos);
               #else
                  return _WorldSpaceCameraPos;
               #endif
            }

            #if _GRABPASSUSED
               #if _STANDARD
                  TEXTURE2D(%GRABTEXTURE%);
                  SAMPLER(sampler_%GRABTEXTURE%);
               #endif

               half3 GetSceneColor(float2 uv)
               {
                  #if _STANDARD
                     return SAMPLE_TEXTURE2D(%GRABTEXTURE%, sampler_%GRABTEXTURE%, uv).rgb;
                  #else
                     return SHADERGRAPH_SAMPLE_SCENE_COLOR(uv);
                  #endif
               }
            #endif



            #if _STANDARD
               sampler2D _CameraDepthTexture;
               float GetSceneDepth(float2 uv) { return SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv); }
               float GetLinear01Depth(float2 uv) { return Linear01Depth(GetSceneDepth(uv)); }
               float GetLinearEyeDepth(float2 uv) { return LinearEyeDepth(GetSceneDepth(uv)); } 
            #else
               float GetSceneDepth(float2 uv) { return SHADERGRAPH_SAMPLE_SCENE_DEPTH(uv); }
               float GetLinear01Depth(float2 uv) { return Linear01Depth(GetSceneDepth(uv), _ZBufferParams); }
               float GetLinearEyeDepth(float2 uv) { return LinearEyeDepth(GetSceneDepth(uv), _ZBufferParams); } 
            #endif

            float3 GetWorldPositionFromDepthBuffer(float2 uv, float3 worldSpaceViewDir)
            {
               float eye = GetLinearEyeDepth(uv);
               float3 camView = mul((float3x3)GetObjectToWorldMatrix(), transpose(mul(GetWorldToObjectMatrix(), UNITY_MATRIX_I_V)) [2].xyz);

               float dt = dot(worldSpaceViewDir, camView);
               float3 div = worldSpaceViewDir/dt;
               float3 wpos = (eye * div) + GetCameraWorldPosition();
               return wpos;
            }

            #if _STANDARD
               sampler2D _CameraDepthNormalsTexture;
               float3 GetSceneNormal(float2 uv, float3 worldSpaceViewDir)
               {
                  float4 depthNorms = tex2D(_CameraDepthNormalsTexture, uv);
                  float3 norms = DecodeViewNormalStereo(depthNorms);
                  norms = mul((float3x3)GetWorldToViewMatrix(), norms) * 0.5 + 0.5;
                  return norms;
               }
            #elif _HDRP
               float3 GetSceneNormal(float2 uv, float3 worldSpaceViewDir)
               {
                  NormalData nd;
                  DecodeFromNormalBuffer(_ScreenSize.xy * uv, nd);
                  return nd.normalWS;
               }
            #elif _URP
               #if (SHADER_LIBRARY_VERSION_MAJOR >= 10)
                  #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"
               #endif

               float3 GetSceneNormal(float2 uv, float3 worldSpaceViewDir)
               {
                  #if (SHADER_LIBRARY_VERSION_MAJOR >= 10)
                     return SampleSceneNormals(uv);
                  #else
                     float3 wpos = GetWorldPositionFromDepthBuffer(uv, worldSpaceViewDir);
                     return normalize(-cross(ddx(wpos), ddy(wpos))) * 0.5 + 0.5;
                  #endif

                }
             #endif

             #if _HDRP

               half3 UnpackNormalmapRGorAG(half4 packednormal)
               {
                     // This do the trick
                  packednormal.x *= packednormal.w;

                  half3 normal;
                  normal.xy = packednormal.xy * 2 - 1;
                  normal.z = sqrt(1 - saturate(dot(normal.xy, normal.xy)));
                  return normal;
               }
               half3 UnpackNormal(half4 packednormal)
               {
                  #if defined(UNITY_NO_DXT5nm)
                     return packednormal.xyz * 2 - 1;
                  #else
                     return UnpackNormalmapRGorAG(packednormal);
                  #endif
               }
               #endif
               #if _HDRP || _URP

               half3 UnpackScaleNormal(half4 packednormal, half scale)
               {
                 #ifndef UNITY_NO_DXT5nm
                   // Unpack normal as DXT5nm (1, y, 1, x) or BC5 (x, y, 0, 1)
                   // Note neutral texture like "bump" is (0, 0, 1, 1) to work with both plain RGB normal and DXT5nm/BC5
                   packednormal.x *= packednormal.w;
                 #endif
                   half3 normal;
                   normal.xy = (packednormal.xy * 2 - 1) * scale;
                   normal.z = sqrt(1 - saturate(dot(normal.xy, normal.xy)));
                   return normal;
               }	

             #endif


            void GetSun(out float3 lightDir, out float3 color)
            {
               lightDir = float3(0.5, 0.5, 0);
               color = 1;
               #if _HDRP
                  if (_DirectionalLightCount > 0)
                  {
                     DirectionalLightData light = _DirectionalLightDatas[0];
                     lightDir = -light.forward.xyz;
                     color = light.color;
                  }
               #elif _STANDARD
			         lightDir = normalize(_WorldSpaceLightPos0.xyz);
                  color = _LightColor0.rgb;
               #elif _URP
	               Light light = GetMainLight();
	               lightDir = light.direction;
	               color = light.color;
               #endif
            }




            CBUFFER_START(UnityPerMaterial)
               float _StencilRef;
               float _StencilWriteMask;
               float _StencilRefDepth;
               float _StencilWriteMaskDepth;
               float _StencilRefMV;
               float _StencilWriteMaskMV;
               float _StencilRefDistortionVec;
               float _StencilWriteMaskDistortionVec;
               float _StencilWriteMaskGBuffer;
               float _StencilRefGBuffer;
               float _ZTestGBuffer;
               float _RequireSplitLighting;
               float _ReceivesSSR;
               float _ZWrite;
               float _TransparentSortPriority;
               float _ZTestDepthEqualForOpaque;
               float _ZTestTransparent;
               float _TransparentBackfaceEnable;
               float _AlphaCutoffEnable;
               float _UseShadowThreshold;

               
	float4 _Color;
	float  _BumpScale;
	float  _Metallic;
	float  _GlossMapScale;
	float3 _Emission;
	float  _Tiling;
	float  _UseUV2;
	float  _UseUV2Alt;





            CBUFFER_END

            

            

            #ifdef unity_WorldToObject
#undef unity_WorldToObject
#endif
#ifdef unity_ObjectToWorld
#undef unity_ObjectToWorld
#endif
#define unity_ObjectToWorld GetObjectToWorldMatrix()
#define unity_WorldToObject GetWorldToObjectMatrix()

	TEXTURE2D(_MainTex);
	SAMPLER(sampler_MainTex);
	TEXTURE2D(_BumpMap);
	SAMPLER(sampler_BumpMap);
	TEXTURE2D(_MetallicGlossMap);
	SAMPLER(sampler_MetallicGlossMap);
	TEXTURE2D(_EmissionMap);
	SAMPLER(sampler_EmissionMap);

	TEXTURE2D(_AlbedoTex);
	SAMPLER(sampler_AlbedoTex);
	TEXTURE2D(_OpacityTex);
	SAMPLER(sampler_OpacityTex);
	TEXTURE2D(_NormalTex);
	SAMPLER(sampler_NormalTex);
	TEXTURE2D(_EmissionTex);
	SAMPLER(sampler_EmissionTex);
	TEXTURE2D(_MosTex);
	SAMPLER(sampler_MosTex);

	void Ext_ModifyVertex0(inout VertexData v, inout ExtraV2F d)
	{
		float4 first  = lerp(v.texcoord0, v.texcoord1, _UseUV2);
		float4 second = lerp(v.texcoord0, v.texcoord1, _UseUV2Alt);

		v.texcoord0 = first * _Tiling;
		v.texcoord1 = second;
	}

	void Ext_SurfaceFunction0(inout Surface o, ShaderData d)
	{
		float4 texMain = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, d.texcoord0);
		float4 gloss   = SAMPLE_TEXTURE2D(_MetallicGlossMap, sampler_MetallicGlossMap, d.texcoord0);
		float4 bump    = SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, d.texcoord0);
		float4 glow    = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, d.texcoord0);
	#if !_HAS_ALPHA_BLEND
		clip(texMain.a * _Color.a - 0.5f);
	#endif
		o.Albedo     = texMain.rgb * _Color.rgb;
		o.Normal     = UnpackScaleNormal(bump, _BumpScale);
		o.Metallic   = gloss.r * _Metallic;
		o.Occlusion  = gloss.g;
		o.Smoothness = gloss.b * _GlossMapScale;
		o.Emission   = glow.rgb * _Emission;
		o.Alpha      = texMain.a * _Color.a;

		// Override albedo?
		float4 albedo = SAMPLE_TEXTURE2D(_AlbedoTex, sampler_AlbedoTex, d.texcoord1);
		o.Albedo = (1.0f - albedo.a) * o.Albedo + albedo.rgb;

	#if _OVERRIDE_OPACITY
		float4 opacity = SAMPLE_TEXTURE2D(_OpacityTex, sampler_OpacityTex, d.texcoord1);
		o.Alpha = (1.0f - opacity.a) * o.Alpha + opacity.r;
	#endif

	#if _OVERRIDE_NORMAL
		float4 normal = SAMPLE_TEXTURE2D(_NormalTex, sampler_NormalTex, d.texcoord1);
		o.Normal = (1.0f - normal.a) * o.Normal + normal.r;
	#endif
	
	#if _OVERRIDE_MOS
		float4 mos = SAMPLE_TEXTURE2D(_MosTex, sampler_MosTex, d.texcoord1);
		o.Metallic   = (1.0f - mos.a) * o.Metallic + mos.r;
		o.Occlusion  = (1.0f - mos.a) * o.Metallic + mos.g;
		o.Smoothness = (1.0f - mos.a) * o.Metallic + mos.b;
	#endif
	
	#if _OVERRIDE_EMISSION
		float4 emission = SAMPLE_TEXTURE2D(_EmissionTex, sampler_EmissionTex, d.texcoord1);
		o.Emission = (1.0f - emission.a) * o.Emission + emission.rgb;
	#endif
	}





        
            void ChainSurfaceFunction(inout Surface l, inout ShaderData d)
            {
                  Ext_SurfaceFunction0(l, d);
                 // Ext_SurfaceFunction1(l, d);
                 // Ext_SurfaceFunction2(l, d);
                 // Ext_SurfaceFunction3(l, d);
                 // Ext_SurfaceFunction4(l, d);
                 // Ext_SurfaceFunction5(l, d);
                 // Ext_SurfaceFunction6(l, d);
                 // Ext_SurfaceFunction7(l, d);
                 // Ext_SurfaceFunction8(l, d);
                 // Ext_SurfaceFunction9(l, d);
		           // Ext_SurfaceFunction10(l, d);
                 // Ext_SurfaceFunction11(l, d);
                 // Ext_SurfaceFunction12(l, d);
                 // Ext_SurfaceFunction13(l, d);
                 // Ext_SurfaceFunction14(l, d);
                 // Ext_SurfaceFunction15(l, d);
                 // Ext_SurfaceFunction16(l, d);
                 // Ext_SurfaceFunction17(l, d);
                 // Ext_SurfaceFunction18(l, d);
		           // Ext_SurfaceFunction19(l, d);
            }

            void ChainModifyVertex(inout VertexData v, inout VertexToPixel v2p)
            {
                 ExtraV2F d;
                 ZERO_INITIALIZE(ExtraV2F, d);
                 ZERO_INITIALIZE(Blackboard, d.blackboard);

                   Ext_ModifyVertex0(v, d);
                 // Ext_ModifyVertex1(v, d);
                 // Ext_ModifyVertex2(v, d);
                 // Ext_ModifyVertex3(v, d);
                 // Ext_ModifyVertex4(v, d);
                 // Ext_ModifyVertex5(v, d);
                 // Ext_ModifyVertex6(v, d);
                 // Ext_ModifyVertex7(v, d);
                 // Ext_ModifyVertex8(v, d);
                 // Ext_ModifyVertex9(v, d);
                 // Ext_ModifyVertex10(v, d);
                 // Ext_ModifyVertex11(v, d);
                 // Ext_ModifyVertex12(v, d);
                 // Ext_ModifyVertex13(v, d);
                 // Ext_ModifyVertex14(v, d);
                 // Ext_ModifyVertex15(v, d);
                 // Ext_ModifyVertex16(v, d);
                 // Ext_ModifyVertex17(v, d);
                 // Ext_ModifyVertex18(v, d);
                 // Ext_ModifyVertex19(v, d);
		
                 // v2p.extraV2F0 = d.extraV2F0;
                 // v2p.extraV2F1 = d.extraV2F1;
                 // v2p.extraV2F2 = d.extraV2F2;
                 // v2p.extraV2F3 = d.extraV2F3;
                 // v2p.extraV2F4 = d.extraV2F4;
                 // v2p.extraV2F5 = d.extraV2F5;
                 // v2p.extraV2F6 = d.extraV2F6;
                 // v2p.extraV2F7 = d.extraV2F7;
            }

            void ChainModifyTessellatedVertex(inout VertexData v, inout VertexToPixel v2p)
            {
               ExtraV2F d;
               ZERO_INITIALIZE(ExtraV2F, d);
               ZERO_INITIALIZE(Blackboard, d.blackboard);
               // d.extraV2F0 = v2p.extraV2F0;
               // d.extraV2F1 = v2p.extraV2F1;
               // d.extraV2F2 = v2p.extraV2F2;
               // d.extraV2F3 = v2p.extraV2F3;
               // d.extraV2F0 = v2p.extraV2F4;
               // d.extraV2F1 = v2p.extraV2F5;
               // d.extraV2F2 = v2p.extraV2F6;
               // d.extraV2F3 = v2p.extraV2F7;

               // Ext_ModifyTessellatedVertex0(v, d);
               // Ext_ModifyTessellatedVertex1(v, d);
               // Ext_ModifyTessellatedVertex2(v, d);
               // Ext_ModifyTessellatedVertex3(v, d);
               // Ext_ModifyTessellatedVertex4(v, d);
               // Ext_ModifyTessellatedVertex5(v, d);
               // Ext_ModifyTessellatedVertex6(v, d);
               // Ext_ModifyTessellatedVertex7(v, d);
               // Ext_ModifyTessellatedVertex8(v, d);
               // Ext_ModifyTessellatedVertex9(v, d);
               // Ext_ModifyTessellatedVertex10(v, d);
               // Ext_ModifyTessellatedVertex11(v, d);
               // Ext_ModifyTessellatedVertex12(v, d);
               // Ext_ModifyTessellatedVertex13(v, d);
               // Ext_ModifyTessellatedVertex14(v, d);
               // Ext_ModifyTessellatedVertex15(v, d);
               // Ext_ModifyTessellatedVertex16(v, d);
               // Ext_ModifyTessellatedVertex17(v, d);
               // Ext_ModifyTessellatedVertex18(v, d);
               // Ext_ModifyTessellatedVertex19(v, d);

               // v2p.extraV2F0 = d.extraV2F0;
               // v2p.extraV2F1 = d.extraV2F1;
               // v2p.extraV2F2 = d.extraV2F2;
               // v2p.extraV2F3 = d.extraV2F3;
               // v2p.extraV2F0 = d.extraV2F4;
               // v2p.extraV2F1 = d.extraV2F5;
               // v2p.extraV2F2 = d.extraV2F6;
               // v2p.extraV2F3 = d.extraV2F7;
            }

            void ChainFinalColorForward(inout Surface l, inout ShaderData d, inout half4 color)
            {
               //   Ext_FinalColorForward0(l, d, color);
               //   Ext_FinalColorForward1(l, d, color);
               //   Ext_FinalColorForward2(l, d, color);
               //   Ext_FinalColorForward3(l, d, color);
               //   Ext_FinalColorForward4(l, d, color);
               //   Ext_FinalColorForward5(l, d, color);
               //   Ext_FinalColorForward6(l, d, color);
               //   Ext_FinalColorForward7(l, d, color);
               //   Ext_FinalColorForward8(l, d, color);
               //   Ext_FinalColorForward9(l, d, color);
               //  Ext_FinalColorForward10(l, d, color);
               //  Ext_FinalColorForward11(l, d, color);
               //  Ext_FinalColorForward12(l, d, color);
               //  Ext_FinalColorForward13(l, d, color);
               //  Ext_FinalColorForward14(l, d, color);
               //  Ext_FinalColorForward15(l, d, color);
               //  Ext_FinalColorForward16(l, d, color);
               //  Ext_FinalColorForward17(l, d, color);
               //  Ext_FinalColorForward18(l, d, color);
               //  Ext_FinalColorForward19(l, d, color);
            }

            void ChainFinalGBufferStandard(inout Surface s, inout ShaderData d, inout half4 GBuffer0, inout half4 GBuffer1, inout half4 GBuffer2, inout half4 outEmission, inout half4 outShadowMask)
            {
               //   Ext_FinalGBufferStandard0(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
               //   Ext_FinalGBufferStandard1(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
               //   Ext_FinalGBufferStandard2(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
               //   Ext_FinalGBufferStandard3(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
               //   Ext_FinalGBufferStandard4(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
               //   Ext_FinalGBufferStandard5(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
               //   Ext_FinalGBufferStandard6(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
               //   Ext_FinalGBufferStandard7(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
               //   Ext_FinalGBufferStandard8(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
               //   Ext_FinalGBufferStandard9(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
               //  Ext_FinalGBufferStandard10(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
               //  Ext_FinalGBufferStandard11(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
               //  Ext_FinalGBufferStandard12(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
               //  Ext_FinalGBufferStandard13(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
               //  Ext_FinalGBufferStandard14(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
               //  Ext_FinalGBufferStandard15(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
               //  Ext_FinalGBufferStandard16(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
               //  Ext_FinalGBufferStandard17(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
               //  Ext_FinalGBufferStandard18(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
               //  Ext_FinalGBufferStandard19(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
            }



            

         ShaderData CreateShaderData(VertexToPixel i
                  #if NEED_FACING
                     , bool facing
                  #endif
         )
         {
            ShaderData d = (ShaderData)0;
            d.clipPos = i.pos;
            d.worldSpacePosition = i.worldPos;

            d.worldSpaceNormal = normalize(i.worldNormal);
            d.worldSpaceTangent = normalize(i.worldTangent.xyz);
            d.tangentSign = i.worldTangent.w;
            float3 bitangent = cross(i.worldTangent.xyz, i.worldNormal) * d.tangentSign * -1;
            

            d.TBNMatrix = float3x3(d.worldSpaceTangent, bitangent, d.worldSpaceNormal);
            d.worldSpaceViewDir = normalize(_WorldSpaceCameraPos - i.worldPos);

            d.tangentSpaceViewDir = mul(d.TBNMatrix, d.worldSpaceViewDir);
             d.texcoord0 = i.texcoord0;
             d.texcoord1 = i.texcoord1;
            // d.texcoord2 = i.texcoord2;
            // d.texcoord3 = i.texcoord3;
            // d.isFrontFace = facing;
            // d.vertexColor = i.vertexColor;

            // these rarely get used, so we back transform them. Usually will be stripped.
            #if _HDRP
                // d.localSpacePosition = mul(unity_WorldToObject, float4(GetCameraRelativePositionWS(i.worldPos), 1)).xyz;
            #else
                // d.localSpacePosition = mul(unity_WorldToObject, float4(i.worldPos, 1)).xyz;
            #endif
            // d.localSpaceNormal = normalize(mul(unity_WorldToObject, float4(i.worldNormal, 1)).xyz);
            // d.localSpaceTangent = normalize(mul(unity_WorldToObject, float4(i.worldTangent.xyz, 1)).xyz);

            // d.screenPos = i.screenPos;
            // d.screenUV = (i.screenPos.xy / i.screenPos.w);

            // d.extraV2F0 = i.extraV2F0;
            // d.extraV2F1 = i.extraV2F1;
            // d.extraV2F2 = i.extraV2F2;
            // d.extraV2F3 = i.extraV2F3;
            // d.extraV2F4 = i.extraV2F4;
            // d.extraV2F5 = i.extraV2F5;
            // d.extraV2F6 = i.extraV2F6;
            // d.extraV2F7 = i.extraV2F7;

            return d;
         }
         

            

struct VaryingsToPS
{
   VertexToPixel vmesh;
   #ifdef VARYINGS_NEED_PASS
      VaryingsPassToPS vpass;
   #endif
};

struct PackedVaryingsToPS
{
   #ifdef VARYINGS_NEED_PASS
      PackedVaryingsPassToPS vpass;
   #endif
   VertexToPixel vmesh;

   UNITY_VERTEX_OUTPUT_STEREO
};

PackedVaryingsToPS PackVaryingsToPS(VaryingsToPS input)
{
   PackedVaryingsToPS output = (PackedVaryingsToPS)0;
   output.vmesh = input.vmesh;
   #ifdef VARYINGS_NEED_PASS
      output.vpass = PackVaryingsPassToPS(input.vpass);
   #endif

   UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
   return output;
}




VertexToPixel VertMesh(VertexData input)
{
    VertexToPixel output = (VertexToPixel)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);

    
    ChainModifyVertex(input, output);


    // This return the camera relative position (if enable)
    float3 positionRWS = TransformObjectToWorld(input.vertex.xyz);
    float3 normalWS = TransformObjectToWorldNormal(input.normal);
    float4 tangentWS = float4(TransformObjectToWorldDir(input.tangent.xyz), input.tangent.w);


    output.worldPos = GetAbsolutePositionWS(positionRWS);
    output.pos = TransformWorldToHClip(positionRWS);
    output.worldNormal = normalWS;
    output.worldTangent = tangentWS;


    output.texcoord0 = input.texcoord0;
    output.texcoord1 = input.texcoord1;
    output.texcoord2 = input.texcoord2;
    // output.texcoord3 = input.texcoord3;
    // output.vertexColor = input.vertexColor;
    // output.screenPos = ComputeScreenPos(output.pos, _ProjectionParams.x);

    return output;
}


#if (SHADERPASS == SHADERPASS_DBUFFER_MESH)
void MeshDecalsPositionZBias(inout VaryingsToPS input)
{
#if defined(UNITY_REVERSED_Z)
    input.vmesh.pos.z -= _DecalMeshDepthBias;
#else
    input.vmesh.pos.z += _DecalMeshDepthBias;
#endif
}
#endif


#if (SHADERPASS == SHADERPASS_LIGHT_TRANSPORT)

// This was not in constant buffer in original unity, so keep outiside. But should be in as ShaderRenderPass frequency
float unity_OneOverOutputBoost;
float unity_MaxOutputValue;

CBUFFER_START(UnityMetaPass)
// x = use uv1 as raster position
// y = use uv2 as raster position
bool4 unity_MetaVertexControl;

// x = return albedo
// y = return normal
bool4 unity_MetaFragmentControl;
CBUFFER_END

PackedVaryingsToPS Vert(VertexData inputMesh)
{
    VaryingsToPS output = (VaryingsToPS)0;
    output.vmesh = (VertexToPixel)0;

    UNITY_SETUP_INSTANCE_ID(inputMesh);
    UNITY_TRANSFER_INSTANCE_ID(inputMesh, output.vmesh);

    // Output UV coordinate in vertex shader
    float2 uv = float2(0.0, 0.0);

    if (unity_MetaVertexControl.x)
    {
        uv = inputMesh.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
    }
    else if (unity_MetaVertexControl.y)
    {
        uv = inputMesh.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
    }

    // OpenGL right now needs to actually use the incoming vertex position
    // so we create a fake dependency on it here that haven't any impact.
    output.vmesh.pos = float4(uv * 2.0 - 1.0, inputMesh.vertex.z > 0 ? 1.0e-4 : 0.0, 1.0);

#ifdef VARYINGS_NEED_POSITION_WS
    output.vmesh.worldPos = TransformObjectToWorld(inputMesh.vertex.xyz);
#endif

#ifdef VARYINGS_NEED_TANGENT_TO_WORLD
    // Normal is required for triplanar mapping
    output.vmesh.worldNormal = TransformObjectToWorldNormal(inputMesh.normal);
    // Not required but assign to silent compiler warning
    output.vmesh.worldTangent = float4(1.0, 0.0, 0.0, 0.0);
#endif

    output.vmesh.texcoord0 = inputMesh.texcoord0;
    output.vmesh.texcoord1 = inputMesh.texcoord1;
    output.vmesh.texcoord2 = inputMesh.texcoord2;
    // output.vmesh.texCoord3 = inputMesh.texcoord3;
    // output.vmesh.vertexColor = inputMesh.vertexColor;

    return PackVaryingsToPS(output);
}
#else

PackedVaryingsToPS Vert(VertexData inputMesh)
{
    VaryingsToPS varyingsType;
    varyingsType.vmesh = VertMesh(inputMesh);
    #if (SHADERPASS == SHADERPASS_DBUFFER_MESH)
       MeshDecalsPositionZBias(varyingsType);
    #endif
    return PackVaryingsToPS(varyingsType);
}

#endif



            

            
                FragInputs BuildFragInputs(VertexToPixel input)
                {
                    UNITY_SETUP_INSTANCE_ID(input);
                    FragInputs output;
                    ZERO_INITIALIZE(FragInputs, output);
            
                    // Init to some default value to make the computer quiet (else it output 'divide by zero' warning even if value is not used).
                    // TODO: this is a really poor workaround, but the variable is used in a bunch of places
                    // to compute normals which are then passed on elsewhere to compute other values...
                    output.tangentToWorld = k_identity3x3;
                    output.positionSS = input.pos;       // input.positionCS is SV_Position
            
                    output.positionRWS = input.worldPos;
                    output.tangentToWorld = BuildTangentToWorld(input.worldTangent, input.worldNormal);
                    output.texCoord0 = input.texcoord0;
                    output.texCoord1 = input.texcoord1;
                    output.texCoord2 = input.texcoord2;
            
                    return output;
                }
            
               void BuildSurfaceData(FragInputs fragInputs, inout Surface surfaceDescription, float3 V, PositionInputs posInput, out SurfaceData surfaceData, out float3 bentNormalWS)
               {
                   // setup defaults -- these are used if the graph doesn't output a value
                   ZERO_INITIALIZE(SurfaceData, surfaceData);
        
                   // specularOcclusion need to be init ahead of decal to quiet the compiler that modify the SurfaceData struct
                   // however specularOcclusion can come from the graph, so need to be init here so it can be override.
                   surfaceData.specularOcclusion = 1.0;
        
                   // copy across graph values, if defined
                   surfaceData.baseColor =                 surfaceDescription.Albedo;
                   surfaceData.perceptualSmoothness =      surfaceDescription.Smoothness;
                   surfaceData.ambientOcclusion =          surfaceDescription.Occlusion;
                   surfaceData.specularOcclusion =         surfaceDescription.SpecularOcclusion;
                   surfaceData.metallic =                  surfaceDescription.Metallic;
                   surfaceData.subsurfaceMask =            surfaceDescription.SubsurfaceMask;
                   surfaceData.thickness =                 surfaceDescription.Thickness;
                   surfaceData.diffusionProfileHash =      asuint(surfaceDescription.DiffusionProfileHash);
                   #if _USESPECULAR
                      surfaceData.specularColor =             surfaceDescription.Specular;
                   #endif
                   surfaceData.coatMask =                  surfaceDescription.CoatMask;
                   surfaceData.anisotropy =                surfaceDescription.Anisotropy;
                   surfaceData.iridescenceMask =           surfaceDescription.IridescenceMask;
                   surfaceData.iridescenceThickness =      surfaceDescription.IridescenceThickness;
        
           #ifdef _HAS_REFRACTION
                   if (_EnableSSRefraction)
                   {
                       // surfaceData.ior =                       surfaceDescription.RefractionIndex;
                       // surfaceData.transmittanceColor =        surfaceDescription.RefractionColor;
                       // surfaceData.atDistance =                surfaceDescription.RefractionDistance;
        
                       surfaceData.transmittanceMask = (1.0 - surfaceDescription.Alpha);
                       surfaceDescription.Alpha = 1.0;
                   }
                   else
                   {
                       surfaceData.ior = 1.0;
                       surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
                       surfaceData.atDistance = 1.0;
                       surfaceData.transmittanceMask = 0.0;
                       surfaceDescription.Alpha = 1.0;
                   }
           #else
                   surfaceData.ior = 1.0;
                   surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
                   surfaceData.atDistance = 1.0;
                   surfaceData.transmittanceMask = 0.0;
           #endif
                
                   // These static material feature allow compile time optimization
                   surfaceData.materialFeatures = MATERIALFEATUREFLAGS_LIT_STANDARD;
           #ifdef _MATERIAL_FEATURE_SUBSURFACE_SCATTERING
                   surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SUBSURFACE_SCATTERING;
           #endif
           #ifdef _MATERIAL_FEATURE_TRANSMISSION
                   surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_TRANSMISSION;
           #endif
           #ifdef _MATERIAL_FEATURE_ANISOTROPY
                   surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_ANISOTROPY;
           #endif
                   // surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_CLEAR_COAT;
        
           #ifdef _MATERIAL_FEATURE_IRIDESCENCE
                   surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_IRIDESCENCE;
           #endif
           #ifdef _MATERIAL_FEATURE_SPECULAR_COLOR
                   surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SPECULAR_COLOR;
           #endif
        
           #if defined (_MATERIAL_FEATURE_SPECULAR_COLOR) && defined (_ENERGY_CONSERVING_SPECULAR)
                   // Require to have setup baseColor
                   // Reproduce the energy conservation done in legacy Unity. Not ideal but better for compatibility and users can unchek it
                   surfaceData.baseColor *= (1.0 - Max3(surfaceData.specularColor.r, surfaceData.specularColor.g, surfaceData.specularColor.b));
           #endif

        
                   // tangent-space normal
                   float3 normalTS = float3(0.0f, 0.0f, 1.0f);
                   normalTS = surfaceDescription.Normal;
        
                   // compute world space normal
                   #if !_WORLDSPACENORMAL
                      surfaceData.normalWS = mul(normalTS, fragInputs.tangentToWorld);
                   #else
                      surfaceData.normalWS = normalTS;  
                   #endif
                   surfaceData.geomNormalWS = fragInputs.tangentToWorld[2];
        
                   surfaceData.tangentWS = normalize(fragInputs.tangentToWorld[0].xyz);    // The tangent is not normalize in tangentToWorld for mikkt. TODO: Check if it expected that we normalize with Morten. Tag: SURFACE_GRADIENT
                   // surfaceData.tangentWS = TransformTangentToWorld(surfaceDescription.Tangent, fragInputs.tangentToWorld);
        
           #if HAVE_DECALS
                   if (_EnableDecals)
                   {
                       #if VERSION_GREATER_EQUAL(10,2)
                          DecalSurfaceData decalSurfaceData = GetDecalSurfaceData(posInput,  surfaceData.geomNormalWS, surfaceDescription.Alpha);
                          ApplyDecalToSurfaceData(decalSurfaceData,  surfaceData.geomNormalWS, surfaceData);
                       #else
                          DecalSurfaceData decalSurfaceData = GetDecalSurfaceData(posInput, surfaceDescription.Alpha);
                          ApplyDecalToSurfaceData(decalSurfaceData, surfaceData);
                       #endif
                   }
           #endif
        
                   bentNormalWS = surfaceData.normalWS;
               
                   surfaceData.tangentWS = Orthonormalize(surfaceData.tangentWS, surfaceData.normalWS);
        
        
                   // By default we use the ambient occlusion with Tri-ace trick (apply outside) for specular occlusion.
                   // If user provide bent normal then we process a better term
           #if defined(_SPECULAR_OCCLUSION_CUSTOM)
                   // Just use the value passed through via the slot (not active otherwise)
           #elif defined(_SPECULAR_OCCLUSION_FROM_AO_BENT_NORMAL)
                   // If we have bent normal and ambient occlusion, process a specular occlusion
                   surfaceData.specularOcclusion = GetSpecularOcclusionFromBentAO(V, bentNormalWS, surfaceData.normalWS, surfaceData.ambientOcclusion, PerceptualSmoothnessToPerceptualRoughness(surfaceData.perceptualSmoothness));
           #elif defined(_AMBIENT_OCCLUSION) && defined(_SPECULAR_OCCLUSION_FROM_AO)
                   surfaceData.specularOcclusion = GetSpecularOcclusionFromAmbientOcclusion(ClampNdotV(dot(surfaceData.normalWS, V)), surfaceData.ambientOcclusion, PerceptualSmoothnessToRoughness(surfaceData.perceptualSmoothness));
           #endif
        
           #ifdef _ENABLE_GEOMETRIC_SPECULAR_AA
                   surfaceData.perceptualSmoothness = GeometricNormalFiltering(surfaceData.perceptualSmoothness, fragInputs.tangentToWorld[2], surfaceDescription.SpecularAAScreenSpaceVariance, surfaceDescription.SpecularAAThreshold);
           #endif
        
           #ifdef DEBUG_DISPLAY
                   if (_DebugMipMapMode != DEBUGMIPMAPMODE_NONE)
                   {
                       // TODO: need to update mip info
                       surfaceData.metallic = 0;
                   }
        
                   // We need to call ApplyDebugToSurfaceData after filling the surfarcedata and before filling builtinData
                   // as it can modify attribute use for static lighting
                   ApplyDebugToSurfaceData(fragInputs.tangentToWorld, surfaceData);
           #endif
               }
        
               void GetSurfaceAndBuiltinData(VertexToPixel m2ps, FragInputs fragInputs, float3 V, inout PositionInputs posInput,
                     out SurfaceData surfaceData, out BuiltinData builtinData, inout Surface l, inout ShaderData d
                     #if NEED_FACING
                        , facing
                     #endif
                  )
               {
                 // Removed since crossfade does not work, probably needs extra material setup.   
                 //#ifdef LOD_FADE_CROSSFADE // enable dithering LOD transition if user select CrossFade transition in LOD group
                 //    uint3 fadeMaskSeed = asuint((int3)(V * _ScreenSize.xyx)); // Quantize V to _ScreenSize values
                 //    LODDitheringTransition(fadeMaskSeed, unity_LODFade.x);
                 //#endif
        
                 d = CreateShaderData(m2ps
                  #if NEED_FACING
                    , facing
                  #endif
                 );

                 

                 l = (Surface)0;

                 l.Albedo = half3(0.5, 0.5, 0.5);
                 l.Normal = float3(0,0,1);
                 l.Occlusion = 1;
                 l.Alpha = 1;

                 #ifdef _DEPTHOFFSET_ON
                    l.outputDepth = posInput.deviceDepth;
                 #endif

                 ChainSurfaceFunction(l, d);

                 #ifdef _DEPTHOFFSET_ON
                    posInput.deviceDepth = l.outputDepth;
                 #endif

                 #if _UNLIT
                     //l.Emission = l.Albedo;
                     //l.Albedo = 0;
                     l.Normal = half3(0,0,1);
                     l.Occlusion = 1;
                     l.Metallic = 0;
                     l.Specular = 0;
                 #endif

                 surfaceData.geomNormalWS = d.worldSpaceNormal;
                 surfaceData.tangentWS = d.worldSpaceTangent;
                 fragInputs.tangentToWorld = d.TBNMatrix;

                 float3 bentNormalWS;
                 BuildSurfaceData(fragInputs, l, V, posInput, surfaceData, bentNormalWS);

                 

                 InitBuiltinData(posInput, l.Alpha, bentNormalWS, -d.worldSpaceNormal, fragInputs.texCoord1, fragInputs.texCoord2, builtinData);

                 builtinData.emissiveColor = l.Emission;
        
        
                 #if (SHADERPASS == SHADERPASS_DISTORTION)
                     //builtinData.distortion = surfaceDescription.Distortion;
                     //builtinData.distortionBlur = surfaceDescription.DistortionBlur;
                     builtinData.distortion = float2(0.0, 0.0);
                     builtinData.distortionBlur = 0.0;
                 #else
                     builtinData.distortion = float2(0.0, 0.0);
                     builtinData.distortionBlur = 0.0;
                 #endif
        
                   PostInitBuiltinData(V, posInput, surfaceData, builtinData);
               }
            
                      
          void Frag(PackedVaryingsToPS packedInput,
          #ifdef OUTPUT_SPLIT_LIGHTING
              out float4 outColor : SV_Target0,  // outSpecularLighting
              out float4 outDiffuseLighting : SV_Target1,
              OUTPUT_SSSBUFFER(outSSSBuffer)
          #else
              out float4 outColor : SV_Target0
          #ifdef _WRITE_TRANSPARENT_MOTION_VECTOR
              , out float4 outMotionVec : SV_Target1
          #endif // _WRITE_TRANSPARENT_MOTION_VECTOR
          #endif // OUTPUT_SPLIT_LIGHTING
          #ifdef _DEPTHOFFSET_ON
              , out float outputDepth : SV_Depth
          #endif
          #if NEED_FACING
            , bool facing : SV_IsFrontFace
          #endif
          )
          {
          #ifdef _WRITE_TRANSPARENT_MOTION_VECTOR
              // Init outMotionVector here to solve compiler warning (potentially unitialized variable)
              // It is init to the value of forceNoMotion (with 2.0)
              outMotionVec = float4(2.0, 0.0, 0.0, 0.0);
          #endif

              UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(packedInput);
              FragInputs input = BuildFragInputs(packedInput.vmesh);

              // We need to readapt the SS position as our screen space positions are for a low res buffer, but we try to access a full res buffer.
              input.positionSS.xy = _OffScreenRendering > 0 ? (input.positionSS.xy * _OffScreenDownsampleFactor) : input.positionSS.xy;

              uint2 tileIndex = uint2(input.positionSS.xy) / GetTileSize();

              // input.positionSS is SV_Position
              PositionInputs posInput = GetPositionInput(input.positionSS.xy, _ScreenSize.zw, input.positionSS.z, input.positionSS.w, input.positionRWS.xyz, tileIndex);

              #ifdef VARYINGS_NEED_POSITION_WS
                 float3 V = GetWorldSpaceNormalizeViewDir(input.positionRWS);
              #else
                 // Unused
                 float3 V = float3(1.0, 1.0, 1.0); // Avoid the division by 0
              #endif

              SurfaceData surfaceData;
              BuiltinData builtinData;
              Surface l;
              ShaderData d;
              GetSurfaceAndBuiltinData(packedInput.vmesh, input, V, posInput, surfaceData, builtinData, l, d
               #if NEED_FACING
                  , facing
               #endif
               );

              BSDFData bsdfData = ConvertSurfaceDataToBSDFData(input.positionSS.xy, surfaceData);

              PreLightData preLightData = GetPreLightData(V, posInput, bsdfData);

              outColor = float4(0.0, 0.0, 0.0, 0.0);

              // We need to skip lighting when doing debug pass because the debug pass is done before lighting so some buffers may not be properly initialized potentially causing crashes on PS4.

          #ifdef DEBUG_DISPLAY
              // Init in debug display mode to quiet warning
          #ifdef OUTPUT_SPLIT_LIGHTING
              outDiffuseLighting = 0;
              ENCODE_INTO_SSSBUFFER(surfaceData, posInput.positionSS, outSSSBuffer);
          #endif

              

              // Same code in ShaderPassForwardUnlit.shader
              // Reminder: _DebugViewMaterialArray[i]
              //   i==0 -> the size used in the buffer
              //   i>0  -> the index used (0 value means nothing)
              // The index stored in this buffer could either be
              //   - a gBufferIndex (always stored in _DebugViewMaterialArray[1] as only one supported)
              //   - a property index which is different for each kind of material even if reflecting the same thing (see MaterialSharedProperty)
              bool viewMaterial = false;
              int bufferSize = int(_DebugViewMaterialArray[0]);
              if (bufferSize != 0)
              {
                  bool needLinearToSRGB = false;
                  float3 result = float3(1.0, 0.0, 1.0);

                  // Loop through the whole buffer
                  // Works because GetSurfaceDataDebug will do nothing if the index is not a known one
                  for (int index = 1; index <= bufferSize; index++)
                  {
                      int indexMaterialProperty = int(_DebugViewMaterialArray[index]);

                      // skip if not really in use
                      if (indexMaterialProperty != 0)
                      {
                          viewMaterial = true;

                          GetPropertiesDataDebug(indexMaterialProperty, result, needLinearToSRGB);
                          GetVaryingsDataDebug(indexMaterialProperty, input, result, needLinearToSRGB);
                          GetBuiltinDataDebug(indexMaterialProperty, builtinData, result, needLinearToSRGB);
                          GetSurfaceDataDebug(indexMaterialProperty, surfaceData, result, needLinearToSRGB);
                          GetBSDFDataDebug(indexMaterialProperty, bsdfData, result, needLinearToSRGB);
                      }
                  }

                  // TEMP!
                  // For now, the final blit in the backbuffer performs an sRGB write
                  // So in the meantime we apply the inverse transform to linear data to compensate.
                  if (!needLinearToSRGB)
                      result = SRGBToLinear(max(0, result));

                  outColor = float4(result, 1.0);
              }

              if (!viewMaterial)
              {
                  if (_DebugFullScreenMode == FULLSCREENDEBUGMODE_VALIDATE_DIFFUSE_COLOR || _DebugFullScreenMode == FULLSCREENDEBUGMODE_VALIDATE_SPECULAR_COLOR)
                  {
                      float3 result = float3(0.0, 0.0, 0.0);

                      GetPBRValidatorDebug(surfaceData, result);

                      outColor = float4(result, 1.0f);
                  }
                  else if (_DebugFullScreenMode == FULLSCREENDEBUGMODE_TRANSPARENCY_OVERDRAW)
                  {
                      float4 result = _DebugTransparencyOverdrawWeight * float4(TRANSPARENCY_OVERDRAW_COST, TRANSPARENCY_OVERDRAW_COST, TRANSPARENCY_OVERDRAW_COST, TRANSPARENCY_OVERDRAW_A);
                      outColor = result;
                  }
                  else
          #endif
                  {
          #ifdef _SURFACE_TYPE_TRANSPARENT
                      uint featureFlags = LIGHT_FEATURE_MASK_FLAGS_TRANSPARENT;
          #else
                      uint featureFlags = LIGHT_FEATURE_MASK_FLAGS_OPAQUE;
          #endif

                      float3 diffuseLighting;
                      float3 specularLighting;

                      #if (SHADER_LIBRARY_VERSION_MAJOR >= 10)
                      {
                         LightLoopOutput lightLoopOutput;
                         LightLoop(V, posInput, preLightData, bsdfData, builtinData, featureFlags, lightLoopOutput);

                         // Alias
                         diffuseLighting = lightLoopOutput.diffuseLighting;
                         specularLighting = lightLoopOutput.specularLighting;
                      }
                      #else
                      {
                         LightLoop(V, posInput, preLightData, bsdfData, builtinData, featureFlags, diffuseLighting, specularLighting);
                      }
                      #endif

                      diffuseLighting *= GetCurrentExposureMultiplier();
                      specularLighting *= GetCurrentExposureMultiplier();

          #ifdef OUTPUT_SPLIT_LIGHTING
                      if (_EnableSubsurfaceScattering != 0 && ShouldOutputSplitLighting(bsdfData))
                      {
                          outColor = float4(specularLighting, 1.0);
                          outDiffuseLighting = float4(TagLightingForSSS(diffuseLighting), 1.0);
                      }
                      else
                      {
                          outColor = float4(diffuseLighting + specularLighting, 1.0);
                          outDiffuseLighting = 0;
                      }
                      ENCODE_INTO_SSSBUFFER(surfaceData, posInput.positionSS, outSSSBuffer);
          #else
                      outColor = ApplyBlendMode(diffuseLighting, specularLighting, builtinData.opacity);
                      outColor = EvaluateAtmosphericScattering(posInput, V, outColor);
          #endif

          ChainFinalColorForward(l, d, outColor);

          #ifdef _WRITE_TRANSPARENT_MOTION_VECTOR
                      VaryingsPassToPS inputPass = UnpackVaryingsPassToPS(packedInput.vpass);
                      bool forceNoMotion = any(unity_MotionVectorsParams.yw == 0.0);
                      // outMotionVec is already initialize at the value of forceNoMotion (see above)
                      if (!forceNoMotion)
                      {
                          float2 motionVec = CalculateMotionVector(inputPass.positionCS, inputPass.previousPositionCS);
                          EncodeMotionVector(motionVec * 0.5, outMotionVec);
                          outMotionVec.zw = 1.0;
                      }
          #endif
                  }

          #ifdef DEBUG_DISPLAY
              }
          #endif

          #ifdef _DEPTHOFFSET_ON
              outputDepth = posInput.deviceDepth;
          #endif
          }

            ENDHLSL
        }

      
      
      Pass
        {
            // based on HDLitPass.template
            Name "META"
            Tags { "LightMode" = "META" }
            
            Cull Off
        
            
        
            //-------------------------------------------------------------------------------------
            // End Render Modes
            //-------------------------------------------------------------------------------------
        
            HLSLPROGRAM
        
            #pragma target 4.5
            #pragma only_renderers d3d11 ps4 xboxone vulkan metal switch
            //#pragma enable_d3d11_debug_symbols
        
            #pragma multi_compile_instancing

            #pragma multi_compile_local _ _ALPHATEST_ON


            #pragma shader_feature _SURFACE_TYPE_TRANSPARENT
            //#pragma shader_feature_local _ _BLENDMODE_ALPHA _BLENDMODE_ADD _BLENDMODE_PRE_MULTIPLY
        
            //-------------------------------------------------------------------------------------
            // Variant Definitions (active field translations to HDRP defines)
            //-------------------------------------------------------------------------------------
            // #define _MATERIAL_FEATURE_SUBSURFACE_SCATTERING 1
            // #define _MATERIAL_FEATURE_TRANSMISSION 1
            // #define _MATERIAL_FEATURE_ANISOTROPY 1
            // #define _MATERIAL_FEATURE_IRIDESCENCE 1
            // #define _MATERIAL_FEATURE_SPECULAR_COLOR 1
            // #define _ENABLE_FOG_ON_TRANSPARENT 1
            // #define _AMBIENT_OCCLUSION 1
            // #define _SPECULAR_OCCLUSION_FROM_AO 1
            // #define _SPECULAR_OCCLUSION_FROM_AO_BENT_NORMAL 1
            // #define _SPECULAR_OCCLUSION_CUSTOM 1
            // #define _ENERGY_CONSERVING_SPECULAR 1
            // #define _ENABLE_GEOMETRIC_SPECULAR_AA 1
            // #define _HAS_REFRACTION 1
            // #define _REFRACTION_PLANE 1
            // #define _REFRACTION_SPHERE 1
            // #define _DISABLE_DECALS 1
            // #define _DISABLE_SSR 1
            // #define _ADD_PRECOMPUTED_VELOCITY
            // #define _WRITE_TRANSPARENT_MOTION_VECTOR 1
            // #define _DEPTHOFFSET_ON 1
            // #define _BLENDMODE_PRESERVE_SPECULAR_LIGHTING 1

            #define SHADERPASS SHADERPASS_LIGHT_TRANSPORT
            #define RAYTRACING_SHADER_GRAPH_HIGH
            #define REQUIRE_DEPTH_TEXTURE
            #define _PASSMETA 1

        
            
   #pragma shader_feature_local _ _OVERRIDE_OPACITY
   #pragma shader_feature_local _ _OVERRIDE_NORMAL
   #pragma shader_feature_local _ _OVERRIDE_MOS
   #pragma shader_feature_local _ _OVERRIDE_EMISSION


	#define _HAS_ALPHA_BLEND 1


   #define _HDRP 1
#define _BLENDMODE_ALPHA 1
#define _SURFACE_TYPE_TRANSPARENT 1


               #pragma vertex Vert
   #pragma fragment Frag
        

            

                  // useful conversion functions to make surface shader code just work

      #define UNITY_DECLARE_TEX2D(name) TEXTURE2D(name); SAMPLER(sampler##name);
      #define UNITY_DECLARE_TEX2D_NOSAMPLER(name) TEXTURE2D(name);
      #define UNITY_DECLARE_TEX2DARRAY(name) TEXTURE2D_ARRAY(name); SAMPLER(sampler##name);
      #define UNITY_DECLARE_TEX2DARRAY_NOSAMPLER(tex) TEXTURE2D_ARRAY(tex);

      #define UNITY_SAMPLE_TEX2DARRAY(tex,coord)            SAMPLE_TEXTURE2D_ARRAY(tex, sampler##tex, coord.xy, coord.z)
      #define UNITY_SAMPLE_TEX2DARRAY_LOD(tex,coord,lod)    SAMPLE_TEXTURE2D_ARRAY_LOD(tex, sampler##tex, coord.xy, coord.z, lod)
      #define UNITY_SAMPLE_TEX2D(tex, coord)                SAMPLE_TEXTURE2D(tex, sampler##tex, coord)
      #define UNITY_SAMPLE_TEX2D_SAMPLER(tex, samp, coord)  SAMPLE_TEXTURE2D(tex, sampler##samp, coord)

      #define UNITY_SAMPLE_TEX2D_LOD(tex,coord, lod)   SAMPLE_TEXTURE2D_LOD(tex, sampler_##tex, coord, lod)
      #define UNITY_SAMPLE_TEX2D_SAMPLER_LOD(tex,samplertex,coord, lod) SAMPLE_TEXTURE2D_LOD (tex, sampler##samplertex,coord, lod)

      #if defined(UNITY_COMPILER_HLSL)
         #define UNITY_INITIALIZE_OUTPUT(type,name) name = (type)0;
      #else
         #define UNITY_INITIALIZE_OUTPUT(type,name)
      #endif

      #define sampler2D_float sampler2D
      #define sampler2D_half sampler2D

      #undef WorldNormalVector
      #define WorldNormalVector(data, normal) mul(normal, data.TBNMatrix)

      #define UnityObjectToWorldNormal(normal) mul(GetObjectToWorldMatrix(), normal)




// HDRP Adapter stuff


            // If we use subsurface scattering, enable output split lighting (for forward pass)
            #if defined(_MATERIAL_FEATURE_SUBSURFACE_SCATTERING) && !defined(_SURFACE_TYPE_TRANSPARENT)
            #define OUTPUT_SPLIT_LIGHTING
            #endif

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Version.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
        
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/NormalSurfaceGradient.hlsl"
        
            // define FragInputs structure
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"
            #if (SHADER_LIBRARY_VERSION_MAJOR >= 10)
               #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/Functions.hlsl"
            #endif


        

            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
        #ifdef DEBUG_DISPLAY
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Debug/DebugDisplay.hlsl"
        #endif
        
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
        
        #if (SHADERPASS == SHADERPASS_FORWARD)
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/Lighting.hlsl"
        
            #define HAS_LIGHTLOOP
        
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/LightLoop/LightLoopDef.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/Lit.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/LightLoop/LightLoop.hlsl"
        #else
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/Lit.hlsl"
        #endif
        
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/BuiltinUtilities.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/MaterialUtilities.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Decal/DecalUtilities.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/LitDecalData.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphFunctions.hlsl"
        
            // Used by SceneSelectionPass
            int _ObjectId;
            int _PassValue;
        
           
            // data across stages, stripped like the above.
            struct VertexToPixel
            {
               float4 pos : SV_POSITION;
               float3 worldPos : TEXCOORD0;
               float3 worldNormal : TEXCOORD1;
               float4 worldTangent : TEXCOORD2;
               float4 texcoord0 : TEXCCOORD3;
               float4 texcoord1 : TEXCCOORD4;
               float4 texcoord2 : TEXCCOORD5;
               // float4 texcoord3 : TEXCCOORD6;
               // float4 screenPos : TEXCOORD7;
               // float4 vertexColor : COLOR;

               // float4 extraV2F0 : TEXCOORD8;
               // float4 extraV2F1 : TEXCOORD9;
               // float4 extraV2F2 : TEXCOORD10;
               // float4 extraV2F3 : TEXCOORD11;
               // float4 extraV2F4 : TEXCOORD12;
               // float4 extraV2F5 : TEXCOORD13;
               // float4 extraV2F6 : TEXCOORD14;
               // float4 extraV2F7 : TEXCOORD15;

               #if UNITY_ANY_INSTANCING_ENABLED
                  uint instanceID : INSTANCEID_SEMANTIC;
               #endif // UNITY_ANY_INSTANCING_ENABLED

               UNITY_VERTEX_OUTPUT_STEREO
            };


  
            
            
            // data describing the user output of a pixel
            struct Surface
            {
               half3 Albedo;
               half Height;
               half3 Normal;
               half Smoothness;
               half3 Emission;
               half Metallic;
               half3 Specular;
               half Occlusion;
               half SpecularPower; // for simple lighting
               half Alpha;
               float outputDepth; // if written, SV_Depth semantic is used. ShaderData.clipPos.z is unused value
               // HDRP Only
               half SpecularOcclusion;
               half SubsurfaceMask;
               half Thickness;
               half CoatMask;
               half CoatSmoothness;
               half Anisotropy;
               half IridescenceMask;
               half IridescenceThickness;
               int DiffusionProfileHash;
            };

            // Data the user declares in blackboard blocks
            struct Blackboard
            {
                
                float blackboardDummyData;
            };

            // data the user might need, this will grow to be big. But easy to strip
            struct ShaderData
            {
               float4 clipPos; // SV_POSITION
               float3 localSpacePosition;
               float3 localSpaceNormal;
               float3 localSpaceTangent;
        
               float3 worldSpacePosition;
               float3 worldSpaceNormal;
               float3 worldSpaceTangent;
               float tangentSign;

               float3 worldSpaceViewDir;
               float3 tangentSpaceViewDir;

               float4 texcoord0;
               float4 texcoord1;
               float4 texcoord2;
               float4 texcoord3;

               float2 screenUV;
               float4 screenPos;

               float4 vertexColor;
               bool isFrontFace;

               float4 extraV2F0;
               float4 extraV2F1;
               float4 extraV2F2;
               float4 extraV2F3;
               float4 extraV2F4;
               float4 extraV2F5;
               float4 extraV2F6;
               float4 extraV2F7;

               float3x3 TBNMatrix;
               Blackboard blackboard;
            };

            struct VertexData
            {
               #if SHADER_TARGET > 30
               // uint vertexID : SV_VertexID;
               #endif
               float4 vertex : POSITION;
               float3 normal : NORMAL;
               float4 tangent : TANGENT;
               float4 texcoord0 : TEXCOORD0;
               float4 texcoord1 : TEXCOORD1;
               float4 texcoord2 : TEXCOORD2;
               // float4 texcoord3 : TEXCOORD3;
               // float4 vertexColor : COLOR;

               #if _HDRP && (_PASSMOTIONVECTOR || (_PASSFORWARD && defined(_WRITE_TRANSPARENT_MOTION_VECTOR)))
                  float3 previousPositionOS : TEXCOORD4; // Contain previous transform position (in case of skinning for example)
                  #if defined (_ADD_PRECOMPUTED_VELOCITY)
                     float3 precomputedVelocity    : TEXCOORD5; // Add Precomputed Velocity (Alembic computes velocities on runtime side).
                  #endif
               #endif

               UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct TessVertex 
            {
               float4 vertex : INTERNALTESSPOS;
               float3 normal : NORMAL;
               float4 tangent : TANGENT;
               float4 texcoord0 : TEXCOORD0;
               float4 texcoord1 : TEXCOORD1;
               float4 texcoord2 : TEXCOORD2;
               // float4 texcoord3 : TEXCOORD3;
               // float4 vertexColor : COLOR;

               
               // float4 extraV2F0 : TEXCOORD4;
               // float4 extraV2F1 : TEXCOORD5;
               // float4 extraV2F2 : TEXCOORD6;
               // float4 extraV2F3 : TEXCOORD7;
               // float4 extraV2F4 : TEXCOORD8;
               // float4 extraV2F5 : TEXCOORD9;
               // float4 extraV2F6 : TEXCOORD10;
               // float4 extraV2F7 : TEXCOORD11;

               #if _HDRP && (_PASSMOTIONVECTOR || (_PASSFORWARD && defined(_WRITE_TRANSPARENT_MOTION_VECTOR)))
                  float3 previousPositionOS : TEXCOORD12; // Contain previous transform position (in case of skinning for example)
                  #if defined (_ADD_PRECOMPUTED_VELOCITY)
                     float3 precomputedVelocity : TEXCOORD13;
                  #endif
               #endif

               UNITY_VERTEX_INPUT_INSTANCE_ID
               UNITY_VERTEX_OUTPUT_STEREO
            };

            struct ExtraV2F
            {
               float4 extraV2F0;
               float4 extraV2F1;
               float4 extraV2F2;
               float4 extraV2F3;
               float4 extraV2F4;
               float4 extraV2F5;
               float4 extraV2F6;
               float4 extraV2F7;
               Blackboard blackboard;
            };


            float3 WorldToTangentSpace(ShaderData d, float3 normal)
            {
               return mul(d.TBNMatrix, normal);
            }

            float3 TangentToWorldSpace(ShaderData d, float3 normal)
            {
               return mul(normal, d.TBNMatrix);
            }

            // in this case, make standard more like SRPs, because we can't fix
            // unity_WorldToObject in HDRP, since it already does macro-fu there

            #if _STANDARD
               float3 TransformWorldToObject(float3 p) { return mul(unity_WorldToObject, float4(p, 1)); };
               float3 TransformObjectToWorld(float3 p) { return mul(unity_ObjectToWorld, float4(p, 1)); };
               float4 TransformWorldToObject(float4 p) { return mul(unity_WorldToObject, p); };
               float4 TransformObjectToWorld(float4 p) { return mul(unity_ObjectToWorld, p); };
               float4x4 GetWorldToObjectMatrix() { return unity_WorldToObject; }
               float4x4 GetObjectToWorldMatrix() { return unity_ObjectToWorld; }
               #if (defined(SHADER_API_D3D11) || defined(SHADER_API_XBOXONE) || defined(UNITY_COMPILER_HLSLCC) || defined(SHADER_API_PSSL) || (SHADER_TARGET_SURFACE_ANALYSIS && !SHADER_TARGET_SURFACE_ANALYSIS_MOJOSHADER))
                 #define UNITY_SAMPLE_TEX2D_LOD(tex,coord, lod) tex.SampleLevel (sampler##tex,coord, lod)
                 #define UNITY_SAMPLE_TEX2D_SAMPLER_LOD(tex,samplertex,coord, lod) tex.SampleLevel (sampler##samplertex,coord, lod)
              #else
                 #define UNITY_SAMPLE_TEX2D_LOD(tex,coord,lod) tex2D (tex,coord,0,lod)
                 #define UNITY_SAMPLE_TEX2D_SAMPLER_LOD(tex,samplertex,coord,lod) tex2D (tex,coord,0,lod)
              #endif

               #undef GetObjectToWorldMatrix()
               #undef GetWorldToObjectMatrix()
               #undef GetWorldToViewMatrix()
               #undef UNITY_MATRIX_I_V
               #undef UNITY_MATRIX_P
               #undef GetWorldToHClipMatrix()
               #undef GetObjectToWorldMatrix()V
               #undef UNITY_MATRIX_T_MV
               #undef UNITY_MATRIX_IT_MV
               #undef GetObjectToWorldMatrix()VP

               #define GetObjectToWorldMatrix()     unity_ObjectToWorld
               #define GetWorldToObjectMatrix()   unity_WorldToObject
               #define GetWorldToViewMatrix()     unity_MatrixV
               #define UNITY_MATRIX_I_V   unity_MatrixInvV
               #define GetViewToHClipMatrix()     OptimizeProjectionMatrix(glstate_matrix_projection)
               #define GetWorldToHClipMatrix()    unity_MatrixVP
               #define GetObjectToWorldMatrix()V    mul(GetWorldToViewMatrix(), GetObjectToWorldMatrix())
               #define UNITY_MATRIX_T_MV  transpose(GetObjectToWorldMatrix()V)
               #define UNITY_MATRIX_IT_MV transpose(mul(GetWorldToObjectMatrix(), UNITY_MATRIX_I_V))
               #define GetObjectToWorldMatrix()VP   mul(GetWorldToHClipMatrix(), GetObjectToWorldMatrix())


            #endif

            float3 GetCameraWorldPosition()
            {
               #if _HDRP
                  return GetCameraRelativePositionWS(_WorldSpaceCameraPos);
               #else
                  return _WorldSpaceCameraPos;
               #endif
            }

            #if _GRABPASSUSED
               #if _STANDARD
                  TEXTURE2D(%GRABTEXTURE%);
                  SAMPLER(sampler_%GRABTEXTURE%);
               #endif

               half3 GetSceneColor(float2 uv)
               {
                  #if _STANDARD
                     return SAMPLE_TEXTURE2D(%GRABTEXTURE%, sampler_%GRABTEXTURE%, uv).rgb;
                  #else
                     return SHADERGRAPH_SAMPLE_SCENE_COLOR(uv);
                  #endif
               }
            #endif



            #if _STANDARD
               sampler2D _CameraDepthTexture;
               float GetSceneDepth(float2 uv) { return SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv); }
               float GetLinear01Depth(float2 uv) { return Linear01Depth(GetSceneDepth(uv)); }
               float GetLinearEyeDepth(float2 uv) { return LinearEyeDepth(GetSceneDepth(uv)); } 
            #else
               float GetSceneDepth(float2 uv) { return SHADERGRAPH_SAMPLE_SCENE_DEPTH(uv); }
               float GetLinear01Depth(float2 uv) { return Linear01Depth(GetSceneDepth(uv), _ZBufferParams); }
               float GetLinearEyeDepth(float2 uv) { return LinearEyeDepth(GetSceneDepth(uv), _ZBufferParams); } 
            #endif

            float3 GetWorldPositionFromDepthBuffer(float2 uv, float3 worldSpaceViewDir)
            {
               float eye = GetLinearEyeDepth(uv);
               float3 camView = mul((float3x3)GetObjectToWorldMatrix(), transpose(mul(GetWorldToObjectMatrix(), UNITY_MATRIX_I_V)) [2].xyz);

               float dt = dot(worldSpaceViewDir, camView);
               float3 div = worldSpaceViewDir/dt;
               float3 wpos = (eye * div) + GetCameraWorldPosition();
               return wpos;
            }

            #if _STANDARD
               sampler2D _CameraDepthNormalsTexture;
               float3 GetSceneNormal(float2 uv, float3 worldSpaceViewDir)
               {
                  float4 depthNorms = tex2D(_CameraDepthNormalsTexture, uv);
                  float3 norms = DecodeViewNormalStereo(depthNorms);
                  norms = mul((float3x3)GetWorldToViewMatrix(), norms) * 0.5 + 0.5;
                  return norms;
               }
            #elif _HDRP
               float3 GetSceneNormal(float2 uv, float3 worldSpaceViewDir)
               {
                  NormalData nd;
                  DecodeFromNormalBuffer(_ScreenSize.xy * uv, nd);
                  return nd.normalWS;
               }
            #elif _URP
               #if (SHADER_LIBRARY_VERSION_MAJOR >= 10)
                  #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"
               #endif

               float3 GetSceneNormal(float2 uv, float3 worldSpaceViewDir)
               {
                  #if (SHADER_LIBRARY_VERSION_MAJOR >= 10)
                     return SampleSceneNormals(uv);
                  #else
                     float3 wpos = GetWorldPositionFromDepthBuffer(uv, worldSpaceViewDir);
                     return normalize(-cross(ddx(wpos), ddy(wpos))) * 0.5 + 0.5;
                  #endif

                }
             #endif

             #if _HDRP

               half3 UnpackNormalmapRGorAG(half4 packednormal)
               {
                     // This do the trick
                  packednormal.x *= packednormal.w;

                  half3 normal;
                  normal.xy = packednormal.xy * 2 - 1;
                  normal.z = sqrt(1 - saturate(dot(normal.xy, normal.xy)));
                  return normal;
               }
               half3 UnpackNormal(half4 packednormal)
               {
                  #if defined(UNITY_NO_DXT5nm)
                     return packednormal.xyz * 2 - 1;
                  #else
                     return UnpackNormalmapRGorAG(packednormal);
                  #endif
               }
               #endif
               #if _HDRP || _URP

               half3 UnpackScaleNormal(half4 packednormal, half scale)
               {
                 #ifndef UNITY_NO_DXT5nm
                   // Unpack normal as DXT5nm (1, y, 1, x) or BC5 (x, y, 0, 1)
                   // Note neutral texture like "bump" is (0, 0, 1, 1) to work with both plain RGB normal and DXT5nm/BC5
                   packednormal.x *= packednormal.w;
                 #endif
                   half3 normal;
                   normal.xy = (packednormal.xy * 2 - 1) * scale;
                   normal.z = sqrt(1 - saturate(dot(normal.xy, normal.xy)));
                   return normal;
               }	

             #endif


            void GetSun(out float3 lightDir, out float3 color)
            {
               lightDir = float3(0.5, 0.5, 0);
               color = 1;
               #if _HDRP
                  if (_DirectionalLightCount > 0)
                  {
                     DirectionalLightData light = _DirectionalLightDatas[0];
                     lightDir = -light.forward.xyz;
                     color = light.color;
                  }
               #elif _STANDARD
			         lightDir = normalize(_WorldSpaceLightPos0.xyz);
                  color = _LightColor0.rgb;
               #elif _URP
	               Light light = GetMainLight();
	               lightDir = light.direction;
	               color = light.color;
               #endif
            }




            CBUFFER_START(UnityPerMaterial)

               float _StencilRef;
               float _StencilWriteMask;
               float _StencilRefDepth;
               float _StencilWriteMaskDepth;
               float _StencilRefMV;
               float _StencilWriteMaskMV;
               float _StencilRefDistortionVec;
               float _StencilWriteMaskDistortionVec;
               float _StencilWriteMaskGBuffer;
               float _StencilRefGBuffer;
               float _ZTestGBuffer;
               float _RequireSplitLighting;
               float _ReceivesSSR;
               float _ZWrite;
               float _TransparentSortPriority;
               float _ZTestDepthEqualForOpaque;
               float _ZTestTransparent;
               float _TransparentBackfaceEnable;
               float _AlphaCutoffEnable;
               float _UseShadowThreshold;

               
	float4 _Color;
	float  _BumpScale;
	float  _Metallic;
	float  _GlossMapScale;
	float3 _Emission;
	float  _Tiling;
	float  _UseUV2;
	float  _UseUV2Alt;





            CBUFFER_END

            

            

            #ifdef unity_WorldToObject
#undef unity_WorldToObject
#endif
#ifdef unity_ObjectToWorld
#undef unity_ObjectToWorld
#endif
#define unity_ObjectToWorld GetObjectToWorldMatrix()
#define unity_WorldToObject GetWorldToObjectMatrix()

	TEXTURE2D(_MainTex);
	SAMPLER(sampler_MainTex);
	TEXTURE2D(_BumpMap);
	SAMPLER(sampler_BumpMap);
	TEXTURE2D(_MetallicGlossMap);
	SAMPLER(sampler_MetallicGlossMap);
	TEXTURE2D(_EmissionMap);
	SAMPLER(sampler_EmissionMap);

	TEXTURE2D(_AlbedoTex);
	SAMPLER(sampler_AlbedoTex);
	TEXTURE2D(_OpacityTex);
	SAMPLER(sampler_OpacityTex);
	TEXTURE2D(_NormalTex);
	SAMPLER(sampler_NormalTex);
	TEXTURE2D(_EmissionTex);
	SAMPLER(sampler_EmissionTex);
	TEXTURE2D(_MosTex);
	SAMPLER(sampler_MosTex);

	void Ext_ModifyVertex0(inout VertexData v, inout ExtraV2F d)
	{
		float4 first  = lerp(v.texcoord0, v.texcoord1, _UseUV2);
		float4 second = lerp(v.texcoord0, v.texcoord1, _UseUV2Alt);

		v.texcoord0 = first * _Tiling;
		v.texcoord1 = second;
	}

	void Ext_SurfaceFunction0(inout Surface o, ShaderData d)
	{
		float4 texMain = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, d.texcoord0);
		float4 gloss   = SAMPLE_TEXTURE2D(_MetallicGlossMap, sampler_MetallicGlossMap, d.texcoord0);
		float4 bump    = SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, d.texcoord0);
		float4 glow    = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, d.texcoord0);
	#if !_HAS_ALPHA_BLEND
		clip(texMain.a * _Color.a - 0.5f);
	#endif
		o.Albedo     = texMain.rgb * _Color.rgb;
		o.Normal     = UnpackScaleNormal(bump, _BumpScale);
		o.Metallic   = gloss.r * _Metallic;
		o.Occlusion  = gloss.g;
		o.Smoothness = gloss.b * _GlossMapScale;
		o.Emission   = glow.rgb * _Emission;
		o.Alpha      = texMain.a * _Color.a;

		// Override albedo?
		float4 albedo = SAMPLE_TEXTURE2D(_AlbedoTex, sampler_AlbedoTex, d.texcoord1);
		o.Albedo = (1.0f - albedo.a) * o.Albedo + albedo.rgb;

	#if _OVERRIDE_OPACITY
		float4 opacity = SAMPLE_TEXTURE2D(_OpacityTex, sampler_OpacityTex, d.texcoord1);
		o.Alpha = (1.0f - opacity.a) * o.Alpha + opacity.r;
	#endif

	#if _OVERRIDE_NORMAL
		float4 normal = SAMPLE_TEXTURE2D(_NormalTex, sampler_NormalTex, d.texcoord1);
		o.Normal = (1.0f - normal.a) * o.Normal + normal.r;
	#endif
	
	#if _OVERRIDE_MOS
		float4 mos = SAMPLE_TEXTURE2D(_MosTex, sampler_MosTex, d.texcoord1);
		o.Metallic   = (1.0f - mos.a) * o.Metallic + mos.r;
		o.Occlusion  = (1.0f - mos.a) * o.Metallic + mos.g;
		o.Smoothness = (1.0f - mos.a) * o.Metallic + mos.b;
	#endif
	
	#if _OVERRIDE_EMISSION
		float4 emission = SAMPLE_TEXTURE2D(_EmissionTex, sampler_EmissionTex, d.texcoord1);
		o.Emission = (1.0f - emission.a) * o.Emission + emission.rgb;
	#endif
	}





        
            void ChainSurfaceFunction(inout Surface l, inout ShaderData d)
            {
                  Ext_SurfaceFunction0(l, d);
                 // Ext_SurfaceFunction1(l, d);
                 // Ext_SurfaceFunction2(l, d);
                 // Ext_SurfaceFunction3(l, d);
                 // Ext_SurfaceFunction4(l, d);
                 // Ext_SurfaceFunction5(l, d);
                 // Ext_SurfaceFunction6(l, d);
                 // Ext_SurfaceFunction7(l, d);
                 // Ext_SurfaceFunction8(l, d);
                 // Ext_SurfaceFunction9(l, d);
		           // Ext_SurfaceFunction10(l, d);
                 // Ext_SurfaceFunction11(l, d);
                 // Ext_SurfaceFunction12(l, d);
                 // Ext_SurfaceFunction13(l, d);
                 // Ext_SurfaceFunction14(l, d);
                 // Ext_SurfaceFunction15(l, d);
                 // Ext_SurfaceFunction16(l, d);
                 // Ext_SurfaceFunction17(l, d);
                 // Ext_SurfaceFunction18(l, d);
		           // Ext_SurfaceFunction19(l, d);
            }

            void ChainModifyVertex(inout VertexData v, inout VertexToPixel v2p)
            {
                 ExtraV2F d;
                 ZERO_INITIALIZE(ExtraV2F, d);
                 ZERO_INITIALIZE(Blackboard, d.blackboard);

                   Ext_ModifyVertex0(v, d);
                 // Ext_ModifyVertex1(v, d);
                 // Ext_ModifyVertex2(v, d);
                 // Ext_ModifyVertex3(v, d);
                 // Ext_ModifyVertex4(v, d);
                 // Ext_ModifyVertex5(v, d);
                 // Ext_ModifyVertex6(v, d);
                 // Ext_ModifyVertex7(v, d);
                 // Ext_ModifyVertex8(v, d);
                 // Ext_ModifyVertex9(v, d);
                 // Ext_ModifyVertex10(v, d);
                 // Ext_ModifyVertex11(v, d);
                 // Ext_ModifyVertex12(v, d);
                 // Ext_ModifyVertex13(v, d);
                 // Ext_ModifyVertex14(v, d);
                 // Ext_ModifyVertex15(v, d);
                 // Ext_ModifyVertex16(v, d);
                 // Ext_ModifyVertex17(v, d);
                 // Ext_ModifyVertex18(v, d);
                 // Ext_ModifyVertex19(v, d);
		
                 // v2p.extraV2F0 = d.extraV2F0;
                 // v2p.extraV2F1 = d.extraV2F1;
                 // v2p.extraV2F2 = d.extraV2F2;
                 // v2p.extraV2F3 = d.extraV2F3;
                 // v2p.extraV2F4 = d.extraV2F4;
                 // v2p.extraV2F5 = d.extraV2F5;
                 // v2p.extraV2F6 = d.extraV2F6;
                 // v2p.extraV2F7 = d.extraV2F7;
            }

            void ChainModifyTessellatedVertex(inout VertexData v, inout VertexToPixel v2p)
            {
               ExtraV2F d;
               ZERO_INITIALIZE(ExtraV2F, d);
               ZERO_INITIALIZE(Blackboard, d.blackboard);
               // d.extraV2F0 = v2p.extraV2F0;
               // d.extraV2F1 = v2p.extraV2F1;
               // d.extraV2F2 = v2p.extraV2F2;
               // d.extraV2F3 = v2p.extraV2F3;
               // d.extraV2F0 = v2p.extraV2F4;
               // d.extraV2F1 = v2p.extraV2F5;
               // d.extraV2F2 = v2p.extraV2F6;
               // d.extraV2F3 = v2p.extraV2F7;

               // Ext_ModifyTessellatedVertex0(v, d);
               // Ext_ModifyTessellatedVertex1(v, d);
               // Ext_ModifyTessellatedVertex2(v, d);
               // Ext_ModifyTessellatedVertex3(v, d);
               // Ext_ModifyTessellatedVertex4(v, d);
               // Ext_ModifyTessellatedVertex5(v, d);
               // Ext_ModifyTessellatedVertex6(v, d);
               // Ext_ModifyTessellatedVertex7(v, d);
               // Ext_ModifyTessellatedVertex8(v, d);
               // Ext_ModifyTessellatedVertex9(v, d);
               // Ext_ModifyTessellatedVertex10(v, d);
               // Ext_ModifyTessellatedVertex11(v, d);
               // Ext_ModifyTessellatedVertex12(v, d);
               // Ext_ModifyTessellatedVertex13(v, d);
               // Ext_ModifyTessellatedVertex14(v, d);
               // Ext_ModifyTessellatedVertex15(v, d);
               // Ext_ModifyTessellatedVertex16(v, d);
               // Ext_ModifyTessellatedVertex17(v, d);
               // Ext_ModifyTessellatedVertex18(v, d);
               // Ext_ModifyTessellatedVertex19(v, d);

               // v2p.extraV2F0 = d.extraV2F0;
               // v2p.extraV2F1 = d.extraV2F1;
               // v2p.extraV2F2 = d.extraV2F2;
               // v2p.extraV2F3 = d.extraV2F3;
               // v2p.extraV2F0 = d.extraV2F4;
               // v2p.extraV2F1 = d.extraV2F5;
               // v2p.extraV2F2 = d.extraV2F6;
               // v2p.extraV2F3 = d.extraV2F7;
            }

            void ChainFinalColorForward(inout Surface l, inout ShaderData d, inout half4 color)
            {
               //   Ext_FinalColorForward0(l, d, color);
               //   Ext_FinalColorForward1(l, d, color);
               //   Ext_FinalColorForward2(l, d, color);
               //   Ext_FinalColorForward3(l, d, color);
               //   Ext_FinalColorForward4(l, d, color);
               //   Ext_FinalColorForward5(l, d, color);
               //   Ext_FinalColorForward6(l, d, color);
               //   Ext_FinalColorForward7(l, d, color);
               //   Ext_FinalColorForward8(l, d, color);
               //   Ext_FinalColorForward9(l, d, color);
               //  Ext_FinalColorForward10(l, d, color);
               //  Ext_FinalColorForward11(l, d, color);
               //  Ext_FinalColorForward12(l, d, color);
               //  Ext_FinalColorForward13(l, d, color);
               //  Ext_FinalColorForward14(l, d, color);
               //  Ext_FinalColorForward15(l, d, color);
               //  Ext_FinalColorForward16(l, d, color);
               //  Ext_FinalColorForward17(l, d, color);
               //  Ext_FinalColorForward18(l, d, color);
               //  Ext_FinalColorForward19(l, d, color);
            }

            void ChainFinalGBufferStandard(inout Surface s, inout ShaderData d, inout half4 GBuffer0, inout half4 GBuffer1, inout half4 GBuffer2, inout half4 outEmission, inout half4 outShadowMask)
            {
               //   Ext_FinalGBufferStandard0(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
               //   Ext_FinalGBufferStandard1(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
               //   Ext_FinalGBufferStandard2(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
               //   Ext_FinalGBufferStandard3(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
               //   Ext_FinalGBufferStandard4(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
               //   Ext_FinalGBufferStandard5(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
               //   Ext_FinalGBufferStandard6(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
               //   Ext_FinalGBufferStandard7(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
               //   Ext_FinalGBufferStandard8(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
               //   Ext_FinalGBufferStandard9(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
               //  Ext_FinalGBufferStandard10(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
               //  Ext_FinalGBufferStandard11(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
               //  Ext_FinalGBufferStandard12(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
               //  Ext_FinalGBufferStandard13(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
               //  Ext_FinalGBufferStandard14(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
               //  Ext_FinalGBufferStandard15(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
               //  Ext_FinalGBufferStandard16(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
               //  Ext_FinalGBufferStandard17(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
               //  Ext_FinalGBufferStandard18(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
               //  Ext_FinalGBufferStandard19(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
            }



            

         ShaderData CreateShaderData(VertexToPixel i
                  #if NEED_FACING
                     , bool facing
                  #endif
         )
         {
            ShaderData d = (ShaderData)0;
            d.clipPos = i.pos;
            d.worldSpacePosition = i.worldPos;

            d.worldSpaceNormal = normalize(i.worldNormal);
            d.worldSpaceTangent = normalize(i.worldTangent.xyz);
            d.tangentSign = i.worldTangent.w;
            float3 bitangent = cross(i.worldTangent.xyz, i.worldNormal) * d.tangentSign * -1;
            

            d.TBNMatrix = float3x3(d.worldSpaceTangent, bitangent, d.worldSpaceNormal);
            d.worldSpaceViewDir = normalize(_WorldSpaceCameraPos - i.worldPos);

            d.tangentSpaceViewDir = mul(d.TBNMatrix, d.worldSpaceViewDir);
             d.texcoord0 = i.texcoord0;
             d.texcoord1 = i.texcoord1;
            // d.texcoord2 = i.texcoord2;
            // d.texcoord3 = i.texcoord3;
            // d.isFrontFace = facing;
            // d.vertexColor = i.vertexColor;

            // these rarely get used, so we back transform them. Usually will be stripped.
            #if _HDRP
                // d.localSpacePosition = mul(unity_WorldToObject, float4(GetCameraRelativePositionWS(i.worldPos), 1)).xyz;
            #else
                // d.localSpacePosition = mul(unity_WorldToObject, float4(i.worldPos, 1)).xyz;
            #endif
            // d.localSpaceNormal = normalize(mul(unity_WorldToObject, float4(i.worldNormal, 1)).xyz);
            // d.localSpaceTangent = normalize(mul(unity_WorldToObject, float4(i.worldTangent.xyz, 1)).xyz);

            // d.screenPos = i.screenPos;
            // d.screenUV = (i.screenPos.xy / i.screenPos.w);

            // d.extraV2F0 = i.extraV2F0;
            // d.extraV2F1 = i.extraV2F1;
            // d.extraV2F2 = i.extraV2F2;
            // d.extraV2F3 = i.extraV2F3;
            // d.extraV2F4 = i.extraV2F4;
            // d.extraV2F5 = i.extraV2F5;
            // d.extraV2F6 = i.extraV2F6;
            // d.extraV2F7 = i.extraV2F7;

            return d;
         }
         

            

struct VaryingsToPS
{
   VertexToPixel vmesh;
   #ifdef VARYINGS_NEED_PASS
      VaryingsPassToPS vpass;
   #endif
};

struct PackedVaryingsToPS
{
   #ifdef VARYINGS_NEED_PASS
      PackedVaryingsPassToPS vpass;
   #endif
   VertexToPixel vmesh;

   UNITY_VERTEX_OUTPUT_STEREO
};

PackedVaryingsToPS PackVaryingsToPS(VaryingsToPS input)
{
   PackedVaryingsToPS output = (PackedVaryingsToPS)0;
   output.vmesh = input.vmesh;
   #ifdef VARYINGS_NEED_PASS
      output.vpass = PackVaryingsPassToPS(input.vpass);
   #endif

   UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
   return output;
}




VertexToPixel VertMesh(VertexData input)
{
    VertexToPixel output = (VertexToPixel)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);

    
    ChainModifyVertex(input, output);


    // This return the camera relative position (if enable)
    float3 positionRWS = TransformObjectToWorld(input.vertex.xyz);
    float3 normalWS = TransformObjectToWorldNormal(input.normal);
    float4 tangentWS = float4(TransformObjectToWorldDir(input.tangent.xyz), input.tangent.w);


    output.worldPos = GetAbsolutePositionWS(positionRWS);
    output.pos = TransformWorldToHClip(positionRWS);
    output.worldNormal = normalWS;
    output.worldTangent = tangentWS;


    output.texcoord0 = input.texcoord0;
    output.texcoord1 = input.texcoord1;
    output.texcoord2 = input.texcoord2;
    // output.texcoord3 = input.texcoord3;
    // output.vertexColor = input.vertexColor;
    // output.screenPos = ComputeScreenPos(output.pos, _ProjectionParams.x);

    return output;
}


#if (SHADERPASS == SHADERPASS_DBUFFER_MESH)
void MeshDecalsPositionZBias(inout VaryingsToPS input)
{
#if defined(UNITY_REVERSED_Z)
    input.vmesh.pos.z -= _DecalMeshDepthBias;
#else
    input.vmesh.pos.z += _DecalMeshDepthBias;
#endif
}
#endif


#if (SHADERPASS == SHADERPASS_LIGHT_TRANSPORT)

// This was not in constant buffer in original unity, so keep outiside. But should be in as ShaderRenderPass frequency
float unity_OneOverOutputBoost;
float unity_MaxOutputValue;

CBUFFER_START(UnityMetaPass)
// x = use uv1 as raster position
// y = use uv2 as raster position
bool4 unity_MetaVertexControl;

// x = return albedo
// y = return normal
bool4 unity_MetaFragmentControl;
CBUFFER_END

PackedVaryingsToPS Vert(VertexData inputMesh)
{
    VaryingsToPS output = (VaryingsToPS)0;
    output.vmesh = (VertexToPixel)0;

    UNITY_SETUP_INSTANCE_ID(inputMesh);
    UNITY_TRANSFER_INSTANCE_ID(inputMesh, output.vmesh);

    // Output UV coordinate in vertex shader
    float2 uv = float2(0.0, 0.0);

    if (unity_MetaVertexControl.x)
    {
        uv = inputMesh.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
    }
    else if (unity_MetaVertexControl.y)
    {
        uv = inputMesh.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
    }

    // OpenGL right now needs to actually use the incoming vertex position
    // so we create a fake dependency on it here that haven't any impact.
    output.vmesh.pos = float4(uv * 2.0 - 1.0, inputMesh.vertex.z > 0 ? 1.0e-4 : 0.0, 1.0);

#ifdef VARYINGS_NEED_POSITION_WS
    output.vmesh.worldPos = TransformObjectToWorld(inputMesh.vertex.xyz);
#endif

#ifdef VARYINGS_NEED_TANGENT_TO_WORLD
    // Normal is required for triplanar mapping
    output.vmesh.worldNormal = TransformObjectToWorldNormal(inputMesh.normal);
    // Not required but assign to silent compiler warning
    output.vmesh.worldTangent = float4(1.0, 0.0, 0.0, 0.0);
#endif

    output.vmesh.texcoord0 = inputMesh.texcoord0;
    output.vmesh.texcoord1 = inputMesh.texcoord1;
    output.vmesh.texcoord2 = inputMesh.texcoord2;
    // output.vmesh.texCoord3 = inputMesh.texcoord3;
    // output.vmesh.vertexColor = inputMesh.vertexColor;

    return PackVaryingsToPS(output);
}
#else

PackedVaryingsToPS Vert(VertexData inputMesh)
{
    VaryingsToPS varyingsType;
    varyingsType.vmesh = VertMesh(inputMesh);
    #if (SHADERPASS == SHADERPASS_DBUFFER_MESH)
       MeshDecalsPositionZBias(varyingsType);
    #endif
    return PackVaryingsToPS(varyingsType);
}

#endif



            

            
                FragInputs BuildFragInputs(VertexToPixel input)
                {
                    UNITY_SETUP_INSTANCE_ID(input);
                    FragInputs output;
                    ZERO_INITIALIZE(FragInputs, output);
            
                    // Init to some default value to make the computer quiet (else it output 'divide by zero' warning even if value is not used).
                    // TODO: this is a really poor workaround, but the variable is used in a bunch of places
                    // to compute normals which are then passed on elsewhere to compute other values...
                    output.tangentToWorld = k_identity3x3;
                    output.positionSS = input.pos;       // input.positionCS is SV_Position
            
                    output.positionRWS = input.worldPos;
                    output.tangentToWorld = BuildTangentToWorld(input.worldTangent, input.worldNormal);
                    output.texCoord0 = input.texcoord0;
                    output.texCoord1 = input.texcoord1;
                    output.texCoord2 = input.texcoord2;
            
                    return output;
                }
            
               void BuildSurfaceData(FragInputs fragInputs, inout Surface surfaceDescription, float3 V, PositionInputs posInput, out SurfaceData surfaceData, out float3 bentNormalWS)
               {
                   // setup defaults -- these are used if the graph doesn't output a value
                   ZERO_INITIALIZE(SurfaceData, surfaceData);
        
                   // specularOcclusion need to be init ahead of decal to quiet the compiler that modify the SurfaceData struct
                   // however specularOcclusion can come from the graph, so need to be init here so it can be override.
                   surfaceData.specularOcclusion = 1.0;
        
                   // copy across graph values, if defined
                   surfaceData.baseColor =                 surfaceDescription.Albedo;
                   surfaceData.perceptualSmoothness =      surfaceDescription.Smoothness;
                   surfaceData.ambientOcclusion =          surfaceDescription.Occlusion;
                   surfaceData.specularOcclusion =         surfaceDescription.SpecularOcclusion;
                   surfaceData.metallic =                  surfaceDescription.Metallic;
                   surfaceData.subsurfaceMask =            surfaceDescription.SubsurfaceMask;
                   surfaceData.thickness =                 surfaceDescription.Thickness;
                   surfaceData.diffusionProfileHash =      asuint(surfaceDescription.DiffusionProfileHash);
                   #if _USESPECULAR
                      surfaceData.specularColor =             surfaceDescription.Specular;
                   #endif
                   surfaceData.coatMask =                  surfaceDescription.CoatMask;
                   surfaceData.anisotropy =                surfaceDescription.Anisotropy;
                   surfaceData.iridescenceMask =           surfaceDescription.IridescenceMask;
                   surfaceData.iridescenceThickness =      surfaceDescription.IridescenceThickness;
        
           #ifdef _HAS_REFRACTION
                   if (_EnableSSRefraction)
                   {
                       // surfaceData.ior =                       surfaceDescription.RefractionIndex;
                       // surfaceData.transmittanceColor =        surfaceDescription.RefractionColor;
                       // surfaceData.atDistance =                surfaceDescription.RefractionDistance;
        
                       surfaceData.transmittanceMask = (1.0 - surfaceDescription.Alpha);
                       surfaceDescription.Alpha = 1.0;
                   }
                   else
                   {
                       surfaceData.ior = 1.0;
                       surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
                       surfaceData.atDistance = 1.0;
                       surfaceData.transmittanceMask = 0.0;
                       surfaceDescription.Alpha = 1.0;
                   }
           #else
                   surfaceData.ior = 1.0;
                   surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
                   surfaceData.atDistance = 1.0;
                   surfaceData.transmittanceMask = 0.0;
           #endif
                
                   // These static material feature allow compile time optimization
                   surfaceData.materialFeatures = MATERIALFEATUREFLAGS_LIT_STANDARD;
           #ifdef _MATERIAL_FEATURE_SUBSURFACE_SCATTERING
                   surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SUBSURFACE_SCATTERING;
           #endif
           #ifdef _MATERIAL_FEATURE_TRANSMISSION
                   surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_TRANSMISSION;
           #endif
           #ifdef _MATERIAL_FEATURE_ANISOTROPY
                   surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_ANISOTROPY;
           #endif
                   // surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_CLEAR_COAT;
        
           #ifdef _MATERIAL_FEATURE_IRIDESCENCE
                   surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_IRIDESCENCE;
           #endif
           #ifdef _MATERIAL_FEATURE_SPECULAR_COLOR
                   surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SPECULAR_COLOR;
           #endif
        
           #if defined (_MATERIAL_FEATURE_SPECULAR_COLOR) && defined (_ENERGY_CONSERVING_SPECULAR)
                   // Require to have setup baseColor
                   // Reproduce the energy conservation done in legacy Unity. Not ideal but better for compatibility and users can unchek it
                   surfaceData.baseColor *= (1.0 - Max3(surfaceData.specularColor.r, surfaceData.specularColor.g, surfaceData.specularColor.b));
           #endif

        
                   // tangent-space normal
                   float3 normalTS = float3(0.0f, 0.0f, 1.0f);
                   normalTS = surfaceDescription.Normal;
        
                   // compute world space normal
                   #if !_WORLDSPACENORMAL
                      surfaceData.normalWS = mul(normalTS, fragInputs.tangentToWorld);
                   #else
                      surfaceData.normalWS = normalTS;  
                   #endif
                   surfaceData.geomNormalWS = fragInputs.tangentToWorld[2];
        
                   surfaceData.tangentWS = normalize(fragInputs.tangentToWorld[0].xyz);    // The tangent is not normalize in tangentToWorld for mikkt. TODO: Check if it expected that we normalize with Morten. Tag: SURFACE_GRADIENT
                   // surfaceData.tangentWS = TransformTangentToWorld(surfaceDescription.Tangent, fragInputs.tangentToWorld);
        
           #if HAVE_DECALS
                   if (_EnableDecals)
                   {
                       #if VERSION_GREATER_EQUAL(10,2)
                          DecalSurfaceData decalSurfaceData = GetDecalSurfaceData(posInput,  surfaceData.geomNormalWS, surfaceDescription.Alpha);
                          ApplyDecalToSurfaceData(decalSurfaceData,  surfaceData.geomNormalWS, surfaceData);
                       #else
                          DecalSurfaceData decalSurfaceData = GetDecalSurfaceData(posInput, surfaceDescription.Alpha);
                          ApplyDecalToSurfaceData(decalSurfaceData, surfaceData);
                       #endif
                   }
           #endif
        
                   bentNormalWS = surfaceData.normalWS;
               
                   surfaceData.tangentWS = Orthonormalize(surfaceData.tangentWS, surfaceData.normalWS);
        
        
                   // By default we use the ambient occlusion with Tri-ace trick (apply outside) for specular occlusion.
                   // If user provide bent normal then we process a better term
           #if defined(_SPECULAR_OCCLUSION_CUSTOM)
                   // Just use the value passed through via the slot (not active otherwise)
           #elif defined(_SPECULAR_OCCLUSION_FROM_AO_BENT_NORMAL)
                   // If we have bent normal and ambient occlusion, process a specular occlusion
                   surfaceData.specularOcclusion = GetSpecularOcclusionFromBentAO(V, bentNormalWS, surfaceData.normalWS, surfaceData.ambientOcclusion, PerceptualSmoothnessToPerceptualRoughness(surfaceData.perceptualSmoothness));
           #elif defined(_AMBIENT_OCCLUSION) && defined(_SPECULAR_OCCLUSION_FROM_AO)
                   surfaceData.specularOcclusion = GetSpecularOcclusionFromAmbientOcclusion(ClampNdotV(dot(surfaceData.normalWS, V)), surfaceData.ambientOcclusion, PerceptualSmoothnessToRoughness(surfaceData.perceptualSmoothness));
           #endif
        
           #ifdef _ENABLE_GEOMETRIC_SPECULAR_AA
                   surfaceData.perceptualSmoothness = GeometricNormalFiltering(surfaceData.perceptualSmoothness, fragInputs.tangentToWorld[2], surfaceDescription.SpecularAAScreenSpaceVariance, surfaceDescription.SpecularAAThreshold);
           #endif
        
           #ifdef DEBUG_DISPLAY
                   if (_DebugMipMapMode != DEBUGMIPMAPMODE_NONE)
                   {
                       // TODO: need to update mip info
                       surfaceData.metallic = 0;
                   }
        
                   // We need to call ApplyDebugToSurfaceData after filling the surfarcedata and before filling builtinData
                   // as it can modify attribute use for static lighting
                   ApplyDebugToSurfaceData(fragInputs.tangentToWorld, surfaceData);
           #endif
               }
        
               void GetSurfaceAndBuiltinData(VertexToPixel m2ps, FragInputs fragInputs, float3 V, inout PositionInputs posInput,
                     out SurfaceData surfaceData, out BuiltinData builtinData, inout Surface l, inout ShaderData d
                     #if NEED_FACING
                        , facing
                     #endif
                  )
               {
                 // Removed since crossfade does not work, probably needs extra material setup.   
                 //#ifdef LOD_FADE_CROSSFADE // enable dithering LOD transition if user select CrossFade transition in LOD group
                 //    uint3 fadeMaskSeed = asuint((int3)(V * _ScreenSize.xyx)); // Quantize V to _ScreenSize values
                 //    LODDitheringTransition(fadeMaskSeed, unity_LODFade.x);
                 //#endif
        
                 d = CreateShaderData(m2ps
                  #if NEED_FACING
                    , facing
                  #endif
                 );

                 

                 l = (Surface)0;

                 l.Albedo = half3(0.5, 0.5, 0.5);
                 l.Normal = float3(0,0,1);
                 l.Occlusion = 1;
                 l.Alpha = 1;

                 #ifdef _DEPTHOFFSET_ON
                    l.outputDepth = posInput.deviceDepth;
                 #endif

                 ChainSurfaceFunction(l, d);

                 #ifdef _DEPTHOFFSET_ON
                    posInput.deviceDepth = l.outputDepth;
                 #endif

                 #if _UNLIT
                     //l.Emission = l.Albedo;
                     //l.Albedo = 0;
                     l.Normal = half3(0,0,1);
                     l.Occlusion = 1;
                     l.Metallic = 0;
                     l.Specular = 0;
                 #endif

                 surfaceData.geomNormalWS = d.worldSpaceNormal;
                 surfaceData.tangentWS = d.worldSpaceTangent;
                 fragInputs.tangentToWorld = d.TBNMatrix;

                 float3 bentNormalWS;
                 BuildSurfaceData(fragInputs, l, V, posInput, surfaceData, bentNormalWS);

                 

                 InitBuiltinData(posInput, l.Alpha, bentNormalWS, -d.worldSpaceNormal, fragInputs.texCoord1, fragInputs.texCoord2, builtinData);

                 builtinData.emissiveColor = l.Emission;
        
        
                 #if (SHADERPASS == SHADERPASS_DISTORTION)
                     //builtinData.distortion = surfaceDescription.Distortion;
                     //builtinData.distortionBlur = surfaceDescription.DistortionBlur;
                     builtinData.distortion = float2(0.0, 0.0);
                     builtinData.distortionBlur = 0.0;
                 #else
                     builtinData.distortion = float2(0.0, 0.0);
                     builtinData.distortionBlur = 0.0;
                 #endif
        
                   PostInitBuiltinData(V, posInput, surfaceData, builtinData);
               }


            float4 Frag(PackedVaryingsToPS packedInput
               #if NEED_FACING
                  , bool facing : SV_IsFrontFace
               #endif

            ) : SV_Target
            {
                FragInputs input = BuildFragInputs(packedInput.vmesh);

                // input.positionSS is SV_Position
                PositionInputs posInput = GetPositionInput(input.positionSS.xy, _ScreenSize.zw, input.positionSS.z, input.positionSS.w, input.positionRWS);

            #ifdef VARYINGS_NEED_POSITION_WS
                float3 V = GetWorldSpaceNormalizeViewDir(input.positionRWS);
            #else
                // Unused
                float3 V = float3(1.0, 1.0, 1.0); // Avoid the division by 0
            #endif

                SurfaceData surfaceData;
                BuiltinData builtinData;
                Surface l;
                ShaderData d;
                GetSurfaceAndBuiltinData(packedInput.vmesh, input, V, posInput, surfaceData, builtinData, l, d
               #if NEED_FACING
                  , facing
               #endif
               );

                // no debug apply during light transport pass

                BSDFData bsdfData = ConvertSurfaceDataToBSDFData(input.positionSS.xy, surfaceData);
                LightTransportData lightTransportData = GetLightTransportData(surfaceData, builtinData, bsdfData);

                // This shader is call two times. Once for getting emissiveColor, the other time to get diffuseColor
                // We use unity_MetaFragmentControl to make the distinction.
                float4 res = float4(0.0, 0.0, 0.0, 1.0);

                if (unity_MetaFragmentControl.x)
                {
                    // Apply diffuseColor Boost from LightmapSettings.
                    // put abs here to silent a warning, no cost, no impact as color is assume to be positive.
                    res.rgb = clamp(pow(abs(lightTransportData.diffuseColor), saturate(unity_OneOverOutputBoost)), 0, unity_MaxOutputValue);
                }

                if (unity_MetaFragmentControl.y)
                {
                    // emissive use HDR format
                    res.rgb = lightTransportData.emissiveColor;
                }

                return res;
            }



            ENDHLSL
        }
        
              Pass
        {
            // based on HDLitPass.template
            Name "SceneSelectionPass"
            Tags { "LightMode" = "SceneSelectionPass" }
        
            ColorMask 0

            

            HLSLPROGRAM
        
            #pragma target 4.5
            #pragma only_renderers d3d11 ps4 xboxone vulkan metal switch
            //#pragma enable_d3d11_debug_symbols
        
            #pragma multi_compile_instancing
 
            #pragma multi_compile_local _ _ALPHATEST_ON


            #pragma shader_feature _SURFACE_TYPE_TRANSPARENT
            //#pragma shader_feature_local _ _BLENDMODE_ALPHA _BLENDMODE_ADD _BLENDMODE_PRE_MULTIPLY
        
            //-------------------------------------------------------------------------------------
            // Variant Definitions (active field translations to HDRP defines)
            //-------------------------------------------------------------------------------------
            // #define _MATERIAL_FEATURE_SUBSURFACE_SCATTERING 1
            // #define _MATERIAL_FEATURE_TRANSMISSION 1
            // #define _MATERIAL_FEATURE_ANISOTROPY 1
            // #define _MATERIAL_FEATURE_IRIDESCENCE 1
            // #define _MATERIAL_FEATURE_SPECULAR_COLOR 1
            // #define _ENABLE_FOG_ON_TRANSPARENT 1
            // #define _AMBIENT_OCCLUSION 1
            // #define _SPECULAR_OCCLUSION_FROM_AO 1
            // #define _SPECULAR_OCCLUSION_FROM_AO_BENT_NORMAL 1
            // #define _SPECULAR_OCCLUSION_CUSTOM 1
            // #define _ENERGY_CONSERVING_SPECULAR 1
            // #define _ENABLE_GEOMETRIC_SPECULAR_AA 1
            // #define _HAS_REFRACTION 1
            // #define _REFRACTION_PLANE 1
            // #define _REFRACTION_SPHERE 1
            // #define _DISABLE_DECALS 1
            // #define _DISABLE_SSR 1
            // #define _ADD_PRECOMPUTED_VELOCITY
            // #define _WRITE_TRANSPARENT_MOTION_VECTOR 1
            // #define _DEPTHOFFSET_ON 1
            // #define _BLENDMODE_PRESERVE_SPECULAR_LIGHTING 1

            #define SHADERPASS SHADERPASS_DEPTH_ONLY
            #define SCENESELECTIONPASS
            #pragma editor_sync_compilation
            #define RAYTRACING_SHADER_GRAPH_HIGH
            #define _PASSSCENESELECT 1

            
   #pragma shader_feature_local _ _OVERRIDE_OPACITY
   #pragma shader_feature_local _ _OVERRIDE_NORMAL
   #pragma shader_feature_local _ _OVERRIDE_MOS
   #pragma shader_feature_local _ _OVERRIDE_EMISSION


	#define _HAS_ALPHA_BLEND 1


   #define _HDRP 1
#define _BLENDMODE_ALPHA 1
#define _SURFACE_TYPE_TRANSPARENT 1


               #pragma vertex Vert
   #pragma fragment Frag
        
            
        
                  // useful conversion functions to make surface shader code just work

      #define UNITY_DECLARE_TEX2D(name) TEXTURE2D(name); SAMPLER(sampler##name);
      #define UNITY_DECLARE_TEX2D_NOSAMPLER(name) TEXTURE2D(name);
      #define UNITY_DECLARE_TEX2DARRAY(name) TEXTURE2D_ARRAY(name); SAMPLER(sampler##name);
      #define UNITY_DECLARE_TEX2DARRAY_NOSAMPLER(tex) TEXTURE2D_ARRAY(tex);

      #define UNITY_SAMPLE_TEX2DARRAY(tex,coord)            SAMPLE_TEXTURE2D_ARRAY(tex, sampler##tex, coord.xy, coord.z)
      #define UNITY_SAMPLE_TEX2DARRAY_LOD(tex,coord,lod)    SAMPLE_TEXTURE2D_ARRAY_LOD(tex, sampler##tex, coord.xy, coord.z, lod)
      #define UNITY_SAMPLE_TEX2D(tex, coord)                SAMPLE_TEXTURE2D(tex, sampler##tex, coord)
      #define UNITY_SAMPLE_TEX2D_SAMPLER(tex, samp, coord)  SAMPLE_TEXTURE2D(tex, sampler##samp, coord)

      #define UNITY_SAMPLE_TEX2D_LOD(tex,coord, lod)   SAMPLE_TEXTURE2D_LOD(tex, sampler_##tex, coord, lod)
      #define UNITY_SAMPLE_TEX2D_SAMPLER_LOD(tex,samplertex,coord, lod) SAMPLE_TEXTURE2D_LOD (tex, sampler##samplertex,coord, lod)

      #if defined(UNITY_COMPILER_HLSL)
         #define UNITY_INITIALIZE_OUTPUT(type,name) name = (type)0;
      #else
         #define UNITY_INITIALIZE_OUTPUT(type,name)
      #endif

      #define sampler2D_float sampler2D
      #define sampler2D_half sampler2D

      #undef WorldNormalVector
      #define WorldNormalVector(data, normal) mul(normal, data.TBNMatrix)

      #define UnityObjectToWorldNormal(normal) mul(GetObjectToWorldMatrix(), normal)




// HDRP Adapter stuff


            // If we use subsurface scattering, enable output split lighting (for forward pass)
            #if defined(_MATERIAL_FEATURE_SUBSURFACE_SCATTERING) && !defined(_SURFACE_TYPE_TRANSPARENT)
            #define OUTPUT_SPLIT_LIGHTING
            #endif

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Version.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
        
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/NormalSurfaceGradient.hlsl"
        
            // define FragInputs structure
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"
            #if (SHADER_LIBRARY_VERSION_MAJOR >= 10)
               #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/Functions.hlsl"
            #endif


        

            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
        #ifdef DEBUG_DISPLAY
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Debug/DebugDisplay.hlsl"
        #endif
        
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
        
        #if (SHADERPASS == SHADERPASS_FORWARD)
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/Lighting.hlsl"
        
            #define HAS_LIGHTLOOP
        
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/LightLoop/LightLoopDef.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/Lit.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/LightLoop/LightLoop.hlsl"
        #else
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/Lit.hlsl"
        #endif
        
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/BuiltinUtilities.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/MaterialUtilities.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Decal/DecalUtilities.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/LitDecalData.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphFunctions.hlsl"
        
            // Used by SceneSelectionPass
            int _ObjectId;
            int _PassValue;
        
           
            // data across stages, stripped like the above.
            struct VertexToPixel
            {
               float4 pos : SV_POSITION;
               float3 worldPos : TEXCOORD0;
               float3 worldNormal : TEXCOORD1;
               float4 worldTangent : TEXCOORD2;
               float4 texcoord0 : TEXCCOORD3;
               float4 texcoord1 : TEXCCOORD4;
               float4 texcoord2 : TEXCCOORD5;
               // float4 texcoord3 : TEXCCOORD6;
               // float4 screenPos : TEXCOORD7;
               // float4 vertexColor : COLOR;

               // float4 extraV2F0 : TEXCOORD8;
               // float4 extraV2F1 : TEXCOORD9;
               // float4 extraV2F2 : TEXCOORD10;
               // float4 extraV2F3 : TEXCOORD11;
               // float4 extraV2F4 : TEXCOORD12;
               // float4 extraV2F5 : TEXCOORD13;
               // float4 extraV2F6 : TEXCOORD14;
               // float4 extraV2F7 : TEXCOORD15;

               #if UNITY_ANY_INSTANCING_ENABLED
                  uint instanceID : INSTANCEID_SEMANTIC;
               #endif // UNITY_ANY_INSTANCING_ENABLED

               UNITY_VERTEX_OUTPUT_STEREO
            };
      
  
            
            
            // data describing the user output of a pixel
            struct Surface
            {
               half3 Albedo;
               half Height;
               half3 Normal;
               half Smoothness;
               half3 Emission;
               half Metallic;
               half3 Specular;
               half Occlusion;
               half SpecularPower; // for simple lighting
               half Alpha;
               float outputDepth; // if written, SV_Depth semantic is used. ShaderData.clipPos.z is unused value
               // HDRP Only
               half SpecularOcclusion;
               half SubsurfaceMask;
               half Thickness;
               half CoatMask;
               half CoatSmoothness;
               half Anisotropy;
               half IridescenceMask;
               half IridescenceThickness;
               int DiffusionProfileHash;
            };

            // Data the user declares in blackboard blocks
            struct Blackboard
            {
                
                float blackboardDummyData;
            };

            // data the user might need, this will grow to be big. But easy to strip
            struct ShaderData
            {
               float4 clipPos; // SV_POSITION
               float3 localSpacePosition;
               float3 localSpaceNormal;
               float3 localSpaceTangent;
        
               float3 worldSpacePosition;
               float3 worldSpaceNormal;
               float3 worldSpaceTangent;
               float tangentSign;

               float3 worldSpaceViewDir;
               float3 tangentSpaceViewDir;

               float4 texcoord0;
               float4 texcoord1;
               float4 texcoord2;
               float4 texcoord3;

               float2 screenUV;
               float4 screenPos;

               float4 vertexColor;
               bool isFrontFace;

               float4 extraV2F0;
               float4 extraV2F1;
               float4 extraV2F2;
               float4 extraV2F3;
               float4 extraV2F4;
               float4 extraV2F5;
               float4 extraV2F6;
               float4 extraV2F7;

               float3x3 TBNMatrix;
               Blackboard blackboard;
            };

            struct VertexData
            {
               #if SHADER_TARGET > 30
               // uint vertexID : SV_VertexID;
               #endif
               float4 vertex : POSITION;
               float3 normal : NORMAL;
               float4 tangent : TANGENT;
               float4 texcoord0 : TEXCOORD0;
               float4 texcoord1 : TEXCOORD1;
               float4 texcoord2 : TEXCOORD2;
               // float4 texcoord3 : TEXCOORD3;
               // float4 vertexColor : COLOR;

               #if _HDRP && (_PASSMOTIONVECTOR || (_PASSFORWARD && defined(_WRITE_TRANSPARENT_MOTION_VECTOR)))
                  float3 previousPositionOS : TEXCOORD4; // Contain previous transform position (in case of skinning for example)
                  #if defined (_ADD_PRECOMPUTED_VELOCITY)
                     float3 precomputedVelocity    : TEXCOORD5; // Add Precomputed Velocity (Alembic computes velocities on runtime side).
                  #endif
               #endif

               UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct TessVertex 
            {
               float4 vertex : INTERNALTESSPOS;
               float3 normal : NORMAL;
               float4 tangent : TANGENT;
               float4 texcoord0 : TEXCOORD0;
               float4 texcoord1 : TEXCOORD1;
               float4 texcoord2 : TEXCOORD2;
               // float4 texcoord3 : TEXCOORD3;
               // float4 vertexColor : COLOR;

               
               // float4 extraV2F0 : TEXCOORD4;
               // float4 extraV2F1 : TEXCOORD5;
               // float4 extraV2F2 : TEXCOORD6;
               // float4 extraV2F3 : TEXCOORD7;
               // float4 extraV2F4 : TEXCOORD8;
               // float4 extraV2F5 : TEXCOORD9;
               // float4 extraV2F6 : TEXCOORD10;
               // float4 extraV2F7 : TEXCOORD11;

               #if _HDRP && (_PASSMOTIONVECTOR || (_PASSFORWARD && defined(_WRITE_TRANSPARENT_MOTION_VECTOR)))
                  float3 previousPositionOS : TEXCOORD12; // Contain previous transform position (in case of skinning for example)
                  #if defined (_ADD_PRECOMPUTED_VELOCITY)
                     float3 precomputedVelocity : TEXCOORD13;
                  #endif
               #endif

               UNITY_VERTEX_INPUT_INSTANCE_ID
               UNITY_VERTEX_OUTPUT_STEREO
            };

            struct ExtraV2F
            {
               float4 extraV2F0;
               float4 extraV2F1;
               float4 extraV2F2;
               float4 extraV2F3;
               float4 extraV2F4;
               float4 extraV2F5;
               float4 extraV2F6;
               float4 extraV2F7;
               Blackboard blackboard;
            };


            float3 WorldToTangentSpace(ShaderData d, float3 normal)
            {
               return mul(d.TBNMatrix, normal);
            }

            float3 TangentToWorldSpace(ShaderData d, float3 normal)
            {
               return mul(normal, d.TBNMatrix);
            }

            // in this case, make standard more like SRPs, because we can't fix
            // unity_WorldToObject in HDRP, since it already does macro-fu there

            #if _STANDARD
               float3 TransformWorldToObject(float3 p) { return mul(unity_WorldToObject, float4(p, 1)); };
               float3 TransformObjectToWorld(float3 p) { return mul(unity_ObjectToWorld, float4(p, 1)); };
               float4 TransformWorldToObject(float4 p) { return mul(unity_WorldToObject, p); };
               float4 TransformObjectToWorld(float4 p) { return mul(unity_ObjectToWorld, p); };
               float4x4 GetWorldToObjectMatrix() { return unity_WorldToObject; }
               float4x4 GetObjectToWorldMatrix() { return unity_ObjectToWorld; }
               #if (defined(SHADER_API_D3D11) || defined(SHADER_API_XBOXONE) || defined(UNITY_COMPILER_HLSLCC) || defined(SHADER_API_PSSL) || (SHADER_TARGET_SURFACE_ANALYSIS && !SHADER_TARGET_SURFACE_ANALYSIS_MOJOSHADER))
                 #define UNITY_SAMPLE_TEX2D_LOD(tex,coord, lod) tex.SampleLevel (sampler##tex,coord, lod)
                 #define UNITY_SAMPLE_TEX2D_SAMPLER_LOD(tex,samplertex,coord, lod) tex.SampleLevel (sampler##samplertex,coord, lod)
              #else
                 #define UNITY_SAMPLE_TEX2D_LOD(tex,coord,lod) tex2D (tex,coord,0,lod)
                 #define UNITY_SAMPLE_TEX2D_SAMPLER_LOD(tex,samplertex,coord,lod) tex2D (tex,coord,0,lod)
              #endif

               #undef GetObjectToWorldMatrix()
               #undef GetWorldToObjectMatrix()
               #undef GetWorldToViewMatrix()
               #undef UNITY_MATRIX_I_V
               #undef UNITY_MATRIX_P
               #undef GetWorldToHClipMatrix()
               #undef GetObjectToWorldMatrix()V
               #undef UNITY_MATRIX_T_MV
               #undef UNITY_MATRIX_IT_MV
               #undef GetObjectToWorldMatrix()VP

               #define GetObjectToWorldMatrix()     unity_ObjectToWorld
               #define GetWorldToObjectMatrix()   unity_WorldToObject
               #define GetWorldToViewMatrix()     unity_MatrixV
               #define UNITY_MATRIX_I_V   unity_MatrixInvV
               #define GetViewToHClipMatrix()     OptimizeProjectionMatrix(glstate_matrix_projection)
               #define GetWorldToHClipMatrix()    unity_MatrixVP
               #define GetObjectToWorldMatrix()V    mul(GetWorldToViewMatrix(), GetObjectToWorldMatrix())
               #define UNITY_MATRIX_T_MV  transpose(GetObjectToWorldMatrix()V)
               #define UNITY_MATRIX_IT_MV transpose(mul(GetWorldToObjectMatrix(), UNITY_MATRIX_I_V))
               #define GetObjectToWorldMatrix()VP   mul(GetWorldToHClipMatrix(), GetObjectToWorldMatrix())


            #endif

            float3 GetCameraWorldPosition()
            {
               #if _HDRP
                  return GetCameraRelativePositionWS(_WorldSpaceCameraPos);
               #else
                  return _WorldSpaceCameraPos;
               #endif
            }

            #if _GRABPASSUSED
               #if _STANDARD
                  TEXTURE2D(%GRABTEXTURE%);
                  SAMPLER(sampler_%GRABTEXTURE%);
               #endif

               half3 GetSceneColor(float2 uv)
               {
                  #if _STANDARD
                     return SAMPLE_TEXTURE2D(%GRABTEXTURE%, sampler_%GRABTEXTURE%, uv).rgb;
                  #else
                     return SHADERGRAPH_SAMPLE_SCENE_COLOR(uv);
                  #endif
               }
            #endif



            #if _STANDARD
               sampler2D _CameraDepthTexture;
               float GetSceneDepth(float2 uv) { return SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv); }
               float GetLinear01Depth(float2 uv) { return Linear01Depth(GetSceneDepth(uv)); }
               float GetLinearEyeDepth(float2 uv) { return LinearEyeDepth(GetSceneDepth(uv)); } 
            #else
               float GetSceneDepth(float2 uv) { return SHADERGRAPH_SAMPLE_SCENE_DEPTH(uv); }
               float GetLinear01Depth(float2 uv) { return Linear01Depth(GetSceneDepth(uv), _ZBufferParams); }
               float GetLinearEyeDepth(float2 uv) { return LinearEyeDepth(GetSceneDepth(uv), _ZBufferParams); } 
            #endif

            float3 GetWorldPositionFromDepthBuffer(float2 uv, float3 worldSpaceViewDir)
            {
               float eye = GetLinearEyeDepth(uv);
               float3 camView = mul((float3x3)GetObjectToWorldMatrix(), transpose(mul(GetWorldToObjectMatrix(), UNITY_MATRIX_I_V)) [2].xyz);

               float dt = dot(worldSpaceViewDir, camView);
               float3 div = worldSpaceViewDir/dt;
               float3 wpos = (eye * div) + GetCameraWorldPosition();
               return wpos;
            }

            #if _STANDARD
               sampler2D _CameraDepthNormalsTexture;
               float3 GetSceneNormal(float2 uv, float3 worldSpaceViewDir)
               {
                  float4 depthNorms = tex2D(_CameraDepthNormalsTexture, uv);
                  float3 norms = DecodeViewNormalStereo(depthNorms);
                  norms = mul((float3x3)GetWorldToViewMatrix(), norms) * 0.5 + 0.5;
                  return norms;
               }
            #elif _HDRP
               float3 GetSceneNormal(float2 uv, float3 worldSpaceViewDir)
               {
                  NormalData nd;
                  DecodeFromNormalBuffer(_ScreenSize.xy * uv, nd);
                  return nd.normalWS;
               }
            #elif _URP
               #if (SHADER_LIBRARY_VERSION_MAJOR >= 10)
                  #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"
               #endif

               float3 GetSceneNormal(float2 uv, float3 worldSpaceViewDir)
               {
                  #if (SHADER_LIBRARY_VERSION_MAJOR >= 10)
                     return SampleSceneNormals(uv);
                  #else
                     float3 wpos = GetWorldPositionFromDepthBuffer(uv, worldSpaceViewDir);
                     return normalize(-cross(ddx(wpos), ddy(wpos))) * 0.5 + 0.5;
                  #endif

                }
             #endif

             #if _HDRP

               half3 UnpackNormalmapRGorAG(half4 packednormal)
               {
                     // This do the trick
                  packednormal.x *= packednormal.w;

                  half3 normal;
                  normal.xy = packednormal.xy * 2 - 1;
                  normal.z = sqrt(1 - saturate(dot(normal.xy, normal.xy)));
                  return normal;
               }
               half3 UnpackNormal(half4 packednormal)
               {
                  #if defined(UNITY_NO_DXT5nm)
                     return packednormal.xyz * 2 - 1;
                  #else
                     return UnpackNormalmapRGorAG(packednormal);
                  #endif
               }
               #endif
               #if _HDRP || _URP

               half3 UnpackScaleNormal(half4 packednormal, half scale)
               {
                 #ifndef UNITY_NO_DXT5nm
                   // Unpack normal as DXT5nm (1, y, 1, x) or BC5 (x, y, 0, 1)
                   // Note neutral texture like "bump" is (0, 0, 1, 1) to work with both plain RGB normal and DXT5nm/BC5
                   packednormal.x *= packednormal.w;
                 #endif
                   half3 normal;
                   normal.xy = (packednormal.xy * 2 - 1) * scale;
                   normal.z = sqrt(1 - saturate(dot(normal.xy, normal.xy)));
                   return normal;
               }	

             #endif


            void GetSun(out float3 lightDir, out float3 color)
            {
               lightDir = float3(0.5, 0.5, 0);
               color = 1;
               #if _HDRP
                  if (_DirectionalLightCount > 0)
                  {
                     DirectionalLightData light = _DirectionalLightDatas[0];
                     lightDir = -light.forward.xyz;
                     color = light.color;
                  }
               #elif _STANDARD
			         lightDir = normalize(_WorldSpaceLightPos0.xyz);
                  color = _LightColor0.rgb;
               #elif _URP
	               Light light = GetMainLight();
	               lightDir = light.direction;
	               color = light.color;
               #endif
            }




            CBUFFER_START(UnityPerMaterial)

               float _StencilRef;
               float _StencilWriteMask;
               float _StencilRefDepth;
               float _StencilWriteMaskDepth;
               float _StencilRefMV;
               float _StencilWriteMaskMV;
               float _StencilRefDistortionVec;
               float _StencilWriteMaskDistortionVec;
               float _StencilWriteMaskGBuffer;
               float _StencilRefGBuffer;
               float _ZTestGBuffer;
               float _RequireSplitLighting;
               float _ReceivesSSR;
               float _ZWrite;
               float _TransparentSortPriority;
               float _ZTestDepthEqualForOpaque;
               float _ZTestTransparent;
               float _TransparentBackfaceEnable;
               float _AlphaCutoffEnable;
               float _UseShadowThreshold;

               
	float4 _Color;
	float  _BumpScale;
	float  _Metallic;
	float  _GlossMapScale;
	float3 _Emission;
	float  _Tiling;
	float  _UseUV2;
	float  _UseUV2Alt;





            CBUFFER_END

            

            

            #ifdef unity_WorldToObject
#undef unity_WorldToObject
#endif
#ifdef unity_ObjectToWorld
#undef unity_ObjectToWorld
#endif
#define unity_ObjectToWorld GetObjectToWorldMatrix()
#define unity_WorldToObject GetWorldToObjectMatrix()

	TEXTURE2D(_MainTex);
	SAMPLER(sampler_MainTex);
	TEXTURE2D(_BumpMap);
	SAMPLER(sampler_BumpMap);
	TEXTURE2D(_MetallicGlossMap);
	SAMPLER(sampler_MetallicGlossMap);
	TEXTURE2D(_EmissionMap);
	SAMPLER(sampler_EmissionMap);

	TEXTURE2D(_AlbedoTex);
	SAMPLER(sampler_AlbedoTex);
	TEXTURE2D(_OpacityTex);
	SAMPLER(sampler_OpacityTex);
	TEXTURE2D(_NormalTex);
	SAMPLER(sampler_NormalTex);
	TEXTURE2D(_EmissionTex);
	SAMPLER(sampler_EmissionTex);
	TEXTURE2D(_MosTex);
	SAMPLER(sampler_MosTex);

	void Ext_ModifyVertex0(inout VertexData v, inout ExtraV2F d)
	{
		float4 first  = lerp(v.texcoord0, v.texcoord1, _UseUV2);
		float4 second = lerp(v.texcoord0, v.texcoord1, _UseUV2Alt);

		v.texcoord0 = first * _Tiling;
		v.texcoord1 = second;
	}

	void Ext_SurfaceFunction0(inout Surface o, ShaderData d)
	{
		float4 texMain = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, d.texcoord0);
		float4 gloss   = SAMPLE_TEXTURE2D(_MetallicGlossMap, sampler_MetallicGlossMap, d.texcoord0);
		float4 bump    = SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, d.texcoord0);
		float4 glow    = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, d.texcoord0);
	#if !_HAS_ALPHA_BLEND
		clip(texMain.a * _Color.a - 0.5f);
	#endif
		o.Albedo     = texMain.rgb * _Color.rgb;
		o.Normal     = UnpackScaleNormal(bump, _BumpScale);
		o.Metallic   = gloss.r * _Metallic;
		o.Occlusion  = gloss.g;
		o.Smoothness = gloss.b * _GlossMapScale;
		o.Emission   = glow.rgb * _Emission;
		o.Alpha      = texMain.a * _Color.a;

		// Override albedo?
		float4 albedo = SAMPLE_TEXTURE2D(_AlbedoTex, sampler_AlbedoTex, d.texcoord1);
		o.Albedo = (1.0f - albedo.a) * o.Albedo + albedo.rgb;

	#if _OVERRIDE_OPACITY
		float4 opacity = SAMPLE_TEXTURE2D(_OpacityTex, sampler_OpacityTex, d.texcoord1);
		o.Alpha = (1.0f - opacity.a) * o.Alpha + opacity.r;
	#endif

	#if _OVERRIDE_NORMAL
		float4 normal = SAMPLE_TEXTURE2D(_NormalTex, sampler_NormalTex, d.texcoord1);
		o.Normal = (1.0f - normal.a) * o.Normal + normal.r;
	#endif
	
	#if _OVERRIDE_MOS
		float4 mos = SAMPLE_TEXTURE2D(_MosTex, sampler_MosTex, d.texcoord1);
		o.Metallic   = (1.0f - mos.a) * o.Metallic + mos.r;
		o.Occlusion  = (1.0f - mos.a) * o.Metallic + mos.g;
		o.Smoothness = (1.0f - mos.a) * o.Metallic + mos.b;
	#endif
	
	#if _OVERRIDE_EMISSION
		float4 emission = SAMPLE_TEXTURE2D(_EmissionTex, sampler_EmissionTex, d.texcoord1);
		o.Emission = (1.0f - emission.a) * o.Emission + emission.rgb;
	#endif
	}





        
            void ChainSurfaceFunction(inout Surface l, inout ShaderData d)
            {
                  Ext_SurfaceFunction0(l, d);
                 // Ext_SurfaceFunction1(l, d);
                 // Ext_SurfaceFunction2(l, d);
                 // Ext_SurfaceFunction3(l, d);
                 // Ext_SurfaceFunction4(l, d);
                 // Ext_SurfaceFunction5(l, d);
                 // Ext_SurfaceFunction6(l, d);
                 // Ext_SurfaceFunction7(l, d);
                 // Ext_SurfaceFunction8(l, d);
                 // Ext_SurfaceFunction9(l, d);
		           // Ext_SurfaceFunction10(l, d);
                 // Ext_SurfaceFunction11(l, d);
                 // Ext_SurfaceFunction12(l, d);
                 // Ext_SurfaceFunction13(l, d);
                 // Ext_SurfaceFunction14(l, d);
                 // Ext_SurfaceFunction15(l, d);
                 // Ext_SurfaceFunction16(l, d);
                 // Ext_SurfaceFunction17(l, d);
                 // Ext_SurfaceFunction18(l, d);
		           // Ext_SurfaceFunction19(l, d);
            }

            void ChainModifyVertex(inout VertexData v, inout VertexToPixel v2p)
            {
                 ExtraV2F d;
                 ZERO_INITIALIZE(ExtraV2F, d);
                 ZERO_INITIALIZE(Blackboard, d.blackboard);

                   Ext_ModifyVertex0(v, d);
                 // Ext_ModifyVertex1(v, d);
                 // Ext_ModifyVertex2(v, d);
                 // Ext_ModifyVertex3(v, d);
                 // Ext_ModifyVertex4(v, d);
                 // Ext_ModifyVertex5(v, d);
                 // Ext_ModifyVertex6(v, d);
                 // Ext_ModifyVertex7(v, d);
                 // Ext_ModifyVertex8(v, d);
                 // Ext_ModifyVertex9(v, d);
                 // Ext_ModifyVertex10(v, d);
                 // Ext_ModifyVertex11(v, d);
                 // Ext_ModifyVertex12(v, d);
                 // Ext_ModifyVertex13(v, d);
                 // Ext_ModifyVertex14(v, d);
                 // Ext_ModifyVertex15(v, d);
                 // Ext_ModifyVertex16(v, d);
                 // Ext_ModifyVertex17(v, d);
                 // Ext_ModifyVertex18(v, d);
                 // Ext_ModifyVertex19(v, d);
		
                 // v2p.extraV2F0 = d.extraV2F0;
                 // v2p.extraV2F1 = d.extraV2F1;
                 // v2p.extraV2F2 = d.extraV2F2;
                 // v2p.extraV2F3 = d.extraV2F3;
                 // v2p.extraV2F4 = d.extraV2F4;
                 // v2p.extraV2F5 = d.extraV2F5;
                 // v2p.extraV2F6 = d.extraV2F6;
                 // v2p.extraV2F7 = d.extraV2F7;
            }

            void ChainModifyTessellatedVertex(inout VertexData v, inout VertexToPixel v2p)
            {
               ExtraV2F d;
               ZERO_INITIALIZE(ExtraV2F, d);
               ZERO_INITIALIZE(Blackboard, d.blackboard);
               // d.extraV2F0 = v2p.extraV2F0;
               // d.extraV2F1 = v2p.extraV2F1;
               // d.extraV2F2 = v2p.extraV2F2;
               // d.extraV2F3 = v2p.extraV2F3;
               // d.extraV2F0 = v2p.extraV2F4;
               // d.extraV2F1 = v2p.extraV2F5;
               // d.extraV2F2 = v2p.extraV2F6;
               // d.extraV2F3 = v2p.extraV2F7;

               // Ext_ModifyTessellatedVertex0(v, d);
               // Ext_ModifyTessellatedVertex1(v, d);
               // Ext_ModifyTessellatedVertex2(v, d);
               // Ext_ModifyTessellatedVertex3(v, d);
               // Ext_ModifyTessellatedVertex4(v, d);
               // Ext_ModifyTessellatedVertex5(v, d);
               // Ext_ModifyTessellatedVertex6(v, d);
               // Ext_ModifyTessellatedVertex7(v, d);
               // Ext_ModifyTessellatedVertex8(v, d);
               // Ext_ModifyTessellatedVertex9(v, d);
               // Ext_ModifyTessellatedVertex10(v, d);
               // Ext_ModifyTessellatedVertex11(v, d);
               // Ext_ModifyTessellatedVertex12(v, d);
               // Ext_ModifyTessellatedVertex13(v, d);
               // Ext_ModifyTessellatedVertex14(v, d);
               // Ext_ModifyTessellatedVertex15(v, d);
               // Ext_ModifyTessellatedVertex16(v, d);
               // Ext_ModifyTessellatedVertex17(v, d);
               // Ext_ModifyTessellatedVertex18(v, d);
               // Ext_ModifyTessellatedVertex19(v, d);

               // v2p.extraV2F0 = d.extraV2F0;
               // v2p.extraV2F1 = d.extraV2F1;
               // v2p.extraV2F2 = d.extraV2F2;
               // v2p.extraV2F3 = d.extraV2F3;
               // v2p.extraV2F0 = d.extraV2F4;
               // v2p.extraV2F1 = d.extraV2F5;
               // v2p.extraV2F2 = d.extraV2F6;
               // v2p.extraV2F3 = d.extraV2F7;
            }

            void ChainFinalColorForward(inout Surface l, inout ShaderData d, inout half4 color)
            {
               //   Ext_FinalColorForward0(l, d, color);
               //   Ext_FinalColorForward1(l, d, color);
               //   Ext_FinalColorForward2(l, d, color);
               //   Ext_FinalColorForward3(l, d, color);
               //   Ext_FinalColorForward4(l, d, color);
               //   Ext_FinalColorForward5(l, d, color);
               //   Ext_FinalColorForward6(l, d, color);
               //   Ext_FinalColorForward7(l, d, color);
               //   Ext_FinalColorForward8(l, d, color);
               //   Ext_FinalColorForward9(l, d, color);
               //  Ext_FinalColorForward10(l, d, color);
               //  Ext_FinalColorForward11(l, d, color);
               //  Ext_FinalColorForward12(l, d, color);
               //  Ext_FinalColorForward13(l, d, color);
               //  Ext_FinalColorForward14(l, d, color);
               //  Ext_FinalColorForward15(l, d, color);
               //  Ext_FinalColorForward16(l, d, color);
               //  Ext_FinalColorForward17(l, d, color);
               //  Ext_FinalColorForward18(l, d, color);
               //  Ext_FinalColorForward19(l, d, color);
            }

            void ChainFinalGBufferStandard(inout Surface s, inout ShaderData d, inout half4 GBuffer0, inout half4 GBuffer1, inout half4 GBuffer2, inout half4 outEmission, inout half4 outShadowMask)
            {
               //   Ext_FinalGBufferStandard0(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
               //   Ext_FinalGBufferStandard1(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
               //   Ext_FinalGBufferStandard2(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
               //   Ext_FinalGBufferStandard3(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
               //   Ext_FinalGBufferStandard4(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
               //   Ext_FinalGBufferStandard5(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
               //   Ext_FinalGBufferStandard6(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
               //   Ext_FinalGBufferStandard7(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
               //   Ext_FinalGBufferStandard8(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
               //   Ext_FinalGBufferStandard9(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
               //  Ext_FinalGBufferStandard10(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
               //  Ext_FinalGBufferStandard11(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
               //  Ext_FinalGBufferStandard12(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
               //  Ext_FinalGBufferStandard13(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
               //  Ext_FinalGBufferStandard14(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
               //  Ext_FinalGBufferStandard15(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
               //  Ext_FinalGBufferStandard16(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
               //  Ext_FinalGBufferStandard17(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
               //  Ext_FinalGBufferStandard18(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
               //  Ext_FinalGBufferStandard19(s, d, GBuffer0, GBuffer1, GBuffer2, outEmission, outShadowMask);
            }



            

         ShaderData CreateShaderData(VertexToPixel i
                  #if NEED_FACING
                     , bool facing
                  #endif
         )
         {
            ShaderData d = (ShaderData)0;
            d.clipPos = i.pos;
            d.worldSpacePosition = i.worldPos;

            d.worldSpaceNormal = normalize(i.worldNormal);
            d.worldSpaceTangent = normalize(i.worldTangent.xyz);
            d.tangentSign = i.worldTangent.w;
            float3 bitangent = cross(i.worldTangent.xyz, i.worldNormal) * d.tangentSign * -1;
            

            d.TBNMatrix = float3x3(d.worldSpaceTangent, bitangent, d.worldSpaceNormal);
            d.worldSpaceViewDir = normalize(_WorldSpaceCameraPos - i.worldPos);

            d.tangentSpaceViewDir = mul(d.TBNMatrix, d.worldSpaceViewDir);
             d.texcoord0 = i.texcoord0;
             d.texcoord1 = i.texcoord1;
            // d.texcoord2 = i.texcoord2;
            // d.texcoord3 = i.texcoord3;
            // d.isFrontFace = facing;
            // d.vertexColor = i.vertexColor;

            // these rarely get used, so we back transform them. Usually will be stripped.
            #if _HDRP
                // d.localSpacePosition = mul(unity_WorldToObject, float4(GetCameraRelativePositionWS(i.worldPos), 1)).xyz;
            #else
                // d.localSpacePosition = mul(unity_WorldToObject, float4(i.worldPos, 1)).xyz;
            #endif
            // d.localSpaceNormal = normalize(mul(unity_WorldToObject, float4(i.worldNormal, 1)).xyz);
            // d.localSpaceTangent = normalize(mul(unity_WorldToObject, float4(i.worldTangent.xyz, 1)).xyz);

            // d.screenPos = i.screenPos;
            // d.screenUV = (i.screenPos.xy / i.screenPos.w);

            // d.extraV2F0 = i.extraV2F0;
            // d.extraV2F1 = i.extraV2F1;
            // d.extraV2F2 = i.extraV2F2;
            // d.extraV2F3 = i.extraV2F3;
            // d.extraV2F4 = i.extraV2F4;
            // d.extraV2F5 = i.extraV2F5;
            // d.extraV2F6 = i.extraV2F6;
            // d.extraV2F7 = i.extraV2F7;

            return d;
         }
         

            

struct VaryingsToPS
{
   VertexToPixel vmesh;
   #ifdef VARYINGS_NEED_PASS
      VaryingsPassToPS vpass;
   #endif
};

struct PackedVaryingsToPS
{
   #ifdef VARYINGS_NEED_PASS
      PackedVaryingsPassToPS vpass;
   #endif
   VertexToPixel vmesh;

   UNITY_VERTEX_OUTPUT_STEREO
};

PackedVaryingsToPS PackVaryingsToPS(VaryingsToPS input)
{
   PackedVaryingsToPS output = (PackedVaryingsToPS)0;
   output.vmesh = input.vmesh;
   #ifdef VARYINGS_NEED_PASS
      output.vpass = PackVaryingsPassToPS(input.vpass);
   #endif

   UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
   return output;
}




VertexToPixel VertMesh(VertexData input)
{
    VertexToPixel output = (VertexToPixel)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);

    
    ChainModifyVertex(input, output);


    // This return the camera relative position (if enable)
    float3 positionRWS = TransformObjectToWorld(input.vertex.xyz);
    float3 normalWS = TransformObjectToWorldNormal(input.normal);
    float4 tangentWS = float4(TransformObjectToWorldDir(input.tangent.xyz), input.tangent.w);


    output.worldPos = GetAbsolutePositionWS(positionRWS);
    output.pos = TransformWorldToHClip(positionRWS);
    output.worldNormal = normalWS;
    output.worldTangent = tangentWS;


    output.texcoord0 = input.texcoord0;
    output.texcoord1 = input.texcoord1;
    output.texcoord2 = input.texcoord2;
    // output.texcoord3 = input.texcoord3;
    // output.vertexColor = input.vertexColor;
    // output.screenPos = ComputeScreenPos(output.pos, _ProjectionParams.x);

    return output;
}


#if (SHADERPASS == SHADERPASS_DBUFFER_MESH)
void MeshDecalsPositionZBias(inout VaryingsToPS input)
{
#if defined(UNITY_REVERSED_Z)
    input.vmesh.pos.z -= _DecalMeshDepthBias;
#else
    input.vmesh.pos.z += _DecalMeshDepthBias;
#endif
}
#endif


#if (SHADERPASS == SHADERPASS_LIGHT_TRANSPORT)

// This was not in constant buffer in original unity, so keep outiside. But should be in as ShaderRenderPass frequency
float unity_OneOverOutputBoost;
float unity_MaxOutputValue;

CBUFFER_START(UnityMetaPass)
// x = use uv1 as raster position
// y = use uv2 as raster position
bool4 unity_MetaVertexControl;

// x = return albedo
// y = return normal
bool4 unity_MetaFragmentControl;
CBUFFER_END

PackedVaryingsToPS Vert(VertexData inputMesh)
{
    VaryingsToPS output = (VaryingsToPS)0;
    output.vmesh = (VertexToPixel)0;

    UNITY_SETUP_INSTANCE_ID(inputMesh);
    UNITY_TRANSFER_INSTANCE_ID(inputMesh, output.vmesh);

    // Output UV coordinate in vertex shader
    float2 uv = float2(0.0, 0.0);

    if (unity_MetaVertexControl.x)
    {
        uv = inputMesh.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
    }
    else if (unity_MetaVertexControl.y)
    {
        uv = inputMesh.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
    }

    // OpenGL right now needs to actually use the incoming vertex position
    // so we create a fake dependency on it here that haven't any impact.
    output.vmesh.pos = float4(uv * 2.0 - 1.0, inputMesh.vertex.z > 0 ? 1.0e-4 : 0.0, 1.0);

#ifdef VARYINGS_NEED_POSITION_WS
    output.vmesh.worldPos = TransformObjectToWorld(inputMesh.vertex.xyz);
#endif

#ifdef VARYINGS_NEED_TANGENT_TO_WORLD
    // Normal is required for triplanar mapping
    output.vmesh.worldNormal = TransformObjectToWorldNormal(inputMesh.normal);
    // Not required but assign to silent compiler warning
    output.vmesh.worldTangent = float4(1.0, 0.0, 0.0, 0.0);
#endif

    output.vmesh.texcoord0 = inputMesh.texcoord0;
    output.vmesh.texcoord1 = inputMesh.texcoord1;
    output.vmesh.texcoord2 = inputMesh.texcoord2;
    // output.vmesh.texCoord3 = inputMesh.texcoord3;
    // output.vmesh.vertexColor = inputMesh.vertexColor;

    return PackVaryingsToPS(output);
}
#else

PackedVaryingsToPS Vert(VertexData inputMesh)
{
    VaryingsToPS varyingsType;
    varyingsType.vmesh = VertMesh(inputMesh);
    #if (SHADERPASS == SHADERPASS_DBUFFER_MESH)
       MeshDecalsPositionZBias(varyingsType);
    #endif
    return PackVaryingsToPS(varyingsType);
}

#endif



            

            
                FragInputs BuildFragInputs(VertexToPixel input)
                {
                    UNITY_SETUP_INSTANCE_ID(input);
                    FragInputs output;
                    ZERO_INITIALIZE(FragInputs, output);
            
                    // Init to some default value to make the computer quiet (else it output 'divide by zero' warning even if value is not used).
                    // TODO: this is a really poor workaround, but the variable is used in a bunch of places
                    // to compute normals which are then passed on elsewhere to compute other values...
                    output.tangentToWorld = k_identity3x3;
                    output.positionSS = input.pos;       // input.positionCS is SV_Position
            
                    output.positionRWS = input.worldPos;
                    output.tangentToWorld = BuildTangentToWorld(input.worldTangent, input.worldNormal);
                    output.texCoord0 = input.texcoord0;
                    output.texCoord1 = input.texcoord1;
                    output.texCoord2 = input.texcoord2;
            
                    return output;
                }
            
               void BuildSurfaceData(FragInputs fragInputs, inout Surface surfaceDescription, float3 V, PositionInputs posInput, out SurfaceData surfaceData, out float3 bentNormalWS)
               {
                   // setup defaults -- these are used if the graph doesn't output a value
                   ZERO_INITIALIZE(SurfaceData, surfaceData);
        
                   // specularOcclusion need to be init ahead of decal to quiet the compiler that modify the SurfaceData struct
                   // however specularOcclusion can come from the graph, so need to be init here so it can be override.
                   surfaceData.specularOcclusion = 1.0;
        
                   // copy across graph values, if defined
                   surfaceData.baseColor =                 surfaceDescription.Albedo;
                   surfaceData.perceptualSmoothness =      surfaceDescription.Smoothness;
                   surfaceData.ambientOcclusion =          surfaceDescription.Occlusion;
                   surfaceData.specularOcclusion =         surfaceDescription.SpecularOcclusion;
                   surfaceData.metallic =                  surfaceDescription.Metallic;
                   surfaceData.subsurfaceMask =            surfaceDescription.SubsurfaceMask;
                   surfaceData.thickness =                 surfaceDescription.Thickness;
                   surfaceData.diffusionProfileHash =      asuint(surfaceDescription.DiffusionProfileHash);
                   #if _USESPECULAR
                      surfaceData.specularColor =             surfaceDescription.Specular;
                   #endif
                   surfaceData.coatMask =                  surfaceDescription.CoatMask;
                   surfaceData.anisotropy =                surfaceDescription.Anisotropy;
                   surfaceData.iridescenceMask =           surfaceDescription.IridescenceMask;
                   surfaceData.iridescenceThickness =      surfaceDescription.IridescenceThickness;
        
           #ifdef _HAS_REFRACTION
                   if (_EnableSSRefraction)
                   {
                       // surfaceData.ior =                       surfaceDescription.RefractionIndex;
                       // surfaceData.transmittanceColor =        surfaceDescription.RefractionColor;
                       // surfaceData.atDistance =                surfaceDescription.RefractionDistance;
        
                       surfaceData.transmittanceMask = (1.0 - surfaceDescription.Alpha);
                       surfaceDescription.Alpha = 1.0;
                   }
                   else
                   {
                       surfaceData.ior = 1.0;
                       surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
                       surfaceData.atDistance = 1.0;
                       surfaceData.transmittanceMask = 0.0;
                       surfaceDescription.Alpha = 1.0;
                   }
           #else
                   surfaceData.ior = 1.0;
                   surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
                   surfaceData.atDistance = 1.0;
                   surfaceData.transmittanceMask = 0.0;
           #endif
                
                   // These static material feature allow compile time optimization
                   surfaceData.materialFeatures = MATERIALFEATUREFLAGS_LIT_STANDARD;
           #ifdef _MATERIAL_FEATURE_SUBSURFACE_SCATTERING
                   surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SUBSURFACE_SCATTERING;
           #endif
           #ifdef _MATERIAL_FEATURE_TRANSMISSION
                   surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_TRANSMISSION;
           #endif
           #ifdef _MATERIAL_FEATURE_ANISOTROPY
                   surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_ANISOTROPY;
           #endif
                   // surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_CLEAR_COAT;
        
           #ifdef _MATERIAL_FEATURE_IRIDESCENCE
                   surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_IRIDESCENCE;
           #endif
           #ifdef _MATERIAL_FEATURE_SPECULAR_COLOR
                   surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SPECULAR_COLOR;
           #endif
        
           #if defined (_MATERIAL_FEATURE_SPECULAR_COLOR) && defined (_ENERGY_CONSERVING_SPECULAR)
                   // Require to have setup baseColor
                   // Reproduce the energy conservation done in legacy Unity. Not ideal but better for compatibility and users can unchek it
                   surfaceData.baseColor *= (1.0 - Max3(surfaceData.specularColor.r, surfaceData.specularColor.g, surfaceData.specularColor.b));
           #endif

        
                   // tangent-space normal
                   float3 normalTS = float3(0.0f, 0.0f, 1.0f);
                   normalTS = surfaceDescription.Normal;
        
                   // compute world space normal
                   #if !_WORLDSPACENORMAL
                      surfaceData.normalWS = mul(normalTS, fragInputs.tangentToWorld);
                   #else
                      surfaceData.normalWS = normalTS;  
                   #endif
                   surfaceData.geomNormalWS = fragInputs.tangentToWorld[2];
        
                   surfaceData.tangentWS = normalize(fragInputs.tangentToWorld[0].xyz);    // The tangent is not normalize in tangentToWorld for mikkt. TODO: Check if it expected that we normalize with Morten. Tag: SURFACE_GRADIENT
                   // surfaceData.tangentWS = TransformTangentToWorld(surfaceDescription.Tangent, fragInputs.tangentToWorld);
        
           #if HAVE_DECALS
                   if (_EnableDecals)
                   {
                       #if VERSION_GREATER_EQUAL(10,2)
                          DecalSurfaceData decalSurfaceData = GetDecalSurfaceData(posInput,  surfaceData.geomNormalWS, surfaceDescription.Alpha);
                          ApplyDecalToSurfaceData(decalSurfaceData,  surfaceData.geomNormalWS, surfaceData);
                       #else
                          DecalSurfaceData decalSurfaceData = GetDecalSurfaceData(posInput, surfaceDescription.Alpha);
                          ApplyDecalToSurfaceData(decalSurfaceData, surfaceData);
                       #endif
                   }
           #endif
        
                   bentNormalWS = surfaceData.normalWS;
               
                   surfaceData.tangentWS = Orthonormalize(surfaceData.tangentWS, surfaceData.normalWS);
        
        
                   // By default we use the ambient occlusion with Tri-ace trick (apply outside) for specular occlusion.
                   // If user provide bent normal then we process a better term
           #if defined(_SPECULAR_OCCLUSION_CUSTOM)
                   // Just use the value passed through via the slot (not active otherwise)
           #elif defined(_SPECULAR_OCCLUSION_FROM_AO_BENT_NORMAL)
                   // If we have bent normal and ambient occlusion, process a specular occlusion
                   surfaceData.specularOcclusion = GetSpecularOcclusionFromBentAO(V, bentNormalWS, surfaceData.normalWS, surfaceData.ambientOcclusion, PerceptualSmoothnessToPerceptualRoughness(surfaceData.perceptualSmoothness));
           #elif defined(_AMBIENT_OCCLUSION) && defined(_SPECULAR_OCCLUSION_FROM_AO)
                   surfaceData.specularOcclusion = GetSpecularOcclusionFromAmbientOcclusion(ClampNdotV(dot(surfaceData.normalWS, V)), surfaceData.ambientOcclusion, PerceptualSmoothnessToRoughness(surfaceData.perceptualSmoothness));
           #endif
        
           #ifdef _ENABLE_GEOMETRIC_SPECULAR_AA
                   surfaceData.perceptualSmoothness = GeometricNormalFiltering(surfaceData.perceptualSmoothness, fragInputs.tangentToWorld[2], surfaceDescription.SpecularAAScreenSpaceVariance, surfaceDescription.SpecularAAThreshold);
           #endif
        
           #ifdef DEBUG_DISPLAY
                   if (_DebugMipMapMode != DEBUGMIPMAPMODE_NONE)
                   {
                       // TODO: need to update mip info
                       surfaceData.metallic = 0;
                   }
        
                   // We need to call ApplyDebugToSurfaceData after filling the surfarcedata and before filling builtinData
                   // as it can modify attribute use for static lighting
                   ApplyDebugToSurfaceData(fragInputs.tangentToWorld, surfaceData);
           #endif
               }
        
               void GetSurfaceAndBuiltinData(VertexToPixel m2ps, FragInputs fragInputs, float3 V, inout PositionInputs posInput,
                     out SurfaceData surfaceData, out BuiltinData builtinData, inout Surface l, inout ShaderData d
                     #if NEED_FACING
                        , facing
                     #endif
                  )
               {
                 // Removed since crossfade does not work, probably needs extra material setup.   
                 //#ifdef LOD_FADE_CROSSFADE // enable dithering LOD transition if user select CrossFade transition in LOD group
                 //    uint3 fadeMaskSeed = asuint((int3)(V * _ScreenSize.xyx)); // Quantize V to _ScreenSize values
                 //    LODDitheringTransition(fadeMaskSeed, unity_LODFade.x);
                 //#endif
        
                 d = CreateShaderData(m2ps
                  #if NEED_FACING
                    , facing
                  #endif
                 );

                 

                 l = (Surface)0;

                 l.Albedo = half3(0.5, 0.5, 0.5);
                 l.Normal = float3(0,0,1);
                 l.Occlusion = 1;
                 l.Alpha = 1;

                 #ifdef _DEPTHOFFSET_ON
                    l.outputDepth = posInput.deviceDepth;
                 #endif

                 ChainSurfaceFunction(l, d);

                 #ifdef _DEPTHOFFSET_ON
                    posInput.deviceDepth = l.outputDepth;
                 #endif

                 #if _UNLIT
                     //l.Emission = l.Albedo;
                     //l.Albedo = 0;
                     l.Normal = half3(0,0,1);
                     l.Occlusion = 1;
                     l.Metallic = 0;
                     l.Specular = 0;
                 #endif

                 surfaceData.geomNormalWS = d.worldSpaceNormal;
                 surfaceData.tangentWS = d.worldSpaceTangent;
                 fragInputs.tangentToWorld = d.TBNMatrix;

                 float3 bentNormalWS;
                 BuildSurfaceData(fragInputs, l, V, posInput, surfaceData, bentNormalWS);

                 

                 InitBuiltinData(posInput, l.Alpha, bentNormalWS, -d.worldSpaceNormal, fragInputs.texCoord1, fragInputs.texCoord2, builtinData);

                 builtinData.emissiveColor = l.Emission;
        
        
                 #if (SHADERPASS == SHADERPASS_DISTORTION)
                     //builtinData.distortion = surfaceDescription.Distortion;
                     //builtinData.distortionBlur = surfaceDescription.DistortionBlur;
                     builtinData.distortion = float2(0.0, 0.0);
                     builtinData.distortionBlur = 0.0;
                 #else
                     builtinData.distortion = float2(0.0, 0.0);
                     builtinData.distortionBlur = 0.0;
                 #endif
        
                   PostInitBuiltinData(V, posInput, surfaceData, builtinData);
               }
        

            
            void Frag(  PackedVaryingsToPS packedInput
            #ifdef WRITE_NORMAL_BUFFER
            , out float4 outNormalBuffer : SV_Target0
                #ifdef WRITE_MSAA_DEPTH
                , out float1 depthColor : SV_Target1
                #endif
            #elif defined(WRITE_MSAA_DEPTH) // When only WRITE_MSAA_DEPTH is define and not WRITE_NORMAL_BUFFER it mean we are Unlit and only need depth, but we still have normal buffer binded
            , out float4 outNormalBuffer : SV_Target0
            , out float1 depthColor : SV_Target1
            #elif defined(SCENESELECTIONPASS)
            , out float4 outColor : SV_Target0
            #endif

            #ifdef _DEPTHOFFSET_ON
            , out float outputDepth : SV_Depth
            #endif
            #if NEED_FACING
            , bool facing : SV_IsFrontFace
            #endif
        )
         {
             UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(packedInput);
             FragInputs input = BuildFragInputs(packedInput.vmesh);

             // input.positionSS is SV_Position
             PositionInputs posInput = GetPositionInput(input.positionSS.xy, _ScreenSize.zw, input.positionSS.z, input.positionSS.w, input.positionRWS);

         #ifdef VARYINGS_NEED_POSITION_WS
             float3 V = GetWorldSpaceNormalizeViewDir(input.positionRWS);
         #else
             // Unused
             float3 V = float3(1.0, 1.0, 1.0); // Avoid the division by 0
         #endif

            SurfaceData surfaceData;
            BuiltinData builtinData;
            Surface l;
            ShaderData d;
            GetSurfaceAndBuiltinData(packedInput.vmesh, input, V, posInput, surfaceData, builtinData, l, d
               #if NEED_FACING
                  , facing
               #endif
               );


         #ifdef _DEPTHOFFSET_ON
             outputDepth = posInput.deviceDepth;
         #endif

         #ifdef WRITE_NORMAL_BUFFER
             EncodeIntoNormalBuffer(ConvertSurfaceDataToNormalData(surfaceData), posInput.positionSS, outNormalBuffer);
             #ifdef WRITE_MSAA_DEPTH
             // In case we are rendering in MSAA, reading the an MSAA depth buffer is way too expensive. To avoid that, we export the depth to a color buffer
             depthColor = packedInput.vmesh.pos.z;
             #endif
         #elif defined(WRITE_MSAA_DEPTH) // When we are MSAA depth only without normal buffer
             // Due to the binding order of these two render targets, we need to have them both declared
             outNormalBuffer = float4(0.0, 0.0, 0.0, 1.0);
             // In case we are rendering in MSAA, reading the an MSAA depth buffer is way too expensive. To avoid that, we export the depth to a color buffer
             depthColor = packedInput.vmesh.pos.z;
         #elif defined(SCENESELECTIONPASS)
             // We use depth prepass for scene selection in the editor, this code allow to output the outline correctly
             outColor = float4(_ObjectId, _PassValue, 1.0, 1.0);
         #endif
         }

         ENDHLSL
     }

        
      
      





   }
   
   
   
}
