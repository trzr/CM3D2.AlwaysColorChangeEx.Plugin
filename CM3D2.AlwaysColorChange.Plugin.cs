using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityInjector.Attributes;
using CM3D2.AlwaysColorChange.Plugin.Data;
using CM3D2.AlwaysColorChange.Plugin.Util;

// 以下の AssemblyVersion は削除しないこと
[assembly: AssemblyVersion("1.0.*")]

namespace CM3D2.AlwaysColorChange.Plugin {
[PluginFilter("CM3D2x64"),
 PluginFilter("CM3D2x86"),
 PluginFilter("CM3D2VRx64"),
 PluginName("CM3D2 AlwaysColorChangeMod"),
 PluginVersion("0.0.4.3")]
class AlwaysColorChange : UnityInjector.PluginBase
{
#if DYNAMIC_PLUGIN
    public DynAlwaysColorChange()
    {
        Console.WriteLine("{0} : ctor()", GetType().Name);
    }

    public override void OnPluginLoad()
    {
        Console.WriteLine("{0} : OnPluginLoad()", GetType().Name);
        Awake();
    }

    public override void OnPluginUnload()
    {
        Console.WriteLine("{0} : OnPluginUnload()", GetType().Name);
//        if (goMainPanel != null) {
//            goMainPanel.SetActive(false);
//        }
        OnDestroy();
    }
#endif

    #region Constants
    public const string Version = "0.0.4.3";
    public const string PlugiName = "AlwaysColorChangeMod";

    private const float GUIWidth = 0.25f;
    private const int marginPx = 4;
    private const int fontPx = 14;
    private const int itemHeightPx = 20;
    #endregion

    private enum TargetLevel {
        // ダンス:ドキドキ☆Fallin' Love
        SceneDance_DDFL = 4,
        // エディット
        SceneEdit = 5,
        // 夜伽
        SceneYotogi = 14,
        // ADVパート
        SceneADV = 15,
        // ダンス:entrance to you
        SceneDance_ETYL = 20,
        // ダンス：scarlet leap
        SceneDance_SCL_Leap = 21
    }

    private enum MenuType {
        None,
        Main,
        Color,
        NodeSelect,
        Save,
        PresetSelect,
        Texture
    }

    #region Variables
    private float fPassedTimeOnLevel = 0f;
    private float fLastInitTime      = 0f;
    private bool initialized         = false;
//    private int sceneLevel;
//    private bool visible;

    private bool cmrCtrlChanged = false;
    private bool showSaveDialog = false;

    private Settings settings = Settings.Instance;

    private PresetManager presetMgr = new PresetManager();
    private UIParams uiParams = new UIParams();
    private MaidHolder holder = MaidHolder.Instance;
   
    private MenuType menuType;
    private SlotInfo currentSlot;

    private Dictionary<int, Shader> changeShaders = new Dictionary<int, Shader>();
    private Dictionary<string, bool> dDelNodes = new Dictionary<string, bool>();
    private Dictionary<string, CCPreset> presets;

    private string presetName = "";
    private bool bApplyChange = false;
    private MenuInfo targetMenuInfo;
    private CCPreset targetPreset;
    private TextureModifier textureModifier;

    private bool bClearMaskEnable = false;
    private bool bSaveBodyPreset  = false;

    private int targetMatno;
    private string texturePath;
    private string targetPropName;
    private Dictionary<int, Dictionary<string, string>> textureFile;
    private FileBrowser fileBrowser;

    TextureEdit textureEdit = new TextureEdit();
    private bool isActive;
    private string SaveFileName;

    private Vector2 scrollViewVector = Vector2.zero;

    #endregion

    #region MonoBehaviour methods
    public void Awake() {
        settings.configPath = DataPath;
        base.ReloadConfig();
        settings.Load((key) => {
              return base.Preferences["Config"][key].Value;
        });

        SaveFileName = Path.Combine(settings.configPath , "AlwaysColorChange.xml");
        LogUtil.Log("SaveFileName", SaveFileName);

        textureModifier = new TextureModifier();
        LoadPresets();
    }

    public void OnDestroy() {
        dispose();
    }

    public void OnLevelWasLoaded(int level) {

        fPassedTimeOnLevel = 0f;
        if (!Enum.IsDefined(typeof(TargetLevel), level)) {
            // active -> disactive 
            if (isActive) {
                dispose();
                isActive = false;
            }
            return;
        }

        menuType = MenuType.None;
        bApplyChange = false;
        isActive = true;
    }

    public void OnGUI()
    {
        if (!isActive) return;

        if (menuType == MenuType.None) return;

        if (Event.current.type == EventType.Layout) {
            if (fileBrowser != null) {
                uiParams.fileBrowserRect = GUI.Window(14, uiParams.fileBrowserRect, DoFileBrowser, Version, uiParams.winStyle);
            } else {
                switch (menuType) {
                    case MenuType.Main:
                        uiParams.winRect = GUI.Window(12, uiParams.winRect, DoMainMenu, Version, uiParams.winStyle);
                        break;
                    case MenuType.Color:
                        uiParams.winRect = GUI.Window(12, uiParams.winRect, DoColorMenu, Version, uiParams.winStyle);
                        break;
                    case MenuType.NodeSelect:
                        uiParams.winRect = GUI.Window(12, uiParams.winRect, DoNodeSelectMenu, Version, uiParams.winStyle);
                        break;
                    case MenuType.Save:
                        uiParams.winRect = GUI.Window(12, uiParams.winRect, DoSaveMenu, Version, uiParams.winStyle);
                        break;
                    case MenuType.PresetSelect:
                        uiParams.winRect = GUI.Window(12, uiParams.winRect, DoSelectPreset, Version, uiParams.winStyle);
                        break;
                    case MenuType.Texture:
                        uiParams.winRect = GUI.Window(12, uiParams.winRect, DoSelectTexture, Version, uiParams.winStyle);
                        break;
                    default:
                        break;
                }
            }
    
            if (showSaveDialog) {
                uiParams.InitModalRect();
                uiParams.modalRect = GUI.ModalWindow(13, uiParams.modalRect, DoSaveModDialog, "保存");
            }
        }
    }
    public void Update()
    {
        fPassedTimeOnLevel += Time.deltaTime;
//        if (!Enum.IsDefined(typeof(TargetLevel), Application.loadedLevel)) return;
        if (!isActive) return;

        if (!initialized && (fPassedTimeOnLevel - fLastInitTime > 1f)) {
            fLastInitTime = fPassedTimeOnLevel;
            initialized = initialize();
            LogUtil.Log("Initialize ", initialized);
        }
        if (!initialized) return ;

        if (Input.GetKeyDown(settings.toggleKey)) {
            menuType = (menuType == MenuType.None)? MenuType.Main : MenuType.None;
        }
        if (Input.GetKeyDown(KeyCode.Alpha0)) {
            if (Debug.isDebugBuild) {
                if (dDelNodes.Any()) {
                    var keyList = new List<string>(dDelNodes.Keys);
                    foreach (string key in keyList) {
                        LogUtil.DebugLog(key);
                    }
                }
            }
        }

        bool isEnableControl = false;
        if (menuType != MenuType.None) {
            var cursor = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
            isEnableControl = uiParams.winRect.Contains(cursor);
        }

        // カメラコントロールの有効化/無効化 (Windowの範囲外では、自身がコントロールを変更したケース以外は更新しない)
        if (isEnableControl) {
            if (GameMain.Instance.MainCamera.GetControl()) {
                GameMain.Instance.MainCamera.SetControl(false);
                UICamera.InputEnable = false;
                cmrCtrlChanged = true;
            }
        } else {
            if (cmrCtrlChanged) {
                GameMain.Instance.MainCamera.SetControl(true);
                UICamera.InputEnable = true;
                cmrCtrlChanged = false;
            }
        }

        if (bApplyChange && !holder.maid.boAllProcPropBUSY) {
            ApplyPreset();
        }

        // テクスチャエディットの反映
        if (menuType != MenuType.Texture) {
            // テクスチャモードでなければ、テクスチャ変更対象を消す
            textureEdit.Clear();

        } else {
            // 各スロットのマテリアルの列挙
            var slotMaterials = new Dictionary<string, List<Material>>();
            foreach (SlotInfo slot in ACConstants.SlotNames.Values) {
                slotMaterials[slot.Name] = holder.GetMaterials(slot);
            }

            // マテリアルで使用されている全テクスチャの列挙
            var textures = new List<Texture2D>();
            foreach (List<Material> materials in slotMaterials.Values) {
                foreach (Material material in materials) {
                    // TODO material に対応するpropNamesに変更
                    string shaderName = material.shader.name;
                    ShaderMapper.MaterialFlag mate = ShaderMapper.resolve(shaderName);
                    if (mate == null) continue;

                    foreach (string propName in mate.propNames) {
                        var tex2d = material.GetTexture(propName) as Texture2D;
                        if (tex2d != null) {
                            textures.Add(tex2d);
                        }
                    }
                }
            }
            textureModifier.Update(
                holder.maid,
                slotMaterials,
                textures,
                textureEdit.slotName,
                textureEdit.materialIndex,
                textureEdit.propName);
        }
    }
    #endregion

