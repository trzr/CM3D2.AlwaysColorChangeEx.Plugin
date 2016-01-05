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
 PluginVersion("0.0.4.5")]
class AlwaysColorChange : UnityInjector.PluginBase
{
    // プラグイン名
    private static volatile string _pluginName;
    // プラグインバージョン
    private static volatile string _version;
    private static byte[] _lock = new byte[0];
    public static string PluginName
    {
        get {
            if (_pluginName == null) {
                lock(_lock) {
                    if (_pluginName == null) {
                        try {
                            // 属性クラスからプラグイン名取得
                            var att = Attribute.GetCustomAttribute( typeof(AlwaysColorChange), typeof( PluginNameAttribute ) ) as PluginNameAttribute;
                            if( att != null ) {
                                _pluginName = att.Name;
                            }
                        } catch( Exception e ) {
                            LogUtil.ErrorLog( e );
                        }
                    }
                }
            }
            return _pluginName;
        }
    }
    // プラグインバージョン取得
    public static string Version
    {
        get {
            if (_version == null) {
                lock(_lock) {
                    if (_version == null) {
                        try {
                            // 属性クラスからバージョン番号取得
                            var att = Attribute.GetCustomAttribute( typeof(AlwaysColorChange), typeof( PluginVersionAttribute ) ) as PluginVersionAttribute;
                            if( att != null ) {
                                _version = att.Version;
                            }
                        } catch( Exception e ) {
                            LogUtil.ErrorLog( e );
                        }
                    }
                }
            }
            return _version;
        }
    }

    #region Constants
    private const float GUIWidth = 0.25f;
    private const int marginPx = 4;
    private const int fontPx = 14;
    private const int itemHeightPx = 18;
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

    private bool bClearMaskEnable = false;
    private bool bSaveBodyPreset  = false;

    private bool isActive;
    private string SaveFileName;

    private Vector2 scrollViewVector = Vector2.zero;

    // テクスチャ変更用
    private TextureModifier textureModifier;
    private TextureEdit textureEdit = new TextureEdit();
    private FileBrowser fileBrowser;
    private string texturePath;
    // TODO 配列化 (matNoは配列のindexとして扱えるはず)
    private Dictionary<int, Dictionary<string, string>> textureFile;

    private Dictionary<string, List<Material>> slotMaterials = new Dictionary<string, List<Material>>(ACConstants.SlotNames.Count);
    #endregion

