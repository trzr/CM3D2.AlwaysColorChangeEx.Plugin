using System;
using System.Collections.Generic;
using UnityEngine;
using CM3D2.AlwaysColorChange.Plugin.Util;

namespace CM3D2.AlwaysColorChange.Plugin.Data
{
    /// <summary>
    /// Description of ACCMaterial.
    /// </summary>
    public class ACCMaterial {
        private static readonly Color WHITE = new Color(1f, 1f, 1f, 1f);
        private const float DEFAULT_FV1 = 10f;
        private const float DEFAULT_FV2 = 1f;
        private const float DEFAULT_FV3 = 1f;
        public Material material;
        public string name;
        public ShaderMapper.ShaderName shader;
        public ShaderMapper.MaterialFlag flag;
        public int renderQueue;
        public Color? color;
        public Color? shadowColor;
        public Color? rimColor;
        public Color? outlineColor;
        public float? shininess;
        public float? outlineWidth;
        public float? rimPower;
        public float? rimShift;
        public float? hiRate;
        public float? hiPow;
        public float? floatVal1;
        public float? floatVal2;
        public float? floatVal3;
        public bool dirty;

        public ACCMaterial(string matName, ShaderMapper.MaterialFlag flag) {
            this.name = matName;
            this.flag = flag;
            this.shader = flag.shader;

            renderQueue = 2000;
            if (flag.hasColor) color = WHITE;
            if (flag.isLighted) {
                shadowColor = WHITE;
                shininess = 0f;
            }
            if (flag.isOutlined) {
                outlineColor = new Color(0, 0, 0, 1);
                outlineWidth = 0f;
            }
            if (flag.isToony) {
                rimColor = WHITE;
                rimPower = 0f;
                rimShift = 0f;
            }
            if (flag.isHair) {
                hiRate = 0f;
                hiPow = 0.001f;
            }
            if (flag.hasFloat1) {
                floatVal1 = DEFAULT_FV1;
            }
            if (flag.hasFloat2) {
                floatVal2 = DEFAULT_FV2;
            }
            if (flag.hasFloat3) {
                floatVal3 = DEFAULT_FV3;
            }
        }
        public ACCMaterial(ACCMaterial src) {
            this.material = src.material;
            this.name = src.name;
            this.shader = src.shader;
            this.flag = src.flag;
            this.renderQueue = src.renderQueue;
            this.color = src.color;
            this.shadowColor = src.shadowColor;
            this.rimColor = src.rimColor;
            this.outlineColor = src.outlineColor;
            this.shininess = src.shininess;
            this.outlineWidth = src.outlineWidth;
            this.rimPower = src.rimPower;
            this.rimShift = src.rimShift;
            this.hiRate = src.hiRate;
            this.hiPow = src.hiPow;
            this.floatVal1 = src.floatVal1;
            this.floatVal2 = src.floatVal2;
            this.floatVal3 = src.floatVal3;

        }
        public ACCMaterial(Material m) {
            this.material = m;
            name = m.name;
            flag = ShaderMapper.resolve(m.shader.name);
            shader = flag.shader;
            renderQueue = m.renderQueue;

            if (flag.hasColor) color = m.GetColor("_Color");
            if (flag.isLighted) {
                shadowColor = m.GetColor("_ShadowColor");
                shininess = m.GetFloat("_Shininess");
            }
            if (flag.isOutlined) {
                outlineColor = m.GetColor("_OutlineColor");
                outlineWidth = m.GetFloat("_OutlineWidth");
            }
            if (flag.isToony) {
                rimColor = m.GetColor("_RimColor");
                rimPower = m.GetFloat("_RimPower");
                rimShift = m.GetFloat("_RimShift");
            }
            if (flag.isHair) {
                hiRate = m.GetFloat("_HiRate");
                hiPow = m.GetFloat("_HiPow");
            }
            if (flag.hasFloat1) {
                floatVal1 = m.GetFloat("_FloatValue1");
            }
            if (flag.hasFloat2) {
                floatVal2 = m.GetFloat("_FloatValue2");
            }
            if (flag.hasFloat3) {
                floatVal3 = m.GetFloat("_FloatValue3");
            }
        }
        public void Update(ShaderMapper.MaterialFlag flag) {
            if (this.flag == flag) return;
                
            this.flag = flag;
            this.shader = flag.shader;
            if (flag.hasColor) {
                if (!color.HasValue) color = WHITE;
            } else {
                color = null;
            }
            if (flag.isLighted) {
                if (!shadowColor.HasValue) shadowColor = WHITE;
                if (!shininess.HasValue) shininess   = 0f;
            } else {
                shadowColor = null;
                shininess   = null;
            }
            if (flag.isOutlined) {
                if (!outlineColor.HasValue) outlineColor = new Color(0f, 0f, 0f, 1f);
                if (!outlineWidth.HasValue) outlineWidth = 0f;
            } else {
                outlineColor = null;
                outlineWidth = null;
            }
            if (flag.isToony) {
                if (!rimColor.HasValue) rimColor = WHITE;
                if (!rimPower.HasValue) rimPower = 0f;
                if (!rimShift.HasValue) rimShift = 0f;
            } else {
                rimColor = null;
                rimPower = null;
                rimShift = null;
            }
            if (flag.isHair) {
                if (!hiRate.HasValue) hiRate = 0f;
                if (!hiPow.HasValue) hiPow = 0.001f;
            } else {
                hiRate = null;
                hiPow = null;
            }
            if (flag.hasFloat1) {
                if (!floatVal1.HasValue) floatVal1 = DEFAULT_FV1;
            } else {
                floatVal1 = null;
            }
            if (flag.hasFloat2) {
                if (!floatVal2.HasValue) floatVal2 = DEFAULT_FV2;
            } else {
                floatVal2 = null;
            }
            if (flag.hasFloat3) {
                if (!floatVal3.HasValue) floatVal3 = DEFAULT_FV3;
            } else {
                floatVal3 = null;
            }
        }
        public void ReflectTo(Material m) {
            m.SetFloat("_SetManualRenderQueue", renderQueue);
            m.renderQueue = renderQueue;

            if (flag.hasColor) {
                m.SetColor("_Color", color.Value);
            }
            if (flag.isLighted) {
                m.SetColor("_ShadowColor", shadowColor.Value);
                m.SetFloat("_Shininess", shininess.Value);
            }
            if (flag.isOutlined) {
                m.SetColor("_OutlineColor", outlineColor.Value);
                m.SetFloat("_OutlineWidth", outlineWidth.Value);
            }
            if (flag.isToony) {
                m.SetColor("_RimColor", rimColor.Value);
                m.SetFloat("_RimPower", rimPower.Value);
                m.SetFloat("_RimShift", rimShift.Value);
            }
            if (flag.isHair) {
                m.SetFloat("_HiRate", hiRate.Value);
                m.SetFloat("_HiPow", hiPow.Value);
            }
            if (flag.isHair) {
                m.SetFloat("_HiRate", hiRate.Value);
                m.SetFloat("_HiPow", hiPow.Value);
            }
            if (flag.hasFloat1) {
                m.SetFloat("_FloatValue1", floatVal1.Value);
            }
            if (flag.hasFloat2) {
                m.SetFloat("_FloatValue2", floatVal2.Value);
            }
            if (flag.hasFloat3) {
                m.SetFloat("_FloatValue3", floatVal3.Value);
            }
            
        }
    }

