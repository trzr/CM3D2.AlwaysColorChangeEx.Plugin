// テクスチャの色変え処理
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using CM3D2.AlwaysColorChangeEx.Plugin.Data;
using CM3D2.AlwaysColorChangeEx.Plugin.Util;
using CM3D2.AlwaysColorChangeEx.Plugin.UI;

namespace CM3D2.AlwaysColorChangeEx.Plugin {
    public class TextureModifier {
        private static readonly TextureModifier INSTANCE = new TextureModifier();
        public static TextureModifier Instance {
            get { return INSTANCE; }
        }

        private readonly FilterParams filterParams = new FilterParams();
        private readonly OriginalTextureCache originalTexCache = new OriginalTextureCache();

        public static UIParams uiParams;
        public static GUIStyle lStyle;
        private static readonly OutputUtil OUT_UTIL = OutputUtil.Instance;
        
        public void Clear() {
            originalTexCache.Clear();
            filterParams.Clear();
        }

        public bool IsValidTarget(Maid maid, string slotName, Material material, string propName) {
            return GetKey(maid, slotName, material, propName) != null;
        }

        public void ProcGUI(Maid maid, string slotName, Material material, string propName) {
            // material 抽出 => texture 抽出
            var tex2D = material.GetTexture(propName) as Texture2D;
            if (tex2D == null || string.IsNullOrEmpty(tex2D.name)) return ;

            var key = CreateKey(maid.Param.status.guid, slotName, material.name, tex2D.name);
            var fp = filterParams.GetOrAdd(key.ToString());
            fp.ProcGUI(tex2D);
        }

        public void Update(Maid maid, Dictionary<string, List<Material>> slotMaterials,
            List<Texture2D> textures, EditTarget texEdit ) {

            originalTexCache.Refresh(textures.ToArray());

            FilterTexture(slotMaterials, textures, maid, texEdit);
        }

        private StringBuilder CreateKey(params string[] names) {
            var length= names.Sum(name => name.Length);
            length += names.Length;// -1
            var key = new StringBuilder(length);

            // wear/Dress_cmo_004_z2/Dress_cmo_004_z2_wear_2
            for (var i=0; i< names.Length; i++) {
                if (i != 0) key.Append('/');
                key.Append(names[i]);
            }
            return key;
        }
        public bool UpdateTex(Maid maid, Material[] slotMaterials, EditTarget texEdit) {
            // material 抽出 => texture 抽出
            if (slotMaterials.Length <=  texEdit.matNo) return false;
            var mat = slotMaterials[texEdit.matNo];

            return UpdateTex(maid, mat, texEdit);
        }

        public bool UpdateTex(Maid maid, Material mat, EditTarget texEdit) {
            var tex2D = mat.GetTexture(texEdit.propName) as Texture2D;
            if (tex2D == null || string.IsNullOrEmpty(tex2D.name)) return false;

            var key = CreateKey(maid.Param.status.guid, texEdit.slotName, mat.name, tex2D.name);
            var filterParam = filterParams.GetOrAdd(key.ToString());

            // スライダー変更がなければ何もしない
            if (!filterParam.IsDirty) return false;
            //LogUtil.DebugLogF("Update Texture. slot={0}, material={0}, tex={1}", texEdit.slotName, mat.name, tex2d.name);

            FilterTexture(tex2D, filterParam);
            return true;
        }
        public bool RemoveCache(Texture2D tex2D) {
            return originalTexCache.Remove(tex2D.name);
        }

        public bool RemoveFilter(Maid maid, string slotName, Material mat, string propName) {
            var tex2D = mat.GetTexture(propName) as Texture2D;
            if (tex2D == null || string.IsNullOrEmpty(tex2D.name)) return false;

            return RemoveFilter(maid, slotName, mat, tex2D);
        }

        public bool RemoveFilter(Maid maid, string slotName, Material mat, Texture2D tex2D) {
            var key = CreateKey(maid.Param.status.guid, slotName, mat.name, tex2D.name);
            return filterParams.Remove(key.ToString());
        }

