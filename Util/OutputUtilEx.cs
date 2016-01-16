/*
 * OutputUtilラッパー
 * カスメ専用クラス等を使うメソッドを拡張するユーティリティ
 */
using System;
using System.IO;
using System.Text;

namespace CM3D2.AlwaysColorChange.Plugin.Util
{
    /// <summary>
    /// Description of OutputUtilEx.
    /// </summary>
    public sealed class OutputUtilEx 
    {
        private static OutputUtilEx instance = new OutputUtilEx();
        
        public static OutputUtilEx Instance {
            get {
                return instance;
            }
        }
        private static readonly OutputUtil util = OutputUtil.Instance;
        
        private OutputUtilEx() { }

        public string GetModDirectory() {
            return util.GetModDirectory();
        }

        public string GetACCDirectory() {
            return util.GetACCDirectory();
        }

        public string GetExportDirectory() {
            return util.GetACCDirectory("Export");
        }

        public string GetACCDirectory(string subName) {
            return util.GetACCDirectory(subName);
        }

        public void WriteBytes(string file, byte[] imageBytes) {
            util.WriteBytes(file, imageBytes);
        }

        public void WriteTex(string file, string txtPath, byte[] imageBytes) {
            util.WriteTex(file, txtPath, imageBytes);
        }

        // infile,outfileで、ファイルが特定できる必要あり
        public void Copy(string infilepath, string outfilepath) {
            util.Copy(infilepath, outfilepath);
        }

        public bool CopyModel(string infile, string outfile, string shader) {
            return util.CopyModel(infile, outfile, shader);
        }

        // 外部DLL依存
        public void Copy(AFileBase infile, string outfilepath) {
            const int buffSize = 8196;
            using ( var writer = new BinaryWriter(File.OpenWrite(outfilepath)) )  {

                var buff = new byte[buffSize];
                int length = 0;
                while ((length = infile.Read(ref buff, buffSize))>= 0) {
                    writer.Write(buff, 0, length);
                }
            }
        }

        // 外部DLL依存
        public void CopyModel(AFileBase infile, string outfilepath) {
            using ( var writer = new BinaryWriter(File.OpenWrite(outfilepath)) )  
            using ( var reader = new BinaryReader(new MemoryStream(infile.ReadAll()), Encoding.UTF8)) {
                util.CopyModel(reader, writer, null);
            }
        }

        public byte[] LoadInternal(string filename) {
            try {
                using (AFileBase aFileBase = global::GameUty.FileOpen(filename)) {
                    if (!aFileBase.IsValid()) {
                        var msg = "指定ファイルが見つかりません。file="+ filename;
                        LogUtil.ErrorLog(msg);
                        throw new ACCException(msg);
                    }
                    return aFileBase.ReadAll();
                }
            } catch (Exception e) {
                var msg = "指定ファイルが読み込めませんでした。"+ filename+ e.Message;
                LogUtil.ErrorLog(msg, e);
                throw new ACCException(msg, e);
            }
        }

    }
}