    public class ACCMaterialsView {
        private const float EPSILON = 0.00001f;
        private static Dictionary<int, Shader> changeShaders = new Dictionary<int, Shader>();
        private static Settings settings = Settings.Instance;

        public static void Clear() {
            changeShaders.Clear();
        }

        // ComboBox用アイテムリスト
        private static GUIContent[] shaderNames;
        private static GUIContent[] ShaderNames {
            get {
                if (shaderNames == null) {
                    shaderNames = new GUIContent[ShaderMapper.ShaderNames.Length];
                    int idx = 0;
                    foreach (ShaderMapper.ShaderName shaderName in ShaderMapper.ShaderNames) {
                        shaderNames[idx++] = new GUIContent(shaderName.Name, shaderName.DisplayName);
                    }
                }
                return shaderNames;
            }
        }

        private int GetIndex(string shaderName) {
            ShaderMapper.ShaderName[] names = ShaderMapper.ShaderNames;
            for (int i=0; i< names.Length; i++) {
                if (names[i].Name == shaderName) {
                    return i;
                }
            }
            return -1;
        }
        ACCMaterial original;
        ACCMaterial edited;
        public ComboBox shaderCombo;
        readonly UIParams uiParams;

        public ACCMaterialsView(Material m, UIParams uiParams) {
            original = new ACCMaterial(m);
            edited = new ACCMaterial(original);
            this.uiParams = uiParams;
        }

