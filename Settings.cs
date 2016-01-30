using System;
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

        public KeyCode toggleKey = KeyCode.F12;
        public string configPath;
        public float shininessMax    =  20f;
        public float shininessMin    =   0f;
        public float outlineWidthMax =   0.1f;
        public float outlineWidthMin =   0f;
        public float rimPowerMax     = 100f;
        public float rimPowerMin     =   0f;
        public float rimShiftMax     =   5f;
        public float rimShiftMin     =  -5f;
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

        public string menuPrefix     = "";
        public string iconSuffix     = "_i_";
        public string resSuffix      = "_mekure_";
        public string txtPrefixMenu  = "Assets/menu/menu/";
        public string txtPrefixTex   = "Assets/texture/texture/";
        public string[] toonTexAddon = new string[0];
    
        // 設定の読み込み
        public void Load(Func<string, string> getValue)
        {
            GetString(getValue("PresetPath"),    ref configPath);
            GetKeyCode(getValue("ToggleWindow"), ref toggleKey);
            GetFloat(getValue("SliderShininessMax"),    ref shininessMax);
            GetFloat(getValue("SliderShininessMin"),    ref shininessMin);
            GetFloat(getValue("SliderOutlineWidthMax"), ref outlineWidthMax);
            GetFloat(getValue("SliderOutlineWidthMin"), ref outlineWidthMin);
            GetFloat(getValue("SliderRimPowerMax"),     ref rimPowerMax);
            GetFloat(getValue("SliderRimPowerMin"),     ref rimPowerMin);
            GetFloat(getValue("SliderRimShiftMax"),     ref rimShiftMax);
            GetFloat(getValue("SliderRimShiftMin"),     ref rimShiftMin);
            GetFloat(getValue("SliderHiRateMax"),       ref hiRateMax);
            GetFloat(getValue("SliderHiRateMin"),       ref hiRateMin);
            GetFloat(getValue("SliderHiPowMax"),        ref hiPowMax);
            GetFloat(getValue("SliderHiPowMin"),        ref hiPowMin);
            GetFloat(getValue("SliderFloatVal1Max"),    ref floatVal1Max);
            GetFloat(getValue("SliderFloatVal1Min"),    ref floatVal1Min);
            GetFloat(getValue("SliderFloatVal2Max"),    ref floatVal2Max);
            GetFloat(getValue("SliderFloatVal2Min"),    ref floatVal2Min);
            GetFloat(getValue("SliderFloatVal3Max"),    ref floatVal3Max);
            GetFloat(getValue("SliderFloatVal3Min"),    ref floatVal3Min);
            var texlist = string.Empty;
            GetString(getValue("ToonTexAddon"),    ref texlist);
            if (texlist.Length > 0) {
                // カンマで分割後trm
                toonTexAddon = texlist.Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
            }
            
        }
       
        static void GetBool(string boolString, ref bool output) {
            bool v;
            if (bool.TryParse(boolString, out v)) {
                output = v;
            }
        }
    
        static void GetFloat(string floatString, ref float output) {
            float v;
            if (float.TryParse(floatString, out v)) {
                output = v;
            }
        }

        static void GetString(string stringVal, ref string output) {
            if(!String.IsNullOrEmpty (stringVal)) {
                output = stringVal;
            }
        }

        static void GetKeyCode(string keyString, ref KeyCode output) {
            if(!String.IsNullOrEmpty (keyString)) {
                try {
                var key = (KeyCode)Enum.Parse(typeof(KeyCode), keyString);
                    output = key;
                #pragma warning disable 0168
                } catch(ArgumentException ignore) { }
                #pragma warning restore 0168
            }
        }
    }
}
