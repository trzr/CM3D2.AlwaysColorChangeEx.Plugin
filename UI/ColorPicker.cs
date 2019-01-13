using System;
using System.Text;
using CM3D2.AlwaysColorChangeEx.Plugin.Util;
using UnityEngine;

namespace CM3D2.AlwaysColorChangeEx.Plugin.UI {

    public class ColorPicker {
        #region Fields/Properties
        private readonly ColorPresetManager _presetMgr;

        public bool expand;
        // 色テクスチャの縁サイズ
        public int texEdgeSize = 0;

        private int _selectedPreset = -1;
        private Vector2 _pos;
        private bool _mapDragging;
        private bool _lightDragging;

        private Color _color;
        public Color Color {
            set {
                if (_color != value) {
                    _color = value;
                    Light = Math.Max(_color.r, Math.Max(_color.g, _color.b));
                    SearchPos(MapTex, ref _color, out _pos);
                    SetTexColor(ref _color);
                    ToColorCode();
                }
            }
            get { return _color; }
        }

        /// <summary>輝度(0-1)</summary>
        private float _light;
        public float Light {
            set {
                if (!Equals(_light, value)) {
                    Transfer(MapBaseTex, MapTex, value);
                    _light = value;
                }
            }
            get { return _light; }
        }

        private void SearchPos(Texture2D tex, ref Color col, out Vector2 destPos) {
            var min = 3f;
            var minx = 0;
            var miny = 0;
            for (var x = 0; x < tex.width; x++) {
                for (var y = 0; y < tex.height; y++) {
                    var dist = DiffColor(tex.GetPixel(x, y), col);
                    if (dist < 0.001f) {
                        destPos.x = x;
                        destPos.y = tex.height - 1 - y;
                        return;
                    }
                    if (dist < min) {
                        min = dist;
                        minx = x;
                        miny = y;
                    }
                }
            }
            destPos.x = minx;
            destPos.y = tex.height - 1 - miny;
        }

        private void Transfer(Texture2D srcTex, Texture2D dstTex, float ratio) {
            var src = srcTex.GetPixels32(0);
            var dst = dstTex.GetPixels32(0);
            var maxIndex = dstTex.width * dstTex.height;
            for (var i = 0; i < maxIndex; i++) {
                dst[i].r = (byte)(src[i].r * ratio);
                dst[i].g = (byte)(src[i].g * ratio);
                dst[i].b = (byte)(src[i].b * ratio);
                dst[i].a = src[i].a;
            }
            dstTex.SetPixels32(dst, 0);
            dstTex.Apply();
        }

        // 色テクスチャ(透過を含まないRGBのみの色を表示)
        private Texture2D _colorTex;
        public Texture2D ColorTex {
            set { _colorTex = value; }
            get {
                if (_colorTex == null) {
                    _colorTex = new Texture2D(16, 16, TextureFormat.RGB24, false);
                }
                return _colorTex;
            }
        }

        private Texture2D _mapTex;
        public Texture2D MapTex {
            get {
                if (_mapTex == null) {
                    var baseTex = MapBaseTex;
                    _mapTex = new Texture2D(baseTex.width, baseTex.height, baseTex.format, false);
                    Transfer(MapBaseTex, _mapTex, _light);
                }
                return _mapTex;
            }
        }

        private GUILayoutOption _iconWidth;
        public GUILayoutOption IconWidth {
            get { return _iconWidth ?? (_iconWidth = GUILayout.Width(ColorTex.width)); }
        }

        private Color GetMapColor(int x, int y) {
            return MapTex.GetPixel(x, MapTex.height - y);
        }
        
        private GUIStyle _texStyle;
        public GUIStyle TexStyle {
            set { _texStyle = value; }
            get { return _texStyle ?? (_texStyle = new GUIStyle {normal = {background = MapTex}}); }
        }
        private GUIStyle _texLightStyle;
        public GUIStyle TexLightStyle {
            set { _texLightStyle = value; }
            get { return _texLightStyle ?? (_texLightStyle = new GUIStyle {normal = {background = LightTex}}); }
        }

        private readonly StringBuilder _colorCode = new StringBuilder(7);
        public string ColorCode { get; private set; }