    #region Private methods
    private void dispose() {
//        goMainPanel = null;
//        visible = false;

        // テクスチャキャッシュを開放する
        textureModifier.Clear();
        changeShaders.Clear();
        dDelNodes.Clear();
        if (presets != null) {
            presets.Clear();
        }

        initialized = false;
    }

    private bool initialize() {
        LogUtil.DebugLog("Initialize ",  Application.loadedLevel);

        holder.maid = GameMain.Instance.CharacterMgr.GetMaid(0);
        if (holder.maid == null) return false;

        InitDelNode();
//            dMaterial.Clear();
//            foreach (string slotname in ACConstants.Slotnames.Keys) {
//                dMaterial.Add(slotname, new CCMaterial());
//            }

        uiParams.Update();
//        uiParams.InitWinRect();
//        uiParams.InitFBRect();

        return initModsSliderNGUI();
    }

    private bool initModsSliderNGUI() {
        return true;
    }
    #endregion


    class TextureEdit
    {
        public string slotName;
        public int materialIndex;
        public string propName;

        public TextureEdit()
        {
            Clear();
        }

        // テクスチャエディット対象を無効にする
        public void Clear()
        {
            slotName = string.Empty;
            materialIndex = -1;
            propName = string.Empty;
        }
    }

    private void DoSelectTexture(int winId)
    {
        var scrollRect = new Rect(uiParams.margin, uiParams.unitHeight, uiParams.winRect.width - uiParams.margin * 2, uiParams.winRect.height - uiParams.unitHeight * 2);
        var conRect = new Rect(0, 0, scrollRect.width - 20, 0);
        var outRect = new Rect(0, 0, uiParams.winRect.width - uiParams.margin * 2, uiParams.itemHeight);
        GUILayoutOption buttonWidth  = GUILayout.Width(conRect.width * 0.1f);
        GUILayoutOption buttonWidth2 = GUILayout.Width(conRect.width * 0.2f);

        GUI.Label(outRect, "テクスチャ変更", uiParams.lStyle);
        scrollViewVector = GUI.BeginScrollView(scrollRect, scrollViewVector, conRect);

        try {
            // materials には null チェックが必要。例えば以下の操作を行う
            // (1) ゲーム側でワンピースを選択
            // (2) AlwaysColorChange側でワンピース→テクスチャ変更を選択
            // (3) ゲーム側でボトムスから衣装を選択
            // この時点でcurrentSlotnameが指すmaterialsが無くなるため、nullとなる
            // ->GetMaterials内で空要素を返すようにする
    
            var materials = holder.GetMaterials(currentSlot);
            
    //            conRect.height += uiParams.unitHeight * (materials.Count() * (PropNames.Count() * 2 + 2)) + uiParams.margin * 2;
            conRect.height += uiParams.unitHeight*2 + uiParams.margin * 2;
            int i = 0;
            foreach (Material material in materials) {
                string shaderName = material.shader.name;
                ShaderMapper.MaterialFlag mate = ShaderMapper.resolve(shaderName);
                if (mate == null) continue;
    
                conRect.height += uiParams.unitHeight * mate.propNames.Count() * 2;
    
                GUILayout.Label(material.name, uiParams.lStyle);
                foreach (string propName in mate.propNames) {
                    bool bTargetElement = (i == textureEdit.materialIndex && propName == textureEdit.propName);
                    
                    GUILayout.BeginHorizontal();
                    try {
                        // エディット用スライダーの開閉
                        if (!textureModifier.IsValidTarget(holder.maid, currentSlot.DisplayName, material, propName)) {
                            // 参照先が Texture2D が取れなかった (例 : RenderTexture だった) 場合はとりあえず
                            // あきらめて何もしない。無理やり書いても良いのかもしれないけど……
                            var tmp = GUI.enabled;
                            GUI.enabled = false;
                            GUILayout.Button("+変更", uiParams.bStyle, buttonWidth2);
                            GUI.enabled = tmp;
                        } else if (bTargetElement) {
                            // エディット中のテクスチャの場合
                            if (GUILayout.Button("-変更", uiParams.bStyle, buttonWidth2)) {
                                textureEdit.Clear();
                            }
                        } else {
                            // エディット中以外のテクスチャの場合
                            if (GUILayout.Button("+変更", uiParams.bStyle, buttonWidth2)) {
                                textureEdit.slotName = currentSlot.DisplayName;
                                textureEdit.materialIndex = i;
                                textureEdit.propName = propName;
                            }
                        }
                        GUILayout.Label(propName, uiParams.lStyle);
                    } finally {
                        GUILayout.EndHorizontal();
                    }
    
                    // テクスチャエディット用スライダー
                    if (bTargetElement) {
                        textureModifier.ProcGUI(holder.maid, currentSlot.DisplayName, materials[i], propName, uiParams.margin, uiParams.fontSize, uiParams.itemHeight);
                    }
        
                    GUILayout.BeginHorizontal();
                    try {
                        textureFile[i][propName] = GUILayout.TextField(textureFile[i][propName], uiParams.textStyle);
                        if (GUILayout.Button("適", uiParams.bStyle, buttonWidth)) {
                            targetMatno = i;
                            targetPropName = propName;
                            ChangeTex(texturePath, textureFile[targetMatno][targetPropName]);
                        }
                        if (GUILayout.Button("...", uiParams.bStyle, buttonWidth)) {
                            OpenFileBrowser(i, propName);
                        }
                    } finally {
                        GUILayout.EndHorizontal();
                    }
                }
                i++;
            }
        } finally {
            GUI.EndScrollView();
        }
        outRect.width = uiParams.winRect.width - uiParams.margin * 2;
        outRect.x = uiParams.margin;
        outRect.y = uiParams.winRect.height - uiParams.unitHeight;
        if (GUI.Button(outRect, "閉じる", uiParams.bStyle)) {
            menuType = MenuType.Color;
        }
        GUI.DragWindow();
    }

    private void OpenFileBrowser(int matno, string propName)
    {
        targetMatno = matno;
        targetPropName = propName;
        fileBrowser = new FileBrowser(
            new Rect(0, 0, uiParams.fileBrowserRect.width, uiParams.fileBrowserRect.height),
            "テクスチャファイル選択",
            FileSelectedCallback
        );
        fileBrowser.SelectionPatterns = new string[] { "*.tex", "*.png" };
        if (!String.IsNullOrEmpty(texturePath)) {
            fileBrowser.CurrentDirectory = texturePath;
        }
    }

    private void DoFileBrowser(int winId)
    {
        fileBrowser.OnGUI();
        GUI.DragWindow();
    }


    private class UIParams {
        private int width;
        private int height;
        private float ratio;
        
        public int margin;
        public int fontSize;
        public int itemHeight;
        public int unitHeight;
        public readonly GUIStyle lStyle = "label";
        public readonly GUIStyle bStyle = "button";
        public readonly GUIStyle tStyle = "toggle";
        public readonly GUIStyle textStyle = "textField";
        public readonly GUIStyle textAreaStyle = "textArea";

        public readonly Color textColor = new Color(1f, 1f, 1f, 0.98f);

        public Rect winRect = new Rect();
        public Rect fileBrowserRect = new Rect();
        public Rect modalRect = new Rect();
        public GUIStyle winStyle = "box";
        public UIParams() {
            Update();
        }

