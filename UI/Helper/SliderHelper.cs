using System;
using System.Collections.Generic;
using System.Globalization;
using CM3D2.AlwaysColorChangeEx.Plugin.Data;
using CM3D2.AlwaysColorChangeEx.Plugin.Util;
using UnityEngine;

namespace CM3D2.AlwaysColorChangeEx.Plugin.UI.Helper {
    public class SliderHelper {
        private readonly UIParams uiParams;
        public float epsilon = 0.000001f;
        public Color textColor;
        public Color textColorRed = Color.red;

        private float sliderMargin;
        private float buttonMargin;
        float labelWidth;
        float sliderInputWidth;
        
        private readonly GUIStyle bStyleSS      = new GUIStyle("button");
        private float baseWidth;
        private GUILayoutOption bWidthOpt;
        private GUILayoutOption bWidthWOpt;
        private GUILayoutOption optItemHeight;
        private GUILayoutOption optInputWidth;

        public SliderHelper(UIParams uiparams) {
            uiParams = uiparams;
            uiParams.Add(updateUI);
        }
        ~SliderHelper() {
            uiParams.Remove(updateUI);
        }

        private void updateUI(UIParams uiparams)  {
            // 幅の28%
            labelWidth = uiparams.colorRect.width * 0.28f;
            sliderMargin = uiparams.margin * 4.5f; // GUILayout.Space(uiParams.FixPx(7));

            buttonMargin = uiparams.margin * 3f;
            sliderInputWidth = uiparams.fontSizeS * 0.5625f * 8; // 最大8文字分としてフォントサイズの比率

            optInputWidth = GUILayout.Width(sliderInputWidth);
            optItemHeight = GUILayout.Height(uiparams.itemHeight);

            textColor = uiparams.textStyleSC.normal.textColor;

            baseWidth = (uiparams.textureRect.width-20)*0.06f;
            bWidthOpt  = GUILayout.Width(baseWidth);
            bWidthWOpt = GUILayout.Width(baseWidth*2);

            bStyleSS.normal.textColor = uiparams.bStyleSC.normal.textColor;
            bStyleSS.alignment = TextAnchor.MiddleCenter;
            bStyleSS.fontSize = uiparams.fontSizeSS;
        }

        public void SetupFloatSlider(string label, EditValue edit, float sliderMin, float sliderMax,
            Action<float> func, float[] vals1, float[] vals2) {
            SetupFloatSlider(label, edit, sliderMin, sliderMax, func, null, vals1, vals2);
        }

        internal void SetupFloatSlider(ShaderPropFloat fprop, EditValue edit, Action<float> func) {
            SetupFloatSlider(fprop.name, edit, fprop.sliderMin, fprop.sliderMax, func, fprop.opts, null,
                fprop.presetVals);
        }

        internal void SetupFloatSlider(string label, EditValue edit, float sliderMin, float sliderMax,
            Action<float> func, PresetOperation[] presetOprs, float[] vals1, float[] vals2) {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, uiParams.lStyle, optItemHeight);
            GUILayout.Space(uiParams.marginL);

            var changed = false;

            Action<float> preset = (val) => {
                var blabel = val.ToString(CultureInfo.InvariantCulture);
                GUILayoutOption opt;
                if (blabel.Length <= 1) opt = bWidthOpt;
                else if (blabel.Length <= 3) opt = bWidthWOpt;
                else opt = GUILayout.Width(baseWidth * 0.5f * (blabel.Length + 1));
                if (!GUILayout.Button(blabel, bStyleSS, opt)) return;

                edit.Set(val);
                changed = true;
            };

            if (vals1 != null)
                foreach (var val in vals1) {
                    preset(val);
                }

            if (vals2 != null)
                foreach (var val in vals2) {
                    preset(val);
                }

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
        private static readonly float DELTA = 0.1f;
        public static readonly float[] DEFAULT_PRESET = {0, 0.5f, 1f};
        public static readonly float[] DEFAULT_PRESET2 = {0, 0.5f, 1f, 1.5f, 2f};

        internal bool setColorSlider(string label, ref EditColor edit, ColorType colType) {
            var expand = true;
            return setColorSlider(label, ref edit, colType, DEFAULT_PRESET2, ref expand);
        }

        internal bool setColorSlider(string label, ref EditColor edit, ColorType colType, IEnumerable<float> vals, ref bool expand) {
            GUILayout.BeginHorizontal();
            var c = edit.val;
            var changed = false;
            try {
                if (GUILayout.Button(label, uiParams.lStyle, optItemHeight)) {
                    expand = !expand;
                }
                if (!expand) return false;

                foreach (var val in vals) {
                    var blabel = val.ToString(CultureInfo.InvariantCulture);
                    if (!GUILayout.Button(blabel, bStyleSS, (blabel.Length > 1) ? bWidthWOpt : bWidthOpt)) continue;
                    c.r = c.g = c.b = val;
                    changed = true;
                }

                if (GUILayout.Button("-", bStyleSS, bWidthOpt)) {
                    if (c.r < DELTA) c.r = 0;
                    else c.r -= DELTA;
                    if (c.g < DELTA) c.g = 0;
                    else c.g -= DELTA;
                    if (c.b < DELTA) c.b = 0;
                    else c.b -= DELTA;

                    changed = true;
                }

                if (GUILayout.Button("+", bStyleSS, bWidthOpt)) {
                    if (c.r + DELTA > 2.0f) c.r = 2;
                    else c.r += DELTA;
                    if (c.g + DELTA > 2.0f) c.g = 2;
                    else c.g += DELTA;
                    if (c.b + DELTA > 2.0f) c.b = 2;
                    else c.b += DELTA;
                    changed = true;
                }
            } finally {
                GUILayout.EndHorizontal();
            }

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

            return changed;
        }
        public bool DrawValueSlider(string label, EditValue edit, float sliderMin, float sliderMax) {
            var changed = false;
            var fontChanged = false;
            GUILayout.BeginHorizontal(optItemHeight);
            try {
                DrawLabel(ref label);

                if (!edit.isSync) {
                    SetTextColor(uiParams.textStyleSC, ref textColorRed);
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
                    SetTextColor(uiParams.textStyleSC, ref textColor);
                }
            }
            return changed;
        }

        public bool DrawValueSlider(string label, ref EditColor edit, int idx, ref float sliderVal) {
            var changed = false;
            var fontChanged = false;
            GUILayout.BeginHorizontal(optItemHeight);
            try {
                DrawLabel(ref label);

                if (!edit.isSyncs[idx]) {
                    SetTextColor(uiParams.textStyleSC, ref textColorRed);
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
                    SetTextColor(uiParams.textStyleSC, ref textColor);
                }
            }
            return changed;
        }

        private void DrawLabel(ref string label) {
            if (label != null) {
                var lWidth = uiParams.fontSizeS * label.Length;
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
                if (!NumberUtil.Equals(slidVal, sliderVal, epsilon)) { // スライダー変更時のみ
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

        private void SetTextColor(GUIStyle style, ref Color c) {
            style.normal.textColor = c;
            style.focused.textColor = c;
            style.active.textColor = c;
            style.hover.textColor = c;
        }
    }
}