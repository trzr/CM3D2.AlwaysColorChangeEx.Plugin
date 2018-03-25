using System;
using System.Collections.Generic;
using UnityEngine;

namespace CM3D2.AlwaysColorChangeEx.Plugin.Util {
    /// <summary>
    /// シーンチェック用クラス
    /// </summary>
    public class CM3D2SceneChecker {
        public enum Mode {
            Normal,
//            ED,
            OH,
        }

        private static bool[] _sceneAvailables = {
            false, false, true,  true,  true,
            true,  false, false, true,  false,
            false, false, true,  false, true,
            true,  true,  false, false, false,
            true,  false, true,  true,  true,
            true,  true,  true,  true,  true,
            true,  true,  true,
        };
        private static bool[] _sceneOHAvailables = {
            false, false, true,  true,  true,
            true,  false, false, true,  false,
            true,  true,  true,  false, true,
            true,  true,  false, true,  true,
            true,  true,  true,  true,  true, // 23c
            true,  true,
        };

        private const bool DEFAULT_VAL = true;
//        public bool isVR;
        public Func<int, bool> IsTarget { get; private set; }
        // ReSharper disable once SimplifyConditionalTernaryExpression
        private readonly Func<int, bool> _isTargetNormal = (level) => _sceneAvailables.Length <= level ? DEFAULT_VAL : _sceneAvailables[level];
        // ReSharper disable once SimplifyConditionalTernaryExpression
        private readonly Func<int, bool> _isTargetOH     = (level) => _sceneOHAvailables.Length <= level ? DEFAULT_VAL :_sceneOHAvailables[level];
        
        public Func<int, bool> IsStockTarget { get; private set;}
        private readonly Func<int, bool> _isStockNormal = (level) => {
                    switch(level) {
                        case 5: case 3: case 27:
                            return true;
                    }
                    return false;
                };
        private readonly Func<int, bool> _isStockOH = (level) => {  
                    switch(level) { 
                        case 4: case 21:
                        return true;
                    }
                    return false;
                };
        

        private Mode _mode = Mode.Normal;

        public CM3D2SceneChecker() {
            IsTarget = _isTargetNormal;
            IsStockTarget = _isStockNormal;
        }
        public Mode GetMode() {
            return _mode;
        }
        public void Init() {
            CheckMode();
            var settings = Settings.Instance;
            switch(_mode) {
                case Mode.Normal:
                    ApplySceneArray(ref _sceneAvailables, settings.disableScenes, settings.enableScenes);
                    break;
                case Mode.OH:
                    ApplySceneArray(ref _sceneOHAvailables, settings.disableOHScenes, settings.enableOHScenes);
                    break;
//                case Mode.ED:
//                    // unsupported.
//                    break;
            }
        }
        private void ApplySceneArray(ref bool[] scenes, IList<int> disables, IList<int> enables) {
            var max = scenes.Length-1;
            if (disables != null) {
                if (max < disables[0]) max = disables[0];
            }
            if (enables != null) {
                if (max < enables[0]) max = enables[0];
            }

            if (max <= scenes.Length - 1) return;

            var tmp = new bool[max+1];
            Array.Copy(scenes, tmp, scenes.Length);
            for (var i=scenes.Length; i< tmp.Length; i++) {
                tmp[i] = DEFAULT_VAL;
            }
            scenes = tmp;
            if (disables != null) {
                foreach (var idx in disables) {
                    scenes[idx] = false;
                }
            }

            if (enables == null) return;
            foreach (var idx in enables) {
                scenes[idx] = true;
            }
        }

        public void CheckMode() {
            var dataPath = Application.dataPath;
            if (dataPath.StartsWith("CM3D2OH", StringComparison.OrdinalIgnoreCase)) {
                _mode = Mode.OH;
                IsTarget = _isTargetOH;
                IsStockTarget = _isStockOH;
//            } else if (dataPath.StartsWith("CM3D2_ED", StringComparison.OrdinalIgnoreCase)) {
//                mode = Mode.ED;
            } else {
                _mode = Mode.Normal;
                IsTarget = _isTargetNormal;
                IsStockTarget = _isStockNormal;
            }
//            isVR = (dataPath.Contains("VRx64"));
        }
    }
}