        public bool IsChanged(Maid maid, string slotName, Material mat, string propName) {
            var tex2d = mat.GetTexture(propName) as Texture2D;
            if (tex2d == null || string.IsNullOrEmpty(tex2d.name)) return false;

            return IsChanged(maid, slotName, mat.name, tex2d.name);
        }
        public bool IsChanged(Maid maid, string slotName, string matName, string texName) 
        {
            var key = CreateKey(maid.Param.status.guid, slotName, matName, texName);

            var filterParam = filterParams.Get(key.ToString());
            return (filterParam != null) && !filterParam.hasNotChanged();
        }
        public FilterParam GetFilter(Maid maid, string slotName, Material mat, int propId) {
            var tex2d = mat.GetTexture(propId) as Texture2D;
            if (tex2d == null || string.IsNullOrEmpty(tex2d.name)) return null;

            return GetFilter(maid, slotName, mat.name, tex2d.name);
        }
        
        public FilterParam GetFilter(Maid maid, string slotName, Material mat, string propName) {
            var tex2d = mat.GetTexture(propName) as Texture2D;
            if (tex2d == null || string.IsNullOrEmpty(tex2d.name)) return null;

            return GetFilter(maid, slotName, mat.name, tex2d.name);
        }
        public FilterParam GetFilter(Maid maid, string slotName, string matName, string texName) {
            var key = CreateKey(maid.Param.status.guid, slotName, matName, texName);

            return filterParams.Get(key.ToString());
        }
        public bool DuplicateFilter(Maid maid, string slotName, Material mat, string fromPropName, string toPropName) {
            var srcTex2d = mat.GetTexture(fromPropName) as Texture2D;
            if (srcTex2d == null || string.IsNullOrEmpty(srcTex2d.name)) return false;

            var srcFilter = GetFilter(maid, slotName, mat.name, srcTex2d.name);
            
            var dstTex2d = mat.GetTexture(toPropName) as Texture2D;
            if (dstTex2d == null || string.IsNullOrEmpty(dstTex2d.name)) return false;

            var key = CreateKey(maid.Param.status.guid, slotName, mat.name, dstTex2d.name);
            var dstFilter = new FilterParam(srcFilter);
            filterParams.Add(key.ToString(), dstFilter);

            FilterTexture(dstTex2d, dstFilter);
            return true;
        }

        public bool ApplyFilter(Maid maid, string slotName, Material mat, string propName, FilterParam filter) {
            var tex2d = mat.GetTexture(propName) as Texture2D;
            if (tex2d == null || string.IsNullOrEmpty(tex2d.name)) return false;

            var key = CreateKey(maid.Param.status.guid, slotName, mat.name, tex2d.name);
            var filter2 = new FilterParam(filter);
            filterParams.Add(key.ToString(), filter2);

            FilterTexture(tex2d, filter2);
            return true;
        }

        private string GetKey(Maid maid, string slotName, Material material, string propName) {
            if (maid == null || material == null || string.IsNullOrEmpty(propName)) {
                return null;
            }

            var tex2d = material.GetTexture(propName) as Texture2D;
            if (tex2d == null || string.IsNullOrEmpty(tex2d.name)) {
                return null;
            }

            return CreateKey(maid.Param.status.guid, slotName, material.name, tex2d.name).ToString();
        }

        // 途中版
        public static Texture2D convert(RenderTexture rtex) {
            var tex2d = new Texture2D(rtex.width, rtex.height, TextureFormat.ARGB32, false, false);
            var old = RenderTexture.active;
            try {
                RenderTexture.active = rtex;
                tex2d.ReadPixels(new Rect(0, 0, rtex.width, rtex.height), 0, 0);
                tex2d.Apply();
            } finally {
                RenderTexture.active = old;
            }
            return tex2d;
        }

//        private FilterParam GetFilterParam(Material material, TextureEdit texEdit) {
//
//            var tex2d = material.GetTexture(texEdit.propName) as Texture2D;
//            if (tex2d == null || string.IsNullOrEmpty(tex2d.name)) {
//                return null;
//            }
//            var key = new StringBuilder();
//            key.Append(texEdit.slotName).Append('/').Append(material.name).Append('/').Append(tex2d.name);
//            rerturn filterParams.GetOrAdd(key.ToString());
//        }

