using System;
using System.Globalization;
using UnityEngine;
using CM3D2.AlwaysColorChangeEx.Plugin.Data;
using CM3D2.AlwaysColorChangeEx.Plugin.Util;
using UnityEngine.Rendering;

namespace CM3D2.AlwaysColorChangeEx.Plugin.UI {
    internal class ACCMaterialsView {
        private const float EPSILON = 0.000001f;

        // ComboBox用アイテムリスト
        private static GUIContent[] shaderNames;
        private static GUIContent[] ShaderNames {
            get {
                if (shaderNames != null) return shaderNames;
//                    CustomShaderHolder.InitShader();
                var length = ShaderType.shaders.Length;
                shaderNames = new GUIContent[length];
                foreach (var shaderType in ShaderType.shaders) {
                    shaderNames[shaderType.idx] = new GUIContent(shaderType.name, shaderType.dispName);
                }
                return shaderNames;
            }
        }
        private static GUIContent[] compareFuncs;
        private static GUIContent[] CompareFuncs {
            get {
                if (compareFuncs == null) {
                    var names = Enum.GetNames(typeof(CompareFunction));
                    compareFuncs = new GUIContent[names.Length];
                    var idx = 0;
                    foreach (var name in names) {
                        compareFuncs[idx++] = new GUIContent(name);
                    }
                }
                return compareFuncs;
            }
        }

        public static void Init(UIParams uiparams) {
            if (uiParams != null) return;

            uiParams = uiparams;
            uiParams.Add(updateUI);
        }

        public static void Clear() {
            //changeShaders.Clear();

            if (uiParams != null) uiParams.Remove(updateUI);
        }
        private static readonly ResourceHolder resHolder = ResourceHolder.Instance;
        private static GUIContent plusIcon;
        private static GUIContent minusIcon;
        private static GUIContent copyIcon;
        private static GUIContent[] pasteIcon;
        private static GUIContent PlusIcon {
            get { return plusIcon ?? (plusIcon = new GUIContent(resHolder.PlusImage)); }
        }
        private static GUIContent MinusIcon {
            get { return minusIcon ?? (minusIcon = new GUIContent(resHolder.MinusImage)); }
        }
        private static GUIContent CopyIcon {
            get { return copyIcon ?? (copyIcon = new GUIContent("コピー", resHolder.CopyImage, "マテリアル情報をクリップボードへコピーする")); }
        }
        private static GUIContent[] PasteIcon {
            get {
                return pasteIcon ?? (pasteIcon = new[] {
                    new GUIContent("全貼付", resHolder.PasteImage, "クリップボードからマテリアル情報を貼付ける"),
                    new GUIContent("指定貼付", resHolder.PasteImage, "クリップボードからマテリアル情報を貼付ける"),
                });
            }
        }
        private static UIParams uiParams;
        //private readonly GUIStyle lStyleC   = new GUIStyle("label");
        //private readonly GUIStyle hsldStyle = new GUIStyle("horizontalSlider");
        private static readonly GUIStyle bStyleLeft    = new GUIStyle("label");
        private static readonly GUIStyle bStyleRight   = new GUIStyle("label");
        private static readonly GUIStyle bStyleSS      = new GUIStyle("button");
        private static GUILayoutOption optItemHeight;
        private static GUILayoutOption optUnitHeight;
        private static GUILayoutOption optInputWidth;
        private static GUILayoutOption optButonWidthS;
        private static GUILayoutOption optButonWidth;
        private static GUILayoutOption optIconWidth;
        private static float labelWidth;
        private static float sliderInputWidth;
        private static float sliderMargin;           // スライダーのy軸マージン
        private static float buttonMargin;
        private static Color txtColor;
        private static Color txtColorRed = Color.red;
        private static GUILayoutOption bWidthOpt;
        private static float baseWidth;
        private static GUILayoutOption bWidthWOpt;
        private static Texture2D Copy(Texture2D src) {
            var dst = new Texture2D(src.width, src.height);
            Color32[] pixels = src.GetPixels32();
            for (int i = 0; i< pixels.Length; i++) {
                pixels[i].r = (byte)(pixels[i].r/2);
                pixels[i].g = (byte)(pixels[i].g/2);
                pixels[i].b = (byte)(pixels[i].b/2);
                pixels[i].a = (byte)(pixels[i].a/2);
            }
            dst.SetPixels32(pixels);
            dst.Apply();
            return dst;
        }
        private static readonly Action<UIParams> updateUI = (uiparams) => {
            // 幅の28%
            labelWidth    = uiparams.colorRect.width*0.28f;
            sliderMargin  = uiparams.margin*4.5f; // GUILayout.Space(uiParams.FixPx(7));

            buttonMargin  = uiparams.margin*3f;
            sliderInputWidth = uiparams.fontSizeS *0.5625f * 8; // 最大8文字分としてフォントサイズの比率

            optIconWidth  = GUILayout.Width(16);
            optButonWidth = GUILayout.Width((uiparams.textureRect.width-20)*0.23f);
            optButonWidthS = GUILayout.Width((uiparams.textureRect.width-20)*0.20f);
            optInputWidth = GUILayout.Width(sliderInputWidth);
            optItemHeight = GUILayout.Height(uiparams.itemHeight);
            optUnitHeight = GUILayout.Height(uiparams.unitHeight);

            txtColor = uiparams.textStyleSC.normal.textColor;

            bStyleLeft.fontStyle = uiparams.lStyleC.fontStyle;
            bStyleLeft.fontSize  = uiparams.fontSize;
            bStyleLeft.normal.textColor = uiparams.lStyleC.normal.textColor;
            bStyleLeft.alignment = TextAnchor.MiddleLeft;

            bStyleRight.fontStyle = uiparams.lStyleC.fontStyle;
            bStyleRight.fontSize  = uiparams.fontSize;
            bStyleRight.normal.textColor = uiparams.lStyleC.normal.textColor;
            bStyleRight.alignment = TextAnchor.MiddleRight;

            baseWidth = (uiparams.textureRect.width-20)*0.06f;
            bWidthOpt  = GUILayout.Width(baseWidth);
            bWidthWOpt = GUILayout.Width(baseWidth*2);

            bStyleSS.normal.textColor = uiparams.bStyleSC.normal.textColor;
            bStyleSS.alignment = TextAnchor.MiddleCenter;

            bStyleSS.fontSize = uiparams.fontSizeSS;
        };

