using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityInjector.Attributes;
using CM3D2.AlwaysColorChangeEx.Plugin.Data;
using CM3D2.AlwaysColorChangeEx.Plugin.UI;
using CM3D2.AlwaysColorChangeEx.Plugin.Util;

[assembly: AssemblyVersion("1.0.*")]
namespace CM3D2.AlwaysColorChangeEx.Plugin {
[PluginFilter("CM3D2x64"),
 PluginFilter("CM3D2x86"),
 PluginFilter("CM3D2VRx64"),
 PluginFilter("CM3D2OHx86"),
 PluginFilter("CM3D2OHx64"),
 PluginFilter("CM3D2OHVRx64"),
 PluginName("CM3D2 AlwaysColorChangeEx"),
 PluginVersion("0.2.0.0")]
class AlwaysColorChangeEx : UnityInjector.PluginBase
{
    // プラグイン名
    public static volatile string PluginName;
    // プラグインバージョン
    public static volatile string Version;
    static AlwaysColorChangeEx() {
        // 属性クラスからプラグイン名取得
        try {
            var att = Attribute.GetCustomAttribute( typeof(AlwaysColorChangeEx), typeof( PluginNameAttribute ) ) as PluginNameAttribute;
            if( att != null ) PluginName = att.Name;
        } catch( Exception e ) {
            LogUtil.ErrorLog( e );
        }        
        // プラグインバージョン取得
        try {
            // 属性クラスからバージョン番号取得
            var att = Attribute.GetCustomAttribute( typeof(AlwaysColorChangeEx), typeof( PluginVersionAttribute ) ) as PluginVersionAttribute;
            if( att != null ) Version = att.Version;
        } catch( Exception e ) {
            LogUtil.ErrorLog( e );
        }
    }
    private enum TargetLevel {
        SceneDance_DDFL = 4,         // ダンス:ドキドキ☆Fallin' Love
        SceneEdit = 5,               // エディット
        SceneUserEdit = 12,          // 男エディット
        SceneYotogi = 14,            // 夜伽
        SceneADV = 15,               // ADVパート
        SceneDance_ETYL = 20,        // ダンス:entrance to you
        SceneDance_SCL_Release = 22, // ダンス:scarlet leap
        SceneFreeModeSelect    = 24, // イベント回想
        SceneDance_SMT_Release = 26, // ダンス:stellar my tears
        ScenePhotoMode = 27,　　　　  // 撮影モード
    }

    private enum MenuType {
        None,
        Main,
        Color,
        NodeSelect,
        MaskSelect,
        Save,
        PresetSelect,
        Texture,
        MaidSelect,
    }
    private const string TITLE_LABEL = "ACC Ex : ";
    private const int WINID_MAIN   = 20201;
    private const int WINID_DIALOG = WINID_MAIN+1;

    #region Variables
    private float fPassedTime     = 0f;
    private float fLastInitTime   = 0f;
    private bool initialized      = false;
//    private int sceneLevel;
//    private bool visible;

    private bool cmrCtrlChanged = false;
    private bool mouseDowned    = false;
    private bool cursorContains = false;

    private Settings settings = Settings.Instance;
    private FileUtilEx outUtil = FileUtilEx.Instance;

    private PresetManager presetMgr = new PresetManager();
    private readonly UIParams uiParams = UIParams.Instance;
    private MaidHolder holder = MaidHolder.Instance;
   
    private MenuType menuType;

    // プリセット名
    private List<string> presetNames = new List<string>();
    // 操作情報
    private Dictionary<string, bool> dDelNodes     = new Dictionary<string, bool>();
    // 表示中状態
    private Dictionary<string, bool> dDelNodeDisps = new Dictionary<string, bool>();
    private Dictionary<TBody.SlotID, MaskInfo> dMaskSlots = new Dictionary<TBody.SlotID, MaskInfo>();
    PresetData currentPreset;
    private string presetName = string.Empty;
    private bool bPresetCastoff = true;
    private bool bPresetApplyNode = true;
    private bool bPresetApplyMask = true;
    private bool bPresetApplyBody = true;
    private bool bPresetApplyWear = true;
    private Maid toApplyPresetMaid = null;

    private bool isSavable;
    private bool isActive;
    private bool texSliderUpped;
    private const int CHANGE_THRESHOLD = 10;
    private int changeCount = 0;

    private Vector2 scrollViewPosition = Vector2.zero;
    // 表示名切り替え
    private bool switchedName;

    // テクスチャ変更用
    //  現在のターゲットのslotに関するメニューが変更されたらGUIを更新。それ以外は更新しない
    private string targetMenu;
    private Material[] targetMaterials;
    private List<ACCMaterialsView> materialViews;
    private List<ACCTexturesView> texViews;
    private ACCSaveMenuView saveView;

    #endregion

