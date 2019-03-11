using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CM3D2.AlwaysColorChangeEx.Plugin.Util;
using UnityEngine;

namespace CM3D2.AlwaysColorChangeEx.Plugin.Data {
    /// <summary>
    /// マテファイルとテキスト変換を行うハンドラクラス
    /// </summary>
    public class MateHandler {
        private static readonly MateHandler INSTANCE = new MateHandler();        
        public static MateHandler Instance {
            get { return INSTANCE;  }
        }
        public static readonly int MATE_SHADER = 0x1;
        public static readonly int MATE_COLOR  = 0x2;
        public static readonly int MATE_FLOAT  = 0x4;
        public static readonly int MATE_TEX    = 0x8;
        public static readonly int MATE_ALL    = 0xf;

        private static readonly Settings settings = Settings.Instance;
        public string filepath;
        public int bufferSize = 8192;
        public string Read(string path=null) {
            if (path == null) {
                path = filepath;
            }
            using (var stream = new BufferedStream(new FileStream(path, FileMode.Open, FileAccess.Read), bufferSize)) {
                using (var reader = new BinaryReader(stream, Encoding.UTF8)) {
                    var header = reader.ReadString(); // hader
                    if (header == "CM3D2_MATERIAL") return Read(reader).ToString();

                    var msg = "正しいmateファイルではありません。ヘッダが不正です。"+ header+ ", file="+ path;
                    throw new Exception(msg);
                }
            }
        }
        private StringBuilder Read(BinaryReader reader) {
            var buff = new StringBuilder(8192);
            
            buff.Append(reader.ReadInt32()).Append("\r\n");
            buff.Append(reader.ReadString()).Append("\r\n"); // name1
            buff.Append(reader.ReadString()).Append("\r\n"); // name2
            buff.Append(reader.ReadString()).Append("\r\n"); // shader1
            buff.Append(reader.ReadString()).Append("\r\n\r\n"); // shader2
            
            while(true) {
                var type = reader.ReadString();
                //writer.Write(type);
                if (type == "end") break;

                var propName = reader.ReadString();
                buff.Append(type).Append("\r\n");
                buff.Append('\t').Append(propName).Append("\r\n");
                switch (type) {
                case "tex":
                    var sub = reader.ReadString();
                    buff.Append('\t').Append(sub).Append("\r\n");
                    switch (sub) {
                    case "tex2d":
                        buff.Append('\t').Append(reader.ReadString()).Append("\r\n");
                        buff.Append('\t').Append(reader.ReadString()).Append("\r\n");
                        buff.Append('\t').Append(reader.ReadSingle())
                            .Append(' ').Append(reader.ReadSingle())
                            .Append(' ').Append(reader.ReadSingle())
                            .Append(' ').Append(reader.ReadSingle()).Append("\r\n");
                        break;
                    case "null":
                        break;
                    case "texRT":
                        buff.Append('\t').Append(reader.ReadString()).Append("\r\n");
                        buff.Append('\t').Append(reader.ReadString()).Append("\r\n");
                        break;
                    }
                    break;
     
                case "col":
                case "vec":
                    buff.Append('\t').Append(reader.ReadSingle())
                        .Append(' ').Append(reader.ReadSingle())
                        .Append(' ').Append(reader.ReadSingle())
                        .Append(' ').Append(reader.ReadSingle()).Append("\r\n");
                    break;
                case "f":
                    buff.Append('\t').Append(reader.ReadSingle()).Append("\r\n");
                    break;
                }
            }

            return buff;
        }

        public void Write(string mateText, string path=null) {
            if (path == null) {
                path = filepath;
            }
            using (var stream = new BufferedStream(new FileStream(path, FileMode.CreateNew, FileAccess.Write), bufferSize)) {
                using (var writer = new BinaryWriter(stream, Encoding.UTF8)) {
                    Write(mateText, writer);
                }
            }
        }

        private string ReadLine(TextReader reader) {
            var line = reader.ReadLine();
            if (line == null) throw new Exception("マテリアルを表すファイルの書式が正しくありません");
            return line;
        }