        private static bool includeTex;
        private static bool includeShader = true;
        private static bool includeOthers = true;
        private static RQResolver rqResolver = RQResolver.Instance;
        //public ACCMaterial original;
        private ClipBoardHandler clipHandler = ClipBoardHandler.Instance;
        public ACCMaterial edited;
        public ComboBoxLO shaderCombo;
        public ComboBoxLO compareCombo;
        public bool expand;
        private int matIdx;
        private int slotIdx;
        public ACCMaterialsView(Renderer r, Material m, int slotIdx, int idx) {
            //original = new ACCMaterial(m, r);
            //edited = new ACCMaterial(original);
            this.slotIdx = slotIdx;
            matIdx = idx;
            edited = new ACCMaterial(m, r);
        }
        public Action<string> tipsCall;
        
        public void Show(bool reload) {
            GUILayout.BeginVertical();
            try {
                GUILayout.BeginHorizontal();
                try {
                    var texIcon = expand? MinusIcon : PlusIcon;
                    if (GUILayout.Button(texIcon, bStyleLeft, optUnitHeight, optIconWidth)) {
                        expand = !expand;
                    }
                    if (GUILayout.Button(edited.name, bStyleLeft, optUnitHeight)) {
                        expand = !expand;
                    }
                    if (!expand) return;

                } finally {
                    GUILayout.EndHorizontal();
                }
                if (edited.type == ShaderType.UNKNOWN) {
                    GUILayout.Label("shader:" + edited.material.shader.name);
                    return;
                }

                GUILayout.BeginHorizontal();
                try {
                    // コピー
                    if (GUILayout.Button(CopyIcon, optUnitHeight,optButonWidthS)) {
                        clipHandler.SetClipboard ( MateHandler.Instance.ToText(edited) );
                        if (tipsCall != null) {
                            tipsCall("マテリアル情報をクリップボードに\nコピーしました");
                        }
                    }

                    GUI.enabled &= clipHandler.isMateText;
                    var icons = PasteIcon;
                    if (GUILayout.Button(icons[0], optUnitHeight,optButonWidthS)) {
                        try {
                            MateHandler.Instance.Write(edited, clipHandler.mateText);
                            if (tipsCall != null) {
                                tipsCall("マテリアル情報を貼付けました");
                            }
                        } catch(Exception e) {
                            LogUtil.Error("failed to import mateText", e);
                        }
                    }
                    includeOthers = GUILayout.Toggle(includeOthers, "CF", uiParams.tStyleSS);
                    includeShader = GUILayout.Toggle(includeShader, "S", uiParams.tStyleSS);
                    includeTex    = GUILayout.Toggle(includeTex, "T", uiParams.tStyleSS);
                    GUI.enabled &= (includeTex |includeShader| includeOthers);
                    if (GUILayout.Button(icons[1], optUnitHeight,optButonWidth)) {
                        try {
                            var pasteFlag = 0;
                            if (includeTex)    pasteFlag |= MateHandler.MATE_TEX;
                            if (includeShader) pasteFlag |= MateHandler.MATE_SHADER;
                            if (includeOthers) pasteFlag |= MateHandler.MATE_COLOR | MateHandler.MATE_FLOAT;
                            LogUtil.DebugF("material pasting from cp... tex={0}, shader={1}, others={2}", includeTex, includeShader, includeOthers);
                            MateHandler.Instance.Write(edited, clipHandler.mateText, pasteFlag);
                        } catch(Exception e) {
                            LogUtil.Error("failed to import mateText", e);
                        }
                        if (tipsCall != null) {
                            tipsCall("マテリアル情報を貼付けました");
                        }
                    }
                } finally {
                    GUI.enabled = true;
                    GUILayout.EndHorizontal();
                }

                var material = edited.material;
                var idx = edited.type.idx;

                if (shaderCombo == null) {
                    var selected = (idx >= 0 && idx < ShaderNames.Length) ? ShaderNames[idx] : GUIContent.none;
                    shaderCombo = new ComboBoxLO(selected, ShaderNames, uiParams.bStyleSC, uiParams.boxStyle, uiParams.listStyle, false);
                } else {
                    shaderCombo.SelectedItemIndex = idx;
                }
                shaderCombo.Show(GUILayout.ExpandWidth(true));//uiParams.optInsideWidth);

                var selectedIdx = shaderCombo.SelectedItemIndex;
                if (idx != selectedIdx && selectedIdx != -1) {
                    LogUtil.Debug("shader changed", idx, "=>", selectedIdx );
                    // シェーダ変更
                    var shaderName0 = ShaderNames[selectedIdx].text;
                    edited.ChangeShader(shaderName0, selectedIdx);
                }
                // MaterialType mat = edited.type;
                if (reload) {
                    edited.renderQueue.Set( material.renderQueue );
                }

                SetupFloatSlider("RQ", edited.renderQueue,
                                 edited.renderQueue.range.editMin, edited.renderQueue.range.editMax,
                                 (rq) => {
                                     material.SetFloat(ShaderPropType.RenderQueue.propId, rq);
                                     material.renderQueue = (int)rq;
                                 }, 
                                 ShaderPropType.RenderQueue.opts,
                                 ShaderPropType.RenderQueue.presetVals,
                                 rqResolver.Resolve(slotIdx));


                var sdType = edited.type;
                for (var i=0; i< sdType.colProps.Length; i++) {
                    var colProp = sdType.colProps[i];
                    var editColor = edited.editColors[i];
                    if (reload) {
                        editColor.Set(material.GetColor(colProp.propId));
//                    } else {
//                        if (!editColor.val.HasValue) {
//                            editColor.Set(colProp.defaultVal);
//                            LogUtil.DebugF("value is empty. set white. color={0}, vals={1}, syncs={2}",
//                                editColor.val, editColor.editVals, editColor.isSyncs);
//                        }
                    }
                        
                    var beforeColor = editColor.val;
                    setColorSlider(colProp.name, ref editColor, colProp.colorType);
                    if (editColor.val != beforeColor) {
                        material.SetColor(colProp.propId, editColor.val);
                    }
                }

                for (var i=0; i< sdType.fProps.Length; i++) {
                    var prop = sdType.fProps[i];
                    if (reload) edited.editVals[i].Set(material.GetFloat(prop.propId));

                    switch (prop.valType) {
                        case ValType.Float:
                            // slider
                            var fprop = prop;
                            // fprop.SetValue(mat, val);
                            SetupFloatSlider(prop.name, edited.editVals[i],
                                             fprop.sliderMin, fprop.sliderMax, 
                                             (val) => fprop.SetValue(material, val),
                                              fprop.opts, null, fprop.presetVals);
                            break;
                        case ValType.Bool:
                            SetupCheckBox(prop.name, edited.editVals[i],
                                (val) => prop.SetValue(material, val));
                            break;
                        case ValType.Enum:
                            SetupComboBox(prop.name, edited.editVals[i],
                                (val) => prop.SetValue(material, val));
                            break;
                    }
                }

            } finally {
                GUILayout.EndVertical();
            }
        }

