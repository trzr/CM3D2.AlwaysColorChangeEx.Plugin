using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace CM3D2.AlwaysColorChangeEx.Plugin.Util {
    public class TexUtil {
        private static TexUtil _lazy;
        private static readonly object _lock = new object();
        public static TexUtil Instance {
            get {
                if (_lazy == null) {
                    lock(_lock) {
                        if (_lazy == null) {
                            _lazy = new TexUtil();
                        }
                    }
                }
                return _lazy;
            }
        }
        private Func<string, byte[]> LoadTex;
        private Func<string, Texture2D> CreateTex;
        /// <summary>
        /// 1.49より前の版向けにAPI参照可否でdelegatorを作成
        /// 旧版向けの互換性のための処置
        /// </summary>
        private TexUtil() {
            Type typeObj = typeof(ImportCM);
            try {
                var method = typeObj.GetMethod("LoadTexture", new [] { typeof(string) });
                if (method != null) {

                    if (method.ReturnType == typeof(Byte[])) {
                        LoadTex = (Func<string, byte[]>)Delegate.CreateDelegate(typeof(Func<string, byte[]>), method);
                    }
                }
            }catch(Exception e) {
                LogUtil.Debug(e);
            }
            if (LoadTex == null) {
                try {
                    var method = typeObj.GetMethod("CreateTexture", new[] { typeof(string) });
                    if (method != null) {

                        if (method.ReturnType == typeof(Texture2D)) {
                            CreateTex = (Func<string, Texture2D>)Delegate.CreateDelegate(typeof(Func<string, Texture2D>), method);
                        }
                    }
                } catch (Exception e) {
                    LogUtil.Debug(e);
                }
            }
        }

        public Texture2D Load(string file) {
            if (LoadTex != null) {
                var tex2d = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                //var loadedTex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                tex2d.LoadImage(LoadTex(file));
                return tex2d;

            } else {
                //return ImportCM.CreateTexture(file);
                return CreateTex(file);
            }

        }
    }
}
