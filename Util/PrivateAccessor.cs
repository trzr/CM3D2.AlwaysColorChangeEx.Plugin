using System;
using System.Reflection;

namespace CM3D2.AlwaysColorChangeEx.Plugin.Util
{
    /// <summary>
    /// privateフィールド用のアクセサ
    /// </summary>
    public sealed class PrivateAccessor
    {
        private static PrivateAccessor instance = new PrivateAccessor();
        
        public static PrivateAccessor Instance {
            get {
                return instance;
            }
        }

        private PrivateAccessor() { }
        public static T Get<T>(object instance, string fieldName) {
            try {
                var field =  instance.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);//  | BindingFlags.GetField | BindingFlags.SetField 
                return (T)field.GetValue(instance);
            } catch(Exception e) {
                LogUtil.Debug(e);
                return default (T);
            }
        }
        public static T Get<T>(Type type, string fieldName) {
            try {
                var field =  type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);//  | BindingFlags.GetField | BindingFlags.SetField 
                return (T)field.GetValue(null);
            } catch(Exception e) {
                LogUtil.Debug(e);
                return default (T);
            }
        }
        public static void Set<T>(object instance, string fieldName, T value) {
            try {
                var field =  instance.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);//  | BindingFlags.GetField | BindingFlags.SetField 
                field.SetValue(instance, value);
            } catch(Exception e) {
                LogUtil.Debug(e);
            }
        }
        public static void Set<T>(Type type, string fieldName, T value) {
            try {
                var field =  type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);//  | BindingFlags.GetField | BindingFlags.SetField 
                field.SetValue(null, value);
            } catch(Exception e) {
                LogUtil.Debug(e);
            }
        }
    }
}
