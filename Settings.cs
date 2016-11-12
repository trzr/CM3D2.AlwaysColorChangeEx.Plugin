using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CM3D2.AlwaysColorChangeEx.Plugin
{
    public sealed class Settings
    {
        private readonly static Settings instance = new Settings();
        public static Settings Instance {
            get { return instance; }
        }

        public KeyCode  toggleKey = KeyCode.F12;
        public EventModifiers toggleModifiers = EventModifiers.None;
        public HashSet<KeyCode> toggleKeyModifier = null;
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
        public float hiPowEditMax        =  100f;
        public float hiPowEditMin        =   0.00001f;
        public float floatVal1EditMax    = 500f;
        public float floatVal1EditMin    =   0f;
        public float floatVal2EditMax    =  50f;
        public float floatVal2EditMin    = -50f;
        public float floatVal3EditMax    =  50f;
        public float floatVal3EditMin    =   0f;

        public float[] shininessRange() {
            return new float[] {shininessMin, shininessMax,};
        }
        public float[] outlineWidthRange() {
            return new float[] {outlineWidthMin, outlineWidthMax, };
        }
        public float[] rimPowerRange() {
            return new float[] {rimPowerMin, rimPowerMax, };
        }
        public float[] rimShiftRange() {
            return new float[] {rimShiftMin, rimShiftMax, };
        }
        public float[] hiRateRange() {
            return new float[] {hiRateMin, hiRateMax, };
        }
        public float[] hiPowRange() {
            return new float[] {hiPowMin, hiPowMax, };
        }
        public float[] floatVal1Range() {
            return new float[] {floatVal1Min, floatVal1Max, };
        }
        public float[] floatVal2Range() {
            return new float[] {floatVal2Min, floatVal2Max, };
        }
        public float[] floatVal3Range() {
            return new float[] {floatVal3Min, floatVal3Max, };
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
            "toonFace", "toonFace002",
            "toonSkin", "toonSkin002",
            "toonBlackA1",
            "toonFace_shadow",
            "toonDress_shadow",
            "toonSkin_Shadow",
            "toon_shadow0","toon_shadow1","toon_shadow2",
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

        private const int MAX_SCENES = 256;
        public List<int> enableScenes;
        public List<int> disableScenes;
        public List<int> enableOHScenes;
        public List<int> disableOHScenes;

        // 設定の読み込み
        public void Load(Func<string, string> getValue)
        {
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

            var texlist = string.Empty;
            Get(getValue("ToonTexAddon"),    ref texlist);
            if (texlist.Length > 0) {
                // カンマで分割後trm
                toonTexAddon = texlist.Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
            }
            texlist = string.Empty;
            Get(getValue("ToonTex"),    ref texlist);
            if (texlist.Length > 0) {
                // カンマで分割後trm
                toonTexes = texlist.Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
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
            var list0 = valString.Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries)
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
            if (bool.TryParse(boolString, out v)) {
                output = v;
                return true;
            }
            return false;
        }
    
        static void Get(string floatString, ref float output) {
            float v;
            if (float.TryParse(floatString, out v)) {
                output = v;
            }
        }

        static bool Get(string stringVal, ref string output) {
            if(stringVal != null) {
                output = stringVal;
                return true;
            }
            return false;
        }

        static void GetKeyCode(string keyString, ref KeyCode output) {
            if(!String.IsNullOrEmpty (keyString)) {
                try {
                var key = (KeyCode)Enum.Parse(typeof(KeyCode), keyString);
                    output = key;
                } catch(ArgumentException) { }
            }
        }
    }
}
