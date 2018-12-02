using System.Collections.Generic;
using System.Linq;
using CM3D2.AlwaysColorChangeEx.Plugin.Data;
using CM3D2.AlwaysColorChangeEx.Plugin.UI.Data;
using CM3D2.AlwaysColorChangeEx.Plugin.UI.Helper;
using CM3D2.AlwaysColorChangeEx.Plugin.Util;
using UnityEngine;

namespace CM3D2.AlwaysColorChangeEx.Plugin.UI {
    public class ACCPartsColorView : BaseView {
        private readonly SliderHelper sliderHelper;
        private readonly ColorPresetManager presetMgr = ColorPresetManager.Instance;

        private Vector2 scrollViewPosition = Vector2.zero;
        private readonly List<EditParts> editPartColors = new List<EditParts>(); 

        public ACCPartsColorView(UIParams uiParams, SliderHelper sliderHelper) {
            this.uiParams = uiParams;
            this.sliderHelper = sliderHelper;
        }
        private GUILayoutOption titleWidth;
        private GUILayoutOption titleHeight;
        private float viewHeight;
        public override void UpdateUI(UIParams uParams) {
            uiParams = uParams;
            titleWidth = GUILayout.Width(uiParams.fontSize * 20f);
            titleHeight = GUILayout.Height(uiParams.titleBarRect.height);

            viewHeight = uiParams.winRect.height - uiParams.unitHeight - uiParams.margin*2f - uiParams.titleBarRect.height;
        }

        public void Update() {}

        private class EditParts {
            public MaidParts.PartsColor parts;
            public EditColor main   = new EditColor(Color.white, ColorType.rgb, false);
            public bool mainExpand = true;
            public readonly ColorPicker mainPicker;
            public EditColor shadow = new EditColor(Color.white, ColorType.rgb, false);
            public bool shadowExpand = true;
            public readonly ColorPicker shadowPicker;

            public readonly EditIntValue c = new EditIntValue(100, EditRange.contrast);
            public readonly EditIntValue shadowC = new EditIntValue(100, EditRange.contrast);
            public readonly EditIntValue shadowRate = new EditIntValue(128, EditRange.rate);
            public bool expand;
            public EditParts(ref MaidParts.PartsColor pc, ColorPresetManager presetMgr) {
                mainPicker = new ColorPicker(presetMgr) {
                    ColorTex = new Texture2D(32, 20, TextureFormat.RGB24, false),
                    texEdgeSize = 2
                };
                shadowPicker = new ColorPicker(presetMgr) {
                    ColorTex = new Texture2D(32, 20, TextureFormat.RGB24, false),
                    texEdgeSize = 2
                };
                var frameCol = Color.white;
                mainPicker.SetTexColor(ref frameCol, 0);
                shadowPicker.SetTexColor(ref frameCol, 0);

                c.Set(pc.m_nMainContrast);
                shadowC.Set(pc.m_nShadowContrast);
                shadowRate.Set(pc.m_nShadowRate);
                parts = pc;
                SetMain(pc);
                SetShadow(pc);
            }

            public void SetParts(MaidParts.PartsColor parts1) {
                if (HasMainChanged(ref parts1)) SetMain(parts1);
                if (HasShadowChanged(ref parts1)) SetShadow(parts1);

                if (c.val != parts1.m_nMainContrast) c.Set(parts1.m_nMainContrast);

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
                var col2 = ColorUtil.HSL2RGB(parts1.m_nShadowHue/255f,
                    parts1.m_nShadowChroma/255f,
                    parts1.m_nShadowBrightness/510f, 1f);
                shadow.Set(col2);
                shadowPicker.Color = col2;
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
            try {
                GUILayout.BeginHorizontal();
                GUILayout.Label("パーツカラー", uiParams.lStyleB, titleWidth, titleHeight);
                GUILayout.EndHorizontal();

                var maid = holder.CurrentMaid;
                if (maid == null) return;
                if (maid.IsBusy) {
                    GUILayout.Space(100);
                    GUILayout.Label("変更中...", uiParams.lStyleB);
                    GUILayout.Space(uiParams.colorRect.height - 105);
                    return;
                }

                scrollViewPosition = GUILayout.BeginScrollView(scrollViewPosition,
                                                               GUILayout.Width(uiParams.colorRect.width),
                                                               GUILayout.Height(viewHeight));
                try {
                    if (!editPartColors.Any()) {
                        for (var pcEnum = MaidParts.PARTS_COLOR.NONE + 1; pcEnum < MaidParts.PARTS_COLOR.MAX; pcEnum++) {
                            var part = maid.Parts.GetPartsColor(pcEnum);
                            editPartColors.Add(new EditParts(ref part, presetMgr));
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
                        if (epc.HasShadowChanged(ref part)) {
                            epc.SetShadow(part);
                        }
                        if (part.m_nMainContrast != epc.c.val) epc.c.Set(part.m_nMainContrast);
                        if (sliderHelper.DrawValueSlider("主C", epc.c)) {
                            part.m_nMainContrast = epc.c.val;
                            maid.Parts.SetPartsColor(pcEnum, part);
                            epc.SetParts(part);
                        }

                        if (part.m_nShadowContrast != epc.shadowC.val) epc.shadowC.Set(part.m_nShadowContrast);
                        if (sliderHelper.DrawValueSlider("影C", epc.shadowC)) {
                            part.m_nShadowContrast = epc.shadowC.val;
                            maid.Parts.SetPartsColor(pcEnum, part);
                            epc.SetParts(part);
                        }

                        if (sliderHelper.DrawColorSlider("主色", ref epc.main, SliderHelper.DEFAULT_PRESET, ref epc.mainExpand, epc.mainPicker)) {
                            epc.ReflectMain();
                            maid.Parts.SetPartsColor(pcEnum, epc.parts);
                        }

                        if (sliderHelper.DrawColorSlider("影色", ref epc.shadow, SliderHelper.DEFAULT_PRESET, ref epc.shadowExpand, epc.shadowPicker)) {
                            epc.ReflectShadow();
                            maid.Parts.SetPartsColor(pcEnum, epc.parts);
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
