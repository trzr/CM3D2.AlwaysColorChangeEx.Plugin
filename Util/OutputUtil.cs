/*
 * 外部化可能なようにC#標準のライブラリのみ使用が望ましい。
 * 定義ファイル関連のユーティリティ
 */
using System;
using System.IO;
using System.Text;
using CM3D2.AlwaysColorChange.Plugin.Data;

namespace CM3D2.AlwaysColorChange.Plugin.Util
{
    /// <summary>
    /// Description of OutputUtil.
    /// </summary>
    public sealed class OutputUtil
    {
        private readonly static OutputUtil instance = new OutputUtil();
        
        public static OutputUtil Instance {
            get { return instance; }
        }
        
        private OutputUtil() { }


        public string GetModDirectory() {
            string fullPath = Path.GetFullPath(".\\");
            string path = Path.Combine(fullPath, "Mod");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            return path;
        }

        public string GetACCDirectory() {
            return GetACCDirectory(null);
        }

        public string GetExportDirectory() {
            return GetACCDirectory("Export");
        }

        public string GetACCDirectory(string subName) {
            string modDir = GetModDirectory();
            string path = Path.Combine(modDir, "ACC");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            if ( !String.IsNullOrEmpty(subName) ) {
                path = Path.Combine(path, subName);
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            }

            return path;
        }

        public void WriteBytes(string file, byte[] imageBytes) {
            using (var writer = new BinaryWriter(File.OpenWrite(file))) {
                writer.Write(imageBytes);
            }
        }

        public void WriteTex(string file, string txtPath, byte[] imageBytes) {
            using (var writer = new BinaryWriter(File.OpenWrite(file))) {
                writer.Write("CM3D2_TEX");
                writer.Write(1000);// Int32
                writer.Write(txtPath);
                writer.Write(imageBytes.Length);
                writer.Write(imageBytes);
            }
        }
 
