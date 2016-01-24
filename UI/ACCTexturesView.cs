using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using CM3D2.AlwaysColorChangeEx.Plugin.Util;

namespace CM3D2.AlwaysColorChangeEx.Plugin.Data
{
    public class ACCTexturesView {
        private static readonly MaidHolder holder = MaidHolder.Instance;
        public static EditTarget editTarget = new EditTarget();
        public static readonly string[] TOON_NAMES = {
            "noTex",
            "toonBlackA1",
            "toonBlueA1",   "toonBlueA2",   "toonBrownA1",
            "toonGrayA1",
            "toonGreenA1",  "toonGreenA2",  "toonOrangeA1",
            "toonPinkA1",   "toonPinkA2",   "toonPurpleA1",
            "toonRedA1",    "toonRedA2",
            "toonYellowA1", "toonYellowA2", "toonYellowA3",
            "toonDress_Shadow",
            "toonFace", "toonFace_Shadow",  "toonFace002",
            "toonSkin", "toonSkin_Shadow",  "toonSkin002",
        };
        private const float EPSILON = 0.00001f;
        private static Settings settings = Settings.Instance;
        static TextureModifier textureModifier = new TextureModifier();

        public static void Init(UIParams uiParams) {
            TextureModifier.uiParams = uiParams;
        }

        public static bool IsChangeTarget() {
            return editTarget.IsValid();
        }

        public static void ClearTarget() {
            editTarget.Clear();
        }

        public static void UpdateTex(Maid maid, Material[] slotMaterials) {
            textureModifier.UpdateTex(maid, slotMaterials, editTarget);
        }
        public static bool IsChangedTexColor(Maid maid, string slot, Material material, string propName) {
            return textureModifier.IsChanged(maid, slot, material, propName);
        }

        public static TextureModifier.FilterParam GetFilter(Maid maid, string slot, Material material, string propName) {
            return textureModifier.GetFilter(maid, slot, material, propName);
        }
        public static Texture2D Filter(Texture2D srcTex, TextureModifier.FilterParam filterParam) {
            return textureModifier.Filter(srcTex, filterParam);
        }

        // TODO メイドが変わると呼び出されるため、保持すべきデータとクリアすべきデータを整理
        // textureModifier上は一部、メイド毎のフィルタデータを保持できる構造
        public static void Clear() {
            textureModifier.Clear();
        }

        // ComboBox用アイテムリスト
        private static GUIContent[] itemNames;
        private static GUIContent[] ItemNames {
            get {
                if (itemNames == null) {
                    itemNames = new GUIContent[TOON_NAMES.Length];
                    int idx = 0;
                    foreach (string name in TOON_NAMES) {
                        itemNames[idx++] = new GUIContent(name, name);
                    }
                }
                return itemNames;
            }
        }

        private static int GetIndex(string rampName) {
            for (int i=0; i< TOON_NAMES.Length; i++) {
                if (TOON_NAMES[i] == rampName) {
                    return i;
                }
            }
            return -1;
        }

        public static List<ACCTexture> Load(Material mate, MaterialType matType) {
            
            if (matType == null) {
                matType = ShaderMapper.resolve(mate.shader.name);
            }

            var ret = new List<ACCTexture>(matType.texPropNames.Length);
            foreach (string propName in matType.texPropNames) {
               ret.Add(new ACCTexture(mate, propName, matType));
            }
            return ret;
        }

        public static FileBrowser fileBrowser;
        private string textureDir;

        int matNo;
        List<ACCTexture> original;
        List<ACCTexture> edited;
        private Dictionary<string, ComboBoxLO> combos = new Dictionary<string, ComboBoxLO>(2);
        readonly UIParams uiParams;
        Material material;
        MaterialType matType;

        public ACCTexturesView(Material m, int matNo, UIParams uiParams) {
            this.uiParams = uiParams;
            this.material = m;
            this.matType = ShaderMapper.resolve(m.shader.name);

            this.original = Load(m, matType);
            this.edited = new List<ACCTexture>(original.Count);
            foreach ( ACCTexture tex in original ) {
                edited.Add(new ACCTexture(tex));
            }

            this.matNo = matNo;
        }