        public bool SetColorCode(string code) {
            if (!IsColorCode(code)) return false;

            var r = Uri.FromHex(code[1]) * 16 + Uri.FromHex(code[2]);
            var g = Uri.FromHex(code[3]) * 16 + Uri.FromHex(code[4]);
            var b = Uri.FromHex(code[5]) * 16 + Uri.FromHex(code[6]);
            Color = new Color(r/255f, g/255f, b/255f, _color.a);

            return true;
        }
        #endregion

        #region Static Fields/Properties
        private static readonly GUIStyle labelStyle = new GUIStyle("label");
        private static readonly GUILayoutOption labelWidth = GUILayout.Width(16f);
        private static readonly Color Empty = Color.clear;

        private const int FRAME_WIDTH = 1;
        private const float RANGE_UNIT = 3f / Mathf.PI;
        public static int size = 256;

        private static Texture2D mapBaseTex;
        public static Texture2D MapBaseTex {
            set {
                if (value != null) {
                    mapBaseTex = value;
                    size = Math.Min(mapBaseTex.width, mapBaseTex.height);
                }
            }
            get {
                if (mapBaseTex == null) {
                    mapBaseTex = CreateRGBMapTex(size+1, size+1);
                }
                return mapBaseTex;
            }
        }

        private static Texture2D lightTex;
        public static Texture2D LightTex {
            get {
                if (lightTex == null) {
                    lightTex = CreateLightTex(16, size, FRAME_WIDTH);
                }
                return lightTex;
            }
        }

        private static Texture2D circleTex;
        public static Texture2D CircleTex {
            get {
                if (circleTex == null) {
                    circleTex = ResourceHolder.Instance.LoadTex("circle");
                }
                return circleTex;
            }
        }
        private static Texture2D crossTex;
        public static Texture2D CrossTex {
            get {
                if (crossTex == null) {
                    crossTex = ResourceHolder.Instance.LoadTex("cross");
                }
                return crossTex;
            }
        }

        public static bool HasColorCodeClip() {
            var clip = ClipBoardHandler.Instance.GetClipboard();
            return IsColorCode(clip);
        }

        public static bool IsColorCode(string code) {
            if (code.Length == 7 && code[0] == '#') {
                for (var i = 1; i < 7; i++) {
                    if (!Uri.IsHexDigit(code[i])) return false;
                }
                return true;
            }
            return false;
        }

        public static Color GetColor(string code) {
            if (!IsColorCode(code)) return Empty;

            var r = Uri.FromHex(code[1]) * 16 + Uri.FromHex(code[2]);
            var g = Uri.FromHex(code[3]) * 16 + Uri.FromHex(code[4]);
            var b = Uri.FromHex(code[5]) * 16 + Uri.FromHex(code[6]);
            return new Color(r/255f, g/255f, b/255f);
        }

        #endregion

        public ColorPicker(ColorPresetManager presetMgr=null) {
            ColorCode = string.Empty;
            this._presetMgr = presetMgr;
        }

        private void ToColorCode() {
            var r = (int)(_color.r * 255);
            var g = (int)(_color.g * 255);
            var b = (int)(_color.b * 255);

            _colorCode.Length = 0;
            _colorCode.Append('#')
                .Append(r.ToString("X2")) // Uppercase:X2
                .Append(g.ToString("X2"))
                .Append(b.ToString("X2"));
            ColorCode = _colorCode.ToString();
        }

        public void SetTexColor(ref Color col) {
            SetTexColor(ref col, texEdgeSize);
        }

        public void SetTexColor(ref Color col, int frame) {
            var tex = (_colorTex == null) ? ColorTex : _colorTex;
            var blockWidth  = tex.width - frame * 2;
            var blockHeight = tex.height - frame * 2;
            var pixels = tex.GetPixels(frame, frame, blockWidth, blockHeight, 0);
            for (var i = 0; i< pixels.Length; i++) {
                pixels[i] = col;
            }
            tex.SetPixels(frame, frame, blockWidth, blockHeight, pixels);
            tex.Apply();
        }

