using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEngine;
using UnityInjector;
using UnityInjector.Attributes;

namespace CM3D2.AlwaysColorChange.Plugin
{
    [PluginFilter("CM3D2x64"),
    PluginFilter("CM3D2x86"),
    PluginFilter("CM3D2VRx64"),
    PluginName("CM3D2 OffScreen"),
    PluginVersion("0.0.2.5")]
    public class AlwaysColorChange : PluginBase
    {
        public const string Version = "0.0.2.5";

        private const float GUIWidth = 0.25f;

        private enum TargetLevel
        {
            //ダンス:ドキドキ☆Fallin' Love
            SceneDance_DDFL = 4,

            // エディット
            SceneEdit = 5,

            // 夜伽
            SceneYotogi = 14,

            // ADVパート
            SceneADV = 15,

            // ダンス:entrance to you
            SceneDance_ETYL = 20
        }

        private enum MenuType
        {
            None,
            Main,
            Color,
            NodeSelect,
            Save,
            PresetSelect
        }

        private Dictionary<string, string> Slotnames;

        private Dictionary<string, string> Nodenames;

        private MenuType menuType;

        private KeyCode toggleKey = KeyCode.F12;

        private Rect winRect;

        private Vector2 scrollViewVector = Vector2.zero;

        private string currentSlotname;

        private Dictionary<string, CCMaterial> dMaterial;

        private Dictionary<string, Shader> InitialShaders;

        private Maid maid;

        private bool bApplyChange = false;

        private CCPreset targetPreset;

        public AlwaysColorChange()
        {
            Slotnames = new Dictionary<string, string>();
            Slotnames.Add("身体", "body");
            Slotnames.Add("乳首", "chikubi");
            Slotnames.Add("アンダーヘア", "underhair");
            Slotnames.Add("頭", "head");
            Slotnames.Add("目", "eye");
            Slotnames.Add("歯", "accHa");
            Slotnames.Add("前髪", "hairF");
            Slotnames.Add("後髪", "hairR");
            Slotnames.Add("横髪", "hairS");
            Slotnames.Add("エクステ毛", "hairT");
            Slotnames.Add("アホ毛", "hairAho");
            Slotnames.Add("帽子", "accHat");
            Slotnames.Add("ヘッドドレス", "headset");
            Slotnames.Add("トップス", "wear");
            Slotnames.Add("ボトムス", "skirt");
            Slotnames.Add("ワンピース", "onepiece");
            Slotnames.Add("水着", "mizugi");
            Slotnames.Add("ブラジャー", "bra");
            Slotnames.Add("パンツ", "panz");
            Slotnames.Add("靴下", "stkg");
            Slotnames.Add("靴", "shoes");
            Slotnames.Add("アクセ：前髪", "accKami_1_");
            Slotnames.Add("アクセ：前髪：左", "accKami_2_");
            Slotnames.Add("アクセ：前髪：右", "accKami_3_");
            Slotnames.Add("アクセ：メガネ", "megane");
            Slotnames.Add("アクセ：アイマスク", "accHead");
            Slotnames.Add("アクセ：手袋", "glove");
            Slotnames.Add("アクセ：鼻", "accHana");
            Slotnames.Add("アクセ：左耳", "accMiMiL");
            Slotnames.Add("アクセ：右耳", "accMiMiR");
            Slotnames.Add("アクセ：ネックレス", "accKubi");
            Slotnames.Add("アクセ：チョーカー", "accKubiwa");
            Slotnames.Add("アクセ：（左）リボン", "accKamiSubL");
            Slotnames.Add("アクセ：右リボン", "accKamiSubR");
            Slotnames.Add("アクセ：左乳首", "accNipL");
            Slotnames.Add("アクセ：右乳首", "accNipR");
            Slotnames.Add("アクセ：腕", "accUde");
            Slotnames.Add("アクセ：へそ", "accHeso");
            Slotnames.Add("アクセ足首", "accAshi");
            Slotnames.Add("アクセ：背中", "accSenaka");
            Slotnames.Add("アクセ：しっぽ", "accShippo");
            Slotnames.Add("アクセ：前穴", "accXXX");
            Slotnames.Add("精液：中", "seieki_naka");
            Slotnames.Add("精液：腹", "seieki_hara");
            Slotnames.Add("精液：顔", "seieki_face");
            Slotnames.Add("精液：胸", "seieki_mune");
            Slotnames.Add("精液：尻", "seieki_hip");
            Slotnames.Add("精液：腕", "seieki_ude");
            Slotnames.Add("精液：足", "seieki_ashi");
            Slotnames.Add("手持アイテム：左", "HandItemL");
            Slotnames.Add("手持ちアイテム：右", "HandItemR");
            Slotnames.Add("首輪？", "kubiwa");
            Slotnames.Add("拘束具：上？", "kousoku_upper");
            Slotnames.Add("拘束具：下？", "kousoku_lower");
            Slotnames.Add("アナルバイブ？", "accAnl");
            Slotnames.Add("バイブ？", "accVag");
            Slotnames.Add("チ○コ", "chinko");
            Nodenames = new Dictionary<string, string>();
            Nodenames.Add("頭", "Bip01 Head");
            Nodenames.Add("首", "Bip01 Neck_SCL_");
            Nodenames.Add("左胸上", "Mune_L_sub");
            Nodenames.Add("左胸下", "Mune_L");
            Nodenames.Add("右胸上", "Mune_R_sub");
            Nodenames.Add("右胸下", "Mune_R");
            Nodenames.Add("骨盤", "Bip01 Pelvis_SCL_");
            Nodenames.Add("脊椎", "Bip01 Spine_SCL_");
            Nodenames.Add("腰中", "Bip01 Spine1_SCL_");
            Nodenames.Add("腹部", "Bip01 Spine0a_SCL_");
            Nodenames.Add("胸部", "Bip01 Spine1a_SCL_");
            Nodenames.Add("股間", "Bip01");
            Nodenames.Add("左尻", "Hip_L");
            Nodenames.Add("右尻", "Hip_R");
            Nodenames.Add("左前腿", "momotwist_L");
            Nodenames.Add("左後腿", "momoniku_L");
            Nodenames.Add("左前腿下部", "momotwist2_L");
            Nodenames.Add("左ふくらはぎ", "Bip01 L Thigh_SCL_");
            Nodenames.Add("左足首", "Bip01 L Calf_SCL_");
            Nodenames.Add("左足小指付け根", "Bip01 L Toe0");
            Nodenames.Add("左足小指先", "Bip01 L Toe01");
            Nodenames.Add("左足中指付け根", "Bip01 L Toe1");
            Nodenames.Add("左足中指先", "Bip01 L Toe11");
            Nodenames.Add("左足親指付け根", "Bip01 L Toe2");
            Nodenames.Add("左足親指先", "Bip01 L Toe21");
            Nodenames.Add("右前腿", "momotwist_R");
            Nodenames.Add("右後腿", "momoniku_R");
            Nodenames.Add("右前腿下部", "momotwist2_R");
            Nodenames.Add("右ふくらはぎ", "Bip01 R Thigh_SCL_");
            Nodenames.Add("右足首", "Bip01 R Calf_SCL_");
            Nodenames.Add("右足小指付け根", "Bip01 R Toe0");
            Nodenames.Add("右足小指先", "Bip01 R Toe01");
            Nodenames.Add("右足中指付け根", "Bip01 R Toe1");
            Nodenames.Add("右足中指先", "Bip01 R Toe11");
            Nodenames.Add("右足親指付け根", "Bip01 R Toe2");
            Nodenames.Add("右足親指先", "Bip01 R Toe21");
            Nodenames.Add("左鎖骨", "Bip01 L Clavicle_SCL_");
            Nodenames.Add("左肩", "Kata_L");
            Nodenames.Add("左肩上腕", "Kata_L_nub");
            Nodenames.Add("左上腕A", "Uppertwist_L");
            Nodenames.Add("左上腕B", "Uppertwist1_L");
            Nodenames.Add("左上腕", "Bip01 L UpperArm");
            Nodenames.Add("左肘", "Bip01 L Forearm");
            Nodenames.Add("左前腕", "Foretwist1_L");
            Nodenames.Add("左手首", "Foretwist_L");
            Nodenames.Add("左手", "Bip01 L Hand");
            Nodenames.Add("左親指付け根", "Bip01 L Finger0");
            Nodenames.Add("左親指関節", "Bip01 L Finger01");
            Nodenames.Add("左親指先", "Bip01 L Finger02");
            Nodenames.Add("左人指し指付け根", "Bip01 L Finger1");
            Nodenames.Add("左人指し指関節", "Bip01 L Finger11");
            Nodenames.Add("左人指し指先", "Bip01 L Finger12");
            Nodenames.Add("左中指付け根", "Bip01 L Finger2");
            Nodenames.Add("左中指関節", "Bip01 L Finger21");
            Nodenames.Add("左中指先", "Bip01 L Finger22");
            Nodenames.Add("左薬指付け根", "Bip01 L Finger3");
            Nodenames.Add("左薬指関節", "Bip01 L Finger31");
            Nodenames.Add("左薬指先", "Bip01 L Finger32");
            Nodenames.Add("左小指付け根", "Bip01 L Finger4");
            Nodenames.Add("左小指関節", "Bip01 L Finger41");
            Nodenames.Add("左小指先", "Bip01 L Finger42");
            Nodenames.Add("右鎖骨", "Bip01 R Clavicle_SCL_");
            Nodenames.Add("右肩", "Kata_R");
            Nodenames.Add("右肩上腕", "Kata_R_nub");
            Nodenames.Add("右上腕A", "Uppertwist_R");
            Nodenames.Add("右上腕B", "Uppertwist1_R");
            Nodenames.Add("右上腕", "Bip01 R UpperArm");
            Nodenames.Add("右肘", "Bip01 R Forearm");
            Nodenames.Add("右前腕", "Foretwist1_R");
            Nodenames.Add("右手首", "Foretwist_R");
            Nodenames.Add("右手", "Bip01 R Hand");
            Nodenames.Add("右親指付け根", "Bip01 R Finger0");
            Nodenames.Add("右親指関節", "Bip01 R Finger01");
            Nodenames.Add("右親指先", "Bip01 R Finger02");
            Nodenames.Add("右人指し指付け根", "Bip01 R Finger1");
            Nodenames.Add("右人指し指関節", "Bip01 R Finger11");
            Nodenames.Add("右人指し指先", "Bip01 R Finger12");
            Nodenames.Add("右中指付け根", "Bip01 R Finger2");
            Nodenames.Add("右中指関節", "Bip01 R Finger21");
            Nodenames.Add("右中指先", "Bip01 R Finger22");
            Nodenames.Add("右薬指付け根", "Bip01 R Finger3");
            Nodenames.Add("右薬指関節", "Bip01 R Finger31");
            Nodenames.Add("右薬指先", "Bip01 R Finger32");
            Nodenames.Add("右小指付け根", "Bip01 R Finger4");
            Nodenames.Add("右小指関節", "Bip01 R Finger41");
            Nodenames.Add("右小指先", "Bip01 R Finger42");

        }

