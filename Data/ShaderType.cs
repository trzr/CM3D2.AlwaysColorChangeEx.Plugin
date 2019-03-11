using System;
using System.Collections.Generic;
using CM3D2.AlwaysColorChangeEx.Plugin.Util;

namespace CM3D2.AlwaysColorChangeEx.Plugin.Data {
    /// <summary>
    /// ドロップダウンから選択するシェーダタイプを定義するクラス
    /// </summary>
    public class ShaderType {
        public static readonly int SHADER_TYPE_CM3D2_MAX;
        // public static readonly int SHADER_TYPE_STANDARD;
        public static readonly ShaderType UNKNOWN = new ShaderType();
        // public static ShaderType STANDARD;

        /// <summary>標準シェーダタイプ</summary>
        public static readonly ShaderType[] shaders;
        private static readonly Dictionary<string, string> shader2Map;
        private static readonly Dictionary<string, ShaderType> shaderMap;
        public static ShaderType Resolve(string name) {
            ShaderType st;
            // ReSharper disable once PossibleNullReferenceException
            if (shaderMap.TryGetValue(name, out st)) return st;
            LogUtil.Log("未対応シェーダのため、シェーダタイプが特定できません。", name);
            st = UNKNOWN;
            return st;
        }

        public static ShaderType Resolve(int shaderIdx) {
            if (shaderIdx < shaders.Length && shaderIdx >= 0) {
                return shaders[shaderIdx];
            }

            LogUtil.Log("指定シェーダのインデックスが範囲外のため、シェーダタイプが特定できません。", shaderIdx);
            return UNKNOWN;
        }

        /// <summary>
        /// シェーダ1から対応するマテリアル名を取得する.
        /// 見つからない場合は空文字を返す.
        /// </summary>
        /// <param name="shader1">シェーダ1</param>
        /// <returns>対応するマテリアル名(シェーダ2)</returns>
        public static string GetMateName(string shader1) {
            string ret;
            // ReSharper disable once PossibleNullReferenceException
            return shader2Map.TryGetValue(shader1, out ret) ? ret : string.Empty;
        }
        
        // シェーダ名の最大文字数を取得
        public static int MaxNameLength() {
            return shaders[SHADER_TYPE_CM3D2_MAX].name.Length;
        }

