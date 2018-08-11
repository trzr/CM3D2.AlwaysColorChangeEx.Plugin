namespace CM3D2.AlwaysColorChangeEx.Plugin.UI {
    public class EditRange<T> {
        public readonly T editMin;
        public readonly T editMax;
        public readonly string format;

        internal EditRange(string fmt, T min, T max) {
            format = fmt;
            editMin = min;
            editMax = max;
        }
    }

    public static class EditRange {
        private static readonly Settings _settings = Settings.Instance;
        public static readonly EditRange<float> renderQueue  = new EditRange<float>("F0", 0f, 5000f);
        public static readonly EditRange<float> shininess    = new EditRange<float>(_settings.shininessFmt, _settings.shininessEditMin, _settings.shininessEditMax);
        public static readonly EditRange<float> outlineWidth = new EditRange<float>(_settings.outlineWidthFmt, _settings.outlineWidthEditMin, _settings.outlineWidthEditMax);
        public static readonly EditRange<float> rimPower  = new EditRange<float>(_settings.rimPowerFmt, _settings.rimPowerEditMin, _settings.rimPowerEditMax);
        public static readonly EditRange<float> rimShift  = new EditRange<float>(_settings.rimShiftFmt, _settings.rimShiftEditMin, _settings.rimShiftEditMax);
        public static readonly EditRange<float> hiRate    = new EditRange<float>(_settings.hiRateFmt, _settings.hiRateEditMin, _settings.hiRateEditMax);
        public static readonly EditRange<float> hiPow     = new EditRange<float>(_settings.hiPowFmt, _settings.hiPowEditMin, _settings.hiPowEditMax);
        public static readonly EditRange<float> floatVal1 = new EditRange<float>(_settings.floatVal1Fmt, _settings.floatVal1EditMin, _settings.floatVal1EditMax);
        public static readonly EditRange<float> floatVal2 = new EditRange<float>(_settings.floatVal2Fmt, _settings.floatVal2EditMin, _settings.floatVal2EditMax);
        public static readonly EditRange<float> floatVal3 = new EditRange<float>(_settings.floatVal3Fmt, _settings.floatVal3EditMin, _settings.floatVal3EditMax);
        public static readonly EditRange<int> hue        = new EditRange<int>("F0", 0, 255);
        public static readonly EditRange<int> saturation = new EditRange<int>("F0", 0, 255);
        public static readonly EditRange<int> light      = new EditRange<int>("F0", 0, 510);
        public static readonly EditRange<int> contrast   = new EditRange<int>("F0", 0, 200);
        public static readonly EditRange<int> rate       = new EditRange<int>("F0", 0, 255);

        public static readonly EditRange<float> boolVal = new EditRange<float>("F0", 0f, 1f);
    }
}