/**
 * プリセットのセーブ・ロード用データクラス群 
 * JSONで出力可能なデータ構造とする
 * Colorなどは不可
 */
using System;
using System.Collections.Generic;
using CM3D2.AlwaysColorChangeEx.Plugin.Util;
using UnityEngine;

namespace CM3D2.AlwaysColorChangeEx.Plugin.Data {
    /// <summary>
    /// プリセット用データクラス
    /// </summary>
    public class PresetData {
        public string name;
        public List<CCSlot> slots = new List<CCSlot>();
        public List<CCMPN> mpns = new List<CCMPN>();
        public List<CCMPNValue> mpnvals = new List<CCMPNValue>();
        public Dictionary<string, CCPartsColor> partsColors = new Dictionary<string, CCPartsColor>();
        public Dictionary<string, bool> delNodes;
        public Dictionary<string, float> boneMorph;
    }

    /// <summary>
    /// スロット情報を扱うデータクラス.
    /// スロットのマスク設定や属するマテリアル情報を含む
    /// </summary>
    public class CCSlot {
        public TBody.SlotID id;
        public SlotState mask;
        public List<CCMaterial> materials;

        public CCSlot() { }
        public CCSlot(TBody.SlotID id) {
            this.id = id;
        }
        public CCSlot(string name) {
            id = (TBody.SlotID)Enum.Parse(typeof(TBody.SlotID), name);
        }
        public void Add(CCMaterial m) {
            if (materials == null) materials = new List<CCMaterial>();
            materials.Add(m);
        }
    }

    public class CCMPN {
        public MPN name;
        public string filename;
        public CCMPN() { }
        public CCMPN(string mpnName, string filename) {
            name = (MPN)Enum.Parse(typeof(MPN), mpnName);
            this.filename = filename;
        }
        public CCMPN(MPN name, string filename) {
            this.name = name;
            this.filename = filename;
        }
    }

    public class CCMPNValue {
        public MPN name;
        public int value;
        public int min;
        public int max;
        public CCMPNValue() { }
        public CCMPNValue(string mpnName, int v, int min, int max) {
            name = (MPN)Enum.Parse(typeof(MPN), mpnName);
            value = v;
            this.min = min;
            this.max = max;
        }
        public CCMPNValue(MPN name, int v, int min, int max) {
            this.name = name;
            value = v;
            this.min = min;
            this.max = max;
        }
    }

    public class CCColor {
        public float r;
        public float g;
        public float b;
        public float a;
        public CCColor() {}
        public CCColor(float r, float g, float b, float a) {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public CCColor(Color color) {
            r = color.r;
            g = color.g;
            b = color.b;
            a = color.a;
        }

        public Color ToColor() {
            return new Color(r, g, b, a);
        }
    }

    public class CCMaterial {
        private static readonly Dictionary<PropKey, Func<CCMaterial, CCColor>> COLOR_DIC =
            new Dictionary<PropKey, Func<CCMaterial, CCColor>>();
        private static readonly Dictionary<PropKey, Func<CCMaterial, float?>> FLOAT_DIC =
            new Dictionary<PropKey, Func<CCMaterial, float?>>();

        static CCMaterial() {
            COLOR_DIC.Add(PropKey._Color,        (mate) => mate.color);
            COLOR_DIC.Add(PropKey._ShadowColor,  (mate) => mate.shadowColor);
            COLOR_DIC.Add(PropKey._RimColor,     (mate) => mate.rimColor);
            COLOR_DIC.Add(PropKey._OutlineColor, (mate) => mate.outlineColor);
//        _SpecColor,
//        _ReflectColor,
//        _Emission,

            FLOAT_DIC.Add(PropKey._Shininess,    (mate) => mate.shininess);
            FLOAT_DIC.Add(PropKey._OutlineWidth, (mate) => mate.outlineWidth);
            FLOAT_DIC.Add(PropKey._RimPower,     (mate) => mate.rimPower);
            FLOAT_DIC.Add(PropKey._RimShift,     (mate) => mate.rimShift);
            FLOAT_DIC.Add(PropKey._HiRate,       (mate) => mate.hiRate);
            FLOAT_DIC.Add(PropKey._HiPow,        (mate) => mate.hiPow);
            FLOAT_DIC.Add(PropKey._Cutoff,       (mate) => mate.cutoff);
            FLOAT_DIC.Add(PropKey._Cutout,       (mate) => mate.cutout);
            FLOAT_DIC.Add(PropKey._FloatValue1,  (mate) => mate.floatVal1);
            FLOAT_DIC.Add(PropKey._FloatValue2,  (mate) => mate.floatVal2);
            FLOAT_DIC.Add(PropKey._FloatValue3,  (mate) => mate.floatVal3);
            FLOAT_DIC.Add(PropKey._ZTest,         (mate) => mate.ztest);
            FLOAT_DIC.Add(PropKey._ZTest2,        (mate) => mate.ztest2);
            FLOAT_DIC.Add(PropKey._ZTest2Alpha,  (mate) => mate.ztest2Alpha);
        }
            
        public string name;
        public string shader;
        // TODO
        public CCColor color;
        public CCColor shadowColor;
        public CCColor rimColor;
        public CCColor outlineColor;
        public float? shininess;
        public float? outlineWidth;
        public float? rimPower;
        public float? rimShift;
        public float? hiRate;
        public float? hiPow;
        public float? cutoff;
        public float? cutout;
        public float? floatVal1;
        public float? floatVal2;
        public float? floatVal3;
        public float? ztest;
        public float? ztest2;
        public float? ztest2Alpha;
        public List<TextureInfo> texList;

        public CCMaterial() {}

