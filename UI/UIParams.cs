using CM3D2.AlwaysColorChangeEx.Plugin.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CM3D2.AlwaysColorChangeEx.Plugin.UI {
    public class UIParams {
        private static readonly UIParams INSTANCE = new UIParams();
        public static UIParams Instance {
            get { return INSTANCE; }
        }
        #region Constants
        private const int marginPx = 2;
        private const int marginLPx = 10;
        private const int itemHeightPx = 18;
        private const int fontPx = 14;
        private const int fontPxS = (int)(fontPx * 0.9f);
        private const int fontPxSS = (int)(fontPx * 0.8f);
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
        public readonly GUIStyle lStyle = new GUIStyle("label");
        // bold
        public readonly GUIStyle lStyleB = new GUIStyle("label");
        // colored
        public readonly GUIStyle lStyleC = new GUIStyle("label");
        // small
        public readonly GUIStyle lStyleS = new GUIStyle("label");
        public readonly GUIStyle lStyleRS = new GUIStyle("label");

        public readonly GUIStyle bStyle = new GUIStyle("button");
        public readonly GUIStyle bStyleSC = new GUIStyle("button");
        public readonly GUIStyle bStyleL = new GUIStyle("button");

        public readonly GUIStyle tStyle = new GUIStyle("toggle");
        public readonly GUIStyle tStyleS = new GUIStyle("toggle");
        public readonly GUIStyle tStyleSS = new GUIStyle("toggle");
        public readonly GUIStyle tStyleL = new GUIStyle("toggle");
        public readonly GUIStyle listStyle = new GUIStyle();
        public readonly GUIStyle textStyle = new GUIStyle("textField");
        public readonly GUIStyle textStyleSC = new GUIStyle("textField");
        public readonly GUIStyle textAreaStyleS = new GUIStyle("textArea");

        public readonly GUIStyle boxStyle = new GUIStyle("box");
        public readonly GUIStyle winStyle = new GUIStyle("box");
        public readonly GUIStyle dialogStyle = new GUIStyle("box");
        public readonly GUIStyle tipsStyle = new GUIStyle("window");

        public readonly Color textColor = new Color(1f, 1f, 1f, 0.98f);

        public Rect titleBarRect = new Rect();

        public Rect winRect = new Rect();
        public Rect fileBrowserRect = new Rect();
        public Rect modalRect = new Rect();

        public Rect mainRect = new Rect();
        public Rect colorRect = new Rect();
        public Rect nodeSelectRect = new Rect();
        public Rect presetSelectRect = new Rect();
        public Rect textureRect = new Rect();
        public Rect labelRect = new Rect();
        public Rect subRect = new Rect();
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
            lStyleB.fontStyle = FontStyle.Bold;
            lStyleB.alignment = txtAlignment;

            lStyle.fontStyle = FontStyle.Normal;
            lStyle.normal.textColor = textColor;
            lStyle.alignment = txtAlignment;
            lStyle.wordWrap = false;
            //lStyle.wordWrap = false;

            lStyleS.fontStyle = FontStyle.Normal;
            lStyleS.normal.textColor = textColor;
            lStyleS.alignment = txtAlignment;

            lStyleRS.fontStyle = FontStyle.Normal;
            lStyleRS.normal.textColor = textColor;
            lStyleRS.alignment = TextAnchor.MiddleRight;

            lStyleC.fontStyle = FontStyle.Normal;
            lStyleC.normal.textColor = new Color(0.82f, 0.88f, 1f, 0.98f);
            lStyleC.alignment = txtAlignment;

            bStyle.normal.textColor = textColor;
            bStyleSC.normal.textColor = textColor;
            bStyleSC.alignment = TextAnchor.MiddleCenter;
            bStyleL.normal.textColor = textColor;
            bStyleL.alignment = TextAnchor.MiddleLeft;

            tStyle.normal.textColor = textColor;
            tStyleS.normal.textColor = textColor;
            tStyleS.alignment = TextAnchor.LowerLeft;
            tStyleSS.normal.textColor = textColor;
            tStyleSS.alignment = TextAnchor.MiddleLeft;

            tStyleL.normal.textColor = textColor;
            tStyleL.alignment = txtAlignment;

            textStyle.normal.textColor = textColor;
            textStyleSC.normal.textColor = textColor;
            textStyleSC.alignment = TextAnchor.MiddleCenter;
            textAreaStyleS.normal.textColor = textColor;

            winStyle.alignment = TextAnchor.UpperRight;
            dialogStyle.alignment = TextAnchor.UpperCenter;
            dialogStyle.normal.textColor = textColor;
            tipsStyle.alignment = TextAnchor.MiddleCenter;
            tipsStyle.wordWrap = true;
        }

        public void Update() {
            var screenSizeChanged = false;

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
            fontSize = FixPx(fontPx);
            fontSizeS = FixPx(fontPxS);
            fontSizeSS = FixPx(fontPxSS);
            fontSizeL = FixPx(fontPxL);
            margin = FixPx(marginPx);
            marginL = FixPx(marginLPx);
            itemHeight = FixPx(itemHeightPx);
            unitHeight = margin + itemHeight;

            lStyle.fontSize = fontSize;
            lStyleC.fontSize = fontSize;
            lStyleB.fontSize = fontSize;

            lStyleS.fontSize = fontSizeS;
            lStyleRS.fontSize = fontSizeS;

            bStyle.fontSize = fontSize;
            bStyleSC.fontSize = fontSizeS;
            bStyleL.fontSize = fontSize;
            tStyle.fontSize = fontSize;
            tStyleS.fontSize = fontSizeS;
            tStyleSS.fontSize = fontSizeSS;
            tStyleL.fontSize = fontSizeL;
            listStyle.fontSize = fontSizeS;
            textStyle.fontSize = fontSize;
            textStyleSC.fontSize = fontSizeS;
            textAreaStyleS.fontSize = fontSizeS;

            LogUtil.DebugF("screen=({0},{1}),margin={2},height={3},ratio={4})", width, height, margin, itemHeight, ratio);

            winStyle.fontSize = fontSize;
            tipsStyle.fontSize = fontSize;
            dialogStyle.fontSize = fontSize;
            InitWinRect();
            InitFBRect();
            InitModalRect();

            subConWidth = winRect.width - margin * 2;
            optBtnHeight = GUILayout.Height(itemHeight);
            // sub
            optSubConWidth = GUILayout.Width(subConWidth);
            optSubConHeight = GUILayout.Height(winRect.height - unitHeight * 3f);
            optSubCon6Height = GUILayout.Height(winRect.height - unitHeight * 6.6f);
            optSubConHalfWidth = GUILayout.Width((winRect.width - marginL * 2) * 0.5f); // margin値が小さい前提になってしまっている
            optSLabelWidth = GUILayout.Width(fontSizeS * 6f);

            mainRect.Set(margin, unitHeight * 5 + margin, winRect.width - margin * 2, winRect.height - unitHeight * 6.5f);
            textureRect.Set(margin, unitHeight, winRect.width - margin * 2, winRect.height - unitHeight * 2.5f);
            var baseWidth = subConWidth - 20;
            optBtnWidth = GUILayout.Width(baseWidth * 0.09f);
            optDBtnWidth = GUILayout.Width(fontSizeS * 5f * 0.6f);
            optContentWidth = GUILayout.MaxWidth(baseWidth * 0.69f);
            optCategoryWidth = GUILayout.MaxWidth(fontSize * 12f * 0.47f);

            nodeSelectRect.Set(margin, unitHeight * 2, winRect.width - margin * 2, winRect.height - unitHeight * 4.5f);
            colorRect.Set(margin, unitHeight * 2, winRect.width - margin, winRect.height - unitHeight * 5);
            labelRect.Set(0, 0, winRect.width - margin * 2, itemHeight * 1.2f);
            subRect.Set(0, itemHeight, winRect.width - margin * 2, itemHeight);

            foreach (var func in updaters) {
                func(this);
            }
        }

        public void InitWinRect() {
            winRect.Set(width - FixPx(310), FixPx(48), FixPx(300), height - FixPx(150));
            titleBarRect.Set(0, 0, winRect.width, 24f);
        }

        public void InitFBRect() {
            fileBrowserRect.Set(width - FixPx(620), FixPx(100), FixPx(600), FixPx(600));
        }

        public void InitModalRect() {
            modalRect.Set(width / 2 - FixPx(300), height / 2 - FixPx(300), FixPx(600), FixPx(600));
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
}
