using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using UnityEngine;
using CM3D2.AlwaysColorChangeEx.Plugin.Data;
using CM3D2.AlwaysColorChangeEx.Plugin.UI;

namespace CM3D2.AlwaysColorChangeEx.Plugin.Util {

    /// <summary>
    /// OutputUtilラッパークラス.
    /// カスメ専用クラス等を扱うメソッドを拡張したユーティリティ
    /// </summary>
    public sealed class FileUtilEx {
        private static readonly FileUtilEx INSTANCE = new FileUtilEx();
        public static FileUtilEx Instance {
            get { return INSTANCE; }
        }
        private static readonly OutputUtil UTIL = OutputUtil.Instance;
        
        private FileUtilEx() { }

        public string GetModDirectory() {
            return UTIL.GetModDirectory();
        }

        public string GetACCDirectory() {
            return UTIL.GetACCDirectory();
        }

        public string GetExportDirectory() {
            return UTIL.GetACCDirectory("Export");
        }

        public string GetACCDirectory(string subName) {
            return UTIL.GetACCDirectory(subName);
        }

        public void WriteBytes(string file, byte[] imageBytes) {
            UTIL.WriteBytes(file, imageBytes);
        }

        public void WriteTexFile(string filepath, string txtPath, byte[] imageBytes) {
            UTIL.WriteTex(filepath, txtPath, imageBytes);
        }

        public void WritePmat(string outpath, string name, float priority, string shader) {
            UTIL.WritePmat(outpath, name, priority, shader);
        }

        // infile,outfileで、ファイルが特定できる必要あり
        public void Copy(string infilepath, string outfilepath) {
            UTIL.Copy(infilepath, outfilepath);
        }

        public bool Exists(string filename) {
            if (!GameUty.ModPriorityToModFolder) {
                if (GameUty.FileSystem.IsExistentFile(filename)) return true;
                if (GameUty.FileSystemMod != null) {
                    return GameUty.FileSystemMod.IsExistentFile(filename);
                }
            } else {
                if (GameUty.FileSystemMod != null && GameUty.FileSystemMod.IsExistentFile(filename)) {
                    return true;
                }

                return GameUty.FileSystem.IsExistentFile(filename);
            }
            return false;
        }

        private const int BUFFER_SIZE = 8192;
        // 外部DLL依存
        // 一旦バイト配列にロードすることなくStreamオブジェクトとして参照可能とする
        public Stream GetStream(string filename) {
            try {
                var aFileBase = GameUty.FileOpen(filename);
                if (aFileBase.IsValid()) return new BufferedStream(new FileBaseStream(aFileBase), BUFFER_SIZE);
                var msg = LogUtil.Error("指定ファイルが見つかりません。file=", filename);
                throw new ACCException(msg.ToString());

                // if (aFileBase.GetSize() < BUFFER_SIZE) {
            } catch (ACCException) {
                throw;
            } catch (Exception e) {
                var msg = LogUtil.Error("指定ファイルが読み込めませんでした。", filename, e);
                throw new ACCException(msg.ToString(), e);
            }
        }
        public Stream GetStream(string filename, out bool onBuffer) {
            try {
                var aFileBase = GameUty.FileOpen(filename);
                if (!aFileBase.IsValid()) {
                    var msg = LogUtil.Error("指定ファイルが見つかりません。file=", filename);
                    throw new ACCException(msg.ToString());
                }

                onBuffer = aFileBase.GetSize() < BUFFER_SIZE;
                return new BufferedStream(new FileBaseStream(aFileBase), BUFFER_SIZE);
            } catch (ACCException) {
                throw;
            } catch (Exception e) {
                var msg = LogUtil.Error("指定ファイルが読み込めませんでした。", filename, e);
                throw new ACCException(msg.ToString(), e);
            }
        }

        public byte[] LoadInternal(string filename) {
            try {
                using (var aFileBase = GameUty.FileOpen(filename)) {
                    if (aFileBase.IsValid()) return aFileBase.ReadAll();

                    var msg = LogUtil.Error("指定ファイルが見つかりません。file=", filename);
                    throw new ACCException(msg.ToString());
                }
            } catch (ACCException) {
                throw;
            } catch (Exception e) {
                var msg = LogUtil.Error("指定ファイルが読み込めませんでした。", filename, e);
                throw new ACCException(msg.ToString(), e);
            }
        }

