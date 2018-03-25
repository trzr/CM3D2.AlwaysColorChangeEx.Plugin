
using System.Reflection;
using UnityEngine;

namespace CM3D2.AlwaysColorChangeEx.Plugin.Util {
    public static class ClipboardCHelper {
        private static PropertyInfo copyBufferProperty;
        static ClipboardCHelper() {
            Init();
        }

        public static bool IsSupport() {
            return copyBufferProperty != null;
        }
        public static string clipBoard {
            get {
                return (string)copyBufferProperty.GetValue(null, null);
            }
            set {
                copyBufferProperty.SetValue(null, value, null);
            }
        }

        private static void Init() {
            if (copyBufferProperty != null) return;
            var typeObj = typeof(GUIUtility);
            copyBufferProperty = typeObj.GetProperty("systemCopyBuffer", BindingFlags.Static | BindingFlags.NonPublic);
            //if (copyBufferProperty == null) {
            //    copyBufferProperty = typeObj.GetProperty("systemCopyBuffer", BindingFlags.Static | BindingFlags.NonPublic);
            //    if (copyBufferProperty == null) {
            //        throw new Exception("failed to access GUIUtility.systemCopyBuffer");
            //    }
            //}
        }
    }
}