        public bool DrawLayout() {
            GUILayout.BeginHorizontal();
            try {
                if (GUILayout.Button(CrossTex, labelStyle, labelWidth)) expand = false;

                GUILayout.Label(string.Empty, TexStyle, GUILayout.Width(_mapTex.width), GUILayout.Height(_mapTex.height));
                var lastRect = GUILayoutUtility.GetLastRect();
                var tex = CircleTex;
                var offset = tex.width * 0.5f;
                GUI.DrawTexture(new Rect(lastRect.x + _pos.x - offset, lastRect.y + _pos.y - offset, tex.width, tex.height), tex, ScaleMode.StretchToFill, true, 0f);
                var changed = MapPickerEvent(ref lastRect);

                GUILayout.Space(20f);
                GUILayout.Label(string.Empty, TexLightStyle, GUILayout.Width(lightTex.width), GUILayout.Height(lightTex.height));
                lastRect = GUILayoutUtility.GetLastRect();
                GUI.DrawTexture(new Rect(lastRect.x+FRAME_WIDTH, lastRect.y + (size-1)*(1-_light) - offset+FRAME_WIDTH, tex.width, tex.height), tex, ScaleMode.StretchToFill, true, 0f);

                changed |= LightSliderEvent(ref lastRect);

                if (_presetMgr != null) {
                    GUILayout.Space(5f);
                    changed |= DrawPresetLayout();
                }
                return changed;
            } finally {
                GUILayout.EndHorizontal();
            }
        }

