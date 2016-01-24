/*
 * 
 */
using System;

namespace CM3D2.AlwaysColorChangeEx.Plugin.Util
{
    /// <summary>
    /// Description of NumberUtil.
    /// </summary>
    public sealed class NumberUtil
    {
        private static readonly NumberUtil instance = new NumberUtil();
        
        public static NumberUtil Instance {
            get {
                return instance;
            }
        }
        
        private NumberUtil() {
        }
        private const float EPSILON = 0.001f;
        public static bool Equals(float f1, float f2) {
            return Math.Abs(f1-f2) < EPSILON;
        }
        public static bool Equals(float f1, float f2, float epsilon) {
            return Math.Abs(f1-f2) < epsilon;
        }
    }
}
