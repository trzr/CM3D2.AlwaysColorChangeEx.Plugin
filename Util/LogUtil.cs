using System;
using System.Text;
using CM3D2.AlwaysColorChangeEx.Plugin;

namespace CM3D2.AlwaysColorChangeEx.Plugin.Util
{
    /// <summary>
    /// ログ出力ユーティリティ
    /// </summary>
    public static class LogUtil
    {

        public static void DebugF(string format, params object[] message) {
#if DEBUG
            var sb = String.Format(format, message);
            Debug(sb);
#endif
        }

        public static void Debug(params object[] message) {
#if DEBUG
            var sb = createMesage(message, "[DEBUG]");
            UnityEngine.Debug.Log(sb);
#endif
        }

        public static String LogF(string format, params object[] message) {
            var sb = String.Format(format, message);
            UnityEngine.Debug.Log(sb);
            return sb;
        }

        public static StringBuilder Log(params object[] message) {
            var sb = createMesage(message);
            UnityEngine.Debug.Log(sb);
            return sb;
        }

        public static String ErrorF(string format, params object[] message) {
            var sb = String.Format(format, message);
            UnityEngine.Debug.LogError(sb);
            return sb;
        }

        public static StringBuilder Error(params object[] message) {
            var sb = createMesage(message);
            UnityEngine.Debug.LogError(sb);
            return sb;
        }

        private static StringBuilder createMesage(object[] message, string prefix=null) {
            var sb = new StringBuilder();
            if (prefix != null) sb.Append(prefix);
            sb.Append(AlwaysColorChangeEx.PluginName).Append(':');
            for (int i = 0; i < message.Length; i++) {
                //if (i > 0) sb.Append(',');
                if (message[i] is Exception) sb.Append(' ');
                sb.Append(message[i]);
            }
            return sb;
        }
    }
}
