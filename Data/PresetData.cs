/**
 * プリセットのセーブ・ロード用データクラス群 
 * JSONで出力可能なデータ構造とする
 * Colorなどは不可
 */
using System;
using System.Collections.Generic;
using UnityEngine;
using CM3D2.AlwaysColorChangeEx.Plugin.Util;

namespace CM3D2.AlwaysColorChangeEx.Plugin.Data
{
    /// <summary>
    /// プリセット用データクラス
    /// </summary>
    public class PresetData
    {
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
    public class CCSlot
    {
        public TBody.SlotID id;
        public SlotState mask;
        public List<CCMaterial> materials;

        public CCSlot() { }
        public CCSlot(TBody.SlotID id) {
            this.id = id;
        }
        public CCSlot(string name) {
            this.id = (TBody.SlotID)Enum.Parse(typeof(TBody.SlotID), name);
        }
        public void Add(CCMaterial m) {
            if (materials == null) materials = new List<CCMaterial>();
            materials.Add(m);
        }
    }
    public class CCMPN
    {
        public MPN name;
        public string filename;
        public CCMPN() { }
        public CCMPN(string mpnName, string filename) {
            this.name = (MPN)Enum.Parse(typeof(MPN), mpnName);
            this.filename = filename;
        }
        public CCMPN(MPN name, string filename) {
            this.name = name;
            this.filename = filename;
        }
    }
    public class CCMPNValue
    {
        public MPN name;
        public int value;
        public int min;
        public int max;
        public CCMPNValue() { }
        public CCMPNValue(string mpnName, int v, int min, int max) {
            this.name = (MPN)Enum.Parse(typeof(MPN), mpnName);
            this.value = v;
            this.min = min;
            this.max = max;
        }
        public CCMPNValue(MPN name, int v, int min, int max) {
            this.name = name;
            this.value = v;
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
            this.r = color.r;
            this.g = color.g;
            this.b = color.b;
            this.a = color.a;
        }
        public Color ToColor() {
            return new Color(r, g, b, a);
        }
    }
    public class CCMaterial
    {
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
        public float? floatVal1;
        public float? floatVal2;
        public float? floatVal3;
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
                    case PropKey._FloatValue1:
                        floatVal1 = fVal;
                        break;
                    case PropKey._FloatValue2:
                        floatVal2 = fVal;
                        break;
                    case PropKey._FloatValue3:
                        floatVal3 = fVal;
                        break;
                }                        
            }
        }        

        public bool Apply(Material m) {
            Shader sh = Shader.Find(shader);
            if (sh == null) return false;

            m.shader = sh;
            var type = ShaderType.Resolve(sh.name);
            if (type == ShaderType.UNKNOWN) return false;

            foreach (var colProp in type.colProps) {
                CCColor cc = null;
                switch(colProp.key) {
                    case PropKey._Color:
                        cc = color;
                        break;
                    case PropKey._ShadowColor:
                        cc = shadowColor;
                        break;
                    case PropKey._RimColor:
                        cc = rimColor;
                        break;
                    case PropKey._OutlineColor:
                        cc = outlineColor;
                        break;
//        _SpecColor,
//        _ReflectColor,
//        _Emission,                        
                }
                if (cc != null) m.SetColor(colProp.propId, cc.ToColor());
                
            }

            foreach (var prop in type.fProps) {
                float? fVal = null;
                switch(prop.key) {
                    case PropKey._Shininess:
                        fVal = shininess;
                        break;
                    case PropKey._OutlineWidth:
                        fVal = outlineWidth;
                        break;
                    case PropKey._RimPower:
                        fVal = rimPower;
                        break;
                    case PropKey._RimShift:
                        fVal = rimShift;
                        break;
                    case PropKey._HiRate:
                        fVal = hiRate;
                        break;
                    case PropKey._HiPow:
                        fVal = hiPow;
                        break;
                    case PropKey._Cutoff:
                        fVal = cutoff;
                        break;
                    case PropKey._FloatValue1:
                        fVal = floatVal1;
                        break;
                    case PropKey._FloatValue2:
                        fVal = floatVal2;
                        break;
                    case PropKey._FloatValue3:
                        fVal = floatVal3;
                        break;
                }
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
            var fp= new TextureModifier.FilterParam();
            fp.Hue.Value = Hue;
            fp.Saturation.Value = Saturation;
            fp.Lightness.Value = Lightness;
            fp.InputMin.Value = InputMin;
            fp.InputMax.Value = InputMax;
            fp.InputMid.Value = InputMid;
            fp.OutputMin.Value = OutputMin;
            fp.OutputMax.Value = OutputMax;
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
        public MaidParts.PartsColor toStruct() {
            var pc = new MaidParts.PartsColor();
            pc.m_bUse = bUse;
            pc.m_nMainHue = mainHue;
            pc.m_nMainChroma = mainChroma;
            pc.m_nMainBrightness = mainBrightness ;
            pc.m_nMainContrast = mainContrast;
            pc.m_nShadowRate = shadowRate;
            pc.m_nShadowHue = shadowHue;
            pc.m_nShadowChroma = shadowChroma;
            pc.m_nShadowBrightness = shadowBrightness;
            pc.m_nShadowContrast = shadowContrast;
            return pc;
        }
    }
}