        /**
         * 表示要素のアイテム数を返す.
         * 戻り値は縦の長さを図る目安となる
         */
        public int itemCount() {
            int count = 4; // title + shaderName + renderQueue + 1
            ShaderMapper.MaterialFlag mate = edited.flag;
            if (mate.hasColor)   count += mate.isTrans ? 5 : 4; // color
            if (mate.isOutlined) count += 6; // CoutlineColor + OutlineWidth(float)
            if (mate.isToony)    count += 8; // RimColor + RimPower,RimShift (float x2)
            if (mate.isLighted)  count += 6; // ShadowColor + Shininess(float)
            if (mate.isHair)     count += 4; // HiRate,HiPow(float x2)
            if (mate.hasFloat1)  count += 2;
            if (mate.hasFloat2)  count += 2;
            if (mate.hasFloat3)  count += 2;
            if (shaderCombo != null && shaderCombo.IsClickedComboButton) {
                count += (int)(shaderCombo.ItemCount * 0.75f);
            }
            return count;
        }

        public void Show(ref Rect outRect) {
            Material material = edited.material;
            edited.dirty = false;
            outRect.x = uiParams.margin;
            GUI.Label(outRect, edited.name, uiParams.lStyle);
            outRect.x += uiParams.margin;
            outRect.width = uiParams.colorRect.width- 20 - uiParams.margin * 3;
            outRect.y += uiParams.unitHeight;

            string shaderName = edited.shader.Name;
            ShaderMapper.MaterialFlag mat = edited.flag;
            int idx = GetIndex(shaderName);

            GUIContent selected = (idx != -1)? ShaderNames[idx] : ShaderNames[4];
            shaderCombo = shaderCombo ?? new ComboBox(outRect, selected, ShaderNames, uiParams.bStyle2, uiParams.boxStyle, uiParams.listStyle);

            shaderCombo.rect = outRect;
            shaderCombo.Show();
            if (shaderCombo.IsClickedComboButton) {
                outRect.y += (shaderCombo.ItemCount*0.75f)*uiParams.unitHeight;
            } else {
                outRect.y += uiParams.unitHeight;
            }

            int selectedIdx = shaderCombo.SelectedItemIndex;
            if (idx != selectedIdx) {
                LogUtil.DebugLog("selected shader changed", idx, "=>", selectedIdx);
                // シェーダ変更
                shaderName = ShaderNames[selectedIdx].text;
                Shader shader = Shader.Find(shaderName);
                if (shader != null) {
                    material.shader = shader;
                    mat = ShaderMapper.resolve(shaderName);
                    // 未設定の項目を有効にする際は、デフォルト値を使うべき？
                    edited.Update(mat);
                    LogUtil.DebugLog("selected shader updated");
                }
            }

            int renderQueue = edited.renderQueue;
            renderQueue = (int)drawModValueSlider(outRect, renderQueue, 0, 5000, String.Format("{0}:{1}", "RQ", renderQueue));
            if (renderQueue != edited.renderQueue) {
                edited.renderQueue = renderQueue;
                material.SetFloat("_SetManualRenderQueue", renderQueue);
                material.renderQueue = renderQueue;
            }
            outRect.y += uiParams.unitHeight;

            if (mat.hasColor) {
                Color color = edited.color.Value;
                setColorSlider(ref outRect, "Color", ref color, mat.isTrans);
                if (color != edited.color.Value) {
                    edited.color = color;
                    material.SetColor("_Color", color);
                }

//                // シェーダ置き換え
//                try {
//                    Shader mShader = material.shader;
//                    if (color.a < 1f) {
//                        // CM3D2のシェーダのみを対象として、_Transのついたシェーダを使用するように変更 ただし、Hairは対応するshader無し,
//                        if (!mat.isTrans && mat.isToony && mat.isLighted && !mat.isHair) {
////                                    Shader shader = Shader.Find(shaderName + "_Trans");
//                            Shader shader = Shader.Find("CM3D2/Toony_Lighted_Trans");
//                            if (shader != null) {
//                                material.shader = shader;
//                                LogUtil.DebugLog(material.name, " changed shader.", shaderName, "=>", shader.name);
//
//                                try {
//                                    // 上書きしない 
//                                    changeShaders.Add(material.GetInstanceID(), mShader);
//                                } catch(ArgumentException ignore) {}
//                            }
//                        }
//
//                    } else {
//                        // 元のシェーダに戻す
//                        Shader temp = null;
//                        if (changeShaders.TryGetValue(material.GetInstanceID(), out temp)) {
//                            material.shader = temp;
//                        }
//                    }
//
//                } catch (Exception e) {
//                    LogUtil.DebugLog(e);
//                }

            }
            if (mat.isLighted) {
                Color shadowColor  = edited.shadowColor.Value;
                setColorSlider(ref outRect, "Shadow Color", ref shadowColor, mat.isTrans);
                if (shadowColor != edited.shadowColor.Value) {
                    edited.shadowColor = shadowColor;
                    material.SetColor("_ShadowColor", shadowColor);
                }
            }
            if (mat.isOutlined) {
                Color outlineColor = edited.outlineColor.Value;
                setColorSlider(ref outRect, "Outline Color", ref outlineColor, false);
                if (outlineColor != edited.outlineColor.Value) {
                    edited.outlineColor = outlineColor;
                    material.SetColor("_OutlineColor", outlineColor);
                }
            }
            if (mat.isToony) {
                Color rimColor = edited.rimColor.Value;
                setColorSlider(ref outRect, "Rim Color", ref rimColor, false);
                if (rimColor != edited.rimColor.Value) {
                    edited.rimColor = rimColor;
                    material.SetColor("_RimColor", rimColor);
                }
            }

            if (mat.isLighted) {
                float shininess = edited.shininess.Value;
                shininess = setValueSlider(ref outRect, "Shininess", "  {0:F2}", shininess, 
                                           settings.shininessMin, settings.shininessMax);
                if ( NotEquals(shininess, edited.shininess.Value) ) {
                    edited.shininess = shininess;
                    material.SetFloat("_Shininess", shininess);
                }
            }
            if (mat.isOutlined) {
                float outlineWidth = edited.outlineWidth.Value;
                outlineWidth = setValueSlider(ref outRect, "OutlineWidth", "  {0:F5}", outlineWidth, 
                                           settings.outlineWidthMin, settings.outlineWidthMax);
                if ( NotEquals(outlineWidth, edited.outlineWidth.Value) ) {
                    edited.outlineWidth = outlineWidth;
                    material.SetFloat("_OutlineWidth", outlineWidth);
                }
            }
            if (mat.isToony) {
                float rimPower     = edited.rimPower.Value;
                rimPower = setValueSlider(ref outRect, "RimPower", "  {0:F2}", rimPower, 
                                           settings.rimPowerMin, settings.rimPowerMax);
                if ( NotEquals(rimPower, edited.rimPower.Value) ) {
                    edited.rimPower = rimPower;
                    material.SetFloat("_RimPower", rimPower);
                }

                float rimShift = edited.rimShift.Value;
                rimShift = setValueSlider(ref outRect, "RimShift", "  {0:F2}", rimShift, 
                                           settings.rimShiftMin, settings.rimShiftMax);
                if ( NotEquals(rimShift, edited.rimShift.Value) ) {
                    edited.rimShift = rimShift;
                    material.SetFloat("_RimShift", rimShift);
                }
            }
            if (mat.isHair) {
                float hiRate       = edited.hiRate.Value;
                hiRate = setValueSlider(ref outRect, "HiRate", "  {0:F2}", hiRate, 
                                           settings.hiRateMin, settings.hiRateMax);
                if ( NotEquals(hiRate, edited.hiRate.Value) ) {
                    edited.hiRate = hiRate;
                    material.SetFloat("_HiRate", hiRate);
                }

                float hiPow        = edited.hiPow.Value;
                hiPow = setValueSlider(ref outRect, "HiPow", "  {0:F4}", hiPow, 
                                           settings.hiPowMin, settings.hiPowMax);
                if ( NotEquals(hiPow, edited.hiPow.Value) ) {
                    edited.hiPow = hiPow;
                    material.SetFloat("_HiPow", hiPow);
                }
            }
            if (mat.hasFloat1) {
                float fv = edited.floatVal1.Value;
                fv = setValueSlider(ref outRect, "FloatValue1", "  {0:F2}", fv, 
                                           settings.floatVal1Min, settings.floatVal1Max);
                if ( NotEquals(fv, edited.floatVal1.Value) ) {
                    edited.floatVal1= fv;
                    material.SetFloat("_FloatValue1", fv);
                }
            }
            if (mat.hasFloat2) {
                float fv = edited.floatVal2.Value;
                fv = setValueSlider(ref outRect, "FloatValue2", "  {0:F2}", fv, 
                                           settings.floatVal2Min, settings.floatVal2Max);
                if ( NotEquals(fv, edited.floatVal2.Value) ) {
                    edited.floatVal2= fv;
                    material.SetFloat("_FloatValue2", fv);
                }
            }
            if (mat.hasFloat3) {
                float fv = edited.floatVal3.Value;
                fv = setValueSlider(ref outRect, "FloatValue3", "  {0:F3}", fv, 
                                           settings.floatVal3Min, settings.floatVal3Max);
                if ( NotEquals(fv, edited.floatVal3.Value) ) {
                    edited.floatVal3= fv;
                    material.SetFloat("_FloatValue3", fv);
                }
            }

            outRect.y += uiParams.margin * 3;
        }
        private static bool NotEquals(float f1, float f2) {
            return Math.Abs(f1- f2) > EPSILON;
        }

