using System.Collections.Generic;
using System.Linq;
using CM3D2.AlwaysColorChangeEx.Plugin.Data;
using CM3D2.AlwaysColorChangeEx.Plugin.UI.Helper;
using CM3D2.AlwaysColorChangeEx.Plugin.Util;
using UnityEngine;

namespace CM3D2.AlwaysColorChangeEx.Plugin.UI {
    public class ACCPartsColorView {
        private readonly UIParams uiParams;
        private readonly SliderHelper sliderHelper;
        private readonly MaidHolder holder = MaidHolder.Instance;

        private Vector2 scrollViewPosition = Vector2.zero;
        private readonly List<EditParts> editPartColors = new List<EditParts>(); 

        public ACCPartsColorView(UIParams uiParams, SliderHelper sliderHelper) {
            this.uiParams = uiParams;
            this.sliderHelper = sliderHelper;
        }

        public void Update() {
        }

        public void Clear() {
        }

        public void Dispose() {
            //UnityEngine.Object.Destroy(boneRenderer);
        }

        private class EditParts {
            public MaidParts.PartsColor parts;
            public EditColor main   = new EditColor(Color.white, ColorType.rgb, false);
            public bool mainExpand = true;
            public readonly ColorPicker mainPicker = new ColorPicker();
            public EditColor shadow = new EditColor(Color.white, ColorType.rgb, false);
            public bool shadowExpand = true;
            public readonly ColorPicker shadowPicker = new ColorPicker();

            public EditIntValue h = new EditIntValue(0, EditRange.hue);
            public EditIntValue s = new EditIntValue(128, EditRange.saturation);
            public EditIntValue l = new EditIntValue(255, EditRange.light);
            public EditIntValue c = new EditIntValue(100, EditRange.contrast);
            public EditIntValue shadowH = new EditIntValue(0, EditRange.hue);
            public EditIntValue shadowS = new EditIntValue(128, EditRange.saturation);
            public EditIntValue shadowL = new EditIntValue(255, EditRange.light);
            public EditIntValue shadowC = new EditIntValue(100, EditRange.contrast);
            public EditIntValue shadowRate = new EditIntValue(128, EditRange.rate);
            public bool expand;
            public EditParts(ref MaidParts.PartsColor pc) {
                mainPicker.ColorTex = new Texture2D(32, 20, TextureFormat.RGB24, false);
                shadowPicker.ColorTex = new Texture2D(32, 20, TextureFormat.RGB24, false);
                mainPicker.texEdgeSize = 2;
                shadowPicker.texEdgeSize = 2;
                var frameCol = Color.white;
                mainPicker.SetTexColor(ref frameCol, 0);
                shadowPicker.SetTexColor(ref frameCol, 0);
//                h.val = pc.m_nMainHue;
//                s.val = pc.m_nMainChroma;
//                l.val = pc.m_nMainBrightness;
                c.Set(pc.m_nMainContrast);
//                shadowH.Set(pc.m_nShadowHue);
//                shadowS.Set(pc.m_nShadowChroma);
//                shadowL.Set(pc.m_nShadowBrightness);
                shadowC.Set(pc.m_nShadowContrast);
                shadowRate.Set(pc.m_nShadowRate);
                parts = pc;
                SetMain(pc);
                SetShadow(pc);
            }

            public void SetParts(MaidParts.PartsColor parts1) {
                if (HasMainChanged(ref parts1)) SetMain(parts1);
                if (HasShadowChanged(ref parts1)) SetShadow(parts1);

//                if (h.val != parts1.m_nMainHue) h.Set(parts1.m_nMainHue);
//                if (s.val != parts1.m_nMainChroma) s.Set(parts1.m_nMainChroma);
//                if (l.val != parts1.m_nMainBrightness) l.Set(parts1.m_nMainBrightness);
                if (c.val != parts1.m_nMainContrast) c.Set(parts1.m_nMainContrast);

//                if (shadowH.val != parts1.m_nShadowHue) shadowH.Set(parts1.m_nShadowHue);
//                if (shadowS.val != parts1.m_nShadowChroma) shadowS.Set(parts1.m_nShadowChroma);
//                if (shadowL.val != parts1.m_nShadowBrightness) shadowL.Set(parts1.m_nShadowBrightness);
                if (shadowC.val != parts1.m_nShadowContrast) shadowC.Set(parts1.m_nShadowContrast);

                if (shadowRate.val != parts1.m_nShadowRate) shadowRate.Set(parts1.m_nShadowRate);

                parts = parts1;
            }

