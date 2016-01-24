/*
 * TODO
*  GUILayoutへ変更
 */
using System;
using System.Collections.Generic;
using UnityEngine;
using CM3D2.AlwaysColorChangeEx.Plugin.Util;

namespace CM3D2.AlwaysColorChangeEx.Plugin.Data
{
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
        public ACCMaterial original;
        public ACCMaterial edited;
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
            MaterialType mate = edited.type;
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
        public void Show(ref Rect outRect) {
            Material material = edited.material;
            outRect.x = uiParams.margin;
            GUI.Label(outRect, edited.name, uiParams.lStyleC);

            outRect.x += uiParams.margin;
            outRect.width = uiParams.colorRect.width- 20 - uiParams.margin * 3;
            outRect.y += uiParams.itemHeight;

            string shaderName = edited.shader.Name;
            int idx = GetIndex(shaderName);

            GUIContent selected = (idx != -1)? ShaderNames[idx] : ShaderNames[4];
            shaderCombo = shaderCombo ?? new ComboBox(outRect, selected, ShaderNames, uiParams.bStyle2, uiParams.boxStyle, uiParams.listStyle);

            shaderCombo.rect = outRect;
            shaderCombo.Show();
            if (shaderCombo.IsClickedComboButton) {
                outRect.y += (shaderCombo.ItemCount*0.9f)*uiParams.itemHeight;
            } else {
                outRect.y += uiParams.itemHeight;
            }

            int selectedIdx = shaderCombo.SelectedItemIndex;
            if (idx != selectedIdx) {
                LogUtil.DebugLog("selected shader changed", idx, "=>", selectedIdx);
                // シェーダ変更
                var shaderName0 = ShaderNames[selectedIdx].text;
                ChangeShader(shaderName0, ref material);
            }
            MaterialType mat = edited.type;

            int renderQueue = edited.renderQueue;
            renderQueue = (int)drawModValueSlider(outRect, renderQueue, 0, 5000, String.Format("{0}:{1}", "RQ", renderQueue));
            if (renderQueue != edited.renderQueue) {
                edited.renderQueue = renderQueue;
                material.SetFloat("_SetManualRenderQueue", renderQueue);
                material.renderQueue = renderQueue;
            }
            outRect.y += uiParams.itemHeight;

            if (mat.hasColor) {
                Color color = edited.color.Value;
                setColorSlider(ref outRect, "Color", ref color, mat.isTrans);
                if (color != edited.color.Value) {
                    edited.color = color;
                    material.SetColor("_Color", color);
                }
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
                float shininess = edited.shininess;
                shininess = setValueSlider(ref outRect, "Shininess", "  {0:F2}", shininess, 
                                           settings.shininessMin, settings.shininessMax);
                if ( NotEquals(shininess, edited.shininess) ) {
                    edited.shininess = shininess;
                    material.SetFloat("_Shininess", shininess);
                }
            }
            if (mat.isOutlined) {
                float outlineWidth = edited.outlineWidth;
                outlineWidth = setValueSlider(ref outRect, "OutlineWidth", "  {0:F5}", outlineWidth, 
                                           settings.outlineWidthMin, settings.outlineWidthMax);
                if ( NotEquals(outlineWidth, edited.outlineWidth) ) {
                    edited.outlineWidth = outlineWidth;
                    material.SetFloat("_OutlineWidth", outlineWidth);
                }
            }
            if (mat.isToony) {
                float rimPower     = edited.rimPower;
                rimPower = setValueSlider(ref outRect, "RimPower", "  {0:F2}", rimPower, 
                                           settings.rimPowerMin, settings.rimPowerMax);
                if ( NotEquals(rimPower, edited.rimPower) ) {
                    edited.rimPower = rimPower;
                    material.SetFloat("_RimPower", rimPower);
                }

                float rimShift = edited.rimShift;
                rimShift = setValueSlider(ref outRect, "RimShift", "  {0:F2}", rimShift, 
                                           settings.rimShiftMin, settings.rimShiftMax);
                if ( NotEquals(rimShift, edited.rimShift) ) {
                    edited.rimShift = rimShift;
                    material.SetFloat("_RimShift", rimShift);
                }
            }
            if (mat.isHair) {
                float hiRate       = edited.hiRate;
                hiRate = setValueSlider(ref outRect, "HiRate", "  {0:F2}", hiRate, 
                                           settings.hiRateMin, settings.hiRateMax);
                if ( NotEquals(hiRate, edited.hiRate) ) {
                    edited.hiRate = hiRate;
                    material.SetFloat("_HiRate", hiRate);
                }

                float hiPow        = edited.hiPow;
                hiPow = setValueSlider(ref outRect, "HiPow", "  {0:F4}", hiPow, 
                                           settings.hiPowMin, settings.hiPowMax);
                if ( NotEquals(hiPow, edited.hiPow) ) {
                    edited.hiPow = hiPow;
                    material.SetFloat("_HiPow", hiPow);
                }
            }
            if (mat.hasFloat1) {
                float fv = edited.floatVal1;
                fv = setValueSlider(ref outRect, "FloatValue1", "  {0:F2}", fv, 
                                           settings.floatVal1Min, settings.floatVal1Max);
                if ( NotEquals(fv, edited.floatVal1) ) {
                    edited.floatVal1= fv;
                    material.SetFloat("_FloatValue1", fv);
                }
            }
            if (mat.hasFloat2) {
                float fv = edited.floatVal2;
                fv = setValueSlider(ref outRect, "FloatValue2", "  {0:F2}", fv, 
                                           settings.floatVal2Min, settings.floatVal2Max);
                if ( NotEquals(fv, edited.floatVal2) ) {
                    edited.floatVal2= fv;
                    material.SetFloat("_FloatValue2", fv);
                }
            }
            if (mat.hasFloat3) {
                float fv = edited.floatVal3;
                fv = setValueSlider(ref outRect, "FloatValue3", "  {0:F3}", fv, 
                                           settings.floatVal3Min, settings.floatVal3Max);
                if ( NotEquals(fv, edited.floatVal3) ) {
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
            int interval = uiParams.unitHeight; //uiParams.itemHeight; 
            outRect.y += interval;
    
            color.r = drawModValueSlider(outRect, color.r, 0f, 2f, String.Format("{0}:{1:F2}", "R", color.r));
            outRect.y += interval;
            color.g = drawModValueSlider(outRect, color.g, 0f, 2f, String.Format("{0}:{1:F2}", "G", color.g));
            outRect.y += interval;
            color.b = drawModValueSlider(outRect, color.b, 0f, 2f, String.Format("{0}:{1:F2}", "B", color.b));
            outRect.y += interval;
            if (isTrans) {
                color.a = drawModValueSlider(outRect, color.a, 0f, 1f, String.Format("{0}:{1:F2}", "A", color.a));
                outRect.y += interval;
            }
        }

        private float setValueSlider(ref Rect outRect, string label, string format, float val, float min, float max) {
            GUI.Label(outRect, label, uiParams.lStyle);
            int interval = uiParams.unitHeight; //uiParams.itemHeight;
            outRect.y += interval;
    
            val = drawModValueSlider(outRect, val, min, max, String.Format(format, (float)val) );
            outRect.y += interval;
    
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
