/*
 * GUIで編集するデータとを扱うデータクラス
 * 3Dオブジェクト上のデータとの対応付け、
 * 定義ファイルからのロード等を行う
 * 
 * 出力するか否か、出力時のファイル名等の制御が複雑なため、クラス構造要整理
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CM3D2.AlwaysColorChangeEx.Plugin.UI;
using CM3D2.AlwaysColorChangeEx.Plugin.Util;
using UnityEngine;

namespace CM3D2.AlwaysColorChangeEx.Plugin.Data {
    /// <summary>メニューを扱うデータクラス</summary>
    public class ACCMenu {
        public int version     { get; set; }
        public string category = string.Empty;
        public string priority = "1000";
        public string name     = string.Empty;
        public string desc     = string.Empty;
        public string icon     = string.Empty;
        public string editicon = string.Empty;
        public bool   editiconExist;           // アイコンファイルが登録済みであるか

        public string editfile { get; set; } // 編集対象ファイル名
        public bool   editfileExist;         // 編集対象ファイルが登録済みであるか
        public string srcfilename { get; set; }

        public string txtpath  = string.Empty;

        // キーとmenuファイル
        public List<ResourceRef> resources { get; set; }
        public Dictionary<string, SlotMaterials> slotMaterials { get; set; }

        public Dictionary<string, List<ACCMaterialEx>> baseMatDic  { get; set; }

        public List<Item> addItems     { get; set; }
        public List<string> delItems   { get; set; }
        public string[] itemParam      { get; set; }
        public List<string> items      { get; set; }
        public List<string> maskItems  { get; set; }
        public List<string> delNodes         { get; set; }
        public List<string> showNodes        { get; set; }
        public List<string[]> delPartsNodes  { get; set; }
        public List<string[]> showPartsNodes { get; set; }


        // <slotID, Item>
        public Dictionary<TBody.SlotID, Item> itemSlots       { get; private set; }
        // 置換ファイル名用Dictionary <filename, Item>
        public Dictionary<string, Item> itemFiles       { get; private set; }
        public Dictionary<string, ResourceRef> resFiles { get; private set; }


        public ACCMenu() {
            slotMaterials   = new Dictionary<string, SlotMaterials>();
            resources       = new List<ResourceRef>();

            itemSlots = new Dictionary<TBody.SlotID, Item>();
            itemFiles = new Dictionary<string, Item>();
            resFiles = new Dictionary<string, ResourceRef>();
            addItems = new List<Item>();
            items = new List<string>();
            delItems = new List<string>();
            maskItems = new List<string>();
            delNodes = new List<string>();
            showNodes = new List<string>();
            delPartsNodes = new List<string[]>();
            showPartsNodes = new List<string[]>();
        }

        /// <summary>編集されたマテリアル情報を元に、各種マテリアル情報の更新状態を抽出する</summary>
        /// <param name="slotName">対象スロット名</param>
        /// <param name="edited">編集されたマテリアル情報</param>
        public void InitMaterials(string slotName, List<ACCMaterial> edited) {

            var needUpdate = false;
            for (var matNo=0; matNo< edited.Count; matNo++) {
                var tm = GetMaterial(slotName, matNo);
                if (tm == null) {
                    // menuのマテリアル変更で指定されていないmaterial
                    tm = new TargetMaterial(slotName, matNo, string.Empty) {onlyModel = true};
                    AddSlotMaterial(tm);

                    // modelファイルからマテリアル情報をロード 毎回ロードするのは非効率
                    //   ※高速化したい場合は、不足しているマテリアル情報を一括して取得する
                    var slot = (TBody.SlotID)Enum.Parse(typeof(TBody.SlotID), slotName, false);
                    Item item;
                    if (itemSlots.TryGetValue(slot, out item)) {
                        // TODO modelからmatNoを元に抽出
                        // item.filename
                    }
                } 

                tm.Init(edited[matNo]);
                needUpdate |= tm.shaderChanged;
                if (tm.onlyModel) {
                    needUpdate |= (tm.hasTexColorChanged | tm.hasTexFileChanged); 
                }
            }

            // modelファイルの更新の必要性をチェック
            //   対象slotのモデルでシェーダが変更されたか否か
            //   あるいは、tex色変更かtexファイル変更が行われたか
            foreach (var item in addItems) {
                if (item.slot == slotName) {
                    item.needUpdate = needUpdate;
                }
            }
        }

        public string EditFileName() {
            return editfile + FileConst.EXT_MENU;
        }

        public string EditIconFileName() {
            return editicon + FileConst.EXT_TEXTURE;
        }

        public TargetMaterial GetMaterial(string slot, int matNo) {
            SlotMaterials slotMat;
            return slotMaterials.TryGetValue(slot, out slotMat) ? slotMat.Get(matNo) : null;
        }

        public void AddSlotMaterial(TargetMaterial tm) {
            LogUtil.Debug("Add slot material", tm.editname);

            SlotMaterials slotMat;
            if (!slotMaterials.TryGetValue(tm.slotName, out slotMat)) {
                slotMat = new SlotMaterials();
                slotMaterials[tm.slotName] = slotMat;
            }
            slotMat.SetMaterial(tm);
        }

        public static void WriteMenuFile(string filepath, ACCMenu menu) {

            try {
                bool onBuffer;
                using (var reader = new BinaryReader(FileUtilEx.Instance.GetStream(menu.srcfilename, out onBuffer), Encoding.UTF8)) {
                    var header = reader.ReadString();

                    if (onBuffer || reader.BaseStream.Position > 0) {
                        if (header == FileConst.HEAD_MENU) {
                            WriteMenuFile(reader, header, filepath, menu);
                            return; 
                        }
                        var msg = headerError(header, menu.srcfilename);
                        throw new ACCException(msg.ToString());
                    }
                }
                // arc内のファイルがロードできない場合の回避策: Sybaris 0410向け対策. 一括読み込み
                using (var reader = new BinaryReader(new MemoryStream(FileUtilEx.Instance.LoadInternal(menu.srcfilename), false), Encoding.UTF8)) {
                    var header = reader.ReadString(); // hader
                    if (header == FileConst.HEAD_MATE) {
                        WriteMenuFile(reader, header, filepath, menu);
                    }
                    var msg = headerError(header, menu.srcfilename);
                    throw new ACCException(msg.ToString());
                }

            } catch (ACCException) {
                throw;
            } catch (Exception e) {
                var msg = "menuファイルの作成に失敗しました。 file="+ filepath;
                LogUtil.Error(msg, e);
                throw new ACCException(msg, e);
            }
        }

        private static void WriteMenuFile(BinaryReader reader, string header, string filepath, ACCMenu menu) {
            using (var dataStream = new MemoryStream())
            using (var dataWriter = new BinaryWriter(dataStream))
            using (var writer = new BinaryWriter(File.OpenWrite(filepath))) {

                writer.Write(header);
                writer.Write(reader.ReadInt32());
                
                // txtpath
                reader.ReadString();
                var txtpath = menu.txtpath;
                if (!txtpath.EndsWith(FileConst.EXT_TXT, StringComparison.OrdinalIgnoreCase)) {
                    txtpath += FileConst.EXT_TXT;
                }
                writer.Write(txtpath);
                // name, category, 説明
                reader.ReadString();
                reader.ReadString();
                reader.ReadString();
                writer.Write(menu.name);
                writer.Write(menu.category);
                var desc = menu.desc.Replace("\n", FileConst.RET);
                writer.Write(desc);

                // readBytes
                var num = reader.ReadInt32();

                var priorityWritten = false;
                while (true) {
                    var size = (int) reader.ReadByte();
                    if (size == 0) {
                        dataWriter.Write((byte)0);
                        break;
                    }

                    var key = reader.ReadString();
                    var param = new string[size-1];
                    for (var i = 0; i < size-1; i++) {
                        param[i] = reader.ReadString();
                    }
                    // パラメータ数が変更される場合があれば…
                    // string[] writeParams = null;

                    // 特定のパラメータを置き換え
                    switch (key) {
                        case "priority":
                            param[0] = menu.priority;
                            priorityWritten = true;
                            break;
                        case "name":
                            param[0] = menu.name;
                            break;
                        case "setumei":
                            param[0] = desc;
                            break;
                        case "icon":
                        case "icons":
                            param[0] = menu.EditIconFileName();
                            break;

                        case "additem":
                            var modelfile = param[0];
                            param[0] = menu.itemFiles[modelfile].EditFileName();
                            LogUtil.Debug("modelfile replaces ", modelfile, "=>",  param[0]);
                            break;
                        case "マテリアル変更":
                            var slot = param[0];
                            var matNo = int.Parse(param[1]);
                            var tm = menu.GetMaterial(slot, matNo);
                            param[2] = tm.EditFileName();
                            break;
                        case "リソース参照":
                            param[1] = menu.resFiles[param[1]].EditFileName();
                            break;
                        case "半脱ぎ":
                            param[0] = menu.resFiles[param[0]].EditFileName();
                            break;
                    }
                    // if (writeParams == null) writeParams = param;
                    dataWriter.Write((byte) (param.Length+1));
                    dataWriter.Write(key);
                    foreach (var wparam in param) {
                        dataWriter.Write(wparam);
                    }
                }

                if (!priorityWritten) {
                    dataWriter.Write((byte) 2);
                    dataWriter.Write("priority");
                    dataWriter.Write(menu.priority);
                }
                writer.Write((int)dataStream.Length);
                writer.Write(dataStream.ToArray());
            }
        }

        public static ACCMenu Load(string filename) {
            var menu = new ACCMenu();

            LogUtil.Debug("loading menu file", filename);
            menu.srcfilename = filename;
            menu.editfile = Path.GetFileNameWithoutExtension(filename);

            try {
                bool onBuffer;
                using (var reader = new BinaryReader(FileUtilEx.Instance.GetStream(menu.srcfilename, out onBuffer), Encoding.UTF8)) {
                    var header = reader.ReadString();
                    if (onBuffer || reader.BaseStream.Position > 0) {
                        if (header == FileConst.HEAD_MENU) {
                            Load(reader, menu.srcfilename, menu);
                            LogUtil.Debug("menu loaded");
                            return menu;
                        }
                        headerError(header, filename);
                        return null;
                    }
                }
               
                // arc内のファイルがロードできない場合の回避策: Sybaris 0410向け対策. 一括読み込み
                using (var reader = new BinaryReader(new MemoryStream(FileUtilEx.Instance.LoadInternal(menu.srcfilename), false), Encoding.UTF8)) {
                    var header = reader.ReadString(); // hader
                    if (header == FileConst.HEAD_MENU) {
                        Load(reader, menu.srcfilename, menu);
                        LogUtil.Debug("menu loaded");
                        return menu;
                    }
                    headerError(header, filename);
                    return null;
                }

            } catch (Exception e) {
                LogUtil.Error("アイテムメニューファイルが読み込めませんでした。", filename, e);
                return null;
            }
        }

        private static StringBuilder headerError(string header, string filename) {
            if (header == FileConst.HEAD_MOD) {
                return LogUtil.Error("MODファイルは未対応。", filename);
            }

            return LogUtil.Error("MENUファイルのヘッダーファイルが正しくありません。", header, ", ", filename);
        }

        private static void Load(BinaryReader reader, string filename, ACCMenu menu) {
            menu.version = reader.ReadInt32();
            menu.txtpath = reader.ReadString();
            // 未使用
            var headerName = reader.ReadString();
            var headerCategory = reader.ReadString();
            var headerSetumei  = reader.ReadString().Replace(FileConst.RET, "\n");
    
            var num2 = reader.ReadInt32();
            while (true) {
                var size = (int) reader.ReadByte();
                if (size == 0) break;
    
                var key = reader.ReadString();
                var param = new string[size-1];
                for (var i = 0; i < size-1; i++) {
                    param[i] = reader.ReadString();
                }
                switch (key) {
                    case "category":
                        menu.category = param[0];
                        break;
    //                            case "メニューフォルダ":
    //                                menu.menuFolder = param[0];
    //                                break;
    //                            case "catno":
    //                                menu.catno = param[0];
    //                                break;
                    case "属性追加":
                        break;
                    case "priority":
                        menu.priority = param[0];
                        break;
                    case "name":
                        menu.name = param[0];
                        break;
                    case "setumei":
                        menu.desc = param[0].Replace(FileConst.RET, "\n");
                        break;
                    case "icon":
                    case "icons":
                        menu.icon = param[0];
                        menu.editicon = Path.GetFileNameWithoutExtension(menu.icon);
                        break;
                    case "アイテムパラメータ":
                        if (param.Length == 3) {
                            menu.itemParam = param;
                            //Array.Copy(param, 0, itemParam, 0, param.Length);
                        }
                        break;
                    case "アイテム":
                        menu.items.Add(Path.GetFileNameWithoutExtension(param[0]));
                        break;
    
                    case "additem":
                        if (param.Length >= 1) {
                            var item = new Item(param);
                            if (!menu.itemFiles.TryGetValue(item.filename, out item.link)) {
                                menu.itemFiles[item.filename] = item;
                            }
                            menu.addItems.Add(item);
                            try {
                                if (item.slot != null) {
                                    var slotId = (TBody.SlotID)Enum.Parse(typeof(TBody.SlotID), item.slot, true);
                                    menu.itemSlots[slotId] = item;
                                }
                                
                            } catch(Exception e) {
                                LogUtil.Debug("failed to parse additem slot", item.slot, e);
                            }
                        }
                        break;
    
                    case "maskitem":
                        if (param.Length >= 1) menu.maskItems.Add(param[0]);
                        break;
    
                    case "delitem":
                        var slot0 = menu.category;
                        if (param.Length >= 1) slot0 = param[0];
                        menu.delItems.Add(slot0);
                        break;
    
                    case "マテリアル変更":
                        var slot = param[0];
                        var matNo = int.Parse(param[1]);
                        var file = param[2];
                        menu.AddSlotMaterial(new TargetMaterial(slot, matNo, file));
    
                        break;
                    case "node消去":
                        menu.delNodes.Add(param[0]);
                        break;
                    case "node表示":
                        menu.showNodes.Add(param[0]);
                        break;
                    case "パーツnode消去":
                        menu.delPartsNodes.Add(param);
                        break;
                    case "パーツnode表示":
                        menu.showPartsNodes.Add(param);
                        break;
                    case "リソース参照":
                        var resRef1 = new ResourceRef(param[0], param[1]);
                        resRef1.menu = menu;
                        menu.resources.Add(resRef1);
                        if (!menu.resFiles.TryGetValue(resRef1.filename, out resRef1.link)) {
                            menu.resFiles[resRef1.filename] = resRef1;
                        }
                        break;
                    case "半脱ぎ":
                        var resRef2 = new ResourceRef(key, param[0]);
                        resRef2.menu = menu;
                        menu.resources.Add(resRef2);
                        if (!menu.resFiles.TryGetValue(resRef2.filename, out resRef2.link)) {
                            menu.resFiles[resRef2.filename] = resRef2;
                        }
                        break;
                    case "color_set":
                        break;
                    //case "アタッチポイントの設定":
                    //    break;
                    //case "tex":
                    //case "テクスチャ変更":
                    //    break;
                    //case "テクスチャ合成":
                    //    break;
                    //case "unsetitem":
                    //    break;
                    //case "commenttype":
                    //    break;
                    //case "color_set"
                    // break;
                }
            }
        }
    }

    public class SlotMaterials {
        public readonly List<TargetMaterial> materials = new List<TargetMaterial>();
        
        /// <summary>マテリアル番号に対応するマテリアル情報を取得する</summary>
        /// <param name="matNo">マテリアル番号</param>
        /// <returns>マテリアル情報 マテリアル番号に対応するデータがない場合はnullを返す</returns>
        public TargetMaterial Get(int matNo) {
            return (matNo < materials.Count) ? materials[matNo] : null;
        }

        public void SetMaterial(TargetMaterial tm) {
            // matNoをリストのインデックスとして挿入
            var lack = tm.matNo - materials.Count;
            if (lack == 0) {
                materials.Add(tm);
            } else if (lack > 0) {
                for(var i=0; i<lack; i++) {
                    materials.Add(null);
                }
                materials.Add(tm);
            } else {
                materials[tm.matNo] = tm;
            }
        }
    }

    // マテリアルの出力対象を指定するための情報
    public class TargetMaterial {
        public string slotName { get; private set;}
        public int    matNo    { get; private set;}
        public string filename { get; private set;}

        public string editfile      { get; set;}  // マテリアルファイル
        public bool   editfileExist { get; set;}  // マテリアルファイルの存在チェック
        public string editname = string.Empty;    // マテリアル名
        public string worksuffix;
        public bool pmatExport       { get; set;} // pmatを出力するか
        public bool uiTexViewed      { get; set;}

        public bool needPmat             { get; set;} // 優先度設定が必要であるか
        public bool needPmatChange       { get; set;} // 優先度設定の変更が必要であるか
        public bool onlyModel            { get; set;} // true:menuにマテリアル変更がないケース
        public bool shaderChanged        { get; set;} // シェーダ変更
        public bool hasParamChanged      { get; set;} // materialのパラメータ変更
        public bool hasTexColorChanged   { get; set;} // texの色変更
        public bool hasTexFileChanged    { get; set;} // texファイルの変更
        public readonly Dictionary<PropKey, TargetTexture> texDic = new Dictionary<PropKey, TargetTexture>(5);

        public ACCMaterial   editedMat { get; set;}
        public ACCMaterialEx srcMat    { get; set;}

        public TargetMaterial(string slot, int matNo, string filename) {
            slotName = slot;
            this.matNo = matNo;
            this.filename = filename;
            editfile = Path.GetFileNameWithoutExtension(filename);
        }

        public string EditFileName() {
            if (worksuffix == null) return editfile + FileConst.EXT_MATERIAL;
            return editfile + worksuffix + FileConst.EXT_MATERIAL;
        }

        public void Init(ACCMaterial edited) {
            editedMat = edited;

            // ファイルからマテリアル情報をロード
            if (onlyModel) {
                // modelファイルからマテリアルのロード
                
            } else if (!string.IsNullOrEmpty(filename)) {
                LogUtil.Debug("load material file", filename);
                srcMat = ACCMaterialEx.Load(filename);
                shaderChanged = (editedMat.type != srcMat.type);
            }

            // pmat チェック
            //  透過型のみを対象とし、
            //     1. マテリアル名に対応するpmatが存在しない場合
            //     2. renderQueueが変更されている場合
            if (edited.type.isTrans) {
                // renderqueueがデフォルト値であれば変更不要
                if (Math.Abs(edited.renderQueue.val - 2000) < 0.01f) {
                    needPmat = false;
                } else {
                    needPmat = true;

                    var matName = (srcMat != null)? srcMat.name2 : edited.name;
                    var srcRq = MaterialUtil.GetRenderQueue(matName);
                    // 既存のマテリアル名に対応するpmatが存在しない => 変更必要
                    if (srcRq < 0) needPmatChange = true;
                    LogUtil.DebugF("render queue: src={0}, edited={1}", srcRq, edited.renderQueue);
    
                    needPmatChange |= !NumberUtil.Equals(edited.renderQueue.val, srcRq, 0.01f);
                    pmatExport = needPmatChange;
                }
            }

            if (!shaderChanged) {
                // TODO modelロードでsrcMatを作成した場合は条件削除可能
                if (srcMat != null) hasParamChanged = editedMat.HasChanged(srcMat);
            }

            editname = editedMat.material.name;
            var maid = MaidHolder.Instance.CurrentMaid;

            // テクスチャの変更フラグチェック
            foreach (var texProp in editedMat.type.texProps) {
            //foreach (string propName in editedMat.type1.texPropNames) {
                LogUtil.Debug("propName:", texProp.key);
                
                var tex = editedMat.material.GetTexture(texProp.propId);
                var filter = ACCTexturesView.GetFilter(maid, slotName, editedMat.material, texProp.propId);
                var colorChanged = (filter != null) && !filter.HasNotChanged();
                var fileChanged = false;
                if (tex != null && srcMat != null) {
                    ACCTextureEx baseTex;
                    if (srcMat.texDic.TryGetValue(texProp.key, out baseTex)) {
                        fileChanged = (baseTex.editname != tex.name);
                    }
                }

                var trgtTex = new TargetTexture(colorChanged, fileChanged, tex) {filter = filter};
                texDic[texProp.key] = trgtTex;
                hasTexColorChanged  |= colorChanged;
                hasTexFileChanged   |= fileChanged;
            }
            LogUtil.Debug("target material initialized");
        }

        public float RenderQueue() {
            return editedMat.renderQueue.val;
        }

        public string ShaderName() {
            return editedMat.type.name;
        }

        public string ShaderNameOrDefault(string defaultVal) {
            return (editedMat != null) ? editedMat.type.name : defaultVal;
        }
    }

    public class TargetTexture {
        public bool colorChanged {get; private set; }
        public bool fileChanged  {get; private set; }
        public bool needOutput   {get; private set; }
        public Texture tex       {get; private set; }
        public TextureModifier.FilterParam filter {get; set; }

        public string editname;
        public bool   editnameExist;

        public string worksuffix;    // 出力時に状況に応じて付与する接尾辞
        public string workfilename;  // 出力時に状況に応じて変わる、変更前のファイル名
        
        public TargetTexture(bool color, bool file, Texture tex) {
            colorChanged = color;
            fileChanged = file;
            this.tex = tex;
            if (tex == null) return;

            editname = Path.GetFileNameWithoutExtension(tex.name);

            // 色変更をせず、登録済texファイルの変更はスキップ (notexとかも出力不要と判断される)
            needOutput |= colorChanged;
            if (needOutput || !fileChanged) return;

            var texfile = tex.name;
            if (!texfile.Contains(".")) texfile += FileConst.EXT_TEXTURE;
            needOutput |= !FileUtilEx.Instance.Exists(texfile);
        }

        public string EditFileNameNoExt() {
            if (worksuffix == null) return editname;
            return editname + worksuffix;
        }

        public string EditFileName() {
            if (editname.EndsWith(FileConst.EXT_TEXTURE, StringComparison.OrdinalIgnoreCase)) {
                // 拡張子があらかじめ設定されている場合
                if (worksuffix == null) return editname;
                return editname.Substring(0, editname.Length - 4)+ worksuffix + FileConst.EXT_TEXTURE;
            }

            if (worksuffix == null) return editname + FileConst.EXT_TEXTURE;
            return editname + worksuffix + FileConst.EXT_TEXTURE;
        }

        public string EditTxtPath() {
            if (worksuffix == null) return Settings.Instance.txtPrefixTex + editname + FileConst.EXT_TXT;
            return Settings.Instance.txtPrefixTex + editname + worksuffix + FileConst.EXT_TXT;
        }
    }

    // リソース参照の情報
    public class ColorSet {
        public ACCMenu menu;
        public string key       { get; private set;}
        public string suffix    { get; private set;}
        public string filename  { get; private set;}
    }

    // リソース参照の情報
    public class ResourceRef {
        public ACCMenu menu;
        public string key       { get; private set;}
        public string suffix    { get; private set;}
        public string filename  { get; private set;}
        public ResourceRef link;

        public string editname  { get; set;}
        public bool   editfileExist;

        public ResourceRef(string key, string filename) {
            this.key      = key;
            suffix   = FileConst.GetResSuffix(key);
            this.filename = filename;
            editname = Path.GetFileNameWithoutExtension(filename);
        }

        public string EditFileName() {
            return editname + FileConst.EXT_MENU;
        }

        public string EditTxtPath() {
            return Settings.Instance.txtPrefixMenu + editname + FileConst.EXT_TXT;
        }

        public List<ReplacedInfo> replaceFiles;

        public Func<string, string[], string[]> ReplaceMenuFunc() {
            replaceFiles = new List<ReplacedInfo>();
            // menuファイル中のmodelとmateを更新する
            // それ以外はそのまま出力
            return (key, param) => {
                switch (key) {
                    case "additem":
                        if (param.Length >= 2) {
                            var slot0 = param[1];
                            try {
                                var slot = (TBody.SlotID)Enum.Parse(typeof(TBody.SlotID), slot0, false);
                                var item = menu.itemSlots[slot];
        
                                // 対応するスロットのアイテムが更新される場合にのみ出力
                                if (item.needUpdate) {
                                    var filename0 = param[0];
                                    string editfile;
                                    // 元のmodelファイルが同一であれば、すでに出力済みと見なす
                                    if (item.filename == filename0) {
                                        item.worksuffix = null;
                                        editfile = item.EditFileName();
                                    } else {
                                        item.worksuffix = suffix;
                                        editfile = item.EditFileName();
                                        replaceFiles.Add( new ReplacedInfo(filename0, editfile, this, item) );
                                    }
                                    param[0] = editfile;
                                }
                            } catch(Exception e) {
                                LogUtil.Debug("failed to parse SlotID:", slot0, e);
                                // LogUtil.DebugLog("failed to parse additem slot", slot0, e);
                            }
                        }
                        break;
        
                    case "マテリアル変更":
                        var slotName  = param[0];
                        var matNo        = int.Parse(param[1]);
                        var filename1 = param[2];
                        var tm = menu.GetMaterial(slotName, matNo);
                        // 上位menuと同名ファイルであれば、出力済のファイル名を指定 (上位側が変更されていなければそのまま）
                        tm.worksuffix = null;
                        if (tm.filename == filename1) {
                            param[2] = tm.EditFileName();
                        } else {
                            if (tm.shaderChanged || tm.hasParamChanged || tm.hasTexFileChanged || tm.hasTexColorChanged) {
                                tm.worksuffix = suffix;
                                var editfile = tm.EditFileName();
                                param[2] = editfile;
                                var replaced = new ReplacedInfo(filename1, editfile, this, tm);
                                replaceFiles.Add(replaced);
                            }
                            // マテリアルの内容が変更されていない場合は変更しない
                        }
                        break;
                }
                return param;
            };
        }
    }

    public class Item {
        public string slot     { get; private set; }
        public string filename { get; private set; }
        public string[] info   { get; private set; }
        public Item link;
        public bool needUpdate;

        public string worksuffix;
        public string editname;
        public bool   editnameExist;
        public Item(string[] info) {
            filename = info[0];
            editname = Path.GetFileNameWithoutExtension(filename);
            if (info.Length > 1) slot = info[1];
            this.info = info;
        }
        public string EditFileName() {
            if (worksuffix == null) return editname + FileConst.EXT_MODEL;
            return editname + worksuffix + FileConst.EXT_MODEL;
        }
        public bool HasSlot() {
            return slot != null;
        }
        // 同一モデルファイルを参照する場合のリンク
        public bool HasLink() {
            return link != null;
        }
    }

    public class ReplacedInfo {
        public string source;
        public string replaced;
        public ResourceRef res;
        public TargetMaterial material;
        public Item item;

        public ReplacedInfo(string src, string replaced, ResourceRef res, Item item) {
            source = src;
            this.replaced = replaced;
            this.res = res;
            this.item = item;
        }
        public ReplacedInfo(string src, string replaced, ResourceRef res, TargetMaterial tm) {
            source = src;
            this.replaced = replaced;
            this.res = res;
            material = tm;
        }
    }
}