    #region MonoBehaviour methods
    public void Awake() 
    {
        // Sybarisのチェック
        var dllpath = Path.Combine(DataPath, @"..\..\opengl32.dll");
        var dirPath = Path.Combine(DataPath, @"..\..\Sybaris");
        if (File.Exists(dllpath) && Directory.Exists(dirPath)) {
            dirPath = Path.GetFullPath(dirPath);
            settings.presetDirPath = Path.Combine(dirPath, @"Plugins\UnityInjector\Config\ACCPresets");
        } else {
            settings.presetDirPath = Path.Combine(DataPath, "ACCPresets");
        }

        base.ReloadConfig();
        settings.Load((key) => base.Preferences["Config"][key].Value);

        LogUtil.Log("PresetDir:", settings.presetDirPath);

        MigratePresets();
        LoadPresetList();
        uiParams.Update();
    }
    public void OnDestroy() 
    {
        SetCameraControl(true);
        Dispose();
        presetNames.Clear();
        LogUtil.DebugLog("Destroyed");
    }
    public void OnLevelWasLoaded(int level) 
    {
        fPassedTime = 0f;
        if (!Enum.IsDefined(typeof(TargetLevel), level)) {
            // active -> disactive 
            if (isActive) {
                SetCameraControl(true);
                initialized = false;
                isActive = false;
            }
            return;
        }

        menuType = MenuType.None;
        mouseDowned    = false;
        cursorContains = false;
        isActive = true;
    }
    public void OnGUI()
    {
        if (!isActive) return;

        if (menuType == MenuType.None) return;

        if (Event.current.type == EventType.Layout) {
            if (!holder.CurrentActivated()) {
                // メイド未選択、あるいは選択中のメイドが無効化された場合
                SetMenu(MenuType.MaidSelect);
                uiParams.winRect = GUI.Window(WINID_MAIN, uiParams.winRect, DoSelectMaid, Version, uiParams.winStyle);

            } else if (ACCTexturesView.fileBrowser != null) {
                uiParams.fileBrowserRect = GUI.Window(WINID_DIALOG, uiParams.fileBrowserRect, DoFileBrowser, Version, uiParams.winStyle);

            } else if (saveView != null && saveView.showDialog) {
                uiParams.modalRect = GUI.ModalWindow(WINID_MAIN, uiParams.modalRect, DoSaveModDialog, "menuエクスポート", uiParams.dialogStyle);

            } else {
                switch (menuType) {
                    case MenuType.Main:
                        uiParams.winRect = GUI.Window(WINID_MAIN, uiParams.winRect, DoMainMenu, Version, uiParams.winStyle);
                        break;
                    case MenuType.Color:
                        uiParams.winRect = GUI.Window(WINID_MAIN, uiParams.winRect, DoColorMenu, Version, uiParams.winStyle);
                        break;
                    case MenuType.MaskSelect:
                        uiParams.winRect = GUI.Window(WINID_MAIN, uiParams.winRect, DoMaskSelectMenu, Version, uiParams.winStyle);
                        break;
                    case MenuType.NodeSelect:
                        uiParams.winRect = GUI.Window(WINID_MAIN, uiParams.winRect, DoNodeSelectMenu, Version, uiParams.winStyle);
                        break;
                    case MenuType.Save:
                        uiParams.winRect = GUI.Window(WINID_MAIN, uiParams.winRect, DoSaveMenu, Version, uiParams.winStyle);
                        break;
                    case MenuType.PresetSelect:
                        uiParams.winRect = GUI.Window(WINID_MAIN, uiParams.winRect, DoSelectPreset, Version, uiParams.winStyle);
                        break;
                    case MenuType.Texture:
                        uiParams.winRect = GUI.Window(WINID_MAIN, uiParams.winRect, DoSelectTexture, Version, uiParams.winStyle);
                        break;
                    case MenuType.MaidSelect:
                        uiParams.winRect = GUI.Window(WINID_MAIN, uiParams.winRect, DoSelectMaid, Version, uiParams.winStyle);
                        break;
                    default:
                        break;
                }
            }

            // 領域内でマウスダウン => マウスアップ 操作の場合に入力をリセット
            if (Input.GetMouseButtonUp(0)) {
                if (mouseDowned)  {
                    Input.ResetInputAxes();
                    texSliderUpped = (menuType == MenuType.Texture);
                }
                mouseDowned = false;
            }
            mouseDowned |= cursorContains && Input.GetMouseButtonDown(0);
        } else {
            //Event.current.type == EventType.repaint
            if (ACCTexturesView.fileBrowser != null) {
                ACCTexturesView.fileBrowser.OnGUI();
            }
        }
    }
    public void Update()
    {
        fPassedTime += Time.deltaTime;
        if (!isActive) return;

        if (!initialized) {
            if (fPassedTime - fLastInitTime <= 1f) return;

            fLastInitTime = fPassedTime;
            initialized = Initialize();
            LogUtil.Log("Initialize ", initialized);
            if (!initialized) return;
        }

        if (InputModifierKey() && Input.GetKeyDown(settings.toggleKey)) {
            SetMenu( (menuType == MenuType.None)? MenuType.Main : MenuType.None );
            mouseDowned    = false;
        }
        UpdateCameraControl();

        // 選択中のメイドが有効でなければ何もしない
        if (!holder.CurrentActivated()) return;

        if (toApplyPresetMaid != null && !toApplyPresetMaid.boAllProcPropBUSY) {
            ApplyPresetProp(currentPreset);
        }

        // テクスチャエディットの反映
        if (menuType == MenuType.Texture) {
            // マウスが離されたタイミングでのみテクスチャ反映
            if (texSliderUpped || Input.GetMouseButtonUp(0)) {
                if (ACCTexturesView.IsChangeTarget()) {
                    ACCTexturesView.UpdateTex(holder.currentMaid, targetMaterials);
                }
            }
        } else {
            // テクスチャモードでなければ、テクスチャ変更対象を消す
            ACCTexturesView.ClearTarget();
        }
    }
    private bool InputModifierKey() {
        if (settings.toggleKeyModifier == null) return true;

        // 修飾キーが指定されている場合、そのキーの入力チェック
        foreach (var keyCode in settings.toggleKeyModifier) {
            if (Input.GetKey(keyCode)) {
                return true;
            }
        }
        return false;
    }
    private void SetCameraControl(bool enable) 
    {
        if (cmrCtrlChanged == enable) {
            GameMain.Instance.MainCamera.SetControl(enable);
            UICamera.InputEnable = enable;
            cmrCtrlChanged = !enable;
        }
    }
    /// <summary>
    /// カーソル位置のチェックを行い、カメラコントロールの有効化/無効化を行う
    /// </summary>
    private void UpdateCameraControl() 
    {
        cursorContains = false;
        if (ACCTexturesView.fileBrowser != null) {
            var cursor = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
            cursorContains = uiParams.fileBrowserRect.Contains(cursor);
            if (!cursorContains) {
                if (saveView != null && saveView.showDialog) {
                    cursorContains = uiParams.modalRect.Contains(cursor);
                }
            }
        } else if (menuType != MenuType.None) {
            var cursor = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
            cursorContains = uiParams.winRect.Contains(cursor);
            if (!cursorContains) {
                if (saveView != null && saveView.showDialog) {
                    cursorContains = uiParams.modalRect.Contains(cursor);
                }
            }
        }

        // カメラコントロールの有効化/無効化 (Windowの範囲外では、自身がコントロールを変更したケース以外は更新しない)
        if (cursorContains) {
            if (GameMain.Instance.MainCamera.GetControl()) {
                SetCameraControl(false);
            }
        } else {
            SetCameraControl(true);
        }
    }
    #endregion

    #region Private methods
    private void Dispose() 
    {
        ClearMaidData();
        SetCameraControl(true);
        mouseDowned    = false;
        cursorContains = false;

        // テクスチャキャッシュを開放する
        ACCTexturesView.Clear();

        initialized = false;
    }
    private bool Initialize() 
    {
        InitMaidInfo();

        uiParams.Update();
        ACCTexturesView.Init(uiParams);
        ACCMaterialsView.Init(uiParams);

        return true;
        //return holder.currentMaid != null;
    }
    private void InitMaidInfo() 
    {
        // ここでは、最初に選択可能なメイドを選択
        holder.UpdateMaid(ClearMaidData);
    }
    private void ClearMaidData() 
    {
        ACCMaterialsView.Clear();
        dDelNodes.Clear();
        dDelNodeDisps.Clear();
        dMaskSlots.Clear();
    }
    private void SetMenu(MenuType type) {
        if (menuType != type) {
            menuType = type;

            uiParams.Update();
        }
    }
    #endregion