        public void Show() {

            GUILayout.BeginVertical(uiParams.inboxStyle);
            try {
                GUILayout.Label(material.name, uiParams.lStyleC);
                foreach (ACCTexture editTex in edited) {                
                    bool bTargetElement = (matNo == editTarget.matNo && editTex.propName == editTarget.propName);
    
                    GUILayout.BeginHorizontal();
                    try {
                        // エディット用スライダーの開閉
                        if (!textureModifier.IsValidTarget(holder.maid, holder.currentSlot.Name, material, editTex.propName)) {
                            // 参照先が Texture2D が取れなかった (例 : RenderTexture だった) 場合はとりあえず
                            // あきらめて何もしない。無理やり書いても良いのかもしれないけど……
                            var tmp = GUI.enabled;
                            GUI.enabled = false;
                            GUILayout.Button("+変更", uiParams.bStyle, uiParams.buttonLWidth);
                            GUI.enabled = tmp;
                        } else if (bTargetElement) {
                            // エディット中のテクスチャの場合
                            if (GUILayout.Button("-変更", uiParams.bStyle, uiParams.buttonLWidth)) {
                                editTarget.Clear();
                            }
                        } else {
                            // エディット中以外のテクスチャの場合
                            if (GUILayout.Button("+変更", uiParams.bStyle, uiParams.buttonLWidth)) {
                                editTarget.slotName = holder.currentSlot.Name;
                                editTarget.matNo = matNo;
                                editTarget.propName = editTex.propName;
                            }
                        }
                        GUILayout.Label(editTex.propName, uiParams.lStyle);
                        if (bTargetElement && editTex.type.isToony && editTex.propName == "_MainTex") {
                            if ( GUILayout.Button("_ShadowTexに反映", uiParams.bStyle2) ) {
                                // 現在のFilterを_ShadowTexにも反映 
                                textureModifier.DuplicateFilter(holder.maid, holder.currentSlot.Name, material, editTex.propName, "_ShadowTex");
                            }
                        }
    
    
                    } finally {
                        GUILayout.EndHorizontal();
                    }
    
                    // テクスチャエディット用スライダー
                    if (bTargetElement) {
                        textureModifier.ProcGUI(holder.maid, holder.currentSlot.Name, material, editTex.propName);
                    }
        
                    float height = uiParams.itemHeight;
                    ComboBoxLO combo = null;
                    if (editTex.toonType != ACCTexture.NONE) {
                        if (combos.TryGetValue(editTex.propName, out combo)) {
                            if (combo.IsClickedComboButton) {
                                height = combo.ItemCount*uiParams.itemHeight*0.8f;
                            }
                        } else {
                            combo = new ComboBoxLO(new GUIContent("選"), ItemNames, uiParams.bStyle, uiParams.boxStyle, uiParams.listStyle, true);
                            combos[editTex.propName] = combo;
                            combo.SelectItem(editTex.editname);
                        }
                    }
                    GUILayout.BeginHorizontal(GUILayout.Height(height));
                    try {
                        string old = editTex.editname;
                        string editName = GUILayout.TextField(editTex.editname, uiParams.textStyle, uiParams.contentWidth);
                        if (editTex.toonType != ACCTexture.NONE) {
                            // 偽コンボボックス
                            int prevSelected = combo.SelectedItemIndex;
                            int selected = combo.Show(uiParams.buttonWidth);
                            if (selected != prevSelected) {
                                editName = ItemNames[selected].text;
                                // 上記textFieldの更新は次回描画時に任せる
                            }
                        }
                        editTex.SetName(editName);
    
                        GUI.enabled = editTex.dirty;
                        if (GUILayout.Button("適", uiParams.bStyle, uiParams.buttonWidth)) {
                            Texture tex = ChangeTexFile(textureDir, editTex.editname, matNo, editTex.propName);
                            if (tex != null) editTex.tex = tex;
                            editTex.dirty = false;
                        }
                        GUI.enabled = true;
                        if (GUILayout.Button("...", uiParams.bStyle, uiParams.buttonWidth)) {
                            OpenFileBrowser(matNo, editTex);
                        }
                    } finally {
                        GUILayout.EndHorizontal();
                    }
                }
            } finally {
                GUILayout.EndVertical();
            }
        }