        public CCMaterial(Material m, ShaderType type) {
            name = m.name;
            shader = m.shader.name;
            
            foreach (var colProp in type.colProps) {
                var ccc = new CCColor(m.GetColor(colProp.propId));
                switch(colProp.key) {
                    case PropKey._Color:
                        color = ccc;
                        break;
                    case PropKey._ShadowColor:
                        shadowColor = ccc;
                        break;
                    case PropKey._RimColor:
                        rimColor = ccc;
                        break;
                    case PropKey._OutlineColor:
                        outlineColor = ccc;
                        break;
//        _SpecColor,
//        _ReflectColor,
//        _Emission,                        
                }
            }

            foreach (var prop in type.fProps) {
                var fVal = m.GetFloat(prop.propId);
                switch(prop.key) {
                    case PropKey._Shininess:
                        shininess = fVal;
                        break;
                    case PropKey._OutlineWidth:
                        outlineWidth = fVal;
                        break;
                    case PropKey._RimPower:
                        rimPower = fVal;
                        break;
                    case PropKey._RimShift:
                        rimShift = fVal;
                        break;
                    case PropKey._HiRate:
                        hiRate = fVal;
                        break;
                    case PropKey._HiPow:
                        hiPow = fVal;
                        break;
                    case PropKey._Cutoff:
                        cutoff = fVal;
                        break;
                    case PropKey._Cutout:
                        cutout = fVal;
                        break;
                    case PropKey._FloatValue1:
                        floatVal1 = fVal;
                        break;
                    case PropKey._FloatValue2:
                        floatVal2 = fVal;
                        break;
                    case PropKey._FloatValue3:
                        floatVal3 = fVal;
                        break;
                    case PropKey._ZTest:
                        ztest = fVal;
                        break;
                    case PropKey._ZTest2:
                        ztest2 = fVal;
                        break;
                    case PropKey._ZTest2Alpha:
                        ztest2Alpha = fVal;
                        break;
                }                        
            }
        }        

        public bool Apply(Material m) {
            var sh = Shader.Find(shader);
            if (sh == null) return false;

            LogUtil.Debug("apply shader:", sh.name);
            m.shader = sh;
            var type = ShaderType.Resolve(sh.name);
            if (type == ShaderType.UNKNOWN) return false;

            foreach (var colProp in type.colProps) {
                Func<CCMaterial, CCColor> func;
                if (COLOR_DIC.TryGetValue(colProp.key, out func)) {
                    m.SetColor(colProp.propId, func(this).ToColor());
                }
            }

            foreach (var prop in type.fProps) {
                Func<CCMaterial, float?> func;
                if (!FLOAT_DIC.TryGetValue(prop.key, out func)) continue;

                var fVal = func(this);
                if (fVal.HasValue) m.SetFloat(prop.propId, fVal.Value);
            }
            return true;
        }

        public void Add(TextureInfo ti) {
            if (texList == null) texList = new List<TextureInfo>();
            texList.Add(ti);
        }
    }

    public class TextureInfo {
        public string propName;
        public string texFile;
        public float offsetX;
        public float offsetY;
        public float scaleX;
        public float scaleY;
        public TexFilter filter;
    }

    public class TexFilter {
        public float Hue;
        public float Saturation;
        public float Lightness;
        public float InputMin;
        public float InputMax;
        public float InputMid;
        public float OutputMin;
        public float OutputMax;
        public TexFilter() { }
        public TexFilter(TextureModifier.FilterParam fp) {
            Hue = fp.Hue;
            Saturation = fp.Saturation;
            Lightness = fp.Lightness;
            InputMin = fp.InputMin;
            InputMax = fp.InputMax;
            InputMid = fp.InputMid;
            OutputMin = fp.OutputMin;
            OutputMax = fp.OutputMax;
        }
        public TextureModifier.FilterParam ToFilter() {
            var fp = new TextureModifier.FilterParam {
                Hue = {Value = Hue},
                Saturation = {Value = Saturation},
                Lightness = {Value = Lightness},
                InputMin = {Value = InputMin},
                InputMax = {Value = InputMax},
                InputMid = {Value = InputMid},
                OutputMin = {Value = OutputMin},
                OutputMax = {Value = OutputMax}
            };
            return fp;
        }
    }

    public class CCPartsColor {
        public bool bUse;
        public int mainHue;
        public int mainChroma;
        public int mainBrightness;
        public int mainContrast;
        public int shadowRate;
        public int shadowHue;
        public int shadowChroma;
        public int shadowBrightness;
        public int shadowContrast;

        public CCPartsColor() {}
        public CCPartsColor(MaidParts.PartsColor pc) {
            bUse           = pc.m_bUse;
            mainHue        = pc.m_nMainHue;
            mainChroma     = pc.m_nMainChroma;
            mainBrightness = pc.m_nMainBrightness;
            mainContrast   = pc.m_nMainContrast;
            shadowRate     = pc.m_nShadowRate;
            shadowHue      = pc.m_nShadowHue;
            shadowChroma   = pc.m_nShadowChroma;
            shadowBrightness = pc.m_nShadowBrightness;
            shadowContrast = pc.m_nShadowContrast;
        }
        public MaidParts.PartsColor ToStruct() {
            var pc = new MaidParts.PartsColor {
                m_bUse = bUse,
                m_nMainHue = mainHue,
                m_nMainChroma = mainChroma,
                m_nMainBrightness = mainBrightness,
                m_nMainContrast = mainContrast,
                m_nShadowRate = shadowRate,
                m_nShadowHue = shadowHue,
                m_nShadowChroma = shadowChroma,
                m_nShadowBrightness = shadowBrightness,
                m_nShadowContrast = shadowContrast
            };
            return pc;
        }
    }
}
