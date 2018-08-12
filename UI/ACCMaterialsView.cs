using System;
using UnityEngine;
using CM3D2.AlwaysColorChangeEx.Plugin.Data;
using CM3D2.AlwaysColorChangeEx.Plugin.Util;
using CM3D2.AlwaysColorChangeEx.Plugin.UI.Helper;

namespace CM3D2.AlwaysColorChangeEx.Plugin.UI {
    internal class ACCMaterialsView {
        // ComboBox用アイテムリスト
        private static GUIContent[] shaderNames;

        private static GUIContent[] ShaderNames {
            get {
                if (shaderNames != null) return shaderNames;
//                    CustomShaderHolder.InitShader();
                var length = ShaderType.shaders.Length;
                shaderNames = new GUIContent[length];
                foreach (var shaderType in ShaderType.shaders) {
                    shaderNames[shaderType.idx] = new GUIContent(shaderType.name, shaderType.dispName);
                }

                return shaderNames;
            }
        }

        public static void Init(UIParams uiparams) {
            if (uiParams != null) return;

            uiParams = uiparams;
            uiParams.Add(updateUI);
        }

        public static void Clear() {
            //changeShaders.Clear();

            if (uiParams != null) uiParams.Remove(updateUI);
        }

        private static readonly ResourceHolder resHolder = ResourceHolder.Instance;
        private static GUIContent plusIcon;
        private static GUIContent minusIcon;
        private static GUIContent copyIcon;
        private static GUIContent[] pasteIcons;

        private static GUIContent PlusIcon {
            get { return plusIcon ?? (plusIcon = new GUIContent(resHolder.PlusImage)); }
        }

        private static GUIContent MinusIcon {
            get { return minusIcon ?? (minusIcon = new GUIContent(resHolder.MinusImage)); }
        }

        private static GUIContent CopyIcon {
            get { return copyIcon ?? (copyIcon = new GUIContent("コピー", resHolder.CopyImage, "マテリアル情報をクリップボードへコピーする")); }
        }

        private static GUIContent[] PasteIcons {
            get {
                return pasteIcons ?? (pasteIcons = new[] {
                    new GUIContent("全貼付", resHolder.PasteImage, "クリップボードからマテリアル情報を貼付ける"),
                    new GUIContent("指定貼付", resHolder.PasteImage, "クリップボードからマテリアル情報を貼付ける"),
                });
            }
        }

        private static UIParams uiParams;

        private static readonly GUIStyle bStyleLeft = new GUIStyle("label");
        private static GUILayoutOption optUnitHeight;
        private static GUILayoutOption optButonWidthS;
        private static GUILayoutOption optButonWidth;
        private static GUILayoutOption optIconWidth;

        private static Texture2D Copy(Texture2D src) {
            var dst = new Texture2D(src.width, src.height);
            var pixels = src.GetPixels32();
            for (var i = 0; i < pixels.Length; i++) {
                pixels[i].r = (byte) (pixels[i].r / 2);
                pixels[i].g = (byte) (pixels[i].g / 2);
                pixels[i].b = (byte) (pixels[i].b / 2);
                pixels[i].a = (byte) (pixels[i].a / 2);
            }

            dst.SetPixels32(pixels);
            dst.Apply();
            return dst;
        }

        private static readonly Action<UIParams> updateUI = (uiparams) => {
            // 幅の28%
            optIconWidth = GUILayout.Width(16);
            optButonWidth = GUILayout.Width((uiparams.textureRect.width - 20) * 0.23f);
            optButonWidthS = GUILayout.Width((uiparams.textureRect.width - 20) * 0.20f);
            optUnitHeight = GUILayout.Height(uiparams.unitHeight);

            bStyleLeft.fontStyle = uiparams.lStyleC.fontStyle;
            bStyleLeft.fontSize = uiparams.fontSize;
            bStyleLeft.normal.textColor = uiparams.lStyleC.normal.textColor;
            bStyleLeft.alignment = TextAnchor.MiddleLeft;
        };

        private static bool includeTex;
        private static bool includeShader = true;
        private static bool includeOthers = true;
        internal static readonly RQResolver rqResolver = RQResolver.Instance;

        //public ACCMaterial original;
        private readonly ClipBoardHandler clipHandler = ClipBoardHandler.Instance;
        public readonly ACCMaterial edited;
        public ComboBoxLO shaderCombo;
        public bool expand;
        private int matIdx;
        internal int slotIdx;
        public Action<string> tipsCall;
        internal readonly SliderHelper sliderHelper;
        internal readonly CheckboxHelper cbHelper;

        public ACCMaterialsView(Renderer r, Material m, int slotIdx, int idx, SliderHelper sliderHelper, CheckboxHelper cbHelper) {
            //original = new ACCMaterial(m, r);
            //edited = new ACCMaterial(original);
            this.slotIdx = slotIdx;
            matIdx = idx;
            edited = new ACCMaterial(m, r, idx);
            this.sliderHelper = sliderHelper;
            this.cbHelper = cbHelper;
        }

