using CM3D2.AlwaysColorChangeEx.Plugin.Util;

namespace CM3D2.AlwaysColorChangeEx.Plugin.UI {
    public abstract class BaseView {
        protected static readonly MaidHolder holder = MaidHolder.Instance;
        protected UIParams uiParams;

        public abstract void UpdateUI(UIParams uiparams);
        public virtual void Clear() { }
        public virtual void Dispose() { }
    }
}