        public void Update() {
            bool screenSizeChanged = false;

            if (Screen.height != height) {
                height = Screen.height;
                screenSizeChanged = true;
            }
            if (Screen.width != width) {
                width = Screen.width;
                screenSizeChanged = true;
            }
            if (!screenSizeChanged) return;

            ratio = (1.0f + (width / 1280.0f - 1.0f) * 0.6f);

            // 画面サイズが変更された場合にのみ更新
            fontSize   = FixPx(fontPx);
            margin     = FixPx(marginPx);
            itemHeight = FixPx(itemHeightPx);
            unitHeight = margin + itemHeight;

            lStyle.fontSize         = fontSize;
            lStyle.normal.textColor = textColor;

            bStyle.fontSize         = fontSize;
            bStyle.normal.textColor = textColor;

            tStyle.fontSize         = fontSize;
            tStyle.normal.textColor = textColor;

            textStyle.fontSize         = fontSize;
            textStyle.normal.textColor = textColor;

            textAreaStyle.fontSize         = fontSize;
            textAreaStyle.normal.textColor = textColor;

            LogUtil.DebugLog(string.Format("screen=({0},{1}),margin={2},height={3},ratio={4})", width, height, margin, itemHeight, ratio));           

            winStyle.fontSize  = fontSize;
            winStyle.alignment = TextAnchor.UpperRight;            
            InitWinRect();
            InitFBRect();
            InitModalRect();
        }
        public void InitWinRect() {
            winRect.Set(        width - FixPx(290),     FixPx(20),               FixPx(280), height - FixPx(120));
        }
        public void InitFBRect() {
            fileBrowserRect.Set(width - FixPx(620),     FixPx(100),              FixPx(600), FixPx(600));                
        }
        public void InitModalRect() {
            modalRect.Set(      width / 2 - FixPx(300), height / 2 - FixPx(300), FixPx(600), FixPx(600));                
        }
        public Rect CreateMainRect() {
            check();
            return new Rect(margin, (unitHeight) * 5 + margin, winRect.width - margin * 2, winRect.height - (unitHeight) * 6);
        }
        public Rect CreateSelTexRect() {
            check();
            return new Rect(margin, unitHeight, winRect.width - margin * 2, winRect.height - unitHeight * 2);
        }
        public Rect CreateRect() {
            check();
            return new Rect(margin, unitHeight * 2, winRect.width - margin * 2, winRect.height - unitHeight * 4);
        }
        public Rect CreateColorRect() {
            check();
             return new Rect(margin, unitHeight * 2, winRect.width - margin * 3, winRect.height - unitHeight * 4);
        }
        private void check() {
            if(margin == 0) {
                Update();
            }
        }
        public int FixPx(int px) {
            return (int)(ratio * px);
        }
    }
    private bool displayed = false;
    private void DoMainMenu(int winID)
    {
        var scrollRect = uiParams.CreateMainRect();
        if (!displayed) {
            // 
            displayed = true;
            LogUtil.DebugLog("MainRect", scrollRect.xMin, scrollRect.yMin, scrollRect.width, scrollRect.height);
        }

        var outRect = new Rect(0, 0, uiParams.winRect.width - uiParams.margin * 2, uiParams.itemHeight);
        GUI.Label(outRect, "強制カラーチェンジ", uiParams.lStyle);
        outRect.y += uiParams.unitHeight;
        if (GUI.Button(outRect, "マスククリア", uiParams.bStyle)) {
            holder.ClearMasks();
            // TODO マスクアンクリア
        }
        outRect.y += uiParams.unitHeight;
        if (GUI.Button(outRect, "ノード表示切り替えへ", uiParams.bStyle)) {
            menuType = MenuType.NodeSelect;
        }
        outRect.y += uiParams.unitHeight;
        if (GUI.Button(outRect, "保存", uiParams.bStyle)) {
            menuType = MenuType.Save;
        }
        if (presets != null && presets.Any()) {
            outRect.y += uiParams.unitHeight;
            if (GUI.Button(outRect, "プリセット適用", uiParams.bStyle)) {
                menuType = MenuType.PresetSelect;
            }
        }
        outRect.y = 0;

        var conRect = new Rect(0, 0, scrollRect.width - 20, 
                               (uiParams.unitHeight) * ACConstants.SlotNames.Count + uiParams.margin * 2);

        scrollViewVector = GUI.BeginScrollView(scrollRect, scrollViewVector, conRect);
        try {
            foreach (SlotInfo slot in ACConstants.SlotNames.Values) {
                if (slot.Id == TBody.SlotID.end) continue;
    
                GUI.enabled = holder.maid.body0.GetSlotVisible(slot.Id);
                // TODO ボタンごと非表示とする場合は、全体のサイズも変更する必要あり
//                if (!holder.maid.body0.GetSlotVisible(slot.Id)) {
//                    continue;
//                }

//                MaidProp prop = holder.maid.GetPropLower(slot.Name.ToLower());
//                if (prop == null) continue;
    
                if (GUI.Button(outRect, slot.DisplayName, uiParams.bStyle)) {
                    currentSlot = slot;
                    menuType = MenuType.Color;
                }
                outRect.y += uiParams.unitHeight;
            }
        } finally {
            GUI.enabled = true;
            GUI.EndScrollView();
        }
        GUI.DragWindow();
    }



    private void FixDelNode(bool bApply)
    {
        if (!dDelNodes.Any()) return;

        foreach (TBodySkin tBodySkin in holder.maid.body0.goSlot) {

            tBodySkin.boVisible = true;
            if (tBodySkin.m_dicDelNodeBody != null) {
                foreach (KeyValuePair<string, bool> entry in dDelNodes) {
                    if (tBodySkin.m_dicDelNodeBody.ContainsKey(entry.Key)) {
                        tBodySkin.m_dicDelNodeBody[entry.Key] = entry.Value;
                    }
                }
            }
        }
        if (bApply) {
            holder.FixFlag();
        }
    }


    protected void FileSelectedCallback(string path)
    {
        fileBrowser = null;
        if (path == null) return;

        texturePath = Path.GetDirectoryName(path);
        textureFile[targetMatno][targetPropName] = Path.GetFileName(path);
        ChangeTex(texturePath, textureFile[targetMatno][targetPropName]);
    }

    private void ChangeTex(string path, string filename)
    {
        if (targetPropName.StartsWith("_")) {
            if (Path.GetExtension(filename).ToLower() == ".tex") {
                holder.maid.body0.ChangeTex(currentSlot.Name, targetMatno, targetPropName, textureFile[targetMatno][targetPropName], null, MaidParts.PARTS_COLOR.NONE);
            } else {
                var materials = holder.GetMaterials(currentSlot);
                byte[] img = UTY.LoadImage(Path.Combine(path, filename));
                var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                tex.LoadImage(img);
                materials[targetMatno].SetTexture(targetPropName, tex);
            }
        } else {
            GameUty.SystemMaterial mat = GameUty.SystemMaterial.Alpha;
            switch (targetPropName) {
                case "Alpha":
                    mat = GameUty.SystemMaterial.Alpha;
                    break;
                case "Multiply":
                    mat = GameUty.SystemMaterial.Multiply;
                    break;
                case "InfinityColor":
                    mat = GameUty.SystemMaterial.InfinityColor;
                    break;
                case "TexTo8bitTex":
                    mat = GameUty.SystemMaterial.TexTo8bitTex;
                    break;
                case "Max":
                    mat = GameUty.SystemMaterial.Max;
                    break;
            }
            // 合成
            if (Path.GetExtension(filename).ToLower() == ".tex") {
                holder.maid.body0.MulTexSet(currentSlot.Name, targetMatno, "_MainTex", 1, textureFile[targetMatno][targetPropName], mat, false, 0, 0, 0, 0);
                holder.maid.body0.MulTexSet(currentSlot.Name, targetMatno, "_ShadowTex", 1, textureFile[targetMatno][targetPropName], mat, false, 0, 0, 0, 0);
                // TODO 合成したtexを保存できないか？
            } else {
                //TODO
            }
        }
    }