        public bool DrawPresetLayout() {
            var changed = false;
            var e = Event.current;
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            try {
                var startIdx = 0;
                while (startIdx < _presetMgr.Count) {
                    GUILayout.BeginVertical();
                    try {
                        var endIdx = (startIdx + 10 < _presetMgr.Count) ? startIdx + 10 : _presetMgr.Count;
                        for (var i = startIdx; i < endIdx; i++) {
                            var presetIcon = _presetMgr.presetIcons[i];
                            GUILayout.Label(presetIcon, _presetMgr.IconStyle);

                            if (_selectedPreset == i) {
                                var lastRect = GUILayoutUtility.GetLastRect();
                                var tex = ColorPresetManager.PresetFocusIcon;
                                GUI.DrawTexture(new Rect(lastRect.x + 1, lastRect.y + 2, tex.width, tex.height), tex, ScaleMode.StretchToFill, true, 0f);
                            }

                            if (e.type == EventType.MouseDown && (e.button == 0 || e.button == 1)) {
                                var mousePos = e.mousePosition;
                                if (GUILayoutUtility.GetLastRect().Contains(mousePos)) {
                                    switch (e.button) {
                                    case 1: // right click
                                        _presetMgr.ClearColor(_selectedPreset);
                                        _selectedPreset = i;
                                        break;
                                    case 0: // left click
                                        if (_selectedPreset == i) {
                                            var code = _presetMgr.presetCodes[i];
                                            SetColorCode(code);
                                            changed = true;
                                        } else {
                                            _selectedPreset = i;
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                    } finally {
                        GUILayout.EndVertical();
                    }
                    startIdx += 10;
                }
            } finally {
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            try {
                GUI.enabled = _selectedPreset != -1;
                try {
                    if (GUILayout.Button("Save", _presetMgr.BtnStyle, _presetMgr.BtnWidth)) {
                        _presetMgr.SetColor(_selectedPreset, ColorCode, ref _color);
                    }

                    if (GUILayout.Button("Delete", _presetMgr.BtnStyle, _presetMgr.BtnWidth)) {
                        _presetMgr.ClearColor(_selectedPreset);
                    }
                } finally {
                    GUI.enabled = true;
                }
            } finally {
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            return changed;
        }

        public bool DrawPicker(ref Rect rect) {
            GUI.Label(rect, string.Empty, TexStyle);
            var tex = CircleTex;
            var offset = tex.width * 0.5f;
            GUI.DrawTexture(new Rect(rect.x + _pos.x-offset, rect.y + _pos.y-offset, tex.width, tex.height), tex, ScaleMode.StretchToFill, true, 0f);

            return MapPickerEvent(ref rect);
        }

        private bool MapPickerEvent(ref Rect rect) {
            if (_lightDragging) return false;

            var e = Event.current;
            if (e.button == 0 && (e.type == EventType.MouseDown || e.type == EventType.MouseDrag)) {

                var mousePos = e.mousePosition;
                if (_mapDragging || rect.Contains(mousePos)) {
                    var x = (int) (mousePos.x - rect.x);
                    var y = (int) (mousePos.y - rect.y);

                    Color col;
                    if (_mapDragging) {
                        var centerX = _mapTex.width / 2;
                        var centerY = Mathf.CeilToInt(_mapTex.height/ 2f); // Y軸反転のため、奇数の場合は切り上げ
                        var radius = Math.Min(centerX, centerY);
                        var dist = Distance(x, y, centerX, centerY);
                        if (dist <= radius) {
                            col = GetMapColor(x, y);
                        } else {
                            // ドラッグ時は範囲を逸脱しても、角度を元に外周色に設定
                            col = GetEdgeColor(x - centerX, -(y - centerY), dist) * _light;
                            col.a = 1f;
                            var mul = radius / dist;
                            // 位置も外周に合わせて補正
                            x = (int)((x-centerX) * mul) + centerX;
                            y = (int)((y-centerY) * mul) + centerY;
                        }

                    } else if (e.type == EventType.MouseDown) {
                        col = GetMapColor(x, y);
                        // 透過色の場合は無視
                        if (Equals(col.a, 0f)) return false;
                        _mapDragging = true;

                    } else {
                        return false;
                    }
                    _color = col;
                    SetTexColor(ref _color, texEdgeSize);
                    ToColorCode();
                    _pos.x = x;
                    _pos.y = y;
                    e.Use();
                    return true;
                }
            } else if (_mapDragging && e.type == EventType.MouseUp) {
                _mapDragging = false;
            }
            return false;
        }

        public void DrawLightScale(ref Rect rect) {
            GUI.Label(rect, string.Empty, TexLightStyle);
            var tex = CircleTex;
            var offset = tex.width * 0.5f;
            var circlePos = new Rect(rect.x+FRAME_WIDTH, rect.y + (size-1)*(1-_light)-offset+FRAME_WIDTH, tex.width, tex.height);
            GUI.DrawTexture(circlePos, tex, ScaleMode.StretchToFill, true, 0f);

            LightSliderEvent(ref rect);
        }

        private bool LightSliderEvent(ref Rect rect) {
            if (_mapDragging) return false;

            var e = Event.current;
            if (e.button == 0 && (e.type == EventType.MouseDown || e.type == EventType.MouseDrag)) {

                var mousePos = e.mousePosition;
                if (e.type == EventType.MouseDown && rect.Contains(mousePos)) _lightDragging = true;
                if (_lightDragging) {
                    var light1 = 1f - (mousePos.y - rect.y - 1)/size;
                    if (1f < light1) light1 = 1f;
                    else if (light1 < 0f) light1 = 0f;

                    if (!Equals(_light, light1)) {
                        Light = light1;

                        _color = GetMapColor((int)_pos.x, (int)_pos.y);
                        SetTexColor(ref _color, texEdgeSize);
                        ToColorCode();
                        e.Use();
                        return true;
                    }
                    e.Use();
                }
            } else if (_lightDragging && e.type == EventType.MouseUp) {
                _lightDragging = false;
            }

            return false;
        }

        private static Texture2D CreateLightTex(int width, int height, int frameWidth) {
            var tex = new Texture2D(width+frameWidth*2, height+frameWidth*2, TextureFormat.RGB24, false);
            var denom = height - 1;
            var frameCol = Color.gray;
            for (var y = frameWidth; y < height+frameWidth; y++) {
                var r = 1 - (float)y / denom;
                var col = new Color(r, r, r);
                for (var x = frameWidth; x < width+frameWidth; x++) {
                    tex.SetPixel(x, height - 1 - y, col);
                }
                // フレーム(左右)
                for (var x = 0; x < frameWidth; x++) {
                    tex.SetPixel(x, height - 1 - y, frameCol);
                }
                for (var x = width+frameWidth; x < frameWidth+frameWidth*2; x++) {
                    tex.SetPixel(x, height - 1 - y, frameCol);
                }
            }
            // フレーム(上下)
            for (var x = 0; x < width + frameWidth * 2; x++) {
                for (var y = 0; y < frameWidth; y++) {
                    tex.SetPixel(x, y, frameCol);
                }
                for (var y = height + frameWidth; y < height + frameWidth * 2; y++) {
                    tex.SetPixel(x, y, frameCol);
                }
            }

            tex.Apply();
            return tex;
        }

        /// <summary>
        /// 指定された幅と高さのRGBマップ（円型）を生成する.
        /// </summary>
        /// <param name="width">画像幅</param>
        /// <param name="height">画像高さ</param>
        /// <returns></returns>
        private static Texture2D CreateRGBMapTex(int width, int height) {
            var tex = new Texture2D(width, height, TextureFormat.ARGB32, false);
            var centerX = width/2;
            var centerY = height/2; // Mathf.FloorToInt(height/2f);
            var radius = Math.Min(centerX, centerY);
            var centerCol = Color.white;
            for (var y = 0; y < height; y++) {
                for (var x = 0; x < width; x++) {
                    var dist = Distance(x, y, centerX, centerY);
                    var distRatio = dist/radius;
                    if (1f < distRatio) {
                        tex.SetPixel(x, y, Empty);
                    } else if (Equals(distRatio, 0f)) {
                        tex.SetPixel(x, y, centerCol);
                    } else {
                        var vecX = x - centerX;
                        var vecY = y - centerY;
                        var edgeCol = GetEdgeColor(vecX, vecY, dist);

                        var color = GetColor(ref centerCol, ref edgeCol, distRatio);
                        tex.SetPixel(x, y, color);
                    }
                }
            }

            return tex;
        }

        // 外周の色を取得する.
        private static Color GetEdgeColor(float vecX, float vecY, float dist) {
            var theta = (float)Math.Acos(vecX / dist);
            if (vecY > 0) theta = -theta;

            theta *= RANGE_UNIT;
            if (-0.5f <= theta && theta < 0.5f) {
                var rotRatio = 0.5f - theta;
                return new Color(rotRatio, 0f, 1f);
            } else if (0.5f <= theta && theta < 1.5f) {
                var rotRatio = theta - 0.5f;
                return new Color(0f, rotRatio, 1f);
            } else if (1.5f <= theta && theta < 2.5f) {
                var rotRatio = 2.5f - theta;
                return new Color(0f, 1f, rotRatio);
            } else if (2.5f <= theta && theta <= 3f) {
                var rotRatio = theta - 2.5f;
                return new Color(rotRatio, 1f, 0f);
            } else if (-3f <= theta && theta < - 2.5f) {
                var rotRatio = theta + 3.5f;
                return new Color(rotRatio, 1f, 0f);
            } else if (-2.5f <= theta && theta < -1.5f) {
                var rotRatio = -1.5f - theta;
                return new Color(1f, rotRatio, 0f);
            } else {//if (-1.5f <= theta && theta < -0.5f) {
                var rotRatio = theta + 1.5f;
                return new Color(1f, 0f, rotRatio);
            }
        }

        private static float Distance(int x1, int y1, int x2, int y2) {
            var dX = x1 - x2;
            var dY = y1 - y2;
            return (float)Math.Sqrt(dX * dX + dY * dY);
        }

        private static bool Equals(float f1, float f2) {
            return Math.Abs(f1 - f2) < 0.001f;
        }

        /// <summary>2色と比率から、２色間の比率に合わせた割合の色を抽出する</summary>
        /// <param name="c1">色1</param>
        /// <param name="c2">色2</param>
        /// <param name="ratio">割合(0-1)</param>
        /// <returns>色</returns>
        private static Color GetColor(ref Color c1, ref Color c2, float ratio) {
            var r = c1.r + ratio * (c2.r - c1.r);
            var g = c1.g + ratio * (c2.g - c1.g);
            var b = c1.b + ratio * (c2.b - c1.b);
            return new Color(r, g, b);
        }
 
        private static float DiffColor(Color c1, Color c2) {
            return Math.Abs(c1.r - c2.r) + Math.Abs(c1.g - c2.g) + Math.Abs(c1.b - c2.b);
        }
    }
}
