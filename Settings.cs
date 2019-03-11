using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CM3D2.AlwaysColorChangeEx.Plugin.Util;
using UnityEngine;

namespace CM3D2.AlwaysColorChangeEx.Plugin {
    public sealed class Settings {
        public static readonly Settings Instance = new Settings();

        public KeyCode  toggleKey = KeyCode.F12;
        public EventModifiers toggleModifiers = EventModifiers.None;
        public HashSet<KeyCode> toggleKeyModifier = null;
        public KeyCode prevKey = KeyCode.Mouse3;
        public KeyCode nextKey = KeyCode.Mouse4;
        public string presetPath;
        public string presetDirPath;
        public float shininessMax    =  20f;
        public float shininessMin    =   0f;
        public float outlineWidthMax =   0.1f;
        public float outlineWidthMin =   0f;
        public float rimPowerMax     = 200f;
        public float rimPowerMin     =-200f;
        public float rimShiftMax     =   1f;
        public float rimShiftMin     =   0f;
        public float hiRateMax       =   1f;
        public float hiRateMin       =   0f;
        public float hiPowMax        =  50f;
        public float hiPowMin        =   0.001f;
        public float floatVal1Max    = 300f;
        public float floatVal1Min    =   0f;
        public float floatVal2Max    =  15f;
        public float floatVal2Min    = -15f;// -20
        public float floatVal3Max    =   1f;
        public float floatVal3Min    =   0f;

        public float shininessEditMax    =  10000f;
        public float shininessEditMin    = -10000f;
        public float outlineWidthEditMax =   1f;
        public float outlineWidthEditMin =   0f;
        public float rimPowerEditMax     = 10000f;
        public float rimPowerEditMin     =-10000f;
        public float rimShiftEditMax     =   5f;
        public float rimShiftEditMin     =   0f;
        public float hiRateEditMax       = 100f;
        public float hiRateEditMin       =   0f;
        public float hiPowEditMax        = 100f;
        public float hiPowEditMin        =   0.00001f;
        public float floatVal1EditMax    = 500f;
        public float floatVal1EditMin    =   0f;
        public float floatVal2EditMax    =  50f;
        public float floatVal2EditMin    = -50f;
        public float floatVal3EditMax    =  50f;
        public float floatVal3EditMin    =   0f;

        public string shininessFmt    = "F2";
        public string outlineWidthFmt = "F5";
        public string rimPowerFmt     = "F3";
        public string rimShiftFmt     = "F5";
        public string hiRateFmt       = "F2";
        public string hiPowFmt        = "F5";
        public string floatVal1Fmt    = "F2";
        public string floatVal2Fmt    = "F3";
        public string floatVal3Fmt    = "F3";

        public float[] shininessRange() {
            return new[] {shininessMin, shininessMax,};
        }
        public float[] outlineWidthRange() {
            return new[] {outlineWidthMin, outlineWidthMax, };
        }
        public float[] rimPowerRange() {
            return new[] {rimPowerMin, rimPowerMax, };
        }
        public float[] rimShiftRange() {
            return new[] {rimShiftMin, rimShiftMax, };
        }
        public float[] hiRateRange() {
            return new[] {hiRateMin, hiRateMax, };
        }
        public float[] hiPowRange() {
            return new[] {hiPowMin, hiPowMax, };
        }
        public float[] floatVal1Range() {
            return new[] {floatVal1Min, floatVal1Max, };
        }
        public float[] floatVal2Range() {
            return new[] {floatVal2Min, floatVal2Max, };
        }
        public float[] floatVal3Range() {
            return new[] {floatVal3Min, floatVal3Max, };
        }
        
        public string fmtColor = "F3";
        
        public string menuPrefix     = "";
        public string iconSuffix     = "_i_";
        public string resSuffix      = "_mekure_";
        public string txtPrefixMenu  = "Assets/menu/menu/";
        public string txtPrefixTex   = "Assets/texture/texture/";
        public string[] toonTexes = {
            "noTex",
            "toonBlueA1",   "toonBlueA2",   "toonBrownA1",
            "toonGrayA1",
            "toonGreenA1",  "toonGreenA2",  "toonGreenA3",
            "toonOrangeA1",
            "toonPinkA1",   "toonPinkA2",   "toonPurpleA1",
            "toonRedA1",    "toonRedA2",
            "toonRedmmm1","toonRedmm1","toonRedm1",
            "toonYellowA1", "toonYellowA2", "toonYellowA3", "toonYellowA4",
#if COM3D2
            "toonFace",
            "toonSkin",
#else
            "toonFace",     "toonFace002",
            "toonSkin",     "toonSkin002",
#endif
            "toonBlackA1",
            "toonFace_shadow",
            "toonDress_shadow",
            "toonSkin_Shadow",
            "toonBlackmm1","toonBlackm1","toonGraymm1","toonGraym1",
            "toonPurplemm1","toonPurplem1",
            "toonSilvera1",
            "toonDressmm_shadow","toonDressm_shadow",
        };
        public string[] toonTexAddon = new string[0];
        public bool toonComboAutoApply = true;
        public bool displaySlotName  = false;
        public bool enableMask = true;
        public bool enableMoza = false;