        private void SetupComboBox(string label, EditValue edit, Action<float> func) {
            GUILayout.BeginHorizontal();
            try {
                GUILayout.Label(label, uiParams.lStyle, optItemHeight);
                
                GUILayout.Space(uiParams.marginL);
                var idx = (int)edit.val;
                if (compareCombo == null) {
                    var selected = (idx >= 0 && idx < CompareFuncs.Length) ? CompareFuncs[idx] : GUIContent.none;
                    compareCombo = new ComboBoxLO(selected, CompareFuncs, uiParams.bStyleSC, uiParams.boxStyle, uiParams.listStyle, false);
                } else {
                    compareCombo.SelectedItemIndex = idx;
                }
                compareCombo.Show(GUILayout.ExpandWidth(true));//uiParams.optInsideWidth);

                var selectedIdx = compareCombo.SelectedItemIndex;
                if (idx == selectedIdx || selectedIdx == -1) return;

                edit.Set(selectedIdx);
                func(selectedIdx);

            } finally {
                GUILayout.EndHorizontal();
            }

        }

        private void SetupCheckBox(string label, EditValue edit, Action<float> func) {
            GUILayout.BeginHorizontal();
            try {
                GUILayout.Label(label, uiParams.lStyle, optItemHeight);
                
                GUILayout.Space(uiParams.marginL);
                var val = edit.val;
                var cont = NumberUtil.Equals(val, 0f) ? ResourceHolder.Instance.Checkoff : ResourceHolder.Instance.Checkon;
                if (!GUILayout.Button(cont, bStyleRight, GUILayout.Width(50))) return;

                val = 1 - val;
                edit.Set(val);
                func(val);
            } finally {
                GUILayout.EndHorizontal();
            }

        }
            
