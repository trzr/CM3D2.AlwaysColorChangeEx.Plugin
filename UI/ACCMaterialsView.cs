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
        private static Settings settings = Settings.Instance;

        // ComboBox用アイテムリスト
        private static GUIContent[] shaderNames;
        private static GUIContent[] ShaderNames {
            get {
                if (shaderNames == null) {
//                    CustomShaderHolder.InitShader();
                    int length = ShaderType.shaders.Length;
                    shaderNames = new GUIContent[length];
                    foreach (ShaderType shaderType in ShaderType.shaders) {
                        shaderNames[shaderType.idx] = new GUIContent(shaderType.name, shaderType.dispName);
                    }
                }
                return shaderNames;
            }
        }

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
        private static ResourceHolder resHolder = ResourceHolder.Instance;
        private static GUIContent plusIcon;
        private static GUIContent minusIcon;
        private static GUIContent copyIcon;
        private static GUIContent[] pasteIcon;
        private static GUIContent PlusIcon {
            get {
                if (plusIcon == null) {
                    plusIcon = new GUIContent(resHolder.PlusImage);
                }
                return plusIcon;
            }
        }
        private static GUIContent MinusIcon {
            get {
                if (minusIcon == null) {
                    minusIcon = new GUIContent(resHolder.MinusImage);
                }
                return minusIcon;
            }
        }
        private static GUIContent CopyIcon {
            get {
                if (copyIcon == null) {
                    copyIcon = new GUIContent("コピー", resHolder.CopyImage, "マテリアル情報をクリップボードへコピーする");
                }
                return copyIcon;
            }
        }
        private static GUIContent[] PasteIcon {
            get {
                if (pasteIcon == null) {
                    pasteIcon = new GUIContent[] {
                        new GUIContent("全貼付", resHolder.PasteImage, "クリップボードからマテリアル情報を貼付ける"),
                        new GUIContent("指定貼付", resHolder.PasteImage, "クリップボードからマテリアル情報を貼付ける"),
                    };
                }
                return pasteIcon;
            }
        }
        private static UIParams uiParams;
        //private readonly GUIStyle lStyleC   = new GUIStyle("label");
        //private readonly GUIStyle hsldStyle = new GUIStyle("horizontalSlider");
        private static GUIStyle bStyleLeft    = new GUIStyle("label");
        private static GUIStyle bStyleSS      = new GUIStyle("button");
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

            baseWidth = (uiparams.textureRect.width-20)*0.06f;
            bWidthOpt  = GUILayout.Width(baseWidth);
            bWidthWOpt = GUILayout.Width(baseWidth*2);

            bStyleSS.normal.textColor = uiparams.bStyleSC.normal.textColor;
            bStyleSS.alignment = TextAnchor.MiddleCenter;

            bStyleSS.fontSize = uiparams.fontSizeSS;
        };
        private static bool includeTex = false;
        private static bool includeShader = true;
        private static bool includeOthers = true;

        //public ACCMaterial original;
        private ClipBoardHandler clipHandler = ClipBoardHandler.Instance;
        public ACCMaterial edited;
        public ComboBoxLO shaderCombo;
        public bool expand;
        private int matIdx;
        public ACCMaterialsView(Renderer r, Material m, int idx) {
            //original = new ACCMaterial(m, r);
            //edited = new ACCMaterial(original);
            this.matIdx = idx;
            edited = new ACCMaterial(m, r);
        }
        public Action<string> tipsCall;

        private readonly bool[] FLAG_RATIO = {true, true, false};
        private readonly bool[] FLAG_INV  = {false, false, true};
        
        public void Show(bool reload) {
            // TODO tooltipをステータスバーに表示
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
                            int FLAG = 0;
                            if (includeTex)    FLAG |= MateHandler.MATE_TEX;
                            if (includeShader) FLAG |= MateHandler.MATE_SHADER;
                            if (includeOthers) FLAG |= MateHandler.MATE_COLOR | MateHandler.MATE_FLOAT;
                            LogUtil.DebugF("flag: tex={0}, shader={1}, others={2}", includeTex, includeShader, includeOthers);
                            MateHandler.Instance.Write(edited, clipHandler.mateText, FLAG);
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

                Material material = edited.material;
                int idx = edited.type.idx;
    
                if (shaderCombo == null) {
                    GUIContent selected = (idx >= 0 && idx < ShaderNames.Length) ? ShaderNames[idx] : GUIContent.none;
                    shaderCombo = new ComboBoxLO(selected, ShaderNames, uiParams.bStyleSC, uiParams.boxStyle, uiParams.listStyle, false);
                } else {
                    shaderCombo.SelectedItemIndex = idx;
                }
                shaderCombo.Show(GUILayout.ExpandWidth(true));//uiParams.optInsideWidth);
    
                int selectedIdx = shaderCombo.SelectedItemIndex;
                if (idx != selectedIdx && selectedIdx != -1) {
                    LogUtil.Debug("shader changed", idx, "=>", selectedIdx);
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
                                 }, ShaderPropType.RenderQueue.presetVals);


                ShaderType sdType = edited.type;
                for (int i=0; i< sdType.colProps.Length; i++) {
                    var colProp = sdType.colProps[i];
                    var editColor = edited.editColors[i];
                    if (reload) {
                        editColor.Set(material.GetColor(colProp.propId));
                    } else {
                        if (!editColor.val.HasValue) {
                            editColor.Set(colProp.defaultVal);
                            LogUtil.DebugF("value is empty. set white. color={0}, vals={1}, syncs={2}",
                                editColor.val, editColor.editVals, editColor.isSyncs);
                        }
                    }
                        
                    Color beforeColor = editColor.val.Value;
                    setColorSlider(colProp.name, ref editColor, colProp.colorType);
                    if (editColor.val.Value != beforeColor) {
                        material.SetColor(colProp.propId, editColor.val.Value);
                    }   
                }
                
                for (int i=0; i< sdType.fProps.Length; i++) {
                    var prop = sdType.fProps[i];
                    switch (prop.valType) {
                        case ValType.Float:
                            // slider
                            var fprop = prop;
                            if (reload) {
                                edited.editVals[i].Set(material.GetFloat(fprop.propId));
                            }
                            // fprop.SetValue(mat, val);
                            SetupFloatSlider(prop.name, edited.editVals[i],
                                             fprop.sliderMin, fprop.sliderMax, 
                                             (val) => fprop.SetValue(material, val),
                                              fprop.opts, fprop.presetVals);
                            break;
                        case ValType.Bool:
                            // TODO チェックボックス
                            break;
                    }
                }

            } finally {
                GUILayout.EndVertical();
            }
        }
            
        private void SetupFloatSlider(string label, EditValue edit, float sliderMin, float sliderMax,
                                      Action<float> func, params float[] vals) {
            SetupFloatSlider(label, edit, sliderMin, sliderMax, func, null, vals);
        }

        private void SetupFloatSlider(string label, EditValue edit, float sliderMin, float sliderMax,
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
                    if (GUILayout.Button("x-1", bStyleSS, bWidthWOpt)) {
                        edit.Set(edit.val * -1f);
                        changed = true;
                    }
                }
            }
            GUILayout.EndHorizontal();

            if (changed || drawValueSlider(null, edit, sliderMin, sliderMax)) {
                func(edit.val);
            }
        }

        private static readonly float delta = 0.1f;
        private void setColorSlider(string label, ref EditColor edit, ColorType colType) {
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
 
            int idx = 0;
            if (colType == ColorType.rgb || colType == ColorType.rgba) {
                changed |= drawValueSlider("R", ref edit, idx++, ref c.r);
                changed |= drawValueSlider("G", ref edit, idx++, ref c.g);
                changed |= drawValueSlider("B", ref edit, idx++, ref c.b);
            }
            if (colType == ColorType.rgba || colType == ColorType.a) {
                changed |= drawValueSlider("A", ref edit, idx, ref c.a);
            }
            if (changed) {
                edit.Set(c);
            }
        }

        private bool drawValueSlider(string label, EditValue edit, float sliderMin, float sliderMax) {
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

                EditRange range = edit.GetRange(idx);
                var val2 = GUILayout.TextField(edit.editVals[idx], uiParams.textStyleSC, optInputWidth);
                if (edit.editVals[idx] != val2) { // 直接書き換えられたケース
                    edit.Set(idx, val2, range);
                }

                changed |= drawSlider(ref sliderVal, range.editMin, range.editMax);
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
