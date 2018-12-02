using CM3D2.AlwaysColorChangeEx.Plugin.Data;
using UnityEngine;

namespace CM3D2.AlwaysColorChangeEx.Plugin.UI.Data {
    /// <summary>
    /// Description of EditColor.
    /// </summary>
    public class EditColor {
        internal static readonly EditRange<float> RANGE_2  = new EditRange<float>("F3", 0f, 2f);
        internal static readonly EditRange<float> RANGE = new EditRange<float>("F3", 0f, 1f);
        private static readonly string[] empty = new string[0];

        private readonly EditRange<float> range;
        private readonly EditRange<float> rangeA;
        public bool hasAlpha;
        public Color val;
        public readonly ColorType type;

        public bool[] isSyncs;
        public string[] editVals;

        public EditColor(Color val1, ColorType type, EditRange<float> range, EditRange<float> range_a) {
            this.type = type;
            this.range = range;
            rangeA = range_a;
            Set( val1 );
        }

        public EditColor(Color val1, ColorType type = ColorType.rgb, bool rangeOver=true) {
            this.type = type;
            range = (rangeOver)? RANGE_2 : RANGE;
            rangeA = RANGE;
            Set( val1 );
        }

        private string[] ToEdit(ref Color c0) {
            switch(type) {
                case ColorType.rgb:
                    return new[] {
                    c0.r.ToString(range.format),
                    c0.g.ToString(range.format),
                    c0.b.ToString(range.format)};
                case ColorType.rgba:
                    return new[] {
                        c0.r.ToString(range.format),
                        c0.g.ToString(range.format),
                        c0.b.ToString(range.format),
                        c0.a.ToString(rangeA.format)};
                case ColorType.a:
                    return new[] {
                        c0.a.ToString(rangeA.format)};
            }
            return empty;
        }

        public void Set(Color val1) {
//            if (!val1.HasValue) {
//                isSyncs = null;
//                editVals = null;
//                return;
//            }
            
            val = val1;
            editVals = ToEdit(ref val);
            if (isSyncs == null) {
                isSyncs = new bool[editVals.Length];
            }
            for (var i=0; i< isSyncs.Length; i++ ) isSyncs[i] = true;
        }

        public float GetValue(int idx) {
            if (type == ColorType.a) {
                return val.a;
            }

            switch (idx) {
            case 0:
                return val.r;
            case 1:
                return val.g;
            case 2:
                return val.b;
            case 3:
                return val.a;
            }
            return 0;
        }

        public EditRange<float> GetRange(int idx) {
            switch(type) {
                case ColorType.rgb:
                    return range;
                case ColorType.rgba:
                    return (idx == 3)? rangeA : range;
                case ColorType.a:
                default:
                    return rangeA;
            }
        }

        public void Set(int idx, string editVal1, EditRange<float> er = null) {
            if (idx >= editVals.Length) return;
            editVals[idx] = editVal1;

            if (er == null) er = GetRange(idx);
            var sync = false;
            float v;
            if (float.TryParse(editVal1, out v)) {
                if (er.editMin> v)       v = er.editMin;
                else if (er.editMax < v) v = er.editMax;
                else sync = true;

                if (sync) {
                    if (type == ColorType.a) {
                        val.a = v;
                    } else {
                        switch(idx) {
                        case 0:
                            val.r = v;
                            break;
                        case 1:
                            val.g = v;
                            break;
                        case 2:
                            val.b = v;
                            break;
                        case 3:
                            val.a = v;
                            break;
                        }                                
                    }
                }
            }
            isSyncs[idx] = sync;
        }
    }
}
