/*
 */
using System;
using System.Collections.Generic;
using UnityEngine;
using CM3D2.AlwaysColorChangeEx.Plugin.Util;

namespace CM3D2.AlwaysColorChangeEx.Plugin.Data
{
    public class ACCMaterialsView {
        private const float EPSILON = 0.00001f;
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

        private static int GetIndex(string shaderName) {
            ShaderName[] names = ShaderMapper.ShaderNames;
            for (int i=0; i< names.Length; i++) {
                if (names[i].Name == shaderName) {
                    return i;
                }
            }
            return -1;
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
        private static UIParams uiParams;
        //private readonly GUIStyle lStyleC   = new GUIStyle("label");
        //private readonly GUIStyle hsldStyle = new GUIStyle("horizontalSlider");
        private static GUILayoutOption optItemHeight;
        private static GUILayoutOption optInputWidth;
        private static float labelWidth;
        private static float sliderInputWidth;
        private static float sliderMargin;           // スライダーのy軸マージン
        private static float buttonMargin;

        private static Action<UIParams> updateUI = (uiparams) => {
            // 幅の28%
            labelWidth    = uiparams.colorRect.width*0.28f;
            sliderMargin  = uiparams.margin*4.5f; // GUILayout.Space(uiParams.FixPx(7));

            buttonMargin  = uiparams.margin*3f;
            sliderInputWidth = uiparams.fontSize2 *0.5625f * 8; // 最大8文字分としてフォントサイズの比率

            optInputWidth = GUILayout.Width(sliderInputWidth);
            optItemHeight = GUILayout.Height(uiparams.itemHeight);
        };

        public ACCMaterial original;
        public ACCMaterial edited;
        public ComboBoxLO shaderCombo;

        public ACCMaterialsView(Material m) {
            original = new ACCMaterial(m);
            edited = new ACCMaterial(original);
        }

        private void ChangeShader(string shaderName, ref Material material) {
            Shader shader = Shader.Find(shaderName);
            if (shader != null) {
                material.shader = shader;
                var mat = ShaderMapper.resolve(shaderName);
                // 未設定の項目を有効にする際は、デフォルト値を使うべき？
                edited.Update(mat);
                LogUtil.DebugLog("selected shader updated");
            }
        }

        public void Show() {
            Material material = edited.material;

            GUILayout.BeginVertical();
            try {
                GUILayout.Label(edited.name, uiParams.lStyleC, optItemHeight);
    
                string shaderName = edited.shader.Name;
                int idx = GetIndex(shaderName);
    
                GUIContent selected = (idx != -1)? ShaderNames[idx] : ShaderNames[4];
                shaderCombo = shaderCombo ?? new ComboBoxLO(selected, ShaderNames, uiParams.bStyle2, uiParams.boxStyle, uiParams.listStyle, false);
                shaderCombo.Show(GUILayout.ExpandWidth(true));//uiParams.optInsideWidth);
    
                int selectedIdx = shaderCombo.SelectedItemIndex;
                if (idx != selectedIdx) {
                    LogUtil.DebugLog("selected shader changed", idx, "=>", selectedIdx);
                    // シェーダ変更
                    var shaderName0 = ShaderNames[selectedIdx].text;
                    ChangeShader(shaderName0, ref material);
                }
                MaterialType mat = edited.type;
    
                float renderQueue = edited.renderQueue;
                var rq = renderQueue.ToString();
                drawValueSlider("RQ", rq, 0, 5000, ref renderQueue);
                if (NotEquals(renderQueue, edited.renderQueue)) {
                    edited.renderQueue = (int)renderQueue;
                    material.SetFloat("_SetManualRenderQueue", renderQueue);
                    material.renderQueue = edited.renderQueue;
                }

                Func<string, string, Color?, Color> ColorSlider = (label, key, c) => {
                    var color = c.Value;
                    setColorSlider(label, ref color, mat.isTrans);
                    if (color != c) {
                        material.SetColor(key, color);
                    }
                    return color;
                };

                if (mat.hasColor) edited.color = ColorSlider("Color", "_Color", edited.color);
                if (mat.isLighted) {
                    edited.shadowColor = ColorSlider("Shadow Color", "_ShadowColor", edited.shadowColor);
                }
                if (mat.isOutlined) {
                    edited.outlineColor = ColorSlider("Outline Color", "_OutlineColor", edited.outlineColor);
                }
                if (mat.isToony) {
                    edited.rimColor = ColorSlider("Rim Color", "_RimColor", edited.rimColor);
                }
    
                if (mat.isLighted) {
                    SetupFloatSlider(material, "Shininess", "_Shininess", "{0:F2}", ref edited.shininess, 
                                    -1000f, 1000f, settings.shininessMin, settings.shininessMax);
                }
                if (mat.isOutlined) {
                    SetupFloatSlider(material, "OutLineWidth", "_OutlineWidth", "{0:F5}", ref edited.outlineWidth, 
                                    0f, 1f, settings.outlineWidthMin, settings.outlineWidthMax);
                }
                if (mat.isToony) {
                    SetupFloatSlider(material, "RimPower", "_RimPower", "{0:F3}", ref edited.rimPower, 
                                    0f, 100f, settings.rimPowerMin, settings.rimPowerMax);

                    SetupFloatSlider(material, "RimShift", "_RimShift", "{0:F3}", ref edited.rimShift,
                                    -10f, 10f, settings.rimShiftMin, settings.rimShiftMax);
                }
                if (mat.isHair) {
                    SetupFloatSlider(material, "HiRate", "_HiRate", "{0:F2}", ref edited.hiRate,
                                     0f, 100f, settings.hiRateMin, settings.hiRateMax);
    
                    SetupFloatSlider(material, "HiPow", "_HiPow", "{0:F4}", ref edited.hiPow,
                                     0.001f, 100f, settings.hiPowMin, settings.hiPowMax);
                }
                if (mat.hasFloat1) {
                    SetupFloatSlider(material, "FloatValue1", "_FloatValue1", "{0:F2}", ref edited.floatVal1,
                                     0f, 500f, settings.floatVal1Min, settings.floatVal1Max);
                }
                if (mat.hasFloat2) {
                    SetupFloatSlider(material, "FloatValue2", "_FloatValue2", "{0:F2}", ref edited.floatVal2,
                                     -50f, 50f, settings.floatVal2Min, settings.floatVal2Max);
                }
                if (mat.hasFloat3) {
                    SetupFloatSlider(material, "FloatValue3", "_FloatValue3", "{0:F3}", ref edited.floatVal3,
                                     0f, 50f, settings.floatVal3Min, settings.floatVal3Max);
                }

            } finally {
                GUILayout.EndVertical();
            }
        }
        private void SetupFloatSlider(Material material, string label, string key, string format, ref float val, params float[] range) {
            float outlineWidth = val;
            setValueSlider(label, null, format, outlineWidth, range[0], range[1], ref outlineWidth, range[2], range[3]);
            if ( NotEquals(outlineWidth, edited.outlineWidth) ) {
                val = outlineWidth;
                material.SetFloat(key, outlineWidth);
            }
        }

        private static bool NotEquals(float f1, float f2) {
            return Math.Abs(f1- f2) > EPSILON;
        }

        private void setColorSlider(string label, ref Color color, bool isTrans) {
            GUILayout.Label(label, uiParams.lStyle, optItemHeight);
    
            drawValueSlider("R", String.Format("{0:F3}", color.r), 0f, 2f, ref color.r, 0f, 2f);
            drawValueSlider("G", String.Format("{0:F3}", color.g), 0f, 2f, ref color.g, 0f, 2f);
            drawValueSlider("B", String.Format("{0:F3}", color.b), 0f, 2f, ref color.b, 0f, 2f);
            if (isTrans) {
                drawValueSlider("A", String.Format("{0:F3}", color.a), 0f, 2f, ref color.a, 0f, 2f);
            }
        }

        private void setValueSlider(string label, string valFormat, float val, float minVal, float maxVal, ref float sliderVal, float min, float max) {
            GUILayout.Label(label, uiParams.lStyle, optItemHeight);
            drawValueSlider(null, string.Format(valFormat, val), minVal, maxVal, ref sliderVal, min, max);
        }
        private void setValueSlider(string label, string valLabel, string valFormat, float val, float minVal, float maxVal, ref float sliderVal, float min, float max) {
            GUILayout.Label(label, uiParams.lStyle, optItemHeight);
            drawValueSlider(valLabel, string.Format(valFormat, val), minVal, maxVal, ref sliderVal, min, max);
        }

        private void drawValueSlider(string label, string val, float minVal, float maxVal, ref float sliderVal) {
            drawValueSlider(label, val, minVal, maxVal, ref sliderVal, minVal, maxVal);
        }
        private void drawValueSlider(string label, string val, float minVal, float maxVal, ref float sliderVal, float min, float max) {
            GUILayout.BeginHorizontal(optItemHeight);
            try {
                if(label != null) {
                    //float lWidth = 13*label.Length;
                    float lWidth = uiParams.fontSize2*label.Length;
                    float space = labelWidth - sliderInputWidth - lWidth;  //- uiParams.labelSpace ;
                    if (space > 0) GUILayout.Space(space);
                    GUILayout.Label(label, uiParams.lStyleS, GUILayout.Width(lWidth));
                } else {
                    GUILayout.Space(labelWidth - sliderInputWidth);
                }
    
                var val2 = GUILayout.TextField(val, uiParams.textStyleC, optInputWidth);
                if (val != val2) { // 直接書き換えされたケース
                    float v;
                    if (float.TryParse(val2, out v)) {
                        if (minVal > v)      v = minVal;
                        else if (maxVal < v) v = maxVal;
                        sliderVal = v;
                    }
                }

                GUILayout.BeginVertical();
                try {
                    GUILayout.Space(sliderMargin);
                    GUILayoutOption opt = GUILayout.ExpandWidth(true);//GUILayout.Width(uiParams.colorRect.width * 0.65f);
                    var changed = GUILayout.HorizontalSlider(sliderVal, min, max, opt);
                    if (Math.Abs(changed - sliderVal) > EPSILON) { // slider変更時のみ
                        if (sliderVal > max || sliderVal < min) {
                            // sliderの範囲外の場合：スライダを移動したケースを検知
                            if (changed < max && changed > min) sliderVal = changed;
                        } else {
                            sliderVal = changed;
                        }
                    }
                } finally {
                    GUILayout.EndVertical();
                }
                GUILayout.Space(buttonMargin);

            } finally {
                GUILayout.EndHorizontal();
            }
        }
    }
}