    private void DoColorMenu(int winID)
    {
        var scrollRect = uiParams.CreateColorRect();
        var outRect = new Rect(uiParams.margin, 0, uiParams.winRect.width - uiParams.margin * 2, uiParams.itemHeight);

        GUI.Label(outRect, "強制カラーチェンジ:" + currentSlot.DisplayName, uiParams.lStyle);
        outRect.y += uiParams.unitHeight;

        List<Material> materialList = holder.GetMaterials(currentSlot);
        if ( GUI.Button(outRect, "テクスチャ変更", uiParams.bStyle) ) {
            textureFile = new Dictionary<int, Dictionary<string, string>>();
            int i=0;
            foreach (Material m in materialList) {
                string shaderName = m.shader.name;
                ShaderMapper.MaterialFlag mate = ShaderMapper.resolve(shaderName);

                if (mate == null) continue;
                textureFile.Add(i, new Dictionary<string, string>(mate.propNames.Length));
                foreach (string propName in mate.propNames) {
                    textureFile[i].Add(propName, "");
                }
                i++;
            }
            menuType = MenuType.Texture;
        }

        outRect.y = 0;
        outRect.width -= uiParams.margin * 2 + 20;
        if (materialList.Any()) {
            var conRect = new Rect(0, 0, scrollRect.width - 20, 0);
            int itemCount = 0;
            foreach (Material material in materialList) {
                itemCount += 3; // title + renderQueue + 1
                ShaderMapper.MaterialFlag mate = ShaderMapper.resolve(material.shader.name);

                if (mate == null) continue;
//                LogUtil.DebugLog("shader=>", material.shader.name, "material=>", ShaderMapper.name(material.shader.name));

                // [color:5, float:2]
                if (mate.hasColor)   itemCount += 5; // color
                if (mate.isOutlined) itemCount += 7; // CoutlineColor + OutlineWidth(float)
                if (mate.isToony)    itemCount += 9; // RimColor + RimPower,RimShift (float x2)
                if (mate.isLighted)  itemCount += 7; // ShadowColor + Shininess(float)
                if (mate.isHair)     itemCount += 4; // HiRate,HiPow(float x2)
            }
            conRect.height += uiParams.unitHeight * itemCount + uiParams.margin;

            scrollViewVector = GUI.BeginScrollView(scrollRect, scrollViewVector, conRect);
            try {
                foreach (Material material in materialList) {
                    outRect.x = uiParams.margin;
                    GUI.Label(outRect, material.name, uiParams.lStyle);
                    outRect.x += uiParams.margin;
                    outRect.width = conRect.width - uiParams.margin * 3;
                    outRect.y += uiParams.unitHeight;
    
                    string shaderName = material.shader.name;
                    ShaderMapper.MaterialFlag mate = ShaderMapper.resolve(shaderName);
    
                    int renderQueue = material.renderQueue;
                    renderQueue = (int)drawModValueSlider(outRect, renderQueue, 0, 5000, String.Format("{0}:{1}", "RQ", material.renderQueue));
                    material.SetFloat("_SetManualRenderQueue", renderQueue);
                    material.renderQueue = renderQueue;
                    outRect.y += uiParams.unitHeight;
    
                    Color sColor       = material.GetColor("_Color");
                    Color shadowColor  = material.GetColor("_ShadowColor");
                    Color outlineColor = material.GetColor("_OutlineColor");
                    Color rimColor     = material.GetColor("_RimColor");
                    float? shininess    = null;
                    float? outlineWidth = null;
                    float? rimPower     = null;
                    float? rimShift     = null;
                    float? hiRate       = null;
                    float? hiPow       = null;
                    /*
                    var colors = new ItemContainer[4];
                    if (mate.hasColor) 
                        colors[0] = new ItemContainer("Color", sColor);
                    if (mate.isLighted)    
                        colors[1] = new ItemContainer("Shadow Color", shadowColor);
                    if (mate.isOutlined)
                        colors[2] = new ItemContainer("Outline Color", outlineColor);
                    if (mate.isToony)
                        colors[3] = new ItemContainer("Rim Color", rimColor);
                    
                    foreach (ItemContainer kv in colors) {
                        if (kv == null) continue;
                        //drawColorPane(ref outRect, sColor, "Color", itemHeight, margin, lStyle);
                        GUI.Label(outRect, kv.label, uiParams.lStyle);
                        outRect.y += uiParams.unitHeight;
                        kv.color.r = drawModValueSlider(outRect, kv.color.r, 0f, 2f, String.Format("{0}:{1:F2}", "R", kv.color.r));
                        outRect.y += uiParams.unitHeight;
                        kv.color.g = drawModValueSlider(outRect, kv.color.g, 0f, 2f, String.Format("{0}:{1:F2}", "G", kv.color.g));
                        outRect.y += uiParams.unitHeight;
                        kv.color.b = drawModValueSlider(outRect, kv.color.b, 0f, 2f, String.Format("{0}:{1:F2}", "B", kv.color.b));
                        outRect.y += uiParams.unitHeight;
                        kv.color.a = drawModValueSlider(outRect, kv.color.a, 0f, 1f, String.Format("{0}:{1:F2}", "A", kv.color.a));
                        outRect.y += uiParams.unitHeight;
                    }*/
                    
                    if (mate.hasColor) {
                        GUI.Label(outRect, "Color", uiParams.lStyle);
                        outRect.y += uiParams.unitHeight;
                        sColor.r = drawModValueSlider(outRect, sColor.r, 0f, 2f, String.Format("{0}:{1:F2}", "R", sColor.r));
                        outRect.y += uiParams.unitHeight;
                        sColor.g = drawModValueSlider(outRect, sColor.g, 0f, 2f, String.Format("{0}:{1:F2}", "G", sColor.g));
                        outRect.y += uiParams.unitHeight;
                        sColor.b = drawModValueSlider(outRect, sColor.b, 0f, 2f, String.Format("{0}:{1:F2}", "B", sColor.b));
                        outRect.y += uiParams.unitHeight;
                        sColor.a = drawModValueSlider(outRect, sColor.a, 0f, 1f, String.Format("{0}:{1:F2}", "A", sColor.a));
                        outRect.y += uiParams.unitHeight;
                    }
                    if (mate.isLighted) {
                        GUI.Label(outRect, "Shadow Color", uiParams.lStyle);
                        outRect.y += uiParams.unitHeight;
                        shadowColor.r = drawModValueSlider(outRect, shadowColor.r, 0f, 2f, String.Format("{0}:{1:F2}", "R", shadowColor.r));
                        outRect.y += uiParams.unitHeight;
                        shadowColor.g = drawModValueSlider(outRect, shadowColor.g, 0f, 2f, String.Format("{0}:{1:F2}", "G", shadowColor.g));
                        outRect.y += uiParams.unitHeight;
                        shadowColor.b = drawModValueSlider(outRect, shadowColor.b, 0f, 2f, String.Format("{0}:{1:F2}", "B", shadowColor.b));
                        outRect.y += uiParams.unitHeight;
                        shadowColor.a = drawModValueSlider(outRect, shadowColor.a, 0f, 1f, String.Format("{0}:{1:F2}", "A", shadowColor.a));
                        outRect.y += uiParams.unitHeight;
                    }
                    if (mate.isOutlined) {
                        GUI.Label(outRect, "Outline Color", uiParams.lStyle);
                        outRect.y += uiParams.unitHeight;
                        outlineColor.r = drawModValueSlider(outRect, outlineColor.r, 0f, 2f, String.Format("{0}:{1:F2}", "R", outlineColor.r));
                        outRect.y += uiParams.unitHeight;
                        outlineColor.g = drawModValueSlider(outRect, outlineColor.g, 0f, 2f, String.Format("{0}:{1:F2}", "G", outlineColor.g));
                        outRect.y += uiParams.unitHeight;
                        outlineColor.b = drawModValueSlider(outRect, outlineColor.b, 0f, 2f, String.Format("{0}:{1:F2}", "B", outlineColor.b));
                        outRect.y += uiParams.unitHeight;
                        outlineColor.a = drawModValueSlider(outRect, outlineColor.a, 0f, 1f, String.Format("{0}:{1:F2}", "A", outlineColor.a));
                        outRect.y += uiParams.unitHeight;
                    }
                    if (mate.isToony) {
                        GUI.Label(outRect, "Rim Color", uiParams.lStyle);
                        outRect.y += uiParams.unitHeight;
                        rimColor.r = drawModValueSlider(outRect, rimColor.r, 0f, 2f, String.Format("{0}:{1:F2}", "R", rimColor.r));
                        outRect.y += uiParams.unitHeight;
                        rimColor.g = drawModValueSlider(outRect, rimColor.g, 0f, 2f, String.Format("{0}:{1:F2}", "G", rimColor.g));
                        outRect.y += uiParams.unitHeight;
                        rimColor.b = drawModValueSlider(outRect, rimColor.b, 0f, 2f, String.Format("{0}:{1:F2}", "B", rimColor.b));
                        outRect.y += uiParams.unitHeight;
                        rimColor.a = drawModValueSlider(outRect, rimColor.a, 0f, 1f, String.Format("{0}:{1:F2}", "A", rimColor.a));
                        outRect.y += uiParams.unitHeight;
                    }
                    if (mate.isLighted) {
                        shininess = material.GetFloat("_Shininess");
                        if (shininess != null) {
                            GUI.Label(outRect, "Shininess", uiParams.lStyle);
                            outRect.y += uiParams.unitHeight;
    //                            shininess = drawModValueSlider(outRect, (float)shininess, -10f, 10f, String.Format("  {0:F2}", (float)shininess));
                            shininess = drawModValueSlider(outRect, (float)shininess, -20f, 20f, String.Format("  {0:F2}", (float)shininess));
                            outRect.y += uiParams.unitHeight;
                        }
                    }
                    if (mate.isOutlined) {
                        outlineWidth = material.GetFloat("_OutlineWidth");
                        if (outlineWidth != null) {
                            GUI.Label(outRect, "OutlineWidth", uiParams.lStyle);
                            outRect.y += uiParams.unitHeight;
                            outlineWidth = drawModValueSlider(outRect, (float)outlineWidth, 0f, 0.1f, String.Format("  {0:F5}", (float)outlineWidth));
                            outRect.y += uiParams.unitHeight;
                        }
                    }
                    if (mate.isToony) {
                        rimPower = material.GetFloat("_RimPower");
                        if (rimPower != null) {
                            GUI.Label(outRect, "RimPower", uiParams.lStyle);
                            outRect.y += uiParams.unitHeight;
                            rimPower = drawModValueSlider(outRect, (float)rimPower, 0f, 100f, String.Format("  {0:F2}", (float)rimPower));
                            outRect.y += uiParams.unitHeight;
                        }
                        rimShift = material.GetFloat("_RimShift");
                        if (rimShift != null) {
                            GUI.Label(outRect, "RimShift", uiParams.lStyle);
                            outRect.y += uiParams.unitHeight;
                            rimShift = drawModValueSlider(outRect, (float)rimShift, -5f, 5f, String.Format("  {0:F2}", (float)rimShift));
                            outRect.y += uiParams.unitHeight;
                        }
                    }
                    if (mate.isHair) {
                        hiRate = material.GetFloat("_HiRate");
                        if (hiRate != null) {
                            GUI.Label(outRect, "HiRate", uiParams.lStyle);
                            outRect.y += uiParams.unitHeight;
                            hiRate = drawModValueSlider(outRect, (float)hiRate, 0f, 1f, String.Format("  {0:F2}", (float)hiRate) );
                            outRect.y += uiParams.unitHeight;
                        }
                        hiPow = material.GetFloat("_HiPow");
                        if (hiPow != null) {
                            GUI.Label(outRect, "HiPow", uiParams.lStyle);
                            outRect.y += uiParams.unitHeight;
                            hiPow = drawModValueSlider(outRect, (float)hiPow, 0.001f, 50f, String.Format("  {0:F4}", (float)hiPow));
                            outRect.y += uiParams.unitHeight;
                        }                        
                    }
    
                    try {
                        Shader mShader = material.shader;
                        // material名は別マテリアルで同一となる場合があるため、オブジェクトのハッシュでキャッシュ
//                        if (!changeShaders.ContainsKey(material.name)) {
//                            if (shaderName == "Hidden/InternalErrorShader") {
//                                changeShaders.Add(material.name, null);
//                            } else {
//                                changeShaders.Add(material.name, mShader);
//                            }
//                        }
                        if (sColor.a < 1f) {
                            // CM3D2のシェーダのみを対象として、_Transのついたシェーダを使用するように変更 ただし、Hairは対応するshader無し,
                            if (!mate.isTrans && mate.isToony && mate.isLighted && !mate.isHair) {
//                                Shader shader = Shader.Find(shaderName + "_Trans");
                                Shader shader = Shader.Find("CM3D2/Toony_Lighted_Trans");
                                if (shader != null) {
                                    material.shader = shader;
                                    LogUtil.DebugLog(material.name, " changed shader.", shaderName, "=>", shader.name);

                                    try {
                                        // 上書きしない 
                                        changeShaders.Add(material.GetInstanceID(), mShader);
                                    } catch(ArgumentException ignoree) {}
                                }
                            }
    
                        } else {
                            // 元のシェーダに戻す
                            Shader temp = null;
                            if (changeShaders.TryGetValue(material.GetInstanceID(), out temp)) {
                                material.shader = temp;
                            }
                        }
    
                    } catch (Exception e) {
                        LogUtil.DebugLog(e);
                    }
    
                    if (mate.hasColor) {
                        material.SetColor("_Color", sColor);
                    }
                    if (mate.isLighted) {
                        material.SetColor("_ShadowColor", shadowColor);
                        if (shininess != null)
                            material.SetFloat("_Shininess", (float)shininess);
                    }
                    if (mate.isToony) {
                        material.SetColor("_RimColor", rimColor);
                        if (rimPower != null)
                            material.SetFloat("_RimPower", (float)rimPower);
                        if (rimShift != null)
                            material.SetFloat("_RimShift", (float)rimShift);
                    }
                    if (mate.isOutlined) {
                        material.SetColor("_OutlineColor", outlineColor);
                        if (outlineWidth != null)
                            material.SetFloat("_OutlineWidth", (float)outlineWidth);
                    }
                    if (mate.isHair) {
                        if (hiRate != null)
                            material.SetFloat("_HiRate", (float)hiRate);
                        if (hiPow != null)
                            material.SetFloat("_HiPow", (float)hiPow);
                    }
                    outRect.y += uiParams.margin * 3;
                }
            } finally {
                GUI.EndScrollView();
            }
        }

        outRect.x = uiParams.margin;
        outRect.y = uiParams.winRect.height - (uiParams.unitHeight) * 2;
        outRect.width = uiParams.winRect.width - uiParams.margin * 2;
        if (GUI.Button(outRect, "menu/mate保存", uiParams.bStyle)) {
            TBody body = holder.maid.body0;
            List<TBodySkin> goSlot = body.goSlot;
            int index = (int)global::TBody.hashSlotName[currentSlot.Name];
            global::TBodySkin tBodySkin = goSlot[index];
            GameObject obj = tBodySkin.obj;
            if (obj == null) return;

            MaidProp prop = holder.maid.GetProp(currentSlot.Name.ToLower());
            if (prop != null) {
                targetMenuInfo = new MenuInfo();
                bool loaded = targetMenuInfo.LoadMenufile(prop.strFileName);
                // 変更可能なマテリアルがない場合はダイアログを表示しない（TODO 表示できない旨通知)
                if (targetMenuInfo.materials.Any()) {
                    showSaveDialog |= loaded;
                }
            }
        }

        outRect.y += uiParams.unitHeight;
        if (GUI.Button(outRect, "閉じる", uiParams.bStyle)) {
            menuType = MenuType.Main;
        }
        GUI.DragWindow();
    }