    #region MonoBehaviour methods
    public void Awake() 
    {
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

    public void OnDestroy() 
    {
        dispose();
        if (presets != null) {
            presets.Clear();
        }
    }

    public void OnLevelWasLoaded(int level) 
    {
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
//        if (Input.GetKeyDown(KeyCode.Alpha0)) {
//            if (dDelNodes.Any()) {
//                var keyList = new List<string>(dDelNodes.Keys);
//                foreach (string key in keyList) {
//                    LogUtil.DebugLog(key);
//                }
//            }
//        }

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
        if (menuType == MenuType.Texture) {
            
            // 必要のないときは処理を行わない
            if (textureEdit.IsValid()) {

                // スロットが置き換わっているか確認(modelファイルが同一であればマテリアル数は同一
                TBodySkin slot = holder.maid.body0.GetSlot(currentSlot.Name);

                string modelFile = slot.m_strModelFileName;
                if (targetModelFile != modelFile) {
                    LogUtil.DebugLog("slot's model changed.", targetModelFile);
                    var materials = holder.GetMaterials(slot);
                    //　対象のスロットのモデルが変更された場合に、マテリアルからtex一覧を初期化
                    InitTexChange(materials, slot);
                }

                textureModifier.UpdateSlot(
                    holder.maid,
                    slotMaterials[currentSlot.Name],
                    textureEdit);
            }
        } else {
            // テクスチャモードでなければ、テクスチャ変更対象を消す
            textureEdit.Clear();
        }
    }
    #endregion



    #region Private methods
    private void dispose() 
    {
//        goMainPanel = null;
//        visible = false;

        // テクスチャキャッシュを開放する
        textureModifier.Clear();
        changeShaders.Clear();
        dDelNodes.Clear();

        initialized = false;
    }

    private bool initialize() 
    {
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

        return initGUI();
    }

    private bool initGUI() 
    {
        return true;
    }
    #endregion

    // 編集中のパーツに対するモデルファイル
    private string targetModelFile;

    // テクスチャ変更のためmaterialリストを再取得
    // 対象のスロットのアイテムが変更された場合も呼び出す必要がある
    void InitTexChange(List<Material> materialList, TBodySkin slot) {
        textureFile = new Dictionary<int, Dictionary<string, string>>();
        int matNo = 0;
        foreach (Material m in materialList) {
            string shaderName = m.shader.name;
            ShaderMapper.MaterialFlag mate = ShaderMapper.resolve(shaderName);

            // 未対応のシェーダはスキップ
            if (mate == null) {
                LogUtil.Log("未知のシェーダが指定されていました", currentSlot.Name, shaderName);
                textureFile.Add(matNo, new Dictionary<string, string>());
            } else {
                textureFile.Add(matNo, new Dictionary<string, string>(mate.propNames.Length));
                foreach (string propName in mate.propNames) {
                    textureFile[matNo].Add(propName, "");
                }
            }
            matNo++;
        }
        if (slot == null) {
            slot = holder.maid.body0.GetSlot(currentSlot.Name);
        }
        LogUtil.DebugLog("slot's model changed.", targetModelFile, "=>", slot.m_strModelFileName);
        slotMaterials[currentSlot.Name] = materialList;
        targetModelFile = slot.m_strModelFileName;
        // 必要であれば、textureModifierの設定を行う
    }

    private void DoSelectTexture(int winId)
    {
        var scrollRect = uiParams.textureRect;
        var conRect = new Rect(0, 0, scrollRect.width - 20, 0);
        GUILayoutOption buttonWidth  = GUILayout.Width(conRect.width * 0.1f);
        GUILayoutOption buttonWidth2 = GUILayout.Width(conRect.width * 0.2f);

        var outRect = uiParams.subRect;
        GUI.Label(outRect, "テクスチャ変更", uiParams.lStyle);
        
        scrollViewVector = GUI.BeginScrollView(scrollRect, scrollViewVector, conRect);
        try {
            // materials には null チェックが必要。例えば以下の操作を行う
            // (1) ゲーム側でワンピースを選択
            // (2) AlwaysColorChange側でワンピース→テクスチャ変更を選択
            // (3) ゲーム側でボトムスから衣装を選択
            // この時点でcurrentSlotnameが指すmaterialsが無くなるため、nullとなる
            // ->GetMaterials内で空要素を返すようにする
    
            // スロットが置き換わっているか確認(modelファイルが同一であればマテリアル数は同一
            TBodySkin slot = holder.maid.body0.GetSlot(currentSlot.Name);

            var materials = holder.GetMaterials(slot);
            //　対象のスロットのモデルが変更された場合に、マテリアルからtex一覧を初期化
            string modelFile = slot.m_strModelFileName;
            if (targetModelFile == null || targetModelFile != modelFile) {
                InitTexChange(materials, slot);
            }

    //            conRect.height += uiParams.unitHeight * (materials.Count() * (PropNames.Count() * 2 + 2)) + uiParams.margin * 2;
            conRect.height += uiParams.unitHeight*2 + uiParams.margin * 2;
            int matNo = 0;
            foreach (Material material in materials) {
                string shaderName = material.shader.name;
                ShaderMapper.MaterialFlag mate = ShaderMapper.resolve(shaderName);
                if (mate == null) continue;
    
                conRect.height += uiParams.unitHeight * mate.propNames.Count() * 2;
    
                GUILayout.Label(material.name, uiParams.lStyle);
                foreach (string propName in mate.propNames) {
                    bool bTargetElement = (matNo == textureEdit.matNo && propName == textureEdit.propName);

                    var dic = textureFile[matNo];

                    // modelが同一の場合であれば、shaderが同じためテクスチャ数も同じ(発生しないはずだが、一応チェック)
                    if (!dic.ContainsKey(propName)) continue;

                    GUILayout.BeginHorizontal();
                    try {
                        // エディット用スライダーの開閉
                        if (!textureModifier.IsValidTarget(holder.maid, currentSlot.Name, material, propName)) {
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
                                textureEdit.slotName = currentSlot.Name;
                                textureEdit.matNo = matNo;
                                textureEdit.propName = propName;
                            }
                        }
                        GUILayout.Label(propName, uiParams.lStyle);
                    } finally {
                        GUILayout.EndHorizontal();
                    }
    
                    // テクスチャエディット用スライダー
                    if (bTargetElement) {
                        textureModifier.ProcGUI(holder.maid, currentSlot.Name, materials[matNo], propName, 
                                                uiParams.margin, uiParams.fontSize, uiParams.itemHeight);
                    }
        
                    GUILayout.BeginHorizontal();
                    try {
                        dic[propName] = GUILayout.TextField(dic[propName], uiParams.textStyle);
                        if (GUILayout.Button("適", uiParams.bStyle, buttonWidth)) {
                            ChangeTexFile(texturePath, dic[propName], matNo, propName);
                        }
                        if (GUILayout.Button("...", uiParams.bStyle, buttonWidth)) {
                            OpenFileBrowser(matNo, propName);
                        }
                    } finally {
                        GUILayout.EndHorizontal();
                    }
                }
                matNo++;
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

    private void OpenFileBrowser(int matNo, string propName)
    {
        fileBrowser = new FileBrowser(
            new Rect(0, 0, uiParams.fileBrowserRect.width, uiParams.fileBrowserRect.height),
            "テクスチャファイル選択",
            (path) => 
            {
                fileBrowser = null;
                if (path == null) return;

                texturePath = Path.GetDirectoryName(path);
                textureFile[matNo][propName] = Path.GetFileName(path);
                ChangeTexFile(texturePath, textureFile[matNo][propName], matNo, propName);

            });
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

        public Rect winRect         = new Rect();
        public Rect fileBrowserRect = new Rect();
        public Rect modalRect       = new Rect();

        public Rect mainRect         = new Rect();
        public Rect mainConRect      = new Rect();
        public Rect colorRect        = new Rect();
        public Rect nodeSelectRect   = new Rect();
        public Rect presetSelectRect = new Rect();
        public Rect textureRect      = new Rect();
        public Rect subRect          = new Rect();

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

            // sub
            mainRect.Set(      margin, unitHeight * 5 + margin, winRect.width - margin * 2, winRect.height - unitHeight * 6);
            mainConRect.Set(   0,      0,              mainRect.width - 20, unitHeight * ACConstants.SlotNames.Count + margin * 2);
            textureRect.Set(   margin, unitHeight,     winRect.width - margin * 2, winRect.height - unitHeight * 2);
            nodeSelectRect.Set(margin, unitHeight * 2, winRect.width - margin * 2, winRect.height - unitHeight * 4);
            colorRect.Set(     margin, unitHeight * 2, winRect.width - margin * 3, winRect.height - unitHeight * 4);
            subRect.Set(       0,      0,              winRect.width - margin * 2, itemHeight);
//            color Sub outRect.Set(       margin, 0,              winRect.width - margin * 2, itemHeight);
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
        public int FixPx(int px) {
            return (int)(ratio * px);
        }
    }
    private bool displayed = false;
    private void DoMainMenu(int winID)
    {
        var scrollRect = uiParams.mainRect;
        if (!displayed) {
            displayed = true;
            LogUtil.DebugLog("MainRect", scrollRect.xMin, scrollRect.yMin, scrollRect.width, scrollRect.height);
        }

        var outRect = uiParams.subRect;
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
        scrollViewVector = GUI.BeginScrollView(scrollRect, scrollViewVector, uiParams.mainConRect);
        try {
            foreach (SlotInfo slot in ACConstants.SlotNames.Values) {
                if (slot.Id == TBody.SlotID.end) continue;
    
                GUI.enabled = holder.maid.body0.GetSlotVisible(slot.Id);
                // TODO ボタンごと非表示とする場合は、全体のサイズも変更する必要あり
//                if (!holder.maid.body0.GetSlotVisible(slot.Id)) {
//                    continue;
//                }

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
        if (bApply) holder.FixFlag();
    }

    private void ChangeTexFile(string path, string filename, int matNo, string propName)
    {
        if (propName.StartsWith("_")) {
            if (Path.GetExtension(filename).ToLower() == ".tex") {
                holder.maid.body0.ChangeTex(currentSlot.Name, matNo, propName, textureFile[matNo][propName], null, MaidParts.PARTS_COLOR.NONE);
            } else {
                var materials = holder.GetMaterials(currentSlot);
                byte[] img = UTY.LoadImage(Path.Combine(path, filename));
                var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                tex.LoadImage(img);
                materials[matNo].SetTexture(propName, tex);
            }
        } else {
            
            GameUty.SystemMaterial mat;
            try {
                mat = (GameUty.SystemMaterial)Enum.Parse(typeof(GameUty.SystemMaterial), propName);
            } catch(ArgumentException e) {
                mat = GameUty.SystemMaterial.Alpha;
            }

            // 合成
            if (Path.GetExtension(filename).ToLower() == ".tex") {
                holder.maid.body0.MulTexSet(currentSlot.Name, matNo, "_MainTex", 1, textureFile[matNo][propName], mat, false, 0, 0, 0, 0);
                holder.maid.body0.MulTexSet(currentSlot.Name, matNo, "_ShadowTex", 1, textureFile[matNo][propName], mat, false, 0, 0, 0, 0);
            } else {
                //TODO
            }
        }
    }

    // TODO 変更前の情報をメモリ上に保持

    private void DoColorMenu(int winID)
    {
        var scrollRect = uiParams.colorRect;
        //var outRect = new Rect(uiParams.margin, 0, uiParams.winRect.width - uiParams.margin * 2, uiParams.itemHeight);
        var outRect = uiParams.subRect;

        GUI.Label(outRect, "強制カラーチェンジ:" + currentSlot.DisplayName, uiParams.lStyle);
        outRect.y += uiParams.unitHeight;

        TBodySkin slot = holder.maid.body0.GetSlot(currentSlot.Name);
        List<Material> materialList = holder.GetMaterials(slot);

        if ( GUI.Button(outRect, "テクスチャ変更", uiParams.bStyle) ) {
            InitTexChange(materialList, slot);
            menuType = MenuType.Texture;
        }

        outRect.y = 0;
        outRect.width -= uiParams.margin * 2 + 20;
        if (materialList.Any()) {
            var conRect = new Rect(0, 0, scrollRect.width - 20, 0);
            int itemCount = 0;
            foreach (Material material in materialList) {
                itemCount += 4; // title + shaderName + renderQueue + 1
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

                    // シェーダ名
                    GUI.Label(outRect, "<"+shaderName+">", uiParams.lStyle);
                    outRect.width = conRect.width - uiParams.margin * 3;
                    outRect.y += uiParams.unitHeight;

                    int renderQueue = material.renderQueue;
                    renderQueue = (int)drawModValueSlider(outRect, renderQueue, 0, 5000, String.Format("{0}:{1}", "RQ", material.renderQueue));
                    material.SetFloat("_SetManualRenderQueue", renderQueue);
                    material.renderQueue = renderQueue;
                    outRect.y += uiParams.unitHeight;
                    // TODO シェーダ表示

                    if (mate.hasColor) {
                        Color sColor       = material.GetColor("_Color");
                        setColorSlider(ref outRect, "Color", ref sColor);
                        material.SetColor("_Color", sColor);

                        // シェーダ置き換え
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
//                                    Shader shader = Shader.Find(shaderName + "_Trans");
                                    Shader shader = Shader.Find("CM3D2/Toony_Lighted_Trans");
                                    if (shader != null) {
                                        material.shader = shader;
                                        LogUtil.DebugLog(material.name, " changed shader.", shaderName, "=>", shader.name);
    
                                        try {
                                            // 上書きしない 
                                            changeShaders.Add(material.GetInstanceID(), mShader);
                                        } catch(ArgumentException ignore) {}
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

                    }
                    if (mate.isLighted) {
                        Color shadowColor  = material.GetColor("_ShadowColor");
                        setColorSlider(ref outRect, "Shadow Color", ref shadowColor);
                        material.SetColor("_ShadowColor", shadowColor);
                    }
                    if (mate.isOutlined) {
                        Color outlineColor = material.GetColor("_OutlineColor");
                        setColorSlider(ref outRect, "Outline Color", ref outlineColor);
                        material.SetColor("_OutlineColor", outlineColor);
                    }
                    if (mate.isToony) {
                        Color rimColor     = material.GetColor("_RimColor");
                        setColorSlider(ref outRect, "Rim Color", ref rimColor);
                        material.SetColor("_RimColor", rimColor);
                    }

                    if (mate.isLighted) {
                        float shininess = material.GetFloat("_Shininess");
                        shininess = setValueSlider(ref outRect, "Shininess", "  {0:F2}", shininess, 
                                                   settings.shininessMin, settings.shininessMax);
                        material.SetFloat("_Shininess", shininess);
                    }
                    if (mate.isOutlined) {
                        float outlineWidth = material.GetFloat("_OutlineWidth");
                        outlineWidth = setValueSlider(ref outRect, "OutlineWidth", "  {0:F5}", outlineWidth, 
                                                   settings.outlineWidthMin, settings.outlineWidthMax);
                        material.SetFloat("_OutlineWidth", outlineWidth);
                    }
                    if (mate.isToony) {
                        float rimPower     = material.GetFloat("_RimPower");
                        rimPower = setValueSlider(ref outRect, "RimPower", "  {0:F2}", rimPower, 
                                                   settings.rimPowerMin, settings.rimPowerMax);
                        material.SetFloat("_RimPower", rimPower);

                        float rimShift = material.GetFloat("_RimShift");
                        rimShift = setValueSlider(ref outRect, "RimShift", "  {0:F2}", rimShift, 
                                                   settings.rimShiftMin, settings.rimShiftMax);
                        material.SetFloat("_RimShift", rimShift);
                    }
                    if (mate.isHair) {
                        float hiRate       = material.GetFloat("_HiRate");
                        hiRate = setValueSlider(ref outRect, "HiRate", "  {0:F2}", hiRate, 
                                                   settings.hiRateMin, settings.hiRateMax);
                        material.SetFloat("_HiRate", hiRate);

                        float hiPow        = material.GetFloat("_HiPow");
                        hiPow = setValueSlider(ref outRect, "HiPow", "  {0:F4}", hiPow, 
                                                   settings.hiPowMin, settings.hiPowMax);
                        material.SetFloat("_HiPow", hiPow);
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
//            List<TBodySkin> goSlot = body.goSlot;
//            int index = (int)global::TBody.hashSlotName[currentSlot.Name];
            global::TBodySkin tBodySkin = body.GetSlot(currentSlot.Name);
            GameObject obj = tBodySkin.obj;
            if (obj == null) return;

            // propにはなぜかslot名の小文字が指定されている
            MaidProp prop = holder.maid.GetProp(currentSlot.Name.ToLower());
            if (prop != null) {
                if (prop.strFileName.EndsWith(MenuInfo.EXT_MOD, StringComparison.CurrentCulture)) {
                    var msg = "modファイルの変更は未対応です " + prop.strFileName;
                    NUty.WinMessageBox(NUty.GetWindowHandle(), msg, "エラー", NUty.MSGBOX.MB_OK);
                } else {
                    targetMenuInfo = new MenuInfo();
                    bool loaded = targetMenuInfo.LoadMenufile(prop.strFileName);
                    // 変更可能なmenuファイルがない場合は保存画面へ遷移しない
                    if (!loaded) {
                        var msg = "変更可能なmenuファイルがありません " + prop.strFileName;
                        NUty.WinMessageBox(NUty.GetWindowHandle(), msg, "エラー", NUty.MSGBOX.MB_OK);
                    }

                    if (targetMenuInfo.materials.Any()) {
                        showSaveDialog |= loaded;
                    }
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
        var scrollRect = uiParams.nodeSelectRect;
        var conRect = new Rect(0, 0, scrollRect.width - 20, 0);
        var outRect = uiParams.subRect;

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
        var outRect = uiParams.subRect;

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
        var scrollRect = uiParams.nodeSelectRect;
        var conRect = new Rect(0, 0, scrollRect.width - 20, 0);
        var outRect = uiParams.subRect;

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
        if (targetPreset == null) return;

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

    private void ApplyPreset() {
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

            // スロット上のマテリアル番号での判断に変更
            List<Material> materials = holder.GetMaterials(slotName);
            int i=0;
            foreach (CCMaterial mat in ccslot.materials.Values) {
                if (i < materials.Count) {
                    Shader sh = Shader.Find(mat.shader);
                    if (sh != null) {
                        materials[i].shader = sh;
                        materials[i].color  = mat.color;
                    }
                } else {
                    LogUtil.Log("マテリアル番号に対する、一致するマテリアルが見つかりません。", slotName, i);
                    break;
                }
                i++;
            }
//            foreach (var material in materials) {
//                if (ccslot.materials.ContainsKey(material.name)) {
//                    // 同名のマテリアルに、シェーダとカラーを適用
//                    material.shader = Shader.Find(ccslot.materials[material.name].shader);
//                    material.color = ccslot.materials[material.name].color;
//                }
//            }
        }

        if (targetPreset.clearMask) {
            holder.ClearMasks();
        } else {
            holder.FixFlag();
        }
        bApplyChange = false;
    }

    private void LoadPresets() {
        if (!File.Exists(SaveFileName)) return;
        presets = presetMgr.Load(SaveFileName);
    }

    private void DoSaveModDialog(int winId) {
        if (targetMenuInfo == null || !targetMenuInfo.materials.Any()) {
            LogUtil.DebugLog("target menu ('s material) is empty. Save dialog cannot be displayed.");
            showSaveDialog = false;
            return;
        }

        var outRect = uiParams.subRect;

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
                NUty.WinMessageBox(NUty.GetWindowHandle(), "メニューファイル[" + targetMenuInfo.filename + MenuInfo.EXT_MENU + "]が既に存在します", "エラー", NUty.MSGBOX.MB_OK);
                return;
            }
            foreach (var mat in targetMenuInfo.materials) {
                if (FileExists(mat[2] + MenuInfo.EXT_MATERIAL)) {
                    NUty.WinMessageBox(NUty.GetWindowHandle(), "マテリアルファイル[" + mat[2] + MenuInfo.EXT_MATERIAL + "]が既に存在します", "エラー", NUty.MSGBOX.MB_OK);
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

    private void setColorSlider(ref Rect outRect, string label, ref Color color) {
        GUI.Label(outRect, label, uiParams.lStyle);
        
        outRect.y += uiParams.unitHeight;

        color.r = drawModValueSlider(outRect, color.r, 0f, 2f, String.Format("{0}:{1:F2}", "R", color.r));
        outRect.y += uiParams.unitHeight;
        color.g = drawModValueSlider(outRect, color.g, 0f, 2f, String.Format("{0}:{1:F2}", "G", color.g));
        outRect.y += uiParams.unitHeight;
        color.b = drawModValueSlider(outRect, color.b, 0f, 2f, String.Format("{0}:{1:F2}", "B", color.b));
        outRect.y += uiParams.unitHeight;
        color.a = drawModValueSlider(outRect, color.a, 0f, 1f, String.Format("{0}:{1:F2}", "A", color.a));
        outRect.y += uiParams.unitHeight;
    }
    private float setValueSlider(ref Rect outRect, string label, string format, float val, float min, float max) {
        GUI.Label(outRect, label, uiParams.lStyle);
        outRect.y += uiParams.unitHeight;

        val = drawModValueSlider(outRect, val, min, max, String.Format(format, (float)val) );
        outRect.y += uiParams.unitHeight;

        return val;
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
        outRect.y += uiParams.FixPx(7);

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