using System;
using CM3D2.AlwaysColorChangeEx.Plugin.Data;
using CM3D2.AlwaysColorChangeEx.Plugin.Render;
using CM3D2.AlwaysColorChangeEx.Plugin.UI.Helper;
using CM3D2.AlwaysColorChangeEx.Plugin.Util;
using UnityEngine;

namespace CM3D2.AlwaysColorChangeEx.Plugin.UI {
    public class ACCBoneSlotView {
        private readonly UIParams uiParams;
        private readonly CustomBoneRenderer boneRenderer;
        private readonly SliderHelper sliderHelper;
        private readonly GUIColorStore colorStore = new GUIColorStore();
        private readonly MaidHolder holder = MaidHolder.Instance;
        private readonly string[] slotNames;

        private EditColor editColor = new EditColor(Color.white, ColorType.rgba, EditColor.RANGE, EditColor.RANGE);
        private bool editExpand;
        private Vector2 scrollViewPosition = Vector2.zero;
        private int selectedSlotID;
        private bool boneVisible;
        private bool skipEmptySlot = true;
        private readonly ColorPicker picker;

        public ACCBoneSlotView(UIParams uiParams, SliderHelper sliderHelper) {
            this.uiParams = uiParams;
            boneRenderer = new CustomBoneRenderer();
            this.sliderHelper = sliderHelper;
            picker = new ColorPicker(ColorPresetManager.Instance) {
                ColorTex = new Texture2D(32, uiParams.itemHeight, TextureFormat.RGB24, false)
            };
            var col = Color.white;
            picker.SetTexColor(ref col);

            slotNames = CreateSlotNames();
        }

        public void Update() {
            boneRenderer.Update();
        }

        public void Clear() {
            boneRenderer.Clear();
        }

        public void Dispose() {
            boneRenderer.Clear();
            //UnityEngine.Object.Destroy(boneRenderer);
        }

        public void Show() {
            GUILayout.BeginVertical();
            var titleWidth = GUILayout.Width(uiParams.fontSize * 20f);
            var titleHeight = GUILayout.Height(uiParams.titleBarRect.height);
            try {
                GUILayout.BeginHorizontal();
                GUILayout.Label("ボーン表示用スロット 選択", uiParams.lStyleB, titleWidth, titleHeight);
                GUILayout.EndHorizontal();

                var maid = holder.CurrentMaid;
                if (maid == null) return;
                if (maid.IsBusy) {
                    GUILayout.Space(100);
                    GUILayout.Label("変更中...", uiParams.lStyleB);
                    GUILayout.Space(uiParams.colorRect.height - 105);
                    return;
                }

                if (boneRenderer.IsEnabled()) {
                    // 選択メイドが変更された場合に一旦クリア
                    if (boneRenderer.TargetId != maid.GetInstanceID()) {
                        boneRenderer.Clear();
                        boneVisible = false;
                    }
                }
                if (sliderHelper.DrawColorSlider("色設定", ref editColor, SliderHelper.DEFAULT_PRESET, ref editExpand, picker)) {
                    boneRenderer.Color = editColor.val;
                }

                if ((int)TBody.SlotID.end - 1 > maid.body0.goSlot.Count) return;

                var width = uiParams.colorRect.width - 30;
                var offset = editExpand ? uiParams.unitHeight * 5f : uiParams.itemHeight-uiParams.margin*3f;
                if (editExpand && picker.expand) offset += ColorPicker.LightTex.height + uiParams.margin*2f;
                var height = uiParams.winRect.height - uiParams.itemHeight * 3f - offset - uiParams.titleBarRect.height;
                var toggleWidth = GUILayout.Width(width * 0.4f);
                var otherWidth = GUILayout.Width(width * 0.6f);
                GUILayout.BeginHorizontal();
                try {
                    GUI.enabled = selectedSlotID != -1 && boneRenderer.IsEnabled();
                    string buttonText;
                    if (boneVisible) {
                        colorStore.SetColor(Color.white, Color.green);
                        buttonText = "ボーン表示";
                    } else {
                        buttonText = "ボーン非表示";
                    }
                    if (GUILayout.Button(buttonText, uiParams.bStyle, uiParams.optBtnHeight, toggleWidth)) {
                        boneVisible = !boneVisible;
                        boneRenderer.SetVisible(boneVisible);
                        var slot = maid.body0.goSlot[selectedSlotID];
                        if (boneVisible && !boneRenderer.IsEnabled() && slot != null && slot.obj != null) {
                            boneRenderer.Setup(slot.obj, slot.RID);
                            boneRenderer.TargetId = maid.GetInstanceID();
                        }
                    }
                    colorStore.Restore();
                    GUI.enabled = true;
                    if (skipEmptySlot) {
                        colorStore.SetColor(Color.white, Color.green);
                        buttonText = "空スロット省略";
                    } else {
                        buttonText = "全スロット表示";
                    }
                    if (GUILayout.Button(buttonText, uiParams.bStyle, uiParams.optBtnHeight, toggleWidth)) {
                        skipEmptySlot = !skipEmptySlot;
                    }
                    colorStore.Restore();
                } finally {
                    GUILayout.EndHorizontal();
                }
                scrollViewPosition = GUILayout.BeginScrollView(scrollViewPosition,
                                                               GUILayout.Width(uiParams.colorRect.width),
                                                               GUILayout.Height(height));
                try {
                    for (var i = 0; i < slotNames.Length; i++) {
                        var slotItem = maid.body0.goSlot[i];
                        var slotEnabled = (slotItem.obj != null && slotItem.morph != null);
                        if (skipEmptySlot && !slotEnabled) continue;

                        GUILayout.BeginHorizontal();
                        try {
                            GUI.enabled = slotEnabled;
                            var selected = (i == selectedSlotID);
                            var toggleOn = GUILayout.Toggle(selected, slotNames[i], uiParams.tStyleS, toggleWidth);
                            if (toggleOn) {
                                if (selected) {
                                    if (slotItem.obj != null) {
                                        // 既に選択済の場合、アイテム変更チェック
                                        if (boneRenderer.ItemID != slotItem.RID) {
                                            boneRenderer.Setup(slotItem.obj, slotItem.RID);
                                            boneRenderer.SetVisible(boneVisible);
                                            boneRenderer.TargetId = maid.GetInstanceID();
                                        }
                                    } else {
                                        boneVisible = false;
                                    }
                                } else {
                                    selectedSlotID = i;
                                    if (slotItem.obj != null) {
                                        boneRenderer.Setup(slotItem.obj, slotItem.RID);
                                        boneRenderer.SetVisible(boneVisible);
                                        boneRenderer.TargetId = maid.GetInstanceID();
                                    }
                                }
                            }
                            GUI.enabled = true;

                            if (slotEnabled) {
                                var modelName = slotItem.m_strModelFileName ?? string.Empty;
                                GUILayout.Label(modelName, uiParams.lStyleS, otherWidth);
                            }
                        } finally {
                            GUILayout.EndHorizontal();
                        }
                    }

                } finally {
                    GUI.EndScrollView();
                }
            } finally {
                GUILayout.EndVertical();
            }
        }
        
        private string[] CreateSlotNames() {
            var allSlotNames = Enum.GetNames(typeof(TBody.SlotID));
            const int max = (int)TBody.SlotID.moza;
            var items = new string[max];
            var idx = 0;
            foreach (var slot in allSlotNames) {
                if (idx >= max) break;
                items[idx++] = slot;
            }

            if (LogUtil.IsDebug()) {
                LogUtil.Debug("slotNames:", items.ToString());
            }
            return items;
        }
    }
}