        public Texture2D LoadTexture(string filename) {
            var tex2D = TexUtil.Instance.Load(filename);
            tex2D.name = Path.GetFileNameWithoutExtension(filename);
            tex2D.wrapMode = TextureWrapMode.Clamp;

            return tex2D;
        }

        public Texture2D LoadTexture(Stream stream) {
            var bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            var tex2D = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            tex2D.LoadImage(bytes);
            tex2D.wrapMode = TextureWrapMode.Clamp;

            return tex2D;
        }

        // 外部DLL依存
        public void Copy(AFileBase infile, string outfilepath) {
            const int buffSize = 8196;
            using ( var writer = new BinaryWriter(File.OpenWrite(outfilepath)) )  {

                var buff = new byte[buffSize];
                int length;
                while ((length = infile.Read(ref buff, buffSize)) > 0) {
                    writer.Write(buff, 0, length);
                }
            }
        }

        public IEnumerable<ReplacedInfo> WriteMenuFile(string infile, string outfilepath, ResourceRef res) {
            bool onBuffer;
            using ( var reader = new BinaryReader(Instance.GetStream(infile, out onBuffer), Encoding.UTF8)) {
                var header = reader.ReadString(); // header
                if (onBuffer || reader.BaseStream.Position > 0) {
                    if (header == FileConst.HEAD_MENU) {
                        return WriteMenuFile(reader, header, outfilepath, res);
                    }

                    var msg = LogUtil.Error("menuファイルを作成しようとしましたが、参照元ファイルのヘッダが正しくありません。", header, ", file=", infile);
                    throw new ACCException(msg.ToString());
                }
            }

            // arc内のファイルがロードできない場合の回避策: Sybaris 0410向け対策. 一括読み込み
            using (var reader = new BinaryReader(new MemoryStream(Instance.LoadInternal(infile), false), Encoding.UTF8)) {
                var header = reader.ReadString(); // hader
                if (header == FileConst.HEAD_MENU) {
                    return WriteMenuFile(reader, header, outfilepath, res);
                }
                var msg = LogUtil.Error("menuファイルを作成しようとしましたが、参照元ファイルのヘッダが正しくありません。", header, ", file=", infile);
                throw new ACCException(msg.ToString());
            }
        }

        private List<ReplacedInfo> WriteMenuFile(BinaryReader reader, string header, string outfilepath, ResourceRef res) {
            using ( var writer = new BinaryWriter(File.OpenWrite(outfilepath)) ) {
                try {
                    UTIL.TransferMenu(reader, writer, header, res.EditTxtPath(), res.ReplaceMenuFunc());
                    return res.replaceFiles;
                } catch(Exception e) {
                    var msg = LogUtil.Error("menuファイルの作成に失敗しました。 file=", outfilepath, e);
                    throw new ACCException(msg.ToString(), e);
                }
            }
        }
        ///
        ///
        public bool WriteModelFile(string infile, string outfilepath, SlotMaterials slotMat) {
            bool onBuffer;
            using ( var reader = new BinaryReader(GetStream(infile, out onBuffer), Encoding.UTF8) ) {

                // ヘッダ
                var header = reader.ReadString();
                if (onBuffer || reader.BaseStream.Position > 0) {
                    if (header == FileConst.HEAD_MODEL) {
                        return WriteModelFile(reader, header, outfilepath, slotMat);
                    }
                    var msg = LogUtil.Error("正しいモデルファイルではありません。ヘッダが不正です。", header, ", infile=", infile);
                    throw new ACCException(msg.ToString());
                }
            }

            // arc内のファイルがロードできない場合の回避策: Sybaris 0410向け対策. 一括読み込み
            using (var reader = new BinaryReader(new MemoryStream(Instance.LoadInternal(infile), false), Encoding.UTF8)) {
                var header = reader.ReadString(); // hader
                if (header == FileConst.HEAD_MODEL) {
                    return WriteModelFile(reader, header, outfilepath, slotMat);

                } 
                var msg = LogUtil.Error("正しいモデルファイルではありません。ヘッダが不正です。", header, ", infile=", infile);
                throw new ACCException(msg.ToString());
            }
        }

        private bool WriteModelFile(BinaryReader reader, string header, string outfilepath, SlotMaterials slotMat) {
            using ( var writer = new BinaryWriter(File.OpenWrite(outfilepath)) ) {
                return TransferModel(reader, header, writer, slotMat);
            }
        }