        private void changeColor(string slotname)
        {
            maid = GameMain.Instance.CharacterMgr.GetMaid(0);
            if (maid == null)
            {
                return;
            }
            DebugLog("target", slotname, Slotnames[slotname]);
            TBody body = maid.body0;
            List<TBodySkin> goSlot = body.goSlot;
            int index = (int)global::TBody.hashSlotName[Slotnames[slotname]];
            global::TBodySkin tBodySkin = goSlot[index];
            GameObject obj = tBodySkin.obj;
            if (obj == null)
            {
                return;
            }
            Transform[] componentsInChildren = obj.transform.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < componentsInChildren.Length; i++)
            {
                Transform transform = componentsInChildren[i];
                Renderer renderer = transform.renderer;
                if (renderer != null && renderer.material != null)
                {
                    Material[] materials = renderer.materials;
                    foreach (Material material in materials)
                    {
                        Shader shader = material.shader;
                        if (!InitialShaders.ContainsKey(material.name))
                        {
                            if (shader.name == "Hidden/InternalErrorShader")
                            {
                                InitialShaders.Add(material.name, null);
                            }
                            else
                            {
                                InitialShaders.Add(material.name, shader);
                            }
                        }

                        string sharderName = shader.name;
                        try
                        {
                            if (dMaterial[slotname].color.a < 1f)
                            {
                                if (sharderName.Contains("Outline"))
                                {
                                    shader = Shader.Find(sharderName.Replace("Outline", "Trans"));
                                    if (shader == null)
                                    {
                                        shader = Shader.Find("CM3D2/Toony_Lighted_Trans");
                                    }
                                    material.shader = shader;
                                }
                            }
                            else
                            {
                                material.shader = InitialShaders[material.name];
                            }
                        }
                        catch (Exception e)
                        {
                            DebugLog(e.StackTrace);
                        }
                        if (material.shader != null)
                        {
                            DebugLog(transform.name, material.name, material.shader.name);
                        }
                        material.color = dMaterial[slotname].color;

                    }
                }
            }
        }