        static ShaderType() {
            var texTypeEmpty = new ShaderPropTex[0];
            var texTypeR  = new []{ShaderPropType.RenderTex,};
            var texType0  = new []{ShaderPropType.MainTex,};
            var texType0a = new []{ShaderPropType.MainTexA,};
            var texType1  = new []{ShaderPropType.MainTex, ShaderPropType.ToonRamp, ShaderPropType.ShadowTex, ShaderPropType.ShadowRateToon, };//ShaderPropType.MultiColTex, };
            var texType1t = new []{ShaderPropType.MainTex, ShaderPropType.ToonRamp, ShaderPropType.ShadowTex, ShaderPropType.ShadowRateToon, ShaderPropType.OutlineTex, ShaderPropType.OutlineToonRamp};
            var texType1a = new []{ShaderPropType.MainTexA, ShaderPropType.ToonRamp, ShaderPropType.ShadowTex, ShaderPropType.ShadowRateToon,};// ShaderPropType.MultiColTex, };
            var texTypeH  = new []{ShaderPropType.MainTex, ShaderPropType.ToonRamp, ShaderPropType.ShadowTex, ShaderPropType.ShadowRateToon, ShaderPropType.HiTex,};// ShaderPropType.MultiColTex, };
            var texTypeHt  = new []{ShaderPropType.MainTex, ShaderPropType.ToonRamp, ShaderPropType.ShadowTex, ShaderPropType.ShadowRateToon, ShaderPropType.HiTex, ShaderPropType.OutlineTex, ShaderPropType.OutlineToonRamp};
            var texTypeStd = new []{ShaderPropType.MainTexA, ShaderPropType.OcclusionMap,
                ShaderPropType.MetallicGlossMap, ShaderPropType.BumpMap, ShaderPropType.ParallaxMap, ShaderPropType.EmissionMap,
                ShaderPropType.DetailMask, ShaderPropType.DetailAlbedoMap, ShaderPropType.DetailNormalMap,
                ShaderPropType.SpecGlossMap
            };
            var texTypeMir  = new []{ShaderPropType.MainTex, ShaderPropType.ReflectionTex};
            
            var colEmpty  = new ShaderPropColor[0];
            var colC      = new []{ShaderPropType.Color, };
            var colCa     = new []{ShaderPropType.ColorA, };
            var colL      = new []{ShaderPropType.Color, ShaderPropType.ShadowColor, };
            var colLa     = new []{ShaderPropType.ColorA,ShaderPropType.ShadowColor, };
            var colTL     = new []{ShaderPropType.Color, ShaderPropType.ShadowColor, ShaderPropType.RimColor, };
            var colTLa    = new []{ShaderPropType.ColorA, ShaderPropType.ShadowColor, ShaderPropType.RimColor, };
            var colTLO    = new []{ShaderPropType.Color, ShaderPropType.ShadowColor, ShaderPropType.RimColor, ShaderPropType.OutlineColor,  };
            var colTLOa   = new []{ShaderPropType.ColorA, ShaderPropType.ShadowColor, ShaderPropType.RimColor, ShaderPropType.OutlineColor,  };
            
            var propEmpty = new ShaderPropFloat[0];
            var propL     = new []{ShaderPropType.Shininess, };
            var propLC1   = new []{ShaderPropType.Shininess, ShaderPropType.Cutoff};
            var propTL    = new []{ShaderPropType.Shininess, ShaderPropType.RimPower, ShaderPropType.RimShift, };
            var propTLZ   = new []{ShaderPropType.Shininess, ShaderPropType.RimPower, ShaderPropType.RimShift, ShaderPropType.ZTest, ShaderPropType.ZTest2, ShaderPropType.ZTest2Alpha};
            var propTLC1  = new []{ShaderPropType.Shininess, ShaderPropType.RimPower, ShaderPropType.RimShift, ShaderPropType.Cutoff };
            var propTLO   = new []{ShaderPropType.Shininess, ShaderPropType.OutlineWidth, ShaderPropType.RimPower, ShaderPropType.RimShift, };
            var propTLH   = new []{ShaderPropType.Shininess, ShaderPropType.RimPower, ShaderPropType.RimShift, ShaderPropType.HiRate, ShaderPropType.HiPow};
            var propTLHO  = new []{ShaderPropType.Shininess, ShaderPropType.OutlineWidth, ShaderPropType.RimPower, ShaderPropType.RimShift, ShaderPropType.HiRate, ShaderPropType.HiPow};
//            var propStd   = new [] {
//                ShaderPropType.Cutoff, ShaderPropType.Glossiness, ShaderPropType.GlossMapScale, ShaderPropType.SmoothnessTexChannel, ShaderPropType.Metallic, ShaderPropType.BumpScale,
//                ShaderPropType.Parallax, ShaderPropType.OcclusionStrength, ShaderPropType.DetailNormalMapScale,
//                ShaderPropType.UVSec, ShaderPropType.Mode, ShaderPropType.SrcBlend, ShaderPropType.DstBlend, ShaderPropType.ZWrite,
//            };
            var propStd   = new [] {
                ShaderPropType.Cutoff, ShaderPropType.OcclusionStrength,
                ShaderPropType.Glossiness, ShaderPropType.GlossMapScale,
                ShaderPropType.SpecularHeighlights, ShaderPropType.GlossyReflections,
                ShaderPropType.BumpScale, ShaderPropType.DetailNormalMapScale,
            };
            count = 0;
            shaders = new[] {
                new ShaderType("CM3D2/Toony_Lighted", "トゥーン",                          texType1,  colTL,   propTL ),
                new ShaderType("CM3D2/Toony_Lighted_Trans", "トゥーン 透過",              texType1a, colTLa,  propTLC1, true ),
                new ShaderType("CM3D2/Toony_Lighted_Trans_NoZ", "トゥーン 透過 NoZ",      texType1a, colTLa,  propTL,  true ),
#if COM3D2
                new ShaderType("CM3D2/Toony_Lighted_Trans_NoZTest", "トゥーン 透過 NoZTest",      texType1a, colTLa,  propTLZ,  true ),
#endif
                new ShaderType("CM3D2/Toony_Lighted_Outline","トゥーン 輪郭線",            texType1,  colTLO,  propTLO ),
                new ShaderType("CM3D2/Toony_Lighted_Outline_Trans","トゥーン 輪郭線 透過", texType1a, colTLOa, propTLO, true ),
#if COM3D2
                new ShaderType("CM3D2/Toony_Lighted_Outline_Tex","トゥーン 輪郭線 Tex",    texType1t, colTLO,  propTLO ),
#endif
                new ShaderType("CM3D2/Toony_Lighted_Hair","トゥーン 髪",                   texTypeH,  colTL,   propTLH ),
                new ShaderType("CM3D2/Toony_Lighted_Hair_Outline","トゥーン 髪 輪郭線",    texTypeH,  colTLO,  propTLHO ),
#if COM3D2
                new ShaderType("CM3D2/Toony_Lighted_Hair_Outline_Tex","トゥーン 髪 輪郭線 Tex", texTypeHt,  colTLO,  propTLHO ),
                new ShaderType("CM3D2/Toony_Lighted_Cutout_AtC", "トゥーン Cutout",       texType1a, colTLa,  propTLC1, true ),
#endif
                new ShaderType("CM3D2/Lighted","非トゥーン",                       texType0,  colL,     propL ),
#if COM3D2
                new ShaderType("CM3D2/Lighted_Cutout_AtC","非トゥーン Cutout",     texType0a,  colLa,     propLC1 ),
#endif
                new ShaderType("CM3D2/Lighted_Trans","透過",            texType0a, colLa,    propL, true ),
                new ShaderType("Unlit/Texture","発光",                  texType0,  colEmpty, propEmpty ),
                new ShaderType("Unlit/Transparent","発光 透過",         texType0a, colEmpty, propEmpty, true ), 
                new ShaderType("Diffuse","リアル",                      texType0,  colC,     propEmpty ),
                new ShaderType("Transparent/Diffuse","リアル 透過",     texType0a, colCa,     propEmpty, true ),
                new ShaderType("CM3D2/Mosaic","モザイク",               texTypeR, colEmpty, new[]{ShaderPropType.FloatValue1}),
                new ShaderType("CM3D2/Man","ご主人様",                  texTypeEmpty, colC, new[]{ShaderPropType.FloatValue2, ShaderPropType.FloatValue3}),
                new ShaderType("CM3D2_Debug/Debug_CM3D2_Normal2Color","法線", texTypeEmpty, colC, propEmpty), // Emission
                // new ShaderType("Standard","Standard", texTypeStd, colC, propStd),
            };
            // SHADER_TYPE_STANDARD = shaders.Length - 1; // 末尾にStandardシェーダが設定される想定
            // SHADER_TYPE_CM3D2_MAX = SHADER_TYPE_STANDARD - 1;
            // STANDARD = shaders[SHADER_TYPE_STANDARD];
            SHADER_TYPE_CM3D2_MAX = shaders.Length - 1;

            shaderMap = new Dictionary<string, ShaderType>(shaders.Length + 2);
            foreach (var s in shaders) {
                shaderMap[s.name] = s;
            };
            shaderMap["Legacy Shaders/Transparent/Diffuse"] = shaderMap["Transparent/Diffuse"];
            shaderMap["Legacy Shaders/Diffuse"] = shaderMap["Diffuse"];

            shader2Map = new Dictionary<string, string>(shaders.Length+1);
            foreach (var s in shaders) {
                shader2Map[s.name] = s.name.Replace("/", "__");
            };
            shader2Map["CM3D2/Toony_Lighted_Hair_Outline_Tex"] = "CM3D2__Toony_Lighted_Hair_Outline";
        }

