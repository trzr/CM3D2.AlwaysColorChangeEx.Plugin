using System;
using System.Collections.Generic;
using CM3D2.AlwaysColorChangeEx.Plugin.UI;
using CM3D2.AlwaysColorChangeEx.Plugin.UI.Data;
using UnityEngine;
using UnityEngine.Rendering;

namespace CM3D2.AlwaysColorChangeEx.Plugin.Data {
    /// <summary>
    /// 各シェーダ(マテリアル）のプロパティ情報を扱うクラス
    /// </summary>
    public abstract class ShaderProp {
        protected ShaderProp(string name, PropKey key, int id, ValType valType) {
            this.name = name;
            this.key = key;
            keyName = key.ToString();
            propId = id;
            Init(valType);
        }

        protected ShaderProp(PropKey key, ValType valType) {
            this.key = key;
            keyName = key.ToString();
            name = keyName.Substring(1);
            propId = Shader.PropertyToID(keyName);
            Init(valType);
        }

        private void Init(ValType valType1) {
            valType = valType1;
            switch(valType1) {
                case ValType.Bool:
                case ValType.Float:
                    type = PropType.f;
                    break;
                case ValType.Color:
                    type = PropType.col;
                    break;
                case ValType.Tex:
                    type = PropType.tex;
                    break;
            }
        }

        private Keyword _keyword = Keyword.NONE;
        public Keyword Keyword {
            get { return _keyword; }
            set {
                _keyword = value;
                KeywordString = Keyword.ToString();
            }
        }
        public string KeywordString = string.Empty;
        public readonly string name;
        public readonly PropKey key;
        public readonly string keyName;
        public readonly int propId;
        public PropType type;
        public ValType valType;
    }

    public class PresetOperation {
        public readonly string label;
        public readonly Func<float, float> func;
        public PresetOperation(string label, Func<float, float> func) {
            this.label = label;
            this.func = func;
        }
    }

    public class ShaderPropFloat :ShaderProp {

        public ShaderPropFloat(string name, PropKey key, int id) : base(name, key, id, ValType.Float) {}
        public ShaderPropFloat(PropKey key) : base(key, ValType.Float) {}

        protected ShaderPropFloat(string name, PropKey key, int id, ValType valType) : base(name, key, id, valType) {}
        protected ShaderPropFloat(PropKey key, ValType valType) : base(key, valType) {}

        public ShaderPropFloat(PropKey key, string format, IList<float> range,
            PresetOperation[] opts, float defaultVal, params float[] presetVals) 
            : this(key, Keyword.NONE, format, range, opts, defaultVal, presetVals) {}

        public ShaderPropFloat(PropKey key, Keyword kwd, string format, IList<float> range,
                                  PresetOperation[] opts, float defaultVal, params float[] presetVals) 
            : this(key, kwd, new EditRange<float>(format, range[2], range[3]), range, opts, defaultVal, presetVals) {}

        public ShaderPropFloat(PropKey key, EditRange<float> range, IList<float> sliderRange,
            PresetOperation[] opts, float defaultVal, params float[] presetVals) 
            : this(key, Keyword.NONE, range, sliderRange, opts, defaultVal, presetVals) {}

        public ShaderPropFloat(PropKey key, Keyword kwd, EditRange<float> range, IList<float> sliderRange,
            PresetOperation[] opts, float defaultVal, params float[] presetVals) : base(key, ValType.Float) {
            this.range = range;
            keyword = kwd;
            sliderMin = sliderRange[0];
            sliderMax = sliderRange[1];
            this.opts = opts;
            this.presetVals = presetVals;
            this.defaultVal = defaultVal;
        }

        public EditRange<float> range;

        public readonly float sliderMin;
        public readonly float sliderMax;
        public readonly Keyword keyword;

        public float defaultVal;
        public readonly PresetOperation[] opts;
        public readonly float[] presetVals;

        public void SetValue(Material m, float val) {
            m.SetFloat(propId, val);
        }
    }

    public class ShaderPropBool :ShaderPropFloat {
        public ShaderPropBool(string name, PropKey key, int id) : base(name, key, id, ValType.Bool) {
            range = EditRange.boolVal;
        }
        public ShaderPropBool(PropKey key) : base(key, ValType.Bool) {
            range = EditRange.boolVal;
        }
        public void SetValue(Material m, bool val) {
            m.SetFloat(propId, val? 1f : 0f);
        }
    }

