using System;
using System.Text;
using CM3D2.AlwaysColorChangeEx.Plugin;

namespace CM3D2.AlwaysColorChangeEx.Plugin.Util {
    /// <summary>
    /// ログ出力ユーティリティ
    /// </summary>
    public static class LogUtil {

        public static bool IsDebug() {
#if DEBUG
            return true;
#else
            return false;
#endif
        }
        public static void DebugF(string format, params object[] message) {
#if DEBUG
            var sb = string.Format(format, message);
            Debug(sb);
#endif
        }

        public static void Debug(params object[] message) {
#if DEBUG
            var sb = CreateMessage(message, "[DEBUG]");
            UnityEngine.Debug.Log(sb);
#endif
        }

        public static string LogF(string format, params object[] message) {
            var sb = string.Format(format, message);
            UnityEngine.Debug.Log(sb);
            return sb;
        }

        public static StringBuilder Log(params object[] message) {
            var sb = CreateMessage(message);
            UnityEngine.Debug.Log(sb);
            return sb;
        }

        public static String ErrorF(string format, params object[] message) {
            var sb = String.Format(format, message);
            UnityEngine.Debug.LogError(sb);
            return sb;
        }

        public static StringBuilder Error(params object[] message) {
            var sb = CreateMessage(message);
            UnityEngine.Debug.LogError(sb);
            return sb;
        }

        private static StringBuilder CreateMessage(object[] message, string prefix=null) {
            var sb = new StringBuilder();
            if (prefix != null) sb.Append(prefix);
            sb.Append(AlwaysColorChangeEx.PluginName).Append(':');
            foreach (var t in message) {
                if (t is Exception) sb.Append(' ');
                sb.Append(t);
            }
            return sb;
        }
    }
}
