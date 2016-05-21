using System;
using System.Collections.Generic;
using System.Windows.Forms.VisualStyles;
using UnityEngine;
using CM3D2.AlwaysColorChangeEx.Plugin.Data;
using CM3D2.AlwaysColorChangeEx.Plugin.Util;

namespace CM3D2.AlwaysColorChangeEx.Plugin.UI
{
    internal class ACCMaterialsView {
        private const float EPSILON = 0.000001f;
        //private static Dictionary<int, Shader> changeShaders = new Dictionary<int, Shader>();
        private static Settings settings = Settings.Instance;

        // ComboBox用アイテムリスト
        private static GUIContent[] shaderNames;
        private static GUIContent[] ShaderNames {
            get {
                if (shaderNames == null) {
                    shaderNames = new GUIContent[ShaderMapper.ShaderNames.Length];
                    int idx = 0;
                    foreach (ShaderName shaderName in ShaderMapper.ShaderNames) {
                        shaderNames[idx++] = new GUIContent(shaderName.Name, shaderName.DisplayName);
                    }
                }
                return shaderNames;
            }
        }

//        private static int GetIndex(string shaderName) {
//            ShaderName[] names = ShaderMapper.ShaderNames;
//            for (int i=0; i< names.Length; i++) {
//                if (names[i].Name == shaderName) {
//                    return i;
//                }
//            }
//            return -1;
//        }
        public static void Init(UIParams uiparams) {
            if (uiParams == null) {
                uiParams = uiparams;
                uiParams.Add(updateUI);
            }
        }
        public static void Clear() {
            
            //changeShaders.Clear();

            if (uiParams != null) uiParams.Remove(updateUI);
        }
        private static UIParams uiParams;
        //private readonly GUIStyle lStyleC   = new GUIStyle("label");
        //private readonly GUIStyle hsldStyle = new GUIStyle("horizontalSlider");
        private static GUIStyle bStyleLeft    = new GUIStyle("label");
        private static GUIStyle bStyleSS      = new GUIStyle("button");
        private static GUILayoutOption optItemHeight;
        private static GUILayoutOption optUnitHeight;
        private static GUILayoutOption optInputWidth;
        private static float labelWidth;
        private static float sliderInputWidth;
        private static float sliderMargin;           // スライダーのy軸マージン
        private static float buttonMargin;
        private static Color txtColor;
        private static Color txtColorRed = Color.red;
        private static GUILayoutOption bWidthOpt;
        private static float baseWidth;
        private static GUILayoutOption bWidthWOpt;
        private static Texture2D copy(Texture2D src) {
            var dst = new Texture2D(src.width, src.height);
            Color32[] pixels = src.GetPixels32();
            for (int i = 0; i< pixels.Length; i++) {
                pixels[i].r = (byte)((int)pixels[i].r/2);
                pixels[i].g = (byte)((int)pixels[i].g/2);
                pixels[i].b = (byte)((int)pixels[i].b/2);
                pixels[i].a = (byte)((int)pixels[i].a/2);
            }
            dst.SetPixels32(pixels);
            dst.Apply();
            return dst;
        }
        private static Action<UIParams> updateUI = (uiparams) => {
            // 幅の28%
            labelWidth    = uiparams.colorRect.width*0.28f;
            sliderMargin  = uiparams.margin*4.5f; // GUILayout.Space(uiParams.FixPx(7));

            buttonMargin  = uiparams.margin*3f;
            sliderInputWidth = uiparams.fontSizeS *0.5625f * 8; // 最大8文字分としてフォントサイズの比率

            optInputWidth = GUILayout.Width(sliderInputWidth);
            optItemHeight = GUILayout.Height(uiparams.itemHeight);
            optUnitHeight = GUILayout.Height(uiparams.unitHeight);

            txtColor = uiparams.textStyleSC.normal.textColor;

            bStyleLeft.fontStyle = uiparams.lStyleC.fontStyle;
            bStyleLeft.fontSize  = uiparams.fontSize;
            bStyleLeft.normal.textColor = uiparams.lStyleC.normal.textColor;
            bStyleLeft.alignment = TextAnchor.MiddleLeft;

            baseWidth = (uiparams.textureRect.width-20)*0.06f;
            bWidthOpt  = GUILayout.Width(baseWidth);
            bWidthWOpt = GUILayout.Width(baseWidth*2);

            bStyleSS.normal.textColor = uiparams.bStyleSC.normal.textColor;
            bStyleSS.alignment = TextAnchor.MiddleCenter;

            bStyleSS.fontSize = uiparams.fontSizeSS;
        };