        private void SetupFloatSlider(string label, EditValue edit, float sliderMin, float sliderMax,
                                      Action<float> func, float[] vals1, float[] vals2) {
            SetupFloatSlider(label, edit, sliderMin, sliderMax, func, null, vals1, vals2);
        }

        
        private void SetupFloatSlider(string label, EditValue edit, float sliderMin, float sliderMax,
                                      Action<float> func, PresetOperation[] presetOprs, float[] vals1, float[] vals2) {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, uiParams.lStyle, optItemHeight);
            GUILayout.Space(uiParams.marginL);

            var changed = false;

            Action<float> preset = (val) =>  {
                var blabel = val.ToString(CultureInfo.InvariantCulture);
                GUILayoutOption opt;
                if (blabel.Length <= 1) opt = bWidthOpt;
                else if (blabel.Length <= 3) opt = bWidthWOpt;
                else opt = GUILayout.Width(baseWidth*0.5f*(blabel.Length+1));
                if (!GUILayout.Button(blabel, bStyleSS, opt)) return;

                edit.Set( val );
                changed = true;
            };

            if (vals1 != null) foreach (var val in vals1) { preset(val); }
            if (vals2 != null) foreach (var val in vals2) { preset(val); }
            
            if (presetOprs != null) {
                foreach (var pset in presetOprs) {
                    var widthOpt = (pset.label.Length == 1) ? bWidthOpt : bWidthWOpt;
                    if (!GUILayout.Button(pset.label, bStyleSS, widthOpt)) continue;

                    edit.SetWithCheck(pset.func(edit.val));
                    changed = true;
                }
            }
            GUILayout.EndHorizontal();

            if (changed || DrawValueSlider(null, edit, sliderMin, sliderMax)) {
                func(edit.val);
            }
        }

        private static readonly float delta = 0.1f;
        private void setColorSlider(string label, ref EditColor edit, ColorType colType) {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, uiParams.lStyle, optItemHeight);

            var c = edit.val;
            var changed = false;
            float[] vals = {0, 0.5f, 1f, 1.5f, 2f};
            foreach (var val in vals) {
                var blabel = val.ToString(CultureInfo.InvariantCulture);
                if (!GUILayout.Button(blabel, bStyleSS, (blabel.Length > 1) ? bWidthWOpt : bWidthOpt)) continue;
                c.r = c.g = c.b = val;
                changed = true;
            }
            if (GUILayout.Button("-", bStyleSS, bWidthOpt)) {
                if (c.r < delta) c.r = 0;
                else c.r -= delta;
                if (c.g < delta) c.g = 0;
                else c.g -= delta;
                if (c.b < delta) c.b = 0;
                else c.b -= delta;

                changed = true;
            }
            if (GUILayout.Button("+", bStyleSS, bWidthOpt)) {
                if (c.r + delta > 2.0f) c.r = 2;
                else c.r += delta;
                if (c.g + delta > 2.0f) c.g = 2;
                else c.g += delta;
                if (c.b + delta > 2.0f) c.b = 2;
                else c.b += delta;
                changed = true;
            }
            GUILayout.EndHorizontal();
 
