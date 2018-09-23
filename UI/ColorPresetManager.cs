using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CM3D2.AlwaysColorChangeEx.Plugin.Util;
using UnityEngine;

namespace CM3D2.AlwaysColorChangeEx.Plugin.UI {
    public class ColorPresetManager {
        #region Static Fields/Properties
        public static readonly ColorPresetManager Instance = new ColorPresetManager();
        private static Texture2D presetBaseIcon;
        public static Texture2D PresetBaseIcon {
            get {
                if (presetBaseIcon == null) {
                    presetBaseIcon = ResourceHolder.Instance.LoadTex("preset_base");
                }
                return presetBaseIcon;
            }
        }
        private static Texture2D presetEmptyIcon;
        public static Texture2D PresetEmptyIcon {
            get {
                if (presetEmptyIcon == null) {
                    presetEmptyIcon = ResourceHolder.Instance.LoadTex("preset_empty");
                }
                return presetEmptyIcon;
            }
        }
        private static Texture2D presetFocusIcon;
        public static Texture2D PresetFocusIcon {
            get {
                if (presetFocusIcon == null) {
                    presetFocusIcon = ResourceHolder.Instance.LoadTex("preset_focus");
                }
                return presetFocusIcon;
            }
        }
        #endregion

        public readonly List<Texture2D> presetIcons = new List<Texture2D>();
        public readonly List<string> presetCodes = new List<string>();
        
        private GUIStyle iconStyle;
        public GUIStyle IconStyle {
            get {
                return iconStyle ?? (iconStyle = new GUIStyle("label") {
                    contentOffset = new Vector2(0, 1),
                    margin = new RectOffset(1, 1, 1, 1),
                    padding = new RectOffset(1, 1, 1, 1)
                });
            }
            set { iconStyle = value; }
        }

        private GUILayoutOption btnWidth;
        public GUILayoutOption BtnWidth {
            get { return btnWidth ?? (btnWidth = GUILayout.Width(BtnStyle.CalcSize(new GUIContent("Delete")).x)); }
            set { btnWidth = value; }
        }
        private GUIStyle btnStyle;
        public GUIStyle BtnStyle {
            get {
                return btnStyle ?? (btnStyle = new GUIStyle("button") {
                    margin = new RectOffset(2, 2, 1, 1),
                    padding = new RectOffset(1, 1, 1, 1),
                    alignment = TextAnchor.MiddleCenter,
                });
            }
            set { btnStyle = value; }
        }

        public int Count = 20;
        public string PresetPath { get; private set; }

        /// <summary>
        /// カラープリセットのCSVファイルパスを設定する.
        /// 設定しない場合はロードも保存もできない.
        /// </summary>
        /// <param name="path">CSVファイルパス</param>
        public void SetPath(string path) {
            PresetPath = path;
            Load();
        }

        /// <summary>
        /// 指定した位置の色情報が有効であるか判断する.
        /// カラーコードの書式の正しさはチェックしない
        /// </summary>
        /// <param name="idx">位置</param>
        /// <returns></returns>
        public bool IsValid(int idx) {
            if (0 <= idx && idx < presetCodes.Count) {
                return presetCodes[idx].Length > 0;
            }

            return false;
        }

        public void ClearColor(int idx) {
            if (idx < 0 && presetCodes.Count <= idx) return;

            presetCodes[idx] = string.Empty;
            presetIcons[idx].SetPixels32(PresetEmptyIcon.GetPixels32(0), 0);
            presetIcons[idx].Apply();

            Save();
        }

        public void SetColor(int idx, string code, ref Color col) {
            presetCodes[idx] = code;
            SetTexColor(ref col, PresetBaseIcon, presetIcons[idx]);
            Save();
        }

        public void SetTexColor(ref Color col, Texture2D srcTex, Texture2D dstTex) {
            var pixels = srcTex.GetPixels32(0);
            for (var i = 0; i< pixels.Length; i++) {
                if (pixels[i].a > 0f) {
                    pixels[i] = col;
                }
            }
            dstTex.SetPixels32(pixels, 0);
            dstTex.Apply();
        }

        public bool Load() {
            if (PresetPath == null) return false;

            presetCodes.Clear();
            presetIcons.Clear();

            var load = false;
            var empty = PresetEmptyIcon;
            if (File.Exists(PresetPath)) {
                try {
                    var presets = File.ReadAllText(PresetPath, Encoding.UTF8);
                    var codes = presets.Split(',');
                    foreach (var code in codes) {
                        var trimmedCode = code.Trim();
                        var col = ColorPicker.GetColor(trimmedCode);
                        Texture2D tex;
                        if (col.a > 0f) {
                            presetCodes.Add(trimmedCode);
                            var baseTex = PresetBaseIcon;
                            tex = new Texture2D(baseTex.width, baseTex.height, baseTex.format, false);
                            SetTexColor(ref col, baseTex, tex);
                        } else {
                            presetCodes.Add(string.Empty);
                            tex = CreateEmpty();
                        }

                        presetIcons.Add(tex);
                        if (presetIcons.Count >= Count) break;
                    }

                    load = true;
                } catch (Exception e) {
                    LogUtil.Error("カラープリセットのロードに失敗しました。", PresetPath, e);
                }
            }
            for (var i = presetIcons.Count; i < Count; i++) {
                var tex = new Texture2D(empty.width, empty.height, empty.format, false);
                tex.SetPixels32(empty.GetPixels32(0), 0);
                tex.Apply();
                presetIcons.Add(tex);
                presetCodes.Add(string.Empty);
            }

            return load;
        }

        private Texture2D CreateEmpty() {
            var empty = PresetEmptyIcon;
            var tex = new Texture2D(empty.width, empty.height, empty.format, false);
            tex.SetPixels32(empty.GetPixels32(0), 0);
            tex.Apply();
            return tex;
        }

        public bool Save() {
            if (PresetPath == null) return false;

            try {
                using (var writer = new StreamWriter(PresetPath, false, Encoding.UTF8, 8192)) {
                    foreach (var code in presetCodes) {
                        writer.Write(code);
                        writer.Write(',');
                    }
                }
            } catch (IOException e) {
                LogUtil.Error("カラープリセットの保存に失敗しました。", PresetPath, e);
                return false;
            }

            LogUtil.Debug("save to color preset:", PresetPath);
            return true;
        }
    }
}