        private void OpenFileBrowser(int matNo1, ACCTexture acctex)
        {
            fileBrowser = new FileBrowser(
                new Rect(0, 0, uiParams.fileBrowserRect.width, uiParams.fileBrowserRect.height),
                "テクスチャファイル選択",
                (path) => 
                {
                    fileBrowser = null;
                    if (path == null) return;
    
                    acctex.filepath = textureDir = Path.GetDirectoryName(path);
                    string name = Path.GetFileName(path);
                    if (acctex.editname != name) {
                        acctex.editname = name;
                        acctex.dirty = true;
                    }
                    ChangeTexFile(textureDir, acctex.editname, matNo1, acctex.propName);
    
                });
            fileBrowser.SelectionPatterns = new string[] { "*.tex", "*.png" };
            if (!String.IsNullOrEmpty(textureDir)) {
                fileBrowser.CurrentDirectory = textureDir;
            }
        }

        private Texture ChangeTexFile(string dir, string filename, int matNo1, string propName)
        {
            Texture tex;
            string extension = Path.GetExtension(filename).ToLower();
            if (extension.Length == 0 || extension == ".tex") {
                string texName;
                if (extension.Length == 0) {
                    texName = filename;
                    filename += ".tex";
                } else {
                    texName = filename.Substring(0, filename.Length - 4);
                }
                holder.maid.body0.ChangeTex(holder.currentSlot.Name, matNo1, propName, filename, null, MaidParts.PARTS_COLOR.NONE);

                // ChangeTexは、Materialからロードした時と違い、nameにファイル名が設定されてしまうため、拡張子を除いた名前を再設定
                tex = material.GetTexture(propName);
                if (tex != null) {
                    tex.name = texName;
                }

            } else {

                TBodySkin slot = holder.maid.body0.GetSlot(holder.currentSlot.Name);
                // 直接イメージをロードして適用(要dir指定)
                var mat = holder.GetMaterial(slot, matNo1);
                if (mat == null) return null;

                byte[] img = UTY.LoadImage(Path.Combine(dir, filename));
                var tex2d = new Texture2D(1, 1, TextureFormat.RGBA32, false);
//                var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);                                                 
                tex2d.LoadImage(img);
                // tex.name = filename;
                slot.listDEL.Add(tex2d);
                tex2d.name = Path.GetFileNameWithoutExtension(filename);
                

                mat.SetTexture(propName, tex2d);
                tex = tex2d;
            }

            // テクスチャ変更後は、以前のFilterParamやキャッシュをリセット
            textureModifier.RemoveFilter(holder.maid, holder.currentSlot.Name, material, propName);
            return tex;
        }

        private void MulTexSet(string filename, int matNo1, string propName)
        {
            GameUty.SystemMaterial mat;
            try {
                mat = (GameUty.SystemMaterial)Enum.Parse(typeof(GameUty.SystemMaterial), propName);
            } catch(ArgumentException e) {
                LogUtil.DebugLog(e);
                mat = GameUty.SystemMaterial.Alpha;
            }

            // 合成
            if (Path.GetExtension(filename).ToLower() == ".tex") {
                holder.maid.body0.MulTexSet(holder.currentSlot.Name, matNo1, "_MainTex", 1, filename, mat, false, 0, 0, 0, 0);
                holder.maid.body0.MulTexSet(holder.currentSlot.Name, matNo1, "_ShadowTex", 1, filename, mat, false, 0, 0, 0, 0);
            } else {
                //TODO
            }
        }
    }
}
