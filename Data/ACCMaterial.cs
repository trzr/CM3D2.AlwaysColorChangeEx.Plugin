using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using UnityEngine;
using CM3D2.AlwaysColorChangeEx.Plugin;
using CM3D2.AlwaysColorChangeEx.Plugin.UI;
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
        internal static Settings settings = Settings.Instance;

        private readonly string[] emptyEdit = new string[0];
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

        public EditValue renderQueue  = new EditValue(2000f, EditRange.renderQueue);

        public EditColor color        = new EditColor(null, true);
        public EditColor shadowColor  = new EditColor(null);
        public EditColor rimColor     = new EditColor(null);
        public EditColor outlineColor = new EditColor(null);
        
        public EditValue shininess    = new EditValue(0f, EditRange.shininess);
        public EditValue outlineWidth = new EditValue(0.002f, EditRange.outlineWidth);
        public EditValue rimPower     = new EditValue(25f, EditRange.rimPower);
        public EditValue rimShift     = new EditValue(0f, EditRange.rimShift);
        public EditValue hiRate       = new EditValue(0f, EditRange.hiRate);
        public EditValue hiPow        = new EditValue(0.001f, EditRange.hiPow);
        public EditValue floatVal1    = new EditValue(DEFAULT_FV1, EditRange.floatVal1);
        public EditValue floatVal2    = new EditValue(DEFAULT_FV2, EditRange.floatVal2);
        public EditValue floatVal3    = new EditValue(DEFAULT_FV3, EditRange.floatVal3);
        public EditValue cutoff       = new EditValue(0, EditRange.floatVal3);

        public string rqEdit;
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

            this.cutoff = src.cutoff;
        }

        public ACCMaterial(Material m) {
            this.material = m;
            name = m.name;
            type = ShaderMapper.resolve(m.shader.name);
            shader = type.shader;
            renderQueue.Set( m.renderQueue );
            rqEdit = renderQueue.ToString();

            if (type.hasColor) {
                color.Set( m.GetColor(PROP_COLOR) );
            }
            if (type.isLighted) {
                shadowColor.Set( m.GetColor(PROP_SHADOWC) );
                shininess.Set( m.GetFloat("_Shininess") );
            }
            if (type.isOutlined) {
                outlineColor.Set( m.GetColor(PROP_OUTLINEC) );
                outlineWidth.Set( m.GetFloat("_OutlineWidth") );
            }
            if (type.isToony) {
                rimColor.Set( m.GetColor(PROP_RIMC) );

                rimPower.Set( m.GetFloat("_RimPower") );
                rimShift.Set( m.GetFloat("_RimShift") );
            }
            if (type.isHair) {
                hiRate.Set( m.GetFloat("_HiRate") );
                hiPow.Set( m.GetFloat("_HiPow") );
            }
            if (type.hasFloat1) {
                floatVal1.Set( m.GetFloat("_FloatValue1") );
            }
            if (type.hasFloat2) {
                floatVal2.Set( m.GetFloat("_FloatValue2") );
            }
            if (type.hasFloat3) {
                floatVal3.Set( m.GetFloat("_FloatValue3") );
            }
            if (type.hasCutoff) {
                cutoff.Set( m.GetFloat("_Cutoff") );
            }
        }
        public void Update(MaterialType matType) {
            if (this.type == matType) return;
                
            this.type = matType;
            this.shader = matType.shader;
            if (matType.hasColor) {
                if (!color.val.HasValue) {
                    if (material != null) color.Set( material.GetColor(PROP_COLOR) );
                    else {
                        color.Set( (original != null && original.color.val.HasValue) ? original.color.val: Color.white );
                    }
                }
            } else {
                color.Set( null );
            }
            if (matType.isLighted) {
                if (!shadowColor.val.HasValue) {
                    if (material != null) shadowColor.Set( material.GetColor(PROP_SHADOWC) );
                    else shadowColor.Set( (original != null && original.shadowColor.val.HasValue) ? original.shadowColor.val : Color.white );
                }
            } else {
                shadowColor.Set( null );
            }
            if (matType.isOutlined) {
                if (!outlineColor.val.HasValue) {
                    if (material != null) outlineColor.Set( material.GetColor(PROP_OUTLINEC) );
                    else outlineColor.Set( (original != null && original.outlineColor.val.HasValue) ? original.outlineColor.val : Color.black );
                }
            } else {
                outlineColor.Set( null );
            }
            if (matType.isToony) {
                if (!rimColor.val.HasValue) {
                    if (material != null) rimColor.Set( material.GetColor(PROP_RIMC) );
                    else rimColor.Set( (original != null && original.rimColor.val.HasValue) ? original.rimColor.val: Color.white );
                }
            } else {
                rimColor.Set( null );
            }
            // TODO テクスチャ情報の初期化
        }
        public void ReflectTo(Material m) {
            m.SetFloat("_SetManualRenderQueue", renderQueue.val);
            m.renderQueue = (int)renderQueue.val;

            if (type.hasColor) {
                m.SetColor(PROP_COLOR, color.val.Value);
            }
            if (type.isLighted) {
                m.SetColor(PROP_SHADOWC, shadowColor.val.Value);
                m.SetFloat("_Shininess", shininess.val);
            }
            if (type.isOutlined) {
                m.SetColor(PROP_OUTLINEC, outlineColor.val.Value);
                m.SetFloat("_OutlineWidth", outlineWidth.val);
            }
            if (type.isToony) {
                m.SetColor(PROP_RIMC, rimColor.val.Value);
                m.SetFloat("_RimPower", rimPower.val);
                m.SetFloat("_RimShift", rimShift.val);
            }
            if (type.isHair) {
                m.SetFloat("_HiRate", hiRate.val);
                m.SetFloat("_HiPow", hiPow.val);
            }
            if (type.isHair) {
                m.SetFloat("_HiRate", hiRate.val);
                m.SetFloat("_HiPow", hiPow.val);
            }
            if (type.hasFloat1) {
                m.SetFloat("_FloatValue1", floatVal1.val);
            }
            if (type.hasFloat2) {
                m.SetFloat("_FloatValue2", floatVal2.val);
            }
            if (type.hasFloat3) {
                m.SetFloat("_FloatValue3", floatVal3.val);
            }
            if (type.hasCutoff) {
                m.SetFloat("_Cutoff", cutoff.val);
            }
        }

        public bool hasChanged(ACCMaterial mate) {
            // 同一シェーダを想定
            if (type.hasColor) {
                if (color != mate.color) return true;
            }
            if (type.isLighted) {
                if (shadowColor != mate.shadowColor) return true;
                if (!NumberUtil.Equals(shininess.val, mate.shininess.val)) return true;
            }
            if (type.isOutlined) {
                if (outlineColor != mate.outlineColor) return true;
                if (!NumberUtil.Equals(outlineWidth.val, mate.outlineWidth.val)) return true;
            }
            if (type.isToony) {
                if (rimColor != mate.rimColor) return true;
                if (!NumberUtil.Equals(rimPower.val, mate.rimPower.val) || !NumberUtil.Equals(rimShift.val, mate.rimShift.val)) return true;
            }
            if (type.isHair) {
                if (!NumberUtil.Equals(hiRate.val, mate.hiRate.val) || !NumberUtil.Equals(hiPow.val, mate.hiPow.val)) return true;
            }
            if (type.hasFloat1) {
                if (!NumberUtil.Equals(floatVal1.val, mate.floatVal1.val)) return true;
            }
            if (type.hasFloat2) {
                if (!NumberUtil.Equals(floatVal2.val, mate.floatVal2.val)) return true;
            }
            if (type.hasFloat3) {
                if (!NumberUtil.Equals(floatVal3.val, mate.floatVal3.val)) return true;
            }
            if (type.hasCutoff) {
                if (!NumberUtil.Equals(cutoff.val, mate.cutoff.val)) return true;
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
                if (header == FileConst.HEAD_MATE) {
                    return Load(reader);

                } else {
                    if (reader.BaseStream.Position != 0) {                        
                        var msg = LogUtil.Log("指定されたファイルのヘッダが不正です。", header, file);
                        throw new ACCException(msg.ToString());
                    }
                }
            }
            // arc内のファイルがロードできない場合の回避策: Sybaris 0410向け対策. 一括読み込み
            using (var reader = new BinaryReader(new MemoryStream(FileUtilEx.Instance.LoadInternal(file), false), Encoding.UTF8)) {
                string header = reader.ReadString(); // hader
                if (header == FileConst.HEAD_MATE) {
                    return Load(reader);
                } else {
                    var msg = LogUtil.Log("指定されたファイルのヘッダが不正です。", header, file);
                    throw new ACCException(msg.ToString());
                }
            }
        }
        private static ACCMaterialEx Load(BinaryReader reader) {
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
                            created.color.Set( c );
                            break;
                        case PropName._ShadowColor:
                            created.shadowColor.Set( c );
                            break;
                        case PropName._RimColor:
                            created.rimColor.Set( c );
                            break;
                        case PropName._OutlineColor:
                            created.outlineColor.Set( c );
                            break;
                        }
                    } catch(Exception e) {
                        LogUtil.Debug("unsupported propName found", propName, e);
                    }
                    break;
                case "f":
                    float f = reader.ReadSingle();
                    try {
                        var pnf = (PropName)Enum.Parse(typeof(PropName), propName);
                        switch (pnf) {
                        case PropName._Shininess:
                            created.shininess.Set( f );
                            break;
                        case PropName._OutlineWidth:
                            created.outlineWidth.Set( f );
                            break;
                        case PropName._RimPower:
                            created.rimPower.Set( f );
                            break;
                        case PropName._RimShift:
                            created.rimShift.Set( f );
                            break;
                        case PropName._HiRate:
                            created.hiRate.Set( f );
                            break;
                        case PropName._HiPow:
                            created.hiPow.Set( f );
                            break;
                        case PropName._FloatValue1:
                            created.floatVal1.Set( f );
                            break;
                        case PropName._FloatValue2:
                            created.floatVal2.Set( f );
                            break;
                        case PropName._FloatValue3:
                            created.floatVal3.Set( f );
                            break;
                        }
                    } catch(Exception e) {
                        LogUtil.Debug("unsupported propName found", propName, e);
                    }
                    break;
                }
            }
            return created;           
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
                                    outUtil.Write(writer, mate.color.val.Value);
                                    break;
                                case PropName._ShadowColor:
                                    outUtil.Write(writer, mate.shadowColor.val.Value);
                                    break;
                                case PropName._OutlineColor:
                                    outUtil.Write(writer, mate.outlineColor.val.Value);
                                    break;
                                case PropName._RimColor:
                                    outUtil.Write(writer, mate.rimColor.val.Value);
                                    break;
                            }
                            break;
                        case PropType.f:
                            switch (propName) {
                                case PropName._Shininess:
                                    writer.Write(mate.shininess.val);
                                    break;
                                case PropName._OutlineWidth:
                                    writer.Write(mate.outlineWidth.val);
                                    break;
                                case PropName._RimPower:
                                    writer.Write(mate.rimPower.val);
                                    break;
                                case PropName._RimShift:
                                    writer.Write(mate.rimShift.val);
                                    break;
                                case PropName._HiRate:
                                    writer.Write(mate.hiRate.val);
                                    break;
                                case PropName._HiPow:
                                    writer.Write(mate.hiPow.val);
                                    break;
                                case PropName._FloatValue1:
                                    writer.Write(mate.floatVal1.val);
                                    break;
                                case PropName._FloatValue2:
                                    writer.Write(mate.floatVal2.val);
                                    break;
                                case PropName._FloatValue3:
                                    writer.Write(mate.floatVal3.val);
                                    break;
                            }
                            break;
                    }
                                                         
             });

        }
    }
}
