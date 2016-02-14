using System;
using System.IO;
using System.Text;
using CM3D2.AlwaysColorChangeEx.Plugin.Data;
using CM3D2.AlwaysColorChangeEx.Plugin.UI;

namespace CM3D2.AlwaysColorChangeEx.Plugin.Util
{
    /// <summary>
    /// 定義ファイル関連のユーティリティクラス.
    /// </summary>
    public sealed class OutputUtil
    {
        private readonly static OutputUtil instance = new OutputUtil();
        
        public static OutputUtil Instance {
            get { return instance; }
        }
        
        private OutputUtil() { }
        private const int BUFFER_SIZE = 8196;

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

        public void WriteTex(string outpath, string txtPath, byte[] imageBytes) {
            using (var writer = new BinaryWriter(File.OpenWrite(outpath))) {
                writer.Write(FileConst.HEAD_TEX);
                writer.Write(1000);// Int32
                writer.Write(txtPath);
                writer.Write(imageBytes.Length);
                writer.Write(imageBytes);
            }
        }

        // infile,outfileで、ファイルが特定できる必要あり
        public void Copy(string infilepath, string outfilepath) {

            using ( var writer = new BinaryWriter(File.OpenWrite(outfilepath)) ) 
            using (var  fs = new FileStream(infilepath, FileMode.Open)) {

                var buff = new byte[BUFFER_SIZE];
                int length = 0;
                while ((length = fs.Read(buff, 0, BUFFER_SIZE))>= 0) {
                    writer.Write(buff, 0, length);
                }
            }
        }

        public void WritePmat(string outpath, string name, float priority, string shader) {
            using ( var writer = new BinaryWriter(File.OpenWrite(outpath)) ) {
                writer.Write(FileConst.HEAD_PMAT);
                writer.Write(1000);// Int32
                writer.Write(name.GetHashCode());
                writer.Write(name);
                writer.Write(priority);
                writer.Write(shader);
            }
        }

        public void TransferMenu(BinaryReader reader, BinaryWriter writer, string txtpath, Func<string, string[], string[]> replace) {
            writer.Write(reader.ReadString()); // header
            writer.Write(reader.ReadInt32());  // version

            reader.ReadString();
            writer.Write(txtpath);

            writer.Write(reader.ReadString()); // headerName
            writer.Write(reader.ReadString()); // headerCategory
            writer.Write(reader.ReadString()); // headerDesc

            using (var dataStream = new MemoryStream())
                using (var dataWriter = new BinaryWriter(dataStream)) {
                int num2 = (int)reader.ReadInt32();
                while (true) {
                    int size = (int) reader.ReadByte();
                    if (size == 0) {
                        dataWriter.Write((byte)0);
                        break;
                    }

                    string key = reader.ReadString();
                    var param = new string[size-1];
                    for (int i = 0; i < size-1; i++) {
                        param[i] = reader.ReadString();
                    }
                    param = replace(key, param);

                    if (param != null) {
                        dataWriter.Write((byte) (param.Length+1));
                        dataWriter.Write(key);
                        foreach (string wparam in param) {
                            dataWriter.Write(wparam);
                        }
                    }
                }
                writer.Write((int)dataStream.Length);
                writer.Write(dataStream.ToArray());
            }
        }
    }
}
