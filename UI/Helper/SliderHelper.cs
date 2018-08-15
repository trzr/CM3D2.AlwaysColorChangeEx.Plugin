using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using CM3D2.AlwaysColorChangeEx.Plugin.Data;
using CM3D2.AlwaysColorChangeEx.Plugin.Util;
using UnityEngine;

namespace CM3D2.AlwaysColorChangeEx.Plugin.UI.Helper {
    public class SliderHelper {
        private static GUIContent copyIcon;
        private static GUIContent pasteIcon;
        private static GUIContent CopyIcon {
            get { return copyIcon ?? (copyIcon = new GUIContent(string.Empty, ResourceHolder.Instance.CopyImage, "カラーコードをクリップボードへコピーする")); }
        }
        private static GUIContent PasteIcon {
            get { return pasteIcon ?? (pasteIcon = new GUIContent(string.Empty, ResourceHolder.Instance.PasteImage, "クリップボードからカラーコードを貼付ける")); }
        }
        private static readonly ClipBoardHandler clipHandler = ClipBoardHandler.Instance;

        private readonly UIParams uiParams;
        public float epsilon = 0.000001f;
        public Color textColor;
        public Color textColorRed = Color.red;

        private float sliderMargin;
        private float buttonMargin;
        float labelWidth;
        float sliderInputWidth;
        
        private readonly GUIStyle bStyleSS = new GUIStyle("button");
        private readonly GUIStyle iconStyleSS = new GUIStyle("label");
        private GUILayoutOption bWidthOpt;
        private GUILayoutOption bWidthWOpt;
        private GUILayoutOption bWidthTOpt;
        private GUILayoutOption optItemHeight;
        private GUILayoutOption optInputWidth;
        private GUILayoutOption optCPWidth;
        private GUILayoutOption optCodeWidth;

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
            sliderInputWidth = uiparams.lStyleS.CalcSize(new GUIContent("zzzzzzzz")).x; // 最大8文字分の幅

            optInputWidth = GUILayout.Width(sliderInputWidth);
            optItemHeight = GUILayout.Height(uiparams.itemHeight);
            optCodeWidth = GUILayout.Width(uiparams.textStyleSC.CalcSize(new GUIContent("#DDDDDD")).x);
            if (optCPWidth == null) optCPWidth = GUILayout.Width(17);

            textColor = uiparams.textStyleSC.normal.textColor;

            bWidthOpt  = GUILayout.Width( bStyleSS.CalcSize(new GUIContent("x")).x ); //GUILayout.Width(baseWidth*0.8f);
            bWidthWOpt = GUILayout.Width( bStyleSS.CalcSize(new GUIContent("xx")).x ); //baseWidth*1.2f);
            bWidthTOpt = GUILayout.Width( bStyleSS.CalcSize(new GUIContent("xxx")).x ); //baseWidth*1.6f);

            bStyleSS.normal.textColor = uiparams.bStyleSC.normal.textColor;
            bStyleSS.alignment = TextAnchor.MiddleCenter;
            bStyleSS.fontSize = uiparams.fontSizeSS;
            bStyleSS.contentOffset = new Vector2(0, 1);
            bStyleSS.margin.left = 1;
            bStyleSS.margin.right = 1;
            bStyleSS.padding.left = 1;
            bStyleSS.padding.right = 1;

            iconStyleSS.normal.textColor = uiparams.lStyleS.normal.textColor;
            iconStyleSS.alignment = TextAnchor.MiddleCenter;
            iconStyleSS.fontSize = uiparams.fontSizeSS;
            iconStyleSS.contentOffset = new Vector2(0, 1);
            iconStyleSS.margin.left  = 0;
            iconStyleSS.margin.right = 0;
            iconStyleSS.padding.left = 1;
            iconStyleSS.padding.right = 1;
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
                var cont = new GUIContent(val.ToString(CultureInfo.InvariantCulture));
                if (!GUILayout.Button(cont, bStyleSS, getWidthOpt(cont))) return;

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
                    var cont = new GUIContent(pset.label);
                    if (!GUILayout.Button(cont, bStyleSS, getWidthOpt(cont))) continue;

