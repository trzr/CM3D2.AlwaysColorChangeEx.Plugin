using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace CM3D2.AlwaysColorChangeEx.Plugin.Util {
    /// <summary>
    /// リソースのロードユーティリティ
    /// </summary>
    public sealed class ResourceHolder {
        public static readonly ResourceHolder Instance = new ResourceHolder();

        private readonly FileUtilEx fileUtil = FileUtilEx.Instance;
        private readonly Assembly asmbl = Assembly.GetExecutingAssembly();
        private ResourceHolder() {}
        private Texture2D dirImage;
        private Texture2D fileImage;
        private Texture2D pictImage;
        private Texture2D copyImage;
        private Texture2D pasteImage;
        private Texture2D plusImage;
        private Texture2D minusImage;
        private Texture2D checkonImage;
        private Texture2D checkoffImage;
        public Texture2D PictImage {
            get { return pictImage ? pictImage : (pictImage = LoadTex("picture")); }
        }
        public Texture2D FileImage {
            get { return fileImage ? dirImage : (fileImage = LoadTex("file")); }
        }
        public Texture2D DirImage {
            get { return dirImage ? dirImage : (dirImage = LoadTex("folder")); }
        }
        public Texture2D CopyImage {
            get { return copyImage ? copyImage : (copyImage = LoadTex("copy")); }
        }
        public Texture2D PasteImage {
            get { return pasteImage ? pasteImage : (pasteImage = LoadTex("paste")); }
        }
        public Texture2D PlusImage {
            get { return plusImage ? plusImage : (plusImage = LoadTex("plus")); }
        }
        public Texture2D MinusImage {
            get { return minusImage ? minusImage : (minusImage = LoadTex("minus")); }
        }
        public Texture2D CheckonImage {
            get { return checkonImage ? checkonImage : (checkonImage = LoadTex("checkon")); }
        }
        public Texture2D CheckoffImage {
            get { return checkoffImage ? checkoffImage : (checkoffImage = LoadTex("checkoff")); }
        }

        private GUIContent checkon;
        public GUIContent Checkon {
            get { return checkon ?? (checkon = new GUIContent(CheckonImage)); }
        }
        private GUIContent checkoff;
        public GUIContent Checkoff {
            get { return checkoff ?? (checkoff = new GUIContent(CheckoffImage)); }
        }

        public Texture2D LoadTex(string name) {
            try {
                using (var fs = asmbl.GetManifestResourceStream(name + ".png")) {
                    var tex2d = fileUtil.LoadTexture(fs);
                    tex2d.name = name;
                    LogUtil.Debug("resource file image loaded :", name);
                    return tex2d;
                }
            } catch(Exception e) {
                LogUtil.Log("アイコンリソースのロードに失敗しました。空として扱います", name, e);
                return new Texture2D(2, 2);
            }
        }

        internal byte[] LoadBytes(string path) {
            try {
                var buffer = new byte[8192];
                using (var fs = asmbl.GetManifestResourceStream(path)) {
                    if (fs != null) {
                        using (var ms = new MemoryStream((int) fs.Length)) {
                            int read;
                            while ((read = fs.Read(buffer, 0, buffer.Length)) > 0) {
                                ms.Write(buffer, 0, read);
                            }
                            return ms.ToArray();
                        }
                    }
                }
            } catch(Exception e) {
                LogUtil.Log("リソースのロードに失敗しました。path=", path, e);
                throw;
            }

            return new byte[0];
        }

        public void Clear() {
            if (pictImage != null)  UnityEngine.Object.DestroyImmediate(pictImage);
            if (dirImage  != null)  UnityEngine.Object.DestroyImmediate(dirImage);
            if (fileImage != null)  UnityEngine.Object.DestroyImmediate(fileImage);
            if (copyImage != null)  UnityEngine.Object.DestroyImmediate(copyImage);
            if (pasteImage != null) UnityEngine.Object.DestroyImmediate(pasteImage);
            if (plusImage  != null) UnityEngine.Object.DestroyImmediate(plusImage);
            if (minusImage != null) UnityEngine.Object.DestroyImmediate(minusImage);
            if (checkonImage != null) UnityEngine.Object.DestroyImmediate(checkonImage);
            if (checkoffImage != null) UnityEngine.Object.DestroyImmediate(checkoffImage);
            pictImage = null;
            dirImage  = null;
            fileImage = null;
            copyImage = null;
            pasteImage = null;
            plusImage  = null;
            minusImage = null;
            checkonImage = null;
            checkoffImage = null;
        }
    }
}