        public void Copy(string infile, string outfile) {

            const int buffSize = 8196;
            using ( var writer = new BinaryWriter(File.OpenWrite(outfile)) ) {
                using (AFileBase aFileBase = global::GameUty.FileOpen(infile)) {
                    
                    if (!aFileBase.IsValid()) {
                        LogUtil.ErrorLog("入力ファイルが見つかりません。", infile);
                        return;
                    }

                    var buff = new byte[buffSize];
                    int length = 0;
                    while ((length = aFileBase.Read(ref buff, buffSize))>= 0) {
                        writer.Write(buff, 0, length);
                    }
                }
            }
        }
        // TODO マテリアル番号と、変更するシェーダ名1,2を指定する必要あり
        public bool CopyModel(string infile, string outfile, string shader) {

            using ( var writer = new BinaryWriter(File.OpenWrite(outfile)) ) {
//                using (AFileBase aFileBase = global::GameUty.FileOpen(infile)) {
                using (var reader = new BinaryReader(File.Open(infile, FileMode.Open), Encoding.UTF8)) {
                    // ヘッダ
                    string head = reader.ReadString();
                    if (head != FileConst.HEAD_MODEL) {
                        LogUtil.ErrorLog("正しいモデルファイルではありません。ヘッダが不正です。", infile, head);
                        return false;
                    }
                    writer.Write(head);
                    writer.Write(reader.ReadInt32());  // ver
                    writer.Write(reader.ReadString()); // "_SM_" + name
                    writer.Write(reader.ReadString()); // base_bone
                    int count = reader.ReadInt32();
                    writer.Write(count);  // num (bone_count)
                    for(int i=0; i< count; i++) {
                        writer.Write(reader.ReadString());
                        writer.Write(reader.ReadByte());
                    }

                    for(int i=0; i< count; i++) {
                        int count2 = reader.ReadInt32();
                        writer.Write(count2);
                    }

                    for(int i=0; i< count; i++) {
                        // x, y, z
                        writer.Write(reader.ReadSingle());
                        writer.Write(reader.ReadSingle());
                        writer.Write(reader.ReadSingle());
                        // x2, y2, z2, w
                        writer.Write(reader.ReadSingle());
                        writer.Write(reader.ReadSingle());
                        writer.Write(reader.ReadSingle());
                        writer.Write(reader.ReadSingle());
                    }
                    int vertexCount = reader.ReadInt32();
                    int facesCount = reader.ReadInt32();
                    int localBoneCount = reader.ReadInt32();
                    writer.Write(vertexCount);
                    writer.Write(facesCount);
                    writer.Write(localBoneCount);

                    for(int i=0; i< localBoneCount; i++) {
                        writer.Write(reader.ReadString());
                    }
                    for(int i=0; i< localBoneCount; i++) {
                        for (int j=0; j< 16; j++) {
                            writer.Write(reader.ReadSingle());
                        }
                    }
                    for(int i=0; i< vertexCount; i++) {
                        for (int j=0; j< 8; j++) {
                            writer.Write(reader.ReadSingle());
                        }
                    }
                    int vertexCount2 = reader.ReadInt32();
                    writer.Write(vertexCount2);
                    for(int i=0; i< vertexCount2; i++) {
                        for (int j=0; j< 4; j++) {
                            writer.Write(reader.ReadSingle());
                        }
                    }
                    for(int i=0; i< vertexCount; i++) {
                        for (int j=0; j< 4; j++) {
                            writer.Write(reader.ReadUInt16());
                        }
                        for (int j=0; j< 4; j++) {
                            writer.Write(reader.ReadSingle());
                        }
                    }
                    for(int i=0; i< facesCount; i++) {
                        int cnt = reader.ReadInt32();
                        writer.Write(cnt);
                        for (int j=0; j< cnt; j++) {
                            writer.Write(reader.ReadInt16());
                        }
                    }
                    // material
                    int mateCount = reader.ReadInt32();
                    writer.Write(mateCount);
                    for(int i=0; i< mateCount; i++) {
                        // TODO material copy
                    }
                    
                    // morph
                    while (true) {
                        string name = reader.ReadString();
                        writer.Write(name);
                        if (name == "end") break;

                        if (name == "morph") {
                            string key = reader.ReadString();
                            writer.Write(key);
                            int num = reader.ReadInt32();
                            writer.Write(num);
                            for (int i=0; i< num; i++) {
                                writer.Write(reader.ReadUInt16());
                                // x, y, z
                                writer.Write(reader.ReadSingle());
                                writer.Write(reader.ReadSingle());
                                writer.Write(reader.ReadSingle());
                                // x, y, z
                                writer.Write(reader.ReadSingle());
                                writer.Write(reader.ReadSingle());
                                writer.Write(reader.ReadSingle());
                            }
                        }
                    }
                }
            }
            return true;
        }
        private bool CopyMaterial(BinaryReader reader, BinaryWriter writer) {
            writer.Write(reader.ReadString()); // name

            // シェーダ名 TODO 
            string shaderName1 = reader.ReadString();
            writer.Write(shaderName1); 
            string shaderName2 = reader.ReadString();
            writer.Write(shaderName2);

            while(true) {
                string type = reader.ReadString();
                writer.Write(type);
                if (type == "end") break;

                string propName = reader.ReadString();
                writer.Write(propName);
                switch (type) {
                    case "tex":
                        string sub = reader.ReadString();
                        writer.Write(sub);
                        switch (sub) {
                            case "tex2d":
                                string texfile = reader.ReadString();
                                string txtpath = reader.ReadString();
                                writer.Write(texfile);
                                writer.Write(txtpath);
                                writer.Write(reader.ReadSingle());
                                writer.Write(reader.ReadSingle());
                                writer.Write(reader.ReadSingle());
                                writer.Write(reader.ReadSingle());
                                break;
                            case "null":
                                break;
                            case "texRT":
                                writer.Write(reader.ReadString());
                                writer.Write(reader.ReadString());
                                break;
                        }
                        break;
                    case "col":
                    case "vec":
                        writer.Write(reader.ReadSingle());
                        writer.Write(reader.ReadSingle());
                        writer.Write(reader.ReadSingle());
                        writer.Write(reader.ReadSingle());
                        break;
                    case "f":
                        writer.Write(reader.ReadSingle());
                        break;
                }
            }
            return true;
        }
    }
}