    // 選択画面の一時選択状態のメイド情報
    private Maid selectedMaid;
    private string selectedName;

    // thumcache等
    private Dictionary<int, GUIContent> contentDic = new Dictionary<int, GUIContent>();

    private GUIContent GetOrAddMaidInfo(Maid m, int idx=-1)
    {
        GUIContent content;
        int id = m.gameObject.GetInstanceID();
        if (!contentDic.TryGetValue(id, out content)) {
            var status = m.Param.status;
            LogUtil.DebugLog("maid:", m.name);
            
            var maidName = (!m.boMAN)? status.last_name + " " + status.first_name : "男"+ (idx+1);
            Texture2D icon = m.GetThumIcon();
            content = new GUIContent(maidName, icon);
            contentDic[id] = content;
        }
        return content;
    }

    private void DoSelectMaid(int winID)
    {
        if (selectedMaid == null) selectedMaid = holder.currentMaid;
        GUILayout.BeginVertical();
        GUILayout.Label("メイド選択", uiParams.lStyleB);
　　    scrollViewPosition = GUILayout.BeginScrollView(scrollViewPosition, uiParams.optSubConWidth, uiParams.optSubConHeight);
        bool hasSelected = false;
        try {
            bool hasMaid = false;
            int count = GameMain.Instance.CharacterMgr.GetMaidCount();
            for (int i=0; i< count; i++) {
                Maid m = GameMain.Instance.CharacterMgr.GetMaid(i);
                if (m != null) {
                    hasMaid = true;
                    GUI.enabled = m.isActiveAndEnabled;
                    bool selected = (selectedMaid == m);
                    if (GUI.enabled && selected) hasSelected = true;

                    var content = GetOrAddMaidInfo(m);
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(uiParams.marginL);
                    bool changed = GUILayout.Toggle(selected, content, uiParams.tStyleL);
                    GUILayout.Space(uiParams.marginL);
                    GUILayout.EndHorizontal();
                    if (changed != selected) {
                        selectedMaid = m;
                        selectedName = content.text;
                    }
                }
            }
            GUI.enabled = true;
            if (!hasMaid) GUILayout.Label("　なし", uiParams.lStyleB);

            GUILayout.Space(uiParams.marginL);
            bool hasMan = false;

            count = GameMain.Instance.CharacterMgr.GetManCount();
            for (int i=0; i< count; i++) {
                Maid m = GameMain.Instance.CharacterMgr.GetMan(i);
                if (m != null && m.isActiveAndEnabled) {
                    if (!hasMan) GUILayout.Label("男選択", uiParams.lStyleB);
                    hasMan = true;
                    bool selected = (selectedMaid == m);
                    if (GUI.enabled && selected) hasSelected = true;

                    var content = GetOrAddMaidInfo(m, i);
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(uiParams.marginL);
                    bool changed = GUILayout.Toggle(selected, content, uiParams.tStyleL);
                    GUILayout.Space(uiParams.marginL);
                    GUILayout.EndHorizontal();
                    if (changed != selected) {
                        selectedMaid = m;
                        selectedName = content.text;
                    }
                }
            }

        } finally {
            GUILayout.EndScrollView();

            GUI.enabled = hasSelected;
            if (GUILayout.Button( "選択", uiParams.bStyle)) {
                SetMenu(MenuType.Main);
                holder.UpdateMaid(selectedMaid, selectedName, ClearMaidData);
                selectedMaid = null;
                selectedName = null;
                contentDic.Clear();
            }
            GUI.enabled = true;
            GUILayout.EndVertical();
        }
        GUI.DragWindow();
    }
    private void DoMainMenu(int winID) 
    {
        GUILayout.BeginVertical();
        try {
            Maid maid = holder.currentMaid;
            GUILayout.Label(TITLE_LABEL + holder.MaidName, uiParams.lStyle);
    
            if (GUILayout.Button("メイド/男 選択", uiParams.bStyle)) {
                SetMenu(MenuType.MaidSelect);
            }
            GUI.enabled = !maid.boMAN;
            GUILayout.BeginHorizontal();
            try {
                if (GUILayout.Button("マスク選択", uiParams.bStyle, uiParams.optSubConHalfWidth)) {
                    SetMenu(MenuType.MaskSelect);
                    InitMaskSlots();
                }
                if (GUILayout.Button("表示ノード選択", uiParams.bStyle, uiParams.optSubConHalfWidth)) {
                    // 初期化済の場合のみ
                    if (dDelNodes.Any()) {
                        dDelNodeDisps = GetDelNodes();
                    }
                    SetMenu(MenuType.NodeSelect);
                }
                
            } finally {
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
            try {
                GUI.enabled &= (toApplyPresetMaid == null);
                if (GUILayout.Button("プリセット保存", uiParams.bStyle, uiParams.optSubConHalfWidth)) {
                    SetMenu(MenuType.Save);
                }
                if (presetNames.Any()) {
                    if (GUILayout.Button("プリセット適用", uiParams.bStyle, uiParams.optSubConHalfWidth)) {
                        SetMenu(MenuType.PresetSelect);
                    }
                }
            } finally {
                GUILayout.EndHorizontal();
            }
            GUI.enabled = true;

            GUILayout.Space(uiParams.margin);
            GUILayout.BeginHorizontal();
            GUILayout.Label("カラーチェンジ スロット選択", uiParams.lStyleC);
            switchedName = GUILayout.Toggle(switchedName, "表示切替", uiParams.tStyleS, uiParams.optSLabelWidth);
            GUILayout.EndHorizontal();
            scrollViewPosition = GUILayout.BeginScrollView(scrollViewPosition, 
                                                           GUILayout.Width(uiParams.mainRect.width),
                                                           GUILayout.Height(uiParams.mainRect.height));
            try {
                foreach (SlotInfo slot in ACConstants.SlotNames.Values) {
                    if (!slot.enable) continue;
                    //if (slot.Id == TBody.SlotID.end) continue;
        
                    // TODO メイド以外の項目については別の方法で可視性を取得する必要あり
                    if (!holder.currentMaid.body0.GetSlotVisible(slot.Id)) {
                        continue;
                    }

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(uiParams.marginL);
                    
                    if (GUILayout.Button(!switchedName?slot.DisplayName: slot.Name, uiParams.bStyleL, GUILayout.ExpandWidth(true))) {
                        holder.currentSlot = slot;
                        SetMenu(MenuType.Color);
                    }
                    if (settings.displaySlotName) {
                        GUILayout.Label(slot.Name, uiParams.lStyleS, uiParams.optCategoryWidth);
                    }
                    GUILayout.Space(uiParams.marginL);
                    GUILayout.EndHorizontal();

                }
            } finally {
                GUI.enabled = true;
                GUILayout.EndScrollView();
            }
            
        } finally {
            GUILayout.EndVertical();
        }
        GUI.DragWindow();
    }

    private List<ACCMaterialsView> initMaterialView(Material[] materials) 
    {
        var ret = new List<ACCMaterialsView>(materials.Length);
        foreach (Material material in materials) { 
            ret.Add(new ACCMaterialsView(material));
        }
        return ret;
    }

    private void DoColorMenu(int winID) 
    {
        GUILayout.Label("強制カラーチェンジ: " + holder.currentSlot.DisplayName, uiParams.lStyleB);
        // TODO 選択アイテム名、説明等を表示 可能であればアイコンも 
        
        scrollViewPosition = GUILayout.BeginScrollView(scrollViewPosition, 
                                                       GUILayout.Width(uiParams.colorRect.width),
                                                       GUILayout.Height(uiParams.colorRect.height));
        try {
            if (holder.currentMaid.IsBusy) {
                GUILayout.Space(100);
                GUILayout.Label("変更中...", uiParams.lStyleB);
                return;
            }

            // **_del_.menuを選択の状態が続いたらメインメニューへ
            // 衣装セットなどは内部的に一旦_del_.menuが選択されるため、一時的に選択された状態をスルー
            string menu = holder.GetCurrentMenuFile();
            if (menu != null) {
                if (menu.EndsWith("_del.menu", StringComparison.OrdinalIgnoreCase)) {
                    if (changeCount++ > CHANGE_THRESHOLD) {
                        SetMenu(MenuType.Main);
                        LogUtil.DebugLog("選択スロットのアイテムが外れたため、メインメニューに戻ります", menu);
                        changeCount = 0;
                    }
                    return;
                }
            }

            // ターゲットのmenuファイルが変更された場合にビューを更新
            if (targetMenu != menu) {
                LogUtil.DebugLog("menufile changed.", targetMenu, "=>", menu);

                // .modファイルは未対応
                isSavable = (menu != null)
                    && !(menu.ToLower().EndsWith(FileConst.EXT_MOD, StringComparison.Ordinal));
                
                targetMenu = menu;
                TBodySkin slot = holder.GetCurrentSlot();
                targetMaterials = holder.GetMaterials(slot);
                materialViews = initMaterialView(targetMaterials);

                changeCount = 0;

                if (isSavable) {
                    // 保存未対応スロットを判定(身体は不可)
                    isSavable &= (holder.currentSlot.Id != TBody.SlotID.body);
                }
            }
    
            if ( GUILayout.Button("テクスチャ変更", uiParams.bStyle) ) {
                texViews = InitTexView(targetMaterials);

                SetMenu(MenuType.Texture);
                return;
            }
    
            if (targetMaterials.Length > 0) {
                foreach (ACCMaterialsView view in materialViews) {
                    view.Show();
                }
            }

        } catch (Exception e) {
            LogUtil.ErrorLog("強制カラーチェンジ画面でエラーが発生しました。メイン画面へ移動します", e);
            SetMenu(MenuType.Main);
        } finally {
            GUILayout.EndScrollView();

            GUI.enabled = isSavable;
            if (GUILayout.Button("menuエクスポート", uiParams.bStyle)) ExportMenu();
            GUI.enabled = true;

            if (GUILayout.Button("閉じる", uiParams.bStyle)) {
                SetMenu(MenuType.Main);
            }
            GUI.DragWindow();
        }
    }
    private void ExportMenu() 
    {
        global::TBodySkin slot = holder.GetCurrentSlot();
        if (slot.obj == null) {
            var msg = "指定スロットが見つかりません。slot=" + holder.currentSlot.Name;
            NUty.WinMessageBox(NUty.GetWindowHandle(), msg, "エラー", NUty.MSGBOX.MB_OK);
            return;
        }

        // propは対応するMPNを指定
        MaidProp prop = holder.currentMaid.GetProp(holder.currentSlot.mpn);
        if (prop != null) {
            if (saveView == null) saveView = new ACCSaveMenuView(uiParams);

            // 変更可能なmenuファイルがない場合は保存画面へ遷移しない
            var targetSlots = saveView.Load(prop.strFileName);
            if (targetSlots == null) {
                var msg = "変更可能なmenuファイルがありません " + prop.strFileName;
                NUty.WinMessageBox(NUty.GetWindowHandle(), msg, "エラー", NUty.MSGBOX.MB_OK);
            } else {
                // menuファイルで指定されているitemのスロットに関連するマテリアル情報を抽出
                foreach (var targetSlot in targetSlots.Keys) {

                    List<ACCMaterial> edited;
                    if (targetSlot == holder.currentSlot.Id) {
                        // カレントスロットの場合は、作成済のマテリアル情報を渡す
                        edited   = new List<ACCMaterial>(materialViews.Count);
                        foreach ( var matView in materialViews ) {
                            edited.Add(matView.edited);
                        }
                    } else {
                        Material[] materials = holder.GetMaterials(targetSlot);
                        edited = new List<ACCMaterial>(materials.Length);
                        foreach (Material mat in materials) {
                            edited.Add(new ACCMaterial(mat));
                        }
                    }
                    saveView.SetEditedMaterials(targetSlot, edited);
                    
                }
                //if (!saveView.CheckData()) {
                //    var msg = "保存可能なmenuファイルではありません " + prop.strFileName;
                //    NUty.WinMessageBox(NUty.GetWindowHandle(), msg, "エラー", NUty.MSGBOX.MB_OK);
                //}
            }
        }        
    }

    private List<ACCTexturesView> InitTexView(Material[] materials) 
    {
        var ret = new List<ACCTexturesView>(materials.Length);
        int matNo = 0;
        foreach (Material material in materials) {
            try {               
                ret.Add(new ACCTexturesView(material, matNo++));
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
            LogUtil.DebugLog("failed to create texture change view. ", e);
        } finally {
            GUILayout.EndScrollView();

            if (GUILayout.Button( "閉じる", uiParams.bStyle, 
                                 uiParams.optSubConWidth, uiParams.optBtnHeight)) {
                SetMenu(MenuType.Color);
            }
            GUI.DragWindow();
        }
    }
    private void DoFileBrowser(int winId)
    {
        ACCTexturesView.fileBrowser.OnGUI();
        GUI.DragWindow();
    }
    private bool InitMaskSlots() 
    {
        if (holder.currentMaid == null) return false;

//        List<int> maskSlots = holder.maid.listMaskSlot;
        foreach (SlotInfo si  in ACConstants.SlotNames.Values) {
            if (!si.enable || !si.maskable) continue;

            TBodySkin slot = holder.currentMaid.body0.GetSlot((int)si.Id);
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
        GUILayoutOption bWidth  = GUILayout.Width(uiParams.subConWidth*0.32f);
        GUILayoutOption bWidthS = GUILayout.Width(uiParams.subConWidth*0.24f);
        GUILayoutOption lStateWidth = GUILayout.Width(uiParams.fontSize*4f);
        GUILayoutOption titleWidth = GUILayout.Width(uiParams.fontSize*10f);
        GUILayout.BeginVertical();
        try {
            // falseがマスクの模様
            GUILayout.BeginHorizontal();
            GUILayout.Label("マスクアイテム選択", uiParams.lStyleB, titleWidth);
            switchedName = GUILayout.Toggle(switchedName, "表示切替", uiParams.tStyleS, uiParams.optSLabelWidth);
            GUILayout.EndHorizontal();

            if (holder.currentMaid == null) return ;

            // 身体からノード一覧と表示状態を取得
            var outRect = uiParams.subRect;
            if (!dMaskSlots.Any()) {
                InitMaskSlots();
            }

            GUILayout.BeginHorizontal();
            try {
                if (GUILayout.Button("同期", uiParams.bStyle, uiParams.optBtnHeight, bWidth)) { 
                    InitMaskSlots();
                }
                if (GUILayout.Button("すべてON", uiParams.bStyle, uiParams.optBtnHeight, bWidth)) {
                    var keys = new List<TBody.SlotID>(dMaskSlots.Keys);
                    foreach (TBody.SlotID key in keys) {
                        dMaskSlots[key].value = false;
                    }
                }
                if (GUILayout.Button("すべてOFF", uiParams.bStyle, uiParams.optBtnHeight, bWidth)) { 
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
                    // if (pair.Key <= TBody.SlotID.eye) continue;

                    MaskInfo maskInfo = pair.Value;
                    string state;
                    // 下着、ヌードモードなどによる非表示
                    if (!holder.currentMaid.body0.GetMask(maskInfo.slotInfo.Id)) {
                        state = "[非表示]";
                        labelStyle.normal.textColor = Color.magenta;
                    } else {
                        maskInfo.UpdateState();
                        switch(maskInfo.state) {
                            case SlotState.NotLoaded:
                                //continue;
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
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(state, labelStyle, lStateWidth);
                    // dMaskSlotsはCM3D2のデータと合わせてマスクオン=falseとし、画面上はマスクオン=選択(true)とする
                    maskInfo.value = !GUILayout.Toggle( !maskInfo.value, !switchedName?maskInfo.slotInfo.DisplayName:maskInfo.slotInfo.Name,
                                                       uiParams.tStyle, uiParams.optContentWidth
                                                       ,GUILayout.ExpandWidth(true)
                                                      );
                    //GUILayout.Label( maskInfo.slotInfo.Name, uiParams.lStyleRS, uiParams.optSlotWidth);
                    GUI.enabled = true;
                    GUILayout.EndHorizontal();
                }
            } finally {
                labelStyle.normal.textColor = bkColor;
                GUI.EndScrollView();
            }
        } finally {
            GUILayout.EndVertical();
        }

        GUILayout.BeginHorizontal();
        try {
            if (GUILayout.Button("一時適用", uiParams.bStyle, bWidthS, uiParams.optBtnHeight)) {
                holder.SetSlotVisibles(dMaskSlots, true);
            }
            if (GUILayout.Button("適用", uiParams.bStyle, bWidthS, uiParams.optBtnHeight)) {
                holder.SetSlotVisibles(dMaskSlots, false);
                holder.FixFlag();
            }
            if (GUILayout.Button("全クリア", uiParams.bStyle, bWidthS, uiParams.optBtnHeight)) {
                holder.SetAllVisible();
            }
            if (GUILayout.Button("戻す", uiParams.bStyle, bWidthS, uiParams.optBtnHeight)) {
                holder.FixFlag();
            }
        } finally {
            GUILayout.EndHorizontal();
        }

        if (GUILayout.Button( "閉じる", uiParams.bStyle, uiParams.optSubConWidth, uiParams.optBtnHeight)) {
            SetMenu(MenuType.Main);
        }

        GUI.DragWindow();
    }
    private bool InitDelNodes(TBodySkin body) {
        if (body == null) {
            if (holder.currentMaid == null) return false;

            // 身体からノード一覧と表示状態を取得
            body = holder.currentMaid.body0.GetSlot(TBody.SlotID.body.ToString());
        }
        Dictionary<string, bool> dic = body.m_dicDelNodeBody;
        foreach (string key in ACConstants.NodeNames.Keys) {
            bool val;
            if (dic.TryGetValue(key, out val)){
                dDelNodes[key] = val;
            }
        }

        return true;
    }
    /// <summary>
    /// 現在のノードの表示状態を表すDictionaryを取得する
    /// </summary>
    /// <returns>現在のノードの表示状態Dic</returns>
    private Dictionary<string, bool> GetDelNodes() {
        if (!dDelNodes.Any()) InitDelNodes(null);

        var keys = new List<string>(dDelNodes.Keys);
        var delNodeDic = new Dictionary<string, bool>(dDelNodes);
        foreach (string key in keys) {
            delNodeDic[key] = true;
        }
        foreach(TBodySkin slot in holder.currentMaid.body0.goSlot) {
            if (slot.obj != null && slot.boVisible) {
                Dictionary<string, bool> slotNodes = slot.m_dicDelNodeBody;
                // 1つでもFalseがあったら非表示とみなす
                foreach (string key in keys) {
                    bool v;
                    if (slotNodes.TryGetValue(key, out v)) {
                        delNodeDic[key] &= v;
                    }
                }
                if (!slot.m_dicDelNodeParts.Any()) continue;

                foreach(Dictionary<string, bool> sub in slot.m_dicDelNodeParts.Values) {
                    foreach(KeyValuePair<string, bool> pair in sub) {
                        if (delNodeDic.ContainsKey(pair.Key)) {
                            delNodeDic[pair.Key] &= pair.Value;
                        }
                    }
                }
            }
        }
        return delNodeDic;
    }
    private void DoNodeSelectMenu(int winID)
    {
        GUILayout.BeginVertical();
        TBodySkin body;
        GUILayoutOption titleWidth = GUILayout.Width(uiParams.fontSize*10f);
        GUILayoutOption lStateWidth = GUILayout.Width(uiParams.fontSize*4f);
        try {
            GUILayout.BeginHorizontal();
            GUILayout.Label("表示ノード選択", uiParams.lStyleB, titleWidth);
            GUILayout.Space(uiParams.margin);
            switchedName = GUILayout.Toggle(switchedName, "表示切替", uiParams.tStyleS, uiParams.optSLabelWidth);
            GUILayout.EndHorizontal();

            if (holder.currentMaid == null) return ;

            // 身体からノード一覧と表示状態を取得
            body = holder.currentMaid.body0.GetSlot(TBody.SlotID.body.ToString());
            var outRect = uiParams.subRect;
            if (!dDelNodes.Any()) {
                InitDelNodes(body);
                dDelNodeDisps = GetDelNodes();
                // 表示ノード状態をUI用データに反映
                foreach (var nodes in dDelNodeDisps) {
                    dDelNodes[nodes.Key] = nodes.Value;
                }
            }

            GUILayout.BeginHorizontal();
            try {
                GUILayoutOption bWidth = GUILayout.Width(uiParams.subConWidth*0.33f);
                if (GUILayout.Button("同期", uiParams.bStyle, uiParams.optBtnHeight, bWidth)) {
                    dDelNodeDisps = GetDelNodes();
                    foreach (var nodes in dDelNodeDisps) {
                        dDelNodes[nodes.Key] = nodes.Value;
                    }
                }
                if (GUILayout.Button("すべてON", uiParams.bStyle, uiParams.optBtnHeight, bWidth)) {
                    var keys = new List<string>(dDelNodes.Keys);
                    foreach (string key in keys) {
                        dDelNodes[key] = true;
                    }
                }
                if (GUILayout.Button("すべてOFF", uiParams.bStyle, uiParams.optBtnHeight, bWidth)) { 
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
                foreach (KeyValuePair<string, NodeItem> pair in ACConstants.NodeNames) {
                    var nodeItem = pair.Value;
                    bool delNode;
                    if (!dDelNodes.TryGetValue(pair.Key, out delNode)) {
                        LogUtil.DebugLog("node name not found.", pair.Key);
                        continue;
                    }
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
                        GUILayout.Label(state, labelStyle, lStateWidth);
                        
                        if (nodeItem.depth != 0) {
                            GUILayout.Space(uiParams.margin * nodeItem.depth*3);
                        }
                        GUI.enabled = isValid;
                        dDelNodes[pair.Key] = GUILayout.Toggle( delNode, !switchedName?nodeItem.DisplayName: pair.Key,
                                                               uiParams.tStyle, uiParams.optContentWidth);
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
        if (GUILayout.Button("適用", uiParams.bStyle, uiParams.optSubConWidth, uiParams.optBtnHeight)) {
            holder.SetDelNodes(dDelNodes, true);
            dDelNodeDisps = new Dictionary<string, bool>(body.m_dicDelNodeBody);
        }
        if (GUILayout.Button( "閉じる", uiParams.bStyle, uiParams.optSubConWidth, uiParams.optBtnHeight)) {
            SetMenu(MenuType.Main);
        }
        GUI.DragWindow();
    }

    bool bPresetSavable;
    private void DoSaveMenu(int winID)
    {
        GUILayout.BeginVertical();
        try {
            GUILayout.Label("プリセット保存", uiParams.lStyleB);
    
            GUILayout.Label("プリセット名", uiParams.lStyle);
            var pname = GUILayout.TextField(presetName, uiParams.textStyle, GUILayout.ExpandWidth(true));
            if (pname != presetName) {
                bPresetSavable = !FileConst.HasInvalidChars(pname);
                presetName = pname;
            }
            if (bPresetSavable) {
                bPresetSavable &= pname.Trim().Length != 0;
            }

            scrollViewPosition = GUILayout.BeginScrollView(scrollViewPosition, uiParams.optSubConWidth, uiParams.optSubCon6Height);
            GUILayout.BeginHorizontal();
            GUILayout.Space(uiParams.marginL);
            GUILayout.Label("《保存済みプリセット一覧》", uiParams.lStyle);
            GUILayout.EndHorizontal();
            foreach (var presetName1 in presetNames) {
                GUILayout.BeginHorizontal();
                GUILayout.Space(uiParams.marginL);
                GUILayout.Label(presetName1, uiParams.lStyleS);
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();

            GUI.enabled = bPresetSavable;
            if (GUILayout.Button("保存", uiParams.bStyle, GUILayout.ExpandWidth(true))) {
                SavePreset();
            }
            GUI.enabled = true;
            if (GUILayout.Button("閉じる", uiParams.bStyle, GUILayout.ExpandWidth(true))) {
                SetMenu(MenuType.Main);
            }
        } finally {
            GUILayout.EndHorizontal();
            GUI.DragWindow();
        }
    }

    private void DoSelectPreset(int winId)
    {
        GUILayout.BeginVertical();
        try {
            GUILayout.Label("プリセット適用", uiParams.lStyleB);
    
            GUILayout.BeginHorizontal();
            GUILayout.Space(uiParams.marginL);
            GUILayout.Label("《適用項目》", uiParams.lStyle);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Space(uiParams.marginL*2);
            bPresetApplyMask = GUILayout.Toggle(bPresetApplyMask, "マスク", uiParams.tStyle);
            bPresetApplyNode = GUILayout.Toggle(bPresetApplyNode, "ノード表示", uiParams.tStyle);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Space(uiParams.marginL*2);
            bPresetApplyBody = GUILayout.Toggle(bPresetApplyBody, "身体", uiParams.tStyle);
            bPresetApplyWear = GUILayout.Toggle(bPresetApplyWear, "衣装", uiParams.tStyle);
            bPresetCastoff   = GUILayout.Toggle(bPresetCastoff,   "衣装外し", uiParams.tStyle);
            GUILayout.EndHorizontal();

            scrollViewPosition = GUILayout.BeginScrollView(scrollViewPosition, uiParams.optSubConWidth, uiParams.optSubCon6Height);
            try {
                foreach (var presetName1 in presetNames) {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button(presetName1, uiParams.bStyleL)) {
                        if (ApplyPreset(presetName1)) {
                            SetMenu(MenuType.Main);
                        }
                    }
                    if (GUILayout.Button("削除", uiParams.bStyle, uiParams.optDBtnWidth)) {
                        DeletePreset(presetName1);
                    }
                    GUILayout.EndHorizontal();
                }
            } finally {
                GUILayout.EndScrollView();
            }
            if (GUILayout.Button("閉じる", uiParams.bStyle)) {
                SetMenu(MenuType.Main);
            }
        } finally {
            GUILayout.EndHorizontal();
            GUI.DragWindow();
        }
    }
    private void DeletePreset(string presetName1) {
        if (!Directory.Exists(settings.presetDirPath)) return;

        var filepath = presetMgr.GetPresetFilepath(presetName1);
        if (File.Exists(filepath)) {
            File.Delete(filepath);
            LoadPresetList();
        }
    }
    private void SavePreset() {
        if (!Directory.Exists(settings.presetDirPath)) Directory.CreateDirectory(settings.presetDirPath);

        try {
            var filepath = presetMgr.GetPresetFilepath(presetName);
            dDelNodeDisps = GetDelNodes();
            presetMgr.Save(filepath, presetName, dDelNodeDisps);
            SetMenu(MenuType.Main);

            // 一覧を更新
            LoadPresetList();
        } catch(NullReferenceException e) {
            LogUtil.ErrorLog(e);
        }        
    }
    private void ApplyPreset() 
    {
        ApplyPreset(currentPreset);
    }
    private bool ApplyPreset(string presetName1) 
    {
        LogUtil.DebugLog("Applying Preset. ", presetName1);
        var filename = presetMgr.GetPresetFilepath(presetName1);
        if (!File.Exists(filename)) return false;

        currentPreset = presetMgr.Load(filename);
        if (currentPreset == null) return false;

        ApplyPreset(currentPreset);
        return true;
    }
    private void ApplyPreset(PresetData preset) {
        if (preset == null) return;

        toApplyPresetMaid = holder.currentMaid;
        if (preset.mpns.Any()) {
            // 衣装チェンジ
            presetMgr.ApplyPresetMPN(toApplyPresetMaid, preset, bPresetApplyBody, bPresetApplyWear, bPresetCastoff);
        } else {
            // 衣装チェンジがない場合は即座に適用
            ApplyPresetProp(preset);
        }
    }
    private void ApplyPresetProp(PresetData preset) {
        try {
            if (bPresetApplyWear) {
                presetMgr.ApplyPresetMaterial(toApplyPresetMaid, preset);
            }
            if (bPresetApplyNode && preset.delNodes != null) {
                // 表示ノードを反映 (プリセットで未定義のノードは変更されない）
                foreach (var node in preset.delNodes) {
                    dDelNodes[node.Key] = node.Value;
                }
                holder.SetDelNodes(toApplyPresetMaid, preset, false);
            }
            if (bPresetApplyMask) {
                holder.SetMaskSlots(toApplyPresetMaid, preset);
            }
            holder.FixFlag(toApplyPresetMaid);
        } finally {
            toApplyPresetMaid = null;
        }
    }
    private void LoadPresetList() {
        try {
            if (!Directory.Exists(settings.presetDirPath)) {
                presetNames.Clear();
                return;
            }

            var files = Directory.GetFiles(settings.presetDirPath, "*.json", SearchOption.AllDirectories);
            int fileNum = files.Count();
            if (fileNum == 0) {
                presetNames.Clear();
            } else {
                Array.Sort(files);
                presetNames = new List<string>(fileNum);
                foreach (var file in files) {
                    presetNames.Add(Path.GetFileNameWithoutExtension(file));
                }
            }
        } catch(Exception e) {
            LogUtil.DebugLog(e);
        }
    }
    /// <summary>
    /// 旧版のプリセットXMLを読み込み、JSONファイル形式で出力する
    /// あくまで互換性のためであり、通常は不要機能
    /// </summary>
    private void MigratePresets() {
        // 新版のディレクトリがある場合はスキップ 
        if (Directory.Exists(settings.presetDirPath)) return;

        // 旧設定ファイルパスからXMLファイルの存在確認
        var oldXml = settings.presetPath ?? Path.Combine(DataPath, "AlwaysColorChangeEx.xml");
        if (File.Exists(oldXml)) {
            var presets = presetMgr.LoadXML(oldXml);
            if (presets == null) return;

            Directory.CreateDirectory(settings.presetDirPath);

            try {
                foreach(var preset in presets) {
                    var filepath = presetMgr.GetPresetFilepath(preset.Key);
                    presetMgr.SavePreset(filepath, preset.Value);
                }
            } catch(Exception e) {
                LogUtil.Log("旧版のプリセットファイルの移行に失敗しました", e);
            }
        }
    }

    private void DoSaveModDialog(int winId) {
        try {
            saveView.Show();
        } catch(Exception e) {
            LogUtil.DebugLog("failed to display save dialog.", e);
        }

        GUI.DragWindow();
    }
}
public class UIParams {
    private static UIParams instance = new UIParams();        
    public static UIParams Instance {
        get { return instance;  }
    }
    #region Constants
    private const int marginPx     = 2;
    private const int marginLPx    = 10;
    private const int itemHeightPx = 18;
    private const int fontPx       = 14;
    private const int fontPxS = (int)(fontPx*0.9f);
    private const int fontPxL = 20;
    #endregion

    private int width;
    private int height;
    private float ratio;
    
    public int margin;
    public int marginL; //
    public int fontSize;
    public int fontSizeS;
    public int fontSizeL;
    public int itemHeight;
    public int unitHeight;
    public readonly GUIStyle lStyle    = new GUIStyle("label");
    // bold
    public readonly GUIStyle lStyleB   = new GUIStyle("label");
    // colored
    public readonly GUIStyle lStyleC   = new GUIStyle("label");
    // small
    public readonly GUIStyle lStyleS   = new GUIStyle("label");
    public readonly GUIStyle lStyleRS  = new GUIStyle("label");

    public readonly GUIStyle bStyle    = new GUIStyle("button");
    public readonly GUIStyle bStyleSC  = new GUIStyle("button");
    public readonly GUIStyle bStyleL   = new GUIStyle("button");

    public readonly GUIStyle tStyle    = new GUIStyle("toggle");
    public readonly GUIStyle tStyleS   = new GUIStyle("toggle");
    public readonly GUIStyle tStyleL   = new GUIStyle("toggle");
    public readonly GUIStyle listStyle = new GUIStyle();
    public readonly GUIStyle textStyle = new GUIStyle("textField");
    public readonly GUIStyle textStyleSC = new GUIStyle("textField");
    public readonly GUIStyle textAreaStyleS = new GUIStyle("textArea");

    public readonly GUIStyle boxStyle      = new GUIStyle("box");
    public readonly GUIStyle winStyle      = new GUIStyle("box");
    public readonly GUIStyle dialogStyle   = new GUIStyle("box");

    public readonly Color textColor = new Color(1f, 1f, 1f, 0.98f);

    public Rect winRect         = new Rect();
    public Rect fileBrowserRect = new Rect();
    public Rect modalRect       = new Rect();

    public Rect mainRect         = new Rect();
    public Rect colorRect        = new Rect();
    public Rect nodeSelectRect   = new Rect();
    public Rect presetSelectRect = new Rect();
    public Rect textureRect      = new Rect();
    public Rect labelRect        = new Rect();
    public Rect subRect          = new Rect();
    public GUILayoutOption optBtnHeight;
    public float subConWidth;
    public GUILayoutOption optSubConWidth;
    public GUILayoutOption optSubConHeight;
    public GUILayoutOption optSubCon6Height;
    public GUILayoutOption optSubConHalfWidth;
    public GUILayoutOption optBtnWidth;
    public GUILayoutOption optCategoryWidth;
    public GUILayoutOption optDBtnWidth;
    public GUILayoutOption optSLabelWidth;

    public GUILayoutOption optContentWidth;

    public UIParams() {
        listStyle.onHover.background = listStyle.hover.background = new Texture2D(2, 2);
        listStyle.padding.left = listStyle.padding.right = 4;
        listStyle.padding.top = listStyle.padding.bottom = 1;
        listStyle.normal.textColor = listStyle.onNormal.textColor =
            listStyle.hover.textColor = listStyle.onHover.textColor =
            listStyle.active.textColor = listStyle.onActive.textColor = Color.white;
        listStyle.focused.textColor = listStyle.onFocused.textColor = Color.blue;

        TextAnchor txtAlignment = TextAnchor.MiddleLeft;
        // Bold
        lStyleB.fontStyle        = FontStyle.Bold;
        lStyleB.alignment        = txtAlignment;

        lStyle.fontStyle         = FontStyle.Normal;
        lStyle.normal.textColor  = textColor;
        lStyle.alignment         = txtAlignment;

        lStyleS.fontStyle        = FontStyle.Normal;
        lStyleS.normal.textColor = textColor;
        lStyleS.alignment        = txtAlignment;

        lStyleRS.fontStyle        = FontStyle.Normal;
        lStyleRS.normal.textColor = textColor;
        lStyleRS.alignment        = TextAnchor.MiddleRight;

        lStyleC.fontStyle        = FontStyle.Normal;
        lStyleC.normal.textColor = new Color(0.82f, 0.88f, 1f, 0.98f);
        lStyleC.alignment        = txtAlignment;

        bStyle.normal.textColor  = textColor;
        bStyleSC.normal.textColor = textColor;
        bStyleSC.alignment = TextAnchor.MiddleCenter;
        bStyleL.normal.textColor  = textColor;
        bStyleL.alignment = TextAnchor.MiddleLeft; 
        
        tStyle.normal.textColor         = textColor;
        tStyleS.normal.textColor        = textColor;
        tStyleS.alignment               = TextAnchor.LowerLeft;
        tStyleL.normal.textColor        = textColor;
        tStyleL.alignment               = txtAlignment;
//        var tex2D = new Texture2D(2, 2);
//        tex2D.SetPixels(new Color[]{Color.white, Color.white});
//        tStyleL.onHover.background = tStyleL.hover.background = tex2D;
//        var texGray = new Texture2D(2, 2);
//        texGray.SetPixels(new Color[]{Color.gray, Color.white});
//        tStyleL.focused.background = tStyleL.onFocused.background = texGray;
//        var tex2 = new Texture2D(2, 2);
//        tex2.SetPixels(new Color[]{Color.gray, Color.grey});
//        tStyleL.active.background = tStyleL.onActive.background = tex2;
        //tStyle.stretchWidth = true;

        textStyle.normal.textColor      = textColor;
        textStyleSC.normal.textColor    = textColor;
        textStyleSC.alignment = TextAnchor.MiddleCenter; 
        textAreaStyleS.normal.textColor = textColor;

        winStyle.alignment    = TextAnchor.UpperRight;
        dialogStyle.alignment = TextAnchor.UpperCenter;
        dialogStyle.normal.textColor = textColor;
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
        fontSizeS  = FixPx(fontPxS);
        fontSizeL  = FixPx(fontPxL);
        margin     = FixPx(marginPx);
        marginL    = FixPx(marginLPx);
        itemHeight = FixPx(itemHeightPx);
        unitHeight = margin + itemHeight;

        lStyle.fontSize        = fontSize;
        lStyleC.fontSize       = fontSize;
        lStyleB.fontSize       = fontSize;

        lStyleS.fontSize       = fontSizeS;
        lStyleRS.fontSize      = fontSizeS;

        bStyle.fontSize        = fontSize;
        bStyleSC.fontSize      = fontSizeS;
        bStyleL.fontSize       = fontSize;
        tStyle.fontSize        = fontSize;
        tStyleS.fontSize       = fontSizeS;
        tStyleL.fontSize       = fontSizeL;
        listStyle.fontSize     = fontSizeS;
        textStyle.fontSize     = fontSize;
        textStyleSC.fontSize    = fontSizeS;
        textAreaStyleS.fontSize = fontSizeS;

        LogUtil.DebugLogF("screen=({0},{1}),margin={2},height={3},ratio={4})", width, height, margin, itemHeight, ratio);

        winStyle.fontSize  = fontSize;
        dialogStyle.fontSize  = fontSize;
        InitWinRect();
        InitFBRect();
        InitModalRect();

        subConWidth = winRect.width - margin * 2;
        optBtnHeight = GUILayout.Height(itemHeight);
        // sub
        optSubConWidth  = GUILayout.Width(subConWidth);
        optSubConHeight = GUILayout.Height(winRect.height - unitHeight *3f);
        optSubCon6Height = GUILayout.Height(winRect.height - unitHeight *6.6f);
        optSubConHalfWidth = GUILayout.Width((winRect.width - marginL*2)*0.5f); // margin値が小さい前提になってしまっている
        optSLabelWidth   = GUILayout.Width(fontSizeS * 6f);

        mainRect.Set(      margin, unitHeight * 5 + margin, winRect.width - margin * 2, winRect.height - unitHeight *6.5f);
        textureRect.Set(   margin, unitHeight,     winRect.width - margin * 2, winRect.height - unitHeight * 2.5f);
        float baseWidth = textureRect.width - 20;
        optBtnWidth   = GUILayout.Width(baseWidth * 0.09f);
        optDBtnWidth     = GUILayout.Width(fontSizeS * 5f * 0.6f);
        optContentWidth  = GUILayout.MaxWidth(baseWidth * 0.69f);
        optCategoryWidth = GUILayout.MaxWidth(fontSize * 12f * 0.47f);

        nodeSelectRect.Set(margin, unitHeight * 2, winRect.width - margin * 2, winRect.height - unitHeight * 4.5f);
        colorRect.Set(     margin, unitHeight * 2, winRect.width - margin * 3, winRect.height - unitHeight * 4);
        labelRect.Set(     0,      0,              winRect.width - margin * 2, itemHeight*1.2f);
        subRect.Set(       0,      itemHeight,     winRect.width - margin * 2, itemHeight);

        foreach (var func in updaters) {
            func(this);
        }
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
    readonly List<Action<UIParams>> updaters = new List<Action<UIParams>>();
    public void Add(Action<UIParams> action) {
        action(this);
        updaters.Add(action);
    }
    public bool Remove(Action<UIParams> action) {
        return updaters.Remove(action);
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