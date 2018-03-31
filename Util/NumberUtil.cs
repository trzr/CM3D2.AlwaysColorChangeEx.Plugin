/*
 * 
 */
using System;

namespace CM3D2.AlwaysColorChangeEx.Plugin.Util {
    /// <summary>
    /// Description of NumberUtil.
    /// </summary>
    public sealed class NumberUtil {
        private static readonly NumberUtil INSTANCE = new NumberUtil();
        
        public static NumberUtil Instance {
            get {
                return INSTANCE;
            }
        }
        
        private NumberUtil() { }

        public static bool Equals(float f1, float f2, float epsilon=ConstantValues.EPSILON) {
            return Math.Abs(f1-f2) < epsilon;
        }
    }
}