        private FilterParam GetFilterParam(Maid maid, string slotName, Material material, string propName) {
            var key = GetKey(maid, slotName, material, propName);
            return filterParams.GetOrAdd(key);
        }

        private void FilterTexture(
            IDictionary<string, List<Material>> slotMaterials,
            List<Texture2D> textures, Maid maid, EditTarget texEdit)
        {
            List<Material> materials;
            if (slotMaterials.TryGetValue(texEdit.slotName, out materials)) {
                FilterTexture(materials, textures, maid, texEdit);
            }
        }
        private void FilterTexture(
            ICollection<Material> slotMaterials,
            List<Texture2D> textures, Maid maid, EditTarget texEdit) {
            Material material = null;
            Texture2D texture = null;
            {
                if ( slotMaterials != null ) {
                    material = slotMaterials.ElementAtOrDefault(texEdit.matNo);
                    if (material != null) {
                        texture = material.GetTexture(texEdit.propName) as Texture2D;
                    }
                }
            }
            if (material == null || texture == null) return;

            var filterParam = GetFilterParam(maid, texEdit.slotName, material, texEdit.propName);
            if (!filterParam.Dirty.Value) return;

            FilterTexture(texture, filterParam);
        }
        // キャッシュにテクスチャソースを保持する
        private void FilterTexture(Texture2D texture, FilterParam filter) {
            var orgTex = originalTexCache.GetOrAdd(texture);
            orgTex.dirty  = false;
            filter.ClearDirtyFlag();

            FilterTexture(texture, orgTex.texture, filter);
        }

        public Texture2D ApplyFilter(Texture2D srcTex, FilterParam filter) {
            var dstTex = UnityEngine.Object.Instantiate(srcTex) as Texture2D;
            FilterTexture(dstTex, srcTex, filter);
            return dstTex;
        }

        public void FilterTexture(Texture2D dstTex, Texture2D srcTex, FilterParam filterParam) {
            var outputBase = filterParam.OutputMin * 0.01f;
            var outputScale = (filterParam.OutputMax - filterParam.OutputMin) * 0.01f;

            var inputDelta = filterParam.InputMax - filterParam.InputMin;
            if (inputDelta < 0.001f) inputDelta = 0.01f; // ゼロ除算を避けるため
            float mid = filterParam.InputMid;
            if (mid < 0.001f) mid = 0.01f;               // NegativeInfinityを避けるため
            var inputExp = Mathf.Log(mid * 0.01f) / Mathf.Log(0.5f);
            var inputBase = (-filterParam.InputMin / (inputDelta));
            var inputScale = 1f / (inputDelta * 0.01f);

            var hue        = filterParam.Hue / 360f;
            var saturation = filterParam.Saturation / 100f;
            var lightness  = filterParam.Lightness / 100f;

            Filter(dstTex, srcTex, (color) =>
                   {
                       Color c = color;

                       c.r = Mathf.Clamp01(c.r * inputScale + inputBase);
                       c.g = Mathf.Clamp01(c.g * inputScale + inputBase);
                       c.b = Mathf.Clamp01(c.b * inputScale + inputBase);

                       if (!NumberUtil.Equals(inputExp, 1f)) {
                           c.r = Mathf.Pow(c.r, inputExp);
                           c.g = Mathf.Pow(c.g, inputExp);
                           c.b = Mathf.Pow(c.b, inputExp);
                       }

                       Vector4 hsl = ColorUtil.RGBToHsl(c);
                       hsl.x = (hsl.x + hue) % 1f;
                       hsl.y *= saturation;
                       hsl.z *= lightness;
                       c = ColorUtil.HslToRGB(hsl);

                       c.r = c.r * outputScale + outputBase;
                       c.g = c.g * outputScale + outputBase;
                       c.b = c.b * outputScale + outputBase;
                       return c;
                });
        }