        private void Awake()
        {
        }

        private void OnLevelWasLoaded(int level)
        {
            if (!Enum.IsDefined(typeof(TargetLevel), level))
            {
                return;
            }

            dMaterial = new Dictionary<string, CCMaterial>();
            InitialShaders = new Dictionary<string, Shader>();
            foreach (string slotname in Slotnames.Keys)
            {
                dMaterial.Add(slotname, new CCMaterial());
            }
            winRect = new Rect(Screen.width - FixPx(250), FixPx(20), FixPx(240), Screen.height - FixPx(30));
            menuType = MenuType.None;
            LoadSettings();
            bApplyChange = false;
        }

        private void Update()
        {
            if (!Enum.IsDefined(typeof(TargetLevel), Application.loadedLevel))
            {
                return;
            }

            maid = GameMain.Instance.CharacterMgr.GetMaid(0);
            if (maid == null)
            {
                return;
            }

            if (Input.GetKeyDown(toggleKey))
            {
                if (menuType == MenuType.None)
                {
                    menuType = MenuType.Main;
                }
                else
                {
                    menuType = MenuType.None;
                }
            }

            if (menuType != MenuType.None)
            {
                if (winRect.Contains(Input.mousePosition))
                {
                    GameMain.Instance.MainCamera.SetControl(false);
                }
                else
                {
                    GameMain.Instance.MainCamera.SetControl(true);
                }
            }

            if (bApplyChange && !maid.boAllProcPropBUSY)
            {
                ApplyPreset();
            }
        }

