
using System;
using UnityEngine;

namespace CM3D2.AlwaysColorChange.Plugin
{
    /// <summary>
    /// Description of SingletonClass1.
    /// </summary>
    public sealed class Settings
    {
        private readonly static Settings instance = new Settings();
        public static Settings Instance {
            get {
                return instance;
            }
        }
//    public bool Enable = false;                             // 表示状態
//    public KeySetting Undo = new KeySetting("Control+Z");   // 戻る
//    public KeySetting Redo = new KeySetting("Control+Y");   // 進む

        public KeyCode toggleKey = KeyCode.F9;
        public string configPath;
    
        // 設定の読み込み
        public void Load(Func<string, string> getString)
        {
            GetString(getString("PresetPath"), ref configPath);
            GetKeySetting(getString("ToggleWindow"), ref toggleKey);
        }
    
    //    // 設定の書き込み
    //    public void Save(Action<string, string> setString)
    //    {
    //        setString("Enable", Enable.ToString());
    //        setString("Undo", Undo.ToString());
    //        setString("Redo", Redo.ToString());
    //        setString("Reset", Reset.ToString());
    //        setString("QuickStore1", QuickStore1.ToString());
    //        setString("QuickStore2", QuickStore2.ToString());
    //        setString("QuickStore3", QuickStore3.ToString());
    //        setString("QuickStore4", QuickStore4.ToString());
    //        setString("QuickLoad1", QuickLoad1.ToString());
    //        setString("QuickLoad2", QuickLoad2.ToString());
    //        setString("QuickLoad3", QuickLoad3.ToString());
    //        setString("QuickLoad4", QuickLoad4.ToString());
    //        setString("WindowX", WindowX.ToString());
    //        setString("WindowY", WindowY.ToString());
    //        setString("WindowW", WindowW.ToString());
    //        setString("WindowH", WindowH.ToString());
    //    }
    
        static void GetBool(string boolString, ref bool output) {
            bool v;
            if (bool.TryParse(boolString, out v)){
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
        static void GetKeySetting(string keyString, ref KeyCode output) {
            if(!String.IsNullOrEmpty (keyString)) {
                try {
                var key = (KeyCode)Enum.Parse(typeof(KeyCode), keyString);
                    output = key;
                } catch(ArgumentException ignore) {}
                
            }
        }
    }
}
