using System;
using System.Text;
using UnityEngine;
using CM3D2.AlwaysColorChangeEx.Plugin;

namespace CM3D2.AlwaysColorChangeEx.Plugin.Util
{
    /// <summary>
    /// Description of Class1.
    /// </summary>
    public static class LogUtil
    {

        public static void DebugLogF(string format, params object[] message) {
#if DEBUG
            var sb = String.Format(format, message);
            Debug.Log(sb);
#endif
        }

        public static void DebugLog(params object[] message) {
#if DEBUG
            var sb = createMesage(message);
            Debug.Log(sb);
#endif
        }

        public static String LogF(string format, params object[] message) {
            var sb = String.Format(format, message);
            Debug.Log(sb);
            return sb;
        }

        public static StringBuilder Log(params object[] message) {
            var sb = createMesage(message);
            Debug.Log(sb);
            return sb;
        }

        public static String ErrorLogF(string format, params object[] message) {
            var sb = String.Format(format, message);
            Debug.LogError(sb);
            return sb;
        }

        public static StringBuilder ErrorLog(params object[] message) {
            var sb = createMesage(message);
            Debug.LogError(sb);
            return sb;
        }

        private static StringBuilder createMesage(object[] message) {
            var sb = new StringBuilder();
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