    public class ShaderPropEnum :ShaderPropFloat {
        public string[] names;
        public ShaderPropEnum(PropKey key, Type enumType, int defaultVal, int min, int max) : base(key, ValType.Enum) {
            names = Enum.GetNames(enumType);
            range = new EditRange<float>("F0", min, max);
            this.defaultVal = defaultVal;
        }
        public ShaderPropEnum(PropKey key, string [] enumNames, int defaultVal, int min, int max) : base(key, ValType.Enum) {
            names = enumNames;
            range = new EditRange<float>("F0", min, max);
            this.defaultVal = defaultVal;
        }
        public void SetValue(Material m, int enumVal) {
            m.SetFloat(propId, enumVal);
        }
    }

    public class ShaderPropColor :ShaderProp {
        public ShaderPropColor(string name, PropKey key, int id, ColorType colType, bool composition=false, Keyword k=Keyword.NONE) : base(name, key, id, ValType.Color) {
            colorType = colType;
            Keyword = k;

            this.composition = composition;
        }
        public ShaderPropColor(PropKey key, ColorType colType, bool composition=false, Keyword k=Keyword.NONE) : base(key, ValType.Color) {
            colorType = colType;
            Keyword = k;

            this.composition = composition;
        }
        public readonly ColorType colorType;
        public readonly bool composition;
        public Color defaultVal;
        public void SetValue(Material m, Color col) {
            m.SetColor(propId, col);
        }
    }

    public class ShaderPropTex : ShaderProp {
        public ShaderPropTex(string name, PropKey key, int id, TexType type, Keyword k=Keyword.NONE) : base(name, key, id, ValType.Tex) {
            texType = type;
            Keyword = k;
        }
        public ShaderPropTex(PropKey key, TexType type, Keyword k=Keyword.NONE) : base(key, ValType.Tex) {
            texType = type;
            Keyword = k;
        }
        public TexType texType;

        public void SetValue(Material m, Texture2D tex) {
            m.SetTexture(propId, tex);
        }
    }

    public static class ShaderPropType {
        static readonly PresetOperation sliderL = new PresetOperation("<", val => val*0.9f);
        static readonly PresetOperation sliderR = new PresetOperation(">", val => val*1.1f);
        static readonly PresetOperation invert  = new PresetOperation("x-1", val => val*-1f);
        static readonly PresetOperation plus1   = new PresetOperation("+", val => val+1);
        static readonly PresetOperation plus10  = new PresetOperation("++", val => val+10);
        static readonly PresetOperation minus1  = new PresetOperation("-", val => val-1);
        static readonly PresetOperation minus10 = new PresetOperation("--", val => val-10);

        private static readonly PresetOperation[] PRESET_EMPTY = new PresetOperation[0];
        private static readonly PresetOperation[] PRESET_RATIO = {sliderL, sliderR};
        private static readonly PresetOperation[] PRESET_INV = { invert };
        private static readonly PresetOperation[] PRESET_PM = { minus10, minus1, plus1, plus10};