            public void ReflectMain() {
                var hsla = ColorUtil.RGB2HSL(ref main.val);
                parts.m_nMainHue = (int)(255*hsla.x);
                parts.m_nMainChroma = (int)(255*hsla.y);
                parts.m_nMainBrightness = (int)(510*hsla.z);
            }
            public void ReflectShadow() {
                var hsla = ColorUtil.RGB2HSL(ref shadow.val);
                parts.m_nShadowHue = (int)(255*hsla.x);
                parts.m_nShadowChroma = (int)(255*hsla.y);
                parts.m_nShadowBrightness = (int)(510*hsla.z);
            }

            public void SetMain(MaidParts.PartsColor parts1) {
                var col = ColorUtil.HSL2RGB(parts1.m_nMainHue/255f, 
                    parts1.m_nMainChroma/255f,
                    parts1.m_nMainBrightness/510f, 1f);
                main.Set(col);
                mainPicker.Color = col;
            }

            public void SetShadow(MaidParts.PartsColor parts1) {
                var col2 = ColorUtil.HSL2RGB(parts.m_nShadowHue/255f, 
                    parts.m_nShadowChroma/255f,
                    parts.m_nShadowBrightness/510f, 1f);
                shadow.Set(col2);
                shadowPicker.Color = col2;
            }
            public bool HasChanged(ref MaidParts.PartsColor parts1) {
                return parts.m_nMainBrightness != parts1.m_nMainBrightness
                       || parts.m_nMainChroma != parts1.m_nMainChroma
                       || parts.m_nMainContrast != parts1.m_nMainContrast
                       || parts.m_nMainHue != parts1.m_nMainHue
                       || parts.m_nShadowBrightness != parts1.m_nShadowBrightness
                       || parts.m_nShadowChroma != parts1.m_nShadowChroma
                       || parts.m_nShadowContrast != parts1.m_nShadowContrast
                       || parts.m_nShadowHue != parts1.m_nShadowHue
                       || parts.m_nShadowRate != parts1.m_nShadowRate;
            }
            public bool HasMainChanged(ref MaidParts.PartsColor parts1) {
                return parts.m_nMainBrightness != parts1.m_nMainBrightness
                       || parts.m_nMainChroma != parts1.m_nMainChroma
                       || parts.m_nMainHue != parts1.m_nMainHue;
            }
            public bool HasShadowChanged(ref MaidParts.PartsColor parts1) {
                return parts.m_nShadowBrightness != parts1.m_nShadowBrightness
                       || parts.m_nShadowChroma != parts1.m_nShadowChroma
                       || parts.m_nShadowHue != parts1.m_nShadowHue;
            }
        }

