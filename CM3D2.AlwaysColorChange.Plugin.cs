using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityInjector;
using UnityInjector.Attributes;

namespace CM3D2.AlwaysColorChange.Plugin
{
    [PluginFilter("CM3D2x64"),
    PluginFilter("CM3D2x86"),
    PluginFilter("CM3D2VRx64"),
    PluginName("CM3D2 OffScreen"),
    PluginVersion("0.0.0.2")]
    public class AlwaysColorChange : PluginBase
    {
        public const string Version = "0.0.0.2";

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
            Color
        }

        private Dictionary<string, string> Slotnames;

        private MenuType menuType;

        private KeyCode toggleKey = KeyCode.F12;

        private Rect winRect;

        private Vector2 scrollViewVector = Vector2.zero;

        private string currentSlotname;

        private Dictionary<string, Color> slotColor;

        private Dictionary<string, Shader> InitialShaders;

        private Maid maid;

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
                            if (slotColor[slotname].a < 1f)
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
                        material.color = slotColor[slotname];

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

            slotColor = new Dictionary<string, Color>();
            InitialShaders = new Dictionary<string, Shader>();
            foreach (string slotname in Slotnames.Keys)
            {
                slotColor.Add(slotname, new Color(1f, 1f, 1f, 1f));
            }
            winRect = new Rect(Screen.width - FixPx(250), FixPx(20), FixPx(240), Screen.height - FixPx(30));
            menuType = MenuType.None;
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
            winStyle.fontSize = FixPx(11);
            winStyle.alignment = TextAnchor.UpperRight;

            switch (menuType)
            {
                case MenuType.Main:
                    winRect = GUI.Window(0, winRect, DoMainMenu, AlwaysColorChange.Version, winStyle);
                    break;
                case MenuType.Color:
                    winRect = GUI.Window(0, winRect, DoColorMenu, AlwaysColorChange.Version, winStyle);
                    break;
                default:
                    break;
            }
        }

        private void DoMainMenu(int winID)
        {
            float margin = FixPx(4);
            float fontSize = FixPx(14);
            float itemHeight = FixPx(18);

            Rect scrollRect = new Rect(margin, (itemHeight + margin) * 2, winRect.width - margin * 2, winRect.height - (itemHeight + margin) * 3);
            Rect conRect = new Rect(0, 0, scrollRect.width - 20, 0);
            Rect outRect = new Rect(0, 0, winRect.width - margin * 2, itemHeight);
            GUIStyle lStyle = "label";
            GUIStyle bStyle = "button";
            GUIStyle tStyle = "toggle";

            Color color = new Color(1f, 1f, 1f, 0.98f);
            lStyle.fontSize = FixPx(11);
            lStyle.normal.textColor = color;
            bStyle.fontSize = FixPx(11);
            bStyle.normal.textColor = color;
            tStyle.fontSize = FixPx(11);
            tStyle.normal.textColor = color;
            GUI.Label(outRect, "強制カラーチェンジ", lStyle);
            outRect.y += itemHeight + margin;
            if (GUI.Button(outRect, "適用", bStyle))
            {
                foreach (string slotname in Slotnames.Keys)
                {
                    changeColor(slotname);
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

        private void DoColorMenu(int winID)
        {
            float margin = FixPx(4);
            float fontSize = FixPx(14);
            float itemHeight = FixPx(18);

            Rect scrollRect = new Rect(margin, (itemHeight + margin), winRect.width - margin * 2, winRect.height - (itemHeight + margin) * 3);
            Rect conRect = new Rect(0, 0, scrollRect.width - 20, 0);
            Rect outRect = new Rect(margin, 0, winRect.width - margin * 2, itemHeight);
            GUIStyle lStyle = "label";
            GUIStyle bStyle = "button";

            Color color = new Color(1f, 1f, 1f, 0.98f);
            lStyle.fontSize = FixPx(11);
            lStyle.normal.textColor = color;
            bStyle.fontSize = FixPx(11);
            bStyle.normal.textColor = color;

            GUI.Label(outRect, "強制カラーチェンジ:" + currentSlotname, lStyle);

            outRect.y = 0;
            outRect.width -= margin;
            List<Material> materialList = GetMaterials(currentSlotname);
            if (materialList != null)
            {
                conRect.height += (itemHeight + margin) * materialList.Count + margin;

                scrollViewVector = GUI.BeginScrollView(scrollRect, scrollViewVector, conRect);

                foreach (Material material in materialList)
                {
                    GUI.Label(outRect, material.name, lStyle);
                    outRect.y += itemHeight + margin;
                    Color sColor = material.color;
                    sColor.r = drawModValueSlider(outRect, sColor.r, 0f, 2f, String.Format("{0}:{1:F2}", "R", sColor.r), lStyle);
                    outRect.y += itemHeight + margin;
                    sColor.g = drawModValueSlider(outRect, sColor.g, 0f, 2f, String.Format("{0}:{1:F2}", "G", sColor.g), lStyle);
                    outRect.y += itemHeight + margin;
                    sColor.b = drawModValueSlider(outRect, sColor.b, 0f, 2f, String.Format("{0}:{1:F2}", "B", sColor.b), lStyle);
                    outRect.y += itemHeight + margin;
                    sColor.a = drawModValueSlider(outRect, sColor.a, 0f, 1f, String.Format("{0}:{1:F2}", "A", sColor.a), lStyle);
                    outRect.y += itemHeight + margin;

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
                    material.color = sColor;

                    outRect.y += margin * 2;
                }
                GUI.EndScrollView();
            }
            outRect.y += itemHeight + margin * 2;

            if (GUI.Button(outRect, "閉じる", bStyle))
            {
                menuType = MenuType.Main;
            }
            GUI.DragWindow();
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


    }
}