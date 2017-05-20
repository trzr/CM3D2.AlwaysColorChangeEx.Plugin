using System;
using System.Collections.Generic;
using CM3D2.AlwaysColorChangeEx.Plugin.Data;
using CM3D2.AlwaysColorChangeEx.Plugin.UI;
using UnityEngine;

namespace CM3D2.AlwaysColorChangeEx.Plugin.Data
{
    /// <summary>
    /// 各シェーダ(マテリアル）のプロパティ情報を扱うクラス
    /// </summary>
    public abstract class ShaderProp
    {
        protected ShaderProp(string name, PropKey key, int id, ValType valType) {
            this.name = name;
            this.key = key;
            this.keyName = key.ToString();
            this.propId = id;
            Init(valType);
        }
        protected ShaderProp(PropKey key, ValType valType) {
            this.key = key;
            this.keyName = key.ToString();
            this.name = keyName.Substring(1);
            this.propId = Shader.PropertyToID(keyName);
            Init(valType);
        }

        private void Init(ValType valType1) {
            this.valType = valType1;
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

        public string name;
        public PropKey key;
        public string keyName;
        public int propId;
        public PropType type;
        public ValType valType;
    }
    public class PresetOperation {
        public string label;
        public Func<float, float> func;
        public PresetOperation(string label, Func<float, float> func) {
            this.label = label;
            this.func = func;
        }
    }
    public class ShaderPropFloat :ShaderProp
    {

        public ShaderPropFloat(string name, PropKey key, int id) : base(name, key, id, ValType.Float) { }
        public ShaderPropFloat(PropKey key) : base(key, ValType.Float) { }

        protected ShaderPropFloat(string name, PropKey key, int id, ValType valType) : base(name, key, id, valType) { }
        public ShaderPropFloat(PropKey key, ValType valType) : base(key, valType) { }

        public ShaderPropFloat(PropKey key, string format, float[] range,
                                  PresetOperation[] opts, float defaultVal, params float[] presetVals) 
            : this(key, new EditRange(format, range[2], range[3]), range, opts, defaultVal, presetVals) {
        }
        public ShaderPropFloat(PropKey key, EditRange range, float[] sliderRange,
                                  PresetOperation[] opts, float defaultVal, params float[] presetVals) : base(key, ValType.Float) {
            this.range = range;
            this.sliderMin = sliderRange[0];
            this.sliderMax = sliderRange[1];
            this.opts = opts;
            this.presetVals = presetVals;
            this.defaultVal = defaultVal;
        }

        public EditRange range;

        public float sliderMin;
        public float sliderMax;

        public float defaultVal;
        public PresetOperation[] opts;
        public float[] presetVals;

        public void SetValue(Material m, float val) {
            m.SetFloat(propId, val);
        }
    }
    public class ShaderPropBool :ShaderPropFloat
    {
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
    public class ShaderPropColor :ShaderProp
    {
        public ShaderPropColor(string name, PropKey key, int id, ColorType colType) : base(name, key, id, ValType.Color) {
            this.colorType = colType;
        }
        public ShaderPropColor(PropKey key, ColorType colType) : base(key, ValType.Color) {
            this.colorType = colType;
        }
        public ColorType colorType;
        public void SetValue(Material m, Color col) {
            m.SetColor(propId, col);
        }
        public Color defaultVal;
    }
    public class ShaderPropTex : ShaderProp
    {
        public ShaderPropTex(string name, PropKey key, int id, TexType type) : base(name, key, id, ValType.Tex) {
            this.texType = type;
        }
        public ShaderPropTex(PropKey key, TexType type) : base(key, ValType.Tex) {
            this.texType = type;
        }
        public TexType texType;

