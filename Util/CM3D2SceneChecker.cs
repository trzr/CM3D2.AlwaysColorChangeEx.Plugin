using System;
using System.Collections.Generic;
using UnityEngine;

namespace CM3D2.AlwaysColorChangeEx.Plugin.Util
{
    /// <summary>
    /// シーンチェック用クラス
    /// </summary>
    public class CM3D2SceneChecker
    {
        public enum Mode {
            Normal,
//            ED,
            OH,
        }

//    private enum TargetLevel {
//        SceneCompetitiveShow = 2,    // 品評会
//        SceneDaily = 3,              // 日常
//        SceneDance_DDFL = 4,         // ダンス:ドキドキ☆Fallin' Love
//        SceneEdit = 5,               // エディット
//        SceneUserEdit = 12,          // 男エディット
//        SceneYotogi = 14,            // 夜伽
//        SceneADV = 15,               // ADVパート
//        SceneStartDaily = 16,        // 
//        SceneDance_ETYL = 20,        // ダンス:entrance to you
//        SceneDance_SCL_Release = 22, // ダンス:scarlet leap
//        SceneDeskCustomize     = 23, // デスクトップカスタマイズ
//        SceneFreeModeSelect    = 24, // イベント回想
//        SceneMaidBattle        = 25, // メイドバトル
//        SceneDance_SMT_Release = 26, // ダンス:stellar my tears
//        ScenePhotoMode = 27,         // 撮影モード
//        SceneDance_RTY_Release = 28, // ダンス:rhythmix to you
//    }
// 29
        private static bool[]  SCENE_AVAILABLES = new bool[] {
            false, false, true,  true,  true,
            true,  false, false, true,  false,
            false, false, true,  false, true,
            true,  true,  false, false, false,
            true,  false, true,  true,  true,
            true,  true,  true,  true,  true, // 29c
            true,  true,  true,
        };
        private static bool[]  SCENE_OHAVAILABLES = new bool[] {
            false, false, true,  true,  true,
            true,  false, false, true,  false,
            true,  true,  true,  false, true,
            true,  true,  false, true,  true,
            true,  true,  true,  true,  true, // 23c
            true,  true            
        };

        private const bool DEFAULT_VAL = false;
        public bool IsVR { get; private set; }
        private Mode mode = Mode.Normal;

        public Mode GetMode() {
            return mode;
        }
        public bool IsTarget(int level) {
            switch(mode) {
                case Mode.Normal:
                    // 範囲外はデフォルト値
                    return SCENE_AVAILABLES.Length <= level ? DEFAULT_VAL : SCENE_AVAILABLES[level];
                case Mode.OH:
                    return SCENE_OHAVAILABLES.Length <= level ? DEFAULT_VAL : SCENE_OHAVAILABLES[level];
//                case Mode.ED:                    
//                    break;
            }
            return DEFAULT_VAL;
        }
        public bool IsStockTarget(int level) {
            switch(mode) {
                case Mode.Normal:
                    switch(level) {
                        case 5: case 3: case 27:
                            return true;
                    }
                    break;
                case Mode.OH:
                    switch(level) {
                        case 4: case 21:
                        return true;
                    }
                    break;
//                case Mode.ED:                    
//                    break;
            }
            return false;
        }
        
        public void Init() {
            CheckMode();
            Settings settings = Settings.Instance;
            switch(mode) {
                case Mode.Normal:
                    applySceneArray(SCENE_AVAILABLES, settings.disableScenes, settings.enableScenes);
                    break;
                case Mode.OH:
                    applySceneArray(SCENE_OHAVAILABLES, settings.disableOHScenes, settings.enableOHScenes);
                    break;
//                case Mode.ED:
//                    // unsupported.
//                    break;
            }
        }
        private void applySceneArray(bool[] scenes, List<int> disables, List<int> enables) {
            int max = scenes.Length-1;
            if (disables != null) {
                if (max < disables[0]) max = disables[0];
            }
            if (enables != null) {
                if (max < enables[0]) max = enables[0];
            }
            if (max > scenes.Length-1) {
                var tmp = new bool[max+1];
                Array.Copy(scenes, tmp, scenes.Length);
                for (int i=scenes.Length; i< tmp.Length; i++) {
                    tmp[i] = DEFAULT_VAL;
                }
                scenes = tmp;
                if (disables != null) {
                    foreach (var idx in disables) {
                        scenes[idx] = false;
                    }
                }
                if (enables != null) {
                    foreach (var idx in enables) {
                        scenes[idx] = true;
                    }                            
                }
            }
        }


        public void CheckMode() {
            var dataPath = Application.dataPath;
            if (dataPath.StartsWith("CM3D2OH", StringComparison.OrdinalIgnoreCase)) {
                mode = Mode.OH;
//            } else if (dataPath.StartsWith("CM3D2_ED", StringComparison.OrdinalIgnoreCase)) {
//                mode = Mode.ED;
            } else {
                mode = Mode.Normal;
            }
            IsVR = (dataPath.Contains("VRx64"));
        }
    }
}
