
using System;
using System.Reflection;
using UnityEngine;

namespace CM3D2.AlwaysColorChangeEx.Plugin.Util
{
    /// <summary>
    /// Description of ResouceHolder.
    /// </summary>
    public sealed class ResourceHolder
    {
        private static readonly ResourceHolder instance = new ResourceHolder();
        public static ResourceHolder Instance {
            get {
                return instance;
            }
        }
        private static FileUtilEx outUtil = FileUtilEx.Instance;
        private ResourceHolder() {}
        private Assembly asmbl = Assembly.GetExecutingAssembly();
        private Texture2D dirImage;
        private Texture2D fileImage;
        private Texture2D pictImage;
        public Texture2D PictImage {
            get {
                if (pictImage == null) pictImage = LoadTex("picture");
                return pictImage;
            }
        }
        public Texture2D FileImage {
            get {
                if (fileImage == null) fileImage = LoadTex("file");
                return fileImage;
            }
        }
        public Texture2D DirImage {
            get {
                if (dirImage == null) dirImage = LoadTex("folder");
                return dirImage;
            }
        }
        private Texture2D LoadTex(string name) {
            try {
                using (var fs = asmbl.GetManifestResourceStream(name + ".png")) {
                    var tex2d = outUtil.LoadTexture(fs);
                    tex2d.name = name;
                    LogUtil.DebugLog("load resource file image");
                    return tex2d;
                }
            } catch(Exception e) {
                LogUtil.DebugLog(e);
                return null;
            }
        }
        
        public void Clear() {
            if (dirImage != null) UnityEngine.Object.DestroyImmediate(dirImage);
            if (fileImage != null) UnityEngine.Object.DestroyImmediate(fileImage);
            dirImage = null;
            fileImage = null;

        }
    }
}