        private static readonly Settings settings = Settings.Instance;
        private static bool _initialized;
        public static void Initialize() {
            if (_initialized) return;

            // 設定値を利用するため、LazyInitとする
            RenderQueue      = new ShaderPropFloat(PropKey._SetManualRenderQueue, EditRange.renderQueue,
                new[]{0, 5000f,}, PRESET_PM, 3000, 2000, 3000);

            Shininess        = new ShaderPropFloat(PropKey._Shininess, EditRange.shininess,
                settings.shininessRange(), PRESET_RATIO, 0, 0, 0.1f, 0.5f, 1, 5);
            OutlineWidth     = new ShaderPropFloat(PropKey._OutlineWidth, EditRange.outlineWidth,
                settings.outlineWidthRange(), null, 0.0001f, 0.0001f, 0.001f, 0.002f);
            RimPower         = new ShaderPropFloat(PropKey._RimPower, EditRange.rimPower,
                settings.rimPowerRange(), PRESET_INV, 0f, 0f, 25f, 50f, 100f);
            RimShift         = new ShaderPropFloat(PropKey._RimShift, EditRange.rimShift,
                settings.rimShiftRange(), PRESET_RATIO, 0f, 0f, 0.25f, 0.5f, 1f);
            HiRate           = new ShaderPropFloat(PropKey._HiRate, EditRange.hiRate, 
                settings.hiRateRange(), PRESET_RATIO, 0f, 0f, 0.5f, 1.0f);
            HiPow            = new ShaderPropFloat(PropKey._HiPow, EditRange.hiPow,
                settings.hiPowRange(), PRESET_RATIO,  0.001f, 0.001f, 1f, 50f);
            FloatValue1      = new ShaderPropFloat(PropKey._FloatValue1, EditRange.floatVal1,
                settings.hiPowRange(), null,  10f, 0f, 100f, 200f);
            FloatValue2      = new ShaderPropFloat(PropKey._FloatValue2, EditRange.floatVal2,
                settings.hiPowRange(), PRESET_INV,  1f, -15, 0f, 1f, 15f);
            FloatValue3      = new ShaderPropFloat(PropKey._FloatValue3, EditRange.floatVal3,
                settings.hiPowRange(), PRESET_RATIO,  1f, 0f, 0.5f, 1f);
            Parallax         = new ShaderPropFloat(PropKey._Parallax, "F3",
                new[]{0.005f, 0.08f, 0.001f, 0.1f}, PRESET_RATIO, 0.02f, 0.02f);
            Cutoff           = new ShaderPropFloat(PropKey._Cutoff, "F3",
                new[]{0f, 1f, 0f, 1f}, PRESET_RATIO, 0.5f, 0f, 0.5f, 1f);
            Cutout           = new ShaderPropFloat(PropKey._Cutout, "F3",
                new[]{0f, 1f, 0f, 1f}, PRESET_RATIO, 0.5f, 0f, 0.5f, 1f);
            EmissionLM       = new ShaderPropBool(PropKey._EmissionLM);
            UseMulticolTex   = new ShaderPropBool(PropKey._UseMulticolTex);
            ZTest            = new ShaderPropEnum(PropKey._ZTest, typeof(CompareFunction), 4, 0, 8);
            ZTest2           = new ShaderPropBool(PropKey._ZTest2);
            ZTest2Alpha      = new ShaderPropFloat(PropKey._ZTest2Alpha, "F3",
                new[]{0f, 1f, 0f, 1f}, PRESET_RATIO, 0.8f, 0f, 0.8f, 1f);

            Strength         = new ShaderPropFloat(PropKey._Strength, "F2",
                new[]{0f, 1f, 0f, 1f}, PRESET_RATIO, 0.2f, 0.2f);
            StencilComp      = new ShaderPropFloat(PropKey._StencilComp, "F0",
                new[]{0f, 255f, 0f, 255f}, PRESET_RATIO, 8f, 8f);
            Stencil          = new ShaderPropFloat(PropKey._Stencil, "F0",
                new[]{0f, 255f, 0f, 255f}, PRESET_RATIO, 0f, 0f);
            StencilOp        = new ShaderPropFloat(PropKey._StencilOp, "F0",
                new[]{0f, 255f, 0f, 255f}, PRESET_RATIO, 0f, 0f);
            StencilWriteMask = new ShaderPropFloat(PropKey._StencilWriteMask, "F0",
                new[]{0f, 255f, 0f, 255f}, PRESET_RATIO, 255f, 255f);
            StencilReadMask  = new ShaderPropFloat(PropKey._StencilReadMask, "F0",
                new[]{0f, 255f, 0f, 255f}, PRESET_RATIO, 255f, 255f);
            ColorMask        = new ShaderPropFloat(PropKey._ColorMask, "F0",
                new[]{0f, 255f, 0f, 255f}, PRESET_RATIO, 255f, 255f);
            EnvAlpha         = new ShaderPropFloat(PropKey._EnvAlpha, "F1",
                new[]{0f, 1f, 0f, 1f}, PRESET_RATIO, 0f, 0f);
            EnvAdd           = new ShaderPropFloat(PropKey._EnvAdd, "F1",
                new[]{1f, 2f, 1f, 2f}, PRESET_RATIO, 1f, 1f);

            Glossiness       = new ShaderPropFloat(PropKey._Glossiness, "F3",
                new[]{0f, 1f, 0f, 1f}, PRESET_RATIO, 0.5f, 0f, 0.5f, 1f);
            GlossMapScale = new ShaderPropFloat(PropKey._GlossMapScale, "F3",
                new[]{0f, 1f, 0f, 1f}, PRESET_RATIO, 1f, 0f, 0.5f, 1f);
            SmoothnessTexChannel = new ShaderPropEnum(PropKey._SmoothnessTextureChannel,
                new[] {"Metallic Alpha", "Albedo Alpha"}, 0, 0, 1);
            EmissionScaleUI = new ShaderPropFloat(PropKey._EmissionScaleUI, "F3",
                new[]{0f, 1f, 0f, 1f}, PRESET_RATIO, 0f, 0f, 0.5f, 1f);
            Metallic = new ShaderPropFloat(PropKey._Metallic, "F3",
                new[]{0f, 1f, 0f, 1f}, PRESET_RATIO, 0f, 0f, 0.5f, 1f);
            BumpScale = new ShaderPropFloat(PropKey._BumpScale, "F3",
                new[]{0.1f, 10f, 0.01f, 100f}, PRESET_RATIO, 1f, 0.1f, 1f, 10f);
            OcclusionStrength = new ShaderPropFloat(PropKey._OcclusionStrength, "F3",
                new[]{0f, 1f, 0f, 1f}, PRESET_RATIO, 1f, 0f, 0.5f, 1f);
            DetailNormalMapScale = new ShaderPropFloat(PropKey._DetailNormalMapScale, "F3",
                new[]{0.1f, 10f, 0.01f, 100f}, PRESET_RATIO, 1f, 0.1f, 1f, 10f);
            Mode        = new ShaderPropFloat(PropKey._Mode, "F0",
                new[]{0f, 4f, 0f, 4f}, PRESET_EMPTY, 0f, 4f);
            ZWrite      = new ShaderPropFloat(PropKey._ZWrite, "F0",
                new[]{0f, 1f, 0f, 1f}, PRESET_EMPTY, 0f, 1f);
            // UV Set for secondary textures
            UVSec = new ShaderPropEnum(PropKey._UVSec, new[] {"UV0", "UV1"}, 0, 0, 1);
            SrcBlend = new ShaderPropEnum(PropKey._SrcBlend, typeof(BlendMode), 0, 0, 1);
            DstBlend = new ShaderPropEnum(PropKey._DstBlend, typeof(BlendMode), 1, 0, 1);
            SpecularHeighlights = new ShaderPropBool(PropKey._SpecularHighlights);
            GlossyReflections = new ShaderPropBool(PropKey._GlossyReflections);

            _initialized = true;
        }