        public bool TransferModel(BinaryReader reader, string header, BinaryWriter writer,SlotMaterials slotMat) {

            writer.Write(header);
            var ver = reader.ReadInt32();
            writer.Write(ver);  // ver
            writer.Write(reader.ReadString()); // "_SM_" + name
            writer.Write(reader.ReadString()); // base_bone
            var count = reader.ReadInt32();
            writer.Write(count);  // num (bone_count)
            for(var i=0; i< count; i++) {
                writer.Write(reader.ReadString()); // ボーン名
                writer.Write(reader.ReadByte());   // フラグ　(_SCL_追加の有無等)
            }

            for(var i=0; i< count; i++) {
                var count2 = reader.ReadInt32();   // parent index
                writer.Write(count2);
            }

            for(var i=0; i< count; i++) {
                // localPosition, localRotation
                // (x, y, z), (x2, y2, z2, w)
                TransferVec(reader, writer, 7);
                // localScale
                if (ver < 2001) continue;

                var readScale = reader.ReadBoolean();
                writer.Write(readScale);
                if (readScale) {
                    TransferVec(reader, writer);
                }
            }
            var vertexCount = reader.ReadInt32();
            var facesCount = reader.ReadInt32();
            var localBoneCount = reader.ReadInt32();
            writer.Write(vertexCount);
            writer.Write(facesCount);
            writer.Write(localBoneCount);

            for(var i=0; i< localBoneCount; i++) {
                writer.Write(reader.ReadString()); // ローカルボーン名
            }
            for(var i=0; i< localBoneCount; i++) {
                TransferVec(reader, writer, 16); // matrix (floatx4, floatx4)
            }
            for(var i=0; i< vertexCount; i++) {
                TransferVec(reader, writer, 8);
            }
            var vertexCount2 = reader.ReadInt32();
            writer.Write(vertexCount2);
            for(var i=0; i< vertexCount2; i++) {
                TransferVec4(reader, writer);
            }
            for(var i=0; i< vertexCount; i++) {
                for (var j=0; j< 4; j++) {
                    writer.Write(reader.ReadUInt16());
                }
                TransferVec4(reader, writer);
            }
            for(var i=0; i< facesCount; i++) {
                var cnt = reader.ReadInt32();
                writer.Write(cnt);
                for (var j=0; j< cnt; j++) {
                    writer.Write(reader.ReadInt16());
                }
            }
            // material
            var mateCount = reader.ReadInt32();
            writer.Write(mateCount);
            for(var matNo=0; matNo< mateCount; matNo++) {
                var tm = slotMat.Get(matNo);
                TransferMaterial(reader, writer, tm, tm.onlyModel);
            }
            
            // morph
            while (reader.PeekChar() != -1) {
                var name = reader.ReadString();
                writer.Write(name);
                if (name == "end") break;

                if (name != "morph") continue;
                var key = reader.ReadString();
                writer.Write(key);
                var num = reader.ReadInt32();
                writer.Write(num);
                for (var i=0; i< num; i++) {
                    writer.Write(reader.ReadUInt16());
                    // (x, y, z), (x, y, z)
                    TransferVec(reader, writer, 6);
                }
            }
            return true;
        }

        public void WriteMateFile(string infile, string outfilepath, TargetMaterial trgtMat) {
            bool onBuffer;
            using ( var reader = new BinaryReader(GetStream(infile, out onBuffer), Encoding.UTF8) ) {
                var header = reader.ReadString();
                if (onBuffer || reader.BaseStream.Position > 0) {
                    if (header == FileConst.HEAD_MATE) {
                        WriteMateFile(reader, header, outfilepath, trgtMat);
                        return;
                    }
                    var msg = LogUtil.Error("正しいmateファイルではありません。ヘッダが不正です。", header, ", infile=", infile);
                    throw new ACCException(msg.ToString());
                }
            }
            
            // arc内のファイルがロードできない場合の回避策: Sybaris 0410向け対策. 一括読み込み
            using (var reader = new BinaryReader(new MemoryStream(Instance.LoadInternal(infile), false), Encoding.UTF8)) {
                var header = reader.ReadString(); // hader
                if (header == FileConst.HEAD_MATE) {
                    WriteMateFile(reader, header, outfilepath, trgtMat);
                }
                var msg = LogUtil.Error("正しいmateファイルではありません。ヘッダが不正です。", header, ", infile=", infile);
                throw new ACCException(msg.ToString());
            }
        }

