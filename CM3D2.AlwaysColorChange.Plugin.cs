using System;
using System.Collections;
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
using CM3D2.AlwaysColorChange.Plugin.UI;
using CM3D2.AlwaysColorChange.Plugin.Util;

// 以下の AssemblyVersion は削除しないこと
[assembly: AssemblyVersion("1.0.*")]
namespace CM3D2.AlwaysColorChange.Plugin {
[PluginFilter("CM3D2x64"),
 PluginFilter("CM3D2x86"),
 PluginFilter("CM3D2VRx64"),
 PluginName("CM3D2 AlwaysColorChangeMod"),
 PluginVersion("0.0.5.2")]
class AlwaysColorChange : UnityInjector.PluginBase
{
    // プラグイン名
    public static volatile string PluginName;
    // プラグインバージョン
    public static volatile string Version;
    static AlwaysColorChange() {
        // 属性クラスからプラグイン名取得
        try {
            var att = Attribute.GetCustomAttribute( typeof(AlwaysColorChange), typeof( PluginNameAttribute ) ) as PluginNameAttribute;
            if( att != null ) PluginName = att.Name;
        } catch( Exception e ) {
            LogUtil.ErrorLog( e );
        }        
        // プラグインバージョン取得
        try {
            // 属性クラスからバージョン番号取得
            var att = Attribute.GetCustomAttribute( typeof(AlwaysColorChange), typeof( PluginVersionAttribute ) ) as PluginVersionAttribute;
            if( att != null ) Version = att.Version;
        } catch( Exception e ) {
            LogUtil.ErrorLog( e );
        }
    }



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
        SceneDance_SCL_Leap = 22
    }

    private enum MenuType {
        None,
        Main,
        Color,
        NodeSelect,
        MaskSelect,
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
    private OutputUtilEx outUtil = OutputUtilEx.Instance;

    private PresetManager presetMgr = new PresetManager();
    private UIParams uiParams = UIParams.Instance;
    private MaidHolder holder = MaidHolder.Instance;
   
    private MenuType menuType;

    // 操作情報
    private Dictionary<string, bool> dDelNodes     = new Dictionary<string, bool>();
    // 表示中状態
    private Dictionary<string, bool> dDelNodeDisps = new Dictionary<string, bool>();
    private Dictionary<TBody.SlotID, MaskInfo> dMaskSlots = new Dictionary<TBody.SlotID, MaskInfo>();
    private Dictionary<string, CCPreset> presets;

    private string presetName = "";
    private bool bApplyChange = false;
    private MenuInfo targetMenuInfo;
    private CCPreset targetPreset;

    private bool bClearMaskEnable = false;
    private bool bSaveBodyPreset  = false;

    private bool isActive;
    private string SaveFileName;

    private Vector2 scrollViewPosition = Vector2.zero;

    // テクスチャ変更用
    //  現在のターゲットのslotに関するメニューが変更されたらGUIを更新。それ以外は更新しない
    private string targetMenu;
    private Material[] targetMaterials;
    private List<ACCMaterialsView> materialViews;
    List<ACCTexturesView> texViews;

//    private Dictionary<string, List<Material>> slotMaterials = new Dictionary<string, List<Material>>(ACConstants.SlotNames.Count);
    #endregion

    #region MonoBehaviour methods
    public void Awake() 
    {
        settings.configPath = DataPath;
        base.ReloadConfig();
        settings.Load((key) => base.Preferences["Config"][key].Value);

        SaveFileName = Path.Combine(settings.configPath , "AlwaysColorChange.xml");
        LogUtil.Log("SaveFileName", SaveFileName);

        LoadPresets();
        uiParams.Update();
    }