        public void Write(string mateText, BinaryWriter writer) {
            writer.Write("CM3D2_MATERIAL");
            var sr = new StringReader(mateText);

            
            int version;
            var line = sr.ReadLine();
            if (line == null || !int.TryParse(line.Trim(), out version)) {
                throw new Exception("バージョンが取得できません");
            }

            writer.Write(version);
            writer.Write(ReadLine(sr).Trim()); // name1
            writer.Write(ReadLine(sr).Trim()); // name2

            writer.Write(ReadLine(sr).Trim()); // shader1
            writer.Write(ReadLine(sr).Trim()); // shader2
            
            while ((line = sr.ReadLine()) != null) {
                line = line.Trim();
                if (line.Length == 0) continue;
                
                writer.Write(line);
                var propName = ReadLine(sr).Trim();
                writer.Write(propName);
                switch (line) {
                case "tex":
                    var sub = ReadLine(sr).Trim();
                    writer.Write(sub);
                    switch (sub) {
                    case "tex2d":
                        writer.Write(ReadLine(sr).Trim());
                        writer.Write(ReadLine(sr).Trim());
                        var vals = ReadLine(sr).Split(' ');
                        if (vals.Length != 4) {
                            throw new Exception("オフセット、スケール値が正しく（４値）指定されていません。propName=" + propName);                                
                        }
                        
                        for (var i=0; i<4; i++) {
                            float f;
                            if ( float.TryParse(vals[i], out f) ) {
                                writer.Write(f);
                            } else {
                                throw new Exception("オフセット、スケール値をfloatに変換できません。propName=" + propName);
                            }
                        }
                        break;
                    case "null":
                        break;
                    case "texRT":
                        writer.Write(ReadLine(sr).Trim());
                        writer.Write(ReadLine(sr).Trim());
                        break;
                    }
                    break;
                case "col":
                case "vec":
                    var colVals = ReadLine(sr).Split(' ');
                    if (colVals.Length != 4) {
                        throw new Exception("Color値の指定が正しく（４値）指定されていません。propName=" + propName);                                
                    }
                    foreach (var colVal in colVals) {
                        float f;
                        if ( float.TryParse(colVal, out f) ) {
                            writer.Write(f);
                        } else {
                            throw new Exception("color値をfloatに変換できません。propName=" + propName);
                        }
                    }                    
                    break;
                case "f":
                    var fStr = ReadLine(sr).Trim();
                    float fVal;
                    if ( float.TryParse(fStr, out fVal) ) {
                        writer.Write(fVal);
                    } else {
                        throw new Exception("f値をfloatに変換できません。propName=" + propName);
                    }
                    break;
                }
            }
            writer.Write("end");
        }

        public string ToText(ACCMaterial target) {
            var mate = target.material;
            var buff = new StringBuilder();
            // ゲーム中にバージョン、name1を保持していないので制限事項
            // mateファイルの特定も少し手間が掛かるため、まずは暫定処置
            buff.Append("1000\r\n");
            buff.Append(target.name.ToLower()).Append("\r\n");
            buff.Append(target.name).Append("\r\n");
            var shaderName = target.type.name;
            buff.Append(shaderName).Append("\r\n");
            buff.Append(ShaderType.GetMateName(shaderName)).Append("\r\n\r\n");
            
            var type = target.type;
            // tex
            foreach (var texProp in type.texProps) {
                buff.Append("tex\r\n");
                var propName = texProp.keyName;
                buff.Append('\t').Append(propName).Append("\r\n");
                var tex = mate.GetTexture(texProp.propId);
                if (tex == null) {
                    buff.Append("\tnull\r\n");
                } else {
                    buff.Append("\ttex2d\r\n");
                    buff.Append('\t').Append(tex.name).Append("\r\n"); // tex name
                    // なんちゃってテキストパス (これもゲーム中にデータが残らないため）
                    buff.Append('\t').Append(settings.txtPrefixTex).Append(tex.name).Append(".png\r\n"); // tex path
                    // 
                    var offset = mate.GetTextureOffset(propName);
                    var scale = mate.GetTextureScale(propName);
                    buff.Append("\t").Append(offset.x).Append(' ').Append(offset.y)
                        .Append(' ').Append(scale.x).Append(' ').Append(scale.y).Append("\r\n");
                }
            }
            // col
            foreach (var colProp in type.colProps) {
                buff.Append("col\r\n");
                var propName = colProp.keyName;
                buff.Append('\t').Append(propName).Append("\r\n");
                var color = mate.GetColor(propName);
                buff.Append('\t').Append(color.r).Append(' ')
                    .Append(color.g).Append(' ')
                    .Append(color.b).Append(' ')
                    .Append(color.a).Append("\r\n");
            }            
       
            // f
            foreach (var prop in type.fProps) {
                buff.Append("f\r\n");
                var propName = prop.keyName;
                buff.Append('\t').Append(propName).Append("\r\n");
                var fVal = mate.GetFloat(propName);
                buff.Append('\t').Append(fVal).Append("\r\n");
            }            
            
            return buff.ToString();        
        }

