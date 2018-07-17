using System;
using CM3D2.AlwaysColorChangeEx.Plugin.Util;
using UnityEngine;
using UnityEngine.Rendering;

namespace CM3D2.AlwaysColorChangeEx.Plugin.UI.Helper {
    public class CheckboxHelper {
        
        private static GUIContent[] compareFuncs;

        private static GUIContent[] CompareFuncs {
            get {
                if (compareFuncs != null) return compareFuncs;

                var names = Enum.GetNames(typeof(CompareFunction));
                compareFuncs = new GUIContent[names.Length];
                var idx = 0;
                foreach (var name in names) {
                    compareFuncs[idx++] = new GUIContent(name);
                }

                return compareFuncs;
            }
        }

        private readonly UIParams uiParams;
        public CheckboxHelper(UIParams uiparams) {
            uiParams = uiparams;
            uiParams.Add(updateUI);
        }
        ~CheckboxHelper() {
            uiParams.Remove(updateUI);
        }
        private readonly GUIStyle bStyleLeft = new GUIStyle("label");
        private readonly GUIStyle bStyleCenter = new GUIStyle("label");
        private GUILayoutOption optItemHeight;
        public ComboBoxLO compareCombo;

        private void updateUI(UIParams uiparams)  {
            // 幅の28%
            optItemHeight = GUILayout.Height(uiparams.itemHeight);

            bStyleLeft.fontStyle = uiparams.lStyleC.fontStyle;
            bStyleLeft.fontSize = uiparams.fontSize;
            bStyleLeft.normal.textColor = uiparams.lStyleC.normal.textColor;
            bStyleLeft.alignment = TextAnchor.MiddleLeft;

            bStyleCenter.fontStyle = uiparams.lStyleC.fontStyle;
            bStyleCenter.fontSize = uiparams.fontSize;
            bStyleCenter.normal.textColor = uiparams.lStyleC.normal.textColor;
            bStyleCenter.alignment = TextAnchor.MiddleCenter;
        }

        internal void ShowComboBox(string label, EditValue edit, Action<int> func) {
            var idx = (int) edit.val;
            if (ShowComboBox(label, CompareFuncs, ref compareCombo, ref idx, func)) {
                edit.Set(idx);
            }
        }

        internal bool ShowComboBox(string label, GUIContent[] items, ref ComboBoxLO combo, ref int idx,
            Action<int> func) {
            GUILayout.BeginHorizontal();
            try {
                GUILayout.Label(label, uiParams.lStyle, optItemHeight);

                GUILayout.Space(uiParams.marginL);
                if (combo == null) {
                    var selected = (idx >= 0 && idx < items.Length) ? items[idx] : GUIContent.none;
                    combo = new ComboBoxLO(selected, items, uiParams.bStyleSC, uiParams.boxStyle, uiParams.listStyle,
                        false);
                } else {
                    combo.SelectedItemIndex = idx;
                }

                combo.Show(GUILayout.ExpandWidth(true)); //uiParams.optInsideWidth);

                var selectedIdx = combo.SelectedItemIndex;
                if (idx == selectedIdx || selectedIdx == -1) return false;

                idx = selectedIdx;
                func(selectedIdx);
                return true;

            } finally {
                GUILayout.EndHorizontal();
            }
        }

        internal void ShowCheckBox(string label, EditValue edit, Action<float> func) {
            GUILayout.BeginHorizontal();
            try {
                GUILayout.Label(label, uiParams.lStyle, optItemHeight);

                GUILayout.Space(uiParams.marginL);
                var val = edit.val;
                var cont = NumberUtil.Equals(val, 0f)
                    ? ResourceHolder.Instance.Checkoff
                    : ResourceHolder.Instance.Checkon;
                if (!GUILayout.Button(cont, bStyleCenter, GUILayout.Width(50))) return;

                val = 1 - val;
                edit.Set(val);
                func(val);
            } finally {
                GUILayout.EndHorizontal();
            }
        }

        internal bool ShowCheckBox(string label, ref bool val, Action<bool> func) {
            GUILayout.BeginHorizontal();
            try {
                GUILayout.Label(label, uiParams.lStyle, optItemHeight);

                // GUILayout.Space(uiParams.marginL);
                var cont = val ? ResourceHolder.Instance.Checkon : ResourceHolder.Instance.Checkoff;
                if (!GUILayout.Button(cont, bStyleCenter, GUILayout.Width(50))) return false;

                val = !val;
                func(val);
                return true;
            } finally {
                GUILayout.EndHorizontal();
            }
        }
    }
}