        private void Filter(Texture2D dstTexture, Texture2D srcTexture, Func<Color32, Color32> mapFunc) {
            if (dstTexture == null || srcTexture == null || dstTexture.width != srcTexture.width || dstTexture.height != srcTexture.height) {
                return;
            }
            var maxIndex = dstTexture.width * dstTexture.height;
            var src = srcTexture.GetPixels32(0);
            var dst = dstTexture.GetPixels32(0);
            for (var i = 0; i < maxIndex; i++) {
                dst[i] = mapFunc(src[i]);
            }
            dstTexture.SetPixels32(dst);
            dstTexture.Apply();
        }

        private class FilterParams {
            private readonly Dictionary<string, FilterParam> _params =　new Dictionary<string, FilterParam>();

            public void Clear() {
                _params.Clear();
            }
            public void Add(string key, FilterParam filter) {
                _params[key] = filter;
            }
            public FilterParam Get(string key) {
                FilterParam p;
                return _params.TryGetValue(key, out p) ? p : null;
            }
            public FilterParam GetOrAdd(string key) {
                FilterParam p;
                if (_params.TryGetValue(key, out p)) return p;
                p = new FilterParam();
                _params[key] = p;
                return p;
            }
            public bool Remove(string key) {
                return _params.Remove(key);
            }
        }

        public class FilterParam {
            private static readonly NamedRange HueRange = new NamedRange("色相", 0f, 360f);
            private static readonly NamedRange SaturRange = new NamedRange("彩度", 0f, 200f);
            private static readonly NamedRange LightRange = new NamedRange("明度", 0f, 200f);
            private static readonly NamedRange InpMinRange = new NamedRange("InpMin", 0f, 100f);
            private static readonly NamedRange InpMaxRange = new NamedRange("InpMax", 0f, 100f);
            private static readonly NamedRange InpMidRange = new NamedRange("InpMid", 0f, 100f);
            private static readonly NamedRange OutMinRange = new NamedRange("OutMin", 0f, 100f);
            private static readonly NamedRange OutMaxRange = new NamedRange("OutMax", 0f, 100f);
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

            public FilterParam() {
                Dirty      = new DirtyFlag();
                Hue        = new DirtyValue(Dirty, HueRange,    HueRange.Min);
                Saturation = new DirtyValue(Dirty, SaturRange,  SaturRange.Max*0.5f);
                Lightness  = new DirtyValue(Dirty, LightRange,  LightRange.Max*0.5f);
                InputMin   = new DirtyValue(Dirty, InpMinRange, InpMinRange.Min);
                InputMax   = new DirtyValue(Dirty, InpMaxRange, InpMaxRange.Max);
                InputMid   = new DirtyValue(Dirty, InpMidRange, InpMidRange.Max*0.5f);
                OutputMin  = new DirtyValue(Dirty, OutMinRange, OutMinRange.Min);
                OutputMax  = new DirtyValue(Dirty, OutMaxRange, OutMaxRange.Max);
            }
            public FilterParam(FilterParam filter) {
                Dirty      = new DirtyFlag();
                Hue        = new DirtyValue(Dirty, HueRange,    filter.Hue.Value);
                Saturation = new DirtyValue(Dirty, SaturRange,  filter.Saturation.Value);
                Lightness  = new DirtyValue(Dirty, LightRange,  filter.Lightness.Value);
                InputMin   = new DirtyValue(Dirty, InpMinRange, filter.InputMin.Value);
                InputMax   = new DirtyValue(Dirty, InpMaxRange, filter.InputMax.Value);
                InputMid   = new DirtyValue(Dirty, InpMidRange, filter.InputMid.Value);
                OutputMin  = new DirtyValue(Dirty, OutMinRange, filter.OutputMin.Value);
                OutputMax  = new DirtyValue(Dirty, OutMaxRange, filter.OutputMax.Value);
            }
            public void Clear() {
                Dirty.Value      = false;
                Hue.Value        = HueRange.Min;
                Saturation.Value = SaturRange.Max*0.5f;
                Lightness.Value  = LightRange.Max*0.5f;
                InputMin.Value   = InpMinRange.Min;
                InputMax.Value   = InpMaxRange.Max;
                InputMid.Value   = InpMidRange.Max*0.5f;
                OutputMin.Value  = OutMinRange.Min;
                OutputMax.Value  = OutMaxRange.Max;
            }