        public ACCMaterial original;
        public ACCMaterial edited;
        public ComboBoxLO shaderCombo;
        public bool expand;

        public ACCMaterialsView(Material m) {
            original = new ACCMaterial(m);
            edited = new ACCMaterial(original);
        }

        private void ChangeShader(string shaderName, ref Material material) {
            Shader shader = Shader.Find(shaderName);
            if (shader != null) {
                material.shader = shader;
                var mat = ShaderMapper.resolve(shaderName);
                edited.Update(mat);
                LogUtil.Debug("selected shader updated");
            }
        }

        private readonly bool[] FLAG_RATIO = {true, true, false};
        private readonly bool[] FLAG_INV  = {false, false, true};
        public void Show() {
            GUILayout.BeginVertical();
            try {
                string matName = (expand? "- " : "+ ") + edited.name;
                if (GUILayout.Button(matName, bStyleLeft, optUnitHeight)) {
                    expand = !expand;
                }
                if (!expand) return;

                Material material = edited.material;                          
                //GUILayout.Label(edited.name, uiParams.lStyleC, optItemHeight);
    
                string shaderName = edited.shader.Name;
                int idx = ShaderMapper.getTypeIndex(shaderName);
    
                GUIContent selected = (idx != -1)? ShaderNames[idx] : ShaderNames[4];
                shaderCombo = shaderCombo ?? new ComboBoxLO(selected, ShaderNames, uiParams.bStyleSC, uiParams.boxStyle, uiParams.listStyle, false);
                shaderCombo.Show(GUILayout.ExpandWidth(true));//uiParams.optInsideWidth);
    
                int selectedIdx = shaderCombo.SelectedItemIndex;
                if (idx != selectedIdx) {
                    LogUtil.Debug("selected shader changed", idx, "=>", selectedIdx);
                    // シェーダ変更
                    var shaderName0 = ShaderNames[selectedIdx].text;
                    ChangeShader(shaderName0, ref material);
                }
                MaterialType mat = edited.type;
    
                SetupFloatSlider("RQ", ref edited.renderQueue,
                                 edited.renderQueue.range.editMin, edited.renderQueue.range.editMax,
                                 (rq) => {
                                     material.SetFloat("_SetManualRenderQueue", rq);
                                     material.renderQueue = (int)rq;
                                 }, 3000, 3100, 3200, 3300);

                Action<string, string, EditColor> ColorSlider = (label, key, edit) => {
                    // TODO シェーダ変更などで未設定のプロパティにデフォルトカラーを設定
                    if (!edit.val.HasValue) {
                        edit.Set( Color.white );
                        LogUtil.DebugF("value is empty. set white. color={0}, vals={1}, syncs={2}",
                               edit.val, edit.editVals, edit.isSyncs);
                    }

                    Color beforeColor = edit.val.Value;
                    setColorSlider(label, ref edit, mat.isTrans);
                    if (edit.val.Value != beforeColor) {
                        material.SetColor(key, edit.val.Value);
                    }
                };

                if (mat.hasColor)   ColorSlider("Color", "_Color", edited.color);
                if (mat.isLighted)  ColorSlider("Shadow Color", "_ShadowColor", edited.shadowColor);
                if (mat.isOutlined) ColorSlider("Outline Color", "_OutlineColor", edited.outlineColor);
                if (mat.isToony)    ColorSlider("Rim Color", "_RimColor", edited.rimColor);
    
                if (mat.isLighted) {
                    SetupFloatSlider("Shininess", ref edited.shininess, 
                                     settings.shininessMin, settings.shininessMax,
                                    (val) => material.SetFloat("_Shininess", val), FLAG_RATIO,  0, 0.1f, 0.5f, 1, 5);
                }
                if (mat.isOutlined) {
                    SetupFloatSlider("OutLineWidth", ref edited.outlineWidth,
                                     settings.outlineWidthMin, settings.outlineWidthMax,
                                     (val) => material.SetFloat("_OutlineWidth", val), null, 0.0001f, 0.001f, 0.002f);
                }
                if (mat.isToony) {
                    SetupFloatSlider("RimPower", ref edited.rimPower, 
                                    settings.rimPowerMin, settings.rimPowerMax,
                                    (val) => material.SetFloat("_RimPower", val), FLAG_INV, 0, 25f, 50f, 100f);

                    SetupFloatSlider("RimShift", ref edited.rimShift,
                                    settings.rimShiftMin, settings.rimShiftMax,
                                    (val) => material.SetFloat("_RimShift", val), FLAG_RATIO, 0f, 0.25f, 0.5f, 1f);
                }
                if (mat.isHair) {
                    SetupFloatSlider("HiRate", ref edited.hiRate,
                                     settings.hiRateMin, settings.hiRateMax,
                                     (val) => material.SetFloat("_HiRate", val), FLAG_RATIO, 0f, 0.5f, 1.0f);

                    SetupFloatSlider("HiPow", ref edited.hiPow,
                                     settings.hiPowMin, settings.hiPowMax,
                                     (val) => material.SetFloat("_HiPow", val), FLAG_RATIO, 0.001f, 1f, 50f);
                }
                if (mat.hasFloat1) {
                    SetupFloatSlider("FloatValue1", ref edited.floatVal1,
                                     settings.floatVal1Min, settings.floatVal1Max,
                                     (val) => material.SetFloat("_FloatValue1", val), 0, 100f, 200f);
                }
                if (mat.hasFloat2) {
                    SetupFloatSlider("FloatValue2", ref edited.floatVal2,
                                     settings.floatVal2Min, settings.floatVal2Max,
                                     (val) => material.SetFloat("_FloatValue2", val), -15, 0, 1, 15);
                }
                if (mat.hasFloat3) {
                    SetupFloatSlider("FloatValue3", ref edited.floatVal3,
                                     settings.floatVal3Min, settings.floatVal3Max,
                                     (val) => material.SetFloat("_FloatValue3", val), FLAG_RATIO, 0, 0.5f, 1f);
                }
                if (mat.hasCutoff) {
                    SetupFloatSlider("Cutoff", ref edited.cutoff,
                                     0f, 100f,
                                     (val) => material.SetFloat("_Cutoff", val));
                }
            } finally {
                GUILayout.EndVertical();
            }
        }
        private void SetupFloatSlider(string label, ref EditValue edit, float sliderMin, float sliderMax,
                                      Action<float> func, params float[] vals) {
            SetupFloatSlider(label, ref edit, sliderMin, sliderMax, func, null, vals);
        }