        private ShaderType() {
            idx = -1;
            name = string.Empty;
            dispName = string.Empty;
            texProps = new ShaderPropTex[0];
            colProps = new ShaderPropColor[0];
            fProps = new ShaderPropFloat[0];
        }

        internal ShaderType(string name, string dispName, ShaderPropTex[] texProps,
                          ShaderPropColor[] colProps, ShaderPropFloat[] props, bool isTrans = false) {
            this.name = name;
            this.dispName = dispName;
            this.texProps = texProps;
            this.colProps = colProps;
            fProps    = props;
            this.isTrans  = isTrans;
            
            if (colProps != null) {
                foreach (var colProp in colProps) {
                    if (colProp != ShaderPropType.ShadowColor) continue;
                    hasShadow = true;
                    break;
                }
            }
            idx = count++;
        }

        public int KeyCount() {
            return texProps.Length + colProps.Length + fProps.Length;
        }

        public ShaderProp GetShaderProp(string propName) {
            try {
                var propKey = (PropKey)Enum.Parse(typeof(PropKey), propName);
                switch (propKey) {
                    case PropKey._MainTex:
                    case PropKey._ToonRamp:
                    case PropKey._ShadowTex:
                    case PropKey._ShadowRateToon:
                    case PropKey._OutlineTex:
                    case PropKey._OutlineToonRamp:
                    case PropKey._HiTex:
                    case PropKey._RenderTex:
                    case PropKey._BumpMap:
                    case PropKey._SpecularTex:
                    case PropKey._DecalTex:
                    case PropKey._Detail:
                    case PropKey._DetailTex:
                    case PropKey._AnisoTex:
                    case PropKey._ParallaxMap:
                    case PropKey._Illum:
                    case PropKey._Cube:
                    case PropKey._ReflectionTex:
                    case PropKey._MultiColTex:
                    case PropKey._EnvMap:
                        foreach (var prop in texProps) {
                            if (prop.key == propKey) {
                                return prop;
                            }
                        }
                        break;
            
                    case PropKey._Color:
                    case PropKey._ShadowColor:
                    case PropKey._RimColor:
                    case PropKey._OutlineColor:
                    case PropKey._SpecColor:
                    case PropKey._ReflectColor:
                    case PropKey._EmissionColor:
                        foreach (var prop in colProps) {
                            if (prop.key == propKey) {
                                return prop;
                            }
                        }
                        break;
                        
                    case PropKey._Shininess:
                    case PropKey._OutlineWidth:
                    case PropKey._RimPower:
                    case PropKey._RimShift:
                    case PropKey._HiRate:
                    case PropKey._HiPow:
                    case PropKey._FloatValue1:
                    case PropKey._FloatValue2:
                    case PropKey._FloatValue3:
                    case PropKey._ZTest:
                    case PropKey._ZTest2:
                    case PropKey._ZTest2Alpha:
                    case PropKey._Parallax:
                    case PropKey._Cutoff:
                    case PropKey._Cutout:
                    case PropKey._EmissionLM:
                    case PropKey._UseMulticolTex:
                    case PropKey._Strength:
                    case PropKey._StencilComp:
                    case PropKey._Stencil:
                    case PropKey._StencilOp:
                    case PropKey._StencilWriteMask:
                    case PropKey._StencilReadMask:
                    case PropKey._ColorMask:
                    case PropKey._EnvAlpha:
                    case PropKey._EnvAdd:
                        foreach (var prop in fProps) {
                            if (prop.key == propKey) {
                                return prop;
                            }
                        }
                        break;
                    case PropKey._SetManualRenderQueue:
                        return ShaderPropType.RenderQueue;
                }
            } catch {
            }
            return null;
        }

        internal static int count;
        public int idx;
        public bool isTrans;
        public string name;
        public string dispName;
        public ShaderPropTex[]   texProps;
        public ShaderPropColor[] colProps;
        public ShaderPropFloat[] fProps;
        public bool hasShadow;

        // Keywords (Shader用キーワード)
        // public Set<string> keys;
    }
}