        public static bool IsParsable(string text) {
            using (var sr = new StringReader(text)) {
                var line = sr.ReadLine();
                if (string.IsNullOrEmpty(line)) return false;
    
                int version;
                if (!int.TryParse(line, out version)) return false;
                
                var val = sr.ReadLine();// name1
                if (val == null || val.Trim().Length == 0 || val[0] == '\t') return false;
                
                val = sr.ReadLine(); // name2
                if (val == null || val.Trim().Length == 0 || val[0] == '\t') return false;

                val = sr.ReadLine(); // shader1
                if (val == null || val.Trim().Length == 0 || val[0] == '\t') return false;

                val = sr.ReadLine(); // shader2
                if (val == null || val.Trim().Length == 0 || val[0] == '\t') return false;

                return true;
            }
        }
        
        public bool Write(ACCMaterial target, string mateText) {
            return Write(target, mateText, MATE_ALL);
        }

        public bool Write(ACCMaterial target, string mateText, int apply) {
            using (var sr = new StringReader(mateText)) {
                var outUtil = FileUtilEx.Instance;
                
                sr.ReadLine(); // version
                var mate = target.material;
    
                // 改良案) マテリアル名を変更か、あるいはマテリアル名に対応するRenderQueueを設定
                sr.ReadLine(); // name1
                sr.ReadLine(); // name2
    
                var shader = sr.ReadLine(); // shader1
                sr.ReadLine(); // shader2
                if ((apply & MATE_SHADER) > 0) {
                    if (mate.shader.name != shader) {
                        target.ChangeShader(shader);
                    }
                }            
                var shaderType = target.type;
                
                var line = sr.ReadLine();
                var work = new List<string>();
                while (line != null) {
                    if (line.Length != 0 && line[0] != '\t') {
                        work.Clear();
                        // 次のタイプまで読み込み
                        var type = line.Trim();
                        while ((line = sr.ReadLine()) != null) {
                            if (line.Length == 0)
                                continue;
                            if (line[0] == '\t') {
                                var val = line.Trim();
                                if (val.Length > 0)
                                    work.Add(val);
                                
                            } else {
                                break;
                            }
                        }
                        if (work.Count == 0) continue;
                        
                        switch (type) {
                            case "tex":
                                if ((apply & MATE_TEX) > 0) {
                                    if (work.Count == 2) {
                                        if (work[1] == "null") {
                                            mate.SetTexture(work[0], null);
                                        }
                                        continue;
                                        
                                    } else if (work.Count < 5)  {
                                        var tmp = string.Empty;
                                        if (work.Count >= 1) {
                                            tmp = "propName="+work[0];
                                        }
                                        LogUtil.Log("指定パラメータが不足しているためtexの適用をスキップします.", tmp);
                                        continue;
                                    }
                                    var propName1 = work[0];
                                    var texName  = work[2];
                                    var texKey = shaderType.GetShaderProp(propName1);
                                    if (texKey == null) {
                                        LogUtil.Log("シェーダに対応していないプロパティのためスキップします.propName=", propName1);
                                        continue;
                                    }
                                    // tex名が同一の場合はは変更しない
                                    var prevTex = mate.GetTexture(texKey.propId);
                                    if (prevTex == null || prevTex.name != texName) {

                                        if (!texName.ToLower().EndsWith(FileConst.EXT_TEXTURE, StringComparison.Ordinal)) {
                                            texName += FileConst.EXT_TEXTURE;
                                        }

                                        if (!outUtil.Exists(texName)) {
                                            LogUtil.LogF("tex({0}) not found. (propName={1})", texName, propName1);
                                            continue;
                                        }

                                        // テクスチャの適用
                                        var tex2D = outUtil.LoadTexture(texName);
                                        mate.SetTexture(texKey.propId, tex2D);
                                    }
                                    var fvals = ParseVals(work[4], propName1, 4);
                                    if (fvals == null) {
                                        LogUtil.DebugF("tex({0}) prop is null", texName);
                                        continue;
                                    }
#if UNITY_5_6_OR_NEWER
                                    mate.SetTextureOffset(texKey.propId, new Vector2(fvals[0], fvals[1]));
                                    mate.SetTextureScale(texKey.propId, new Vector2(fvals[2], fvals[3]));
#else
                                    mate.SetTextureOffset(texKey.keyName, new Vector2(fvals[0], fvals[1]));
                                    mate.SetTextureScale(texKey.keyName, new Vector2(fvals[2], fvals[3]));
#endif
                                    LogUtil.DebugF("tex({0}) loaded to {1}", texName, texKey.keyName);
                                }
                                break;

                            case "col":
                            case "vec":
                                if ((apply & MATE_COLOR) > 0) {
                                    if (work.Count < 2)
                                        continue;

                                    var propName2 = work[0];
                                    var texKey2 = shaderType.GetShaderProp(propName2);
                                    if (texKey2 == null) {
                                        LogUtil.Log("シェーダに対応していないプロパティのためスキップします.propName=", propName2);
                                        continue;
                                    }
                                    var colVals = ParseVals(work[1], propName2, 4);
                                    if (colVals == null)
                                        continue;

                                    var color = new Color(colVals[0], colVals[1], colVals[2], colVals[3]);
                                    mate.SetColor(texKey2.propId, color);
                                    LogUtil.DebugF("color set ({0})", propName2);
                                }
                                break;

                            case "f":
                                if ((apply & MATE_FLOAT) > 0) {
                                    if (work.Count < 2)
                                        continue;

                                    var propName3 = work[0];
                                    var texKey3 = shaderType.GetShaderProp(propName3);
                                    if (texKey3 == null) {
                                        LogUtil.Log("シェーダに対応していないプロパティのためスキップします.propName=", propName3);
                                        continue;
                                    }
                                    float fVal;
                                    if (!float.TryParse(work[1], out fVal)) {
                                        LogUtil.Log("指定文字列はfloatに変換できません。スキップします。propName={0}, text={1}", propName3, work[1]);
                                        continue;
                                    }
                                    
                                    mate.SetFloat(texKey3.propId, fVal);
                                    LogUtil.DebugF("float set({0})", propName3);
                                }
                                break;
                        }
                        
                    } else {
                        line = sr.ReadLine();
                    }
                }
            }
            return true;
        }

        private float[] ParseVals(string text, string propName = null, int count=4) {
            var vals = text.Split(' ');
            if (vals.Length < count) {
                LogUtil.LogF("float値が正しく（{0}個）指定されていません。スキップします。propName={1}", count, propName);
                return null;
            }
            var fvals = new float[count];
            for (var i=0; i<count; i++) {
                float f;
                if ( !float.TryParse(vals[i], out f) ) {
                    LogUtil.Log("指定文字列はfloatに変換できません。スキップします。propName={0}, text={1}", propName, vals[i]);
                    return null;
                }
                fvals[i] = f; 
            }
            return fvals;
        }
    }
}