        internal static ShaderPropFloat RenderQueue;
        internal static ShaderPropFloat Shininess;
        internal static ShaderPropFloat OutlineWidth;
        internal static ShaderPropFloat RimPower;
        internal static ShaderPropFloat RimShift;
        internal static ShaderPropFloat HiRate;
        internal static ShaderPropFloat HiPow;
        internal static ShaderPropFloat FloatValue1;
        internal static ShaderPropFloat FloatValue2;
        internal static ShaderPropFloat FloatValue3;
        internal static ShaderPropFloat Parallax;
        internal static ShaderPropFloat Cutoff;
        internal static ShaderPropFloat Cutout;
        internal static ShaderPropEnum ZTest;
        internal static ShaderPropBool ZTest2;
        internal static ShaderPropFloat ZTest2Alpha;
        internal static ShaderPropBool EmissionLM;
        internal static ShaderPropBool UseMulticolTex;
        internal static ShaderPropFloat Strength;
        internal static ShaderPropFloat StencilComp;
        internal static ShaderPropFloat Stencil;
        internal static ShaderPropFloat StencilOp;
        internal static ShaderPropFloat StencilWriteMask;
        internal static ShaderPropFloat StencilReadMask;
        internal static ShaderPropFloat ColorMask;
        internal static ShaderPropFloat EnvAlpha;
        internal static ShaderPropFloat EnvAdd;
        internal static ShaderPropFloat EmissionScaleUI;
        internal static ShaderPropFloat Glossiness;
        internal static ShaderPropFloat GlossMapScale;
        internal static ShaderPropEnum SmoothnessTexChannel;
        internal static ShaderPropFloat Metallic;
        internal static ShaderPropFloat BumpScale;
        internal static ShaderPropFloat OcclusionStrength;
        internal static ShaderPropFloat DetailNormalMapScale;
        internal static ShaderPropEnum UVSec;
        internal static ShaderPropFloat Mode;
        internal static ShaderPropEnum SrcBlend;
        internal static ShaderPropEnum DstBlend;
        internal static ShaderPropFloat ZWrite;
        internal static ShaderPropBool SpecularHeighlights;
        internal static ShaderPropBool GlossyReflections;

