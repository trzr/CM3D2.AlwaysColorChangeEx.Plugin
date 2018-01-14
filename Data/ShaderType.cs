using System;
using System.Collections.Generic;
using CM3D2.AlwaysColorChangeEx.Plugin.Util;

namespace CM3D2.AlwaysColorChangeEx.Plugin.Data
{
    /// <summary>
    /// ドロップダウンから選択するシェーダタイプを定義するクラス
    /// </summary>
    public class ShaderType
    {
        /// <summary>標準シェーダタイプ</summary>
        public static ShaderType[] shaders;
        private static Dictionary<string, string> shader2Map;
        private static Dictionary<string, ShaderType> shaderMap;
        public static ShaderType Resolve(string name) {
            if (shaderMap == null) Init();

            ShaderType st;
            if (!shaderMap.TryGetValue(name, out st)) {
                LogUtil.Log("未対応シェーダのため、シェーダタイプが特定できません。", name);
                st = ShaderType.UNKNOWN;
            }
            return st;
        }
        public static ShaderType Resolve(int shaderIdx) {
            if (shaderMap == null) Init();

            if (shaderIdx < shaders.Length && shaderIdx >= 0) {
                return shaders[shaderIdx];
            } else {
                LogUtil.Log("指定シェーダのインデックスが範囲外のため、シェーダタイプが特定できません。", shaderIdx);
                return ShaderType.UNKNOWN;
            }
        }
        /// <summary>
        /// シェーダ1から対応するシェーダ2を取得する
        /// </summary>
        /// <param name="shader1">シェーダ1</param>
        /// <returns>対応するシェーダ2</returns>
        public static string GetShader2(string shader1) {
            if (shader2Map == null) Init();

            string ret;
            return shader2Map.TryGetValue(shader1, out ret) ? ret : string.Empty;
        }
        
        // シェーダ名の最大文字数を取得
        public static int MaxNameLength() {
            return shaders[shaders.Length-1].name.Length;
        }