    private bool InitDelNode() {
        if (holder.maid == null) return false;

        // 身体からノード一覧と表示状態を取得
        TBody body = holder.maid.body0;
        List<TBodySkin> goSlot = body.goSlot;

        int index = (int)global::TBody.hashSlotName[TBody.SlotID.body.ToString()];
        TBodySkin tBodySkin = goSlot[index];
        Dictionary<string, bool> dic = tBodySkin.m_dicDelNodeBody;
        foreach (string key in ACConstants.NodeNames.Keys) {
            if (dic.ContainsKey(key)) {
                dDelNodes.Add(key, dic[key]);
            }
        }
        return true;
    }

    private void DoNodeSelectMenu(int winID)
    {
        var scrollRect = uiParams.CreateRect();
        var conRect = new Rect(0, 0, scrollRect.width - 20, 0);
        var outRect = new Rect(0, 0, uiParams.winRect.width - uiParams.margin * 2, uiParams.itemHeight);

        GUI.Label(outRect, "表示ノード選択", uiParams.lStyle);
        outRect.y += uiParams.unitHeight;

        if (!dDelNodes.Any()) {
            InitDelNode();
        }

        if (GUI.Button(outRect, "すべてON", uiParams.bStyle)) {
            var keyList = new List<string>(dDelNodes.Keys);
            foreach (string key in keyList) {
                dDelNodes[key] = true;
            }
        }

        //conRect.height += (uiParams.unitHeight) * dDelNodes.Count + uiParams.margin * 2;
        conRect.height += (uiParams.unitHeight) * ACConstants.NodeNames.Count + uiParams.margin * 2;
        outRect.y = 0;
        outRect.x = uiParams.margin * 2;

        scrollViewVector = GUI.BeginScrollView(scrollRect, scrollViewVector, conRect);
        try {
            foreach (KeyValuePair<string, string> pair in ACConstants.NodeNames) {
                if (dDelNodes.ContainsKey(pair.Key)) {
                    dDelNodes[pair.Key] = GUI.Toggle(outRect, dDelNodes[pair.Key], pair.Value, uiParams.tStyle);
                    outRect.y += uiParams.unitHeight;
                }
            }
        } finally {
            GUI.EndScrollView();
        }
        outRect.y = uiParams.winRect.height - (uiParams.unitHeight) * 2;
        if (GUI.Button(outRect, "適用", uiParams.bStyle)) {
            FixDelNode(true);
        }
        outRect.y += uiParams.unitHeight;
        if (GUI.Button(outRect, "閉じる", uiParams.bStyle)) {
            menuType = MenuType.Main;
        }
        GUI.DragWindow();
    }

