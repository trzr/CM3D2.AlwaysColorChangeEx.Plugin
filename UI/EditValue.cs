
namespace CM3D2.AlwaysColorChangeEx.Plugin.UI {
    /// <summary>
    /// float用エディット値クラス
    /// </summary>
    public class EditValue : EditValueBase<float> {
        internal EditValue(float val1, EditRange<float> attr) : base(val1, attr) {}

        public EditValue(float val1, string format, float min, float max) : base(val1, format, min, max) {}

        protected override bool TryParse(string edit, out float v) {
            return float.TryParse(edit, out v);
        }
    }
}
