/*
 * 保存ダイアログ
 */
using System;
using UnityEngine;
using CM3D2.AlwaysColorChange.Plugin.Data;

namespace CM3D2.AlwaysColorChange.Plugin.UI
{
    public class ACCMenu {
        public string menufile;
        public string icon;
        public string slot;
        public string priority;
        public string name;
        public string desc;
    }

    /// <summary>
    /// Description of ACCSaveModView.
    /// </summary>
    public class ACCSaveModView
    {
//        private static Dictionary<int, Shader> changeShaders = new Dictionary<int, Shader>();
        private static Settings settings = Settings.Instance;
        // ComboBox用アイテムリスト
//        private static GUIContent[] shaderNames;
//        private static GUIContent[] ShaderNames {
//            get {
//                if (shaderNames == null) {
//                    shaderNames = new GUIContent[ShaderMapper.ShaderNames.Length];
//                    int idx = 0;
//                    foreach (ShaderMapper.ShaderName shaderName in ShaderMapper.ShaderNames) {
//                        shaderNames[idx++] = new GUIContent(shaderName.Name, shaderName.DisplayName);
//                    }
//                }
//                return shaderNames;
//            }
//        }

        private static int GetIndex(string shaderName) {
            ShaderMapper.ShaderName[] names = ShaderMapper.ShaderNames;
            for (int i=0; i< names.Length; i++) {
                if (names[i].Name == shaderName) {
                    return i;
                }
            }
            return -1;
        }
        public static void Clear() {
//            changeShaders.Clear();
        }

        public ComboBox shaderCombo;
        readonly UIParams uiParams;
        public bool showDialog;

        public ACCMenu target = new ACCMenu();
        public ACCSaveModView(UIParams uiParams) {
            this.uiParams = uiParams;
        }
        public void Show() {
            GUILayout.BeginVertical();
            try {
                GUILayout.BeginHorizontal();
                GUILayout.Label("メニュー", uiParams.lStyle, uiParams.modalLabelWidth);
                target.menufile = GUILayout.TextField(target.menufile, uiParams.textStyle);
                GUILayout.EndHorizontal();
                
                GUILayout.BeginHorizontal();
                GUILayout.Label("アイコン", uiParams.lStyle, uiParams.modalLabelWidth);
                target.icon = GUILayout.TextField(target.icon, uiParams.textStyle);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("名前", uiParams.lStyle, uiParams.modalLabelWidth);
                target.name = GUILayout.TextField(target.name, uiParams.textStyle);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("説明", uiParams.lStyle, uiParams.modalLabelWidth);
                target.desc = GUILayout.TextField(target.desc, uiParams.textAreaStyle);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("menu優先度", uiParams.lStyle, uiParams.modalLabelWidth);
                var edited = GUILayout.TextField(target.priority, uiParams.textAreaStyle);
                if (target.priority != edited ) {
                    // float?
                    int v;
                    if (int.TryParse(edited, out v)) {
                        if (v >= 0) {
                            target.priority = v.ToString();
                        }
                    }
                }
                GUILayout.EndHorizontal();

                string gname = "マテリアル";
                GUILayoutUtility.BeginGroup(gname);
                try {
                    
                } finally {
                    GUILayoutUtility.EndGroup(gname);
                }

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("保存", uiParams.bStyle)) {
                    SaveMod();
                }
                if (GUILayout.Button("閉じる", uiParams.bStyle)) {
                    showDialog = false;
                }
                GUILayout.EndHorizontal();


            } finally {
                GUILayout.EndVertical();
            }
        }
        private bool SaveMod()
        {

            return true;
        }
    }
}