    private void DoSaveMenu(int winID)
    {
        var outRect = new Rect(0, 0, uiParams.winRect.width - uiParams.margin * 2, uiParams.itemHeight);

        GUI.Label(outRect, "保存", uiParams.lStyle);
        outRect.y += uiParams.unitHeight;
        outRect.width = uiParams.winRect.width * 0.3f - uiParams.margin;

        GUI.Label(outRect, "プリセット名", uiParams.lStyle);
        outRect.x += outRect.width;
        outRect.width = uiParams.winRect.width * 0.7f - uiParams.margin;
        presetName = GUI.TextField(outRect, presetName, uiParams.textStyle);

        outRect.x = uiParams.margin;
        outRect.y += outRect.height + uiParams.margin;
        outRect.width = uiParams.winRect.width - uiParams.margin * 2;

        bClearMaskEnable = GUI.Toggle(outRect, bClearMaskEnable, "マスククリアを有効にする", uiParams.tStyle);
        outRect.y += outRect.height + uiParams.margin;

        bSaveBodyPreset = GUI.Toggle(outRect, bSaveBodyPreset, "身体も保存する", uiParams.tStyle);
        outRect.y += outRect.height + uiParams.margin;

        if (GUI.Button(outRect, "保存", uiParams.bStyle)) {
            try {
                if (presetName.Equals("")) {
                    // 名無しはNG
                    return;
                }
                presetMgr.Save(SaveFileName, presetName, bClearMaskEnable, bSaveBodyPreset, dDelNodes);
                menuType = MenuType.Main;

                // TODO 読み直すのではなく、メモリ上に追加するだけとしたい
                LoadPresets();
            } catch(NullReferenceException e) {
                LogUtil.ErrorLog(e);
            }
        }
        outRect.y += outRect.height + uiParams.margin;
        if (GUI.Button(outRect, "閉じる", uiParams.bStyle)) {
            menuType = MenuType.Main;
        }

        GUI.DragWindow();

    }

    private void DoSelectPreset(int winId)
    {

        var scrollRect = uiParams.CreateRect();
        var conRect = new Rect(0, 0, scrollRect.width - 20, 0);
        var outRect = new Rect(0, 0, uiParams.winRect.width - uiParams.margin * 2, uiParams.itemHeight);

        GUI.Label(outRect, "プリセット適用", uiParams.lStyle);
        outRect.y += uiParams.unitHeight;

        conRect.height += (uiParams.unitHeight) * presets.Count + uiParams.margin * 2;
        outRect.y = 0;
        outRect.x = uiParams.margin * 2;

        scrollViewVector = GUI.BeginScrollView(scrollRect, scrollViewVector, conRect);
        try {
            foreach (var preset in presets) {
                if (GUI.Button(outRect, preset.Key, uiParams.bStyle)) {
                    targetPreset = preset.Value;
                    ApplyMpns();
                    menuType = MenuType.Main;
                }
                outRect.y += uiParams.unitHeight;
            }
        } finally {
            GUI.EndScrollView();
        }
        outRect.y = uiParams.winRect.height - (uiParams.unitHeight) + uiParams.margin;
        if (GUI.Button(outRect, "閉じる", uiParams.bStyle)) {
            menuType = MenuType.Main;
        }
        GUI.DragWindow();
    }

    private void ApplyMpns()
    {
        if (targetPreset == null) {
            return;
        }

        if (targetPreset.mpns == null || targetPreset.mpns.Count == 0) {
            ApplyPreset();
        } else {

            // 衣装チェンジ
            foreach (string key in targetPreset.mpns.Keys) {
                if (targetPreset.mpns[key].EndsWith("_del.menu")) {
                    continue;
                }
                if (targetPreset.mpns[key].EndsWith(".mod")) {
                    string sFilePath = Path.GetFullPath(".\\") + "Mod\\" + targetPreset.mpns[key];
                    if (!File.Exists(sFilePath)) {
                        continue;
                    }
                }
                holder.maid.SetProp(key, targetPreset.mpns[key], targetPreset.mpns[key].ToLower().GetHashCode(), false);
            }
            holder.FixFlag();
            bApplyChange = true;
        }
    }

    private void ApplyPreset()
    {
        dDelNodes = targetPreset.delNodes;
        FixDelNode(false);

        // 保存対象を列挙型に変更（表示名変更時に対応できなくなるため）
        foreach (var ccslot in targetPreset.slots.Values) {
            string slotName = ccslot.name;
            if (!Enum.IsDefined(typeof(TBody.SlotID), slotName)) {
                // 旧版互換のため,旧表示名でパース
                try {
                    TBody.SlotID slotID = ACConstants.Slotnames[slotName];
                    slotName = slotID.ToString();
                } catch(KeyNotFoundException e) {
                    continue;
                }
            }

            List<Material> materials = holder.GetMaterials(slotName);
            foreach (var material in materials) {
                if (ccslot.materials.ContainsKey(material.name)) {
                    // 同名のマテリアルに、シェーダとカラーを適用
                    material.shader = Shader.Find(ccslot.materials[material.name].shader);
                    material.color = ccslot.materials[material.name].color;
                }
            }
        }

        if (targetPreset.clearMask) {
            holder.ClearMasks();
        } else {
            holder.FixFlag();
        }
        bApplyChange = false;
    }

    private void LoadPresets()
    {
        if (!File.Exists(SaveFileName)) return;
        presets = presetMgr.Load(SaveFileName);
    }


    private void DoSaveModDialog(int winId)
    {
        if (targetMenuInfo == null || !targetMenuInfo.materials.Any()) {
            LogUtil.DebugLog("target menu ('s material) is empty. Save dialog cannot be displayed.");
            showSaveDialog = false;
            return;
        }

        var outRect = new Rect(0, 0, uiParams.modalRect.width - uiParams.margin * 2, uiParams.itemHeight);

        outRect.x = uiParams.margin;
        outRect.y = uiParams.unitHeight;
        outRect.width = uiParams.modalRect.width * 0.2f - uiParams.margin;
        GUI.Label(outRect, "メニュー", uiParams.lStyle);
        outRect.x += outRect.width;
        outRect.width = uiParams.modalRect.width * 0.8f - uiParams.margin;
        targetMenuInfo.filename = GUI.TextField(outRect, targetMenuInfo.filename, uiParams.textStyle);

        outRect.x = uiParams.margin;
        outRect.y += outRect.height + uiParams.margin;
        outRect.width = uiParams.modalRect.width * 0.2f - uiParams.margin;
        GUI.Label(outRect, "アイコン", uiParams.lStyle);
        outRect.x += outRect.width;
        outRect.width = uiParams.modalRect.width * 0.8f - uiParams.margin;
        targetMenuInfo.icons = GUI.TextField(outRect, targetMenuInfo.icons, uiParams.textStyle);

        outRect.x = uiParams.margin;
        outRect.y += outRect.height + uiParams.margin;
        outRect.width = uiParams.modalRect.width * 0.2f - uiParams.margin;
        GUI.Label(outRect, "名前", uiParams.lStyle);
        outRect.x += outRect.width;
        outRect.width = uiParams.modalRect.width * 0.8f - uiParams.margin;
        targetMenuInfo.name = GUI.TextField(outRect, targetMenuInfo.name, uiParams.textStyle);

        outRect.x = uiParams.margin;
        outRect.y += outRect.height + uiParams.margin;
        outRect.width = uiParams.modalRect.width * 0.2f - uiParams.margin;
        GUI.Label(outRect, "説明", uiParams.lStyle);
        outRect.x += outRect.width;
        outRect.width = uiParams.modalRect.width * 0.8f - uiParams.margin;
        outRect.height = uiParams.itemHeight * 4;
        targetMenuInfo.setumei = GUI.TextArea(outRect, targetMenuInfo.setumei, uiParams.textAreaStyle);

        outRect.y += outRect.height + uiParams.margin;
        outRect.height = uiParams.itemHeight;
        foreach (string[] material in targetMenuInfo.materials) {
            outRect.x = uiParams.margin;
            outRect.width = uiParams.modalRect.width * 0.2f - uiParams.margin;
            GUI.Label(outRect, "マテリアル" + material[1], uiParams.lStyle);

            outRect.x += outRect.width;
            outRect.width = uiParams.modalRect.width * 0.8f - uiParams.margin;
            material[2] = GUI.TextField(outRect, material[2], uiParams.textStyle);
            outRect.y += outRect.height + uiParams.margin;
        }

        /*
        for (int i = 0; i < targetMenuInfo.resources.Count(); i++)
        {
            outRect.x = margin;
            outRect.width = modalRect.width * 0.2f - margin;
            GUI.Label(outRect, targetMenuInfo.resources[i][0], lStyle);
            outRect.x += outRect.width;
            outRect.width = modalRect.width * 0.8f - margin;
            targetMenuInfo.resources[i][1] = GUI.TextField(outRect, targetMenuInfo.resources[i][1], uiParams.textStyle);
            outRect.y += outRect.height + margin;
        }
        */
        foreach (string[] addItem in targetMenuInfo.addItems) {
            outRect.x = uiParams.margin;
            outRect.width = uiParams.modalRect.width * 0.2f - uiParams.margin;
            GUI.Label(outRect, "addItem:" + addItem[1], uiParams.lStyle);
            outRect.x += outRect.width;
            outRect.width = uiParams.modalRect.width * 0.8f - uiParams.margin;
            addItem[0] = GUI.TextField(outRect, addItem[0], uiParams.textStyle);
            outRect.y += outRect.height + uiParams.margin;
        }
        outRect.x = uiParams.margin;
        outRect.y += outRect.height + uiParams.margin;
        outRect.width = uiParams.modalRect.width / 2 - uiParams.margin * 2;

        if (GUI.Button(outRect, "保存", uiParams.bStyle)) {
            if (FileExists(targetMenuInfo.filename + MenuInfo.EXT_MENU)) {
                NUty.WinMessageBox(NUty.GetWindowHandle(), "メニューファイル[" + targetMenuInfo.filename + MenuInfo.EXT_MENU + "]が既に存在します", "エラー", 0);
                return;
            }
            foreach (var mat in targetMenuInfo.materials) {
                if (FileExists(mat[2] + MenuInfo.EXT_MATERIAL)) {
                    NUty.WinMessageBox(NUty.GetWindowHandle(), "マテリアルファイル[" + mat[2] + MenuInfo.EXT_MATERIAL + "]が既に存在します", "エラー", 0);
                    return;
                }
            }
            SaveMod(currentSlot);
            showSaveDialog = false;
        }
        outRect.x += outRect.width + uiParams.margin;

        showSaveDialog &= !(GUI.Button(outRect, "閉じる", uiParams.bStyle));;
        
        GUI.DragWindow();
    }