            private const float THRESHOLD = 0.01f;
            // 初期値から変更されたかを判定する
            public bool hasNotChanged() {
                return ( Hue.Value       < THRESHOLD &&
                         InputMin.Value  < THRESHOLD &&
                         OutputMin.Value < THRESHOLD &&
                        (InpMaxRange.Max - InputMax.Value)  < THRESHOLD &&
                        (OutMaxRange.Max - OutputMax.Value) < THRESHOLD &&
                         Math.Abs(Lightness.Value - 100f)   < THRESHOLD &&
                         Math.Abs(Saturation.Value - 100f)  < THRESHOLD &&
                         Math.Abs(InputMid.Value - 50f)     < THRESHOLD   );
            }
            public void ClearDirtyFlag() {
                Dirty.Value = false;
            }

            private string message;
            private long endTicks;
            public void ProcGUI(Texture2D tex2d) {
                float margin = uiParams.margin;
                guiSlider(margin, Hue);
                guiSlider(margin, Saturation);
                guiSlider(margin, Lightness);
                if (guiSlider(margin, InputMin)) {
                    if (InputMin.Value > InputMax.Value) InputMax.Value = InputMin.Value;
                    //if (InputMin.Value > InputMid.Value) InputMid.Value = InputMin.Value;
                }
                if (guiSlider(margin, InputMax)) {
                    if (InputMax.Value < InputMin.Value) InputMin.Value = InputMax.Value;
                    //if (InputMax.Value < InputMid.Value) InputMid.Value = InputMax.Value;
                }
                guiSlider(margin, InputMid);
                // if (InputMid.Value > InputMax.Value) InputMax.Value = InputMid.Value;
                // if (InputMid.Value < InputMin.Value) InputMin.Value = InputMid.Value;
                
                if (guiSlider(margin, OutputMin)) {
                    if (OutputMin.Value > OutputMax.Value) OutputMax.Value = OutputMin.Value;
                }
                if (guiSlider(margin, OutputMax)) {
                    if (OutputMin.Value > OutputMax.Value) OutputMin.Value = OutputMax.Value;
                }

                GUILayout.Space(margin);
                GUILayout.BeginHorizontal();
                try {
                    GUILayout.Space(margin * 4f);
                    if (GUILayout.Button("リセット", uiParams.bStyleSC)) {
                        Clear();
                        Dirty.Value = true;
                    }
                    if (GUILayout.Button("png出力", uiParams.bStyleSC)) {
                        try {
                            var bytes = tex2d.EncodeToPNG();
                            var dir = OUT_UTIL.GetExportDirectory();
                            var date = DateTime.Now;
                            var name = tex2d.name + "_" + date.ToString("MMddHHmmss") + ".png";
                            var path = Path.Combine(dir, name);
                            
                            OUT_UTIL.WriteBytes(path, bytes);
                            endTicks = date.Ticks + 10000000L * 10;
                            message = name + "を出力しました";
                            LogUtil.Log("png ファイルを出力しました。file=", path);
                        } catch(Exception e) {
                            var date = DateTime.Now;
                            endTicks = date.Ticks + 10000000L * 10;
                            message = "png出力に失敗しました。";
                            LogUtil.Log(message, e);
                        }
                        //StartCoroutine( DelaySecond(10, () => {message = string.Empty; }) );
                    }
                    GUILayout.Space(margin * 4f);
                } finally {
                    GUILayout.EndHorizontal();
                }
                if (!string.IsNullOrEmpty(message)) {
                    GUILayout.Label(message, uiParams.lStyleS);
                    if (endTicks <= DateTime.Now.Ticks) {
                        message = string.Empty;
                    }
                } else {
                    GUILayout.Space(margin * 2f);
                }
            }

