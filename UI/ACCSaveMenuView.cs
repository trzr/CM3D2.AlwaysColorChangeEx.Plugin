/*
 * 保存ダイアログ
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using CM3D2.AlwaysColorChangeEx.Plugin.Data;
using CM3D2.AlwaysColorChangeEx.Plugin.Util;

namespace CM3D2.AlwaysColorChangeEx.Plugin.UI
{
    /// <summary>
    /// Description of ACCSaveModView.
    /// </summary>
    public class ACCSaveMenuView
    {
        private static readonly Settings settings = Settings.Instance;
        private static readonly OutputUtilEx outUtil = OutputUtilEx.Instance;

        public static void Clear() { }

        private readonly UIParams uiParams;
        public ComboBox shaderCombo;
        public bool showDialog;

        private Vector2 scrollViewPosition = Vector2.zero;
        private bool nameInterlocked;
        private bool nameChanged;
        private Color changedColor = Color.red;

        public ACCMenu trgtMenu;
        public ACCSaveMenuView(UIParams uiParams) {
            this.uiParams = uiParams;
        }
        public Dictionary<TBody.SlotID, Item> Load(string filename) {
            trgtMenu = ACCMenu.Load(filename);
            nameInterlocked = false;
            if (trgtMenu != null) {
                showDialog = true;
                return trgtMenu.itemSlots;
            }
            return null;
        }

        public void SetEditedMaterials(TBody.SlotID slot, List<ACCMaterial> edited) {
            LogUtil.DebugLog("setEditedMaterials", slot, edited.Count);
            
            string slotName = slot.ToString();
            trgtMenu.InitMaterials(slotName, edited);
        }

        //public bool CheckData() {
        //    return true;
        //}

        // TODO 複数のマテリアルのmateファイルが同一の場合
        // 　　　　設定、textureが同じであるかを判定して、同一にしてよいかを判定⇒変更前のファイルが同一であれば一緒。
        // 複数のスロットで同一のmodelファイルを指定している場合、
        // 　現状は、他方へリンクを張り、同一modelとして出力
        //   シェーダが異なる場合は別modelとして出力する必要がある(TODO)

        public void Show() {
            if (trgtMenu == null) return;

            scrollViewPosition = GUILayout.BeginScrollView(scrollViewPosition, 
                                           GUILayout.Width(uiParams.modalRect.width-20),
                                           GUILayout.Height(uiParams.modalRect.height-55));

            float indentWidth = uiParams.margin*8f;
            GUILayoutOption extLabelWidth = GUILayout.Width(uiParams.fontSize2*3);
            GUILayoutOption shaderWidth   = GUILayout.Width(uiParams.fontSize2*ShaderMapper.MaxNameLength()*0.68f);
            GUILayoutOption propNameWidth = GUILayout.Width(uiParams.fontSize2*14*0.68f);

            GUILayout.BeginVertical();
            Color txtColr = uiParams.textStyle.normal.textColor;
            Color errColr = Color.red;
            try {
                GUILayout.BeginHorizontal();
                GUILayout.Label("メニュー", uiParams.lStyle, uiParams.modalLabelWidth);
                string before = trgtMenu.editfile;
                if (trgtMenu.editfileExist) uiParams.textStyle.normal.textColor = errColr;
                trgtMenu.editfile = GUILayout.TextField(trgtMenu.editfile, uiParams.textStyle);
                if (trgtMenu.editfileExist) uiParams.textStyle.normal.textColor = txtColr;;
                nameChanged |= (trgtMenu.editfile != before);
                GUILayout.Label(FileConst.EXT_MENU, uiParams.lStyleS, extLabelWidth);

                bool src = nameInterlocked;
                nameInterlocked = GUILayout.Toggle(nameInterlocked, "名前連動", uiParams.tStyle, uiParams.modalLabelWidth);
                if (nameInterlocked && src != nameInterlocked) {
                    nameChanged = true;
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("内部パス", uiParams.lStyle, uiParams.modalLabelWidth);
                trgtMenu.txtpath = GUILayout.TextField(trgtMenu.txtpath, uiParams.textStyle);
                GUILayout.EndHorizontal();
                
                GUILayout.BeginHorizontal();
                GUILayout.Label("優先度", uiParams.lStyle, uiParams.modalLabelWidth);
                var editedPriority = GUILayout.TextField(trgtMenu.priority, 10, uiParams.textStyle, uiParams.modalHalfWidth);
                if (trgtMenu.priority != editedPriority ) {
                    // float?
                    int v;
                    if (int.TryParse(editedPriority, out v)) {
                        if (v >= 0) {
                            trgtMenu.priority = v.ToString();
                        }
                    }
                }
                GUILayout.Space(indentWidth);
                GUILayout.Label("カテゴリ", uiParams.lStyleS);
                GUILayout.Label(trgtMenu.category, uiParams.lStyleS);

                if (GUILayout.Button("↑ パス自動設定", uiParams.bStyle)) {
                    trgtMenu.txtpath = settings.txtPrefixTex + trgtMenu.editfile;
                }
                GUILayout.EndHorizontal();
                
                GUILayout.BeginHorizontal();
                GUILayout.Label("アイコン", uiParams.lStyle, uiParams.modalLabelWidth);
                GUI.enabled = !nameInterlocked;
                if (nameInterlocked && nameChanged) {
                    if (!trgtMenu.editfile.ToLower().EndsWith(settings.iconSuffix, StringComparison.Ordinal)) {
                        trgtMenu.editicon = trgtMenu.editfile + settings.iconSuffix;
                    } else {
                        trgtMenu.editicon = trgtMenu.editfile;
                    }
                }
                if (trgtMenu.editfileExist) uiParams.textStyle.normal.textColor = errColr;
                trgtMenu.editicon = GUILayout.TextField(trgtMenu.editicon, uiParams.textStyle);
                if (trgtMenu.editfileExist) uiParams.textStyle.normal.textColor = txtColr;
                GUI.enabled = true;

                GUILayout.Label(FileConst.EXT_TEXTURE, uiParams.lStyleS, extLabelWidth);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("名前", uiParams.lStyle, uiParams.modalLabelWidth);
                trgtMenu.name = GUILayout.TextField(trgtMenu.name, uiParams.textStyle);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal(uiParams.twoLineHeight);
                GUILayout.Label("説明", uiParams.lStyle, uiParams.modalLabelWidth);
                trgtMenu.desc = GUILayout.TextArea(trgtMenu.desc, uiParams.textAreaStyle, uiParams.twoLineHeight);
                GUILayout.EndHorizontal();

                const string gname = "material";
                GUILayoutUtility.BeginGroup(gname);
                try {
                    foreach (var pair in trgtMenu.slotMaterials) {
                        GUILayout.Label("マテリアル情報 (" + pair.Key + ")", uiParams.lStyle);
                        foreach (var trgtMat in pair.Value.materials) {
                            if (trgtMat == null)　continue;

                            GUILayout.BeginHorizontal();
                            try {
                                GUILayout.Space(indentWidth);
                                if (trgtMat.onlyModel) uiParams.lStyleS.normal.textColor = Color.cyan;
                                GUILayout.Label("マテリアル" + trgtMat.matNo, uiParams.lStyleS, uiParams.modalSubLabelWidth);
                                if (trgtMat.onlyModel) uiParams.lStyleS.normal.textColor = txtColr;
    
                                // マテリアルファイル名
                                if (trgtMat.onlyModel) {
                                    GUILayout.Label("(.mateファイル無し)", uiParams.lStyleS);
                                } else {
                                    if (trgtMat.editfileExist) uiParams.textStyle.normal.textColor = errColr;
                                    GUI.enabled = !nameInterlocked;
                                    if (nameInterlocked && nameChanged) {
                                        trgtMat.editfile = trgtMenu.editfile + trgtMat.matNo;
                                    }
                                    trgtMat.editfile = GUILayout.TextField(trgtMat.editfile, uiParams.textStyle);
                                    if (trgtMat.editfileExist) uiParams.textStyle.normal.textColor = txtColr;
                                    GUI.enabled = true;
                                    GUILayout.Label(FileConst.EXT_MATERIAL, uiParams.lStyleS, extLabelWidth);
                                    if (trgtMat.needPmat && trgtMat.needPmatChange) {
                                        GUILayout.Label("|"+FileConst.EXT_PMAT, uiParams.lStyleS, extLabelWidth);
                                    }
                                }
    
                                // マテリアル名
                                if (!trgtMat.needPmat) {
                                    // 名前は任意
                                    trgtMat.editname = GUILayout.TextField(trgtMat.editname, uiParams.textStyle);    
                                } else {
                                    if (trgtMat.needPmatChange) {
                                        // pmat出力する場合
                                        trgtMat.editname = trgtMat.editfile;
                                        trgtMat.editname = GUILayout.TextField(trgtMat.editname, uiParams.textStyle);
                                    } else {
                                        GUI.enabled = false;
                                        trgtMat.editname = GUILayout.TextField(trgtMat.editname, uiParams.textStyle);
                                        GUI.enabled = true;
                                    }
                                }

                            } catch(Exception e) {
                                LogUtil.DebugLog("failed to disp material name.", trgtMat.editname,  e);
                            } finally {
                                GUILayout.EndHorizontal();
                            }

                            GUILayout.BeginHorizontal();
                            try {
                                GUILayout.Space(indentWidth*2);
                                string blabel = trgtMat.uiTexViewed? "－": "＋";
                                if (GUILayout.Button(blabel, uiParams.buttonWidth)) {
                                    trgtMat.uiTexViewed = !trgtMat.uiTexViewed;
                                }
                                string shaderName = trgtMat.ShaderNameOrDefault("不明");
                                
                                GUILayout.Label("シェーダ : " + shaderName, uiParams.lStyleS, shaderWidth);
                                GUILayout.Space(indentWidth);
                                uiParams.lStyleS.normal.textColor = changedColor;
                                GUILayout.Label(trgtMat.shaderChanged? "変更有":"" , uiParams.lStyleS);
                                uiParams.lStyleS.normal.textColor = txtColr;
                                // TODO pmat出力の有無指定
                                if (!trgtMat.needPmat) {
                                    GUILayout.Label("pmat不要(透過無)", uiParams.lStyleS, uiParams.modalLabelWidth);
                                } else {
                                    if (trgtMat.needPmatChange) {                                        
                                        GUI.enabled = false;
                                        trgtMat.pmatExport = GUILayout.Toggle(trgtMat.pmatExport, "pmat出力", uiParams.lStyleS, uiParams.modalLabelWidth);
                                        GUI.enabled = true;
                                    } else {
                                        GUILayout.Label("既存pmat利用", uiParams.lStyleS, uiParams.modalLabelWidth);
                                    }
                                }
                            } catch(Exception e) {
                                LogUtil.DebugLog("failed to disp shader", trgtMat.editname, e);
                            } finally {
                                GUILayout.EndHorizontal();
                            }

                            if (trgtMat.uiTexViewed) {
                                GUILayout.BeginVertical();
                                try {
                                    // 現在のマテリアルからテクスチャ取得
                                    Material mat = trgtMat.editedMat.material;
                                    foreach (var propName in trgtMat.editedMat.type.texPropNames) {
                                        var trgtTex = trgtMat.texDic[propName];
                                        if (trgtTex.tex == null) continue;

                                        GUILayout.BeginHorizontal(uiParams.modalSubItemHeight);
                                        GUILayout.Space(indentWidth*4);
                                        GUILayout.Label(propName, uiParams.lStyleS, propNameWidth);
                                        if (trgtTex.needOutput) {
                                            GUI.enabled = !nameInterlocked;
                                            if (nameInterlocked && nameChanged) {
                                                trgtTex.editname = trgtMat.editfile + FileConst.GetTexSuffix(propName);
                                            }
                                        } else {
                                            GUI.enabled = false;
                                        }
                                        if (!trgtMat.onlyModel) {
                                            if (trgtTex.editnameExist) uiParams.textStyle.normal.textColor = errColr;
                                            trgtTex.editname = GUILayout.TextField(trgtTex.editname, uiParams.textStyle);
                                            if (trgtTex.editnameExist) uiParams.textStyle.normal.textColor = txtColr;
                                            GUI.enabled = true;

                                            if (trgtTex.colorChanged) {
                                                uiParams.lStyleS.normal.textColor = changedColor;
                                                GUILayout.Label("色変更", uiParams.lStyleS, extLabelWidth);
                                                uiParams.lStyleS.normal.textColor = txtColr;
                                            } else {
                                                if (trgtTex.fileChanged) {
                                                    uiParams.lStyleS.normal.textColor = changedColor;
                                                    GUILayout.Label("変更有" , uiParams.lStyleS, extLabelWidth);
                                                    uiParams.lStyleS.normal.textColor = txtColr;
                                                }
                                            }
                                            // texは色変更があった場合にのみ出力

                                        } else {
                                            // modelファイルのマテリアルを使用している場合
                                            if (trgtTex.editnameExist) uiParams.lStyle.normal.textColor = errColr;
                                            GUILayout.Label(trgtTex.editname, uiParams.lStyleS);
                                            if (trgtTex.editnameExist) uiParams.lStyle.normal.textColor = txtColr;
                                        }
                                        GUI.enabled = true;
                                        
                                        GUILayout.EndHorizontal();
                                    }
                                } catch(Exception e) {
                                    LogUtil.DebugLog("display failed for tex", e);
                                } finally {
                                    GUI.enabled = true;
                                    GUILayout.EndVertical();
                                }
                            } else if (nameInterlocked && nameChanged) {
                                // 非表示でもデータは更新
                                foreach (var propName in trgtMat.editedMat.type.texPropNames) {
                                    var trgtTex = trgtMat.texDic[propName];
                                    if (trgtTex.tex == null) continue;

                                    if (trgtTex.needOutput) {
                                        trgtTex.editname = trgtMat.editfile + FileConst.GetTexSuffix(propName);
                                    }
                                }
                            }
                            // GUILayout.EndHorizontal();
                        }
                    }
                } catch(Exception e) {
                    LogUtil.ErrorLog("failed to display material", e);
                } finally {
                    GUILayoutUtility.EndGroup(gname);
                }

                if (trgtMenu.addItems.Any()) {
                    const string gname2 = "item";
                    GUILayoutUtility.BeginGroup(gname2);
                    GUILayout.Label("additem (model)", uiParams.lStyle);
                    foreach (var item in trgtMenu.addItems) {
    
                        try {
                            if (item.HasSlot()) {
                                GUILayout.BeginHorizontal();
                                GUILayout.Space(indentWidth);
                                GUILayout.Label(item.slot, uiParams.lStyleS, uiParams.modalLabelWidth);

                                if (item.editnameExist) {
                                    uiParams.textStyle.normal.textColor = errColr;
                                    uiParams.lStyleS.normal.textColor   = errColr;
                                }
                                // 同一ファイル名のモデルを参照するリンクがあった場合は変更できないラベルとする
                                if (item.HasLink()) {
                                    GUILayout.Label(item.link.EditFileName(), uiParams.lStyleS);
                                } else if (!item.needUpdate) {
                                    GUILayout.Label(item.EditFileName(), uiParams.lStyleS);
                                } else {

                                    GUI.enabled = !nameInterlocked;
                                    if (nameInterlocked && nameChanged) {
                                        var suffix = FileConst.GetModelSuffix(item.slot);
                                        if (trgtMenu.editfile.Contains(suffix)) {
                                            item.editname　= trgtMenu.editfile ;
                                        } else {
                                            item.editname　= trgtMenu.editfile + suffix;
                                        }
                                    }
                                    item.editname = GUILayout.TextField(item.editname, uiParams.textStyle);
                                    GUI.enabled = true;

                                    GUILayout.Label(FileConst.EXT_MODEL, uiParams.lStyleS, extLabelWidth);
                                }
                                if (item.editnameExist) {
                                    uiParams.textStyle.normal.textColor = txtColr;
                                    uiParams.lStyleS.normal.textColor   = txtColr;
                                }
                                GUILayout.EndHorizontal();
    
                                if (item.info.Length >=3) {
                                    GUILayout.BeginHorizontal();
                                    GUILayout.Space(indentWidth);
                                    GUILayout.Label("追加情報", uiParams.lStyleS, uiParams.modalLabelWidth);
                                    var sb = new StringBuilder();
                                    for (int i=2; i< item.info.Length; i++) {
                                        sb.Append(item.info[i]).Append(", ");
                                    }
                                    GUILayout.Label(sb.ToString(), uiParams.lStyleS);
                                    GUILayout.EndHorizontal();
                                }
                            }
                        } catch(Exception e) {
                            LogUtil.DebugLog("failed to disp item.", item.slot, e);
                        } finally {
                            GUILayoutUtility.EndGroup(gname2);
                        }
                    }
                }
                if (trgtMenu.resources.Any()) {
                    const string gname3 = "resource";
                    GUILayoutUtility.BeginGroup(gname3);
                    try {
                        GUILayout.Label("リソース参照", uiParams.lStyle);
                        foreach (var resRef in trgtMenu.resources) {
                            GUILayout.BeginHorizontal();
                            GUILayout.Space(indentWidth);
                            GUILayout.Label(resRef.key, uiParams.lStyleS, uiParams.modalSubLabelWidth);
                            if (nameInterlocked && nameChanged) {
                                resRef.editname = trgtMenu.editfile + resRef.suffix;
                            }
                            GUI.enabled = !nameInterlocked;
                            if (resRef.editfileExist) uiParams.textStyle.normal.textColor = errColr;
                            resRef.editname = GUILayout.TextField(resRef.editname, uiParams.textStyle);
                            if (resRef.editfileExist) uiParams.textStyle.normal.textColor = txtColr;
                            GUI.enabled = true;
                            GUILayout.Label(FileConst.EXT_MENU, uiParams.lStyleS, extLabelWidth);
                            GUILayout.EndHorizontal();
                        }

                    } catch(Exception e) {
                        LogUtil.DebugLog("failed to disp res", e);
                    } finally {
                        GUILayoutUtility.EndGroup(gname3);
                    }
                }

                nameChanged = false;
            } finally {
                GUILayout.EndVertical();
                GUILayout.EndScrollView();
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("保存", uiParams.bStyle)) {
                if (IsWritable(trgtMenu)) {
                    if (SaveFiles(trgtMenu)) {
                        var logmsg = "エクスポートが完了しました。出力先=" +  outUtil.GetACCDirectory(trgtMenu.editfile);
                        NUty.WinMessageBox(NUty.GetWindowHandle(), logmsg, "情報", NUty.MSGBOX.MB_OK);
                        showDialog = false;
                    }
                } else {
                    const string logmsg = "出力ファイルが登録済みか重複が存在するため、保存処理を行いませんでした。";
                    NUty.WinMessageBox(NUty.GetWindowHandle(), logmsg, "エラー", NUty.MSGBOX.MB_OK);
                }
            }
            if (GUILayout.Button("閉じる", uiParams.bStyle)) {
                showDialog = false;
            }
            GUILayout.EndHorizontal();
        }

        // ファイルの重複確認
        private bool IsWritable(ACCMenu menu) {
            if (menu.editfile.Length == 0) {
                return false;
            }

            string outDir = outUtil.GetACCDirectory();
            outDir = Path.Combine(outDir, trgtMenu.editfile);
            if (Directory.Exists(outDir)) {
                LogUtil.DebugLog("出力ディレクトリが既に存在します。", outDir);
                menu.editfileExist = true;
                return false;
            }
            
            bool hasDuplicate = false;
            // 別々の情報ファイルが重複しないか + 登録済みファイルが存在するかをチェック 
            var writeFiles = new HashSet<string> ();

            var menufile = menu.EditFileName();
            writeFiles.Add(menufile);
            menu.editfileExist = false;
            if (outUtil.Exists(menufile)) {
                LogUtil.DebugLog("既に登録済みのファイル有り", menufile);
                hasDuplicate = true;
                menu.editfileExist = true;
            }

            // icon
            menu.editiconExist = false;
            string iconfilepath = menu.EditIconFileName();
            if (outUtil.Exists(iconfilepath)) {
                LogUtil.LogF("アイコンファイル({0})は既に登録済み", iconfilepath);
                hasDuplicate = true;
                menu.editiconExist = true;
            }
            writeFiles.Add(iconfilepath);

            // modelファイル
            foreach (var pair in menu.itemFiles) {
                Item item = pair.Value;
                if (item.needUpdate) {
                    item.editnameExist = false; // clear
                    string filename = item.EditFileName();
                    if (HasAlreadyWritten(writeFiles, filename)) {
                        LogUtil.DebugLog("重複出力あり", filename);
                        hasDuplicate = true;
                        item.editnameExist = true; // フラグを分けるべきか？
                        continue;
                    }
                    if (outUtil.Exists(filename)) {
                        LogUtil.DebugLog("既に登録済みのファイル有り", filename);
                        hasDuplicate = true;
                        item.editnameExist = true;
                    }
                }
            }
            // mateファイル
            foreach (var tm in menu.slotMaterials.Values) {
                foreach(var trgtMat in tm.materials) {
                    // modelファイルのみの場合は好み ⇒変更する場合はmenuにマテリアル変更を追加する必要あり
                    if (trgtMat.onlyModel) continue;

                    trgtMat.editfileExist = false;
                    var filename = trgtMat.EditFileName();
                    if (HasAlreadyWritten(writeFiles, filename)) {
                        LogUtil.DebugLog("重複出力あり", filename);
                        hasDuplicate = true;
                        trgtMat.editfileExist = true; // フラグを分けるべきか？
                        continue;                        
                    }
                    if (outUtil.Exists(filename)) {
                        LogUtil.DebugLog("既に登録済みのファイル有り", filename);
                        hasDuplicate = true;
                        trgtMat.editfileExist = true;
                    }
                    // pmat
                    if (trgtMat.needPmatChange) {
                        var name = Path.GetFileNameWithoutExtension(filename);
                        var pmatfile = name + FileConst.EXT_PMAT;
                        if (HasAlreadyWritten(writeFiles, pmatfile)) {
                            LogUtil.DebugLog("重複出力あり", pmatfile);
                            hasDuplicate = true;
                            trgtMat.editfileExist = true; // フラグを分けるべきか？
                            continue;                        
                        }
                        if (outUtil.Exists(pmatfile)) {
                            LogUtil.DebugLog("既に登録済みのファイル有り", pmatfile);
                            hasDuplicate = true;
                            trgtMat.editfileExist = true;
                        }
                    }

                    // texファイル
                    foreach (var trgtTex in trgtMat.texDic.Values) {
                        if (!trgtTex.needOutput) continue;
                        trgtTex.editnameExist = false;

                        var texfilename = trgtTex.EditFileName();
                        if (HasAlreadyWritten(writeFiles, texfilename)) {
                            LogUtil.DebugLog("重複出力あり", texfilename);
                            hasDuplicate = true;
                            trgtTex.editnameExist = true; // フラグを分けるべきか？
                            continue;                        
                        }
                        if (outUtil.Exists(texfilename)) {
                            LogUtil.DebugLog("既に登録済みのファイル有り", texfilename);
                            hasDuplicate = true;
                            trgtTex.editnameExist = true;
                        }
                    }
                }
            }

            // めくれ、ずらし用menuファイルの出力
            foreach (var res in menu.resFiles.Values) {

                res.editfileExist = false;
                var filename = res.EditFileName();
                if (HasAlreadyWritten(writeFiles, filename)) {
                    LogUtil.DebugLog("重複出力あり", filename);
                    hasDuplicate = true;
                    res.editfileExist = true; // フラグを分けるべきか？
                    continue;                        
                }
                if (outUtil.Exists(filename)) {
                    LogUtil.DebugLog("既に登録済みのファイル有り", filename);
                    hasDuplicate = true;
                    res.editfileExist = true;
                }
            }
            return !hasDuplicate;
        }

        public bool SaveFiles(ACCMenu menu) {
            var outDir = outUtil.GetACCDirectory(trgtMenu.editfile);
            if (!Directory.Exists(outDir)) Directory.CreateDirectory(outDir);

            LogUtil.DebugLog("output path", outDir);
            string filepath = Path.Combine(outDir, menu.EditFileName());

            // menu出力
            ACCMenu.WriteMenuFile(filepath, menu);

            var writeFiles = new HashSet<string>();

            // filterがあれば、適用しアイコンを変更
            string iconfilepath = Path.Combine(outDir, menu.EditIconFileName());
            writeFiles.Add(iconfilepath);
            string icontxt = settings.txtPrefixTex + iconfilepath;
            outUtil.CopyTex(menu.icon, iconfilepath, icontxt, null);
            LogUtil.DebugLog("texファイルを出力しました。", iconfilepath);

            // model 出力 (additem)
            foreach (var pair in menu.itemFiles) {
                string infile = pair.Key;
                Item item = pair.Value;
                if (item.needUpdate) {
                    string filename = item.EditFileName();
                    if (HasAlreadyWritten(writeFiles, filename)) continue;

                    string modelfilepath = Path.Combine(outDir, filename);
    
                    // modelファイルのスロットのマテリアル/テクスチャ情報を抽出
                    SlotMaterials slotMat = menu.slotMaterials[item.slot];
                    // onlyModelでない場合はshader変更のみとしておく
                    // material slotとmatNoで特定
                    // texture  propName
                    // 必要に応じてtex出力
                    outUtil.WriteModelFile(infile, modelfilepath, slotMat);
                    LogUtil.DebugLog("modelファイルを出力しました。", modelfilepath);
                }
            }

            // mat出力
            foreach (var tm in menu.slotMaterials.Values) {
                foreach(var trgtMat in tm.materials) {
                    // マテリアル変更が指定された場合 => TODO 切り替え可能とする場合：menuにマテリアル変更を追加する必要あり
                    if (!trgtMat.onlyModel) {
                        var filename = trgtMat.EditFileName();
                        if (HasAlreadyWritten(writeFiles, filename)) continue;
    
                        string matefilepath = Path.Combine(outDir, filename);
                        outUtil.WriteMateFile(trgtMat.filename, matefilepath, trgtMat);
                        LogUtil.DebugLog("mateファイルを出力しました。", matefilepath);
    
                        if (trgtMat.needPmatChange) {
                            var name = Path.GetFileNameWithoutExtension(filename);
                            var pmatfile = name + FileConst.EXT_PMAT;
                            string pmatfilepath = Path.Combine(outDir, pmatfile);
                            outUtil.WritePmat(pmatfilepath, trgtMat.editname, 
                                              trgtMat.RenderQueue(), trgtMat.ShaderName());
                            LogUtil.DebugLog("pmatファイルを出力しました。", pmatfilepath);
                        }
                    }

                    // テクスチャ出力
                    foreach (var trgtTex in trgtMat.texDic.Values) {
                        if (!trgtTex.needOutput) continue;

                        var tex2d = trgtTex.tex as Texture2D;
                        if (tex2d == null) {
                            LogUtil.DebugLog("tex is not Texture2D", trgtTex.editname);
                            continue;
                            // TODO RenderTexの場合は無理やりTexture2Dに変換も可能だが…
                        }
                        var texfilename = trgtTex.EditFileName();
                        if (HasAlreadyWritten(writeFiles, texfilename)) continue;
                            
                        string texfilepath = Path.Combine(outDir, texfilename);
                        outUtil.WriteTexFile(texfilepath, trgtTex.EditTxtPath(), tex2d.EncodeToPNG());
                        LogUtil.DebugLog("texファイルを出力しました。", texfilepath);
                    }
                }
            }

            // めくれ、ずらし用menuファイルの出力
            foreach (var res in menu.resFiles.Values) {

                // 各モデルファイルで出力が必要となるファイルについて、
                // 元が同名のファイルを参照している場合でも関係なく出力
                // 設定が違う場合もある上、editnameはすべて別名になるはず
                var filename = res.EditFileName();
                if (HasAlreadyWritten(writeFiles, filename)) continue;


                string menufilepath = Path.Combine(outDir, filename);
                var toCreateFiles = outUtil.WriteMenuFile(res.filename, menufilepath, res);
                LogUtil.DebugLog("menuファイルを出力しました。", menufilepath);

                foreach (var toCreate in toCreateFiles) {
                    // modelを出力
                    if (toCreate.item != null) {
                        var filename0 = toCreate.replaced;
                        if (HasAlreadyWritten(writeFiles, filename0)) continue;

                        string modelfilepath = Path.Combine(outDir, filename0);
                        SlotMaterials slotMat = menu.slotMaterials[toCreate.item.slot];
                        // TODO リプレースが想定される情報であるかチェック 
                        outUtil.WriteModelFile(toCreate.source, modelfilepath, slotMat);
                        
                    // .mate出力
                    } else if (toCreate.material != null) {
                        TargetMaterial trgtMat = toCreate.material;
                        var filename0 = toCreate.replaced;
                        if (HasAlreadyWritten(writeFiles, filename0)) continue;

                        // mate出力==別のtexファイルを出力する可能性有り
                        string matefilepath = Path.Combine(outDir, filename0);
                        outUtil.WriteMateFile(toCreate.source, matefilepath, trgtMat);
                        // マテリアル名は上位と同じにして、同一pmatを使用する
                        //if (trgtMat.needPmatChange) {
                        //    var name = Path.GetFileNameWithoutExtension(filename);
                        //    var pmatfile = name + FileConst.EXT_PMAT;
                        //    string pmatfilepath = Path.Combine(outDir, pmatfile);
                        //    outUtil.WritePmat(pmatfilepath, trgtMat.editname, 
                        //                      trgtMat.RenderQueue(), trgtMat.ShaderName());
                        //    LogUtil.DebugLog("pmatファイルを出力しました。", pmatfilepath);
                        //}

                        foreach (var tex in trgtMat.texDic.Values) {
                            if (!tex.needOutput) continue;

                            var tex2d = tex.tex as Texture2D;
                            if (tex2d == null) {
                                LogUtil.DebugLog("tex is not 2D", tex.editname);
                                continue;
                            }
                            var texfilename = tex.EditFileName();
                            if (HasAlreadyWritten(writeFiles, texfilename)) continue;

                            // テクスチャをロードし、フィルタを適用
                            Texture2D srcTex;
                            if (tex.fileChanged) { // texファイル変更済の場合はロードされたデータ済みから出力
                                srcTex = tex2d;
                            } else {
                                srcTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                                string texfile = tex.workfilename + FileConst.EXT_TEXTURE;
                                if (!outUtil.Exists(texfile)) {
                                    LogUtil.LogF("リソース参照で使用されているtexファイル({0})が見つかりません。このため、texファイルを出力できませんでした。, ", texfile);
                                    continue;
                                }
                                srcTex.LoadImage( ImportCM.LoadTexture(texfile) );
                            }

                            if (tex.colorChanged) {
                                srcTex = ACCTexturesView.Filter(srcTex, tex.filter);
                            }
                            string texfilepath = Path.Combine(outDir, texfilename);
                            outUtil.WriteTexFile(texfilepath, tex.EditTxtPath(), srcTex.EncodeToPNG());
                            LogUtil.DebugLog("texファイルを出力しました。", texfilepath);
                        }
                    }
                }
            }
            return true;
        }
        private bool HasAlreadyWritten(ICollection<string> writtenFiles, string filename) {
            if (writtenFiles.Contains(filename)) {
                LogUtil.DebugLog("既に出力済のため、出力をスキップします。", filename);
                return true;
            }
            writtenFiles.Add(filename);
            return false;
        }
    }
}