        internal static readonly ShaderPropColor Color         = new ShaderPropColor(PropKey._Color, ColorType.rgb, true);
        internal static readonly ShaderPropColor ColorA        = new ShaderPropColor(PropKey._Color, ColorType.rgba, true);
        internal static readonly ShaderPropColor ShadowColor   = new ShaderPropColor(PropKey._ShadowColor, ColorType.rgb, true);
        internal static readonly ShaderPropColor RimColor      = new ShaderPropColor(PropKey._RimColor, ColorType.rgb, true);
        internal static readonly ShaderPropColor OutlineColor  = new ShaderPropColor(PropKey._OutlineColor, ColorType.rgb);
        internal static readonly ShaderPropColor SpecColor     = new ShaderPropColor(PropKey._SpecColor, ColorType.rgb);
        internal static readonly ShaderPropColor ReflectColor  = new ShaderPropColor(PropKey._ReflectColor, ColorType.rgba);
        internal static readonly ShaderPropColor EmissionColor = new ShaderPropColor(PropKey._EmissionColor, ColorType.rgb);

        internal static readonly ShaderPropTex MainTex         = new ShaderPropTex(PropKey._MainTex, TexType.rgb);
        internal static readonly ShaderPropTex MainTexA        = new ShaderPropTex(PropKey._MainTex, TexType.rgba);
        internal static readonly ShaderPropTex ToonRamp        = new ShaderPropTex(PropKey._ToonRamp, TexType.rgb);
        internal static readonly ShaderPropTex ShadowTex       = new ShaderPropTex(PropKey._ShadowTex, TexType.rgb);
        internal static readonly ShaderPropTex ShadowRateToon  = new ShaderPropTex(PropKey._ShadowRateToon, TexType.rgb);
        internal static readonly ShaderPropTex OutlineTex      = new ShaderPropTex(PropKey._OutlineTex, TexType.rgb);
        internal static readonly ShaderPropTex OutlineToonRamp = new ShaderPropTex(PropKey._OutlineToonRamp, TexType.rgb);
        internal static readonly ShaderPropTex HiTex           = new ShaderPropTex(PropKey._HiTex, TexType.rgb);
        internal static readonly ShaderPropTex RenderTex       = new ShaderPropTex(PropKey._RenderTex, TexType.nulltex);
        internal static readonly ShaderPropTex BumpMap         = new ShaderPropTex(PropKey._BumpMap, TexType.bump, Keyword._NORMALMAP);
        internal static readonly ShaderPropTex SpecularTex     = new ShaderPropTex(PropKey._SpecularTex, TexType.nulltex);
        internal static readonly ShaderPropTex DecalTex        = new ShaderPropTex(PropKey._DecalTex, TexType.rgba);
        internal static readonly ShaderPropTex Detail          = new ShaderPropTex(PropKey._Detail, TexType.rgb);
        internal static readonly ShaderPropTex DetailTex       = new ShaderPropTex(PropKey._DetailTex, TexType.rgb);
        internal static readonly ShaderPropTex AnisoTex        = new ShaderPropTex(PropKey._AnisoTex, TexType.nulltex);
        internal static readonly ShaderPropTex Illum           = new ShaderPropTex(PropKey._Illum, TexType.a);
        internal static readonly ShaderPropTex Cube            = new ShaderPropTex(PropKey._Cube, TexType.cube);
        internal static readonly ShaderPropTex ReflectionTex   = new ShaderPropTex(PropKey._ReflectionTex, TexType.rgb);
        internal static readonly ShaderPropTex MultiColTex     = new ShaderPropTex(PropKey._MultiColTex, TexType.rgba);
        internal static readonly ShaderPropTex EnvMap          = new ShaderPropTex(PropKey._EnvMap, TexType.cube);
        internal static readonly ShaderPropTex MetallicGlossMap = new ShaderPropTex(PropKey._MetallicGlossMap, TexType.rgb, Keyword._METALLICGLOSSMAP);
        internal static readonly ShaderPropTex OcclusionMap   = new ShaderPropTex(PropKey._OcclusionMap, TexType.rgb);
        internal static readonly ShaderPropTex ParallaxMap     = new ShaderPropTex(PropKey._ParallaxMap, TexType.a, Keyword._PARALLAXMAP);
        internal static readonly ShaderPropTex EmissionMap     = new ShaderPropTex(PropKey._EmissionMap, TexType.a, Keyword._EMISSION);
        internal static readonly ShaderPropTex SpecGlossMap    = new ShaderPropTex(PropKey._SpecGlossMap, TexType.rgb, Keyword._SPECGLOSSMAP);
        internal static readonly ShaderPropTex DetailMask      = new ShaderPropTex(PropKey._DetailMask, TexType.rgb);// Keyword._DETAIL_MULX2
        internal static readonly ShaderPropTex DetailAlbedoMap = new ShaderPropTex(PropKey._DetailAlbedoMap, TexType.rgb, Keyword._DETAIL_MULX2);
        internal static readonly ShaderPropTex DetailNormalMap = new ShaderPropTex(PropKey._DetailNormalMap, TexType.bump, Keyword._NORMALMAP);
    }
    