        public void Show() {
            GUILayout.BeginVertical();
            var titleWidth = GUILayout.Width(uiParams.fontSize * 20f);
            var titleHeight = GUILayout.Height(uiParams.titleBarRect.height);
            try {
                GUILayout.BeginHorizontal();
                GUILayout.Label("パートカラー", uiParams.lStyleB, titleWidth, titleHeight);
                GUILayout.EndHorizontal();

                var maid = holder.CurrentMaid;
                if (maid == null) return;
                if (maid.IsBusy) {
                    GUILayout.Space(100);
                    GUILayout.Label("変更中...", uiParams.lStyleB);
                    GUILayout.Space(uiParams.colorRect.height - 105);
                    return;
                }

                var height = uiParams.winRect.height - uiParams.unitHeight - uiParams.margin*2f - uiParams.titleBarRect.height;
                scrollViewPosition = GUILayout.BeginScrollView(scrollViewPosition,
                                                               GUILayout.Width(uiParams.colorRect.width),
                                                               GUILayout.Height(height));
                try {
                    if (!editPartColors.Any()) {
                        for (var pcEnum = MaidParts.PARTS_COLOR.NONE + 1; pcEnum < MaidParts.PARTS_COLOR.MAX; pcEnum++) {
                            var part = maid.Parts.GetPartsColor(pcEnum);
                            editPartColors.Add(new EditParts(ref part));
                        }
                    }

                    for (var pcEnum = MaidParts.PARTS_COLOR.NONE+1; pcEnum < MaidParts.PARTS_COLOR.MAX; pcEnum++) {
                        var part = maid.Parts.GetPartsColor(pcEnum);
                        var idx = (int)pcEnum;
                        var epc = editPartColors[idx];
                        
                        GUILayout.BeginHorizontal();
                        try {
                            if (GUILayout.Button(pcEnum.ToString(), uiParams.lStyleB)) {
                                epc.expand = !epc.expand;
                            }

                            var label = part.m_bUse ? "未使用" : "使用中";
                            GUILayout.Label(label, uiParams.lStyleRS);
                        } finally {
                            GUILayout.EndHorizontal();
                        }
                        if (!epc.expand) continue;

                        if (part.m_nShadowRate != epc.shadowRate.val) epc.shadowRate.Set(part.m_nShadowRate);
                        if (sliderHelper.DrawValueSlider("影率", epc.shadowRate)) {
                            part.m_nShadowRate = epc.shadowRate.val;
                            maid.Parts.SetPartsColor(pcEnum, part);
                            epc.SetParts(part);
                        }

                        if (epc.HasMainChanged(ref part)) {
                            epc.SetMain(part);
                        }
                        if (sliderHelper.DrawColorSlider("色", ref epc.main, SliderHelper.DEFAULT_PRESET, ref epc.mainExpand, epc.mainPicker)) {
                            epc.ReflectMain();
                            maid.Parts.SetPartsColor(pcEnum, epc.parts);
                        }
                        if (part.m_nMainContrast != epc.c.val) epc.c.Set(part.m_nMainContrast);
                        if (sliderHelper.DrawValueSlider("C", epc.c)) {
                            part.m_nMainContrast = epc.c.val;
                            maid.Parts.SetPartsColor(pcEnum, part);
                            epc.SetParts(part);
                        }

                        if (epc.HasShadowChanged(ref part)) {
                            epc.SetShadow(part);
                        }
                        if (sliderHelper.DrawColorSlider("影色", ref epc.shadow, SliderHelper.DEFAULT_PRESET, ref epc.shadowExpand, epc.shadowPicker)) {
                            epc.ReflectShadow();
                            maid.Parts.SetPartsColor(pcEnum, epc.parts);
                        }

//                        if (part.m_nMainHue != epc.h.val) epc.h.Set(part.m_nMainHue);
//                        if (sliderHelper.DrawValueSlider("H", epc.h)) {
//                            part.m_nMainHue = epc.h.val;
//                            maid.Parts.SetPartsColor(pcEnum, part);
//                            epc.SetParts(part);
//                        }
//                        if (part.m_nMainChroma != epc.s.val) epc.s.Set(part.m_nMainChroma);
//                        if (sliderHelper.DrawValueSlider("S", epc.s)) {
//                            part.m_nMainChroma = epc.s.val;
//                            maid.Parts.SetPartsColor(pcEnum, part);
//                            epc.SetParts(part);
//                        }
//                        if (part.m_nMainBrightness != epc.l.val) epc.l.Set(part.m_nMainBrightness);
//                        if (sliderHelper.DrawValueSlider("L", epc.l)) {
//                            part.m_nMainBrightness = epc.l.val;
//                            maid.Parts.SetPartsColor(pcEnum, part);
//                            epc.SetParts(part);
//                        }
//                        if (part.m_nShadowHue != epc.shadowH.val) epc.shadowH.Set(part.m_nShadowHue);
//                        if (sliderHelper.DrawValueSlider("H(影)", epc.shadowH)) {
//                            part.m_nShadowHue = epc.shadowH.val;
//                            maid.Parts.SetPartsColor(pcEnum, part);
//                            epc.SetParts(part);
//                        }
//                        if (part.m_nShadowChroma != epc.shadowS.val) epc.shadowS.Set(part.m_nShadowChroma);
//                        if (sliderHelper.DrawValueSlider("S(影)", epc.shadowS)) {
//                            part.m_nShadowChroma = epc.shadowS.val;
//                            maid.Parts.SetPartsColor(pcEnum, part);
//                            epc.SetParts(part);
//                        }
//                        if (part.m_nShadowBrightness != epc.shadowL.val) epc.shadowL.Set(part.m_nShadowBrightness);
//                        if (sliderHelper.DrawValueSlider("L(影)", epc.shadowL)) {
//                            part.m_nShadowBrightness = epc.shadowL.val;
//                            maid.Parts.SetPartsColor(pcEnum, part);
//                            epc.SetParts(part);
//                        }
                        if (part.m_nShadowContrast != epc.shadowC.val) epc.shadowC.Set(part.m_nShadowContrast);
                        if (sliderHelper.DrawValueSlider("C", epc.shadowC)) {
                            part.m_nShadowContrast = epc.shadowC.val;
                            maid.Parts.SetPartsColor(pcEnum, part);
                            epc.SetParts(part);
                        }
                    }

                } finally {
                    GUI.EndScrollView();
                }
            } finally {
                GUILayout.EndVertical();
            }
        }
    }
}