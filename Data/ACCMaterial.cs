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
    /// Description of ACCMaterial.
    /// </summary>
    public class ACCMaterial {
        const float EPSILON = 0.00001f;
        private const float DEFAULT_FV1 = 10f;
        private const float DEFAULT_FV2 = 1f;
        private const float DEFAULT_FV3 = 1f;
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

        public ACCMaterial(string matName, MaterialType matType) {
            this.name = matName;
            this.type = matType;
            this.shader = matType.shader;

            renderQueue = 2000;
            if (matType.hasColor) color = Color.white;
            if (matType.isLighted) {
                shadowColor = Color.white;
            }
            if (matType.isOutlined) {
                outlineColor = Color.black;
            }
            if (matType.isToony) {
                rimColor = Color.white;
            }
        }
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

            if (type.hasColor) color = m.GetColor("_Color");
            if (type.isLighted) {
                shadowColor = m.GetColor("_ShadowColor");
                shininess = m.GetFloat("_Shininess");
            }
            if (type.isOutlined) {
                outlineColor = m.GetColor("_OutlineColor");
                outlineWidth = m.GetFloat("_OutlineWidth");
            }
            if (type.isToony) {
                rimColor = m.GetColor("_RimColor");
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
                if (!color.HasValue) color = Color.white;
            } else {
                color = null;
            }
            if (matType.isLighted) {
                if (!shadowColor.HasValue) shadowColor = Color.white;
            } else {
                shadowColor = null;
            }
            if (matType.isOutlined) {
                if (!outlineColor.HasValue) outlineColor = Color.black;
            } else {
                outlineColor = null;
            }
            if (matType.isToony) {
                if (!rimColor.HasValue) rimColor = Color.white;
            } else {
                rimColor = null;
            }
            
            // TODO テクスチャ情報の初期化
        }
        public void ReflectTo(Material m) {
            m.SetFloat("_SetManualRenderQueue", renderQueue);
            m.renderQueue = renderQueue;

            if (type.hasColor) {
                m.SetColor("_Color", color.Value);
            }
            if (type.isLighted) {
                m.SetColor("_ShadowColor", shadowColor.Value);
                m.SetFloat("_Shininess", shininess);
            }
            if (type.isOutlined) {
                m.SetColor("_OutlineColor", outlineColor.Value);
                m.SetFloat("_OutlineWidth", outlineWidth);
            }
            if (type.isToony) {
                m.SetColor("_RimColor", rimColor.Value);
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
        private bool equals(float f1, float f2) {
            return Math.Abs(f1- f2) < EPSILON;
        }

        public bool hasChanged(ACCMaterial mate) {
            // 同一シェーダを想定
            
            if (type.hasColor) {
                if (color != mate.color) return true;
            }
            if (type.isLighted) {
                if (shadowColor != mate.shadowColor) return true;
                if (!equals(shininess, mate.shininess)) return true;
            }
            if (type.isOutlined) {
                if (outlineColor != mate.outlineColor) return true;
                if (!equals(outlineWidth, mate.outlineWidth)) return true;
            }
            if (type.isToony) {
                if (rimColor != mate.rimColor) return true;
                if (!equals(rimPower, mate.rimPower) || !equals(rimShift, mate.rimShift)) return true;
            }
            if (type.isHair) {
                if (!equals(hiRate, mate.hiRate) || !equals(hiPow, mate.hiPow)) return true;
            }
            if (type.hasFloat1) {
                if (!equals(floatVal1, mate.floatVal1)) return true;
            }
            if (type.hasFloat2) {
                if (!equals(floatVal2, mate.floatVal2)) return true;
            }
            if (type.hasFloat3) {
                if (!equals(floatVal3, mate.floatVal3)) return true;
            }
            return false;
        }
//        public bool ShaderChanged() {
//            return original != null && (shader != original.shader);
//        }
    }
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
                        switch (propName) {
                        case "_Color":
                            created.color = c;
                            break;
                        case "_ShadowColor":
                            created.shadowColor = c;
                            break;
                        case "_RimColor":
                            created.rimColor = c;
                            break;
                        case "_OutlineColor":
                            created.outlineColor = c;
                            break;
                        }
                            
                        break;
                    case "f":
                        float f = reader.ReadSingle();
                        switch (propName) {
                        case "_Shininess":
                            created.shininess = f;
                            break;
                        case "_OutlineWidth":
                            created.outlineWidth = f;
                            break;
                        case "_RimPower":
                            created.rimPower = f;
                            break;
                        case "_RimShift":
                            created.rimShift = f;
                            break;
                        case "_HiRate":
                            created.hiRate = f;
                            break;
                        case "_HiPow":
                            created.hiPow = f;
                            break;
                        case "_FloatValue1":
                            created.floatVal1 = f;
                            break;
                        case "_FloatValue2":
                            created.floatVal2 = f;
                            break;
                        case "_FloatValue3":
                            created.floatVal3 = f;
                            break;
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