        private void OnGUI()
        {
            if (!Enum.IsDefined(typeof(TargetLevel), Application.loadedLevel))
            {
                return;
            }
            if (menuType == MenuType.None)
                return;

            Maid maid = GameMain.Instance.CharacterMgr.GetMaid(0);
            if (maid == null)
                return;

            GUIStyle winStyle = "box";
            winStyle.fontSize = FixPx(fontPx);
            winStyle.alignment = TextAnchor.UpperRight;

            switch (menuType)
            {
                case MenuType.Main:
                    winRect = GUI.Window(12, winRect, DoMainMenu, AlwaysColorChange.Version, winStyle);
                    break;
                case MenuType.Color:
                    winRect = GUI.Window(12, winRect, DoColorMenu, AlwaysColorChange.Version, winStyle);
                    break;
                case MenuType.NodeSelect:
                    winRect = GUI.Window(12, winRect, DoNodeSelectMenu, AlwaysColorChange.Version, winStyle);
                    break;
                case MenuType.Save:
                    winRect = GUI.Window(12, winRect, DoSaveMenu, AlwaysColorChange.Version, winStyle);
                    break;
                case MenuType.PresetSelect:
                    winRect = GUI.Window(12, winRect, DoSelectPreset, AlwaysColorChange.Version, winStyle);
                    break;
                default:
                    break;
            }

            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                if (dDelNodes != null)
                {
                    List<string> keyList = new List<string>(dDelNodes.Keys);
                    foreach (string key in keyList)
                    {
                        Debug.Log(key);
                    }
                }
            }
        }

        private int marginPx = 4;
        private int fontPx = 14;
        private int itemHeightPx = 18;

        private void DoMainMenu(int winID)
        {
            float margin = FixPx(marginPx);
            float fontSize = FixPx(fontPx);
            float itemHeight = FixPx(itemHeightPx);

            Rect scrollRect = new Rect(margin, (itemHeight + margin) * 5 + margin, winRect.width - margin * 2, winRect.height - (itemHeight + margin) * 6);
            Rect conRect = new Rect(0, 0, scrollRect.width - 20, 0);
            Rect outRect = new Rect(0, 0, winRect.width - margin * 2, itemHeight);
            GUIStyle lStyle = "label";
            GUIStyle bStyle = "button";
            GUIStyle tStyle = "toggle";

            Color color = new Color(1f, 1f, 1f, 0.98f);
            lStyle.fontSize = FixPx(fontPx);
            lStyle.normal.textColor = color;
            bStyle.fontSize = FixPx(fontPx);
            bStyle.normal.textColor = color;
            tStyle.fontSize = FixPx(fontPx);
            tStyle.normal.textColor = color;
            GUI.Label(outRect, "強制カラーチェンジ", lStyle);
            outRect.y += itemHeight + margin;
            if (GUI.Button(outRect, "マスククリア", bStyle))
            {
                ClearMasks();
            }
            outRect.y += itemHeight + margin;
            if (GUI.Button(outRect, "ノード表示切り替えへ", bStyle))
            {
                menuType = MenuType.NodeSelect;
            }
            outRect.y += itemHeight + margin;
            if (GUI.Button(outRect, "保存", bStyle))
            {
                menuType = MenuType.Save;
            }
            if (presets != null && presets.Count > 0)
            {
                outRect.y += itemHeight + margin;
                if (GUI.Button(outRect, "プリセット適用", bStyle))
                {
                    LoadSettings();
                    menuType = MenuType.PresetSelect;
                }
            }
            outRect.y = 0;

            conRect.height += (itemHeight + margin) * Slotnames.Count + margin * 2;

            scrollViewVector = GUI.BeginScrollView(scrollRect, scrollViewVector, conRect);

            foreach (string slotname in Slotnames.Keys)
            {
                if (slotname.Equals("end"))
                    continue;
                if (GUI.Button(outRect, slotname, bStyle))
                {
                    currentSlotname = slotname;
                    menuType = MenuType.Color;
                }
                outRect.y += itemHeight + margin;
            }
            GUI.EndScrollView();
            GUI.DragWindow();
        }

        private void ClearMasks()
        {
            for (int i = 0; i < maid.body0.goSlot.Count; i++)
            {
                TBodySkin tBodySkin = maid.body0.goSlot[i];
                tBodySkin.boVisible = true;
                tBodySkin.listMaskSlot.Clear();
            }
            maid.body0.FixMaskFlag();
            maid.body0.FixVisibleFlag(false);
            maid.AllProcPropSeqStart();
        }

        private Dictionary<string, bool> dDelNodes;

        private void FixDelNode(bool bApply)
        {
            if (dDelNodes == null)
            {
                return;
            }
            for (int i = 0; i < maid.body0.goSlot.Count; i++)
            {
                TBodySkin tBodySkin = maid.body0.goSlot[i];
                tBodySkin.boVisible = true;
                if (tBodySkin.m_dicDelNodeBody != null)
                {
                    foreach (string key in dDelNodes.Keys)
                    {
                        if (tBodySkin.m_dicDelNodeBody.ContainsKey(key))
                        {
                            tBodySkin.m_dicDelNodeBody[key] = dDelNodes[key];
                        }
                    }
                }
            }
            if (bApply)
            {
                maid.body0.FixMaskFlag();
                maid.body0.FixVisibleFlag(false);
                maid.AllProcPropSeqStart();
            }
        }

        private List<Material> GetMaterials(string slotname)
        {
            TBody body = maid.body0;
            List<TBodySkin> goSlot = body.goSlot;
            int index = (int)global::TBody.hashSlotName[Slotnames[slotname]];
            global::TBodySkin tBodySkin = goSlot[index];
            GameObject obj = tBodySkin.obj;
            if (obj == null)
            {
                return null;
            }
            List<Material> materialList = new List<Material>();
            Transform[] componentsInChildren = obj.transform.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < componentsInChildren.Length; i++)
            {
                Transform transform = componentsInChildren[i];
                Renderer renderer = transform.renderer;
                if (renderer != null && renderer.material != null && renderer.material.shader != null)
                {
                    materialList.AddRange(renderer.materials);
                }
            }
            return materialList;
        }

        private List<Renderer> GetRenderers(string slotname)
        {
            TBody body = maid.body0;
            List<TBodySkin> goSlot = body.goSlot;
            int index = (int)global::TBody.hashSlotName[Slotnames[slotname]];
            global::TBodySkin tBodySkin = goSlot[index];
            GameObject obj = tBodySkin.obj;
            if (obj == null)
            {
                return null;
            }
            List<Renderer> rendererList = new List<Renderer>();
            Transform[] componentsInChildren = obj.transform.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < componentsInChildren.Length; i++)
            {
                Transform transform = componentsInChildren[i];
                Renderer renderer = transform.renderer;
                if (renderer != null)
                {
                    rendererList.Add(renderer);
                }
            }
            return rendererList;
        }

        private void DoColorMenu(int winID)
        {
            float margin = FixPx(marginPx);
            float fontSize = FixPx(fontPx);
            float itemHeight = FixPx(itemHeightPx);

            Rect scrollRect = new Rect(margin, (itemHeight + margin), winRect.width - margin * 2, winRect.height - (itemHeight + margin) * 3);
            Rect conRect = new Rect(0, 0, scrollRect.width - 20, 0);
            Rect outRect = new Rect(margin, 0, winRect.width - margin * 2, itemHeight);
            GUIStyle lStyle = "label";
            GUIStyle bStyle = "button";
            GUIStyle tStyle = "toggle";

            Color color = new Color(1f, 1f, 1f, 0.98f);
            lStyle.fontSize = FixPx(fontPx);
            lStyle.normal.textColor = color;
            bStyle.fontSize = FixPx(fontPx);
            bStyle.normal.textColor = color;
            tStyle.fontSize = FixPx(fontPx);
            tStyle.normal.textColor = color;

            GUI.Label(outRect, "強制カラーチェンジ:" + currentSlotname, lStyle);

            outRect.y = 0;
            outRect.width -= margin * 2 + 20;
            List<Material> materialList = GetMaterials(currentSlotname);
            if (materialList != null)
            {
                conRect.height += (itemHeight + margin) * materialList.Count * 30 + margin;

                scrollViewVector = GUI.BeginScrollView(scrollRect, scrollViewVector, conRect);

                foreach (Material material in materialList)
                {
                    outRect.x = margin;
                    GUI.Label(outRect, material.name, lStyle);
                    outRect.y += itemHeight + margin;
                    outRect.x += margin;
                    GUI.Label(outRect, "Color", lStyle);
                    outRect.y += itemHeight + margin;
                    Color sColor = material.GetColor("_Color");
                    sColor.r = drawModValueSlider(outRect, sColor.r, 0f, 2f, String.Format("{0}:{1:F2}", "R", sColor.r), lStyle);
                    outRect.y += itemHeight + margin;
                    sColor.g = drawModValueSlider(outRect, sColor.g, 0f, 2f, String.Format("{0}:{1:F2}", "G", sColor.g), lStyle);
                    outRect.y += itemHeight + margin;
                    sColor.b = drawModValueSlider(outRect, sColor.b, 0f, 2f, String.Format("{0}:{1:F2}", "B", sColor.b), lStyle);
                    outRect.y += itemHeight + margin;
                    sColor.a = drawModValueSlider(outRect, sColor.a, 0f, 1f, String.Format("{0}:{1:F2}", "A", sColor.a), lStyle);
                    outRect.y += itemHeight + margin;

                    Color shadowColor = material.GetColor("_ShadowColor");
                    if (shadowColor != null)
                    {
                        GUI.Label(outRect, "Shadow Color", lStyle);
                        outRect.y += itemHeight + margin;
                        shadowColor.r = drawModValueSlider(outRect, shadowColor.r, 0f, 2f, String.Format("{0}:{1:F2}", "R", shadowColor.r), lStyle);
                        outRect.y += itemHeight + margin;
                        shadowColor.g = drawModValueSlider(outRect, shadowColor.g, 0f, 2f, String.Format("{0}:{1:F2}", "G", shadowColor.g), lStyle);
                        outRect.y += itemHeight + margin;
                        shadowColor.b = drawModValueSlider(outRect, shadowColor.b, 0f, 2f, String.Format("{0}:{1:F2}", "B", shadowColor.b), lStyle);
                        outRect.y += itemHeight + margin;
                        shadowColor.a = drawModValueSlider(outRect, shadowColor.a, 0f, 1f, String.Format("{0}:{1:F2}", "A", shadowColor.a), lStyle);
                        outRect.y += itemHeight + margin;
                    }
                    Color outlineColor = material.GetColor("_OutlineColor");
                    if (outlineColor != null)
                    {
                        GUI.Label(outRect, "Outline Color", lStyle);
                        outRect.y += itemHeight + margin;
                        outlineColor.r = drawModValueSlider(outRect, outlineColor.r, 0f, 2f, String.Format("{0}:{1:F2}", "R", outlineColor.r), lStyle);
                        outRect.y += itemHeight + margin;
                        outlineColor.g = drawModValueSlider(outRect, outlineColor.g, 0f, 2f, String.Format("{0}:{1:F2}", "G", outlineColor.g), lStyle);
                        outRect.y += itemHeight + margin;
                        outlineColor.b = drawModValueSlider(outRect, outlineColor.b, 0f, 2f, String.Format("{0}:{1:F2}", "B", outlineColor.b), lStyle);
                        outRect.y += itemHeight + margin;
                        outlineColor.a = drawModValueSlider(outRect, outlineColor.a, 0f, 1f, String.Format("{0}:{1:F2}", "A", outlineColor.a), lStyle);
                        outRect.y += itemHeight + margin;
                    }
                    Color rimColor = material.GetColor("_RimColor");
                    if (rimColor != null)
                    {
                        GUI.Label(outRect, "Rim Color", lStyle);
                        outRect.y += itemHeight + margin;
                        rimColor.r = drawModValueSlider(outRect, rimColor.r, 0f, 2f, String.Format("{0}:{1:F2}", "R", rimColor.r), lStyle);
                        outRect.y += itemHeight + margin;
                        rimColor.g = drawModValueSlider(outRect, rimColor.g, 0f, 2f, String.Format("{0}:{1:F2}", "G", rimColor.g), lStyle);
                        outRect.y += itemHeight + margin;
                        rimColor.b = drawModValueSlider(outRect, rimColor.b, 0f, 2f, String.Format("{0}:{1:F2}", "B", rimColor.b), lStyle);
                        outRect.y += itemHeight + margin;
                        rimColor.a = drawModValueSlider(outRect, rimColor.a, 0f, 1f, String.Format("{0}:{1:F2}", "A", rimColor.a), lStyle);
                        outRect.y += itemHeight + margin;
                    }
                    float? shininess = material.GetFloat("_Shininess");
                    if (shininess != null)
                    {
                        GUI.Label(outRect, "Shininess", lStyle);
                        outRect.y += itemHeight + margin;
                        shininess = drawModValueSlider(outRect, (float)shininess, 0f, 10f, String.Format("  {0:F2}", (float)shininess), lStyle);
                        outRect.y += itemHeight + margin;
                    }
                    float? outlineWidth = material.GetFloat("_OutlineWidth");
                    if (outlineWidth != null)
                    {
                        GUI.Label(outRect, "OutlineWidth", lStyle);
                        outRect.y += itemHeight + margin;
                        outlineWidth = drawModValueSlider(outRect, (float)outlineWidth, 0f, 0.1f, String.Format("  {0:F5}", (float)outlineWidth), lStyle);
                        outRect.y += itemHeight + margin;
                    }
                    float? rimPower = material.GetFloat("_RimPower");
                    if (rimPower != null)
                    {
                        GUI.Label(outRect, "RimPower", lStyle);
                        outRect.y += itemHeight + margin;
                        rimPower = drawModValueSlider(outRect, (float)rimPower, 0f, 100f, String.Format("  {0:F2}", (float)rimPower), lStyle);
                        outRect.y += itemHeight + margin;
                    }
                    float? rimShift = material.GetFloat("_RimShift");
                    if (rimShift != null)
                    {
                        GUI.Label(outRect, "RimShift", lStyle);
                        outRect.y += itemHeight + margin;
                        rimShift = drawModValueSlider(outRect, (float)rimShift, 0f, 5f, String.Format("  {0:F2}", (float)rimShift), lStyle);
                        outRect.y += itemHeight + margin;
                    }


                    string sharderName = material.shader.name;
                    try
                    {
                        Shader mShader = material.shader;
                        if (!InitialShaders.ContainsKey(material.name))
                        {
                            if (mShader.name == "Hidden/InternalErrorShader")
                            {
                                InitialShaders.Add(material.name, null);
                            }
                            else
                            {
                                InitialShaders.Add(material.name, mShader);
                            }
                        }
                        if (sColor.a < 1f)
                        {
                            if (sharderName.Contains("Outline"))
                            {
                                Shader shader = Shader.Find(sharderName.Replace("Outline", "Trans"));
                                if (shader == null)
                                {
                                    shader = Shader.Find("CM3D2/Toony_Lighted_Trans");
                                }
                                material.shader = shader;
                            }
                        }
                        else
                        {
                            material.shader = InitialShaders[material.name];
                        }
                    }
                    catch (Exception e)
                    {
                        DebugLog(e.StackTrace);
                    }
                    material.SetColor("_Color", sColor);
                    if (shadowColor != null)
                    material.SetColor("_ShadowColor", shadowColor);
                    if (outlineColor != null)
                        material.SetColor("_OutlineColor", outlineColor);
                    if (rimColor != null)
                        material.SetColor("_RimColor", rimColor);
                    if (shininess != null)
                        material.SetFloat("_Shininess", (float)shininess);
                    if (outlineWidth != null)
                        material.SetFloat("_OutlineWidth", (float)outlineWidth);
                    if (rimPower != null)
                        material.SetFloat("_RimPower", (float)rimPower);
                    if (rimShift != null)
                        material.SetFloat("_RimShift", (float)rimShift);

                    outRect.y += margin * 3;
                }

                GUI.EndScrollView();
            }

            outRect.x = margin;
            outRect.y = winRect.height - itemHeight - margin;
            outRect.width = winRect.width - margin * 2;
            if (GUI.Button(outRect, "閉じる", bStyle))
            {
                menuType = MenuType.Main;
            }
            GUI.DragWindow();
        }

        private void DoNodeSelectMenu(int winID)
        {
            float margin = FixPx(marginPx);
            float fontSize = FixPx(fontPx);
            float itemHeight = FixPx(itemHeightPx);

            Rect scrollRect = new Rect(margin, (itemHeight + margin) * 2, winRect.width - margin * 2, winRect.height - (itemHeight + margin) * 4);
            Rect conRect = new Rect(0, 0, scrollRect.width - 20, 0);
            Rect outRect = new Rect(0, 0, winRect.width - margin * 2, itemHeight);
            GUIStyle lStyle = "label";
            GUIStyle bStyle = "button";
            GUIStyle tStyle = "toggle";

            Color color = new Color(1f, 1f, 1f, 0.98f);
            lStyle.fontSize = FixPx(fontPx);
            lStyle.normal.textColor = color;
            bStyle.fontSize = FixPx(fontPx);
            bStyle.normal.textColor = color;
            tStyle.fontSize = FixPx(fontPx);
            tStyle.normal.textColor = color;
            GUI.Label(outRect, "表示ノード選択", lStyle);
            outRect.y += itemHeight + margin;
            if (GUI.Button(outRect, "すべてON", bStyle))
            {
                List<string> keyList = new List<string>(dDelNodes.Keys);
                foreach (string key in keyList)
                {
                    dDelNodes[key] = true;
                }
            }

            if (dDelNodes == null)
            {
                dDelNodes = new Dictionary<string, bool>();

                TBody body = maid.body0;
                List<TBodySkin> goSlot = body.goSlot;
                int index = (int)global::TBody.hashSlotName[Slotnames["身体"]];
                TBodySkin tBodySkin = goSlot[index];
                Dictionary<string, bool> dic = tBodySkin.m_dicDelNodeBody;
                foreach (string key in Nodenames.Keys)
                {
                    if (dic.ContainsKey(Nodenames[key]))
                    {
                        dDelNodes.Add(Nodenames[key], dic[Nodenames[key]]);
                    }
                }
            }

            conRect.height += (itemHeight + margin) * dDelNodes.Count + margin * 2;
            outRect.y = 0;
            outRect.x = margin * 2;

            scrollViewVector = GUI.BeginScrollView(scrollRect, scrollViewVector, conRect);
            foreach (string key in Nodenames.Keys)
            {
                if (dDelNodes.ContainsKey(Nodenames[key]))
                {
                    dDelNodes[Nodenames[key]] = GUI.Toggle(outRect, dDelNodes[Nodenames[key]], key, tStyle);
                    outRect.y += itemHeight + margin;
                }
            }

            GUI.EndScrollView();
            outRect.y = winRect.height - (itemHeight + margin) * 2;
            if (GUI.Button(outRect, "適用", bStyle))
            {
                FixDelNode(true);
            }
            outRect.y += itemHeight + margin;
            if (GUI.Button(outRect, "閉じる", bStyle))
            {
                menuType = MenuType.Main;
            }
            GUI.DragWindow();
        }

        private string presetName = "";

        private bool bClearMaskEnable = false;

        private bool bSaveBodyPreset = false;

        private void DoSaveMenu(int winID)
        {
            float margin = FixPx(marginPx);
            float fontSize = FixPx(fontPx);
            float itemHeight = FixPx(itemHeightPx);

            Rect outRect = new Rect(0, 0, winRect.width - margin * 2, itemHeight);
            GUIStyle lStyle = "label";
            GUIStyle bStyle = "button";
            GUIStyle tStyle = "toggle";
            GUIStyle textStyle = "textField";

            Color color = new Color(1f, 1f, 1f, 0.98f);
            lStyle.fontSize = FixPx(fontPx);
            lStyle.normal.textColor = color;
            bStyle.fontSize = FixPx(fontPx);
            bStyle.normal.textColor = color;
            tStyle.fontSize = FixPx(fontPx);
            tStyle.normal.textColor = color;
            textStyle.fontSize = FixPx(fontPx);
            textStyle.normal.textColor = color;

            GUI.Label(outRect, "保存", lStyle);
            outRect.y += itemHeight + margin;
            outRect.width = winRect.width * 0.3f - margin;
            lStyle.fontSize = FixPx(fontPx);
            GUI.Label(outRect, "プリセット名", lStyle);
            outRect.x += outRect.width;
            outRect.width = winRect.width * 0.7f - margin;
            presetName = GUI.TextField(outRect, presetName, textStyle);
            outRect.x = margin;
            outRect.y += outRect.height + margin;
            outRect.width = winRect.width - margin * 2;

            bClearMaskEnable = GUI.Toggle(outRect, bClearMaskEnable, "マスククリアを有効にする", tStyle);
            outRect.y += outRect.height + margin;

            bSaveBodyPreset = GUI.Toggle(outRect, bSaveBodyPreset, "身体も保存する", tStyle);
            outRect.y += outRect.height + margin;

            if (GUI.Button(outRect, "保存", bStyle))
            {
                if (presetName.Equals(""))
                {
                    // 名無しはNG
                    return;
                }
                SavePreset();
                menuType = MenuType.Main;
            }
            outRect.y += outRect.height + margin;
            if (GUI.Button(outRect, "閉じる", bStyle))
            {
                menuType = MenuType.Main;
            }

            GUI.DragWindow();

        }

        private void DoSelectPreset(int winId)
        {
            float margin = FixPx(marginPx);
            float fontSize = FixPx(fontPx);
            float itemHeight = FixPx(itemHeightPx);

            Rect scrollRect = new Rect(margin, (itemHeight + margin) * 2, winRect.width - margin * 2, winRect.height - (itemHeight + margin) * 4);
            Rect conRect = new Rect(0, 0, scrollRect.width - 20, 0);
            Rect outRect = new Rect(0, 0, winRect.width - margin * 2, itemHeight);
            GUIStyle lStyle = "label";
            GUIStyle bStyle = "button";
            GUIStyle tStyle = "toggle";

            Color color = new Color(1f, 1f, 1f, 0.98f);
            lStyle.fontSize = FixPx(fontPx);
            lStyle.normal.textColor = color;
            bStyle.fontSize = FixPx(fontPx);
            bStyle.normal.textColor = color;
            tStyle.fontSize = FixPx(fontPx);
            tStyle.normal.textColor = color;
            GUI.Label(outRect, "プリセット適用", lStyle);
            outRect.y += itemHeight + margin;

            conRect.height += (itemHeight + margin) * presets.Count + margin * 2;
            outRect.y = 0;
            outRect.x = margin * 2;

            scrollViewVector = GUI.BeginScrollView(scrollRect, scrollViewVector, conRect);
            foreach (var preset in presets)
            {
                if (GUI.Button(outRect, preset.Key, bStyle))
                {
                    targetPreset = preset.Value;
                    ApplyMpns();
                    menuType = MenuType.Main;
                }
                outRect.y += itemHeight + margin;
            }

            GUI.EndScrollView();
            outRect.y = winRect.height - (itemHeight + margin) + margin;
            if (GUI.Button(outRect, "閉じる", bStyle))
            {
                menuType = MenuType.Main;
            }
            GUI.DragWindow();
        }

        private void ApplyMpns()
        {
            if (targetPreset == null)
            {
                return;
            }

            if (targetPreset.mpns == null || targetPreset.mpns.Count == 0)
            {
                ApplyPreset();
            }
            else
            {

                // 衣装チェンジ
                foreach (string key in targetPreset.mpns.Keys)
                {
                    if (targetPreset.mpns[key].EndsWith("_del.menu"))
                    {
                        continue;
                    }
                    if (targetPreset.mpns[key].EndsWith(".mod"))
                    {
                        string sFilePath = Path.GetFullPath(".\\") + "Mod\\" + targetPreset.mpns[key];
                        if (!File.Exists(sFilePath))
                        {
                            continue;
                        }
                    }
                    maid.SetProp(key, targetPreset.mpns[key], targetPreset.mpns[key].ToLower().GetHashCode(), false);
                }
                maid.body0.FixMaskFlag();
                maid.body0.FixVisibleFlag(false);
                maid.AllProcPropSeqStart();
                bApplyChange = true;
            }
        }

        private void ApplyPreset()
        {
            dDelNodes = targetPreset.delNodes;
            FixDelNode(false);

            foreach (var slot in targetPreset.slots.Values)
            {
                if (!Slotnames.ContainsKey(slot.name))
                {
                    continue;
                }
                List<Material> materials = GetMaterials(slot.name);
                if (materials != null)
                {
                    foreach (var material in materials)
                    {
                        if (slot.materials.ContainsKey(material.name))
                        {
                            material.shader = Shader.Find(slot.materials[material.name].shader);
                            material.color = slot.materials[material.name].color;
                        }
                    }
                }
            }

            if (targetPreset.clearMask)
            {
                ClearMasks();
            }
            maid.body0.FixMaskFlag();
            maid.body0.FixVisibleFlag(false);
            maid.AllProcPropSeqStart();
            bApplyChange = false;
        }

        private string SaveFileName = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\Config\AlwaysColorChange.xml";

        private void SavePreset()
        {
            if (!File.Exists(SaveFileName))
            {
                var xml = new XDocument(
                    new XDeclaration("1.0", "utf-8", "true"),
                    new XElement("ColorChange",
                        new XAttribute("toggleKey", toggleKey)
                        )
                    );
                xml.Save(SaveFileName);
            }
            else
            {
                RemovePreset(presetName);
            }

            var xdoc = XDocument.Load(SaveFileName);

            var preset = new XElement("preset",
                new XAttribute("name", presetName),
                new XAttribute("clearMask", bClearMaskEnable)
                );
            var slots = new XElement("slots");
            foreach (string slotname in Slotnames.Keys)
            {
                List<Material> materialList = GetMaterials(slotname);
                if (materialList != null)
                {
                    var slot = new XElement("slot",
                        new XAttribute("slotname", slotname)
                        );
                    foreach (Material material in materialList)
                    {
                        Color color = material.GetColor("_Color");
                        Color shadowColor = material.GetColor("_ShadowColor");
                        Color rimColor = material.GetColor("_RimColor");
                        Color outlineColor = material.GetColor("_OutlineColor");
                        float shininess = material.GetFloat("_Shininess");
                        float outlineWidth = material.GetFloat("_OutlineWidth");
                        float rimPower = material.GetFloat("_RimPower");
                        float rimShift = material.GetFloat("_RimShift");
                        var materialNode = new XElement("material",
                            new XElement("name", material.name),
                            new XElement("shader", material.shader.name),
                            new XElement("color",
                                new XAttribute("R", color.r),
                                new XAttribute("G", color.g),
                                new XAttribute("B", color.b),
                                new XAttribute("A", color.a)),
                            new XElement("shadowColor",
                                new XAttribute("R", shadowColor.r),
                                new XAttribute("G", shadowColor.g),
                                new XAttribute("B", shadowColor.b),
                                new XAttribute("A", shadowColor.a)),
                            new XElement("rimColor",
                                new XAttribute("R", rimColor.r),
                                new XAttribute("G", rimColor.g),
                                new XAttribute("B", rimColor.b),
                                new XAttribute("A", rimColor.a)),
                            new XElement("outlineColor",
                                new XAttribute("R", outlineColor.r),
                                new XAttribute("G", outlineColor.g),
                                new XAttribute("B", outlineColor.b),
                                new XAttribute("A", outlineColor.a)),
                            new XElement("shininess", shininess),
                            new XElement("outlineWidth", outlineWidth),
                            new XElement("rimPower", rimPower),
                            new XElement("rimShift", rimShift)
                        );
                        slot.Add(materialNode);
                    }
                    slots.Add(slot);

                }
            }
            preset.Add(slots);

            var mpns = new XElement("mpns");
            if (bSaveBodyPreset)
            {
                for (int i = (int)MPN_TYPE_RANGE.BODY_START; i <= (int)MPN_TYPE_RANGE.BODY_END; i++)
                {
                    var mpn = (MPN)Enum.ToObject(typeof(MPN), i);
                    MaidProp mp = maid.GetProp(mpn);
                    if (!String.IsNullOrEmpty(mp.strFileName))
                    {
                        var mpnNode = new XElement("mpn",
                            new XAttribute("name", Enum.GetName(typeof(MPN), mpn)),
                            mp.strFileName);
                        mpns.Add(mpnNode);
                    }
                }
            }

            for (int i = (int)MPN_TYPE_RANGE.WEAR_START; i <= (int)MPN_TYPE_RANGE.WEAR_END; i++)
            {
                var mpn = (MPN)Enum.ToObject(typeof(MPN), i);
                MaidProp mp = maid.GetProp(mpn);
                if (!String.IsNullOrEmpty(mp.strFileName))
                {
                    var mpnNode = new XElement("mpn",
                        new XAttribute("name", Enum.GetName(typeof(MPN), mpn)),
                        mp.strFileName);
                    mpns.Add(mpnNode);
                }
            }
            preset.Add(mpns);

            var delNodes = new XElement("nodes");
            foreach (string key in Nodenames.Keys)
            {
                bool b = true;
                if (dDelNodes != null && dDelNodes.ContainsKey(Nodenames[key]))
                {
                    b = dDelNodes[Nodenames[key]];
                }
                var node = new XElement("node",
                    new XAttribute("name", key),
                    new XAttribute("visible", b));
                delNodes.Add(node);
            }
            preset.Add(delNodes);

            var presetNodes = xdoc.Descendants("preset");
            if (presetNodes.Count() == 0)
            {
                xdoc.Root.AddFirst(preset);
            }
            else
            {
                presetNodes.Last().AddAfterSelf(preset);
            }
            xdoc.Save(SaveFileName);
        }

        private void RemovePreset(string presetName)
        {
            var xdoc = XDocument.Load(SaveFileName);
            IEnumerable<XElement> removeTarget =
                from el in xdoc.Descendants("preset")
                where (string)el.Attribute("name") == presetName
                select el;

            if (removeTarget.Count() > 0)
            {
                foreach (var elem in removeTarget.ToList())
                {
                    DebugLog("remove preset", elem.ToString());
                    elem.Remove();
                }
                xdoc.Save(SaveFileName);
            }
        }

        private Dictionary<string, CCPreset> presets;

        private void LoadSettings()
        {
            if (!File.Exists(SaveFileName))
            {
                return;
            }
            var xdoc = XDocument.Load(SaveFileName);
            var toggleKey = xdoc.Root.Attribute("toggleKey");
            if (toggleKey != null && !String.IsNullOrEmpty(toggleKey.Value))
            {
                foreach (string keyName in Enum.GetNames(typeof(KeyCode)))
                {
                    if (toggleKey.Value.Equals(keyName))
                    {
                        this.toggleKey = (KeyCode)Enum.Parse(typeof(KeyCode), toggleKey.Value);
                    }
                }
            }
            var presetNodes = xdoc.Descendants("preset");
            if (presetNodes.Count() == 0)
            {
                return;
            }
            presets = new Dictionary<string, CCPreset>();
            foreach (var presetNode in presetNodes)
            {
                CCPreset preset = new CCPreset();
                preset.name = presetNode.Attribute("name").Value;
                var clearMask = presetNode.Attribute("clearMask");
                if (clearMask != null && !String.IsNullOrEmpty(clearMask.Value))
                {
                    preset.clearMask = (bool)clearMask;
                }

                preset.slots = new Dictionary<string, CCSlot>();
                var slotNodes = presetNode.Element("slots").Elements("slot");
                foreach (var slotNode in slotNodes)
                {
                    var slot = new CCSlot();
                    slot.name = slotNode.Attribute("slotname").Value;
                    slot.materials = new Dictionary<string, CCMaterial>();
                    var materialNodes = slotNode.Elements("material");
                    foreach (var materialNode in materialNodes)
                    {
                        var material = new CCMaterial();
                        material.name = materialNode.Element("name").Value;
                        material.shader = materialNode.Element("shader").Value;
                        var colorNode = materialNode.Element("color");
                        var r = (float)colorNode.Attribute("R");
                        var g = (float)colorNode.Attribute("G");
                        var b = (float)colorNode.Attribute("B");
                        var a = (float)colorNode.Attribute("A");
                        material.color = new Color(r, g, b, a);
                        colorNode = materialNode.Element("shadowColor");
                        if (colorNode != null)
                        {
                            r = (float)colorNode.Attribute("R");
                            g = (float)colorNode.Attribute("G");
                            b = (float)colorNode.Attribute("B");
                            a = (float)colorNode.Attribute("A");
                        }
                        material.shadowColor = new Color(r, g, b, a);
                        colorNode = materialNode.Element("rimColor");
                        if (colorNode != null)
                        {
                            r = (float)colorNode.Attribute("R");
                            g = (float)colorNode.Attribute("G");
                            b = (float)colorNode.Attribute("B");
                            a = (float)colorNode.Attribute("A");
                        }
                        material.rimColor = new Color(r, g, b, a);
                        colorNode = materialNode.Element("outlineColor");
                        if (colorNode != null)
                        {
                            r = (float)colorNode.Attribute("R");
                            g = (float)colorNode.Attribute("G");
                            b = (float)colorNode.Attribute("B");
                            a = (float)colorNode.Attribute("A");
                        }
                        material.outlineColor = new Color(r, g, b, a);

                        var f = materialNode.Element("shininess");
                        if (f != null)
                        {
                            material.shininess = (float)f;
                        }
                        f = materialNode.Element("outlineWidth");
                        if (f != null)
                        {
                            material.outlineWidth = (float)f;
                        }
                        f = materialNode.Element("rimPower");
                        if (f != null)
                        {
                            material.rimPower = (float)f;
                        }
                        f = materialNode.Element("rimShift");
                        if (f != null)
                        {
                            material.rimShift = (float)f;
                        }
                        slot.materials.Add(material.name, material);
                    }
                    preset.slots.Add(slot.name, slot);
                }

                preset.mpns = new Dictionary<string, string>();
                var mpnNodes = presetNode.Element("mpns").Elements("mpn");
                foreach (var mpnNode in mpnNodes)
                {
                    preset.mpns.Add(mpnNode.Attribute("name").Value, mpnNode.Value);
                }

                preset.delNodes = new Dictionary<string, bool>();
                var nodes = presetNode.Element("nodes").Elements("node");
                foreach (var node in nodes)
                {
                    string key = Nodenames[node.Attribute("name").Value];
                    preset.delNodes.Add(key, (bool)node.Attribute("visible"));
                }

                presets.Add(preset.name, preset);
            }
        }

        private float drawModValueSlider(Rect outRect, float value, float min, float max, string label, GUIStyle lstyle)
        {
            float conWidth = outRect.width;

            outRect.width = conWidth * 0.3f;
            GUI.Label(outRect, label, lstyle);
            outRect.x += outRect.width;

            outRect.width = conWidth * 0.7f;
            outRect.y += FixPx(5);

            return GUI.HorizontalSlider(outRect, value, min, max);
        }

        public int FixPx(int px)
        {
            return (int)((1.0f + (Screen.width / 1280.0f - 1.0f) * 0.6f) * px);
        }


        private const string DebugLogHeader = "AlwaysColorChange: ";

        public static void DebugLog(params string[] message)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(DebugLogHeader);
            for (int i = 0; i < message.Length; i++)
            {
                if (i > 0)
                {
                    sb.Append(":");
                }
                sb.Append(message[i]);
            }
            Debug.Log(sb.ToString());
        }

        public static void ErrorLog(params string[] message)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(DebugLogHeader);
            for (int i = 0; i < message.Length; i++)
            {
                if (i > 0)
                {
                    sb.Append(":");
                }
                sb.Append(message[i]);
            }
            Debug.LogError(sb.ToString());
        }

        class CCPreset
        {
            public string name;

            public bool clearMask = false;

            public Dictionary<string, CCSlot> slots;

            public Dictionary<string, string> mpns;

            public Dictionary<string, bool> delNodes;
        }
        class CCSlot
        {
            public string name;

            public bool enabled = false;

            public Dictionary<string, CCMaterial> materials;
        }
        class CCMaterial
        {
            public string name;
            public string shader;
            public Color color = new Color(1,1,1,1);
            public Color shadowColor = new Color(1, 1, 1, 1);
            public Color rimColor = new Color(1, 1, 1, 1);
            public Color outlineColor = new Color(0, 0, 0, 1);
            public float shininess = 0;
            public float outlineWidth = 0;
            public float rimPower = 0;
            public float rimShift = 0;
        }
        
    }
}