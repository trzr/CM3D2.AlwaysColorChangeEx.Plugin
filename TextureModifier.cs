// テクスチャの色変え処理
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CM3D2.AlwaysColorChange.Plugin
{
    internal class TextureModifier
    {
        private FilterParams filterParams = new FilterParams();
        private OriginalTextureCache originalTextureCache = new OriginalTextureCache();

        public void Clear()
        {
            originalTextureCache.Clear();
        }

        public bool IsValidTarget(Maid maid, string slotName, Material material, string propName)
        {
            return GetKey(maid, slotName, material, propName) != null;
        }

        public void ProcGUI(Maid maid, string slotName, Material material, string propName, float margin, float fontSize, float itemHeight)
        {
            FilterParam fp = Get(maid, slotName, material, propName);
            if (fp != null)
            {
                fp.ProcGUI(margin, fontSize, itemHeight);
            }
        }

        public void Update(
            Maid maid,
            Dictionary<string, List<Material>> slotMaterials,
            List<Texture2D> textures,
            string slotName, int materialIndex, string propName
        )
        {
            originalTextureCache.Refresh(textures.ToArray());

            // マウスボタンが離されたタイミングでフィルターを適用する
            if (Input.GetMouseButtonUp(0))
            {
                FilterTexture(slotMaterials, textures, maid, slotName, materialIndex, propName);
            }
        }

        private string GetKey(Maid maid, string slotName, Material material, string propName)
        {
            if (maid == null || material == null || string.IsNullOrEmpty(propName))
            {
                return null;
            }
            var tex2d = material.GetTexture(propName) as Texture2D;
            if (tex2d == null || string.IsNullOrEmpty(tex2d.name))
            {
                return null;
            }
            return string.Format("{0}/{1}/{2}/{3}"
                , maid.Param.status.guid
                , slotName
                , material.name
                , tex2d.name);
        }

        private FilterParam Get(Maid maid, string slotName, Material material, string propName)
        {
            string key = GetKey(maid, slotName, material, propName);
            return filterParams.GetOrAdd(key);
        }

        private void FilterTexture(
            Dictionary<string, List<Material>> slotMaterials,
            List<Texture2D> textures, Maid maid, string slotName, int materialIndex, string propName
        )
        {
            Material material = null;
            Texture2D texture = null;
            {
                List<Material> materials;
                if (slotMaterials.TryGetValue(slotName, out materials) && materials != null)
                {
                    material = materials.ElementAtOrDefault(materialIndex);
                    if (material != null)
                    {
                        texture = material.GetTexture(propName) as Texture2D;
                    }
                }
            }
            if (material == null || texture == null)
            {
                return;
            }

            FilterParam filterParam = Get(maid, slotName, material, propName);
            if (!filterParam.Dirty.Value)
            {
                return;
            }

            originalTextureCache.SetDirty(texture);
            filterParam.ClearDirtyFlag();

            float outputBase = filterParam.OutputMin * 0.01f;
            float outputScale = (filterParam.OutputMax - filterParam.OutputMin) * 0.01f;

            float inputExp = Mathf.Log(filterParam.InputMid * 0.01f) / Mathf.Log(0.5f);
            float inputBase = (-filterParam.InputMin / (filterParam.InputMax - filterParam.InputMin));
            float inputScale = 1f / ((filterParam.InputMax - filterParam.InputMin) * 0.01f);

            float hue = filterParam.Hue / 360f;
            float saturation = filterParam.Saturation / 100f;
            float lightness = filterParam.Lightness / 100f;

            Filter(texture, originalTextureCache.GetOriginalTexture(texture), (color) =>
                {
                    Color c = color;

                    c.r = Mathf.Clamp01(c.r * inputScale + inputBase);
                    c.g = Mathf.Clamp01(c.g * inputScale + inputBase);
                    c.b = Mathf.Clamp01(c.b * inputScale + inputBase);

                    c.r = Mathf.Pow(c.r, inputExp);
                    c.g = Mathf.Pow(c.g, inputExp);
                    c.b = Mathf.Pow(c.b, inputExp);

                    Vector4 hsl = ColorUtil.ColorToHsl(c);
                    hsl.x = (hsl.x + hue) % 1f;
                    hsl.y *= saturation;
                    hsl.z *= lightness;
                    c = ColorUtil.HslToColor(hsl);

                    c.r = c.r * outputScale + outputBase;
                    c.g = c.g * outputScale + outputBase;
                    c.b = c.b * outputScale + outputBase;

                    return c;
                });
        }

        private void Filter(Texture2D dstTexture, Texture2D srcTexture, Func<Color32, Color32> mapFunc)
        {
            if (dstTexture == null || srcTexture == null || dstTexture.width != srcTexture.width || dstTexture.height != srcTexture.height)
            {
                return;
            }
            int maxIndex = dstTexture.width * dstTexture.height;
            Color32[] src = srcTexture.GetPixels32(0);
            Color32[] dst = dstTexture.GetPixels32(0);
            for (int i = 0; i < maxIndex; i++)
            {
                dst[i] = mapFunc(src[i]);
            }
            dstTexture.SetPixels32(dst);
            dstTexture.Apply();
        }

        private class FilterParams
        {
            private Dictionary<string, FilterParam> params_;

            public FilterParams()
            {
                Clear();
            }

            public void Clear()
            {
                params_ = new Dictionary<string, FilterParam>();
            }

            public FilterParam GetOrAdd(string key)
            {
                FilterParam p;
                if (params_.TryGetValue(key, out p))
                {
                    return p;
                }
                p = new FilterParam();
                params_[key] = p;
                return p;
            }
        }

        private class FilterParam
        {
            public bool IsDirty { get { return Dirty.Value; } }

            public DirtyFlag Dirty;
            public DirtyValue Hue;
            public DirtyValue Saturation;
            public DirtyValue Lightness;
            public DirtyValue InputMin;
            public DirtyValue InputMax;
            public DirtyValue InputMid;
            public DirtyValue OutputMin;
            public DirtyValue OutputMax;

            public FilterParam()
            {
                Clear();
            }

            public void Clear()
            {
                Dirty = new DirtyFlag();
                Hue = new DirtyValue(Dirty, "色相", 0f, 0f, 360f);
                Saturation = new DirtyValue(Dirty, "彩度", 100f, 0f, 200f);
                Lightness = new DirtyValue(Dirty, "明度", 100f, 0f, 200f);
                InputMin = new DirtyValue(Dirty, "InpMin", 0f, 0f, 100f);
                InputMax = new DirtyValue(Dirty, "InpMax", 100f, 0f, 100f);
                InputMid = new DirtyValue(Dirty, "InpMid", 50f, 0f, 100f);
                OutputMin = new DirtyValue(Dirty, "OutMin", 0f, 0f, 100f);
                OutputMax = new DirtyValue(Dirty, "OutMax", 100f, 0f, 100f);
            }

            public void ClearDirtyFlag()
            {
                Dirty.Value = false;
            }

            public void ProcGUI(float margin, float fontSize, float itemHeight)
            {
                guiSlider(margin, Hue);
                guiSlider(margin, Saturation);
                guiSlider(margin, Lightness);
                guiSlider(margin, InputMin);
                guiSlider(margin, InputMax);
                guiSlider(margin, InputMid);
                guiSlider(margin, OutputMin);
                guiSlider(margin, OutputMax);

                GUILayout.Space(margin * 2f);
                GUILayout.BeginHorizontal();
                GUILayout.Space(margin * 8f);
                if (GUILayout.Button("リセット"))
                {
                    Clear();
                    Dirty.Value = true;
                }
                GUILayout.Space(margin * 8f);
                GUILayout.EndHorizontal();
                GUILayout.Space(margin * 2f);
            }

            private static void guiSlider(float margin, DirtyValue dirtyValue)
            {
                float val = dirtyValue.Value;
                GUILayout.BeginHorizontal();
                GUILayout.Label(dirtyValue.Name, GUILayout.Width(64));
                GUILayout.Label(string.Format("{0:F0}", val), GUILayout.Width(32));
                val = GUILayout.HorizontalSlider(val, dirtyValue.Min, dirtyValue.Max);
                GUILayout.Space(margin * 2f);
                GUILayout.EndHorizontal();
                dirtyValue.Value = val;
            }
        }

        private class DirtyFlag
        {
            public bool Value = false;
        }

        private class DirtyValue
        {
            private DirtyFlag dirtyFlag;
            private float val;

            public string Name { get; private set; }
            public float Min { get; private set; }
            public float Max { get; private set; }
            public float Value
            {
                get
                {
                    return val;
                }
                set
                {
                    float v = Mathf.Clamp(value, Min, Max);
                    if (!val.Equals(v))
                    {
                        val = v;
                        dirtyFlag.Value = true;
                    }
                }
            }

            public DirtyValue(DirtyFlag dirtyFlag, string name, float val, float min, float max)
            {
                Name = name;
                Min = min;
                Max = max;
                this.dirtyFlag = dirtyFlag;
                this.val = val;
            }

            public static implicit operator float (DirtyValue dirtyValue)
            {
                return dirtyValue.val;
            }
        }

        private class OriginalTextureCache
        {
            private Dictionary<string, Texture2D> OriginalTextures;
            private Dictionary<string, bool> DirtyTextures;

            public OriginalTextureCache()
            {
                Clear();
            }

            public void Clear()
            {
                if (OriginalTextures != null)
                {
                    // テクスチャの開放
                    foreach (Texture2D tex in OriginalTextures.Values)
                    {
                        try
                        {
                            UnityEngine.Object.Destroy(tex);
                        }
                        catch { }
                    }
                }
                OriginalTextures = new Dictionary<string, Texture2D>();
                DirtyTextures = new Dictionary<string, bool>();
            }

            public void Refresh(Texture2D[] maidTextures)
            {
                // 既に使われなくなったテクスチャを削除
                var names = new List<string>();
                foreach (string name in OriginalTextures.Keys)
                {
                    bool b = false;
                    foreach (Texture2D t in maidTextures)
                    {
                        if (t.name == name)
                        {
                            b = true;
                            break;
                        }
                    }
                    if (!b)
                    {
                        names.Add(name);
                    }
                }
                foreach (string name in names)
                {
                    Texture2D tex;
                    if (OriginalTextures.TryGetValue(name, out tex))
                    {
                        UnityEngine.Object.Destroy(tex);
                    }
                    OriginalTextures.Remove(name);
                    DirtyTextures.Remove(name);
                }

                // 知らないテクスチャを追加
                foreach (Texture2D t in maidTextures)
                {
                    if (!OriginalTextures.ContainsKey(t.name) && !IsDirty(t))
                    {
                        OriginalTextures[t.name] = UnityEngine.Object.Instantiate(t) as Texture2D;
                        DirtyTextures[t.name] = false;
                    }
                }
            }

            public void SetDirty(Texture2D texture)
            {
                if (texture != null)
                {
                    SetDirty(texture.name);
                }
            }

            public void SetDirty(string name)
            {
                DirtyTextures[name] = true;
            }

            public bool IsDirty(Texture2D texture)
            {
                return texture != null && IsDirty(texture.name);
            }

            public bool IsDirty(string name)
            {
                bool b;
                if (DirtyTextures.TryGetValue(name, out b))
                {
                    return b;
                }
                return false;
            }

            public Texture2D GetOriginalTexture(Texture2D texture)
            {
                return texture != null ? GetOriginalTexture(texture.name) : null;
            }

            public Texture2D GetOriginalTexture(string name)
            {
                Texture2D t;
                if (OriginalTextures.TryGetValue(name, out t))
                {
                    return t;
                }
                return null;
            }
        }

        private static class ColorUtil
        {
            // Color -> HSL 変換
            public static Vector4 ColorToHsl(Color c)
            {
                c.r = Mathf.Clamp01(c.r);
                c.g = Mathf.Clamp01(c.g);
                c.b = Mathf.Clamp01(c.b);

                float max = Mathf.Max(c.r, Mathf.Max(c.g, c.b));
                float min = Mathf.Min(c.r, Mathf.Min(c.g, c.b));

                float h = 0f;
                float s = 0f;
                float l = (max + min) / 2f;
                float d = max - min;
                if (d != 0f)
                {
                    s = (l > 0.5f) ? (d / (2f - max - min)) : (d / (max + min));
                    if (max == c.r)
                    {
                        h = (c.g - c.b) / d + (c.g < c.b ? 6f : 0f);
                    }
                    else if (max == c.g)
                    {
                        h = (c.b - c.r) / d + 2f;
                    }
                    else
                    {
                        h = (c.r - c.g) / d + 4f;
                    }
                    h /= 6f;
                }
                return new Vector4(h, s, l, c.a);
            }

            // HSL -> Color 変換
            public static Color HslToColor(Vector4 hsl)
            {
                Color c;
                c.a = hsl.w;

                float h = hsl.x;
                float s = hsl.y;
                float l = hsl.z;

                if (s == 0f)
                {
                    c.r = l;
                    c.g = l;
                    c.b = l;
                }
                else
                {
                    float y = (l < 0.5f) ? (l * (1f + s)) : ((l + s) - l * s);
                    float x = 2f * l - y;
                    c.r = Hue(x, y, h + 1f / 3f);
                    c.g = Hue(x, y, h);
                    c.b = Hue(x, y, h - 1f / 3f);
                }

                return c;
            }

            private static float Hue(float x, float y, float t)
            {
                if (t < 0f)
                {
                    t += 1f;
                }
                else if (t > 1f)
                {
                    t -= 1f;
                }

                if (t < 1f / 6f)
                {
                    return x + (y - x) * 6f * t;
                }
                else if (t < 2f / 6f)
                {
                    return y;
                }
                else if (t < 4f / 6f)
                {
                    return x + (y - x) * 6f * (2f / 3f - t);
                }
                else
                {
                    return x;
                }
            }
        }
    }
}