    public void OnDestroy() 
    {
        SetCameraControl(true);
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
                SetCameraControl(true);
                initialized = false;
                // dispose();
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
            if (ACCTexturesView.fileBrowser != null) {
                uiParams.fileBrowserRect = GUI.Window(14, uiParams.fileBrowserRect, DoFileBrowser, Version, uiParams.winStyle);
            } else {
                switch (menuType) {
                    case MenuType.Main:
                        uiParams.winRect = GUI.Window(12, uiParams.winRect, DoMainMenu, Version, uiParams.winStyle);
                        break;
                    case MenuType.Color:
                        uiParams.winRect = GUI.Window(12, uiParams.winRect, DoColorMenu, Version, uiParams.winStyle);
                        break;
                    case MenuType.MaskSelect:
                        uiParams.winRect = GUI.Window(12, uiParams.winRect, DoMaskSelectMenu, Version, uiParams.winStyle);
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
                // uiParams.InitModalRect();
                uiParams.modalRect = GUI.ModalWindow(13, uiParams.modalRect, DoSaveModDialog, "保存");
            }
        }
    }
    private void SetCameraControl(bool enable) {
        if (cmrCtrlChanged == enable) {
            GameMain.Instance.MainCamera.SetControl(enable);
            UICamera.InputEnable = enable;
            cmrCtrlChanged = !enable;
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

        bool isEnableControl = false;
        if (menuType != MenuType.None) {
            var cursor = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
            isEnableControl = uiParams.winRect.Contains(cursor);
        }

        // カメラコントロールの有効化/無効化 (Windowの範囲外では、自身がコントロールを変更したケース以外は更新しない)
        if (isEnableControl) {
            if (GameMain.Instance.MainCamera.GetControl()) {
                SetCameraControl(false);
            }
        } else {
            SetCameraControl(true);
        }

        if (bApplyChange && !holder.maid.boAllProcPropBUSY) {
            ApplyPreset();
        }

        // テクスチャエディットの反映
        if (menuType == MenuType.Texture) {
            
            // 必要のないときは処理を行わない
            if (ACCTexturesView.IsChangeTarget()) {

                // マウスが離されたタイミングでのみテクスチャ合成
                if (Input.GetMouseButtonUp(0)) {
                    ACCTexturesView.UpdateTex(holder.maid, targetMaterials);
                }
            }
        } else {
            // テクスチャモードでなければ、テクスチャ変更対象を消す
            ACCTexturesView.ClearTarget();
        }
    }
    #endregion

    #region Private methods
    private void dispose() 
    {
        ClearMaidData();
        SetCameraControl(true);

        initialized = false;
    }
    private void ClearMaidData() {
        // テクスチャキャッシュを開放する
        ACCMaterialsView.Clear();
        ACCTexturesView.Clear();
        dDelNodes.Clear();
        dDelNodeDisps.Clear();
        dMaskSlots.Clear();
    }
        

    private bool initialize() 
    {
        LogUtil.DebugLog("Initialize ",  Application.loadedLevel);

        uiParams.Update();
        ACCTexturesView.Init(uiParams);

        LogUtil.DebugLogF("maid count:{0}, man count:{1}", 
                          GameMain.Instance.CharacterMgr.GetMaidCount(),
                          GameMain.Instance.CharacterMgr.GetManCount());

        if (holder.UpdateMaid()) {
            ClearMaidData();
        }
        if (holder.maid == null) return false;

//            dMaterial.Clear();
//            foreach (string slotname in ACConstants.Slotnames.Keys) {
//                dMaterial.Add(slotname, new CCMaterial());
//            }

        return initGUI();
    }

    private bool initGUI() 
    {
        return true;
    }
    #endregion

    private void DoMainMenu(int winID)
    {
        GUI.Label(uiParams.labelRect, "強制カラーチェンジ", uiParams.lStyleB);

        var outRect = uiParams.subRect;
        if (GUI.Button(outRect, "マスク選択", uiParams.bStyle)) {
            menuType = MenuType.MaskSelect;
            InitMaskSlots();
        }
        outRect.y += uiParams.unitHeight;
        if (GUI.Button(outRect, "ノード表示切替え", uiParams.bStyle)) {
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
        outRect.width -= 20;
        scrollViewPosition = GUI.BeginScrollView(uiParams.mainRect, scrollViewPosition, uiParams.mainConRect);
        try {
            foreach (SlotInfo slot in ACConstants.SlotNames.Values) {
                //if (slot.Id == TBody.SlotID.end) continue;
    
                // FIXME メイド以外の項目については別の方法で可視性を取得する必要あり
                GUI.enabled = holder.maid.body0.GetSlotVisible(slot.Id);

                // TODO ボタンごと非表示とする場合は、全体のサイズも変更する必要あり
//                if (!holder.maid.body0.GetSlotVisible(slot.Id)) {
//                    continue;
//                }

                if (GUI.Button(outRect, slot.DisplayName, uiParams.bStyle)) {
                    holder.currentSlot = slot;
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

    // TODO 変更前の情報をメモリ上に保持

    private List<ACCMaterialsView> initMaterialView(Material[] materials) {
        var ret = new List<ACCMaterialsView>(materials.Length);
        foreach (Material material in materials) { 
            ret.Add(new ACCMaterialsView(material, uiParams));
        }
        return ret;
    }

    private void DoColorMenu(int winID)
    {
        var scrollRect = uiParams.colorRect;
        GUI.Label(uiParams.labelRect, "強制カラーチェンジ: " + holder.currentSlot.DisplayName, uiParams.lStyleB);

        var outRect = uiParams.subRect;
        try {
            // ターゲットのmenuファイルが変更された場合にビューを更新
            string menu = holder.GetCurrentMenuFile();
            if (targetMenu != menu) {
                LogUtil.DebugLog("menufile changed.", targetMenu, "=>", menu);
    
                targetMenu = menu;
                TBodySkin slot = holder.GetCurrentSlot();
                targetMaterials = holder.GetMaterials(slot);
                materialViews = initMaterialView(targetMaterials);
            }
    
            if ( GUI.Button(outRect, "テクスチャ変更", uiParams.bStyle) ) {
                texViews = InitTexView(targetMaterials);

                menuType = MenuType.Texture;
                return;
            }
    
            outRect.y = 0;
            outRect.width -= uiParams.margin * 2 + 20;
            if (targetMaterials.Length > 0) {
                // 表示アイテム数に基づいて、スクロール内の領域を算出
                int itemCount = 0;
                foreach (ACCMaterialsView view in materialViews) {
                    itemCount += view.itemCount();
                }
                var conRect = new Rect(0, 0, scrollRect.width - 20, 
                                       uiParams.unitHeight * itemCount + uiParams.margin);
                scrollViewPosition = GUI.BeginScrollView(scrollRect, scrollViewPosition, conRect);
                try {
                    foreach (ACCMaterialsView view in materialViews) {
                        view.Show(ref outRect);
                    }
                } finally {
                    GUI.EndScrollView();
                }
            }
    
            outRect.x = uiParams.margin;
            outRect.y = uiParams.winRect.height - (uiParams.unitHeight) * 2;
            outRect.width = uiParams.subRect.width;
            GUI.enabled = (targetMenu != null);
            try {
                if (GUI.Button(outRect, "menu/mate保存", uiParams.bStyle)) {
                    global::TBodySkin slot = holder.GetCurrentSlot();
                    if (slot.obj == null) return;
        
                    // propは対応するMPNを指定
                    MaidProp prop = holder.maid.GetProp(holder.currentSlot.mpn);
                    if (prop != null) {
                        if (prop.strFileName.EndsWith(FileConst.EXT_MOD, StringComparison.CurrentCulture)) {
                            var msg = "modファイルの変更/保存は現在未対応です " + prop.strFileName;
                            NUty.WinMessageBox(NUty.GetWindowHandle(), msg, "エラー", NUty.MSGBOX.MB_OK);
                        } else {
                            targetMenuInfo = new MenuInfo();
                            bool loaded = targetMenuInfo.LoadMenufile(prop.strFileName);
                            // 変更可能なmenuファイルがない場合は保存画面へ遷移しない
                            if (!loaded) {
                                var msg = "変更可能なmenuファイルがありません " + prop.strFileName;
                                NUty.WinMessageBox(NUty.GetWindowHandle(), msg, "エラー", NUty.MSGBOX.MB_OK);
                            }
                            showSaveDialog |= loaded;
                        }
                    }
                }
            } finally {
                GUI.enabled = true;
            }

        } catch (Exception e) {
            LogUtil.ErrorLog("強制カラーチェンジ画面でエラーが発生しました。メイン画面へ移動します", e);
            menuType = MenuType.Main;
        } finally {
            outRect.y += uiParams.unitHeight;
            if (GUI.Button(outRect, "閉じる", uiParams.bStyle)) {
                menuType = MenuType.Main;
            }
            GUI.DragWindow();
        }
    }

    private List<ACCTexturesView> InitTexView(Material[] materials) {

        var ret = new List<ACCTexturesView>(materials.Length);
        int matNo = 0;
        foreach (Material material in materials) {
            try {               
                ret.Add(new ACCTexturesView(material, matNo++, uiParams));
            } catch(Exception e) {
                LogUtil.ErrorLog(material.name, e);
            }
        }
        return ret;
    }

    private void DoSelectTexture(int winId)
    {
        // テクスチャ変更画面 GUILayout使用
        GUILayout.Label("テクスチャ変更", uiParams.lStyleB);
        scrollViewPosition = GUILayout.BeginScrollView(scrollViewPosition, 
                                                       GUILayout.Width(uiParams.textureRect.width),
                                                       GUILayout.Height(uiParams.textureRect.height));
        try {
            string menu = holder.GetCurrentMenuFile();
            if (targetMenu != menu) {
                LogUtil.DebugLogF("manufile changed. {0}=>{1}", targetMenu, menu);
                targetMenu = menu;
                targetMaterials = holder.GetMaterials();
                texViews = InitTexView(targetMaterials);
            }

            foreach (ACCTexturesView view in texViews) {
                view.Show();
            }
        } catch(Exception e) {
            LogUtil.ErrorLog("failed to create texture change view", e);
        } finally {
            GUILayout.EndScrollView();

            if (GUILayout.Button( "閉じる", uiParams.bStyle, 
                                 GUILayout.Width(uiParams.closeRect.width), GUILayout.Height(uiParams.closeRect.height))) {
                menuType = MenuType.Color;
            }
            GUI.DragWindow();
        }
    }

    private void DoFileBrowser(int winId)
    {
        ACCTexturesView.fileBrowser.OnGUI();
        GUI.DragWindow();
    }


    private bool InitMaskSlots() {
        if (holder.maid == null) return false;

//        List<int> maskSlots = holder.maid.listMaskSlot;

        foreach (SlotInfo si  in ACConstants.SlotNames.Values) {
            if (!si.maskable) continue;

            TBodySkin slot = holder.maid.body0.GetSlot((int)si.Id);
            MaskInfo mi;
            if (!dMaskSlots.TryGetValue(si.Id, out mi)) {
                mi = new MaskInfo(si, slot);
                dMaskSlots[si.Id] = mi;
            } else {
                mi.slot = slot;
            }
            mi.value = slot.boVisible;
        }
        return true;
    }

    private void DoMaskSelectMenu(int winID)
    {
        GUILayoutOption bHeight = GUILayout.Height(uiParams.closeRect.height);
        GUILayoutOption bWidth = GUILayout.Width(uiParams.closeRect.width*0.33f);
        GUILayout.BeginVertical();
        try {
            // falseがマスクの模様
            GUILayout.Label("マスクアイテム選択", uiParams.lStyleB);
    
            if (holder.maid == null) return ;

            // 身体からノード一覧と表示状態を取得
            var outRect = uiParams.subRect;
            if (!dMaskSlots.Any()) {
                InitMaskSlots();
            }

            GUILayout.BeginHorizontal();
            try {
                if (GUILayout.Button("同期", uiParams.bStyle, bHeight, bWidth)) { 
                    InitMaskSlots();
                }
                if (GUILayout.Button("すべてON", uiParams.bStyle, bHeight, bWidth)) {
                    var keys = new List<TBody.SlotID>(dMaskSlots.Keys);
                    foreach (TBody.SlotID key in keys) {
                        dMaskSlots[key].value = false;
                    }
                }
                if (GUILayout.Button("すべてOFF", uiParams.bStyle, bHeight, bWidth)) { 
                    var keys = new List<TBody.SlotID>(dMaskSlots.Keys);
                    foreach (TBody.SlotID key in keys) {
                        dMaskSlots[key].value = true;
                    }
                }
                // 下着モード,水着モード,ヌードモード
            } finally {
                GUILayout.EndHorizontal();
            }
            scrollViewPosition = GUILayout.BeginScrollView(scrollViewPosition, 
                                                           GUILayout.Width(uiParams.nodeSelectRect.width),
                                                           GUILayout.Height(uiParams.nodeSelectRect.height));
            GUIStyle labelStyle = uiParams.lStyle;
            Color bkColor = labelStyle.normal.textColor;
            try {
                foreach (KeyValuePair<TBody.SlotID, MaskInfo> pair in dMaskSlots) {
                    if (pair.Key <= TBody.SlotID.eye) continue;
                    GUILayout.BeginHorizontal();
                    try {
                        MaskInfo maskInfo = pair.Value;
                        string state;
                        // 下着、ヌードモードなどによる非表示
                        if (!holder.maid.body0.GetMask(maskInfo.slotInfo.Id)) {
                            state = "[非表示]";
                            labelStyle.normal.textColor = Color.magenta;
                        } else {
                            maskInfo.UpdateState();
                            switch(maskInfo.state) {
                                case SlotState.NotLoaded:
                                    state = "[未読込]";
                                    labelStyle.normal.textColor = Color.red;
                                    GUI.enabled = false;
                                    break;
                                case SlotState.Masked:
                                    state = "[マスク]";
                                    labelStyle.normal.textColor = Color.cyan;
                                    break;
                                case SlotState.Displayed:
                                    state = "[表示中]";
                                    labelStyle.normal.textColor = bkColor;
                                    break;
                                default:
                                    state = "unknown";
                                    labelStyle.normal.textColor = Color.red;
                                    GUI.enabled = false;
                                    break;
                            }
                        }
                        GUILayout.Label(state, labelStyle);
                        // dMaskSlotsはCM3D2のデータと合わせてマスクオン=falseとし、画面上はマスクオン=選択(true)とする
                        maskInfo.value = !GUILayout.Toggle( !maskInfo.value, maskInfo.slotInfo.DisplayName, 
                                                           uiParams.tStyle, uiParams.contentWidth);
                        GUI.enabled = true;
                    } finally {
                        GUILayout.EndHorizontal();
                    }
                }
            } finally {
                uiParams.lStyle.normal.textColor = bkColor;
                GUI.EndScrollView();
            }
        } finally {
            GUILayout.EndVertical();
        }

        GUILayout.BeginHorizontal();
        try {
            if (GUILayout.Button("適用", uiParams.bStyle, bWidth, bHeight)) {
                holder.SetSlotVisibles(dMaskSlots);
            }
            if (GUILayout.Button("全クリア", uiParams.bStyle, bWidth, bHeight)) {
                holder.SetAllVisible();
            }
            if (GUILayout.Button("戻す", uiParams.bStyle, bWidth, bHeight)) {
                holder.FixFlag();
            }
        } finally {
            GUILayout.EndHorizontal();
        }

        if (GUILayout.Button( "閉じる", uiParams.bStyle, GUILayout.Width(uiParams.closeRect.width), bHeight)) {
            menuType = MenuType.Main;
        }

        GUI.DragWindow();
    }

    
    private bool InitDelNode(TBodySkin body) {
        if (body == null) {
            if (holder.maid == null) return false;

            // 身体からノード一覧と表示状態を取得
            body = holder.maid.body0.GetSlot(TBody.SlotID.body.ToString());
        }

        dDelNodes.Clear();
        Dictionary<string, bool> dic = body.m_dicDelNodeBody;
        foreach (string key in ACConstants.NodeNames.Keys) {
            if (dic.ContainsKey(key)) dDelNodes[key] = true;
        }

        // 有効なスロットを走査し、DelNodeフラグが一つでもFalseのものをサーチ
        var keys = new List<string>(dDelNodes.Keys);
        foreach(TBodySkin slot in holder.maid.body0.goSlot) {
            if (slot.obj != null && slot.boVisible) {
                Dictionary<string, bool> slotNodes = slot.m_dicDelNodeBody;
                // 1つでもFalseがあったら非表示とみなす
                foreach (string key in keys) {                    
                    bool v;
                    if (slotNodes.TryGetValue(key, out v)) {
                        dDelNodes[key] &= v;
                    }
                }
                if (!slot.m_dicDelNodeParts.Any()) continue;

                foreach(Dictionary<string, bool> sub in slot.m_dicDelNodeParts.Values) {
                    foreach(KeyValuePair<string, bool> pair in sub) {
                        if (dDelNodes.ContainsKey(pair.Key)) {
                            dDelNodes[pair.Key] &= pair.Value;
                        }
                    }
                }
            }
        }
        dDelNodeDisps = new Dictionary<string, bool>(dDelNodes);
        return true;
    }

    private void DoNodeSelectMenu(int winID)
    {
        GUILayoutOption bHeight = GUILayout.Height(uiParams.closeRect.height);
        GUILayout.BeginVertical();
        TBodySkin body;
        try {
            GUILayout.Label("表示ノード選択", uiParams.lStyleB);
    
            if (holder.maid == null) return ;

            // 身体からノード一覧と表示状態を取得
            body = holder.maid.body0.GetSlot(TBody.SlotID.body.ToString());
            var outRect = uiParams.subRect;
            if (!dDelNodes.Any()) {
                InitDelNode(body);
            }

            GUILayout.BeginHorizontal();
            try {
                GUILayoutOption bWidth = GUILayout.Width(uiParams.closeRect.width*0.33f);
                if (GUILayout.Button("同期", uiParams.bStyle, bHeight, bWidth)) { 
                    InitDelNode(body);
                }
                if (GUILayout.Button("すべてON", uiParams.bStyle, bHeight, bWidth)) {
                    var keys = new List<string>(dDelNodes.Keys);
                    foreach (string key in keys) {
                        dDelNodes[key] = true;
                    }
                }
                if (GUILayout.Button("すべてOFF", uiParams.bStyle, bHeight, bWidth)) { 
                    var keys = new List<string>(dDelNodes.Keys);
                    foreach (string key in keys) {
                        dDelNodes[key] = false;
                    }
                }

            } finally {
                GUILayout.EndHorizontal();
            }
            scrollViewPosition = GUILayout.BeginScrollView(scrollViewPosition, 
                                                           GUILayout.Width(uiParams.nodeSelectRect.width),
                                                           GUILayout.Height(uiParams.nodeSelectRect.height));
            GUIStyle labelStyle = uiParams.lStyle;
            Color bkColor = labelStyle.normal.textColor;
            try {
                foreach (KeyValuePair<string, string> pair in ACConstants.NodeNames) {
                    GUILayout.BeginHorizontal();
                    try {
                        string state;
                        bool isValid = true;
                        bool bDel;
                        if (dDelNodeDisps.TryGetValue(pair.Key, out bDel)) {
                            if (bDel) {
                                state = "[表示中]";
                                labelStyle.normal.textColor = bkColor;
                            } else {
                                state = "[非表示]";
                                labelStyle.normal.textColor = Color.magenta;
                            }
                        } else {
                            state = "[不　明]";
                            labelStyle.normal.textColor = Color.red;
                            isValid = false;
                        }
                        GUILayout.Label(state, labelStyle);
                        GUI.enabled = isValid;
                        dDelNodes[pair.Key] = GUILayout.Toggle( dDelNodes[pair.Key], pair.Value, uiParams.tStyle, uiParams.contentWidth);
                        GUI.enabled = true;
                    } finally {
                        GUILayout.EndHorizontal();
                    }
                }
            } finally {
                uiParams.lStyle.normal.textColor = bkColor;
                GUI.EndScrollView();
            }
        } finally {
            GUILayout.EndVertical();
        }
        if (GUILayout.Button("適用", uiParams.bStyle, GUILayout.Width(uiParams.closeRect.width), bHeight)) {
            holder.SetDelNodes(dDelNodes, true);
            dDelNodeDisps = new Dictionary<string, bool>(body.m_dicDelNodeBody);
        }
        if (GUILayout.Button( "閉じる", uiParams.bStyle, GUILayout.Width(uiParams.closeRect.width), bHeight)) {
            menuType = MenuType.Main;
        }

        GUI.DragWindow();
    }

    private void DoSaveMenu(int winID)
    {
        GUI.Label(uiParams.labelRect, "保存", uiParams.lStyleB);

        var outRect = uiParams.subRect;
        outRect.width = uiParams.winRect.width * 0.3f - uiParams.margin;

        GUI.Label(outRect, "プリセット名", uiParams.lStyleB);
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
                // 名無しはNG
                if (presetName.Length == 0) return;
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

        GUI.Label(uiParams.labelRect, "プリセット適用", uiParams.lStyleB);

        var outRect = uiParams.subRect;
        conRect.height += (uiParams.unitHeight) * presets.Count + uiParams.margin * 2;
        outRect.y = 0;
        outRect.x = uiParams.margin * 2;

        scrollViewPosition = GUI.BeginScrollView(scrollRect, scrollViewPosition, conRect);
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
        holder.SetDelNodes(dDelNodes, false);

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
            Material[] materials = holder.GetMaterials(slotName);
            int i=0;
            foreach (CCMaterial cmat in ccslot.materials.Values) {
                if (i < materials.Length) {
                    Shader sh = Shader.Find(cmat.shader);
                    if (sh != null) {
                        materials[i].shader = sh;
                        materials[i].color  = cmat.color;
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
            holder.SetAllVisible();
        } else {
            holder.FixFlag();
        }
        bApplyChange = false;
    }

    private void LoadPresets() {
        if (!File.Exists(SaveFileName)) return;
        presets = presetMgr.Load(SaveFileName);
    }

    private ACCSaveModView saveView;
    private void DoSaveModDialog(int winId) {
        if (targetMenuInfo == null ) {
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

        foreach ( TargetMaterial tm in targetMenuInfo.materials) {
            outRect.x = uiParams.margin;
            outRect.width = uiParams.modalRect.width * 0.2f - uiParams.margin;
            GUI.Label(outRect, "マテリアル" + tm.matNo, uiParams.lStyle);

            outRect.x += outRect.width;
            outRect.width = uiParams.modalRect.width * 0.8f - uiParams.margin;
            tm.editname = GUI.TextField(outRect, tm.editname, uiParams.textStyle);
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
            var menufile = targetMenuInfo.filename + FileConst.EXT_MENU;
            if (FileExists(menufile)) {
                NUty.WinMessageBox(NUty.GetWindowHandle(), "メニューファイル[" + menufile+ "]が既に存在します", "エラー", NUty.MSGBOX.MB_OK);
                return;
            }
            foreach (var mat in targetMenuInfo.materials) {
                var matefile = mat.editname + FileConst.EXT_MATERIAL;
                if (FileExists(matefile)) {
                    NUty.WinMessageBox(NUty.GetWindowHandle(), "materialファイル[" + matefile + "]が既に存在します", "エラー", NUty.MSGBOX.MB_OK);
                    return;
                }
            }
            if (SaveMod(holder.currentSlot)) {
                showSaveDialog = false;
            }
        }
        outRect.x += outRect.width + uiParams.margin;

        showSaveDialog &= !(GUI.Button(outRect, "閉じる", uiParams.bStyle));;
        
        GUI.DragWindow();
    }

    private bool SaveMod(SlotInfo si)
    {
        TBodySkin slot = holder.maid.body0.GetSlot((int)si.Id);
        if (slot.obj == null) return false;

        // TODO 画面で変更された情報を元にファイル重複がないかチェック
        // menu, model, mate, tex
        

        MenuInfo menuInfo = targetMenuInfo;

        MaidProp prop = holder.maid.GetProp(si.mpn);
        String baseMenufile = prop.strFileName;

        string outDir = outUtil.GetACCDirectory();
        outDir = Path.Combine(outDir, menuInfo.filename);
        if (!Directory.Exists(outDir)) Directory.CreateDirectory(outDir);

        LogUtil.DebugLog("output path", outDir);

        try {
            // シェーダの変更されたマテリアルを抽出、matNoとmaterialのセット
            // TODO 置換すべき情報を抽出

            // menu
            MenuWrite(baseMenufile, outDir, menuInfo);
    
            // material
            foreach ( List<TargetMaterial> tmList in menuInfo.baseMaterials.Values) {
                // カテゴリとマテリアル番号でマテリアルオブジェクトを参照
                // menuInfo.materials[i];
                foreach (TargetMaterial tm in tmList) {
                    MateWrite(outDir, tm.filename, tm.editname, slot.obj);
                }
            }
    
            // model
            for (int i = 0; i < menuInfo.baseAddItems.Count(); i++) {
                if (!FileExists(menuInfo.addItems[i][0] + FileConst.EXT_MODEL)) {
                    ModelWrite(outDir, menuInfo.baseAddItems[i][0], menuInfo.addItems[i][0]);
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
    
            // TODO アイコンもカラー変更可能にすべきか？ _mainTexから変更情報を抽出
            // icon
            if (!FileExists(menuInfo.icons + FileConst.EXT_TEXTURE)) {
                TexWrite(outDir, menuInfo.baseIcons, menuInfo.icons);
            }

        } catch(Exception e) {
            var msg = "保存に失敗しました。\n  " + e.Message;
            NUty.WinMessageBox(NUty.GetWindowHandle(), msg, "エラー", NUty.MSGBOX.MB_OK);
            return false;
        }
        return true;
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

    private void MenuWrite(string basemenufile, string outDir, MenuInfo menu)
    {
        byte[] cd = null;
        try {
            using (AFileBase aFileBase = global::GameUty.FileOpen(basemenufile)) {
                if (!aFileBase.IsValid()) {
                    var msg = "アイテムメニューファイルが見つかりません。"+ basemenufile;
                    LogUtil.ErrorLog(msg);
                    throw new ACCException(msg);
                }
                cd = aFileBase.ReadAll();
            }
        } catch (Exception ex2) {
            var msg = "アイテムメニューファイルが読み込めませんでした。"+ basemenufile;
            LogUtil.ErrorLog(msg, ex2);
            throw new ACCException(msg, ex2);
        }

        var materials = new Dictionary<string, string>();
        try {
            //.menuの保存
            using (var headerMs = new MemoryStream())
            using (var dataMs = new MemoryStream())
            using (var headerWriter = new BinaryWriter(headerMs))
            using (var dataWriter = new BinaryWriter(dataMs)) 
            using (var binaryReader = new BinaryReader(new MemoryStream(cd), Encoding.UTF8)) {
                string text = binaryReader.ReadString();
                if (text != FileConst.HEAD_MENU) {
                    var msg = "ヘッダーファイルが不正です。ヘッダ=" + text + ", file=" + basemenufile;
                    LogUtil.ErrorLog(msg);
                    throw new ACCException(msg);
                }
                headerWriter.Write(text);
                int num = binaryReader.ReadInt32();
                headerWriter.Write(num);
                string txtpath = binaryReader.ReadString();
                int pos = txtpath.LastIndexOf("/", StringComparison.CurrentCulture);
                if (pos >= 0) {
                    txtpath = txtpath.Substring(0, pos + 1) + Path.GetFileNameWithoutExtension(basemenufile) + ".txt";
                }
                headerWriter.Write(txtpath);
                string menuName = binaryReader.ReadString(); // 名前を置き換え
                headerWriter.Write(menu.name);
                string category = binaryReader.ReadString();
                headerWriter.Write(category);
                string comment = binaryReader.ReadString();
                headerWriter.Write(menu.setumei.Replace("\n", FileConst.RET));
                int num2 = (int)binaryReader.ReadInt32();

                bool materialWrited = false;
                bool addItemWrited = false;

                while (true) {
                    byte b = binaryReader.ReadByte();
                    int size = (int)b;
                    if (size == 0) {
                        dataWriter.Write((byte)0);
                        break;
                    }
                    var param = new string[size];
                    for (int i = 0; i < size; i++) {
                        param[i] = binaryReader.ReadString();
                    }

                    switch (param[0]) {
                    case "name":
                        param[1] = menu.name;
                        break;
                    case "setumei":
                        param[1] = menu.setumei.Replace("\n", FileConst.RET);
                        break;
                    case "priority":
                        param[1] = "9999";
                        break;
                    case "icons":
                        param[1] = menu.icons + FileConst.EXT_TEXTURE;
                        break;
                    case "マテリアル変更":
                        if (!materialWrited) {
                            materialWrited = true;
                            foreach (var mat in menu.materials) {
                                dataWriter.Write(b);
                                dataWriter.Write("マテリアル変更");
                                dataWriter.Write(mat.category);
                                dataWriter.Write(mat.matNo.ToString());
                                dataWriter.Write(mat.editname + FileConst.EXT_MATERIAL);
                            }
                        }
                        continue;
                    case "additem":
                        if (!addItemWrited) {
                            addItemWrited = true;
                            foreach (var items in menu.addItems) {
                                dataWriter.Write(b);
                                dataWriter.Write("additem");
                                dataWriter.Write(items[0] + FileConst.EXT_MODEL);
                                for (int i = 1; i < items.Length; i++)
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
                using (var writer = new BinaryWriter(File.OpenWrite(Path.Combine(outDir, menu.filename + FileConst.EXT_MENU)))) {
                    writer.Write(headerMs.ToArray());
                    writer.Write((int)dataMs.Length);
                    writer.Write(dataMs.ToArray());
                }
            }
        } catch (Exception e) {
            Debug.Log(e);
            throw new ACCException(null, e);
        }
    }

    private void TexWrite(string path, string infile, string outname)
    {
        string filename = infile + FileConst.EXT_TEXTURE;
        try {
            using (AFileBase aFileBase = global::GameUty.FileOpen(filename)) {
                if (!aFileBase.IsValid()) {
                    LogUtil.ErrorLog("テクスチャファイルが見つかりません。", filename);
                    return;
                }
                byte[] cd = aFileBase.ReadAll();
                using (var writer = new BinaryWriter(File.OpenWrite(Path.Combine(path, outname + FileConst.EXT_TEXTURE)))) {
                    writer.Write(cd);
                }
            }
        } catch (Exception ex2) {
            LogUtil.ErrorLog("テクスチャファイルが読み込めませんでした。", filename, ex2.Message);
        }
    }

    private void ModelWrite(string path, string infile, string outname)
    {
        string filename = infile + FileConst.EXT_MODEL;
        try {
            using (AFileBase infileBase = global::GameUty.FileOpen(filename)) {
                if (!infileBase.IsValid()) {
                    LogUtil.ErrorLog("入力ファイルが見つかりません。", infile);
                    return;
                }

                string outfile = Path.Combine(path, outname + FileConst.EXT_MODEL);
                outUtil.CopyModel(infileBase, outfile);
                // TODO Shader変更
            }

        } catch (Exception ex2) {
            LogUtil.ErrorLog("Modelファイルが読み込めませんでした。", filename, ex2.Message);
        }
    }

    private void MateWrite(string path, string infile, string outname, GameObject gobj)
    {
        LogUtil.DebugLog("output material file", infile, outname);
        byte[] cd = null;
        if (!infile.EndsWith(FileConst.EXT_MATERIAL, StringComparison.CurrentCulture)) {
            infile += FileConst.EXT_MATERIAL;
        }
        try {
            using (AFileBase aFileBase = global::GameUty.FileOpen(infile)) {
                if (!aFileBase.IsValid()) {
                    var msg = "materialファイルが見つかりません。"+ infile;
                    LogUtil.ErrorLog(msg);
                    throw new ACCException(msg);
                }
                cd = aFileBase.ReadAll();
            }
        } catch (Exception e) {
            var msg = "materialファイルが読み込めません。"+ infile + ".\n" + e.Message;
            LogUtil.ErrorLog(msg, e);
            throw new ACCException(msg, e);
        }

        try {
            //.mateの保存
            using (var headerMs = new MemoryStream())
            using (var dataMs = new MemoryStream())
            using (var headerWriter = new BinaryWriter(headerMs))
            using (var dataWriter = new BinaryWriter(dataMs)) 
            using (var binReader = new BinaryReader(new MemoryStream(cd), Encoding.UTF8)) {
                string text = binReader.ReadString();
                if (text != FileConst.HEAD_MATE) {
                    var msg = "ファイルヘッダが正しくありません。ヘッダ="+ text +", file="+ infile;
                    LogUtil.ErrorLog(msg);
                    throw new ACCException(msg);
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
                    var msg = "mateファイルのmaterial nameに対応するマテリアルが見つかりません"+ name2;
                    LogUtil.ErrorLog(msg);
                    throw new ACCException(msg);
                }
                headerWriter.Write(name2);
                string shader1 = binReader.ReadString();
                headerWriter.Write(shader1);
                string shader2 = binReader.ReadString();
                headerWriter.Write(shader2);

                while (true) {
                    string key = binReader.ReadString();
                    dataWriter.Write(key);
                    if (key == "end") break;

                    string propertyName = binReader.ReadString();
                    dataWriter.Write(propertyName);
                    switch(key) {
                    case "tex":
                        string type = binReader.ReadString();
                        dataWriter.Write(type);
                        switch (type) {
                        case "null":
                            break;
                        case "tex2d":
                            string tex = binReader.ReadString();
                            dataWriter.Write(tex);
                            string asset = binReader.ReadString();
                            dataWriter.Write(asset);
                            dataWriter.Write(binReader.ReadSingle());
                            dataWriter.Write(binReader.ReadSingle());
                            dataWriter.Write(binReader.ReadSingle());
                            dataWriter.Write(binReader.ReadSingle());
                            break;
                        case "texRT":
                            string text7 = binReader.ReadString();
                            dataWriter.Write(text7);
                            string text8 = binReader.ReadString();
                            dataWriter.Write(text8);
                            break;
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
                using (var writer = new BinaryWriter(File.OpenWrite(Path.Combine(path, outname + FileConst.EXT_MATERIAL)))) {
                    writer.Write(headerMs.ToArray());
                    writer.Write(dataMs.ToArray());
                }
            }
        } catch (ACCException e) {
            throw;
        } catch (Exception e) {
            var msg = "materialファイルの出力に失敗しました。 " + outname;
            LogUtil.ErrorLog(msg, e);
            throw new ACCException(msg, e);
        }
    }

}
public class UIParams {
    private static UIParams instance = new UIParams();        
    public static UIParams Instance {
        get { return instance;  }
    }
    #region Constants
    private const int marginPx     = 2;
    private const int fontPx       = 14;
    private const int itemHeightPx = 18;
    private const int fontPx2 = (int)(fontPx*0.9f);
    #endregion

    private int width;
    private int height;
    private float ratio;
    
    public int margin;
    public int fontSize;
    public int fontSize2;
    public int itemHeight;
    public int unitHeight;
    public readonly GUIStyle lStyleB   = new GUIStyle("label");
    public readonly GUIStyle lStyle    = new GUIStyle("label");
    public readonly GUIStyle lStyleC   = new GUIStyle("label");
    public readonly GUIStyle bStyle    = new GUIStyle("button");
    public readonly GUIStyle bStyle2   = new GUIStyle("button");
    public readonly GUIStyle tStyle    = new GUIStyle("toggle");
    public readonly GUIStyle listStyle = new GUIStyle("list");
    public readonly GUIStyle textStyle = new GUIStyle("textField");
    public readonly GUIStyle textStyle2 = new GUIStyle("textField");
    public readonly GUIStyle textAreaStyle = new GUIStyle("textArea");

    public readonly GUIStyle boxStyle      = new GUIStyle("box");
    public readonly GUIStyle inboxStyle    = new GUIStyle("box");
    public readonly GUIStyle winStyle      = new GUIStyle("box");

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
    public Rect labelRect        = new Rect();
    public Rect subRect          = new Rect();
    public Rect closeRect        = new Rect();
    public GUILayoutOption buttonWidth;
    public GUILayoutOption buttonLWidth;
    public GUILayoutOption contentWidth;
    public GUILayoutOption modalLabelWidth;

    public UIParams() {
        listStyle.onHover.background = listStyle.hover.background = new Texture2D(2, 2);
        listStyle.padding.left = listStyle.padding.right = 4;
        listStyle.padding.top = listStyle.padding.bottom = 1;
        listStyle.normal.textColor = listStyle.onNormal.textColor =
            listStyle.hover.textColor = listStyle.onHover.textColor =
            listStyle.active.textColor = listStyle.onActive.textColor =
            listStyle.focused.textColor = listStyle.onFocused.textColor = Color.white;

        // Bold
        lStyleB.fontStyle        = FontStyle.Bold;
        lStyle.fontStyle         = FontStyle.Normal;
        lStyle.normal.textColor  = textColor;
        lStyleC.fontStyle        = FontStyle.Normal;
        lStyleC.normal.textColor = new Color(0.82f, 0.88f, 1f, 0.98f);

        bStyle.normal.textColor  = textColor;
        bStyle2.normal.textColor = textColor;
        tStyle.normal.textColor        = textColor;
        textStyle.normal.textColor     = textColor;
        textStyle2.normal.textColor    = textColor;
        textAreaStyle.normal.textColor = textColor;

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
        fontSize2  = FixPx(fontPx2);
        unitHeight = margin + itemHeight;

        lStyle.fontSize        = fontSize;
        lStyleC.fontSize       = fontSize;
        lStyleB.fontSize       = fontSize;
        bStyle.fontSize        = fontSize;
        bStyle2.fontSize       = fontSize2;
        tStyle.fontSize        = fontSize;
        listStyle.fontSize     = fontSize2;
        textStyle.fontSize     = fontSize;
        textStyle.fontSize     = fontSize2;
        textAreaStyle.fontSize = fontSize;

        LogUtil.DebugLogF("screen=({0},{1}),margin={2},height={3},ratio={4})", width, height, margin, itemHeight, ratio);

        winStyle.fontSize  = fontSize;
        winStyle.alignment = TextAnchor.UpperRight;            
        InitWinRect();
        InitFBRect();
        InitModalRect();

        // sub
        mainRect.Set(      margin, unitHeight * 5 + margin, winRect.width - margin * 2, winRect.height - unitHeight * 5.5f);
        mainConRect.Set(   0,      0,              mainRect.width - 24, unitHeight * ACConstants.SlotNames.Count + margin * 2);
        textureRect.Set(   margin, unitHeight,     winRect.width - margin * 2, winRect.height - unitHeight * 2.5f);
        float baseWidth = textureRect.width - 20;
        buttonWidth  = GUILayout.Width(baseWidth * 0.09f);
        buttonLWidth = GUILayout.Width(baseWidth * 0.2f);
        contentWidth = GUILayout.MaxWidth(baseWidth * 0.69f);
        nodeSelectRect.Set(margin, unitHeight * 2, winRect.width - margin * 2, winRect.height - unitHeight * 4.5f);

        colorRect.Set(     margin, unitHeight * 2, winRect.width - margin * 3, winRect.height - unitHeight * 4);
        labelRect.Set(     0,      0,              winRect.width - margin * 2, itemHeight*1.2f);
        subRect.Set(       0,      itemHeight,     winRect.width - margin * 2, itemHeight);
        closeRect.Set(     margin, winRect.height - unitHeight,  winRect.width - margin * 2, itemHeight);

        // modal
        modalLabelWidth = GUILayout.Width(modalRect.width * 0.2f);
        
    }

    public void InitWinRect() {
        winRect.Set(        width - FixPx(290),     FixPx(48),               FixPx(280), height - FixPx(150));
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

public enum SlotState {
    Displayed,
    NonDisplay,
    Masked,
    NotLoaded,
}
public class MaskInfo {
    public readonly SlotInfo slotInfo;
    public TBodySkin slot;
    public SlotState state;
    public bool value;
    public MaskInfo(SlotInfo si, TBodySkin slot) {
        this.slotInfo = si;
        this.slot = slot;
    }

    public void UpdateState() {
        if (slot.obj == null) {
            state = SlotState.NotLoaded;
        } else if (!slot.boVisible) {
            state = SlotState.Masked;
        } else {
            state = SlotState.Displayed;
        }
    }
}
}