        public void WriteMateFile(BinaryReader reader, string header, string outfilepath, TargetMaterial trgtMat) {

            using ( var writer = new BinaryWriter(File.OpenWrite(outfilepath)) ) {
                writer.Write(header);              // ヘッダ (CM3D2_MATERIAL)
                writer.Write(reader.ReadInt32());  // バージョン
                writer.Write(reader.ReadString()); // マテリアル名1

                TransferMaterial(reader, writer, trgtMat, true);
            }
        }

        // modelファイル内のマテリアル情報を対象とした転送処理
        // .mateファイルのheader, version, name1は存在しない
        public void TransferMaterial(BinaryReader reader, BinaryWriter writer, TargetMaterial trgtMat, bool overwrite) {

            // マテリアル名
            reader.ReadString();
            writer.Write(trgtMat.editname);

            var shaderName1 = reader.ReadString();
            var shaderName2 = reader.ReadString();
            if (trgtMat.shaderChanged) {
                shaderName1 = trgtMat.ShaderNameOrDefault(shaderName1);
                shaderName2 = ShaderType.GetShader2(shaderName1);
            }
            writer.Write(shaderName1);
            writer.Write(shaderName2);

            //var matType = trgtMat.editedMat.type1;
            var shaderType = trgtMat.editedMat.type;
            var writed = new HashSet<PropKey>();
            while(reader.PeekChar() != -1) {
                var type = reader.ReadString();
                //writer.Write(type);
                if (type == "end") break;

                var propName = reader.ReadString();
                //shaderType.
                var shaderProp = shaderType.GetShaderProp(propName);
                if (shaderProp == null) {
                    // シェーダに対応していないプロパティは読み捨て
                    DiscardMateProp(reader, type);
                    continue;
                }
                
                if (!overwrite) { 
                    // .mateからマテリアル変更で書き換えるため、そのまま転送
                    // ただし、model上に記述されたマテリアルで指定されたtexファイルは存在する必要あり
                    TransferMateProp(reader, writer, type, propName);
                } else {
                    switch (type) {
                    case "tex":
                        // .mateによるマテリアル変更がないケースのみ書き換える
                        // 
                        // texプロパティがある場合にのみ設定
                        TargetTexture trgtTex;
                        trgtMat.texDic.TryGetValue(shaderProp.key, out trgtTex);
                        if (trgtTex == null || trgtTex.tex == null || trgtTex.fileChanged || trgtTex.colorChanged) {
                            // 変更がある場合にのみ書き換え (空のものはnull指定)
                            if (trgtTex != null) trgtTex.worksuffix = trgtMat.worksuffix;
                            string srcfile = null;
                            TransferMateProp(reader, null, type, null, ref srcfile);
                            if (trgtTex != null) trgtTex.workfilename = srcfile;

                            WriteTex(writer, propName, trgtMat, trgtTex);
                        } else {
                            // 変更がないものはそのまま転送
                            TransferMateProp(reader, writer, type, propName);
                        }
                        break;
                    case "col":
                    case "vec":
                        Write(writer, type, propName);
                        Write(writer, trgtMat.editedMat.material.GetColor(propName));
                        
                        DiscardMateProp(reader, type);
                        break;
                    case "f":
                        Write(writer, type, propName);
                        Write(writer, trgtMat.editedMat.material.GetFloat(propName));
                        
                        DiscardMateProp(reader, type);
                        break;
                    }
                }
                writed.Add(shaderProp.key);
            }

            // シェーダで設定されるプロパティ数が一致しない場合、不足propを追記
            
            if (shaderType.KeyCount() != writed.Count()) {
                foreach (var texProp in shaderType.texProps) {
                    if (writed.Contains(texProp.key)) continue;

                    TargetTexture trgtTex;
                    trgtMat.texDic.TryGetValue(texProp.key, out trgtTex);
                    WriteTex(writer, texProp.keyName, trgtMat, trgtTex);
                }

                foreach (var prop in shaderType.colProps) {
                    if (writed.Contains(prop.key)) continue;

                    Write(writer, prop.type.ToString(), prop.keyName);
                    Write(writer, trgtMat.editedMat.material.GetColor(prop.propId));
                }
                
                foreach (var prop in shaderType.fProps) {
                    if (writed.Contains(prop.key)) continue;

                    Write(writer, prop.type.ToString(), prop.keyName);
                    Write(writer, trgtMat.editedMat.material.GetFloat(prop.propId));
                }

            }

            writer.Write("end");
        }

