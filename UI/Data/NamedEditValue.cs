using System;
using CM3D2.AlwaysColorChangeEx.Plugin.Util;

namespace CM3D2.AlwaysColorChangeEx.Plugin.UI.Data {
    public class NamedEditValue: EditValue {
        public readonly string name;
        public Action<float> act;
        public NamedEditValue(string name, float val1, EditRange<float> attr, Action<float> act) : base(val1, attr) {
            this.name = name;
            this.act = act;
        }

        public NamedEditValue(string name, float val1, string format, float min, float max, Action<float> act) : base(val1, format, min, max) {
            this.name = name;
            this.act = act;
        }

        public void Update(float value) {
            if (!NumberUtil.Equals(val, value)) {
                Set(value);
            }
        }

    }
}