        private void setColorSlider(ref Rect outRect, string label, ref Color color, bool isTrans) {
            GUI.Label(outRect, label, uiParams.lStyle);
            
            outRect.y += uiParams.unitHeight;
    
            color.r = drawModValueSlider(outRect, color.r, 0f, 2f, String.Format("{0}:{1:F2}", "R", color.r));
            outRect.y += uiParams.unitHeight;
            color.g = drawModValueSlider(outRect, color.g, 0f, 2f, String.Format("{0}:{1:F2}", "G", color.g));
            outRect.y += uiParams.unitHeight;
            color.b = drawModValueSlider(outRect, color.b, 0f, 2f, String.Format("{0}:{1:F2}", "B", color.b));
            outRect.y += uiParams.unitHeight;
            if (isTrans) {
                color.a = drawModValueSlider(outRect, color.a, 0f, 1f, String.Format("{0}:{1:F2}", "A", color.a));
                outRect.y += uiParams.unitHeight;
            }
        }

        private float setValueSlider(ref Rect outRect, string label, string format, float val, float min, float max) {
            GUI.Label(outRect, label, uiParams.lStyle);
            outRect.y += uiParams.unitHeight;
    
            val = drawModValueSlider(outRect, val, min, max, String.Format(format, (float)val) );
            outRect.y += uiParams.unitHeight;
    
            return val;
        }
    
        private float drawModValueSlider(Rect outRect, float value, float min, float max, string label)
        {
            float conWidth = outRect.width;
    
            float margin = uiParams.margin*3;
            outRect.x += margin;
            outRect.width = conWidth * 0.35f -margin;
            GUI.Label(outRect, label, uiParams.lStyle);
            outRect.x += outRect.width - margin;
    
            outRect.width = conWidth * 0.65f;
            outRect.y += uiParams.FixPx(7);
    
            return GUI.HorizontalSlider(outRect, value, min, max);
        }
    }
}