    public enum PropType {
        tex,
        col,
        f,
    }
    public enum ValType {
        Tex,
        Color,
        Float,
        Bool,
        Keyword,
        Enum,
    }
    public enum ColorType {
        rgba,
        rgb,
        a,
    }
    public enum TexType {
        rgb,
        rgba,
        toon,
        bump,
        a,
        cube,
        nulltex,
    }

    public enum Keyword {
        NONE,
        _SPECGLOSSMAP,
        _METALLICGLOSSMAP,
        _PARALLAXMAP,
        _DETAIL_MULX2,
        _EMISSION,
        _NORMALMAP,
        _ALPHATEST_ON,
        _ALPHABLEND_ON,
        _ALPHAPREMULTIPLY_ON,
        _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A,
    }

    public enum PropKey {
        _MainTex,
        _ToonRamp,
        _ShadowTex,
        _ShadowRateToon,
        _OutlineTex,
        _OutlineToonRamp,
        _HiTex,
        _RenderTex,
        _BumpMap,
        _SpecularTex,
        _DecalTex,

        _Detail,
        _DetailMask,
        _DetailAlbedoMap,
        _DetailNormalMap,
        _DetailNormalMapScale,
        _DetailTex,

        _AnisoTex,
        _ParallaxMap,
        _Illum,
        _Cube,
        _ReflectionTex,
        _MultiColTex,
        _EnvMap,
        _MetallicGlossMap,
        _OcclusionMap,
        _EmissionMap,
        _SpecGlossMap,

        _Color,
        _ShadowColor,
        _RimColor,
        _OutlineColor,
        _SpecColor,
        _ReflectColor,
        _EmissionColor,

        _Shininess,
        _OutlineWidth,
        _RimPower,
        _RimShift,
        _HiRate,
        _HiPow,
        _FloatValue1,
        _FloatValue2,
        _FloatValue3,
        _Parallax,
        _Cutoff,
        _Cutout,
        _EmissionLM,
        _EmissionScaleUI,
        _UseMulticolTex,
        _Strength,
        _StencilComp,
        _Stencil,
        _StencilOp,
        _StencilWriteMask,
        _StencilReadMask,
        _ColorMask,
        _EnvAlpha,
        _EnvAdd,
        _ZTest,
        _ZTest2,
        _ZTest2Alpha,
        _UVSec,
        _Mode,
        _SrcBlend,
        _DstBlend,
        _ZWrite,
        _Glossiness,
        _GlossMapScale,
        _Metallic,
        _BumpScale,
        _OcclusionStrength,
        _SmoothnessTextureChannel,

        // Toggle
        _SpecularHighlights,
        _GlossyReflections,
        
        _SetManualRenderQueue,

//        _MyLightColor0,
//        _MyLightColor1,
//        _TintColor,
//        _FurLength,
//        _AnisoOffset,
//        PixelSnap,

        
        custom,
        Unkown,
    }
}
