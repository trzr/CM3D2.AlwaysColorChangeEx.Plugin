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

        private static bool[]  SCENE_AVAILABLES = new bool[] {
            false, false, true,  true,  true,
            true,  false, false, true,  false,
            false, false, true,  false, true,
            true,  true,  false, false, false,
            true,  false, true,  true,  true,
            true,  true,  true,  true,  true, // 29
            true,  true,  true,  true,  true,
            true,  
        };
        private static bool[]  SCENE_OHAVAILABLES = new bool[] {
            false, false, true,  true,  true,
            true,  false, false, true,  false,
            true,  true,  true,  false, true,
            true,  true,  false, true,  true,
            true,  true,  true,  true,  true, // 24
            true,  true,  true,
        };

        private const bool DEFAULT_VAL = true;

        private Mode mode = Mode.Normal;
        public Mode GetMode() {
            return mode;
        }
        public Func<int, bool> IsTarget { get; private set; }
        private readonly Func<int, bool> isTargetNormal = (level) => SCENE_AVAILABLES.Length <= level ? DEFAULT_VAL : SCENE_AVAILABLES[level];
        private readonly Func<int, bool> isTargetOH     = (level) => SCENE_OHAVAILABLES.Length <= level ? DEFAULT_VAL : SCENE_OHAVAILABLES[level];
        
        public Func<int, bool> IsStockTarget { get; private set;}
        private readonly Func<int, bool> isStockNormal = (level) => {
                    switch(level) {
                        case 5: case 3: case 27:
                            return true;
                    }
                    return false;
                };
        private readonly Func<int, bool> isStockOH = (level) => {  
                    switch(level) { 
                        case 4: case 21:
                        return true;
                    }
                    return false;
                };
        public CM3D2SceneChecker() {
            IsTarget = isTargetNormal;
            IsStockTarget = isStockNormal;
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
                IsTarget = isTargetOH;
                IsStockTarget = isStockOH;
//            } else if (dataPath.StartsWith("CM3D2_ED", StringComparison.OrdinalIgnoreCase)) {
//                mode = Mode.ED;
            } else {
                mode = Mode.Normal;
                IsTarget = isTargetNormal;
                IsStockTarget = isStockNormal;
            }

        }
    }
}
