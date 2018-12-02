
namespace CM3D2.AlwaysColorChangeEx.Plugin.UI.Data {
    /// <summary>
    /// int型用のエディット値クラス
    /// </summary>
    public class EditIntValue : EditValueBase<int> {
        
        internal EditIntValue(int val1, EditRange<int> attr) : base(val1, attr) {}

        public EditIntValue(int val1, string format, int min, int max) : base(val1, format, min, max) {}

        protected override bool TryParse(string edit, out int v) {
            return int.TryParse(edit, out v);
        }
    }
}
