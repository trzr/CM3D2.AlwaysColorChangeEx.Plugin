using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace CM3D2.AlwaysColorChangeEx.Plugin.Util {
    public class TexUtil {
        private static volatile TexUtil _lazy;
        private static readonly object LOCK = new object();
        public static TexUtil Instance {
            get {
                if (_lazy != null) return _lazy;
                lock (LOCK) {
                    if (_lazy == null) {
                        _lazy = new TexUtil();
                    }
                }
                return _lazy;
            }
        }
        private Func<string, byte[]> LoadTex;
        private Func<string, Texture2D> CreateTex;
        /// <summary>
        /// 1.49より前の版向けにAPI参照可否でdelegatorを作成
        /// 旧版向け(互換性)のための処置
        /// </summary>
        private TexUtil() {
            var typeObj = typeof(ImportCM);
            try {
                var method = typeObj.GetMethod("LoadTexture", new [] { typeof(string) });
                if (method != null) {
                    if (method.ReturnType == typeof(byte[])) {
                        LoadTex = (Func<string, byte[]>)Delegate.CreateDelegate(typeof(Func<string, byte[]>), method);
                        LogUtil.Debug("using old mode (tex access API)");
                    }
                }
            } catch(Exception e) {
                LogUtil.Error("failed to initialize tex access API", e);
            }

            if (LoadTex != null) return;
            {
                var method = typeObj.GetMethod("CreateTexture", new[] { typeof(string) });
                if (method == null) return;
                if (method.ReturnType == typeof(Texture2D)) {
                    CreateTex = (Func<string, Texture2D>)Delegate.CreateDelegate(typeof(Func<string, Texture2D>), method);
                }
            }
        }

        public Texture2D Load(string file) {
            if (LoadTex == null) return CreateTex(file);
            var tex2D = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            //var loadedTex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            tex2D.LoadImage(LoadTex(file));
            return tex2D;

            //return ImportCM.CreateTexture(file);

        }
    }
}