            private bool guiSlider(float margin, DirtyValue dirtyValue) {
                var val = dirtyValue.Value;
                GUILayout.BeginHorizontal();
                try {
                    GUILayout.Label(dirtyValue.Name, uiParams.lStyle, GUILayout.Width(64));
                    GUILayout.Label(val.ToString("F0"), uiParams.lStyle, GUILayout.Width(32));

                    GUILayout.BeginVertical();
                    GUILayout.Space(margin*5f);
                    val = GUILayout.HorizontalSlider(val, dirtyValue.Min, dirtyValue.Max );
                    GUILayout.EndVertical();
                    GUILayout.Space(margin *3);
                } finally {
                    GUILayout.EndHorizontal();
                }

                if (NumberUtil.Equals(dirtyValue.Value, val)) return false;
                dirtyValue.Value = val;
                return true;
            }
        }

        public class DirtyFlag {
            public bool Value;
        }

        public class NamedRange {
            public string Name { get; private set; }
            public float Min { get; private set; }
            public float Max { get; private set; }
            public NamedRange(string name, float min, float max) {
                Name = name;
                Min = min;
                Max = max;
            }
        }
        public class DirtyValue {
            private DirtyFlag dirtyFlag;
            private float val;
            private NamedRange range;
            public string Name { get { return range.Name;} }
            public float Min { get { return range.Min;} }
            public float Max { get { return range.Max;} }

            public float Value {
                get { return val; }
                set {
                    var v = Mathf.Clamp(value, Min, Max);
                    if (val.Equals(v)) return;
                    val = v;
                    dirtyFlag.Value = true;
                }
            }

            public DirtyValue(DirtyFlag dirtyFlag, NamedRange range, float val ) {
                this.range = range;
                this.dirtyFlag = dirtyFlag;
                this.val = val;
            }

            public static implicit operator float (DirtyValue dirtyValue) {
                return dirtyValue.val;
            }
        }

        private class TextureHolder {
            public bool dirty;
            public Texture2D texture;
        }

        private class OriginalTextureCache {
            private readonly Dictionary<string, TextureHolder> _cacheDic = 
                new Dictionary<string, TextureHolder>();

            public void Clear() {
                // テクスチャの開放
                foreach (var texHolder in _cacheDic.Values) {
                    try {
                        UnityEngine.Object.Destroy(texHolder.texture);
                    } catch {
                        // ignored
                    }
                }
            }

            public void Refresh(Texture2D[] maidTextures) {
                // 既に使われなくなったテクスチャを削除
                var nonExistNames = new List<string>();
                foreach (var key in _cacheDic.Keys) {
                    var b = maidTextures.Any(t => t.name == key);
                    if (!b) {
                        nonExistNames.Add(key);
                    }
                }

                foreach (var name in nonExistNames) {
                    TextureHolder texHolder;
                    if (_cacheDic.TryGetValue(name, out texHolder)) {
                        UnityEngine.Object.Destroy(texHolder.texture);
                    }
                    _cacheDic.Remove(name);
                }

                // 知らないテクスチャを追加
                foreach (var t in maidTextures) {
                    if (_cacheDic.ContainsKey(t.name)) continue;
                    var texHolder = new TextureHolder {
                        texture = UnityEngine.Object.Instantiate(t),
                        dirty = false
                    };
                    _cacheDic[t.name] = texHolder;
                }
            }
            public TextureHolder GetOrAdd(Texture2D tex) {
                TextureHolder holder;
                if (_cacheDic.TryGetValue(tex.name, out holder)) return holder;

                holder = new TextureHolder {texture = UnityEngine.Object.Instantiate(tex)};
                _cacheDic[tex.name] = holder;
                return holder;
            }
            /// <summary>テクスチャキャッシュを削除する</summary>
            /// <param name="texName">テクスチャ名</param>
            /// <returns>削除された場合にtrue</returns>
            public bool Remove(string texName) {
                TextureHolder holder;
                if (!_cacheDic.TryGetValue(texName, out holder)) return false;

                UnityEngine.Object.DestroyImmediate(holder.texture);
                return _cacheDic.Remove(texName);
            }
            public void SetDirty(Texture2D texture) {
                if (texture != null) {
                    SetDirty(texture.name);
                }
            }

