using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using CM3D2.AlwaysColorChange.Plugin.Util;

namespace CM3D2.AlwaysColorChange.Plugin.Data
{
    /// <summary>
    /// Description of ACCMaterial.
    /// </summary>
    public class ACCTexture {
        public const int RAMP   = 1;
        public const int SHADOW_RATE = 2;
        public const int NONE = 0;
        public Texture tex;

        public string propName;
        public string name = string.Empty;
        public string filepath;
        public Vector2? texOffset;
        public Vector2? texScale;
        public int toonType;
        public bool dirty;
        
        public ACCTexture(Material mate, string propName) {
            this.propName = propName;

            if (propName == "_ToonRamp" ) {
                toonType = RAMP;
            } else if (propName == "_ShadowRateToon") {
                toonType = SHADOW_RATE;
            }
            this.tex = mate.GetTexture(propName);

            if (tex != null) {
                this.name = tex.name;
                if (tex is Texture2D) {
                   texOffset = mate.GetTextureOffset(propName);
                   texScale  = mate.GetTextureScale(propName);
                }
            } else {
                LogUtil.LogF("tex not found. propname={0}, material={1}", propName, mate.name);
                this.tex = new Texture2D(2, 2);
                // テクスチャを追加セット
                mate.SetTexture(propName, this.tex);
            }
        }        
        public ACCTexture(ACCTexture src) {
            this.propName  = src.propName;

            this.name      = src.name;
            this.filepath  = src.filepath;
            this.texOffset = src.texOffset;
            this.texScale  = src.texScale;

            this.toonType  = src.toonType;
        }
        public bool SetName(string name) {
            if (this.name != name) {
                this.name = name;
                this.dirty = true;
                return true;
            }
            return false;
        }
    }

    public class ACCTexturesView {
        private static readonly MaidHolder holder = MaidHolder.Instance;
        public static EditTarget editTarget = new EditTarget();
        public static readonly string[] RAMP_NAMES = {
            "NoTex",
            "ToonBlackA1",
            "ToonBlueA1",   "ToonBlueA2",   "ToonBrownA1",
            "ToonGrayA1",
            "ToonGreenA1",  "ToonGreenA2",  "ToonOrangeA1",
            "ToonPinkA1",   "ToonPinkA2",   "ToonPurpleA1",
            "ToonRedA1",    "ToonRedA2",
            "ToonYellowA1", "ToonYellowA2", "ToonYellowA3",
            "ToonDress_Shadow",
            "ToonFace", "ToonFace_Shadow",  "ToonFace002",
            "ToonSkin", "ToonSkin_Shadow",  "ToonSkin002",
        };
        private const float EPSILON = 0.00001f;
        private static Settings settings = Settings.Instance;
        static TextureModifier textureModifier = new TextureModifier();

