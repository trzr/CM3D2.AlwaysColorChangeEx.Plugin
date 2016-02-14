/*
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using CM3D2.AlwaysColorChangeEx.Plugin;
using CM3D2.AlwaysColorChangeEx.Plugin.Util;

namespace CM3D2.AlwaysColorChangeEx.Plugin.Data
{
    /// <summary>
    /// マテリアルの変更情報を扱うデータクラス.
    /// スライダー操作中のデータを保持する.
    /// 
    /// CCMaterialと重複有り.　統合可能ならしたいが… 
    /// </summary>
    public class ACCMaterial {
        private const float DEFAULT_FV1 = 10f;
        private const float DEFAULT_FV2 = 1f;
        private const float DEFAULT_FV3 = 1f;
        public static readonly string PROP_COLOR    = PropName._Color.ToString();
        public static readonly string PROP_SHADOWC  = PropName._ShadowColor.ToString();
        public static readonly string PROP_OUTLINEC = PropName._OutlineColor.ToString();
        public static readonly string PROP_RIMC     = PropName._RimColor.ToString();

        public ACCMaterial original {get; private set;}
        public Material material;
        public string name;
        public ShaderName shader;
        public MaterialType type;
        public int renderQueue;
        public Color? color;
        public Color? shadowColor;
        public Color? rimColor;
        public Color? outlineColor;
        public float shininess;
        public float outlineWidth = 0.002f;
        public float rimPower = 25f;
        public float rimShift;
        public float hiRate;
        public float hiPow = 0.001f;
        public float floatVal1 = DEFAULT_FV1;
        public float floatVal2 = DEFAULT_FV2;
        public float floatVal3 = DEFAULT_FV3;

        protected ACCMaterial() {}

        public ACCMaterial(ACCMaterial src) {
            this.original = src;
            this.material = src.material;
            this.name = src.name;
            this.shader = src.shader;
            this.type = src.type;
            this.renderQueue = src.renderQueue;
            this.color = src.color;
            this.shadowColor = src.shadowColor;
            this.rimColor = src.rimColor;
            this.outlineColor = src.outlineColor;
            this.shininess = src.shininess;
            this.outlineWidth = src.outlineWidth;
            this.rimPower = src.rimPower;
            this.rimShift = src.rimShift;
            this.hiRate = src.hiRate;
            this.hiPow = src.hiPow;
            this.floatVal1 = src.floatVal1;
            this.floatVal2 = src.floatVal2;
            this.floatVal3 = src.floatVal3;
        }

        public ACCMaterial(Material m) {
            this.material = m;
            name = m.name;
            type = ShaderMapper.resolve(m.shader.name);
            shader = type.shader;
            renderQueue = m.renderQueue;

            if (type.hasColor) color = m.GetColor(PROP_COLOR);
            if (type.isLighted) {
                shadowColor = m.GetColor(PROP_SHADOWC);
                shininess = m.GetFloat("_Shininess");
            }
            if (type.isOutlined) {
                outlineColor = m.GetColor(PROP_OUTLINEC);
                outlineWidth = m.GetFloat("_OutlineWidth");
            }
            if (type.isToony) {
                rimColor = m.GetColor(PROP_RIMC);
                rimPower = m.GetFloat("_RimPower");
                rimShift = m.GetFloat("_RimShift");
            }
            if (type.isHair) {
                hiRate = m.GetFloat("_HiRate");
                hiPow = m.GetFloat("_HiPow");
            }
            if (type.hasFloat1) {
                floatVal1 = m.GetFloat("_FloatValue1");
            }
            if (type.hasFloat2) {
                floatVal2 = m.GetFloat("_FloatValue2");
            }
            if (type.hasFloat3) {
                floatVal3 = m.GetFloat("_FloatValue3");
            }
        }
        public void Update(MaterialType matType) {
            if (this.type == matType) return;
                
            this.type = matType;
            this.shader = matType.shader;
            if (matType.hasColor) {
                if (!color.HasValue) {
                    if (material != null) color = material.GetColor(PROP_COLOR);
                    else {
                        color = (original != null && original.color.HasValue) ? original.color: Color.white;
                    }
                }
            } else {
                color = null;
            }
            if (matType.isLighted) {
                if (!shadowColor.HasValue) {
                    if (material != null) shadowColor = material.GetColor(PROP_SHADOWC);
                    else shadowColor = (original != null && original.shadowColor.HasValue) ? original.shadowColor : Color.white;
                }
            } else {
                shadowColor = null;
            }
            if (matType.isOutlined) {
                if (!outlineColor.HasValue) {
                    if (material != null) outlineColor = material.GetColor(PROP_OUTLINEC);
                    else outlineColor = (original != null && original.outlineColor.HasValue) ? original.outlineColor : Color.black;
                }
            } else {
                outlineColor = null;
            }
            if (matType.isToony) {
                if (!rimColor.HasValue) {
                    if (material != null) rimColor = material.GetColor(PROP_RIMC);
                    else rimColor = (original != null && original.rimColor.HasValue) ? original.rimColor: Color.white;
                }
            } else {
                rimColor = null;
            }
            // TODO テクスチャ情報の初期化
        }
        public void ReflectTo(Material m) {
            m.SetFloat("_SetManualRenderQueue", renderQueue);
            m.renderQueue = renderQueue;

            if (type.hasColor) {
                m.SetColor(PROP_COLOR, color.Value);
            }
            if (type.isLighted) {
                m.SetColor(PROP_SHADOWC, shadowColor.Value);
                m.SetFloat("_Shininess", shininess);
            }
            if (type.isOutlined) {
                m.SetColor(PROP_OUTLINEC, outlineColor.Value);
                m.SetFloat("_OutlineWidth", outlineWidth);
            }
            if (type.isToony) {
                m.SetColor(PROP_RIMC, rimColor.Value);
                m.SetFloat("_RimPower", rimPower);
                m.SetFloat("_RimShift", rimShift);
            }
            if (type.isHair) {
                m.SetFloat("_HiRate", hiRate);
                m.SetFloat("_HiPow", hiPow);
            }
            if (type.isHair) {
                m.SetFloat("_HiRate", hiRate);
                m.SetFloat("_HiPow", hiPow);
            }
            if (type.hasFloat1) {
                m.SetFloat("_FloatValue1", floatVal1);
            }
            if (type.hasFloat2) {
                m.SetFloat("_FloatValue2", floatVal2);
            }
            if (type.hasFloat3) {
                m.SetFloat("_FloatValue3", floatVal3);
            }
        }

        public bool hasChanged(ACCMaterial mate) {
            // 同一シェーダを想定
            if (type.hasColor) {
                if (color != mate.color) return true;
            }
            if (type.isLighted) {
                if (shadowColor != mate.shadowColor) return true;
                if (!NumberUtil.Equals(shininess, mate.shininess)) return true;
            }
            if (type.isOutlined) {
                if (outlineColor != mate.outlineColor) return true;
                if (!NumberUtil.Equals(outlineWidth, mate.outlineWidth)) return true;
            }
            if (type.isToony) {
                if (rimColor != mate.rimColor) return true;
                if (!NumberUtil.Equals(rimPower, mate.rimPower) || !NumberUtil.Equals(rimShift, mate.rimShift)) return true;
            }
            if (type.isHair) {
                if (!NumberUtil.Equals(hiRate, mate.hiRate) || !NumberUtil.Equals(hiPow, mate.hiPow)) return true;
            }
            if (type.hasFloat1) {
                if (!NumberUtil.Equals(floatVal1, mate.floatVal1)) return true;
            }
            if (type.hasFloat2) {
                if (!NumberUtil.Equals(floatVal2, mate.floatVal2)) return true;
            }
            if (type.hasFloat3) {
                if (!NumberUtil.Equals(floatVal3, mate.floatVal3)) return true;
            }
            return false;
        }
//        public bool ShaderChanged() {
//            return original != null && (shader != original.shader);
//        }
    }
    /// <summary>
    /// エクスポート機能用の機能を拡張したデータクラス
    /// </summary>
    public class ACCMaterialEx : ACCMaterial {
        private static readonly FileUtilEx outUtil = FileUtilEx.Instance;
        public Dictionary<string, ACCTextureEx> texDic = new Dictionary<string, ACCTextureEx>(5);
        public string name1;
        public string name2;
        private ACCMaterialEx() : base() { }

        public static ACCMaterialEx Load(string file) {

            using ( var reader = new BinaryReader(FileUtilEx.Instance.GetStream(file), Encoding.UTF8)) {
                string header = reader.ReadString(); // hader
                if (header != FileConst.HEAD_MATE) {
                    var msg = LogUtil.Log("指定されたファイルのヘッダが不正です。", header, file);
                    throw new ACCException(msg.ToString());
                }
                var created = new ACCMaterialEx();
                int version = reader.ReadInt32();
                created.name1 = reader.ReadString();
                created.name2 = reader.ReadString();
                string shaderName1 = reader.ReadString();
                created.type = ShaderMapper.resolve(shaderName1);
                created.shader = created.type.shader;
                
                string shaderName2 = reader.ReadString();

                while(true) {
                    string type = reader.ReadString();
                    if (type == "end") break;

                    string propName = reader.ReadString();
                    switch (type) {
                        case "tex":
                            string sub = reader.ReadString();
                            switch (sub) {
                            case "tex2d":
                                var tex = new ACCTextureEx(propName);
                                tex.editname = reader.ReadString();
                                tex.txtpath  = reader.ReadString();
                                tex.texOffset = new Vector2(reader.ReadSingle(),
                                                            reader.ReadSingle());
                                tex.texScale  = new Vector2(reader.ReadSingle(),
                                                            reader.ReadSingle());                                
                                created.texDic[propName] = tex;
                                break;
                            case "null":
                                break;
                            case "texRT":
                                reader.ReadString();
                                reader.ReadString();
                                break;
                        }
                        break;
                    case "col":
                    case "vec":
                        var c = new Color(reader.ReadSingle(), reader.ReadSingle(),
                                          reader.ReadSingle(), reader.ReadSingle());
                        try {
                            var pnc = (PropName)Enum.Parse(typeof(PropName), propName);
                            switch (pnc) {
                            case PropName._Color:
                                created.color = c;
                                break;
                            case PropName._ShadowColor:
                                created.shadowColor = c;
                                break;
                            case PropName._RimColor:
                                created.rimColor = c;
                                break;
                            case PropName._OutlineColor:
                                created.outlineColor = c;
                                break;
                            }
                        } catch(Exception e) {
                            LogUtil.DebugLog("unsupported propName found", propName, e);
                        }
                        break;
                    case "f":
                        float f = reader.ReadSingle();
                        try {
                            var pnf = (PropName)Enum.Parse(typeof(PropName), propName);
                            switch (pnf) {
                            case PropName._Shininess:
                                created.shininess = f;
                                break;
                            case PropName._OutlineWidth:
                                created.outlineWidth = f;
                                break;
                            case PropName._RimPower:
                                created.rimPower = f;
                                break;
                            case PropName._RimShift:
                                created.rimShift = f;
                                break;
                            case PropName._HiRate:
                                created.hiRate = f;
                                break;
                            case PropName._HiPow:
                                created.hiPow = f;
                                break;
                            case PropName._FloatValue1:
                                created.floatVal1 = f;
                                break;
                            case PropName._FloatValue2:
                                created.floatVal2 = f;
                                break;
                            case PropName._FloatValue3:
                                created.floatVal3 = f;
                                break;
                            }
                        } catch(Exception e) {
                            LogUtil.DebugLog("unsupported propName found", propName, e);
                        }
                        break;
                    }
                }
                return created;            
            }
        }
        public static void Write(string filepath, ACCMaterialEx mate) {
            using ( var writer = new BinaryWriter(File.OpenWrite(filepath)) ) {
                Write(writer, mate);
            }
        }
        public static void Write(BinaryWriter writer, ACCMaterialEx mate) {
            writer.Write(FileConst.HEAD_MATE);
            writer.Write(1000); // version

            writer.Write(mate.name1);
            writer.Write(mate.name2);

            var shaderName1 = mate.type.shader.Name;
            writer.Write(shaderName1);
            var shaderName2 = ShaderMapper.GatShader2(shaderName1);
            writer.Write(shaderName2);

            EnumExt<PropName>.Exec(
                propName => {
                    if ( !mate.type.IsValidProp(propName) ) return;

                    var propNameString = propName.ToString();

                    var type = ShaderMapper.GetType(propName);
                    writer.Write(type.ToString());
                    writer.Write(propNameString);
                    switch (type) {
                        case PropType.tex:
                            if (propName == PropName._RenderTex) {
                                writer.Write("null");
                            } else {
                                writer.Write("tex2d");
                                var tex = mate.texDic[propNameString];
                                writer.Write( tex.editname );
                                writer.Write( tex.txtpath );

                                outUtil.Write(writer,  tex.texOffset.Value);
                                outUtil.Write(writer,  tex.texScale.Value);
                            }
                            break;
                        case PropType.col:
                            switch (propName) {
                                case PropName._Color:
                                    outUtil.Write(writer, mate.color.Value);
                                    break;
                                case PropName._ShadowColor:
                                    outUtil.Write(writer, mate.shadowColor.Value);
                                    break;
                                case PropName._OutlineColor:
                                    outUtil.Write(writer, mate.outlineColor.Value);
                                    break;
                                case PropName._RimColor:
                                    outUtil.Write(writer, mate.rimColor.Value);
                                    break;
                            }
                            break;
                        case PropType.f:
                            switch (propName) {
                                case PropName._Shininess:
                                    writer.Write(mate.shininess);
                                    break;
                                case PropName._OutlineWidth:
                                    writer.Write(mate.outlineWidth);
                                    break;
                                case PropName._RimPower:
                                    writer.Write(mate.rimPower);
                                    break;
                                case PropName._RimShift:
                                    writer.Write(mate.rimShift);
                                    break;
                                case PropName._HiRate:
                                    writer.Write(mate.hiRate);
                                    break;
                                case PropName._HiPow:
                                    writer.Write(mate.hiPow);
                                    break;
                                case PropName._FloatValue1:
                                    writer.Write(mate.floatVal1);
                                    break;
                                case PropName._FloatValue2:
                                    writer.Write(mate.floatVal2);
                                    break;
                                case PropName._FloatValue3:
                                    writer.Write(mate.floatVal3);
                                    break;
                            }
                            break;
                    }
                                                         
             });

        }
    }
}
