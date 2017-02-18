using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Reflection;
using Schedule;
using UnityEngine;
using UnityEngine.UI;
using UnityInjector;
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
 PluginName("CM3D2_ACCex"),
 PluginVersion("0.2.9.5")]
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
            LogUtil.Error( e );
        }        
        // プラグインバージョン取得
        try {
            // 属性クラスからバージョン番号取得
            var att = Attribute.GetCustomAttribute( typeof(AlwaysColorChangeEx), typeof( PluginVersionAttribute ) ) as PluginVersionAttribute;
            if( att != null ) Version = att.Version;
        } catch( Exception e ) {
            LogUtil.Error( e );
        }
    }
    internal MonoBehaviour plugin;

    private CM3D2SceneChecker checker = new CM3D2SceneChecker();

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
    private const string TITLE_LABEL = "ACCex : ";
    private const int WINID_MAIN   = 20201;
    private const int WINID_DIALOG = WINID_MAIN+1;
    private const int WINID_TIPS   = WINID_MAIN+2;

    private const EventModifiers modifierKey = EventModifiers.Shift | EventModifiers.Control | EventModifiers.Alt;

    #region Variables
    private float fPassedTime     = 0f;
    private float fLastInitTime   = 0f;
    private bool initialized      = false;
//    private int sceneLevel;
//    private bool visible;

    private bool bUseStockMaid  = false;
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
    private bool bPresetApplyNode = false;
    private bool bPresetApplyMask = false;
    private bool bPresetApplyBody = true;
    private bool bPresetApplyWear = true;
    private bool bPresetApplyBodyProp   = true;
    private bool bPresetApplyPartsColor = true;
    private Maid toApplyPresetMaid = null;

    private bool isSavable;
    private bool isActive;
    private bool texSliderUpped;
    
    private const int applyDeleFrame = 10;
    private const int tipsSecond = 2;
    private IntervalCounter changeCounter = new IntervalCounter(15);
    // ゲーム上の表示データの再ロード間隔
    private IntervalCounter refreshCounter = new IntervalCounter(60);
    private Vector2 scrollViewPosition = Vector2.zero;
    // 表示名切り替え
    private bool switchedName;

    // テクスチャ変更用
    //  現在のターゲットのslotに関するメニューが変更されたらGUIを更新。それ以外は更新しない
    private int targetMenuId;
    private bool slotDropped;
    private Material[] targetMaterials;
    private readonly Material[] EMPTY_ARRAY = new Material[0];
    private List<ACCMaterialsView> materialViews;
    private List<ACCTexturesView> texViews;
    private ACCSaveMenuView saveView;
    #endregion

    public AlwaysColorChangeEx() {
        plugin = this;
    }

    #region MonoBehaviour methods
    public void Awake() 
    {
        UnityEngine.Object.DontDestroyOnLoad(this);
        
        // リダイレクトで存在しないパスが渡されてしまうケースがあるため、
        // Sybarisチェックを先に行う (リダイレクトによるパスではディレクトリ作成・削除が動作しない）
        var dllpath = Path.Combine(DataPath, @"..\..\opengl32.dll");
        var dirPath = Path.Combine(DataPath, @"..\..\Sybaris");
        if (File.Exists(dllpath) && Directory.Exists(dirPath)) {
            dirPath = Path.GetFullPath(dirPath);
            settings.presetDirPath = Path.Combine(dirPath, @"Plugins\UnityInjector\Config\ACCPresets");
        } else {
            settings.presetDirPath = Path.Combine(DataPath, "ACCPresets");
            // string dllPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            // string dllDir = Path.GetDirectoryName(dllPath);
            // settings.presetDirPath = Path.Combine(dllDir, @"Config\ACCPresets");
        }

        base.ReloadConfig();
        settings.Load((key) => base.Preferences["Config"][key].Value);
        LogUtil.Log("PresetDir:", settings.presetDirPath);

        checker.Init();
        
        MigratePresets();
        LoadPresetList();
        uiParams.Update();

        // Initialize
        ShaderPropType.Initialize();
    }