                    edit.SetWithCheck(pset.func(edit.val));
                    changed = true;
                }
            }

            GUILayout.EndHorizontal();

            if (changed || DrawValueSlider(null, edit, sliderMin, sliderMax)) {
                func(edit.val);
            }
        }

        private GUILayoutOption getWidthOpt(GUIContent cont) {
            switch (cont.text.Length) {
            case 0:
            case 1:
                return bWidthOpt;
            case 2:
                return bWidthWOpt;
            case 3:
                return bWidthTOpt;
            default:
                return GUILayout.Width( bStyleSS.CalcSize(cont).x + 2 ); //baseWidth * 0.45f * (length + 1));
            }
        }

        private static readonly float DELTA = 0.1f;
        public static readonly float[] DEFAULT_PRESET = {0, 0.5f, 1f};
        public static readonly float[] DEFAULT_PRESET2 = {0, 0.5f, 1f, 1.5f, 2f};

        internal bool DrawColorSlider(ShaderPropColor colProp, ref EditColor edit, ColorPicker picker=null) {
            var expand = true;
            var presets = (colProp.composition) ? DEFAULT_PRESET2 : DEFAULT_PRESET;
                
            return DrawColorSlider(colProp.name, ref edit, colProp.colorType, presets, ref expand, picker);
        }

        internal bool DrawColorSlider(string label, ref EditColor edit, IEnumerable<float> vals, ref bool expand, ColorPicker picker = null) {
            return DrawColorSlider(label, ref edit, edit.type, vals, ref expand, picker);
        }

        internal bool DrawColorSlider(string label, ref EditColor edit, ColorType colType, IEnumerable<float> vals, ref bool expand, ColorPicker picker=null) {
            GUILayout.BeginHorizontal();
            var c = edit.val;
            var changed = false;
            try {
                if (picker != null) {
                    picker.Color = c;
                    if (GUILayout.Button(picker.ColorTex, uiParams.lStyleS, optItemHeight, picker.IconWidth, GUILayout.ExpandWidth(false))) {
                        picker.expand = !picker.expand;
                    }
                }
                if (GUILayout.Button(label, uiParams.lStyle, optItemHeight)) {
                    expand = !expand;
                }
                if (picker != null && ColorPicker.IsColorCode(picker.ColorCode)) {
                    if (GUILayout.Button(CopyIcon, uiParams.lStyle, optCPWidth, optItemHeight)) {
                        clipHandler.SetClipboard(picker.ColorCode);
                    }

                    var clip = clipHandler.GetClipboard();
                    GUI.enabled &= ColorPicker.IsColorCode(clip);
                    try {
                        if (GUILayout.Button(PasteIcon, uiParams.lStyle, optCPWidth, optItemHeight)) {
                            try {
                                if (picker.SetColorCode(clip)) {
                                    c = picker.Color;
                                    changed = true;
                                }
                            } catch (Exception e) {
                                LogUtil.Error("failed to import color-code", e);
                            }
                        }
                    } finally {
                        GUI.enabled = true;
                    }

                    var code = GUILayout.TextField(picker.ColorCode, 7, uiParams.textStyleSC, optCodeWidth);
                    if (code != picker.ColorCode && picker.SetColorCode(code)) {
                        c = picker.Color;
                        changed = true;
                    }
                }
                
                if (!expand) return false;

                foreach (var val in vals) {
                    var cont = new GUIContent(val.ToString(CultureInfo.InvariantCulture));
                    if (!GUILayout.Button(cont, bStyleSS, getWidthOpt(cont))) continue;
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
                    if (c.r + DELTA > edit.GetRange(0).editMax) c.r = edit.GetRange(0).editMax;
                    else c.r += DELTA;
                    if (c.g + DELTA > edit.GetRange(1).editMax) c.g = edit.GetRange(1).editMax;
                    else c.g += DELTA;
                    if (c.b + DELTA > edit.GetRange(2).editMax) c.b = edit.GetRange(2).editMax;
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
            if (picker != null) {
                if (picker.expand && picker.DrawLayout()) {
                    c = picker.Color;
                    changed = true;
                }
            }
            if (changed) {
                edit.Set(c);
            }

            return changed;
        }

        public bool DrawValueSlider(string label, EditValue edit) {
            return DrawValueSlider(label, edit, edit.range.editMin, edit.range.editMax);
        }
        public bool DrawValueSlider(string label, EditIntValue edit) {
            return DrawValueSlider(label, edit,
                () => {
                    var sliderVal = edit.val;
                    if (DrawSlider(ref sliderVal, edit.range.editMin, edit.range.editMax)) {
                        edit.Set(sliderVal);
                        return true;
                    }
                    return false;
                });
        }

        public bool DrawValueSlider(string label, EditValue edit, float sliderMin, float sliderMax) {
            return DrawValueSlider(label, edit,
                () => {
                    var sliderVal = edit.val;
                    if (DrawSlider(ref sliderVal, sliderMin, sliderMax)) {
                        edit.Set(sliderVal);
                        return true;
                    }
                    return false;
                });
        }

        public bool DrawValueSlider<T>(string label, EditValueBase<T> edit, Func<bool> drawSlider) where T : IComparable, IFormattable {
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

                changed |= drawSlider();
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
                var size = uiParams.lStyleS.CalcSize(new GUIContent(label));
                var lWidth = size.x;
                var space = labelWidth - sliderInputWidth - lWidth;  //- uiParams.labelSpace ;
                if (space > 0) GUILayout.Space(space);
                GUILayout.Label(label, uiParams.lStyleS, GUILayout.Width(lWidth));
            } else {
                GUILayout.Space(labelWidth - sliderInputWidth);
            }
        }
        private bool DrawSlider(ref float sliderVal, float min, float max) {
            var val = sliderVal;
            var changed = DrawSlider(() => {
                var slidVal = GUILayout.HorizontalSlider(val, min, max, GUILayout.ExpandWidth(true));
                if (!NumberUtil.Equals(slidVal, val, epsilon)) { // スライダー変更時のみ
                    if (val < min || max < val) {
                        // スライダーの範囲外の場合：スライダーを移動したケースを検知
                        if (min < slidVal && slidVal < max) {
                            val = slidVal;
                            return true;
                        }
                    } else {
                        val = slidVal;
                        return true;
                    }
                }
                return false;
            });
            if (changed) sliderVal = val;
            return changed;
        }
        private bool DrawSlider(ref int sliderVal, int min, int max) {
            var val = sliderVal;
            var changed = DrawSlider(() => {
                var slidVal = (int)GUILayout.HorizontalSlider(val, min, max, GUILayout.ExpandWidth(true));
                if (slidVal !=  val) { // スライダー変更時のみ
                    if (val < min || max < val) {
                        // スライダーの範囲外の場合：スライダーを移動したケースを検知
                        if (min < slidVal && slidVal < max) {
                            val = slidVal;
                            return true;
                        }
                    } else {
                        val = slidVal;
                        return true;
                    }
                }

                return false;
            });
            if (changed) sliderVal = val;
            return changed;
        }

        private bool DrawSlider(Func<bool> func) {
            var changed = false;
            GUILayout.BeginVertical();
            try {
                GUILayout.Space(sliderMargin);
                changed |= func();
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