    private void SaveMod(SlotInfo slot)
    {
        TBody body = holder.maid.body0;
        List<TBodySkin> goSlot = body.goSlot;
        int index = (int)global::TBody.hashSlotName[slot.Name];
        global::TBodySkin tBodySkin = goSlot[index];
        GameObject gobj = tBodySkin.obj;
        if (gobj == null) {
            return;
        }
        MaidProp prop = holder.maid.GetProp(slot.Name.ToLower());

        MenuInfo menuInfo = targetMenuInfo;
        // 出力先
        String filename = prop.strFileName;
        string fullPath = Path.GetFullPath(".\\");
        string path = Path.Combine(fullPath, "Mod");
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        path = Path.Combine(path, "ACC");
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);

        path = Path.Combine(path, menuInfo.filename);
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);

        LogUtil.DebugLog("output path", path);

        // menu
        if (!MenuWrite(path, filename, menuInfo)) return;

        var materials = new Material[menuInfo.baseMaterials.Count];
        for (int i=0; i< materials.Length; i++) {
            var matefileName = menuInfo.baseMaterials[i][2];
        }
//        Transform[] components = gobj.transform.GetComponentsInChildren<Transform>(true);
//        foreach (Transform tf in components) {
//            Renderer r = tf.renderer;
//            if (r != null && r.material != null && r.material.shader != null) {
//                 
//                materialList.AddRange(r.materials);
//                {
//                    var buf = new StringBuilder();
//                    buf.Append(r.name).Append(" 's materials=");
//                    foreach (Material m in r.materials) {
//                        buf.Append(m.name).Append(",");
//                    }
//                    LogUtil.DebugLog(buf);
//                }
//            }
//        }
//
//        // TODO 通知：マテリアルがないため出力できない
//        if (!materialList.Any()) {
//            LogUtil.DebugLog("output mod. material file is empty", filename);
//            return;
//        }

        // material
        for( int i=0; i<menuInfo.baseMaterials.Count; i++) {
            // カテゴリとマテリアル番号でマテリアルオブジェクトを参照
            // menuInfo.materials[i];
            if (!MateWrite(path, menuInfo.baseMaterials[i][2], menuInfo.materials[i][2], gobj)) {
                // TODO ディレクトリの削除 
                return;
            }
        }

