using UnityEngine;

namespace CM3D2.AlwaysColorChangeEx.Plugin.UI.Helper {
            /// <summary>GUI色設定</summary>
    public class GUIColorStore {
        #region Methods
        /// <summary>背景色とコンテンツを設定する</summary>
        /// <param name="contentColor">コンテンツ色</param>
        /// <param name="backgroundColor">背景色</param>
        public void SetColor(Color contentColor, Color? backgroundColor) {
            _backgroundColor = GUI.backgroundColor;
            _contentColor    = GUI.contentColor;

            if (backgroundColor.HasValue) {
                GUI.backgroundColor = backgroundColor.Value;
            }
            GUI.contentColor    = contentColor;
        }

        /// <summary>背景色とコンテンツを設定する</summary>
        /// <param name="contentColor">コンテンツ色</param>
        /// <param name="backgroundColor">背景色</param>
        public void SetColor(ref Color contentColor, ref Color? backgroundColor) {
            _backgroundColor = GUI.backgroundColor;
            _contentColor    = GUI.contentColor;

            if (backgroundColor.HasValue) {
                GUI.backgroundColor = backgroundColor.Value;
            }
            GUI.contentColor    = contentColor;
        }

        /// <summary>背景色とコンテンツを設定する</summary>
        /// <param name="contentColor">コンテンツ色</param>
        /// <param name="backgroundColor">背景色</param>
        public void SetColor(ref Color contentColor, ref Color backgroundColor) {
            _backgroundColor = GUI.backgroundColor;
            _contentColor    = GUI.contentColor;

            GUI.backgroundColor = backgroundColor;
            GUI.contentColor    = contentColor;
        }

        /// <summary>背景色を設定する</summary>
        /// <param name="backgroundColor">背景色</param>
        public void SetBGColor(ref Color? backgroundColor) {
            if (!backgroundColor.HasValue) return;

            _backgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = backgroundColor.Value;
        }

        /// <summary>背景色を設定する</summary>
        /// <param name="backgroundColor">背景色</param>
        public void SetBGColor(ref Color backgroundColor) {
            _backgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = backgroundColor;
        }

        /// <summary>コンテンツ色を設定する</summary>
        /// <param name="contentColor">コンテンツ色</param>
        public void SetContentColor(ref Color contentColor) {
            _contentColor = GUI.contentColor;
            GUI.contentColor = contentColor;
        }

        /// <summary>元に戻す</summary>
        public void Restore() {
            if (_backgroundColor.HasValue) {
                GUI.backgroundColor = _backgroundColor.Value;
                _backgroundColor = null;
            }

            if (!_contentColor.HasValue) return;
            GUI.contentColor = _contentColor.Value;
            _contentColor = null;
        }
        #endregion

        #region Fileds
        private Color? _backgroundColor;
        private Color? _contentColor;
    
        private static readonly GUIColorStore INSTANCE = new GUIColorStore();
        public static GUIColorStore Default { get { return INSTANCE; } }
        #endregion
    }
}