        static ShaderType() {
            Init();
        }
        public static readonly ShaderType UNKNOWN = new ShaderType();
        static void Init() {
            var texTypeEmpty = new ShaderPropTex[0];
            var texTypeR  = new ShaderPropTex[]{ShaderPropType.RenderTex,};
            var texType0  = new ShaderPropTex[]{ShaderPropType.MainTex,};
            var texType0a = new ShaderPropTex[]{ShaderPropType.MainTex_a,};
            var texType1  = new ShaderPropTex[]{ShaderPropType.MainTex, ShaderPropType.ToonRamp, ShaderPropType.ShadowTex, ShaderPropType.ShadowRateToon, };//ShaderPropType.MultiColTex, };
            var texType1a = new ShaderPropTex[]{ShaderPropType.MainTex_a, ShaderPropType.ToonRamp, ShaderPropType.ShadowTex, ShaderPropType.ShadowRateToon,};// ShaderPropType.MultiColTex, };
            var texTypeH  = new ShaderPropTex[]{ShaderPropType.MainTex, ShaderPropType.ToonRamp, ShaderPropType.ShadowTex, ShaderPropType.ShadowRateToon, ShaderPropType.HiTex,};// ShaderPropType.MultiColTex, };
            
            var colEmpty  = new ShaderPropColor[0];
            var colC      = new ShaderPropColor[]{ShaderPropType.Color, };
            var colCa     = new ShaderPropColor[]{ShaderPropType.ColorA, };
            var colL      = new ShaderPropColor[]{ShaderPropType.Color, ShaderPropType.ShadowColor, };
            var colLa     = new ShaderPropColor[]{ShaderPropType.ColorA,ShaderPropType.ShadowColor, };
            var colTL     = new ShaderPropColor[]{ShaderPropType.Color, ShaderPropType.ShadowColor, ShaderPropType.RimColor, };
            var colTLa    = new ShaderPropColor[]{ShaderPropType.ColorA, ShaderPropType.ShadowColor, ShaderPropType.RimColor, };
            var colTLO    = new ShaderPropColor[]{ShaderPropType.Color, ShaderPropType.ShadowColor, ShaderPropType.RimColor, ShaderPropType.OutlineColor,  };
            var colTLOa   = new ShaderPropColor[]{ShaderPropType.ColorA, ShaderPropType.ShadowColor, ShaderPropType.RimColor, ShaderPropType.OutlineColor,  };
            
            var propEmpty = new ShaderPropFloat[0];
            var propL     = new ShaderPropFloat[]{ShaderPropType.Shininess, };
            var propTL    = new ShaderPropFloat[]{ShaderPropType.Shininess, ShaderPropType.RimPower, ShaderPropType.RimShift, };
            var propTLC   = new ShaderPropFloat[]{ShaderPropType.Shininess, ShaderPropType.RimPower, ShaderPropType.RimShift, ShaderPropType.Cutoff, };
            var propTLO   = new ShaderPropFloat[]{ShaderPropType.Shininess, ShaderPropType.OutlineWidth, ShaderPropType.RimPower, ShaderPropType.RimShift, };
            var propTLH   = new ShaderPropFloat[]{ShaderPropType.Shininess, ShaderPropType.RimPower, ShaderPropType.RimShift, ShaderPropType.HiRate, ShaderPropType.HiPow};
            var propTLHO  = new ShaderPropFloat[]{ShaderPropType.Shininess, ShaderPropType.OutlineWidth, ShaderPropType.RimPower, ShaderPropType.RimShift, ShaderPropType.HiRate, ShaderPropType.HiPow};
            
            ShaderType.count = 0;
            shaders = new ShaderType[] {
                new ShaderType("CM3D2/Toony_Lighted", "トゥーン",                          texType1,  colTL,   propTL ),
                new ShaderType("CM3D2/Toony_Lighted_Trans",　"トゥーン 透過",              texType1a, colTLa,  propTLC, true ),
                new ShaderType("CM3D2/Toony_Lighted_Trans_NoZ",　"トゥーン 透過 NoZ",      texType1a, colTLa,  propTL, true ),
                new ShaderType("CM3D2/Toony_Lighted_Outline","トゥーン 輪郭線",            texType1,  colTLO,  propTLO ),
                new ShaderType("CM3D2/Toony_Lighted_Outline_Trans","トゥーン 輪郭線 透過", texType1a, colTLOa, propTLO, true ),
                new ShaderType("CM3D2/Toony_Lighted_Hair","トゥーン 髪",                   texTypeH,  colTL,   propTLH ),
                new ShaderType("CM3D2/Toony_Lighted_Hair_Outline","トゥーン 髪 輪郭線",    texTypeH,  colTLO,  propTLHO ),
                new ShaderType("CM3D2/Lighted","非トゥーン",            texType0,  colL,     propL ),
                new ShaderType("CM3D2/Lighted_Trans","透過",            texType0a, colLa,    propL, true ),
                new ShaderType("Unlit/Texture","発光",                  texType0,  colEmpty, propEmpty ),
                new ShaderType("Unlit/Transparent","発光 透過",         texType0a, colEmpty, propEmpty, true ), 
                new ShaderType("Diffuse","リアル",                      texType0,  colC,     propEmpty ),
                new ShaderType("Transparent/Diffuse","リアル 透過",     texType0a, colCa,     propEmpty, true ),
                new ShaderType("CM3D2/Mosaic","モザイク",               texTypeR, colEmpty, new ShaderPropFloat[]{ShaderPropType.FloatValue1}),
                new ShaderType("CM3D2/Man","ご主人様",                  texTypeEmpty, colC, new ShaderPropFloat[]{ShaderPropType.FloatValue2, ShaderPropType.FloatValue3}),
                new ShaderType("CM3D2_Debug/Debug_CM3D2_Normal2Color","法線", texTypeEmpty, colC, propEmpty), // Emission
            };
            shaderMap = new Dictionary<string, ShaderType>(shaders.Length);
            foreach (var s in shaders) {
                shaderMap[s.name] = s;
            };
            shaderMap["Legacy Shaders/Transparent/Diffuse"] = shaderMap["Transparent/Diffuse"];
            shaderMap["Legacy Shaders/Diffuse"] = shaderMap["Diffuse"];
            shader2Map = new Dictionary<string, string>(shaders.Length);
            foreach (var s in shaders) {
                shader2Map[s.name] = s.name.Replace("/", "__");
            };
        }
        private ShaderType() {
            this.idx = -1;
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
            this.fProps    = props;
            this.isTrans  = isTrans;
            
            if (colProps != null) {
                foreach (var colProp in colProps) {
                    if (colProp == ShaderPropType.ShadowColor) {
                        hasShadow = true;
                        break;
                    }
                }
            }
            this.idx = count++;
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
                    case PropKey._Emission:
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
                    case PropKey._Parallax:
                    case PropKey._Cutoff:
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

    }
}