            public void SetDirty(string name) {
                _cacheDic[name].dirty = true;
            }

            public bool IsDirty(Texture2D texture) {
                return texture != null && IsDirty(texture.name);
            }

            public bool IsDirty(string name) {
                TextureHolder texHolder;
                return _cacheDic.TryGetValue(name, out texHolder) && texHolder.dirty;
            }

            public Texture2D GetOriginalTexture(Texture2D texture) {
                return texture != null ? GetOriginalTexture(texture.name) : null;
            }

            public Texture2D GetOriginalTexture(string name) {
                TextureHolder t;
                return _cacheDic.TryGetValue(name, out t) ? t.texture : null;
            }
        }

        private static class ColorUtil {
            // RGB -> HSL 変換
            public static Vector4 RGBToHsl(Color c) {
                c.r = Mathf.Clamp01(c.r);
                c.g = Mathf.Clamp01(c.g);
                c.b = Mathf.Clamp01(c.b);

                float max = Mathf.Max(c.r, Mathf.Max(c.g, c.b));
                float min = Mathf.Min(c.r, Mathf.Min(c.g, c.b));

                var h = 0f;
                var s = 0f;
                var l = (max + min) / 2f;
                var d = max - min;

                // FIXME float compare
                if (NumberUtil.Equals(d, 0f)) return new Vector4(h, s, l, c.a);

                s = l > 0.5f ? (d / (2f - max - min)) : (d / (max + min));
                if (NumberUtil.Equals(max, c.r)) {
                    h = (c.g - c.b) / d + (c.g < c.b ? 6f : 0f);
                } else if (NumberUtil.Equals(max, c.g)) {
                    h = (c.b - c.r) / d + 2f;
                } else {
                    h = (c.r - c.g) / d + 4f;
                }
                h /= 6f;
                return new Vector4(h, s, l, c.a);
            }

            // HSL -> RGB 変換
            public static Color HslToRGB(Vector4 hsl) {
                Color c;
                c.a = hsl.w;

                var h = hsl.x;
                var s = hsl.y;
                var l = hsl.z;

                if (NumberUtil.Equals(s, 0f)) {
                    c.r = l;
                    c.g = l;
                    c.b = l;
                } else {
                    var y = (l < 0.5f) ? (l * (1f + s)) : ((l + s) - l * s);
                    var x = 2f * l - y;
                    c.r = Hue(x, y, h + 1f / 3f);
                    c.g = Hue(x, y, h);
                    c.b = Hue(x, y, h - 1f / 3f);
                }

                return c;
            }

            private static float Hue(float x, float y, float t) {
                if (t < 0f) {
                    t += 1f;
                } else if (t > 1f) {
                    t -= 1f;
                } 

                if (t < 1f / 6f) return x + (y - x) * 6f * t;
                if (t < 2f / 6f) return y;
                if (t < 4f / 6f) return x + (y - x) * 6f * (4f / 6f - t);
                return x;
            }
        }
    }

    public class EditTarget {
        public string slotName;
        public int matNo;
        public string propName;
        public PropKey propKey;

        public EditTarget() {
            Clear();
        }

        // テクスチャエディット対象を無効にする
        public void Clear() {
            slotName = string.Empty;
            matNo = -1;
            propName = string.Empty;
            propKey = PropKey.Unkown;
        }
        public bool IsValid() {
            return (matNo >= 0 && slotName.Length != 0 && propName.Length != 0);
        }
    }
}