        private void WriteTex(BinaryWriter writer, string propName, TargetMaterial tm, TargetTexture trgtTex) {
            Write(writer, "tex");
            Write(writer, propName);

            var sub = "tex2d";
            if (trgtTex == null || trgtTex.tex == null)  {
                sub = "null";
            }
            Write(writer, sub);
            switch (sub) {
            case "tex2d":
                // カラー変更時にはファイル生成するため、ファイル名も変更が必要
                if (trgtTex.fileChanged || trgtTex.colorChanged) {
                    Write(writer, trgtTex.EditFileNameNoExt()); // 拡張子不要
                    //Write(writer, trgtTex.EditFileName());
                    Write(writer, trgtTex.EditTxtPath());

                    Write(writer, tm.editedMat.material.GetTextureOffset(propName));
                    Write(writer, tm.editedMat.material.GetTextureScale(propName));
                }
                break;
            case "null":
                break;
            case "texRT":            // texRTはない
                writer.Write(string.Empty);
                writer.Write(string.Empty);
                break;
            }
        }

        private void DiscardMateProp(BinaryReader reader, string type) {
            TransferMateProp(reader, null, type, null);
        }

        private void TransferMateProp(BinaryReader reader, BinaryWriter writer, string type, string propName, ref string texfile) {
            Write(writer, type, propName);
            switch (type) {
                case "tex":
                    var sub = reader.ReadString();
                    Write(writer, sub);
                    switch (sub) {
                        case "tex2d":
                            var file = reader.ReadString();
                            texfile = file;
                            var txtpath = reader.ReadString();
                            Write(writer, file, txtpath);
                            TransferVec4(reader, writer);
                            break;
                        case "null":
                            break;
                        case "texRT":
                            TransferString(reader, writer, 2);
                            break;
                    }
                    break;
                case "col":
                case "vec":
                    TransferVec4(reader, writer);
                    break;
                case "f":
                    Write(writer, reader.ReadSingle());
                    break;
            }
        }

        private void TransferMateProp(BinaryReader reader, BinaryWriter writer, string type, string propName) {
            string file = null;
            TransferMateProp(reader, writer, type, propName, ref file);
        }
        
        private void Write(BinaryWriter writer, params string[] data) {
            if (writer == null) return;
            foreach (var d in data)  writer.Write(d);
        }

        private void Write(BinaryWriter writer, float data) {
            if (writer != null) writer.Write(data);
        }

        private void Write(BinaryWriter writer, byte data) {
            if (writer != null) writer.Write(data);
        }

        public void Write(BinaryWriter writer, Vector2 data) {
            if (writer == null) return;
            writer.Write(data.x);
            writer.Write(data.y);
        }

        private void Write(BinaryWriter writer, params float[] data) {
            if (writer == null) return;
            foreach (var f in data)  writer.Write(f);
        }

        public void Write(BinaryWriter writer, Color color) {
            if (writer == null) return;
            writer.Write(color.r);
            writer.Write(color.g);
            writer.Write(color.b);
            writer.Write(color.a);
        }

        private void TransferVec(BinaryReader reader, BinaryWriter writer, int size=3) {
            for(var i=0; i<size; i++) {
                var data = reader.ReadSingle();
                if (writer != null) writer.Write(data);
            }
        }

        private void TransferVec4(BinaryReader reader, BinaryWriter writer) {
            TransferVec(reader, writer, 4);
        }

        private void TransferString(BinaryReader reader, BinaryWriter writer, int count) {
            for(var i=0; i<count; i++) {
                var data = reader.ReadString();
                if (writer != null) writer.Write(data);
            }
        }

        public void CopyTex(string infile, string outfilepath, string txtpath, TextureModifier.FilterParam filter) {
            // テクスチャをロードし、フィルタを適用
            var srcTex = TexUtil.Instance.Load(infile);
            var dstTex = (filter != null) ? ACCTexturesView.Filter(srcTex, filter) : srcTex;

            WriteTexFile(outfilepath, txtpath, dstTex.EncodeToPNG());
            if (srcTex != dstTex) UnityEngine.Object.DestroyImmediate(dstTex);
            UnityEngine.Object.DestroyImmediate(srcTex);
        }
    }
}