        public void Show(bool reload) {
            GUILayout.BeginVertical();
            try {
                GUILayout.BeginHorizontal();
                try {
                    var texIcon = expand ? MinusIcon : PlusIcon;
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

                if (edited.type == ShaderType.UNKNOWN) {
                    GUILayout.Label("shader: " + edited.material.shader.name);
                    return;
                }

                GUILayout.BeginHorizontal();
                try {
                    // コピー
                    if (GUILayout.Button(CopyIcon, optUnitHeight, optButonWidthS)) {
                        clipHandler.SetClipboard(MateHandler.Instance.ToText(edited));
                        if (tipsCall != null) {
                            tipsCall("マテリアル情報をクリップボードに\nコピーしました");
                        }
                    }

                    GUI.enabled &= clipHandler.isMateText;
                    var icons = PasteIcons;
                    if (GUILayout.Button(icons[0], optUnitHeight, optButonWidthS)) {
                        try {
                            MateHandler.Instance.Write(edited, clipHandler.mateText);
                            if (tipsCall != null) {
                                tipsCall("マテリアル情報を貼付けました");
                            }
                        } catch (Exception e) {
                            LogUtil.Error("failed to import mateText", e);
                        }
                    }

                    includeOthers = GUILayout.Toggle(includeOthers, "CF", uiParams.tStyleSS);
                    includeShader = GUILayout.Toggle(includeShader, "S", uiParams.tStyleSS);
                    includeTex = GUILayout.Toggle(includeTex, "T", uiParams.tStyleSS);
                    GUI.enabled &= (includeTex | includeShader | includeOthers);
                    if (GUILayout.Button(icons[1], optUnitHeight, optButonWidth)) {
                        try {
                            var pasteFlag = 0;
                            if (includeTex)    pasteFlag |= MateHandler.MATE_TEX;
                            if (includeShader) pasteFlag |= MateHandler.MATE_SHADER;
                            if (includeOthers) pasteFlag |= MateHandler.MATE_COLOR | MateHandler.MATE_FLOAT;
                            LogUtil.DebugF("material pasting from cp... tex={0}, shader={1}, others={2}", includeTex,
                                includeShader, includeOthers);
                            MateHandler.Instance.Write(edited, clipHandler.mateText, pasteFlag);
                        } catch (Exception e) {
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

                var material = edited.material;
                var idx = edited.type.idx;

                if (shaderCombo == null) {
                    var selected = (idx >= 0 && idx < ShaderNames.Length) ? ShaderNames[idx] : GUIContent.none;
                    shaderCombo = new ComboBoxLO(selected, ShaderNames, uiParams.bStyleSC, uiParams.boxStyle, uiParams.listStyle, false);
                } else {
                    shaderCombo.SelectedItemIndex = idx;
                }
                shaderCombo.Show(GUILayout.ExpandWidth(true)); //uiParams.optInsideWidth);

                var selectedIdx = shaderCombo.SelectedItemIndex;
                if (idx != selectedIdx && selectedIdx != -1) {
                    LogUtil.Debug("shader changed", idx, "=>", selectedIdx);

                    // シェーダ変更
                    var shaderName0 = ShaderNames[selectedIdx].text;
                    edited.ChangeShader(shaderName0, selectedIdx);
                }

                // MaterialType mat = edited.type;
                if (reload) {
                    edited.renderQueue.Set(material.renderQueue);
                }

                sliderHelper.SetupFloatSlider("RQ", edited.renderQueue,
                    edited.renderQueue.range.editMin, edited.renderQueue.range.editMax,
                    (rq) => {
                        material.SetFloat(ShaderPropType.RenderQueue.propId, rq);
                        material.renderQueue = (int) rq;
                    },
                    ShaderPropType.RenderQueue.opts,
                    ShaderPropType.RenderQueue.presetVals,
                    rqResolver.Resolve(slotIdx));


                var sdType = edited.type;
                for (var i = 0; i < sdType.colProps.Length; i++) {
                    var colProp = sdType.colProps[i];
                    var editColor = edited.editColors[i];
                    var picker = edited.pickers[i];
                    if (reload) {
                        editColor.Set(material.GetColor(colProp.propId));
//                    } else {
//                        if (!editColor.val.HasValue) {
//                            editColor.Set(colProp.defaultVal);
//                            LogUtil.DebugF("value is empty. set white. color={0}, vals={1}, syncs={2}",
//                                editColor.val, editColor.editVals, editColor.isSyncs);
//                        }
                    }

                    if (sliderHelper.DrawColorSlider(colProp, ref editColor, picker)) {
                        material.SetColor(colProp.propId, editColor.val);
                    }
                }

                for (var i = 0; i < sdType.fProps.Length; i++) {
                    var prop = sdType.fProps[i];
                    if (reload) edited.editVals[i].Set(material.GetFloat(prop.propId));

                    switch (prop.valType) {
                    case ValType.Float:
                        // slider
                        var fprop = prop;
                        // fprop.SetValue(mat, val);
                        sliderHelper.SetupFloatSlider(fprop, edited.editVals[i], (val) => fprop.SetValue(material, val));
                        break;
                    case ValType.Bool:
                        cbHelper.ShowCheckBox(prop.name, edited.editVals[i],
                            (val) => prop.SetValue(material, val));
                        break;
                    case ValType.Enum:
                        cbHelper.ShowComboBox(prop.name, edited.editVals[i],
                            (val) => prop.SetValue(material, val));
                        break;
                    }
                }

            } finally {
                GUILayout.EndVertical();
            }
        }

    }
}
