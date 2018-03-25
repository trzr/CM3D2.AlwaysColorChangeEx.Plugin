
namespace CM3D2.AlwaysColorChangeEx.Plugin.UI {
    /// <summary>
    /// Description of EditValue.
    /// </summary>
    public class EditValue {
        public bool isSync;
        public float val;

        public string editVal;
        internal EditRange range;
        
        internal EditValue(float val1, EditRange attr) {
            range = attr;
            Set( val1 );
        }
        public EditValue(float val1, string format, float min, float max) {
            range = new EditRange(format, min, max);
            Set( val1 );
        }
        public void Set(float val1) {
            val = val1;
            editVal = val.ToString(range.format);
            isSync = true;
        }
        public void SetWithCheck(float val1) {
            if (range.editMin > val1) val = range.editMin;
            else if (range.editMax < val1) val = range.editMax;
            else val = val1;
            editVal = val.ToString(range.format);
            isSync = true;
        }
        public void Set(string editVal1) {
            editVal = editVal1;

            float v;
                isSync = false;
            if (float.TryParse(editVal1, out v)) {
                if (range.editMin> v)       v = range.editMin;
                else if (range.editMax < v) v = range.editMax;
                else isSync = true;
                val = v;
            }
        }
    }
    public class EditRange {
        static Settings settings = Settings.Instance;
        public static readonly EditRange renderQueue  = new EditRange("F0", 0, 5000f);
        public static readonly EditRange shininess    = new EditRange(settings.shininessFmt, settings.shininessEditMin, settings.shininessEditMax);
        public static readonly EditRange outlineWidth = new EditRange(settings.outlineWidthFmt, settings.outlineWidthEditMin, settings.outlineWidthEditMax);
        public static readonly EditRange rimPower  = new EditRange(settings.rimPowerFmt, settings.rimPowerEditMin, settings.rimPowerEditMax);
        public static readonly EditRange rimShift  = new EditRange(settings.rimShiftFmt, settings.rimShiftEditMin, settings.rimShiftEditMax);
        public static readonly EditRange hiRate    = new EditRange(settings.hiRateFmt, settings.hiRateEditMin, settings.hiRateEditMax);
        public static readonly EditRange hiPow     = new EditRange(settings.hiPowFmt, settings.hiPowEditMin, settings.hiPowEditMax);
        public static readonly EditRange floatVal1 = new EditRange(settings.floatVal1Fmt, settings.floatVal1EditMin, settings.floatVal1EditMax);
        public static readonly EditRange floatVal2 = new EditRange(settings.floatVal2Fmt, settings.floatVal2EditMin, settings.floatVal2EditMax);
        public static readonly EditRange floatVal3 = new EditRange(settings.floatVal3Fmt, settings.floatVal3EditMin, settings.floatVal3EditMax);

        public static readonly EditRange boolVal = new EditRange("F0", 0, 1f);
        public float editMin;
        public float editMax;
        public string format;

        internal EditRange(string fmt, float min, float max) {
            format = fmt;
            editMin = min;
            editMax = max;
        }
    }
}
