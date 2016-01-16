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
        public string baseFilename
        { get; set; }
        public string baseIcons
        { get; set; }
        public Dictionary<string, List<TargetMaterial>> baseMaterials
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
        public List<string[]> addItems { get; set; }
        public List<string> delItems   { get; set; }
        public List<string> maskItems  { get; set; }
        public List<TargetMaterial> materials
        { get; set; }
        public List<string> delNodes
        { get; set; }
        public List<string> showNodes
        { get; set; }
        public List<string[]> delPartsNodes
        { get; set; }
        public List<string[]> showPartsNodes
        { get; set; }
        // キーとmenuファイル
        public Dictionary<string, string> resDic
        { get; set; }
        public MenuInfo()
        {
            baseMaterials = new Dictionary<string, List<TargetMaterial>>();
            baseAddItems = new List<string[]>();
            baseResources = new List<string[]>();
            itemParam = new string[3];
            items = new List<string>();
            addItems = new List<string[]>();
            delItems = new List<string>();
            maskItems = new List<string>();
            materials = new List<TargetMaterial>();
            delNodes = new List<string>();
            showNodes = new List<string>();
            delPartsNodes = new List<string[]>();
            showPartsNodes = new List<string[]>();
        }

        public bool LoadMenufile(string filename)
        {
            LogUtil.DebugLog("loading menu file", filename);
            this.baseFilename = filename;
            this.filename = Path.GetFileNameWithoutExtension(filename);

            try {
                byte[] menuBytes = OutputUtilEx.Instance.LoadInternal(filename);
                using (var binaryReader = new BinaryReader(new MemoryStream(menuBytes), Encoding.UTF8)) {
                    string head = binaryReader.ReadString();
                    if (head != FileConst.HEAD_MENU) {
                        if (head == FileConst.HEAD_MOD) {
                            LogUtil.ErrorLog("MODファイルは未対応。", filename);
                        } else {
                            LogUtil.ErrorLog("MENUファイルのヘッダーファイルが正しくありません", head, filename);
                        }
                        return false;
                    }
                    version = binaryReader.ReadInt32();
                    txtpath = binaryReader.ReadString();
                    headerName = binaryReader.ReadString();
                    headerCategory = binaryReader.ReadString();
                    headerSetumei  = binaryReader.ReadString().Replace(FileConst.RET, "\n");

                    int num2 = (int)binaryReader.ReadInt32();
                    while (true) {
                        int size = (int) binaryReader.ReadByte();
                        if (size == 0) break;

                        string key = binaryReader.ReadString();
                        var param = new string[size-1];
                        for (int i = 0; i < size-1; i++) {
                            param[i] = binaryReader.ReadString();
                        }
                        switch (key) {
                            case "メニューフォルダ":
                                menuFolder = param[0];
                                break;
                            case "category":
                                category = param[0];
                                break;
                            case "catno":
                                catno = param[0];
                                break;
                            case "属性追加":
                                break;
                            case "priority":
                                priority = param[0];
                                break;
                            case "name":
                                name = param[0];
                                break;
                            case "setumei":
                                setumei = param[0].Replace(FileConst.RET, "\n");
                                break;
                            case "icon":
                            case "icons":
                                baseIcons = Path.GetFileNameWithoutExtension(param[0]);
                                icons = baseIcons;
                                break;
                            case "アイテムパラメータ":
                                if (param.Length == 3) {
                                    Array.Copy(param, 0, itemParam, 0, param.Length);
                                }
                                break;
                            case "アイテム":
                                items.Add(Path.GetFileNameWithoutExtension(param[0]));
                                break;

                            case "additem":
                                param[0] = Path.GetFileNameWithoutExtension(param[0]);
                                baseAddItems.Add(param);
                                addItems.Add((string[])param.Clone());
                                break;
                            case "maskitem":
                                if (param.Length >= 1) {
                                    maskItems.Add(param[0]);
                                }
                                break;
                            case "delitem":
                                string slot0 = category;
                                if (param.Length >= 1) slot0 = param[0];
                                delItems.Add(slot0);
                                break;
                            case "マテリアル変更":
                                string slot = param[0];
                                int matNo = int.Parse(param[1]);
                                string file = param[2];
                                List<TargetMaterial> mats;
                                if (!baseMaterials.TryGetValue(slot, out mats)) {
                                    mats = new List<TargetMaterial>();
                                    baseMaterials[slot] = mats;
                                }
                                var tm = new TargetMaterial(slot, matNo, file);
                                mats.Add(tm);
                                materials.Add(tm);
                                break;
                            case "node消去":
                                delNodes.Add(param[0]);
                                break;
                            case "node表示":
                                showNodes.Add(param[0]);
                                break;
                            // TODO
                            case "パーツnode消去":
                                delPartsNodes.Add(param);
                                break;
                            case "パーツnode表示":
                                showPartsNodes.Add(param);
                                break;
                            case "リソース参照":
                                if (resDic == null) resDic = new Dictionary<string, string>();
                                resDic[param[0]] = param[1];
                                break;
                            case "半脱ぎ":
                                if (resDic == null) resDic = new Dictionary<string, string>();
                                resDic["半脱ぎ"] = param[0];
                                break;
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
                            case "commenttype":
                                break;
                            // TODO 
                            //case "colorset"
                            // break;

                        }
                    }
                }

            } catch (Exception e) {
                LogUtil.ErrorLog("アイテムメニューファイルが読み込めませんでした。", filename, e);
                return false;
            }

            LogUtil.DebugLog("loaded. return true");
            return true;
        }
    }
    public class TargetMaterial {
        public string category { get; private set;}
        public int matNo       { get; private set;}
        public string filename { get; private set;}
        public string editname { get; set;}
        public TargetMaterial(string cat, int matNo, string filename) {
            this.category = cat;
            this.matNo = matNo;
            this.filename = filename;
            this.editname = Path.GetFileNameWithoutExtension(filename);
        }
    }

}