//    Action<Maid, MaidProp> Notifier = (Maid m, MaidProp prop) => {
//    };

    public void OnDestroy() 
    {
        SetCameraControl(true);
        Dispose();
        presetNames.Clear();
        //detector.Clear();
        LogUtil.Debug("Destroyed");
        
    }
    public void OnLevelWasLoaded(int level) 
    {
        fPassedTime = 0f;
        bUseStockMaid = false;
 
        if ( !checker.IsTarget(level) ) {
            if (isActive) {
                SetCameraControl(true);
                initialized = false;
                isActive = false;
            }
            return;
        }

        bUseStockMaid = checker.IsStockTarget(level);
        menuType = MenuType.None;
        mouseDowned    = false;
        cursorContains = false;
        isActive = true;
    }
    // public void FixedUpdate()
    // {
    //     if (!isActive) return;

    //     // メイド情報の変更検知
    //     detector.Detect();
    // }
    public void Update()
    {
        fPassedTime += Time.deltaTime;
        if (!isActive) return;

        if (!initialized) {
            if (fPassedTime - fLastInitTime <= 1f) return;

            fLastInitTime = fPassedTime;
            initialized = Initialize();
            LogUtil.Debug("Initialized ", initialized);
            if (!initialized) return;
        }

        if (InputModifierKey() && Input.GetKeyDown(settings.toggleKey)) {
            SetMenu( (menuType == MenuType.None)? MenuType.Main : MenuType.None );
            mouseDowned    = false;
        }
        UpdateCameraControl();

        // 選択中のメイドが有効でなければ何もしない
        if (!holder.CurrentActivated()) return;
        
        if (toApplyPresetMaid != null && !toApplyPresetMaid.IsBusy) {
            var targetMaid = toApplyPresetMaid;
            toApplyPresetMaid = null;
            plugin.StartCoroutine( DelayFrame(applyDeleFrame, () => ApplyPresetProp(targetMaid, currentPreset)) );
        }
        if (ACCTexturesView.fileBrowser != null) {
            ACCTexturesView.fileBrowser.Update();
        }
        // テクスチャエディットの反映
        if (menuType == MenuType.Texture) {
            // マウスが離されたタイミングでのみテクスチャ反映
            if (texSliderUpped || Input.GetMouseButtonUp(0)) {
                if (ACCTexturesView.IsChangeTarget()) {
                    ACCTexturesView.UpdateTex(holder.currentMaid, targetMaterials);
                }
                texSliderUpped = false;
            }
        } else {
            // テクスチャモードでなければ、テクスチャ変更対象を消す
            ACCTexturesView.ClearTarget();
        }
    }

    public void OnGUI()
    {
        if (!isActive) return;
        if (menuType == MenuType.None) return;
        if (settings.SSWithoutUI && !IsEnabledUICamera()) return; // UI無し撮影

        try {
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
                OnTips();
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

            }
        } finally {
        }
    }
    #endregion

    private const int tipsMargin = 24;
    private bool displayTips;
    private Rect tipRect;
    private string tips;
    private void OnTips() {
        if (displayTips && tips != null) {
            GUI.Window(WINID_TIPS, tipRect, DoTips, tips, uiParams.tipsStyle);
        }
    }
        
    public void SetTips(string message) {
        
        int lineNum = 1;
        foreach (var chr in message) {
            if (chr == '\n') lineNum++;
        }
        if (lineNum == 1) lineNum += (message.Length / 15);
        float height = lineNum*uiParams.fontSize*19/14 + 30;

        if (height > 400) height = 400;
        tipRect = new Rect(uiParams.winRect.x+tipsMargin, uiParams.winRect.yMin+150,
                           uiParams.winRect.width-tipsMargin*2, height);
        displayTips = true;
        tips = message;

        plugin.StartCoroutine(DelaySecond(tipsSecond, () => {
             displayTips = false;
             tips = null;
        }) );
    }
    public void DoTips(int winID) {
        GUI.BringWindowToFront(winID);
    }

    bool IsEnabledUICamera() { 
        return UICamera.currentCamera != null && UICamera.currentCamera.enabled; 
    }

    private bool InputModifierKey() {
        EventModifiers em = Event.current.modifiers;
        if (settings.toggleModifiers == EventModifiers.None) {
            // 修飾キーが押されていない事を確認(Shift/Alt/Ctrl)
            return (em & modifierKey) == EventModifiers.None;
        }
        // 修飾キーが指定されている場合、そのキーの入力チェック
        return (em & settings.toggleModifiers) != EventModifiers.None;
    }

    private void SetCameraControl(bool enable) {
        if (cmrCtrlChanged == enable) {
            GameMain.Instance.MainCamera.SetControl(enable);
            UICamera.InputEnable = enable;
            cmrCtrlChanged = !enable;
        }
    }
    /// <summary>
    /// カーソル位置のチェックを行い、カメラコントロールの有効化/無効化を行う
    /// </summary>
    private void UpdateCameraControl() {
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

    #region Private methods
    private void Dispose() {
        ClearMaidData();
        SetCameraControl(true);
        mouseDowned    = false;
        cursorContains = false;

        // テクスチャキャッシュを開放する
        ACCTexturesView.Clear();
        ResourceHolder.Instance.Clear();
            //OnDestroy();

        initialized = false;
    }

    private bool Initialize() {
        InitMaidInfo();

        uiParams.Update();
        ACCTexturesView.Init(uiParams);
        ACCMaterialsView.Init(uiParams);

        return true;
        //return holder.currentMaid != null;
    }

    private void InitMaidInfo() {
        // ここでは、最初に選択可能なメイドを選択
        holder.UpdateMaid(ClearMaidData);
    }

    // http://qiita.com/toRisouP/items/e402b15b36a8f9097ee9
    IEnumerator DelayFrame(int delayFrame, Action act)　{
        for (var i = 0; i < delayFrame; i++) {
            yield return null;
        }
        act();
    }
    IEnumerator DelaySecond(int second, Action act)　{
        yield return new WaitForSeconds(second);
        act();
    }

    private void ClearMaidData() {
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

    private GUIContent GetOrAddMaidInfo(Maid m, int idx=-1) {
        GUIContent content;
        int id = m.gameObject.GetInstanceID();
        if (!contentDic.TryGetValue(id, out content)) {
            var status = m.Param.status;
            LogUtil.Debug("maid:", m.name);
            
            var maidName = (!m.boMAN)? status.last_name + " " + status.first_name : "男"+ (idx+1);
            Texture2D icon = m.GetThumIcon();
            content = new GUIContent(maidName, icon);
            contentDic[id] = content;
        }
        return content;
    }

    private bool IsEnabled(Maid m) {
        return m.isActiveAndEnabled && m.Visible ;// && m.body0.Face != null;
    }

    internal class SelectMaidData {
        public Maid maid;
        public GUIContent content;
        internal SelectMaidData(Maid maid0, GUIContent content0) {
            maid = maid0;
            content = content0;
        }
    }

    List<SelectMaidData> maidList = new List<SelectMaidData>();
    List<SelectMaidData> manList = new List<SelectMaidData>();

    private void InitMaidList() {
        maidList.Clear();
        manList.Clear();
        CharacterMgr chrMgr = GameMain.Instance.CharacterMgr;
        
        if (bUseStockMaid) {
            AddMaidList(maidList, chrMgr.GetStockMaid, chrMgr.GetStockMaidCount());
        } else {
            AddMaidList(maidList, chrMgr.GetMaid, chrMgr.GetMaidCount());
        }

        if (bUseStockMaid) {
            AddMaidList(manList, chrMgr.GetStockMan, chrMgr.GetStockManCount());
        } else {
            AddMaidList(manList, chrMgr.GetMan, chrMgr.GetManCount());
        }
    }
    private void AddMaidList(ICollection<SelectMaidData> list, Func<int, Maid> GetMaid, int count) {
        int idx = 0;
        for (int i=0; i< count; i++) {
            Maid m = GetMaid(i);
            if (m != null && IsEnabled(m)) {
                var status = m.Param.status;
                
                string maidName;
                if (!m.boMAN) {
                    maidName = status.last_name + " " + status.first_name;
                } else {
                    maidName = "男"+ (idx+1);
                    idx++;
                }
                Texture2D icon = m.GetThumIcon();
                var content = new GUIContent(maidName, icon);
                list.Add(new SelectMaidData(m, content));
            }
        }      
    }

    private void DoSelectMaid(int winID) {
        if (selectedMaid == null) selectedMaid = holder.currentMaid;

        GUILayout.BeginVertical();
        GUILayout.Label("メイド選択", uiParams.lStyleB);
        scrollViewPosition = GUILayout.BeginScrollView(scrollViewPosition, uiParams.optSubConWidth, uiParams.optSubConHeight);
        CharacterMgr chrMgr = GameMain.Instance.CharacterMgr;
        bool hasSelected = false;
        try {
            foreach (var maidData in maidList) {
                GUI.enabled = IsEnabled(maidData.maid);
                bool selected = (selectedMaid == maidData.maid);
                if (GUI.enabled && selected) hasSelected = true;

                GUILayout.BeginHorizontal();
                GUILayout.Space(uiParams.marginL);
                bool changed = GUILayout.Toggle(selected, maidData.content, uiParams.tStyleL);
                GUILayout.Space(uiParams.marginL);
                GUILayout.EndHorizontal();
                if (changed != selected) {
                    selectedMaid = maidData.maid;
                    selectedName = maidData.content.text;
                }
            }
            GUI.enabled = true;
            if (!maidList.Any()) GUILayout.Label("　なし", uiParams.lStyleB);

            GUILayout.Space(uiParams.marginL);

            if (manList.Any()) {
                // LogUtil.Debug("manList:", manList.Count);
                GUILayout.Label("男選択", uiParams.lStyleB);
                foreach (var manData in manList) {
                    Maid m = manData.maid;
                    GUI.enabled = IsEnabled(m);
                    bool selected = (selectedMaid == m);
                    if (GUI.enabled && selected) hasSelected = true;

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(uiParams.marginL);
                    bool changed = GUILayout.Toggle(selected, manData.content, uiParams.tStyleL);
                    GUILayout.Space(uiParams.marginL);
                    GUILayout.EndHorizontal();
                    if (changed != selected) {
                        selectedMaid = m;
                        selectedName = manData.content.text;
                    }
                }
                GUI.enabled = true;
                //if (!manList.Any()) GUILayout.Label("　なし", uiParams.lStyleB);
            }

        } finally {
            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            GUI.enabled = hasSelected;
            if (GUILayout.Button( "選択", uiParams.bStyle, uiParams.optSubConHalfWidth)) {
                SetMenu(MenuType.Main);
                holder.UpdateMaid(selectedMaid, selectedName, ClearMaidData);
                selectedMaid = null;
                selectedName = null;
                contentDic.Clear();
            }
            GUI.enabled = true;
            
            if (GUILayout.Button( "一覧更新", uiParams.bStyle, uiParams.optSubConHalfWidth)) {
                InitMaidList();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
        GUI.DragWindow(uiParams.titleBarRect);
    }

    private void DoMainMenu(int winID) {
        GUILayout.BeginVertical();
        try {
            Maid maid = holder.currentMaid;
            GUILayout.Label(TITLE_LABEL + holder.MaidName, uiParams.lStyle);
    
            if (GUILayout.Button("メイド/男 選択", uiParams.bStyle)) {
                InitMaidList();
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
                GUI.enabled &= (holder.isOfficial) && (toApplyPresetMaid == null);
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
            GUILayout.Label("マテ情報変更 スロット選択", uiParams.lStyleC);
            switchedName = GUILayout.Toggle(switchedName, "表示切替", uiParams.tStyleS, uiParams.optSLabelWidth);
            GUILayout.EndHorizontal();
            scrollViewPosition = GUILayout.BeginScrollView(scrollViewPosition, 
                                                           GUILayout.Width(uiParams.mainRect.width),
                                                           GUILayout.Height(uiParams.mainRect.height));
            try {
                var currentBody = holder.currentMaid.body0;
                if (holder.isOfficial) {
                    foreach (SlotInfo slot in ACConstants.SlotNames.Values) {
                        if (!slot.enable) continue;
    
                        // 身体からノード一覧と表示状態を取得
                        if (!currentBody.GetSlotLoaded(slot.Id)) continue;
    
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
                } else {
                    // 下記処理は、公式のスロットIDと異なるスロットを設定するプラグイン等が導入されていた場合のため
                    int idx = 0;
                    int count = currentBody.goSlot.Count;
                    for (int i=0; i< count; i++) {
                        TBodySkin tbodySlot = currentBody.goSlot[i];
                        if (!settings.enableMoza && i == count-1) {
                            if (tbodySlot.Category == "moza") continue;
                        }
                        // if (!slot.enable) continue;

                        // slot loaded
                        if (tbodySlot.obj != null) {
                            GUILayout.BeginHorizontal();
                            GUILayout.Space(uiParams.marginL);
                            
                            if (GUILayout.Button(tbodySlot.Category, uiParams.bStyleL, GUILayout.ExpandWidth(true))) {
                                holder.currentSlot = ACConstants.SlotNames[(TBody.SlotID)idx];
                                SetMenu(MenuType.Color);
                            }
                            GUILayout.Space(uiParams.marginL);
                            GUILayout.EndHorizontal();
                        }
                        idx++;
                    }
                }
            } finally {
                GUI.enabled = true;
                GUILayout.EndScrollView();
            }
            
        } finally {
            GUILayout.EndVertical();
        }
        GUI.DragWindow(uiParams.titleBarRect);
    }

    private List<ACCMaterialsView> InitMaterialView(Renderer r, string menufile) {
        // TODO menufile
        
        var materials = r.materials;
        int idx = 0;
        var ret = new List<ACCMaterialsView>(materials.Length);
        foreach (Material material in materials) { 
            var view = new ACCMaterialsView(r, material, idx++);
                view.tipsCall = SetTips;
            ret.Add(view);

            // マテリアル数が少ない場合はデフォルトで表示
            view.expand = (materials.Length <= 2);
        }
        return ret;
    }

    private GUIContent title;
    private void DoColorMenu(int winID) {
        TBodySkin slot = holder.GetCurrentSlot();
        if (title == null) {
            title = new GUIContent("マテリアル情報変更: " + (holder.isOfficial ? holder.currentSlot.DisplayName : slot.Category));
        }
            
        GUILayout.Label(title, uiParams.lStyleB);
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
            int menuId = holder.GetCurrentMenuFileID();
            if (menuId != 0) {
                if (slotDropped) {
                    if (changeCounter.Next()) {
                        SetMenu(MenuType.Main);
                        LogUtil.Debug("select slot item dropped. return to main menu.", menuId);
                        slotDropped = false;
                    }
                    return;
                }
            }

            // ターゲットのmenuファイルが変更された場合にビューを更新
            if (targetMenuId != menuId) {
                title = null;
                // .modファイルは未対応
                var menufile = holder.GetCurrentMenuFile();

                LogUtil.Debug("menufile changed.", targetMenuId, "=>", menuId, " : ", menufile);

                isSavable = (menufile != null)
                    && !(menufile.ToLower().EndsWith(FileConst.EXT_MOD, StringComparison.Ordinal));
                
                targetMenuId = menuId;
                var rendererer1 = holder.GetRenderer(slot);
                if (rendererer1 != null) {
                    targetMaterials = rendererer1.materials;
                    materialViews = InitMaterialView(rendererer1, menufile);
                } else {
                    targetMaterials = EMPTY_ARRAY;
                }
                
                // slotにデータが装着されていないかを判定
                slotDropped = (slot.obj == null);
                changeCounter.Reset();

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
                bool reload = refreshCounter.Next();
                if (reload) ClipBoardHandler.Instance.Reload();

                foreach (ACCMaterialsView view in materialViews) {
                    view.Show(reload);
                }
            }

        } catch (Exception e) {
            LogUtil.Error("マテリアル情報変更画面でエラーが発生しました。メイン画面へ移動します", e);
            SetMenu(MenuType.Main);
            targetMenuId = 0;
        } finally {
            GUILayout.EndScrollView();

            GUI.enabled = isSavable;
            if (GUILayout.Button("menuエクスポート", uiParams.bStyle)) ExportMenu();
            GUI.enabled = true;

            if (GUILayout.Button("閉じる", uiParams.bStyle)) {
                SetMenu(MenuType.Main);
                targetMenuId = 0;
            }
            GUI.DragWindow(uiParams.titleBarRect);
        }
    }

    private void ExportMenu() {
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

    private List<ACCTexturesView> InitTexView(Material[] materials) {
        var ret = new List<ACCTexturesView>(materials.Length);
        int matNo = 0;
        foreach (Material material in materials) {
            try {
                var view = new ACCTexturesView(material, matNo++);
                ret.Add(view);

                // マテリアル数が少ない場合はデフォルトで表示
                view.expand = (materials.Length <= 2);
                
            } catch(Exception e) {
                LogUtil.Error(material.name, e);
            }
        }
        return ret;
    }

    private void DoSelectTexture(int winId) {
        // テクスチャ変更画面 GUILayout使用
        GUILayout.Label("テクスチャ変更", uiParams.lStyleB);
        scrollViewPosition = GUILayout.BeginScrollView(scrollViewPosition, 
                                                       GUILayout.Width(uiParams.textureRect.width),
                                                       GUILayout.Height(uiParams.textureRect.height));
        try {
            int menuId = holder.GetCurrentMenuFileID();
            if (targetMenuId != menuId) {
                LogUtil.DebugF("manufile changed. {0}=>{1}", targetMenuId, menuId);
                targetMenuId = menuId;
                targetMaterials = holder.GetMaterials();
                texViews = InitTexView(targetMaterials);
            }

            foreach (ACCTexturesView view in texViews) {
                view.Show();
            }
        } catch(Exception e) {
            LogUtil.Debug("failed to create texture change view. ", e);
        } finally {
            GUILayout.EndScrollView();

            if (GUILayout.Button( "閉じる", uiParams.bStyle, 
                                 uiParams.optSubConWidth, uiParams.optBtnHeight)) {
                SetMenu(MenuType.Color);
            }
            GUI.DragWindow(uiParams.titleBarRect);
        }
    }

    private void DoFileBrowser(int winId) {
        ACCTexturesView.fileBrowser.OnGUI();
        GUI.DragWindow(uiParams.titleBarRect);
    }

    private bool InitMaskSlots() {
        if (holder.currentMaid == null) return false;

//        List<int> maskSlots = holder.maid.listMaskSlot;
        foreach (SlotInfo si  in ACConstants.SlotNames.Values) {
            if (!si.enable || !si.maskable) continue;
            int slotNo = (int)si.Id;
            if (slotNo >= holder.currentMaid.body0.goSlot.Count) continue;

            TBodySkin slot = holder.currentMaid.body0.GetSlot(slotNo);
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

    private void DoMaskSelectMenu(int winID) {
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

        GUI.DragWindow(uiParams.titleBarRect);
    }

    private bool InitDelNodes(TBodySkin body) {
        if (body == null) {
            if (holder.currentMaid == null) return false;

            const int slotNo = (int)TBody.SlotID.body;
            // 身体からノード一覧と表示状態を取得
            if (slotNo >= holder.currentMaid.body0.goSlot.Count) return false;
            body = holder.currentMaid.body0.GetSlot(slotNo);
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
            if (slot.obj == null || !slot.boVisible) continue;

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

            const int slotNo = (int)TBody.SlotID.body;
            // 身体からノード一覧と表示状態を取得
            if (slotNo >= holder.currentMaid.body0.goSlot.Count) return;
            body = holder.currentMaid.body0.GetSlot(slotNo);
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
                    SyncNodes();
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
                        LogUtil.Debug("node name not found.", pair.Key);
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
        GUILayout.BeginHorizontal();
        try {
            if (GUILayout.Button("適用", uiParams.bStyle, uiParams.optSubConHalfWidth, uiParams.optBtnHeight)) {
            holder.SetDelNodes(dDelNodes, true);
                //dDelNodeDisps = new Dictionary<string, bool>(body.m_dicDelNodeBody);
                plugin.StartCoroutine(DelayFrame(3, SyncNodes));
            }
            GUILayout.Space(uiParams.margin);
            if (GUILayout.Button("強制適用", uiParams.bStyle, uiParams.optSubConHalfWidth, uiParams.optBtnHeight)) {
                holder.SetDelNodesForce(dDelNodes, true);
                //dDelNodeDisps = new Dictionary<string, bool>(body.m_dicDelNodeBody);
                plugin.StartCoroutine(DelayFrame(3, SyncNodes));
            }
        } finally {
            GUILayout.EndHorizontal();
        }
        if (GUILayout.Button( "閉じる", uiParams.bStyle, uiParams.optSubConWidth, uiParams.optBtnHeight)) {
            SetMenu(MenuType.Main);
        }
        GUI.DragWindow(uiParams.titleBarRect);
    }
    private void SyncNodes() {
        dDelNodeDisps = GetDelNodes();
        foreach (var nodes in dDelNodeDisps) {
            dDelNodes[nodes.Key] = nodes.Value;
        }
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
                if (GUILayout.Button(presetName1, uiParams.lStyleS)) {
                    presetName = presetName1;
                    bPresetSavable = !FileConst.HasInvalidChars(presetName);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();

            GUI.enabled = bPresetSavable;
            if (GUILayout.Button("保存", uiParams.bStyle, GUILayout.ExpandWidth(true))) {
                SavePreset(presetName);
            }
            GUI.enabled = true;
            if (GUILayout.Button("閉じる", uiParams.bStyle, GUILayout.ExpandWidth(true))) {
                SetMenu(MenuType.Main);
            }
        } finally {
            GUILayout.EndHorizontal();
            GUI.DragWindow(uiParams.titleBarRect);
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
            GUILayout.Space(uiParams.marginL);
            bPresetApplyBodyProp = GUILayout.Toggle(bPresetApplyBodyProp, "身体設定値", uiParams.tStyle);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Space(uiParams.marginL*2);
            bPresetApplyMask = GUILayout.Toggle(bPresetApplyMask, "マスク", uiParams.tStyle);
            bPresetApplyNode = GUILayout.Toggle(bPresetApplyNode, "ノード表示", uiParams.tStyle);
            bPresetApplyPartsColor = GUILayout.Toggle(bPresetApplyPartsColor, "無限色", uiParams.tStyle);
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
            GUI.DragWindow(uiParams.titleBarRect);
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

    private void SavePreset(string presetName1) {
        if (!Directory.Exists(settings.presetDirPath)) Directory.CreateDirectory(settings.presetDirPath);

        try {
            var filepath = presetMgr.GetPresetFilepath(presetName1);
            dDelNodeDisps = GetDelNodes();
            presetMgr.Save(filepath, presetName1, dDelNodeDisps);
            SetMenu(MenuType.Main);

            // 一覧を更新
            LoadPresetList();
        } catch(Exception e) {
            LogUtil.Error(e);
        }        
    }

    private void ApplyPreset() {
        ApplyPreset(currentPreset);
    }

    private bool ApplyPreset(string presetName1) {
        LogUtil.Debug("Applying Preset. ", presetName1);
        var filename = presetMgr.GetPresetFilepath(presetName1);
        if (!File.Exists(filename)) return false;

        currentPreset = presetMgr.Load(filename);
        if (currentPreset == null) return false;

        ApplyPreset(currentPreset);
        return true;
    }

    private void ApplyPreset(PresetData preset) {
        if (preset == null) return;

        var maid = holder.currentMaid;;
        // 衣装チェンジ
        if (preset.mpns.Any()) {
            presetMgr.ApplyPresetMPN(maid, preset, bPresetApplyBody, bPresetApplyWear, bPresetCastoff);
        }
        // 身体設定値
        if (bPresetApplyBodyProp & preset.mpnvals.Any()) {
            presetMgr.ApplyPresetMPNProp(maid, preset);
        }
        
        // 一旦、衣装や身体情報を適用⇒反映待ちをして、Coroutineにて残りを適用
        holder.FixFlag(maid);
        toApplyPresetMaid = maid; 

        // 後で実行 toApplyPresetMaid を指定することでメイド情報のロード完了後に実行
        //ApplyPresetProp(preset);
    }
    
    // ACCの変更情報を適用する
    private void ApplyPresetProp(Maid targetMaid, PresetData preset) {
        try {
            // 対象メイドが変更された場合はスキップ
            if (holder.currentMaid != targetMaid) return;

            if (bPresetApplyWear) {
                presetMgr.ApplyPresetMaterial(targetMaid, preset);
            }
            if (bPresetApplyNode && preset.delNodes != null) {
                // 表示ノードを反映 (プリセットで未定義のノードは変更されない）
                foreach (var node in preset.delNodes) {
                    dDelNodes[node.Key] = node.Value;
                }
                holder.SetDelNodes(targetMaid, preset, false);
            }
            if (bPresetApplyMask) {
                holder.SetMaskSlots(targetMaid, preset);
            }
            holder.FixFlag(targetMaid);

            // freeColor
            if (bPresetApplyPartsColor && preset.partsColors.Any()) {
                presetMgr.ApplyPresetPartsColor(targetMaid, preset);
            }

        } finally {
            LogUtil.Debug("Preset applyed");
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
            LogUtil.Debug(e);
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
            LogUtil.Debug("failed to display save dialog.", e);
        }

        GUI.DragWindow(uiParams.titleBarRect);
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
    private const int fontPxSS = (int)(fontPx*0.8f);
    private const int fontPxL = 20;
    #endregion

    private int width;
    private int height;
    private float ratio;
    
    public int margin;
    public int marginL; //
    public int fontSize;
    public int fontSizeS;
    public int fontSizeSS;
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
    public readonly GUIStyle tStyleSS  = new GUIStyle("toggle");
    public readonly GUIStyle tStyleL   = new GUIStyle("toggle");
    public readonly GUIStyle listStyle = new GUIStyle();
    public readonly GUIStyle textStyle = new GUIStyle("textField");
    public readonly GUIStyle textStyleSC = new GUIStyle("textField");
    public readonly GUIStyle textAreaStyleS = new GUIStyle("textArea");

    public readonly GUIStyle boxStyle      = new GUIStyle("box");
    public readonly GUIStyle winStyle      = new GUIStyle("box");
    public readonly GUIStyle dialogStyle   = new GUIStyle("box");
    public readonly GUIStyle tipsStyle      = new GUIStyle("window");

    public readonly Color textColor = new Color(1f, 1f, 1f, 0.98f);

    public Rect titleBarRect    = new Rect();

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
        //lStyle.wordWrap          = false;

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
        tStyleSS.normal.textColor       = textColor;
        tStyleSS.alignment              = TextAnchor.MiddleLeft;
        
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
        tipsStyle.alignment = TextAnchor.MiddleCenter;
        tipsStyle.wordWrap = true;
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
        fontSizeSS = FixPx(fontPxSS);
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
        tStyleSS.fontSize      = fontSizeSS;
        tStyleL.fontSize       = fontSizeL;
        listStyle.fontSize     = fontSizeS;
        textStyle.fontSize     = fontSize;
        textStyleSC.fontSize    = fontSizeS;
        textAreaStyleS.fontSize = fontSizeS;

        LogUtil.DebugF("screen=({0},{1}),margin={2},height={3},ratio={4})", width, height, margin, itemHeight, ratio);

        winStyle.fontSize  = fontSize;
        tipsStyle.fontSize = fontSize;
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
        titleBarRect.Set (0, 0,  winRect.width, 24f);

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