        public static void Init(UIParams uiParams) {
            TextureModifier.lStyle = uiParams.lStyle;
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

        // TODO メイドが変わると呼び出されるため、保持すべきデータとクリアすべきデータを整理
        // textureModifier上は一部、メイド毎のフィルタデータを保持できる構造になっている
        public static void Clear() {
            textureModifier.Clear();
        }

        // ComboBox用アイテムリスト
        private static GUIContent[] rampNames;
        private static GUIContent[] RampNames {
            get {
                if (rampNames == null) {
                    rampNames = new GUIContent[RAMP_NAMES.Length];
                    int idx = 0;
                    foreach (string name in RAMP_NAMES) {
                        rampNames[idx++] = new GUIContent(name, name);
                    }
                }
                return rampNames;
            }
        }

        private static int GetIndex(string rampName) {
            for (int i=0; i< RAMP_NAMES.Length; i++) {
                if (RAMP_NAMES[i] == rampName) {
                    return i;
                }
            }
            return -1;
        }

        public static List<ACCTexture> Load(Material mate, ShaderMapper.MaterialFlag flag) {
            
            if (flag == null) {
                flag = ShaderMapper.resolve(mate.shader.name);
            }

            var ret = new List<ACCTexture>(flag.propNames.Length);
            foreach (string propName in flag.propNames) {
               ret.Add(new ACCTexture(mate, propName));
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
        ShaderMapper.MaterialFlag flag;

        public ACCTexturesView(Material m, int matNo, UIParams uiParams) {
            this.uiParams = uiParams;
            this.material = m;
            this.flag = ShaderMapper.resolve(m.shader.name);

            this.original = Load(m, flag);
            this.edited = new List<ACCTexture>(original.Count);
            foreach ( ACCTexture tex in original ) {
                edited.Add(new ACCTexture(tex));
            }

            this.matNo = matNo;
        }

        public void Show() {
            string shaderName = material.shader.name;
//            ShaderMapper.MaterialFlag mate = ShaderMapper.resolve(shaderName);
//            if (mate == null) continue;

            GUILayout.BeginVertical(uiParams.inboxStyle);
            try {
                GUILayout.Label(material.name, uiParams.lStyleC);
                foreach (ACCTexture tex in edited) {                
                    bool bTargetElement = (matNo == editTarget.matNo && tex.propName == editTarget.propName);
    
                    GUILayout.BeginHorizontal();
                    try {
                        // エディット用スライダーの開閉
                        if (!textureModifier.IsValidTarget(holder.maid, holder.currentSlot.Name, material, tex.propName)) {
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
                                editTarget.propName = tex.propName;
                            }
                        }
                        GUILayout.Label(tex.propName, uiParams.lStyle);
    
    
                    } finally {
                        GUILayout.EndHorizontal();
                    }
    
                    // テクスチャエディット用スライダー
                    if (bTargetElement) {
                        textureModifier.ProcGUI(holder.maid, holder.currentSlot.Name, material, tex.propName, 
                                                uiParams.margin, uiParams.fontSize, uiParams.itemHeight);
                    }
        
                    float height = uiParams.itemHeight;
                    ComboBoxLO combo = null;
                    if (tex.toonType != ACCTexture.NONE) {
                        if (combos.TryGetValue(tex.propName, out combo)) {
                            if (combo.IsClickedComboButton) {
                                height = combo.ItemCount*uiParams.itemHeight*0.8f;
                            }
                        } else {
                            combo = new ComboBoxLO(new GUIContent("選"), RampNames, uiParams.bStyle, uiParams.boxStyle, uiParams.listStyle, true);
                            combos[tex.propName] = combo;
                            combo.SelectItem(tex.name);
                        }
                    }
                    GUILayout.BeginHorizontal(GUILayout.Height(height));
                    try {
                        string old = tex.name;
                        string editName = GUILayout.TextField(tex.name, uiParams.textStyle, uiParams.contentWidth);
                        if (tex.toonType != ACCTexture.NONE) {
                            // 偽コンボボックス
                            int prevSelected = combo.SelectedItemIndex;
                            int selected = combo.Show(uiParams.buttonWidth);
                            if (selected != prevSelected) {
                                editName = RampNames[selected].text;
                                // 上記textFieldの更新は次回描画時に任せる
                            }
                        }
                        tex.SetName(editName);
    
                        GUI.enabled = tex.dirty;
                        if (GUILayout.Button("適", uiParams.bStyle, uiParams.buttonWidth)) {
                            ChangeTexFile(textureDir, tex.name, matNo, tex.propName);
                            tex.dirty = false;
                        }
                        GUI.enabled = true;
                        if (GUILayout.Button("...", uiParams.bStyle, uiParams.buttonWidth)) {
                            OpenFileBrowser(matNo, tex);
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
                    if (acctex.name != name) {
                        acctex.name = name;
                        acctex.dirty = true;
                    }
                    ChangeTexFile(textureDir, acctex.name, matNo1, acctex.propName);
    
                });
            fileBrowser.SelectionPatterns = new string[] { "*.tex", "*.png" };
            if (!String.IsNullOrEmpty(textureDir)) {
                fileBrowser.CurrentDirectory = textureDir;
            }
        }

        private void ChangeTexFile(string dir, string filename, int matNo1, string propName)
        {
            string extension = Path.GetExtension(filename).ToLower();
            if (extension.Length == 0 || extension == ".tex") {
                if (extension.Length == 0) filename += ".tex";
                holder.maid.body0.ChangeTex(holder.currentSlot.Name, matNo1, propName, filename, null, MaidParts.PARTS_COLOR.NONE);

            } else {

                TBodySkin slot = holder.maid.body0.GetSlot(holder.currentSlot.Name);
                // 直接イメージをロードして適用(要dir指定)
                var material = holder.GetMaterial(slot, matNo1);
                if (material == null) return;

                byte[] img = UTY.LoadImage(Path.Combine(dir, filename));
                var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
//                var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);                                                 
                tex.LoadImage(img);
                // tex.name = filename;
                slot.listDEL.Add(tex);

                material.SetTexture(propName, tex);
            }
        }
        private void MulTexSet(string filename, int matNo1, string propName)
        {
            GameUty.SystemMaterial mat;
            try {
                mat = (GameUty.SystemMaterial)Enum.Parse(typeof(GameUty.SystemMaterial), propName);
            } catch(ArgumentException e) {
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
    public class EditTarget
    {
        public string slotName;
        public int matNo;
        public string propName;

        public EditTarget()
        {
            Clear();
        }

        // テクスチャエディット対象を無効にする
        public void Clear()
        {
            slotName = string.Empty;
            matNo = -1;
            propName = string.Empty;
        }
        public bool IsValid() {
            return (matNo >= 0 && slotName.Length != 0 && propName.Length != 0);
        }
    }
}