            var idx = 0;
            if (colType == ColorType.rgb || colType == ColorType.rgba) {
                changed |= DrawValueSlider("R", ref edit, idx++, ref c.r);
                changed |= DrawValueSlider("G", ref edit, idx++, ref c.g);
                changed |= DrawValueSlider("B", ref edit, idx++, ref c.b);
            }
            if (colType == ColorType.rgba || colType == ColorType.a) {
                changed |= DrawValueSlider("A", ref edit, idx, ref c.a);
            }
            if (changed) {
                edit.Set(c);
            }
        }

        private bool DrawValueSlider(string label, EditValue edit, float sliderMin, float sliderMax) {
            var changed = false;
            var fontChanged = false;
            GUILayout.BeginHorizontal(optItemHeight);
            try {
                DrawLabel(ref label);

                if (!edit.isSync) {
                    SetTextColor(uiParams.textStyleSC, ref txtColorRed);
                    fontChanged = true;
                }

                var editedVal = GUILayout.TextField(edit.editVal, uiParams.textStyleSC, optInputWidth);
                if (edit.editVal != editedVal) { // 直接書き換えられたケース
                    edit.Set(editedVal);
                    changed |= edit.isSync; // テキスト値書き換え、かつ値が同期⇒変更とみなす
                }

                var sliderVal = edit.val;
                if (DrawSlider(ref sliderVal, sliderMin, sliderMax)) {
                    edit.Set(sliderVal);
                    changed = true;
                }
                GUILayout.Space(buttonMargin);

            } finally {
                GUILayout.EndHorizontal();
                if (fontChanged) {
                    SetTextColor(uiParams.textStyleSC, ref txtColor);
                }
            }
            return changed;
        }

        private void SetTextColor(GUIStyle style, ref Color c) {
            style.normal.textColor = c;
            style.focused.textColor = c;
            style.active.textColor = c;
            style.hover.textColor = c;
        }

        private bool DrawValueSlider(string label, ref EditColor edit, int idx, ref float sliderVal) {
            var changed = false;
            var fontChanged = false;
            GUILayout.BeginHorizontal(optItemHeight);
            try {
                DrawLabel(ref label);

                if (!edit.isSyncs[idx]) {
                    SetTextColor(uiParams.textStyleSC, ref txtColorRed);
                    fontChanged = true;
                }

                var range = edit.GetRange(idx);
                var val2 = GUILayout.TextField(edit.editVals[idx], uiParams.textStyleSC, optInputWidth);
                if (edit.editVals[idx] != val2) { // 直接書き換えられたケース
                    edit.Set(idx, val2, range);
                }

                changed |= DrawSlider(ref sliderVal, range.editMin, range.editMax);
                GUILayout.Space(buttonMargin);

            } catch(Exception e) {
                LogUtil.DebugF("{0}, idx={1}, color={2}, vals.length={3}, syncs.length={4}, e={5}",
                               label, idx, edit.val, edit.editVals.Length, edit.isSyncs.Length, e);
                throw;
            } finally {
                GUILayout.EndHorizontal();
                if (fontChanged) {
                    SetTextColor(uiParams.textStyleSC, ref txtColor);
                }
            }
            return changed;
        }

        private void DrawLabel(ref string label) {
            if(label != null) {
                //float lWidth = 13*label.Length;
                var lWidth = uiParams.fontSizeS*label.Length;
                var space = labelWidth - sliderInputWidth - lWidth;  //- uiParams.labelSpace ;
                if (space > 0) GUILayout.Space(space);
                GUILayout.Label(label, uiParams.lStyleS, GUILayout.Width(lWidth));
            } else {
                GUILayout.Space(labelWidth - sliderInputWidth);
            }
        }
        private bool DrawSlider(ref float sliderVal, float min, float max) {
            var changed = false;
            GUILayout.BeginVertical();
            try {
                GUILayout.Space(sliderMargin);
                var opt = GUILayout.ExpandWidth(true);//GUILayout.Width(uiParams.colorRect.width * 0.65f);
                var slidVal = GUILayout.HorizontalSlider(sliderVal, min, max, opt);
                if (!NumberUtil.Equals(slidVal, sliderVal, EPSILON)) { // スライダー変更時のみ
                    if (sliderVal > max || sliderVal < min) {
                        // スライダーの範囲外の場合：スライダーを移動したケースを検知
                        if (slidVal < max && slidVal > min) {
                            sliderVal = slidVal;
                            changed = true;
                        }
                    } else {
                        sliderVal = slidVal;
                        changed = true;
                    }
                }
            } finally {
                GUILayout.EndVertical();
            }
            return changed;
        }
    }
}