        private void SetupFloatSlider(string label, ref EditValue edit, float sliderMin, float sliderMax,
                                      Action<float> func, bool[] mulVals, params float[] vals) {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, uiParams.lStyle, optItemHeight);
            GUILayout.Space(uiParams.marginL);

            bool changed = false;
            for (int i=0; i< vals.Length; i++) {
                string blabel = vals[i].ToString();
                GUILayoutOption opt;
                if (blabel.Length <= 1) opt = bWidthOpt;
                else if (blabel.Length <= 3) opt = bWidthWOpt;
                else opt = GUILayout.Width(baseWidth*0.5f*(blabel.Length+1));
                if (GUILayout.Button(blabel, bStyleSS, opt)) {
                    edit.Set( vals[i] );
                    changed = true;
                }
            }
            if (mulVals != null && mulVals.Length >= 3) {
                if (mulVals[0]) {
                    if (GUILayout.Button("<", bStyleSS, bWidthOpt)) {
                        edit.SetWithCheck(edit.val * 0.9f);
                        changed = true;
                    }
                }
                if (mulVals[1]) {
                    if (GUILayout.Button(">", bStyleSS, bWidthOpt)) {
                        edit.SetWithCheck(edit.val * 1.1f);
                        changed = true;
                    }
                }
                if (mulVals[2]) {
                    if (GUILayout.Button("*-1", bStyleSS, bWidthWOpt)) {
                        edit.Set(edit.val * -1f);
                        changed = true;
                    }
                }
            }
            GUILayout.EndHorizontal();

