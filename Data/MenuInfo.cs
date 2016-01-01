/*
 * Menu情報を扱うデータクラス
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CM3D2.AlwaysColorChange.Plugin.Util;

namespace CM3D2.AlwaysColorChange.Plugin.Data
{
    /// <summary>
    /// Description of Class1.
    /// </summary>
    public class MenuInfo
    {
        #region Constants
        public const string HEAD = "CM3D2_MENU";
        public const string HEAD_MOD = "CM3D2_MOD";
        public const string RET = "《改行》";
        public const string EXT_MOD     = ".mod";
        public const string EXT_MENU     = ".menu";
        public const string EXT_MATERIAL = ".mate";
        public const string EXT_MODEL    = ".model";
        public const string EXT_TEXTURE  = ".tex";
        #endregion

        public string baseFilename
        { get; set; }

        public string baseIcons
        { get; set; }

        public List<string[]> baseMaterials
        { get; set; }

        public List<string[]> baseAddItems
        { get; set; }

        public List<string[]> baseResources
        { get; set; }

        public string outputPath
        { get; set; }

        public string filename
        { get; set; }

        public int version
        { get; set; }

        public string txtpath
        { get; set; }

        public string headerName
        { get; set; }

        public string headerCategory
        { get; set; }

        public string headerSetumei
        { get; set; }

        public string menuFolder
        { get; set; }

        public string category
        { get; set; }

        public string catno
        { get; set; }

        public string priority
        { get; set; }

        public string name
        { get; set; }

        public string setumei
        { get; set; }

        public string icons
        { get; set; }

        public string[] itemParam
        { get; set; }

        public List<string> items
        { get; set; }

        public List<string[]> addItems
        { get; set; }

        public List<string> maskItems
        { get; set; }

        public List<string[]> materials
        { get; set; }

        public List<string> delNodes
        { get; set; }

        public List<string> showNodes
        { get; set; }

        public List<string[]> delPartsNodes
        { get; set; }

        public List<string[]> showPartsNodes
        { get; set; }

        public List<string[]> resources
        { get; set; }

        public MenuInfo()
        {
            baseMaterials = new List<string[]>();
            baseAddItems = new List<string[]>();
            baseResources = new List<string[]>();
            itemParam = new string[3];
            items = new List<string>();
            addItems = new List<string[]>();
            maskItems = new List<string>();
            materials = new List<string[]>();
            delNodes = new List<string>();
            showNodes = new List<string>();
            delPartsNodes = new List<string[]>();
            showPartsNodes = new List<string[]>();
            resources = new List<string[]>();
        }

        public bool LoadMenufile(string filename)
        {
            LogUtil.DebugLog("loading menu file", filename);
            this.baseFilename = filename;
            this.filename = Path.GetFileNameWithoutExtension(filename);
            byte[] cd = null;
            try {
                using (AFileBase aFileBase = global::GameUty.FileOpen(filename)) {
                    if (!aFileBase.IsValid()) {
                        LogUtil.ErrorLog("アイテムメニューファイルが見つかりません。", filename);
                        return false;
                    }
                    cd = aFileBase.ReadAll();
                }
            } catch (Exception ex2) {
                LogUtil.ErrorLog("アイテムメニューファイルが読み込めませんでした。", filename, ex2.Message);
                return false;
            }

            try {
                using (var binaryReader = new BinaryReader(new MemoryStream(cd), Encoding.UTF8)) {
                    string text = binaryReader.ReadString();
                    if (text != HEAD) {
                        if (text == HEAD_MOD) {
                            LogUtil.ErrorLog("MODファイルは未対応。", filename);
                        } else {
                            LogUtil.ErrorLog("例外: ヘッダーファイルが不正です。", text, filename);
                        }
                        return false;
                    }
                    version = binaryReader.ReadInt32();
                    txtpath = binaryReader.ReadString();
                    headerName = binaryReader.ReadString();
                    headerCategory = binaryReader.ReadString();
                    headerSetumei = binaryReader.ReadString().Replace(RET, "\n");

                    int num2 = (int)binaryReader.ReadInt32();
                    while (true) {
                        int size = (int) binaryReader.ReadByte();
                        if (size == 0) break;

                        var param = new string[size];
                        for (int i = 0; i < size; i++) {
                            param[i] = binaryReader.ReadString();
                        }
                        switch (param[0]) {
                            case "メニューフォルダ":
                                menuFolder = param[1];
                                break;
                            case "category":
                                category = param[1];
                                break;
                            case "catno":
                                catno = param[1];
                                break;
                            case "属性追加":
                                break;
                            case "priority":
                                priority = param[1];
                                break;
                            case "name":
                                name = param[1];
                                break;
                            case "setumei":
                                setumei = param[1].Replace(RET, "\n");
                                break;
                            case "icon":
                            case "icons":
                                baseIcons = Path.GetFileNameWithoutExtension(param[1]);
                                icons = baseIcons;
                                break;
                            case "アイテムパラメータ":
                                //                                    itemParam = new String[3];
                                itemParam[0] = param[1];
                                itemParam[1] = param[2];
                                itemParam[2] = param[3];
                                break;
                            case "アイテム":
                                items.Add(Path.GetFileNameWithoutExtension(param[1]));
                                break;

                            case "additem":
//                                var ai = new string[2];
//                                ai[0] = Path.GetFileNameWithoutExtension(param[1]);
//                                ai[1] = param[2];
//                                baseAddItems.Add(ai);
//                                ai = new string[2];
//                                ai[0] = Path.GetFileNameWithoutExtension(param[1]);
//                                ai[1] = param[2];
//                                addItems.Add(ai);

                                var ai = new string[size-1];
                                Array.Copy(param, 1, ai, 0, size-1);
                                ai[0] = Path.GetFileNameWithoutExtension(ai[0]);
                                baseAddItems.Add(ai);
                                addItems.Add((string[])ai.Clone());
                                break;
                            case "maskitem":
                                maskItems.Add(param[1]);
                                break;
                            case "マテリアル変更":
                                var mat = new string[size-1];
                                Array.Copy(param, 1, mat, 0, size-1);
                                mat[2] = Path.GetFileNameWithoutExtension(mat[2]);
                                LogUtil.DebugLog(mat);
                                baseMaterials.Add(mat);
                                materials.Add((string[])mat.Clone());
                                break;
                            case "node消去":
                                delNodes.Add(param[1]);
                                break;
                            case "node表示":
                                showNodes.Add(param[1]);
                                break;
                            // TODO
                            case "消去ノード":
                                break;
                            case "パーツnode消去":
                                var dp = new string[size-1];
                                Array.Copy(param, 1, dp, 0, size-1);
                                delPartsNodes.Add(dp);
                                break;
                            case "パーツnode表示":
                                var sp = new string[size-1];
                                Array.Copy(param, 1, sp, 0, size-1);
                                showPartsNodes.Add(sp);
                                break;
                            case "リソース参照":
                                var rs = new string[size-1];
                                Array.Copy(param, 1, rs, 0, size-1);
                                rs[1] = Path.GetFileNameWithoutExtension(rs[1]);
                                baseResources.Add(rs);
                                resources.Add((string[])rs.Clone());
                                break;
                            // TODO
                            case "テクスチャ合成":
                                break;
                            case "アタッチポイントの設定":
                                break;
                            case "tex":
                            case "テクスチャ変更":
                                break;
                            // TODO
                            case "unsetitem":
                                break;
                            // TODO
                            case "delitem":
                                break;
                            case "commenttype":
                                break;
                            // TODO 
                            //case "colorset"
                            // break;

                        }
                    }
                }

            } catch (Exception e) {
                LogUtil.DebugLog(e);
                return false;
            }
            LogUtil.DebugLog("loaded. return true");
            return true;
        }
    }

}
