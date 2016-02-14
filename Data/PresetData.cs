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
        public float? floatVal1;
        public float? floatVal2;
        public float? floatVal3;
        public List<TextureInfo> texList;

        public CCMaterial() {}
        public CCMaterial(Material m, MaterialType mate) {
            name = m.name;
            shader = m.shader.name;
            
            if (mate.hasColor) {
                color = new CCColor(m.GetColor("_Color"));
            }
            if (mate.isLighted) {
                shadowColor = new CCColor(m.GetColor("_ShadowColor"));
            }
            if (mate.isToony) {
                rimColor = new CCColor(m.GetColor("_RimColor"));
            }
            if (mate.isOutlined) {
                outlineColor = new CCColor(m.GetColor("_OutlineColor"));
            }
            if (mate.isLighted) {
                shininess    = m.GetFloat("_Shininess");
            }
            if (mate.isOutlined) {
                outlineWidth = m.GetFloat("_OutlineWidth");
            }
            if (mate.isToony) {
                rimPower     = m.GetFloat("_RimPower");
                rimShift     = m.GetFloat("_RimShift");
            }
            if (mate.isHair) {
                hiRate       = m.GetFloat("_HiRate");
                hiPow        = m.GetFloat("_HiPow");
            }
            if (mate.hasFloat1) {
                floatVal1    = m.GetFloat("_FloatValue1");
            }
            if (mate.hasFloat2) {
                floatVal2    = m.GetFloat("_FloatValue2");
            }
            if (mate.hasFloat3) {
                floatVal3    = m.GetFloat("_FloatValue3");
            }           
        }
        public bool Apply(Material m) {
            Shader sh = Shader.Find(shader);
            if (sh == null) return false;

            m.shader = sh;
            MaterialType mate = ShaderMapper.resolve(shader);
            if (mate.hasColor && color != null) {
                m.SetColor("_Color", color.ToColor());
            }
            if (mate.isLighted && shadowColor != null) {
                m.SetColor("_ShadowColor", shadowColor.ToColor());
            }
            if (mate.isToony && rimColor != null) {
                m.SetColor("_RimColor", rimColor.ToColor());
            }
            if (mate.isOutlined && outlineColor != null) {
                m.SetColor("_OutlineColor", outlineColor.ToColor());
            }
            if (mate.isLighted && shininess.HasValue) {
                m.SetFloat("_Shininess", shininess.Value);
            }
            if (mate.isOutlined && outlineWidth.HasValue) {
                m.SetFloat("_OutlineWidth", outlineWidth.Value);
            }
            if (mate.isToony) {
                if (rimPower.HasValue) m.SetFloat("_RimPower", rimPower.Value);
                if (rimShift.HasValue) m.SetFloat("_RimShift", rimShift.Value);
            }
            if (mate.isHair) {
                if (hiRate.HasValue) m.SetFloat("_HiRate", hiRate.Value);
                if (hiPow.HasValue)  m.SetFloat("_HiPow", hiPow.Value);
            }
            if (mate.hasFloat1) {
                if (floatVal1.HasValue) m.SetFloat("_FloatValue1", floatVal1.Value);
            }
            if (mate.hasFloat2) {
                if (floatVal2.HasValue) m.SetFloat("_FloatValue2", floatVal2.Value);
            }
            if (mate.hasFloat3) {
                if (floatVal3.HasValue) m.SetFloat("_FloatValue3", floatVal3.Value);
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
}