            if (changed || drawValueSlider(null, ref edit, sliderMin, sliderMax)) {
                func(edit.val);
            }
        }

        private static readonly float delta = 0.1f;
        private void setColorSlider(string label, ref EditColor edit, bool isTrans) {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, uiParams.lStyle, optItemHeight);


            Color c = edit.val.Value;
            bool changed = false;
            float[] vals = {0, 0.5f, 1f, 1.5f, 2f};
            foreach (var val in vals) {
                string blabel = val.ToString();
                if (GUILayout.Button(blabel, bStyleSS,  (blabel.Length> 1)? bWidthWOpt : bWidthOpt)) {
                    c.r = c.g = c.b = val;
                    changed = true;
                }
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
 
            changed |= drawValueSlider("R", ref edit, 0, ref c.r);
            changed |= drawValueSlider("G", ref edit, 1, ref c.g);
            changed |= drawValueSlider("B", ref edit, 2, ref c.b);
            if (edit.hasAlpha && isTrans) {
                changed |= drawValueSlider("A", ref edit, 3, ref c.a);
            }
            if (changed) {
                edit.Set(c);
            }
        }

        private bool drawValueSlider(string label, ref EditValue edit, float sliderMin, float sliderMax) {
            bool changed = false;
            bool fontChanged = false;
            GUILayout.BeginHorizontal(optItemHeight);
            try {
                drawLabel(ref label);

                if (!edit.isSync) {
                    SetTextColor(uiParams.textStyleSC, ref txtColorRed);
                    fontChanged = true;
                }

                var editedVal = GUILayout.TextField(edit.editVal, uiParams.textStyleSC, optInputWidth);
                if (edit.editVal != editedVal) { // 直接書き換えられたケース
                    edit.Set(editedVal);
                    changed |= edit.isSync; // テキスト値書き換え、かつ値が同期⇒変更とみなす
                }

                float sliderVal = edit.val;
                if (drawSlider(ref sliderVal, sliderMin, sliderMax)) {
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

        private bool drawValueSlider(string label, ref EditColor edit, int idx, ref float sliderVal) {
            bool changed = false;
            bool fontChanged = false;
            GUILayout.BeginHorizontal(optItemHeight);
            try {
                drawLabel(ref label);

                if (!edit.isSyncs[idx]) {
                    SetTextColor(uiParams.textStyleSC, ref txtColorRed);
                    fontChanged = true;
                }

                EditRange range = EditColor.GetRange(idx);
                var val2 = GUILayout.TextField(edit.editVals[idx], uiParams.textStyleSC, optInputWidth);
                if (edit.editVals[idx] != val2) { // 直接書き換えられたケース
                    edit.Set(idx, val2, range);
                }

                changed |= drawSlider(ref sliderVal, range.editMin, range.editMax);
                GUILayout.Space(buttonMargin);

            } catch(Exception e) {
                LogUtil.DebugF("{0}, idx={1}, color={2}, vals={3}, syncs={4}, e={5}",
                               label, idx, edit.val, edit.editVals, edit.isSyncs, e);
                throw;
            } finally {
                GUILayout.EndHorizontal();
                if (fontChanged) {
                    SetTextColor(uiParams.textStyleSC, ref txtColor);
                }
            }
            return changed;
        }
        private void drawLabel(ref string label) {
            if(label != null) {
                //float lWidth = 13*label.Length;
                float lWidth = uiParams.fontSizeS*label.Length;
                float space = labelWidth - sliderInputWidth - lWidth;  //- uiParams.labelSpace ;
                if (space > 0) GUILayout.Space(space);
                GUILayout.Label(label, uiParams.lStyleS, GUILayout.Width(lWidth));
            } else {
                GUILayout.Space(labelWidth - sliderInputWidth);
            }
        }
        private bool drawSlider(ref float sliderVal, float min, float max) {
            bool changed = false;
            GUILayout.BeginVertical();
            try {
                GUILayout.Space(sliderMargin);
                GUILayoutOption opt = GUILayout.ExpandWidth(true);//GUILayout.Width(uiParams.colorRect.width * 0.65f);
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