        public void SetValue(Material m, Texture2D tex) {
//            m.SetFloat(key, val? 1f : 0f);
        }
    }
    public static class ShaderPropType {
        static PresetOperation sliderL = new PresetOperation("<", (val) => val*0.9f);
        static PresetOperation sliderR = new PresetOperation(">", (val) => val*1.1f);
        static PresetOperation invert  = new PresetOperation("x-1", (val) => val*-1f);
        static PresetOperation plus1   = new PresetOperation("+", (val) => val+1);
        static PresetOperation plus10   = new PresetOperation("++", (val) => val+10);
        static PresetOperation minus1  = new PresetOperation("-", (val) => val-1);
        static PresetOperation minus10  = new PresetOperation("--", (val) => val-10);

        private static readonly PresetOperation[] PRESET_RATIO = {sliderL, sliderR};
        private static readonly PresetOperation[] PRESET_INV = { invert };
        private static readonly PresetOperation[] PRESET_PM = { minus10, minus1, plus1, plus10};

        private static readonly Settings settings = Settings.Instance;
        private static Dictionary<string, ShaderProp> customProps = new Dictionary<string, ShaderProp>();
        private static bool initialized;
        public static void Initialize() {
            if (!initialized) {
                // 設定値を利用するため、LazyInitとする
                RenderQueue      = new ShaderPropFloat(PropKey._SetManualRenderQueue, EditRange.renderQueue,
                                                       new float[]{0, 5000f,}, PRESET_PM, 3000, 2000, 3000);

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
                Parallax         = new ShaderPropFloat(PropKey._Parallax, "F4",
                                                       new float[] {0.005f, 0.08f, 0.001f, 0.1f}, PRESET_RATIO, 0.02f, 0.02f);
                Cutoff           = new ShaderPropFloat(PropKey._Cutoff, "F3",
                                                       new float[] {0f, 1f, 0f, 1f}, PRESET_RATIO, 0.5f, 0.5f);
                EmissionLM       = new ShaderPropBool(PropKey._EmissionLM);
                UseMulticolTex   = new ShaderPropBool(PropKey._UseMulticolTex);

                Strength         = new ShaderPropFloat(PropKey._Strength, "F2",
                                                       new float[] {0f, 1f, 0f, 1f}, PRESET_RATIO, 0.2f, 0.2f);
                StencilComp      = new ShaderPropFloat(PropKey._StencilComp, "F0",
                                                       new float[] {0f, 255f, 0f, 255f}, PRESET_RATIO, 8f, 8f);
                Stencil          = new ShaderPropFloat(PropKey._Stencil, "F0",
                                                       new float[] {0f, 255f, 0f, 255f}, PRESET_RATIO, 0f, 0f);
                StencilOp        = new ShaderPropFloat(PropKey._StencilOp, "F0",
                                                       new float[] {0f, 255f, 0f, 255f}, PRESET_RATIO, 0f, 0f);
                StencilWriteMask = new ShaderPropFloat(PropKey._StencilWriteMask, "F0",
                                                       new float[] {0f, 255f, 0f, 255f}, PRESET_RATIO, 255f, 255f);
                StencilReadMask  = new ShaderPropFloat(PropKey._StencilReadMask, "F0",
                                                       new float[] {0f, 255f, 0f, 255f}, PRESET_RATIO, 255f, 255f);
                ColorMask        = new ShaderPropFloat(PropKey._ColorMask, "F0",
                                                       new float[] {0f, 255f, 0f, 255f}, PRESET_RATIO, 255f, 255f);
                EnvAlpha         = new ShaderPropFloat(PropKey._EnvAlpha, "F1",
                                                       new float[] {0f, 1f, 0f, 1f}, PRESET_RATIO, 0f, 0f);
                EnvAdd           = new ShaderPropFloat(PropKey._EnvAdd, "F1",
                                                       new float[] {1f, 2f, 1f, 2f}, PRESET_RATIO, 1f, 1f);

                initialized = true;
            }
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

        internal readonly static ShaderPropColor Color        = new ShaderPropColor(PropKey._Color, ColorType.rgb);
        internal readonly static ShaderPropColor ColorA       = new ShaderPropColor(PropKey._Color, ColorType.rgba);
        internal readonly static ShaderPropColor ShadowColor  = new ShaderPropColor(PropKey._ShadowColor, ColorType.rgb);
        internal readonly static ShaderPropColor RimColor     = new ShaderPropColor(PropKey._RimColor, ColorType.rgb);
        internal readonly static ShaderPropColor OutlineColor = new ShaderPropColor(PropKey._OutlineColor, ColorType.rgb);
        internal readonly static ShaderPropColor SpecColor    = new ShaderPropColor(PropKey._SpecColor, ColorType.rgba);
        internal readonly static ShaderPropColor ReflectColor = new ShaderPropColor(PropKey._ReflectColor, ColorType.rgba);
        internal readonly static ShaderPropColor Emission     = new ShaderPropColor(PropKey._Emission, ColorType.rgba);

        internal readonly static ShaderPropTex MainTex        = new ShaderPropTex(PropKey._MainTex, TexType.rgb);
        internal readonly static ShaderPropTex MainTex_a      = new ShaderPropTex(PropKey._MainTex, TexType.rgba);
        internal readonly static ShaderPropTex ToonRamp       = new ShaderPropTex(PropKey._ToonRamp, TexType.rgb);
        internal readonly static ShaderPropTex ShadowTex      = new ShaderPropTex(PropKey._ShadowTex, TexType.rgb);
        internal readonly static ShaderPropTex ShadowRateToon = new ShaderPropTex(PropKey._ShadowRateToon, TexType.rgb);
        internal readonly static ShaderPropTex HiTex          = new ShaderPropTex(PropKey._HiTex, TexType.rgb);
        internal readonly static ShaderPropTex RenderTex      = new ShaderPropTex(PropKey._RenderTex, TexType.nulltex);
        internal readonly static ShaderPropTex BumpMap        = new ShaderPropTex(PropKey._BumpMap, TexType.bump);
        internal readonly static ShaderPropTex SpecularTex    = new ShaderPropTex(PropKey._SpecularTex, TexType.nulltex);
        internal readonly static ShaderPropTex DecalTex       = new ShaderPropTex(PropKey._DecalTex, TexType.rgba);
        internal readonly static ShaderPropTex Detail         = new ShaderPropTex(PropKey._Detail, TexType.rgb);
        internal readonly static ShaderPropTex DetailTex      = new ShaderPropTex(PropKey._DetailTex, TexType.rgb);
        internal readonly static ShaderPropTex AnisoTex       = new ShaderPropTex(PropKey._AnisoTex, TexType.nulltex);
        internal readonly static ShaderPropTex ParallaxMap    = new ShaderPropTex(PropKey._ParallaxMap, TexType.a);
        internal readonly static ShaderPropTex Illum          = new ShaderPropTex(PropKey._Illum, TexType.a);
        internal readonly static ShaderPropTex Cube           = new ShaderPropTex(PropKey._Cube, TexType.cube);
        internal readonly static ShaderPropTex ReflectionTex  = new ShaderPropTex(PropKey._ReflectionTex, TexType.rgb);
        internal readonly static ShaderPropTex MultiColTex    = new ShaderPropTex(PropKey._MultiColTex, TexType.rgba);
        internal readonly static ShaderPropTex EnvMap         = new ShaderPropTex(PropKey._EnvMap, TexType.cube);        
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

    public enum PropKey {
        _MainTex,
        _ToonRamp,
        _ShadowTex,
        _ShadowRateToon,
        _HiTex,
        _RenderTex,
        _BumpMap,
        _SpecularTex,
        _DecalTex,
        _Detail,
        _DetailTex,
        _AnisoTex,
        _ParallaxMap,
        _Illum,
        _Cube,
        _ReflectionTex,
        _MultiColTex,
        _EnvMap,

        _Color,
        _ShadowColor,
        _RimColor,
        _OutlineColor,
        _SpecColor,
        _ReflectColor,
        _Emission,

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
        _EmissionLM,
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