        // model
        for (int i = 0; i < menuInfo.baseAddItems.Count(); i++) {
            if (!FileExists(menuInfo.addItems[i][0] + MenuInfo.EXT_MODEL)) {
                ModelWrite(path, menuInfo.baseAddItems[i][0], menuInfo.addItems[i][0]);
            }
        }
        /*
        for (int i = 0; i < menuInfo.baseResources.Count(); i++)
        {
            if (!FileExists(menuInfo.baseResources[i][1] + MenuInfo.EXT_MODEL))
            {
                ModelWrite(path, menuInfo.baseResources[i][1], menuInfo.resources[i][1]);
            }
        }
        */
        // icon
        if (!FileExists(menuInfo.icons + MenuInfo.EXT_TEXTURE)) {
            TexWrite(path, menuInfo.baseIcons, menuInfo.icons);
        }
    }

    private float drawModValueSlider(Rect outRect, float value, float min, float max, string label)
    {
        float conWidth = outRect.width;

        float margin = uiParams.margin*3;
        outRect.x += margin;
        outRect.width = conWidth * 0.35f -margin;
        GUI.Label(outRect, label, uiParams.lStyle);
        outRect.x += outRect.width - margin;

        outRect.width = conWidth * 0.65f;
        outRect.y += uiParams.FixPx(5);

        return GUI.HorizontalSlider(outRect, value, min, max);
    }

    private bool FileExists(string filename)
    {
        using (AFileBase aFileBase = global::GameUty.FileOpen(filename)) {
            if (!aFileBase.IsValid()) {
                return false;
            }
        }
        return true;
    }

    private bool MenuWrite(string path, string filename, MenuInfo menu)
    {
        byte[] cd = null;
        var materials = new Dictionary<string, string>();
        try {
            using (AFileBase aFileBase = global::GameUty.FileOpen(filename)) {
                if (!aFileBase.IsValid()) {
                    LogUtil.ErrorLog("アイテムメニューファイルが見つかりません。", filename);
                    return false;
                }
                cd = aFileBase.ReadAll();
            }
        } catch (Exception ex2) {
            LogUtil.ErrorLog("アイテムメニューファイルが読み込めませんでした。", filename, ex2.Message);
            return false;
        }
        try {
            //.menuの保存
            using (var headerMs = new MemoryStream())
            using (var dataMs = new MemoryStream())
            using (var headerWriter = new BinaryWriter(headerMs))
            using (var dataWriter = new BinaryWriter(dataMs)) {
                using (var binaryReader = new BinaryReader(new MemoryStream(cd), Encoding.UTF8)) {
                    string text = binaryReader.ReadString();
                    if (text != MenuInfo.HEAD) {
                        LogUtil.ErrorLog("例外: ヘッダーファイルが不正です。", text, filename);
                        return false;
                    }
                    headerWriter.Write(text);
                    int num = binaryReader.ReadInt32();
                    headerWriter.Write(num);
                    string txtpath = binaryReader.ReadString();
                    int pos = txtpath.LastIndexOf("/", StringComparison.CurrentCulture);
                    if (pos >= 0) {
                        txtpath = txtpath.Substring(0, pos + 1) + Path.GetFileNameWithoutExtension(filename) + ".txt";
                    }
                    headerWriter.Write(txtpath);
                    string menuName = binaryReader.ReadString(); // 名前を置き換え
                    headerWriter.Write(menu.name);
                    string category = binaryReader.ReadString();
                    headerWriter.Write(category);
                    string comment = binaryReader.ReadString();
                    headerWriter.Write(menu.setumei.Replace("\n", MenuInfo.RET));
                    int num2 = (int)binaryReader.ReadInt32();

                    bool materialWrited = false;
                    bool addItemWrited = false;

                    while (true) {
                        byte b = binaryReader.ReadByte();
                        int size = (int)b;
                        if (size == 0) {
                            dataWriter.Write((char)0);
                            break;
                        }
                        var param = new string[size];
                        for (int i = 0; i < size; i++) {
                            param[i] = binaryReader.ReadString();
                        }

                        if (param[0] == "name") {
                            param[1] = menu.name;
                        } else if (param[0] == "setumei") {
                            param[1] = menu.setumei.Replace("\n", MenuInfo.RET);
                        } else if (param[0] == "priority") {
                            param[1] = "9999";
                        } else if (param[0] == "icons") {
                            param[1] = menu.icons + MenuInfo.EXT_TEXTURE;
                        } else if (param[0] == "マテリアル変更") {
                            if (!materialWrited) {
                                materialWrited = true;
                                foreach (var mat in menu.materials) {
                                    dataWriter.Write(b);
                                    dataWriter.Write("マテリアル変更");
                                    dataWriter.Write(mat[0]);
                                    dataWriter.Write(mat[1]);
                                    dataWriter.Write(mat[2] + MenuInfo.EXT_MATERIAL);
                                }
                            }
                            continue;
                        } else if (param[0] == "additem") {
                            if (!addItemWrited) {
                                addItemWrited = true;
                                foreach (var items in menu.addItems) {
                                    dataWriter.Write(b);
                                    dataWriter.Write("additem");
                                    dataWriter.Write(items[0] + MenuInfo.EXT_MODEL);
                                    for(int i=1; i< items.Length; i++) 
                                        dataWriter.Write(items[i]);
                                }
                            }
                            continue;
                        }
                        dataWriter.Write(b);
                        for (int i = 0; i < size; i++) {
                            dataWriter.Write(param[i]);
                        }
                    }
                }
                using (var writer = new BinaryWriter(File.OpenWrite(Path.Combine(path, menu.filename + MenuInfo.EXT_MENU)))) {
                    writer.Write(headerMs.ToArray());
                    writer.Write((int)dataMs.Length);
                    writer.Write(dataMs.ToArray());
                }
            }
        } catch (Exception e) {
            Debug.Log(e);
            return false;
        }
        return true;
    }

    private void TexWrite(string path, string infile, string outname)
    {
        string filename = infile + MenuInfo.EXT_TEXTURE;
        try {
            using (AFileBase aFileBase = global::GameUty.FileOpen(filename)) {
                if (!aFileBase.IsValid()) {
                    LogUtil.ErrorLog("テクスチャファイルが見つかりません。", filename);
                    return;
                }
                byte[] cd = aFileBase.ReadAll();
                using (var writer = new BinaryWriter(File.OpenWrite(Path.Combine(path, outname + MenuInfo.EXT_TEXTURE)))) {
                    writer.Write(cd);
                }
            }
        } catch (Exception ex2) {
            LogUtil.ErrorLog("テクスチャファイルが読み込めませんでした。", filename, ex2.Message);
        }
    }

    private void ModelWrite(string path, string infile, string outname)
    {
        string filename = infile + MenuInfo.EXT_MODEL;
        try {
            using (AFileBase aFileBase = global::GameUty.FileOpen(filename)) {
                if (!aFileBase.IsValid()) {
                    LogUtil.ErrorLog("Modelファイルが見つかりません。", filename);
                    return;
                }
                byte[] cd = aFileBase.ReadAll();
                using (var writer = new BinaryWriter(File.OpenWrite(Path.Combine(path, outname + MenuInfo.EXT_MODEL)))) {
                    writer.Write(cd);
                }
                // TODO Shader変更
            }
        } catch (Exception ex2) {
            LogUtil.ErrorLog("Modelファイルが読み込めませんでした。", filename, ex2.Message);
        }
    }

    private bool MateWrite(string path, string infile, string outname, GameObject gobj)
    {
        LogUtil.DebugLog("output material file", infile, outname);
        byte[] cd = null;
        string infilename = infile + MenuInfo.EXT_MATERIAL;
        try {
            using (AFileBase aFileBase = global::GameUty.FileOpen(infilename)) {
                if (!aFileBase.IsValid()) {
                    LogUtil.ErrorLog("マテリアルファイルが見つかりません。", infilename);
                    return false;
                }
                cd = aFileBase.ReadAll();
            }
        } catch (Exception ex2) {
            LogUtil.ErrorLog("マテリアルファイルが読み込めませんでした。", infilename, ex2.Message);
        }
        try {
            //.mateの保存
            using (var headerMs = new MemoryStream())
            using (var dataMs = new MemoryStream())
            using (var headerWriter = new BinaryWriter(headerMs))
            using (var dataWriter = new BinaryWriter(dataMs)) {
                using (var binReader = new BinaryReader(new MemoryStream(cd), Encoding.UTF8)) {
                    string text = binReader.ReadString();
                    if (text != "CM3D2_MATERIAL") {
                        LogUtil.ErrorLog("ヘッダーファイルが不正です。", text, infilename);
                        return false;
                    }
                    headerWriter.Write(text);
                    int num = binReader.ReadInt32();
                    headerWriter.Write(num);
                    string name1 = binReader.ReadString();
                    headerWriter.Write(outname);
                    string name2 = binReader.ReadString();

                    Material material = null;
                    // name2でマテリアル名を参照
                    Transform[] components = gobj.transform.GetComponentsInChildren<Transform>(true);
                    foreach (Transform tf in components) {
                        Renderer r = tf.renderer;
                        if (r != null && r.material != null && r.material.shader != null) {
                            foreach (Material m in r.materials) {
                                if (m.name.ToLower() == name2.ToLower()) {
                                    material = m;
                                    break;
                                }
                            }
                        }
                    }
                     if (material == null) {
                        LogUtil.ErrorLog("mateファイルのname2に対応するマテリアルが見つかりません", name2);
                        return false;
                    }
                    headerWriter.Write(name2);
                    string shader1 = binReader.ReadString();
                    headerWriter.Write(shader1);
                    string shader2 = binReader.ReadString();
                    headerWriter.Write(shader2);

                    while (true) {
                        string key = binReader.ReadString();
                        dataWriter.Write(key);
                        if (key == "end") {
                            break;
                        }
                        string propertyName = binReader.ReadString();
                        dataWriter.Write(propertyName);
                        switch(key) {
                        case "tex":
                            string type = binReader.ReadString();
                            dataWriter.Write(type);
                            if (type == "null") {
                            } else if (type == "tex2d") {
                                // TODO テクスチャ名を変更
                                string tex = binReader.ReadString();
                                dataWriter.Write(tex);
                                // TODO 合わせてアセット名も更新(変更があったものだけ)
                                string asset = binReader.ReadString();
                                dataWriter.Write(asset);

                                // Vector2 offset;
                                dataWriter.Write(binReader.ReadSingle());
                                dataWriter.Write(binReader.ReadSingle());

                                // Vector2 scale;
                                dataWriter.Write(binReader.ReadSingle());
                                dataWriter.Write(binReader.ReadSingle());

                            } else if (type == "texRT") {
                                string text7 = binReader.ReadString();
                                dataWriter.Write(text7);
                                string text8 = binReader.ReadString();
                                dataWriter.Write(text8);
                            }
                            break;
                        case "col":
                            Color mColor = material.GetColor(propertyName);
                            if (mColor != null) {
                                for (int i=0; i<4; i++) binReader.ReadSingle();
                                dataWriter.Write(mColor.r);
                                dataWriter.Write(mColor.g);
                                dataWriter.Write(mColor.b);
                                dataWriter.Write(mColor.a);
                            } else {
                                // color
                                for (int i=0; i<4; i++)
                                    dataWriter.Write(binReader.ReadSingle());
                            }
                            break;
                        case "vec":
                            // vector4
                            for (int i=0; i<4; i++)
                                dataWriter.Write(binReader.ReadSingle());
                            break;
                        case "f":
                            float value2 = binReader.ReadSingle();
                            float val = material.GetFloat(propertyName);
                            dataWriter.Write(val);
                            break;
                        default:
                            LogUtil.ErrorLog("マテリアルが読み込めません。不正なマテリアルプロパティ型です ", key);
                            break;
                        }

                    }
                }
                using (var writer = new BinaryWriter(File.OpenWrite(Path.Combine(path, outname + MenuInfo.EXT_MATERIAL)))) {
                    writer.Write(headerMs.ToArray());
                    writer.Write(dataMs.ToArray());
                }
            }
        } catch (Exception e) {
            LogUtil.ErrorLog(e);
            return false;
        }
        return true;
    }
}

}