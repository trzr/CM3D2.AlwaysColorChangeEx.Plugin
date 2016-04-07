
using System;
using UnityEngine;

namespace CM3D2.AlwaysColorChangeEx.Plugin.UI
{
    /// <summary>
    /// Description of EditColor.
    /// </summary>
    public class EditColor
    {
        internal static readonly EditRange range  = new EditRange("F3", 0f, 2f);
        internal static readonly EditRange range_a = new EditRange("F3", 0f, 1f);

        public bool hasAlpha;
        public Color? val;

        public bool[] isSyncs;
        public string[] editVals;
        
        public EditColor(Color? val1, bool hasAlpha = false) {
            this.hasAlpha = hasAlpha;
            Set( val1 );
        }

        private string[] ToEdit(ref Color? c0) {
            Color c = c0.Value;
            if (hasAlpha) {
                return new string[] {
                    c.r.ToString(range.format),
                    c.g.ToString(range.format),
                    c.b.ToString(range.format),
                    c.a.ToString(range.format)};
            } else {
                return new string[] {
                    c.r.ToString(range.format),
                    c.g.ToString(range.format),
                    c.b.ToString(range.format)};
            }
        }

        public void Set(Color? val1) {
            if (!val1.HasValue) {
                isSyncs = null;
                editVals = null;
                val = null;
                return;
            }
            
            this.val = val1;
            editVals = ToEdit(ref val);
            if (isSyncs == null) {
                isSyncs = new bool[editVals.Length];
            }
            for (int i=0; i< isSyncs.Length; i++ ) isSyncs[i] = true;
        }
        public float GetValue(int idx) {
            switch(idx) {
                case 0:
                    return val.Value.r;
                case 1:
                    return val.Value.g;
                case 2:
                    return val.Value.b;
                case 3:
                    return val.Value.a;
            }
            return 0;
        }
        public static EditRange GetRange(int idx) {
            return (idx == 3)? range_a : range;
        }

        public void Set(int idx, string editVal1, EditRange er = null) {
            if (idx < editVals.Length) {
                editVals[idx] = editVal1;

                if (er == null) er = GetRange(idx);
                bool sync = false;
                float v;
                if (float.TryParse(editVal1, out v)) {
                    if (er.editMin> v)       v = er.editMin;
                    else if (er.editMax < v) v = er.editMax;
                    else sync = true;

                    if (sync) {
                        Color c = val.Value;
                        switch(idx) {
                            case 0:
                                c.r = v;
                                break;
                            case 1:
                                c.g = v;
                                break;
                            case 2:
                                c.b = v;
                                break;
                            case 3:
                                c.a = v;
                                break;
                        }
                        val = c;
                    }
                }
                isSyncs[idx] = sync;
            }
        }
    }
}
