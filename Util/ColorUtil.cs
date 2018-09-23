using System;
using UnityEngine;

namespace CM3D2.AlwaysColorChangeEx.Plugin.Util {

    public static class ColorUtil {
        private const float EPSILON = 0.001f;

        public static bool Equals(float f1, float f2) {
            return Mathf.Abs(f1 - f2) < EPSILON;
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

        /// <summary>
        /// RGB -> HSL変換
        /// Vector4:(H, S, L, Alpha)
        /// </summary>
        /// <param name="c">カラー</param>
        /// <returns>HSL+alphaの4値ベクター</returns>
        public static Vector4 RGB2HSL(ref Color c) {
            var r = Mathf.Clamp01(c.r);
            var g = Mathf.Clamp01(c.g);
            var b = Mathf.Clamp01(c.b);

            var max = Mathf.Max(r, Mathf.Max(g, b));
            var min = Mathf.Min(r, Mathf.Min(g, b));

            var h = 0f;
            var s = 0f;
            var l = (max + min) * 0.5f;
            var cnt = max - min; // 収束値CNT

            if (Equals(cnt, 0f)) return new Vector4(h, s, l, c.a);

            s = l > 0.5f ? (cnt / (2f - max - min)) : (cnt / (max + min));
            if (Equals(max, r)) {
                h = (g - b) / cnt + (g < b ? 6f : 0f);
            } else if (Equals(max, g)) {
                h = (b - r) / cnt + 2f;
            } else {
                h = (r - g) / cnt + 4f;
            }

            h /= 6f;
            return new Vector4(h, s, l, c.a);
        }

        // HSL -> RGB 変換
        public static Color HSL2RGB(float h, float s, float l, float a) {
            Color c;
            c.a = a;

            if (Equals(s, 0f)) {
                c.r = l;
                c.g = l;
                c.b = l;
            } else {
                var y = (l < 0.5f) ? (l * (1f + s)) : ((l + s) - l * s);
                var x = 2f * l - y;
                c.r = Hue(x, y, h + 1f / 3f);
                c.g = Hue(x, y, h);
                c.b = Hue(x, y, h - 1f / 3f);
            }

            return c;
        }

        public static Color HSL2RGB(ref Vector4 hsl) {
            return HSL2RGB(hsl.x, hsl.y, hsl.z, hsl.w);
        }

        public static Vector3 RGB2HSV(ref Color c) {
            var r = Mathf.Clamp01(c.r);
            var g = Mathf.Clamp01(c.g);
            var b = Mathf.Clamp01(c.b);

            var max = Mathf.Max(r, Mathf.Max(g, b));
            var min = Mathf.Min(r, Mathf.Min(g, b));

            var d = max - min;
            float s;
            if (Equals(max, 0f)) {
                s = 0f;
            } else {
                s = d / max;
            }

            float h;
            if (Equals(max, r)) {
                h = (b - g) / d;
            } else if (Equals(max, g)) {
                h = (b - r) / d + 2f;
            } else {
                h = (r - g) / d + 4f;
            }

            h /= 6f;
            if (h < 0f) {
                h += 1f;
            }

            var v = max;
            return new Vector3(h, s, v);
        }

        /// <summary>
        /// HSVからRGBに変換する.
        /// </summary>
        /// <param name="h">色相 H:[0,1]</param>
        /// <param name="s">彩度 S:[0,1]</param>
        /// <param name="v">明度 V:[0,1]</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static Color HSV2RGB(float h, float s, float v) {
            if (Equals(s, 0f)) {
                return new Color(v, v, v);
            }

            if (Equals(v, 0f)) {
                return Color.black;
            }

            var h0 = h * 6f;
            var hi = (int) Mathf.Floor(h0);
            var r = h0 - hi;
            var m = v * (1f - s);
            var n = v * (1f - s * r);
            var k = v * (1f - s * (1f - r));
            switch (hi) {
            case 0:
            case 6:
                return new Color(v, k, m);
            case 1:
            case 7:
                return new Color(n, v, m);
            case 2:
                return new Color(m, v, k);
            case 3:
                return new Color(m, n, v);
            case 4:
                return new Color(k, m, v);
            case 5:
                return new Color(v, m, n);
            }

            throw new ArgumentException("failed to convert Color(HSV to RGB)");
        }

        private static float Hue(float x, float y, float t) {
            if (t < 0f) {
                t += 1f;
            } else if (t > 1f) {
                t -= 1f;
            }

            if (t < 1f / 6f) return x + (y - x) * 6f * t;
            if (t < 3f / 6f) return y;
            if (t < 4f / 6f) return x + (y - x) * 6f * (4f / 6f - t);
            return x;
        }
    }
}