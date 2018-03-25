using System;
using UnityEngine;
using CM3D2.AlwaysColorChangeEx.Plugin.Util;

namespace CM3D2.AlwaysColorChangeEx.Plugin.Data {
    /// <summary>
    /// </summary>
    public class ACCTexture {
        public const int RAMP        = 1;
        public const int SHADOW_RATE = 2;
        public const int NONE        = 0;
        public Texture tex;

        public ACCTexture original { get; private set;}

        //public MaterialType type;
        public ShaderType type;
        public ShaderPropTex prop;
        public PropKey propKey;
        public string propName;
        public string editname = string.Empty;
        public string filepath;
        public Vector2 texOffset = Vector2.zero;
        public Vector2 texScale = Vector2.one;
        public int toonType;
        public bool dirty;
        
        
        private ACCTexture(PropKey propKey) {
            this.propKey = propKey;
            propName = propKey.ToString();

            switch (propKey) {
            case PropKey._ToonRamp:
                toonType = RAMP;
                break;
            case PropKey._ShadowRateToon:
                toonType = SHADOW_RATE;
                break;
            }
        }
        protected ACCTexture(string propName) {
            this.propName = propName;
            propKey = (PropKey)Enum.Parse(typeof(PropKey), propName);
            switch (propKey) {
            case PropKey._ToonRamp:
                toonType = RAMP;
                break;
            case PropKey._ShadowRateToon:
                toonType = SHADOW_RATE;
                break;
            }
        }
        public ACCTexture(Texture tex, Material mate, ShaderPropTex texProp, ShaderType type) :this(texProp.key) {
            this.tex = tex;
            this.type = type;
            prop = texProp;

            editname = tex.name;
            if (tex is Texture2D) {
               texOffset = mate.GetTextureOffset(propName);
               texScale  = mate.GetTextureScale(propName);
            } else {
                LogUtil.DebugF("propName({0}): texture type:{1}", propName, tex.GetType());
            }
            
//            } else {
//                // シェーダ切り替えなどで、元々存在しないテクスチャの場合
//                LogUtil.DebugF("texture not found. propname={0}, material={1}", propName, mate.name);
//                // 空のテクスチャは作成しない
////                this.tex = new Texture2D(2, 2);
////                this.tex.name = string.Empty;
////                // テクスチャを追加セット
////                mate.SetTexture(propName, this.tex);
//            }
        }
        public static ACCTexture Create(Material mate, ShaderPropTex texProp, ShaderType type) {
            var tex = mate.GetTexture(texProp.propId);
            
            return tex == null ? null : new ACCTexture(tex, mate, texProp, type);
        }

        public ACCTexture(ACCTexture src) {
            original  = src;
            propName  = src.propName;
            type      = src.type;
            prop      = src.prop;
            propKey   = src.propKey;

            editname  = src.editname;
            filepath  = src.filepath;
            texOffset = src.texOffset;
            texScale  = src.texScale;

            toonType  = src.toonType;
        }

        public bool SetName(string name) {
            if (string.Equals(editname, name, StringComparison.CurrentCultureIgnoreCase)) return false;
            editname = name;
            dirty = true;
            return true;
        }
    }

    public class ACCTextureEx : ACCTexture {
        public string txtpath;

        public ACCTextureEx(string propName) : base(propName) {}

    }
}
