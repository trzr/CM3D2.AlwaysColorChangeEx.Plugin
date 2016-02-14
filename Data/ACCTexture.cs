/*
 */
using System;
using UnityEngine;
using CM3D2.AlwaysColorChangeEx.Plugin.Util;

namespace CM3D2.AlwaysColorChangeEx.Plugin.Data
{
    /// <summary>
    /// </summary>
    public class ACCTexture {
        public const int RAMP        = 1;
        public const int SHADOW_RATE = 2;
        public const int NONE        = 0;
        public Texture tex;

        public ACCTexture original { get; private set;}

        public MaterialType type;
        public string propName;
        public string editname = string.Empty;
        public string filepath;
        public Vector2? texOffset;
        public Vector2? texScale;
        public int toonType;
        public bool dirty;
        
        public ACCTexture(string propName) {
            this.propName = propName;

            if (propName == PropName._ToonRamp.ToString() ) {
                toonType = RAMP;
            } else if (propName == PropName._ShadowRateToon.ToString() ) {
                toonType = SHADOW_RATE;
            }
        }
        public ACCTexture(Material mate, string propName, MaterialType type) :this(propName) {
            this.type = type;
            this.tex = mate.GetTexture(propName);

            if (tex != null) {
                this.editname = tex.name;
                if (tex is Texture2D) {
                   texOffset = mate.GetTextureOffset(propName);
                   texScale  = mate.GetTextureScale(propName);
                } else {
                    LogUtil.DebugLogF("propName({0}): texture type:{1}", propName, tex.GetType());
                }
            } else {
                // シェーダ切り替えなどで、元々存在しないテクスチャの場合
                LogUtil.DebugLogF("texture not found. propname={0}, material={1}", propName, mate.name);
                // 空のテクスチャは作成しない
//                this.tex = new Texture2D(2, 2);
//                this.tex.name = string.Empty;
//                // テクスチャを追加セット
//                mate.SetTexture(propName, this.tex);
            }
        }

        public ACCTexture(ACCTexture src) {
            this.original  = src;
            this.propName  = src.propName;
            this.type      = src.type;

            this.editname  = src.editname;
            this.filepath  = src.filepath;
            this.texOffset = src.texOffset;
            this.texScale  = src.texScale;

            this.toonType  = src.toonType;
        }

        public bool SetName(string name) {
            if (this.editname.ToLower() != name.ToLower()) {
                this.editname = name;
                this.dirty = true;
                return true;
            }
            return false;
        }
    }

    public class ACCTextureEx : ACCTexture {
        public string txtpath;

        public ACCTextureEx(string propName) : base(propName) {}

    }
}