        public bool SSWithoutUI = false;

        private static readonly int MAX_SCENES = 256;
        public List<int> enableScenes;
        public List<int> disableScenes;
        public List<int> enableOHScenes;
        public List<int> disableOHScenes;

        // 設定の読み込み
        public void Load(Func<string, string> getValue) {
            Get(getValue("PresetPath"),    ref presetPath);
            Get(getValue("PresetDirPath"), ref presetDirPath);
            GetKeyCode(getValue("ToggleWindow"), ref toggleKey);
            string keylist = null;
            if (Get(getValue("ToggleWindowModifier"), ref keylist) && keylist != null) {
                keylist = keylist.ToLower();
                if (keylist.Contains("alt")) {
                    toggleModifiers |= EventModifiers.Alt;
                }
                if (keylist.Contains("control")) {
                    toggleModifiers |= EventModifiers.Control;
                }
                if (keylist.Contains("shift")) {
                    toggleModifiers |= EventModifiers.Shift;
                }
            }
            GetKeyCode(getValue("PrevKey"), ref prevKey);
            GetKeyCode(getValue("NextKey"), ref nextKey);

            Get(getValue("SliderShininessMax"),    ref shininessMax);
            Get(getValue("SliderShininessMin"),    ref shininessMin);
            Get(getValue("SliderOutlineWidthMax"), ref outlineWidthMax);
            Get(getValue("SliderOutlineWidthMin"), ref outlineWidthMin);
            Get(getValue("SliderRimPowerMax"),     ref rimPowerMax);
            Get(getValue("SliderRimPowerMin"),     ref rimPowerMin);
            Get(getValue("SliderRimShiftMax"),     ref rimShiftMax);
            Get(getValue("SliderRimShiftMin"),     ref rimShiftMin);
            Get(getValue("SliderHiRateMax"),       ref hiRateMax);
            Get(getValue("SliderHiRateMin"),       ref hiRateMin);
            Get(getValue("SliderHiPowMax"),        ref hiPowMax);
            Get(getValue("SliderHiPowMin"),        ref hiPowMin);
            Get(getValue("SliderFloatVal1Max"),    ref floatVal1Max);
            Get(getValue("SliderFloatVal1Min"),    ref floatVal1Min);
            Get(getValue("SliderFloatVal2Max"),    ref floatVal2Max);
            Get(getValue("SliderFloatVal2Min"),    ref floatVal2Min);
            Get(getValue("SliderFloatVal3Max"),    ref floatVal3Max);
            Get(getValue("SliderFloatVal3Min"),    ref floatVal3Min);

            Get(getValue("EditShininessMax"),    ref shininessEditMax);
            Get(getValue("EditShininessMin"),    ref shininessEditMin);
            Get(getValue("EditOutlineWidthMax"), ref outlineWidthEditMax);
            Get(getValue("EditOutlineWidthMin"), ref outlineWidthEditMin);
            Get(getValue("EditRimPowerMax"),     ref rimPowerEditMax);
            Get(getValue("EditRimPowerMin"),     ref rimPowerEditMin);
            Get(getValue("EditRimShiftMax"),     ref rimShiftEditMax);
            Get(getValue("EditRimShiftMin"),     ref rimShiftEditMin);
            Get(getValue("EditHiRateMax"),       ref hiRateEditMax);
            Get(getValue("EditHiRateMin"),       ref hiRateEditMin);
            Get(getValue("EditHiPowMax"),        ref hiPowEditMax);
            Get(getValue("EditHiPowMin"),        ref hiPowEditMin);
            Get(getValue("EditFloatVal1Max"),    ref floatVal1EditMax);
            Get(getValue("EditFloatVal1Min"),    ref floatVal1EditMin);
            Get(getValue("EditFloatVal2Max"),    ref floatVal2EditMax);
            Get(getValue("EditFloatVal2Min"),    ref floatVal2EditMin);
            Get(getValue("EditFloatVal3Max"),    ref floatVal3EditMax);
            Get(getValue("EditFloatVal3Min"),    ref floatVal3EditMin);

            GetFormat(getValue("EditShininessFormat"),    ref shininessFmt);
            GetFormat(getValue("EditOutlineWidthFormat"), ref outlineWidthFmt);
            GetFormat(getValue("EditRimPowerFormat"),     ref rimPowerFmt);
            GetFormat(getValue("EditRimShiftFormat"),     ref rimShiftFmt);
            GetFormat(getValue("EditHiRateFormat"),       ref hiRateFmt);
            GetFormat(getValue("EditHiPowFormat"),        ref hiPowFmt);
            GetFormat(getValue("EditFloatVal1Format"),    ref floatVal1Fmt);
            GetFormat(getValue("EditFloatVal2Format"),    ref floatVal2Fmt);
            GetFormat(getValue("EditFloatVal3Format"),    ref floatVal3Fmt);

            var texlist = string.Empty;
            Get(getValue("ToonTexAddon"),    ref texlist);
            if (texlist.Length > 0) {
                // カンマで分割後trm
                toonTexAddon = texlist.Split(new[]{','}, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
                if (LogUtil.IsDebug()) {
                    var buff = new StringBuilder();
                    foreach (var tex in toonTexAddon) {
                        buff.Append(tex).Append(',');
                    }
                    LogUtil.Debug("loading toon addon: ", buff);
                }
            }
            texlist = string.Empty;
            Get(getValue("ToonTex"),    ref texlist);
            if (texlist.Length > 0) {
                // カンマで分割後trm
                toonTexes = texlist.Split(new[]{','}, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
                if (LogUtil.IsDebug()) {
                    var buff = new StringBuilder();
                    foreach (var tex in toonTexes) {
                        buff.Append(tex).Append(',');
                    }
                    LogUtil.Debug("loading toon texes: ", buff);
                }
            }

            Get(getValue("ToonComboAutoApply"), ref toonComboAutoApply);
            Get(getValue("DisplaySlotName"),    ref displaySlotName);
            Get(getValue("EnableMask"),         ref enableMask);   
            Get(getValue("EnableMoza"),         ref enableMoza);
            Get(getValue("SSWithoutUI"),        ref SSWithoutUI);

            var listStr = string.Empty;
            Get(getValue("EnableScenes"),    ref listStr);
            if (listStr.Length > 0) ParseList(listStr, ref enableScenes);
            listStr = string.Empty;
            Get(getValue("EnableOHScenes"),  ref listStr);
            if (listStr.Length > 0) ParseList(listStr, ref enableOHScenes);
            listStr = string.Empty;
            Get(getValue("DisableScenes"),  ref listStr);
            if (listStr.Length > 0) ParseList(listStr, ref disableScenes);
            listStr = string.Empty;
            Get(getValue("DisableOHScenes"),  ref listStr);
            if (listStr.Length > 0) ParseList(listStr, ref disableOHScenes);
        }

        static void ParseList(string valString, ref List<int> ret) {
            var list0 = valString.Split(new[]{','}, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => {
                            int val;
                            return int.TryParse(p, out val) ? val : -1;
                        })
                .Where (val => (val > 0 && val < MAX_SCENES))
                .OrderByDescending(val => val);
            if (list0.Any()) {
                ret = (List<int>)list0;
            }
        }

        static bool Get(string boolString, ref bool output) {
            bool v;
            if (!bool.TryParse(boolString, out v)) return false;
            output = v;
            return true;
        }
    
        static void Get(string floatString, ref float output) {
            float v;
            if (float.TryParse(floatString, out v)) {
                output = v;
            }
        }
        static void Get(string numString, ref int output) {
            int v;
            if (int.TryParse(numString, out v)) {
                output = v;
            }
        }
        static readonly float VERF_VALUE = 12.3f;
        static void GetFormat(string format, ref string output) {
            if (format == null) return;

            float f;
            if (float.TryParse(VERF_VALUE.ToString(format), out f)) {
                output = format;
            } else {
                if (format.Length > 0) {
                    LogUtil.Log("failed to parse Format string:", format);
                }
            }
        }

        static bool Get(string stringVal, ref string output) {
            if (stringVal == null) return false;
            output = stringVal;
            return true;
        }

        private static void GetKeyCode(string keyString, ref KeyCode output) {
            if (string.IsNullOrEmpty(keyString)) return;
            try {
                var key = (KeyCode)Enum.Parse(typeof(KeyCode), keyString);
                output = key;
            } catch(ArgumentException) { }
        }
    }
}
