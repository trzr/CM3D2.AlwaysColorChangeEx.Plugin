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
            "toonBlueA1",   "toonBlueA2",   "toonBrownA1",
            "toonGrayA1",
            "toonGreenA1",  "toonGreenA2",  "toonOrangeA1",
            "toonPinkA1",   "toonPinkA2",   "toonPurpleA1",
            "toonRedA1",    "toonRedA2",
            "toonYellowA1", "toonYellowA2", "toonYellowA3",
            "toonFace", "toonFace002",
            "toonSkin", "toonSkin002",
            "toonBlackA1",
            "toonFace_Shadow",
            "toonDress_Shadow",
            "toonSkin_Shadow",
        };
        private const float EPSILON = 0.00001f;
        private static Settings settings = Settings.Instance;
        static TextureModifier textureModifier = new TextureModifier();

        public static void Init(UIParams uiparams) {
            TextureModifier.uiParams = uiparams;
            if (uiParams == null) {
                uiParams = uiparams;
                uiParams.Add(updateUI);

                InitUIParams(uiparams);
            }
        }

        // テクスチャキャッシュをクリアする
        // メイドが変わっても保持すべき情報であるため、基本的にはPluginを破棄するタイミングでクリアが望ましい
        public static void Clear() {
            // textureModifierのFilterやテクスチャキャッシュは一部、メイド毎に保持できる構造
            textureModifier.Clear();
            if (uiParams != null) uiParams.Remove(updateUI);
        }
        private static UIParams uiParams;
        private static GUILayoutOption buttonWidth;
        private static GUILayoutOption buttonLWidth;
        private static GUILayoutOption contentWidth;
        private static float comboWidth;
        private static readonly GUIStyle inboxStyle = new GUIStyle("box");
        private static readonly GUIStyle listStyle = new GUIStyle("list");
        private static int fontSize;
        private static int fontSizeS;

        private static void InitUIParams(UIParams uiparam) {
            // 背景設定
            inboxStyle.normal.background = new Texture2D(1, 1);
            var color = new Color(0.3f, 0.3f, 0.3f, 0.3f);
            var colorArray = inboxStyle.normal.background.GetPixels();
            for(int i=0; i<colorArray.Length; i++) {
                colorArray[i] = color;
            }
            inboxStyle.normal.background.SetPixels(colorArray);
            inboxStyle.normal.background.Apply();
            inboxStyle.padding.left = inboxStyle.padding.right = 2;

            // コンボ用リスト
            listStyle.onHover.background = listStyle.hover.background = new Texture2D(2, 2);
            listStyle.padding.left = listStyle.padding.right = 4;
            listStyle.padding.top = listStyle.padding.bottom = 1;
            listStyle.normal.textColor = listStyle.onNormal.textColor =
                listStyle.hover.textColor = listStyle.onHover.textColor =
                listStyle.active.textColor = listStyle.onActive.textColor =
                listStyle.focused.textColor = listStyle.onFocused.textColor = Color.white;
        }

        private static Action<UIParams> updateUI = (uiparams) => {
            float baseWidth = uiparams.textureRect.width - 20;
            buttonWidth  = GUILayout.Width(baseWidth * 0.09f);
            buttonLWidth = GUILayout.Width(baseWidth * 0.2f);
            contentWidth = GUILayout.MaxWidth(baseWidth * 0.69f);

            comboWidth = uiparams.textureRect.width*0.65f;
            fontSize  = uiparams.fontSize;
            fontSizeS = uiparams.fontSizeS;
            listStyle.fontSize = fontSizeS;
        };

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

        private static FileUtilEx outUtil = FileUtilEx.Instance;
        // ComboBox用アイテムリスト
        private static GUIContent[] itemNames;
        private static GUIContent[] ItemNames {
            get {
                if (itemNames == null) {
                    var list = new List<GUIContent>();
                    foreach (string name in TOON_NAMES) {
                        var texfile = name + FileConst.EXT_TEXTURE;
                        Texture2D tex = load(texfile);
                        list.Add(new GUIContent(name, tex, name));
                    }

                    foreach (var texfile in settings.toonTexAddon) {
                        var filename = texfile;
                        if (!texfile.EndsWith(FileConst.EXT_TEXTURE, StringComparison.OrdinalIgnoreCase)) {
                            filename += FileConst.EXT_TEXTURE;
                        }
                        Texture2D tex = load(filename);
                        if (tex != null) {
                            list.Add(new GUIContent(texfile, tex, texfile));
                        }
                    }
                    itemNames = list.ToArray();

                }
                return itemNames;
            }
        }
        private static Texture2D load(string texfile) {
            Texture2D tex = null;
            if (outUtil.Exists(texfile)) {
                tex = outUtil.LoadTexture(texfile);
                CM3D2.AlwaysColorChangeEx.Plugin.Util.TextureScale.Bilinear(tex, 100, 5); // サイズ変更
                //TextureScale.Bilinear(tex, 84, 4);
                //TextureScale.Bilinear(tex, 126, 7);
            }
            return tex;
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
        Material material;
        MaterialType matType;

        public ACCTexturesView(Material m, int matNo) {
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

            GUILayout.BeginVertical(inboxStyle);
            try {
                GUILayout.Label(material.name, uiParams.lStyleC);
                foreach (ACCTexture editTex in edited) {                
                    bool bTargetElement = (matNo == editTarget.matNo && editTex.propName == editTarget.propName);
    
                    GUILayout.BeginHorizontal();
                    try {
                        // エディット用スライダーの開閉
                        if (!textureModifier.IsValidTarget(holder.currentMaid, holder.currentSlot.Name, material, editTex.propName)) {
                            // 参照先が Texture2D が取れなかった (例 : RenderTexture だった) 場合はとりあえず
                            // あきらめて何もしない。無理やり書いても良いのかもしれないけど……
                            var tmp = GUI.enabled;
                            GUI.enabled = false;
                            GUILayout.Button("+変更", uiParams.bStyle, buttonLWidth);
                            GUI.enabled = tmp;
                        } else if (bTargetElement) {
                            // エディット中のテクスチャの場合
                            if (GUILayout.Button("-変更", uiParams.bStyle, buttonLWidth)) {
                                editTarget.Clear();
                            }
                        } else {
                            // エディット中以外のテクスチャの場合
                            if (GUILayout.Button("+変更", uiParams.bStyle, buttonLWidth)) {
                                editTarget.slotName = holder.currentSlot.Name;
                                editTarget.matNo = matNo;
                                editTarget.propName = editTex.propName;
                            }
                        }
                        GUILayout.Label(editTex.propName, uiParams.lStyle);
                        if (bTargetElement && editTex.type.isToony && editTex.propName == "_MainTex") {
                            if ( GUILayout.Button("_ShadowTexに反映", uiParams.bStyleSC) ) {
                                // 現在のFilterを_ShadowTexにも反映 
                                textureModifier.DuplicateFilter(holder.currentMaid, holder.currentSlot.Name, material, editTex.propName, "_ShadowTex");
                            }
                        }
                    } finally {
                        GUILayout.EndHorizontal();
                    }
    
                    // テクスチャエディット用スライダー
                    if (bTargetElement) {
                        textureModifier.ProcGUI(holder.currentMaid, holder.currentSlot.Name, material, editTex.propName);
                    }
        
                    float height = uiParams.itemHeight;
                    ComboBoxLO combo = null;
                    if (editTex.toonType != ACCTexture.NONE) {
                        if (combos.TryGetValue(editTex.propName, out combo)) {
                            if (combo.IsClickedComboButton) {
                                height = combo.ItemCount*uiParams.itemHeight*0.8f;
                            }
                        } else {
                            combo = new ComboBoxLO(new GUIContent("選"), ItemNames, uiParams.bStyle, uiParams.boxStyle, listStyle, true);
                            combo.SetItemWidth(comboWidth);
                            combos[editTex.propName] = combo;
                            combo.SelectItem(editTex.editname);
                        }
                    }
                    GUILayout.BeginHorizontal(GUILayout.Height(height));
                    try {
                        bool hideField = false;
                        string editName = editTex.editname;
                        if (editTex.toonType != ACCTexture.NONE) {
                            // 偽コンボボックス
                            int prevSelected = combo.SelectedItemIndex;
                            int selected = combo.Show(uiParams.optBtnWidth);
                            if (selected != prevSelected) {
                                editName = ItemNames[selected].text;
                            }
                            hideField = combo.IsClickedComboButton;
                        }
                        if (hideField) uiParams.textStyle.fontSize = (int)(fontSizeS*0.8);
                        editName = GUILayout.TextField(editName, uiParams.textStyle, contentWidth);
                        editTex.SetName(editName);

                        if (hideField) uiParams.textStyle.fontSize = fontSize;
                        
    
                        GUI.enabled = editTex.dirty;
                        if (GUILayout.Button("適", uiParams.bStyle, uiParams.optBtnWidth)) {
                            Texture tex = ChangeTexFile(textureDir, editTex.editname, matNo, editTex.propName);
                            if (tex != null) editTex.tex = tex;
                            editTex.dirty = false;
                        }
                        GUI.enabled = true;
                        if (GUILayout.Button("...", uiParams.bStyle, uiParams.optBtnWidth)) {
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
            Texture changedTex;
            // キャッシュ削除用に変更前のテクスチャを取得
            var srcTex = material.GetTexture(propName) as Texture2D;
            string extension = Path.GetExtension(filename).ToLower();
            if (extension.Length == 0 || extension == FileConst.EXT_TEXTURE) {
                string texName;
                if (extension.Length == 0) {
                    texName = filename;
                    filename += FileConst.EXT_TEXTURE;
                } else {
                    texName = filename.Substring(0, filename.Length - 4);
                }
                holder.currentMaid.body0.ChangeTex(holder.currentSlot.Name, matNo1, propName, filename, null, MaidParts.PARTS_COLOR.NONE);

                // ChangeTexは、Materialからロードした時と違い、nameにファイル名が設定されてしまうため、
                // 拡張子を除いた名前を再設定
                changedTex = material.GetTexture(propName);
                if (changedTex != null) {
                    changedTex.name = texName;
                }
            } else {
                TBodySkin slot = holder.currentMaid.body0.GetSlot(holder.currentSlot.Name);
                // 直接イメージをロードして適用(要dir指定)
                var mat = holder.GetMaterial(slot, matNo1);
                if (mat == null) return null;

                byte[] img = UTY.LoadImage(Path.Combine(dir, filename));
                var tex2d = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                tex2d.LoadImage(img);
                slot.listDEL.Add(tex2d);
                // tex以外は拡張子を付与したままとする
                tex2d.name = filename;   //Path.GetFileNameWithoutExtension(filename);

                mat.SetTexture(propName, tex2d);
                changedTex = tex2d;
            }

            // テクスチャ変更後は、以前のFilterParamやキャッシュをリセット
            if (srcTex != null) {
                textureModifier.RemoveCache(srcTex);
                textureModifier.RemoveFilter(holder.currentMaid, holder.currentSlot.Name, material, srcTex);
            }
            return changedTex;
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
            if (filename.EndsWith(FileConst.EXT_TEXTURE, StringComparison.OrdinalIgnoreCase)) {
                holder.currentMaid.body0.MulTexSet(holder.currentSlot.Name, matNo1, "_MainTex", 1, filename, mat, false, 0, 0, 0, 0);
                holder.currentMaid.body0.MulTexSet(holder.currentSlot.Name, matNo1, "_ShadowTex", 1, filename, mat, false, 0, 0, 0, 0);
            } else {
                //TODO
            }
        }
    }
}
