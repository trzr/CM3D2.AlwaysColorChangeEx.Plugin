namespace CM3D2.AlwaysColorChangeEx.Plugin.Util {
    ///
    /// カーソル・ドラッグ操作などを対象としたユーティリティクラス.
    ///
    internal class UIHelper {

        internal bool cmrCtrlChanged;
        internal bool cursorContains;

        internal bool IsEnabledUICamera() {
            return UICamera.currentCamera != null && UICamera.currentCamera.enabled;
        }

        internal void SetCameraControl(bool enable) {
            if (cmrCtrlChanged != enable) return;

            GameMain.Instance.MainCamera.SetControl(enable);
            UICamera.InputEnable = enable;
            cmrCtrlChanged = !enable;
        }

        /// <summary> カーソル位置に応じて、カメラコントロールの有効化/無効化を行う </summary>
        internal void UpdateCameraControl(bool contains) {
            cursorContains = contains;
            // カメラコントロールの有効化/無効化 (Windowの範囲外では、自身がコントロールを変更したケース以外は更新しない)
            if (cursorContains) {
                if (GameMain.Instance.MainCamera.GetControl()) {
                    SetCameraControl(false);
                }
            } else {
                SetCameraControl(true);
            }
        }
    }
}
